using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Common.Window.ModalWindow;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RPGMaker.Codebase.Editor.Common.Window
{
    public class MenuWindowParams : ScriptableSingleton<MenuWindowParams>
    {
        public bool IsRpgEditorPlayMode;
    }

    /// <summary>
    ///     メニュー用のWindow
    /// </summary>
    public class MenuWindow : BaseWindow
    {
        public enum BtnType
        {
            New = 1,
            Open,
            Save,
            Cat,
            Paste,
            Map,
            Back,
            Event,

            Pen = 15,
            Rectangle,
            Ellipse,
            Fill,
            Shadow,
            ZoomIn,
            ZoomOut,
            ActualSize,

            Addon = 24,
            SoundTest,
            EventSearch,
            Material,

            Play = 29,
            Close,
            Stop = 31,

            Deploy,
            App,

            //Play,
            Store,
            Package,

            // 消しゴム
            Eraser = 33,
        }

        private MenuEditorView _menuEditorView;

        public void Init() {
            //表示されていたら、UniteのイベントのWindowを閉じる
            WindowLayoutManager.CloseWindow(WindowLayoutManager.WindowLayoutId.MapEventExecutionContentsWindow);
            WindowLayoutManager.CloseWindow(WindowLayoutManager.WindowLayoutId.MapEventCommandSettingWindow);

            rootVisualElement.Clear();
            var size = new Vector2(1200f, 37f);
            maxSize = size;
            minSize = size;
            wantsMouseMove = false;

            var root = rootVisualElement;
            _menuEditorView = new MenuEditorView(this);
            root.Add(_menuEditorView);
        }

        public void Select(int btnIndex) {
            switch ((BtnType) btnIndex)
            {
                case BtnType.New:
                    ProjectNew();
                    break;
                case BtnType.Open:
                    ProjectOpen();
                    break;
                case BtnType.Close:
                    ProjectClose();
                    break;
                case BtnType.Save:
                    ProjectSave();
                    break;
                case BtnType.Deploy:
                    ProjectDeploy();
                    break;
                case BtnType.App:
                case BtnType.Pen:
                    ProjectApp();
                    break;
                case BtnType.Play:
                    TestPlay();
                    break;
                case BtnType.Stop:
                    TestPlay();
                    break;
                case BtnType.Store:
                    AssetStoreOpen();
                    break;
                case BtnType.Package:
                    PackageManagerOpen();
                    break;

                case BtnType.Addon:
                    ShowAddonList();
                    break;
            }
        }

        /// <summary>
        /// ボタンの状態更新
        /// </summary>
        /// <param name="flg"></param>
        public void SetButtonEnabled(bool flg) {
            var playButton = _menuEditorView.GetIconImage((int)BtnType.Play);
            var penButton = _menuEditorView.GetIconImage((int)BtnType.Pen);
            playButton?.SetEnabled(flg);
            penButton?.SetEnabled(flg);
        }
        

        /// <summary>
        ///     新規プロジェクト
        /// </summary>
        private void ProjectNew() {
            var menuWindowTemplateModalWindow = ScriptableObject.CreateInstance<InitialProjectAssetImportWindow>();
            menuWindowTemplateModalWindow.ShowWindow();

            AnalyticsManager.Instance.PostEvent(
                AnalyticsManager.EventName.action,
                AnalyticsManager.EventParameter.new_);
        }

        /// <summary>
        ///     プロジェクトを開く
        /// </summary>
        private void ProjectOpen() {
            EditorApplication.ExecuteMenuItem("File/Open Project...");

            AnalyticsManager.Instance.PostEvent(
                AnalyticsManager.EventName.action,
                AnalyticsManager.EventParameter.open);
        }

        /// <summary>
        ///     プロジェクトを閉じる
        /// </summary>
        private void ProjectClose() {
#if UNITY_EDITOR_WIN
            EditorApplication.ExecuteMenuItem("File/Exit");
#else
            EditorApplication.Exit(0);
#endif
            AnalyticsManager.Instance.PostEvent(
                AnalyticsManager.EventName.action,
                AnalyticsManager.EventParameter.close);
        }

        /// <summary>
        ///     プロジェクトの保存
        /// </summary>
        private void ProjectSave() {
            EditorApplication.ExecuteMenuItem("File/Save");

            AnalyticsManager.Instance.PostEvent(
                AnalyticsManager.EventName.action,
                AnalyticsManager.EventParameter.save);
        }

        /// <summary>
        ///     プロジェクトのデプロイ
        /// </summary>
        private void ProjectDeploy() {
            EditorApplication.ExecuteMenuItem("File/Build Settings...");
        }

        /// <summary>
        ///     ゲームをアプリ化
        /// </summary>
        private void ProjectApp() {
            //現在の設定値をBuildSettingsに反映
            var databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;
            var systemSettingDataModel = databaseManagementService.LoadSystem();

            var screenWidth = systemSettingDataModel.DisplaySize[systemSettingDataModel.displaySize].x;
            var screenHeight = systemSettingDataModel.DisplaySize[systemSettingDataModel.displaySize].y;

            PlayerSettings.defaultScreenWidth = screenWidth;
            PlayerSettings.defaultScreenHeight = screenHeight;
            PlayerSettings.defaultWebScreenWidth = screenWidth;
            PlayerSettings.defaultWebScreenHeight = screenHeight;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.allowFullscreenSwitch = false;

            //今はBuild Settingsを開く
            EditorApplication.ExecuteMenuItem("File/Build Settings...");

            AnalyticsManager.Instance.PostEvent(
                AnalyticsManager.EventName.action,
                AnalyticsManager.EventParameter.deploy);
        }

        /// <summary>
        ///     アドオンリストの呼び出し
        /// </summary>
        private void ShowAddonList() {
            var addonListModalWindow = new AddonListModalWindow();
            addonListModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Add-on List"), data => { });
        }

        private void TestPlay() {
            if (!EditorApplication.isPlaying)
            {
                if (RpgMakerEditor.IsImportEffekseer)
                {
                    EditorUtility.RequestScriptReload();
                    RpgMakerEditor.IsImportEffekseer = false;
                }

                //ヒエラルキーのイベントを選択していないとき
                WindowLayoutManager.CloseWindows(new List<WindowLayoutManager.WindowLayoutId>()
                {
                    WindowLayoutManager.WindowLayoutId.MapEventExecutionContentsWindow,
                    WindowLayoutManager.WindowLayoutId.MapEventRouteWindow,
                    WindowLayoutManager.WindowLayoutId.MapEventCommandSettingWindow,
                    WindowLayoutManager.WindowLayoutId.MapEventMoveCommandWindow
                });

                EditorSceneManager.OpenScene("Assets/RPGMaker/Codebase/Runtime/Title/Title.unity");
                EditorApplication.isPlaying = true;
                MenuWindowParams.instance.IsRpgEditorPlayMode = true;
            }
            else
            {
                EditorApplication.isPlaying = false;
                MenuWindowParams.instance.IsRpgEditorPlayMode = false;
            }

            AnalyticsManager.Instance.PostEvent(
                AnalyticsManager.EventName.action,
                AnalyticsManager.EventParameter.testplay);
        }

        private void TestStop() {
        }

        /// <summary>
        ///     AssetStoreを開く
        /// </summary>
        private void AssetStoreOpen() {
            EditorApplication.ExecuteMenuItem("Window/Asset Store");
        }

        /// <summary>
        ///     PackageManagerを開く
        /// </summary>
        private void PackageManagerOpen() {
            EditorApplication.ExecuteMenuItem("Window/Package Manager");
        }

        private void CreateGUI()
        {
            if (float.IsNaN(rootVisualElement.layout.width) ||
                float.IsNaN(rootVisualElement.layout.height) ||
                rootVisualElement.childCount == 0)
                RpgMakerEditor.WindowMaximizationRecoveringProcess();
        }
    }
}