using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;

namespace RPGMaker.Codebase.Runtime.Event.Message
{
    /// <summary>
    /// [メッセージ]-[文章のスクロール]
    /// </summary>
    public class MessageTextScrollProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            if (!HudDistributor.Instance.NowHudHandler().IsMessageScrollWindowActive())
                HudDistributor.Instance.NowHudHandler().OpenMessageScrollWindow();

            //加速数値
            HudDistributor.Instance.NowHudHandler().SetScrollSpeed(int.Parse(command.parameters[0]));
            HudDistributor.Instance.NowHudHandler().SetScrollNoFast(command.parameters[1] == "1");
            ProcessEndAction();
        }


        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}