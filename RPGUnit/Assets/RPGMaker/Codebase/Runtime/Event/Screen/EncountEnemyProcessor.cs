using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Encounter;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Runtime.Battle;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map;
using System.Threading.Tasks;

namespace RPGMaker.Codebase.Runtime.Event.Screen
{
    /// <summary>
    /// [シーン制御]-[戦闘の処理]
    /// </summary>
    public class EncountEnemyProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            //先頭のキャラクターのタイル情報を設定
            MapManager.CurrentTileData(MapManager.OperatingCharacter.GetCurrentPositionOnTile());
            //エンカウントデータ取得
            var databaseManagementService = new DatabaseManagementService();
            //現在のマップの情報を取得
            var encount = MapManager.encountManager.GetEncounterDataModel();
            var troopId = "EVENTDATA";
            if (encount == null)
            {
                //無い場合は新規に作成する
                encount = EncounterDataModel.CreateDefault("EVENTDATA");
                encount.mapId = MapManager.CurrentMapDataModel.id;
                encount.name = "EVENTDATA";
                //背景設定は無し
                encount.backImage1 = "";
                encount.backImage2 = "";
            }

            if (!BattleManager.IsBattle)
                //エネミーエンカウントの導入
                switch (command.parameters[0])
                {
                    //直接
                    case "0":
                        troopId = command.parameters[1];
                        break;
                    //変数
                    case "1":
                        //Variablesのマスタデータから、対象の変数を取得
                        var variables = DataManager.Self().GetFlags().variables;
                        var value = variables.FindIndex(x => x.id == command.parameters[1]);

                        //対象の変数に、現在設定されている値を取得
                        var variablesNum = int.Parse(DataManager.Self().GetRuntimeSaveDataModel().variables.data[value]);

                        //敵グループから、該当のSerialNumberを検索して設定する
                        var troops = DataManager.Self().GetTroopDataModels();
                        var troopIndex = troops.FindIndex(x => x.SerialNumber == variablesNum);

                        //敵グループが存在した場合はバトルへ移動
                        if (troopIndex >= 0 && troops.Count > troopIndex)
                        {
                            troopId = troops[troopIndex].id;
                        }
                        //敵グループが存在しなければ飛ばす
                        else
                        {
                            ProcessEndAction();
                            return;
                        }
                        break;
                    //ランダム
                    case "2":
                        break;
                }

            //troopId が "EVENTDATA"の場合には、マップ設定に従ってバトルに突入するが、
            //エンカウント設定（敵グループ）が存在しない場合には突入できないため、処理を行わずに後続の処理を行う
            if (troopId == "EVENTDATA" && (encount == null || encount.troopList == null || encount.troopList.Count == 0))
            {
                ProcessEndAction();
                return;
            }


            //逃走可能、敗北可能のフラグを設定
            bool canEscape;
            bool canLose;
            if (int.Parse(command.parameters[2]) == 1)
                //逃走可能
                canEscape = true;
            else
                canEscape = false;
            if (int.Parse(command.parameters[3]) == 1)
                //敗北可能
                canLose = true;
            else
                canLose = false;

            //一時的にイベントを中断し、バトルへ遷移する
            MapEventExecutionController.Instance.PauseEvent(ProcessEndAction);
            MapManager.StartBattle(encount, troopId, canEscape, canLose);
        }
        private async void ProcessEndAction() {
            //若干の待ち時間を設けて、後続の処理を行う
            await Task.Delay(1);
            SendBackToLauncher.Invoke();
        }
    }
}