using RPGMaker.Codebase.Runtime.Common.Component;
using System.Collections.Generic;

namespace RPGMaker.Codebase.Runtime.Common
{
    /// <summary>
    ///     ゲームの状態に応じて、実行するべきキーやマウスイベントを決定する
    /// </summary>
    public class HudDistributor
    {
        /// <summary>
        ///     シングルトンのインスタンス
        /// </summary>
        private static HudDistributor instance;

        /// <summary>
        ///     ゲームの状態に応じたHudHandler
        /// </summary>
        private readonly Dictionary<GameStateHandler.GameState, HudHandler> stateHudHandlerList =
            new Dictionary<GameStateHandler.GameState, HudHandler>();


        /// <summary>
        ///     インスタンス返却
        /// </summary>
        public static HudDistributor Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new HudDistributor();
                    
                    var gameObject = new UnityEngine.GameObject();
                    var hudHandler = new HudHandler(gameObject);
                    if (instance.stateHudHandlerList.ContainsKey(GameStateHandler.GameState.MAX))
                    {
                        instance.stateHudHandlerList.Remove(GameStateHandler.GameState.MAX);
                    }
                    instance.stateHudHandlerList.Add(GameStateHandler.GameState.MAX, hudHandler);
                }

                return instance;
            }
        }

        /// <summary>
        ///     現在のHudHandlerを返却
        /// </summary>
        /// <returns>現在のHudHandler</returns>
        public HudHandler NowHudHandler() {
            if (GameStateHandler.IsMap())
                return stateHudHandlerList[GameStateHandler.GameState.MAP];
            if (GameStateHandler.IsBattle()) 
                return stateHudHandlerList[GameStateHandler.GameState.BATTLE];
            return null;
        }

        public HudHandler StaticHudHandler() {
            return stateHudHandlerList[GameStateHandler.GameState.MAX];
        }

        /// <summary>
        ///     HudHandlerを登録
        /// </summary>
        /// <returns>新規に登録した場合true</returns>
        public bool AddHudHandler(HudHandler hudHandler) {
            if (GameStateHandler.IsMap())
            {
                if (stateHudHandlerList.ContainsKey(GameStateHandler.GameState.MAP))
                    RemoveHudHandler();
                stateHudHandlerList.Add(GameStateHandler.GameState.MAP, hudHandler);
            }
            else if (GameStateHandler.IsBattle())
            {
                if (stateHudHandlerList.ContainsKey(GameStateHandler.GameState.BATTLE))
                    RemoveHudHandler();
                stateHudHandlerList.Add(GameStateHandler.GameState.BATTLE, hudHandler);
            }
            return true;
        }

        /// <summary>
        ///     HudHandlerを削除
        /// </summary>
        /// <returns>削除した場合true</returns>
        public bool RemoveHudHandler() {
            NowHudHandler().AllDestroy();
            if (GameStateHandler.IsMap())
                stateHudHandlerList.Remove(GameStateHandler.GameState.MAP);
            else if (GameStateHandler.IsBattle()) stateHudHandlerList.Remove(GameStateHandler.GameState.BATTLE);
            return true;
        }

        /// <summary>
        ///     現在のHudHandlerで描画している全てのオブジェクトを削除する
        /// </summary>
        /// <returns>削除した場合true</returns>
        public bool AllDestroyNowHudHandler() {
            if (NowHudHandler() == null) return false;
            NowHudHandler().AllDestroy();
            return true;
        }

        /// <summary>
        ///     現在のHudHandlerで描画している全てのオブジェクトを削除する
        /// </summary>
        /// <returns>削除した場合true</returns>
        public bool AllDestroyHudHandler() {
            if (stateHudHandlerList.ContainsKey(GameStateHandler.GameState.MAP))
            {
                stateHudHandlerList[GameStateHandler.GameState.MAP]?.AllDestroy();
                stateHudHandlerList?.Remove(GameStateHandler.GameState.MAP);
            }
            if (stateHudHandlerList.ContainsKey(GameStateHandler.GameState.BATTLE))
            {
                stateHudHandlerList[GameStateHandler.GameState.BATTLE]?.AllDestroy();
                stateHudHandlerList?.Remove(GameStateHandler.GameState.BATTLE);
            }
            return true;
        }
    }
}