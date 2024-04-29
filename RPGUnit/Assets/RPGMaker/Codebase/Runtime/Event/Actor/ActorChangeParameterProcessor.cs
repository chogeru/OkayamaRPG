using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Actor;

namespace RPGMaker.Codebase.Runtime.Event.Actor
{
    /// <summary>
    /// [アクター]-[能力値の増減]
    /// </summary>
    public class ActorChangeParameterProcessor : AbstractEventCommandProcessor
    {
        private ActorChangeParameter _actor;

        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            if (_actor == null)
            {
                _actor = new ActorChangeParameter();
                _actor.Init(DataManager.Self().GetRuntimeSaveDataModel());
            }

            _actor.ChangeParameter(command);
            ProcessEndAction();
        }

        private void ProcessEndAction() {
            _actor = null;
            SendBackToLauncher.Invoke();
        }
    }
}