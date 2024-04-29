using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;

namespace RPGMaker.Codebase.Runtime.Event.Wait
{
    /// <summary>
    /// [タイミング]-[ウェイト]
    /// </summary>
    public class TimingWaitProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            TimeHandler.Instance.AddTimeAction(float.Parse(command.parameters[0]) / 60f, ProcessEndAction, false);
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}