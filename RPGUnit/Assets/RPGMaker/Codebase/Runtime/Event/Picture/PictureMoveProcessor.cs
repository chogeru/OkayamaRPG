using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;

namespace RPGMaker.Codebase.Runtime.Event.Picture
{
    /// <summary>
    /// [ピクチャ]-[ピクチャの移動]
    /// </summary>
    public class PictureMoveProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            if (int.TryParse(command.parameters[0], out var result) == false ||
                HudDistributor.Instance.NowHudHandler().GetPicture(int.Parse(command.parameters[0])) == null)
            {
                ProcessEndAction();
                return;
            }

            HudDistributor.Instance.NowHudHandler().PictureInit();

            //画像の番号
            var pictureNumber = int.Parse(command.parameters[0]);

            //アンカー
            HudDistributor.Instance.NowHudHandler().SetAnchor(pictureNumber, int.Parse(command.parameters[2]));

            //幅,高さ
            HudDistributor.Instance.NowHudHandler().PlayPictureSize(pictureNumber,
                int.Parse(command.parameters[10]),
                int.Parse(command.parameters[6]),
                int.Parse(command.parameters[7]));

            //不透明度
            HudDistributor.Instance.NowHudHandler().PlayChangeColor(null,
                HudDistributor.Instance.NowHudHandler().GetPicture(int.Parse(command.parameters[0])).color * 255f,
                pictureNumber,
                int.Parse(command.parameters[8]),
                int.Parse(command.parameters[10]),
                false);

            //"通常", "加算", "乗算", "スクリーン";
            HudDistributor.Instance.NowHudHandler().SetProcessingType(pictureNumber, int.Parse(command.parameters[9]));

            //移動
            HudDistributor.Instance.NowHudHandler().PlayMove(
                ProcessEndAction,
                pictureNumber,
                int.Parse(command.parameters[1]),
                int.Parse(command.parameters[3]),
                command.parameters[4], command.parameters[5],
                int.Parse(command.parameters[10]),
                command.parameters[11] == "1");
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}