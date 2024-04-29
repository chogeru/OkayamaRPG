using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Actor;

namespace RPGMaker.Codebase.Runtime.Event.Actor
{
    /// <summary>
    /// [アクター]-[レベルの増減]
    /// </summary>
    public class ActorChangeLevelProcessor : AbstractEventCommandProcessor
    {
        private ActorChangeLevel _actor;

        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            if (_actor == null)
            {
                _actor = new ActorChangeLevel();
                _actor.Init(
                    DataManager.Self().GetRuntimeSaveDataModel().variables,
                    DataManager.Self().GetActorDataModels()
                );
            }

            _actor.ChangeLevel(command, ProcessEndAction);
        }

        private void ProcessEndAction() {
            _actor = null;
            SendBackToLauncher.Invoke();
        }
    }
}