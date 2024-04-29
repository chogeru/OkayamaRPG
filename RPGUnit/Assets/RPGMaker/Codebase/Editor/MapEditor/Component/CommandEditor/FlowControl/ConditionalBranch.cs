using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Armor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Class;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Enemy;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Item;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCustom;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.State;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Vehicle;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Weapon;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event.EventDataModel;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.FlowControl
{
    /// <summary>
    ///     [分岐設定]コマンドのコマンド設定枠の表示物
    /// </summary>
    public class ConditionalBranch : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_conditional_branch.uxml";

        private List<ArmorDataModel>          _armorDataModels;
        private List<CharacterActorDataModel> _characterActorDataModels;
        private List<ClassDataModel>          _classDataModels;
        private List<EnemyDataModel>          _enemyDataModels;

        private FlagDataModel       _flagDataModel;
        private List<ItemDataModel> _itemDataModels;

        private int                        _multipleChoiceCount;
        private List<SkillCustomDataModel> _skillCustomDataModels;
        private List<StateDataModel>       _stateDataModels;

        private EventCommand            _targetCommand;
        private int                     _toggleActiveCount;
        private int                     _toggleActiveNum;
        private List<VehiclesDataModel> _vehiclesDataModels;
        private List<WeaponDataModel>   _weaponDataModels;
        private VisualElement           actionList;
        private List<string>            actorClassID;
        private List<string>            actorClassName;
        private VisualElement           actorList;
        private ImTextField               actorName;
        private RadioButton                  actorName_toggle;
        private List<RadioButton>            actorSubToggleList;
        private RadioButton                  appearance_toggle;
        private RadioButton                  armor_toggle;
        private List<string>            armorIDList;
        private VisualElement           armorList;
        private List<string>            armorNameList;
        private VisualElement           buttonList;
        private List<string>            characterActorIDList;
        private List<string>            characterActorNameList;
        private VisualElement           characterEventList;
        private RadioButton                  class_toggle;
        private VisualElement           classList;
        private VisualElement           conditionalC;
        private VisualElement           conditionalList;
        private VisualElement           conditionalListB;
        private IntegerField            constant;
        private RadioButton                  constant_toggle;
        private VisualElement           direction;
        private VisualElement           provisionalMapContainer;
        private List<string>            enemyIDArray;
        private VisualElement           enemyList;
        private List<string>            enemyNameArray;
        private RadioButton enemyState_toggle;
        private VisualElement           enemyStateList;
        private Toggle                  equipArmor_toggle;
        private Toggle                  equipWeapon_toggle;
        private IntegerField            goldField;
        private VisualElement           haveArmorList;
        private VisualElement           haveWeaponList;
        private List<string>            itemID;
        private VisualElement           itemList;
        private List<string>            itemName;

        private List<Toggle>  mainToggleList;
        private IntegerField  minutes;
        private VisualElement OnOff;

        private RadioButton        party_toggle;
        private IntegerField  second;
        private VisualElement selfOnOff;
        private VisualElement selfswitchList;
        private RadioButton        skill_toggle;
        private List<string>  skillIDList;
        private VisualElement skillList;
        private List<string>  skillNameList;
        private RadioButton state_toggle;
        private List<string>  stateIDArray;
        private VisualElement stateList;
        private List<string>  stateNameArray;
        private List<string>  switchDropdownChoices;

        private VisualElement switchList;
        private List<string>  switchNameDropdownChoices;
        private VisualElement variable;
        private RadioButton variable_toggle;
        private List<string>  variableDropdownChoices;
        private VisualElement variableList;
        private List<string>  variableNameDropdownChoices;
        private VisualElement vehicleList;
        private List<string>  vehiclesArray;
        private List<string>  vehiclesIDArray;
        private RadioButton        weapon_toggle;
        private List<string>  weaponIDList;
        private VisualElement weaponList;
        private List<string>  weaponNameList;

        private GenericPopupFieldBase<TargetCharacterChoice> _targetCharacterPopupField;

        public ConditionalBranch(
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

            InitListName();

            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();

            _targetCommand = EventDataModels[EventIndex].eventCommands[EventCommandIndex];
            if (_targetCommand.parameters == null || _targetCommand.parameters.Count == 0)
            {
                for (var i = 0; i < 51; i++)
                    _targetCommand.parameters.Add("0");

                _targetCommand.parameters[3] = "1";
                _targetCommand.parameters[4] = switchDropdownChoices[0];
                //変数の初期値を定数へ
                _targetCommand.parameters[9] = "1";
                _targetCommand.parameters[23] = _classDataModels[0].basic.id;
                _targetCommand.parameters[24] = _skillCustomDataModels[0].basic.id;
                if(_weaponDataModels.Count > 0) _targetCommand.parameters[25] = _weaponDataModels[0].basic.id;
                if(_armorDataModels.Count > 0) _targetCommand.parameters[26] = _armorDataModels[0].basic.id;
                if(_stateDataModels.Count > 0) _targetCommand.parameters[27] = _stateDataModels[0].id;

                var eventCommands = new List<EventCommand>();
                eventCommands.Add(new EventCommand(0, new List<string>(), new List<EventCommandMoveRoute>()));
                eventCommands.Add(new EventCommand((int) EventEnum.EVENT_CODE_FLOW_ENDIF,
                    new List<string>(), new List<EventCommandMoveRoute>()));

                for (var i = 0; i < eventCommands.Count; i++)
                    EventDataModels[EventIndex].eventCommands.Insert(EventCommandIndex + i + 1, eventCommands[i]);

                _targetCommand.parameters[33] = "-2";

                EventManagementService.SaveEvent(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }
            else
            {
                if (_targetCommand.parameters.Count < 51)
                    for (var i = _targetCommand.parameters.Count; i < 51; i++)
                        _targetCommand.parameters.Add("0");
            }

            InitUI();
            MainToggleInit();

            RadioButton elseOn = RootElement.Query<RadioButton>("radioButton-eventCommand-display114");
            RadioButton elseOff = RootElement.Query<RadioButton>("radioButton-eventCommand-display115");

            elseOn.value = _targetCommand.parameters[0] == "1";
            elseOn.RegisterValueChangedCallback(o =>
            {
                elseOff.value = !elseOn.value;

                // 条件を満たさない分岐を作る場合は分岐を作成
                if (elseOn.value)
                {
                    _targetCommand.parameters[0] = "1";

                    var commandList = EventDataModels[EventIndex].eventCommands;
                    var checkElseBranch = false;
                    for (var i = EventCommandIndex; i < commandList.Count; i++)
                        if (commandList[i].code == (int) EventEnum.EVENT_CODE_FLOW_ELSE &&
                            commandList[i].indent == _targetCommand.indent)
                        {
                            checkElseBranch = true;
                            break;
                        }
                        else if (commandList[i].code == (int) EventEnum.EVENT_CODE_FLOW_ENDIF &&
                                 commandList[i].indent == _targetCommand.indent)
                        {
                            break;
                        }

                    // Else用の分岐が無ければ作成
                    if (!checkElseBranch)
                    {
                        var elseLine = new EventCommand((int) EventEnum.EVENT_CODE_FLOW_ELSE, new List<string>(),
                            new List<EventCommandMoveRoute>());
                        var blackLine = new EventCommand(0, new List<string>(), new List<EventCommandMoveRoute>());

                        // 対象の[分岐設定]コマンドから「分岐終了」を探索し、挿入位置を確認する
                        var insertIndex = commandList.FindIndex(EventCommandIndex,
                            v => v.code == (int) EventEnum.EVENT_CODE_FLOW_ENDIF && v.indent == _targetCommand.indent);
                        insertIndex -= 1;
                        if (insertIndex < 0) return;

                        // 挿入位置からElse用の分岐と空白の行をそれぞれ追加
                        commandList.Insert(insertIndex + 1, elseLine);
                        commandList.Insert(insertIndex + 2, blackLine);
                    }

                    Save(EventDataModels[EventIndex]);
                }
            });

            elseOff.value = _targetCommand.parameters[0] == "0";
            elseOff.RegisterValueChangedCallback(o =>
            {
                elseOn.value = !elseOff.value;

                // 条件を満たさない分岐を作らない場合は分岐を削除
                if (elseOff.value)
                {
                    _targetCommand.parameters[0] = "0";

                    var commandList = EventDataModels[EventIndex].eventCommands;
                    var checkElseBranch = false;
                    for (var i = EventCommandIndex; i < commandList.Count; i++)
                        if (commandList[i].code == (int) EventEnum.EVENT_CODE_FLOW_ELSE &&
                            commandList[i].indent == _targetCommand.indent)
                        {
                            checkElseBranch = true;
                            break;
                        }
                        else if (commandList[i].code == (int) EventEnum.EVENT_CODE_FLOW_ENDIF &&
                                 commandList[i].indent == _targetCommand.indent)
                        {
                            break;
                        }

                    // Else用の分岐があれば削除
                    if (checkElseBranch)
                    {
                        // 対象のコマンドにとって現在の末尾のインデックスと新しく末尾になるインデックスを算出してその間にあるコマンドを全て削除する
                        var oldTailIndex = commandList.FindIndex(EventCommandIndex,
                            v => v.code == (int) EventEnum.EVENT_CODE_FLOW_ENDIF && v.indent == _targetCommand.indent);
                        var newTailIndex = commandList.FindIndex(EventCommandIndex,
                            v => v.code == (int) EventEnum.EVENT_CODE_FLOW_ELSE && v.indent == _targetCommand.indent);
                        if (oldTailIndex < 0 || newTailIndex < 0) return;

                        // 削除実施
                        commandList.RemoveRange(newTailIndex, oldTailIndex - newTailIndex);
                    }

                    Save(EventDataModels[EventIndex]);
                }
            });

            RadioButton multipleOn = RootElement.Query<RadioButton>("radioButton-eventCommand-display116");
            RadioButton multipleOff = RootElement.Query<RadioButton>("radioButton-eventCommand-display117");
            VisualElement andOr = RootElement.Query<VisualElement>("AndOr");
            if (_targetCommand.parameters[1] == "0")
            {
                multipleOff.value = true;
                andOr.SetEnabled(false);
            }
            else
            {
                multipleOn.value = true;
                andOr.SetEnabled(true);
            }

            multipleOn.RegisterValueChangedCallback(o =>
            {
                if (!multipleOn.value)
                {
                    multipleOn.value = false;
                    multipleOff.value = true;
                }

                if (multipleOn.value)
                {
                    multipleOff.value = false;
                    _targetCommand.parameters[1] = "1";
                    andOr.SetEnabled(true);
                    MultipleProcess();
                    Save(EventDataModels[EventIndex]);
                }
            });
            multipleOff.RegisterValueChangedCallback(o =>
            {
                if (!multipleOff.value)
                {
                    multipleOff.value = false;
                    multipleOn.value = true;
                }

                if (multipleOff.value)
                {
                    multipleOn.value = false;

                    _targetCommand.parameters[1] = "0";
                    andOr.SetEnabled(false);
                    MultipleProcess();
                    Save(EventDataModels[EventIndex]);
                }
            });

            var andOrList = EditorLocalize.LocalizeTexts(new List<string> {"WORD_1142", "WORD_1143"});
            var selectID = int.Parse(_targetCommand.parameters[2]);
            if (selectID == -1)
                selectID = 0;
            var andOrPopupField = new PopupFieldBase<string>(andOrList, selectID);
            andOr.Clear();
            andOr.Add(andOrPopupField);
            andOrPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[2] = andOrList.IndexOf(andOrPopupField.value).ToString();
                AndOrProcess();
                Save(EventDataModels[EventIndex]);
            });

            /*--------------------------------------------------------------スイッチ-------------------------------------------------------------------------------*/


            selectID = switchDropdownChoices.IndexOf(_targetCommand.parameters[4]);
            if (selectID == -1)
                selectID = 0;
            var event_switchPopupField = new PopupFieldBase<string>(switchNameDropdownChoices, selectID);
            switchList.Clear();
            switchList.Add(event_switchPopupField);
            event_switchPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[4] =
                    switchDropdownChoices[event_switchPopupField.index];
                EditEvent(FlowKind.SWITCH, CreateEventParameters(FlowKind.SWITCH));
                Save(EventDataModels[EventIndex]);
            });

            var onOffList = EditorLocalize.LocalizeTexts(new List<string> {"WORD_0052", "WORD_0533"});
            selectID = int.Parse(_targetCommand.parameters[5]);
            if (selectID == -1)
                selectID = 0;
            var OnOffPopupField = new PopupFieldBase<string>(onOffList, selectID);
            OnOff.Clear();
            OnOff.Add(OnOffPopupField);
            OnOffPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[5] = onOffList.IndexOf(OnOffPopupField.value).ToString();
                EditEvent(FlowKind.SWITCH, CreateEventParameters(FlowKind.SWITCH));
                Save(EventDataModels[EventIndex]);
            });
            /*--------------------------------------------------------------スイッチ-------------------------------------------------------------------------------*/
            /*--------------------------------------------------------------変数-------------------------------------------------------------------------------*/


            selectID = variableDropdownChoices.IndexOf(EventDataModels[EventIndex]
                .eventCommands[EventCommandIndex]
                .parameters[7]);
            if (selectID == -1)
                selectID = 0;
            var variableListPopupField = new PopupFieldBase<string>(variableNameDropdownChoices, selectID);
            variableList.Clear();
            variableList.Add(variableListPopupField);
            variableListPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[7] =
                    variableDropdownChoices[variableListPopupField.index];
                EditEvent(FlowKind.VARIABLE, CreateEventParameters(FlowKind.VARIABLE));
                Save(EventDataModels[EventIndex]);
            });

            var conditionalAList = EditorLocalize.LocalizeTexts(new List<string>
                {"WORD_1508", "WORD_1509", "WORD_1510", "WORD_1511", "WORD_1512", "WORD_1513"});
            selectID = int.Parse(_targetCommand.parameters[8]);
            if (selectID == -1)
                selectID = 0;
            var conditionalListPopupField = new PopupFieldBase<string>(conditionalAList, selectID);
            conditionalList.Clear();
            conditionalList.Add(conditionalListPopupField);
            conditionalListPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[8] =
                    conditionalAList.IndexOf(conditionalListPopupField.value).ToString();
                EditEvent(FlowKind.VARIABLE, CreateEventParameters(FlowKind.VARIABLE));
                Save(EventDataModels[EventIndex]);
            });


            if (_targetCommand.parameters[9] == "1")
                constant_toggle.value = true;
            else
                variable_toggle.value = true;
            constant_toggle.RegisterValueChangedCallback(o =>
            {
                if (!constant_toggle.value)
                {
                    constant_toggle.value = false;
                    variable_toggle.value = true;
                }

                if (constant_toggle.value)
                {
                    variable_toggle.value = false;
                    _targetCommand.parameters[9] = "1";
                    EditEvent(FlowKind.VARIABLE, CreateEventParameters(FlowKind.VARIABLE));
                    Save(EventDataModels[EventIndex]);
                }
            });
            variable_toggle.RegisterValueChangedCallback(o =>
            {
                if (!variable_toggle.value)
                {
                    variable_toggle.value = false;
                    constant_toggle.value = true;
                }

                if (variable_toggle.value)
                {
                    constant_toggle.value = false;
                    _targetCommand.parameters[9] = "0";
                    EditEvent(FlowKind.VARIABLE, CreateEventParameters(FlowKind.VARIABLE));
                    Save(EventDataModels[EventIndex]);
                }
            });

            constant.value = int.Parse(_targetCommand.parameters[10]);
            constant.RegisterCallback<FocusOutEvent>(evt =>
            {
                _targetCommand.parameters[10] = constant.value.ToString();
                EditEvent(FlowKind.VARIABLE, CreateEventParameters(FlowKind.VARIABLE));
                Save(EventDataModels[EventIndex]);
            });


            selectID = variableDropdownChoices.IndexOf(EventDataModels[EventIndex]
                .eventCommands[EventCommandIndex]
                .parameters[11]);
            if (selectID == -1)
                selectID = 0;
            var variablePopupField = new PopupFieldBase<string>(variableNameDropdownChoices, selectID);
            variable.Clear();
            variable.Add(variablePopupField);
            variablePopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[11] = variableDropdownChoices[variablePopupField.index];
                EditEvent(FlowKind.VARIABLE, CreateEventParameters(FlowKind.VARIABLE));
                Save(EventDataModels[EventIndex]);
            });

            /*--------------------------------------------------------------変数-------------------------------------------------------------------------------*/

            /*--------------------------------------------------------------セルフスイッチ-------------------------------------------------------------------------------*/
            var selfswitchStringList = new List<string> {"A", "B", "C", "D"};
            if (!int.TryParse(_targetCommand.parameters[13], out selectID)) selectID = 0;

            var selfswitchListPopupField = new PopupFieldBase<string>(selfswitchStringList, selectID);
            selfswitchList.Clear();
            selfswitchList.Add(selfswitchListPopupField);
            selfswitchListPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[13] =
                    selfswitchStringList.IndexOf(selfswitchListPopupField.value).ToString();
                EditEvent(FlowKind.SELF_SWITCH, CreateEventParameters(FlowKind.SELF_SWITCH));
                Save(EventDataModels[EventIndex]);
            });

            selectID = int.Parse(_targetCommand.parameters[14]);
            if (selectID == -1)
                selectID = 0;
            var selfOnOffPopupField = new PopupFieldBase<string>(onOffList, selectID);
            selfOnOff.Clear();
            selfOnOff.Add(selfOnOffPopupField);
            selfOnOffPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[14] = onOffList.IndexOf(selfOnOffPopupField.value).ToString();
                EditEvent(FlowKind.SELF_SWITCH, CreateEventParameters(FlowKind.SELF_SWITCH));
                Save(EventDataModels[EventIndex]);
            });
            /*--------------------------------------------------------------セルフスイッチ-------------------------------------------------------------------------------*/

            /*--------------------------------------------------------------タイマー-------------------------------------------------------------------------------*/
            var conditionalListBList = EditorLocalize.LocalizeTexts(new List<string> {"WORD_1509", "WORD_1510"});
            selectID = int.Parse(_targetCommand.parameters[16]);
            if (selectID == -1)
                selectID = 0;
            var conditionalListBPopupField = new PopupFieldBase<string>(conditionalListBList, selectID);
            conditionalListB.Clear();
            conditionalListB.Add(conditionalListBPopupField);
            conditionalListBPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[16] =
                    conditionalListBList.IndexOf(conditionalListBPopupField.value).ToString();
                EditEvent(FlowKind.TIMER, CreateEventParameters(FlowKind.TIMER));
                Save(EventDataModels[EventIndex]);
            });

            minutes.value = int.Parse(_targetCommand.parameters[17]);
            second.value = int.Parse(_targetCommand.parameters[18]);
            minutes.RegisterCallback<FocusOutEvent>(evt =>
            {
                if (minutes.value > 99)
                    minutes.value = 99;
                if (minutes.value < 0)
                    minutes.value = 0;
                _targetCommand.parameters[17] = minutes.value.ToString();
                EditEvent(FlowKind.TIMER, CreateEventParameters(FlowKind.TIMER));
                Save(EventDataModels[EventIndex]);
            });
            second.RegisterCallback<FocusOutEvent>(evt =>
            {
                if (second.value > 59)
                    second.value = 59;
                if (second.value < 0)
                    second.value = 0;
                _targetCommand.parameters[18] = second.value.ToString();
                EditEvent(FlowKind.TIMER, CreateEventParameters(FlowKind.TIMER));
                Save(EventDataModels[EventIndex]);
            });


            /*--------------------------------------------------------------タイマー-------------------------------------------------------------------------------*/

            /*--------------------------------------------------------------アクター-------------------------------------------------------------------------------*/

            int defaultSelect  = int.Parse(_targetCommand.parameters[21]);
            new CommonToggleSelector().SetRadioSelector(
                actorSubToggleList,
                defaultSelect , new List<System.Action>
                {
                    //120
                    () =>
                    {
                        SetActorSub(0);
                    },
                    //121
                    () =>
                    {
                        SetActorSub(1);
                    },
                    //122
                    () =>
                    {
                        SetActorSub(2);
                    },
                    //123
                    () =>
                    {
                        SetActorSub(3);
                    },
                    //124
                    () =>
                    {
                        SetActorSub(4);
                    },
                    //125
                    () =>
                    {
                        SetActorSub(5);
                    },
                    //126
                    () =>
                    {
                        SetActorSub(6);
                    }
                });

            void SetActorSub(int index) {
                _targetCommand.parameters[21] = index.ToString();
                EditEvent(FlowKind.ACTOR, CreateEventParameters(FlowKind.ACTOR));
                Save(EventDataModels[EventIndex]);

            }

            selectID = 0;
            selectID = characterActorIDList.IndexOf(_targetCommand.parameters[20]);
            if (selectID == -1)
                selectID = 0;
            var actorPopupField = new PopupFieldBase<string>(characterActorNameList, selectID);
            actorList.Clear();
            actorList.Add(actorPopupField);
            actorPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[20] =
                    characterActorIDList[actorPopupField.index];
                EditEvent(FlowKind.ACTOR, CreateEventParameters(FlowKind.ACTOR));
                Save(EventDataModels[EventIndex]);
            });

            actorName.RegisterCallback<FocusOutEvent>(evt =>
            {
                _targetCommand.parameters[22] = actorName.value;
                EditEvent(FlowKind.ACTOR, CreateEventParameters(FlowKind.ACTOR));
                Save(EventDataModels[EventIndex]);
            });


            selectID = 0;
            if (actorSubToggleList[2].value)
                selectID = actorClassID.IndexOf(_targetCommand.parameters[23]);
            if (selectID == -1)
                selectID = 0;
            var classPopupField = new PopupFieldBase<string>(actorClassName, selectID);
            classList.Clear();
            classList.Add(classPopupField);
            classPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[23] = actorClassID[classPopupField.index];
                EditEvent(FlowKind.ACTOR, CreateEventParameters(FlowKind.ACTOR));
                Save(EventDataModels[EventIndex]);
            });


            selectID = 0;
            if (actorSubToggleList[3].value)
                selectID = skillIDList.IndexOf(_targetCommand.parameters[24]);
            if (selectID == -1)
                selectID = 0;
            var skillPopupField = new PopupFieldBase<string>(skillNameList, selectID);
            skillList.Clear();
            skillList.Add(skillPopupField);
            skillPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[24] = skillIDList[skillPopupField.index];
                EditEvent(FlowKind.ACTOR, CreateEventParameters(FlowKind.ACTOR));
                Save(EventDataModels[EventIndex]);
            });


            selectID = 0;

            if (actorSubToggleList[4].value)
                selectID = weaponIDList.IndexOf(_targetCommand.parameters[25]);
            if (selectID == -1)
                selectID = 0;
            var weaponPopupField = new PopupFieldBase<string>(weaponNameList, selectID);
            weaponList.Clear();
            weaponList.Add(weaponPopupField);
            weaponPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[25] = weaponIDList[weaponPopupField.index];
                EditEvent(FlowKind.ACTOR, CreateEventParameters(FlowKind.ACTOR));
                Save(EventDataModels[EventIndex]);
            });


            selectID = 0;

            if (actorSubToggleList[5].value)
                selectID = armorIDList.IndexOf(_targetCommand.parameters[26]);
            if (selectID == -1)
                selectID = 0;
            var armorPopupField = new PopupFieldBase<string>(armorNameList, selectID);
            armorList.Clear();
            armorList.Add(armorPopupField);
            armorPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[26] = armorIDList[armorPopupField.index];
                EditEvent(FlowKind.ACTOR, CreateEventParameters(FlowKind.ACTOR));
                Save(EventDataModels[EventIndex]);
            });


            selectID = 0;
            if (actorSubToggleList[6].value)
                selectID = stateIDArray.IndexOf(_targetCommand.parameters[27]);
            bool isNone = false;

            if (selectID == -1)
            {
                selectID = 0;
                isNone = true;
            }

            var statePopupField = new PopupFieldBase<string>(stateNameArray, selectID);
            stateList.Clear();
            stateList.Add(statePopupField);
            statePopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[27] = stateIDArray[stateNameArray.IndexOf(statePopupField.value)];
                EditEvent(FlowKind.ACTOR, CreateEventParameters(FlowKind.ACTOR));
                Save(EventDataModels[EventIndex]);
            });

            //設定しているステートがデータ上になかった場合、「なし」を表示させる
            if (isNone)
            {
                statePopupField.ChangeButtonText(EditorLocalize.LocalizeText("WORD_0113"));
            }

            /*--------------------------------------------------------------アクター-------------------------------------------------------------------------------*/
            /*--------------------------------------------------------------敵キャラ-------------------------------------------------------------------------------*/

            // 敵キャラ番号の選択肢取得
            var enemyDropdownChoices = GetEnemyNameList();

            // 敵キャラ番号の選択肢の初期値
            var memberNo = 0;
            int.TryParse(_targetCommand.parameters[29], out memberNo);
            // 敵キャラ数に変動があった場合などで、選択肢の数が合わなくなった場合は、初期値0とする
            if (enemyDropdownChoices.Count <= memberNo) memberNo = 0;

            // 敵キャラ番号の選択肢表示
            var enemyPopupField = new PopupFieldBase<string>(enemyDropdownChoices, memberNo);
            enemyList.Clear();
            enemyList.Add(enemyPopupField);
            enemyPopupField.RegisterValueChangedCallback(_ =>
            {
                _targetCommand.parameters[29] = enemyPopupField.index.ToString();
                EditEvent(FlowKind.ENEMY, CreateEventParameters(FlowKind.ENEMY));
                Save(EventDataModels[EventIndex]);
            });

            if (_targetCommand.parameters[30] == "0")
                appearance_toggle.value = true;
            else
                enemyState_toggle.value = true;
            appearance_toggle.RegisterValueChangedCallback(o =>
            {
                if (!appearance_toggle.value)
                {
                    appearance_toggle.value = false;
                    enemyState_toggle.value = true;
                }

                if (appearance_toggle.value)
                {
                    enemyState_toggle.value = false;
                    _targetCommand.parameters[30] = "0";
                    enemyStateList.SetEnabled(false);
                    EditEvent(FlowKind.ENEMY, CreateEventParameters(FlowKind.ENEMY));
                    Save(EventDataModels[EventIndex]);
                }
            });
            enemyState_toggle.RegisterValueChangedCallback(o =>
            {
                if (!enemyState_toggle.value)
                {
                    enemyState_toggle.value = false;
                    appearance_toggle.value = true;
                }

                if (enemyState_toggle.value)
                {
                    appearance_toggle.value = false;
                    _targetCommand.parameters[30] = "1";
                    enemyStateList.SetEnabled(true);
                    EditEvent(FlowKind.ENEMY, CreateEventParameters(FlowKind.ENEMY));
                    Save(EventDataModels[EventIndex]);
                }
            });

            selectID = stateIDArray.IndexOf(_targetCommand.parameters[31]);
            if (selectID == -1)
                selectID = 0;
            var enemyStatePopupField = new PopupFieldBase<string>(stateNameArray, selectID);
            enemyStateList.Clear();
            enemyStateList.Add(enemyStatePopupField);
            enemyStatePopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[31] = stateIDArray[stateNameArray.IndexOf(enemyStatePopupField.value)];
                EditEvent(FlowKind.ENEMY, CreateEventParameters(FlowKind.ENEMY));
                Save(EventDataModels[EventIndex]);
            });
            /*--------------------------------------------------------------敵キャラ-------------------------------------------------------------------------------*/

            /*--------------------------------------------------------------キャラクター-------------------------------------------------------------------------------*/
            {
                int targetCharacterParameterIndex = 33;
                AddOrHideProvisionalMapAndAddTargetCharacterPopupField(
                    targetCharacterParameterIndex,
                    provisionalMapPopupField =>
                    {
                        _targetCharacterPopupField = AddTargetCharacterPopupField(
                            characterEventList,
                            targetCharacterParameterIndex,
                            changeEvent =>
                            {
                                EventCommand.parameters[targetCharacterParameterIndex] = changeEvent.newValue.Id;
                                EditEvent(FlowKind.CHARACTER, CreateEventParameters(FlowKind.CHARACTER));
                                Save(EventDataModel);
                            },
                            forceMapId: provisionalMapPopupField?.value.MapDataModel?.id);
                    });
            }

            selectID = int.Parse(_targetCommand.parameters[34]);
            if (selectID == -1)
                selectID = 0;
            var direct = EditorLocalize.LocalizeTexts(new List<string>
                {"WORD_0815", "WORD_0813", "WORD_0814", "WORD_0812"});
            var directPopupField = new PopupFieldBase<string>(direct, selectID);
            direction.Clear();
            direction.Add(directPopupField);
            directPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[34] = direct.IndexOf(directPopupField.value).ToString();
                EditEvent(FlowKind.CHARACTER, CreateEventParameters(FlowKind.CHARACTER));
                Save(EventDataModels[EventIndex]);
            });
            /*--------------------------------------------------------------キャラクター-------------------------------------------------------------------------------*/

            /*--------------------------------------------------------------乗り物-------------------------------------------------------------------------------*/


            selectID = 0;
            selectID = vehiclesIDArray.IndexOf(_targetCommand.parameters[36]);
            if (selectID == -1)
                selectID = 0;
            var VehiclesPopupField = new PopupFieldBase<string>(vehiclesArray, selectID);
            vehicleList.Clear();
            vehicleList.Add(VehiclesPopupField);
            VehiclesPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[36] = vehiclesIDArray[vehiclesArray.IndexOf(VehiclesPopupField.value)];
                EditEvent(FlowKind.VEHICLE, CreateEventParameters(FlowKind.VEHICLE));
                Save(EventDataModels[EventIndex]);
            });
            /*--------------------------------------------------------------乗り物-------------------------------------------------------------------------------*/

            /*--------------------------------------------------------------所持金-------------------------------------------------------------------------------*/
            var conditionalCList =
                EditorLocalize.LocalizeTexts(new List<string> {"WORD_1509", "WORD_1510", "WORD_1512"});
            selectID = int.Parse(_targetCommand.parameters[38]);
            if (selectID == -1)
                selectID = 0;
            var conditionalCPopupField = new PopupFieldBase<string>(conditionalCList, selectID);
            conditionalC.Clear();
            conditionalC.Add(conditionalCPopupField);
            conditionalCPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[38] = conditionalCList.IndexOf(conditionalCPopupField.value).ToString();
                EditEvent(FlowKind.GOLD, CreateEventParameters(FlowKind.GOLD));
                Save(EventDataModels[EventIndex]);
            });


            var gold = 0;
            if (!int.TryParse(_targetCommand.parameters[39], out gold)) goldField.value = 0;

            goldField.value =
                gold;
            goldField.RegisterCallback<FocusOutEvent>(evt =>
            {
                _targetCommand.parameters[39] = goldField.value.ToString();
                EditEvent(FlowKind.GOLD, CreateEventParameters(FlowKind.GOLD));
                Save(EventDataModels[EventIndex]);
            });

            /*--------------------------------------------------------------所持金-------------------------------------------------------------------------------*/

            /*--------------------------------------------------------------アイテム-------------------------------------------------------------------------------*/


            selectID = itemID.IndexOf(_targetCommand.parameters[41]);
            if (selectID == -1)
                selectID = 0;
            var itemPopupField = new PopupFieldBase<string>(itemName, selectID);
            itemList.Clear();
            itemList.Add(itemPopupField);
            itemPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[41] = itemID[itemPopupField.index];
                EditEvent(FlowKind.ITEM, CreateEventParameters(FlowKind.ITEM));
                Save(EventDataModels[EventIndex]);
            });
            /*--------------------------------------------------------------アイテム-------------------------------------------------------------------------------*/

            /*--------------------------------------------------------------武器-------------------------------------------------------------------------------*/
            if (_targetCommand.parameters[44] == "1")
                equipWeapon_toggle.value = true;
            equipWeapon_toggle.RegisterValueChangedCallback(o =>
            {
                _targetCommand.parameters[44] = equipWeapon_toggle.value ? "1" : "0";
                enemyStateList.SetEnabled(true);
                EditEvent(FlowKind.WEAPON, CreateEventParameters(FlowKind.WEAPON));
                Save(EventDataModels[EventIndex]);
            });

            selectID = weaponIDList.IndexOf(_targetCommand.parameters[43]);
            if (selectID == -1)
                selectID = 0;
            var haveWeaponListPopupField = new PopupFieldBase<string>(weaponNameList, selectID);
            haveWeaponList.Clear();
            haveWeaponList.Add(haveWeaponListPopupField);
            haveWeaponListPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[43] = weaponIDList[haveWeaponListPopupField.index];
                EditEvent(FlowKind.WEAPON, CreateEventParameters(FlowKind.WEAPON));
                Save(EventDataModels[EventIndex]);
            });


            /*--------------------------------------------------------------武器-------------------------------------------------------------------------------*/

            /*--------------------------------------------------------------防具-------------------------------------------------------------------------------*/
            if (_targetCommand.parameters[47] == "1")
                equipArmor_toggle.value = true;
            equipArmor_toggle.RegisterValueChangedCallback(o =>
            {
                _targetCommand.parameters[47] = equipArmor_toggle.value ? "1" : "0";
                enemyStateList.SetEnabled(true);
                EditEvent(FlowKind.ARMOR, CreateEventParameters(FlowKind.ARMOR));
                Save(EventDataModels[EventIndex]);
            });

            selectID = armorIDList.IndexOf(_targetCommand.parameters[46]);
            if (selectID == -1)
                selectID = 0;
            var haveArmorListPopupField = new PopupFieldBase<string>(armorNameList, selectID);
            haveArmorList.Clear();
            haveArmorList.Add(haveArmorListPopupField);
            haveArmorListPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[46] = armorIDList[haveArmorListPopupField.index];
                EditEvent(FlowKind.ARMOR, CreateEventParameters(FlowKind.ARMOR));
                Save(EventDataModels[EventIndex]);
            });


            /*--------------------------------------------------------------防具-------------------------------------------------------------------------------*/

            /*--------------------------------------------------------------ボタン-------------------------------------------------------------------------------*/
            var buttons = EditorLocalize.LocalizeTexts(new List<string>
            {
                "WORD_0792", "WORD_1530", "WORD_1531", "WORD_1532", "WORD_1533", "WORD_1534", "WORD_1535", "WORD_1536",
                "WORD_1537"
            });
            var pushed = EditorLocalize.LocalizeTexts(new List<string>
                {"WORD_1163", "WORD_1164", "WORD_1165"});
            if (!int.TryParse(_targetCommand.parameters[49], out selectID)) selectID = 0;


            var buttonListPopupField = new PopupFieldBase<string>(buttons, selectID);
            buttonList.Clear();
            buttonList.Add(buttonListPopupField);
            buttonListPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[49] =
                    buttons.IndexOf(buttonListPopupField.value).ToString();
                EditEvent(FlowKind.BUTTON, CreateEventParameters(FlowKind.BUTTON));
                Save(EventDataModels[EventIndex]);
            });

            selectID = int.Parse(_targetCommand.parameters[50]);
            if (selectID == -1)
                selectID = 0;
            var actionListPopupField = new PopupFieldBase<string>(pushed, selectID);
            actionList.Clear();
            actionList.Add(actionListPopupField);
            actionListPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[50] = pushed.IndexOf(actionListPopupField.value).ToString();
                EditEvent(FlowKind.BUTTON, CreateEventParameters(FlowKind.BUTTON));
                Save(EventDataModels[EventIndex]);
            });

            /*--------------------------------------------------------------ボタン-------------------------------------------------------------------------------*/

            if (_targetCommand.parameters[1] == "1")
            {
                //複数選択の場合、選択肢の選択可能状態を更新
                //引数は未使用
                ToggleActiveSettings(0);
            }
        }


        private void InitUI() {
            Toggle switch_toggle = RootElement.Query<Toggle>("switch_toggle");
            Toggle variableList_toggle = RootElement.Query<Toggle>("variableList_toggle");
            Toggle selfswitch_toggle = RootElement.Query<Toggle>("selfswitch_toggle");
            Toggle timer_toggle = RootElement.Query<Toggle>("timer_toggle");
            Toggle actor_toggle = RootElement.Query<Toggle>("actor_toggle");


            Toggle enemy_toggle = RootElement.Query<Toggle>("enemy_toggle");
            Toggle character_toggle = RootElement.Query<Toggle>("character_toggle");
            Toggle vehicle_toggle = RootElement.Query<Toggle>("vehicle_toggle");
            Toggle gold_toggle = RootElement.Query<Toggle>("gold_toggle");
            Toggle item_toggle = RootElement.Query<Toggle>("item_toggle");
            Toggle haveWeapon_toggle = RootElement.Query<Toggle>("haveWeapon_toggle");
            Toggle haveArmor_toggle = RootElement.Query<Toggle>("haveArmor_toggle");
            Toggle button_toggle = RootElement.Query<Toggle>("button_toggle");

            mainToggleList = new List<Toggle>();
            mainToggleList.Add(switch_toggle);
            mainToggleList.Add(variableList_toggle);
            mainToggleList.Add(selfswitch_toggle);
            mainToggleList.Add(timer_toggle);
            mainToggleList.Add(actor_toggle);
            mainToggleList.Add(enemy_toggle);
            mainToggleList.Add(character_toggle);
            mainToggleList.Add(vehicle_toggle);
            mainToggleList.Add(gold_toggle);
            mainToggleList.Add(item_toggle);
            mainToggleList.Add(haveWeapon_toggle);
            mainToggleList.Add(haveArmor_toggle);
            mainToggleList.Add(button_toggle);

            switchList = RootElement.Query<VisualElement>("switchList");
            OnOff = RootElement.Query<VisualElement>("OnOff");
            variableList = RootElement.Query<VisualElement>("variableList");
            conditionalList = RootElement.Query<VisualElement>("conditionalList");
            constant_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display118");
            constant = RootElement.Query<IntegerField>("constant");
            variable_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display119");
            variable = RootElement.Query<VisualElement>("variable");
            selfswitchList = RootElement.Query<VisualElement>("selfswitchList");
            selfOnOff = RootElement.Query<VisualElement>("selfOnOff");
            conditionalListB = RootElement.Query<VisualElement>("conditionalListB");
            minutes = RootElement.Query<IntegerField>("minutes");
            second = RootElement.Query<IntegerField>("second");
            actorList = RootElement.Query<VisualElement>("actorList");

            party_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display120");
            actorName_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display121");
            actorName = RootElement.Query<ImTextField>("actorName");
            class_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display122");
            classList = RootElement.Query<VisualElement>("classList");
            skill_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display123");
            skillList = RootElement.Query<VisualElement>("skillList");
            weapon_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display124");
            weaponList = RootElement.Query<VisualElement>("weaponList");
            armor_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display125");
            armorList = RootElement.Query<VisualElement>("armorList");
            state_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display126");
            stateList = RootElement.Query<VisualElement>("stateList");

            enemyList = RootElement.Query<VisualElement>("enemyList");
            appearance_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display127");
            enemyState_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display128");
            enemyStateList = RootElement.Query<VisualElement>("enemyStateList");
            characterEventList = RootElement.Query<VisualElement>("characterEventList");
            direction = RootElement.Query<VisualElement>("direction");
            provisionalMapContainer = RootElement.Query<VisualElement>("provisional_map_popupfield_container");
            vehicleList = RootElement.Query<VisualElement>("vehicleList");
            conditionalC = RootElement.Query<VisualElement>("conditionalC");
            goldField = RootElement.Query<IntegerField>("gold");
            itemList = RootElement.Query<VisualElement>("itemList");
            haveWeaponList = RootElement.Query<VisualElement>("haveWeaponList");
            equipWeapon_toggle = RootElement.Query<Toggle>("equipWeapon_toggle");
            haveArmorList = RootElement.Query<VisualElement>("haveArmorList");
            equipArmor_toggle = RootElement.Query<Toggle>("equipArmor_toggle");
            buttonList = RootElement.Query<VisualElement>("buttonList");
            actionList = RootElement.Query<VisualElement>("actionList");

            actorSubToggleList = new List<RadioButton>();
            actorSubToggleList.Add(party_toggle);
            actorSubToggleList.Add(actorName_toggle);
            actorSubToggleList.Add(class_toggle);
            actorSubToggleList.Add(skill_toggle);
            actorSubToggleList.Add(weapon_toggle);
            actorSubToggleList.Add(armor_toggle);
            actorSubToggleList.Add(state_toggle);

            var index = int.Parse(_targetCommand.parameters[21]);
            if (index == -1)
                index = 0;
            actorSubToggleList[index].value = true;
            actorSubToggleList[index].SetEnabled(false);

            //複数選択の状態の場合には、以下の処理を実施
            if (_targetCommand.parameters[1] == "1")
                for (var i = EventCommandIndex + 1;
                        i < EventDataModels[EventIndex].eventCommands.Count;
                        i++)
                    //今選択されてる複数選択がANDなのかORなのか
                    if (_targetCommand.parameters[2] == "0")
                    {
                        if (EventDataModels[EventIndex].eventCommands[i].code ==
                            (int) EventEnum.EVENT_CODE_FLOW_AND)
                            _multipleChoiceCount++;
                        else
                            break;
                    }
                    else
                    {
                        if (EventDataModels[EventIndex].eventCommands[i].code ==
                            (int) EventEnum.EVENT_CODE_FLOW_OR)
                            _multipleChoiceCount++;
                        else
                            break;
                    }
        }

        private void InitListName() {
            var noName = EditorLocalize.LocalizeText("WORD_1518"); // 名称未設定

            switchDropdownChoices = new List<string>();
            switchNameDropdownChoices = new List<string>();
            variableDropdownChoices = new List<string>();
            variableNameDropdownChoices = new List<string>();
            characterActorNameList = new List<string>();
            characterActorIDList = new List<string>();
            actorClassName = new List<string>();
            actorClassID = new List<string>();
            skillNameList = new List<string>();
            skillIDList = new List<string>();
            weaponNameList = new List<string>();
            weaponIDList = new List<string>();
            armorNameList = new List<string>();
            armorIDList = new List<string>();
            stateNameArray = new List<string>();
            stateIDArray = new List<string>();
            enemyNameArray = new List<string>();
            enemyIDArray = new List<string>();
            vehiclesArray = new List<string>();
            vehiclesIDArray = new List<string>();
            itemName = new List<string>();
            itemID = new List<string>();

            _flagDataModel = DatabaseManagementService.LoadFlags();
            _characterActorDataModels = DatabaseManagementService.LoadCharacterActor();
            _classDataModels = DatabaseManagementService.LoadCharacterActorClass();
            _weaponDataModels = DatabaseManagementService.LoadWeapon();
            _skillCustomDataModels = DatabaseManagementService.LoadSkillCustom();
            _armorDataModels = DatabaseManagementService.LoadArmor();
            _stateDataModels = DatabaseManagementService.LoadStateEdit();
            _enemyDataModels = DatabaseManagementService.LoadEnemy();
            _vehiclesDataModels = DatabaseManagementService.LoadCharacterVehicles();
            _itemDataModels = DatabaseManagementService.LoadItem();

            for (var i = 0; i < _flagDataModel.switches.Count; i++)
            {
                var name = "";
                switchDropdownChoices.Add(_flagDataModel.switches[i].id);
                name = _flagDataModel.switches[i].name == "" ? noName : _flagDataModel.switches[i].name;

                switchNameDropdownChoices.Add(name);
            }

            for (var i = 0; i < _flagDataModel.variables.Count; i++)
            {
                var name = "";
                variableDropdownChoices.Add(_flagDataModel.variables[i].id);
                name = _flagDataModel.variables[i].name == "" ? noName : _flagDataModel.variables[i].name;

                variableNameDropdownChoices.Add(name);
            }

            for (var i = 0; i < _characterActorDataModels.Count; i++)
            {
                characterActorIDList.Add(_characterActorDataModels[i].uuId);
                characterActorNameList.Add(_characterActorDataModels[i].basic.name);
            }

            for (var i = 0; i < _classDataModels.Count; i++)
            {
                actorClassName.Add(_classDataModels[i].basic.name);
                actorClassID.Add(_classDataModels[i].basic.id);
            }

            for (var i = 0; i < _weaponDataModels.Count; i++)
            {
                weaponNameList.Add(_weaponDataModels[i].basic.name);
                weaponIDList.Add(_weaponDataModels[i].basic.id);
            }

            for (var i = 0; i < _skillCustomDataModels.Count; i++)
            {
                skillNameList.Add(_skillCustomDataModels[i].basic.name);
                skillIDList.Add(_skillCustomDataModels[i].basic.id);
            }

            for (var i = 0; i < _armorDataModels.Count; i++)
            {
                armorNameList.Add(_armorDataModels[i].basic.name);
                armorIDList.Add(_armorDataModels[i].basic.id);
            }

            for (var i = 0; i < _stateDataModels.Count; i++)
            {
                stateNameArray.Add(_stateDataModels[i].name);
                stateIDArray.Add(_stateDataModels[i].id);
            }

            for (var i = 0; i < _enemyDataModels.Count; i++)
            {
                enemyNameArray.Add(_enemyDataModels[i].name);
                enemyIDArray.Add(_enemyDataModels[i].id);
            }

            for (var i = 0; i < _vehiclesDataModels.Count; i++)
            {
                vehiclesArray.Add(_vehiclesDataModels[i].name);
                vehiclesIDArray.Add(_vehiclesDataModels[i].id);
            }

            for (var i = 0; i < _itemDataModels.Count; i++)
            {
                itemName.Add(_itemDataModels[i].basic.name);
                itemID.Add(_itemDataModels[i].basic.id);
            }
        }

        private void MultipleProcess() {
            if (_targetCommand.parameters[1] == "0")
            {
                if (_toggleActiveCount == 0 && _multipleChoiceCount == 0)
                {
                    if (_toggleActiveNum != 0)
                        return;
                    if (_targetCommand.parameters[4] != "0" && _targetCommand.parameters[5] != "0")
                        return;
                }

                //複数選択している時にoffにした場合の処理
                for (var i = 0; i < _multipleChoiceCount; i++)
                    EventDataModels[EventIndex].eventCommands.RemoveAt(EventCommandIndex + 1);
                _multipleChoiceCount = 0;
                _toggleActiveCount = 0;
                _toggleActiveNum = 0;

                mainToggleList[0].value = true;
                mainToggleList[0].SetEnabled(false);
                for (var i = 1; i < mainToggleList.Count; i++)
                    mainToggleList[i].value = false;
            }
            else
            {
                if (_toggleActiveCount != 0 && _multipleChoiceCount != 0)
                    return;
                AddEvent(_targetCommand.parameters[2], CreateEventParameters((FlowKind) _toggleActiveNum));
                _multipleChoiceCount = 1;
                _toggleActiveCount = 1;
            }
        }

        private void AndOrProcess() {
            //各And,Orのイベントが事前に設置されていて、ユーザーがAndOrを変えた時の処理
            //複数選択の状態じゃなければ抜ける
            if (_targetCommand.parameters[1] == "0")
                return;

            for (var i = EventCommandIndex + 1; i < EventDataModels[EventIndex].eventCommands.Count; i++)
                //今選択されてる複数選択がANDなのかORなのか
                if (_targetCommand.parameters[2] == "0")
                {
                    if (EventDataModels[EventIndex].eventCommands[i].code == (int) EventEnum.EVENT_CODE_FLOW_OR)
                        EventDataModels[EventIndex].eventCommands[i].code = (int) EventEnum.EVENT_CODE_FLOW_AND;
                }
                else
                {
                    if (EventDataModels[EventIndex].eventCommands[i].code == (int) EventEnum.EVENT_CODE_FLOW_AND)
                        EventDataModels[EventIndex].eventCommands[i].code = (int) EventEnum.EVENT_CODE_FLOW_OR;
                }
        }

        private void MainToggleInit() {
            _toggleActiveCount = 0;
            if (_targetCommand.parameters[3] == "1")
            {
                mainToggleList[0].value = true;
                switchList.SetEnabled(true);
                OnOff.SetEnabled(true);
            }

            if (_targetCommand.parameters[6] == "1")
            {
                mainToggleList[1].value = true;
                variableList.SetEnabled(true);
                conditionalList.SetEnabled(true);
                constant_toggle.SetEnabled(true);
                constant.SetEnabled(true);
                variable_toggle.SetEnabled(true);
                variable.SetEnabled(true);
            }

            if (_targetCommand.parameters[12] == "1")
            {
                mainToggleList[2].value = true;
                selfswitchList.SetEnabled(true);
                selfOnOff.SetEnabled(true);
            }

            if (_targetCommand.parameters[15] == "1")
            {
                mainToggleList[3].value = true;
                _targetCommand.parameters[15] = "1";
                conditionalListB.SetEnabled(true);
                minutes.SetEnabled(true);
                second.SetEnabled(true);
            }

            if (_targetCommand.parameters[19] == "1")
            {
                mainToggleList[4].value = true;
                actorList.SetEnabled(true);
                actorName.SetEnabled(true);
                classList.SetEnabled(true);
                skillList.SetEnabled(true);
                weaponList.SetEnabled(true);
                armorList.SetEnabled(true);
                stateList.SetEnabled(true);
                for (var j = 0; j < actorSubToggleList.Count; j++)
                    actorSubToggleList[j].SetEnabled(true);
            }

            if (_targetCommand.parameters[28] == "1")
            {
                mainToggleList[5].value = true;
                enemyList.SetEnabled(true);
                appearance_toggle.SetEnabled(true);
                enemyState_toggle.SetEnabled(true);
                enemyStateList.SetEnabled(true);
            }

            if (_targetCommand.parameters[32] == "1")
            {
                mainToggleList[6].value = true;
                characterEventList.SetEnabled(true);
                direction.SetEnabled(true);
                provisionalMapContainer.SetEnabled(true);
            }

            if (_targetCommand.parameters[35] == "1")
            {
                mainToggleList[7].value = true;
                vehicleList.SetEnabled(true);
            }

            if (_targetCommand.parameters[37] == "1")
            {
                mainToggleList[8].value = true;
                conditionalC.SetEnabled(true);
                goldField.SetEnabled(true);
            }

            if (_targetCommand.parameters[40] == "1")
            {
                mainToggleList[9].value = true;
                itemList.SetEnabled(true);
            }

            if (_targetCommand.parameters[42] == "1")
            {
                mainToggleList[10].value = true;
                haveWeaponList.SetEnabled(true);
                equipWeapon_toggle.SetEnabled(true);
            }

            if (_targetCommand.parameters[45] == "1")
            {
                mainToggleList[11].value = true;
                haveArmorList.SetEnabled(true);
                equipArmor_toggle.SetEnabled(true);
            }

            if (_targetCommand.parameters[48] == "1")
            {
                mainToggleList[12].value = true;
                buttonList.SetEnabled(true);
                actionList.SetEnabled(true);
            }

            MainToggleProcess();
            InitToggleEnabled();
        }

        /// <summary>
        /// トグルのON/OFF切り替え
        /// </summary>
        private void MainToggleProcess() {
            //メインのトグル処理
            for (var i = 0; i < mainToggleList.Count; i++)
            {
                var kind = (FlowKind) i;
                mainToggleList[(int) kind].RegisterValueChangedCallback(o =>
                {
                    var selectID = 0;
                    if (!mainToggleList[(int) kind].value)
                    {
                        switch (kind)
                        {
                            case FlowKind.SWITCH:
                                _targetCommand.parameters[3] = "0";
                                switchList.SetEnabled(false);
                                OnOff.SetEnabled(false);
                                break;
                            case FlowKind.VARIABLE:
                                _targetCommand.parameters[6] = "0";
                                variableList.SetEnabled(false);
                                conditionalList.SetEnabled(false);
                                constant_toggle.SetEnabled(false);
                                constant.SetEnabled(false);
                                variable_toggle.SetEnabled(false);
                                variable.SetEnabled(false);
                                break;
                            case FlowKind.SELF_SWITCH:
                                _targetCommand.parameters[12] = "0";
                                selfswitchList.SetEnabled(false);
                                selfOnOff.SetEnabled(false);
                                break;
                            case FlowKind.TIMER:
                                _targetCommand.parameters[15] = "0";
                                conditionalListB.SetEnabled(false);
                                minutes.SetEnabled(false);
                                second.SetEnabled(false);
                                break;
                            case FlowKind.ACTOR:
                                _targetCommand.parameters[19] = "0";
                                actorList.SetEnabled(false);
                                actorName.SetEnabled(false);
                                classList.SetEnabled(false);
                                skillList.SetEnabled(false);
                                weaponList.SetEnabled(false);
                                armorList.SetEnabled(false);
                                stateList.SetEnabled(false);
                                for (var j = 0; j < actorSubToggleList.Count; j++)
                                    actorSubToggleList[j].SetEnabled(false);
                                break;
                            case FlowKind.ENEMY:
                                _targetCommand.parameters[28] = "0";
                                enemyList.SetEnabled(false);
                                appearance_toggle.SetEnabled(false);
                                enemyState_toggle.SetEnabled(false);
                                enemyStateList.SetEnabled(false);
                                break;
                            case FlowKind.CHARACTER:
                                _targetCommand.parameters[32] = "0";
                                characterEventList.SetEnabled(false);
                                direction.SetEnabled(false);
                                provisionalMapContainer.SetEnabled(false);
                                break;
                            case FlowKind.VEHICLE:
                                _targetCommand.parameters[35] = "0";
                                vehicleList.SetEnabled(false);
                                break;
                            case FlowKind.GOLD:
                                _targetCommand.parameters[37] = "0";
                                conditionalC.SetEnabled(false);
                                goldField.SetEnabled(false);
                                break;
                            case FlowKind.ITEM:
                                _targetCommand.parameters[40] = "0";
                                itemList.SetEnabled(false);
                                break;
                            case FlowKind.WEAPON:
                                _targetCommand.parameters[42] = "0";
                                haveWeaponList.SetEnabled(false);
                                equipWeapon_toggle.SetEnabled(false);
                                break;
                            case FlowKind.ARMOR:
                                _targetCommand.parameters[45] = "0";
                                haveArmorList.SetEnabled(false);
                                equipArmor_toggle.SetEnabled(false);
                                break;
                            case FlowKind.BUTTON:
                                _targetCommand.parameters[48] = "0";
                                buttonList.SetEnabled(false);
                                actionList.SetEnabled(false);
                                break;
                        }

                        if (_targetCommand.parameters[1] == "1")
                            SubEvent(_targetCommand.parameters[2], kind);
                    }

                    if (mainToggleList[(int) kind].value)
                    {
                        ToggleActiveSettings((int) kind);
                        _toggleActiveNum = (int) kind;

                        switch (kind)
                        {
                            case FlowKind.SWITCH:
                                _targetCommand.parameters[3] = "1";
                                selectID = switchDropdownChoices.IndexOf(_targetCommand.parameters[4]);
                                if (selectID == -1)
                                    selectID = 0;
                                _targetCommand.parameters[4] = _flagDataModel.switches.Count > 0
                                    ? _flagDataModel.switches[selectID].id
                                    : "";
                                switchList.SetEnabled(true);
                                OnOff.SetEnabled(true);
                                break;
                            case FlowKind.VARIABLE:
                                _targetCommand.parameters[6] = "1";
                                selectID = variableDropdownChoices.IndexOf(_targetCommand.parameters[7]);
                                if (selectID == -1)
                                    selectID = 0;
                                _targetCommand.parameters[7] = _flagDataModel.variables.Count > 0
                                    ? _flagDataModel.variables[selectID].id
                                    : "";
                                variableList.SetEnabled(true);
                                conditionalList.SetEnabled(true);
                                constant_toggle.SetEnabled(true);
                                constant.SetEnabled(true);
                                variable_toggle.SetEnabled(true);
                                variable.SetEnabled(true);
                                break;
                            case FlowKind.SELF_SWITCH:
                                _targetCommand.parameters[12] = "1";
                                selfswitchList.SetEnabled(true);
                                selfOnOff.SetEnabled(true);
                                break;
                            case FlowKind.TIMER:
                                _targetCommand.parameters[15] = "1";
                                conditionalListB.SetEnabled(true);
                                minutes.SetEnabled(true);
                                second.SetEnabled(true);
                                break;
                            case FlowKind.ACTOR:
                                _targetCommand.parameters[19] = "1";
                                selectID = characterActorIDList.IndexOf(_targetCommand.parameters[20]);
                                if (selectID == -1)
                                    selectID = 0;
                                _targetCommand.parameters[20] = _characterActorDataModels[selectID].uuId;
                                var index = int.Parse(_targetCommand.parameters[21]);
                                if (index == -1)
                                    index = 0;
                                actorSubToggleList[index].value = true;

                                actorList.SetEnabled(true);
                                actorName.SetEnabled(true);
                                classList.SetEnabled(true);
                                skillList.SetEnabled(true);
                                weaponList.SetEnabled(true);
                                armorList.SetEnabled(true);
                                stateList.SetEnabled(true);
                                for (var j = 0; j < actorSubToggleList.Count; j++)
                                    actorSubToggleList[j].SetEnabled(true);
                                break;
                            case FlowKind.ENEMY:
                                _targetCommand.parameters[28] = "1";
                                selectID = 0;
                                selectID = stateIDArray.IndexOf(_targetCommand.parameters[31]);
                                if (selectID == -1)
                                    selectID = 0;
                                _targetCommand.parameters[31] = _stateDataModels.Count > 0 ? _stateDataModels[selectID].id : "";
                                enemyList.SetEnabled(true);
                                appearance_toggle.SetEnabled(true);
                                enemyState_toggle.SetEnabled(true);
                                enemyStateList.SetEnabled(enemyState_toggle.value);
                                break;
                            case FlowKind.CHARACTER:
                                _targetCommand.parameters[32] = "1";
                                characterEventList.SetEnabled(true);
                                direction.SetEnabled(true);
                                provisionalMapContainer.SetEnabled(true);
                                break;
                            case FlowKind.VEHICLE:
                                _targetCommand.parameters[35] = "1";
                                selectID = vehiclesIDArray.IndexOf(_targetCommand.parameters[36]);
                                if (selectID == -1)
                                    selectID = 0;
                                var vehicleId = _vehiclesDataModels.Count > 0 ?_vehiclesDataModels[selectID].id : "";
                                _targetCommand.parameters[36] = vehicleId;
                                vehicleList.SetEnabled(true);
                                break;
                            case FlowKind.GOLD:
                                _targetCommand.parameters[37] = "1";
                                conditionalC.SetEnabled(true);
                                goldField.SetEnabled(true);
                                break;
                            case FlowKind.ITEM:
                                _targetCommand.parameters[40] = "1";
                                selectID = itemID.IndexOf(_targetCommand.parameters[41]);
                                if (selectID == -1)
                                    selectID = 0;
                                _targetCommand.parameters[41] = _itemDataModels.Count > 0 ? _itemDataModels[selectID].basic.id : "";
                                itemList.SetEnabled(true);
                                break;
                            case FlowKind.WEAPON:
                                _targetCommand
                                    .parameters[42] = "1";
                                selectID = weaponIDList.IndexOf(_targetCommand.parameters[43]);
                                if (selectID == -1)
                                    selectID = 0;
                                _targetCommand.parameters[43] = _weaponDataModels.Count > 0 ? _weaponDataModels[selectID].basic.id : "";
                                haveWeaponList.SetEnabled(true);
                                equipWeapon_toggle.SetEnabled(true);
                                break;
                            case FlowKind.ARMOR:
                                _targetCommand.parameters[45] = "1";
                                selectID = armorIDList.IndexOf(_targetCommand.parameters[46]);
                                if (selectID == -1)
                                    selectID = 0;
                                _targetCommand.parameters[46] = _armorDataModels.Count > 0 ? _armorDataModels[selectID].basic.id : "";
                                haveArmorList.SetEnabled(true);
                                equipArmor_toggle.SetEnabled(true);
                                break;
                            case FlowKind.BUTTON:
                                _targetCommand.parameters[48] = "1";
                                buttonList.SetEnabled(true);
                                actionList.SetEnabled(true);
                                break;
                        }

                        if (_targetCommand.parameters[1] == "1")
                            AddEvent(_targetCommand.parameters[2], CreateEventParameters(kind));
                    }

                    if (_targetCommand.parameters[1] == "1")
                    {
                        //複数選択の場合、選択肢の選択可能状態を更新
                        //引数は未使用
                        ToggleActiveSettings(0);
                    }

                    Save(EventDataModels[EventIndex]);
                });
            }
        }

        /// <summary>
        /// 初期設定時、トグルがOFFの場合いじれない箇所を非アクティブにする
        /// </summary>
        private void InitToggleEnabled() {
            if (_targetCommand.parameters[3] == "0")
            {
                switchList.SetEnabled(false);
                OnOff.SetEnabled(false);
            }
            if (_targetCommand.parameters[6] == "0")
            {
                variableList.SetEnabled(false);
                conditionalList.SetEnabled(false);
                constant_toggle.SetEnabled(false);
                constant.SetEnabled(false);
                variable_toggle.SetEnabled(false);
                variable.SetEnabled(false);
            }
            if (_targetCommand.parameters[12] == "0")
            {
                selfswitchList.SetEnabled(false);
                selfOnOff.SetEnabled(false);
            }
            if (_targetCommand.parameters[15] == "0")
            {
                conditionalListB.SetEnabled(false);
                minutes.SetEnabled(false);
                second.SetEnabled(false);
            }
            if (_targetCommand.parameters[19] == "0")
            {
                actorList.SetEnabled(false);
                actorName.SetEnabled(false);
                classList.SetEnabled(false);
                skillList.SetEnabled(false);
                weaponList.SetEnabled(false);
                armorList.SetEnabled(false);
                stateList.SetEnabled(false);
                for (var j = 0; j < actorSubToggleList.Count; j++)
                    actorSubToggleList[j].SetEnabled(false);
            }
            if (_targetCommand.parameters[28] == "0")
            {
                enemyList.SetEnabled(false);
                appearance_toggle.SetEnabled(false);
                enemyState_toggle.SetEnabled(false);
                enemyStateList.SetEnabled(false);
            }
            if (_targetCommand.parameters[32] == "0")
            {
                characterEventList.SetEnabled(false);
                direction.SetEnabled(false);
                provisionalMapContainer.SetEnabled(false);
            }
            if (_targetCommand.parameters[35] == "0")
            {
                vehicleList.SetEnabled(false);
            }
            if (_targetCommand.parameters[37] == "0")
            {
                conditionalC.SetEnabled(false);
                goldField.SetEnabled(false);
            }
            if (_targetCommand.parameters[40] == "0")
            {
                itemList.SetEnabled(false);
            }
            if (_targetCommand.parameters[42] == "0")
            {
                haveWeaponList.SetEnabled(false);
                equipWeapon_toggle.SetEnabled(false);
            }
            if (_targetCommand.parameters[45] == "0")
            {
                haveArmorList.SetEnabled(false);
                equipArmor_toggle.SetEnabled(false);
            }
            if (_targetCommand.parameters[48] == "0")
            {
                buttonList.SetEnabled(false);
                actionList.SetEnabled(false);
            }
        }

        private void ToggleActiveSettings(int index) {
            //複数選択ではない場合
            if (_targetCommand.parameters[1] != "1")
            {
                //選択された項目以外は、選択可能状態とする
                for (var i = 0; i < mainToggleList.Count; i++)
                {
                    if (index != i)
                    {
                        mainToggleList[i].value = false;
                        mainToggleList[i].SetEnabled(true);
                    }
                }
                //選択された項目を、選択不可状態とする
                mainToggleList[index].SetEnabled(false);
            }
            //複数選択の場合
            else
            {
                //複数選択の場合には、一旦全て選択可能状態とする
                int checkCount = 0;
                for (var i = 0; i < mainToggleList.Count; i++)
                {
                    mainToggleList[i].SetEnabled(true);
                    if (mainToggleList[i].value)
                    {
                        checkCount++;
                    }
                }

                if (checkCount == 1)
                {
                    //現在の選択数が1の場合、現在選択中のものは、選択不可状態とする
                    for (int i = 0; i < mainToggleList.Count; i++)
                    {
                        if (mainToggleList[i].value)
                        {
                            mainToggleList[i].SetEnabled(false);
                            break;
                        }
                    }
                }
            }
        }

        private List<string> CreateEventParameters(FlowKind kind) {
            var parameters = new List<string>();
            switch (kind)
            {
                case FlowKind.SWITCH:
                    parameters = new List<string>
                    {
                        ((int) kind).ToString(),
                        _targetCommand.parameters[4],
                        _targetCommand.parameters[5]
                    };
                    break;
                case FlowKind.VARIABLE:
                    parameters = new List<string>
                    {
                        ((int) kind).ToString(),
                        _targetCommand.parameters[7],
                        _targetCommand.parameters[8],
                        _targetCommand.parameters[9],
                        _targetCommand.parameters[10],
                        _targetCommand.parameters[11]
                    };
                    break;
                case FlowKind.SELF_SWITCH:
                    parameters = new List<string>
                    {
                        ((int) kind).ToString(),
                        _targetCommand.parameters[13],
                        _targetCommand.parameters[14]
                    };
                    break;
                case FlowKind.TIMER:
                    parameters = new List<string>
                    {
                        ((int) kind).ToString(),
                        _targetCommand.parameters[16],
                        _targetCommand.parameters[17],
                        _targetCommand.parameters[18]
                    };
                    break;
                case FlowKind.ACTOR:
                    parameters = new List<string>
                    {
                        ((int) kind).ToString(),
                        _targetCommand.parameters[20],
                        _targetCommand.parameters[21],
                        _targetCommand.parameters[22],
                        _targetCommand.parameters[23],
                        _targetCommand.parameters[24],
                        _targetCommand.parameters[25],
                        _targetCommand.parameters[26],
                        _targetCommand.parameters[27]
                    };
                    break;
                case FlowKind.ENEMY:
                    parameters = new List<string>
                    {
                        ((int) kind).ToString(),
                        _targetCommand.parameters[29],
                        _targetCommand.parameters[30],
                        _targetCommand.parameters[31],
                        _targetCommand.parameters[32]
                    };
                    break;
                case FlowKind.CHARACTER:
                    parameters = new List<string>
                    {
                        ((int) kind).ToString(),
                        _targetCommand.parameters[33],
                        _targetCommand.parameters[34]
                    };
                    break;
                case FlowKind.VEHICLE:
                    parameters = new List<string>
                    {
                        ((int) kind).ToString(),
                        _targetCommand.parameters[36]
                    };
                    break;
                case FlowKind.GOLD:
                    parameters = new List<string>
                    {
                        ((int) kind).ToString(),
                        _targetCommand.parameters[38],
                        _targetCommand.parameters[39]
                    };
                    break;
                case FlowKind.ITEM:
                    parameters = new List<string>
                    {
                        ((int) kind).ToString(),
                        _targetCommand.parameters[41]
                    };
                    break;
                case FlowKind.WEAPON:
                    parameters = new List<string>
                    {
                        ((int) kind).ToString(),
                        _targetCommand.parameters[43]
                    };
                    break;
                case FlowKind.ARMOR:
                    parameters = new List<string>
                    {
                        ((int) kind).ToString(),
                        _targetCommand.parameters[46]
                    };
                    break;
                case FlowKind.BUTTON:
                    parameters = new List<string>
                    {
                        ((int) kind).ToString(),
                        _targetCommand.parameters[49],
                        _targetCommand.parameters[50]
                    };
                    break;
            }

            return parameters;
        }

        private void AddEvent(string isAndEvent, List<string> parameters) {
            _toggleActiveCount++;

            EventCommand eventDatas;
            if (_toggleActiveCount <= _multipleChoiceCount) return;

            if (isAndEvent == "0")
                eventDatas = new EventCommand((int) EventEnum.EVENT_CODE_FLOW_AND, parameters,
                    new List<EventCommandMoveRoute>());
            else
                eventDatas = new EventCommand((int) EventEnum.EVENT_CODE_FLOW_OR, parameters,
                    new List<EventCommandMoveRoute>());

            var index = EventCommandIndex + _toggleActiveCount;
            EventDataModels[EventIndex].eventCommands.Insert(index, eventDatas);
            _multipleChoiceCount++;
        }

        private void SubEvent(string isAndEvent, FlowKind toggleNum) {
            for (var i = EventCommandIndex + 1; i < EventDataModels[EventIndex].eventCommands.Count; i++)
                if (EventDataModels[EventIndex].eventCommands[i].code ==
                    (int) EventEnum.EVENT_CODE_FLOW_AND ||
                    EventDataModels[EventIndex].eventCommands[i].code ==
                    (int) EventEnum.EVENT_CODE_FLOW_OR)
                    if (EventDataModels[EventIndex].eventCommands[i].parameters[0] ==
                        ((int) toggleNum).ToString())
                    {
                        EventDataModels[EventIndex].eventCommands.RemoveAt(i);
                        _toggleActiveCount--;
                        _multipleChoiceCount--;
                        break;
                    }
        }

        private void EditEvent(FlowKind toggleNum, List<string> parameters) {
            //複数選択がOnになっているか
            if (_targetCommand.parameters[1] == "1")
                for (var i = EventCommandIndex + 1;
                        i < EventDataModels[EventIndex].eventCommands.Count;
                        i++)
                    //編集するのがAndかOrのイベントか
                    if (EventDataModels[EventIndex].eventCommands[i].code ==
                        (int) EventEnum.EVENT_CODE_FLOW_AND ||
                        EventDataModels[EventIndex].eventCommands[i].code ==
                        (int) EventEnum.EVENT_CODE_FLOW_OR)
                        if (EventDataModels[EventIndex].eventCommands[i].parameters[0] ==
                            ((int) toggleNum).ToString())
                        {
                            EventDataModels[EventIndex].eventCommands[i].parameters = parameters;
                            break;
                        }
        }

        private enum FlowKind
        {
            SWITCH,
            VARIABLE,
            SELF_SWITCH,
            TIMER,
            ACTOR,
            ENEMY,
            CHARACTER,
            VEHICLE,
            GOLD,
            ITEM,
            WEAPON,
            ARMOR,
            BUTTON
        }
    }
}