using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.State;
using RPGMaker.Codebase.Runtime.Common;
using System.Collections.Generic;
using System.Linq;

namespace RPGMaker.Codebase.Runtime.Battle.Objects
{
    /// <summary>
    /// Game_Action の結果を記述したオブジェクト
    /// </summary>
    public class GameActionResult
    {
        /// <summary>
        /// 使ったか
        /// </summary>
        public bool Used { get; set; } = false;
        /// <summary>
        /// 失敗か
        /// </summary>
        public bool Missed { get; set; } = false;
        /// <summary>
        /// [回避]か
        /// </summary>
        public bool Evaded { get; set; } = false;
        /// <summary>
        /// [物理攻撃]か
        /// </summary>
        public bool Physical { get; set; } = false;
        /// <summary>
        /// [吸収]か
        /// </summary>
        public bool Drain { get; set; } = false;
        /// <summary>
        /// [会心]か
        /// </summary>
        public bool Critical { get; set; } = false;
        /// <summary>
        /// 成功か
        /// </summary>
        public bool Success { get; set; } = false;
        /// <summary>
        /// HPに変化があったか
        /// </summary>
        public bool HpAffected { get; set; } = false;
        /// <summary>
        /// HPのダメージ量
        /// </summary>
        public int HpDamage { get; set; } = 0;
        /// <summary>
        /// MPのダメージ量
        /// </summary>
        public int MpDamage { get; set; } = 0;
        /// <summary>
        /// TPのダメージ量
        /// </summary>
        public int TpDamage { get; set; } = 0;
        /// <summary>
        /// 付加された[ステート]の配列
        /// </summary>
        public List<string> AddedStates { get; set; } = new List<string>();
        /// <summary>
        /// 削除された[ステート]の配列
        /// </summary>
        public List<string> RemovedStates { get; set; } = new List<string>();
        /// <summary>
        /// 付加された[強化]の配列
        /// </summary>
        public List<int> AddedBuffs { get; set; } = new List<int>();
        /// <summary>
        /// 付加された[弱体]の配列
        /// </summary>
        public List<int> AddedDebuffs { get; set; } = new List<int>();
        /// <summary>
        /// 削除された[強化][弱体]の配列
        /// </summary>
        public List<int> RemovedBuffs { get; set; } = new List<int>();

        /// <summary>
        /// 付加された[ステート]の配列を返す
        /// </summary>
        /// <returns></returns>
        public List<StateDataModel> AddedStateObjects() {
            return AddedStates.Aggregate(
                new List<StateDataModel>(), (ret, stateId) =>
                {
                    ret.Add(DataManager.Self().GetStateDataModel(stateId));
                    return ret;
                });
        }

        /// <summary>
        /// 削除されたステートの配列を返す
        /// </summary>
        /// <returns></returns>
        public List<StateDataModel> RemovedStateObjects() {
            return RemovedStates.Aggregate(
                new List<StateDataModel>(), (ret, stateId) =>
                {
                    ret.Add(DataManager.Self().GetStateDataModel(stateId));
                    return ret;
                });
        }

        /// <summary>
        /// 指定ステートが効果を発揮したか
        /// </summary>
        /// <returns></returns>
        public bool IsStatusAffected() {
            return AddedStates.Count > 0 || RemovedStates.Count > 0 ||
                   AddedBuffs.Count > 0 || AddedDebuffs.Count > 0 ||
                   RemovedBuffs.Count > 0;
        }

        /// <summary>
        /// 攻撃がヒットしたか
        /// </summary>
        /// <returns></returns>
        public bool IsHit() {
            return Used && !Missed && !Evaded;
        }

        /// <summary>
        /// 指定ステートが付加されたか
        /// </summary>
        /// <param name="stateId"></param>
        /// <returns></returns>
        public bool IsStateAdded(string stateId) {
            return AddedStates.Contains(stateId);
        }

        /// <summary>
        /// 指定ステートの付加を追加
        /// </summary>
        /// <param name="stateId"></param>
        public void PushAddedState(string stateId) {
            if (!IsStateAdded(stateId)) AddedStates.Add(stateId);
        }

        /// <summary>
        /// 指定ステートが削除されたか
        /// </summary>
        /// <param name="stateId"></param>
        /// <returns></returns>
        public bool IsStateRemoved(string stateId) {
            return RemovedStates.Contains(stateId);
        }

        /// <summary>
        /// 指定ステートの削除を追加
        /// </summary>
        /// <param name="stateId"></param>
        public void PushRemovedState(string stateId) {
            if (!IsStateRemoved(stateId)) RemovedStates.Add(stateId);
        }

        /// <summary>
        /// 指定された能力値に[強化]が付加されたか
        /// </summary>
        /// <param name="paramId"></param>
        /// <returns></returns>
        public bool IsBuffAdded(int paramId) {
            return AddedBuffs.Contains(paramId);
        }

        /// <summary>
        /// 指定された能力値の[強化]の付加を追加
        /// </summary>
        /// <param name="paramId"></param>
        public void PushAddedBuff(int paramId) {
            if (!IsBuffAdded(paramId)) AddedBuffs.Add(paramId);
        }

        /// <summary>
        /// 指定された能力値に[弱体]が付加されたか
        /// </summary>
        /// <param name="paramId"></param>
        /// <returns></returns>
        public bool IsDebuffAdded(int paramId) {
            return AddedDebuffs.Contains(paramId);
        }

        /// <summary>
        /// 指定された能力値の[弱体]の付加を追加
        /// </summary>
        /// <param name="paramId"></param>
        public void PushAddedDebuff(int paramId) {
            if (!IsDebuffAdded(paramId)) AddedDebuffs.Add(paramId);
        }

        /// <summary>
        /// 指定された能力値の[強化]が削除されたか
        /// </summary>
        /// <param name="paramId"></param>
        /// <returns></returns>
        public bool IsBuffRemoved(int paramId) {
            return RemovedBuffs.Contains(paramId);
        }

        /// <summary>
        /// 指定された能力値の[強化]の削除を追加
        /// </summary>
        /// <param name="paramId"></param>
        public void PushRemovedBuff(int paramId) {
            if (!IsBuffRemoved(paramId)) RemovedBuffs.Add(paramId);
        }
    }
}