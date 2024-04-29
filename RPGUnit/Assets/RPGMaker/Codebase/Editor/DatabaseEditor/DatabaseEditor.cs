using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;

namespace RPGMaker.Codebase.Editor.DatabaseEditor
{
    public static class DatabaseEditor
    {
        private static SceneWindow _sceneWindow;

        public static void Init() {
            _sceneWindow =
                WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.DatabaseSceneWindow) as
                    SceneWindow;
        }

        public static SceneWindow GetDatabaseSceneWindow() {
            return _sceneWindow;
        }
    }
}