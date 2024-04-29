using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Armor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Class;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCustom;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Weapon;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Hierarchy.Enum;
using RPGMaker.Codebase.Editor.Inspector.ExpGraph.View;
using RPGMaker.Codebase.Editor.Inspector.StatusAutoGuide.View;
using RPGMaker.Codebase.Editor.Inspector.StatusGraph.View;
using RPGMaker.Codebase.Editor.Inspector.Trait.View;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.Editor.Inspector.Trait.View.TraitsInspectorElement;

namespace RPGMaker.Codebase.Editor.Inspector.CharacterClass.View
{
    /// <summary>
    /// [キャラクター]-[職業の編集] Inspector
    /// </summary>
    public class CharacterClassInspectorElement : AbstractInspectorElement
    {
        // 定数
        //-----------------------------------------------------------------------
        private const int INCREASE_MIN_VALUE = 10;
        private const int INCREASE_MAX_VALUE = 50;
        private const int GROWTH_MIN_VALUE   = 0;
        private const int GROWTH_MAX_VALUE   = 40;

        // 能力の最大値
        public static readonly List<int> PARAM_MAX_VALUE = new List<int>
        {
            9999, // 最大HP
            9999, // 最大MP
            9999, // 最大攻撃力
            9999, // 最大防御力
            9999, // 最大魔法攻撃力
            9999, // 最大魔法防御力
            9999, // 最大速度
            9999 // 最大運
        };

        private static readonly int PEAK_LEVEL_MAX_VALUE = 99;

        private static readonly int PEAK_LEVEL_MIN_VALUE = 2;
        //private ClassDataModel         _class   = null;

        //Idの保持
        private readonly string _id;

        private readonly List<int> PARAM_MIN_VALUE = new List<int>
        {
            1, // 最低HP
            0, // 最低MP
            0, // 最低攻撃力
            0, // 最低防御力
            0, // 最低魔法攻撃力
            0, // 最低魔法防御力
            0, // 最低速度
            0 // 最低運
        };

        private ClassDataModel       _actorClass;
        private List<ClassDataModel> _actorClassDataModels;
        private List<ClassDataModel> _classDataModels;

        // データ
        //-----------------------------------------------------------------------
        //ヒエラルキー側の保持
        private List<SkillCustomDataModel> _skillCustomDataModels;
        private SystemSettingDataModel     _systemSettingDataModel;

        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/CharacterClass/Asset/inspector_character_class.uxml"; } }

        public CharacterClassInspectorElement(string id) {
            _id = id;
            Refresh();
        }

        private int _itemId;
        private int _saveId;

