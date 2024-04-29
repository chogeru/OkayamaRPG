using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.Trait.View
{
    /// <summary>
    /// アクター/職業/敵に設定する[特徴]
    /// </summary>
    public class TraitsInspectorElement : AbstractInspectorElement
    {
        public delegate void CallBackTrait(object data);

        public enum TraitsType
        {
            TRAITS_TYPE_ACTOR = 0,
            TRAITS_TYPE_CLASS,
            TRAITS_TYPE_WEAPON,
            TRAITS_TYPE_ARMOR,
            TRAITS_TYPE_STATE,
            TRAITS_TYPE_ENEMY
        }

        private readonly Dictionary<TraitsAbilityScore, string> _abilityScoreDictionary =
            EditorLocalize.LocalizeDictionaryValues(new Dictionary<TraitsAbilityScore, string>
            {
                {TraitsAbilityScore.NORMAL_ABILITY_SCORE, "WORD_1398"},
                {TraitsAbilityScore.ADD_ABILITY_SCORE, "WORD_0402"},
                {TraitsAbilityScore.SPECIAL_ABILITY_SCORE, "WORD_0404"}
            });

        private readonly bool[,] _abilityScoreEnabled =
        {
            //通常能力値
            {true, true, false, false, true, false},
            //追加能力値
            {true, true, true, true, true, true},
            //特殊能力値
            {true, true, true, true, true, true}
        };

        //追加能力
        private readonly List<string> _additionAbility = EditorLocalize.LocalizeTexts(new List<string>
        {
            "WORD_0183", "WORD_0184", "WORD_0403",
            "WORD_2596", "WORD_1399",
            "WORD_1400", "WORD_1401", "WORD_1402", "WORD_1403", "WORD_1404"
        });

        private readonly bool[,] _additionEnabled =
        {
            //命中率
            {true, true, true, true, true, true},
            //回避率
            {true, true, true, true, true, true},
            //会心率
            {true, true, true, true, true, true},
            //回避率
            {true, true, true, true, true, true},
            //魔法回避率
            {true, true, true, true, true, true},
            //魔法反射率
            {true, true, true, true, true, true},
            //反撃率
            {true, true, true, true, true, true},
            //HP再生率
            {true, true, true, true, true, true},
            //MP再生率
            {true, true, true, true, true, true},
            //TP再生率
            {true, true, true, true, true, true}
        };

        //防具タイプリスト
        private List<string> _armorArray = new List<string>();

        private readonly Dictionary<TraitsAttack, string> _attackDictionary =
            EditorLocalize.LocalizeDictionaryValues(new Dictionary<TraitsAttack, string>
            {
                {TraitsAttack.ATTACK_ATTRIBUTE, "WORD_1414"},
                {TraitsAttack.ATTACK_STATE, "WORD_1415"},
                {TraitsAttack.ATTACK_SPEED_CORRECTION, "WORD_1416"},
                {TraitsAttack.ATTACK_ADD_COUNT, "WORD_1417"},
                {TraitsAttack.ATTACK_SKILL, "WORD_1418"}
            });

        private readonly bool[,] _attackEnabled =
        {
            //攻撃時属性
            {true, true, true, true, true, true},
            //攻撃時ステート
            {true, true, true, true, true, true},
            //攻撃速度補正
            {true, true, true, true, true, true},
            //攻撃追加回数
            {true, true, true, true, true, true},
            //攻撃スキル
            {true, true, true, true, true, true}
        };

        //一部共通で使えるList
        //属性private 
        private readonly List<string> _attribute = new List<string>();

        private CallBackTrait _callBackTrait;

        private readonly Dictionary<TraitsEnums.TraitsCategory, string> _categoryDictionary =
            EditorLocalize.LocalizeDictionaryValues(new Dictionary<TraitsEnums.TraitsCategory, string>
            {
                {TraitsEnums.TraitsCategory.RESISTANCE, "WORD_1370"},
                {TraitsEnums.TraitsCategory.ABILITY_SCORE, "WORD_0085"},
                {TraitsEnums.TraitsCategory.ATTACK, "WORD_0158"},
                {TraitsEnums.TraitsCategory.SKILL, "WORD_0069"},
                {TraitsEnums.TraitsCategory.EQUIPMENT, "WORD_0070"},
                {TraitsEnums.TraitsCategory.OTHER, "WORD_0868"}
            });

        private readonly bool[,] _categoryEnabled =
        {
            //耐性
            {true, true, true, true, true, true},
            //能力値
            {true, true, true, true, true, true},
            //攻撃
            {true, true, true, true, true, true},
            //スキル
            {true, true, true, true, true, false},
            //装備
            {true, true, true, true, true, false},
            //装備
            {true, true, true, true, true, true}
        };

        //通常能力
        private readonly List<string> _commonAbility = EditorLocalize.LocalizeTexts(new List<string>
            {"WORD_0395", "WORD_0539", "WORD_0177", "WORD_0178", "WORD_0179", "WORD_0180", "WORD_0181", "WORD_0182"});

        //消滅エフェクト
        private readonly List<string> _disappearanceEffect =
            EditorLocalize.LocalizeTexts(new List<string> {"WORD_0548", "WORD_1436", "WORD_1437", "WORD_1438"});

        //装備タイプリスト
        private List<string> _equipArray = new List<string>();

        private readonly Dictionary<TraitsEquipmentEnum, string> _equipmentDictionary =
            EditorLocalize.LocalizeDictionaryValues(new Dictionary<TraitsEquipmentEnum, string>
            {
                {TraitsEquipmentEnum.ADD_WEAPON_TYPE, "WORD_1423"},
                {TraitsEquipmentEnum.ADD_ARMOR_TYPE, "WORD_1424"},
                {TraitsEquipmentEnum.FIXED_EQUIPMENT, "WORD_1425"},
                {TraitsEquipmentEnum.SEALED_EQUIPMENT, "WORD_1426"},
                {TraitsEquipmentEnum.SLOT_TYPE, "WORD_1427"}
            });

        private readonly bool[,] _equipmentEnabled =
        {
            //武器タイプ装備
            {true, false, true, true, true, false},
            //防具タイプ装備
            {true, false, true, true, true, false},
            //装備固定
            {true, true, true, true, true, false},
            //装備封印
            {true, true, true, true, true, false},
            //スロットタイプ
            {true, true, true, true, true, false}
        };

        private readonly Dictionary<TraitsOtherEnum, string> _otherDictionary =
            EditorLocalize.LocalizeDictionaryValues(new Dictionary<TraitsOtherEnum, string>
            {
                {TraitsOtherEnum.ADD_ACTION_COUNT, "WORD_1430"},
                {TraitsOtherEnum.SPECIAL_FLAG, "WORD_1431"},
                {TraitsOtherEnum.DISAPPEARANCE_EFFECT, "WORD_1435"},
                {TraitsOtherEnum.PARTY_ABILITY, "WORD_1439"}
            });

        private readonly bool[,] _otherEnabled =
        {
            //行動回数追加
            {true, true, true, true, true, true},
            //特殊フラグ
            {true, true, true, true, true, false},
            //消滅エフェクト
            {false, false, false, false, false, true},
            //パーティ能力
            {true, true, true, true, true, false}
        };

        //パーティ能力
        private readonly List<string> _partyAbility = EditorLocalize.LocalizeTexts(new List<string>
            {"WORD_1395", "WORD_1440", "WORD_1441", "WORD_1442", "WORD_1443", "WORD_1444"});

        private readonly Dictionary<TraitsEnums.TraitsResistance, string> _resistanceDictionary =
            EditorLocalize.LocalizeDictionaryValues(new Dictionary<TraitsEnums.TraitsResistance, string>
            {
                {TraitsEnums.TraitsResistance.ATTRIBUTE_REINFORCEMENT, "WORD_1371"},
                {TraitsEnums.TraitsResistance.ATTRIBUTE_WEAKEND, "WORD_1372"},
                {TraitsEnums.TraitsResistance.STATE_REINFORCEMENT, "WORD_1373"},
                {TraitsEnums.TraitsResistance.STATE_WEAKEND, "WORD_1397"}
            });

        private readonly bool[,] _resistanceEnabled =
        {
            //属性有効度
            {true, true, true, true, true, true},
            //弱体有効度
            {true, true, true, true, true, true},
            //ステート有効度
            {true, true, true, true, true, true},
            //ステート無効化
            {true, true, true, true, true, true}
        };

        //スキルリスト
        private List<string> _skillArray = new List<string>();

        private readonly Dictionary<TraitsSkillEnum, string> _skillDictionary =
            EditorLocalize.LocalizeDictionaryValues(new Dictionary<TraitsSkillEnum, string>
            {
                {TraitsSkillEnum.ADD_TYPE, "WORD_1419"},
                {TraitsSkillEnum.SEALED_TYPE, "WORD_1420"},
                {TraitsSkillEnum.ADD_SKILL, "WORD_1421"},
                {TraitsSkillEnum.SEALED_SKILL, "WORD_1422"}
            });

        private readonly bool[,] _skillEnabled =
        {
            //スキルタイプ追加
            {false, false, false, false, false, false},
            //スキルタイプ封印
            {true, true, true, true, true, false},
            //スキル追加
            {true, true, true, true, true, false},
            //スキル封印
            {true, true, true, true, true, false}
        };

        //スキルタイプリスト
        private List<string> _skillTypeArray = new List<string>();

        //スロットタイプ
        private readonly List<string> _slotType =
            EditorLocalize.LocalizeTexts(new List<string> {"WORD_0548", "WORD_1429"});

        //特殊能力
        private readonly List<string> _specialAbility = EditorLocalize.LocalizeTexts(new List<string>
        {
            "WORD_0405", "WORD_1405", "WORD_1406", "WORD_1407", "WORD_1408", "WORD_1409", "WORD_1410", "WORD_1411",
            "WORD_1412", "WORD_1413"
        });

        private readonly bool[,] _specialEnabled =
        {
            //狙われ率
            {true, false, true, true, true, false},
            //防御効果率
            {true, true, true, true, true, true},
            //回復効果率
            {true, true, true, true, true, true},
            //薬の知識
            {true, true, true, true, true, true},
            //MP消費率
            {true, true, true, true, true, false},
            //TPチャージ率
            {true, true, true, true, true, false},
            //物理ダメージ率
            {true, true, true, true, true, true},
            //魔法ダメージ率
            {true, true, true, true, true, true},
            //床ダメージ率
            {true, true, true, true, true, false},
            //経験値獲得率
            {true, true, true, true, true, false}
        };

        //特殊フラグ
        private readonly List<string> _specialFrag =
            EditorLocalize.LocalizeTexts(new List<string> {"WORD_1432", "WORD_0159", "WORD_0262", "WORD_1434"});

        //ステートリスト
        private List<string>               _stateNameArray = new List<string>();
        private List<TraitCommonDataModel> _trait;
        private Button                     _traitsAdd;

        private VisualElement _traitsArea;

        private TraitsType _traitsType;

        //武器タイプリスト
        private List<string> _weponTypeArray = new List<string>();

        //アーマーだったらtrueになる
        public bool isArmor = false;

        private readonly string mainUxml =
            "Assets/RPGMaker/Codebase/Editor/Inspector/Trait/Asset/traits.uxml";

        public void Init(List<TraitCommonDataModel> traits, TraitsType traitsType, CallBackTrait callBack) {
            //UI生成
            Clear();
            var Items = new VisualElement();
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(mainUxml);
            VisualElement labelFromUXML = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromUXML);
            labelFromUXML.style.flexGrow = 1;
            Items.Add(labelFromUXML);
            Add(Items);

            //引数保持
            _trait = traits;
            _traitsType = traitsType;
            _callBackTrait = callBack;

            //必要なデータの初期化
            var stateData = databaseManagementService.LoadStateEdit();
            _stateNameArray = new List<string>();
            for (var i = 0; i < stateData.Count; i++) _stateNameArray.Add(stateData[i].name);

            var skillTypeData = databaseManagementService.LoadSystem();
            _skillTypeArray = new List<string>();
            for (var i = 0; i < skillTypeData.skillTypes.Count; i++)
                _skillTypeArray.Add(skillTypeData.skillTypes[i].value);

            var skillData = databaseManagementService.LoadSkillCustom();
            _skillArray = new List<string>();
            for (var i = 0; i < skillData.Count; i++) _skillArray.Add(skillData[i].basic.name);

            var weponTypeData = databaseManagementService.LoadSystem();
            _weponTypeArray = new List<string>();
            for (var i = 0; i < weponTypeData.weaponTypes.Count; i++)
                _weponTypeArray.Add(weponTypeData.weaponTypes[i].value);

            var armorData = databaseManagementService.LoadSystem();
            _armorArray = new List<string>();
            for (var i = 0; i < armorData.armorTypes.Count; i++) _armorArray.Add(armorData.armorTypes[i].name);

            var equipData = databaseManagementService.LoadSystem();
            _equipArray = new List<string>();
            for (var i = 0; i < equipData.equipTypes.Count; i++) _equipArray.Add(equipData.equipTypes[i].name);

            _traitsArea = Items.Query<VisualElement>("traits_area");
            _traitsAdd = Items.Query<Button>("traits_add");
            _traitsAdd.AddToClassList("add_trait");
            for (var i = 0; i < _trait.Count; i++) SetCategory(i);

            //特徴の追加を押された時
            _traitsAdd.clickable.clicked += () =>
            {
                _trait = traits;
                _trait.Add(new TraitCommonDataModel(1, 1, 0, 100));
                _callBackTrait(_trait);

                SetCategory(_trait.Count - 1);
            };
        }

        //カテゴリー
        private void SetCategory(int index) {
            var traitsBlock = new VisualElement();
            var traitsBlock2 = new VisualElement();
            //削除ボタン専用
            var deleteBlock = new VisualElement();
            //カテゴリー
            var categoryBlock = new VisualElement();
            //項目
            var itemBlock = new VisualElement();
            //効果＋数値入力
            var effectBlock = new VisualElement();
            var valueRowBlock = new VisualElement();
            var valueBlock = new VisualElement();
            traitsBlock.AddToClassList("traits_list_area");
            traitsBlock2.AddToClassList("traits_list_area");
            categoryBlock.AddToClassList("traits_block_50_first");
            itemBlock.AddToClassList("traits_block_50_second");
            effectBlock.AddToClassList("traits_block_50_first");
            valueRowBlock.AddToClassList("traits_block_50_second");
            valueBlock.AddToClassList("traits_list_area");
            _traitsArea.Add(traitsBlock);
            _traitsArea.Add(traitsBlock2);
            traitsBlock.Add(categoryBlock);
            traitsBlock.Add(itemBlock);
            traitsBlock2.Add(effectBlock);
            traitsBlock2.Add(valueRowBlock);
            valueRowBlock.Add(valueBlock);
            _traitsArea.Add(deleteBlock);

            //カテゴリ情報
            var traits1DropdownChoicesWork = _categoryDictionary.Values.ToList();
            var traits1DropdownChoices = new List<string>();

            //カテゴリ情報を表示するかどうかを、特徴の設定対象から判断する
            var popupIndex = 0;
            for (var i = 0; i < traits1DropdownChoicesWork.Count; i++)
            {
                if (_categoryEnabled[i, (int) _traitsType]) traits1DropdownChoices.Add(traits1DropdownChoicesWork[i]);

                if (_trait[index].categoryId - 1 == i) popupIndex = traits1DropdownChoices.Count() - 1;
            }

            //PU生成
            var traits1DropdownPopupField = new PopupFieldBase<string>(traits1DropdownChoices, popupIndex);
            categoryBlock.Add(traits1DropdownPopupField);
            traits1DropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                var pair = _categoryDictionary.FirstOrDefault(c => c.Value == traits1DropdownPopupField.value);
                var key = pair.Key;
                _trait[index].categoryId = (int) key;
                //表示しない項目があるため、初期値を変更する
                
                //スキル
                if (key == TraitsEnums.TraitsCategory.SKILL)
                {
                    //スキルタイプ追加がないため、スキルタイプ封印を初期値
                    _trait[index].traitsId = (int) TraitsSkillEnum.SEALED_TYPE;
                }
                //能力値
                else if (key == TraitsEnums.TraitsCategory.ABILITY_SCORE)
                {
                    //敵
                    if (_traitsType == TraitsType.TRAITS_TYPE_ENEMY)
                    {
                        //通常能力値がないため、追加能力値を初期値に
                        _trait[index].traitsId = (int) TraitsAbilityScore.ADD_ABILITY_SCORE; 
                        _trait[index].value = 0;
                    }
                    //武器
                    else if (_traitsType == TraitsType.TRAITS_TYPE_WEAPON)
                    {
                        //通常能力値がないため、追加能力値を初期値に
                        _trait[index].traitsId = (int) TraitsAbilityScore.ADD_ABILITY_SCORE; 
                        _trait[index].value = 0;
                    }
                    //防具
                    else if (_traitsType == TraitsType.TRAITS_TYPE_ARMOR)
                    {
                        //通常能力値がないため、追加能力値を初期値に
                        _trait[index].traitsId = (int) TraitsAbilityScore.ADD_ABILITY_SCORE; 
                        _trait[index].value = 0;
                    }
                }
                //装備
                else if (key == TraitsEnums.TraitsCategory.EQUIPMENT)
                {
                    //職業
                    if (_traitsType == TraitsType.TRAITS_TYPE_CLASS)
                    {
                        //武器タイプ装備、防具タイプ装備がないため、装備固定を初期値
                        _trait[index].traitsId = (int) TraitsEquipmentEnum.FIXED_EQUIPMENT;
                        _trait[index].value = 1000;
                    }
                }
                else
                {
                    _trait[index].traitsId = 1;
                    _trait[index].value = 1000;
                }
                _trait[index].effectId = 0;
                SetTraits(index, traitsBlock, deleteBlock, categoryBlock, itemBlock, effectBlock, valueBlock);
                SetEffects(index, traitsBlock, deleteBlock, categoryBlock, itemBlock, effectBlock, valueBlock);
                _callBackTrait(_trait);
            });
            SetTraits(index, traitsBlock, deleteBlock, categoryBlock, itemBlock, effectBlock, valueBlock);
        }

        //項目
        private void SetTraits(
            int index,
            VisualElement traitsBlock,
            VisualElement deleteBlock,
            VisualElement categoryBlock,
            VisualElement itemBlock,
            VisualElement effectBlock,
            VisualElement valueBlock
        ) {
            itemBlock.Clear();
            itemBlock.AddToClassList("traits_block_50_second");


            var effectDropdownChoicesWork = _resistanceDictionary.Values.ToList();
            var effectDropdownChoices = new List<string>();
            var popupIndex = 1;
            if (_trait[index].categoryId == (int) TraitsEnums.TraitsCategory.RESISTANCE)
            {
                //情報を表示するかどうかを、特徴の設定対象から判断する
                effectDropdownChoicesWork = _resistanceDictionary.Values.ToList();
                for (var i = 0; i < effectDropdownChoicesWork.Count; i++)
                {
                    if (_resistanceEnabled[i, (int) _traitsType])
                        effectDropdownChoices.Add(effectDropdownChoicesWork[i]);

                    if (_trait[index].traitsId - 1 == i) popupIndex = effectDropdownChoices.Count();
                }
            }
            else if (_trait[index].categoryId == (int) TraitsEnums.TraitsCategory.ABILITY_SCORE)
            {
                effectDropdownChoicesWork = _abilityScoreDictionary.Values.ToList();
                for (var i = 0; i < effectDropdownChoicesWork.Count; i++)
                {
                    if (_abilityScoreEnabled[i, (int) _traitsType])
                        effectDropdownChoices.Add(effectDropdownChoicesWork[i]);

                    if (_trait[index].traitsId - 1 == i) popupIndex = effectDropdownChoices.Count();
                }
            }
            else if (_trait[index].categoryId == (int) TraitsEnums.TraitsCategory.ATTACK)
            {
                effectDropdownChoicesWork = _attackDictionary.Values.ToList();
                for (var i = 0; i < effectDropdownChoicesWork.Count; i++)
                {
                    if (_attackEnabled[i, (int) _traitsType]) effectDropdownChoices.Add(effectDropdownChoicesWork[i]);

                    if (_trait[index].traitsId - 1 == i) popupIndex = effectDropdownChoices.Count();
                }
            }
            else if (_trait[index].categoryId == (int) TraitsEnums.TraitsCategory.SKILL)
            {
                effectDropdownChoicesWork = _skillDictionary.Values.ToList();
                for (var i = 0; i < effectDropdownChoicesWork.Count; i++)
                {
                    if (_skillEnabled[i, (int) _traitsType]) effectDropdownChoices.Add(effectDropdownChoicesWork[i]);

                    if (_trait[index].traitsId - 1 == i) popupIndex = effectDropdownChoices.Count();
                }
            }
            else if (_trait[index].categoryId == (int) TraitsEnums.TraitsCategory.EQUIPMENT)
            {
                effectDropdownChoicesWork = _equipmentDictionary.Values.ToList();
                for (var i = 0; i < effectDropdownChoicesWork.Count; i++)
                {
                    if (_equipmentEnabled[i, (int) _traitsType])
                        effectDropdownChoices.Add(effectDropdownChoicesWork[i]);

                    if (_trait[index].traitsId - 1 == i) popupIndex = effectDropdownChoices.Count();
                }
            }
            else if (_trait[index].categoryId == (int) TraitsEnums.TraitsCategory.OTHER)
            {
                effectDropdownChoicesWork = _otherDictionary.Values.ToList();
                for (var i = 0; i < effectDropdownChoicesWork.Count; i++)
                {
                    if (_otherEnabled[i, (int) _traitsType]) effectDropdownChoices.Add(effectDropdownChoicesWork[i]);

                    if (_trait[index].traitsId - 1 == i) popupIndex = effectDropdownChoices.Count();
                }
            }

            if (popupIndex < 1) popupIndex = 1;

            //PU作成
            var effectDropdownPopupField = new PopupFieldBase<string>(effectDropdownChoices, popupIndex - 1);
            itemBlock.Add(effectDropdownPopupField);
            effectDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                _trait[index].effectId = 0;
                
                if (_trait[index].categoryId == (int) TraitsEnums.TraitsCategory.RESISTANCE)
                {
                    var pair = _resistanceDictionary.FirstOrDefault(c => c.Value == effectDropdownPopupField.value);
                    var key = pair.Key;
                    _trait[index].traitsId = (int) key;
                }
                else if (_trait[index].categoryId == (int) TraitsEnums.TraitsCategory.ABILITY_SCORE)
                {
                    var pair = _abilityScoreDictionary.FirstOrDefault(c => c.Value == effectDropdownPopupField.value);
                    var key = pair.Key;
                    _trait[index].traitsId = (int) key;
                    _trait[index].value = 1000;
                    if (_trait[index].traitsId == 2)
                        _trait[index].value = 0;
                    
                    if (_traitsType == TraitsType.TRAITS_TYPE_ENEMY || _traitsType == TraitsType.TRAITS_TYPE_CLASS)
                    {
                        //敵キャラ、職業の場合「狙われ率」がないため、初期値を「防御効果率」に変更
                        _trait[index].effectId = 1;
                    }
                }
                else if (_trait[index].categoryId == (int) TraitsEnums.TraitsCategory.ATTACK)
                {
                    var pair = _attackDictionary.FirstOrDefault(c => c.Value == effectDropdownPopupField.value);
                    var key = pair.Key;
                    _trait[index].traitsId = (int) key;
                    _trait[index].value = 1000;
                    if (_trait[index].traitsId == 3 || _trait[index].traitsId == 4)
                        _trait[index].value = 0;
                }
                else if (_trait[index].categoryId == (int) TraitsEnums.TraitsCategory.SKILL)
                {
                    var pair = _skillDictionary.FirstOrDefault(c => c.Value == effectDropdownPopupField.value);
                    var key = pair.Key;
                    _trait[index].traitsId = (int) key;
                }
                else if (_trait[index].categoryId == (int) TraitsEnums.TraitsCategory.EQUIPMENT)
                {
                    var pair = _equipmentDictionary.FirstOrDefault(c => c.Value == effectDropdownPopupField.value);
                    var key = pair.Key;
                    _trait[index].traitsId = (int) key;
                }
                else if (_trait[index].categoryId == (int) TraitsEnums.TraitsCategory.OTHER)
                {
                    var pair = _otherDictionary.FirstOrDefault(c => c.Value == effectDropdownPopupField.value);
                    var key = pair.Key;
                    _trait[index].traitsId = (int) key;
                    _trait[index].value = 0;
                }

                SetEffects(index, traitsBlock, deleteBlock, categoryBlock, itemBlock, effectBlock, valueBlock);
                _callBackTrait(_trait);
            });
            SetEffects(index, traitsBlock, deleteBlock, categoryBlock, itemBlock, effectBlock, valueBlock);
        }

        //効果と値
        private void SetEffects(
            int index,
            VisualElement traitsBlock,
            VisualElement deleteBlock,
            VisualElement categoryBlock,
            VisualElement itemBlock,
            VisualElement effectBlock,
            VisualElement valueBlock
        ) {
            effectBlock.Clear();
            effectBlock.AddToClassList("traits_block_50_second");
            valueBlock.Clear();
            valueBlock.AddToClassList("traits_list_area");

            IntegerField traitInput = new IntegerField();
            FloatField floatField = new FloatField();

            //初期値の代入
            traitInput.value = _trait[index].value;

            //耐性
            if (_trait[index].categoryId == (int) TraitsEnums.TraitsCategory.RESISTANCE)
            {
                //属性有効度
                if (_trait[index].traitsId == (int) TraitsEnums.TraitsResistance.ATTRIBUTE_REINFORCEMENT)
                {
                    //仮の属性を今の属性へ変更を実施
                    _attribute.Clear();
                    foreach (var a in databaseManagementService.LoadSystem().elements)
                        _attribute.Add(EditorLocalize.LocalizeText(a.value));

                    var traits4DropdownChoices = _attribute;
                    var traits4DropdownPopupField =
                        new PopupFieldBase<string>(traits4DropdownChoices, _trait[index].effectId);
                    effectBlock.Add(traits4DropdownPopupField);
                    traits4DropdownPopupField.RegisterValueChangedCallback(evt =>
                    {
                        _trait[index].effectId = traits4DropdownPopupField.index;
                        _callBackTrait(_trait);
                    });
                    Label label = new Label("*");
                    valueBlock.Add(label);
                    floatField.value = _trait[index].value / 10f;
                    floatField.label = "%";
                    floatField.style.flexDirection = FlexDirection.RowReverse;
                    valueBlock.Add(floatField);
                    BaseInputFieldHandler.FloatFieldCallback(floatField, evt =>
                    {
                        _trait[index].value = (int)(floatField.value * 10);
                        _callBackTrait(_trait);
                    }, 0.0f, 1000.0f);
                }

                //弱体有効度
                if (_trait[index].traitsId == (int) TraitsEnums.TraitsResistance.ATTRIBUTE_WEAKEND)
                {
                    var traits4DropdownChoices = _commonAbility;
                    var traits4DropdownPopupField =
                        new PopupFieldBase<string>(traits4DropdownChoices, _trait[index].effectId);
                    effectBlock.Add(traits4DropdownPopupField);
                    traits4DropdownPopupField.RegisterValueChangedCallback(evt =>
                    {
                        _trait[index].effectId = traits4DropdownPopupField.index;
                        _callBackTrait(_trait);
                    });
                    Label label = new Label("*");
                    valueBlock.Add(label);
                    floatField.value = _trait[index].value / 10f;
                    floatField.label = "%";
                    floatField.style.flexDirection = FlexDirection.RowReverse;
                    valueBlock.Add(floatField);
                    BaseInputFieldHandler.FloatFieldCallback(floatField, evt =>
                    {
                        _trait[index].value = (int)(floatField.value * 10);
                        _callBackTrait(_trait);
                    }, 0.0f, 1000.0f);
                }

                //ステート有効度
                if (_trait[index].traitsId == (int) TraitsEnums.TraitsResistance.STATE_REINFORCEMENT)
                {
                    var traits4DropdownChoices = _stateNameArray;
                    var traits4DropdownPopupField =
                        new PopupFieldBase<string>(traits4DropdownChoices, _trait[index].effectId);
                   effectBlock.Add(traits4DropdownPopupField);
                    traits4DropdownPopupField.RegisterValueChangedCallback(evt =>
                    {
                        _trait[index].effectId = traits4DropdownPopupField.index;
                        _callBackTrait(_trait);
                    });
                    Label label = new Label("*");
                    valueBlock.Add(label);
                    floatField.value = _trait[index].value / 10f;
                    floatField.label = "%";
                    floatField.style.flexDirection = FlexDirection.RowReverse;
                    valueBlock.Add(floatField);
                    BaseInputFieldHandler.FloatFieldCallback(floatField, evt =>
                    {
                        _trait[index].value = (int) (floatField.value * 10);
                        _callBackTrait(_trait);
                    }, 0.0f, 1000.0f);
                }

                //ステート無効化
                if (_trait[index].traitsId == (int) TraitsEnums.TraitsResistance.STATE_WEAKEND)
                {
                    var traits4DropdownChoices = _stateNameArray;
                    var traits4DropdownPopupField =
                        new PopupFieldBase<string>(traits4DropdownChoices, _trait[index].effectId);
                    effectBlock.Add(traits4DropdownPopupField);
                    traits4DropdownPopupField.RegisterValueChangedCallback(evt =>
                    {
                        _trait[index].effectId = traits4DropdownPopupField.index;
                        _callBackTrait(_trait);
                    });
                }

                //能力値
            }
            else if (_trait[index].categoryId == (int) TraitsEnums.TraitsCategory.ABILITY_SCORE)
            {
                //通常能力値
                if (_trait[index].traitsId == (int) TraitsAbilityScore.NORMAL_ABILITY_SCORE)
                {
                    var traits4DropdownChoices = _commonAbility;
                    var traits4DropdownPopupField =
                        new PopupFieldBase<string>(traits4DropdownChoices, _trait[index].effectId);
                    effectBlock.Add(traits4DropdownPopupField);
                    traits4DropdownPopupField.RegisterValueChangedCallback(evt =>
                    {
                        _trait[index].effectId = traits4DropdownPopupField.index;
                        _callBackTrait(_trait);
                    });
                    Label label = new Label("*");
                    valueBlock.Add(label);
                    floatField.value = _trait[index].value / 10f;
                    floatField.label = "%";
                    floatField.style.flexDirection = FlexDirection.RowReverse;
                    valueBlock.Add(floatField);
                    BaseInputFieldHandler.FloatFieldCallback(floatField, evt =>
                    {
                        _trait[index].value = (int)(floatField.value * 10);
                        _callBackTrait(_trait);
                    }, 0.0f, 1000.0f);
                }

                //追加能力値
                if (_trait[index].traitsId == (int) TraitsAbilityScore.ADD_ABILITY_SCORE)
                {
                    var traits4DropdownChoicesWork = _additionAbility;
                    var traits4DropdownChoices = new List<string>();
                    //カテゴリ情報を表示するかどうかを、特徴の設定対象から判断する
                    var popupIndex = 0;
                    for (var i = 0; i < traits4DropdownChoicesWork.Count; i++)
                    {
                        if (_additionEnabled[i, (int) _traitsType])
                            traits4DropdownChoices.Add(traits4DropdownChoicesWork[i]);

                        if (_trait[index].effectId == i) popupIndex = traits4DropdownChoices.Count() - 1;
                    }

                    if (popupIndex < 0) popupIndex = 0;

                    var traits4DropdownPopupField = new PopupFieldBase<string>(traits4DropdownChoices, popupIndex);
                    effectBlock.Add(traits4DropdownPopupField);
                    traits4DropdownPopupField.RegisterValueChangedCallback(evt =>
                    {
                        var pair = _additionAbility.IndexOf(traits4DropdownPopupField.value);
                        _trait[index].effectId = pair;
                        _callBackTrait(_trait);
                    });
                    Label label = new Label("+");
                    valueBlock.Add(label);
                    floatField.value = _trait[index].value / 10f;
                    floatField.label = "%";
                    floatField.style.flexDirection = FlexDirection.RowReverse;
                    valueBlock.Add(floatField);
                    BaseInputFieldHandler.FloatFieldCallback(floatField, evt =>
                    {
                        _trait[index].value = (int) (floatField.value * 10);
                        _callBackTrait(_trait);
                    }, -1000.0f, 1000.0f);
                }

                //特殊能力値
                if (_trait[index].traitsId == (int) TraitsAbilityScore.SPECIAL_ABILITY_SCORE)
                {
                    var traits4DropdownChoicesWork = _specialAbility;
                    var traits4DropdownChoices = new List<string>();
                    //カテゴリ情報を表示するかどうかを、特徴の設定対象から判断する
                    var popupIndex = 0;
                    for (var i = 0; i < traits4DropdownChoicesWork.Count; i++)
                    {
                        if (_specialEnabled[i, (int) _traitsType])
                            traits4DropdownChoices.Add(traits4DropdownChoicesWork[i]);

                        if (_trait[index].effectId == i) popupIndex = traits4DropdownChoices.Count() - 1;
                    }

                    if (popupIndex < 0) popupIndex = 0;

                    var traits4DropdownPopupField = new PopupFieldBase<string>(traits4DropdownChoices, popupIndex);
                    effectBlock.Add(traits4DropdownPopupField);
                    traits4DropdownPopupField.RegisterValueChangedCallback(evt =>
                    {
                        var pair = _specialAbility.IndexOf(traits4DropdownPopupField.value);
                        _trait[index].effectId = pair;
                        _callBackTrait(_trait);
                    });
                    Label label = new Label("*");
                    valueBlock.Add(label);
                    floatField.value = _trait[index].value / 10f;
                    floatField.label = "%";
                    floatField.style.flexDirection = FlexDirection.RowReverse;
                    valueBlock.Add(floatField);
                    BaseInputFieldHandler.FloatFieldCallback(floatField, evt =>
                    {
                        _trait[index].value = (int)(floatField.value * 10);
                        _callBackTrait(_trait);
                    }, 0.0f, 1000.0f);
                }

                //攻撃
            }
            else if (_trait[index].categoryId == (int) TraitsEnums.TraitsCategory.ATTACK)
            {
                //攻撃時属性
                if (_trait[index].traitsId == (int) TraitsAttack.ATTACK_ATTRIBUTE)
                {
                    //仮の属性を今の属性へ変更を実施
                    _attribute.Clear();
                    foreach (var a in databaseManagementService.LoadSystem().elements)
                        _attribute.Add(EditorLocalize.LocalizeText(a.value));

                    var traits4DropdownChoices = _attribute;
                    var traits4DropdownPopupField =
                        new PopupFieldBase<string>(traits4DropdownChoices, _trait[index].effectId);
                    effectBlock.Add(traits4DropdownPopupField);
                    traits4DropdownPopupField.RegisterValueChangedCallback(evt =>
                    {
                        _trait[index].effectId = traits4DropdownPopupField.index;
                        _callBackTrait(_trait);
                    });
                }

                //攻撃時ステート
                if (_trait[index].traitsId == (int) TraitsAttack.ATTACK_STATE)
                {
                    var traits4DropdownChoices = _stateNameArray;
                    var traits4DropdownPopupField =
                        new PopupFieldBase<string>(traits4DropdownChoices, _trait[index].effectId);
                    effectBlock.Add(traits4DropdownPopupField);
                    traits4DropdownPopupField.RegisterValueChangedCallback(evt =>
                    {
                        _trait[index].effectId = traits4DropdownPopupField.index;
                        _callBackTrait(_trait);
                    });
                    Label label = new Label("+");
                    valueBlock.Add(label);
                    floatField.value = _trait[index].value / 10f;
                    floatField.label = "%";
                    floatField.style.flexDirection = FlexDirection.RowReverse;
                    valueBlock.Add(floatField);
                    BaseInputFieldHandler.FloatFieldCallback(floatField, evt =>
                    {
                        _trait[index].value = (int)(floatField.value * 10);
                        _callBackTrait(_trait);
                    }, 0.0f, 1000.0f);
                }

                //攻撃速度補正
                if (_trait[index].traitsId == (int) TraitsAttack.ATTACK_SPEED_CORRECTION)
                {
                    floatField.value = _trait[index].value / 10f;
                    effectBlock.Add(floatField);
                    BaseInputFieldHandler.FloatFieldCallback(floatField, evt =>
                    {
                        _trait[index].value = (int)(floatField.value * 10);
                        _callBackTrait(_trait);
                    }, 0.0f, 1000.0f);
                }

                //攻撃追加回数
                if (_trait[index].traitsId == (int) TraitsAttack.ATTACK_ADD_COUNT)
                {
                    effectBlock.Add(traitInput);
                    BaseInputFieldHandler.IntegerFieldCallback(traitInput, evt =>
                    {
                        _trait[index].value = traitInput.value;
                        _callBackTrait(_trait);
                    }, -9, 9);
                }

                //攻撃スキル
                if (_trait[index].traitsId == (int) TraitsAttack.ATTACK_SKILL)
                {
                    var traits4DropdownChoices = _skillArray;
                    var traits4DropdownPopupField =
                        new PopupFieldBase<string>(traits4DropdownChoices, _trait[index].effectId);
                    effectBlock.Add(traits4DropdownPopupField);
                    traits4DropdownPopupField.RegisterValueChangedCallback(evt =>
                    {
                        _trait[index].effectId = traits4DropdownPopupField.index;
                        _callBackTrait(_trait);
                    });
                }
            }
            else if (_trait[index].categoryId == (int) TraitsEnums.TraitsCategory.SKILL)
            {
                //スキルタイプ追加
                if (_trait[index].traitsId == (int) TraitsSkillEnum.ADD_TYPE)
                {
                    var traits4DropdownChoices = _skillTypeArray;
                    var traits4DropdownPopupField =
                        new PopupFieldBase<string>(traits4DropdownChoices, _trait[index].effectId);
                    effectBlock.Add(traits4DropdownPopupField);
                    traits4DropdownPopupField.RegisterValueChangedCallback(evt =>
                    {
                        _trait[index].effectId = traits4DropdownPopupField.index;
                        _callBackTrait(_trait);
                    });
                }

                //スキルタイプ封印
                if (_trait[index].traitsId == (int) TraitsSkillEnum.SEALED_TYPE)
                {
                    var traits4DropdownChoices = _skillTypeArray;
                    var traits4DropdownPopupField =
                        new PopupFieldBase<string>(traits4DropdownChoices, _trait[index].effectId);
                    effectBlock.Add(traits4DropdownPopupField);
                    traits4DropdownPopupField.RegisterValueChangedCallback(evt =>
                    {
                        _trait[index].effectId = traits4DropdownPopupField.index;
                        _callBackTrait(_trait);
                    });
                }

                //スキル追加
                if (_trait[index].traitsId == (int) TraitsSkillEnum.ADD_SKILL)
                {
                    var traits4DropdownChoices = _skillArray;
                    var traits4DropdownPopupField =
                        new PopupFieldBase<string>(traits4DropdownChoices, _trait[index].effectId);
                    effectBlock.Add(traits4DropdownPopupField);
                    traits4DropdownPopupField.RegisterValueChangedCallback(evt =>
                    {
                        _trait[index].effectId = traits4DropdownPopupField.index;
                        _callBackTrait(_trait);
                    });
                }

                //スキル封印
                if (_trait[index].traitsId == (int) TraitsSkillEnum.SEALED_SKILL)
                {
                    var traits4DropdownChoices = _skillArray;
                    var traits4DropdownPopupField =
                        new PopupFieldBase<string>(traits4DropdownChoices, _trait[index].effectId);
                    effectBlock.Add(traits4DropdownPopupField);
                    traits4DropdownPopupField.RegisterValueChangedCallback(evt =>
                    {
                        _trait[index].effectId = traits4DropdownPopupField.index;
                        _callBackTrait(_trait);
                    });
                }

                //装備
            }
            else if (_trait[index].categoryId == (int) TraitsEnums.TraitsCategory.EQUIPMENT)
            {
                //武器タイプ装備
                if (_trait[index].traitsId == (int) TraitsEquipmentEnum.ADD_WEAPON_TYPE)
                {
                    var traits4DropdownChoices = _weponTypeArray;
                    var traits4DropdownPopupField =
                        new PopupFieldBase<string>(traits4DropdownChoices, _trait[index].effectId);
                    effectBlock.Add(traits4DropdownPopupField);
                    traits4DropdownPopupField.RegisterValueChangedCallback(evt =>
                    {
                        _trait[index].effectId = traits4DropdownPopupField.index;
                        _callBackTrait(_trait);
                    });
                }

                //防具タイプ装備
                if (_trait[index].traitsId == (int) TraitsEquipmentEnum.ADD_ARMOR_TYPE)
                {
                    var traits4DropdownChoices = _armorArray;
                    var traits4DropdownPopupField =
                        new PopupFieldBase<string>(traits4DropdownChoices, _trait[index].effectId);
                    effectBlock.Add(traits4DropdownPopupField);
                    traits4DropdownPopupField.RegisterValueChangedCallback(evt =>
                    {
                        _trait[index].effectId = traits4DropdownPopupField.index;
                        _callBackTrait(_trait);
                    });
                }

                //装備固定
                if (_trait[index].traitsId == (int) TraitsEquipmentEnum.FIXED_EQUIPMENT)
                {
                    var traits4DropdownChoices = _equipArray;
                    var traits4DropdownPopupField =
                        new PopupFieldBase<string>(traits4DropdownChoices, _trait[index].effectId);
                    effectBlock.Add(traits4DropdownPopupField);
                    traits4DropdownPopupField.RegisterValueChangedCallback(evt =>
                    {
                        _trait[index].effectId = traits4DropdownPopupField.index;
                        _callBackTrait(_trait);
                    });
                }

                //装備封印
                if (_trait[index].traitsId == (int) TraitsEquipmentEnum.SEALED_EQUIPMENT)
                {
                    var traits4DropdownChoices = _equipArray;
                    var traits4DropdownPopupField =
                        new PopupFieldBase<string>(traits4DropdownChoices, _trait[index].effectId);
                    effectBlock.Add(traits4DropdownPopupField);
                    traits4DropdownPopupField.RegisterValueChangedCallback(evt =>
                    {
                        _trait[index].effectId = traits4DropdownPopupField.index;
                        _callBackTrait(_trait);
                    });
                }

                //スロットタイプ
                if (_trait[index].traitsId == (int) TraitsEquipmentEnum.SLOT_TYPE)
                {
                    var traits4DropdownChoices = _slotType;
                    var traits4DropdownPopupField =
                        new PopupFieldBase<string>(traits4DropdownChoices, _trait[index].effectId);
                    effectBlock.Add(traits4DropdownPopupField);
                    traits4DropdownPopupField.RegisterValueChangedCallback(evt =>
                    {
                        _trait[index].effectId = traits4DropdownPopupField.index;
                        _callBackTrait(_trait);
                    });
                }

                //その他
            }
            else if (_trait[index].categoryId == (int) TraitsEnums.TraitsCategory.OTHER)
            {
                //行動回数追加
                if (_trait[index].traitsId == (int) TraitsOtherEnum.ADD_ACTION_COUNT)
                {
                    effectBlock.Add(traitInput);
                    BaseInputFieldHandler.IntegerFieldCallback(traitInput, evt =>
                    {
                        _trait[index].value = traitInput.value;
                        _callBackTrait(_trait);
                    }, 0, 100);
                }

                //特殊フラグ
                if (_trait[index].traitsId == (int) TraitsOtherEnum.SPECIAL_FLAG)
                {
                    var traits4DropdownChoices = _specialFrag;
                    var traits4DropdownPopupField =
                        new PopupFieldBase<string>(traits4DropdownChoices, _trait[index].effectId);
                    effectBlock.Add(traits4DropdownPopupField);
                    traits4DropdownPopupField.RegisterValueChangedCallback(evt =>
                    {
                        _trait[index].effectId = traits4DropdownPopupField.index;
                        _callBackTrait(_trait);
                    });
                }

                //消滅エフェクト
                if (_trait[index].traitsId == (int) TraitsOtherEnum.DISAPPEARANCE_EFFECT)
                {
                    var traits4DropdownChoices = _disappearanceEffect;
                    var traits4DropdownPopupField =
                        new PopupFieldBase<string>(traits4DropdownChoices, _trait[index].effectId);
                    effectBlock.Add(traits4DropdownPopupField);
                    traits4DropdownPopupField.RegisterValueChangedCallback(evt =>
                    {
                        _trait[index].effectId = traits4DropdownPopupField.index;
                        _callBackTrait(_trait);
                    });
                }

                //パーティ能力
                if (_trait[index].traitsId == (int) TraitsOtherEnum.PARTY_ABILITY)
                {
                    var traits4DropdownChoices = _partyAbility;
                    var traits4DropdownPopupField =
                        new PopupFieldBase<string>(traits4DropdownChoices, _trait[index].effectId);
                    effectBlock.Add(traits4DropdownPopupField);
                    traits4DropdownPopupField.RegisterValueChangedCallback(evt =>
                    {
                        _trait[index].effectId = traits4DropdownPopupField.index;
                        _callBackTrait(_trait);
                    });
                }
            }

            //削除ボタン
            deleteBlock.Clear();
            var deleteBtn = new Button();
            deleteBtn.AddToClassList("small");
            deleteBtn.Add(new Label(EditorLocalize.LocalizeText("WORD_0383")));
            deleteBlock.Add(deleteBtn);
            deleteBtn.clickable.clicked += () =>
            {
                _trait.RemoveAt(index);
                _callBackTrait(_trait);
                Init(_trait, _traitsType, _callBackTrait);
            };

            //防具だった場合0番目に削除ボタンは存在しない
            if (isArmor && index == 0) deleteBtn.style.display = DisplayStyle.None;
        }
    }
}