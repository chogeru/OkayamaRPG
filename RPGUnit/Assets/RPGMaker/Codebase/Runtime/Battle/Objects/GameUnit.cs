using RPGMaker.Codebase.Runtime.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RPGMaker.Codebase.Runtime.Battle.Objects
{
    /// <summary>
    /// 戦闘時のグループを扱うクラス
    /// </summary>
    public abstract class GameUnit
    {
        /// <summary>
        /// 戦闘中か
        /// </summary>
        private bool _inBattle = false;

        /// <summary>
        /// 戦闘中か
        /// </summary>
        /// <returns></returns>
        public bool InBattle() {
            return _inBattle;
        }

        /// <summary>
        /// 戦闘中のバトラー生死問わず全て配列で返す
        /// </summary>
        /// <returns></returns>
        public virtual List<GameBattler> Members() {
            return new List<GameBattler>();
        }

        /// <summary>
        /// 生存しているバトラーを配列で返す
        /// </summary>
        /// <returns></returns>
        public virtual List<GameBattler> AliveMembers() {
            return Members().FindAll(member => member.IsAlive());
        }

        /// <summary>
        /// 死亡しているバトラーを配列で返す
        /// </summary>
        /// <returns></returns>
        public virtual List<GameBattler> DeadMembers() {
            return Members().FindAll(member => { return member.IsDead(); });
        }

        /// <summary>
        /// 動ける(死亡や麻痺などでない)バトラーを配列で返す
        /// </summary>
        /// <returns></returns>
        public List<GameBattler> MovableMembers() {
            return Members().FindAll(member => member.CanMove());
        }

        /// <summary>
        /// アクションを取り消す
        /// </summary>
        public void ClearActions() {
            Members().ForEach(member => member.ClearActions());
        }

        /// <summary>
        /// ユニットの素早さを返す
        /// </summary>
        /// <returns></returns>
        public double Agility() {
            var members = Members();
            if (members.Count == 0) return 1;

            var sum = members.Aggregate(0, (r, member) => { return r + member.Agi; });
            return sum / members.Count;
        }

        /// <summary>
        /// 生きているメンバーの[狙われ率]の合計を返す
        /// </summary>
        /// <returns></returns>
        public double TgrSum() {
            return AliveMembers().Aggregate((double) 0, (r, member) => { return r + member.Tgr; });
        }

        /// <summary>
        /// 含まれるバトラーからランダムに1体を返す
        /// </summary>
        /// <returns></returns>
        public GameBattler RandomTarget() {
            var tgrRand = TforuUtility.MathRandom() * TgrSum();
            GameBattler target = null;
            AliveMembers().ForEach(member =>
            {
                tgrRand -= member.Tgr;
                if (tgrRand <= 0 && target == null) target = member;
            });
            return target;
        }

        /// <summary>
        /// 死亡したバトラーからランダムに1体を返す
        /// </summary>
        /// <returns></returns>
        public GameBattler RandomDeadTarget() {
            var members = DeadMembers();
            if (members.Count == 0) return null;

            return members[(int) Math.Floor(TforuUtility.MathRandom() * members.Count)];
        }

        /// <summary>
        /// 生存,死亡を問わず、バトラーからランダムに1体を返す
        /// </summary>
        /// <returns></returns>
        public GameBattler RandomAllTarget() {
            var tgrRand = TforuUtility.MathRandom() * TgrSum();
            GameBattler target = null;
            Members().ForEach(member =>
            {
                tgrRand -= member.Tgr;
                if (tgrRand <= 0 && target == null) target = member;
            });
            return target;
        }

        /// <summary>
        /// 指定番号のメンバーを優先して生きているメンバーを返す
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public GameBattler SmoothTarget(int index) {
            if (index < 0) index = 0;
            if (index >= Members().Count) index = Members().Count - 1;

            var member = Members()[index];
            return member != null && member.IsAlive() ? member : AliveMembers().ElementAtOrDefault(0);
        }

        /// <summary>
        /// 指定番号のメンバーを優先して死亡しているメンバーを返す
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public GameBattler SmoothDeadTarget(int index) {
            if (index < 0) index = 0;

            var member = Members()[index];
            return member != null && member.IsDead() ? member : DeadMembers().ElementAtOrDefault(0);
        }

        /// <summary>
        /// 指定番号のメンバーを返却する
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public GameBattler SmoothAllTarget(int index) {
            if (index < 0) index = 0;
            if (index >= Members().Count) index = Members().Count - 1;

            var member = Members()[index];
            return member != null ? member : AliveMembers().ElementAtOrDefault(0);
        }

        /// <summary>
        /// アクションの結果を取り消す
        /// </summary>
        public void ClearResults() {
            Members().ForEach(member => member.ClearResult());
        }

        /// <summary>
        /// 戦闘開始時に呼ばれるハンドラ
        /// </summary>
        public void OnBattleStart() {
            Members().ForEach(member => { member.OnBattleStart(); });
            _inBattle = true;
        }

        /// <summary>
        /// 戦闘終了時に呼ばれるハンドラ
        /// </summary>
        public void OnBattleEnd() {
            _inBattle = false;
            Members().ForEach(member => { member.OnBattleEnd(); });
        }

        /// <summary>
        /// 戦闘行動を作成する
        /// </summary>
        public void MakeActions() {
            Members().ForEach(member => { member.MakeActions(); });
        }

        /// <summary>
        /// 指定されたバトラーを選択する
        /// </summary>
        /// <param name="activeMember"></param>
        public void Select(GameBattler activeMember) {
            Members().ForEach(member =>
            {
                if (member == activeMember)
                    member.Select();
                else
                    member.Deselect();
            });
        }

        /// <summary>
        /// 全バトラーが死亡したか
        /// </summary>
        /// <returns></returns>
        public bool IsAllDead() {
            return AliveMembers().Count == 0;
        }

        /// <summary>
        /// 身代わりのバトラーを返す
        /// </summary>
        /// <returns></returns>
        public GameBattler SubstituteBattler() {
            var members = Members();
            for (var i = 0; i < members.Count; i++)
                if (members[i].IsSubstitute())
                    return members[i];

            return null;
        }
    }
}