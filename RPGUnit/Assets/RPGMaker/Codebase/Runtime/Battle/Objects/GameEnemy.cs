using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Enemy;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.Runtime.Battle.Wrapper;
using RPGMaker.Codebase.Runtime.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Battle.Objects
{
    /// <summary>
    /// 戦闘中の[敵キャラ]のパラメータの取得、画像の設定を行うクラス
    /// </summary>
    public class GameEnemy : GameBattler
    {
        /// <summary>
        /// 敵キャラID
        /// </summary>
        public string EnemyId { get; set; } = "";
        /// <summary>
        /// [敵キャラ]のデータベース情報を返す
        /// </summary>
        public EnemyDataModel Enemy { get; private set; }
        /// <summary>
        /// 敵グループ
        /// </summary>
        public GameTroop BelongingTroop { get; }
        /// <summary>
        /// 接尾辞(A,B…など)
        /// </summary>
        private string _letter;
        /// <summary>
        /// 群(2体以上)か
        /// </summary>
        private bool   _plural;
        /// <summary>
        /// 画面上の x座標
        /// </summary>
        public double ScreenX { get; set; }
        /// <summary>
        /// 画面上の y座標
        /// </summary>
        public double ScreenY { get; set; }

        private readonly SystemSettingDataModel _systemSetting;

        /// <summary>
        /// コンストラクタ
        /// MVとは異なり、Uniteは自動整列ありきで動作するため、引数のx, yは配置場所のindexとしている    
        /// </summary>
        /// <param name="enemyId"></param>
        /// <param name="belongingTroop"></param>
        public GameEnemy(string enemyId, GameTroop belongingTroop, int x, int y) {
            //readonly のプロパティ値設定
            BelongingTroop = belongingTroop;
            _systemSetting = DataManager.Self().GetSystemDataModel();

            InitMembers();
            Setup(enemyId, x, y);
        }

        /// <summary>
        /// メンバ変数を初期化
        /// </summary>
        public override void InitMembers() {
            base.InitMembers();
        }

        /// <summary>
        /// 指定の敵と座標で Gama_Enemy を設定
        /// MVとは異なり、Uniteは自動整列ありきで動作するため、引数のx, yは配置場所のindexとしている    
        /// </summary>
        /// <param name="enemyId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Setup(string enemyId, int x, int y) {
            Enemy = DataManager.Self().GetEnemyDataModel(EnemyId);
            EnemyId = enemyId;
            ScreenX = x;
            ScreenY = y;
            Enemy = DataManager.Self().GetEnemyDataModel(EnemyId);
            RecoverAll();
        }

        /// <summary>
        /// 敵か
        /// </summary>
        /// <returns></returns>
        public override bool IsEnemy() {
            return true;
        }

        /// <summary>
        /// [敵グループ]を返す
        /// </summary>
        /// <returns></returns>
        public override GameUnit FriendsUnit() {
            return BelongingTroop;
        }

        /// <summary>
        /// 味方パーティを返す
        /// </summary>
        /// <returns></returns>
        public override GameUnit OpponentsUnit() {
            return DataManager.Self().GetGameParty();
        }

        /// <summary>
        /// [敵キャラ]の番号を返す
        /// </summary>
        /// <returns></returns>
        public override int Index() {
            return BelongingTroop.Members().IndexOf(this);
        }

        /// <summary>
        /// 戦闘に参加しているか
        /// </summary>
        /// <returns></returns>
        public override bool IsBattleMember() {
            return Index() >= 0;
        }

        /// <summary>
        /// 特徴オブジェクトを配列で返す
        /// </summary>
        /// <returns></returns>
        public override List<TraitCommonDataModel> TraitObjects() {
            List<TraitCommonDataModel> traitList = base.TraitObjects();

            //敵に設定されている特徴を付加
            if (Enemy.traits != null && Enemy.traits.Count > 0)
                foreach (var trait in Enemy.traits)
                    if (!traitList.Contains(trait))
                        traitList.Add(trait);

            return traitList;
        }

        /// <summary>
        /// 指定通常能力値の基本値を返す
        /// </summary>
        /// <param name="paramId"></param>
        /// <returns></returns>
        public override int ParamBase(int paramId) {
            return Enemy.param[paramId];
        }

        /// <summary>
        /// [敵キャラ]の[経験値]を返す
        /// </summary>
        /// <returns></returns>
        public int Exp() {
            return Enemy.exp;
        }

        /// <summary>
        /// [所持金]を返す
        /// </summary>
        /// <returns></returns>
        public int Gold() {
            return Enemy.gold;
        }

        /// <summary>
        /// ドロップアイテムを生成して返す
        /// </summary>
        /// <returns></returns>
        public List<GameItem> MakeDropItems() {
            return Enemy.dropItems.Aggregate(new List<GameItem>(), (r, di) =>
            {
                if (di.kind > 0 && TforuUtility.MathRandom() < (di.denominator * DropItemRate()) / 100f)
                    r.Add(ItemObject(di.kind, di.dataId));

                return r;
            });
        }

        /// <summary>
        /// ドロップアイテム出現率を返す
        /// </summary>
        /// <returns></returns>
        public int DropItemRate() {
            return DataManager.Self().GetGameParty().HasDropItemDouble() ? 2 : 1;
        }

        /// <summary>
        /// 指定したアイテムを返す
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="dataId"></param>
        /// <returns></returns>
        public GameItem ItemObject(int kind, string dataId) {
            switch (kind)
            {
                case 1:
                    return new GameItem(dataId, GameItem.DataClassEnum.Item);
                case 2:
                    return new GameItem(dataId, GameItem.DataClassEnum.Weapon);
                case 3:
                    return new GameItem(dataId, GameItem.DataClassEnum.Armor);
                default:
                    return null;
            }
        }

        /// <summary>
        /// 画像が表示されているか
        /// </summary>
        /// <returns></returns>
        public override bool IsSpriteVisible() {
            return true;
        }

        /// <summary>
        /// 色相
        /// </summary>
        /// <returns></returns>
        public int BattlerHue() {
            return Enemy.battlerHue;
        }

        /// <summary>
        /// [名前]を返す
        /// </summary>
        /// <returns></returns>
        public string OriginalName() {
            return Enemy.name;
        }

        /// <summary>
        /// 接尾辞つきの名前を返す
        /// </summary>
        /// <returns></returns>
        public string Name() {
            return OriginalName() + (_plural ? _letter : "");
        }

        /// <summary>
        /// 接尾辞(A,B…など)が付いていない名前か
        /// </summary>
        /// <returns></returns>
        public bool IsLetterEmpty() {
            return (_letter == null || _letter == "");
        }

        /// <summary>
        /// 接尾辞(A,B…など)を設定
        /// </summary>
        /// <param name="letter"></param>
        public void SetLetter(string letter) {
            _letter = letter;
        }

        /// <summary>
        /// 群(2体以上)であるか設定
        /// </summary>
        /// <param name="plural"></param>
        public void SetPlural(bool plural) {
            _plural = plural;
        }

        /// <summary>
        /// 指定アクションの開始動作を実行
        /// </summary>
        /// <param name="action"></param>
        public override void PerformActionStart(GameAction action) {
            base.PerformActionStart(action);
            RequestEffect("whiten");
        }

        /// <summary>
        /// 指定アクションを実行
        /// </summary>
        /// <param name="action"></param>
        public override void PerformAction(GameAction action) {
            base.PerformAction(action);
        }

        /// <summary>
        /// 行動終了を実行
        /// </summary>
        public override void PerformActionEnd() {
            base.PerformActionEnd();
        }

        /// <summary>
        /// 被ダメージ動作を実行
        /// </summary>
        public override void PerformDamage() {
            base.PerformDamage();
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, _systemSetting.soundSetting.enemyDamage);
            SoundManager.Self().PlaySe();
            RequestEffect("blink");
        }

        /// <summary>
        /// 倒れる動作を実行
        /// </summary>
        public override void PerformCollapse() {
            base.PerformCollapse();
            switch (CollapseType())
            {
                case 0:
                    RequestEffect("collapse");
                    SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, DataManager.Self().GetSystemDataModel().soundSetting.enemyCollapse);
                    SoundManager.Self().PlaySe();
                    break;
                case 1:
                    RequestEffect("bossCollapse");
                    SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, DataManager.Self().GetSystemDataModel().soundSetting.bossCollapse1);
                    SoundManager.Self().PlaySe();
                    break;
                case 2:
                    RequestEffect("instantCollapse");
                    break;
            }
        }

        /// <summary>
        /// 指定の[敵キャラ]へ変更(変身)
        /// </summary>
        /// <param name="enemyId">変身後の敵キャラのID</param>
        public void Transform(string enemyId) {
            // 変身後敵キャラが存在するかチェック
            var transformEnemy = DataManager.Self().GetEnemyDataModel(enemyId);
            if (transformEnemy == null) return;

            //表示されていなかったら、変身させない
            if (!IsAppeared() || !IsAlive()) return;


            var spriteset = BattleManager.GetSpriteSet();

            // 各パラメータの更新
            var name = OriginalName();
            EnemyId = enemyId;
            Enemy = transformEnemy;
            spriteset.TransformEnemy(this);
            if (OriginalName() != name)
            {
                _letter = "";
                _plural = false;
            }

            Refresh();
            if (NumActions() > 0) MakeActions();
        }

        /// <summary>
        /// [行動パターン]が[条件]に合致しているか
        /// </summary>
        /// <param name="enemyAction"></param>
        /// <returns></returns>
        public bool MeetsCondition(EnemyDataModel.EnemyAction enemyAction) {
            var param1 = enemyAction.conditionParam1;
            var param2 = enemyAction.conditionParam2;
            switch (enemyAction.conditionType)
            {
                case 1:
                    return MeetsTurnCondition(param1, param2);
                case 2:
                    return MeetsHpCondition(param1, param2);
                case 3:
                    return MeetsMpCondition(param1, param2);
                case 4:
                    return MeetsStateCondition(param1);
                case 5:
                    return MeetsPartyLevelCondition(param1);
                case 6:
                    return MeetsSwitchCondition(param1);
                default:
                    return true;
            }
        }

        /// <summary>
        /// [条件 - ターン]に合致しているか
        /// </summary>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <returns></returns>
        public bool MeetsTurnCondition(int param1, int param2) {
            var n = BelongingTroop.TurnCount;
            if (param2 == 0)
            {
                return n == param1;
            }
            return (n > 0 && n >= param1 && n % param2 == param1 % param2);
            //if (param2 == 0 || n - param1 < 0) return false;
            //return (n - param1) % param2 == 0;
        }

        /// <summary>
        /// [条件 - HP]に合致しているか
        /// </summary>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <returns></returns>
        public bool MeetsHpCondition(int param1, int param2) {
            return HpRate() * 100f >= param1 && HpRate() * 100f <= param2;
        }

        /// <summary>
        /// [条件 - MP]に合致しているか
        /// </summary>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        /// <returns></returns>
        public bool MeetsMpCondition(int param1, int param2) {
            return MpRate() * 100f >= param1 && MpRate() * 100f <= param2;
        }

        /// <summary>
        /// [条件 - ステート]に合致しているか
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public bool MeetsStateCondition(int param) {
            return IsStateAffected(ConvertUniteData.StateSerialNoToUuid(param));
        }

        /// <summary>
        /// [条件 - パーティLv]に合致しているか
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public bool MeetsPartyLevelCondition(int param) {
            return DataManager.Self().GetGameParty().HighestLevel() >= param;
        }

        /// <summary>
        /// [条件 - スイッチ]に合致しているか
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public bool MeetsSwitchCondition(int param) {
            return DataManager.Self().GetRuntimeSaveDataModel().switches.data[param];
        }

        /// <summary>
        /// [行動パターン]が実行可能か
        /// </summary>
        /// <param name="enemyAction"></param>
        /// <returns></returns>
        public bool IsActionValid(EnemyDataModel.EnemyAction enemyAction) {
            return MeetsCondition(enemyAction) &&
                   CanUse(new GameItem(enemyAction.skillId, GameItem.DataClassEnum.Skill));
        }

        /// <summary>
        /// 指定した[行動パターン]リストの中から選択した[行動パターン]を返す
        /// </summary>
        /// <param name="actionList"></param>
        /// <param name="ratingZero"></param>
        /// <returns></returns>
        public EnemyDataModel.EnemyAction SelectAction(List<EnemyDataModel.EnemyAction> actionList) {
            // 発動可能なアクションのリストを生成
            var validActions = actionList.FindAll(action => IsActionValid(action));
            if (validActions == null || validActions.Count == 0)
            {
                return null;
            }

            // 最大レーティング
            var maxRating = validActions.Max(action => action.rating);

            // レーティング差3以上を除外
            validActions.RemoveAll(action => action.rating - maxRating < -2);

            // 最大値
            var max = validActions.FindAll(action => action.rating == maxRating).Count * 3
                + validActions.FindAll(action => action.rating == maxRating - 1).Count * 2
                + validActions.FindAll(action => action.rating == maxRating - 2).Count;

            // ランダム生成した値よりレーティング値が大きければ返却
            var random = Random.Range(1, max + 1);
            for (int i = 0; i < validActions.Count; i++)
            {
                var rating = validActions[i].rating == maxRating ? 3 :
                    validActions[i].rating == maxRating - 1 ? 2 : 1;

                if (rating >= random)
                    return validActions[i];

                random -= rating;
            }

            return null;
        }

        /// <summary>
        /// 指定した[行動パターン]リストを元に全行動を選択
        /// </summary>
        /// <param name="actionList"></param>
        public void SelectAllActions(List<EnemyDataModel.EnemyAction> actionList) {
            for (var i = 0; i < NumActions(); i++) Actions[i].SetEnemyAction(SelectAction(actionList));
        }

        /// <summary>
        /// アニメーションを生成
        /// </summary>
        public override void MakeActions() {
            base.MakeActions();

            if (NumActions() > 0)
            {
                var actionList = Enemy.actions;
                if (actionList.Count > 0) SelectAllActions(actionList);
            }

            SetActionState(ActionStateEnum.Waiting);
        }

        //================================================================================
        // 以下はUniteで追加されたメソッド
        //================================================================================

        /// <summary>
        /// 全ての状態異常を返却
        /// </summary>
        /// <returns></returns>
        public override List<TraitCommonDataModel> AllTraits() {
            var ret = base.AllTraits();
            return AddTraitsElement(ret);
        }

        /// <summary>
        /// 属性設定から特徴を付与する
        /// </summary>
        public List<TraitCommonDataModel> AddTraitsElement(List<TraitCommonDataModel> ret) {
            //属性追加
            //属性はSerialNoで持つ
            var enemyDataModel = DataManager.Self().GetEnemyDataModel(EnemyId);
            MyElement = new List<int>();

            //無し以外の属性を詰める
            if (enemyDataModel.elements[0] != 0)
                MyElement.Add(enemyDataModel.elements[0]);

            return ret;
        }
    }
}