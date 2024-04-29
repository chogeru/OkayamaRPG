using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Actor;

namespace RPGMaker.Codebase.Runtime.Event.Actor
{
    /// <summary>
    /// [アクター]-[HPの増減]
    /// </summary>
    public class ActorChangeHpProcessor : AbstractEventCommandProcessor
    {
        private ActorChangeHp _actor;

        protected override void Process(string eventId, EventDataModel.EventCommand command) {
            if (_actor == null)
            {
                _actor = new ActorChangeHp();
                _actor.Init(DataManager.Self().GetRuntimeSaveDataModel());
            }

            _actor.ChangeHP(command);
            ProcessWait();
        }

        private void ProcessWait() {
            DataManager.Self().IsGameOverCheck = true;
            ProcessEndAction();
        }

        private void ProcessEndAction() {
            _actor = null;
            SendBackToLauncher.Invoke();
        }
    }
}