        /**
         * 更新処理
         */
        override protected void RefreshContents() {
            base.RefreshContents();
            _actorClassDataModels = databaseManagementService.LoadCharacterActorClass();
            _classDataModels = databaseManagementService.LoadClassCommon();
            _systemSettingDataModel = databaseManagementService.LoadSystem();
            _skillCustomDataModels = databaseManagementService.LoadSkillCustom();

            var itemId = 0;
            if (_id != "-1")
            {
                //おそらく0番目しかない
                for (var i = 0; i < _actorClassDataModels.Count; i++)
                    if (_actorClassDataModels[i].id == _id)
                    {
                        itemId = i;
                        _actorClass = _actorClassDataModels[i];
                        break;
                    }

                //あり得ないと思うが。
                if (_actorClass.parameter.maxHp.Count == 0)
                {
                    _actorClass.parameter = ClassDataModel.Parameter.CreateDefault();
                    Save();
                }
            }
            else
            {
                //新規作成
                _actorClass = ClassDataModel.CreateDefault(Guid.NewGuid().ToString(),
                    "#" + string.Format("{0:D4}", _classDataModels.Count + 1) +
                    EditorLocalize.LocalizeText("WORD_1518"));
                _classDataModels.Add(_actorClass);
                Save();
            }

            _itemId = itemId;
            Initialize();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();

            //class_basic_id
            Label classBasicId = RootContainer.Query<Label>("class_basic_id");
            classBasicId.text = _actorClass.SerialNumberString;

            //名前の入力部分
            ImTextField classBasicName = RootContainer.Query<ImTextField>("class_basic_name");
            classBasicName.value = _actorClass.basic.name;
            classBasicName.RegisterCallback<FocusOutEvent>(evt =>
            {
                _actorClass.basic.name = classBasicName.value;
                Save(_itemId);
                _UpdateSceneView();
            });

            var systemData = databaseManagementService.LoadSystem();

            //属性タイプの部分
            //属性タイプの追加ボタン
            //Button addElementType = RootContainer.Query<Button>("add_elementType");
            //このVisualElementに要素を追加していく
            VisualElement elementTypeArea = RootContainer.Query<VisualElement>("elementType_area");

            //最初に表示させる用
            SummonElementType();

            //各属性タイプの表示、制御メソッド
            void SummonElementType() {
                elementTypeArea.Clear();
                //今の職業の属性タイプ分回す
                {
                    //追加するVisualElement
                    var addElementTypeArea = new VisualElement();
                    var elementTypeDropdown = new VisualElement();

                    addElementTypeArea.Add(elementTypeDropdown);
                    elementTypeArea.Add(addElementTypeArea);

                    var dropdownNum = 0;
                    for (var j = 0; j < _systemSettingDataModel.elements.Count; j++)
                        //現在の属性タイプ
                        if (_systemSettingDataModel.elements[j].id ==
                            _actorClass.element)
                        {
                            dropdownNum = j;
                            break;
                        }

                    //属性タイプのドロップダウン表示
                    var elementTypesName = new List<string>();
                    foreach (var element in systemData.elements)
                        elementTypesName.Add(element.value);
                    if (elementTypesName.Count > 0)
                    {
                        var elementEquipmentTypePopupField = new PopupFieldBase<string>(elementTypesName,
                            dropdownNum);
                        elementTypeDropdown.Add(elementEquipmentTypePopupField);
                        elementEquipmentTypePopupField.RegisterValueChangedCallback(evt =>
                        {
                            _actorClass.element = _systemSettingDataModel
                                .elements[elementTypesName.IndexOf(elementEquipmentTypePopupField.value)].id;
                            Save(_itemId);
                        });
                    }
                }
            }

            //ウェポンタイプの部分
            //ウェポンタイプの追加ボタン
            Button addWeaponType = RootContainer.Query<Button>("add_weaponType");
            //このVisualElementに要素を追加していく
            VisualElement weaponTypeArea = RootContainer.Query<VisualElement>("weaponType_area");

            //最初に表示させる用
            SummonWeaponType();
            //追加ボタンが押されたら増える
            addWeaponType.clicked += () =>
            {
                _actorClass.weaponTypes.Add("");
                SummonWeaponType();
                Save(_itemId);
            };

            //各武器タイプの表示、制御メソッド
            void SummonWeaponType() {
                weaponTypeArea.Clear();
                //今の職業の武器タイプ分回す
                for (var i = 0; i < _actorClass.weaponTypes.Count; i++)
                {
                    var num = i;
                    //追加するVisualElement
                    var addWeaponTypeArea = new InspectorItemUnit();
                    var weaponTypeLabel = new Label();
                    var rightArea = new VisualElement();
                    rightArea.AddToClassList("multiple_item_in_row");
                    var weaponTypeDropdown = new VisualElement();
                    weaponTypeDropdown.style.flexGrow = 1;
                    var deleteWeaponType = new Button();
                    deleteWeaponType.AddToClassList("small");

                    addWeaponTypeArea.Add(weaponTypeLabel);
                    rightArea.Add(weaponTypeDropdown);
                    rightArea.Add(deleteWeaponType);
                    addWeaponTypeArea.Add(rightArea);
                    weaponTypeArea.Add(addWeaponTypeArea);

                    addWeaponTypeArea.AddToClassList("row_element");
                    weaponTypeLabel.text = EditorLocalize.LocalizeText("WORD_0378") + (i + 1);
                    weaponTypeLabel.name = i.ToString();
                    deleteWeaponType.text = EditorLocalize.LocalizeText("WORD_0383");

                    var dropdownNum = 0;
                    for (var j = 0; j < systemData.weaponTypes.Count; j++)
                        //現在のウェポンタイプ
                        if (systemData.weaponTypes[j].id ==
                            _actorClass.weaponTypes[i])
                        {
                            dropdownNum = j;
                            break;
                        }

                    //ウェポンタイプのドロップダウン表示
                    var weaponTypesName = new List<string>();
                    foreach (var weapon in systemData.weaponTypes)
                        weaponTypesName.Add(weapon.value);

                    if (weaponTypesName.Count > 0)
                    {
                        var weaponEquipmentTypePopupField = new PopupFieldBase<string>(weaponTypesName,
                            dropdownNum);
                        weaponTypeDropdown.Add(weaponEquipmentTypePopupField);
                        weaponEquipmentTypePopupField.RegisterValueChangedCallback(evt =>
                        {
                            _actorClass.weaponTypes[num] =
                                systemData.weaponTypes[weaponEquipmentTypePopupField.index].id;
                            Save(_itemId);
                        });
                        //ウェポンタイプのdelete処理
                        deleteWeaponType.clicked += () =>
                        {
                            //武器タイプのラベルに各タイプの番号を仕込んであるのでそこから削除を行う
                            _actorClass.weaponTypes = _actorClass.weaponTypes
                                .Where((source, index) => index != int.Parse(weaponTypeLabel.name)).ToList();
                            SummonWeaponType();
                            Save(_itemId);
                        };
                    }
                }

                SetInitialEquip();
            }

            //アーマータイプの部分
            //アーマータイプの追加ボタン
            Button addArmorType = RootContainer.Query<Button>("add_armorType");
            //このVisualElementに要素を追加していく
            VisualElement armorType_area = RootContainer.Query<VisualElement>("armorType_area");

            //最初に表示させる用
            SummonArmorType();
            //追加ボタンが押されたら増える
            addArmorType.clicked += () =>
            {
                _actorClass.armorTypes.Add("");
                SummonArmorType();
                Save(_itemId);
            };

            //各防具タイプの表示、制御メソッド
            void SummonArmorType() {
                armorType_area.Clear();
                //今の職業の防具タイプ分回す
                for (var i = 0; i < _actorClass.armorTypes.Count; i++)
                {
                    var num = i;
                    //追加するVisualElement
                    var addArmorTypeArea = new InspectorItemUnit();
                    var armorTypeLabel = new Label();
                    var rightArea = new VisualElement();
                    var armorTypeDropdown = new VisualElement();
                    armorTypeDropdown.style.flexGrow = 1;
                    var deleteArmorType = new Button();
                    deleteArmorType.AddToClassList("small");

                    addArmorTypeArea.Add(armorTypeLabel);
                    rightArea.Add(armorTypeDropdown);
                    rightArea.Add(deleteArmorType);
                    addArmorTypeArea.Add(rightArea);
                    armorType_area.Add(addArmorTypeArea);

                    rightArea.AddToClassList("row_element");
                    armorTypeLabel.text = EditorLocalize.LocalizeText("WORD_0379") + (i + 1);
                    armorTypeLabel.name = i.ToString();
                    deleteArmorType.text = EditorLocalize.LocalizeText("WORD_0383");

                    var dropdownNum = 0;
                    for (var j = 0; j < systemData.armorTypes.Count; j++)
                        //現在のアーマータイプ
                        if (systemData.armorTypes[j].id == _actorClass.armorTypes[i])
                        {
                            dropdownNum = j;
                            break;
                        }

                    //アーマータイプのドロップダウン表示
                    var armorTypesName = new List<string>();
                    foreach (var armor in systemData.armorTypes)
                        armorTypesName.Add(armor.name);
                    if (armorTypesName.Count > 0)
                    {
                        var armorEquipmentTypePopupField = new PopupFieldBase<string>(armorTypesName,
                            dropdownNum);
                        armorTypeDropdown.Add(armorEquipmentTypePopupField);
                        armorEquipmentTypePopupField.RegisterValueChangedCallback(evt =>
                        {
                            _actorClass.armorTypes[num] =
                                systemData.armorTypes[armorEquipmentTypePopupField.index].id;
                            Save(_itemId);
                        });
                        //アーマータイプのdelete処理
                        deleteArmorType.clicked += () =>
                        {
                            //武器タイプのラベルに各タイプの番号を仕込んであるのでそこから削除を行う
                            _actorClass.armorTypes = _actorClass.armorTypes
                                .Where((source, index) => index != int.Parse(armorTypeLabel.name)).ToList();
                            SummonArmorType();
                            Save(_itemId);
                        };
                    }
                }

                SetInitialEquip();
            }

            // スキル
            // 追加ボタン
            Button addSkillType = RootContainer.Query<Button>("add_skillType");
            //このVisualElementに要素を追加していく
            VisualElement skillType_area = RootContainer.Query<VisualElement>("skillType_area");

            //最初に表示させる用
            SummonSkillType();
            //追加ボタンが押されたら増える
            addSkillType.clicked += () =>
            {
                var skillType = ClassDataModel.SkillType.CreateDefault();
                skillType.skillId = "1";
                _actorClass.skillTypes.Add(skillType);
                SummonSkillType();
                Save(_itemId);
            };

            //各スキルタイプの表示、制御メソッド
            void SummonSkillType() {
                skillType_area.Clear();
                //今の職業のスキルタイプ分回す
                for (var i = 0; i < _actorClass.skillTypes.Count; i++)
                {
                    var num = i;
                    //追加するVisualElement
                    var addSkillTypeArea = new Foldout {text = EditorLocalize.LocalizeText("WORD_0381") + (i + 1)};
                    addSkillTypeArea.name = "foldout_"  + _actorClass.basic.id + "_" + (i + 1);
                    //var skillTypeLabel = new Label();
                    var rightArea = new VisualElement();
                    var skillTypeDropdown = new VisualElement();
                    skillTypeDropdown.style.flexGrow = 1;
                    var skillIdLabel = new Label();
                    skillIdLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                    var skillIdDropdown = new VisualElement();
                    skillIdDropdown.style.flexGrow = 1f;
                    var skillLevelLabel = new Label();
                    skillLevelLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                    var skillLevelField = new IntegerField();
                    skillLevelField.AddToClassList("input_field_3");
                    var deleteSkillType = new Button();
                    deleteSkillType.AddToClassList("small");

                    rightArea.Add(skillTypeDropdown);
                    rightArea.Add(skillIdLabel);
                    rightArea.Add(skillIdDropdown);
                    rightArea.Add(skillLevelLabel);
                    rightArea.Add(skillLevelField);
                    addSkillTypeArea.Add(rightArea);
                    skillType_area.Add(addSkillTypeArea);
                    addSkillTypeArea.Add(deleteSkillType);

                    rightArea.AddToClassList("row_element");
                    skillIdLabel.text = EditorLocalize.LocalizeText("WORD_2592");
                    skillIdLabel.name = i.ToString();
                    skillLevelLabel.text = EditorLocalize.LocalizeText("WORD_0382");
                    skillLevelField.maxLength = 2;
                    skillLevelField.value = _actorClass.skillTypes[num].level;
                    deleteSkillType.text = EditorLocalize.LocalizeText("WORD_0383");

                    // このループ内で編集するスキルのデータ
                    SkillCustomDataModel targetModel = null;

                    if (string.IsNullOrEmpty(_actorClass.skillTypes[i].skillId))
                    {
                        // 生成直後でIDが空の場合はひとまず先頭のスキルの情報を使用する
                        targetModel = _skillCustomDataModels[0];
                    }
                    else
                    {
                        // 生成直後ではないのにIDが一致するスキルが見つからなかった場合は警告を出す
                        targetModel =
                            _skillCustomDataModels.FirstOrDefault(v => v.basic.id == _actorClass.skillTypes[i].skillId);

                        if (targetModel == null) targetModel = _skillCustomDataModels[0];
                    }

                    // スキルタイプのドロップダウン表示
                    var skillTypeList = _systemSettingDataModel.skillTypes.Select(v => v.value).ToList();
                    if (skillTypeList.Count > 0)
                    {
                        var skillEquipmentTypePopupField
                            = new PopupFieldBase<string>(skillTypeList, targetModel.basic.skillType);
                        skillTypeDropdown.Add(skillEquipmentTypePopupField);

                        skillEquipmentTypePopupField.RegisterValueChangedCallback(evt =>
                        {
                            // 現在選択されている選択肢のインデックスを確認し、対応した職業データ内に格納
                            foreach (var skill in _skillCustomDataModels)
                                if (skill.basic.skillType == skillEquipmentTypePopupField.index)
                                {
                                    _actorClass.skillTypes[num].skillId = skill.basic.id;
                                    break;
                                }

                            SummonSkillType();
                            Save(_itemId);
                        });
                    }

                    // タイプが一致するスキルの名前一覧を取得
                    var skillList = _skillCustomDataModels.Where(v => v.basic.skillType == targetModel.basic.skillType);
                    var nameList = skillList.Select(v => v.basic.name).ToList();
                    var skillEquipmentIdPopupField =
                        new PopupFieldBase<string>(nameList, nameList.IndexOf(targetModel.basic.name));
                    skillIdDropdown.Add(skillEquipmentIdPopupField);

                    skillEquipmentIdPopupField.RegisterValueChangedCallback(evt =>
                    {
                        var selectSkill = skillList.ElementAt(skillEquipmentIdPopupField.index);
                        _actorClass.skillTypes[num].skillId = selectSkill.basic.id;
                        Save(_itemId);
                    });

                    // スキル習得レベルの入力欄
                    skillLevelField.RegisterCallback<FocusOutEvent>(evt =>
                    {
                        // 最大、最低値範囲
                        if (skillLevelField.value > 99)
                            skillLevelField.value = 99;
                        else if (skillLevelField.value < 0)
                            skillLevelField.value = 0;

                        _actorClass.skillTypes[num].level = skillLevelField.value;
                        Save(_itemId);
                    });

                    // スキルのdelete処理
                    deleteSkillType.clicked += () =>
                    {
                        //武器タイプのラベルに各タイプの番号を仕込んであるのでそこから削除を行う
                        _actorClass.skillTypes = _actorClass.skillTypes
                            .Where((source, index) => index != int.Parse(skillIdLabel.name)).ToList();
                        SummonSkillType();
                        Save(_itemId);
                    };
                }
            }

            //----------------------------------
            // 経験値曲線
            //----------------------------------
            
            ScrollView scrollView = RootContainer.Query<ScrollView>("graph_scroll_view");
            scrollView.style.overflow = Overflow.Hidden;
            // 経験値グラフ取得
            ExpGraphElement exp_graph = RootContainer.Query<ExpGraphElement>("exp_graph");
            exp_graph.style.overflow = Overflow.Hidden;
            // 入力フィールド取得
            IntegerField increaseValueA = RootContainer.Query<IntegerField>("increase_value_a");
            IntegerField increaseValueB = RootContainer.Query<IntegerField>("increase_value_b");
            IntegerField growthParam = RootContainer.Query<IntegerField>("growth_param");

            // 傾きA　この辺りも簡略化できそう
            increaseValueA.value = _actorClass.expScore.increaseValueA;

            if (increaseValueA.value < INCREASE_MIN_VALUE)
                increaseValueA.value = INCREASE_MIN_VALUE;
            else if (increaseValueA.value > INCREASE_MAX_VALUE)
                increaseValueA.value = INCREASE_MAX_VALUE;

            increaseValueA.RegisterCallback<FocusOutEvent>(evt =>
            {
                if (increaseValueA.value < INCREASE_MIN_VALUE)
                    increaseValueA.value = INCREASE_MIN_VALUE;
                else if (increaseValueA.value > INCREASE_MAX_VALUE)
                    increaseValueA.value = INCREASE_MAX_VALUE;

                // グラフに適用
                exp_graph.SetExp(_actorClass, _classDataModels[0].maxLevel,
                    _classDataModels[0].clearLevel, increaseValueA.value, increaseValueB.value,
                    growthParam.value, _classDataModels[0].expGainIncreaseValue);

                _actorClass.expScore.increaseValueA = increaseValueA.value;
                Save(_itemId);
            });
            // 傾きB
            increaseValueB.value = _actorClass.expScore.increaseValueB;

            if (increaseValueB.value < INCREASE_MIN_VALUE)
                increaseValueB.value = INCREASE_MIN_VALUE;
            else if (increaseValueB.value > INCREASE_MAX_VALUE)
                increaseValueB.value = INCREASE_MAX_VALUE;

            increaseValueB.RegisterCallback<FocusOutEvent>(evt =>
            {
                if (increaseValueB.value < INCREASE_MIN_VALUE)
                    increaseValueB.value = INCREASE_MIN_VALUE;
                else if (increaseValueB.value > INCREASE_MAX_VALUE)
                    increaseValueB.value = INCREASE_MAX_VALUE;

                // グラフに適用
                exp_graph.SetExp(_actorClass, _classDataModels[0].maxLevel,
                    _classDataModels[0].clearLevel, increaseValueA.value, increaseValueB.value,
                    growthParam.value, _classDataModels[0].expGainIncreaseValue);

                _actorClass.expScore.increaseValueB = increaseValueB.value;
                Save(_itemId);
            });
            // 補正値
            growthParam.value = _actorClass.expScore.growType;
            growthParam.RegisterCallback<FocusOutEvent>(evt =>
            {
                if (growthParam.value < GROWTH_MIN_VALUE)
                    growthParam.value = GROWTH_MIN_VALUE;
                else if (growthParam.value > GROWTH_MAX_VALUE)
                    growthParam.value = GROWTH_MAX_VALUE;

                // グラフに適用
                exp_graph.SetExp(_actorClass, _classDataModels[0].maxLevel,
                    _classDataModels[0].clearLevel, increaseValueA.value, increaseValueB.value,
                    growthParam.value, _classDataModels[0].expGainIncreaseValue);

                _actorClass.expScore.growType = growthParam.value;
                Save(_itemId);
            });

            // ボタン周り
            Button next_exp_table = RootContainer.Query<Button>("next_exp_table");
            Button total_exp_table = RootContainer.Query<Button>("total_exp_table");

            // グラフに適用
            exp_graph.SetExp(_actorClass, _classDataModels[0].maxLevel,
                _classDataModels[0].clearLevel, increaseValueA.value, increaseValueB.value,
                growthParam.value, _classDataModels[0].expGainIncreaseValue);

            next_exp_table.focusable = false;
            var btnBackColor = next_exp_table.style.backgroundColor;
            next_exp_table.style.backgroundColor = Color.gray;
            next_exp_table.clicked += () =>
            {
                next_exp_table.focusable = false;
                total_exp_table.focusable = true;
                next_exp_table.style.backgroundColor = Color.gray;
                total_exp_table.style.backgroundColor = btnBackColor;
                exp_graph.DispNextExp();
            };

            total_exp_table.clicked += () =>
            {
                next_exp_table.focusable = true;
                total_exp_table.focusable = false;
                total_exp_table.style.backgroundColor = Color.gray;
                next_exp_table.style.backgroundColor = btnBackColor;
                exp_graph.DispTotalExp();
            };

            //----------------------------------
            // オートガイド、初期化
            //----------------------------------
            // パラメータに代入する順番
            var classParam = new List<ClassDataModel.Ability>
            {
                _actorClass.abilityScore.maxHp,
                _actorClass.abilityScore.maxMp,
                _actorClass.abilityScore.attack,
                _actorClass.abilityScore.defense,
                _actorClass.abilityScore.magicAttack,
                _actorClass.abilityScore.magicDefense,
                _actorClass.abilityScore.speed,
                _actorClass.abilityScore.luck
            };

            StatusAutoGuideElement status_auto_guide = RootContainer.Query<StatusAutoGuideElement>("status_auto_guide");
            
            //新規作成時、オートガイド設定されていないため、設定する
            if (_actorClass.autoGuide.maxHp == -1 && _actorClass.autoGuide.attack == -1 &&
                _actorClass.autoGuide.defense == -1 && _actorClass.autoGuide.magicAttack == -1 &&
                _actorClass.autoGuide.magicDefense == -1 && _actorClass.autoGuide.speed == -1 &&
                _actorClass.autoGuide.luck == -1)
            {
                _actorClass.autoGuide.maxHp = 0;
                _actorClass.autoGuide.attack = 0;
                _actorClass.autoGuide.defense = 0;
                _actorClass.autoGuide.magicAttack = 0;
                _actorClass.autoGuide.magicDefense = 0;
                _actorClass.autoGuide.speed = 0;
                _actorClass.autoGuide.luck = 0;
                status_auto_guide.InitStatusGuide(_actorClass.autoGuide, _actorClass.parameter);
                ExecStatusAutoGuide(_itemId, status_auto_guide, RootContainer);
            }
            else
            {
                status_auto_guide.InitStatusGuide(_actorClass.autoGuide, _actorClass.parameter);
            }

            // 適用ボタン設定
            Button autogide_apply = RootContainer.Query<Button>("autogide_apply");
            autogide_apply.clicked += () =>
            {
                ExecStatusAutoGuide(_itemId, status_auto_guide, RootContainer);
            };

            //----------------------------------
            // 能力値グラフ周り
            //----------------------------------

 
            // パラメータのLv1の値、Maxの値、ピークLv設定、成長タイプ、グラフ設定
            for (var i = 0; i < RootContainer.Query<IntegerField>("class_param_min").ToList().Count; i++)
            {
                var count = i;

                // パラメータのLv1の値
                var classParamMin = RootContainer.Query<IntegerField>("class_param_min").AtIndex(i);
                // パラメータMaxの値
                var classParamMax = RootContainer.Query<IntegerField>("class_param_max").AtIndex(i);
                // ピークLv設定
                var classParamPeak = RootContainer.Query<IntegerField>("class_param_peak").AtIndex(i);
                // 成長タイプ
                var classGrowthType = RootContainer.Query<Slider>("growth_type").AtIndex(i);
                
                var classGrowthTypeField = RootContainer.Query<IntegerField>("growth_type_field").AtIndex(i);
                // グラフ設定
                var hp_graph = RootContainer.Query<Button>("graph").AtIndex(i);
                var status_graph = RootContainer.Query<StatusGraphElement>().AtIndex(i);

                // 値代入
                classParamMin.value = classParam[i].paramOne;
                classParamMax.value = classParam[i].paramMax;
                classParamPeak.value = classParam[i].paramPeakLv;
                classGrowthType.lowValue = -10.0f;
                classGrowthType.highValue = 10.0f;
                classGrowthType.value = classParam[i].growType;

                classGrowthTypeField.value = classParam[i].growType;

                // グラフの初期化
                status_graph.SetParamOne(classParam[i].paramOne);
                status_graph.SetParamMax(classParam[i].paramMax);
                status_graph.SetParamPeakLv(classParam[i].paramPeakLv);
                status_graph.SetParamGrow(classParam[i].growType);
                status_graph.InitStatusGraph(_actorClass.parameter,
                    (StatusGraphElement.Type) Enum.ToObject(typeof(StatusGraphElement.Type), i));

                // コールバック登録
                classParamMin.RegisterCallback<FocusOutEvent>(evt =>
                {
                    if (classParamMin.value < PARAM_MIN_VALUE[count])
                        classParamMin.value = PARAM_MIN_VALUE[count];

                    if (classParamMin.value > PARAM_MAX_VALUE[count])
                        classParamMin.value = PARAM_MAX_VALUE[count];

                    classParam[count].paramOne = classParamMin.value;
                    status_graph.SetParamOne(classParam[count].paramOne);
                    status_graph.UpDateGraph(_actorClass.parameter);
                    Save(_itemId);
                });
                classParamMax.RegisterCallback<FocusOutEvent>(evt =>
                {
                    if (classParamMax.value < PARAM_MIN_VALUE[count])
                        classParamMax.value = PARAM_MIN_VALUE[count];

                    if (classParamMax.value > PARAM_MAX_VALUE[count])
                        classParamMax.value = PARAM_MAX_VALUE[count];

                    classParam[count].paramMax = classParamMax.value;
                    status_graph.SetParamMax(classParam[count].paramMax);
                    status_graph.UpDateGraph(_actorClass.parameter);
                    Save(_itemId);
                });
                classParamPeak.RegisterCallback<FocusOutEvent>(evt =>
                {
                    if (classParamPeak.value < PEAK_LEVEL_MIN_VALUE)
                        classParamPeak.value = PEAK_LEVEL_MIN_VALUE;

                    if (classParamPeak.value > PEAK_LEVEL_MAX_VALUE)
                        classParamPeak.value = PEAK_LEVEL_MAX_VALUE;

                    classParam[count].paramPeakLv = classParamPeak.value;
                    status_graph.SetParamPeakLv(classParam[count].paramPeakLv);
                    status_graph.UpDateGraph(_actorClass.parameter);
                    Save(_itemId);
                });
                classGrowthType.RegisterValueChangedCallback(evt =>
                {
                    classGrowthTypeField.value = (int) classGrowthType.value;
                    classParam[count].growType = (int) classGrowthType.value;
                    status_graph.SetParamGrow(classParam[count].growType);
                    status_graph.UpDateGraph(_actorClass.parameter);
                    Save(_itemId);
                });
                
                BaseInputFieldHandler.IntegerFieldCallback(classGrowthTypeField, evt =>
                {
                    classGrowthType.value = classGrowthTypeField.value;
                    classParam[count].growType = classGrowthTypeField.value;
                    status_graph.SetParamGrow(classParam[count].growType);
                    status_graph.UpDateGraph(_actorClass.parameter);
                    Save(_itemId);
                }, -10, 10);
                
                hp_graph.clicked += () => { };
            }

            //----------------------------------
            // 追加能力値
            //----------------------------------
            FloatField classAbilityAddHitRate = RootContainer.Query<FloatField>("class_ability_add_hit_rate");
            if (_actorClass.traits.Count > 0)
                classAbilityAddHitRate.value = (_actorClass.traits[0].value / 10f);
            else
                _actorClass.traits.Add(new TraitCommonDataModel(2, 2, 7, 0));

            BaseInputFieldHandler.FloatFieldCallback(classAbilityAddHitRate, evt =>
            {
                _actorClass.traits[0].value = (int)(classAbilityAddHitRate.value * 10);
                Save(_itemId);
            }, 0, 1000);

            FloatField classAbilityAddEvasionRate = RootContainer.Query<FloatField>("class_ability_add_evasion_rate");
            if (_actorClass.traits.Count > 1)
                classAbilityAddEvasionRate.value = _actorClass.traits[1].value / 10f;
            else
                _actorClass.traits.Add(new TraitCommonDataModel(2, 2, 8, 0));

            BaseInputFieldHandler.FloatFieldCallback(classAbilityAddEvasionRate, evt =>
            {
                _actorClass.traits[1].value = (int)(classAbilityAddEvasionRate.value * 10);
                Save(_itemId);
            }, 0, 1000);

            FloatField classAbilityAddCriticalRate = RootContainer.Query<FloatField>("class_ability_add_critical_rate");
            if (_actorClass.traits.Count > 2)
                classAbilityAddCriticalRate.value = _actorClass.traits[2].value / 10f;
            else
                _actorClass.traits.Add(new TraitCommonDataModel(2, 2, 9, 0));

            BaseInputFieldHandler.FloatFieldCallback(classAbilityAddCriticalRate, evt =>
            {
                _actorClass.traits[2].value = (int)(classAbilityAddCriticalRate.value * 10);
                Save(_itemId);
            }, 0, 1000);

            //特殊能力値
            //狙われ率
            FloatField classAbilitySpTargetedRate = RootContainer.Query<FloatField>("class_ability_sp_targeted_rate");
            if (_actorClass.traits.Count > 3)
            {
                classAbilitySpTargetedRate.value = _actorClass.traits[3].value / 10f;
                _actorClass.traits[3].effectId = 0;
            }
            else
            {
                _actorClass.traits.Add(new TraitCommonDataModel(2, 3, 0, 0));
            }

            BaseInputFieldHandler.FloatFieldCallback(classAbilitySpTargetedRate, evt =>
            {
                _actorClass.traits[3].value = (int)(classAbilitySpTargetedRate.value * 10);
                Save(_itemId);
            }, 0, 1000);


            var traitsWork = new List<TraitCommonDataModel>();
            if (_actorClass.traits.Count > 3)
                for (var i = 4; i < _actorClass.traits.Count; i++)
                    traitsWork.Add(_actorClass.traits[i]);

            //上記で固定特徴を持っているので3つ飛ばす
            List<TraitCommonDataModel> enemyTraits;
            VisualElement classTraitsArea = RootContainer.Query<VisualElement>("class_ability_other_area");
            var traitWindow = new TraitsInspectorElement();
            classTraitsArea.Add(traitWindow);
            traitWindow.Init(traitsWork, TraitsType.TRAITS_TYPE_CLASS, evt =>
            {
                enemyTraits = (List<TraitCommonDataModel>) evt;
                //上で三つ飛ばした部分から更新をする
                traitsWork = enemyTraits;

                //固定以外の特徴の削除
                for (var i = _actorClass.traits.Count - 1; i > 3; i--) _actorClass.traits.RemoveAt(i);

                //固定以外の特徴の更新
                for (var i = 0; i < enemyTraits.Count; i++)
                    _actorClass.traits.Add(enemyTraits[i]);

                for (var i = 0; i < _classDataModels.Count; i++)
                    if (_classDataModels[i].id == _actorClass.id)
                    {
                        _classDataModels[i] = _actorClass;
                        break;
                    }

                //保存
                Save(_itemId);
            });
        }

