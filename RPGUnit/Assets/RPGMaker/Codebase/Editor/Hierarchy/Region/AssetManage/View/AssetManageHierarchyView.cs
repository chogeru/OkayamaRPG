using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Base.View.Component;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.AssetManage.View
{
    /// <summary>
    /// 素材管理のHierarchyView
    /// </summary>
    public class AssetManageHierarchyView : AbstractHierarchyView
    {
        // const
        //--------------------------------------------------------------------------------------------------------------
        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Hierarchy/Region/AssetManage/Asset/database_asset.uxml"; } }

        // ヒエラルキー本体クラス
        //--------------------------------------------------------------------------------------------------------------
        private readonly AssetManageHierarchy _assetManageHierarchy;

        // 利用するデータ
        //--------------------------------------------------------------------------------------------------------------
        private List<AssetManageDataModel> _walkingCharacterAssets;
        private List<AssetManageDataModel> _actorAssets;
        private List<AssetManageDataModel> _battleEffectAssets;
        private List<AssetManageDataModel> _objectAssets;
        private List<AssetManageDataModel> _popupIconAssets;
        private List<AssetManageDataModel> _stateOverlapAssets;
        private List<AssetManageDataModel> _weaponAssets;

        // 状態
        //--------------------------------------------------------------------------------------------------------------

        // UI要素
        //--------------------------------------------------------------------------------------------------------------
        private HierarchyItemListView _actorListView;
        private HierarchyItemListView _battleEffectListView;
        private HierarchyItemListView _objectListView;
        private HierarchyItemListView _popupIconListView;
        private HierarchyItemListView _stateOverlapListView;
        private HierarchyItemListView _walkingCharacterListView;
        private HierarchyItemListView _weaponListView;
        private const int foldoutCount = 2;


        //--------------------------------------------------------------------------------------------------------------
        //
        // methods
        //
        //--------------------------------------------------------------------------------------------------------------

        // 初期化・更新系
        //--------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="assetManageHierarchy"></param>
        public AssetManageHierarchyView(AssetManageHierarchy assetManageHierarchy) {
            _assetManageHierarchy = assetManageHierarchy;
            InitUI();
        }

        /// <summary>
        /// 各コンテンツデータの初期化
        /// </summary>
        override protected void InitContentsData() {
            SetFoldout("walkingCharacterFoldout");
            _walkingCharacterListView = new HierarchyItemListView(ViewName + "Walking");
            ((VisualElement) UxmlElement.Query<VisualElement>("walkingCharacterListContainer")).Add(_walkingCharacterListView);

            SetFoldout("objectFoldout");
            _objectListView = new HierarchyItemListView(ViewName + "Object");
            ((VisualElement) UxmlElement.Query<VisualElement>("objectListContainer")).Add(_objectListView);

            SetFoldout("popupIconFoldout");
            _popupIconListView = new HierarchyItemListView(ViewName + "PopupIcon");
            ((VisualElement) UxmlElement.Query<VisualElement>("popupIconListContainer")).Add(_popupIconListView);

            SetFoldout("actorFoldout");
            _actorListView = new HierarchyItemListView(ViewName + "Actor");
            ((VisualElement) UxmlElement.Query<VisualElement>("actorListContainer")).Add(_actorListView);

            SetFoldout("weaponFoldout");
            _weaponListView = new HierarchyItemListView(ViewName + "Weapon");
            ((VisualElement) UxmlElement.Query<VisualElement>("weaponListContainer")).Add(_weaponListView);

            SetFoldout("stateOverlapFoldout");
            _stateOverlapListView = new HierarchyItemListView(ViewName + "StateOverlap");
            ((VisualElement) UxmlElement.Query<VisualElement>("stateListContainer")).Add(_stateOverlapListView);

            SetFoldout("battleEffectFoldout");
            _battleEffectListView = new HierarchyItemListView(ViewName + "BattleEffect");
            ((VisualElement) UxmlElement.Query<VisualElement>("battleEffectListContainer")).Add(_battleEffectListView);

            SetFoldout("foldout_sv_battle_list");

            //Foldoutの開閉状態保持用
            for (int i = 0; i < foldoutCount; i++)
                SetFoldout("foldout_" + (i + 1));

            InitEventHandlers();
        }

        /// <summary>
        /// イベントの初期設定
        /// </summary>
        private void InitEventHandlers() {
            InitContextMenu(RegistrationLimit.None);
            var dic = new Dictionary<string, List<string>>
            {
                {
                    KeyNameAssetWalking,
                    new List<string>()
                    {
                        EditorLocalize.LocalizeText("WORD_1322"), EditorLocalize.LocalizeText("WORD_1323")
                    }
                },
                {
                    KeyNameAssetObject,
                    new List<string>()
                    {
                        EditorLocalize.LocalizeText("WORD_1327"), EditorLocalize.LocalizeText("WORD_1328")
                    }
                },
                {
                    KeyNameAssetPopupIcon,
                    new List<string>()
                    {
                        EditorLocalize.LocalizeText("WORD_1331"), EditorLocalize.LocalizeText("WORD_1332")
                    }
                },
                {
                    KeyNameAssetActor,
                    new List<string>()
                    {
                        EditorLocalize.LocalizeText("WORD_0302"), EditorLocalize.LocalizeText("WORD_0303")
                    }
                },
                {
                    KeyNameAssetWeapon,
                    new List<string>()
                    {
                        EditorLocalize.LocalizeText("WORD_0512"), EditorLocalize.LocalizeText("WORD_0513")
                    }
                },
                {
                    KeyNameAssetState,
                    new List<string>()
                    {
                        EditorLocalize.LocalizeText("WORD_1345"), EditorLocalize.LocalizeText("WORD_1346")
                    }
                },
                {
                    KeyNameAssetBattleEffect,
                    new List<string>()
                    {
                        EditorLocalize.LocalizeText("WORD_1349"), EditorLocalize.LocalizeText("WORD_1519")
                    }
                },
            };
            SetParentContextMenu(dic);

            // 歩行キャラ
            _walkingCharacterListView.SetEventHandler(
                (i, value) => { _assetManageHierarchy.OpenAssetManageInspector(_walkingCharacterAssets[i]); },
                (i, value) =>
                {
                    var childDic = new Dictionary<string, ContextMenuData>
                    {
                        {
                            KeyNameAssetWalking, new ContextMenuData()
                            {
                                UuId = _walkingCharacterAssets[i].id,
                                Names =
                                    new List<string>()
                                    {
                                        EditorLocalize.LocalizeText("WORD_1462"),
                                        EditorLocalize.LocalizeText("WORD_0383")
                                    },
                                SerialNumber = i,
                                DisplayStartNum = DisplayStartNum.None
                            }
                        }
                    };
                    SetChildContextMenu(childDic);
                });

            // オブジェクト
            _objectListView.SetEventHandler(
                (i, value) => { _assetManageHierarchy.OpenAssetManageInspector(_objectAssets[i]); },
                (i, value) =>
                {
                    var childDic = new Dictionary<string, ContextMenuData>
                    {
                        {
                            KeyNameAssetObject, new ContextMenuData()
                            {
                                UuId = _objectAssets[i].id,
                                Names =
                                    new List<string>()
                                    {
                                        EditorLocalize.LocalizeText("WORD_1462"),
                                        EditorLocalize.LocalizeText("WORD_0383")
                                    },
                                SerialNumber = i,
                                DisplayStartNum = DisplayStartNum.None
                            }
                        }
                    };
                    SetChildContextMenu(childDic);
                });

            // フキダシアイコン
            _popupIconListView.SetEventHandler(
                (i, value) => { _assetManageHierarchy.OpenAssetManageInspector(_popupIconAssets[i]); },
                (i, value) =>
                {
                    var childDic = new Dictionary<string, ContextMenuData>
                    {
                        {
                            KeyNameAssetPopupIcon, new ContextMenuData()
                            {
                                UuId = _popupIconAssets[i].id,
                                Names =
                                    new List<string>()
                                    {
                                        EditorLocalize.LocalizeText("WORD_1462"),
                                        EditorLocalize.LocalizeText("WORD_0383")
                                    },
                                SerialNumber = i,
                                DisplayStartNum = DisplayStartNum.None
                            }
                        }
                    };
                    SetChildContextMenu(childDic);
                });

            // アクター
            _actorListView.SetEventHandler(
                (i, value) => { _assetManageHierarchy.OpenAssetManageInspector(_actorAssets[i]); },
                (i, value) =>
                {
                    var childDic = new Dictionary<string, ContextMenuData>
                    {
                        {
                            KeyNameAssetActor, new ContextMenuData()
                            {
                                UuId = _actorAssets[i].id,
                                Names =
                                    new List<string>()
                                    {
                                        EditorLocalize.LocalizeText("WORD_1462"),
                                        EditorLocalize.LocalizeText("WORD_0383")
                                    },
                                SerialNumber = i,
                                DisplayStartNum = DisplayStartNum.None
                            }
                        }
                    };
                    SetChildContextMenu(childDic);
                });

            // 武器
            _weaponListView.SetEventHandler(
                (i, value) => { _assetManageHierarchy.OpenAssetManageInspector(_weaponAssets[i]); },
                (i, value) =>
                {
                    var childDic = new Dictionary<string, ContextMenuData>
                    {
                        {
                            KeyNameAssetWeapon, new ContextMenuData()
                            {
                                UuId = _weaponAssets[i].id,
                                Names =
                                    new List<string>()
                                    {
                                        EditorLocalize.LocalizeText("WORD_1462"),
                                        EditorLocalize.LocalizeText("WORD_0383")
                                    },
                                SerialNumber = i,
                                DisplayStartNum = DisplayStartNum.None
                            }
                        }
                    };
                    SetChildContextMenu(childDic);
                });

            // ステートの重ね合わせ
            _stateOverlapListView.SetEventHandler(
                (i, value) => { _assetManageHierarchy.OpenAssetManageInspector(_stateOverlapAssets[i]); },
                (i, value) =>
                {
                    var childDic = new Dictionary<string, ContextMenuData>
                    {
                        {
                            KeyNameAssetState, new ContextMenuData()
                            {
                                UuId = _stateOverlapAssets[i].id,
                                Names =
                                    new List<string>()
                                    {
                                        EditorLocalize.LocalizeText("WORD_1462"),
                                        EditorLocalize.LocalizeText("WORD_0383")
                                    },
                                SerialNumber = i,
                                DisplayStartNum = DisplayStartNum.None
                            }
                        }
                    };
                    SetChildContextMenu(childDic);
                });

            // バトルエフェクト
            _battleEffectListView.SetEventHandler(
                (i, value) => { _assetManageHierarchy.OpenAssetManageInspector(_battleEffectAssets[i]); },
                (i, value) =>
                {
                    var childDic = new Dictionary<string, ContextMenuData>
                    {
                        {
                            KeyNameAssetBattleEffect, new ContextMenuData()
                            {
                                UuId = _battleEffectAssets[i].id,
                                Names =
                                    new List<string>()
                                    {
                                        EditorLocalize.LocalizeText("WORD_1462"),
                                        EditorLocalize.LocalizeText("WORD_0383")
                                    },
                                SerialNumber = i,
                                DisplayStartNum = DisplayStartNum.None
                            }
                        }
                    };
                    SetChildContextMenu(childDic);
                });
        }

        protected override VisualElement CreateDataModel(string keyName) {
            VisualElement visualElement = base.CreateDataModel(keyName);
            switch (keyName)
            {
                case KeyNameAssetWalking:
                    _assetManageHierarchy.CreateAssetManageDataModel(AssetCategoryEnum.MOVE_CHARACTER);
                    visualElement = LastWalkingCharacterIndex();
                    break;
                case KeyNameAssetObject:
                    _assetManageHierarchy.CreateAssetManageDataModel(AssetCategoryEnum.OBJECT);
                    visualElement = LastObjectIndex();
                    break;
                case KeyNameAssetPopupIcon:
                    _assetManageHierarchy.CreateAssetManageDataModel(AssetCategoryEnum.POPUP);
                    visualElement = LastPopupIconIndex();
                    break;
                case KeyNameAssetActor:
                    _assetManageHierarchy.CreateAssetManageDataModel(AssetCategoryEnum.SV_BATTLE_CHARACTER);
                    visualElement = LastActorIndex();
                    break;
                case KeyNameAssetWeapon:
                    _assetManageHierarchy.CreateAssetManageDataModel(AssetCategoryEnum.SV_WEAPON);
                    visualElement = LastWeaponIndex();
                    break;
                case KeyNameAssetState:
                    _assetManageHierarchy.CreateAssetManageDataModel(AssetCategoryEnum.SUPERPOSITION);
                    visualElement = LastStateOverlapIndex();
                    break;
                case KeyNameAssetBattleEffect:
                    _assetManageHierarchy.CreateAssetManageDataModel(AssetCategoryEnum.BATTLE_EFFECT);
                    visualElement = LastBattleEffectIndex();
                    break;
            }

            return visualElement;
        }

        protected override VisualElement DuplicateDataModel(string keyName, string uuId) {
            VisualElement visualElement = base.DuplicateDataModel(keyName, uuId);
            switch (keyName)
            {
                case KeyNameAssetWalking:
                    var walkingCharacterAsset = _walkingCharacterAssets.FirstOrDefault(w => w.id == uuId);
                    _assetManageHierarchy.DuplicateAssetManageDataModel(walkingCharacterAsset);
                    visualElement = LastWalkingCharacterIndex();
                    break;
                case KeyNameAssetObject:
                    var objectAsset  = _objectAssets.FirstOrDefault(o => o.id == uuId);
                    _assetManageHierarchy.DuplicateAssetManageDataModel(objectAsset);
                    visualElement = LastObjectIndex();
                    break;
                case KeyNameAssetPopupIcon:
                    var popupIconAsset = _popupIconAssets.FirstOrDefault(p => p.id == uuId);
                    _assetManageHierarchy.DuplicateAssetManageDataModel(popupIconAsset);
                    visualElement = LastPopupIconIndex();
                    break;
                case KeyNameAssetActor:
                    var actorAsset = _actorAssets.FirstOrDefault(p => p.id == uuId);
                    _assetManageHierarchy.DuplicateAssetManageDataModel(actorAsset);
                    visualElement = LastActorIndex();
                    break;
                case KeyNameAssetWeapon:
                    var weaponAsset = _weaponAssets.FirstOrDefault(w => w.id == uuId);
                    _assetManageHierarchy.DuplicateAssetManageDataModel(weaponAsset);
                    visualElement = LastWeaponIndex();
                    break;
                case KeyNameAssetState:
                    var stateOverlapAsset = _stateOverlapAssets.FirstOrDefault(s => s.id == uuId);
                    _assetManageHierarchy.DuplicateAssetManageDataModel(stateOverlapAsset);
                    visualElement = LastStateOverlapIndex();
                    break;
                case KeyNameAssetBattleEffect:
                    var battleEffectAsset = _battleEffectAssets.FirstOrDefault(b => b.id == uuId);
                    _assetManageHierarchy.DuplicateAssetManageDataModel(battleEffectAsset);
                    visualElement = LastBattleEffectIndex();
                    break;
            }
            return visualElement;
        }

        protected override VisualElement DeleteDataModel(string keyName, string uuId) {
            VisualElement visualElement = base.DuplicateDataModel(keyName, uuId);
            int index = 0;
            List<VisualElement> elements;

            switch (keyName)
            {
                case KeyNameAssetWalking:
                    AssetManageDataModel walkingCharacterAsset = null;
                    index = 0;
                    for (int i = 0; i < _walkingCharacterAssets.Count; i++)
                    {
                        if (_walkingCharacterAssets[i].id == uuId)
                        {
                            walkingCharacterAsset = _walkingCharacterAssets[i];
                            index = i;
                            break;
                        }
                    }
                    _assetManageHierarchy.DeleteAssetManageDataModel(walkingCharacterAsset);
                   elements = new List<VisualElement>();
                    _walkingCharacterListView.Query<Button>().ForEach(button => { elements.Add(button); });
                    visualElement = elements.Count - 1 < index
                        ? LastWalkingCharacterIndex()
                        : elements.FirstOrDefault(e => e.name == "AssetManageHierarchyViewWalking" + index);

                    break;
                case KeyNameAssetObject:
                    AssetManageDataModel objectAsset = null;
                    index = 0;
                    for (int i = 0; i < _objectAssets.Count; i++)
                    {
                        if (_objectAssets[i].id == uuId)
                        {
                            objectAsset = _objectAssets[i];
                            index = i;
                            break;
                        }
                    }
                    _assetManageHierarchy.DeleteAssetManageDataModel(objectAsset);
                    elements = new List<VisualElement>();
                    _objectListView.Query<Button>().ForEach(button => { elements.Add(button); });
                    visualElement = elements.Count - 1 < index
                        ? LastObjectIndex()
                        : elements.FirstOrDefault(e => e.name == "AssetManageHierarchyViewObject" + index);
                    break;
                case KeyNameAssetPopupIcon:
                    AssetManageDataModel popupIconAsset = null;
                    index = 0;
                    for (int i = 0; i < _popupIconAssets.Count; i++)
                    {
                        if (_popupIconAssets[i].id == uuId)
                        {
                            popupIconAsset = _popupIconAssets[i];
                            index = i;
                            break;
                        }
                    }
                    _assetManageHierarchy.DeleteAssetManageDataModel(popupIconAsset);
                    elements = new List<VisualElement>();
                    _popupIconListView.Query<Button>().ForEach(button => { elements.Add(button); });
                    visualElement = elements.Count - 1 < index
                        ? LastPopupIconIndex()
                        : elements.FirstOrDefault(e => e.name == "AssetManageHierarchyViewPopupIcon" + index);
                    break;
                case KeyNameAssetActor:
                    AssetManageDataModel actorAsset = null;
                    index = 0;
                    for (int i = 0; i < _actorAssets.Count; i++)
                    {
                        if (_actorAssets[i].id == uuId)
                        {
                            actorAsset = _actorAssets[i];
                            index = i;
                            break;
                        }
                    }
                    _assetManageHierarchy.DeleteAssetManageDataModel(actorAsset);
                    elements = new List<VisualElement>();
                    _actorListView.Query<Button>().ForEach(button => { elements.Add(button); });
                    visualElement = elements.Count - 1 < index
                        ? LastActorIndex()
                        : elements.FirstOrDefault(e => e.name == "AssetManageHierarchyViewActor" + index);
                    break;
                case KeyNameAssetWeapon:
                    AssetManageDataModel weaponAsset = null;
                    index = 0;
                    for (int i = 0; i < _weaponAssets.Count; i++)
                    {
                        if (_weaponAssets[i].id == uuId)
                        {
                            weaponAsset = _weaponAssets[i];
                            index = i;
                            break;
                        }
                    }
                    _assetManageHierarchy.DeleteAssetManageDataModel(weaponAsset);
                    elements = new List<VisualElement>();
                    _weaponListView.Query<Button>().ForEach(button => { elements.Add(button); });
                    visualElement = elements.Count - 1 < index
                        ? LastWeaponIndex()
                        : elements.FirstOrDefault(e => e.name == "AssetManageHierarchyViewWeapon" + index);
                    break;
                case KeyNameAssetState:
                    AssetManageDataModel stateOverlapAsset = null;
                    index = 0;
                    for (int i = 0; i < _stateOverlapAssets.Count; i++)
                    {
                        if (_stateOverlapAssets[i].id == uuId)
                        {
                            stateOverlapAsset = _stateOverlapAssets[i];
                            index = i;
                            break;
                        }
                    }
                    _assetManageHierarchy.DeleteAssetManageDataModel(stateOverlapAsset);
                    elements = new List<VisualElement>();
                    _stateOverlapListView.Query<Button>().ForEach(button => { elements.Add(button); });
                    visualElement = elements.Count - 1 < index
                        ? LastStateOverlapIndex()
                        : elements.FirstOrDefault(e => e.name == "AssetManageHierarchyViewStateOverlap" + index);
                    break;
                case KeyNameAssetBattleEffect:
                    AssetManageDataModel battleEffectAsset = null;
                    index = 0;
                    for (int i = 0; i < _battleEffectAssets.Count; i++)
                    {
                        if (_battleEffectAssets[i].id == uuId)
                        {
                            battleEffectAsset = _battleEffectAssets[i];
                            index = i;
                            break;
                        }
                    }
                    _assetManageHierarchy.DeleteAssetManageDataModel(battleEffectAsset);
                    elements = new List<VisualElement>();
                    _battleEffectListView.Query<Button>().ForEach(button => { elements.Add(button); });
                    visualElement = elements.Count - 1 < index
                        ? LastBattleEffectIndex()
                        : elements.FirstOrDefault(e => e.name == "AssetManageHierarchyViewBattleEffect" + index);
                    break;
            }

            return visualElement;
        }

        /// <summary>
        /// データ更新
        /// </summary>
        /// <param name="walkingCharacterAssets"></param>
        /// <param name="objectAssets"></param>
        /// <param name="popupIconAssets"></param>
        /// <param name="actorAssets"></param>
        /// <param name="weaponAssets"></param>
        /// <param name="stateAssets"></param>
        /// <param name="battleEffectAssets"></param>
        public void Refresh(
            [CanBeNull] List<AssetManageDataModel> walkingCharacterAssets = null,
            [CanBeNull] List<AssetManageDataModel> objectAssets = null,
            [CanBeNull] List<AssetManageDataModel> popupIconAssets = null,
            [CanBeNull] List<AssetManageDataModel> actorAssets = null,
            [CanBeNull] List<AssetManageDataModel> weaponAssets = null,
            [CanBeNull] List<AssetManageDataModel> stateAssets = null,
            [CanBeNull] List<AssetManageDataModel> battleEffectAssets = null
        ) {
            _walkingCharacterAssets = walkingCharacterAssets ?? _walkingCharacterAssets;
            _objectAssets = objectAssets ?? _objectAssets;
            _popupIconAssets = popupIconAssets ?? _popupIconAssets;
            _actorAssets = actorAssets ?? _actorAssets;
            _weaponAssets = weaponAssets ?? _weaponAssets;
            _stateOverlapAssets = stateAssets ?? _stateOverlapAssets;
            _battleEffectAssets = battleEffectAssets ?? _battleEffectAssets;

            base.Refresh();
        }

        /// <summary>
        /// データ更新
        /// </summary>
        protected override void RefreshContents() {
            base.RefreshContents();
            _walkingCharacterListView.Refresh(_walkingCharacterAssets.Select(item => item.name).ToList());
            _objectListView.Refresh(_objectAssets.Select(item => item.name).ToList());
            _popupIconListView.Refresh(_popupIconAssets.Select(item => item.name).ToList());
            _actorListView.Refresh(_actorAssets.Select(item => item.name).ToList());
            _weaponListView.Refresh(_weaponAssets.Select(item => item.name).ToList());
            _stateOverlapListView.Refresh(_stateOverlapAssets.Select(item => item.name).ToList());
            _battleEffectListView.Refresh(_battleEffectAssets.Select(item => item.name).ToList());
        }

        /// <summary>
        /// 最終選択していた歩行キャラを返却
        /// </summary>
        /// <returns></returns>
        public VisualElement LastWalkingCharacterIndex() {
            var elements = new List<VisualElement>();
            _walkingCharacterListView.Query<Button>().ForEach(button => { elements.Add(button); });

            //選択可能なVisualElementが存在しない場合には、Inspectorを初期化して終了する
            if (elements.Count == 0)
            {
                Inspector.Inspector.Clear();
                return null;
            }

            return elements[elements.Count - 1];
        }

        /// <summary>
        /// 最終選択していたオブジェクトを返却
        /// </summary>
        /// <returns></returns>
        public VisualElement LastObjectIndex() {
            var elements = new List<VisualElement>();
            _objectListView.Query<Button>().ForEach(button => { elements.Add(button); });

            //選択可能なVisualElementが存在しない場合には、Inspectorを初期化して終了する
            if (elements.Count == 0)
            {
                Inspector.Inspector.Clear();
                return null;
            }

            return elements[elements.Count - 1];
        }

        /// <summary>
        /// 最終選択していたフキダシを返却
        /// </summary>
        /// <returns></returns>
        public VisualElement LastPopupIconIndex() {
            var elements = new List<VisualElement>();
            _popupIconListView.Query<Button>().ForEach(button => { elements.Add(button); });

            //選択可能なVisualElementが存在しない場合には、Inspectorを初期化して終了する
            if (elements.Count == 0)
            {
                Inspector.Inspector.Clear();
                return null;
            }

            return elements[elements.Count - 1];
        }

        /// <summary>
        /// 最終選択していたアクターを返却
        /// </summary>
        /// <returns></returns>
        public VisualElement LastActorIndex() {
            var elements = new List<VisualElement>();
            _actorListView.Query<Button>().ForEach(button => { elements.Add(button); });

            //選択可能なVisualElementが存在しない場合には、Inspectorを初期化して終了する
            if (elements.Count == 0)
            {
                Inspector.Inspector.Clear();
                return null;
            }

            return elements[elements.Count - 1];
        }

        /// <summary>
        /// 最終選択していた武器を返却
        /// </summary>
        /// <returns></returns>
        public VisualElement LastWeaponIndex() {
            var elements = new List<VisualElement>();
            _weaponListView.Query<Button>().ForEach(button => { elements.Add(button); });

            //選択可能なVisualElementが存在しない場合には、Inspectorを初期化して終了する
            if (elements.Count == 0)
            {
                Inspector.Inspector.Clear();
                return null;
            }

            return elements[elements.Count - 1];
        }

        /// <summary>
        /// 最終選択していたステートを返却
        /// </summary>
        /// <returns></returns>
        public VisualElement LastStateOverlapIndex() {
            var elements = new List<VisualElement>();
            _stateOverlapListView.Query<Button>().ForEach(button => { elements.Add(button); });

            //選択可能なVisualElementが存在しない場合には、Inspectorを初期化して終了する
            if (elements.Count == 0)
            {
                Inspector.Inspector.Clear();
                return null;
            }

            return elements[elements.Count - 1];
        }

        /// <summary>
        /// 最終選択していたバトルエフェクトを返却
        /// </summary>
        /// <returns></returns>
        public VisualElement LastBattleEffectIndex() {
            var elements = new List<VisualElement>();
            _battleEffectListView.Query<Button>().ForEach(button => { elements.Add(button); });

            //選択可能なVisualElementが存在しない場合には、Inspectorを初期化して終了する
            if (elements.Count == 0)
            {
                Inspector.Inspector.Clear();
                return null;
            }

            return elements[elements.Count - 1];
        }
    }
}