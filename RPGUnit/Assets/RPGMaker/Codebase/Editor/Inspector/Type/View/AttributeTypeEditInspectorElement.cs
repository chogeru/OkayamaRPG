using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using RPGMaker.Codebase.Editor.Hierarchy.Enum;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.Type.View
{
    /// <summary>
    /// [タイプの編集]-[属性] Inspector
    /// </summary>
    public class AttributeTypeEditInspectorElement : AbstractInspectorElement
    {
        private SystemSettingDataModel.Element _element;
        private SystemSettingDataModel         _systemSettingDataModel;

        private readonly string correlationUxml =
            "Assets/RPGMaker/Codebase/Editor/Inspector/Type/Asset/inspector_typeEdit_correlation.uxml";

        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/Type/Asset/inspector_typeEdit.uxml"; } }

        public AttributeTypeEditInspectorElement(SystemSettingDataModel.Element element) {
            _systemSettingDataModel = databaseManagementService.LoadSystem();
            foreach (var sysElement in _systemSettingDataModel.elements)
                if (sysElement.id == element.id)
                    _element = sysElement;

            Refresh();
        }

        override protected void RefreshContents() {
            base.RefreshContents();
            Initialize();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();

            VisualElement stateActionConstraints = RootContainer.Query<VisualElement>("attribute_type_list");
            stateActionConstraints.style.display = DisplayStyle.Flex;
            Element();
        }

        private void Element() {
            Label attributeId = RootContainer.Query<Label>("attribute_ID");
            attributeId.text = _element.SerialNumberString;

            ImTextField attributeName = RootContainer.Query<ImTextField>("attribute_name");
            attributeName.value = _element.value;
            attributeName.RegisterCallback<FocusOutEvent>(o =>
            {
                _element.value = attributeName.value;
                SaveData();
                _UpdateSceneView();
            });

            // 画像選択
            //------------------------------------------------------------------------------------------------------------------------------
            // プレビュー画像
            Image iconImage = RootContainer.Query<Image>("icon_image");
            iconImage.scaleMode = ScaleMode.ScaleToFit;
            iconImage.image =
                AssetDatabase.LoadAssetAtPath<Texture2D>(PathManager.IMAGE_ICON + _element.icon + ".png");

            // 画像名
            Label iconImageName = RootContainer.Query<Label>("icon_image_name");
            iconImageName.text = _element.icon;

            // 画像変更ボタン
            Button iconChangeBtn = RootContainer.Query<Button>("icon_image_change_btn");
            iconChangeBtn.clicked += () =>
            {
                var selectModalWindow = new ImageSelectModalWindow(PathManager.IMAGE_ICON, true);
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Image"), data =>
                {
                    var imageName = (string) data;
                    _element.icon = imageName;
                    iconImageName.text = _element.icon;
                    iconImage.image = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        PathManager.IMAGE_ICON + _element.icon + ".png");
                    SaveData();
                }, _element.icon);
            };

            // 画像インポートボタン
            Button iconImportBtn = RootContainer.Query<Button>("icon_image_import_btn");
            iconImportBtn.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("png", PathManager.IMAGE_ICON);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    _element.icon = path;
                    iconImageName.text = _element.icon;
                    iconImage.image = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        PathManager.IMAGE_ICON + _element.icon + ".png");
                    SaveData();
                }
            };

            IntegerField sameElementDamage = RootContainer.Query<IntegerField>("sameElement_damage");
            sameElementDamage.value = _element.sameElement;
            sameElementDamage.RegisterCallback<FocusOutEvent>(o =>
            {
                _element.sameElement = sameElementDamage.value;
                SaveData();
            });

            var elementNameList = new List<string>();
            var count = 0;
            for (var i = 0; i < _systemSettingDataModel.elements.Count; i++)
                if (elementNameList.Contains(_systemSettingDataModel.elements[i].value))
                {
                    count++;
                    elementNameList.Add(_systemSettingDataModel.elements[i].value +
                                        count);
                }
                else
                {
                    elementNameList.Add(_systemSettingDataModel.elements[i].value);
                }

            for (var i = 0; i < _element.advantageous.Count; i++)
            {
                ElementCorrelationCreate(true, i, RootContainer);
                ElementCorrelationSetting(true, i, elementNameList, RootContainer);
            }

            for (var i = 0; i < _element.disadvantage.Count; i++)
            {
                ElementCorrelationCreate(false, i, RootContainer);
                ElementCorrelationSetting(false, i, elementNameList, RootContainer);
            }

            //優勢の追加部分
            Button advantagesTypeButton = RootContainer.Query<Button>("advantages_type_button");
            advantagesTypeButton.clicked += () =>
            {
                var data = new SystemSettingDataModel.Advantage(0, 0);
                _element.advantageous.Add(data);
                ElementCorrelationCreate(true, _element.advantageous.Count - 1, RootContainer);
                ElementCorrelationSetting(true, _element.advantageous.Count - 1, elementNameList, RootContainer);
            };

            IntegerField advantagesTypeNum = RootContainer.Query<IntegerField>("advantages_type_num");
            advantagesTypeNum.value = _element.advantageous.Count;
            advantagesTypeNum.RegisterCallback<FocusOutEvent>(o =>
            {
                var max = _element.advantageous.Count;
                var advantageousCount = advantagesTypeNum.value -
                                        _element.advantageous.Count;

                if (0 < advantageousCount)
                {
                    for (var i = 0; i < advantageousCount; i++)
                    {
                        var data = new SystemSettingDataModel.Advantage(0, 0);
                        _element.advantageous.Add(data);
                        ElementCorrelationCreate(true, max + i, RootContainer);
                        ElementCorrelationSetting(true, max + i, elementNameList, RootContainer);
                    }
                }
                else if (advantageousCount != 0)
                {
                    advantageousCount = advantageousCount * -1;
                    for (var i = 0; i < advantageousCount; i++)
                        RootContainer.Q<VisualElement>("advantages_type_list").RemoveAt(max - i - 1);

                    _element.advantageous
                        .RemoveRange(max - advantageousCount, advantageousCount);
                }

                SaveData();
            });

            //劣勢の追加部分
            Button inferiorityTypeButton = RootContainer.Query<Button>("inferiority_type_button");
            inferiorityTypeButton.clicked += () =>
            {
                var data = new SystemSettingDataModel.Disadvantage(0, 1000);
                _element.disadvantage.Add(data);
                ElementCorrelationCreate(false, _element.disadvantage.Count - 1, RootContainer);
                ElementCorrelationSetting(false, _element.disadvantage.Count - 1, elementNameList, RootContainer);
            };

            IntegerField inferiorityTypeNum = RootContainer.Query<IntegerField>("inferiority_type_num");
            inferiorityTypeNum.value = _element.disadvantage.Count;
            inferiorityTypeNum.RegisterCallback<FocusOutEvent>(o =>
            {
                var max = _element.disadvantage.Count;
                var disadvantageCount = inferiorityTypeNum.value -
                                        _element.disadvantage.Count;

                if (0 < disadvantageCount)
                {
                    for (var i = 0; i < disadvantageCount; i++)
                    {
                        var data = new SystemSettingDataModel.Disadvantage(0, 0);
                        _element.disadvantage.Add(data);
                        ElementCorrelationCreate(false, max + i, RootContainer);
                        ElementCorrelationSetting(false, max + i, elementNameList, RootContainer);
                    }
                }
                else if (disadvantageCount != 0)
                {
                    disadvantageCount = disadvantageCount * -1;
                    for (var i = 0; i < disadvantageCount; i++)
                        RootContainer.Q<VisualElement>("inferiority_type_list").RemoveAt(max - i - 1);

                    _element.disadvantage
                        .RemoveRange(max - disadvantageCount, disadvantageCount);
                }

                SaveData();
            });
        }

        private void ElementCorrelationCreate(bool isTargetEffect, int i, VisualElement RootContainer) {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(correlationUxml);
            VisualElement labelFromUxml = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromUxml);
            Foldout attributeFoldout = labelFromUxml.Query<Foldout>("attribute_foldout");
            attributeFoldout.name = "attribute_foldout_" + _element.id + "_" + (isTargetEffect ? "0_" : "1_") + i;

            VisualElement attribute = labelFromUxml.Query<VisualElement>("attribute");
            Label attributeName = labelFromUxml.Query<Label>("attribute_name");
            Button delete = labelFromUxml.Query<Button>("delete");
            IntegerField damageMultiplier = labelFromUxml.Query<IntegerField>("damage_multiplier");
            if (isTargetEffect)
            {
                attributeFoldout.text = EditorLocalize.LocalizeText("WORD_1281") + " " + (i + 1);
                attribute.name = "advantages_" + attribute.name + (i + 1);
                damageMultiplier.name = "advantages_" + damageMultiplier.name + (i + 1);
                delete.name = "advantages_" + delete.name + (i + 1);
                attributeName.text = EditorLocalize.LocalizeText("WORD_3017");

                RootContainer.Q<VisualElement>("advantages_type_list").Add(labelFromUxml);
            }
            else
            {
                attributeFoldout.text = EditorLocalize.LocalizeText("WORD_1284") + " " + (i + 1);
                attribute.name = "inferiority_" + attribute.name + (i + 1);
                damageMultiplier.name = "inferiority_" + damageMultiplier.name + (i + 1);
                delete.name = "inferiority_" + delete.name + (i + 1);
                attributeName.text = EditorLocalize.LocalizeText("WORD_3018");

                RootContainer.Q<VisualElement>("inferiority_type_list").Add(labelFromUxml);
            }

            SaveData();
        }

        private void ElementCorrelationSetting(
            bool isTargetEffect,
            int i,
            List<string> elementNameList,
            VisualElement RootContainer
        ) {
            if (isTargetEffect)
            {
                VisualElement advantagesAttribute = RootContainer.Query<VisualElement>("advantages_attribute" + (i + 1));

                var advantagesAttributePopupField =
                    new PopupFieldBase<string>(elementNameList, _element.advantageous[i].element);
                advantagesAttribute.Add(advantagesAttributePopupField);
                advantagesAttributePopupField.RegisterValueChangedCallback(vt =>
                {
                    _element.advantageous[i].element = advantagesAttributePopupField.index;
                    SaveData();
                });

                IntegerField advantagesDamageMultiplier =
                    RootContainer.Query<IntegerField>("advantages_damage_multiplier" + (i + 1));
                advantagesDamageMultiplier.value = _element.advantageous[i].magnification / 10;
                BaseInputFieldHandler.IntegerFieldCallback(advantagesDamageMultiplier, evt =>
                {
                    _element.advantageous[i].magnification = advantagesDamageMultiplier.value * 10;
                    SaveData();
                    
                }, 0, 100);

                //削除ボタン
                Button advantagesDelete = RootContainer.Query<Button>("advantages_delete" + (i + 1));
                advantagesDelete.clicked += () =>
                {
                    RootContainer.Q<VisualElement>("advantages_type_list").RemoveAt(i);
                    _element.advantageous.RemoveAt(i);

                    SaveData();
                    RefreshScroll();
                };
            }
            else
            {
                VisualElement inferiorityAttribute =
                    RootContainer.Query<VisualElement>("inferiority_attribute" + (i + 1));
                var inferiorityAttributePopupField = new PopupFieldBase<string>(elementNameList,
                    _element.disadvantage[i].element);
                inferiorityAttribute.Add(inferiorityAttributePopupField);
                inferiorityAttributePopupField.RegisterValueChangedCallback(evt =>
                {
                    _element.disadvantage[i].element = inferiorityAttributePopupField.index;
                    SaveData();
                });
                IntegerField inferiorityDamageMultiplier =
                    RootContainer.Query<IntegerField>("inferiority_damage_multiplier" + (i + 1));
                inferiorityDamageMultiplier.value = _element
                    .disadvantage[i].magnification / 10;
                BaseInputFieldHandler.IntegerFieldCallback(inferiorityDamageMultiplier, evt =>
                {
                    _element.disadvantage[i].magnification = inferiorityDamageMultiplier.value * 10;
                    SaveData();
                    
                }, 100, 200);
                
                //削除ボタン
                Button inferiorityDelete = RootContainer.Query<Button>("inferiority_delete" + (i + 1));
                inferiorityDelete.clicked += () =>
                {
                    RootContainer.Q<VisualElement>("inferiority_type_list").RemoveAt(i);
                    _element.disadvantage.RemoveAt(i);

                    SaveData();
                    RefreshScroll();
                };
            }
        }

        private void SaveData() {
            databaseManagementService.SaveSystem(_systemSettingDataModel);
        }

        private void _UpdateSceneView() {
            _ = Editor.Hierarchy.Hierarchy.Refresh(Region.TypeEdit, _element.id);
        }
    }
}