        private void ExecStatusAutoGuide(int id, StatusAutoGuideElement statusAutoGuide, VisualElement items) {
            
            var classParam = new List<ClassDataModel.Ability>
            {
                _actorClass.abilityScore.maxHp,
                _actorClass.abilityScore.maxMp,
                _actorClass.abilityScore.attack,
                _actorClass.abilityScore.defense,
                _actorClass.abilityScore.magicAttack,
                _actorClass.abilityScore.magicDefense,
                _actorClass.abilityScore.speed,
                _actorClass.abilityScore.luck
            };

            // オートガイドパラメータ作成
            var param = statusAutoGuide.CreateParameter();

            // 簡略化できそう
            for (var i = 0; i < items.Query<IntegerField>("class_param_min").ToList().Count; i++)
            {
                var count = i;

                // パラメータ設定先
                var classParamMin = items.Query<IntegerField>("class_param_min").AtIndex(i);
                var classParamMax = items.Query<IntegerField>("class_param_max").AtIndex(i);

                // 値制限
                if (param[i, 0] > PARAM_MAX_VALUE[count])
                    param[i, 0] = PARAM_MAX_VALUE[count];
                else if (param[i, 0] < PARAM_MIN_VALUE[count])
                    param[i, 0] = PARAM_MIN_VALUE[count];

                if (param[i, 1] > PARAM_MAX_VALUE[count])
                    param[i, 1] = PARAM_MAX_VALUE[count];
                else if (param[i, 1] < PARAM_MIN_VALUE[count])
                    param[i, 1] = PARAM_MIN_VALUE[count];

                classParamMax.value = param[i, 0];
                classParamMin.value = param[i, 1];

                classParam[count].paramOne = classParamMin.value;
                classParam[count].paramMax = classParamMax.value;

                // グラフの更新
                var status_graph = items.Query<StatusGraphElement>().AtIndex(i);
                status_graph.SetParamOne(classParam[count].paramOne);
                status_graph.SetParamMax(classParam[count].paramMax);
                status_graph.UpDateGraph(_actorClass.parameter);
            }

            var autoGuide = statusAutoGuide.GetAutoGuideParams();
            _actorClass.autoGuide.maxHp = autoGuide[0];
            _actorClass.autoGuide.attack = autoGuide[1];
            _actorClass.autoGuide.defense = autoGuide[2];
            _actorClass.autoGuide.magicAttack = autoGuide[3];
            _actorClass.autoGuide.magicDefense = autoGuide[4];
            _actorClass.autoGuide.speed = autoGuide[5];
            _actorClass.autoGuide.luck = autoGuide[6];

            Save(_itemId);
        }

