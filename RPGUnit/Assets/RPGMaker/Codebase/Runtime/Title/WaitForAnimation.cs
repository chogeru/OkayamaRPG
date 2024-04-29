using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Title
{
    public class WaitForAnimation : CustomYieldInstruction
    {
        private Animator m_animator;
        private int      m_lastStateHash;
        private int      m_layerNo;

        public WaitForAnimation(Animator animator, int layerNo) {
            Init(animator, layerNo, animator.GetCurrentAnimatorStateInfo(layerNo).fullPathHash);
        }

        public override bool keepWaiting
        {
            get
            {
                var currentAnimatorState = m_animator.GetCurrentAnimatorStateInfo(m_layerNo);
                return currentAnimatorState.fullPathHash == m_lastStateHash &&
                       currentAnimatorState.normalizedTime < 1;
            }
        }

        private void Init(Animator animator, int layerNo, int hash) {
            m_layerNo = layerNo;
            m_animator = animator;
            m_lastStateHash = hash;
        }
    }
}