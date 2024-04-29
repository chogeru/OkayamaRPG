// #define TEST_PREVIE_SCENE_AGING

using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Region.AssetManage.View;
using System;
using System.Collections.Generic;
using System.Linq;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Animation;
using UnityEngine;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.AssetManage
{
    /// <summary>
    /// 素材管理のHierarchy
    /// </summary>
    public class AssetManageHierarchy : AbstractHierarchy
    {
        private List<AssetManageDataModel> _actorAssets;
        private List<AssetManageDataModel> _battleEffectAssets;
        private List<AssetManageDataModel> _objectAssets;
        private AssetManageRepository.OrderManager.OrderData _orderData;
        private List<AssetManageDataModel> _popupIconAssets;
        private List<AssetManageDataModel> _stateOverlapAssets;
        private List<AssetManageDataModel> _walkingCharacterAssets;
        private List<AssetManageDataModel> _weaponAssets;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AssetManageHierarchy() {
            LoadData();
            View = new AssetManageHierarchyView(this);
        }

        /// <summary>
        /// View
        /// </summary>
        public AssetManageHierarchyView View { get; }

        /// <summary>
        /// データの読込
        /// </summary>
        override protected void LoadData() {
            base.LoadData();
            _orderData = AssetManageRepository.OrderManager.Load();
            _walkingCharacterAssets = databaseManagementService.LoadWalkingCharacterAssets();
            _objectAssets = databaseManagementService.LoadObjectAssets();
            _popupIconAssets = databaseManagementService.LoadPopupIconAssets();
            _actorAssets = databaseManagementService.LoadActorAssets();
            _weaponAssets = databaseManagementService.LoadWeaponAssets();
            _stateOverlapAssets = databaseManagementService.LoadStateOverlapAssets();
            _battleEffectAssets = databaseManagementService.LoadBattleEffectAssets();

            //orderDataの並び順でソートする
            for (int i = 0; i < _orderData.orderDataList[(int) AssetCategoryEnum.MOVE_CHARACTER].idList.Count; i++)
            {
                for (int j = i; j < _walkingCharacterAssets.Count; j++)
                {
                    if (_orderData.orderDataList[(int) AssetCategoryEnum.MOVE_CHARACTER].idList[i] == _walkingCharacterAssets[j].id)
                    {
                        var work = _walkingCharacterAssets[i];
                        _walkingCharacterAssets[i] = _walkingCharacterAssets[j];
                        _walkingCharacterAssets[j] = work;
                        break;
                    }
                }
            }
            for (int i = 0; i < _orderData.orderDataList[(int) AssetCategoryEnum.OBJECT].idList.Count; i++)
            {
                for (int j = i; j < _objectAssets.Count; j++)
                {
                    if (_orderData.orderDataList[(int) AssetCategoryEnum.OBJECT].idList[i] == _objectAssets[j].id)
                    {
                        var work = _objectAssets[i];
                        _objectAssets[i] = _objectAssets[j];
                        _objectAssets[j] = work;
                        break;
                    }
                }
            }
            for (int i = 0; i < _orderData.orderDataList[(int) AssetCategoryEnum.POPUP].idList.Count; i++)
            {
                for (int j = i; j < _popupIconAssets.Count; j++)
                {
                    if (_orderData.orderDataList[(int) AssetCategoryEnum.POPUP].idList[i] == _popupIconAssets[j].id)
                    {
                        var work = _popupIconAssets[i];
                        _popupIconAssets[i] = _popupIconAssets[j];
                        _popupIconAssets[j] = work;
                        break;
                    }
                }
            }
            for (int i = 0; i < _orderData.orderDataList[(int) AssetCategoryEnum.SV_BATTLE_CHARACTER].idList.Count; i++)
            {
                for (int j = i; j < _actorAssets.Count; j++)
                {
                    if (_orderData.orderDataList[(int) AssetCategoryEnum.SV_BATTLE_CHARACTER].idList[i] == _actorAssets[j].id)
                    {
                        var work = _actorAssets[i];
                        _actorAssets[i] = _actorAssets[j];
                        _actorAssets[j] = work;
                        break;
                    }
                }
            }
            for (int i = 0; i < _orderData.orderDataList[(int) AssetCategoryEnum.SV_WEAPON].idList.Count; i++)
            {
                for (int j = i; j < _weaponAssets.Count; j++)
                {
                    if (_orderData.orderDataList[(int) AssetCategoryEnum.SV_WEAPON].idList[i] == _weaponAssets[j].id)
                    {
                        var work = _weaponAssets[i];
                        _weaponAssets[i] = _weaponAssets[j];
                        _weaponAssets[j] = work;
                        break;
                    }
                }
            }
            for (int i = 0; i < _orderData.orderDataList[(int) AssetCategoryEnum.SUPERPOSITION].idList.Count; i++)
            {
                for (int j = i; j < _stateOverlapAssets.Count; j++)
                {
                    if (_orderData.orderDataList[(int) AssetCategoryEnum.SUPERPOSITION].idList[i] == _stateOverlapAssets[j].id)
                    {
                        var work = _stateOverlapAssets[i];
                        _stateOverlapAssets[i] = _stateOverlapAssets[j];
                        _stateOverlapAssets[j] = work;
                        break;
                    }
                }
            }
            for (int i = 0; i < _orderData.orderDataList[(int) AssetCategoryEnum.BATTLE_EFFECT].idList.Count; i++)
            {
                for (int j = 0; j < _battleEffectAssets.Count; j++)
                {
                    if (_orderData.orderDataList[(int) AssetCategoryEnum.BATTLE_EFFECT].idList[i] == _battleEffectAssets[j].id)
                    {
                        var work = _battleEffectAssets[i];
                        _battleEffectAssets[i] = _battleEffectAssets[j];
                        _battleEffectAssets[j] = work;
                        break;
                    }
                }
            }

            // serialを振り直す
            foreach (var asset in _walkingCharacterAssets.Select((value, index) => new { value, index }))
                asset.value.SerialNumber = asset.index + 1;
            foreach (var asset in _objectAssets.Select((value, index) => new { value, index }))
                asset.value.SerialNumber = asset.index + 1;
            foreach (var asset in _popupIconAssets.Select((value, index) => new { value, index }))
                asset.value.SerialNumber = asset.index + 1;
            foreach (var asset in _actorAssets.Select((value, index) => new { value, index }))
                asset.value.SerialNumber = asset.index + 1;
            foreach (var asset in _weaponAssets.Select((value, index) => new { value, index }))
                asset.value.SerialNumber = asset.index + 1;
            foreach (var asset in _stateOverlapAssets.Select((value, index) => new { value, index }))
                asset.value.SerialNumber = asset.index + 1;
            foreach (var asset in _battleEffectAssets.Select((value, index) => new { value, index }))
                asset.value.SerialNumber = asset.index + 1;
        }

        /// <summary>
        /// Viewの更新
        /// </summary>
        protected override void UpdateView(string updateData = null) {
            base.UpdateView();
            View.Refresh(
                _walkingCharacterAssets,
                _objectAssets,
                _popupIconAssets,
                _actorAssets,
                _weaponAssets,
                _stateOverlapAssets,
                _battleEffectAssets
            );
        }

        /// <summary>
        /// 素材管理のInspector表示
        /// </summary>
        /// <param name="assetManageDataModel"></param>
        public void OpenAssetManageInspector(AssetManageDataModel assetManageDataModel) {
            Inspector.Inspector.AssetManageEditView(assetManageDataModel);

#if TEST_PREVIE_SCENE_AGING
            CoreSystem.Helper.DebugUtil.EditorRepeatExecution(
                () => { Inspector.Inspector.AssetManageEditView(assetManageDataModel); },
                "歩行キャラ",
                100,
                0.1f);
#endif
        }

        /// <summary>
        /// 素材管理の新規作成
        /// </summary>
        /// <param name="assetType"></param>
        public void CreateAssetManageDataModel(AssetCategoryEnum assetType) {
            var newModel = AssetManageDataModel.CreateDefault(
                (int) assetType,
                _orderData.orderDataList[(int) assetType].idList.Count);
            newModel.name = "#" + string.Format("{0:D4}", _orderData.orderDataList[(int) assetType].idList.Count + 1) +
                            "　" + EditorLocalize.LocalizeText("WORD_1518");
            newModel.SerialNumber = _orderData.orderDataList[(int) assetType].idList.Count + 1;

            if (ImageManager.GetSvIdList(assetType).Count > 0)
            {
                var imageId = ImageManager.GetSvIdList(assetType)[0].id;
                var inputString =
                    UnityEditorWrapper.AssetDatabaseWrapper
                        .LoadAssetAtPath<TextAsset>(
                            "Assets/RPGMaker/Storage/AssetManage/JSON/Assets/" + imageId + ".json");
                var assetManageData = JsonHelper.FromJson<AssetManageDataModel>(inputString.text);
                newModel.imageSettings = assetManageData.imageSettings;
            }
            else
            {
                newModel.imageSettings = new List<AssetManageDataModel.ImageSetting>();
                switch (newModel.assetTypeId)
                {
                    case (int) AssetCategoryEnum.MOVE_CHARACTER:
                    case (int) AssetCategoryEnum.OBJECT:
                        for (int i = 0; i < 5; i++)
                        {
                            newModel.imageSettings.Add(new AssetManageDataModel.ImageSetting("", 0, 0, 1,1));
                        }
                        break;
                    case (int) AssetCategoryEnum.SV_BATTLE_CHARACTER:
                        for (int i = 0; i < 18; i++)
                        {
                            newModel.imageSettings.Add(new AssetManageDataModel.ImageSetting("", 0, 0, 1,1));
                        }
                        break;
                    case (int) AssetCategoryEnum.POPUP:
                    case (int) AssetCategoryEnum.SV_WEAPON:
                    case (int) AssetCategoryEnum.SUPERPOSITION:
                    case (int) AssetCategoryEnum.BATTLE_EFFECT:
                        newModel.imageSettings.Add(new AssetManageDataModel.ImageSetting("", 0, 0, 1,1));
                        break;
                }
            }

            var list = _orderData.orderDataList[(int) assetType].idList.ToList();
            list.Add(newModel.id);
            _orderData.orderDataList[(int) assetType].idList = list;

            switch (newModel.assetTypeId)
            {
                case (int) AssetCategoryEnum.MOVE_CHARACTER:
                    _walkingCharacterAssets.Add(newModel);
                    break;
                case (int) AssetCategoryEnum.OBJECT:
                    _objectAssets.Add(newModel);
                    break;
                case (int) AssetCategoryEnum.SV_BATTLE_CHARACTER:
                    _actorAssets.Add(newModel);
                    break;
                case (int) AssetCategoryEnum.POPUP:
                    _popupIconAssets.Add(newModel);
                    break;
                case (int) AssetCategoryEnum.SV_WEAPON:
                    _weaponAssets.Add(newModel);
                    break;
                case (int) AssetCategoryEnum.SUPERPOSITION:
                    _stateOverlapAssets.Add(newModel);
                    break;
                case (int) AssetCategoryEnum.BATTLE_EFFECT:
                    _battleEffectAssets.Add(newModel);
                    break;
            }

            databaseManagementService.SaveAssetManage(newModel);
            AssetManageRepository.OrderManager.Save(_orderData);

            Refresh();

            switch (newModel.assetTypeId)
            {
                case (int) AssetCategoryEnum.MOVE_CHARACTER:
                    Hierarchy.InvokeSelectableElementAction(View.LastWalkingCharacterIndex());
                    break;
                case (int) AssetCategoryEnum.OBJECT:
                    Hierarchy.InvokeSelectableElementAction(View.LastObjectIndex());
                    break;
                case (int) AssetCategoryEnum.SV_BATTLE_CHARACTER:
                    Hierarchy.InvokeSelectableElementAction(View.LastActorIndex());
                    break;
                case (int) AssetCategoryEnum.POPUP:
                    Hierarchy.InvokeSelectableElementAction(View.LastPopupIconIndex());
                    break;
                case (int) AssetCategoryEnum.SV_WEAPON:
                    Hierarchy.InvokeSelectableElementAction(View.LastWeaponIndex());
                    break;
                case (int) AssetCategoryEnum.SUPERPOSITION:
                    Hierarchy.InvokeSelectableElementAction(View.LastStateOverlapIndex());
                    break;
                case (int) AssetCategoryEnum.BATTLE_EFFECT:
                    Hierarchy.InvokeSelectableElementAction(View.LastBattleEffectIndex());
                    break;
            }
        }

        /// <summary>
        /// 素材管理のコピー＆貼り付け処理
        /// </summary>
        /// <param name="assetManageDataModel"></param>
        public void DuplicateAssetManageDataModel(AssetManageDataModel assetManageDataModel) {
            var duplicated = assetManageDataModel.DataClone();
            duplicated.id = Guid.NewGuid().ToString();
            var dataModelNames = GetAssetManageDataModel(duplicated).Select(a => a.name).ToList();
            duplicated.name = CreateDuplicateName(dataModelNames, duplicated.name);
            duplicated.sort = _orderData.orderDataList[duplicated.assetTypeId].idList.ToList().Count;
            duplicated.SerialNumber = _orderData.orderDataList[duplicated.assetTypeId].idList.ToList().Count + 1;

            var list = _orderData.orderDataList[duplicated.assetTypeId].idList.ToList();
            list.Add(duplicated.id);
            _orderData.orderDataList[duplicated.assetTypeId].idList = list;

            switch (duplicated.assetTypeId)
            {
                case (int) AssetCategoryEnum.MOVE_CHARACTER:
                    _walkingCharacterAssets.Add(duplicated);
                    break;
                case (int) AssetCategoryEnum.OBJECT:
                    _objectAssets.Add(duplicated);
                    break;
                case (int) AssetCategoryEnum.SV_BATTLE_CHARACTER:
                    _actorAssets.Add(duplicated);
                    break;
                case (int) AssetCategoryEnum.POPUP:
                    _popupIconAssets.Add(duplicated);
                    break;
                case (int) AssetCategoryEnum.SV_WEAPON:
                    _weaponAssets.Add(duplicated);
                    break;
                case (int) AssetCategoryEnum.SUPERPOSITION:
                    _stateOverlapAssets.Add(duplicated);
                    break;
                case (int) AssetCategoryEnum.BATTLE_EFFECT:
                    _battleEffectAssets.Add(duplicated);
                    break;
            }

            databaseManagementService.SaveAssetManage(duplicated);
            AssetManageRepository.OrderManager.Save(_orderData);
            Refresh();
        }

        private List<AssetManageDataModel> GetAssetManageDataModel(AssetManageDataModel duplicated) {
            List<AssetManageDataModel> manageDataModels = null;
            switch (duplicated.assetTypeId)
            {
                case (int) AssetCategoryEnum.MOVE_CHARACTER:
                    manageDataModels =_walkingCharacterAssets;
                    break;
                case (int) AssetCategoryEnum.OBJECT:
                    manageDataModels = _objectAssets;
                    break;
                case (int) AssetCategoryEnum.SV_BATTLE_CHARACTER:
                    manageDataModels = _actorAssets;
                    break;
                case (int) AssetCategoryEnum.POPUP:
                    manageDataModels = _popupIconAssets;
                    break;
                case (int) AssetCategoryEnum.SV_WEAPON:
                    manageDataModels = _weaponAssets;
                    break;
                case (int) AssetCategoryEnum.SUPERPOSITION:
                    manageDataModels = _stateOverlapAssets;
                    break;
                case (int) AssetCategoryEnum.BATTLE_EFFECT:
                    manageDataModels = _battleEffectAssets;
                    break;
            }

            return manageDataModels;
        }

        /// <summary>
        /// 素材管理の削除
        /// </summary>
        /// <param name="assetManageDataModel"></param>
        public void DeleteAssetManageDataModel(AssetManageDataModel assetManageDataModel) {
            // 全体のデータから要素を削除する
            _orderData.orderDataList[assetManageDataModel.assetTypeId].idList.Remove(assetManageDataModel.id);

            //それぞれのデータの削除を実施
            switch (assetManageDataModel.assetTypeId)
            {
                case (int) AssetCategoryEnum.MOVE_CHARACTER:
                    _walkingCharacterAssets.Remove(assetManageDataModel);
                    foreach (var asset in _walkingCharacterAssets.Select((value, index) => new { value, index }))
                        asset.value.SerialNumber = asset.index + 1;
                    break;
                case (int) AssetCategoryEnum.OBJECT:
                    _objectAssets.Remove(assetManageDataModel);
                    foreach (var asset in _objectAssets.Select((value, index) => new { value, index }))
                        asset.value.SerialNumber = asset.index + 1;
                    break;
                case (int) AssetCategoryEnum.SV_BATTLE_CHARACTER:
                    _actorAssets.Remove(assetManageDataModel);
                    foreach (var asset in _actorAssets.Select((value, index) => new { value, index }))
                        asset.value.SerialNumber = asset.index + 1;
                    break;
                case (int) AssetCategoryEnum.POPUP:
                    _popupIconAssets.Remove(assetManageDataModel);
                    foreach (var asset in _popupIconAssets.Select((value, index) => new { value, index }))
                        asset.value.SerialNumber = asset.index + 1;
                    break;
                case (int) AssetCategoryEnum.SV_WEAPON:
                    _weaponAssets.Remove(assetManageDataModel);
                    foreach (var asset in _weaponAssets.Select((value, index) => new { value, index }))
                        asset.value.SerialNumber = asset.index + 1;
                    break;
                case (int) AssetCategoryEnum.SUPERPOSITION:
                    _stateOverlapAssets.Remove(assetManageDataModel);
                    foreach (var asset in _stateOverlapAssets.Select((value, index) => new { value, index }))
                        asset.value.SerialNumber = asset.index + 1;
                    break;
                case (int) AssetCategoryEnum.BATTLE_EFFECT:
                    _battleEffectAssets.Remove(assetManageDataModel);
                    foreach (var asset in _battleEffectAssets.Select((value, index) => new { value, index }))
                        asset.value.SerialNumber = asset.index + 1;
                    break;
            }

            AssetManageRepository.OrderManager.Save(_orderData);
            databaseManagementService.SaveAssetManage(assetManageDataModel, true);
            Refresh();
        }
    }
}