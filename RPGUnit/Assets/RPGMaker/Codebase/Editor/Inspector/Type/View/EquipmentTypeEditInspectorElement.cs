using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Hierarchy.Enum;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.Type.View
{
    /// <summary>
    /// [タイプの編集]-[装備タイプ] Inspector
    /// </summary>
    public class EquipmentTypeEditInspectorElement : AbstractInspectorElement
    {
        private readonly SystemSettingDataModel.EquipType _equipmentType;
        private readonly SystemSettingDataModel           _systemSettingDataModel;

        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/Type/Asset/inspector_typeEdit.uxml"; } }

        public EquipmentTypeEditInspectorElement(SystemSettingDataModel.EquipType equipmentType) {
            _systemSettingDataModel = databaseManagementService.LoadSystem();
            foreach (var sysEquipment in _systemSettingDataModel.equipTypes)
                if (equipmentType.id == sysEquipment.id)
                    _equipmentType = sysEquipment;

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

            Foldout equipment_type_list = RootContainer.Query<Foldout>("equipment_type_list");
            equipment_type_list.style.display = DisplayStyle.Flex;
            Equipment();
        }

        private void Equipment() {
            Label type_edit_equipment_ID = RootContainer.Query<Label>("type_edit_equipment_ID");
            type_edit_equipment_ID.text = _equipmentType.SerialNumberString;

            ImTextField type_edit_equipment_name = RootContainer.Query<ImTextField>("type_edit_equipment_name");
            type_edit_equipment_name.value = _equipmentType.name;
            type_edit_equipment_name.RegisterCallback<FocusOutEvent>(o =>
            {
                _equipmentType.name = type_edit_equipment_name.value;
                databaseManagementService.SaveSystem(_systemSettingDataModel);
                _ = Editor.Hierarchy.Hierarchy.Refresh(Region.TypeEdit);
            });
        }
    }
}