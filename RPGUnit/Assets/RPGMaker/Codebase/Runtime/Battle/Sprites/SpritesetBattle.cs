using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Enemy;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Troop;
using RPGMaker.Codebase.CoreSystem.Knowledge.Misc;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Battle.Window;
using RPGMaker.Codebase.Runtime.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Battle.Sprites
{
    /// <summary>
    /// 戦闘シーン用のスプライトセット。[背景][アクター][敵キャラ]を含む
    /// Unite用に作り替えている
    /// </summary>
    public class SpritesetBattle : MonoBehaviour
    {
        /// <summary>
        /// アクター
        /// </summary>
        private List<SpriteActor> _actorSprites = new List<SpriteActor>();
        /// <summary>
        /// バトル背景1
        /// </summary>
        [SerializeField] private Image  _back1Sprite;
        /// <summary>
        /// バトル背景2
        /// </summary>
        [SerializeField] private Image  _back2Sprite;
        /// <summary>
        /// 敵
        /// </summary>
        private List<SpriteEnemy> _enemySprites = new List<SpriteEnemy>();
        /// <summary>
        /// フロントの親オブジェクト
        /// </summary>
        [SerializeField] private GameObject _frontObject;
        /// <summary>
        /// サイドビューの親オブジェクト
        /// </summary>
        [SerializeField] private GameObject _sideObject;
        /// <summary>
        /// プレビューかどうか
        /// </summary>
        private bool _isRenderBattleScenePreview;
        /// <summary>
        /// システム設定のDataModel
        /// </summary>
        private SystemSettingDataModel _systemdata;
        /// <summary>
        /// アクターのClone元Object
        /// </summary>
        [SerializeField] private SpriteActor originActor;
        /// <summary>
        /// 敵のClone元Object
        /// </summary>
        [SerializeField] private SpriteEnemy originEnemy;
        /// <summary>
        /// 戦闘シーンのプレビュー用に使用
        /// </summary>
        private GameObject _battleWindowObj = null;
        /// <summary>
        /// 小型のサイズ
        /// </summary>
        private const int _sizeSmall = 224;
        /// <summary>
        /// 大型のサイズ
        /// </summary>
        private const int _sizeLarge = 344;
        /// <summary>
        /// ボス型のサイズ幅
        /// </summary>
        private const int _sizeBossWidth = 856;
        /// <summary>
        /// ボス型のサイズ高
        /// </summary>
        private const int _sizeBossHeight = 680;
        /// <summary>
        /// 傾斜の値に応じてX座標を調整する際に利用する値（小型）
        /// </summary>
        private const int _offsetSmallX = 12;
        /// <summary>
        /// 傾斜の値に応じてX座標を調整する際に利用する値（大型）
        /// </summary>
        private const int _offsetLargeX = 6;


        private List<TroopDataModel.FrontViewMember> _frontViewMember = null;

        /// <summary>
        ///     バトルシーンプレビュー表示。
        /// </summary>
        /// <
        /// <remarks>
        ///     EditorのBattleScenePreviewクラスから呼ぶメソッド。
        /// </remarks>
        public void RenderBattleScenePreview(string troopId) {
            _isRenderBattleScenePreview = true;

            try
            {
                gameObject.SetActive(true);
                DataManager.Self().SetTroopForBattle(new GameTroop(troopId), true);

                //戦闘シーンのプレビューの場合
                if (troopId == TroopDataModel.TROOP_PREVIEW)
                {
                    //バトルメニューの生成
                    if (_battleWindowObj == null)
                    {
                        _battleWindowObj = new WindowInitialize().Create(gameObject);
                        _battleWindowObj.transform.SetParent(gameObject.transform.parent);
                        _battleWindowObj.transform.localPosition = new Vector3(0f,0f,0f);
                    }
                
                    var logWindow = _battleWindowObj.transform.Find("WindowBattleLog").GetComponent<WindowBattleLog>();
                    var statusWindow = _battleWindowObj.transform.Find("WindowBattleStatus").GetComponent<WindowBattleStatus>();
                    var partyCommandWindow = _battleWindowObj.transform.Find("WindowPartyCommand").GetComponent<WindowPartyCommand>();
                    var actorCommandWindow = _battleWindowObj.transform.Find("WindowActorCommand").GetComponent<WindowActorCommand>();
                    var helpWindow = _battleWindowObj.transform.Find("WindowHelp").GetComponent<WindowHelp>();
                    var skillWindow = _battleWindowObj.transform.Find("WindowBattleSkill").GetComponent<WindowBattleSkill>();
                    var itemWindow = _battleWindowObj.transform.Find("WindowBattleItem").GetComponent<WindowBattleItem>();
                    var actorWindow = _battleWindowObj.transform.Find("WindowBattleActor").GetComponent<WindowBattleActor>();
                    var enemyWindow = _battleWindowObj.transform.Find("WindowBattleEnemy").GetComponent<WindowBattleEnemy>();
                    var messageWindow = _battleWindowObj.transform.Find("WindowMessage").GetComponent<WindowMessage>();

                    logWindow.gameObject.SetActive(false);
                    helpWindow.gameObject.SetActive(false);
                    skillWindow.gameObject.SetActive(false);
                    itemWindow.gameObject.SetActive(false);
                    enemyWindow.gameObject.SetActive(false);
                    messageWindow.gameObject.SetActive(false);
                    partyCommandWindow.gameObject.SetActive(false);
                    actorWindow.gameObject.SetActive(false);
                    
                    //ステータス表示部分、設定
                    statusWindow.Initialize();
                    statusWindow.Open();
                    
                    actorCommandWindow.gameObject.SetActive(true);
                    actorCommandWindow.Initialize();
                    actorCommandWindow.SetBattlePreviewMode(DataManager.Self().GetGameParty().Actors[0]);
                    actorCommandWindow.Show();
                    actorCommandWindow.Select(0);
                    actorCommandWindow.Open();
                }

                _systemdata = DataManager.Self().GetSystemDataModel();

                CreateBattleback();
                CreateEnemies();

                if (troopId == TroopDataModel.TROOP_PREVIEW)
                {
                    CreateActors(true);
                }

                foreach (var enemySprite in _enemySprites) enemySprite.Update();

                transform.Find("FrontView").GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
                transform.Find("SideView").GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);

                // 存在していたアクターは移動済みにする
                for (int i = 0; i < _actorSprites.Count; i++)
                {
                    _actorSprites[i].SetEndEntryMotionPreview();
                }
            }
            catch (Exception)
            {
            }

            _isRenderBattleScenePreview = false;
        }
        
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        public async void Initialize() {
            _systemdata = DataManager.Self().GetSystemDataModel();
            gameObject.SetActive(true);

            CreateBattleback();
            CreateEnemies();
            CreateActors();

            transform.Find("FrontView").GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
            transform.Find("SideView").GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);

            BattleManager.SetSpriteset(this);
            await Task.Delay(10);
        }

        /// <summary>
        /// Update処理
        /// </summary>
        public void UpdateTimeHandler() {
        }

        /// <summary>
        /// 背景画像設定
        /// </summary>
        public void CreateBattleback() {
            _back1Sprite.sprite = ImageManager.LoadBattleback1(Battleback1Name());
            _back2Sprite.sprite = ImageManager.LoadBattleback2(Battleback2Name());

            if (_systemdata.battleScene.viewType == 1)
            {
                //サイドビューの場合は、180px上に上げる
                _back2Sprite.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 180);
            }
        }
        
        /// <summary>
        /// 背景画像1の名称返却
        /// </summary>
        /// <returns></returns>
        public string Battleback1Name() {
            return
                new string[]
                {
                    // イベントに設定した背景優先。
                    BattleSceneTransition.Instance.EventMapBackgroundImage1,
                    DataManager.Self().GetGameTroop().Troop.backImage1,
                    BattleSceneTransition.Instance.EncounterDataBackgroundImage1,
                }.FirstOrDefault(s => !string.IsNullOrEmpty(s)) ?? string.Empty;
        }

        /// <summary>
        /// 背景画像2の名称返却
        /// </summary>
        /// <returns></returns>
        public string Battleback2Name() {
            return
                new string[]
                {
                    // イベントに設定した背景優先。
                    BattleSceneTransition.Instance.EventMapBackgroundImage2,
                    DataManager.Self().GetGameTroop().Troop.backImage2,
                    BattleSceneTransition.Instance.EncounterDataBackgroundImage2,
                }.FirstOrDefault(s => !string.IsNullOrEmpty(s)) ?? string.Empty;
        }


        /// <summary>
        /// 敵生成
        /// </summary>
        public void CreateEnemies() {
            if (originEnemy == null)
                return;

            //サイド、フロントビュー側の表示切替
            var parent = _sideObject;
            if (_systemdata.battleScene.viewType == 0)
            {
                parent = _frontObject;
                _sideObject.gameObject.SetActive(false);
            }
            else
            {
                parent = _sideObject;
                _frontObject.gameObject.SetActive(false);
            }

            //各データ取得
            var troopData = DataManager.Self().GetGameTroop().Troop;
            var enemies = DataManager.Self().GetGameTroop().Members();
            var enemyData = DataManager.Self().GetEnemyDataModels();

            _frontViewMember = new List<TroopDataModel.FrontViewMember>();

            for(int i = 0;i < troopData.frontViewMembers.Count; i++)
            {
                _frontViewMember.Add(troopData.frontViewMembers[i].DataClone());
            }


            //リスト初期化
            var sprites = new List<SpriteEnemy>();

            //サイドビューだった場合自動配列を実施
            if (_systemdata.battleScene.viewType == 1)
            {
                for (var i = 0; i < enemies.Count; i++)
                {
                    //敵データ取得
                    var sideViewMember = troopData.sideViewMembers[i];
                    EnemyDataModel enemyDataWork = null;
                    for (int i2 = 0; i2 < enemyData.Count; i2++)
                        if (enemyData[i2].id == sideViewMember.enemyId)
                        {
                            enemyDataWork = enemyData[i2];
                            break;
                        }

                    //配置場所の決定
                    parent = SetPositionObj(sideViewMember.position1, sideViewMember.position2, enemyDataWork.images.autofit, enemyDataWork.images.autofitPattern);

                    //敵画像配置
                    var enemy = (GameEnemy) enemies[i];
                    var enemySprite = Instantiate(originEnemy, originEnemy.transform.parent, true);
                    enemySprite.Initialize(enemy);
                    enemySprite.UpdateBitmap();

                    sprites.Add(enemySprite);
                    enemySprite.transform.SetParent(parent.transform);

                    //上揃え、下揃えの適用
                    Vector2 position = enemySprite.transform.parent.GetComponent<RectTransform>().anchoredPosition;


                    //サイズ調節
                    if (enemyDataWork.images.autofit == 0)
                    {
                        //位置を調整する
                        //一旦、自動Fitを外す
                        enemySprite.GetComponent<AspectRatioFitter>().enabled = false;
                        enemySprite.GetComponent<Image>().preserveAspect = false;
                        float enemyHeight = enemySprite.GetComponent<RectTransform>().sizeDelta.y;
                        float enemyWidth = enemySprite.GetComponent<RectTransform>().sizeDelta.x;

                        if (enemyDataWork.images.autofitPattern == 0)
                        {
                            //小型
                            if (enemyWidth < enemyHeight)
                                enemyHeight = _sizeSmall;
                            else
                                enemyHeight = 1.0f * _sizeSmall / enemyWidth;
                        }
                        else if (enemyDataWork.images.autofitPattern == 1)
                        {
                            //大型
                            if (enemyWidth < enemyHeight)
                                enemyHeight = _sizeLarge;
                            else
                                enemyHeight = 1.0f * _sizeLarge / enemyWidth;
                        }
                        else if (enemyDataWork.images.autofitPattern == 2)
                        {
                            //ボス型
                            if (1.0f * _sizeBossWidth / enemyWidth > 1.0f * _sizeBossHeight / enemyHeight)
                                enemyHeight = _sizeBossHeight;
                            else
                                enemyHeight = 1.0f * _sizeBossWidth / _sizeBossWidth;
                        }

                        float height = 0;
                        if (enemyDataWork.images.autofitPattern == 0)
                            height = _sizeSmall / 2.0f;
                        else if (enemyDataWork.images.autofitPattern == 1)
                            height = _sizeLarge / 2.0f;
                        else if (enemyDataWork.images.autofitPattern == 2)
                            height = _sizeBossHeight / 2.0f;

                        if (enemyDataWork.images.battleAlignment == 1)
                        {
                            //下揃え
                            enemySprite.transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                                position.x, position.y + enemyHeight / 2.0f - height);
                        }
                        else if (enemyDataWork.images.battleAlignment == 2)
                        {
                            //上揃え
                            enemySprite.transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                                position.x, position.y - enemyHeight / 2.0f + height);
                        }

                        //自動調節
                        enemySprite.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                        enemySprite.GetComponent<AspectRatioFitter>().enabled = true;
                        enemySprite.GetComponent<Image>().preserveAspect = true;
                    }
                    else
                    {
                        //自動調節しない
                        //一旦、自動Fitを外す
                        enemySprite.GetComponent<AspectRatioFitter>().enabled = false;
                        enemySprite.GetComponent<Image>().preserveAspect = false;

                        //親のサイズを変更する
                        float enemyWidth = enemySprite.GetComponent<RectTransform>().sizeDelta.x * enemyDataWork.images.scale / 100.0f;
                        float enemyHeight = enemySprite.GetComponent<RectTransform>().sizeDelta.y * enemyDataWork.images.scale / 100.0f;
                        float enemySize = enemyWidth < enemyHeight ? enemyHeight : enemyWidth;

                        //大きい方の、正方形にする
                        enemySprite.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(enemySize, enemySize);

                        //位置を調整しなおす
                        float height = 0;
                        if (enemyDataWork.images.autofitPattern == 0)
                            height = _sizeSmall / 2.0f;
                        else if (enemyDataWork.images.autofitPattern == 1)
                            height = _sizeLarge / 2.0f;
                        else if (enemyDataWork.images.autofitPattern == 2)
                            height = _sizeBossHeight / 2.0f;

                        if (enemyDataWork.images.battleAlignment == 1)
                        {
                            //下揃え
                            enemySprite.transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                                position.x, position.y + enemyHeight / 2.0f - height);
                        }
                        else if (enemyDataWork.images.battleAlignment == 2)
                        {
                            //上揃え
                            enemySprite.transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                                position.x, position.y - enemyHeight / 2.0f + height);
                        }

                        //親のサイズを変更後に、改めて自動Fitにする
                        enemySprite.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                        enemySprite.GetComponent<AspectRatioFitter>().enabled = true;
                        enemySprite.GetComponent<Image>().preserveAspect = true;
                    }

                    //エネミーオブジェクトの調整部位
                    enemySprite.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    //敵の透過度の初期値を255にする
                    enemySprite.Opacity = 255;
                }
                //傾斜させる
                sideEnemyInclined(_systemdata.battleScene.sideEnemyInclined);
                //敵をソートする
                sprites.Sort(CompareEnemySprite);
            }
            else
            {
                //敵一覧
                var gameEnemies = DataManager.Self().GetGameTroop().Enemies;
                //gameEnemies.Sort((a, b) => a.PositionIndex - b.PositionIndex);

                //画面の左上を0,0とするための補正用に、画面サイズを取得
                float ypos = DataManager.Self().GetSystemDataModel().DisplaySize[DataManager.Self().GetSystemDataModel().displaySize].y / 2.0f;

                //敵の配置場所は、全ての敵の幅の合算値から算出する
                //計算により敵の配置位置を動的に変更するため、出現位置（1～8）は、あくまでも敵の並び順にのみ影響する
                //配置位置は最後に調整するため、敵の出現位置を配列で保持しておく
                List<FrontEnemyDisplayData> positions = new List<FrontEnemyDisplayData>();

                //全ての敵の描画を行う
                for (int i = 0; i < gameEnemies.Count; i++)
                {
                    //配置場所の決定
                    var frontViewMember = troopData.frontViewMembers[i];
                    parent = _frontObject.transform.Find("Items" + i).gameObject;
                    EnemyDataModel enemyDataWork = null;
                    for (int i2 = 0; i2 < enemyData.Count; i2++)
                        if (enemyData[i2].id == frontViewMember.enemyId)
                        {
                            enemyDataWork = enemyData[i2];
                            break;
                        }

                    //敵画像配置
                    var enemy = (GameEnemy) enemies[i];
                    var enemySprite = Instantiate(originEnemy, originEnemy.transform.parent, true);
                    enemySprite.Initialize(enemy);
                    enemySprite.UpdateBitmap();

#if UNITY_EDITOR
                    //プレビューで使用する
                    if (!EditorApplication.isPlaying) enemySprite.InitVisibility();
#endif
                    sprites.Add(enemySprite);
                    enemySprite.transform.SetParent(parent.transform);

                    //配置場所を保持
                    FrontEnemyDisplayData frontEnemyDisplayData = new FrontEnemyDisplayData(frontViewMember.position, 0, 0, enemySprite, i);

                    //自動調節しない場合の値
                    float enemyWidth = 0;
                    float enemyHeight = 0;
                    float enemySize = 0;

                    //サイズは、フロントビューでの小型/大型/ボス型に合わせる
                    if (enemyDataWork.images.autofit == 0)
                    {
                        //自動調節
                        //小型
                        if (enemyDataWork.images.autofitPattern == 0)
                        {
                            parent.GetComponent<RectTransform>().sizeDelta = new Vector2(_sizeSmall, _sizeSmall);
                            frontEnemyDisplayData.width = _sizeSmall;
                        }
                        //大型
                        else if (enemyDataWork.images.autofitPattern == 1)
                        {
                            parent.GetComponent<RectTransform>().sizeDelta = new Vector2(_sizeLarge, _sizeLarge);
                            frontEnemyDisplayData.width = _sizeLarge;
                        }
                        //ボス型
                        else if (enemyDataWork.images.autofitPattern == 2)
                        {
                            parent.GetComponent<RectTransform>().sizeDelta = new Vector2(_sizeBossWidth, _sizeBossHeight);
                            frontEnemyDisplayData.width = _sizeBossWidth;
                        }
                        enemySprite.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                        enemySprite.GetComponent<AspectRatioFitter>().enabled = true;
                        enemySprite.GetComponent<Image>().preserveAspect = true;
                    }
                    else
                    {
                        //自動調節しない
                        //一旦、自動Fitを外す
                        enemySprite.GetComponent<AspectRatioFitter>().enabled = false;
                        enemySprite.GetComponent<Image>().preserveAspect = false;

                        enemyWidth = enemySprite.GetComponent<RectTransform>().sizeDelta.x * enemyDataWork.images.scale / 100.0f;
                        enemyHeight = enemySprite.GetComponent<RectTransform>().sizeDelta.y * enemyDataWork.images.scale / 100.0f;
                        enemySize = enemyWidth < enemyHeight ? enemyHeight : enemyWidth;

                        //大きい方の、正方形にする
                        parent.GetComponent<RectTransform>().sizeDelta = new Vector2(enemySize, enemySize);

                        //親のサイズを変更後に、改めて自動Fitにする
                        enemySprite.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                        enemySprite.GetComponent<AspectRatioFitter>().enabled = true;
                        enemySprite.GetComponent<Image>().preserveAspect = true;

                        frontEnemyDisplayData.width = (int) (enemyWidth);
                    }

                    //下揃えや上揃えは、y座標の位置を調整することで行う
                    int positionY = 0;
                    if (enemyDataWork.images.battleAlignment == 0)
                    {
                        //自動
                        //小型
                        if (enemyDataWork.images.autofitPattern == 0)
                            positionY = _sizeSmall / 2;
                        //大型
                        else if (enemyDataWork.images.autofitPattern == 1)
                            positionY = _sizeLarge / 2;
                        //ボス型
                        else if (enemyDataWork.images.autofitPattern == 2)
                            positionY = _sizeBossHeight / 2;

                        if (-1 * _systemdata.battleScene.frontEnemyPositionY + positionY > 0)
                            frontEnemyDisplayData.positionY = (int) ypos;
                        else
                            frontEnemyDisplayData.positionY = (int) (ypos - _systemdata.battleScene.frontEnemyPositionY + positionY);
                    }
                    if (enemyDataWork.images.battleAlignment == 1)
                    {
                        //下揃え
                        if (enemyDataWork.images.autofit == 0)
                        {
                            //自動調節する
                            //小型
                            if (enemyDataWork.images.autofitPattern == 0)
                                positionY = _sizeSmall / 2;
                            //大型
                            else if (enemyDataWork.images.autofitPattern == 1)
                                positionY = _sizeLarge / 2;
                            //ボス型
                            else if (enemyDataWork.images.autofitPattern == 2)
                                positionY = _sizeBossHeight / 2;
                        }
                        else
                        {
                            //自動調節しない
                            float height = enemyHeight;
                            positionY = (int)(height / 2);
                        }

                        frontEnemyDisplayData.positionY = (int) (ypos - _systemdata.battleScene.frontEnemyPositionY + positionY);
                    }
                    else if (enemyDataWork.images.battleAlignment == 2)
                    {
                        //上揃え
                        //調整値の算出
                        //小型
                        if (enemyDataWork.images.autofitPattern == 0)
                            positionY = _sizeSmall;
                        //大型
                        else if (enemyDataWork.images.autofitPattern == 1)
                            positionY = _sizeLarge;
                        //ボス型
                        else if (enemyDataWork.images.autofitPattern == 2)
                            positionY = _sizeBossHeight;

                        int posA = -1 * _systemdata.battleScene.frontEnemyPositionY + positionY;
                        if (posA > 0) posA = 0;

                        //画像表示サイズ
                        if (enemyDataWork.images.autofit == 0)
                        {
                            //自動調節する
                            //小型
                            if (enemyDataWork.images.autofitPattern == 0)
                                positionY = _sizeSmall / 2;
                            //大型
                            else if (enemyDataWork.images.autofitPattern == 1)
                                positionY = _sizeLarge / 2;
                            //ボス型
                            else if (enemyDataWork.images.autofitPattern == 2)
                                positionY = _sizeBossHeight / 2;
                        }
                        else
                        {
                            //自動調節しない
                            float height = enemyHeight;
                            positionY = (int) (height / 2);
                        }

                        frontEnemyDisplayData.positionY = (int) (ypos + posA - positionY);
                    }

                    //座標設定用に保持
                    positions.Add(frontEnemyDisplayData);
#if UNITY_EDITOR
                    //プレビューで使用する
                    if (!EditorApplication.isPlaying) enemySprite.Opacity = 255;
#endif
                }

                //X座標を調整する
                //position順に並べ替え
                positions.Sort((a, b) => { return a.position - b.position; });

                //敵の幅の合算値
                int totalWidth = 0;
                foreach (FrontEnemyDisplayData data in positions) totalWidth += data.width;

                //調整値
                float paramA = 0;
                float paramB = 0;

                //開始位置は、敵の幅の合算値 / 2
                float startPosX = (int) (-1 * totalWidth / 2.0f);

                if (totalWidth > 1720)
                {
                    paramA = (1.0f * totalWidth - 1720) / (positions.Count - 1);
                    paramB = ((1.0f * totalWidth - 1720) - paramA * (1.0f * positions.Count - 1)) / 2;
                    startPosX = (-1 * DataManager.Self().GetSystemDataModel().DisplaySize[DataManager.Self().GetSystemDataModel().displaySize].x / 2.0f) + 100 - paramB;
                }

                //X座標を設定
                float sub = 0;
                for (int pos = 0; pos < positions.Count; pos++)
                {
                    //座標を設定
                    positions[pos].enemySprite.transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                        startPosX + positions[pos].width / 2.0f - sub,
                        positions[pos].positionY);

                    //次の敵キャラクターの座標計算用
                    startPosX += positions[pos].width - sub;
                    sub = paramA;

                    //並べ替え用
                    sprites.Add(positions[pos].enemySprite);
                }
            }

            _enemySprites = sprites;

            if (_isRenderBattleScenePreview)
            {
                DestroyImmediate(originEnemy.gameObject);
            }
            else
            {
                Destroy(originEnemy.gameObject);
            }
        }

        /// <summary>
        /// 敵の変身時に配置、大きさを変える
        /// </summary>
        public void TransformEnemy(GameEnemy gameEnemy) {

    
            //サイド、フロントビュー側の表示切替
            var parent = _sideObject;
            if (_systemdata.battleScene.viewType == 0)
            {
                parent = _frontObject;
                _sideObject.gameObject.SetActive(false);
            }
            else
            {
                parent = _sideObject;
                _frontObject.gameObject.SetActive(false);
            }

            //各データ取得
            var troopData = DataManager.Self().GetGameTroop().Troop;
            var enemies = DataManager.Self().GetGameTroop().Members();
            var enemyData = DataManager.Self().GetEnemyDataModels();

            //リスト初期化
            //var sprites = new List<SpriteEnemy>();

            //サイドビューだった場合自動配列を実施
            if (_systemdata.battleScene.viewType == 1)
            {

                //敵データ取得
                TroopDataModel.SideViewMember sideViewMember = troopData.sideViewMembers[gameEnemy.Index()]; ;

                EnemyDataModel enemyDataWork = null;
                for (int i2 = 0; i2 < enemyData.Count; i2++)
                    if (enemyData[i2].id == gameEnemy.EnemyId)
                    {
                        enemyDataWork = enemyData[i2];
                        break;
                    }

                //配置場所の決定
                parent = SetPositionObj(sideViewMember.position1, sideViewMember.position2, enemyDataWork.images.autofit, enemyDataWork.images.autofitPattern);

                //敵画像配置
                var enemy = gameEnemy;

                SpriteEnemy enemySprite = null;//Instantiate(originEnemy, originEnemy.transform.parent, true);
                for (var i3 = 0; i3 < _enemySprites.Count; i3++)
                {
                    if (gameEnemy.EnemyId == _enemySprites[i3].EnemyId())
                    {
                        enemySprite = _enemySprites[i3];
                        break;
                    }
                }
                enemySprite.transform.SetParent(parent.transform);


                //上揃え、下揃えの適用
                Vector2 position = enemySprite.transform.parent.GetComponent<RectTransform>().anchoredPosition;


                //サイズ調節
                if (enemyDataWork.images.autofit == 0)
                {

                    //位置を調整する
                    //一旦、自動Fitを外す
                    enemySprite.GetComponent<AspectRatioFitter>().enabled = false;
                    enemySprite.GetComponent<Image>().preserveAspect = false;
                    enemySprite.GetComponent<Image>().SetNativeSize();
                    float enemyHeight = enemySprite.GetComponent<RectTransform>().sizeDelta.y;
                    float enemyWidth = enemySprite.GetComponent<RectTransform>().sizeDelta.x;

                    if (enemyDataWork.images.autofitPattern == 0)
                    {
                        //小型
                        if (enemyWidth < enemyHeight)
                            enemyHeight = _sizeSmall;
                        else
                            enemyHeight = 1.0f * _sizeSmall / enemyWidth;
                    }
                    else if (enemyDataWork.images.autofitPattern == 1)
                    {
                        //大型
                        if (enemyWidth < enemyHeight)
                            enemyHeight = _sizeLarge;
                        else
                            enemyHeight = 1.0f * _sizeLarge / enemyWidth;
                    }
                    else if (enemyDataWork.images.autofitPattern == 2)
                    {
                        //ボス型
                        if (1.0f * _sizeBossWidth / enemyWidth > 1.0f * _sizeBossHeight / enemyHeight)
                            enemyHeight = _sizeBossHeight;
                        else
                            enemyHeight = 1.0f * _sizeBossWidth / _sizeBossWidth;
                    }

                    float height = 0;
                    if (enemyDataWork.images.autofitPattern == 0)
                        height = _sizeSmall / 2.0f;
                    else if (enemyDataWork.images.autofitPattern == 1)
                        height = _sizeLarge / 2.0f;
                    else if (enemyDataWork.images.autofitPattern == 2)
                        height = _sizeBossHeight / 2.0f;

                    if (enemyDataWork.images.battleAlignment == 1)
                    {
                        //下揃え
                        enemySprite.transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                            position.x, position.y + enemyHeight / 2.0f - height);
                    }
                    else if (enemyDataWork.images.battleAlignment == 2)
                    {
                        //上揃え
                        enemySprite.transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                            position.x, position.y - enemyHeight / 2.0f + height);
                    }

                    //自動調節
                    enemySprite.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                    enemySprite.GetComponent<AspectRatioFitter>().enabled = true;
                    enemySprite.GetComponent<Image>().preserveAspect = true;
                }
                else
                {
                    //自動調節しない
                    //一旦、自動Fitを外す
                    enemySprite.GetComponent<AspectRatioFitter>().enabled = false;
                    enemySprite.GetComponent<Image>().preserveAspect = false;
                    enemySprite.GetComponent<Image>().SetNativeSize();

                    //親のサイズを変更する
                    float enemyWidth = enemySprite.GetComponent<RectTransform>().sizeDelta.x * enemyDataWork.images.scale / 100.0f;
                    float enemyHeight = enemySprite.GetComponent<RectTransform>().sizeDelta.y * enemyDataWork.images.scale / 100.0f;
                    float enemySize = enemyWidth < enemyHeight ? enemyHeight : enemyWidth;

                    //大きい方の、正方形にする
                    enemySprite.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(enemySize, enemySize);

                    //位置を調整しなおす
                    float height = 0;
                    if (enemyDataWork.images.autofitPattern == 0)
                        height = _sizeSmall / 2.0f;
                    else if (enemyDataWork.images.autofitPattern == 1)
                        height = _sizeLarge / 2.0f;
                    else if (enemyDataWork.images.autofitPattern == 2)
                        height = _sizeBossHeight / 2.0f;

                    if (enemyDataWork.images.battleAlignment == 1)
                    {
                        //下揃え
                        enemySprite.transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                            position.x, position.y + enemyHeight / 2.0f - height);
                    }
                    else if (enemyDataWork.images.battleAlignment == 2)
                    {
                        //上揃え
                        enemySprite.transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                            position.x, position.y - enemyHeight / 2.0f + height);
                    }

                    //親のサイズを変更後に、改めて自動Fitにする
                    enemySprite.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                    enemySprite.GetComponent<AspectRatioFitter>().enabled = true;
                    enemySprite.GetComponent<Image>().preserveAspect = true;
                }

                //エネミーオブジェクトの調整部位
                enemySprite.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
            else
            {
                //敵一覧
                var gameEnemies = DataManager.Self().GetGameTroop().Enemies;
                //gameEnemies.Sort((a, b) => a.PositionIndex - b.PositionIndex);

                //画面の左上を0,0とするための補正用に、画面サイズを取得
                float ypos = DataManager.Self().GetSystemDataModel().DisplaySize[DataManager.Self().GetSystemDataModel().displaySize].y / 2.0f;

                //敵の配置場所は、全ての敵の幅の合算値から算出する
                //計算により敵の配置位置を動的に変更するため、出現位置（1～8）は、あくまでも敵の並び順にのみ影響する
                //配置位置は最後に調整するため、敵の出現位置を配列で保持しておく
                List<FrontEnemyDisplayData> positions = new List<FrontEnemyDisplayData>();

                //全ての敵の描画を行う
                for (int i = 0; i < gameEnemies.Count; i++)
                {
                    //配置場所の決定
                    var frontViewMember = _frontViewMember[i];
                    parent = _frontObject.transform.Find("Items" + i).gameObject;
                    EnemyDataModel enemyDataWork = null;

                    //変身後のみ、すげ替える
                    if (i == gameEnemy.Index())
                    {
                        for (int i2 = 0; i2 < enemyData.Count; i2++)
                            if (enemyData[i2].id == gameEnemy.EnemyId)
                            {
                                enemyDataWork = enemyData[i2];
                                _frontViewMember[i].enemyId = gameEnemy.EnemyId;
                                break;
                            }
                    }
                    else
                    {
                        for (int i2 = 0; i2 < enemyData.Count; i2++)
                            if (enemyData[i2].id == frontViewMember.enemyId)
                            {
                                enemyDataWork = enemyData[i2];
                                break;
                            }
                    }

                    //敵画像配置
                    var enemy = (GameEnemy) enemies[i];
                    var enemySprite = _enemySprites[i];
                    enemySprite.Initialize(enemy);
                    enemySprite.UpdateBitmap();

#if UNITY_EDITOR
                    //プレビューで使用する
                    if (!EditorApplication.isPlaying) enemySprite.InitVisibility();
#endif
                    //sprites.Add(enemySprite);
                    enemySprite.transform.SetParent(parent.transform);

                    //配置場所を保持
                    FrontEnemyDisplayData frontEnemyDisplayData = new FrontEnemyDisplayData(frontViewMember.position, 0, 0, enemySprite, i);

                    //自動調節しない場合の値
                    float enemyWidth = 0;
                    float enemyHeight = 0;
                    float enemySize = 0;

                    //一旦、自動Fitを外す
                    enemySprite.GetComponent<AspectRatioFitter>().enabled = false;
                    enemySprite.GetComponent<Image>().preserveAspect = false;
                    enemySprite.GetComponent<Image>().SetNativeSize();


                    //サイズは、フロントビューでの小型/大型/ボス型に合わせる
                    if (enemyDataWork.images.autofit == 0)
                    {
                        //自動調節
                        //小型
                        if (enemyDataWork.images.autofitPattern == 0)
                        {
                            parent.GetComponent<RectTransform>().sizeDelta = new Vector2(_sizeSmall, _sizeSmall);
                            frontEnemyDisplayData.width = _sizeSmall;
                        }
                        //大型
                        else if (enemyDataWork.images.autofitPattern == 1)
                        {
                            parent.GetComponent<RectTransform>().sizeDelta = new Vector2(_sizeLarge, _sizeLarge);
                            frontEnemyDisplayData.width = _sizeLarge;
                        }
                        //ボス型
                        else if (enemyDataWork.images.autofitPattern == 2)
                        {
                            parent.GetComponent<RectTransform>().sizeDelta = new Vector2(_sizeBossWidth, _sizeBossHeight);
                            frontEnemyDisplayData.width = _sizeBossWidth;
                        }
                        enemySprite.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                        enemySprite.GetComponent<AspectRatioFitter>().enabled = true;
                        enemySprite.GetComponent<Image>().preserveAspect = true;
                    }
                    else
                    {
                        //自動調節しない
                        enemyWidth = enemySprite.GetComponent<RectTransform>().sizeDelta.x * enemyDataWork.images.scale / 100.0f;
                        enemyHeight = enemySprite.GetComponent<RectTransform>().sizeDelta.y * enemyDataWork.images.scale / 100.0f;
                        enemySize = enemyWidth < enemyHeight ? enemyHeight : enemyWidth;

                        //大きい方の、正方形にする
                        parent.GetComponent<RectTransform>().sizeDelta = new Vector2(enemySize, enemySize);

                        //親のサイズを変更後に、改めて自動Fitにする
                        enemySprite.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                        enemySprite.GetComponent<AspectRatioFitter>().enabled = true;
                        enemySprite.GetComponent<Image>().preserveAspect = true;

                        frontEnemyDisplayData.width = (int) (enemyWidth);
                    }

                    //下揃えや上揃えは、y座標の位置を調整することで行う
                    int positionY = 0;
                    if (enemyDataWork.images.battleAlignment == 0)
                    {
                        //自動
                        //小型
                        if (enemyDataWork.images.autofitPattern == 0)
                            positionY = _sizeSmall / 2;
                        //大型
                        else if (enemyDataWork.images.autofitPattern == 1)
                            positionY = _sizeLarge / 2;
                        //ボス型
                        else if (enemyDataWork.images.autofitPattern == 2)
                            positionY = _sizeBossHeight / 2;

                        if (-1 * _systemdata.battleScene.frontEnemyPositionY + positionY > 0)
                            frontEnemyDisplayData.positionY = (int) ypos;
                        else
                            frontEnemyDisplayData.positionY = (int) (ypos - _systemdata.battleScene.frontEnemyPositionY + positionY);
                    }
                    if (enemyDataWork.images.battleAlignment == 1)
                    {
                        //下揃え
                        if (enemyDataWork.images.autofit == 0)
                        {
                            //自動調節する
                            //小型
                            if (enemyDataWork.images.autofitPattern == 0)
                                positionY = _sizeSmall / 2;
                            //大型
                            else if (enemyDataWork.images.autofitPattern == 1)
                                positionY = _sizeLarge / 2;
                            //ボス型
                            else if (enemyDataWork.images.autofitPattern == 2)
                                positionY = _sizeBossHeight / 2;
                        }
                        else
                        {
                            //自動調節しない
                            float height = enemyHeight;
                            positionY = (int) (height / 2);
                        }

                        frontEnemyDisplayData.positionY = (int) (ypos - _systemdata.battleScene.frontEnemyPositionY + positionY);
                    }
                    else if (enemyDataWork.images.battleAlignment == 2)
                    {
                        //上揃え
                        //調整値の算出
                        //小型
                        if (enemyDataWork.images.autofitPattern == 0)
                            positionY = _sizeSmall;
                        //大型
                        else if (enemyDataWork.images.autofitPattern == 1)
                            positionY = _sizeLarge;
                        //ボス型
                        else if (enemyDataWork.images.autofitPattern == 2)
                            positionY = _sizeBossHeight;

                        int posA = -1 * _systemdata.battleScene.frontEnemyPositionY + positionY;
                        if (posA > 0) posA = 0;

                        //画像表示サイズ
                        if (enemyDataWork.images.autofit == 0)
                        {
                            //自動調節する
                            //小型
                            if (enemyDataWork.images.autofitPattern == 0)
                                positionY = _sizeSmall / 2;
                            //大型
                            else if (enemyDataWork.images.autofitPattern == 1)
                                positionY = _sizeLarge / 2;
                            //ボス型
                            else if (enemyDataWork.images.autofitPattern == 2)
                                positionY = _sizeBossHeight / 2;
                        }
                        else
                        {
                            //自動調節しない
                            float height = enemyHeight;
                            positionY = (int) (height / 2);
                        }

                        frontEnemyDisplayData.positionY = (int) (ypos + posA - positionY);
                    }

                    //座標設定用に保持
                    positions.Add(frontEnemyDisplayData);
#if UNITY_EDITOR
                    //プレビューで使用する
                    if (!EditorApplication.isPlaying) enemySprite.Opacity = 255;
#endif
                }

                //X座標を調整する
                //position順に並べ替え
                positions.Sort((a, b) => { return a.position - b.position; });

                //敵の幅の合算値
                int totalWidth = 0;
                foreach (FrontEnemyDisplayData data in positions) totalWidth += data.width;

                //調整値
                float paramA = 0;
                float paramB = 0;

                //開始位置は、敵の幅の合算値 / 2
                float startPosX = (int) (-1 * totalWidth / 2.0f);

                if (totalWidth > 1720)
                {
                    paramA = (1.0f * totalWidth - 1720) / (positions.Count - 1);
                    paramB = ((1.0f * totalWidth - 1720) - paramA * (1.0f * positions.Count - 1)) / 2;
                    startPosX = (-1 * DataManager.Self().GetSystemDataModel().DisplaySize[DataManager.Self().GetSystemDataModel().displaySize].x / 2.0f) + 100 - paramB;
                }

                //X座標を設定
                float sub = 0;
                for (int pos = 0; pos < positions.Count; pos++)
                {
                    //座標を設定
                    positions[pos].enemySprite.transform.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                        startPosX + positions[pos].width / 2.0f - sub,
                        positions[pos].positionY);

                    //次の敵キャラクターの座標計算用
                    startPosX += positions[pos].width - sub;
                    sub = paramA;
                }
            }
        }


        /// <summary>
        /// 敵データの整列 コマンド入力時に並ぶ順
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public int CompareEnemySprite(SpriteEnemy a, SpriteEnemy b) {
            if (a.Y != b.Y)
                return (int) (a.Y - b.Y);
            return 1;
        }

        /// <summary>
        /// 敵データの整列 コマンド入力時に並ぶ順
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public int CompareEnemySpriteFront(SpriteEnemy a, SpriteEnemy b) {
            if (a.X != b.X)
                return (int) (a.X - b.X);
            return 1;
        }

        /// <summary>
        /// アクター生成
        /// </summary>
        public void CreateActors(bool battlePreview = false) {
            var battlers = DataManager.Self().GetGameParty().BattleMembers();

            // 存在するアクターを保持して削除
            var existActors = new List<string>();
            if (_actorSprites.Count > 0)
            {
                if (battlePreview) 
                    return;
                foreach (var actor in _actorSprites)
                {
                    existActors.Add(actor.ActorId());
                    DestroyImmediate(actor.gameObject);
                }
            }
            _actorSprites = new List<SpriteActor>();

            //サイドビューの場合SDキャラクターの表示を行う
            int num = 0;
            if (_systemdata.battleScene.viewType == 1)
                for (var i = 0; i < battlers.Count; i++)
                {
                    var actor = battlers.ElementAtOrDefault(i);
                    var actorSprite = Instantiate(originActor, originActor.transform.parent, true);
                    actorSprite.gameObject.SetActive(true);
                    actorSprite.Initialize(actor);
                    var pos = actorSprite.transform.position;

                    pos.z -= (float) (i * 0.01);
                    actorSprite.transform.position = pos;
                    _actorSprites.Add(actorSprite);

                    if (!battlers[i].IsEscaped)
                    {
                        _actorSprites[i].SetActorHome(num++);
                    }

                    // 存在していたアクターは移動済みにする
                    for (int i2 = 0; i2 < existActors.Count; i2++)
                        if (existActors[i2] == actorSprite.ActorId())
                        {
                            actorSprite.SetEndEntryMotion();
                            break;
                        }
                }
        }

        public void UpdateActorsPosition() {
            //サイドビューの場合SDキャラクターの表示を行う
            if (_systemdata.battleScene.viewType == 1)
            {
                int num = 0;
                for (int i = 0; i < _actorSprites.Count; i++)
                {
                    if (_actorSprites[i].IsEscaped())
                    {
                        continue;
                    }
                    _actorSprites[i].SetActorHome(num++);
                }
            }
        }

        /// <summary>
        /// アクターの表示非表示切り替え
        /// </summary>
        /// <param name="id"></param>
        public void UpdateActorHide(string id) {
            SpriteActor member = null;
            for (int i = 0; i < _actorSprites.Count; i++)
                if (_actorSprites[i].ActorId() == id)
                {
                    member = _actorSprites[i];
                    break;
                }
            member.UpdateHide();
        }

        /// <summary>
        /// 敵の表示非表示切り替え
        /// </summary>
        /// <param name="id"></param>
        public void UpdateEnemyHide(string id) {
            SpriteEnemy member = null;
            for (int i = 0; i < _enemySprites.Count; i++)
                if (_enemySprites[i].EnemyId() == id)
                {
                    member = _enemySprites[i];
                    break;
                }
            member.UpdateHide();
        }

        /// <summary>
        /// 敵を選択
        /// </summary>
        /// <param name="id"></param>
        public void SelectEnemy(GameEnemy enemy) {
            for (int i = 0; i < _enemySprites.Count; i++)
            {
                _enemySprites[i].ResetSelect();
                if (_enemySprites[i].GetGameEnemy() == enemy)
                {
                    _enemySprites[i].StartEffect("select");
                }
            }
        }

        /// <summary>
        /// 配置場所に該当する、親のGameObjectを返却する
        /// </summary>
        /// <param name="pos1"></param>
        /// <param name="pos2"></param>
        /// <returns></returns>
        private GameObject SetPositionObj(int pos1, int pos2, int autofit, int autofitPattern) {
            //前衛、後衛
            var p1 = pos1 == 0 ? 0 : 2;
            //中型またはボス型の場合
            if (autofitPattern > 0)
            {
                p1++;
            }

            //上中下
            var p2 = pos2 == 0 ? 0 : pos2 == 1 ? 2 : 4;
            //ボス型の場合
            if (autofitPattern == 2)
            {
                p2 = 5;
            }

            //配置位置のGameObjectを返却する
            return _sideObject.transform.Find("Items" + p1 + "/Items" + p2).gameObject;
        }
        
        /// <summary>
        /// 傾斜度の反映メソッド
        /// 傾斜度が中に入る
        /// エネミーの傾斜
        /// </summary>
        /// <param name="inclined"></param>
        private void sideEnemyInclined(int inclined) {
            //角度計算ではなく、X座標計算を行う
            //Items0から4までの傾斜を付ける
            //小型
            for (int pos1 = 0; pos1 <= 2; pos1+=2)
            {
                //上の敵
                Vector2 pos = transform.GetComponent<RectTransform>().anchoredPosition;
                pos = new Vector2(pos.x + _offsetSmallX * inclined, 0);
                _sideObject.transform.Find("Items" + pos1 + "/Items0").GetComponent<RectTransform>().anchoredPosition += pos;

                //中央の敵は変更無し

                //下の敵
                pos = transform.GetComponent<RectTransform>().anchoredPosition;
                pos = new Vector2(pos.x - _offsetSmallX * inclined, 0);
                _sideObject.transform.Find("Items" + pos1 + "/Items4").GetComponent<RectTransform>().anchoredPosition += pos;
            }
            //大型
            for (int pos1 = 1; pos1 <= 3; pos1 += 2)
            {
                //上の敵
                Vector2 pos = transform.GetComponent<RectTransform>().anchoredPosition;
                pos = new Vector2(pos.x + _offsetLargeX * inclined, 0);
                _sideObject.transform.Find("Items" + pos1 + "/Items0").GetComponent<RectTransform>().anchoredPosition += pos;

                //下の敵
                pos = transform.GetComponent<RectTransform>().anchoredPosition;
                pos = new Vector2(pos.x - _offsetLargeX * inclined, 0);
                _sideObject.transform.Find("Items" + pos1 + "/Items4").GetComponent<RectTransform>().anchoredPosition += pos;
            }
        }

        /// <summary>
        /// バトル画面に存在する全ての敵、アクターのSpriteBattleデータを返却
        /// </summary>
        /// <returns></returns>
        public List<SpriteBattler> BattlerSprites() {
            var list1 = _enemySprites.Aggregate(new List<SpriteBattler>(), (l, e) =>
            {
                l.Add(e);
                return l;
            });
            return _actorSprites.Aggregate(list1, (l, e) =>
            {
                l.Add(e);
                return l;
            });
        }

        /// <summary>
        /// バトル画面に存在する全ての敵、アクターの中で、エフェクト中のものが存在するかどうかを返却
        /// </summary>
        /// <returns></returns>
        public bool IsEffecting() {
            return BattlerSprites().Any((sprite) => { return sprite.IsEffecting(); });
        }

        /// <summary>
        /// アニメーション再生中か
        /// </summary>
        /// <returns></returns>
        public bool IsAnimationPlaying() {
            return BattlerSprites().Any((sprite) => { return sprite.IsAnimationPlaying(); });
        }

        /// <summary>
        /// バトル画面に存在する全ての敵、アクターの中で、移動中のものが存在するかどうか返却
        /// </summary>
        /// <returns></returns>
        public bool IsAnyoneMoving() {
            return BattlerSprites().Any((sprite) => { return sprite.IsMoving(); });
        }

        /// <summary>
        /// 処理中か
        /// </summary>
        /// <returns></returns>
        public bool IsBusy() {
            return IsAnimationPlaying() || IsAnyoneMoving();
        }

        /// <summary>
        /// フロントビューの配置用
        /// </summary>
        private class FrontEnemyDisplayData
        {
            public int position;
            public int positionY;
            public int width;
            public SpriteEnemy enemySprite;
            public int positionZ;

            public FrontEnemyDisplayData(int position, int positionY, int width, SpriteEnemy enemySprite, int positionZ) {
                this.position = position;
                this.positionY = positionY;
                this.width = width;
                this.enemySprite = enemySprite;
                this.positionZ = positionZ;
            }
        }
    }
}