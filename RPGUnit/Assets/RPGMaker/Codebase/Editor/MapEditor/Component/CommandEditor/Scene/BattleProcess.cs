using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Scene
{
    public class BattleProcess : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_battle_process.uxml";

        public BattleProcess(
            VisualElement rootElement,
            List<EventDataModel> eventDataModels,
            int eventIndex,
            int eventCommandIndex
        )
            : base(rootElement, eventDataModels, eventIndex, eventCommandIndex) {
        }

        public override void Invoke() {
            var commandTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SettingUxml);
            VisualElement commandFromUxml = commandTree.CloneTree();
            EditorLocalize.LocalizeElements(commandFromUxml);
            commandFromUxml.style.flexGrow = 1;
            RootElement.Add(commandFromUxml);

            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();

            //各データのロード処理
            var troopDataModels = DatabaseManagementService.LoadTroop();
            var flagDataModel = DatabaseManagementService.LoadFlags();

            //初期値
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Count == 0)
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add(troopDataModels[0].id);
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            //敵グループのリスト
            var troopNameList = new List<string>();
            var troopIdList = new List<string>();
            var selectID = 0;
            for (var i = 0; i < troopDataModels.Count; i++)
            {
                troopNameList.Add(troopDataModels[i].name);
                troopIdList.Add(troopDataModels[i].id);
            }

            //初期選択
            selectID = troopIdList.IndexOf(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1]);
            if (selectID == -1)
                selectID = 0;
            var troopPopupField = new PopupFieldBase<string>(troopNameList, selectID);

            //敵グループ選択時処理
            troopPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = troopIdList[troopPopupField.index];
                Save(EventDataModels[EventIndex]);
            });

            //変数のリスト
            var variableNameList = new List<string>();
            var variableIdNameList = new List<string>();
            selectID = 0;
            for (var i = 0; i < flagDataModel.variables.Count; i++)
            {
                if (flagDataModel.variables[i].name == "")
                    variableNameList.Add(EditorLocalize.LocalizeText("WORD_1518"));
                else
                    variableNameList.Add(flagDataModel.variables[i].name);

                variableIdNameList.Add(flagDataModel.variables[i].id);
            }

            //初期選択
            selectID = variableIdNameList.IndexOf(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1]);
            if (selectID == -1)
                selectID = 0;
            var variablePopupField = new PopupFieldBase<string>(variableNameList, selectID);

            //変数選択時処理
            variablePopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = variableIdNameList[variablePopupField.index];
                Save(EventDataModels[EventIndex]);
            });

            //トグル設定
            var toggleList = new List<RadioButton>();
            toggleList.Add(RootElement.Query<RadioButton>("radioButton-eventCommand-display140"));
            toggleList.Add(RootElement.Query<RadioButton>("radioButton-eventCommand-display141"));
            toggleList.Add(RootElement.Query<RadioButton>("radioButton-eventCommand-display142"));

            //トグル切り替え時の非表示設定領域
            var toggleElementList = new List<VisualElement>();
            toggleElementList.Add(RootElement.Query<VisualElement>("direct_toggle_element"));
            toggleElementList.Add(RootElement.Query<VisualElement>("variable_toggle_element"));
            toggleElementList.Add(new VisualElement());

            //トグル切替時のAction
            var toggleActions = new List<Action>
            {
                //直接指定
                () =>
                {
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "0";
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = troopIdList[troopPopupField.index];
                    Save(EventDataModels[EventIndex]);
                },
                //変数指定
                () =>
                {
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "1";
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = variableIdNameList[variablePopupField.index];
                    Save(EventDataModels[EventIndex]);
                },
                //ランダム
                () =>
                {
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "2";
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = "0";
                    Save(EventDataModels[EventIndex]);
                }
            };

            //VisualElementへ貼り付け
            toggleElementList[0].Add(troopPopupField);
            toggleElementList[1].Add(variablePopupField);
            troopPopupField.AddToClassList("toggle_contents");
            variablePopupField.AddToClassList("toggle_contents");

            //トグルの登録
            new CommonToggleSelector().SetRadioInVisualElementSelector(toggleList, toggleElementList,
                int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0]), toggleActions);

            //逃走可、敗北可
            Toggle escape_toggle = RootElement.Query<Toggle>("escape_toggle");
            Toggle defeat_toggle = RootElement.Query<Toggle>("defeat_toggle");
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] == "1")
                escape_toggle.value = true;
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] == "1")
                defeat_toggle.value = true;
            escape_toggle.RegisterValueChangedCallback(o =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] =
                    escape_toggle.value ? "1" : "0";
                DefeatEscapeCheck(escape_toggle.value, defeat_toggle.value);
                Save(EventDataModels[EventIndex]);
            });
            defeat_toggle.RegisterValueChangedCallback(o =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] =
                    defeat_toggle.value ? "1" : "0";
                DefeatEscapeCheck(escape_toggle.value, defeat_toggle.value);
                Save(EventDataModels[EventIndex]);
            });
        }

        /// <summary>
        ///     逃走、敗北のトグルを見てMapEditor　Contentsに記述するものの処理
        /// </summary>
        /// <param name="escape"></param>
        /// <param name="defeat"></param>
        private void DefeatEscapeCheck(bool escape, bool defeat) {
            //敗北、逃走の記述が書かれているか確認
            var defeatIsInText = false;
            var escapeIsInText = false;
            var endIsInText = false;
            //各記述のIndexを格納
            var escapeIndex = 0;
            var defeatIndex = 0;
            var endIndex = 0;

            var eventCount = EventDataModels[EventIndex].eventCommands.Count;

            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex + 1].code == (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_WIN)
            {
                for (var i = EventCommandIndex + 1; i < eventCount; i++)
                {
                    if (EventDataModels[EventIndex].eventCommands[i].code ==
                        (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_ESCAPE)
                    {
                        escapeIsInText = true;
                        escapeIndex = i;
                    }

                    if (EventDataModels[EventIndex].eventCommands[i].code ==
                        (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_LOSE)
                    {
                        defeatIsInText = true;
                        defeatIndex = i;
                    }

                    if (EventDataModels[EventIndex].eventCommands[i].code ==
                        (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_END)
                    {
                        endIsInText = true;
                        endIndex = i;
                    }

                    if (endIsInText && (escapeIsInText || defeatIsInText))
                        break;
                }
            }

            if (!escape)
                if (escapeIsInText)
                {
                    eventCount = EventDataModels[EventIndex].eventCommands.Count;
                    //敗北がTRUEならESCAPEだけいらないのでそれだけ消す
                    if (defeat)
                        for (var i = escapeIndex; i < eventCount; i++)
                        {
                            if (EventDataModels[EventIndex].eventCommands[i].code ==
                                (int) EventEnum
                                    .EVENT_CODE_SCENE_SET_BATTLE_CONFIG_LOSE)
                            {
                                var eventDataList =
                                    EventDataModels[EventIndex].eventCommands.ToList();
                                eventDataList.RemoveAll(s => s == null);
                                EventDataModels[EventIndex].eventCommands = eventDataList;
                                break;
                            }

                            EventDataModels[EventIndex].eventCommands[i] = null;
                        }
                    //敗北もFALSEなら全部消す
                    else
                        for (var i = EventCommandIndex + 1; i < eventCount; i++)
                        {
                            if (EventDataModels[EventIndex].eventCommands[i].code ==
                                (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_END)
                            {
                                EventDataModels[EventIndex].eventCommands[i] = null;
                                var eventDataList =
                                    EventDataModels[EventIndex].eventCommands.ToList();
                                eventDataList.RemoveAll(s => s == null);
                                EventDataModels[EventIndex].eventCommands = eventDataList;
                                break;
                            }

                            EventDataModels[EventIndex].eventCommands[i] = null;
                        }
                }

            if (!defeat)
            {
                eventCount = EventDataModels[EventIndex].eventCommands.Count;
                if (defeatIsInText)
                {
                    //逃走がTRUEならLOSEだけいらないのでそれだけ消す
                    if (escape)
                        for (var i = defeatIndex; i < eventCount; i++)
                        {
                            var code = EventDataModels[EventIndex].eventCommands[i].code;
                            if (code == (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_END ||
                                code == (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_ESCAPE)
                            {
                                var eventDataList =
                                    EventDataModels[EventIndex].eventCommands.ToList();
                                eventDataList.RemoveAll(s => s == null);
                                EventDataModels[EventIndex].eventCommands = eventDataList;
                                break;
                            }

                            EventDataModels[EventIndex].eventCommands[i] = null;
                        }
                    //逃走もFALSEなら全部消す
                    else
                        for (var i = EventCommandIndex + 1; i < eventCount; i++)
                        {
                            if (EventDataModels[EventIndex].eventCommands[i].code ==
                                (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_END)
                            {
                                EventDataModels[EventIndex].eventCommands[i] = null;
                                var eventDataList =
                                    EventDataModels[EventIndex].eventCommands.ToList();
                                eventDataList.RemoveAll(s => s == null);
                                EventDataModels[EventIndex].eventCommands = eventDataList;
                                break;
                            }

                            EventDataModels[EventIndex].eventCommands[i] = null;
                        }
                }
            }

            //既に何方かが書かれているので邪魔しないようにイベントを導入
            if (escape && defeat)
            {
                var data = EventDataModels[EventIndex].eventCommands
                    .ToList();
                if (!escapeIsInText)
                {
                    var eventDatas = new List<EventDataModel.EventCommand>();
                    eventDatas.Add(new EventDataModel.EventCommand(0, new List<string>(),
                        new List<EventDataModel.EventCommandMoveRoute>()));
                    eventDatas.Add(new EventDataModel.EventCommand(0, new List<string>(),
                        new List<EventDataModel.EventCommandMoveRoute>()));
                    SetEventData(ref eventDatas, 0,
                        (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_ESCAPE);
                    SetEventData(ref eventDatas, 1, 0);
                    //敗北の上に追加または終了の上に追加
                    if (defeatIsInText)
                        for (var i = 0; i < 2; i++)
                            data.Insert(defeatIndex - 1, eventDatas[i]);
                    else
                        for (var i = 0; i < 2; i++)
                            data.Insert(endIndex - 1, eventDatas[i]);
                }

                if (!defeatIsInText)
                {
                    var eventDatas = new List<EventDataModel.EventCommand>();
                    eventDatas.Add(new EventDataModel.EventCommand(0, new List<string>(),
                        new List<EventDataModel.EventCommandMoveRoute>()));
                    eventDatas.Add(new EventDataModel.EventCommand(0, new List<string>(),
                        new List<EventDataModel.EventCommandMoveRoute>()));
                    SetEventData(ref eventDatas, 0,
                        (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_LOSE);
                    SetEventData(ref eventDatas, 1, 0);
                    //Endの上に追加
                    for (var i = 0; i < 2; i++)
                        data.Insert(endIndex - 1, eventDatas[i]);
                }

                EventDataModels[EventIndex].eventCommands = data;
            }
            //逃走のトグルがON且つ既にイベントが書かれてなければ以下を処理する
            else if (escape && !escapeIsInText)
            {
                var data = EventDataModels[EventIndex].eventCommands.ToList();
                var eventDatas = new List<EventDataModel.EventCommand>();
                for (var i = 0; i < 5; i++)
                    eventDatas.Add(new EventDataModel.EventCommand(0, new List<string>(),
                        new List<EventDataModel.EventCommandMoveRoute>()));
                SetEventData(ref eventDatas, 0,
                    (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_WIN);
                SetEventData(ref eventDatas, 1, 0);
                SetEventData(ref eventDatas, 2,
                    (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_ESCAPE);
                SetEventData(ref eventDatas, 3, 0);
                SetEventData(ref eventDatas, 4,
                    (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_END);
                for (var i = 0; i < 5; i++)
                    data.Insert(EventCommandIndex + i + 1, eventDatas[i]);
                EventDataModels[EventIndex].eventCommands = data;
            }
            //敗北のトグルがON且つ既にイベントが書かれてなければ以下を処理する
            else if (defeat && !defeatIsInText)
            {
                var data = EventDataModels[EventIndex].eventCommands.ToList();
                var eventDatas = new List<EventDataModel.EventCommand>();
                for (var i = 0; i < 5; i++)
                    eventDatas.Add(new EventDataModel.EventCommand(0, new List<string>(),
                        new List<EventDataModel.EventCommandMoveRoute>()));
                SetEventData(ref eventDatas, 0,
                    (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_WIN);
                SetEventData(ref eventDatas, 1, 0);
                SetEventData(ref eventDatas, 2,
                    (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_LOSE);
                SetEventData(ref eventDatas, 3, 0);
                SetEventData(ref eventDatas, 4,
                    (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_END);
                for (var i = 0; i < 5; i++)
                    data.Insert(EventCommandIndex + i + 1, eventDatas[i]);
                EventDataModels[EventIndex].eventCommands = data;
            }
        }

        private void SetEventData(ref List<EventDataModel.EventCommand> dataList, int index, int code) {
            dataList[index].code = code;
            dataList[index].parameters = new List<string>();
        }
    }
}