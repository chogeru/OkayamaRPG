using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;

namespace RPGMaker.Codebase.Runtime.Event.AudioVideo
{
    /// <summary>
    /// [オーディオビデオ]-[BGSのフェードアウト]
    /// </summary>
    public class BgsFadeoutProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            SoundManager.Self().Init();
            //サウンドの再生
            SoundManager.Self().FadeoutBgm(SoundManager.SYSTEM_AUDIO_BGS, int.Parse(command.parameters[0]));
            //次のイベントへ
            ProcessEndAction();
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}