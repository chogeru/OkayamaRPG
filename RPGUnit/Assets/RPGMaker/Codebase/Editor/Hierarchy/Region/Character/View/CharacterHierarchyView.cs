using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Class;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Vehicle;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Base.View.Component;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Character.View
{
    /// <summary>
    /// キャラクターのHierarchyView
    /// </summary>
    public class CharacterHierarchyView : AbstractHierarchyView
    {
        // const
        //--------------------------------------------------------------------------------------------------------------
        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Hierarchy/Region/Character/Asset/database_characters.uxml"; } }

        // 利用するデータ
        //--------------------------------------------------------------------------------------------------------------
        private List<CharacterActorDataModel> _characterActorDataModels;
        private CharacterActorDataModel _actorDataModel;
        private List<ClassDataModel> _classDataModels;
        private List<CharacterActorDataModel> _npcCharacterActorDataModels;
        private List<VehiclesDataModel> _vehiclesDataModels;

        // ヒエラルキー本体クラス
        //--------------------------------------------------------------------------------------------------------------
        private readonly CharacterHierarchy _characterHierarchy;

        // UI要素
        //--------------------------------------------------------------------------------------------------------------
        private HierarchyItemListView _actorListView;
        private HierarchyItemListView _jobListView;
        private HierarchyItemListView _npcListView;
        private Button _initialPartySettingButton;
        private Button _jobCommonSettingButton;
        private HierarchyItemListView _vehicleListView;

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
        /// <param name="characterHierarchy"></param>
        public CharacterHierarchyView(CharacterHierarchy characterHierarchy) {
            _characterHierarchy = characterHierarchy;
            InitUI();
        }

        /// <summary>
        /// 各コンテンツデータの初期化
        /// </summary>
        override protected void InitContentsData() {
            //アクター
            SetFoldout("characterMasterFoldout");
            SetFoldout("actorFoldout");
            _actorListView = new HierarchyItemListView(ViewName + "Actor");
            ((VisualElement) UxmlElement.Query<VisualElement>("actorListContainer")).Add(_actorListView);

            //NPC
            SetFoldout("npcFoldout");
            _npcListView = new HierarchyItemListView(ViewName + "Npc");
            ((VisualElement) UxmlElement.Query<VisualElement>("npcListContainer")).Add(_npcListView);

            //初期パーティ
            _initialPartySettingButton = UxmlElement.Query<Button>("initialPartySettingButton");

            //乗り物
            SetFoldout("vehicleFoldout");
            _vehicleListView = new HierarchyItemListView(ViewName + "Vehicle");
            ((VisualElement) UxmlElement.Query<VisualElement>("vehicleListContainer")).Add(_vehicleListView);

            //職業
            SetFoldout("jobFoldout");
            _jobCommonSettingButton = UxmlElement.Query<Button>("jobCommonSettingButton");
            _jobListView = new HierarchyItemListView(ViewName + "Class");
            ((VisualElement) UxmlElement.Query<VisualElement>("jobListContainer")).Add(_jobListView);

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
                    KeyNameActor,
                    new List<string>()
                    {
                        EditorLocalize.LocalizeText("WORD_0302"), EditorLocalize.LocalizeText("WORD_0303")
                    }
                },
                {
                    KeyNameNpc,
                    new List<string>()
                    {
                        EditorLocalize.LocalizeText("WORD_0307"), EditorLocalize.LocalizeText("WORD_0308")
                    }
                },
                {
                    KeyNameVehicle,
                    new List<string>()
                    {
                        EditorLocalize.LocalizeText("WORD_0316"), EditorLocalize.LocalizeText("WORD_0317")
                    }
                },
                {
                    KeyNameJob,
                    new List<string>()
                    {
                        EditorLocalize.LocalizeText("WORD_0330"), EditorLocalize.LocalizeText("WORD_0331")
                    }
                }
            };
            SetParentContextMenu(dic);
            
            // アクターリストアイテムクリック時
            _actorListView.SetEventHandler(
                (i, value) =>
                {
                    Inspector.Inspector.CharacterView((int) ActorTypeEnum.ACTOR, _characterActorDataModels[i].uuId,
                        this);
                },
                (i, value) =>
                {
                    var childDic = new Dictionary<string, ContextMenuData>
                    {
                        {
                            KeyNameActor, new ContextMenuData()
                            {
                                UuId = _characterActorDataModels[i].uuId,
                                Names =
                                    new List<string>()
                                    {
                                        EditorLocalize.LocalizeText("WORD_0304"),
                                        EditorLocalize.LocalizeText("WORD_0305")
                                    },
                                SerialNumber = i,
                                DisplayStartNum = DisplayStartNum.One
                            }
                        }
                    };
                    SetChildContextMenu(childDic);
                });
            
            // NPCリストアイテムクリック時
            _npcListView.SetEventHandler(
                (i, value) =>
                {
                    Inspector.Inspector.CharacterView((int) ActorTypeEnum.NPC, _npcCharacterActorDataModels[i].uuId,
                        this);
                },
                (i, value) =>
                {
                    var childDic = new Dictionary<string, ContextMenuData>
                    {
                        {
                            KeyNameNpc, new ContextMenuData()
                            {
                                UuId = _npcCharacterActorDataModels[i].uuId,
                                Names =
                                    new List<string>()
                                    {
                                        EditorLocalize.LocalizeText("WORD_0309"),
                                        EditorLocalize.LocalizeText("WORD_0310")
                                    },
                                SerialNumber = i,
                                DisplayStartNum = DisplayStartNum.None
                            }
                        }
                    };
                    SetChildContextMenu(childDic);
                });

            // 初期パーティー設定ボタンクリック時
            Editor.Hierarchy.Hierarchy.AddSelectableElementAndAction(_initialPartySettingButton,
                () => { _characterHierarchy.OpenInitialPartySettingInspector(); });
            _initialPartySettingButton.clickable.clicked += () =>
            {
                Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(_initialPartySettingButton);
            };
            
            // 乗り物リストアイテムクリック時
            _vehicleListView.SetEventHandler(
                (i, value) => { _characterHierarchy.OpenVehicleInspector(_vehiclesDataModels[i]); },
                (i, value) =>
                {
                    var childDic = new Dictionary<string, ContextMenuData>
                    {
                        {
                            KeyNameVehicle, new ContextMenuData()
                            {
                                UuId = _vehiclesDataModels[i].id,
                                Names =
                                    new List<string>()
                                    {
                                        EditorLocalize.LocalizeText("WORD_0318"),
                                        EditorLocalize.LocalizeText("WORD_0319")
                                    },
                                SerialNumber = i,
                                DisplayStartNum = DisplayStartNum.None
                            }
                        }
                    };
                    SetChildContextMenu(childDic);
                });
            
            // 職業リストアイテムクリック時
            _jobListView.SetEventHandler(
                (i, value) => { _characterHierarchy.OpenClassInspector(_classDataModels[i]); },
                (i, value) =>
                {
                    var childDic = new Dictionary<string, ContextMenuData>
                    {
                        {
                            KeyNameJob, new ContextMenuData()
                            {
                                UuId = _classDataModels[i].id,
                                Names =
                                    new List<string>()
                                    {
                                        EditorLocalize.LocalizeText("WORD_0332"),
                                        EditorLocalize.LocalizeText("WORD_0333")
                                    },
                                SerialNumber = i,
                                DisplayStartNum = DisplayStartNum.One
                            }
                        }
                    };
                    SetChildContextMenu(childDic);
                });

            // 職業の共通設定クリック時
            Editor.Hierarchy.Hierarchy.AddSelectableElementAndAction(_jobCommonSettingButton, () =>
            {
                _characterHierarchy.OpenClassInspector(null);
            });
            _jobCommonSettingButton.clickable.clicked += () =>
            {
                Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(_jobCommonSettingButton);
            };
        }


        protected override VisualElement CreateDataModel(string keyName) {
            var visualElement = base.CreateDataModel(keyName);
            switch (keyName)
            {
                case KeyNameActor:
                    _characterHierarchy.CreateCharacterActorDataModel(this);
                    visualElement = LastActorIndex();
                    break;
                case KeyNameNpc:
                    _characterHierarchy.CreateNpcCharacterActorDataModel(this);
                    visualElement = LastNpcIndex();
                    break;
                case KeyNameVehicle:
                    _characterHierarchy.CreateVehicleDataModel();
                    visualElement = LastVehicleIndex();
                    break;
                case KeyNameJob:
                    _characterHierarchy.CreateClassDataModel();
                    visualElement = LastClassIndex();
                    break;
            }

            return visualElement;
        }

        protected override VisualElement DuplicateDataModel(string keyName, string uuId) {
            var visualElement = base.DuplicateDataModel(keyName, uuId);
            switch (keyName)
            {
                case KeyNameActor:
                case KeyNameNpc:
                    CharacterActorDataModel actorDataModel =
                        _characterActorDataModels.FirstOrDefault(actor => actor.uuId == uuId) ??
                        _npcCharacterActorDataModels.FirstOrDefault(actor => actor.uuId == uuId);
                    _characterHierarchy.PasteActorOrNpcDataModel(this, actorDataModel);
                    visualElement = actorDataModel?.charaType == (int) ActorTypeEnum.ACTOR
                        ? LastActorIndex()
                        : LastNpcIndex();
                    break;
                case KeyNameVehicle:
                    VehiclesDataModel vehiclesDataModel =
                        _vehiclesDataModels.FirstOrDefault(vehicle => vehicle.id == uuId);
                    _characterHierarchy.PasteVehicleDataModel(vehiclesDataModel);
                    visualElement = LastVehicleIndex();
                    break;
                case KeyNameJob:
                    ClassDataModel classDataModel = _classDataModels.FirstOrDefault(c => c.id == uuId);
                    _characterHierarchy.PasteClassDataModel(classDataModel);
                    visualElement = LastClassIndex();
                    break;
            }
            return visualElement;
        }

        protected override VisualElement DeleteDataModel(string keyName, string uuId) {
            var visualElement = base.DeleteDataModel(keyName, uuId);
            int index = 0;
            List<VisualElement> elements;
            switch (keyName)
            {
                case KeyNameActor:
                    CharacterActorDataModel actorDataModel = null;
                    index = 0;
                    for (int i = 0; i < _characterActorDataModels.Count; i++)
                    {
                        if (_characterActorDataModels[i].uuId == uuId)
                        {
                            actorDataModel = _characterActorDataModels[i];
                            index = i;
                            break;
                        }
                    }
                    _characterHierarchy.DeleteCharacterActorDataModel(actorDataModel);
                    elements = new List<VisualElement>();
                    _actorListView.Query<Button>().ForEach(button => { elements.Add(button); });
                    visualElement = elements.Count - 1 < index
                        ? LastActorIndex()
                        : elements.FirstOrDefault(e => e.name == "CharacterHierarchyViewActor" + index);
                    break;
                case KeyNameNpc:
                    CharacterActorDataModel npcDataModel = null;
                    index = 0;
                    for (int i = 0; i < _npcCharacterActorDataModels.Count; i++)
                    {
                        if (_npcCharacterActorDataModels[i].uuId == uuId)
                        {
                            npcDataModel = _npcCharacterActorDataModels[i];
                            index = i;
                            break;
                        }
                    }
                    _characterHierarchy.DeleteNpcCharacterActorDataModel(npcDataModel);
                    elements = new List<VisualElement>();
                    _npcListView.Query<Button>().ForEach(button => { elements.Add(button); });
                    visualElement = elements.Count - 1 < index
                        ? LastNpcIndex()
                        : elements.FirstOrDefault(e => e.name == "CharacterHierarchyViewNpc" + index);

                    break;
                case KeyNameVehicle:
                    VehiclesDataModel vehiclesDataModel = null;
                    index = 0;
                    for (int i = 0; i < _vehiclesDataModels.Count; i++)
                    {
                        if (_vehiclesDataModels[i].id == uuId)
                        {
                            vehiclesDataModel = _vehiclesDataModels[i];
                            index = i;
                            break;
                        }
                    }

                    _characterHierarchy.DeleteVehicleDataModel(vehiclesDataModel);
                    elements = new List<VisualElement>();
                    _vehicleListView.Query<Button>().ForEach(button => { elements.Add(button); });
                    visualElement = elements.Count - 1 < index
                        ? LastVehicleIndex()
                        : elements.FirstOrDefault(e => e.name == "CharacterHierarchyViewVehicle" + index);

                    break;
                case KeyNameJob:
                    ClassDataModel classDataModel = null;
                    index = 0;
                    for (int i = 0; i < _classDataModels.Count; i++)
                    {
                        if (_classDataModels[i].id == uuId)
                        {
                            classDataModel = _classDataModels[i];
                            index = i;
                            break;
                        }
                    }
                    _characterHierarchy.DeleteClassDataModel(classDataModel);
                    elements = new List<VisualElement>();
                    _jobListView.Query<Button>().ForEach(button => { elements.Add(button); });
                    visualElement = elements.Count - 1 < index
                        ? LastClassIndex()
                        : elements.FirstOrDefault(e => e.name == "CharacterHierarchyViewClass" + index);

                    break;
            }
            return visualElement;
        }

        // データ更新
        //--------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// データ更新
        /// </summary>
        /// <param name="characterActorDataModels"></param>
        /// <param name="npcCharacterActorDataModels"></param>
        /// <param name="vehiclesDataModels"></param>
        /// <param name="classDataModels"></param>
        /// <param name="enemyDataModels"></param>
        /// <param name="troopDataModels"></param>
        /// <param name="eventBattleDataModels"></param>
        /// <param name="encounterDataModels"></param>
        public void Refresh(
            [CanBeNull] List<CharacterActorDataModel> characterActorDataModels = null,
            [CanBeNull] List<CharacterActorDataModel> npcCharacterActorDataModels = null,
            [CanBeNull] List<VehiclesDataModel> vehiclesDataModels = null,
            [CanBeNull] List<ClassDataModel> classDataModels = null
        ) {
            if (characterActorDataModels != null) _characterActorDataModels = characterActorDataModels;
            if (npcCharacterActorDataModels != null) _npcCharacterActorDataModels = npcCharacterActorDataModels;
            if (vehiclesDataModels != null) _vehiclesDataModels = vehiclesDataModels;
            if (classDataModels != null) _classDataModels = classDataModels;
            base.Refresh();
        }

        /// <summary>
        /// データ更新
        /// </summary>
        protected override void RefreshContents() {
            base.RefreshContents();
            UpdateCharacter();
            UpdateVehicle();
            UpdateClass();
        }

        /// <summary>
        /// キャラクターの更新
        /// </summary>
        public void UpdateCharacter() {
            _actorListView.Refresh(_characterActorDataModels.Select(item => item.basic.name).ToList());
            _npcListView.Refresh(_npcCharacterActorDataModels.Select(item => item.basic.name).ToList());
        }

        /// <summary>
        /// 乗り物の更新
        /// </summary>
        public void UpdateVehicle() {
            _vehicleListView.Refresh(_vehiclesDataModels.Select(item => item.name).ToList());
        }

        /// <summary>
        /// 職業の更新
        /// </summary>
        public void UpdateClass() {
            _jobListView.Refresh(_classDataModels.Select(item => item.basic.name).ToList());
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
        /// 最終選択していたNPCを返却
        /// </summary>
        /// <returns></returns>
        public VisualElement LastNpcIndex() {
            var elements = new List<VisualElement>();
            _npcListView.Query<Button>().ForEach(button => { elements.Add(button); });

            //選択可能なVisualElementが存在しない場合には、Inspectorを初期化して終了する
            if (elements.Count == 0)
            {
                Inspector.Inspector.Clear();
                return null;
            }

            return elements[elements.Count - 1];
        }

        /// <summary>
        /// 最終選択していた乗り物を返却
        /// </summary>
        /// <returns></returns>
        public VisualElement LastVehicleIndex() {
            var elements = new List<VisualElement>();
            _vehicleListView.Query<Button>().ForEach(button => { elements.Add(button); });

            //選択可能なVisualElementが存在しない場合には、Inspectorを初期化して終了する
            if (elements.Count == 0)
            {
                Inspector.Inspector.Clear();
                return null;
            }

            return elements[elements.Count - 1];
        }

        /// <summary>
        /// 最終選択していた職業を返却
        /// </summary>
        /// <returns></returns>
        public VisualElement LastClassIndex() {
            var elements = new List<VisualElement>();
            _jobListView.Query<Button>().ForEach(button => { elements.Add(button); });

            //選択可能なVisualElementが存在しない場合には、Inspectorを初期化して終了する
            if (elements.Count == 0)
            {
                Inspector.Inspector.Clear();
                return null;
            }

            return elements[elements.Count - 1];
        }

        // イベントハンドラ
        //--------------------------------------------------------------------------------------------------------------
    }
}