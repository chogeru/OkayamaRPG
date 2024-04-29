using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Message;
using RPGMaker.Codebase.Runtime.Common.Enum;

namespace RPGMaker.Codebase.Runtime.Event.Message
{
    /// <summary>
    /// [メッセージ]-[数値入力の処理]
    /// </summary>
    public class MessageInputNumberProcessor : AbstractEventCommandProcessor
    {

        private MessageInputNumber _messageInputNumber;
        
        //変数のIndex
        private int _index;

        //現在入力されている値
        private int _number;
        //今の桁

        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            _number = 0;
            // 変数のindex取得
            var varData = new DatabaseManagementService().LoadFlags().variables;
            for (var i = 0; i < varData.Count; i++)
                if (varData[i].id == command.parameters[0])
                {
                    _index = i;
                    break;
                }

            if (!HudDistributor.Instance.NowHudHandler().IsInputNumWindowActive())
            {
                _messageInputNumber = HudDistributor.Instance.NowHudHandler().OpenInputNumWindow(command.parameters[1]);
            }
            InputDistributor.AddInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT,HandleType.Decide, ProcessEndAction);
            InputDistributor.AddInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT,HandleType.LeftKeyDown, LeftMoveCursor);
            InputDistributor.AddInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT,HandleType.RightKeyDown, RightMoveCursor);
            InputDistributor.AddInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT,HandleType.UpKeyDown, NumAdd);
            InputDistributor.AddInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT,HandleType.DownKeyDown, NumExcept);
            
            _messageInputNumber.DownButton.onClick.AddListener(NumExcept);
            _messageInputNumber.UpButton.onClick.AddListener(NumAdd);
            _messageInputNumber.DecideButton.onClick.AddListener(ProcessEndAction);

            for (int i = 0; i < _messageInputNumber.NumbersButton.Count; i++)
            {
                int index = i;
                _messageInputNumber.NumbersButton[index].onClick.AddListener(() =>
                {
                    ClickMoveCursor(index);
                });
            }
        }

        private void ClickMoveCursor(int index) {
            _messageInputNumber.ClickCursor(index);
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, DataManager.Self().GetSystemDataModel().soundSetting.cursor);
            SoundManager.Self().PlaySe();
        }

        //一桁、二桁の切り替え
        private void LeftMoveCursor() {
            _number = HudDistributor.Instance.NowHudHandler().InputNumWindowOperation(HandleType.Left);
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, DataManager.Self().GetSystemDataModel().soundSetting.cursor);
            SoundManager.Self().PlaySe();
        }

        private void RightMoveCursor() {
            _number = HudDistributor.Instance.NowHudHandler().InputNumWindowOperation(HandleType.Right);
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, DataManager.Self().GetSystemDataModel().soundSetting.cursor);
            SoundManager.Self().PlaySe();
        }

        //数値増減
        private void NumAdd() {
            _number = HudDistributor.Instance.NowHudHandler().InputNumWindowOperation(HandleType.Up);
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, DataManager.Self().GetSystemDataModel().soundSetting.cursor);
            SoundManager.Self().PlaySe();
        }

        private void NumExcept() {
            _number = HudDistributor.Instance.NowHudHandler().InputNumWindowOperation(HandleType.Down);
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, DataManager.Self().GetSystemDataModel().soundSetting.cursor);
            SoundManager.Self().PlaySe();
        }

        private void ProcessEndAction() {
            var data = DataManager.Self().GetRuntimeSaveDataModel();
            data.variables.data[_index] = HudDistributor.Instance.NowHudHandler().InputNumNumber().ToString();

            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, DataManager.Self().GetSystemDataModel().soundSetting.ok);
            SoundManager.Self().PlaySe();

            //キー入力の削除
            InputDistributor.RemoveInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT, HandleType.LeftKeyDown, LeftMoveCursor);
            InputDistributor.RemoveInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT, HandleType.RightKeyDown, RightMoveCursor);
            InputDistributor.RemoveInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT, HandleType.UpKeyDown, NumAdd);
            InputDistributor.RemoveInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT, HandleType.DownKeyDown, NumExcept);
            InputDistributor.RemoveInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT, HandleType.Decide, ProcessEndAction);
            // メッセージウィンドウを閉じて、ランチャーに戻す
            HudDistributor.Instance.NowHudHandler().CloseInputNumWindow();
            HudDistributor.Instance.NowHudHandler().CloseMessageWindow();
            SendBackToLauncher.Invoke();
        }
    }
}