using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event.EventDataModel;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Message
{
    /// <summary>
    /// [選択肢]コマンドのコマンド設定枠の表示物
    /// </summary>
    public class InputSelect : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_input_select.uxml";

        private EventCommand targetCommand = null;
        private PopupFieldBase<string> _defaultSelectPopupField;
        private PopupFieldBase<string> _cancelSelectPopupField;
        private bool _cancelBranchEnable = false;

        public InputSelect(
            VisualElement rootElement,
            List<EventDataModel> eventDataModels,
            int eventIndex,
            int eventCommandIndex
        )
            : base(rootElement, eventDataModels, eventIndex, eventCommandIndex) {}

        public override void Invoke() {
            var commandTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SettingUxml);
            VisualElement commandFromUxml = commandTree.CloneTree();
            EditorLocalize.LocalizeElements(commandFromUxml);
            commandFromUxml.style.flexGrow = 1;
            RootElement.Add(commandFromUxml);

            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            
            targetCommand = EventDataModels[EventIndex].eventCommands[EventCommandIndex];
            if (targetCommand.parameters.Count == 0)
            {
                // 選択肢数
                targetCommand.parameters.Add("2");
                // 背景
                targetCommand.parameters.Add("0");
                // ウィンドウ位置
                targetCommand.parameters.Add("-1");
                // デフォルトの選択肢
                targetCommand.parameters.Add("0");
                // キャンセル時の選択肢
                targetCommand.parameters.Add("1");
                targetCommand.parameters.Add("");

                SelectListSetting(2, 0);
                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }
            //各ドロップダウン
            //選択肢
            List<string> choicesNumSelectTextDropdownChoices = new List<string>() {"1", "2", "3", "4", "5", "6"};
            //背景
            List<string> backGroundParameters = new List<string>() {"WORD_1196", "WORD_1197", "WORD_1198"};
            //ウィンドウ位置
            List<string> windowPositionChoices =
                EditorLocalize.LocalizeTexts(new List<string>() {"WORD_0366", "WORD_0813", "WORD_0298", "WORD_0814"});
            
            //デフォルト選択肢（なし、選択肢1~6）
            List<string> defaultChoices= new List<string>
            {
                EditorLocalize.LocalizeText("WORD_0113"), 
                EditorLocalize.LocalizeText("WORD_1538"), 
                EditorLocalize.LocalizeText("WORD_1539"), 
                EditorLocalize.LocalizeText("WORD_1540"), 
                EditorLocalize.LocalizeText("WORD_1541"), 
                EditorLocalize.LocalizeText("WORD_1542"), 
                EditorLocalize.LocalizeText("WORD_1543")
            };

            //キャンセル選択肢（分岐、禁止、選択肢１～６）
            List<string> cancelChoices= new List<string>
            {
                EditorLocalize.LocalizeText("WORD_1206"), 
                EditorLocalize.LocalizeText("WORD_0775"), 
                EditorLocalize.LocalizeText("WORD_1538"), 
                EditorLocalize.LocalizeText("WORD_1539"), 
                EditorLocalize.LocalizeText("WORD_1540"), 
                EditorLocalize.LocalizeText("WORD_1541"), 
                EditorLocalize.LocalizeText("WORD_1542"), 
                EditorLocalize.LocalizeText("WORD_1543")
            };

            //選択肢
            VisualElement choicesNumSelect = RootElement.Q<VisualElement>("command_inputSelect")
                .Query<VisualElement>("choices_num_select");
            int index = 0;
            int oldchoicesNum = int.Parse(targetCommand.parameters[0]);
            if (targetCommand.parameters[0] != "")
                index = choicesNumSelectTextDropdownChoices.IndexOf(targetCommand.parameters[0]);
            if (index == -1)
                index = 0;
            PopupFieldBase<string> choicesNumSelectPopupField =
                new PopupFieldBase<string>(choicesNumSelectTextDropdownChoices, index);

            //背景
            VisualElement windowPosition = RootElement.Q<VisualElement>("command_inputSelect")
                .Query<VisualElement>("backGround_select");
            List<string> windowPositionTextDropdownChoices =
                EditorLocalize.LocalizeTexts(new List<string>(backGroundParameters));
            PopupFieldBase<string> windowPositionPopupField =
                new PopupFieldBase<string>(windowPositionTextDropdownChoices, targetCommand.parameters[1] == "" ? 0 : int.Parse(targetCommand.parameters[1]));

            //ウィンドウ位置
            {
                VisualElement window_pos_select = RootElement.Q<VisualElement>("command_inputSelect").Query<VisualElement>("window_pos");
                int posIndex = 0;
                posIndex = int.TryParse(targetCommand.parameters[2], out posIndex) ? posIndex : -1;

                PopupFieldBase<string> windowPosSelectPopupField = new PopupFieldBase<string>(windowPositionChoices, posIndex + 1);
                windowPosSelectPopupField.RegisterValueChangedCallback((evt =>
                {
                    // MV準拠の設計にするため、 -1から始まるインデックスで格納する
                    targetCommand.parameters[2] = (windowPosSelectPopupField.index - 1).ToString();
                    Save(EventDataModels[EventIndex]);
                }));

                window_pos_select.Clear();
                window_pos_select.Add(windowPosSelectPopupField);
            }

            // デフォルト選択肢
            {
                VisualElement default_select = RootElement.Q<VisualElement>("command_inputSelect").Query<VisualElement>("default");
                var defaultIndex = 0;
                defaultIndex = int.TryParse(targetCommand.parameters[3], out defaultIndex) ? defaultIndex : 0;

                _defaultSelectPopupField = new PopupFieldBase<string>(defaultChoices.Take(oldchoicesNum + 1).ToList(), defaultIndex + 1);
                _defaultSelectPopupField.RegisterValueChangedCallback((evt =>
                {
                    // MV準拠の設計にするため、 -1~5のインデックスで格納する
                    targetCommand.parameters[3] = (_defaultSelectPopupField.index - 1).ToString();
                    Save(EventDataModels[EventIndex]);
                }));

                default_select.Clear();
                default_select.Add(_defaultSelectPopupField);
            }

            // キャンセル時の選択肢
            {
                VisualElement cancel_select = RootElement.Q<VisualElement>("command_inputSelect").Query<VisualElement>("cancel");
                var cancelChoicesIndex = 0;
                cancelChoicesIndex = int.TryParse(targetCommand.parameters[4], out cancelChoicesIndex) ? cancelChoicesIndex : 1;
                _cancelBranchEnable = cancelChoicesIndex == -2;

                _cancelSelectPopupField = new PopupFieldBase<string>(cancelChoices.Take(oldchoicesNum + 2).ToList(), cancelChoicesIndex + 2);
                _cancelSelectPopupField.RegisterValueChangedCallback((evt =>
                {
                    // 分岐の切り替え
                    SwitchCancelBranch(_cancelSelectPopupField.index == 0);

                    // MV準拠の設計にするため、 -2~5のインデックスで格納する
                    targetCommand.parameters[4] = (_cancelSelectPopupField.index - 2).ToString();
                    Save(EventDataModels[EventIndex]);
                }));
                
                cancel_select.Clear();
                cancel_select.Add(_cancelSelectPopupField);
            }
            
            //各プルダウンの追加箇所
            //選択肢
            choicesNumSelect.Clear();
            choicesNumSelect.Add(choicesNumSelectPopupField);
            choicesNumSelectPopupField.RegisterValueChangedCallback((evt =>
            {
                targetCommand.parameters[0] = choicesNumSelectPopupField.value;
                SelectListSetting(int.Parse(choicesNumSelectPopupField.value), oldchoicesNum);
                oldchoicesNum = int.Parse(choicesNumSelectPopupField.value);

                Save(EventDataModels[EventIndex]);
                DefaultSelectTextValueChange(int.Parse(targetCommand.parameters[0]));
                CancelSelectTextValueChange(int.Parse(targetCommand.parameters[0]));
            }));

            //背景
            windowPosition.Clear();
            windowPosition.Add(windowPositionPopupField);
            windowPositionPopupField.RegisterValueChangedCallback((evt =>
            {
                targetCommand.parameters[1] = windowPositionPopupField.index.ToString();
                Save(EventDataModels[EventIndex]);
            }));

            // デフォルトの選択肢数の更新用
            void DefaultSelectTextValueChange(int value)
            {
                // なしの為に+1しています
                _defaultSelectPopupField.RefreshChoices(defaultChoices.Take(value + 1).ToList());

                // 要素数よりも大きいインデックスを指定している場合は末尾のインデックスを持たせる
                var currentIndex = int.Parse(targetCommand.parameters[3]);
                if(currentIndex >= value)
                {
                    targetCommand.parameters[3] = (currentIndex - 1).ToString();
                    Save(EventDataModels[EventIndex]);

                    RootElement.Clear();
                    this.Invoke();
                }
            }

            // キャンセルの選択肢数の更新用
            void CancelSelectTextValueChange(int value)
            {
                //　分岐、禁止の為に+2しています
                _cancelSelectPopupField.RefreshChoices(cancelChoices.Take(value + 2).ToList());
                
                // 要素数よりも大きいインデックスを指定している場合は末尾のインデックスを持たせる
                var currentIndex = int.Parse(targetCommand.parameters[4]);
                if(currentIndex >= value)
                {
                    targetCommand.parameters[4] = (currentIndex - 1).ToString();
                    Save(EventDataModels[EventIndex]);

                    RootElement.Clear();
                    this.Invoke();
                }
            }
        }

        /// <summary>
        /// [選択肢数]の変更に伴う実行内容枠の選択肢の増減処理
        /// </summary>
        /// <param name="newNum">新しい選択肢数</param>
        /// <param name="oldNum">直前までの選択肢数</param>
        void SelectListSetting(int newNum, int oldNum) {
            var commandList = EventDataModels[EventIndex].eventCommands.ToList();
            var markerCode = _cancelBranchEnable ? EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_CANCELED : EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_END;

            if (oldNum <= newNum)   //前の値から数が増えてるのでその分増やす
            {
                for(int choiceNum = oldNum + 1; choiceNum <= newNum; choiceNum++)
                {
                    // 対象の[選択肢]コマンドから「分岐終了」を探索し、挿入位置を確認する
                    int insertIndex = commandList.FindIndex(EventCommandIndex, v => v.code == (int) markerCode && v.indent == targetCommand.indent);
                    if(insertIndex == -1)
                    {
                        return;
                    }
                    
                    // 挿入位置から選択肢用の行と空白の行をそれぞれ追加
                    var selectedCommand = new EventCommand(0, new List<string>(), new List<EventCommandMoveRoute>());
                    var blankCommand = new EventCommand(0, new List<string>(), new List<EventCommandMoveRoute>());
                    SetEventData(ref selectedCommand, (int)EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_SELECTED, choiceNum);
                    SetEventData(ref blankCommand, 0, 0);
                    commandList.Insert(insertIndex, selectedCommand);
                    commandList.Insert(insertIndex + 1, blankCommand);
                }
            }
            else    //前の値から数が減ってるのでその分減らす
            {
                // 減少前の末尾のインデックスと新しく末尾になるインデックスを算出してその間にあるコマンドを全て削除する
                int oldTailIndex = commandList.FindIndex(EventCommandIndex, v => v.code == (int) markerCode && v.indent == targetCommand.indent);
                int newTailIndex = 0;
                int rowCount = EventCommandIndex + 1;
                for(int choiceCount = 0; choiceCount <= newNum; rowCount++)
                {
                    if(commandList[rowCount].indent == targetCommand.indent)
                    {
                        choiceCount++;
                    }

                    if(rowCount >= commandList.Count)
                        return;
                }
                newTailIndex = rowCount - 1;

                if(oldTailIndex < 0 || oldTailIndex < newTailIndex)
                {
                    return;
                }

                // 削除実施
                for (int i = oldTailIndex - 1; i >= newTailIndex; i--)
                {
                    commandList.RemoveAt(i);
                }
            }

            EventDataModels[EventIndex].eventCommands = commandList;
        }

        /// <summary>
        /// [キャンセル]の変更に伴う分岐先の増減処理
        /// 分岐を有効にする場合は分岐を作成し、無効にする場合は分岐内のコマンドごと削除する
        /// </summary>
        /// <param name="enable">キャンセル時に遷移するブランチを有効にするどうか</param>
        void SwitchCancelBranch(bool enable) {
            var commandList = EventDataModels[EventIndex].eventCommands.ToList();

            if (enable && !_cancelBranchEnable)
            {
                // 対象の[選択肢]コマンドから「分岐終了」を探索し、挿入位置を確認する
                int insertIndex = commandList.FindIndex(EventCommandIndex, 
                    v => v.code == (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_END && v.indent == targetCommand.indent);
                if(insertIndex == -1)
                {
                    return;
                }
                
                // 挿入位置からキャンセル分岐用の行と空白の行をそれぞれ追加
                var cancelCommand = new EventCommand((int)EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_CANCELED,
                    new List<string>(), new List<EventCommandMoveRoute>());
                var blankCommand = new EventCommand(0, new List<string>(), new List<EventCommandMoveRoute>());
                commandList.Insert(insertIndex, cancelCommand);
                commandList.Insert(insertIndex + 1, blankCommand);
                _cancelBranchEnable = true;
            }
            else if (!enable && _cancelBranchEnable)
            {
                // 減少前の末尾のインデックスと新しく末尾になるインデックスを算出してその間にあるコマンドを全て削除する
                int oldTailIndex = commandList.FindIndex(EventCommandIndex,
                    v => v.code == (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_END && v.indent == targetCommand.indent);
                int newTailIndex  = commandList.FindIndex(EventCommandIndex,
                    v => v.code == (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_CANCELED && v.indent == targetCommand.indent);

                if(oldTailIndex == -1 || newTailIndex == -1 || oldTailIndex < newTailIndex)
                {
                    return;
                }

                // 削除実施
                for (int i = oldTailIndex - 1; i >= newTailIndex; i--)
                {
                    commandList.RemoveAt(i);
                }
                _cancelBranchEnable = false;
            }

            EventDataModels[EventIndex].eventCommands = commandList;
        }

        void SetEventData(ref EventCommand data, int code, int num) {
            data.code = code;
            string name = "";
            if(num == 1)
            {
                name = EditorLocalize.LocalizeText("WORD_3058");
            }else if (num == 2)
            {
                name = EditorLocalize.LocalizeText("WORD_3059");
            }
            data.parameters = new List<string>() {(num).ToString(), EventIndex.ToString(), name };
        }
    }
}