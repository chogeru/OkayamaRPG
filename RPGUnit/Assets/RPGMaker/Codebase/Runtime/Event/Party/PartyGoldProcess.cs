using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Party;

namespace RPGMaker.Codebase.Runtime.Event.Party
{
    /// <summary>
    /// [パーティ]-[所持金の増減]
    /// </summary>
    public class PartyGoldProcess : AbstractEventCommandProcessor
    {
        private PartyGold _partyGold;

        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            var type = int.Parse(command.parameters[0]);
            var variable = int.Parse(command.parameters[1]);
            var value = command.parameters[2];

            _partyGold = new PartyGold();
            _partyGold.Init(type, variable, value);
            _partyGold.SetPartyGold();
            ProcessEndAction();
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}