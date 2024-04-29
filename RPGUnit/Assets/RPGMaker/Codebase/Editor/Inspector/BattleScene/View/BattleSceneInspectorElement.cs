// #define USE_TEST_BATTLE_SCENE
// #define EDITOR_SCENE_CONTROLLER_LOG

using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Armor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Encounter;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Enemy;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Troop;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Weapon;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Knowledge.Misc;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using RPGMaker.Codebase.Runtime.Battle;
using RPGMaker.Codebase.Runtime.Battle.BattleSceneTest;
using RPGMaker.Codebase.Runtime.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.CoreSystem.Knowledge.Misc.BattleSceneTransition;
using Random = UnityEngine.Random;

namespace RPGMaker.Codebase.Editor.Inspector.BattleScene.View
{
    /// <summary>
    /// [バトルの編集]-[戦闘シーン] Inspector
    /// </summary>
    public class BattleSceneInspectorElement : AbstractInspectorElement
    {
        private const string SO_PATH = "Assets/RPGMaker/Storage/Initializations/SO/battleTest.asset";

#if USE_TEST_BATTLE_SCENE
        private const string BattleSceneFilePath =
 "Assets/RPGMaker/Codebase/Runtime/Battle/BattleSceneTest/BattleSceneTest.unity";
#else
        private const string BattleSceneFilePath = "Assets/RPGMaker/Codebase/Runtime/Battle/Battle.unity";
#endif

        private const int InclinedMin          = -10;
        private const int InclinedMax          = +10;
        private const int MaxEnemyCountOfFront = 8;

        //フロントサイドの敵グループ上限数
        private const int FrontEnemyMax = 8;
        private const int SideEnemyMax  = 6;
        
        private SystemSettingDataModel _systemData;

        // 『基本設定』の情報。
        private SystemSettingDataModel.BattleScene _battleScene;


        // 『戦闘テスト』の情報。
        private BattleTestScriptableObject _testScriptable;

        private Button               _enemyAddButton;
        private List<EnemyDataModel> _enemyDataModels;

        private VisualElement[] _actorParentVes;

        // UI要素
        
        private RadioButton        _battleSceneViewTypeToggle1;
        private RadioButton        _battleSceneViewTypeToggle2;
        private InspectorItemUnit _battleSceneFrontTypeToggles;
        private VisualElement _battleSceneFrontView;
        private VisualElement _battleSceneSideView;
        private IntegerField  _battleSceneFrontY;
        private IntegerField  _battleSceneSideX;
        private IntegerField  _battleSceneSideY;
        private IntegerField  _battleSceneSidePartyInclined;
        private IntegerField  _battleSceneSideActorY;
        private IntegerField  _battleSceneSideEnemyInclined;
        private Button        _battleTestButton;
        private VisualElement _sceneMapFoldout;
        private VisualElement _sceneUnitFoldout;
        
        private SceneWindow _sceneWindow;

        Button[] actorAddButtons;
        
        List<PopupFieldBase<string>> _actorsFieldBase = new List<PopupFieldBase<string>>();

        /// <summary>
        ///     『戦闘テスト』の情報。
        /// </summary>
        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/BattleScene/Asset/inspector_battle_scene.uxml"; } }

        override protected void RefreshContents() {
            base.RefreshContents();

            _systemData = databaseManagementService.LoadSystem();
            _battleScene = _systemData.battleScene;

            _testScriptable = AssetDatabase.LoadAssetAtPath<BattleTestScriptableObject>(SO_PATH);
            if (_testScriptable == null)
            {
                _testScriptable = ScriptableObject.CreateInstance<BattleTestScriptableObject>();
                AssetDatabase.CreateAsset(_testScriptable, SO_PATH);
            }
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            Initialize();
        }

        protected override void InitializeContents() {
            base.InitializeContents();
            Refresh();
            
            _battleSceneViewTypeToggle1 = RootContainer.Query<RadioButton>("radioButton-battleEdit-display1");
            _battleSceneViewTypeToggle2 = RootContainer.Query<RadioButton>("radioButton-battleEdit-display2");
            _battleSceneFrontView = RootContainer.Query<VisualElement>("battle_scene_frontView");
            _battleSceneSideView = RootContainer.Query<VisualElement>("battle_scene_sideView");
            _battleSceneFrontTypeToggles = RootContainer.Query<InspectorItemUnit>("battle_scene_front_type_toggles");
            _battleSceneFrontTypeToggles.Clear();
            //フロント部分
            _battleSceneFrontY = RootContainer.Query<IntegerField>("battle_scene_front_y");
            //サイド部分
            _battleSceneSideX = RootContainer.Query<IntegerField>("battle_scene_side_x");
            _battleSceneSideY = RootContainer.Query<IntegerField>("battle_scene_side_y");
            _battleSceneSidePartyInclined = RootContainer.Query<IntegerField>("battle_scene_side_party_inclined");
            _battleSceneSideActorY = RootContainer.Query<IntegerField>("battle_scene_side_actor_y");
            _battleSceneSideEnemyInclined = RootContainer.Query<IntegerField>("battle_scene_side_enemy_inclined");

            _battleTestButton = RootContainer.Q<Button>("battle_scene_map_test_exec");

            if (EditorApplication.isPlayingOrWillChangePlaymode) 
                _battleTestButton.text = EditorLocalize.LocalizeText("WORD_4001");

            EditorApplication.playModeStateChanged += (PlayModeStateChange data) =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                    _battleTestButton.text = EditorLocalize.LocalizeText("WORD_4001");
                else
                    _battleTestButton.text = EditorLocalize.LocalizeText("WORD_0643");
            };

            _battleTestButton.clicked += () =>
            {
                if (!EditorApplication.isPlaying)
                {
                    EditorSceneController.PlayBattleTestScene(BattleSceneFilePath, _testScriptable);
                }
                else
                {
                    EditorSceneController.OnScenePlayEnd("");
                }
            };

            _sceneMapFoldout = RootContainer.Query<VisualElement>("scene_map_foldout");
            _sceneUnitFoldout = RootContainer.Query<VisualElement>("scene_unit_foldout");
            
            SetBasicItems();
            SetBattleTest();
            SetBattleTestButtonEnabled();
            
            _sceneWindow =
                WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.DatabaseSceneWindow) as
                    SceneWindow;
            _sceneWindow.Create(SceneWindow.PreviewId.BattleScene);

