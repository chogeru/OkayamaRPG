using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Addon;

namespace RPGMaker.Codebase.Runtime.Event.AudioVideo
{
    public class AddonCommandProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            var parameters =
                new AddonParameterContainer(JsonHelper.FromJsonArray<AddonParameter>(command.parameters[2]));
            AddonManager.Instance.CallAddonCommand(command.parameters[0], command.parameters[1], parameters, eventID,
                () =>
                {
                    //次のイベントへ
                    ProcessEndAction();
                });
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}