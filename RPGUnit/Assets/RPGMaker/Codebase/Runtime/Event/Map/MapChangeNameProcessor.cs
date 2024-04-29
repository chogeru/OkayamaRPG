using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.Runtime.Common;

namespace RPGMaker.Codebase.Runtime.Event.Map
{
    /// <summary>
    /// [マップ]-[マップ名表示の変更]
    /// </summary>
    public class MapChangeNameProcessor : AbstractEventCommandProcessor
    {
        private RuntimePlayerDataModel _runtimePlayerDataModel;

        protected override void Process(string eventId, EventDataModel.EventCommand command) {
            //表示切り替え
            DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.nameDisplay =
                int.Parse(command.parameters[0]);
            //マップ名の表示
            HudDistributor.Instance.NowHudHandler().PlayChangeMapName();

            ProcessEndAction();
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}