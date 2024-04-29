//#define USE_TRACE_PRINT

#if USE_TRACE_PRINT
using RPGMaker.Codebase.CoreSystem.Helper;
#endif
using System.Diagnostics;

namespace RPGMaker.Codebase.Runtime.Common
{
    /// <summary>
    ///     ゲーム実行中の、ゲームの状態を保持するクラス
    /// </summary>
    public static class GameStateHandler
    {
        /// <summary>
        ///     ゲームの状態の定義. ゲームの状態とは、表示する画面を差しています.
        /// </summary>
        public enum GameState
        {
            NONE = -1,
            TITLE = 0,
            MAP,
            MENU,
            BATTLE,
            EVENT,
            BATTLE_EVENT,
            GAME_OVER,
            BEFORE_TITLE,
            MAX = 99
        }

        /// <summary>
        ///     ゲームの状態
        /// </summary>
        private static GameState _currentGameState = GameState.NONE;

        /// <summary>
        /// 現在の状態を返却する.
        /// </summary>
        /// <returns>現在の状態</returns>
        public static GameState CurrentGameState() {
            return _currentGameState;
        }
        /// <summary>
        ///     新しい状態を登録する.
        /// </summary>
        /// <param name="gameState">新しい状態</param>
        /// <returns>状態を更新した場合はtrue</returns>
        public static void SetGameState(GameState gameState) {
            if (gameState == _currentGameState)
            {
                return;
            }

            TracePrint(gameState);
            _currentGameState = gameState;
            InputDistributor.RenewInputHandler();
        }

        /// <summary>
        ///     現在の状態がMAP、MENU、EVENTかどうかを返却する
        /// </summary>
        /// <returns>現在の状態がMAP、MENU、EVENTの場合true</returns>
        public static bool IsMap() {
            if (_currentGameState == GameState.MAP ||
                _currentGameState == GameState.EVENT ||
                _currentGameState == GameState.MENU)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     現在の状態がMENUかどうかを返却する
        /// </summary>
        /// <returns>現在の状態がMAP、MENU、EVENTの場合true</returns>
        public static bool IsMenu() {
            if (_currentGameState == GameState.MENU)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     現在の状態がBATTLEまたは、BATTLE_EVENTかどうかを返却する
        /// </summary>
        /// <returns>現在の状態がMAPまたは、EVENTの場合true</returns>
        public static bool IsBattle() {
            if (_currentGameState == GameState.BATTLE || _currentGameState == GameState.BATTLE_EVENT)
            {
                return true;
            }

            return false;
        }

        [Conditional("USE_TRACE_PRINT")]
        private static void TracePrint(GameState gameState)
        {
#if USE_TRACE_PRINT
            DebugUtil.Log($"## GameState = {_currentGameState} -> {gameState}");
#endif
        }
    }
}