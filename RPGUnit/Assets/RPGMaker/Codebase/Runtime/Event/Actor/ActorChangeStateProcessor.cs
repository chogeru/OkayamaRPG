using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Actor;

namespace RPGMaker.Codebase.Runtime.Event.Actor
{
    /// <summary>
    /// [アクター]-[ステートの変更]
    /// </summary>
    public class ActorChangeStateProcessor : AbstractEventCommandProcessor
    {
        private ActorChangeState _actor;

        protected override void Process(string eventId, EventDataModel.EventCommand command) {
            if (_actor == null)
            {
                _actor = new ActorChangeState();
                _actor.Init(
                    DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels,
                    DataManager.Self().GetRuntimeSaveDataModel().variables,
                    DataManager.Self().GetActorDataModels()
                );
            }

            var state = DataManager.Self().GetStateDataModel(command.parameters[3]);
            _actor.ChangeState(state, command);
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