        /**
         * 保存処理
         */
        private void Save(int id = -1) {
            _saveId = id;
            base.Save();
        }

        protected override void SaveContents() {
            base.SaveContents();

            //セーブ部位の作成
            //後で作成する
            var _actorClassDataModels = databaseManagementService.LoadCharacterActorClass();

            // -1は新規追加
            if (_saveId == -1)
                _actorClassDataModels.Add(_actorClass);
            else
                _actorClassDataModels[_saveId] = _actorClass;

            //params更新
            _actorClass.UpdateGraph();

            //更新処理
            databaseManagementService.SaveCharacterActorClass(_actorClassDataModels);
        }

        private void _UpdateSceneView() {
            _ = Editor.Hierarchy.Hierarchy.Refresh(Region.Character, _actorClass.id);
        }


        private void SetInitialEquip() {

            //初期装備
            //職業の防具タイプを取得
            //防具を取得
            //そのうえで装備タイプが一緒だった箇所に初期装備として表示させる

            var equipTypes = databaseManagementService.LoadSystem().equipTypes;
            var wList = databaseManagementService.LoadWeapon();
            var weaponList = new List<WeaponDataModel>();

            foreach (var cw in _actorClass.weaponTypes)
            foreach (var w in wList)
                if (w.basic.weaponTypeId == cw)
                    weaponList.Add(w);

            var actors = databaseManagementService.LoadCharacterActor();


            var aList = databaseManagementService.LoadArmor();
            var armorList = new List<ArmorDataModel>();
            foreach (var ca in _actorClass.armorTypes)
            foreach (var l in aList)
                if (l.basic.armorTypeId == ca)
                    armorList.Add(l);

            foreach (var actor in actors)
            {
                if (actor.basic.classId != _actorClass.id) continue;

                if (actor.equips.Count == 0) actor.equips = new List<CharacterActorDataModel.Equipment>();

                var data = new List<CharacterActorDataModel.Equipment>();
                foreach (var t in equipTypes)
                {
                    var dataWork = new CharacterActorDataModel.Equipment(t.id, "");
                    foreach (var t1 in actor.equips)
                        if (t.id == t1.type)
                        {
                            dataWork.value = t1.value;
                            break;
                        }

                    data.Add(dataWork);
                }

                actor.equips = data;
                
                WeaponDataModel weaponData = null;
                var strList = new List<string>();
                strList.Add(EditorLocalize.LocalizeText("WORD_0113"));
                var defaultIndex = -1;
                for (var j = 0; j < weaponList.Count; j++) strList.Add(weaponList[j].basic.name);
                try
                {
                    for (int i = 0; i < weaponList.Count; i++)
                        if (weaponList[i].basic.id == actor.equips[0].value)
                        {
                            weaponData = weaponList[i];
                            break;
                        }
                    defaultIndex = strList.IndexOf(weaponData.basic.name);
                }
                catch (Exception)
                {
                }
                
                if (defaultIndex < 0)
                {
                    //このケースでは、職業を変更した等の理由で、装備不可能なものが選択されているため、初期化する
                    defaultIndex = 0;
                    actor.equips[0].value = "";
                }

                for (var i = 1; i < equipTypes.Count; i++)
                {
                    strList = new List<string>();
                    strList.Add(EditorLocalize.LocalizeText("WORD_0113"));
                    for (var j = 0; j < armorList.Count; j++)
                        if (equipTypes[i].id == armorList[j].basic.equipmentTypeId)
                            strList.Add(armorList[j].basic.name);

                    ArmorDataModel armorData = null;
                    defaultIndex = -1;
                    try
                    {
                        for (int i2 = 0; i2 < armorList.Count; i2++)
                            if (armorList[i2].basic.id == actor.equips[i].value)
                            {
                                armorData = armorList[i2];
                                break;
                            }
                        defaultIndex = strList.IndexOf(armorData.basic.name);
                    }
                    catch (Exception)
                    {
                    }

                    if (defaultIndex < 0)
                    {
                        //このケースでは、職業を変更した等の理由で、装備不可能なものが選択されているため、初期化する
                        defaultIndex = 0;
                        actor.equips[i].value = "";
                    }
                }

            }

            databaseManagementService.SaveCharacterActor(actors);
        }
    }
}