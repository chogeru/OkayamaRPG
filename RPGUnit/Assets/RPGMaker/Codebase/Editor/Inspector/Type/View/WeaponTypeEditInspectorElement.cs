using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Hierarchy.Enum;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.Type.View
{
    /// <summary>
    /// [タイプの編集]-[武器タイプ] Inspector
    /// </summary>
    public class WeaponTypeEditInspectorElement : AbstractInspectorElement
    {
        private          List<AssetManageDataModel> _assetManageData;
        private readonly SystemSettingDataModel     _systemSettingDataModel;

        private readonly SystemSettingDataModel.WeaponType _weaponType;

        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/Type/Asset/inspector_typeEdit.uxml"; } }

        public WeaponTypeEditInspectorElement(SystemSettingDataModel.WeaponType weaponType) {
            _systemSettingDataModel = databaseManagementService.LoadSystem();
            foreach (var sysWeapon in _systemSettingDataModel.weaponTypes)
                if (weaponType.id == sysWeapon.id)
                    _weaponType = sysWeapon;
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

            Foldout weapon_type_list = RootContainer.Query<Foldout>("weapon_type_list");
            weapon_type_list.style.display = DisplayStyle.Flex;
            Weapon();
        }

        private void Weapon() {
            var battleEnum = new BattleEnums();

            Label type_edit_weapon_ID = RootContainer.Query<Label>("type_edit_weapon_ID");
            type_edit_weapon_ID.text = _weaponType.SerialNumberString;

            ImTextField type_edit_weapon_name = RootContainer.Query<ImTextField>("type_edit_weapon_name");
            type_edit_weapon_name.value = _weaponType.value;
            type_edit_weapon_name.RegisterCallback<FocusOutEvent>(o =>
            {
                _weaponType.value = type_edit_weapon_name.value;
                SaveData();
                _UpdateSceneView();
            });

            VisualElement weapon_setting_motion = RootContainer.Query<VisualElement>("weapon_setting_motion");
            var weapon_setting_motionPopupField = new PopupFieldBase<string>(
                EditorLocalize.LocalizeTexts(battleEnum.AttackMotionLabel),
                _weaponType.motionId);
            weapon_setting_motion.Add(weapon_setting_motionPopupField);
            weapon_setting_motionPopupField.RegisterValueChangedCallback(evt =>
            {
                _weaponType.motionId =
                    battleEnum.AttackMotionLabel.IndexOf(weapon_setting_motionPopupField.value);
                SaveData();
            });

            // 武器素材の画像プレビュー
            _assetManageData = databaseManagementService.LoadAssetManage();
            Image weaponSettingImagePreview = RootContainer.Query<Image>("weapon_setting_image_preview");
            weaponSettingImagePreview.scaleMode = ScaleMode.ScaleToFit;
            SetWeaponImage(weaponSettingImagePreview);

            //武器素材IDの部分
            VisualElement weaponSettingImage = RootContainer.Query<VisualElement>("weapon_setting_image");
            var weaponImagesList = new List<string>()
            {
                EditorLocalize.LocalizeText("WORD_0113")
            };
            var weaponImagesIdList = new List<string>()
            {
                ""
            };
            foreach (var assetManageData in _assetManageData
                .Where(assetManageData => assetManageData.assetTypeId == (int) AssetCategoryEnum.SV_WEAPON))
            {
                weaponImagesList.Add(assetManageData.name);
                weaponImagesIdList.Add(assetManageData.id);
            }

            int ImageId() {
                var returnIndex = 0;
                for (var i = 0; i < weaponImagesIdList.Count; i++)
                    if (weaponImagesIdList[i] == _weaponType.image)
                        returnIndex = i;

                return returnIndex;
            }

            var weaponSettingImagePopupField = new PopupFieldBase<string>(weaponImagesList, ImageId());

            weaponSettingImage.Add(weaponSettingImagePopupField);
            weaponSettingImagePopupField.RegisterValueChangedCallback(evt =>
            {
                _weaponType.image = weaponImagesIdList[weaponSettingImagePopupField.index];
                SetWeaponImage(weaponSettingImagePreview);
                SaveData();
            });
        }

        private void SetWeaponImage(Image weaponSettingImagePreview) {
            string imagePath = "";
            for (int i = 0; i < _assetManageData.Count; i++)
                if (_assetManageData[i].id == _weaponType.image)
                {
                    imagePath = _assetManageData[i]?.imageSettings?[0]?.path;
                }
            weaponSettingImagePreview.image = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(
                PathManager.IMAGE_WEAPON + imagePath);
        }

        private void SaveData() {
            databaseManagementService.SaveSystem(_systemSettingDataModel);
        }

        private void _UpdateSceneView() {
            _ = Editor.Hierarchy.Hierarchy.Refresh(Region.TypeEdit, _weaponType.id);
        }
    }
}