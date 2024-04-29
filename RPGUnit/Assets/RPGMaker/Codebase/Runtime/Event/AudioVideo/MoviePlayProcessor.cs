using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;

namespace RPGMaker.Codebase.Runtime.Event.AudioVideo
{
    /// <summary>
    /// ムービーの再生
    /// </summary>
    public class MoviePlayProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            if (SoundManager.Self().IsMovie(command.parameters[0]))
            {
                //初期化
                HudDistributor.Instance.NowHudHandler().MovieInit();
                
                //読み込む動画名を入れて再生
                HudDistributor.Instance.NowHudHandler().AddMovie(command.parameters[0], ()=>
                {
                    //次のイベントへ
                    ProcessEndAction();
                });
            }
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
        
    }
}