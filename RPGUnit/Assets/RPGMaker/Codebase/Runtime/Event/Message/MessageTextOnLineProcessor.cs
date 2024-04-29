using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Enum;
using System.Threading.Tasks;

namespace RPGMaker.Codebase.Runtime.Event.Message
{
    /// <summary>
    /// [メッセージ]-[文章の表示] 文章表示部分
    /// </summary>
    public class MessageTextOnLineProcessor : AbstractEventCommandProcessor
    {
        private bool _isEnd;
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            if (!HudDistributor.Instance.NowHudHandler().IsMessageWindowActive())
                HudDistributor.Instance.NowHudHandler().OpenMessageWindow();

            _isEnd = false;
            HudDistributor.Instance.NowHudHandler().SetShowMessage(command.parameters[0]);

            // 次イベントが選択肢か数値入力であれば、入力待ちをせずに先に進む
            if (EventCommandChainLauncher.GetNextEventCode() != CoreSystem.Knowledge.Enum.EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT &&
                EventCommandChainLauncher.GetNextEventCode() != CoreSystem.Knowledge.Enum.EventEnum.EVENT_CODE_MESSAGE_INPUT_NUMBER)
                HudDistributor.Instance.NowHudHandler().SetIsNotWaitInput(false);
            else
                HudDistributor.Instance.NowHudHandler().SetIsNotWaitInput(true);

            DelayAndSetEvent();
            TimeHandler.Instance.AddTimeActionEveryFrame(ProcessNotWaitInputEnd);
        }

        private async void DelayAndSetEvent() {
            await Task.Delay(100);
            if (_isEnd) return;
            InputDistributor.AddInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT, HandleType.Decide, ProcessEndAction);
            InputDistributor.AddInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT, HandleType.LeftClick, ProcessEndAction);
        }

        private void ProcessEndAction() {
            if (HudDistributor.Instance.NowHudHandler().IsInputWait())
            {
                HudDistributor.Instance.NowHudHandler().Next();
                SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, DataManager.Self().GetSystemDataModel().soundSetting.ok);
                SoundManager.Self().PlaySe();
                return;
            }

            if (!HudDistributor.Instance.NowHudHandler().IsInputEnd())
            {
                return;
            }

            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, DataManager.Self().GetSystemDataModel().soundSetting.ok);
            SoundManager.Self().PlaySe();

            TimeHandler.Instance.RemoveTimeAction(ProcessNotWaitInputEnd);

            var gameState =
                GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT;
            InputDistributor.RemoveInputHandler(gameState, HandleType.Decide, ProcessEndAction);
            InputDistributor.RemoveInputHandler(gameState, HandleType.LeftClick, ProcessEndAction);

            // 次イベントが選択肢か数値入力であれば閉じない
            if (EventCommandChainLauncher.GetNextEventCode() != CoreSystem.Knowledge.Enum.EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT &&
                EventCommandChainLauncher.GetNextEventCode() != CoreSystem.Knowledge.Enum.EventEnum.EVENT_CODE_MESSAGE_INPUT_NUMBER)
                HudDistributor.Instance.NowHudHandler().CloseMessageWindow();

            //次のシーケンス実行にWaitを挟まない
            _skipWait = true;
            _isEnd = true;
            SendBackToLauncher.Invoke();
        }

        private void ProcessNotWaitInputEnd() {
            if (HudDistributor.Instance.NowHudHandler().IsNotWaitInput())
            {
                if (!HudDistributor.Instance.NowHudHandler().IsInputEnd()) return;
                TimeHandler.Instance.RemoveTimeAction(ProcessNotWaitInputEnd);

                InputDistributor.RemoveInputHandler(
                    GameStateHandler.IsMap()
                        ? GameStateHandler.GameState.EVENT
                        : GameStateHandler.GameState.BATTLE_EVENT, HandleType.Decide, ProcessEndAction);
                InputDistributor.RemoveInputHandler(
                    GameStateHandler.IsMap()
                        ? GameStateHandler.GameState.EVENT
                        : GameStateHandler.GameState.BATTLE_EVENT, HandleType.LeftClick, ProcessEndAction);

                // 次イベントが選択肢か数値入力であれば閉じない
                if (EventCommandChainLauncher.GetNextEventCode() != CoreSystem.Knowledge.Enum.EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT &&
                    EventCommandChainLauncher.GetNextEventCode() != CoreSystem.Knowledge.Enum.EventEnum.EVENT_CODE_MESSAGE_INPUT_NUMBER)
                    HudDistributor.Instance.NowHudHandler().CloseMessageWindow();

                //次のシーケンス実行にWaitを挟まない
                _skipWait = true;
                _isEnd = true;
                SendBackToLauncher.Invoke();
            }
        }
    }
}