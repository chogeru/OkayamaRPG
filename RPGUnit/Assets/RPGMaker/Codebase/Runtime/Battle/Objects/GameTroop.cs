using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventBattle;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Troop;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.Runtime.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RPGMaker.Codebase.Runtime.Battle.Objects
{
    /// <summary>
    /// 戦闘シーンでの[敵グループ]を定義したクラス
    /// </summary>
    public class GameTroop : GameUnit
    {
        /// <summary>
        /// [static] 名前の接尾辞(A〜Zの半角記号)
        /// </summary>
        public static readonly ReadOnlyCollection<string>
            LetterTableHalf = new List<string>
            {
                " A", " B", " C", " D", " E", " F", " G", " H", " I", " J", " K", " L", " M",
                " N", " O", " P", " Q", " R", " S", " T", " U", " V", " W", " X", " Y", " Z"
            }.AsReadOnly();

        /// <summary>
        /// [static] 名前の接尾辞(A〜Zの全角記号)
        /// </summary>
        public static readonly ReadOnlyCollection<string>
            LetterTableFull = new List<string>
            {
                "Ａ", "Ｂ", "Ｃ", "Ｄ", "Ｅ", "Ｆ", "Ｇ", "Ｈ", "Ｉ", "Ｊ", "Ｋ", "Ｌ", "Ｍ",
                "Ｎ", "Ｏ", "Ｐ", "Ｑ", "Ｒ", "Ｓ", "Ｔ", "Ｕ", "Ｖ", "Ｗ", "Ｘ", "Ｙ", "Ｚ"
            }.AsReadOnly();

        /// <summary>
        /// 敵グループデータ
        /// </summary>
        public TroopDataModel Troop { get; set; }
        /// <summary>
        /// 敵グループID
        /// </summary>
        private string _troopId;
        /// <summary>
        /// バトルイベントのページ番号を実行したかどうか
        /// </summary>
        private List<bool> _eventFlags = new List<bool>();
        /// <summary>
        /// 敵データ
        /// </summary>
        public List<GameEnemy> Enemies { get; } = new List<GameEnemy>();
        /// <summary>
        /// 敵データ（GameBattler）
        /// </summary>
        /// <returns></returns>
        public override List<GameBattler> Members() {
            return Enemies.Aggregate(new List<GameBattler>(), (l, e) =>
            {
                l.Add(e);
                return l;
            });
        }

        /// <summary>
        /// ターン数
        /// </summary>
        public int TurnCount { get; set; }
        /// <summary>
        /// 重複の敵が存在している場合の、敵の種類ごとの出現数
        /// </summary>
        private readonly Dictionary<string, int> _namesCount = new Dictionary<string, int>();
        /// <summary>
        /// 実行中のバトルに関するバトルイベントデータ
        /// </summary>
        private EventBattleDataModel _eventBattleData;
        /// <summary>
        /// 実行中のバトルに関するイベントデータ
        /// </summary>
        private List<EventDataModel> _eventData = new List<EventDataModel>();
        /// <summary>
        /// バトル用のCommandChainLauncherの配列
        /// </summary>
        private List<BattleEventCommandChainLauncher> _eventCommandChainLauncher = new List<BattleEventCommandChainLauncher>();

        /// <summary>
        /// 途中参加する敵データ
        /// </summary>
        private List<EnemyAdd> _enemiesAppear;
        /// <summary>
        /// コモンイベントを実行中かどうか
        /// </summary>
        public bool IsRunningCommon { get; set; }
        public List<BattleEventCommandChainLauncher> EventManager => _eventCommandChainLauncher;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="troopId"></param>
        public GameTroop(string troopId) {
            //初期化処理
            Setup(troopId);
        }

        /// <summary>
        /// 初期設定
        /// </summary>
        /// <param name="troopId"></param>
        public void Setup(string troopId) {
            //敵グループ設定
            _troopId = troopId;
            Troop = DataManager.Self().GetTroopDataModel(_troopId);

            //敵データの設定
            InitializeEnemies();

            //イベントデータ初期化
            InitializeBattleEvent();

            //敵のユニーク名設定
            MakeUniqueNames();
        }

        /// <summary>
        /// 敵データの設定
        /// </summary>
        private void InitializeEnemies() {
            //途中参加する敵キャラクター初期化
            _enemiesAppear = new List<EnemyAdd>();
            int enemyCount = 0;
            var viewtype = DataManager.Self().GetSystemDataModel().battleScene.viewType;

            if (viewtype == 0)
            {
                //フロントビューの場合
                foreach (var member in Troop.frontViewMembers)
                {
                    if (DataManager.Self().GetEnemyDataModel(member.enemyId) == null) continue;

                    //敵データの読込
                    var enemy = new GameEnemy(member.enemyId, this, member.position, 0);
                    Enemies.Add(enemy);

                    //途中参加の場合には、ターン開始時は非表示とする
                    if (member.conditions > 0)
                    {
                        var add = new EnemyAdd(member.enemyId, enemyCount, member.appearanceTurn);
                        _enemiesAppear.Add(add);
                        enemy.Hide();
                    }
                    enemyCount++;
                }
            }
            else
            {
                //サイドビューの場合
                foreach (var member in Troop.sideViewMembers)
                {
                    if (DataManager.Self().GetEnemyDataModel(member.enemyId) == null) continue;

                    //敵データの読込
                    var enemy = new GameEnemy(member.enemyId, this, member.position1, member.position2);
                    Enemies.Add(enemy);

                    //途中参加の場合には、ターン開始時は非表示とする
                    if (member.conditions > 0)
                    {
                        var add = new EnemyAdd(member.enemyId, enemyCount, member.appearanceTurn);
                        _enemiesAppear.Add(add);
                        enemy.Hide();
                    }

                    enemyCount++;
                }
            }
        }

        /// <summary>
        /// バトルイベントの設定
        /// </summary>
        private void InitializeBattleEvent() {
            var eventManagementService = new EventManagementService();
            var eventBattle = eventManagementService.LoadEventBattle();
            var events = eventManagementService.LoadEvent();

            //初期化処理
            _eventData = new List<EventDataModel>();
            _eventFlags = new List<bool>();

            //該当の敵グループに紐づいているイベントデータを取得
            _eventBattleData = null;
            for (int i = 0; i < eventBattle.Count; i++)
                if (eventBattle[i].eventId == Troop.battleEventId)
                {
                    _eventBattleData = eventBattle[i].DataClone<EventBattleDataModel>();
                    break;
                }

            if (_eventBattleData != null)
            {
                //敵グループのイベント内の、pages に設定されているイベントIDから、EventDataModelを取得
                foreach (var pageData in _eventBattleData.pages)
                {
                    EventDataModel eventData = eventManagementService.LoadEventById(pageData.eventId);
                    _eventData.Add(eventData);

                    //CommandChainLauncher作成
                    BattleEventCommandChainLauncher launcher = new BattleEventCommandChainLauncher();
                    launcher.SetTroop(this);
                    launcher.SetEventData(eventData, _eventBattleData);
                    _eventCommandChainLauncher.Add(launcher);

                    //実行済みフラグを初期化
                    _eventFlags.Add(false);
                }
            }
        }

        /// <summary>
        /// 全[敵キャラ]に(A〜Zを割り振って)固有名をつける
        /// </summary>
        public void MakeUniqueNames() {
            var table = LetterTable();
            Members().ForEach(enemy =>
            {
                if (enemy.IsAlive() && ((GameEnemy) enemy).IsLetterEmpty())
                {
                    var name = ((GameEnemy) enemy).OriginalName();
                    var n = _namesCount.ContainsKey(name) ? _namesCount[name] : 0;
                    ((GameEnemy) enemy).SetLetter(table[n % table.Count]);
                    _namesCount[name] = n + 1;
                }
            });
            Members().ForEach(enemy =>
            {
                var name = ((GameEnemy) enemy).OriginalName();
                if ((_namesCount.ContainsKey(name) ? _namesCount[name] : 0) >= 2) ((GameEnemy) enemy).SetPlural(true);
            });
        }

        /// <summary>
        /// 名前の接尾辞(A〜Z)を配列で返す
        /// </summary>
        /// <returns></returns>
        public ReadOnlyCollection<string> LetterTable() {
            return LetterTableHalf;
            //return DataManager.Self().GetGameSystem().IsCJK() ? LetterTableFull : LetterTableHalf;
        }

        /// <summary>
        /// [敵キャラ]の名前を配列で返す
        /// </summary>
        /// <returns></returns>
        public List<string> EnemyNames() {
            var names = new List<string>();
            Members().ForEach(enemy =>
            {
                var name = ((GameEnemy) enemy).OriginalName();
                if (enemy.IsAlive() && !names.Contains(name)) names.Add(name);
            });
            return names;
        }

        /// <summary>
        /// 指定ページが条件に合っているか
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public bool MeetsConditions(EventBattleDataModel.EventBattlePage page) {
            var c = page.condition;

            // 実行しない
            if (c.run == 1)
                return false;

            // 実行契機の設定なし
            if (c.turnEnd == 0 && c.turn.enabled == 0 && c.enemyHp.enabled == 0 &&
                c.actorHp.enabled == 0 && c.switchData.enabled == 0)
                return false;

            // ターン終了時
            if (c.turnEnd == 1)
                if (!BattleManager.IsTurnEnd())
                    return false;

            // ターン設定
            if (c.turn.enabled == 1)
            {
                var n = TurnCount;
                var start = c.turn.start;
                var end = c.turn.end;
                if (end == 0 && n != start) return false;
                if (end > 0 && (n < 1 || n < start || n % end != start % end)) return false;
            }

            // 敵体力
            if (c.enemyHp.enabled == 1)
            {
                //c.enemyHp.enemyId には敵グループに設定されている敵のindex番号が入る（0,1,2…）
                //もしも値を取得できなかった場合には、この条件は無かったこととする
                try
                {
                    var enemy = Enemies[int.Parse(c.enemyHp.enemyId)];
                    if (enemy == null || enemy.HpRate() * 100 > c.enemyHp.value) return false;
                }
                catch (Exception) {
                    return false;
                }
            }

            // アクター体力
            if (c.actorHp.enabled == 1)
            {
                var actor = DataManager.Self().GetGameParty().Actors
                    .FirstOrDefault(a => a.ActorId == c.actorHp.actorId);

                if (actor == null || actor.HpRate() * 100 > c.actorHp.value) return false;
            }

            // スイッチ設定
            if (c.switchData.enabled == 1)
            {
                // フラグを検索して一致した番号を渡す
                var flagsData = new DatabaseManagementService().LoadFlags();
                var flagNum = 0;
                for (var i = 0; i < DataManager.Self().GetRuntimeSaveDataModel().switches.data.Count; i++)
                    if (c.switchData.switchId == flagsData.switches[i].id)
                    {
                        flagNum = i;
                        break;
                    }

                if (!DataManager.Self().GetRuntimeSaveDataModel().switches.data
                    .ElementAtOrDefault(flagNum))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// バトルイベントの準備
        /// </summary>
        /// <returns></returns>
        public void SetupBattleEvent() {
            //既に実行中のイベントがある場合は処理終了
            List<BattleEventCommandChainLauncher> launchers = _eventCommandChainLauncher.FindAll(launcher => launcher.IsRunning());
            if (launchers != null && launchers.Count > 0) return;

            //CommonEvent実行中の場合
            if (IsRunningCommon)
            {
                //イベント開始時に、LogWindowを非表示にする
                BattleManager.GetLogWindow().Hide();
                BattleManager.GetLogWindow().gameObject.SetActive(false);
                _eventCommandChainLauncher[^1].Starting((dummy, dummy2) =>
                {
                    IsRunningCommon = false;
                    _eventCommandChainLauncher.RemoveAt(_eventCommandChainLauncher.Count - 1);

                    //イベント終了後に、LogWindowをShowする
                    BattleManager.GetLogWindow().gameObject.SetActive(true);
                    BattleManager.GetLogWindow().Show();
                });
                return;
            }

            //通常のイベント
            for (var i = 0; i < _eventCommandChainLauncher.Count; i++)
            {
                //条件に合致しており、イベント実行済みではないものを実行する
                if (MeetsConditions(_eventBattleData.pages[i]) && !_eventFlags[i])
                {
                    //イベント開始時に、LogWindowを非表示にする
                    BattleManager.GetLogWindow().Hide();
                    BattleManager.GetLogWindow().gameObject.SetActive(false);
                    _eventCommandChainLauncher[i].Starting((dummy, dummy2) => {
                        //イベント終了時に、バトル画面ではなくなっている場合
                        if (!GameStateHandler.IsBattle()) return;

                        //イベント終了後に、LogWindowをShowする
                        BattleManager.GetLogWindow().gameObject.SetActive(true);
                        BattleManager.GetLogWindow().Show();
                    });
                    //実行するスパンが Battle/Turnのものについては、イベント実行済みフラグを立てる
                    if (_eventBattleData.pages[i].condition.span <= 1)
                    {
                        _eventFlags[i] = true;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// ターンを進める
        /// </summary>
        public void IncreaseTurn() {
            //実行するスパンがTurnのものについては、イベント実行済みフラグを落とす
            for (var i = 0; i < _eventCommandChainLauncher.Count; i++)
            {
                if (_eventBattleData.pages[i].condition.span == 1)
                {
                    _eventFlags[i] = false;
                }
            }
            TurnCount++;
        }

        /// <summary>
        /// 敵が途中から出現又は、ターン指定で出現する場合に、
        /// コマンド入力前に敵を出現させる
        /// </summary>
        public void AppearEnemy() {
            //敵が途中から出現又は、ターン指定で出現する場合
            if (_enemiesAppear != null && _enemiesAppear.Count > 0)
            {
                for (var i = 0; i < _enemiesAppear.Count; i++)
                {
                    //該当のターンである
                    //TurnCountはターン開始時に加算されるため、+1することで、開始しようとしているターン数になる
                    if (_enemiesAppear[i].Turn <= TurnCount + 1)
                    {
                        //敵データを取得
                        var enemy = Members().ElementAtOrDefault(_enemiesAppear[i].Number);
                        //敵が存在しており、かつ出現していない場合
                        if (enemy != null && !enemy.IsAppeared())
                        {
                            //敵を出現させる
                            enemy.Appear();
                            //敵の名称を再割り当て
                            DataManager.Self().GetGameTroop().MakeUniqueNames();
                            //敵選択Windowを再生成
                            BattleManager.SceneBattle.CreateEnemyWindow();
                        }

                        //出現させた敵情報を削除
                        _enemiesAppear.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        /// <summary>
        /// [敵キャラ]の合計EXPを返す
        /// </summary>
        /// <returns></returns>
        public int ExpTotal() {
            return DeadMembers().Aggregate(0, (r, enemy) => { return r + ((GameEnemy) enemy).Exp(); });
        }

        /// <summary>
        /// [敵キャラ]からの合計取得金額を返す
        /// </summary>
        /// <returns></returns>
        public int GoldTotal() {
            return DeadMembers().Aggregate(0, (r, enemy) => { return r + ((GameEnemy) enemy).Gold(); }) * GoldRate();
        }

        /// <summary>
        /// プレイヤーパーティが持つ金額の取得倍率を返す。スキルやアイテムの効果で上下する
        /// </summary>
        /// <returns></returns>
        public int GoldRate() {
            return DataManager.Self().GetGameParty().HasGoldDouble() ? 2 : 1;
        }

        /// <summary>
        /// ドロップアイテムを作成して配列で返す
        /// </summary>
        /// <returns></returns>
        public List<GameItem> MakeDropItems() {
            var items = new List<GameItem>();
            return DeadMembers().Aggregate(new List<GameItem>(), (r, enemy) =>
            {
                var gameItems = (enemy as GameEnemy)?.MakeDropItems();
                if (gameItems != null)
                    foreach (var item in gameItems)
                        items.Add(item);

                return items;
            });
        }

        /// <summary>
        /// イベント実行中かどうかを返却する
        /// </summary>
        /// <returns></returns>
        public bool IsEventRunning() {
            //既に実行中のイベントがあるかどうか
            List<BattleEventCommandChainLauncher> launchers = _eventCommandChainLauncher.FindAll(launcher => launcher.IsRunning());
            if (launchers != null && launchers.Count > 0) return true;
            return false;
        }

        public void StartCommonEvent(EventDataModel eventDataModelData, EventBattleDataModel eventBattleData) {
            BattleEventCommandChainLauncher launcher = new BattleEventCommandChainLauncher();
            launcher.Init();
            launcher.SetEventData(eventDataModelData, eventBattleData);
            _eventCommandChainLauncher.Add(launcher);
        }
    }

    /// <summary>
    /// 途中参加の敵データ Unite独自実装
    /// </summary>
    public class EnemyAdd
    {
        public string Id;
        public int Number;
        public int Turn;

        public EnemyAdd(string id,int number, int turn) {
            this.Id = id;
            this.Number = number;
            this.Turn = turn;
        }
    }
}