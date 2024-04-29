using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Event.Picture
{
    /// <summary>
    /// [ピクチャ]-[ピクチャの色調変更]
    /// </summary>
    public class PictureChangeColorProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            HudDistributor.Instance.NowHudHandler().PictureInit();
            if (int.TryParse(command.parameters[0], out var result) == false ||
                HudDistributor.Instance.NowHudHandler().GetPicture(int.Parse(command.parameters[0])) == null)
            {
                ProcessEndAction();
                return;
            }

            var color = new Color(int.Parse(command.parameters[2]), int.Parse(command.parameters[3]),
                int.Parse(command.parameters[4]));
            HudDistributor.Instance.NowHudHandler().PlayChangeColor(
                ProcessEndAction,
                color,
                int.Parse(command.parameters[0]),
                int.Parse(command.parameters[5]),
                int.Parse(command.parameters[6]),
                command.parameters[7] == "1"
            );
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}