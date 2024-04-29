using RPGMaker.Codebase.Runtime.Common;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Map.Menu
{
    public class DebugToolWindow : MonoBehaviour
    {
        [SerializeField] private DebugToolGroupMenu _debugToolGroupMenu = null;

        private void Update() {
            if (Input.GetKeyDown(KeyCode.F9))
            {
                var uiCanvas = transform.GetChild(0).gameObject;
                var active = uiCanvas.activeSelf;
                if (!active)
                {
                    if (!MenuManager.IsShopActive && !MenuManager.IsMenuActive && !MapEventExecutionController.Instance.CheckRunningEvent())
                    {
                        var menuBase = GetMenuBase();
                        if (menuBase != null)
                        {
                            menuBase.MenuClose(false);
                        }
                        //状態の更新
                        MapEventExecutionController.Instance.PauseEvent();
                        GameStateHandler.SetGameState(GameStateHandler.GameState.MENU);
                        MenuManager.IsMenuActive = true;
                        MenuManager.IsShopActive = true;    // disable to open menu.
                        _debugToolGroupMenu.Init(this);
                        _debugToolGroupMenu.gameObject.SetActive(true);
                        uiCanvas.SetActive(true);
                    }
                }
                else
                {
                    BackMenu();
                }
            }
        }

        MenuBase GetMenuBase() {
            var menuBases = transform.parent.GetComponentsInChildren<MenuBase>();
            foreach (var menuBase in menuBases){
                if (menuBase.transform.parent != transform)
                {
                    return menuBase;
                }
            }
            return null;
        }

        public void BackMenu() {
            var uiCanvas = transform.GetChild(0).gameObject;
            var active = uiCanvas.activeSelf;
            if (active)
            {
                //状態の更新
                MapEventExecutionController.Instance.ResumeEvent();
                GameStateHandler.SetGameState(GameStateHandler.GameState.MAP);
                MenuManager.IsMenuActive = false;
                MenuManager.IsShopActive = false;
                var menuBase = GetMenuBase();
                if (menuBase != null)
                {
                    menuBase.MenuClose(true);
                }
                _debugToolGroupMenu.Final();
                uiCanvas.SetActive(false);
            }
        }
    }
}
