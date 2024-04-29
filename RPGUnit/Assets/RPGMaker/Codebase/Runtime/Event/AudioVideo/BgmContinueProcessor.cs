using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;

namespace RPGMaker.Codebase.Runtime.Event.AudioVideo
{
    /// <summary>
    /// [オーディオビデオ]-[BGMの再開]
    /// </summary>
    public class BgmContinueProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            SoundManager.Self().Init();
            //サウンドの保存
            SoundManager.Self().ContinueBgm();
            //次のイベントへ
            ProcessEndAction();
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}