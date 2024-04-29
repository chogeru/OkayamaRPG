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
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event.EventDataModel;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Actor
{
    /// <summary>
    ///     [アクター設定の変更]コマンドのコマンド設定枠の表示物
    ///     実行内容枠での表示について、全てにチェックが付いている場合は
    ///     ◆アクター設定の変更：名前を表示（この行を基点とする）
    ///     インデントをずらして二つ名を表示
    ///     インデントをずらしてプロフィールを表示
    ///     のように表示する。イベントコードやパラメータの状態で基点かそうでないかを判定し、諸々の処理を行う
    /// </summary>
    public class ChangeName : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_change_name.uxml";

        private const EventEnum CHANGE_ACTOR_NAME     = EventEnum.EVENT_CODE_ACTOR_CHANGE_NAME;
        private const EventEnum CHANGE_ACTOR_NICKNAME = EventEnum.EVENT_CODE_ACTOR_CHANGE_NICKNAME;
        private const EventEnum CHANGE_ACTOR_PROFILE  = EventEnum.EVENT_CODE_ACTOR_CHANGE_PROFILE;

        private readonly Dictionary<EventEnum, bool> _showCommandDic = new Dictionary<EventEnum, bool>
        {
            {EventEnum.EVENT_CODE_ACTOR_CHANGE_NAME, false},
            {EventEnum.EVENT_CODE_ACTOR_CHANGE_NICKNAME, false},
            {EventEnum.EVENT_CODE_ACTOR_CHANGE_PROFILE, false}
        };


        private EventCommand _targetCommand;
        private int          _targetIndex = -1;

        public ChangeName(
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

            var characterActorDataModels = DatabaseManagementService.LoadCharacterActor().FindAll(actor => actor.charaType == (int) ActorTypeEnum.ACTOR);

            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();

            _targetIndex = GetBaseCommandIndex(EventDataModels[EventIndex].eventCommands, EventCommandIndex);
            _targetCommand = EventDataModels[EventIndex].eventCommands[_targetIndex];
            if (_targetCommand.parameters.Count == 0)
            {
                _targetCommand.parameters.Add(characterActorDataModels[0].uuId);
                //名前
                _targetCommand.parameters.Add("0");
                _targetCommand.parameters.Add("");
                //ニックネーム
                _targetCommand.parameters.Add("0");
                _targetCommand.parameters.Add("");
                //プロフィール
                _targetCommand.parameters.Add("0");
                _targetCommand.parameters.Add("");
                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            // 表示中判定用のフラグの初期化
            var commandCount = 0;
            _showCommandDic[CHANGE_ACTOR_NAME] = _targetCommand.parameters[1] == "1";
            if (_showCommandDic[CHANGE_ACTOR_NAME])
                commandCount++;

            _showCommandDic[CHANGE_ACTOR_NICKNAME] = _targetCommand.parameters[3] == "1";
            if (_showCommandDic[CHANGE_ACTOR_NICKNAME])
                commandCount++;

            _showCommandDic[CHANGE_ACTOR_PROFILE] = _targetCommand.parameters[5] == "1";
            if (_showCommandDic[CHANGE_ACTOR_PROFILE])
                commandCount++;

            if (commandCount == 1 && !_showCommandDic[CHANGE_ACTOR_NAME])
            {
                _showCommandDic[CHANGE_ACTOR_NICKNAME] = false;
                _showCommandDic[CHANGE_ACTOR_PROFILE] = false;
            }


            VisualElement actor = RootElement.Query<VisualElement>("actor");

            var characterActorNameList = new List<string>();
            var characterActorIDList = new List<string>();
            for (var i = 0; i < characterActorDataModels.Count; i++)
            {
                characterActorNameList.Add(characterActorDataModels[i].basic.name);
                characterActorIDList.Add(characterActorDataModels[i].uuId);
            }

            var selectID = characterActorIDList.IndexOf(_targetCommand.parameters[0]);
            if (selectID == -1)
                selectID = 0;
            var actorPopupField = new PopupFieldBase<string>(characterActorNameList, selectID);
            actor.Clear();
            actor.Add(actorPopupField);
            actorPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[0] = characterActorIDList[actorPopupField.index];
                Save(EventDataModels[EventIndex]);
            });

            Toggle name_toggle = RootElement.Query<Toggle>("name_toggle");
            Toggle nickname_toggle = RootElement.Query<Toggle>("nickname_toggle");
            Toggle profile_toggle = RootElement.Query<Toggle>("profile_toggle");
            ImTextField name = RootElement.Query<ImTextField>("name");
            ImTextField nickname = RootElement.Query<ImTextField>("nickname");
            ImTextField profile = RootElement.Query<ImTextField>("profile");

            //初期表示の読み込み
            //名前
            name.value = _targetCommand.parameters[2];
            name_toggle.value = _targetCommand.parameters[1] == "1";
            name.SetEnabled(name_toggle.value);

            //ニックネーム
            nickname.value = _targetCommand.parameters[4];
            nickname_toggle.value = _targetCommand.parameters[3] == "1";
            nickname.SetEnabled(nickname_toggle.value);

            //プロフィール
            profile.value = _targetCommand.parameters[6];
            profile_toggle.value = _targetCommand.parameters[5] == "1";
            profile.SetEnabled(profile_toggle.value);

            name_toggle.RegisterValueChangedCallback(o =>
            {
                name.SetEnabled(name_toggle.value);
                _targetCommand.parameters[1] = name_toggle.value ? "1" : "0";
                _targetCommand.parameters[2] = name.value;

                if (name_toggle.value)
                    AddEventCommand(CHANGE_ACTOR_NAME);
                else
                    SubEventCommand(CHANGE_ACTOR_NAME);
                ApplyParameter();
                Save(EventDataModels[EventIndex]);
            });
            nickname_toggle.RegisterValueChangedCallback(o =>
            {
                nickname.SetEnabled(nickname_toggle.value);
                _targetCommand.parameters[3] = nickname_toggle.value ? "1" : "0";
                _targetCommand.parameters[4] = nickname.value;

                if (nickname_toggle.value)
                    AddEventCommand(CHANGE_ACTOR_NICKNAME);
                else
                    SubEventCommand(CHANGE_ACTOR_NICKNAME);
                ApplyParameter();
                Save(EventDataModels[EventIndex]);
            });
            profile_toggle.RegisterValueChangedCallback(o =>
            {
                profile.SetEnabled(profile_toggle.value);
                _targetCommand.parameters[5] = profile_toggle.value ? "1" : "0";
                _targetCommand.parameters[6] = profile.value;

                if (profile_toggle.value)
                    AddEventCommand(CHANGE_ACTOR_PROFILE);
                else
                    SubEventCommand(CHANGE_ACTOR_PROFILE);
                ApplyParameter();
                Save(EventDataModels[EventIndex]);
            });

            name.RegisterCallback<FocusOutEvent>(evt =>
            {
                _targetCommand.parameters[2] = name.value;
                ApplyParameter();
                Save(EventDataModels[EventIndex]);
            });

            nickname.RegisterCallback<FocusOutEvent>(evt =>
            {
                _targetCommand.parameters[4] = nickname.value;
                ApplyParameter();
                Save(EventDataModels[EventIndex]);
            });

            profile.RegisterCallback<FocusOutEvent>(evt =>
            {
                _targetCommand.parameters[6] = profile.value;
                ApplyParameter();
                Save(EventDataModels[EventIndex]);
            });
        }

        /// <summary>
        ///     基点のコマンドを別のイベントコードに変更する
        /// </summary>
        private void EditBaseCommand(EventEnum eventCode) {
            var showNickname = _showCommandDic[CHANGE_ACTOR_NICKNAME] || _targetCommand.parameters[3] == "1";
            var showProfile = _showCommandDic[CHANGE_ACTOR_PROFILE] || _targetCommand.parameters[5] == "1";

            _targetCommand.code = (int) eventCode;
            // 先頭の変更に合わせて先頭だった表示内容を後続として生成する
            switch (eventCode)
            {
                case CHANGE_ACTOR_NAME:
                    _showCommandDic[CHANGE_ACTOR_NAME] = !_showCommandDic[CHANGE_ACTOR_NAME];
                    if (showNickname)
                        AddEventCommand(CHANGE_ACTOR_NICKNAME);
                    else if (showProfile) AddEventCommand(CHANGE_ACTOR_PROFILE);
                    break;

                case CHANGE_ACTOR_NICKNAME:
                    if (_showCommandDic[CHANGE_ACTOR_NICKNAME]) SubEventCommand(CHANGE_ACTOR_NICKNAME);
                    if (showProfile) AddEventCommand(CHANGE_ACTOR_PROFILE);
                    _showCommandDic[CHANGE_ACTOR_NAME] = false;
                    break;

                case CHANGE_ACTOR_PROFILE:
                    if (_showCommandDic[CHANGE_ACTOR_NICKNAME]) SubEventCommand(CHANGE_ACTOR_NICKNAME);
                    if (_showCommandDic[CHANGE_ACTOR_PROFILE]) SubEventCommand(CHANGE_ACTOR_PROFILE);
                    _showCommandDic[CHANGE_ACTOR_NAME] = false;
                    break;
            }

            Save(EventDataModels[EventIndex]);
        }

        /// <summary>
        ///     コマンドを追加する
        /// </summary>
        private void AddEventCommand(EventEnum eventCode) {
            var toggleDic = new Dictionary<EventEnum, bool>
            {
                {EventEnum.EVENT_CODE_ACTOR_CHANGE_NAME, _targetCommand.parameters[1] == "1"},
                {EventEnum.EVENT_CODE_ACTOR_CHANGE_NICKNAME, _targetCommand.parameters[3] == "1"},
                {EventEnum.EVENT_CODE_ACTOR_CHANGE_PROFILE, _targetCommand.parameters[5] == "1"}
            };

            // 既に表示中の場合は処理をしない
            if (_showCommandDic[eventCode])
                return;
            // 有効な表示が無い状態なら先頭の行を変更する
            if (_targetCommand.code == (int) CHANGE_ACTOR_NAME && _showCommandDic.Count(v => v.Value) == 0 &&
                toggleDic.Count(v => v.Value) == 0)
            {
                EditBaseCommand(eventCode);
                return;
            }

            // 基点を[名前]に変更する
            if (eventCode == CHANGE_ACTOR_NAME)
            {
                EditBaseCommand(eventCode);
                return;
            }

            // 挿入先のインデックスを確認する
            var offset = 0;
            foreach (var pair in _showCommandDic.Where(v => v.Key == eventCode || v.Value))
            {
                if (pair.Key == eventCode || !pair.Value)
                    break;
                offset++;
            }

            // 先頭は[名前]以外表示中でも表示フラグがfalseになるので補正する
            if (offset == 0)
            {
                var togglePairs = toggleDic.Where(v => v.Value);
                if (togglePairs.First().Key == eventCode)
                {
                    EditBaseCommand(eventCode);
                    return;
                }

                offset++;
            }


            var insertIndex = _targetIndex + offset;
            // 基点からズラした行に追加する
            _showCommandDic[eventCode] = true;
            var command = new EventCommand((int) eventCode, _targetCommand.parameters,
                new List<EventCommandMoveRoute>());
            EventDataModels[EventIndex].eventCommands.Insert(insertIndex, command);
            Save(EventDataModels[EventIndex]);
        }

        /// <summary>
        ///     コマンドを削除する
        /// </summary>
        private void SubEventCommand(EventEnum eventCode) {
            // 有効な表示が無い状態なら基点の行は削除せず、[名前]を表示するだけ表示する
            if (_showCommandDic.Count(v => v.Value) == 0)
            {
                _targetCommand.code = (int) CHANGE_ACTOR_NAME;
                Save(EventDataModels[EventIndex]);
                return;
            }

            // 既に削除済みの場合は処理をしない
            if (!_showCommandDic[eventCode])
            {
                if (_targetCommand.code == (int) eventCode)
                {
                    // 先頭の行で且つ他に有効な表示がある場合は先頭のコマンドを変更する
                    var nextCommand = _showCommandDic.FirstOrDefault(v => v.Key != eventCode && v.Value);
                    if (!nextCommand.Equals(default(KeyValuePair<EventEnum, bool>)))
                        EditBaseCommand(nextCommand.Key);
                }

                return;
            }

            // 基点を別のコマンドに変更する
            if (eventCode == CHANGE_ACTOR_NAME)
            {
                var nextCommand = _showCommandDic.FirstOrDefault(v => v.Key != eventCode && v.Value);
                if (!nextCommand.Equals(default(KeyValuePair<EventEnum, bool>)))
                    EditBaseCommand(nextCommand.Key);
                else
                    EditBaseCommand(CHANGE_ACTOR_NAME);
                return;
            }

            // 削除元のインデックスを確認する
            var offset = 0;
            var showCommands = _showCommandDic.Where(v => v.Value);
            if (showCommands.Count() == 1 && showCommands.ElementAt(0).Key == eventCode)
                // 最後の1つの場合は先頭の1つ下にあるはず
                offset = 1;
            else
                foreach (var pair in showCommands)
                {
                    if (pair.Key == eventCode)
                        break;
                    offset++;
                }

            var deleteIndex = _targetIndex + offset;

            // 基点からズラした行を削除する
            if (EventDataModels[EventIndex].eventCommands[deleteIndex].code == (int) eventCode)
            {
                _showCommandDic[eventCode] = false;
                EventDataModels[EventIndex].eventCommands.RemoveAt(deleteIndex);
                Save(EventDataModels[EventIndex]);
            }
        }


        /// <summary>
        ///     基点となる[アクター設定の変更]のインデックスを取得する
        /// </summary>
        /// <param name="commands">イベントコマンド一覧</param>
        /// <param name="currentIndex">現在選択しているイベントコマンドのインデックス</param>
        /// <returns>1行目にあたる[アクター設定の変更]のコマンドのインデックス</returns>
        public static int GetBaseCommandIndex(IEnumerable<EventCommand> commands, int currentIndex) {
            var currentCommand = commands.ElementAt(currentIndex);
            var code = (EventEnum) Enum.ToObject(typeof(EventEnum), currentCommand.code);
            var offset = 0;

            var nameToggleValue = currentCommand.parameters.Count == 0 ? false : currentCommand.parameters[1] == "1";
            var nicknameToggleValue =
                currentCommand.parameters.Count == 0 ? false : currentCommand.parameters[3] == "1";

            switch (code)
            {
                case CHANGE_ACTOR_NAME:
                    // [名前]が有効もしくは全て無効な場合は1行目は必ずEVENT_CODE_ACTOR_CHANGE_NAMEになる
                    return currentIndex;

                case CHANGE_ACTOR_NICKNAME:
                    if (nameToggleValue) offset++;
                    break;

                case CHANGE_ACTOR_PROFILE:
                    if (nameToggleValue) offset++;
                    if (nicknameToggleValue) offset++;
                    break;
            }

            // 手前に表示される分を加味したインデックスが先頭のインデックス
            return currentIndex - offset;
        }

        /// <summary>
        ///     関連コマンドにparametersの参照を渡す
        /// </summary>
        private void ApplyParameter() {
            var commandCount = 0;
            if (_targetCommand.parameters[1] == "1")
                commandCount++;
            if (_targetCommand.parameters[3] == "1")
                commandCount++;
            if (_targetCommand.parameters[5] == "1")
                commandCount++;

            for (var i = _targetIndex; commandCount > 0; commandCount--, i++)
            {
                var tmpCommand = EventDataModels[EventIndex].eventCommands[i];
                // 編集中以外のコマンドに編集中のコマンドのparametersの参照を渡す
                if (tmpCommand.code != _targetCommand.code) tmpCommand.parameters = _targetCommand.parameters;
            }
        }
    }
}