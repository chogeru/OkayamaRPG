using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static RPGMaker.Codebase.Runtime.Common.InputHandler;
using HandleType = RPGMaker.Codebase.Runtime.Common.Enum.HandleType;

namespace RPGMaker.Codebase.Runtime.Common
{
    /// <summary>
    /// ゲームの状態に応じて、実行するべきキーやマウスイベントを決定する
    /// </summary>
    public static class InputDistributor
    {

        /// <summary>
        /// ゲームの状態に応じたInputHandlerHandler
        /// </summary>
        private static Dictionary<GameStateHandler.GameState, Dictionary<HandleType, Dictionary<string, Action>>> _stateInputHandlerList = new Dictionary<GameStateHandler.GameState, Dictionary<HandleType, Dictionary<string, Action>>>();

        /// <summary>
        /// キーやマウスのイベントに対して実行するコールバック処理の登録
        /// </summary>
        /// <param name="gameState"></param>
        /// <param name="handleType">キーやマウスのイベント種別</param>
        /// <param name="action">実行するコールバック処理</param>
        public static void AddInputHandler(GameStateHandler.GameState gameState, HandleType handleType, Action action, string actionName = "") {
            if (actionName == "")
                actionName = handleType.ToString() + "_" + action.Method.Name;

            //現在のGameStateでイベント登録が行われていない場合、初期化
            if (!_stateInputHandlerList.ContainsKey(gameState))
            {
                _stateInputHandlerList.Add(gameState, new Dictionary<HandleType, Dictionary<string, Action>>());
            }
            if (!_stateInputHandlerList[gameState].ContainsKey(handleType))
            {
                _stateInputHandlerList[gameState].Add(handleType, new Dictionary<string, Action>());
            }

            //現在のGameStateで、同一のイベント登録が行われている場合は、処理終了
            if (_stateInputHandlerList[gameState][handleType].ContainsKey(actionName))
            {
                //同一のイベントが登録済みのため、処理終了
                return;
            }

            //同一のイベント登録が無いため、新たに登録
            _stateInputHandlerList[gameState][handleType][actionName] = action;
            if (GameStateHandler.CurrentGameState() == gameState)
            {
                //同じゲームの状態だった場合、InputHandlerへ登録する
                RegisterInputAction(handleType, action);
            }
        }

        /// <summary>
        /// キーやマウスのイベントに対して実行するコールバック処理の削除
        /// </summary>
        /// <param name="handleType">キーやマウスのイベント種別</param>
        /// <param name="action">実行するコールバック処理</param>
        public static void RemoveInputHandler(GameStateHandler.GameState gameState, HandleType handleType, Action action, string actionName = "") {
            if (actionName == "")
                actionName = handleType.ToString() + "_" + action.Method.Name;

            //現在のGameStateでイベント登録が行われていない場合、初期化する
            //本処理はフェールセーフ
            if (!_stateInputHandlerList.ContainsKey(gameState))
            {
                _stateInputHandlerList.Add(gameState, new Dictionary<HandleType, Dictionary<string, Action>>());
            }
            if (!_stateInputHandlerList[gameState].ContainsKey(handleType))
            {
                _stateInputHandlerList[gameState].Add(handleType, new Dictionary<string, Action>());
            }
            //現在のGameStateで、同一のイベント登録が行われている場合は、処理終了
            if (_stateInputHandlerList[gameState][handleType].ContainsKey(actionName))
            {
                //同一のイベントが登録済みのため、処理終了
                _stateInputHandlerList[gameState][handleType].Remove(actionName);
            }

            DeregisterInputAction(handleType, action);
        }


        /// <summary>
        /// GameStateに紐づくキーやマウスのイベントを破棄する
        /// </summary>
        public static void RemoveInputHandlerWithGameState(GameStateHandler.GameState currentDisplay) {
            if (_stateInputHandlerList.ContainsKey(currentDisplay))
            {
                _stateInputHandlerList.Remove(currentDisplay);
                _stateInputHandlerList.Add(currentDisplay, new Dictionary<HandleType, Dictionary<string, Action>>());
            }
            if (GameStateHandler.CurrentGameState() == currentDisplay)
            {
                //破棄
                AllDeregisterInputAction();
            }
        }

        /// <summary>
        /// ゲーム状態が変更されたら再登録をかける
        /// </summary>
        public static void RenewInputHandler() {
            if (_stateInputHandlerList == null && _stateInputHandlerList?.Count == 0) return;
            if (_stateInputHandlerList != null && !_stateInputHandlerList.ContainsKey(GameStateHandler.CurrentGameState())) return;

            var actionsClone = _stateInputHandlerList?[GameStateHandler.CurrentGameState()].ToList();
            
            if (actionsClone == null) return;

            //破棄
            AllDeregisterInputAction();
            actionsClone.ForEach(action =>
            {
                foreach (var pair in action.Value)
                    RegisterInputAction(action.Key, pair.Value);
            });
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod()]
        static void Init() {
            _stateInputHandlerList = null;
            _stateInputHandlerList = new Dictionary<GameStateHandler.GameState, Dictionary<HandleType, Dictionary<string, Action>>>();
        }
#endif
    }
}