            InitAndRenderSceneView();
        }

        /// <summary>
        ///     基本データ
        /// </summary>
        private void SetBasicItems() {
            // フロントビュー/サイドビューtoggle
            int defaultSelect = _battleScene.viewType;
            new CommonToggleSelector().SetRadioInVisualElementSelector(
                new List<RadioButton> { _battleSceneViewTypeToggle1, _battleSceneViewTypeToggle2 },
                new List<VisualElement> { _battleSceneFrontView, _battleSceneSideView },
                defaultSelect, new List<Action>
                {
                    () =>
                    {
                        _battleScene.viewType = _battleSceneViewTypeToggle1.value ? 0 : 1;
                        Save();
                        AdjustEnemies();
                        InitAndRenderSceneView();
                    },
                    () =>
                    {
                        _battleScene.viewType = _battleSceneViewTypeToggle1.value ? 0 : 1;
                        Save();
                        AdjustEnemies();
                        InitAndRenderSceneView();
                    }
                });

            void AdjustEnemies() {
                if (_battleScene.viewType == 0 && _testScriptable.enemyIds.Count > MaxEnemyCountOfFront)
                {
                    _testScriptable.enemyIds = _testScriptable.enemyIds.Take(MaxEnemyCountOfFront).ToList();
                    SetEnemies();
                    _SaveSo();
                    SetBattleTestButtonEnabled();
                }
                else
                {
                    SetEnemyAddButtonEnabled();
                }

                if (_battleScene.viewType == 1 && _testScriptable.enemyIds.Count > SideEnemyMax)
                {
                    while (_testScriptable.enemyIds.Count > SideEnemyMax)
                        _testScriptable.enemyIds.RemoveAt(_testScriptable.enemyIds.Count - 1);
                    SetEnemies();
                    _SaveSo();
                    SetBattleTestButtonEnabled();
                }

                // サイドビュー。
                if (_battleScene.viewType == 1)
                {
                    // 全てのモンスター出現最大数を、サイドビューでの最大値におさめる。
                    var modified = false;
                    var encounterDataModels = databaseManagementService.LoadEncounter();
                    foreach (var encounterDataModel in encounterDataModels)
                        if (encounterDataModel.enemyMax > SideEnemyMax)
                        {
                            encounterDataModel.enemyMax = SideEnemyMax;
                            modified = true;
                        }

                    if (modified) databaseManagementService.SaveEncounter(encounterDataModels);
                }
            }

            UIElementsUtil.AddToggleGroup(
                _battleSceneFrontTypeToggles,
                EditorLocalize.LocalizeText("WORD_0636"),
                EditorLocalize.LocalizeTexts(new List<string> {"WORD_0052", "WORD_0533"}),
                _battleScene.frontMiddleStartFlag,
                clickToggleIndex =>
                {
                    _battleScene.frontMiddleStartFlag = clickToggleIndex;
                    Save();
                });
            _battleSceneFrontTypeToggles.SetClass();


            //モンスターの位置調整(Y)
            _battleSceneFrontY.value = _battleScene.frontEnemyPositionY;
            BaseInputFieldHandler.IntegerFieldCallback(_battleSceneFrontY, evt =>
            {
                _battleScene.frontEnemyPositionY = _battleSceneFrontY.value;
                Save();
                InitAndRenderSceneView();
            }, 360, 720);

            //サイドパーティ表示位置
            //x.y
            _battleSceneSideX.value = _battleScene.sidePartyPosition[0];
            BaseInputFieldHandler.IntegerFieldCallback(_battleSceneSideX, evt =>
            {
                _battleScene.sidePartyPosition[0] = _battleSceneSideX.value;
                Save();
                InitAndRenderSceneView();
            }, -1920, 1920);
            
            
            _battleSceneSideY.value = _battleScene.sidePartyPosition[1];
            BaseInputFieldHandler.IntegerFieldCallback(_battleSceneSideY, evt =>
            {
                _battleScene.sidePartyPosition[1] = _battleSceneSideY.value;
                Save();
                InitAndRenderSceneView();
            }, -1080, 1080);


            //サイド-パーティーの傾斜表示
            _battleSceneSidePartyInclined.value = _battleScene.sidePartyInclined;
            BaseInputFieldHandler.IntegerFieldCallback(_battleSceneSidePartyInclined, evt =>
            {
                _battleSceneSidePartyInclined.value =
                    Math.Min(Math.Max(_battleSceneSidePartyInclined.value, InclinedMin), InclinedMax);
                _battleScene.sidePartyInclined = _battleSceneSidePartyInclined.value;
                Save();
                InitAndRenderSceneView();
            }, -10, 10);

            //サイド-アクターの間隔
            _battleSceneSideActorY.value = _battleScene.sideActorSpace;

            BaseInputFieldHandler.IntegerFieldCallback(_battleSceneSideActorY, evt =>
            {
                _battleScene.sideActorSpace = _battleSceneSideActorY.value;
                Save();
                InitAndRenderSceneView();
            }, 0, 10);

            //サイド-敵キャラの傾斜表示
            _battleSceneSideEnemyInclined.value = _battleScene.sideEnemyInclined;
            _battleSceneSideEnemyInclined.RegisterCallback<FocusOutEvent>(evt =>
            {
                _battleSceneSideEnemyInclined.value =
                    Math.Min(Math.Max(_battleSceneSideEnemyInclined.value, InclinedMin), InclinedMax);
                _battleScene.sideEnemyInclined = _battleSceneSideEnemyInclined.value;
                Save();
                InitAndRenderSceneView();
            });
        }
        
        /// <summary>
        /// 戦闘シーンのプレビュー設定
        /// </summary>
        private void InitAndRenderSceneView() {
            if (_sceneWindow == null)
                return;

            //Runtime実行中
            if (EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
            {
                //Runtime実行中は、戦闘シーンのプレビュー表示は行わない
                //そのためデータ刷新を行わずに処理を終了する
                _sceneWindow.Clear();

                //GameViewに切り替わらないケースがあるため、明示的にGameViewにフォーカスをあてる
                var assembly = typeof(EditorWindow).Assembly;
                var type = assembly.GetType("UnityEditor.GameView");
                EditorWindow gameView = EditorWindow.GetWindow(type);
                gameView.Focus();

                return;
            }

            DataManager.Self().ClearRuntimeSaveDataModel();
            DataManager.Self().ReloadGameParty();
            TroopDataModel troop = DataManager.Self().GetTroopDataModels().FirstOrDefault(t => t.id == TroopDataModel.TROOP_PREVIEW);
            //新しく固定のデータを作成し、末尾につける
            if (troop == null)
            {
                troop = new TroopDataModel();
                troop.id = TroopDataModel.TROOP_PREVIEW;
                troop.frontViewMembers = new List<TroopDataModel.FrontViewMember>();
                troop.sideViewMembers = new List<TroopDataModel.SideViewMember>();
                var enemy = DataManager.Self().GetEnemyDataModels().First();
                for (int i = 0; i < 8; i++)
                {
                    TroopDataModel.FrontViewMember
                        enemyData = new TroopDataModel.FrontViewMember(enemy.id, i, 0, 0);
                    troop.frontViewMembers.Add(enemyData);
                }

                for (int i = 0; i < 6; i++)
                {
                    TroopDataModel.SideViewMember enemyData =
                        new TroopDataModel.SideViewMember(enemy.id, i / 3, i % 3, 0, 0);
                    troop.sideViewMembers.Add(enemyData);
                }

                DataManager.Self().GetTroopDataModels().Add(troop);
            }

            _sceneWindow.Init(troop);
            _sceneWindow.Render();
        }

        /// <summary>
        ///     戦闘テスト
        /// </summary>
        private void SetBattleTest() {
            //戦闘テスト
            RadioButton useMapRegionSettingToggle = RootContainer.Query<RadioButton>("radioButton-battleEdit-display3");
            RadioButton useUnitSettingToggle = RootContainer.Query<RadioButton>("radioButton-battleEdit-display4");
            RadioButton useEnemyCharaToggle = RootContainer.Query<RadioButton>("radioButton-battleEdit-display5");
            RadioButton useEnemyTroopToggle = RootContainer.Query<RadioButton>("radioButton-battleEdit-display6");

            if (_sceneMapFoldout == null) _sceneMapFoldout = RootContainer.Query<VisualElement>("scene_map_foldout");

            //チェックボックスの初期値
            int defaultSelect = _testScriptable.useMapRegionSetting ? 0 : 1;
            new CommonToggleSelector().SetRadioInVisualElementSelector(
                new List<RadioButton> { useMapRegionSettingToggle, useUnitSettingToggle },
                new List<VisualElement> { _sceneMapFoldout, _sceneUnitFoldout },
                defaultSelect, new List<Action>
                {
                    () =>
                    {
                        _testScriptable.useMapRegionSetting = useMapRegionSettingToggle.value;
                        _SaveSo();
                        SetBattleTestButtonEnabled();
                    },
                    () =>
                    {
                        _testScriptable.useMapRegionSetting = useMapRegionSettingToggle.value;
                        _SaveSo();
                        SetBattleTestButtonEnabled();
                    }
                });

            useEnemyCharaToggle.value = _testScriptable.useEnemyChara;
            useEnemyTroopToggle.value = !_testScriptable.useEnemyChara;

            //マップ指定と個別指定の切り替え
            SetMapPopupField();
            SetRegionPopupField();

            // マップの選択のPopupFieldを追加。
            void SetMapPopupField() {
                var dataModels = Editor.Hierarchy.Hierarchy.mapManagementService.LoadMaps();

                var names = new List<string>() {EditorLocalize.LocalizeText("WORD_0113")};
                var ids = new List<string>() {"-1"};
                foreach (var map in dataModels.ToList())
                {
                    names.Add(map.name);
                    ids.Add(map.id);
                }

                var index = ids.Select((item, count) => (item, count))
                         .Where(pair => pair.item == _testScriptable.mapId).Select(pair => pair.count)
                         .FirstOrDefault();

                var mapId = index >= 0 ? ids[index] : string.Empty;
                if (_testScriptable.mapId != mapId)
                {
                    _testScriptable.mapId = mapId;
                    _SaveSo();
                    SetBattleTestButtonEnabled();
                }

                var popupField = new PopupFieldBase<string>(names, index);
                popupField.RegisterValueChangedCallback(evt =>
                {
                    _testScriptable.mapId = ids[popupField.index];
                    SetRegionPopupField();
                    _SaveSo();
                    SetBattleTestButtonEnabled();
                });

                var parentVe = RootContainer.Q<VisualElement>("battle_scene_map_dropdown");
                parentVe.Clear();
                parentVe.Add(popupField);
            }

            // リージョンの選択のPopupFieldを追加。
            void SetRegionPopupField() {
                var dataModels =
                    databaseManagementService.LoadEncounter().Where(e => e.mapId == _testScriptable.mapId).ToList();

                var names = new List<string>();
                foreach (var encounterDataModel in dataModels)
                {
                    if (encounterDataModel.region == 0)
                    {
                        names.Add(EditorLocalize.LocalizeText("WORD_2587"));
                    }
                    else
                    {
                        names.Add("[" + encounterDataModel.region + "]");
                    }
                }

                var index = dataModels.Any()
                    ? dataModels.Select((item, count) => (item, count))
                        .Where(pair => pair.item.region == _testScriptable.regionId).Select(pair => pair.count)
                        .FirstOrDefault()
                    : -1;

                var regionId = index >= 0 ? dataModels[index].region : -1;
                if (_testScriptable.regionId != regionId)
                {
                    _testScriptable.regionId = regionId;
                    _SaveSo();
                    SetBattleTestButtonEnabled();
                }

                PopupFieldBase<string> popupField;
                if (names.Count > 0)
                {
                    popupField = new PopupFieldBase<string>(names, index);
                    popupField.RegisterValueChangedCallback(evt =>
                    {
                        _testScriptable.regionId = dataModels[popupField.index].region;
                        _SaveSo();
                        SetBattleTestButtonEnabled();
                    });
                }
                else
                {
                    popupField =
                        new PopupFieldBase<string>(new List<string> {EditorLocalize.LocalizeText("WORD_0113")}, 0);
                }

                var parentVe = RootContainer.Q<VisualElement>("battle_scene_region_dropdown");
                parentVe.Clear();
                parentVe.Add(popupField);
            }

            //背景選択
            // プレビュー画像
            Image previewImage1 = RootContainer.Query<Image>("battle_scene_bg_top_image");
            previewImage1.scaleMode = ScaleMode.ScaleToFit;
            previewImage1.image = ImageManager.LoadBattleback1(_testScriptable.bgImageName1)?.texture;

            // 画像名
            Label imageNameLabel1 = RootContainer.Query<Label>("battle_scene_bg_top_image_name");
            imageNameLabel1.text = ImageManager.GetBattlebackName(_testScriptable.bgImageName1, 1) + ".png";

            // 画像変更ボタン
            Button changeButton1 = RootContainer.Query<Button>("battle_scene_bg_top_image_change");
            changeButton1.clicked += () =>
            {
                var selectModalWindow = new ImageSelectModalWindow(PathManager.BATTLE_BACKGROUND_1, true);
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Image"), data =>
                {
                    var imageName = (string) data;
                    _testScriptable.bgImageName1 = imageName;
                    imageNameLabel1.text = ImageManager.GetBattlebackName(imageName, 1) + ".png";
                    previewImage1.image = ImageManager.LoadBattleback1(_testScriptable.bgImageName1).texture;
                    _SaveSo();
                    SetBattleTestButtonEnabled();
                }, _testScriptable.bgImageName1);
            };

            // 背景画像インポート
            Button importButton1 = RootContainer.Query<Button>("battle_scene_bg_top_image_import");
            importButton1.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("png", PathManager.BATTLE_BACKGROUND_1);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    _testScriptable.bgImageName1 = path;
                    imageNameLabel1.text = ImageManager.GetBattlebackName(path, 1) + ".png";
                    previewImage1.image = ImageManager.LoadBattleback1(_testScriptable.bgImageName1).texture;
                    _SaveSo();
                    SetBattleTestButtonEnabled();
                    Refresh();
                }
            };

            // 背景画像（下）設定
            //------------------------------------------------------------------------------------------------------------------------------            
            // プレビュー画像
            Image previewImage2 = RootContainer.Query<Image>("battle_scene_bg_bottom_image");
            previewImage2.scaleMode = ScaleMode.ScaleToFit;
            previewImage2.image = ImageManager.LoadBattleback2(_testScriptable.bgImageName2)?.texture;

            // 画像名
            Label imageNameLabel2 = RootContainer.Query<Label>("battle_scene_bg_bottom_image_name");
            imageNameLabel2.text = ImageManager.GetBattlebackName(_testScriptable.bgImageName2, 2) + ".png";

            // 画像変更ボタン
            Button changeButton2 = RootContainer.Query<Button>("battle_scene_bg_bottom_image_change");
            changeButton2.clicked += () =>
            {
                var selectModalWindow = new ImageSelectModalWindow(PathManager.BATTLE_BACKGROUND_2, true);
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Image"), data =>
                {
                    var imageName = (string) data;
                    _testScriptable.bgImageName2 = imageName;
                    imageNameLabel2.text = ImageManager.GetBattlebackName(imageName, 2) + ".png";
                    previewImage2.image = ImageManager.LoadBattleback2(_testScriptable.bgImageName2).texture;
                    _SaveSo();
                    SetBattleTestButtonEnabled();
                }, _testScriptable.bgImageName2);
            };

            // 背景画像インポート
            Button importButton2 = RootContainer.Query<Button>("battle_scene_bg_bottom_image_import");
            importButton2.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("png", PathManager.BATTLE_BACKGROUND_2);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    _testScriptable.bgImageName2 = path;
                    imageNameLabel2.text = ImageManager.GetBattlebackName(path, 2) + ".png";
                    previewImage2.image = ImageManager.LoadBattleback2(_testScriptable.bgImageName2).texture;
                    _SaveSo();
                    SetBattleTestButtonEnabled();
                    Refresh();
                }
            };

            //敵キャラと敵グループの切り替え
            var enemyType1LabelDropdown = RootContainer.Q<VisualElement>("battle_scene_enemy_label_select_dropdown");
            var enemyType2LabelDropdown =
                RootContainer.Q<VisualElement>("battle_scene_enemy_group_label_select_dropdown");
            RefreshEnemyTypeToggle();

            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {useEnemyCharaToggle, useEnemyTroopToggle},
                _testScriptable.useEnemyChara ? 0 : 1, new List<Action>
                {
                    //敵キャラ
                    () =>
                    {
                        _testScriptable.useEnemyChara = true;
                        RefreshEnemyTypeToggle();
                        _SaveSo();
                        SetBattleTestButtonEnabled();
                    },
                    //敵グループ
                    () =>
                    {
                        _testScriptable.useEnemyChara = false;
                        RefreshEnemyTypeToggle();
                        _SaveSo();
                        SetBattleTestButtonEnabled();
                    }
                    
                });

            void RefreshEnemyTypeToggle() {
                useEnemyCharaToggle.value = _testScriptable.useEnemyChara;
                enemyType1LabelDropdown.SetEnabled(useEnemyCharaToggle.value);
                RootContainer.Q<VisualElement>("battle_scene_enemy_select_dropdown").SetEnabled(useEnemyCharaToggle.value);

                useEnemyTroopToggle.value = !_testScriptable.useEnemyChara;
                enemyType2LabelDropdown.SetEnabled(useEnemyTroopToggle.value);
            }

            _enemyDataModels = databaseManagementService.LoadEnemy();

            // 敵キャラ追加ボタン。
            {
                _enemyAddButton?.Clear();
                _enemyAddButton = RootContainer.Q<Button>("enemy_add_button");
                _enemyAddButton.clicked += () =>
                {
                    //_battleScene.viewTypeは0がフロント
                    if (
                        _battleScene.viewType == 0 && _testScriptable.enemyIds.Count < FrontEnemyMax ||
                        _battleScene.viewType == 1 && _testScriptable.enemyIds.Count < SideEnemyMax
                    )
                    {
                        _testScriptable.enemyIds.Add("-1");
                        SetEnemies();
                        _SaveSo();
                    }
                };
            }

            SetEnemies();
            SetTroop();

            _actorParentVes = new VisualElement[]
            {
                RootContainer.Query<VisualElement>("battle_scene_map_actor_list_area"),
                RootContainer.Query<VisualElement>("battle_scene_unit_actor_list_area")
            };

            if (actorAddButtons != null)
            {
                foreach (var actorAddButton in actorAddButtons)
                {
                    actorAddButton.clicked += null;
                    actorAddButton.Clear();
                }
            }
            actorAddButtons = null;
            
            actorAddButtons = new Button[]
            {
                RootContainer.Query<Button>("battle_scene_map_actor_add"),
                RootContainer.Query<Button>("battle_scene_unit_actor_add")
            };

            foreach (BattleTestScriptableObject.TestType testType in
                Enum.GetValues(typeof(BattleTestScriptableObject.TestType)))
            {
                // 「アクター追加」ボタン。
                var actorAddButton = actorAddButtons[(int) testType];

                actorAddButton.SetEnabled(BattleTestScriptableObject.Actor.IsCreatable(databaseManagementService));
                actorAddButton.SetEnabled(_testScriptable.testTypedActors[(int) testType].Count < 4);
                // 「アクター追加」ボタンクリック時の動作。
                actorAddButton.clicked += () =>
                {
                    var actor = BattleTestScriptableObject.Actor.CreateDefault(databaseManagementService);
                    AddActorFoldout(testType, actor, true);
                    _SaveSo();
                    actorAddButton.SetEnabled(_testScriptable.testTypedActors[(int) testType].Count < 4);
                    SetBattleTestButtonEnabled();
                };

                // アクターFoldout列を設定。
                SetTestTypedActorFoldouts(testType);
            }
        }

        private void SetEnemyAddButtonEnabled() {
            if (_enemyAddButton != null)
                _enemyAddButton.SetEnabled(
                    _enemyDataModels.Count > 0 &&
                    !(_battleScene.viewType == 0 && _testScriptable.enemyIds.Count >= MaxEnemyCountOfFront));
        }

        //敵キャラ選択ドロップダウン
        private void SetEnemies() {
            var dataModels = _enemyDataModels;

            var names = new List<string>() {EditorLocalize.LocalizeText("WORD_0113")};
            var ids =  new List<string>() {"-1"};
            foreach (var enemy in dataModels)
            {
                names.Add(enemy.name);
                ids.Add(enemy.id);
            }

            var parentVe = RootContainer.Q<VisualElement>("battle_scene_enemy_select_dropdown");
            parentVe.Clear();

            for (var enemyIndex = 0; enemyIndex < _testScriptable.enemyIds.Count; enemyIndex++)
            {
                var ve = new VisualElement();
                ve.style.flexDirection = FlexDirection.Row;

                var index =
                    ids.Select((item, count) => (item, count))
                        .Where(pair => pair.item == _testScriptable.enemyIds[enemyIndex]).Select(pair => pair.count)
                        .FirstOrDefault();

                // ラムダ式から現在値をキャプチャできるよう新規変数に代入。
                var capturedEnemyIndex = enemyIndex;

                var popupField = new PopupFieldBase<string>(names, index);
                popupField.RegisterValueChangedCallback(evt =>
                {
                    _testScriptable.enemyIds[capturedEnemyIndex] = ids[popupField.index];
                    _SaveSo();
                    SetBattleTestButtonEnabled();
                });

                var button = new Button {text = EditorLocalize.LocalizeText("WORD_0383")};
                button.AddToClassList("small");
                button.clicked += () =>
                {
                    _testScriptable.enemyIds.RemoveAt(capturedEnemyIndex);
                    SetEnemies();
                    _SaveSo();
                    SetBattleTestButtonEnabled();
                };

                ve.Add(popupField);
                ve.Add(button);

                parentVe.Add(ve);
            }

            SetEnemyAddButtonEnabled();
        }

        //敵グループ選択ドロップダウン
        private void SetTroop() {
            var dataModels = databaseManagementService.LoadTroop();

            var names = new List<string>() {EditorLocalize.LocalizeText("WORD_0113")};
            var ids =  new List<string>() {"-1"};
            foreach (var troop in dataModels)
            {
                names.Add(troop.name);
                ids.Add(troop.id);
            }

            var index =
                ids.Select((item, count) => (item, count)).Where(pair => pair.item == _testScriptable.troopId)
                    .Select(pair => pair.count).FirstOrDefault();

            var popupField = new PopupFieldBase<string>(names, index);
            popupField.RegisterValueChangedCallback(evt =>
            {
                _testScriptable.troopId = ids[popupField.index];
                _SaveSo();
                SetBattleTestButtonEnabled();
            });

            var parentVe = RootContainer.Q<VisualElement>("battle_scene_enemy_group_select_dropdown");
            parentVe.Clear();
            parentVe.Add(popupField);
        }

        private void SetTestTypedActorFoldouts(BattleTestScriptableObject.TestType testType) {
            _actorParentVes[(int) testType].Clear();
            for (int i = 0; i < _testScriptable.testTypedActors[(int) testType].Count; i++) 
            {
                //アクターが存在するかどうかを確認し、存在した場合にのみ追加処理を実施
                var actor = _testScriptable.testTypedActors[(int) testType][i];
                var actorDataModels = databaseManagementService.LoadCharacterActor();
                for (int j = 0; j < actorDataModels.Count; j++)
                {
                    if (actorDataModels[j].uuId == actor.id)
                    {
                        AddActorFoldout(testType, actor);
                        return;
                    }
                }

                //アクターが存在しない場合には、リストから削除する
                _testScriptable.testTypedActors[(int) testType].RemoveAt(i);
                i--;
            }
        }

        private void AddActorFoldout(
            BattleTestScriptableObject.TestType testType,
            BattleTestScriptableObject.Actor actor,
            bool isAdd = false
        ) {
            var parentVe = _actorParentVes[(int) testType];
            var actorIndex = parentVe.childCount;

            DebugUtil.Assert(_testScriptable.testTypedActors[(int) testType][actorIndex] == actor);

            var foldout = new Foldout
            {
                text = EditorLocalize.LocalizeText("WORD_0301") + " " + (actorIndex + 1)
            };
            foldout.AddToClassList("foldout_transparent");
            foldout.name = "foldout_actorfoldout_" + (testType == BattleTestScriptableObject.TestType.Map ? "map_" : "unit_") + (actorIndex + 1);
            
            parentVe.Add(foldout);

            SetActorInFoldout(foldout, actor, testType, isAdd);
        }

        // アクター情報UIをフォールドアウト内に設定。
        private void SetActorInFoldout(VisualElement parentVe, BattleTestScriptableObject.Actor actor, BattleTestScriptableObject.TestType testType, bool isAdd) {
            parentVe.Clear();

            var equipTypes = databaseManagementService.LoadSystem().equipTypes;
            var weaponTypes = databaseManagementService.LoadSystem().weaponTypes;
            var armorTypes = databaseManagementService.LoadSystem().armorTypes;
            var weaponDataModels = databaseManagementService.LoadWeapon();
            var armorDataModels = databaseManagementService.LoadArmor();

            var actorDataModels = databaseManagementService.LoadCharacterActor();
            var classDataModels = databaseManagementService.LoadCharacterActorClass();
            List<CharacterActorDataModel> actorDataModelsWork = new List<CharacterActorDataModel>();
            //なし用のデータを作成
            var dummyActor = CharacterActorDataModel.CreateDefault("-1", EditorLocalize.LocalizeText("WORD_0113"), 0);
            dummyActor.basic.classId = classDataModels.First().id;
            dummyActor.equips = new List<CharacterActorDataModel.Equipment>();
            for (int i = 0; i < DataManager.Self().GetSystemDataModel().equipTypes.Count; i++)
            {
                var dataWork = new CharacterActorDataModel.Equipment(DataManager.Self().GetSystemDataModel().equipTypes[i].id, "");
                dummyActor.equips.Add(dataWork);
            }
            actorDataModelsWork.Add(dummyActor);
            for (int i = 0; i < actorDataModels.Count; i++)
            {
                if (actorDataModels[i].charaType == (int) ActorTypeEnum.ACTOR)
                    actorDataModelsWork.Add(actorDataModels[i]);
            }
            var actorDataModel = actorDataModelsWork.ForceSingleOrDefault(adm => adm.uuId == actor.id);
            var classDataModel = classDataModels.ForceSingleOrDefault(cdm => cdm.id == actorDataModel.basic.classId);

            //職業が存在しなかった場合には、職業の先頭を指定
            if (classDataModel == null)
            {
                classDataModel = classDataModels[0];
            }

            // アクターのPopupField。
            {
                var names = new List<string>();
                foreach (var actorName in actorDataModelsWork.Select(e => e.basic.name).ToList())
                {
                    names.Add(actorName);
                }

                var testTypedActor = _testScriptable.testTypedActors[(int) testType];
                var selectedActors = new List<string>();
                foreach (var act in testTypedActor)
                {
                    //なしは除外する
                    if (act.id != "-1")
                    {
                        CharacterActorDataModel actWork = null;
                        for (int i = 0; i < actorDataModelsWork.Count; i++)
                            if (actorDataModelsWork[i].uuId == act.id)
                            {
                                actWork = actorDataModelsWork[i];
                                break;
                            }

                        if (actWork == null) return;
                        selectedActors.Add(actWork.basic.name);
                    }
                }
                

                var index = actorDataModelsWork.Select((item, count) => (item, count))
                    .Where(pair => pair.item.uuId == actor.id).Select(pair => pair.count).FirstOrDefault();

                if (isAdd)
                {
                    index = 0;
                    actor.id = "-1";
                    _testScriptable.testTypedActors[(int) testType].Add(actor);
                }

                var popupField = new PopupFieldBase<string>(names, index, null, null, 0, selectedActors);
                _actorsFieldBase.Add(popupField);
                actor.id = actorDataModelsWork[index].uuId;
                popupField.RegisterValueChangedCallback(evt =>
                {
                    actor.id = actorDataModelsWork[popupField.index].uuId;
                    for (int i = 0; i < actor.EquipIds.Length; i++)
                    {
                        actor.SetEquipId(i, "");
                    }
                    
                    SetActorInFoldout(parentVe, actor, testType, false);
                    _SaveSo();
                    SetBattleTestButtonEnabled();
                    selectedActors = new List<string>();
                    foreach (var act in testTypedActor)
                    {
                        //なしは除外する
                        if (act.id != "-1")
                        {
                            CharacterActorDataModel actWork = null;
                            for (int i = 0; i < actorDataModelsWork.Count; i++)
                                if (actorDataModelsWork[i].uuId == act.id)
                                {
                                    actWork = actorDataModelsWork[i];
                                    break;
                                }
                            selectedActors.Add(actWork.basic.name);
                        }
                    }

                    //名前リスト更新
                    foreach (var fieldBase in _actorsFieldBase)
                    {
                        fieldBase.RefreshChoices(names, selectedActors);
                    }
                });
                //名前リスト更新
                foreach (var fieldBase in _actorsFieldBase)
                {
                    fieldBase.RefreshChoices(names, selectedActors);
                }


                parentVe.Clear();
                parentVe.Add(popupField);
            }

            Action updateStateValues;

            // ステータス。

            var statusFoldout = new Foldout {text = EditorLocalize.LocalizeText("WORD_0071")};
            statusFoldout.AddToClassList("foldout_transparent");

            var parameter = classDataModel.parameter;
            var stateNameValues = new[]
            {
                ("WORD_0133", parameter.maxHp),
                ("WORD_0135", parameter.maxMp),
                ("WORD_0177", parameter.attack),
                ("WORD_0178", parameter.defense),
                ("WORD_0179", parameter.magicAttack),
                ("WORD_0180", parameter.magicDefense),
                ("WORD_0181", parameter.speed),
                ("WORD_0182", parameter.luck)
            };

            // ステータス表示を追加。
            Dictionary<string, Label> statusLabel = new Dictionary<string, Label>();
            foreach (var (name, values) in stateNameValues)
            {
                var stateVe = new InspectorItemUnit();
                statusFoldout.Add(stateVe);
                stateVe.style.flexDirection = FlexDirection.Row;

                var nameLebel = new Label {text = EditorLocalize.LocalizeText(name)};
                stateVe.Add(nameLebel);
                nameLebel.style.marginLeft = 10;
                nameLebel.style.minWidth = 50;

                var valueLebel = new Label {name = "state_value", text = GetStateValue(values).ToString()};
                stateVe.Add(valueLebel);
                statusLabel.Add(name, valueLebel);
            }

            // 各ステート値を更新。
            updateStateValues = () =>
            {
                foreach (var (name, values) in stateNameValues)
                {
                    bool flg = false;
                    int[] addparams = new int[9];
                    for (var equipIndex = 0; equipIndex < equipTypes.Count; equipIndex++)
                    {
                        flg = false;
                        string equipId = actor.GetEquipId(equipIndex);
                        for (int i = 0; i < weaponDataModels.Count; i++)
                            if (weaponDataModels[i].basic.id == equipId)
                            {
                                flg = true;
                                for (int j = 0; j < weaponDataModels[i].parameters.Count && j < addparams.Length; j++)
                                    addparams[j] += weaponDataModels[i].parameters[j];
                                break;
                            }

                        if (!flg)
                            for (int i = 0; i < armorDataModels.Count; i++)
                                if (armorDataModels[i].basic.id == equipId)
                                {
                                    for (int j = 0; j < armorDataModels[i].parameters.Count && j < addparams.Length; j++)
                                    {
                                        addparams[j] += armorDataModels[i].parameters[j];
                                    }
                                    break;
                                }
                    }

                    int param = 0;
                    switch (name)
                    {
                        case "WORD_0133":
                            param = GetStateValue(values) + addparams[0];
                            break;
                        case "WORD_0135":
                            param = GetStateValue(values) + addparams[1];
                            break;
                        case "WORD_0177":
                            param = GetStateValue(values) + addparams[2];
                            break;
                        case "WORD_0178":
                            param = GetStateValue(values) + addparams[3];
                            break;
                        case "WORD_0179":
                            param = GetStateValue(values) + addparams[4];
                            break;
                        case "WORD_0180":
                            param = GetStateValue(values) + addparams[5];
                            break;
                        case "WORD_0181":
                            param = GetStateValue(values) + addparams[6];
                            break;
                        case "WORD_0182":
                            param = GetStateValue(values) + addparams[7];
                            break;
                    }

                    statusLabel[name].text = param.ToString();
                }
            };

            // ステート値を取得。
            int GetStateValue(List<int> values) {
                //「なし」だったらステータスを0にする
                if (actor.id == "-1")
                {
                    return values[0];
                }
                return values[CSharpUtil.ClampIndex(values, actor.level)];
            }


            // 装備。
            {
                var equipsFoldoutParent = new Foldout {text = EditorLocalize.LocalizeText("WORD_0070")};
                equipsFoldoutParent.AddToClassList("foldout_transparent");

                VisualElement equipsFoldout = new VisualElement();
                equipsFoldout.SetEnabled(actor.id != "-1");
                equipsFoldoutParent.Add(equipsFoldout);

                parentVe.Add(equipsFoldoutParent);

                //初期装備
                //職業の防具タイプを取得
                //防具を取得
                //そのうえで装備タイプが一緒だった箇所に初期装備として表示させる
                var classWeaponDataModels = new List<WeaponDataModel>();
                foreach (var classWeaponTypeId in classDataModel.weaponTypes)
                foreach (var weaponDataModel in weaponDataModels)
                    if (weaponDataModel.basic.weaponTypeId == classWeaponTypeId)
                        classWeaponDataModels.Add(weaponDataModel);

                var classArmorDataModels = new List<ArmorDataModel>();
                foreach (var classArmorTypeId in classDataModel.armorTypes)
                foreach (var armorDataModel in armorDataModels)
                    if (armorDataModel.basic.armorTypeId == classArmorTypeId)
                        classArmorDataModels.Add(armorDataModel);

                var choiceEquips = new List<(string id, string name)>
                    {("", EditorLocalize.LocalizeText("WORD_0113"))};
                var choiceArmorEquips = new List<(string id, string name)>
                    {("", EditorLocalize.LocalizeText("WORD_0113"))};

                for (var equipIndex = 0; equipIndex < equipTypes.Count; equipIndex++)
                {
                    var ve = new InspectorItemUnit();
                    equipsFoldout.Add(ve);
                    ve.style.flexDirection = FlexDirection.Row;

                    var equipTypeNames = new List<string>();

                    if (equipIndex == 0)
                    {
                        var initId = "";

                        foreach (var weaponType in weaponTypes)
                            if (initId == weaponType.id)
                            {
                                equipTypeNames.Add(weaponType.value);
                                break;
                            }

                        foreach (var classWeaponTypeId in classDataModel.weaponTypes)
                        foreach (var weaponType in weaponTypes)
                            if (classWeaponTypeId == weaponType.id)
                                equipTypeNames.Add(weaponType.value);

                        foreach (var weaponDataModel in classWeaponDataModels)
                            choiceEquips.Add((weaponDataModel.basic.id, weaponDataModel.basic.name));
                    }
                    else
                    {
                        for (var j = 0; j < classArmorDataModels.Count; j++)
                            if (equipTypes[equipIndex].id == classArmorDataModels[j].basic.equipmentTypeId)
                                if (j < armorTypes.Count)
                                {
                                    equipTypeNames.Add(armorTypes[j].name);
                                    choiceArmorEquips.Add((classArmorDataModels[j].basic.id,
                                        classArmorDataModels[j].basic.name));
                                }
                    }

                    // 装備部位名ラベル。
                    var equipPartNameLabel = new Label(equipTypes[equipIndex].name);
                    equipPartNameLabel.style.marginLeft = 10;
                    equipPartNameLabel.style.minWidth = 50;
                    equipPartNameLabel.style.paddingTop = 3;
                    if (equipTypeNames.Any()) equipPartNameLabel.tooltip = string.Join(",", equipTypeNames);

                    var index =
                        choiceEquips.Select((item, index) => (item, index))
                            .Where(piar => piar.item.id == actor.GetEquipId(equipIndex)).Select(piar => piar.index)
                            .FirstOrDefault();

                    // ラムダ式から現在値をキャプチャできるよう新規変数に代入。
                    var capturedEquipIndex = equipIndex;

                    var equipPopupField = new PopupFieldBase<string>(choiceEquips.Select(e => e.name).ToList(), index);
                    equipPopupField.RegisterValueChangedCallback(evt =>
                    {
                        actor.SetEquipId(capturedEquipIndex, choiceEquips[equipPopupField.index].id);
                        _SaveSo();
                        updateStateValues();
                        SetBattleTestButtonEnabled();
                    });
                }

                //ActorDataを取得する
                //このデータ自体には変更を加えないため、以降はCloneしたデータを利用する
                CharacterActorDataModel actorData = null;
                for (int i = 0; i < actorDataModelsWork.Count; i++)
                    if (actorDataModelsWork[i].uuId == actor.id)
                        actorData = actorDataModelsWork[i].DataClone<CharacterActorDataModel>();
                var classData = classDataModels;
                var classIndex =
                    classData.IndexOf(classData.FirstOrDefault(data => data.id == actorData.basic.classId));

                //職業が存在しなかった場合には、先頭の職業を指定
                if (classIndex < 0)
                    classIndex = 0;

                var count = 0;
                if (actor.EquipIds.Length == 0)
                {
                    foreach (var equip in actorData.equips)
                    {
                        actor.SetEquipId(count, equip.value);
                        count++;
                    }
                }
                else
                {
                    
                    foreach (var equipId in actor.EquipIds)
                    {
                        actor.SetEquipId(count, equipId);
                        count++;
                    }
                }

                updateStateValues();

                //初期装備
                //職業の防具タイプを取得
                //防具を取得
                //そのうえで装備タイプが一緒だった箇所に初期装備として表示させる
                var wList = databaseManagementService.LoadWeapon();
                var weaponList = new List<WeaponDataModel>();

                foreach (var cw in classData[classIndex].weaponTypes)
                foreach (var w in wList)
                    if (w.basic.weaponTypeId == cw)
                        weaponList.Add(w);


                var aList = databaseManagementService.LoadArmor();
                var armorList = new List<ArmorDataModel>();
                foreach (var ca in classData[classIndex].armorTypes)
                foreach (var l in aList)
                    if (l.basic.armorTypeId == ca)
                        armorList.Add(l);

                if (actorData.equips.Count == 0) actorData.equips = new List<CharacterActorDataModel.Equipment>();

                var data = new List<CharacterActorDataModel.Equipment>();
                for (var i = 0; i < equipTypes.Count; i++)
                {
                    var dataWork = new CharacterActorDataModel.Equipment(equipTypes[i].id, "");
                    for (var j = 0; j < actorData.equips.Count; j++)
                        if (equipTypes[i].id == actorData.equips[j].type)
                        {
                            dataWork.value = actorData.equips[j].value;
                            break;
                        }

                    data.Add(dataWork);
                }

                if (actor.EquipIds.Length == 0) actorData.equips = data;

                //初期武器設定
                VisualElement weaponElement = new InspectorItemUnit();
                var la = new Label(equipTypes[0].name);
                weaponElement.Add(la);
                var strList = new List<string>();
                strList.Add(EditorLocalize.LocalizeText("WORD_0113"));
                var defaultIndex = -1;
                for (var j = 0; j < weaponList.Count; j++) strList.Add(weaponList[j].basic.name);
                WeaponDataModel weaponData = null;
                try
                {
                    for (int i = 0; i < weaponList.Count; i++)
                        if (weaponList[i].basic.id == actor.GetEquipId(0))
                        {
                            weaponData = weaponList[i];
                            break;
                        }
                    defaultIndex = strList.IndexOf(weaponData.basic.name);
                }
                catch (Exception)
                {
                }

                if (defaultIndex < 0)
                {
                    //このケースでは、職業を変更した等の理由で、装備不可能なものが選択されているため、初期化する
                    defaultIndex = 0;
                    actorData.equips[0].value = "";
                }

                var weaponPopupField = new PopupFieldBase<string>(strList, defaultIndex);
                weaponElement.Add(weaponPopupField);
                weaponPopupField.RegisterValueChangedCallback(evt =>
                {
                    WeaponDataModel weaponData = null;
                    for (int i = 0; i < weaponList.Count; i++)
                        if (weaponList[i].basic.name == weaponPopupField.value)
                        {
                            weaponData = weaponList[i];
                            break;
                        }
                    if (weaponData != null)
                    {
                        actor.SetEquipId(0, choiceEquips[weaponPopupField.index].id);
                    }
                    else
                    {
                        //初期装備(武器)無しの時
                        actor.SetEquipId(0, choiceEquips[weaponPopupField.index].id);
                    }

                    _SaveSo();
                    updateStateValues();
                });

                equipsFoldout.Add(weaponElement);


                //初期防具設定
                for (var i = 1; i < equipTypes.Count; i++)
                {
                    VisualElement element = new InspectorItemUnit();
                    la = new Label(equipTypes[i].name);
                    element.Add(la);
                    strList = new List<string>();
                    var aromorIds = new List<string>() {"-1"};
                    strList.Add(EditorLocalize.LocalizeText("WORD_0113"));
                    for (var j = 0; j < armorList.Count; j++)
                    {
                        if (equipTypes[i].id == armorList[j].basic.equipmentTypeId)
                        {
                            strList.Add(armorList[j].basic.name);
                            aromorIds.Add(armorList[j].basic.id);
                        }
                    }

                    ArmorDataModel armorData = null;
                    defaultIndex = -1;
                    for (int i2 = 0; i2 < armorList.Count; i2++)
                        if (armorList[i2].basic.id == actor.GetEquipId(i))
                        {
                            armorData = armorList[i2];
                            break;
                        }
                    defaultIndex = strList.IndexOf(armorData?.basic.name);

                    if (defaultIndex < 0)
                    {
                        //このケースでは、職業を変更した等の理由で、装備不可能なものが選択されているため、初期化する
                        defaultIndex = 0;
                        actorData.equips[i].value = "";
                    }

                    var armorPopupField = new PopupFieldBase<string>(strList, defaultIndex);
                    element.Add(armorPopupField);
                    var index = i;
                    armorPopupField.RegisterValueChangedCallback(evt =>
                    {
                        ArmorDataModel armorDataWork = null;
                        for (int i2 = 0; i2 < armorList.Count; i2++)
                            if (armorList[i2].basic.name == armorPopupField.value)
                            {
                                armorDataWork = armorList[i2];
                                break;
                            }

                        if (armorDataWork != null)
                        {
                            actor.SetEquipId(index, aromorIds[armorPopupField.index]);
                        }
                        else
                        {
                            //初期装備(防具)無しの時
                            actor.SetEquipId(index, aromorIds[armorPopupField.index]);
                        }

                        _SaveSo();
                        updateStateValues();
                    });
                    equipsFoldout.Add(element);
                }
            }

            // レベル。
            {
                actor.level = CSharpUtil.Clamp(actor.level, actorDataModel.initialLevel, actorDataModel.maxLevel);
                var levelContainer = new InspectorItemUnit();

                var level = new IntegerField
                {
                    maxLength = 3,
                    value = actor.level
                };
                level.SetEnabled(actor.id != "-1");

                level.RegisterCallback<FocusOutEvent>(evt =>
                {
                    level.value =
                        CSharpUtil.Clamp(level.value, actorDataModel.initialLevel, actorDataModel.maxLevel);
                    actor.level = level.value;
                    updateStateValues();
                    _SaveSo();
                    SetBattleTestButtonEnabled();
                });

                levelContainer.Add(new Label {text = EditorLocalize.LocalizeText("WORD_0139")});
                levelContainer.Add(level);

                parentVe.Add(levelContainer);
            }
            
            //最後にステータスを表示
            parentVe.Add(statusFoldout);
            
            //削除
            var deleteButton = new Button();
            deleteButton.text = EditorLocalize.LocalizeText("WORD_0383");
            deleteButton.clicked += () =>
            {
                _testScriptable.testTypedActors[(int) testType].Remove(actor);
                SetTestTypedActorFoldouts(testType);
                _SaveSo();
                SetBattleTestButtonEnabled();
                actorAddButtons[(int)testType].SetEnabled(_testScriptable.testTypedActors[(int) testType].Count < 4);
            };
            parentVe.Add(deleteButton);
        }

        override protected void SaveContents() {
            //セーブ部位の作成
            _systemData.battleScene = _battleScene;
            databaseManagementService.SaveSystem(_systemData);
        }

        private void _SaveSo() {
            var testScriptable = ScriptableObject.CreateInstance<BattleTestScriptableObject>();
            testScriptable.useMapRegionSetting = _testScriptable.useMapRegionSetting;
            testScriptable.mapId = _testScriptable.mapId;
            testScriptable.regionId = _testScriptable.regionId;
            testScriptable.bgImageName1 = _testScriptable.bgImageName1;
            testScriptable.bgImageName2 = _testScriptable.bgImageName2;
            testScriptable.useEnemyChara = _testScriptable.useEnemyChara;
            testScriptable.enemyIds = _testScriptable.enemyIds;
            testScriptable.troopId = _testScriptable.troopId;
            testScriptable.testTypedActors = _testScriptable.testTypedActors;
            AssetDatabase.CreateAsset(testScriptable, SO_PATH);
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
        }

        /// <summary>
        /// 戦闘テストボタンの状態
        /// </summary>
        private void SetBattleTestButtonEnabled() {
            
            var map = true;
            var enemies = true;
            var troop = true;

            if (_testScriptable.useMapRegionSetting)
            {
                map = false;
                var encounterDataModel = databaseManagementService.LoadEncounter().ForceSingleOrDefault(e =>
                    e.mapId == _testScriptable.mapId && e.region == _testScriptable.regionId);

                //エンカウントのなかの敵が設定されているか
                if (encounterDataModel != null)
                {
                    var enemy = false;
                    for (int i = 0; i < encounterDataModel.enemyList.Count; i++)
                    {
                        if (encounterDataModel.enemyList[i].enemyId != "-1")
                        {
                            enemy = true;
                            break;
                        }
                    }

                    var troopList = false;
                    for (int i = 0; i < encounterDataModel.troopList.Count; i++)
                    {
                        if (encounterDataModel.troopList[i].troopId != "-1")
                        {
                            troopList = true;
                            break;
                        }
                    }

                    map = enemy || troopList;
                }
            }
            else
            {
                if (_testScriptable.useEnemyChara)
                {
                    enemies = false;
                    for (int i = 0; i < _testScriptable.enemyIds.Count; i++)
                    {
                        if (_testScriptable.enemyIds[i] != "-1")
                        {
                            enemies = true;
                            break;
                        }
                    }
                }
                else
                {
                    troop = _testScriptable.troopId != "-1";
                }
            }

            var actors = false;
            for (int i = 0; i < _testScriptable.testTypedActors[_testScriptable.useMapRegionSetting ? 0 : 1].Count; i++)
            {
                if (_testScriptable.testTypedActors[_testScriptable.useMapRegionSetting ? 0 : 1][i].id != "-1")
                {
                    actors = true;
                    break;
                }
            }

            var enabled = map && enemies && troop && actors;
            
            _battleTestButton.SetEnabled(enabled);
        }
    }

    /// <summary>
    ///     エディタからシーン再生を制御する。
    /// </summary>
    [InitializeOnLoad]
    public static class EditorSceneController
    {
        /// <summary>
        ///     静的コンストラクタ。
        /// </summary>
        /// <remarks>
        ///     クラスのInitializeOnLoad属性により、エディター起動後に呼び出される。
        ///     遅延実行により必要なタイミングで呼び出される。
        /// </remarks>
        static EditorSceneController() {
#if EDITOR_SCENE_CONTROLLER_LOG
            DebugUtil.Log(MethodBase.GetCurrentMethod());
#endif

            using (new DebugUtil.IndentLog())
            {
#if EDITOR_SCENE_CONTROLLER_LOG
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                EditorSceneManager.sceneOpening += OnSceneOpening;
                EditorSceneManager.sceneOpened += OnSceneOpened;
                EditorSceneManager.sceneUnloaded += OnSceneUnloaded;
                EditorSceneManager.sceneClosing += OnSceneClosing;
                EditorSceneManager.sceneClosed += OnSceneClosed;
#endif

                BattleSceneTest.ScenePlayEndEvent += OnScenePlayEnd;
                SceneBattle.BattleTest.ScenePlayEndEvent += OnScenePlayEnd;
            }
        }

        /// <summary>
        ///     戦闘テストシーンを再生する。
        /// </summary>
        /// <param name="sceneAssertFilePath">シーンアセットファイルパス</param>
        public static void PlayBattleTestScene(string sceneAssertFilePath, BattleTestScriptableObject battleTestSo) {
#if EDITOR_SCENE_CONTROLLER_LOG
            DebugUtil.Log(MethodBase.GetCurrentMethod(), sceneAssertFilePath, battleTestSo);
#endif

            using (new DebugUtil.IndentLog())
            {
#if !USE_TEST_BATTLE_SCENE
                var databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;
                var encounterDataModel = EncounterDataModel.CreateDefault(string.Empty);

                if (battleTestSo.useMapRegionSetting)
                {
                    //マップ指定。
                    encounterDataModel = databaseManagementService.LoadEncounter().ForceSingleOrDefault(e =>
                        e.mapId == battleTestSo.mapId && e.region == battleTestSo.regionId);
                }
                else
                {
                    // 個別指定。
                    encounterDataModel.backImage1 = battleTestSo.bgImageName1;
                    encounterDataModel.backImage2 = battleTestSo.bgImageName2;

                    if (battleTestSo.useEnemyChara)
                        // 敵キャラ。
                        encounterDataModel.enemyList = battleTestSo.enemyIds
                            .Select(enemyId => new EncounterDataModel.Enemy(enemyId, 1, 1)).ToList();
                    else
                        // 敵グループ。
                        encounterDataModel.troopList = new List<EncounterDataModel.Troop>
                        {
                            new EncounterDataModel.Troop(battleTestSo.troopId, 1)
                        };
                }
                
                //「なし」がいたら抜く
                List<EncounterDataModel.Enemy> enemies = new List<EncounterDataModel.Enemy>();
                for (int i = 0; i < encounterDataModel.enemyList.Count; i++)
                {
                    bool flg = encounterDataModel.enemyList[i].enemyId == "-1";
                    if (!flg)
                    {
                        enemies.Add(encounterDataModel.enemyList[i]);
                    }
                }

                encounterDataModel.enemyList = enemies;

                BattleSceneTransition.Initialize();

                // エンカウンター情報を設定。
                BattleSceneTransition.Instance.EncounterDataModel = encounterDataModel;

                if (battleTestSo.useMapRegionSetting)
                {
                    //背景指定
                    BattleSceneTransition.Instance.EncounterDataBackgroundImage1 = encounterDataModel.backImage1;
                    BattleSceneTransition.Instance.EncounterDataBackgroundImage2 = encounterDataModel.backImage2;
                    

                    //マップのリージョン指定の場合は、敵 or 敵グループのエンカウントの設定を行う
                    //どの敵 or 敵グループが出るかどうかの抽選処理
                    //0: 敵
                    //1: 敵グループとする
                    var encountType = 0;
                    if (encounterDataModel.enemyList.Count == 0)
                        //敵未設定のため、敵グループ確定
                        encountType = 1;
                    else if (encounterDataModel.troopList.Count == 0)
                        //敵グループ未設定のため、敵確定
                        encountType = 0;
                    else
                    {
                        //乱数で決める
                        //encountType = Random.Range(0, 2);
                        int encountTypeWork = encounterDataModel.troopPer;
                        if (encountTypeWork < 0)
                        {
                            encountTypeWork = 0;
                        }
                        else if (encountTypeWork > 100)
                        {
                            encountTypeWork = 100;
                        }
                        int encountTypeRand = UnityEngine.Random.Range(0, 101);
                        if (encountTypeRand < encountTypeWork)
                        {
                            //敵グループ
                            encountType = 1;
                        }
                        else
                        {
                            //自動マッチング
                            encountType = 0;
                        }
                    }

                    //敵グループの場合の処理
                    if (encountType == 1)
                    {
                        //敵グループごとに設定されている重みに従って処理する
                        //重みを全部足す
                        var weight = 0;
                        for (var i = 0; i < encounterDataModel.troopList.Count; i++)
                            weight += encounterDataModel.troopList[i].weight;
                        //乱数
                        var selectWeight = Random.Range(0, weight);
                        //出現する敵を決定
                        var flg = false;
                        weight = 0;
                        for (var i = 0; i < encounterDataModel.troopList.Count; i++)
                        {
                            weight += encounterDataModel.troopList[i].weight;
                            if (weight > selectWeight)
                            {
                                BattleSceneTransition.Instance.SelectTroopId = encounterDataModel.troopList[i].troopId;
                                flg = true;
                                break;
                            }
                        }

                        //上記に不備があり、決まらないことが万が一あったら先頭のものにする
                        if (!flg)
                        {
                            if (encounterDataModel.troopList.Count > 0)
                                BattleSceneTransition.Instance.SelectTroopId = encounterDataModel.troopList[0].troopId;
                            else
                                BattleSceneTransition.Instance.SelectTroopId =
                                    databaseManagementService.LoadTroop().First().id;
                        }
                    }
                    else
                    {
                        var t = databaseManagementService.CreateEnemyToTroopDataModel(TroopDataModel.TROOP_BTATLE_TEST,
                            DataManager.Self().GetSystemDataModel().battleScene.viewType, encounterDataModel);
                        BattleSceneTransition.Instance.SelectTroopId = TroopDataModel.TROOP_BTATLE_TEST;
                        var troopWork = databaseManagementService.LoadTroop()
                            .FirstOrDefault(t => t.id == TroopDataModel.TROOP_BTATLE_TEST);
                        //セーブしないとRuntime側に持っていけない
                        var troops = databaseManagementService.LoadTroop();
                        if (troopWork == null)
                        {
                            troops.Add(t);
                        }
                        else
                        {
                            for (int i = 0; i < troops.Count; i++)
                            {
                                if (troops[i].id == t.id)
                                {
                                    troops[i] = t;
                                    break;
                                }
                            }
                        }

                        databaseManagementService.SaveTroop(troops);
                    }
                }
                else
                {
                    //背景指定
                    BattleSceneTransition.Instance.EncounterDataBackgroundImage1 = battleTestSo.bgImageName1;
                    BattleSceneTransition.Instance.EncounterDataBackgroundImage2 = battleTestSo.bgImageName2;
                    
                    if (battleTestSo.useEnemyChara)
                    {
                        encounterDataModel.enemyMax = encounterDataModel.enemyList.Count;
                        var t = databaseManagementService.CreateEnemyToTroopDataModel(TroopDataModel.TROOP_BTATLE_TEST,
                            DataManager.Self().GetSystemDataModel().battleScene.viewType, encounterDataModel, false);
                        BattleSceneTransition.Instance.SelectTroopId = TroopDataModel.TROOP_BTATLE_TEST;
                        var troopWork = databaseManagementService.LoadTroop()
                            .FirstOrDefault(troop => troop.id == TroopDataModel.TROOP_BTATLE_TEST);
                        //セーブしないとRuntime側に持っていけない
                        var troops = databaseManagementService.LoadTroop();
                        if (troopWork == null)
                        {
                            troops.Add(t);
                        }
                        else
                        {
                            for (int i = 0; i < troops.Count; i++)
                            {
                                if (troops[i].id == t.id)
                                {
                                    troops[i] = t;
                                    break;
                                }
                            }
                        }
                        databaseManagementService.SaveTroop(troops);
                    }
                    else
                    {
                        BattleSceneTransition.Instance.SelectTroopId = encounterDataModel.troopList[0].troopId;
                    }
                    
                }

                // アクター列を設定。
                BattleSceneTransition.Instance.Actors =
                    battleTestSo.actors.Select(actor =>
                        new BattleSceneTransition.Actor
                        {
                            id = actor.id,
                            level = actor.level,
                            equipIds = actor.EquipIds
                        } 
                    ).ToArray();

                // 同一のアクターが居た場合には消す
                List<Actor> actors = new List<Actor>();
                for (int i = 0; i < BattleSceneTransition.Instance.Actors.Length; i++)
                {
                    bool flg = false;
                    for (int j = 0; j < actors.Count; j++)
                    {
                        if (BattleSceneTransition.Instance.Actors[i].id ==  actors[j].id)
                        {
                            flg = true;
                            break;
                        }
                    }

                    //「なし」がいたら抜く
                    if (BattleSceneTransition.Instance.Actors[i].id == "-1")
                    {
                        flg = true;
                    }
                    
                    
                    if (!flg)
                    {
                        actors.Add(BattleSceneTransition.Instance.Actors[i]);
                    }
                }
                BattleSceneTransition.Instance.Actors = actors.ToArray();
#endif

                EditorSceneManager.OpenScene(sceneAssertFilePath);
                EditorApplication.isPlaying = true;
                Application.targetFrameRate = 60;

                //InitAllWindow();
            }
        }
        private static async void InitAllWindow() {
            AssetDatabase.Refresh();
            await Task.Delay(100);
            RpgMakerEditor.InitWindows();
        }

#if EDITOR_SCENE_CONTROLLER_LOG
        private static PlayModeStateChange _playModeStateChange;

        /// <summary>
        /// 再生モード状態が変更された。
        /// </summary>
        /// <param name="state">再生モード状態</param>
        private static void OnPlayModeStateChanged(PlayModeStateChange state) {
            DebugUtil.Log(MethodBase.GetCurrentMethod(), state);

            _playModeStateChange = state;
            using (new DebugUtil.IndentLog())
            {

            }
        }

        private static void OnSceneUnloaded(Scene scene) {
            DebugUtil.Log(MethodBase.GetCurrentMethod(), scene.name);
            using (new DebugUtil.IndentLog())
            {

            }
        }

        private static void OnSceneOpening(string path, OpenSceneMode mode) {
            DebugUtil.Log(MethodBase.GetCurrentMethod(), path, mode);
            using (new DebugUtil.IndentLog())
            {
                
            }
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode) {
            DebugUtil.Log(MethodBase.GetCurrentMethod(), scene.name, mode);
            using (new DebugUtil.IndentLog())
            {
                
            }
        }

        private static void OnSceneClosing(Scene scene, bool removingScene) {
            DebugUtil.Log(MethodBase.GetCurrentMethod(), scene.name, removingScene);
            using (new DebugUtil.IndentLog())
            {

            }
        }

        private static void OnSceneClosed(Scene scene) {
            DebugUtil.Log(MethodBase.GetCurrentMethod(), scene.name);
            using (new DebugUtil.IndentLog())
            {

            }
        }

#endif

        /// <summary>
        ///     シーン終了通知受信。
        /// </summary>
        /// <param name="sceneName">シーン名</param>
        public static void OnScenePlayEnd(string sceneName) {
#if EDITOR_SCENE_CONTROLLER_LOG
            DebugUtil.Log(MethodBase.GetCurrentMethod(), sceneName);
#endif
            using (new DebugUtil.IndentLog())
            {
                EditorApplication.isPlaying = false;
                InitAllWindow();
            }
        }
    }
}