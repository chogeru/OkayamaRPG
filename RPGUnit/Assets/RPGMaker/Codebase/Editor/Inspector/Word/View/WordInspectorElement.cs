using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.WordDefinition;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.Word.View
{
    /// <summary>
    /// [初期設定]-[用語] Inspector
    /// </summary>
    public class WordInspectorElement : AbstractInspectorElement
    {
        private readonly List<string> _basicStrArray = new List<string>
        {
            "Level",
            "LevelShort",
            "Hp",
            "HpShort",
            "Mp",
            "MpShort",
            "Tp",
            "TpShort",
            "Exp",
            "ExpShort",
            "Money"
        };

        private readonly List<string> _commandStrArray = new List<string>
        {
            "Battle",
            "Escape",
            "Attack",
            "Guard",
            "Item",
            "Skill",
            "Equipment",
            "Status",
            "Sort",
            "Option",
            "Save",
            "GameEnd",
            "Weapon",
            "Armor",
            "KeyItem",
            "Equipment2",
            "StrongestEquipment",
            "RemoveAll",
            "Buy",
            "Sell",
            "NewGame",
            "MenuContinue",
            "BackTitle",
            "Pause",
            "AlwaysDash",
            "SaveCommand",
            "VolumeBgm",
            "VolumeBgs",
            "VolumeMe",
            "VolumeSe",
            "PosessionNum"
        };

        private readonly List<string> _messageStrArray = new List<string>
        {
            "ExpTotal",
            "ExpNext",
            "SaveMessage",
            "LoadMessage",
            "File",
            "PartyName",
            "Emerge",
            "Preemptive",
            "Surprise",
            "EscapeStart",
            "EscapeFailure",
            "Victory",
            "Defeat",
            "ObtainExp",
            "ObtainGold",
            "ObtainItem",
            "LevelUp",
            "ObtainSkill",
            "UseItem",
            "CriticalToEnemy",
            "CriticalToActor",
            "ActorDamage",
            "ActorRecovery",
            "ActorGain",
            "ActorLoss",
            "ActorDrain",
            "ActorNoDamage",
            "ActorNoHit",
            "EnemyDamage",
            "EnemyRecovery",
            "EnemyGain",
            "EnemyLoss",
            "EnemyDrain",
            "EnemyNoDamage",
            "EnemyNoHit",
            "Evasion",
            "MagicEvasion",
            "MagicReflection",
            "CounterAttack",
            "Substitute",
            "BuffAdd",
            "DebuffAdd",
            "BuffRemove",
            "ActionFailure"
        };

        private readonly int         _num;

        private readonly List<string> _statusStrArray = new List<string>
        {
            "MaxHp",
            "MaxMp",
            "Attack",
            "Guard",
            "Magic",
            "MagicGuard",
            "Speed",
            "Luck",
            "Hit",
            "Evasion"
        };

        private WordDefinitionDataModel _wordDefinitionDataModel;

        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/Word/Asset/inspector_word.uxml"; } }

        public WordInspectorElement(int num) {
            _num = num;
            Refresh();
        }

        override protected void RefreshContents() {
            base.RefreshContents();
            _wordDefinitionDataModel = databaseManagementService.LoadWordDefinition();

            switch (_num)
            {
                case 0:
                    CreateBasicStates();
                    break;
                case 1:
                    CreateStatus();
                    break;
                case 2:
                    CreateCommand();
                    break;
                case 3:
                    CreateMessagesNomal();
                    break;
                case 4:
                    CreateMessagesBattle();
                    break;
            }
        }

        private void CreateBasicStates() {
            base.InitializeContents();

            VisualElement basicStatusFoldout = RootContainer.Query<VisualElement>("basic_status_foldout");
            VisualElement commandsFoldout = RootContainer.Query<VisualElement>("commands_foldout");
            VisualElement statusFoldout = RootContainer.Query<VisualElement>("status_foldout");
            VisualElement messagesNomalFoldout = RootContainer.Query<VisualElement>("messages_nomal_foldout");
            VisualElement messagesBattleFoldout = RootContainer.Query<VisualElement>("messages_battle_foldout");
            basicStatusFoldout.style.display = DisplayStyle.Flex;
            commandsFoldout.style.display = DisplayStyle.None;
            statusFoldout.style.display = DisplayStyle.None;
            messagesNomalFoldout.style.display = DisplayStyle.None;
            messagesBattleFoldout.style.display = DisplayStyle.None;

            var toggles = new List<Toggle>();
            var fields = new List<ImTextField>();
            for (var i = 0; i < _basicStrArray.Count; i++)
            {
                toggles.Add(RootContainer.Query<Toggle>("basic_states_toggle_" + i));
                fields.Add(RootContainer.Query<ImTextField>("basic_states_" + i));
            }

            for (var i = 0; i < _basicStrArray.Count; i++)
            {
                //初期値
                toggles[i].value = _wordDefinitionDataModel.basicStatus.GetData(_basicStrArray[i])
                    .enabled == 1;
                fields[i].SetEnabled(toggles[i].value);
                //ワードの初期値
                fields[i].value = _wordDefinitionDataModel.basicStatus.GetData(_basicStrArray[i]).value;

                var toggle = toggles[i];
                toggle.name = i.ToString();

                //トグル投下処理
                toggle.RegisterValueChangedCallback(evt =>
                {
                    var num = int.Parse(toggle.name);

                    _wordDefinitionDataModel.basicStatus.GetData(_basicStrArray[num]).enabled =
                        toggle.value ? 1 : 0;
                    fields[num].SetEnabled(toggle.value);

                    Save();
                });

                //テキスト入力
                var textField = fields[i];
                textField.name = i.ToString();
                textField.RegisterCallback<FocusOutEvent>(evt =>
                {
                    var num = int.Parse(textField.name);
                    _wordDefinitionDataModel.basicStatus.GetData(_basicStrArray[num]).value =
                        fields[num].value;
                    Save();
                });
            }
        }

        private void CreateCommand() {
            base.InitializeContents();

            VisualElement basicStatusFoldout = RootContainer.Query<VisualElement>("basic_status_foldout");
            VisualElement commandsFoldout = RootContainer.Query<VisualElement>("commands_foldout");
            VisualElement statusFoldout = RootContainer.Query<VisualElement>("status_foldout");
            VisualElement messagesNomalFoldout = RootContainer.Query<VisualElement>("messages_nomal_foldout");
            VisualElement messagesBattleFoldout = RootContainer.Query<VisualElement>("messages_battle_foldout");
            basicStatusFoldout.style.display = DisplayStyle.None;
            commandsFoldout.style.display = DisplayStyle.Flex;
            statusFoldout.style.display = DisplayStyle.None;
            messagesNomalFoldout.style.display = DisplayStyle.None;
            messagesBattleFoldout.style.display = DisplayStyle.None;

            var toggles = new List<Toggle>();
            var fields = new List<ImTextField>();
            for (var i = 0; i < _commandStrArray.Count; i++)
            {
                toggles.Add(RootContainer.Query<Toggle>("command_toggle_" + i));
                fields.Add(RootContainer.Query<ImTextField>("command_" + i));
            }

            for (var i = 0; i < _commandStrArray.Count; i++)
            {
                //初期値
                toggles[i].value = _wordDefinitionDataModel.commands.GetData(_commandStrArray[i])
                    .enabled == 1;
                fields[i].SetEnabled(toggles[i].value);
                //ワードの初期値
                fields[i].value = _wordDefinitionDataModel.commands.GetData(_commandStrArray[i]).value;

                var toggle = toggles[i];
                toggle.name = i.ToString();

                //トグル投下処理
                toggle.RegisterValueChangedCallback(evt =>
                {
                    var num = int.Parse(toggle.name);

                    _wordDefinitionDataModel.commands.GetData(_commandStrArray[num]).enabled =
                        toggle.value ? 1 : 0;
                    fields[num].SetEnabled(toggle.value);

                    Save();
                });

                //テキスト入力
                var textField = fields[i];
                textField.name = i.ToString();
                textField.RegisterCallback<FocusOutEvent>(evt =>
                {
                    var num = int.Parse(textField.name);
                    _wordDefinitionDataModel.commands.GetData(_commandStrArray[num]).value =
                        fields[num].value;
                    Save();
                });
            }
        }

        private void CreateStatus() {
            base.InitializeContents();

            VisualElement basicStatusFoldout = RootContainer.Query<VisualElement>("basic_status_foldout");
            VisualElement commandsFoldout = RootContainer.Query<VisualElement>("commands_foldout");
            VisualElement statusFoldout = RootContainer.Query<VisualElement>("status_foldout");
            VisualElement messagesNomalFoldout = RootContainer.Query<VisualElement>("messages_nomal_foldout");
            VisualElement messagesBattleFoldout = RootContainer.Query<VisualElement>("messages_battle_foldout");
            basicStatusFoldout.style.display = DisplayStyle.None;
            commandsFoldout.style.display = DisplayStyle.None;
            statusFoldout.style.display = DisplayStyle.Flex;
            messagesNomalFoldout.style.display = DisplayStyle.None;
            messagesBattleFoldout.style.display = DisplayStyle.None;

            var toggles = new List<Toggle>();
            var fields = new List<ImTextField>();
            for (var i = 0; i < _statusStrArray.Count; i++)
            {
                toggles.Add(RootContainer.Query<Toggle>("status_toggle_" + i));
                fields.Add(RootContainer.Query<ImTextField>("status_" + i));
            }

            for (var i = 0; i < _statusStrArray.Count; i++)
            {
                //初期値
                toggles[i].value = _wordDefinitionDataModel.status.GetData(_statusStrArray[i])
                    .enabled == 1;
                fields[i].SetEnabled(toggles[i].value);
                //ワードの初期値
                fields[i].value = _wordDefinitionDataModel.status.GetData(_statusStrArray[i]).value;

                var toggle = toggles[i];
                toggle.name = i.ToString();

                //トグル投下処理
                toggle.RegisterValueChangedCallback(evt =>
                {
                    var num = int.Parse(toggle.name);
                    _wordDefinitionDataModel.status.GetData(_statusStrArray[num]).enabled =
                        toggle.value ? 1 : 0;
                    fields[num].SetEnabled(toggle.value);

                    Save();
                });

                //テキスト入力
                var textField = fields[i];
                textField.name = i.ToString();
                textField.RegisterCallback<FocusOutEvent>(evt =>
                {
                    var num = int.Parse(textField.name);
                    _wordDefinitionDataModel.status.GetData(_statusStrArray[num]).value =
                        fields[num].value;
                    Save();
                });
            }
        }


        private void CreateMessagesNomal() {
            base.InitializeContents();

            VisualElement basicStatusFoldout = RootContainer.Query<VisualElement>("basic_status_foldout");
            VisualElement commandsFoldout = RootContainer.Query<VisualElement>("commands_foldout");
            VisualElement statusFoldout = RootContainer.Query<VisualElement>("status_foldout");
            VisualElement messagesNomalFoldout = RootContainer.Query<VisualElement>("messages_nomal_foldout");
            VisualElement messagesBattleFoldout = RootContainer.Query<VisualElement>("messages_battle_foldout");
            basicStatusFoldout.style.display = DisplayStyle.None;
            commandsFoldout.style.display = DisplayStyle.None;
            statusFoldout.style.display = DisplayStyle.None;
            messagesNomalFoldout.style.display = DisplayStyle.Flex;
            messagesBattleFoldout.style.display = DisplayStyle.None;

            var toggles = new List<Toggle>();
            var fields = new List<ImTextField>();
            for (var i = 0; i < 6; i++)
            {
                toggles.Add(RootContainer.Query<Toggle>("messages_toggle_" + i));
                fields.Add(RootContainer.Query<ImTextField>("messages_" + i));
            }

            for (var i = 0; i < 6; i++)
            {
                //初期値
                toggles[i].value = _wordDefinitionDataModel.messages.GetData(_messageStrArray[i])
                    .enabled == 1;
                fields[i].SetEnabled(toggles[i].value);
                //ワードの初期値
                fields[i].value = _wordDefinitionDataModel.messages.GetData(_messageStrArray[i]).value;

                var toggle = toggles[i];
                toggle.name = i.ToString();

                //トグル投下処理
                toggle.RegisterValueChangedCallback(evt =>
                {
                    var num = int.Parse(toggle.name);

                    _wordDefinitionDataModel.messages.GetData(_messageStrArray[num]).enabled =
                        toggle.value ? 1 : 0;
                    fields[num].SetEnabled(toggle.value);

                    Save();
                });

                //テキスト入力
                var textField = fields[i];
                textField.name = i.ToString();
                textField.RegisterCallback<FocusOutEvent>(evt =>
                {
                    var num = int.Parse(textField.name);
                    _wordDefinitionDataModel.messages.GetData(_messageStrArray[num]).value =
                        fields[num].value;
                    Save();
                });
            }
        }

        private void CreateMessagesBattle() {
            base.InitializeContents();

            VisualElement basicStatusFoldout = RootContainer.Query<VisualElement>("basic_status_foldout");
            VisualElement commandsFoldout = RootContainer.Query<VisualElement>("commands_foldout");
            VisualElement statusFoldout = RootContainer.Query<VisualElement>("status_foldout");
            VisualElement messagesNomalFoldout = RootContainer.Query<VisualElement>("messages_nomal_foldout");
            VisualElement messagesBattleFoldout = RootContainer.Query<VisualElement>("messages_battle_foldout");
            basicStatusFoldout.style.display = DisplayStyle.None;
            commandsFoldout.style.display = DisplayStyle.None;
            statusFoldout.style.display = DisplayStyle.None;
            messagesNomalFoldout.style.display = DisplayStyle.None;
            messagesBattleFoldout.style.display = DisplayStyle.Flex;

            var toggles = new List<Toggle>();
            var fields = new List<ImTextField>();
            for (var i = 0; i < _messageStrArray.Count; i++)
            {
                toggles.Add(RootContainer.Query<Toggle>("messages_toggle_" + i));
                fields.Add(RootContainer.Query<ImTextField>("messages_" + i));
            }

            for (var i = 6; i < _messageStrArray.Count; i++)
            {
                //初期値
                toggles[i].value = _wordDefinitionDataModel.messages.GetData(_messageStrArray[i])
                    .enabled == 1;
                fields[i].SetEnabled(toggles[i].value);
                //ワードの初期値
                fields[i].value = _wordDefinitionDataModel.messages.GetData(_messageStrArray[i]).value;

                var toggle = toggles[i];
                toggle.name = i.ToString();

                //トグル投下処理
                toggle.RegisterValueChangedCallback(evt =>
                {
                    var num = int.Parse(toggle.name);

                    _wordDefinitionDataModel.messages.GetData(_messageStrArray[num]).enabled =
                        toggle.value ? 1 : 0;
                    fields[num].SetEnabled(toggle.value);

                    Save();
                });
                var textField = fields[i];
                textField.name = i.ToString();
                textField.RegisterCallback<FocusOutEvent>(evt =>
                {
                    var num = int.Parse(textField.name);
                    _wordDefinitionDataModel.messages.GetData(_messageStrArray[num]).value =
                        fields[num].value;
                    Save();
                });
            }
        }

        override protected void SaveContents() {
            //セーブ部位の作成
            databaseManagementService.SaveWord(_wordDefinitionDataModel);
        }
    }
}