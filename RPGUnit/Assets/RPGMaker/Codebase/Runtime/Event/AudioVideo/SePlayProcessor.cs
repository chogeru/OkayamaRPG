using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;

namespace RPGMaker.Codebase.Runtime.Event.AudioVideo
{
    /// <summary>
    /// [オーディオビデオ]-[SEの演奏]
    /// </summary>
    public class SePlayProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            //サウンドデータの生成
            var sound = new SoundCommonDataModel(command.parameters[0], int.Parse(command.parameters[3]),
                int.Parse(command.parameters[2]), int.Parse(command.parameters[1]));
            SoundManager.Self().Init();
            //データのセット
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, sound);
            //サウンドの再生
            SoundManager.Self().PlaySe();
            //次のイベントへ
            ProcessEndAction();
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}