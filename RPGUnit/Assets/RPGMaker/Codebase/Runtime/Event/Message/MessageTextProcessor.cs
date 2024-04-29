using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;

namespace RPGMaker.Codebase.Runtime.Event.Message
{
    /// <summary>
    /// [メッセージ]-[文章の表示]
    /// </summary>
    public class MessageTextProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            //アクターIDで変動するもの
            var name = command.parameters[6];
            var icon = command.parameters[7];
            var picture = command.parameters[8];

            //アクターIDが指定されている場合
            if (command.parameters.Count >= 10)
                if (command.parameters[9] != "")
                {
                    bool flg = false;
                    var runtimeActorDataModels = DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels;
                    for (var i = 0; i < runtimeActorDataModels.Count; i++)
                    {
                        if (runtimeActorDataModels[i].actorId == command.parameters[9])
                        {
                            name = runtimeActorDataModels[i].name;
                            icon = runtimeActorDataModels[i].faceImage;
                            picture = runtimeActorDataModels[i].advImage;
                            flg = true;
                            break;
                        }
                    }
                    if (!flg)
                    {
                        //アクターIDが指定されているが、RuntimeActorDataModelには存在しない
                        //このケースでは、マスタデータから検索する
                        CharacterActorDataModel data = DataManager.Self().GetActorDataModel(command.parameters[9]);
                        if (data != null)
                        {
                            name = data.basic.name;
                            icon = data.image.face;
                            picture = data.image.adv;
                        }
                    }
                }

            if (!HudDistributor.Instance.NowHudHandler().IsMessageWindowActive())
            {
                HudDistributor.Instance.NowHudHandler().OpenMessageWindow();
            }

            //名前表示
            if (command.parameters[0] != "0")
            {
                HudDistributor.Instance.NowHudHandler().ShowName(name);
            }

            //顔アイコン表示
            if (command.parameters[1] != "0")
            {
                if (icon != "" && icon != "0")
                    HudDistributor.Instance.NowHudHandler().ShowFaceIcon(icon);
            }

            //ピクチャ表示
            if (command.parameters[2] != "0")
            {
                if (command.parameters[2] != "" && command.parameters[2] != "0")
                    if (command.parameters[2] == "1")
                        HudDistributor.Instance.NowHudHandler().ShowPicture(picture);
                    else
                        HudDistributor.Instance.NowHudHandler().ShowPicture(command.parameters[2]);
            }

            //ウィンドウのカラー変更
            if (int.TryParse(command.parameters[3], out var n))
                HudDistributor.Instance.NowHudHandler().SetMessageWindowColor(int.Parse(command.parameters[3]));
            //座標
            HudDistributor.Instance.NowHudHandler().SetMessageWindowPos(int.Parse(command.parameters[4]));

            // ここで入力待ちする必要はないのでそのまま流す
            ProcessEndAction();
        }

        private void ProcessEndAction() {
            //次のシーケンス実行にWaitを挟まない
            _skipWait = true;
            SendBackToLauncher.Invoke();
        }
    }
}