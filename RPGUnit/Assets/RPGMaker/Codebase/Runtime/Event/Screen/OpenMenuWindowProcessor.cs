using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map;

namespace RPGMaker.Codebase.Runtime.Event.Screen
{
    /// <summary>
    /// [シーン制御]-[メニュー画面を開く]
    /// </summary>
    public class OpenMenuWindowProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            //一時的にイベントを中断し、メニューへ遷移する
            MapEventExecutionController.Instance.PauseEvent(ProcessEndAction);
            MapManager.menu.MenuOpen(true);
        }

        private void ProcessEndAction() {
            //キーイベント破棄のため、若干待つ
            TimeHandler.Instance.AddTimeActionFrame(1, ProcessEndActionWait, false);
        }

        private void ProcessEndActionWait() {
            SendBackToLauncher.Invoke();
        }
    }
}