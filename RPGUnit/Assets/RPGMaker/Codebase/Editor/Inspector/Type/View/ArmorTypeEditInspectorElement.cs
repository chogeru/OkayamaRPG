using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Hierarchy.Enum;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.Type.View
{
    /// <summary>
    /// [タイプの編集]-[防具タイプ] Inspector
    /// </summary>
    public class ArmorTypeEditInspectorElement : AbstractInspectorElement
    {
        private readonly SystemSettingDataModel.ArmorType _armorType;
        private readonly SystemSettingDataModel           _systemSettingDataModel;

        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/Type/Asset/inspector_typeEdit.uxml"; } }

        public ArmorTypeEditInspectorElement(SystemSettingDataModel.ArmorType armorType) {
            _systemSettingDataModel = databaseManagementService.LoadSystem();
            foreach (var sysArmor in _systemSettingDataModel.armorTypes)
                if (armorType.id == sysArmor.id)
                    _armorType = sysArmor;

            Refresh();
        }

        /// <summary>
        /// データの更新
        /// </summary>
        override protected void RefreshContents() {
            base.RefreshContents();
            Initialize();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();

            Foldout armor_type_list = RootContainer.Query<Foldout>("armor_type_list");
            armor_type_list.style.display = DisplayStyle.Flex;

            Armor();
        }

        private void Armor() {
            Label type_edit_armor_ID = RootContainer.Query<Label>("type_edit_armor_ID");
            type_edit_armor_ID.text = _armorType.SerialNumberString;

            ImTextField type_edit_armor_name = RootContainer.Query<ImTextField>("type_edit_armor_name");
            type_edit_armor_name.value = _armorType.name;
            type_edit_armor_name.RegisterCallback<FocusOutEvent>(o =>
            {
                _armorType.name = type_edit_armor_name.value;
                _UpdateSceneView();
            });
        }

        private void _UpdateSceneView() {
            databaseManagementService.SaveSystem(_systemSettingDataModel);
            _ = Editor.Hierarchy.Hierarchy.Refresh(Region.TypeEdit);
        }
    }
}