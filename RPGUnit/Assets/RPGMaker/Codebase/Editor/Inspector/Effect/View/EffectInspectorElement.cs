using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.Effect.View
{
    /// <summary>
    /// Inspectorで利用する[使用効果]用のクラス
    /// </summary>
    public class EffectInspectorElement : AbstractInspectorElement
    {
        public delegate void CallBackTrait(object data);

        /// <summary>
        ///     画面によって出す項目を制限する
        /// </summary>
        public enum DisplayType
        {
            None,
            Item,
            skill
        }

        private CallBackTrait _callBackTrait;


        private readonly Dictionary<EffectCategoryEnum, string> _categoryDictionary =
            EditorLocalize.LocalizeDictionaryValues(new Dictionary<EffectCategoryEnum, string>
            {
                {EffectCategoryEnum.STATE, "WORD_0602"},
                {EffectCategoryEnum.PARAMS, "WORD_0085"},
                {EffectCategoryEnum.OTHER, "WORD_0868"}
            });

        private DisplayType _displayType = DisplayType.None;

        private readonly List<string> _escapeArray = EditorLocalize.LocalizeTexts(new List<string>
        {
            "WORD_0157"
        });

        //アイテムの場合
        private readonly Dictionary<EffectOtherEnum, string> _otherDictionaryByItem =
            EditorLocalize.LocalizeDictionaryValues(new Dictionary<EffectOtherEnum, string>
            {
                {EffectOtherEnum.SPECIAL_EFFECT, "WORD_4010"},
                {EffectOtherEnum.ADD_PARAMETER, "WORD_1452"},
                {EffectOtherEnum.MASTER_SKILL, "WORD_0221"},
                {EffectOtherEnum.COMMON_EVENT, "WORD_0506"}
            });

        //スキルの場合
        private readonly Dictionary<EffectOtherEnum, string> _otherDictionaryBySkill =
            EditorLocalize.LocalizeDictionaryValues(new Dictionary<EffectOtherEnum, string>
            {
                {EffectOtherEnum.SPECIAL_EFFECT, "WORD_4010"},
                {EffectOtherEnum.COMMON_EVENT, "WORD_0506"}
            });

        private readonly Dictionary<EffectParameterEnum, string> _paramDictionary =
            EditorLocalize.LocalizeDictionaryValues(new Dictionary<EffectParameterEnum, string>
            {
                {EffectParameterEnum.ADD_BUFF, "WORD_0264"},
                {EffectParameterEnum.ADD_DEBUFF, "WORD_1448"},
                {EffectParameterEnum.DELETE_BUFF, "WORD_1449"},
                {EffectParameterEnum.DELETE_DEBUFF, "WORD_1450"}
            });

        private readonly List<string> _paramNameArray = EditorLocalize.LocalizeTexts(new List<string>
        {
            "WORD_0395", "WORD_0539", "WORD_0177", "WORD_0178", "WORD_0179", "WORD_0180", "WORD_0181", "WORD_0182"
        });

        private readonly List<string> _skillNameArray = new List<string>();

        private readonly Dictionary<EffectStateEnum, string> _stateDictionary =
            EditorLocalize.LocalizeDictionaryValues(new Dictionary<EffectStateEnum, string>
            {
                {EffectStateEnum.ADD_STATE, "WORD_1445"},
                {EffectStateEnum.DELETE_STATE, "WORD_1446"}
            });

        private readonly List<string>               _stateNameArray = new List<string>();
        private          List<TraitCommonDataModel> _trait;
        private          Button                     _traitsAdd;

        private VisualElement _traitsArea;

        private readonly string mainUxml =
            "Assets/RPGMaker/Codebase/Editor/Inspector/Effect/Asset/effect.uxml";


        public void Init(
            List<TraitCommonDataModel> trait,
            CallBackTrait callBack,
            DisplayType displayType
        ) {
            _trait = trait;
            _callBackTrait = callBack;
            _displayType = displayType;
            Clear();

            var Items = new VisualElement();
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(mainUxml);
            VisualElement labelFromUXML = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromUXML);
            labelFromUXML.style.flexGrow = 1;
            Items.Add(labelFromUXML);
            Add(Items);

            var stateDataModels = databaseManagementService.LoadStateEdit();
            _stateNameArray.Clear();
            for (var i = 0; i < stateDataModels.Count; i++) _stateNameArray.Add(stateDataModels[i].name);

            var skillCustomDataModels = databaseManagementService.LoadSkillCustom();
            _skillNameArray.Clear();
            for (var i = 0; i < skillCustomDataModels.Count; i++)
                _skillNameArray.Add(skillCustomDataModels[i].basic.name);

            _traitsArea = Items.Query<VisualElement>("traits_area");
            _traitsAdd = Items.Query<Button>("traits_add");
            _traitsAdd.AddToClassList("add_trait");
            for (var i = 0; i < _trait.Count; i++) SetCategory(i);

            //特徴の追加を押された時
            _traitsAdd.clickable.clicked += () =>
            {
                _trait = trait;
                _trait.Add(new TraitCommonDataModel(1, 1, 0, 0));
                _callBackTrait(_trait);

                SetCategory(_trait.Count - 1);
            };
        }

        /// <summary>
        ///     カテゴリー
        /// </summary>
        /// <param name="Items"></param>
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
            
            
            var effectCategoryDropdownChoices = _categoryDictionary.Values.ToList();
            var effectCategoryDropdownPopupField =
                new PopupFieldBase<string>(effectCategoryDropdownChoices, _trait[index].categoryId - 1);
            categoryBlock.Add(effectCategoryDropdownPopupField);
            effectCategoryDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                var key = EffectCategoryEnum.OTHER;
                var pair = _categoryDictionary.FirstOrDefault(c => c.Value == effectCategoryDropdownPopupField.value);
                key = pair.Key;

                _trait[index].categoryId = (int) key;
                _trait[index].traitsId = 1;
                _trait[index].effectId = 0;
                _trait[index].value = 0;
                SetTraits(index, traitsBlock, deleteBlock, categoryBlock, itemBlock, effectBlock,valueBlock);
                _callBackTrait(_trait);
            });
            SetTraits(index, traitsBlock, deleteBlock, categoryBlock, itemBlock, effectBlock, valueBlock);
        }

        /// <summary>
        ///     項目
        /// </summary>
        /// <param name="Items"></param>
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
            var effectDropdownChoices = _stateDictionary.Values.ToList();
            var popupIndex = 0;
            if (_trait[index].categoryId == (int) EffectCategoryEnum.STATE)
            {
                effectDropdownChoices = _stateDictionary.Values.ToList();
                popupIndex = _trait[index].traitsId - 1;
            }
            else if (_trait[index].categoryId == (int) EffectCategoryEnum.PARAMS)
            {
                effectDropdownChoices = _paramDictionary.Values.ToList();
                popupIndex = _trait[index].traitsId - 1;
            }
            else if (_trait[index].categoryId == (int) EffectCategoryEnum.OTHER)
            {
                if (_displayType == DisplayType.skill)
                {
                    effectDropdownChoices = _otherDictionaryBySkill.Values.ToList();
                    popupIndex = _trait[index].traitsId == 4 ? 2 - 1:_trait[index].traitsId - 1;
                }
                else
                {
                    effectDropdownChoices = _otherDictionaryByItem.Values.ToList();
                    popupIndex = _trait[index].traitsId - 1;
                }
            }

            var effectDropdownPopupField = new PopupFieldBase<string>(effectDropdownChoices, popupIndex);

            itemBlock.Add(effectDropdownPopupField);
            effectDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                if (_trait[index].categoryId == (int) EffectCategoryEnum.STATE)
                {
                    var pair = _stateDictionary.FirstOrDefault(c => c.Value == effectDropdownPopupField.value);
                    var key = pair.Key;
                    _trait[index].traitsId = (int) key;
                }
                else if (_trait[index].categoryId == (int) EffectCategoryEnum.PARAMS)
                {
                    var pair = _paramDictionary.FirstOrDefault(c => c.Value == effectDropdownPopupField.value);
                    var key = pair.Key;
                    _trait[index].traitsId = (int) key;
                }
                else if (_trait[index].categoryId == (int) EffectCategoryEnum.OTHER)
                {
                    //アイテム側に全部入っているので、アイテムの方で検索を行う
                    var pair = _otherDictionaryByItem.FirstOrDefault(c => c.Value == effectDropdownPopupField.value);
                    var key = pair.Key;
                    _trait[index].traitsId = (int) key;
                }

                _trait[index].effectId = 0;
                _trait[index].value = 0;
                SetEffects(index, traitsBlock, deleteBlock, categoryBlock, itemBlock, effectBlock, valueBlock);
                _callBackTrait(_trait);
            });

            SetEffects(index, traitsBlock, deleteBlock, categoryBlock, itemBlock, effectBlock, valueBlock);
        }

        /// <summary>
        ///     効果と値
        /// </summary>
        public void SetEffects(
            int index,
            VisualElement traitsBlock,
            VisualElement deleteBlock,
            VisualElement categoryBlock,
            VisualElement itemBlock,
            VisualElement effectBlock,
            VisualElement valueBlock
        ) {
            effectBlock.Clear();
            valueBlock.Clear();
            var effectDropdownChoices = _stateNameArray;

            var ecategoryBlockntCommonDataModels = new EventManagementService().LoadEventCommon();
            var commons = ecategoryBlockntCommonDataModels;
            var commonList = new List<string>();
            foreach (var c in commons) commonList.Add(c.name);

            if (_trait[index].categoryId == (int) EffectCategoryEnum.STATE)
            {
                effectDropdownChoices = _stateNameArray;
            }
            else if (_trait[index].categoryId == (int) EffectCategoryEnum.PARAMS)
            {
                effectDropdownChoices = _paramNameArray;
            }
            else if (_trait[index].categoryId == (int) EffectCategoryEnum.OTHER)
            {
                if (_trait[index].traitsId == (int) EffectOtherEnum.SPECIAL_EFFECT)
                    effectDropdownChoices = _escapeArray;
                else if (_trait[index].traitsId == (int) EffectOtherEnum.ADD_PARAMETER)
                    effectDropdownChoices = _paramNameArray;
                else if (_trait[index].traitsId == (int) EffectOtherEnum.MASTER_SKILL)
                    effectDropdownChoices = _skillNameArray;
                else if (_trait[index].traitsId == (int) EffectOtherEnum.COMMON_EVENT)
                    effectDropdownChoices = commonList;
            }

            var effectDropdownPopupField = new PopupFieldBase<string>(effectDropdownChoices, _trait[index].effectId);

            effectBlock.Add(effectDropdownPopupField);
            effectDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                if (_trait[index].categoryId == (int) EffectCategoryEnum.STATE)
                {
                    _trait[index].effectId = _stateNameArray.LastIndexOf(effectDropdownPopupField.value);
                }
                else if (_trait[index].categoryId == (int) EffectCategoryEnum.PARAMS)
                {
                    _trait[index].effectId = _paramNameArray.LastIndexOf(effectDropdownPopupField.value);
                }
                else if (_trait[index].categoryId == (int) EffectCategoryEnum.OTHER)
                {
                    if (_trait[index].traitsId == (int) EffectOtherEnum.SPECIAL_EFFECT)
                        _trait[index].effectId = 0;
                    else if (_trait[index].traitsId == (int) EffectOtherEnum.ADD_PARAMETER)
                        _trait[index].effectId = _paramNameArray.LastIndexOf(effectDropdownPopupField.value);
                    else if (_trait[index].traitsId == (int) EffectOtherEnum.MASTER_SKILL)
                        _trait[index].effectId = _skillNameArray.LastIndexOf(effectDropdownPopupField.value);
                    else if (_trait[index].traitsId == (int) EffectOtherEnum.COMMON_EVENT)
                        _trait[index].effectId = effectDropdownChoices.LastIndexOf(effectDropdownPopupField.value);
                }

                _callBackTrait(_trait);
            });

            int SwitchValueInput() {
                //「ステート」-「付与、解除」の際は百分率になります
                if (_trait[index].categoryId == (int) EffectCategoryEnum.STATE)
                    if (_trait[index].traitsId == (int) EffectStateEnum.ADD_STATE ||
                        _trait[index].traitsId == (int) EffectStateEnum.DELETE_STATE)
                        return 1;

                //「能力値」-「強化、弱化」の際はターン数になります
                if (_trait[index].categoryId == (int) EffectCategoryEnum.PARAMS)
                    if (_trait[index].traitsId == (int) EffectParameterEnum.ADD_BUFF ||
                        _trait[index].traitsId == (int) EffectParameterEnum.ADD_DEBUFF)
                        return 2;

                //「その他」-「成長」の際には負の値が入力できないように
                if (_trait[index].categoryId == (int) EffectCategoryEnum.OTHER)
                    if (_trait[index].traitsId == (int) EffectOtherEnum.ADD_PARAMETER)
                        return 3;

                return 0;
            }

            //数値入力が必要な場合に入る
            if (SwitchValueInput() != 0)
            {
                var effectValue = new IntegerField();
                valueBlock.Add(effectValue);
                effectValue.value = _trait[index].value;

                //百分率と負の値が入らないのSwitch
                switch (SwitchValueInput())
                {
                    case 1:
                        BaseInputFieldHandler.IntegerFieldCallback(effectValue, evt =>
                        {
                            _trait[index].value = effectValue.value;
                            _callBackTrait(_trait);
                        }, 0, 100);
                        break;
                    case 2:
                        BaseInputFieldHandler.IntegerFieldCallback(effectValue, evt =>
                        {
                            _trait[index].value = effectValue.value;
                            _callBackTrait(_trait);
                        }, 0, 9999);
                        break;
                    case 3:
                        BaseInputFieldHandler.IntegerFieldCallback(effectValue, evt =>
                        {
                            _trait[index].value = effectValue.value;
                            _callBackTrait(_trait);
                        }, 0, 1000);
                        break;
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
                var trait = _trait;
                trait.RemoveAt(index);
                Init(trait, _callBackTrait, _displayType);
            };
        }
    }
}