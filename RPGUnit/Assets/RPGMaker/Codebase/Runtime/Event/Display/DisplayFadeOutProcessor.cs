using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;

namespace RPGMaker.Codebase.Runtime.Event.Display
{
    /// <summary>
    /// [画面]-[画面のフェードアウト]
    /// </summary>
    public class DisplayFadeOutProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            HudDistributor.Instance.NowHudHandler().DisplayInit();
            HudDistributor.Instance.NowHudHandler().FadeOut(ProcessEndAction, UnityEngine.Color.black);
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}