using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;

namespace RPGMaker.Codebase.Runtime.Event.AudioVideo
{
    /// <summary>
    /// [オーディオビデオ]-[BGMの演奏]
    /// </summary>
    public class BgmPlayProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            var soundName = command.parameters[0];
            if (!SoundManager.Self().IsBgmPlaying(soundName))
            {
                SoundManager.Self().Init();

                //サウンドデータの生成(曲名、位相、ピッチ、ボリューム)
                var sound = new SoundCommonDataModel(
                    soundName,
                    int.Parse(command.parameters[3]),
                    int.Parse(command.parameters[2]),
                    int.Parse(command.parameters[1]));

                //データのセット
                SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_BGM, sound);

                //BGM再生
                PlaySound();
                return;
            }

            //次のイベントへ
            ProcessEndAction();
        }

        private async void PlaySound() {
            //サウンドの再生
            await SoundManager.Self().PlayBgm();

            //次のイベントへ
            ProcessEndAction();
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}