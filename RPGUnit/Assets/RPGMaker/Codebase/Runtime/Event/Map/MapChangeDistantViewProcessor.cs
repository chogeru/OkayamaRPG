using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Map;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Event.Map
{
    /// <summary>
    /// [マップ]-[遠景の変更]
    /// </summary>
    public class MapChangeDistantView : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            var parallax = MapManager.CurrentMapDataModel.Parallax;
            // ファイル名
            if (!string.IsNullOrEmpty(command.parameters[0]))
                parallax.name = command.parameters[0];

            // 横方向のスクロールについて設定
            if (!string.IsNullOrEmpty(command.parameters[1]))
            {
                if (command.parameters[1] == "0")
                {
                    parallax.loopX = false;
                    parallax.sx = 0;
                }
                else if (command.parameters[1] == "1" && !string.IsNullOrEmpty(command.parameters[3]))
                {
                    parallax.loopX = true;
                    parallax.sx = int.Parse(command.parameters[3]);
                }
            }

            // 縦方向のスクロールについて設定
            if (!string.IsNullOrEmpty(command.parameters[2]))
            {
                if (command.parameters[2] == "0")
                {
                    parallax.loopY = false;
                    parallax.sy = 0;
                }
                else if (command.parameters[2] == "1" && !string.IsNullOrEmpty(command.parameters[4]))
                {
                    parallax.loopY = true;
                    parallax.sy = int.Parse(command.parameters[4]);
                }
            }

            // コマンドの設定を背景に反映
            var distantViewManager = Object.FindObjectOfType(typeof(DistantViewManager)) as DistantViewManager;
            if (distantViewManager == null) return;
            distantViewManager.SetData(parallax);

            ProcessEndAction();
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}