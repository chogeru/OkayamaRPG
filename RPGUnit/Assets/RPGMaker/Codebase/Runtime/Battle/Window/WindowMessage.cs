using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.ControlCharacter;
using TMPro;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Battle.Window
{
    /// <summary>
    /// [文章の表示]をするウィンドウ
    /// Uniteではバトル終了時の、画面下部に表示するメッセージでのみ利用する
    /// また、文章自体は ControlCharacter で表示制御を行う
    /// </summary>
    public class WindowMessage : WindowBattle
    {
        private ControlCharacter _controlCharacter = null;
        private TextState _textState;
        private bool NextPage;
        [SerializeField] protected TextMeshProUGUI textField;

        /// <summary>
        /// 初期化
        /// </summary>
        override public void Initialize() {
            //共通UIの適応を開始
            Init();
            base.Initialize();
            InitMembers();
        }

        /// <summary>
        /// メンバ変数を初期化
        /// </summary>
        public void InitMembers() {
            _textState = null;
            Openness = 0;
            textField.text = "";
            NextPage = false;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        override public void UpdateTimeHandler() {
            CheckToNotClose();
            base.UpdateTimeHandler();
            while (!IsOpening() && !IsClosing())
                if (UpdateInput())
                {
                    return;
                }
                else if (UpdateMessage())
                {
                    return;
                }
                else if (CanStart())
                {
                    StartMessage();
                }
                else
                {
                    return;
                }
        }

        /// <summary>
        /// 閉じないようにチェック
        /// </summary>
        public void CheckToNotClose() {
            if (IsClosing() && IsOpen())
                if (DoesContinue())
                    Open();
        }

        /// <summary>
        /// 開始可能か
        /// </summary>
        /// <returns></returns>
        public bool CanStart() {
            return DataManager.Self().GetGameMessage().HasText();
        }

        /// <summary>
        /// メッセージ表示の開始
        /// </summary>
        public void StartMessage() {
            //メッセージを表示中の場合は処理しない
            if (_textState != null && _textState.text != null)
                return;

            //メッセージを取得する
            textField.text = "";
            _textState = new TextState();
            _textState.index = 0;
            _textState.text = DataManager.Self().GetGameMessage().AllText();
            
            //メッセージをControlCharacterへ渡して、表示を移譲する
            //未だControlCharacterを生成していない場合、作成する
            if (_controlCharacter == null)
            {
                var setting = DataManager.Self().GetUiSettingDataModel().talkMenu.characterMenu.talkFontSetting;
                _controlCharacter = textField.gameObject.AddComponent<ControlCharacter>();
                _controlCharacter.InitControl(textField.gameObject, _textState.text,
                    DataManager.Self().GetUiSettingDataModel().commonMenus[0].menuFontSetting.font,
                    DataManager.Self().GetUiSettingDataModel().commonMenus[0].menuFontSetting.size,
                    new Color(DataManager.Self().GetUiSettingDataModel().commonMenus[0].menuFontSetting.color[0] / 255f,
                                DataManager.Self().GetUiSettingDataModel().commonMenus[0].menuFontSetting.color[1] / 255f,
                                DataManager.Self().GetUiSettingDataModel().commonMenus[0].menuFontSetting.color[2] / 255f),
                    null,
                    isAllSkip: false);
            }

            //メッセージ表示開始
            if (_controlCharacter != null)
            {
                _controlCharacter.ResetForBattleMessageWindow(_textState.text);
                _controlCharacter.ExecCharacter();
            }

            Opacity = 255;
            Open();
        }

        /// <summary>
        /// メッセージを停止してウィンドウを閉じる
        /// </summary>
        public void TerminateMessage() {
            Close();

            DataManager.Self().GetGameMessage().Clear();
        }

        /// <summary>
        /// 入力のアップデート
        /// </summary>
        /// <returns></returns>
        public bool UpdateInput() {
            //ControlCharacterでの表示が完了し、キー入力待ちかどうかの判定
            if (Pause)
            {
                if (IsTriggered())
                {
                    //キー入力待ち状態を解除
                    Pause = false;
                    //メッセージを消去し、次のシーケンスに進む
                    _textState.text = null;
                    TerminateMessage();
                }

                return true;
            }
            else if (NextPage)
            {
                if (IsTriggered())
                {
                    //キー入力待ち状態を解除
                    NextPage = false;
                    _textState.index = 0;

                    //制御文字による改ページの場合
                    if (_controlCharacter.IsNextPageControlCharacter)
                    {
                        var trimStringEndWord = "\\f";
                        _textState.text = _textState.text.
                            Substring(_textState.text.IndexOf(trimStringEndWord) + trimStringEndWord.Length).
                            TrimStart('\n');
                    }
                    //文字数オーバーによる改ページの場合
                    else
                    {
                        _textState.text = _textState.text.Substring(_controlCharacter.processer.NowIndex);
                    }

                    _controlCharacter.InitControl(textField.gameObject, _textState.text,
                        DataManager.Self().GetUiSettingDataModel().commonMenus[0].menuFontSetting.font,
                        DataManager.Self().GetUiSettingDataModel().commonMenus[0].menuFontSetting.size,
                        new Color(DataManager.Self().GetUiSettingDataModel().commonMenus[0].menuFontSetting.color[0] / 255f,
                                    DataManager.Self().GetUiSettingDataModel().commonMenus[0].menuFontSetting.color[1] / 255f,
                                    DataManager.Self().GetUiSettingDataModel().commonMenus[0].menuFontSetting.color[2] / 255f),
                        null,
                        isAllSkip: false);

                    _controlCharacter.ResetForBattleMessageWindow(_textState.text);
                    _controlCharacter.ExecCharacter();
                }
            }

            return false;
        }

        /// <summary>
        /// メッセージのアップデート
        /// </summary>
        /// <returns></returns>
        public bool UpdateMessage() {
            if (_textState != null && _textState.text != null)
            {
                //ControlCharacterで文字を表示中の場合、待ち状態に入るか、メッセージを最後まで
                //再生し終わっている時に、キー入力待ち状態とする
                if (_controlCharacter != null)
                {
                    Pause = _controlCharacter.IsWaitForButtonInput;
                    NextPage = _controlCharacter.IsNextPage;
                    if (NextPage)
                    {
                        Pause = false;
                    }
                    else if (!Pause && _controlCharacter.IsEnd)
                    {
                        Pause = true;
                    }
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 決定・キャンセルなどのトリガが起動されたか
        /// </summary>
        /// <returns></returns>
        public bool IsTriggered() {
            if (InputHandler.OnDown(Common.Enum.HandleType.LeftClick) || 
                InputHandler.OnDown(Common.Enum.HandleType.Decide) || 
                InputHandler.OnDown(Common.Enum.HandleType.Back) || 
                InputHandler.OnDown(Common.Enum.HandleType.RightClick) ||
                InputHandler.OnPress(Common.Enum.HandleType.LeftClick) ||
                InputHandler.OnPress(Common.Enum.HandleType.Decide) ||
                InputHandler.OnPress(Common.Enum.HandleType.Back) ||
                InputHandler.OnPress(Common.Enum.HandleType.RightClick))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// まだテキストがあり、かつ設定が変更されていないか
        /// </summary>
        /// <returns></returns>
        public bool DoesContinue() {
            return DataManager.Self().GetGameMessage().HasText();
        }

        /// <summary>
        /// ウィンドウが開いている途中か
        /// </summary>
        /// <returns></returns>
        override public bool IsOpening() {
            return base.IsOpening();
        }

        /// <summary>
        /// ウィンドウが閉じている途中か
        /// </summary>
        /// <returns></returns>
        override public bool IsClosing() {
            return base.IsClosing();
        }

        /// <summary>
        /// ウィンドウを閉じる
        /// </summary>
        public override void Close() {
            base.Close();
        }
    }
}