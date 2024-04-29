using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Vehicle;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.Map.View
{
    /// <summary>
    /// [マップ設定]-[マップリスト]-[各マップ]-[マップ編集]-[コリジョンの配置] Inspector
    /// </summary>
    public class BackgroundCollisionInspector : AbstractInspectorElement
    {
        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/Map/Asset/background_collision_inspector.uxml"; } }
        private const string vehicleTrafficUxml =
            "Assets/RPGMaker/Codebase/Editor/Inspector/Map/Asset/VehiclePass.uxml";

        private Dictionary<string, string> _damageFloorTypeDictionary;
        private PopupFieldBase<string>     _damageFloorTypeSelect;
        private FloatField                 _damageFloorValueText;

        private readonly DatabaseManagementService _databaseManagementService;
        private          Toggle                    _isBushToggle;
        private          Toggle                    _isDamageFloorToggle;
        private          Toggle                    _isLadderToggle;
        private          Toggle                    _isPassBottomToggle;
        private          Toggle                    _isPassLeftToggle;
        private          Toggle                    _isPassRightToggle;
        private          Toggle                    _isPassTopToggle;

        private Dictionary<string, string> _passTypeDictionary;

        private PopupFieldBase<string> _passTypeSelect;

        private readonly TileDataModel _tileDataModel;
        private          Button        _vehicleAddButton;

        //乗り物部分UI
        private VisualElement              _vehicleAria;
        private Button                     _vehicleDelete;
        private VisualElement              _vehicleFoldDown;
        private Dictionary<string, string> _vehiclePassTypeDictionary;
        private PopupFieldBase<string>     _vehiclePassTypeSelect;

        private readonly List<VehiclesDataModel> _vehiclesDataModels;

        public BackgroundCollisionInspector(TileDataModel tileDataModel, Action onClickRegisterBtn) {
            _tileDataModel = tileDataModel;
            _databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;
            _vehiclesDataModels = _databaseManagementService.LoadCharacterVehicles();

            LoadDictionaries();
            Initialize();
        }

        protected override void RefreshContents() {
            base.RefreshContents();
            LoadDictionaries();
            Initialize();
        }

        private void LoadDictionaries() {
            _passTypeDictionary = EditorLocalize.LocalizeDictionaryValues(new Dictionary<string, string>
            {
                {TileDataModel.PassType.CanPassNormally.ToString(), "WORD_0808"},
                {TileDataModel.PassType.CanPassUnder.ToString(), "WORD_0809"},
                {TileDataModel.PassType.CannotPass.ToString(), "WORD_0810"}
            });

            _vehiclePassTypeDictionary = EditorLocalize.LocalizeDictionaryValues(new Dictionary<string, string>
            {
                {TileDataModel.PassType.CanPassNormally.ToString(), "WORD_0808"},
                {TileDataModel.PassType.CanPassUnder.ToString(), "WORD_0809"},
                {TileDataModel.PassType.CannotPass.ToString(), "WORD_0810"}
            });

            _damageFloorTypeDictionary = EditorLocalize.LocalizeDictionaryValues(new Dictionary<string, string>
            {
                {TileDataModel.DamageFloorType.None.ToString(), "WORD_0113"},
                {TileDataModel.DamageFloorType.Fix.ToString(), "WORD_0820"},
                {TileDataModel.DamageFloorType.Rate.ToString(), "WORD_0821"}
            });
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();
            _isPassTopToggle = RootContainer.Query<Toggle>("is_pass_top_toggle");
            _isPassBottomToggle = RootContainer.Query<Toggle>("is_pass_bottom_toggle");
            _isPassLeftToggle = RootContainer.Query<Toggle>("is_pass_left_toggle");
            _isPassRightToggle = RootContainer.Query<Toggle>("is_pass_right_toggle");
            _isLadderToggle = RootContainer.Query<Toggle>("is_ladder_toggle");
            _isBushToggle = RootContainer.Query<Toggle>("is_bush_toggle");
            _isDamageFloorToggle = RootContainer.Query<Toggle>("is_damage_floor_toggle");
            _damageFloorValueText = RootContainer.Query<FloatField>("damage_floor_value_text");
            _vehicleAria = RootContainer.Query<VisualElement>("vehicle_aria");
            _vehicleAddButton = RootContainer.Query<Button>("vehicle_add_button");

            SetEntityToUI();
        }

        private static PopupFieldBase<string> MakePopupField(
            Dictionary<string, string> dictionary,
            VisualElement parentContainer,
            string containerName,
            int defaultIndex
        ) {
            var RootContainer = (VisualElement) parentContainer.Query<VisualElement>(containerName);
            var popupField = new PopupFieldBase<string>(dictionary.Values.ToList(), defaultIndex);
            RootContainer.Add(popupField);
            return popupField;
        }

        private void SetEntityToUI() {
            _passTypeSelect = MakePopupField(_passTypeDictionary, RootContainer, "pass_type_select_container",(int)_tileDataModel.passType);

            _passTypeSelect.value = _passTypeDictionary[_tileDataModel.passType.ToString()];
            _passTypeSelect.RegisterValueChangedCallback(evt =>
            {
                _tileDataModel.passType =
                    (TileDataModel.PassType) Enum.ToObject(typeof(TileDataModel.PassType), _passTypeSelect.index);
                SaveTile();
            });
            _isPassTopToggle.value = _tileDataModel.isPassTop;
            _isPassTopToggle.RegisterValueChangedCallback(evt =>
            {
                _tileDataModel.isPassTop = evt.newValue;
                SaveTile();
            });
            _isPassBottomToggle.value = _tileDataModel.isPassBottom;
            _isPassBottomToggle.RegisterValueChangedCallback(evt =>
            {
                _tileDataModel.isPassBottom = evt.newValue;
                SaveTile();
            });
            _isPassLeftToggle.value = _tileDataModel.isPassLeft;
            _isPassLeftToggle.RegisterValueChangedCallback(evt =>
            {
                _tileDataModel.isPassLeft = evt.newValue;
                SaveTile();
            });
            _isPassRightToggle.value = _tileDataModel.isPassRight;
            _isPassRightToggle.RegisterValueChangedCallback(evt =>
            {
                _tileDataModel.isPassRight = evt.newValue;
                SaveTile();
            });
            _isLadderToggle.value = _tileDataModel.isLadder;
            _isLadderToggle.RegisterValueChangedCallback(evt =>
            {
                _tileDataModel.isLadder = evt.newValue;
                SaveTile();
            });
            _isBushToggle.value = _tileDataModel.isBush;
            _isBushToggle.RegisterValueChangedCallback(evt =>
            {
                _tileDataModel.isBush = evt.newValue;
                SaveTile();
            });
            _isDamageFloorToggle.value = _tileDataModel.isDamageFloor;
            _isDamageFloorToggle.RegisterValueChangedCallback(evt =>
            {
                _tileDataModel.isDamageFloor = evt.newValue;
                SaveTile();
            });
            _damageFloorTypeSelect =
                MakePopupField(_damageFloorTypeDictionary, RootContainer, "damage_floor_type_select_container",(int)_tileDataModel.damageFloorType);

            _damageFloorTypeSelect.RegisterValueChangedCallback(evt =>
            {
                _tileDataModel.damageFloorType = (TileDataModel.DamageFloorType)_damageFloorTypeSelect.index;
                SaveTile();
            });
            _damageFloorValueText.value = _tileDataModel.damageFloorValue;
            _damageFloorValueText.RegisterValueChangedCallback(evt =>
            {
                _tileDataModel.damageFloorValue = evt.newValue;
                SaveTile();
            });

            //乗り物部分の初期値
            var index = 0;
            //乗り物部分の初期値
            foreach (var vehicleType in _tileDataModel.vehicleTypes)
            {
                AddVehicle(vehicleType, 0, index);
                index++;
            }

            //乗り物部分の追加
            _vehicleAddButton.clicked += () =>
            {
                AddVehicle(new VehicleType(0, _vehiclesDataModels[0].id), 1, _tileDataModel.vehicleTypes.Count);
                SaveTile();;
            };
        }

        //乗り物の項目の追加
        //乗り物タイプ、新規追加は「1」初期値は「0」
        private void AddVehicle(VehicleType type, int add, int index) {
            var vehicleVisualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(vehicleTrafficUxml);
            var vehicleContainer = vehicleVisualTree.CloneTree();
            EditorLocalize.LocalizeElements(vehicleContainer);
            vehicleContainer.style.flexGrow = 1;
            var vehicleIndex = index;

            _vehicleFoldDown = vehicleContainer.Query<VisualElement>("vehicle_fold_down");
            Toggle vehicleIsPassToggle = vehicleContainer.Query<Toggle>("is_pass_toggle");
            Toggle vehicleIsPassUnderToggle = vehicleContainer.Query<Toggle>("is_pass_under_toggle");
            _vehicleDelete = vehicleContainer.Query<Button>("vehicle_delete");

            //乗り物選択
            var VehicleDropdownPopupField =
                new PopupFieldBase<string>(VehicleList(), VehicleIdToIndex(type.vehicleId));
            _vehicleFoldDown.Clear();
            _vehicleFoldDown.Add(VehicleDropdownPopupField);
            VehicleDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                type.vehicleId = _vehiclesDataModels[VehicleDropdownPopupField.index].id;
                SaveTile();;
            });

            // 通行設定
            switch (type.vehiclePassType)
            {
                case TileDataModel.PassType.CanPassNormally:
                    vehicleIsPassToggle.value = true;
                    break;
                case TileDataModel.PassType.CanPassUnder:
                    vehicleIsPassUnderToggle.value = true;
                    break;
                case TileDataModel.PassType.CannotPass:
                    break;
            }

            vehicleIsPassToggle.RegisterValueChangedCallback(evt =>
            {
                OnPassToggleChanged(
                    vehicleIsPassToggle,
                    vehicleIsPassUnderToggle,
                    ref type.vehiclePassType,
                    TileDataModel.PassType.CanPassNormally,
                    false);

                _tileDataModel.vehicleTypes[vehicleIndex] = type;

                SaveTile();;
            });

            vehicleIsPassUnderToggle.RegisterValueChangedCallback(evt =>
            {
                OnPassToggleChanged(
                    vehicleIsPassUnderToggle,
                    vehicleIsPassToggle,
                    ref type.vehiclePassType,
                    TileDataModel.PassType.CanPassUnder,
                    false);

                _tileDataModel.vehicleTypes[vehicleIndex] = type;

                SaveTile();;
            });

            //削除
            _vehicleDelete.clicked += () =>
            {
                _tileDataModel.vehicleTypes.Remove(type);
                vehicleContainer.Clear();
                SaveTile();;
            };

            //新規追加の場合はデータにも追加する
            if (add == 1) _tileDataModel.vehicleTypes.Add(type);

            _vehicleAria.Add(vehicleContainer);
        }
        
        private void OnPassToggleChanged(
            Toggle changedToggle,
            Toggle otherToggle, 
            ref TileDataModel.PassType pathTypeToSet,
            TileDataModel.PassType pathTypeWhenTrue,
            bool includeChangePassDirection)
        {
            if (changedToggle.value)
            {
                pathTypeToSet = pathTypeWhenTrue;
                otherToggle.value = false;

                if (includeChangePassDirection)
                {
                    // 全向きオフなら全向きオンに。
                    List<Toggle> toggles =
                        new(){ _isPassTopToggle, _isPassBottomToggle, _isPassLeftToggle, _isPassRightToggle };
                    if (toggles.All(toggle => !toggle.value))
                    {
                        toggles.ForEach(toggle => toggle.value = true);
                    }
                }
            }
            else if (!otherToggle.value)
            {
                // 『通れる』と『下を潜って通れる』のどちらもfalseなら『通れない』を設定。
                pathTypeToSet = TileDataModel.PassType.CannotPass;
            }
        }

        //乗り物の選択
        private List<string> VehicleList() {
            var returnList = new List<string>();
            foreach (var vehiclesDataModel in _vehiclesDataModels) returnList.Add(vehiclesDataModel.name);

            return returnList;
        }

        //乗り物IDをIndexに
        private int VehicleIdToIndex(string id) {
            var returnindex = 0;
            for (var i = 0; i < _vehiclesDataModels.Count; i++)
                if (_vehiclesDataModels[i].id == id)
                {
                    returnindex = i;
                    break;
                }

            return returnindex;
        }

        private static int GetIndexOfDictionary(Dictionary<string, string> dictionary, string targetKey) {
            var index = dictionary.Keys.ToList().IndexOf(targetKey);
            return index > -1 ? index : 0;
        }

        private async void SaveTile() {
            await MapEditor.MapEditor.SaveTile(_tileDataModel);
            MapEditor.MapEditor.ReloadTiles();
        }
    }
}