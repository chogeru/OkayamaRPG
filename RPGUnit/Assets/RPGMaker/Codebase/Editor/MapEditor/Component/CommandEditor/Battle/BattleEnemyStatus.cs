using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event.EventDataModel;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Battle
{
    /// <summary>
    ///     [敵キャラのステータス増減]コマンドのコマンド設定枠の表示物
    /// </summary>
    public class BattleEnemyStatus : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_battle_enemy_status.uxml";

        private EventCommand _targetCommand;
        private bool         isHPCommandActive;
        private bool         isMPCommandActive;
        private bool         isTPCommandActive;

        public BattleEnemyStatus(
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

            var EnemyDropdownChoices = GetEnemyNameList();
            var variables = _GetVariablesList();
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();

            _targetCommand = EventDataModels[EventIndex].eventCommands[EventCommandIndex];
            if (_targetCommand.parameters.Count < 26)
            {
                _targetCommand.parameters.Add("0");
                for (var i = 1; i < 26; i++)
                    _targetCommand.parameters.Add("0");

                _targetCommand.parameters[1] = "True";
                _targetCommand.parameters[2] = "up";
                _targetCommand.parameters[4] = "True";
                _targetCommand.parameters[5] = "1";
                _targetCommand.parameters[9] = variables[0].id;
                _targetCommand.parameters[10] = "False";
                _targetCommand.parameters[11] = "up";
                _targetCommand.parameters[13] = "True";
                _targetCommand.parameters[14] = "1";
                _targetCommand.parameters[18] = variables[0].id;
                _targetCommand.parameters[19] = "False";
                _targetCommand.parameters[20] = "up";
                _targetCommand.parameters[22] = "True";
                _targetCommand.parameters[23] = "1";
                _targetCommand.parameters[25] = variables[0].id;

                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            var count = 0;
            if (_targetCommand.parameters[1] == "True")
            {
                count++;
                isHPCommandActive = true;
            }

            if (_targetCommand.parameters[10] == "True")
            {
                count++;
                isMPCommandActive = true;
            }

            if (_targetCommand.parameters[19] == "True")
            {
                count++;
                isTPCommandActive = true;
            }

            if (count == 1 && !isHPCommandActive)
            {
                isHPCommandActive = false;
                isMPCommandActive = false;
                isTPCommandActive = false;
            }
            
            if (EnemyDropdownChoices.Count == 0)
            {
                VisualElement enemyArea = RootElement.Q<VisualElement>("battle_enemy_status")
                    .Query<VisualElement>("enemy_area");
                enemyArea.style.display = DisplayStyle.None;
                _targetCommand.parameters[0] = "0";
                Save(EventDataModels[EventIndex]);
                return;
            }

            //エネミードロップダウン
            VisualElement EnemySelect = RootElement.Q<VisualElement>("battle_enemy_status")
                .Query<VisualElement>("enemy_select");
            var EnemyListId = int.Parse(_targetCommand.parameters[0]);
            

            //選択肢に名前を表示売る際に一時的に使用するList
            var EnemyDropdownPopupField = new PopupFieldBase<string>(EnemyDropdownChoices, EnemyListId);
            EnemySelect.Clear();
            EnemySelect.Add(EnemyDropdownPopupField);
            EnemyDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[0] = EnemyDropdownPopupField.index.ToString();
                EditEvent();
                Save(EventDataModels[EventIndex]);
            });

            Toggle HP_toggle = RootElement.Q<VisualElement>("battle_enemy_status").Query<Toggle>("HP_toggle");
            switch (_targetCommand.parameters[1])
            {
                case "True":
                    HP_toggle.value = true;
                    break;
                case "False":
                    HP_toggle.value = false;
                    break;
            }

            VisualElement HP_area = RootElement.Q<VisualElement>("battle_enemy_status").Query<VisualElement>("HP_area");
            if (HP_toggle.value) HP_area.style.display = DisplayStyle.Flex;

            if (!HP_toggle.value) HP_area.style.display = DisplayStyle.None;

            //増やす・減らす
            RadioButton HP_up = RootElement.Q<VisualElement>("battle_enemy_status").Query<RadioButton>("radioButton-eventCommand-display143");
            RadioButton HP_down = RootElement.Q<VisualElement>("battle_enemy_status").Query<RadioButton>("radioButton-eventCommand-display144");
            //戦闘不能
            Toggle HP_unable_fight =
                RootElement.Q<VisualElement>("battle_enemy_status").Query<Toggle>("HP_unable_fight");
            //初期値
            switch (_targetCommand.parameters[2])
            {
                case "up":
                    HP_up.value = true;
                    HP_down.value = false;
                    HP_unable_fight.SetEnabled(false);
                    break;
                case "down":
                    HP_up.value = false;
                    HP_down.value = true;
                    HP_unable_fight.SetEnabled(true);
                    break;
            }

            switch (_targetCommand.parameters[3])
            {
                case "True":
                    HP_unable_fight.value = true;
                    break;
                case "False":
                    HP_unable_fight.value = false;
                    break;
            }
            
            var defaultHpUpDown =_targetCommand.parameters[2] == "up" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {HP_up, HP_down},
                defaultHpUpDown, new List<System.Action>
                {
                    //増やす
                    () =>
                    {
                        HP_unable_fight.SetEnabled(true);
                        _targetCommand.parameters[2] = "up";
                        HP_unable_fight.SetEnabled(false);
                        HP_unable_fight.value = false;
                        EditEvent();
                        Save(EventDataModels[EventIndex]);
                    },
                    //減らす
                    () =>
                    {
                        HP_unable_fight.SetEnabled(false);
                        _targetCommand.parameters[2] = "down";
                        HP_up.value = false;
                        HP_unable_fight.SetEnabled(true);

                        EditEvent();
                        Save(EventDataModels[EventIndex]);
                    }
                });

            HP_unable_fight.RegisterValueChangedCallback(o =>
            {
                _targetCommand.parameters[3] = HP_unable_fight.value.ToString();
                EditEvent();
                Save(EventDataModels[EventIndex]);
            });
            //オペランド
            //定数
            RadioButton HP_constant = RootElement.Q<VisualElement>("battle_enemy_status").Query<RadioButton>("radioButton-eventCommand-display145");
            IntegerField HP_constant_value = RootElement.Q<VisualElement>("battle_enemy_status").Query<IntegerField>("HP_constant_value");
            //最大HPに対する割合
            RadioButton HP_rate = RootElement.Q<VisualElement>("battle_enemy_status").Query<RadioButton>("radioButton-eventCommand-display146");
            IntegerField HP_rate_value = RootElement.Q<VisualElement>("battle_enemy_status").Query<IntegerField>("HP_rate_value");
            //変数
            RadioButton HP_variable = RootElement.Q<VisualElement>("battle_enemy_status").Query<RadioButton>("radioButton-eventCommand-display147");
            VisualElement HP_variable_dropdown = RootElement.Q<VisualElement>("battle_enemy_status")
                .Query<VisualElement>("HP_variable_dropdown");

            //初期値
            if (_targetCommand.parameters[4] == "True")
            {
                HP_constant_value.SetEnabled(true);
                HP_rate_value.SetEnabled(false);
                HP_variable_dropdown.SetEnabled(false);
            }

            if (_targetCommand.parameters[6] == "True")
            {
                HP_constant_value.SetEnabled(false);
                HP_rate_value.SetEnabled(true);
                HP_variable_dropdown.SetEnabled(false);
            }

            if (_targetCommand.parameters[8] == "True")
            {
                HP_constant_value.SetEnabled(false);
                HP_rate_value.SetEnabled(false);
                HP_variable_dropdown.SetEnabled(true);
            }

            HP_constant_value.value = int.Parse(_targetCommand.parameters[5]);
            HP_rate_value.value = int.Parse(_targetCommand.parameters[7]);
            
            var defaultHpConstant = _targetCommand.parameters[4] == "True" ? 0 : _targetCommand.parameters[6] == "True" ? 1 : 2;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {HP_constant, HP_rate, HP_variable},
                defaultHpConstant, new List<System.Action>
                {
                    //定数
                    () =>
                    {
                        _targetCommand.parameters[4] = HP_constant.value.ToString();
                        _targetCommand.parameters[6] = "False";
                        _targetCommand.parameters[8] = "False";
                        HP_constant_value.SetEnabled(true);
                        HP_rate_value.SetEnabled(false);
                        HP_variable_dropdown.SetEnabled(false);
                        EditEvent();
                        Save(EventDataModels[EventIndex]);
                    },
                    //最大HPに対する割合
                    () =>
                    {
                        _targetCommand.parameters[4] = "False";
                        _targetCommand.parameters[6] = HP_rate.value.ToString();
                        _targetCommand.parameters[8] = "False";
                        HP_constant_value.SetEnabled(false);
                        HP_rate_value.SetEnabled(true);
                        HP_variable_dropdown.SetEnabled(false);
                        EditEvent();
                        Save(EventDataModels[EventIndex]);
                    },
                    //変数
                    () =>
                    {
                        _targetCommand.parameters[4] = "False";
                        _targetCommand.parameters[6] = "False";
                        _targetCommand.parameters[8] = HP_variable.value.ToString();
                        HP_constant_value.SetEnabled(false);
                        HP_rate_value.SetEnabled(false);
                        HP_variable_dropdown.SetEnabled(true);

                        EditEvent();
                        Save(EventDataModels[EventIndex]);
                    }

                });
            
            BaseInputFieldHandler.IntegerFieldCallback(HP_constant_value, evt =>
            {
                _targetCommand.parameters[5] = HP_constant_value.value.ToString();
                EditEvent();
                Save(EventDataModels[EventIndex]);
            }, 1, 1);
            
            BaseInputFieldHandler.IntegerFieldCallback(HP_rate_value, evt =>
            {
                _targetCommand.parameters[7] = HP_rate_value.value.ToString();
                EditEvent();
                Save(EventDataModels[EventIndex]);
            }, 0, 100);
            //変数リストドロップダウン
            var HPVariableDropdown = _GetVariablesList();
            var HPVariableListId = 0;
            var isHpVariable = false;
            for (var i = 0; i < HPVariableDropdown.Count; i++)
            {
                if (HPVariableDropdown[i].id == _targetCommand.parameters[9])
                {
                    HPVariableListId = i;
                    isHpVariable = true;
                    break;
                }
            }

            if (!isHpVariable)
            {
                _targetCommand.parameters[9] = HPVariableDropdown[0].id;
                Save(EventDataModels[EventIndex]);
            }

            //選択肢に名前を表示売る際に一時的に使用するList
            var HPVariableName = new List<string>();
            var HPVariableIDList = new List<string>();
            for (var i = 0; i < HPVariableDropdown.Count; i++)
            {
                if (HPVariableDropdown[i].name == "")
                    HPVariableName.Add(EditorLocalize.LocalizeText("WORD_1518"));
                else
                    HPVariableName.Add(HPVariableDropdown[i].name);

                HPVariableIDList.Add(HPVariableDropdown[i].id);
            }

            var HPVariableDropdownPopupField =
                new PopupFieldBase<string>(HPVariableName, HPVariableListId);
            HP_variable_dropdown.Clear();
            HP_variable_dropdown.Add(HPVariableDropdownPopupField);
            HPVariableDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[9] = HPVariableIDList[HPVariableDropdownPopupField.index];
                EditEvent();
                Save(EventDataModels[EventIndex]);
            });
            Toggle MP_toggle = RootElement.Q<VisualElement>("battle_enemy_status").Query<Toggle>("MP_toggle");
            switch (_targetCommand.parameters[10])
            {
                case "True":
                    MP_toggle.value = true;
                    break;
                case "False":
                    MP_toggle.value = false;
                    break;
            }

            VisualElement MP_area = RootElement.Q<VisualElement>("battle_enemy_status").Query<VisualElement>("MP_area");
            if (MP_toggle.value) MP_area.style.display = DisplayStyle.Flex;

            if (!MP_toggle.value) MP_area.style.display = DisplayStyle.None;
            
            //増やす・減らす
            RadioButton MP_up = RootElement.Q<VisualElement>("battle_enemy_status").Query<RadioButton>("radioButton-eventCommand-display148");
            RadioButton MP_down = RootElement.Q<VisualElement>("battle_enemy_status").Query<RadioButton>("radioButton-eventCommand-display149");

            //初期値
            switch (_targetCommand.parameters[11])
            {
                case "up":
                    MP_up.value = true;
                    MP_down.value = false;
                    break;
                case "down":
                    MP_up.value = false;
                    MP_down.value = true;
                    break;
            }
            
            var defaultMpUpDown =_targetCommand.parameters[11] == "up" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {MP_up, MP_down},
                defaultMpUpDown, new List<System.Action>
                {
                    //増やす
                    () =>
                    {
                        _targetCommand.parameters[11] = "up";
                        EditEvent();
                        Save(EventDataModels[EventIndex]);
                    },
                    //減らす
                    () =>
                    {
                        _targetCommand.parameters[11] = "down";
                        EditEvent();
                        Save(EventDataModels[EventIndex]);
                    }
                });

            //オペランド
            //定数
            RadioButton MP_constant = RootElement.Q<VisualElement>("battle_enemy_status").Query<RadioButton>("radioButton-eventCommand-display150");
            IntegerField MP_constant_value = RootElement.Q<VisualElement>("battle_enemy_status").Query<IntegerField>("MP_constant_value");
            //最大MPに対する割合
            RadioButton MP_rate = RootElement.Q<VisualElement>("battle_enemy_status").Query<RadioButton>("radioButton-eventCommand-display151");
            IntegerField MP_rate_value = RootElement.Q<VisualElement>("battle_enemy_status").Query<IntegerField>("MP_rate_value");
            //変数
            RadioButton MP_variable = RootElement.Q<VisualElement>("battle_enemy_status").Query<RadioButton>("radioButton-eventCommand-display152");
            VisualElement MP_variable_dropdown = RootElement.Q<VisualElement>("battle_enemy_status")
                .Query<VisualElement>("MP_variable_dropdown");
            //初期値
            if (_targetCommand.parameters[13] == "True")
            {
                MP_constant_value.SetEnabled(true);
                MP_rate_value.SetEnabled(false);
                MP_variable_dropdown.SetEnabled(false);
            }

            if (_targetCommand.parameters[15] == "True")
            {
                MP_constant_value.SetEnabled(false);
                MP_rate_value.SetEnabled(true);
                MP_variable_dropdown.SetEnabled(false);
            }

            if (_targetCommand.parameters[17] == "True")
            {
                MP_constant_value.SetEnabled(false);
                MP_rate_value.SetEnabled(false);
                MP_variable_dropdown.SetEnabled(true);
            }

            MP_constant_value.value = int.Parse(_targetCommand.parameters[14]);
            MP_rate_value.value = int.Parse(_targetCommand.parameters[16]);
            
            var defaultMpConstant = _targetCommand.parameters[13] == "True" ? 0 : _targetCommand.parameters[15] == "True" ? 1 : 2;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {MP_constant, MP_rate, MP_variable},
                defaultMpConstant, new List<System.Action>
                {
                    //定数
                    () =>
                    {
                        _targetCommand.parameters[13] = MP_constant.value.ToString();
                        _targetCommand.parameters[15] = "False";
                        _targetCommand.parameters[17] = "False";
                        MP_constant_value.SetEnabled(true);
                        MP_rate_value.SetEnabled(false);
                        MP_variable_dropdown.SetEnabled(false);
                        EditEvent();
                        Save(EventDataModels[EventIndex]);
                    },
                    //最大MPに対する割合
                    () =>
                    {
                        _targetCommand.parameters[13] = "False";
                        _targetCommand.parameters[15] = MP_rate.value.ToString();
                        _targetCommand.parameters[17] = "False";
                        MP_constant_value.SetEnabled(false);
                        MP_rate_value.SetEnabled(true);
                        MP_variable_dropdown.SetEnabled(false);
                        EditEvent();
                        Save(EventDataModels[EventIndex]);
                    },
                    //変数
                    () =>
                    {
                        _targetCommand.parameters[13] = "False";
                        _targetCommand.parameters[15] = "False";
                        _targetCommand.parameters[17] = MP_variable.value.ToString();
                        MP_constant_value.SetEnabled(false);
                        MP_rate_value.SetEnabled(false);
                        MP_variable_dropdown.SetEnabled(true);
                        EditEvent();
                        Save(EventDataModels[EventIndex]);
                    }
                });

            BaseInputFieldHandler.IntegerFieldCallback(MP_constant_value, evt =>
            {
                _targetCommand.parameters[14] = MP_constant_value.value.ToString();
                EditEvent();
                Save(EventDataModels[EventIndex]);
            }, 1, 1);

            BaseInputFieldHandler.IntegerFieldCallback(MP_rate_value, evt =>
            {
                _targetCommand.parameters[16] = MP_rate_value.value.ToString();
                EditEvent();
                Save(EventDataModels[EventIndex]);
            }, 0, 100);

            //変数リストドロップダウン
            var MPVariableDropdown = _GetVariablesList();
            var MPVariableListId = 0;
            var isMpVariable = false;
            for (var i = 0; i < MPVariableDropdown.Count; i++)
                if (MPVariableDropdown[i].id == _targetCommand.parameters[18])
                {
                    MPVariableListId = i;
                    break;
                }
            
            if (!isMpVariable)
            {
                _targetCommand.parameters[18] = MPVariableDropdown[0].id;
                Save(EventDataModels[EventIndex]);
            }

            //選択肢に名前を表示売る際に一時的に使用するList
            var MPVariableName = new List<string>();
            var MPVariableIDList = new List<string>();
            
            for (var i = 0; i < MPVariableDropdown.Count; i++)
            {
                if (MPVariableDropdown[i].name == "")
                    MPVariableName.Add(EditorLocalize.LocalizeText("WORD_1518"));
                else
                    MPVariableName.Add(MPVariableDropdown[i].name);

                MPVariableIDList.Add(MPVariableDropdown[i].id);
            }
            
            


            var MPVariableDropdownPopupField =
                new PopupFieldBase<string>(MPVariableName, MPVariableListId);
            MP_variable_dropdown.Clear();
            MP_variable_dropdown.Add(MPVariableDropdownPopupField);
            MPVariableDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[18] = MPVariableIDList[MPVariableDropdownPopupField.index];
                EditEvent();
                Save(EventDataModels[EventIndex]);
            });
            Toggle TP_toggle = RootElement.Q<VisualElement>("battle_enemy_status").Query<Toggle>("TP_toggle");
            switch (_targetCommand.parameters[19])
            {
                case "True":
                    TP_toggle.value = true;
                    break;
                case "False":
                    TP_toggle.value = false;
                    break;
            }

            VisualElement TP_area = RootElement.Q<VisualElement>("battle_enemy_status").Query<VisualElement>("TP_area");
            if (TP_toggle.value) TP_area.style.display = DisplayStyle.Flex;

            if (!TP_toggle.value) TP_area.style.display = DisplayStyle.None;
            
            HP_toggle.RegisterValueChangedCallback(evt =>
            {
                if (_targetCommand.parameters[10] == "False" && _targetCommand.parameters[19] == "False")
                {
                    HP_toggle.value = true;
                    return;
                }
                _targetCommand.parameters[1] =
                    HP_toggle.value.ToString();
                if (HP_toggle.value)
                {
                    AddHpCommand();
                    HP_area.style.display = DisplayStyle.Flex;
                }

                if (!HP_toggle.value)
                {
                    SubEvent(Status.HP);
                    HP_area.style.display = DisplayStyle.None;
                }

                EditEvent();
                Save(EventDataModels[EventIndex]);
            });

            MP_toggle.RegisterValueChangedCallback(evt =>
            {
                if (_targetCommand.parameters[1] == "False" && _targetCommand.parameters[19] == "False")
                {
                    MP_toggle.value = true;
                    return;
                }

                _targetCommand.parameters[10] = MP_toggle.value.ToString();
                if (MP_toggle.value)
                {
                    AddMpTpEvent(Status.MP);
                    MP_area.style.display = DisplayStyle.Flex;
                }

                if (!MP_toggle.value)
                {
                    SubEvent(Status.MP);
                    MP_area.style.display = DisplayStyle.None;
                }

                EditEvent();
                Save(EventDataModels[EventIndex]);
            });

            TP_toggle.RegisterValueChangedCallback(evt =>
            {
                if (_targetCommand.parameters[1] == "False" && _targetCommand.parameters[10] == "False")
                {
                    TP_toggle.value = true;
                    return;
                }

                _targetCommand.parameters[19] = TP_toggle.value.ToString();
                if (TP_toggle.value)
                {
                    AddMpTpEvent(Status.TP);
                    TP_area.style.display = DisplayStyle.Flex;
                }

                if (!TP_toggle.value)
                {
                    SubEvent(Status.TP);
                    TP_area.style.display = DisplayStyle.None;
                }

                EditEvent();
                Save(EventDataModels[EventIndex]);
            });
            //増やす・減らす
            RadioButton TP_up = RootElement.Q<VisualElement>("battle_enemy_status").Query<RadioButton>("radioButton-eventCommand-display153");
            RadioButton TP_down = RootElement.Q<VisualElement>("battle_enemy_status").Query<RadioButton>("radioButton-eventCommand-display154");

            //初期値
            switch (_targetCommand.parameters[20])
            {
                case "up":
                    TP_up.value = true;
                    TP_down.value = false;
                    break;
                case "down":
                    TP_up.value = false;
                    TP_down.value = true;
                    break;
            }
            
            var defaultTpUpDown =_targetCommand.parameters[20] == "up" ? 0 : 1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {TP_up, TP_down},
                defaultTpUpDown, new List<System.Action>
                {
                    //増やす
                    () =>
                    {
                        _targetCommand.parameters[20] = "up";
                        TP_down.value = false;
                        EditEvent();
                        Save(EventDataModels[EventIndex]);
                    },
                    //減らす
                    () =>
                    {
                        _targetCommand.parameters[20] = "down";
                        TP_up.value = false;
                        EditEvent();
                        Save(EventDataModels[EventIndex]);
                    }
                });
            
            //オペランド
            //定数
            RadioButton TP_constant = RootElement.Q<VisualElement>("battle_enemy_status").Query<RadioButton>("radioButton-eventCommand-display155");
            IntegerField TP_constant_value = RootElement.Q<VisualElement>("battle_enemy_status").Query<IntegerField>("TP_constant_value");
            //変数
            RadioButton TP_variable = RootElement.Q<VisualElement>("battle_enemy_status").Query<RadioButton>("radioButton-eventCommand-display156");
            VisualElement TP_variable_dropdown = RootElement.Q<VisualElement>("battle_enemy_status").Query<VisualElement>("TP_variable_dropdown");
            //初期値
            if (_targetCommand.parameters[22] == "True")
            {
                TP_constant_value.SetEnabled(true);
                TP_variable_dropdown.SetEnabled(false);
            }

            if (_targetCommand.parameters[24] == "True")
            {
                TP_constant_value.SetEnabled(false);
                TP_variable_dropdown.SetEnabled(true);
            }

            TP_constant_value.value = int.Parse(_targetCommand.parameters[23]);
            
            
            var defaultTpConstant = _targetCommand.parameters[22] == "True" ? 0 :  1;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {TP_constant, TP_variable},
                defaultTpConstant, new List<System.Action>
                {
                    //定数
                    () =>
                    {
                        _targetCommand.parameters[22] = TP_constant.value.ToString();
                        _targetCommand.parameters[24] = "False";
                        TP_constant_value.SetEnabled(true);
                        TP_variable_dropdown.SetEnabled(false);
                        EditEvent();
                        Save(EventDataModels[EventIndex]);
                    },
                    //変数
                    () =>
                    {
                        _targetCommand.parameters[22] = "False";
                        _targetCommand.parameters[24] = TP_variable.value.ToString();
                        TP_constant_value.SetEnabled(false);
                        TP_variable_dropdown.SetEnabled(true);
                        EditEvent();
                        Save(EventDataModels[EventIndex]);
                    }
                });

            BaseInputFieldHandler.IntegerFieldCallback(TP_constant_value, evt =>
            {
                _targetCommand.parameters[23] = TP_constant_value.value.ToString();
                EditEvent();
                Save(EventDataModels[EventIndex]);
            }, 1, 1);

            //変数リストドロップダウン
            var TPVariableDropdown = _GetVariablesList();
            var TPVariableListId = 0;
            var isTpVariable = false;
            for (var i = 0; i < TPVariableDropdown.Count; i++)
                if (TPVariableDropdown[i].id == _targetCommand.parameters[25])
                {
                    TPVariableListId = i;
                    isTpVariable = true;
                    break;
                }
            
            if (!isTpVariable)
            {
                _targetCommand.parameters[25] = TPVariableDropdown[0].id;
                Save(EventDataModels[EventIndex]);
            }


            //選択肢に名前を表示売る際に一時的に使用するList
            var TPVariableName = new List<string>();
            var TPVariableIDList = new List<string>();
            for (var i = 0; i < TPVariableDropdown.Count; i++)
            {
                if (TPVariableDropdown[i].name == "")
                    TPVariableName.Add(EditorLocalize.LocalizeText("WORD_1518"));
                else
                    TPVariableName.Add(TPVariableDropdown[i].name);

                TPVariableIDList.Add(TPVariableDropdown[i].id);
            }


            var TPVariableDropdownPopupField =
                new PopupFieldBase<string>(TPVariableName, TPVariableListId);
            TP_variable_dropdown.Clear();
            TP_variable_dropdown.Add(TPVariableDropdownPopupField);
            TPVariableDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[25] = TPVariableIDList[TPVariableDropdownPopupField.index];
                EditEvent();
                Save(EventDataModels[EventIndex]);
            });
        }
        //変数リストの取得
        private List<FlagDataModel.Variable> _GetVariablesList() {
            var flagDataModel = DatabaseManagementService.LoadFlags();
            var fileNames = new List<FlagDataModel.Variable>();
            for (var i = 0; i < flagDataModel.variables.Count; i++) fileNames.Add(flagDataModel.variables[i]);

            return fileNames;
        }

        private bool IsAddCheck(Status kind) {
            if (kind == Status.MP && isMPCommandActive) //MPが既に追加されているので駄目
                return false;
            if (kind == Status.TP && isTPCommandActive) //TPが既に追加されているので駄目
                return false;

            return true;
        }

        /// <summary>
        ///     [敵キャラのステータス増減]のうち、HPの増減に関する行を追加する
        /// </summary>
        private void AddHpCommand() {
            EventCommand command = null;
            var offset = 0;
            var parameters = _targetCommand.parameters;
            if (isHPCommandActive) // HPが既に表示されている場合は処理しない
                return;

            isHPCommandActive = true;
            if (!isMPCommandActive && _targetCommand.parameters[10] == "True")
            {
                command = new EventCommand((int) EventEnum.EVENT_CODE_BATTLE_CHANGE_MP,
                    parameters, new List<EventCommandMoveRoute>());
                isMPCommandActive = true;
                offset = 1;
            }
            else if (!isTPCommandActive && _targetCommand.parameters[19] == "True")
            {
                command = new EventCommand((int) EventEnum.EVENT_CODE_BATTLE_CHANGE_TP,
                    parameters, new List<EventCommandMoveRoute>());
                isTPCommandActive = true;
                offset = isMPCommandActive ? 2 : 1;
            }

            // 元となる[敵キャラのステータス増減]コマンドからズラした行に追加する
            var index = GetBaseCommandIndex() + offset;
            EventDataModels[EventIndex].eventCommands.Insert(index, command);
            Save(EventDataModels[EventIndex]);
        }

        /// <summary>
        ///     [敵キャラのステータス増減]のうち、MPもしくはTPの増減に関する行を追加する
        /// </summary>
        private void AddMpTpEvent(Status status) {
            EventCommand command = null;
            var offset = 0;
            var parameters = _targetCommand.parameters;
            var hpToggle = _targetCommand.parameters[1] == "True"
                ? true
                : false;
            var mpToggle = _targetCommand.parameters[10] == "True"
                ? true
                : false;
            var tpToggle = _targetCommand.parameters[19] == "True"
                ? true
                : false;

            if (!IsAddCheck(status))
                return;

            if (status == Status.MP)
            {
                if (!hpToggle && mpToggle && !tpToggle) return;

                if (!isHPCommandActive)
                {
                    command = new EventCommand((int) EventEnum.EVENT_CODE_BATTLE_CHANGE_TP,
                        parameters, new List<EventCommandMoveRoute>());
                    isTPCommandActive = true;
                }
                else
                {
                    command = new EventCommand((int) EventEnum.EVENT_CODE_BATTLE_CHANGE_MP,
                        parameters, new List<EventCommandMoveRoute>());
                    isMPCommandActive = true;
                }

                offset = 1;
            }
            else if (status == Status.TP)
            {
                if (!hpToggle && !mpToggle && tpToggle) return;

                command = new EventCommand((int) EventEnum.EVENT_CODE_BATTLE_CHANGE_TP, parameters,
                    new List<EventCommandMoveRoute>());
                isTPCommandActive = true;
                offset = isMPCommandActive ? 2 : 1;
            }

            var index = GetBaseCommandIndex() + offset;
            EventDataModels[EventIndex].eventCommands.Insert(index, command);
            Save(EventDataModels[EventIndex]);
        }

        /// <summary>
        ///     [敵キャラのステータス増減]のうち、指定されたステータスの増減に関する行を削除する
        /// </summary>
        private void SubEvent(Status status) {
            for (var i = GetBaseCommandIndex() + 1; i < EventDataModels[EventIndex].eventCommands.Count; i++)
            {
                if (status == Status.HP)
                {
                    isHPCommandActive = false;
                    if (EventDataModels[EventIndex].eventCommands[i].code ==
                        (int) EventEnum.EVENT_CODE_BATTLE_CHANGE_MP)
                    {
                        isMPCommandActive = false;
                        EventDataModels[EventIndex].eventCommands.RemoveAt(i);
                        break;
                    }

                    if (EventDataModels[EventIndex].eventCommands[i].code ==
                        (int) EventEnum.EVENT_CODE_BATTLE_CHANGE_TP)
                    {
                        isTPCommandActive = false;
                        EventDataModels[EventIndex].eventCommands.RemoveAt(i);
                        break;
                    }
                }

                if (status == Status.MP)
                {
                    if (EventDataModels[EventIndex].eventCommands[i].code ==
                        (int) EventEnum.EVENT_CODE_BATTLE_CHANGE_MP)
                    {
                        isMPCommandActive = false;
                        EventDataModels[EventIndex].eventCommands.RemoveAt(i);
                        break;
                    }

                    if (!isHPCommandActive)
                    {
                        isTPCommandActive = false;
                        EventDataModels[EventIndex].eventCommands.RemoveAt(i);
                        break;
                    }
                }
                else if (status == Status.TP)
                {
                    if (EventDataModels[EventIndex].eventCommands[i].code ==
                        (int) EventEnum.EVENT_CODE_BATTLE_CHANGE_TP)
                    {
                        isTPCommandActive = false;
                        EventDataModels[EventIndex].eventCommands.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///     基点となる[敵キャラのステータス増減]のインデックスを取得する
        /// </summary>
        /// <returns>code == EVENT_CODE_BATTLE_CHANGE_STATUSとなるコマンドのインデックス</returns>
        private int GetBaseCommandIndex() {
            if (_targetCommand.code == (int) EventEnum.EVENT_CODE_BATTLE_CHANGE_STATUS) return EventCommandIndex;

            // 自分が基点となるコマンドではなかった場合は上に向かって探索する
            for (var i = EventCommandIndex - 1; i >= 0; i--)
                if (EventDataModels[EventIndex].eventCommands[i].code ==
                    (int) EventEnum.EVENT_CODE_BATTLE_CHANGE_STATUS)
                    return i;

            return -1;
        }

        /// <summary>
        ///     関連コマンドにparametersの参照を渡す
        /// </summary>
        private void EditEvent() {
            var count = 0;
            if (_targetCommand.parameters[1] == "True")
                count++;
            if (_targetCommand.parameters[10] == "True")
                count++;
            if (_targetCommand.parameters[19] == "True")
                count++;

            for (var i = GetBaseCommandIndex(); count > 0; count--, i++)
            {
                var tmpCommand = EventDataModels[EventIndex].eventCommands[i];
                // 編集中以外のコマンドに編集中のコマンドのparametersの参照を渡す
                if (tmpCommand.code != _targetCommand.code) tmpCommand.parameters = _targetCommand.parameters;
            }
        }

        private enum Status
        {
            HP,
            MP,
            TP
        }
    }
}