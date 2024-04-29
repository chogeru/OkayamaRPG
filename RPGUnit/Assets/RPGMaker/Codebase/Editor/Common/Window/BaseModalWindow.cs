using System.IO;
using UnityEditor;
using UnityEngine;

namespace RPGMaker.Codebase.Editor.Common.Window
{
    public class BaseModalWindow : EditorWindow
    {
        public delegate void CallBackWidow(object data);
        protected CallBackWidow _callBackWindow;

        protected virtual string ModalUxml { get; }
        protected virtual string ModalUss { get; }

        public virtual void ShowWindow(string modalTitle, CallBackWidow callBack) {
            var wnd = GetWindow<BaseModalWindow>();

            if (callBack != null) _callBackWindow = callBack;

            wnd.titleContent = new GUIContent(modalTitle);
            wnd.ShowModal();
            wnd.Init();
        }

        public virtual void Init() {
        }

        protected void MoveDirectory(string source, string destination) {
            // 移動元のディレクトリとその中身を取得
            DirectoryInfo sourceDirectory = new DirectoryInfo(source);

            // 移動先のディレクトリが存在しない場合は作成
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            // 移動元のディレクトリ内のファイルをすべて移動
            foreach (FileInfo file in sourceDirectory.GetFiles())
            {
                string tempPath = Path.Combine(destination, file.Name);
                file.MoveTo(tempPath);
            }

            // サブディレクトリの移動（再帰呼び出し）
            foreach (DirectoryInfo subDirectory in sourceDirectory.GetDirectories())
            {
                string tempPath = Path.Combine(destination, subDirectory.Name);
                MoveDirectory(subDirectory.FullName, tempPath);
            }

            // 移動元のディレクトリを削除
            sourceDirectory.Delete(true);
        }
    }
}