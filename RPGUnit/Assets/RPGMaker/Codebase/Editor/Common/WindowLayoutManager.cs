using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common.Window;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Base.View;
using RPGMaker.Codebase.Editor.Inspector;
using RPGMaker.Codebase.Editor.MapEditor.Window.EventEdit;
using RPGMaker.Codebase.Editor.MapEditor.Window.MapEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RPGMaker.Codebase.Editor.Common
{
    public static class WindowLayoutManager
    {
        public enum WindowLayoutId
        {
            None                    = 0,
            MenuWindow              = 1,
            DatabaseHierarchyWindow = 2,

            MapSceneWindow = 11,
            MapInspectorWindow,
            MapEditWindow,
            MapBattleEditWindow,
            MapTileEditWindow,
            MapTileGroupEditWindow,
            MapEventEditWindow,
            MapEventCommandSettingWindow,       // 『イベントコマンド』枠 (設定)。┐名前   CommandSettingWindowクラス。
            MapEventCommandWindow,              // 『イベントコマンド』枠 (選択)。┘重複   CommandWindowクラス。 ※MZではモーダルウィンドウ。
            MapEventExecutionContentsWindow,    // 『イベント実行内容』枠。                ExecutionContentsWindowクラス。
            MapEventMoveCommandWindow,
            MapEventRouteWindow,

            OutlineSceneWindow = 51,
            OutlineInspectorWindow,

            DatabaseSceneWindow = 101,
            DatabaseInspectorWindow,
            DatabaseTestWindow = 104,

            GameView = 201
        }

        public enum WindowLayoutPattern
        {
            SceneTop = 0,
            SceneBottom,
            InspectorTop,
            InspectorBottom
        }

        // enum
        //-------------------------------------------------------------------------
        public enum WindowLayoutType
        {
            Database = 0,
            Map,
            Outline,
            Popup
        }

        // layout IDごとのウィンドウオープンメソッド
        // （IDごとにOpenWindow<T>のTをマッピングしたいのだが、T単体ではできないので、やむなくメソッドごと）
        private static readonly Dictionary<WindowLayoutId, Action<Type>> WindowOpenMethods = new()
            {
                //menu
                {
                    WindowLayoutId.MenuWindow,
                    dockTo => { OpenWindow<MenuWindow>(WindowLayoutId.MenuWindow, dockTo); }
                },
                // map editor
                {
                    WindowLayoutId.MapEditWindow,
                    dockTo => { OpenWindow<MapEditWindow>(WindowLayoutId.MapEditWindow, dockTo); }
                },
                {
                    WindowLayoutId.MapBattleEditWindow,
                    dockTo => { OpenWindow<BattleEditWindow>(WindowLayoutId.MapBattleEditWindow, dockTo); }
                },
                {
                    WindowLayoutId.MapTileEditWindow,
                    dockTo => { OpenWindow<TileEditWindow>(WindowLayoutId.MapTileEditWindow, dockTo); }
                },
                {
                    WindowLayoutId.MapTileGroupEditWindow,
                    dockTo => { OpenWindow<TileGroupEditWindow>(WindowLayoutId.MapTileGroupEditWindow, dockTo); }
                },
                {
                    WindowLayoutId.MapEventEditWindow,
                    dockTo => { OpenWindow<EventEditWindow>(WindowLayoutId.MapEventEditWindow, dockTo); }
                },
                {
                    WindowLayoutId.MapInspectorWindow,
                    dockTo =>
                    {
                        //OpenWindow<MapEditor.Window.InspectorWindow>(WindowLayoutId.MapInspectorWindow, dockTo);
                    }
                },
                {
                    WindowLayoutId.MapEventCommandSettingWindow,
                    dockTo => { OpenWindow<CommandSettingWindow>(WindowLayoutId.MapEventCommandSettingWindow, dockTo); }
                },
                {
                    WindowLayoutId.MapEventCommandWindow,
                    dockTo => { OpenWindow<CommandWindow>(WindowLayoutId.MapEventCommandWindow, dockTo); }
                },
                {
                    WindowLayoutId.MapEventExecutionContentsWindow,
                    dockTo =>
                    {
                        OpenWindow<ExecutionContentsWindow>(WindowLayoutId.MapEventExecutionContentsWindow, dockTo);
                    }
                },

                // database editor
                {
                    WindowLayoutId.DatabaseHierarchyWindow,
                    dockTo => { OpenWindow<HierarchyWindow>(WindowLayoutId.DatabaseHierarchyWindow, dockTo); }
                },
                {
                    WindowLayoutId.DatabaseSceneWindow,
                    //(dockTo) => { OpenWindow<AnimationView>(WindowLayoutId.DatabaseSceneWindow, dockTo); }
                    dockTo => { OpenWindow<SceneWindow>(WindowLayoutId.DatabaseSceneWindow, dockTo); }
                },
                {
                    WindowLayoutId.DatabaseInspectorWindow,
                    dockTo => { OpenWindow<InspectorWindow>(WindowLayoutId.DatabaseInspectorWindow, dockTo); }
                },
                //{
                //    WindowLayoutId.DatabaseAnimationWindow,
                //    dockTo => { OpenWindow<AnimationPreview>(WindowLayoutId.DatabaseAnimationWindow, dockTo); }
                //},

                // outline editor
                {
                    WindowLayoutId.OutlineSceneWindow,
                    dockTo =>
                    {
                        OpenWindow<OutlineEditor.Window.SceneWindow>(WindowLayoutId.OutlineSceneWindow, dockTo);
                    }
                },
                {
                    WindowLayoutId.OutlineInspectorWindow,
                    dockTo =>
                    {
                        //OpenWindow<OutlineEditor.Window.InspectorWindow>(WindowLayoutId.OutlineInspectorWindow,dockTo);
                    }
                },
                {
                    WindowLayoutId.GameView,
                    dockTo =>
                    {
                        var assembly = typeof(EditorWindow).Assembly;
                        var type = assembly.GetType("UnityEditor.GameView");
                        var gameView = EditorWindow.GetWindow(type);
                        gameView.Show();
                    }
                }
            };

        // property
        //-------------------------------------------------------------------------
        // 現在開かれているwindow群
        private static readonly Dictionary<WindowLayoutId, EditorWindow> CurrentActiveWindows = new();

        // 現在のレイアウトタイプ
        private static WindowLayoutType _currentWindowLayoutType;

        // method
        //-------------------------------------------------------------------------

        /// <summary>
        ///     機能単位でWindow（群）を開き、レイアウトする
        /// </summary>
        /// <param name="type"></param>
        public static void SetWindowsByLayoutType(WindowLayoutType type) {
            switch (type)
            {
                case WindowLayoutType.Database:
                    switch (_currentWindowLayoutType)
                    {
                        case WindowLayoutType.Database:
                            // do nothing
                            break;
                        case WindowLayoutType.Map:
                            WindowOpenMethods[WindowLayoutId.DatabaseSceneWindow]
                                .Invoke(CurrentActiveWindows[WindowLayoutId.MapEditWindow].GetType());
                            WindowOpenMethods[WindowLayoutId.DatabaseInspectorWindow]
                                .Invoke(CurrentActiveWindows[WindowLayoutId.MapInspectorWindow].GetType());
                            CloseWindow(WindowLayoutId.MapEditWindow);
                            CloseWindow(WindowLayoutId.MapInspectorWindow);
                            break;
                        case WindowLayoutType.Outline:
                            WindowOpenMethods[WindowLayoutId.DatabaseSceneWindow]
                                .Invoke(CurrentActiveWindows[WindowLayoutId.OutlineSceneWindow].GetType());
                            WindowOpenMethods[WindowLayoutId.DatabaseInspectorWindow]
                                .Invoke(CurrentActiveWindows[WindowLayoutId.OutlineInspectorWindow].GetType());
                            CloseWindow(WindowLayoutId.OutlineSceneWindow);
                            CloseWindow(WindowLayoutId.OutlineInspectorWindow);
                            break;
                        case WindowLayoutType.Popup:
                            break;
                        default:
                            throw new Exception();
                    }

                    break;
                case WindowLayoutType.Map:
                    switch (_currentWindowLayoutType)
                    {
                        case WindowLayoutType.Database:
                            WindowOpenMethods[WindowLayoutId.MapEditWindow]
                                .Invoke(CurrentActiveWindows[WindowLayoutId.DatabaseSceneWindow].GetType());
                            WindowOpenMethods[WindowLayoutId.MapInspectorWindow]
                                .Invoke(CurrentActiveWindows[WindowLayoutId.DatabaseInspectorWindow].GetType());
                            CloseWindow(WindowLayoutId.DatabaseSceneWindow);
                            CloseWindow(WindowLayoutId.DatabaseInspectorWindow);
                            break;
                        case WindowLayoutType.Map:
                            // do nothing
                            break;
                        case WindowLayoutType.Outline:
                            WindowOpenMethods[WindowLayoutId.MapEditWindow]
                                .Invoke(CurrentActiveWindows[WindowLayoutId.OutlineSceneWindow].GetType());
                            WindowOpenMethods[WindowLayoutId.MapInspectorWindow]
                                .Invoke(CurrentActiveWindows[WindowLayoutId.OutlineInspectorWindow].GetType());
                            CloseWindow(WindowLayoutId.OutlineSceneWindow);
                            CloseWindow(WindowLayoutId.OutlineInspectorWindow);
                            break;
                        case WindowLayoutType.Popup:
                            break;
                        default:
                            throw new Exception();
                    }

                    break;
                case WindowLayoutType.Outline:
                    switch (_currentWindowLayoutType)
                    {
                        case WindowLayoutType.Database:
                            WindowOpenMethods[WindowLayoutId.OutlineSceneWindow]
                                .Invoke(CurrentActiveWindows[WindowLayoutId.DatabaseSceneWindow].GetType());
                            WindowOpenMethods[WindowLayoutId.OutlineInspectorWindow]
                                .Invoke(CurrentActiveWindows[WindowLayoutId.DatabaseInspectorWindow].GetType());
                            CloseWindow(WindowLayoutId.DatabaseSceneWindow);
                            CloseWindow(WindowLayoutId.DatabaseInspectorWindow);
                            break;
                        case WindowLayoutType.Map:
                            WindowOpenMethods[WindowLayoutId.OutlineSceneWindow]
                                .Invoke(CurrentActiveWindows[WindowLayoutId.MapEditWindow].GetType());
                            WindowOpenMethods[WindowLayoutId.OutlineInspectorWindow]
                                .Invoke(CurrentActiveWindows[WindowLayoutId.MapInspectorWindow].GetType());
                            CloseWindow(WindowLayoutId.MapEditWindow);
                            CloseWindow(WindowLayoutId.MapInspectorWindow);
                            break;
                        case WindowLayoutType.Outline:
                            // do nothing
                            break;
                        case WindowLayoutType.Popup:
                            break;
                        default:
                            throw new Exception();
                    }

                    break;
                case WindowLayoutType.Popup:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            _currentWindowLayoutType = type;
        }

        /// <summary>
        ///     ウィンドウを開き、ドッキングする
        /// </summary>
        public static EditorWindow OpenAndDockWindow(
            WindowLayoutId windowIdToOpen,
            WindowLayoutId windowIdToDock,
            Docker.DockPosition dockPosition
        ) {
            if (windowIdToOpen == windowIdToDock) return CurrentActiveWindows[windowIdToOpen];

            WindowOpenMethods[windowIdToOpen].Invoke(null);
            WindowOpenMethods[windowIdToDock].Invoke(null);
            var windowToOpen = CurrentActiveWindows[windowIdToOpen];
            var windowToDock = CurrentActiveWindows[windowIdToDock];

            Docker.DockTo(windowToDock, windowToOpen, dockPosition);
            return CurrentActiveWindows[windowIdToOpen];
        }

        /**
         * layoutIDからウィンドウを取得（開いてなければ開く）
         */
        public static EditorWindow GetOrOpenWindow(
            WindowLayoutId targetWindowLayoutId,
            [CanBeNull] Type typeOfDesiredDockNextTo = null
        ) {
            WindowOpenMethods[targetWindowLayoutId].Invoke(typeOfDesiredDockNextTo);
            return GetActiveWindow(targetWindowLayoutId);
        }

        /**
         * ウィンドウを入れ替える
         */
        public static EditorWindow SwitchWindows(WindowLayoutId newWindowLayoutId, WindowLayoutId oldWindowLayoutId) {
            if (newWindowLayoutId == oldWindowLayoutId) return GetActiveWindow(oldWindowLayoutId);

            if (CurrentActiveWindows.TryGetValue(oldWindowLayoutId, out EditorWindow oldWindow))
            {
                WindowOpenMethods[newWindowLayoutId].Invoke(oldWindow.GetType());
                CloseWindow(oldWindowLayoutId);
                return GetActiveWindow(newWindowLayoutId);
            }
            else
            {
                return GetOrOpenWindow(newWindowLayoutId);
            }
        }

        /**
         * ウィンドウを開く
         */
        private static void OpenWindow<T>(
            WindowLayoutId targetWindowLayoutId,
            [CanBeNull] Type typeOfDesiredDockNextTo = null
        )
            where T : EditorWindow
        {
            var window = EditorWindow.GetWindow<T>(desiredDockNextTo: typeOfDesiredDockNextTo);
            if (!CurrentActiveWindows.ContainsKey(targetWindowLayoutId))
            {
                CurrentActiveWindows.Add(targetWindowLayoutId, window);
            }
            else if (CurrentActiveWindows[targetWindowLayoutId] == null)
            {
                CurrentActiveWindows.Remove(targetWindowLayoutId);
                CurrentActiveWindows.Add(targetWindowLayoutId, window);
            }
        }

        /**
         * ウィンドウを閉じる
         */
        public static void CloseWindow(WindowLayoutId targetWindowLayoutId) {
            if (!CurrentActiveWindows.ContainsKey(targetWindowLayoutId)) return;
            CurrentActiveWindows[targetWindowLayoutId].Close();
            CurrentActiveWindows.Remove(targetWindowLayoutId);
        }

        /**
         * ウィンドウ（複数）を閉じる
         */
        public static void CloseWindows(List<WindowLayoutId> targetWindowLayoutIds) {
            targetWindowLayoutIds?.ForEach(CloseWindow);
        }

        /**
         * 指定したIDのウィンドウが現在開かれているか
         */
        public static bool IsActiveWindow(WindowLayoutId layoutId) {
            return CurrentActiveWindows.ContainsKey(layoutId);
        }

        /**
         * 指定したIDのウィンドウを取得する
         */
        public static EditorWindow GetActiveWindow(WindowLayoutId layoutId) {
            return CurrentActiveWindows.ContainsKey(layoutId) ? CurrentActiveWindows[layoutId] : null;
        }

        /*
         * AssetManagePreviewのSceneWindowがある？
         */
        public static bool HasAssetManagePreviewSceneWindow() {
            var sceneWindow = (SceneWindow) GetActiveWindow(WindowLayoutId.DatabaseSceneWindow);
            return sceneWindow.GetManagePreview() != null;
        }

        /*
         * イベント実行内容枠で、アニメーションの表示イベントが選択されている？
         */
        public static bool IsCharacterShowAnimationEventSelecting() {
            var currentSelectingCommand =
                (GetActiveWindow(WindowLayoutId.MapEventExecutionContentsWindow) as ExecutionContentsWindow)
                ?.GetCurrentSelectingCommand();
            return
                currentSelectingCommand != null &&
                (EventEnum) currentSelectingCommand.code == EventEnum.EVENT_CODE_CHARACTER_SHOW_ANIMATION;
        }

        /*
         * イベント関連のウィンドウを閉じる。
         */
        public static void CloseEventSubWindows() {
            CloseWindows(new List<WindowLayoutId>()
            {
                WindowLayoutId.MapEventExecutionContentsWindow,
                WindowLayoutId.MapEventRouteWindow,
                WindowLayoutId.MapEventCommandSettingWindow,
                WindowLayoutId.MapEventMoveCommandWindow
            });
        }

        /*
         * Unityのリソースから、既に開かれている任意の型のウィンドウのインスタンスを取得。
         */
        public static EditorWindow GetWindowFromResources<T>() where T : EditorWindow
        {
            var editorWindows = Resources.FindObjectsOfTypeAll<T>();
            return editorWindows.SingleOrDefault();
        }
    }
}