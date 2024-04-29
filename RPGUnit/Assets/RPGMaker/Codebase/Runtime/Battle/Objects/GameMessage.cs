using RPGMaker.Codebase.Runtime.Common;
using System.Collections.Generic;

namespace RPGMaker.Codebase.Runtime.Battle.Objects
{
    /// <summary>
    /// メッセージや選択肢の文字列や設定をこのクラスに一時保存し、ウィンドウが表示の際に参照するクラス
    /// Game_Message はデータを保持しているだけで、表示の際はウィンドウ側から参照される
    /// なお、メッセージの終了待ちには Game_Interpreter.setWaitMode() メソッドが使われる
    /// 
    /// イベント関連は別で処理しており、本クラスではバトルのメッセージ表示しか行わない予定
    /// 最低限の処理のみ残して、他は削除する
    /// </summary>
    public class GameMessage
    {
        /// <summary>
        /// 文章
        /// </summary>
        private List<string> _texts       = new List<string>();

        /// <summary>
        /// 消去
        /// </summary>
        public void Clear() {
            _texts = new List<string>();
        }

        /// <summary>
        /// テキストを追加する
        /// </summary>
        /// <param name="text"></param>
        public void Add(string text) {
            _texts.Add(text);
        }

        /// <summary>
        /// メッセージがテキストを持っているか
        /// </summary>
        /// <returns></returns>
        public bool HasText() {
            return _texts.Count > 0;
        }

        /// <summary>
        /// 表示や入力・選択の最中か
        /// Uniteでは選択肢、数値入力、アイテム選択はイベントで表示するため、それでの判定を行う
        /// </summary>
        /// <returns></returns>
        public bool IsBusy() {
            return HasText() || (GameStateHandler.CurrentGameState() == GameStateHandler.GameState.BATTLE_EVENT);
        }

        /// <summary>
        /// 次のページを生成する
        /// </summary>
        public void NewPage() {
            if (_texts.Count > 0)
            {
                _texts[_texts.Count - 1] += "\\f";
            }
        }

        /// <summary>
        /// メッセージに含まれるすべてのテキストを返す
        /// </summary>
        /// <returns></returns>
        public string AllText() {
            return string.Join("\n", _texts);
        }
    }
}