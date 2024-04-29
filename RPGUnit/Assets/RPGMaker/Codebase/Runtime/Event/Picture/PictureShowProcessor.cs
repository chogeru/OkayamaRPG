using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;

namespace RPGMaker.Codebase.Runtime.Event.Picture
{
    /// <summary>
    /// [ピクチャ]-[ピクチャの表示]
    /// </summary>
    public class PictureShowProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            //画像の番号
            var pictureNumber = int.Parse(command.parameters[0]);
            HudDistributor.Instance.NowHudHandler().PictureInit();
            //画像の表示
            HudDistributor.Instance.NowHudHandler().AddPicture(pictureNumber, command.parameters[1]);

            //アンカー
            HudDistributor.Instance.NowHudHandler().SetAnchor(pictureNumber, int.Parse(command.parameters[2]));

            //座標なのか、変数なのか
            HudDistributor.Instance.NowHudHandler().SetPosition(pictureNumber,
                int.Parse(command.parameters[3]),
                command.parameters[4], command.parameters[5]);

            //幅,高さ
            HudDistributor.Instance.NowHudHandler().SetPictureSize(pictureNumber,
                int.Parse(command.parameters[6]),
                int.Parse(command.parameters[7]));

            //不透明度
            HudDistributor.Instance.NowHudHandler().SetPictureOpacity(pictureNumber, int.Parse(command.parameters[8]));

            //"通常", "加算", "乗算", "スクリーン";
            HudDistributor.Instance.NowHudHandler().SetProcessingType(pictureNumber, int.Parse(command.parameters[9]));

            //セーブデータへの保存用
            HudDistributor.Instance.NowHudHandler().AddPictureParameter(pictureNumber, command.parameters);

            ProcessEndAction();
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}