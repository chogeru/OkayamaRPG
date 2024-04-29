using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Armor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event.EventDataModel;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.GameProgress
{
    /// <summary>
    ///     [変数の操作]のコマンド設定枠の表示物
    /// </summary>
    public class GameVariable : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_variable_control.uxml";

        private List<ArmorDataModel> _armorDataModels;

        private SystemSettingDataModel _systemSettingDataModel;
        private List<string>           _variableIdList;
        private List<string>           _variableNameList;
        private RadioButton                 actor_toggle;

        private VisualElement            actorList;
        private RadioButton                   armor_toggle;
        private int                      armorId;
        private List<List<List<string>>> armorIDList;
        private VisualElement            armorList;
        private List<List<List<string>>> armorNameList;
        private PopupFieldBase<string>   armorPopupField;
        private VisualElement            armorType;
        private int                      armorTypeId;
        private PopupFieldBase<string>   armorTypePopupField;
        private List<string>             armorTypesIDList;
        private List<string>             armorTypesNameList;
        private RadioButton                   character_toggle;

        private IntegerField constant;

        private RadioButton        constant_toggle;
        private VisualElement dataList;
        private RadioButton        enemy_toggle;
        private VisualElement enemyList;
        private VisualElement enemyStatusList;

        private VisualElement equipmentList;

        private PopupFieldBase<string> equipmentPopupField;

        private int          equipmentTypeId;
        private List<string> equipTypesIDList;

        private List<string>  equipTypesNameList;
        private VisualElement eventList;
        private VisualElement provisionalMapContainer;
        private VisualElement itemNum;

        private RadioButton        itemNum_toggle;
        private RadioButton        justBefore_toggle;
        private VisualElement justBeforeList;
        private RadioButton        other_toggle;
        private VisualElement otherList;
        private RadioButton        party_toggle;
        private IntegerField  rand_max;
        private IntegerField  rand_min;
        private RadioButton        rand_toggle;
        private IntegerField  range_max;
        private IntegerField  range_min;
        private RadioButton        range_toggle;

        private VisualElement sole;

        private RadioButton        sole_toggle;
        private VisualElement sortOderList;
        private VisualElement statusList;
        private VisualElement variable;
        private RadioButton        variable_toggle;
        private VisualElement weapon;
        private RadioButton        weapon_toggle;

        private List<RadioButton> _radioButtons;

        private GenericPopupFieldBase<TargetCharacterChoice> _targetCharacterPopupField;

        private bool _initializeFlg;

        public GameVariable(
            VisualElement rootElement,
            List<EventDataModel> eventDataModels,
            int eventIndex,
            int eventCommandIndex
        )
            : base(rootElement, eventDataModels, eventIndex, eventCommandIndex) {
        }

        public override void Invoke() {
            _initializeFlg = false;

            var commandTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SettingUxml);
            VisualElement commandFromUxml = commandTree.CloneTree();
            EditorLocalize.LocalizeElements(commandFromUxml);
            commandFromUxml.style.flexGrow = 1;
            RootElement.Add(commandFromUxml);
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();

            _variableNameList = new List<string>();
            _variableIdList = new List<string>();
            ReloadVariableList();
            if (EventCommand.parameters.Count == 0)
            {
                // [変数の操作]が単独の場合：変数のID / [変数の操作]が範囲の場合：範囲の最小値
                EventCommand.parameters.Add(_variableIdList[0]);
                // 範囲の最大値
                EventCommand.parameters.Add("1");
                EventCommand.parameters.Add("0");
                // オペランド（0: 定数、1: 変数、2: 乱数、3: ゲームデータ）
                EventCommand.parameters.Add("0");
                // ゲームデータ（0: アイテム所持数 ... 8: その他）
                EventCommand.parameters.Add("0");
                EventCommand.parameters.Add("0");
                EventCommand.parameters.Add("0");
                EventCommand.parameters.Add("0");
                // 操作の範囲（0: 単独、1: 範囲）
                EventCommand.parameters.Add("0");

                EventManagementService.SaveEvent(EventDataModel);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            //部品初期化
            sole_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display91");
            range_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display92");

            constant_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display93");
            variable_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display94");
            rand_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display95");

            itemNum_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display96");
            weapon_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display97");
            armor_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display98");
            actor_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display99");
            enemy_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display100");
            character_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display101");
            party_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display102");
            justBefore_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display103");
            other_toggle = RootElement.Query<RadioButton>("radioButton-eventCommand-display104");
            
            _radioButtons = new List<RadioButton>()
            {
                constant_toggle,
                variable_toggle,
                rand_toggle,
                itemNum_toggle,
                weapon_toggle,
                armor_toggle,
                actor_toggle,
                enemy_toggle,
                character_toggle,
                party_toggle,
                justBefore_toggle,
                other_toggle
            };

            sole = RootElement.Query<VisualElement>("sole");
            range_min = RootElement.Query<IntegerField>("range_min");
            range_max = RootElement.Query<IntegerField>("range_max");
            constant = RootElement.Query<IntegerField>("constant");
            variable = RootElement.Query<VisualElement>("variable");
            rand_min = RootElement.Query<IntegerField>("rand_min");
            rand_max = RootElement.Query<IntegerField>("rand_max");
            itemNum = RootElement.Query<VisualElement>("itemNum");
            weapon = RootElement.Query<VisualElement>("weapon");
            equipmentList = RootElement.Query<VisualElement>("equipmentList");
            armorType = RootElement.Query<VisualElement>("armorType");
            armorList = RootElement.Query<VisualElement>("armorList");
            actorList = RootElement.Query<VisualElement>("actorList");
            statusList = RootElement.Query<VisualElement>("statusList");
            enemyList = RootElement.Query<VisualElement>("enemyList");
            enemyStatusList = RootElement.Query<VisualElement>("enemyStatusList");
            eventList = RootElement.Query<VisualElement>("eventList");
            dataList = RootElement.Query<VisualElement>("dataList");
            provisionalMapContainer = RootElement.Query<VisualElement>("provisional_map_popupfield_container");
            sortOderList = RootElement.Query<VisualElement>("sortOderList");
            justBeforeList = RootElement.Query<VisualElement>("justBeforList");
            otherList = RootElement.Query<VisualElement>("otherList");

            // [単独]プルダウンメニュー
            var selectID = _variableIdList.IndexOf(EventCommand.parameters[0]);
            if (selectID == -1)
                selectID = 0;
            var solePopupField = new PopupFieldBase<string>(_variableNameList, selectID);
            sole.Clear();
            sole.Add(solePopupField);
            solePopupField.RegisterValueChangedCallback(_ =>
            {
                if (!_initializeFlg) return;
                _initializeFlg = false;
                WaitMilliSecond();

                EventCommand.parameters[0] = _variableIdList[solePopupField.index];
                Save(EventDataModel);
            });

            // [単独]トグル
            SwitchControlEditItem(EventCommand.parameters[8]);
            sole_toggle.RegisterValueChangedCallback(o =>
            {
                if (!_initializeFlg) return;
                _initializeFlg = false;
                WaitMilliSecond();

                if (sole_toggle.value)
                {
                    EventCommand.parameters[8] = "0";
                    EventCommand.parameters[0] = _variableIdList[solePopupField.index];
                    Save(EventDataModel);
                }

                SwitchControlEditItem(EventCommand.parameters[8]);
            });

            // [範囲]トグル
            range_toggle.RegisterValueChangedCallback(o =>
            {
                if (!_initializeFlg) return;
                _initializeFlg = false;
                WaitMilliSecond();

                if (range_toggle.value)
                {
                    EventCommand.parameters[8] = "1";
                    EventCommand.parameters[0] = range_min.value.ToString();
                    EventCommand.parameters[1] = range_max.value.ToString();
                    Save(EventDataModel);
                }

                SwitchControlEditItem(EventCommand.parameters[8]);
            });


            // [範囲]最小値入力欄
            var minValue = 1;
            if (EventCommand.parameters[8] == "1")
                if (!int.TryParse(EventCommand.parameters[0], out minValue) || minValue < 1)
                    minValue = 1;

            range_min.value = minValue;
            range_min.RegisterCallback<FocusOutEvent>(o =>
            {
                // 「1 ≦ 最小値 ≦ 最大値」の範囲内に数値を収める 
                range_min.value = Math.Min(Math.Max(range_min.value, 1), range_max.value);

                EventCommand.parameters[0] = range_min.value.ToString();
                Save(EventDataModel);
            });

            // [範囲]最大値入力欄
            var maxValue = 1;
            if (EventCommand.parameters[8] == "1")
                if (!int.TryParse(EventCommand.parameters[1], out maxValue) || maxValue < 1)
                    maxValue = 1;

            range_max.value = maxValue;
            range_max.RegisterCallback<FocusOutEvent>(o =>
            {
                // 「最小値 ≦ 最大値 ≦ 変数の個数」の範囲内に数値を収める 
                range_max.value = Math.Min(Math.Max(range_max.value, range_min.value), _variableIdList.Count);

                EventCommand.parameters[1] = range_max.value.ToString();
                Save(EventDataModel);
            });

            // 操作
            VisualElement control = RootElement.Query<VisualElement>("control");
            var controlSelectTextDropdownChoices = EditorLocalize.LocalizeTexts(new List<string>
                    {"WORD_1016", "WORD_2600", "WORD_1017", "WORD_2601", "WORD_1018", "WORD_1019"});

            selectID = int.Parse(EventCommand.parameters[2]);
            if (selectID == -1)
                selectID = 0;

            var controlPopupField = new PopupFieldBase<string>(controlSelectTextDropdownChoices, selectID);
            control.Clear();
            control.Add(controlPopupField);
            controlPopupField.RegisterValueChangedCallback(evt =>
            {
                if (!_initializeFlg) return;
                _initializeFlg = false;
                WaitMilliSecond();

                EventCommand.parameters[2] = controlSelectTextDropdownChoices.IndexOf(controlPopupField.value).ToString();
                Save(EventDataModel);
            });

            //トグルの初期表示
            int operandNum = 0;
            int GameDataTypeNum = 0;
            if (EventCommand.parameters[3] != "")
            {
                try
                {
                    operandNum = int.Parse(EventCommand.parameters[3]);
                } catch (Exception) { }
            }
            if (EventCommand.parameters[4] != "")
            {
                try
                {
                    GameDataTypeNum = int.Parse(EventCommand.parameters[4]);
                } catch (Exception) { }
            }
            ToggleSettings((Operand) Enum.ToObject(typeof(Operand), operandNum),
                (GameDataType) Enum.ToObject(typeof(GameDataType), GameDataTypeNum));

            //定数設定
            if (EventCommand.parameters[3] == "0")
                constant.value = int.Parse(EventCommand.parameters[4]);

            constant.RegisterCallback<FocusOutEvent>(evt =>
            {
                constant.value = CSharpUtil.Clamp(
                    constant.value,
                    Runtime.Common.Component.Hud.GameProgress.GameVal.MinValue,
                    Runtime.Common.Component.Hud.GameProgress.GameVal.MaxValue);
                EventCommand.parameters[4] = constant.value.ToString();
                Save(EventDataModel);
            });

            //変数設定
            selectID = -1;
            if (EventCommand.parameters[3] == "1")
            {
                if (int.TryParse(EventCommand.parameters[4], out var n))
                {
                    EventCommand.parameters[4] = _variableIdList[int.Parse(EventCommand.parameters[4])];
                    Save(EventDataModel);
                }

                selectID = _variableIdList.IndexOf(EventCommand.parameters[4]);
            }
            if (selectID == -1)
                selectID = 0;

            var variablePopupField = new PopupFieldBase<string>(_variableNameList, selectID);
            variable.Clear();
            variable.Add(variablePopupField);
            variablePopupField.RegisterValueChangedCallback(o =>
            {
                if (!_initializeFlg) return;
                _initializeFlg = false;
                WaitMilliSecond();

                EventCommand.parameters[4] = _variableIdList[variablePopupField.index];
                Save(EventDataModel);
            });
            
            //乱数設定
            if (EventCommand.parameters[3] == "2")
            {
                rand_min.value = int.Parse(EventCommand.parameters[4]);
                rand_max.value = int.Parse(EventCommand.parameters[5]);
            }

            rand_min.RegisterCallback<FocusOutEvent>(evt =>
            {
                //最大値以下の際に入る
                if (rand_min.value <= rand_max.value)
                {
                    EventCommand.parameters[4] = rand_min.value.ToString();
                    Save(EventDataModel);
                }
                rand_min.value = int.Parse(EventCommand.parameters[4]);
            });

            rand_max.RegisterCallback<FocusOutEvent>(evt =>
            {
                EventCommand.parameters[5] = rand_max.value.ToString();
                Save(EventDataModel);
            });

            rand_toggle.RegisterValueChangedCallback(o =>
            {
                
            });

            //ゲームデータ
            //アイテム
            var itemDataModels = DatabaseManagementService.LoadItem();
            var itemNameList = new List<string>();
            var itemIDList = new List<string>();
            for (var i = 0; i < itemDataModels.Count; i++)
            {
                itemNameList.Add(itemDataModels[i].basic.name);
                itemIDList.Add(itemDataModels[i].basic.id);
            }

            selectID = -1;
            if (EventCommand.parameters[3] == "3")
                selectID = itemIDList.IndexOf(EventCommand.parameters[5]);
            else
                selectID = 0;
            if (selectID == -1)
                selectID = 0;

            var itemPopupField = new PopupFieldBase<string>(itemNameList, selectID);
            itemNum.Clear();
            itemNum.Add(itemPopupField);
            itemPopupField.RegisterValueChangedCallback(evt =>
            {
                if (!_initializeFlg) return;
                _initializeFlg = false;
                WaitMilliSecond();

                EventCommand.parameters[5] = itemIDList[itemPopupField.index];
                Save(EventDataModel);
            });

            itemNum_toggle.value = EventCommand.parameters[3] == "3" && EventCommand.parameters[4] == "0";
            itemNum.SetEnabled(itemNum_toggle.value);

            //武器
            var weaponDataModels = DatabaseManagementService.LoadWeapon();
            var weaponNameList = new List<string>();
            var weaponIDList = new List<string>();
            selectID = 0;
            for (var i = 0; i < weaponDataModels.Count; i++)
            {
                weaponNameList.Add(weaponDataModels[i].basic.name);
                weaponIDList.Add(weaponDataModels[i].basic.id);
            }

            if (EventCommand.parameters[3] == "3" && EventCommand.parameters[4] == "1")
                selectID = weaponIDList.IndexOf(EventCommand.parameters[5]);
            if (selectID == -1)
                selectID = 0;

            var weaponPopupField = new PopupFieldBase<string>(weaponNameList, selectID);
            weapon.Clear();
            weapon.Add(weaponPopupField);
            weaponPopupField.RegisterValueChangedCallback(evt =>
            {
                if (!_initializeFlg) return;
                _initializeFlg = false;
                WaitMilliSecond();

                EventCommand.parameters[5] = weaponIDList[weaponPopupField.index];
                Save(EventDataModel);
            });
            
            //防具
            _systemSettingDataModel = DatabaseManagementService.LoadSystem();
            _armorDataModels = DatabaseManagementService.LoadArmor();

            equipmentTypeId = 0;
            armorTypeId = 0;
            armorId = 0;

            equipTypesNameList = new List<string>();
            equipTypesIDList = new List<string>();

            armorTypesNameList = new List<string>();
            armorTypesIDList = new List<string>();

            armorNameList = new List<List<List<string>>>();
            armorIDList = new List<List<List<string>>>();

            for (var i = 1; i < _systemSettingDataModel.equipTypes.Count; i++)
            {
                equipTypesNameList.Add(_systemSettingDataModel.equipTypes[i].name);
                equipTypesIDList.Add(_systemSettingDataModel.equipTypes[i].id);
                armorNameList.Add(new List<List<string>>());
                armorIDList.Add(new List<List<string>>());
            }

            foreach (var armorTypes in _systemSettingDataModel.armorTypes)
            {
                armorTypesNameList.Add(armorTypes.name);
                armorTypesIDList.Add(armorTypes.id);

                for (var i = 0; i < armorNameList.Count; i++)
                {
                    armorNameList[i].Add(new List<string>());
                    armorIDList[i].Add(new List<string>());
                }
            }

            foreach (var armor in _armorDataModels)
            {
                var equipIndex = equipTypesIDList.IndexOf(armor.basic.equipmentTypeId);
                var typeIndex = armorTypesIDList.IndexOf(armor.basic.armorTypeId);
                if (equipIndex != -1 && typeIndex != -1)
                {
                    armorNameList[equipIndex][typeIndex].Add(armor.basic.name);
                    armorIDList[equipIndex][typeIndex].Add(armor.basic.id);
                }
            }

            if (EventCommand.parameters[3] == "3" && EventCommand.parameters[4] == "2")
            {
                equipmentTypeId = equipTypesIDList.IndexOf(EventCommand.parameters[5]);
                if (equipmentTypeId == -1)
                    equipmentTypeId = 0;
                armorTypeId = armorTypesIDList.IndexOf(EventCommand.parameters[6]);
                if (armorTypeId == -1)
                    armorTypeId = 0;
            }

            equipmentPopupField = new PopupFieldBase<string>(equipTypesNameList, equipmentTypeId);
            armorTypePopupField = new PopupFieldBase<string>(armorTypesNameList, armorTypeId);


            equipmentList.Clear();
            equipmentList.Add(equipmentPopupField);
            equipmentPopupField.RegisterValueChangedCallback(evt =>
            {
                if (!_initializeFlg) return;
                _initializeFlg = false;
                WaitMilliSecond();

                EventCommand.parameters[5] = equipTypesIDList[equipmentPopupField.index];

                CreateArmorPopupField();
                Save(EventDataModel);
            });

            armorType.Clear();
            armorType.Add(armorTypePopupField);
            armorTypePopupField.RegisterValueChangedCallback(evt =>
            {
                if (!_initializeFlg) return;
                _initializeFlg = false;
                WaitMilliSecond();

                EventCommand.parameters[6] = armorTypesIDList[armorTypePopupField.index];
                CreateArmorPopupField();
                Save(EventDataModel);
            });

            CreateArmorPopupField();
            
            //アクター
            var characterActorDataModels = DatabaseManagementService.LoadCharacterActor();
            var characterActorNameList = new List<string>();
            var characterActorIDList = new List<string>();
            selectID = 0;
            for (var i = 0; i < characterActorDataModels.Count; i++)
            {
                if (characterActorDataModels[i].charaType == (int)ActorTypeEnum.ACTOR)
                {
                    characterActorNameList.Add(characterActorDataModels[i].basic.name);
                    characterActorIDList.Add(characterActorDataModels[i].uuId);
                }
            }

            if (EventCommand.parameters[3] == "3" && EventCommand.parameters[4] == "3")
                selectID = characterActorIDList.IndexOf(EventCommand.parameters[5]);
            if (selectID == -1)
                selectID = 0;

            var actorPopupField = new PopupFieldBase<string>(characterActorNameList, selectID);
            actorList.Clear();
            actorList.Add(actorPopupField);
            actorPopupField.RegisterValueChangedCallback(evt =>
            {
                if (!_initializeFlg) return;
                _initializeFlg = false;
                WaitMilliSecond();

                EventCommand.parameters[5] = characterActorIDList[actorPopupField.index];
                Save(EventDataModel);
            });

            var statusSelectTextDropdownChoices = EditorLocalize.LocalizeTexts(new List<string>
            {
                "WORD_0139", "WORD_0144", "WORD_0133", "WORD_0135", "WORD_0395", "WORD_0539", "WORD_0177", "WORD_0178",
                "WORD_0179", "WORD_0180", "WORD_0181", "WORD_0182", "WORD_0136"
            });
            if (EventCommand.parameters[3] == "3" && EventCommand.parameters[4] == "3")
                selectID = int.Parse(EventCommand.parameters[6]);
            if (selectID == -1)
                selectID = 0;

            var statusListPopupField = new PopupFieldBase<string>(statusSelectTextDropdownChoices, selectID);
            statusList.Clear();
            statusList.Add(statusListPopupField);
            statusListPopupField.RegisterValueChangedCallback(evt =>
            {
                if (!_initializeFlg) return;
                _initializeFlg = false;
                WaitMilliSecond();

                EventCommand.parameters[6] = statusSelectTextDropdownChoices.IndexOf(statusListPopupField.value).ToString();
                Save(EventDataModel);
            });

            //敵
            var enemyDataModels = DatabaseManagementService.LoadEnemy();
            var enemyNameList = new List<string>();
            var enemyIDList = new List<string>();
            selectID = 0;
            for (var i = 0; i < enemyDataModels.Count; i++)
            {
                enemyNameList.Add(enemyDataModels[i].name);
                enemyIDList.Add(enemyDataModels[i].id);
                if (EventCommand.parameters[5] == enemyDataModels[i].id)
                    selectID = i;
            }

            if (EventCommand.parameters[3] == "3" && EventCommand.parameters[4] == "4")
                if (selectID == -1)
                    selectID = 0;

            var enemyPopupField = new PopupFieldBase<string>(enemyNameList, selectID);
            enemyList.Clear();
            enemyList.Add(enemyPopupField);
            enemyPopupField.RegisterValueChangedCallback(evt =>
            {
                if (!_initializeFlg) return;
                _initializeFlg = false;
                WaitMilliSecond();

                EventCommand.parameters[5] = enemyIDList[enemyPopupField.index];
                Save(EventDataModel);
            });

            var enemyStatusSelectTextDropdownChoices = EditorLocalize.LocalizeTexts(new List<string>
            {
                "WORD_0133", "WORD_0135", "WORD_0395", "WORD_0539", "WORD_0177", "WORD_0178", "WORD_0179", "WORD_0180",
                "WORD_0181", "WORD_0182", "WORD_0136"
            });
            if (EventCommand.parameters[3] == "3" && EventCommand.parameters[4] == "4")
                selectID = int.Parse(EventCommand.parameters[6]);

            var enemyStatusListPopupField = new PopupFieldBase<string>(enemyStatusSelectTextDropdownChoices, selectID);
            enemyStatusList.Clear();
            enemyStatusList.Add(enemyStatusListPopupField);
            enemyStatusListPopupField.RegisterValueChangedCallback(evt =>
            {
                if (!_initializeFlg) return;
                _initializeFlg = false;
                WaitMilliSecond();

                EventCommand.parameters[6] = enemyStatusSelectTextDropdownChoices.IndexOf(enemyStatusListPopupField.value).ToString();
                Save(EventDataModel);
            });
            
            //イベント
            {
                int targetCharacterParameterIndex = 5;
                AddOrHideProvisionalMapAndAddTargetCharacterPopupField(
                    targetCharacterParameterIndex,
                    provisionalMapPopupField =>
                    {
                        _targetCharacterPopupField = AddTargetCharacterPopupField(
                            eventList,
                            targetCharacterParameterIndex,
                            changeEvent =>
                            {
                                if (!_initializeFlg)
                                    return;
                                _initializeFlg = false;
                                WaitMilliSecond();

                                EventCommand.parameters[targetCharacterParameterIndex] = changeEvent.newValue.Id;
                                Save(EventDataModel);
                            },
                            forceMapId: provisionalMapPopupField?.value.MapDataModel?.id,
                            // 『ゲームデータ』>『キャラクター』がオフの場合、強制的に選択項目indexを0にする。
                            // (多分パラメータ配列要素を使い回ししているのが理由)。
                            forceDefaultIndexIsZero:
                                !(EventCommand.parameters[3] == "3" && EventCommand.parameters[4] == "5"),
                            //パラメータ配列要素を使いまわしているため、対象のトグルが有効な場合にのみ初期化処理を通す
                            forceInitialize: character_toggle.value);
                    });
            }

            selectID = 0;

            var dataListSelectTextDropdownChoices = EditorLocalize.LocalizeTexts(new List<string>
                    {"WORD_1025", "WORD_1026", "WORD_0858", "WORD_1027", "WORD_1028"});
            if (EventCommand.parameters[3] == "3" && EventCommand.parameters[4] == "5")
                selectID = int.Parse(EventCommand.parameters[6]);

            var dataListPopupField = new PopupFieldBase<string>(dataListSelectTextDropdownChoices, selectID);
            dataList.Clear();
            dataList.Add(dataListPopupField);
            dataListPopupField.RegisterValueChangedCallback(evt =>
            {
                if (!_initializeFlg) return;
                _initializeFlg = false;
                WaitMilliSecond();

                EventCommand.parameters[6] = dataListSelectTextDropdownChoices.IndexOf(dataListPopupField.value).ToString();
                Save(EventDataModel);
            });
            
            // [ゲームデータ]>[パーティ]のプルダウンメニュー
            var partyIndexChoices = new List<string>();
            var partyIndex = 0;

            //ゲーム中にパーティメンバーは最大4人となるため、4人で決め打ちの値となる
            partyIndexChoices.Add(EditorLocalize.LocalizeText("WORD_1604") + "#1");
            partyIndexChoices.Add(EditorLocalize.LocalizeText("WORD_1604") + "#2");
            partyIndexChoices.Add(EditorLocalize.LocalizeText("WORD_1604") + "#3");
            partyIndexChoices.Add(EditorLocalize.LocalizeText("WORD_1604") + "#4");

            if (EventCommand.parameters[3] == "3" && EventCommand.parameters[4] == "6")
                int.TryParse(EventCommand.parameters[5], out partyIndex);

            var sortOderListPopupField = new PopupFieldBase<string>(partyIndexChoices, partyIndex);
            sortOderList.Clear();
            sortOderList.Add(sortOderListPopupField);
            sortOderListPopupField.RegisterValueChangedCallback(evt =>
            {
                if (!_initializeFlg) return;
                _initializeFlg = false;
                WaitMilliSecond();

                EventCommand.parameters[5] = sortOderListPopupField.index.ToString();
                Save(EventDataModel);
            });

            

            // [ゲームデータ]>[パーティ]
            var justBeforeListIDList = EditorLocalize.LocalizeTexts(new List<string>
            {
                "WORD_1035", "WORD_1036", "WORD_1037", "WORD_1038", "WORD_1039", "WORD_1040"
            });

            selectID = 0;

            if (EventCommand.parameters[3] == "3" && EventCommand.parameters[4] == "7")
                selectID = int.Parse(EventCommand.parameters[5]);
            if (selectID < 0)
                selectID = 0;

            var justBeforeListPopupField = new PopupFieldBase<string>(justBeforeListIDList, selectID);
            justBeforeList.Clear();
            justBeforeList.Add(justBeforeListPopupField);
            justBeforeListPopupField.RegisterValueChangedCallback(evt =>
            {
                if (!_initializeFlg) return;
                _initializeFlg = false;
                WaitMilliSecond();

                EventCommand.parameters[5] = justBeforeListIDList.IndexOf(justBeforeListPopupField.value).ToString();
                Save(EventDataModel);
            });

            var otherListIDList = EditorLocalize.LocalizeTexts(new List<string>
            {
                "WORD_0995", "WORD_1041", "WORD_0581", "WORD_0698", "WORD_1042", "WORD_1043", "WORD_1044", "WORD_1045",
                "WORD_1046", "WORD_1047"
            });

            selectID = 0;

            if (EventCommand.parameters[3] == "3" && EventCommand.parameters[4] == "8")
                selectID = int.Parse(EventCommand.parameters[5]);
            if (selectID == -1)
                selectID = 0;

            var otherListPopupField = new PopupFieldBase<string>(otherListIDList, selectID);
            otherList.Clear();
            otherList.Add(otherListPopupField);
            otherListPopupField.RegisterValueChangedCallback(evt =>
            {
                if (!_initializeFlg) return;
                _initializeFlg = false;
                WaitMilliSecond();

                EventCommand.parameters[5] = otherListIDList.IndexOf(otherListPopupField.value).ToString();
                Save(EventDataModel);
            });
            
            var defaultSelect = int.Parse(EventCommand.parameters[3]);
            if (EventCommand.parameters[3] == "3")
            {
                if (EventCommand.parameters[4] == "0")
                {
                    defaultSelect = 3;
                }
                else if (EventCommand.parameters[4] == "1")
                {
                    defaultSelect = 4;
                }
                else if (EventCommand.parameters[4] == "2")
                {
                    defaultSelect = 5;
                }
                else if (EventCommand.parameters[4] == "3")
                {
                    defaultSelect = 6;
                }
                else if (EventCommand.parameters[4] == "4")
                {
                    defaultSelect = 7;
                }
                else if (EventCommand.parameters[4] == "5")
                {
                    defaultSelect = 8;
                }
                else if (EventCommand.parameters[4] == "6")
                {
                    defaultSelect = 9;
                }
                else if (EventCommand.parameters[4] == "7")
                {
                    defaultSelect = 10;
                }
                else if (EventCommand.parameters[4] == "8")
                {
                    defaultSelect = 11;
                }
            }

            new CommonToggleSelector().SetRadioSelector(
                _radioButtons,
                defaultSelect, new List<System.Action>
                {
                    //ON
                    () =>
                    {
                        if (!_initializeFlg) return;
                        _initializeFlg = false;
                        WaitMilliSecond();

                        EventCommand.parameters[3] = "0";
                        EventCommand.parameters[4] = constant.value.ToString();
                        Save(EventDataModel);
                        constant.SetEnabled(true);
                        ToggleSettings(Operand.CONSTANT, GameDataType.NONE);
                    },
                    //
                    () =>
                    {
                        if (!_initializeFlg) return;
                        _initializeFlg = false;
                        WaitMilliSecond();

                        EventCommand.parameters[3] = "1";
                        EventCommand.parameters[4] = _variableIdList[variablePopupField.index];
                        Save(EventDataModel);
                        variable.SetEnabled(true);
                        ToggleSettings(Operand.VARIABLE, GameDataType.NONE);
                    },
                    //
                    () =>
                    {
                        if (!_initializeFlg) return;
                        _initializeFlg = false;
                        WaitMilliSecond();

                        EventCommand.parameters[3] = "2";
                        EventCommand.parameters[4] = rand_min.value.ToString();
                        EventCommand.parameters[5] = rand_max.value.ToString();
                        Save(EventDataModel);
                        rand_min.SetEnabled(true);
                        rand_max.SetEnabled(true);
                        ToggleSettings(Operand.RANDOM_NUMBER, GameDataType.NONE);
                    },
                    //Item
                    () =>
                    {
                        if (!_initializeFlg) return;
                        _initializeFlg = false;
                        WaitMilliSecond();

                        EventCommand.parameters[3] = "3";
                        EventCommand.parameters[4] = "0";
                        EventCommand.parameters[5] = itemIDList[itemPopupField.index];
                        Save(EventDataModel);
                        itemNum.SetEnabled(true);
                        ToggleSettings(Operand.GAMEDATA, GameDataType.ITEM);
                    },
                    //Weapon
                    () =>
                    {
                        if (!_initializeFlg) return;
                        _initializeFlg = false;
                        WaitMilliSecond();

                        EventCommand.parameters[3] = "3";
                        EventCommand.parameters[4] = "1";
                        EventCommand.parameters[5] = weaponIDList[weaponPopupField.index];
                        Save(EventDataModel);
                        weapon.SetEnabled(true);
                        ToggleSettings(Operand.GAMEDATA, GameDataType.WEAPONS);
                    },
                    //Armor
                    () =>
                    {
                        if (!_initializeFlg) return;
                        _initializeFlg = false;
                        WaitMilliSecond();

                        EventCommand.parameters[3] = "3";
                        EventCommand.parameters[4] = "2";
                        EventCommand.parameters[5] = equipTypesIDList[equipmentPopupField.index];
                        EventCommand.parameters[6] = armorTypesIDList[armorTypePopupField.index];

                        if (armorPopupField.value == EditorLocalize.LocalizeText("WORD_0113"))
                        {
                            EventCommand.parameters[7] = armorPopupField.value;
                        }
                        else
                        {
                            var list = armorIDList[equipmentPopupField.index][armorTypePopupField.index];
                            EventCommand.parameters[7] = list[armorPopupField.index];
                        }

                        Save(EventDataModel);
                        equipmentList.SetEnabled(true);
                        armorType.SetEnabled(true);
                        armorList.SetEnabled(true);
                        ToggleSettings(Operand.GAMEDATA, GameDataType.ARMOR);
                    },
                    //Actor
                    () =>
                    {
                        if (!_initializeFlg) return;
                        _initializeFlg = false;
                        WaitMilliSecond();

                        if (actor_toggle.value)
                        {
                            EventCommand.parameters[3] = "3";
                            EventCommand.parameters[4] = "3";
                            EventCommand.parameters[5] = characterActorIDList[actorPopupField.index];
                            EventCommand.parameters[6] = statusSelectTextDropdownChoices.IndexOf(statusListPopupField.value).ToString();
                            Save(EventDataModel);
                            actorList.SetEnabled(true);
                            statusList.SetEnabled(true);
                            ToggleSettings(Operand.GAMEDATA, GameDataType.ACTORS);
                        }
                    },
                    //ENemy
                    () =>
                    {
                        if (!_initializeFlg) return;
                        _initializeFlg = false;
                        WaitMilliSecond();

                        if (enemy_toggle.value)
                        {
                            EventCommand.parameters[3] = "3";
                            EventCommand.parameters[4] = "4";
                            EventCommand.parameters[5] = enemyIDList[enemyPopupField.index];
                            EventCommand.parameters[6] = enemyStatusSelectTextDropdownChoices.IndexOf(enemyStatusListPopupField.value).ToString();
                            Save(EventDataModel);
                            enemyList.SetEnabled(true);
                            enemyStatusList.SetEnabled(true);
                            ToggleSettings(Operand.GAMEDATA, GameDataType.ENEMIES);
                        }
                    },
                    //Chara
                    () =>
                    {
                        if (!_initializeFlg) return;
                        _initializeFlg = false;
                        WaitMilliSecond();

                        if (character_toggle.value)
                        {
                            EventCommand.parameters[3] = "3";
                            EventCommand.parameters[4] = "5";
                            EventCommand.parameters[5] = _targetCharacterPopupField.value.Id;
                            EventCommand.parameters[6] = dataListSelectTextDropdownChoices.IndexOf(dataListPopupField.value).ToString();
                            Save(EventDataModel);
                            eventList.SetEnabled(true);
                            dataList.SetEnabled(true);
                            provisionalMapContainer.SetEnabled(true);
                            ToggleSettings(Operand.GAMEDATA, GameDataType.CHARACTER);
                        }
                    },
                    //Party
                    () =>
                    {
                        if (!_initializeFlg) return;
                        _initializeFlg = false;
                        WaitMilliSecond();

                        if (party_toggle.value)
                        {
                            EventCommand.parameters[3] = "3";
                            EventCommand.parameters[4] = "6";
                            EventCommand.parameters[5] = sortOderListPopupField.index.ToString();
                            Save(EventDataModel);
                            sortOderList.SetEnabled(true);
                            ToggleSettings(Operand.GAMEDATA, GameDataType.PARTY);
                        }
                    },
                    //JustBefore
                    () =>
                    {
                        if (!_initializeFlg) return;
                        _initializeFlg = false;
                        WaitMilliSecond();

                        if (justBefore_toggle.value)
                        {
                            EventCommand.parameters[3] = "3";
                            EventCommand.parameters[4] = "7";
                            EventCommand.parameters[5] = justBeforeListIDList.IndexOf(justBeforeListPopupField.value).ToString();
                            Save(EventDataModel);
                            justBeforeList.SetEnabled(true);
                            ToggleSettings(Operand.GAMEDATA, GameDataType.JUST_BEFORE);
                        }
                    },
                    //Other
                    () =>
                    {
                        if (!_initializeFlg) return;
                        _initializeFlg = false;
                        WaitMilliSecond();

                        if (other_toggle.value)
                        {
                            EventCommand.parameters[3] = "3";
                            EventCommand.parameters[4] = "8";
                            EventCommand.parameters[5] = otherListIDList.IndexOf(otherListPopupField.value).ToString();
                            Save(EventDataModel);
                            otherList.SetEnabled(true);
                            ToggleSettings(Operand.GAMEDATA, GameDataType.OTHER);
                        }
                    },
                });

            //少し待ってから初期化終了
            WaitMilliSecond();
        }

        private async void WaitMilliSecond() {
            await Task.Delay(1);
            _initializeFlg = true;
        }

        private void ToggleSettings(Operand operand, GameDataType gameDataType) {
            //全てDisabledとする
            constant_toggle.value = false;
            constant.SetEnabled(false);

            variable_toggle.value = false;
            variable.SetEnabled(false);

            rand_toggle.value = false;
            rand_min.SetEnabled(false);
            rand_max.SetEnabled(false);

            itemNum_toggle.value = false;
            itemNum.SetEnabled(false);

            weapon_toggle.value = false;
            weapon.SetEnabled(false);

            armor_toggle.value = false;
            equipmentList.SetEnabled(false);
            armorList.SetEnabled(false);
            armorType.SetEnabled(false);

            actor_toggle.value = false;
            actorList.SetEnabled(false);
            statusList.SetEnabled(false);

            enemy_toggle.value = false;
            enemyList.SetEnabled(false);
            enemyStatusList.SetEnabled(false);

            character_toggle.value = false;
            eventList.SetEnabled(false);
            dataList.SetEnabled(false);
            provisionalMapContainer.SetEnabled(false);

            party_toggle.value = false;
            sortOderList.SetEnabled(false);

            justBefore_toggle.value = false;
            justBeforeList.SetEnabled(false);

            other_toggle.value = false;
            otherList.SetEnabled(false);

            //ラジオボタンは全て入力可能状態とする
            constant_toggle.SetEnabled(true);
            variable_toggle.SetEnabled(true);
            rand_toggle.SetEnabled(true);
            itemNum_toggle.SetEnabled(true);
            weapon_toggle.SetEnabled(true);
            armor_toggle.SetEnabled(true);
            actor_toggle.SetEnabled(true);
            enemy_toggle.SetEnabled(true);
            character_toggle.SetEnabled(true);
            party_toggle.SetEnabled(true);
            justBefore_toggle.SetEnabled(true);
            other_toggle.SetEnabled(true);

            //定数
            if (operand == Operand.CONSTANT)
            {
                constant_toggle.value = true;
                constant.SetEnabled(true);
            }

            //変数
            else if (operand == Operand.VARIABLE)
            {
                variable_toggle.value = true;
                variable.SetEnabled(true);
            }

            //乱数
            else if (operand == Operand.RANDOM_NUMBER)
            {
                rand_toggle.value = true;
                rand_min.SetEnabled(true);
                rand_max.SetEnabled(true);
            }

            //ゲームデータ
            else
            {
                //アイテム
                if (gameDataType == GameDataType.ITEM)
                {
                    itemNum_toggle.value = true;
                    itemNum.SetEnabled(true);
                }

                //武器
                else if (gameDataType == GameDataType.WEAPONS)
                {
                    weapon_toggle.value = true;
                    weapon.SetEnabled(true);
                }

                //防具
                else if (gameDataType == GameDataType.ARMOR)
                {
                    armor_toggle.value = true;
                    equipmentList.SetEnabled(true);
                    armorList.SetEnabled(true);
                    armorType.SetEnabled(true);
                }

                //アクター
                else if (gameDataType == GameDataType.ACTORS)
                {
                    actor_toggle.value = true;
                    actorList.SetEnabled(true);
                    statusList.SetEnabled(true);
                }

                //敵
                else if (gameDataType == GameDataType.ENEMIES)
                {
                    enemy_toggle.value = true;
                    enemyList.SetEnabled(true);
                    enemyStatusList.SetEnabled(true);
                }

                //イベント
                else if (gameDataType == GameDataType.CHARACTER)
                {
                    character_toggle.value = true;
                    eventList.SetEnabled(true);
                    dataList.SetEnabled(true);
                    provisionalMapContainer.SetEnabled(true);
                }

                //パーティ
                else if (gameDataType == GameDataType.PARTY)
                {
                    party_toggle.value = true;
                    sortOderList.SetEnabled(true);
                }

                //JustBefore
                else if (gameDataType == GameDataType.JUST_BEFORE)
                {
                    justBefore_toggle.value = true;
                    justBeforeList.SetEnabled(true);
                }

                //その他
                else if (gameDataType == GameDataType.OTHER)
                {
                    other_toggle.value = true;
                    otherList.SetEnabled(true);
                }
            }
        }

        /// <summary>
        ///     クラス内で保持している変数の情報リストの更新
        /// </summary>
        private void ReloadVariableList() {
            var variables = DatabaseManagementService.LoadFlags().variables;
            _variableNameList.Clear();
            _variableIdList.Clear();

            for (var i = 0; i < variables.Count; i++)
            {
                if (variables[i].name == "")
                    _variableNameList.Add(EditorLocalize.LocalizeText("WORD_1518"));
                else
                    _variableNameList.Add(variables[i].name);

                _variableIdList.Add(variables[i].id);
            }
        }

        /// <summary>
        ///     変数の操作範囲に基づいて有効にする設定項目を切り替える
        /// </summary>
        /// <param name="control">0: 単独、1: 範囲</param>
        private void SwitchControlEditItem(string control) {
            sole_toggle.value = control == "0";
            sole.SetEnabled(sole_toggle.value);

            range_toggle.value = control == "1";
            range_min.SetEnabled(range_toggle.value);
            range_max.SetEnabled(range_toggle.value);
        }

        private void CreateArmorPopupField() {
            if (EventCommand.parameters[3] == "3" && EventCommand.parameters[4] == "2")
            {
                armorId = 0;
                if (EventCommand.parameters[7] != EditorLocalize.LocalizeText("WORD_0113"))
                {
                    var list = armorIDList[equipmentPopupField.index][armorTypePopupField.index];
                    armorId = list.IndexOf(EventCommand.parameters[7]);
                    if (armorId == -1)
                        armorId = 0;
                }
            }

            var names = armorNameList[equipmentPopupField.index][armorTypePopupField.index];
            if (names.Count == 0)
                names.Add(EditorLocalize.LocalizeText("WORD_0113"));
            armorPopupField = new PopupFieldBase<string>(names, armorId);
            armorList.Clear();
            armorList.Add(armorPopupField);
            armorPopupField.RegisterValueChangedCallback(evt =>
            {
                if (!_initializeFlg) return;
                if (armorPopupField.value == EditorLocalize.LocalizeText("WORD_0113"))
                {
                    EventCommand.parameters[7] = armorPopupField.value;
                }
                else
                {
                    var list = armorIDList[equipmentPopupField.index][armorTypePopupField.index];
                    EventCommand.parameters[7] = list[armorPopupField.index];
                }

                Save(EventDataModel);
            });
        }

        private enum Operand
        {
            CONSTANT,
            VARIABLE,
            RANDOM_NUMBER,
            GAMEDATA
        }

        private enum GameDataType
        {
            ITEM,
            WEAPONS,
            ARMOR,
            ACTORS,
            ENEMIES,
            CHARACTER,
            PARTY,
            JUST_BEFORE,
            OTHER,
            NONE
        }
    }
}