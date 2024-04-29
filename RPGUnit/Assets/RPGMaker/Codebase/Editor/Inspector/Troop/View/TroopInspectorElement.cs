using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Enemy;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventBattle;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Troop;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Knowledge.Misc;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using RPGMaker.Codebase.Editor.Hierarchy.Enum;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Character.View;
using RPGMaker.Codebase.Editor.MapEditor.Window.EventEdit;
using RPGMaker.Codebase.Runtime.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.Troop.View
{
    /// <summary>
    /// [バトルの編集]-[敵グループ] Inspector
    /// </summary>
    public class TroopInspectorElement : AbstractInspectorElement
    {
        private        int                     _currentSelectEventId;
        private        int                     _currentSelectId;

        private          VisualElement              _enemyGroupFrontEnemyDropdown;
        private          VisualElement              _enemyGroupSideEnemyDropdown;
        private          EventBattleDataModel       _eventBattleData;
        private          List<EventBattleDataModel> _eventBattleDataModels;
        private          EventDataModel             _eventData;

        //フロントサイドの敵グループ上限数
        private readonly int _frontEnemyMax = 8;

        /// <summary>
        ///     フロント側敵設定の箱
        /// </summary>
        private VisualElement _frontViewSwitch;

        //ヒエラルキーを触るための各々の保持
        private readonly string _id;

        //敵のSizeの判断基準用の変数
        private readonly SceneWindow _sceneWindow;

        private Foldout _frontViewFoldout;
        private List<InspectorItemUnit> _frontPositionList;

        /// <summary>
        ///     サイド側敵設定の箱
        /// </summary>
        private VisualElement _sideViewSwitch;

        // イベント用テキスト
        private readonly List<string> _spanList = EditorLocalize.LocalizeTexts(new List<string>
        {
            "WORD_0632",
            "WORD_0626",
            "WORD_0633"
        });

        private List<TroopDataModel> _troopDataModels;

        private TroopDataModel _troopList;
        private int            _viewType;

        //フロント敵グループのUXML
        private readonly string frontUxml =
            "Assets/RPGMaker/Codebase/Editor/Inspector/Troop/Asset/enemyGroup_front.uxml";

        //インスペクター枠のUXML
        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/Troop/Asset/inspector_troop.uxml"; } }

        //サイド敵グループのUXML
        private readonly string sideUxml =
            "Assets/RPGMaker/Codebase/Editor/Inspector/Troop/Asset/enemyGroup_side.uxml";

        public TroopInspectorElement(string id, BattleHierarchyView element, int eventNum = -1) {
            // 直前のバトルのエンカウントデータのバトル背景がプレビューシーン枠に表示されないように初期化しておく。
            BattleSceneTransition.Initialize();

            _id = id;
            _eventBattleData = null;
            _currentSelectEventId = eventNum;

            _sceneWindow =
                WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.DatabaseSceneWindow) as
                    SceneWindow;
            _sceneWindow.Create(SceneWindow.PreviewId.BattleScene);

            Initialize();

            InitAndRenderSceneView();
        }

        protected override void RefreshContents() {
            base.RefreshContents();
            Initialize();
        }

        override protected void InitializeContents() {
            base.InitializeContents();

            _troopDataModels = databaseManagementService.LoadTroop();
            _eventBattleDataModels = eventManagementService.LoadEventBattle();

            //ビュータイプ取得
            var systemScriptableObject = databaseManagementService.LoadSystem();
            _viewType = systemScriptableObject.battleScene.viewType;

            if (_id != "-1")
            {
                for (var i = 0; i < _troopDataModels.Count; i++)
                    if (_troopDataModels[i].id == _id)
                    {
                        _troopList = _troopDataModels[i];
                        // グループ一致時にeventを取得する
                        for (var i2 = 0; i2 < _eventBattleDataModels.Count; i2++)
                            if (_troopList.battleEventId == _eventBattleDataModels[i2].eventId)
                                _eventBattleData = _eventBattleDataModels[i2];

                        // バトルイベントの処理(-1は無視、-2は新規追加、-3はコピー)
                        // 新規追加
                        if (_currentSelectEventId == -1)
                        {
                        }
                        else if (_currentSelectEventId == -2)
                        {
                            // nullだったらここでバトルイベント追加
                            if (_eventBattleData == null)
                            {
                                // バトルイベント作成
                                var eventData = EventBattleDataModel.CreateDefault();
                                eventData.eventId = Guid.NewGuid().ToString();
                                _eventBattleData = eventData;
                                _eventBattleDataModels.Add(eventData);
                                // バトルイベントID設定
                                _troopList.battleEventId = eventData.eventId;
                            }

                            // イベントページを追加
                            _eventBattleData.pages.Add(EventBattleDataModel.CreateDefaultEventBattlePage(0));
                            var pageId = Guid.NewGuid().ToString();
                            _eventBattleData.pages[_eventBattleData.pages.Count - 1].eventId = pageId;
                            // 追加したイベントの番号を入れる
                            _currentSelectEventId = _eventBattleData.pages.Count - 1;

                            // イベント作成
                            _eventData = new EventDataModel(pageId, 0, 0, new List<EventDataModel.EventCommand>());

                            eventManagementService.SaveEvent(_eventData);
                            Save();
                        }
                        // コピペ
                        else if (_currentSelectEventId == -3 && _eventBattleData != null)
                        {
                            // 指定番号が存在する
                            if (_eventBattleData.pages.Count > _currentSelectEventId)
                            {
                                _eventBattleData.pages.Add(_eventBattleData.pages[_currentSelectEventId]);
                                Save();
                            }
                        }

                        break;
                    }
            }
            else
            {
                var troopData = new TroopDataModel();
                troopData.id = Guid.NewGuid().ToString();
                troopData.name = "#" + string.Format("{0:D4}", _troopDataModels.Count + 1);
                _troopDataModels.Add(troopData);
                _troopList = troopData;

                Save();
            }

            SetTroop();
        }

        private void InitAndRenderSceneView() {
            _sceneWindow.Init(_troopList);
            _sceneWindow.Render();
        }

        //------------------------------------------------------------------
        // イベントエディタを開く
        public static async void LaunchEventEditMode(EventDataModel eventDataModel) {
            var eventDataModelEntity = eventDataModel;
            WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.DatabaseSceneWindow);
            MapEditor.MapEditor.ChangeEvent(eventDataModelEntity);
            await Task.Delay(50);

            // 『イベントコマンド』枠を開く。
            var commandSettingWindow = (CommandSettingWindow)WindowLayoutManager.GetActiveWindow(
                WindowLayoutManager.WindowLayoutId.MapEventCommandSettingWindow);
            if (commandSettingWindow == null)
            {
                commandSettingWindow = WindowLayoutManager.OpenAndDockWindow(
                    WindowLayoutManager.WindowLayoutId.MapEventCommandSettingWindow,
                    WindowLayoutManager.WindowLayoutId.DatabaseInspectorWindow,
                    Docker.DockPosition.Bottom
                ) as CommandSettingWindow;
                commandSettingWindow.titleContent =
                    new GUIContent(EditorLocalize.LocalizeWindowTitle("WORD_1570"));
            }

            // 『イベント実行内容』枠を開く。
            // (『イベントコマンド』枠を参照しているので、『イベントコマンド』枠の後で開く)
            var executionContentsWindow = (ExecutionContentsWindow)WindowLayoutManager.GetActiveWindow(
                WindowLayoutManager.WindowLayoutId.MapEventExecutionContentsWindow);
            if (executionContentsWindow == null)
            {
                executionContentsWindow = WindowLayoutManager.OpenAndDockWindow(
                    WindowLayoutManager.WindowLayoutId.MapEventExecutionContentsWindow,
                    WindowLayoutManager.WindowLayoutId.DatabaseSceneWindow,
                    Docker.DockPosition.Bottom
                ) as ExecutionContentsWindow;
                executionContentsWindow.titleContent =
                    new GUIContent(EditorLocalize.LocalizeWindowTitle("WORD_1569"));
            }

            executionContentsWindow.Init(eventDataModelEntity, true, ExecutionContentsWindow.EventType.Battle);
        }


        private void SetTroop() {
            Label enemyGroupId = RootContainer.Query<Label>("enemyGroup_id");
            enemyGroupId.text = _troopList.SerialNumberString;

            ImTextField enemyGroupName = RootContainer.Query<ImTextField>("enemyGroup_name");
            enemyGroupName.value = _troopList.name;
            enemyGroupName.RegisterCallback<FocusOutEvent>(evt =>
            {
                _troopList.name = enemyGroupName.value;
                Save();
                UpdateData(true);
            });

            //敵の名前の自動入力
            Button enemyNameRandom = RootContainer.Query<Button>("enemy_name_random");
            enemyNameRandom.clickable.clicked += () =>
            {
                //情報取得
                var enemyScriptableObject = databaseManagementService.LoadEnemy();
                var systemScriptableObject = databaseManagementService.LoadSystem();
                var enemyList = enemyScriptableObject.ToList();

                //敵グループ内の敵の名前のリスト
                var names = new List<string>();

                //現在のビュータイプで読み込む敵グループを分岐させる
                switch (systemScriptableObject.battleScene.viewType)
                {
                    case 0:
                        //フロントビューの敵取得
                        foreach (var frontViewMember in _troopList.frontViewMembers)
                        {
                            for (int i = 0; i < enemyList.Count; i++)
                                if (enemyList[i].id == frontViewMember.enemyId)
                                {
                                    names.Add(enemyList[i].name);
                                    break;
                                }
                        }

                        break;
                    case 1:
                        //サイドビューの敵取得
                        foreach (var sideVewMembers in _troopList.sideViewMembers)
                        {
                            for (int i = 0; i < enemyList.Count; i++)
                                if (enemyList[i].id == sideVewMembers.enemyId)
                                {
                                    names.Add(enemyList[i].name);
                                    break;
                                }
                        }

                        break;
                }

                //同じ名前でまとめる
                names.Sort();
                var enemyName = "";

                //グループ内に一人だった場合そのまま表示を行う
                if (names.Count == 0)
                {
                    enemyName = EditorLocalize.LocalizeText("WORD_1518");
                }
                else if (names.Count == 1)
                {
                    enemyName = names[0];
                }
                //複数の敵がいる時の処理
                else
                {
                    //名前の被り確認用
                    var beforeName = names[0];
                    //複数いた場合の語尾の番号
                    var num = 0;

                    for (var i = 0; i < names.Count; i++)
                    {
                        if (beforeName == names[i])
                        {
                            num++;
                        }
                        else
                        {
                            //表示結果に追加
                            enemyName += beforeName + numController(num) + "、";

                            //初期化
                            beforeName = names[i];
                            num = 1;
                        }

                        //最後に最後を追加する
                        if (names.Count == i + 1)
                            //表示結果に追加
                            enemyName += beforeName + numController(num);
                    }

                    //numの表示周りの制御
                    string numController(int input) {
                        var retunString = "";
                        if (input > 1) retunString += "*" + input;

                        return retunString;
                    }
                }

                //表示更新
                enemyGroupName.value = enemyName;
                //データ更新
                _troopList.name = enemyName;
                Save();
                UpdateData(true);
            };

            // 背景画像（下）設定
            //------------------------------------------------------------------------------------------------------------------------------            
            // プレビュー画像
            Image previewImage1 = RootContainer.Query<Image>("battle_scene_bg_top_image");
            previewImage1.scaleMode = ScaleMode.ScaleToFit;
            previewImage1.image = ImageManager.LoadBattleback1(_troopList.backImage1)?.texture;

            // 画像名
            Label imageNameLabel1 = RootContainer.Query<Label>("battle_scene_bg_top_image_name");
            if (_troopList.backImage1 == "")
                imageNameLabel1.text = EditorLocalize.LocalizeText("WORD_1595");
            else
                imageNameLabel1.text = ImageManager.GetBattlebackName(_troopList.backImage1, 1) + ".png";

            // 画像変更ボタン
            Button changeButton1 = RootContainer.Query<Button>("battle_scene_bg_top_image_change");
            changeButton1.clicked += () =>
            {
                var selectModalWindow = new ImageSelectModalWindow(PathManager.BATTLE_BACKGROUND_1, true, "WORD_1595");
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Image"), data =>
                {
                    var imageName = (string) data;
                    _troopList.backImage1 = imageName;
                    if (string.IsNullOrEmpty(_troopList.backImage1))
                    {
                        //マップの設定を使用する
                        imageNameLabel1.text = EditorLocalize.LocalizeText("WORD_1595");
                        previewImage1.image = ImageManager.LoadBattleback1(_troopList.backImage1).texture;
                    }
                    else
                    {
                        imageNameLabel1.text = ImageManager.GetBattlebackName(_troopList.backImage1, 1) + ".png";
                        previewImage1.image = ImageManager.LoadBattleback1(_troopList.backImage1).texture;
                    }

                    Save();
                    InitAndRenderSceneView();
                }, _troopList.backImage1);
            };

            // 背景画像インポート
            Button importButton1 = RootContainer.Query<Button>("battle_scene_bg_top_image_import");
            importButton1.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("png", PathManager.BATTLE_BACKGROUND_1);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    _troopList.backImage1 = path;
                    imageNameLabel1.text = ImageManager.GetBattlebackName(_troopList.backImage1, 1) + ".png";
                    previewImage1.image = ImageManager.LoadBattleback1(_troopList.backImage1).texture;
                    Save();
                    InitAndRenderSceneView();
                    Refresh();
                }
            };

            // 背景画像（上）設定
            //------------------------------------------------------------------------------------------------------------------------------            
            // プレビュー画像
            Image previewImage2 = RootContainer.Query<Image>("battle_scene_bg_bottom_image");
            previewImage2.scaleMode = ScaleMode.ScaleToFit;
            previewImage2.image = ImageManager.LoadBattleback2(_troopList.backImage2)?.texture;

            // 画像名
            Label imageNameLabel2 = RootContainer.Query<Label>("battle_scene_bg_bottom_image_name");
            if (_troopList.backImage2 == "")
                imageNameLabel2.text = EditorLocalize.LocalizeText("WORD_1595");
            else
                imageNameLabel2.text = ImageManager.GetBattlebackName(_troopList.backImage2, 2) + ".png";

            // 画像変更ボタン
            Button changeButton2 = RootContainer.Query<Button>("battle_scene_bg_bottom_image_change");
            changeButton2.clicked += () =>
            {
                var selectModalWindow = new ImageSelectModalWindow(PathManager.BATTLE_BACKGROUND_2, true, "WORD_1595");
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Image"), data =>
                {
                    var imageName = (string) data;
                    _troopList.backImage2 = imageName;
                    if (string.IsNullOrEmpty(_troopList.backImage2))
                    {
                        //マップの設定を使用する
                        imageNameLabel2.text = EditorLocalize.LocalizeText("WORD_1595");
                        previewImage2.image = ImageManager.LoadBattleback2(_troopList.backImage2).texture;
                    }
                    else
                    {
                        imageNameLabel2.text = ImageManager.GetBattlebackName(_troopList.backImage2, 2) + ".png";
                        previewImage2.image = ImageManager.LoadBattleback2(_troopList.backImage2).texture;
                    }

                    Save();
                    InitAndRenderSceneView();
                }, _troopList.backImage2);
            };

            // 背景画像インポート
            Button importButton2 = RootContainer.Query<Button>("battle_scene_bg_bottom_image_import");
            importButton2.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("png", PathManager.BATTLE_BACKGROUND_2);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    _troopList.backImage2 = path;
                    imageNameLabel2.text = ImageManager.GetBattlebackName(_troopList.backImage2, 2) + ".png";
                    previewImage2.image = ImageManager.LoadBattleback2(_troopList.backImage2).texture;
                    Save();
                    InitAndRenderSceneView();
                    Refresh();
                }
            };

            //フロント、サイドの表示切替
            _frontViewSwitch = RootContainer.Query<VisualElement>("front_view_switch");
            _sideViewSwitch = RootContainer.Query<VisualElement>("side_view_switch");

            //フロントビュー、サイドビューの描画処理実施
            RefreshEnemyGroupSideEnemyDropdown();
            RefreshEnemyGroupFrontEnemyDropdown();

            //------------------------------------
            // バトルイベント
            // -1は要素を消して処理しない
            if (_currentSelectEventId == -1 || _currentSelectEventId == -2)
            {
                RootContainer.Q<Foldout>("battle_event").style.display = DisplayStyle.None;
            }
            else if (_currentSelectEventId != -1)
            {
                if (_eventBattleData == null)
                    return;

                // ページ番号設定
                RootContainer.Q<Label>("event_page").text = (_currentSelectEventId + 1).ToString();

                var turnArea = RootContainer.Q<VisualElement>("turn_area");
                var enemyArea = RootContainer.Q<VisualElement>("enemy_area");
                var actorArea = RootContainer.Q<VisualElement>("actor_area");
                var switchArea = RootContainer.Q<VisualElement>("switch_dropdown");

                // トグルに対応するデータを入れる
                int[] toggleData =
                {
                    _eventBattleData.pages[_currentSelectEventId].condition.run,
                    _eventBattleData.pages[_currentSelectEventId].condition.turnEnd,
                    _eventBattleData.pages[_currentSelectEventId].condition.turn.enabled,
                    _eventBattleData.pages[_currentSelectEventId].condition.enemyHp.enabled,
                    _eventBattleData.pages[_currentSelectEventId].condition.actorHp.enabled,
                    _eventBattleData.pages[_currentSelectEventId].condition.switchData.enabled
                };
                
                turnArea.SetEnabled(toggleData[2] ==1);
                enemyArea.SetEnabled(toggleData[3] ==1);
                actorArea.SetEnabled(toggleData[4] ==1);
                switchArea.SetEnabled(toggleData[5] ==1);

                var battle_event_toggle = new List<Toggle>();
                for (var i = 0; i < 6; i++)
                {
                    var toggle = RootContainer.Query<Toggle>("battle_event_toggle").AtIndex(i);
                    toggle.value = false;
                    battle_event_toggle.Add(toggle);
                }

                for (var i = 0; i < battle_event_toggle.Count; i++)
                {
                    var num = i;
                    battle_event_toggle[num].value = Convert.ToBoolean(toggleData[num]);
                    battle_event_toggle[num].RegisterValueChangedCallback(evt =>
                    {
                        switch (num)
                        {
                            case 0:
                                _eventBattleData.pages[_currentSelectEventId].condition.run =
                                    Convert.ToInt32(battle_event_toggle[num].value);
                                break;
                            case 1:
                                _eventBattleData.pages[_currentSelectEventId].condition.turnEnd =
                                    Convert.ToInt32(battle_event_toggle[num].value);
                                break;
                            case 2:
                                _eventBattleData.pages[_currentSelectEventId].condition.turn.enabled =
                                    Convert.ToInt32(battle_event_toggle[num].value);
                                turnArea.SetEnabled(battle_event_toggle[num].value);
                                break;
                            case 3:
                                _eventBattleData.pages[_currentSelectEventId].condition.enemyHp.enabled =
                                    Convert.ToInt32(battle_event_toggle[num].value);
                                enemyArea.SetEnabled(battle_event_toggle[num].value);
                                break;
                            case 4:
                                _eventBattleData.pages[_currentSelectEventId].condition.actorHp.enabled =
                                    Convert.ToInt32(battle_event_toggle[num].value);
                                actorArea.SetEnabled(battle_event_toggle[num].value);
                                break;
                            case 5:
                                _eventBattleData.pages[_currentSelectEventId].condition.switchData.enabled =
                                    Convert.ToInt32(battle_event_toggle[num].value);
                                switchArea.SetEnabled(battle_event_toggle[num].value);
                                break;
                        }
                        

                        if (num == (int) BattleEventConditions.None)
                        {
                            //実行しないがtrueの時
                            if (battle_event_toggle[(int) BattleEventConditions.None].value)
                            {
                                //自分自身は押せない
                                battle_event_toggle[(int) BattleEventConditions.None].SetEnabled(false);
                                //それ以外はfalseになる
                                battle_event_toggle[(int) BattleEventConditions.TurnEnd].value = false;
                                battle_event_toggle[(int) BattleEventConditions.TurnDesignation].value = false;
                                battle_event_toggle[(int) BattleEventConditions.EnemyHp].value = false;
                                battle_event_toggle[(int) BattleEventConditions.ActorHp].value = false;
                                battle_event_toggle[(int) BattleEventConditions.Switch].value = false;
                            }

                            //実行しないがfalseの時
                            if (!battle_event_toggle[(int) BattleEventConditions.None].value)
                                //自分自身は押せる
                                battle_event_toggle[(int) BattleEventConditions.None].SetEnabled(true);
                        }
                        else
                        {
                            //実行しない以外がtrueになったら
                            if (
                                battle_event_toggle[(int) BattleEventConditions.TurnEnd].value ||
                                battle_event_toggle[(int) BattleEventConditions.TurnDesignation].value ||
                                battle_event_toggle[(int) BattleEventConditions.EnemyHp].value ||
                                battle_event_toggle[(int) BattleEventConditions.ActorHp].value ||
                                battle_event_toggle[(int) BattleEventConditions.Switch].value
                            )
                            {
                                battle_event_toggle[(int) BattleEventConditions.None].value = false;
                                battle_event_toggle[(int) BattleEventConditions.None].SetEnabled(true);
                            }

                            //実行しない以外が全員falseになったら
                            if (
                                !battle_event_toggle[(int) BattleEventConditions.TurnEnd].value &&
                                !battle_event_toggle[(int) BattleEventConditions.TurnDesignation].value &&
                                !battle_event_toggle[(int) BattleEventConditions.EnemyHp].value &&
                                !battle_event_toggle[(int) BattleEventConditions.ActorHp].value &&
                                !battle_event_toggle[(int) BattleEventConditions.Switch].value
                            )
                                battle_event_toggle[(int) BattleEventConditions.None].value = true;
                        }
                        Save();
                    });
                }

                if(battle_event_toggle[(int) BattleEventConditions.None].value)
                    battle_event_toggle[(int) BattleEventConditions.None].SetEnabled(false);



                // ターン指定のパラメータ
                for (var i = 0; i < RootContainer.Query<IntegerField>("turn_param").ToList().Count; i++)
                {
                    var num = i;
                    var integer = RootContainer.Query<IntegerField>("turn_param").AtIndex(i);
                    if (num == 0)
                        integer.value = _eventBattleData.pages[_currentSelectEventId].condition.turn.start;
                    else
                        integer.value = _eventBattleData.pages[_currentSelectEventId].condition.turn.end;

                    BaseInputFieldHandler.IntegerFieldCallback(integer, evt =>
                    {
                        if (num == 0)
                            _eventBattleData.pages[_currentSelectEventId].condition.turn.start = integer.value;
                        else
                            _eventBattleData.pages[_currentSelectEventId].condition.turn.end = integer.value;
                        Save();
                    }, 0, 9999);
                }

                // 敵キャラのパラメータ
                // dropdown
                // 敵の名前取得
                var enemyData = databaseManagementService.LoadEnemy();
                var enemyNames = new List<string>();
                var enemyNum = 0;
                // 初期値設定
                if (_eventBattleData.pages[_currentSelectEventId].condition.enemyHp.enemyId == "" &&
                    enemyData.Count > 0)
                    _eventBattleData.pages[_currentSelectEventId].condition.enemyHp.enemyId = "0";

                //サイドビュー探索
                foreach (var member in _troopList.sideViewMembers)
                {
                    for (int i = 0; i < enemyData.Count; i++)
                        if (enemyData[i].id == member.enemyId)
                        {
                            enemyNames.Add(enemyData[i].name);
                            break;
                        }
                }
                
                //フロントビュー探索
                if (enemyNames.Count == 0)
                {
                    foreach (var member in _troopList.frontViewMembers)
                    {
                        for (int i = 0; i < enemyData.Count; i++)
                            if (enemyData[i].id == member.enemyId)
                            {
                                enemyNames.Add(enemyData[i].name);
                                break;
                            }
                    }
                }

                if (_eventBattleData.pages[_currentSelectEventId].condition.enemyHp.enemyId != "")
                    enemyNum = int.Parse(_eventBattleData.pages[_currentSelectEventId].condition.enemyHp.enemyId);

                // 選択肢設定
                VisualElement enemy_id = RootContainer.Query<VisualElement>("enemy_dropdown");
                var enemyDropdownPopupField = new PopupFieldBase<string>(enemyNames, enemyNum);
                enemy_id.Add(enemyDropdownPopupField);
                enemyDropdownPopupField.RegisterValueChangedCallback(evt =>
                {
                    _eventBattleData.pages[_currentSelectEventId].condition.enemyHp.enemyId = enemyDropdownPopupField.index.ToString();
                    Save();
                });
                // integer
                var enemy_param = RootContainer.Q<IntegerField>("enemy_param");
                enemy_param.value = _eventBattleData.pages[_currentSelectEventId].condition.enemyHp.value;
                enemy_param.RegisterValueChangedCallback(evt =>
                {
                    // 値制限
                    if (enemy_param.value < 0)
                        enemy_param.value = 0;
                    else if (enemy_param.value > 100)
                        enemy_param.value = 100;

                    _eventBattleData.pages[_currentSelectEventId].condition.enemyHp.value = enemy_param.value;
                    Save();
                });

                // アクターのパラメータ
                // dropdown
                // アクターの名前取得
                var actorData = databaseManagementService.LoadCharacterActor().FindAll(a => a.charaType == (int) ActorTypeEnum.ACTOR);
                var actorNames = new List<string>();
                var actorNum = 0;
                // 初期値設定
                if (_eventBattleData.pages[_currentSelectEventId].condition.actorHp.actorId == "" &&
                    actorData.Count > 0)
                    _eventBattleData.pages[_currentSelectEventId].condition.actorHp.actorId = actorData[0].uuId;
                for (var i = 0; i < actorData.Count; i++)
                {
                    actorNames.Add(actorData[i].basic.name);
                    if (actorData[i].uuId ==
                        _eventBattleData.pages[_currentSelectEventId].condition.actorHp.actorId)
                        actorNum = i;
                }

                // 選択肢設定
                VisualElement actor_id = RootContainer.Query<VisualElement>("actor_dropdown");
                var actorDropdownPopupField = new PopupFieldBase<string>(actorNames, actorNum);
                actor_id.Add(actorDropdownPopupField);
                actorDropdownPopupField.RegisterValueChangedCallback(evt =>
                {
                    _eventBattleData.pages[_currentSelectEventId].condition.actorHp.actorId =
                        actorData[actorDropdownPopupField.index].uuId;
                    Save();
                });
                // integer
                var actor_param = RootContainer.Q<IntegerField>("actor_param");
                actor_param.value = _eventBattleData.pages[_currentSelectEventId].condition.actorHp.value;
                actor_param.RegisterValueChangedCallback(evt =>
                {
                    // 値制限
                    if (actor_param.value < 0)
                        actor_param.value = 0;
                    else if (actor_param.value > 100)
                        actor_param.value = 100;

                    _eventBattleData.pages[_currentSelectEventId].condition.actorHp.value = actor_param.value;
                    Save();
                });

                // スイッチ
                // dropdown
                // スイッチ取得
                var flagsData = databaseManagementService.LoadFlags();
                var flagsName = new List<string>();
                var flagsNum = 0;
                for (var i = 0; i < flagsData.switches.Count; i++)
                {
                    flagsName.Add(string.Format("{0:D4}", i + 1) + " " + flagsData.switches[i].name);
                    if (flagsData.switches[i].id ==
                        _eventBattleData.pages[_currentSelectEventId].condition.switchData.switchId)
                        flagsNum = i;
                }

                VisualElement switchDropdown = RootContainer.Query<VisualElement>("switch_dropdown");
                var switchDropdownPopupField = new PopupFieldBase<string>(flagsName, flagsNum);
                switchDropdown.Add(switchDropdownPopupField);
                switchDropdownPopupField.RegisterValueChangedCallback(evt =>
                {
                    _eventBattleData.pages[_currentSelectEventId].condition.switchData.switchId =
                        flagsData.switches[switchDropdownPopupField.index].id;
                    Save();
                });

                // スパン設定
                VisualElement span = RootContainer.Query<VisualElement>("enemyGroup_span_dropdown");
                var spanDropdownPopupField = new PopupFieldBase<string>(_spanList,
                    _eventBattleData.pages[_currentSelectEventId].condition.span);
                span.Add(spanDropdownPopupField);
                spanDropdownPopupField.RegisterValueChangedCallback(evt =>
                {
                    _eventBattleData.pages[_currentSelectEventId].condition.span = spanDropdownPopupField.index;
                    Save();
                });

                // イベントエディタボタン
                //最初から、イベントエディタ表示
                var eventId = _eventBattleData.pages[_currentSelectEventId].eventId;
                var eventData = eventManagementService.LoadEventById(eventId);
                if (eventData == null) return;

                // バトルグループのバトルイベント編集。
                _eventData = eventData;
                LaunchEventEditMode(_eventData);
            }
        }


        /// <summary>
        ///     フロント側敵設定
        /// </summary>
        /// <param name="index"></param>
        private void _SetFrontEnemy(int index) {
            //フロントビューのFoldout
            _frontViewFoldout = new Foldout();
            _frontViewFoldout.AddToClassList("foldout_transparent");
            _frontViewFoldout.text = EditorLocalize.LocalizeText("WORD_0559") + (index + 1);
            _frontViewFoldout.name = "foldout_" + _id + "_frontview_" + (index + 1);
            _enemyGroupFrontEnemyDropdown.Add(_frontViewFoldout);

            //Foldout内のコンテンツ
            var frontViewItemUnit = new InspectorItemUnit();

            //敵一覧
            var enemyScriptableObject = databaseManagementService.LoadEnemy();
            var enemyList = enemyScriptableObject.ToList();
            var enemyNameList = new List<string>();
            foreach (var e in enemyList) enemyNameList.Add(e.name);

            var enemyPopupField = new PopupFieldBase<string>(enemyNameList, 0);
            bool isEnemy = false;
            for (var i = 0; i < enemyList.Count; i++)
                if (enemyList[i].id == _troopList.frontViewMembers[index].enemyId)
                {
                    enemyPopupField = new PopupFieldBase<string>(enemyNameList, i);
                    isEnemy = true;
                    break;
                }
            
            if (!isEnemy)
            {
                _currentSelectId = index;
                DeleteEnemy(CurrentType.Front);
                return;
            }

            frontViewItemUnit.Add(enemyPopupField);
            enemyPopupField.RegisterValueChangedCallback(evt =>
            {
                _troopList.frontViewMembers[index].enemyId = enemyList[enemyPopupField.index].id;
                Save();
                UpdateData();
                InitAndRenderSceneView();
            });
            _frontViewFoldout.Add(frontViewItemUnit);

            //配置場所を追加
            var enemyPosition = new InspectorItemUnit();
            _frontViewFoldout.Add(enemyPosition);

            //配置場所については、他のEnemyが更新されるたびに更新が必要なため、更新箇所を保持
            _frontPositionList.Add(enemyPosition);

            //出現条件
            var enemyCondition = new InspectorItemUnit();
            _frontViewFoldout.Add(enemyCondition);
            var l2 = new Label(EditorLocalize.LocalizeText("WORD_0613"));
            enemyCondition.Add(l2);
            var enemyGroupFrontConditionsDropdownChoices =
                EditorLocalize.LocalizeTexts(new List<string> {"WORD_0578", "WORD_0636", "WORD_0626"});
            var enemyGroupFrontConditionsDropdownPopupField =
                new PopupFieldBase<string>(enemyGroupFrontConditionsDropdownChoices, _troopList.frontViewMembers[index].conditions);
            enemyGroupFrontConditionsDropdownPopupField.style.width = 200;
            enemyCondition.Add(enemyGroupFrontConditionsDropdownPopupField);

            var turn = new InspectorItemUnit();
            _frontViewFoldout.Add(turn);

            //ターンの初期制御
            turn.SetEnabled(false);
            if (_troopList.frontViewMembers[index].conditions ==
                enemyGroupFrontConditionsDropdownChoices.IndexOf(EditorLocalize.LocalizeText("WORD_0626")) || 
                _troopList.frontViewMembers[index].conditions == enemyGroupFrontConditionsDropdownChoices.IndexOf(EditorLocalize.LocalizeText("WORD_0636")))
                turn.SetEnabled(true);

            enemyGroupFrontConditionsDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                _troopList.frontViewMembers[index].conditions = enemyGroupFrontConditionsDropdownPopupField.index;
                //ターンの表示を設定
                turn.SetEnabled(false);
                if (enemyGroupFrontConditionsDropdownPopupField.value == EditorLocalize.LocalizeText("WORD_0626") ||
                    enemyGroupFrontConditionsDropdownPopupField.value == EditorLocalize.LocalizeText("WORD_0636"))
                    turn.SetEnabled(true);

                turn.Clear();
                _SetTurnFront(index, turn);
                Save();
                InitAndRenderSceneView();
            });

            //ターン設定
            turn.Clear();
            _SetTurnFront(index, turn);

            //プレビュー
            var enemyGroupFrontPreviewContainer = new InspectorItemUnit();
            var enemyName = "";
            var enemyGroupFrontPreview = new Button();
            enemyGroupFrontPreview.style.display = DisplayStyle.None;
            enemyGroupFrontPreview.text = EditorLocalize.LocalizeText("WORD_0991");
            enemyGroupFrontPreviewContainer.Add(enemyGroupFrontPreview);
            _frontViewFoldout.Add(enemyGroupFrontPreviewContainer);
            enemyGroupFrontPreview.clicked += () =>
            {
                foreach (var enemyData in enemyList)
                    if (enemyData.id == _troopList.frontViewMembers[index].enemyId)
                    {
                        enemyName = enemyData.images.image;
                        break;
                    }

                var PreviewWindow = new PreviewWindow();
                PreviewWindow.ImagePath = PathManager.IMAGE_ENEMY;
                PreviewWindow.previewImage = enemyName;
                PreviewWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Image"), data => { });
            };

            var deleteButton = new Button {text = EditorLocalize.LocalizeText("WORD_0383")};
            deleteButton.clickable.clicked += () =>
            {
                _currentSelectId = index;
                DeleteEnemy(CurrentType.Front);
                UpdateData();
            };
            _frontViewFoldout.Add(deleteButton);

            SetFoldout(_frontViewFoldout);
        }

        private void _SetFrontEnemyPosition() {
            for (int index = 0; index < _frontPositionList.Count; index++)
            {
                //リストをクリア
                _frontPositionList[index].Clear();

                //ラベル設定
                var label = new Label(EditorLocalize.LocalizeText("WORD_0094"));
                _frontPositionList[index].Add(label);

                //配置位置
                var enemyGroupFrontPositionDropdownChoices = new List<string> { "1", "2", "3", "4", "5", "6", "7", "8" };

                //既に使われている表示位置を除外するための処理
                var usedPosition = new List<string>();
                foreach (var member in _troopList.frontViewMembers)
                    usedPosition.Add((member.position + 1).ToString());

                //PU生成
                PopupFieldBase<string> enemyGroupPopupField = new PopupFieldBase<string>(
                    enemyGroupFrontPositionDropdownChoices,
                    _troopList.frontViewMembers[index].position,
                    null, null, 0,
                    usedPosition
                );
                enemyGroupPopupField.style.width = 200;
                _frontPositionList[index].Add(enemyGroupPopupField);

                //値変更時のCB処理
                int enemyIndex = index;
                enemyGroupPopupField.RegisterValueChangedCallback(evt =>
                {
                    //設定後の値を保存
                    _troopList.frontViewMembers[enemyIndex].position = enemyGroupPopupField.index;
                    Save();
                    InitAndRenderSceneView();

                    //配置位置のみ再描画
                    _SetFrontEnemyPosition();
                });
            }
        }

        /// <summary>
        ///     サイド側敵設定
        /// </summary>
        /// <param name="index"></param>
        private void _SetSideEnemy(int index) {
            //サイドビューのFoldout
            var sideViewFoldout = new Foldout();
            sideViewFoldout.AddToClassList("foldout_transparent");
            sideViewFoldout.text = EditorLocalize.LocalizeText("WORD_0559") + (index + 1);
            sideViewFoldout.name = "foldout_" + _id + "_sideview_" + (index + 1);
            _enemyGroupSideEnemyDropdown.Add(sideViewFoldout);

            //該当の敵を取得
            var enemyScriptableObject = databaseManagementService.LoadEnemy();
            var enemyList = enemyScriptableObject.ToList();
            int enemyIndex = 0;
            bool isEnemy = false;
            for (var i = 0; i < enemyList.Count; i++)
                if (enemyList[i].id == _troopList.sideViewMembers[index].enemyId)
                {
                    enemyIndex = i;
                    isEnemy = true;
                    break;
                }

            if (!isEnemy)
            {
                _currentSelectId = index;
                DeleteEnemy(CurrentType.Side);
                return;
            }

            //押せないトグルのリスト
            var pos1Enable = new List<int>();
            var pos2Enable = new List<int>();
            var pos2Check = new List<bool>();
            pos2Check.Add(false);
            pos2Check.Add(false);
            pos2Check.Add(false);

            for (var i = 0; i < 2; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    //今いる列の上中下の押せるものの確認
                    if (_troopList.sideViewMembers[index].position1 == i)
                    {
                        if (Used(i, j))
                        {
                            pos2Enable.Add(j);
                            pos2Check[j] = false;
                        }
                        else
                        {
                            pos2Check[j] = true;
                        }
                    }
                }
                //上中下がすべて埋まっている場合
                if (Used(i, 0) && Used(i, 1) && Used(i, 2))
                {
                    pos1Enable.Add(i);
                }
            }

            //敵一覧
            var enemyListView = new VisualElement();
            enemyListView.AddToClassList("enemy_list_area");
            sideViewFoldout.Add(enemyListView);

            //敵一覧表示
            var enemyNameList = new List<string>();
            var enemyListWork = new List<EnemyDataModel>();

            //現在設定されている敵よりも大きな敵に変更できない場合、その敵を選択肢から除外する
            if (enemyList[enemyIndex].images.autofitPattern == 0)
            {
                //小型である
                //現在設定されている前衛/後衛の上か下が空いていれば、大型は設置可能
                //現在設定されている前衛/後衛に、他に誰もいなければボス型は設置可能
                if (!(_troopList.sideViewMembers[index].position2 == 0 && pos2Check[1] ||
                    _troopList.sideViewMembers[index].position2 == 1 && (pos2Check[0] || pos2Check[2]) ||
                    _troopList.sideViewMembers[index].position2 == 2 && pos2Check[1]))
                {
                    //例外処理
                    //小型と同列に要る可能性があるのは、小型と大型
                    //自身が真ん中に配置されておらず、他の敵も真ん中には配置されていなければ、大型は配置可能
                    bool flg = false;
                    if (_troopList.sideViewMembers[index].position2 != 1)
                    {
                        for (int num = 0; num < _troopList.sideViewMembers.Count; num++)
                        {
                            if (num == index) continue;
                            if (_troopList.sideViewMembers[num].position1 == _troopList.sideViewMembers[index].position1)
                            {
                                if (_troopList.sideViewMembers[num].position2 == 1)
                                {
                                    flg = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (flg)
                    {
                        //大型もボス型もおけないため、小型だけを詰める
                        for (int i = 0; i < enemyList.Count; i++)
                        {
                            if (enemyList[i].images.autofitPattern == 0)
                            {
                                enemyListWork.Add(enemyList[i]);
                            }
                        }
                    }
                    else
                    {
                        //ボス型をおけないため、小型と大型だけを詰める
                        for (int i = 0; i < enemyList.Count; i++)
                        {
                            if (enemyList[i].images.autofitPattern < 2)
                            {
                                enemyListWork.Add(enemyList[i]);
                            }
                        }
                    }
                }
                else if (!(_troopList.sideViewMembers[index].position2 == 0 && pos2Check[1] && pos2Check[2] ||
                    _troopList.sideViewMembers[index].position2 == 1 && pos2Check[0] && pos2Check[2] ||
                    _troopList.sideViewMembers[index].position2 == 2 && pos2Check[0] && pos2Check[1]) ||
                    _troopList.sideViewMembers.Count >= 2)
                {
                    //ボス型をおけないため、小型と大型だけを詰める
                    for (int i = 0; i < enemyList.Count; i++)
                    {
                        if (enemyList[i].images.autofitPattern < 2)
                        {
                            enemyListWork.Add(enemyList[i]);
                        }
                    }
                }
                else
                {
                    //全ての敵を詰める
                    for (int i = 0; i < enemyList.Count; i++)
                    {
                        enemyListWork.Add(enemyList[i]);
                    }
                }
            }
            else if (enemyList[enemyIndex].images.autofitPattern == 1)
            {
                //大型である
                if (!(_troopList.sideViewMembers[index].position2 == 0 && pos2Check[2] ||
                    _troopList.sideViewMembers[index].position2 == 2 && pos2Check[0]))
                {
                    //ボス型をおけないため、小型と大型だけを詰める
                    for (int i = 0; i < enemyList.Count; i++)
                    {
                        if (enemyList[i].images.autofitPattern < 2)
                        {
                            enemyListWork.Add(enemyList[i]);
                        }
                    }
                }
                else
                {
                    //全ての敵を詰める
                    for (int i = 0; i < enemyList.Count; i++)
                    {
                        enemyListWork.Add(enemyList[i]);
                    }
                }
            }
            else
            {
                //全ての敵を詰める
                for (int i = 0; i < enemyList.Count; i++)
                {
                    enemyListWork.Add(enemyList[i]);
                }
            }
            foreach (var e in enemyListWork) enemyNameList.Add(e.name);

            var enemyPopupField = new PopupFieldBase<string>(enemyNameList, 0);
            for (var i = 0; i < enemyListWork.Count; i++)
                if (enemyListWork[i].id == _troopList.sideViewMembers[index].enemyId)
                {
                    enemyPopupField = new PopupFieldBase<string>(enemyNameList, i);
                    break;
                }

            enemyListView.Add(enemyPopupField);
            enemyPopupField.RegisterValueChangedCallback(evt =>
            {
                _troopList.sideViewMembers[index].enemyId = enemyListWork[enemyPopupField.index].id;

                //選択された敵が大型、ボス型の場合には、配置位置を補正する
                for (var i = 0; i < enemyList.Count; i++)
                    if (enemyList[i].id == _troopList.sideViewMembers[index].enemyId)
                    {
                        if (enemyList[i].images.autofitPattern == 2)
                        {
                            //ボス型だった場合は強制的に中に配置
                            _troopList.sideViewMembers[index].position2 = 1;
                        }
                        else if (enemyList[i].images.autofitPattern == 1 && _troopList.sideViewMembers[index].position2 == 1)
                        {
                            //大型だった場合は、上か下の空いている方に配置
                            bool flg = false;
                            for (int j = 0; j < _troopList.sideViewMembers.Count; j++)
                            {
                                if (j != index && _troopList.sideViewMembers[j].position1 == _troopList.sideViewMembers[index].position1)
                                {
                                    if (_troopList.sideViewMembers[j].position2 == 0) _troopList.sideViewMembers[index].position2 = 2;
                                    else _troopList.sideViewMembers[index].position2 = 0;
                                    flg = true;
                                    break;
                                }
                            }
                            if (!flg)
                            {
                                //どちらにおいても良いため、上に置く
                                _troopList.sideViewMembers[index].position2 = 0;
                            }
                        }
                        break;
                    }

                Save();
                InitAndRenderSceneView();
                UpdateData();
                RefreshEnemyGroupSideEnemyDropdown();
            });

            //表示位置1
            {
                var tg1 = new InspectorItemUnit();
                sideViewFoldout.Add(tg1);
                UIElementsUtil.AddToggleGroupEnable(
                    tg1,
                    EditorLocalize.LocalizeText("WORD_0617"),
                    EditorLocalize.LocalizeTexts(new List<string> {"WORD_0618", "WORD_0619"}),
                    _troopList.sideViewMembers[index].position1,
                    pos1Enable,
                    clickToggleIndex =>
                    {
                        _troopList.sideViewMembers[index].position1 = clickToggleIndex;

                        //上中下の再割り振り
                        for (var i = 0; i < 3; i++)
                            if (!Used(clickToggleIndex, i))
                                _troopList.sideViewMembers[index].position2 = i;

                        Save();
                        InitAndRenderSceneView();
                        RefreshEnemyGroupSideEnemyDropdown();
                    });
            }

            //表示位置2
            {
                var tg2 = new InspectorItemUnit();
                sideViewFoldout.Add(tg2);
                UIElementsUtil.AddToggleGroupEnable(
                    tg2,
                    EditorLocalize.LocalizeText("WORD_0620"),
                    EditorLocalize.LocalizeTexts(new List<string> {"WORD_0297", "WORD_0298", "WORD_0299"}),
                    _troopList.sideViewMembers[index].position2,
                    pos2Enable,
                    clickToggleIndex =>
                    {
                        _troopList.sideViewMembers[index].position2 = clickToggleIndex;
                        Save();
                        InitAndRenderSceneView();
                        RefreshEnemyGroupSideEnemyDropdown();
                    });
            }

            //出現条件
            var enemyCondition = new InspectorItemUnit();
            sideViewFoldout.Add(enemyCondition);
            var l2 = new Label(EditorLocalize.LocalizeText("WORD_0613"));
            enemyCondition.Add(l2);
            var enemyGroupSideConditionsDropdownChoices =
                EditorLocalize.LocalizeTexts(new List<string> {"WORD_0578", "WORD_0636", "WORD_0626"});
            var enemyGroupSideConditionsDropdownPopupField =
                new PopupFieldBase<string>(enemyGroupSideConditionsDropdownChoices, _troopList.sideViewMembers[index].conditions);
            enemyGroupSideConditionsDropdownPopupField.style.width = 200;
            enemyCondition.Add(enemyGroupSideConditionsDropdownPopupField);

            var turn = new InspectorItemUnit();
            sideViewFoldout.Add(turn);

            //ターンの初期制御
            turn.SetEnabled(false);
            if (_troopList.sideViewMembers[index].conditions == enemyGroupSideConditionsDropdownChoices.IndexOf(EditorLocalize.LocalizeText("WORD_0636")) ||
                _troopList.sideViewMembers[index].conditions == enemyGroupSideConditionsDropdownChoices.IndexOf(EditorLocalize.LocalizeText("WORD_0626")))
                turn.SetEnabled(true);

            enemyGroupSideConditionsDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                _troopList.sideViewMembers[index].conditions = enemyGroupSideConditionsDropdownPopupField.index;
                //ターンの表示を設定
                turn.SetEnabled(false);
                if (_troopList.sideViewMembers[index].conditions ==
                    enemyGroupSideConditionsDropdownChoices.IndexOf(EditorLocalize.LocalizeText("WORD_0626")) ||
                    _troopList.sideViewMembers[index].conditions ==
                    enemyGroupSideConditionsDropdownChoices.IndexOf(EditorLocalize.LocalizeText("WORD_0636")))
                    turn.SetEnabled(true);

                turn.Clear();
                _SetTurnSide(index, turn);
                Save();
                InitAndRenderSceneView();
            });

            //ターン設定
            turn.Clear();
            _SetTurnSide(index, turn);


            //プレビュー
            var enemyGroupSidePreviewContainer = new InspectorItemUnit();
            var enemyName = "";
            var enemyGroupSidePreview = new Button();
            enemyGroupSidePreview.style.display = DisplayStyle.None;
            enemyGroupSidePreview.text = EditorLocalize.LocalizeText("WORD_0991");
            enemyGroupSidePreviewContainer.Add(enemyGroupSidePreview);
            sideViewFoldout.Add(enemyGroupSidePreviewContainer);
            enemyGroupSidePreview.clicked += () =>
            {
                foreach (var enemyData in enemyList)
                    if (enemyData.id == _troopList.sideViewMembers[index].enemyId)
                    {
                        enemyName = enemyData.images.image;
                        break;
                    }

                var PreviewWindow = new PreviewWindow();
                PreviewWindow.ImagePath = PathManager.IMAGE_ENEMY;
                PreviewWindow.previewImage = enemyName;
                PreviewWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Image"), data => { });
            };

            var deleteButton = new Button {text = EditorLocalize.LocalizeText("WORD_0383")};
            deleteButton.clickable.clicked += () =>
            {
                _currentSelectId = index;
                DeleteEnemy(CurrentType.Side);
                UpdateData();
            };
            sideViewFoldout.Add(deleteButton);

            SetFoldout(sideViewFoldout);
        }

        /// <summary>
        ///     ターン時の設定
        /// </summary>
        /// <param name="index"></param>
        /// <param name="RootContainer"></param>
        private void _SetTurnFront(int index, VisualElement RootContainer) {
            var i = new IntegerField();
            i.maxLength = 3;
            i.value = _troopList.frontViewMembers[index].appearanceTurn;
            BaseInputFieldHandler.IntegerFieldCallback(i, evt =>
            {
                _troopList.frontViewMembers[index].appearanceTurn = i.value;
                Save();
            }, 0, 999);

            RootContainer.Add(new Label {text = EditorLocalize.LocalizeText("WORD_0626")});
            RootContainer.Add(i);
        }

        private void _SetTurnSide(int index, VisualElement RootContainer) {
            var i = new IntegerField();
            i.maxLength = 3;
            i.value = _troopList.sideViewMembers[index].appearanceTurn;
            BaseInputFieldHandler.IntegerFieldCallback(i, evt =>
            {
                _troopList.sideViewMembers[index].appearanceTurn = i.value;
                Save();
            }, 0, 999);

            RootContainer.Add(new Label {text = EditorLocalize.LocalizeText("WORD_0626")});
            RootContainer.Add(i);
        }

        override protected void SaveContents() {
            //セーブ部位の作成
            databaseManagementService.SaveTroop(_troopDataModels);
            eventManagementService.SaveEventBattle(_eventBattleDataModels);
        }

        private void DeleteEnemy(CurrentType currentType) {
            switch (currentType)
            {
                case CurrentType.Front:
                    _troopList.frontViewMembers.RemoveAt(_currentSelectId);
                    RefreshEnemyGroupFrontEnemyDropdown();
                    Save();
                    InitAndRenderSceneView();
                    break;
                case CurrentType.Side:
                    _troopList.sideViewMembers.RemoveAt(_currentSelectId);
                    RefreshEnemyGroupSideEnemyDropdown();
                    Save();
                    InitAndRenderSceneView();
                    break;
            }
        }

        private void UpdateData(bool isChangeName = false) {
            if (isChangeName)
                _ = Editor.Hierarchy.Hierarchy.Refresh(Region.Battle, _id);
        }

        //フロント敵グループの更新処理
        private void RefreshEnemyGroupFrontEnemyDropdown() {
            var Items = new VisualElement();
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(frontUxml);
            VisualElement labelFromUXML = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromUXML);
            labelFromUXML.style.flexGrow = 1;
            Items.Add(labelFromUXML);

            Button enemyGroupFrontCleraButton = Items.Query<Button>("enemyGroup_front_clear_button");
            enemyGroupFrontCleraButton.clicked += () =>
            {
                _troopList.frontViewMembers.Clear();
                Save();
                InitAndRenderSceneView();
                UpdateData();
                RefreshScroll();
            };

            //フロント側敵設定
            Button enemyFrontAdd = Items.Query<Button>("enemy_front_add");
            _enemyGroupFrontEnemyDropdown = Items.Query<VisualElement>("enemyGroup_front_enemy_dropdown");

            //フロント敵追加押したとき
            enemyFrontAdd.clickable.clicked += () =>
            {
                //フロントの敵上限数に達していた場合は追加しない
                if (_troopList.frontViewMembers.Count < _frontEnemyMax)
                {
                    //ポジションの割り振り
                    var position = 0;
                    for (var i = 0; i < 8; i++)
                        if (!used(i))
                        {
                            position = i;
                            break;
                        }

                    //既にそのポジションが使われているかの確認
                    bool used(int num) {
                        for (var i = 0; i < _troopList.frontViewMembers.Count; i++)
                            //使われていた
                            if (_troopList.frontViewMembers[i].position == num)
                                return true;

                        //使われていなかった
                        return false;
                    }

                    //新しく追加
                    _troopList.frontViewMembers.Add(new TroopDataModel.FrontViewMember(
                        "",
                        position,
                        0,
                        0
                    ));

                    // 追加時に敵が存在すれば先頭を設定する
                    var enemyScriptableObject = databaseManagementService.LoadEnemy();
                    var enemyList = enemyScriptableObject.ToList();
                    if (enemyList != null && 0 < enemyList.Count)
                        _troopList.frontViewMembers[_troopList.frontViewMembers.Count - 1].enemyId = enemyList[0].id;

                    Save();
                    InitAndRenderSceneView();
                    UpdateData();
                    RefreshEnemyGroupFrontEnemyDropdown();
                }
            };

            _enemyGroupFrontEnemyDropdown.Clear();

            //フロントビューの配置場所を描画するリストの初期化
            _frontPositionList = new List<InspectorItemUnit>();

            //敵情報描画
            var enemyData = DataManager.Self().GetEnemyDataModels();
            for (var i = 0; i < _troopList.frontViewMembers.Count; i++)
            {
                //存在する敵のみ処理する
                bool flg = false;
                for (var j = 0; j < enemyData.Count; j++)
                {
                    if (enemyData[j].id == _troopList.frontViewMembers[i].enemyId)
                    {
                        _SetFrontEnemy(i);
                        flg = true;
                        break;
                    }
                }
                if (!flg)
                {
                    _troopList.frontViewMembers.RemoveAt(i);
                    i--;
                }

            }

            //敵の配置場所描画
            _SetFrontEnemyPosition();

            _frontViewSwitch.Clear();
            _frontViewSwitch.Add(Items);
        }

        //サイド敵グループの更新処理
        private void RefreshEnemyGroupSideEnemyDropdown() {
            var Items = new VisualElement();
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(sideUxml);
            VisualElement labelFromUXML = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromUXML);
            labelFromUXML.style.flexGrow = 1;
            Items.Add(labelFromUXML);

            Button enemyGroupSideCleraButton = Items.Query<Button>("enemyGroup_side_clera_button");
            enemyGroupSideCleraButton.clicked += () =>
            {
                _troopList.sideViewMembers.Clear();
                Save();
                InitAndRenderSceneView();
                UpdateData();
                RefreshScroll();
            };

            //※※サイド側敵設定※※
            Button enemySideAdd = Items.Query<Button>("enemy_side_add");
            _enemyGroupSideEnemyDropdown = Items.Query<VisualElement>("enemyGroup_side_enemy_dropdown");

            //サイド敵追加押したとき
            enemySideAdd.clickable.clicked += () =>
            {
                //最初に追加するのは小型とする
                var enemyScriptableObject = databaseManagementService.LoadEnemy();
                var enemyList = enemyScriptableObject.ToList();
                int index = -1;
                for (int i = 0; enemyList != null && i < enemyList.Count; i++)
                {
                    if (enemyList[i].images.autofitPattern == 0)
                    {
                        index = i;
                        break;
                    }
                }

                //ポジションの割り振り
                var position1 = -1;
                var position2 = -1;
                for (var i = 0; i < 2; i++)
                {
                    for (var j = 0; j < 3; j++)
                    {
                        if (!Used(i, j))
                        {
                            position1 = i;
                            position2 = j;
                            break;
                        }
                    }
                }

                //配置できる場所がなければ追加しない
                if (position1 == -1 && position2 == -1)
                    return;

                //新しく追加
                _troopList.sideViewMembers.Add(new TroopDataModel.SideViewMember(
                    "",
                    position1,
                    position2,
                    0,
                    0)
                );

                // 追加時に敵が存在すれば先頭を設定する
                if (enemyList != null && index < enemyList.Count && index != -1)
                    _troopList.sideViewMembers[_troopList.sideViewMembers.Count - 1].enemyId = enemyList[index].id;

                Save();
                InitAndRenderSceneView();
                UpdateData();
                RefreshEnemyGroupSideEnemyDropdown();
            };

            _enemyGroupSideEnemyDropdown.Clear();

            var enemyData = DataManager.Self().GetEnemyDataModels();
            for (var i = 0; i < _troopList.sideViewMembers.Count; i++)
            {
                //存在する敵のみ処理する
                bool flg = false;
                for (var j = 0; j < enemyData.Count; j++)
                {
                    if (enemyData[j].id == _troopList.sideViewMembers[i].enemyId)
                    {
                        _SetSideEnemy(i);
                        flg = true;
                        break;
                    }
                }
                if (!flg)
                {
                    _troopList.sideViewMembers.RemoveAt(i);
                    i--;
                }
            }
            _sideViewSwitch.Clear();
            _sideViewSwitch.Add(Items);
        }

        //サイドビューで既にそのポジションが使われているかの確認
        private bool Used(int num1, int num2) {
            var enemyData = DataManager.Self().GetEnemyDataModels();
            for (var i = 0; i < _troopList.sideViewMembers.Count; i++)
            {
                //使われていた
                if (_troopList.sideViewMembers[i].position1 == num1 && _troopList.sideViewMembers[i].position2 == num2)
                {
                    return true;
                }

                //このEnemyが大型の場合、配置場所が上または下なら、中も利用していることとする
                EnemyDataModel enemyDataWork = null;
                for (int i2 = 0; i2 < enemyData.Count; i2++)
                    if (enemyData[i2].id == _troopList.sideViewMembers[i].enemyId)
                    {
                        enemyDataWork = enemyData[i2];
                        break;
                    }

                //敵が存在しない場合にはcontinue
                if (enemyDataWork == null) continue;

                if (enemyDataWork.images.autofitPattern == 1)
                {
                    if (num2 == 1 && _troopList.sideViewMembers[i].position1 == num1)
                    {
                        return true;
                    }
                }

                //このEnemyがボス型の場合、num1にもnum2にも置けない
                if (enemyDataWork.images.autofitPattern == 2)
                {
                    return true;
                }
            }

            //使われていなかった
            return false;
        }

        private enum CurrentType
        {
            Front = 0,
            Side  = 1
        }

        //バトルイベントの条件
        private enum BattleEventConditions
        {
            None = 0,
            TurnEnd,
            TurnDesignation,
            EnemyHp,
            ActorHp,
            Switch
        }

        public void ClearSceneWindow() {
            _sceneWindow?.Clear();
        }
    }
}