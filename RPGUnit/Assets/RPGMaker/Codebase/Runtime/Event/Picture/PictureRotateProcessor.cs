using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;

namespace RPGMaker.Codebase.Runtime.Event.Picture
{
    /// <summary>
    /// [ピクチャ]-[ピクチャの回転]
    /// </summary>
    public class PictureRotateProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            HudDistributor.Instance.NowHudHandler().PictureInit();
            HudDistributor.Instance.NowHudHandler().PlayRotation(
                int.Parse(command.parameters[0]),
                int.Parse(command.parameters[1]));
            SendBackToLauncher.Invoke();
        }
    }
}