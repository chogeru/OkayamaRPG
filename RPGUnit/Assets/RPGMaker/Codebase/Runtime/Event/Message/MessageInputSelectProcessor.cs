using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Message;
using RPGMaker.Codebase.Runtime.Common.Enum;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UI;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event.EventDataModel;

namespace RPGMaker.Codebase.Runtime.Event.Message
{
    /// <summary>
    /// [メッセージ]-[選択肢]
    /// </summary>
    public class MessageInputSelectProcessor : AbstractEventCommandProcessor
    {
        private int _cancelIndex = -1;
        private List<int> _changeIndex;
        private bool _isCancelFlag;
        private bool _isFirstSelect;
        private EventCommand _targetCommand;
        private bool _isSendback = false;

        private MessageInputSelect _messageInputSelect;

        protected override void Process(string eventId, EventCommand command, List<EventCommand> eventCommands) {
            _isFirstSelect = false;
            _targetCommand = command;
            if (!HudDistributor.Instance.NowHudHandler().IsInputSelectWindowActive())
                _messageInputSelect = HudDistributor.Instance.NowHudHandler().OpenInputSelectWindow();
            _changeIndex = new List<int>();
            _isSendback = false;

            var nowIndex = EventCommandChainLauncher.GetIndex();
            var nowIndent = _targetCommand.indent;

            // 選択肢テキストの反映
            var selectText = new List<string>();
            for (var i = nowIndex; i < eventCommands.Count; i++)
                if (eventCommands[i].code == (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_SELECTED &&
                    eventCommands[i].indent == nowIndent)
                {
                    selectText.Add(eventCommands[i].parameters[2]);
                    // 選択肢毎のジャンプ先を入れる
                    _changeIndex.Add(i);
                }
                else if (eventCommands[i].code == (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_CANCELED &&
                         eventCommands[i].indent == nowIndent)
                {
                    _cancelIndex = i;
                }
                else if (eventCommands[i].code == (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_END &&
                         eventCommands[i].indent == nowIndent)
                {
                    break;
                }

            // HUDに選択肢テキストを適用
            for (var i = 0; i < selectText.Count; i++)
                HudDistributor.Instance.NowHudHandler().ActiveSelectFrame(selectText[i]);

            if (_targetCommand.parameters[0] == "0")
                return;

            //ウィンドウカラー設定
            HudDistributor.Instance.NowHudHandler().SetInputSelectWindowColor(int.Parse(_targetCommand.parameters[1]));
            //ウィンドウの位置設定
            HudDistributor.Instance.NowHudHandler().SetInputSelectWindowPos(int.Parse(_targetCommand.parameters[2]));
            //キャンセル禁止
            if (int.Parse(_targetCommand.parameters[4]) != -1)
            {
                InputDistributor.AddInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT,HandleType.Back, ProcessCancelAction);
                _isCancelFlag = true;
            }

            InputDistributor.AddInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT,HandleType.Decide, ProcessDecideAction);
            InputDistributor.AddInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT,HandleType.UpKeyDown, UpCursor);
            InputDistributor.AddInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT,HandleType.LeftKeyDown, UpCursor);
            InputDistributor.AddInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT,HandleType.DownKeyDown, DownCursor);
            InputDistributor.AddInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT,HandleType.RightKeyDown, DownCursor);

            bool flg = false;
            var buttons = _messageInputSelect.GetButtons();
            for (var i = 0; i < buttons.Count; i++)
            {
                int index = i;
                buttons[i].GetComponent<WindowButtonBase>().OnClick = new Button.ButtonClickedEvent();
                buttons[i].GetComponent<WindowButtonBase>().OnClick.AddListener(() =>
                {
                    if (buttons != null && buttons.Count > index && buttons[index] != null)
                        ProcessClickDecideAction(index);
                });
                buttons[i].GetComponent<WindowButtonBase>().OnFocus = new Button.ButtonClickedEvent();
                buttons[i].GetComponent<WindowButtonBase>().OnFocus.AddListener(() =>
                {
                    _messageInputSelect.SetSelectNum(index);
                    _isFirstSelect = true;
                });

                //マスタデータ不備により、indexがずれている可能性があるため、選択肢が範囲内かどうかも合わせてチェックする
                if (int.Parse(_targetCommand.parameters[3]) == index && int.Parse(_targetCommand.parameters[3]) < _changeIndex.Count)
                {
                    FirstFocusSetting(buttons[i], index);
                    flg = true;
                }
            }

            //ちらつき防止
            if (flg)
            {
                for (int i = 0; i < buttons.Count; i++)
                {
                    buttons[i].GetComponent<WindowButtonBase>().SetTransparent(true);
                }
            }
        }

        private async void FirstFocusSetting(Button button, int index) {
            try
            {
                await Task.Delay(100);

                //初期フォーカス設定
                if (_isSendback) return;
                _messageInputSelect.SetSelectNum(index);
                _isFirstSelect = true;
                button.Select();

                await Task.Delay(1);

                //ちらつき防止
                if (_isSendback) return;
                var buttons = _messageInputSelect.GetButtons();
                for (int i = 0; i < buttons.Count; i++)
                {
                    buttons[i].GetComponent<WindowButtonBase>().SetTransparent(false);
                }
            } catch (Exception) { }
        }

        private void UpCursor() {
            if (!_isFirstSelect)
            {
                _messageInputSelect.SetSelectNum(_messageInputSelect.GetActiveButtonsCount() - 1);
                _isFirstSelect = true;
            }
        }

        private void DownCursor() {
            if (!_isFirstSelect)
            {
                _messageInputSelect.SetSelectNum(0);
                _isFirstSelect = true;
            }
        }

        private void ProcessDecideAction() {
            // 選択肢によってindexを変更
            var selectNum = HudDistributor.Instance.NowHudHandler().GetSelectNum();
            if (selectNum < 0) return;
            EventCommandChainLauncher.SetIndex(_changeIndex[selectNum]);

            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, DataManager.Self().GetSystemDataModel().soundSetting.ok);
            SoundManager.Self().PlaySe();

            DeregisterInputAction();
            HudDistributor.Instance.NowHudHandler().CloseInputSelectWindow();
            HudDistributor.Instance.NowHudHandler().CloseMessageWindow();
            SendBack();
        }

        private void ProcessClickDecideAction(int i) {
            // 選択肢によってindexを変更
            _messageInputSelect.SetSelectNum(i);
            var selectNum = HudDistributor.Instance.NowHudHandler().GetSelectNum();
            EventCommandChainLauncher.SetIndex(_changeIndex[selectNum]);

            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, DataManager.Self().GetSystemDataModel().soundSetting.ok);
            SoundManager.Self().PlaySe();

            DeregisterInputAction();
            HudDistributor.Instance.NowHudHandler().CloseInputSelectWindow();
            HudDistributor.Instance.NowHudHandler().CloseMessageWindow();
            SendBack();
        }

        /// <summary>
        ///     キャンセルキー押下時の挙動
        /// </summary>
        /// <returns></returns>
        private void ProcessCancelAction() {
            // この段階で設定されていない場合は[キャンセル]に選択肢の番号が登録されている
            if (_cancelIndex == -1)
            {
                int index;
                if (!int.TryParse(_targetCommand.parameters[4], out index))
                    _cancelIndex = -1;
                else
                    _cancelIndex = _changeIndex[index];
            }

            if (_cancelIndex >= 0)
                EventCommandChainLauncher.SetIndex(_cancelIndex);

            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, DataManager.Self().GetSystemDataModel().soundSetting.cancel);
            SoundManager.Self().PlaySe();

            DeregisterInputAction();
            HudDistributor.Instance.NowHudHandler().CloseInputSelectWindow();
            HudDistributor.Instance.NowHudHandler().CloseMessageWindow();
            SendBack();
        }

        private void SendBack() {
            _isSendback = true;
            _cancelIndex = -1;
            SendBackToLauncher.Invoke();
        }

        private void DeregisterInputAction()
        {
            if (_isCancelFlag)
            {
                InputDistributor.RemoveInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT, HandleType.Back, ProcessCancelAction);
            }
            InputDistributor.RemoveInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT, HandleType.Decide, ProcessDecideAction);
            InputDistributor.RemoveInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT, HandleType.UpKeyDown, UpCursor);
            InputDistributor.RemoveInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT, HandleType.LeftKeyDown, UpCursor);
            InputDistributor.RemoveInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT, HandleType.DownKeyDown, DownCursor);
            InputDistributor.RemoveInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT, HandleType.RightKeyDown, DownCursor);
        }
    }
}