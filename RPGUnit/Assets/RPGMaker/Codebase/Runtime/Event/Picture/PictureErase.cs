using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;

namespace RPGMaker.Codebase.Runtime.Event.Picture
{
    /// <summary>
    /// [ピクチャ]-[ピクチャ消去]
    /// </summary>
    public class PictureErase : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            HudDistributor.Instance.NowHudHandler().PictureInit();
            if (int.TryParse(command.parameters[0], out var result) == false ||
                HudDistributor.Instance.NowHudHandler().GetPicture(int.Parse(command.parameters[0])) == null)
            {
                ProcessEndAction();
                return;
            }

            HudDistributor.Instance.NowHudHandler().DeletePicture(int.Parse(command.parameters[0]));
            ProcessEndAction();
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}