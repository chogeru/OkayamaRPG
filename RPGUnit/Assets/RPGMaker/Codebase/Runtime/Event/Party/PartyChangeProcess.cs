using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Party;

namespace RPGMaker.Codebase.Runtime.Event.Party
{
    /// <summary>
    /// [パーティ]-[メンバーの入れ替え]
    /// </summary>
    public class PartyChangeProcess : AbstractEventCommandProcessor
    {
        private PartyChange _partyChange;

        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            var actorID = command.parameters[0];
            var isAdd = command.parameters[1] == "1";
            var isInit = command.parameters[2] == "1";

            _partyChange = new PartyChange();
            _partyChange.Init(actorID, isAdd, isInit);
            _partyChange.SetPartyChange();
            ProcessEndAction();
        }

        private void ProcessEndAction() {
            _partyChange = null;
            SendBackToLauncher.Invoke();
        }
    }
}