using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Party;

namespace RPGMaker.Codebase.Runtime.Event.Party
{
    /// <summary>
    /// [パーティ]-[アイテムの増減]
    /// </summary>
    public class PartyItemProcess : AbstractEventCommandProcessor
    {
        private PartyItem _partyItem;

        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            var itemId = command.parameters[0];
            var type = int.Parse(command.parameters[1]);
            var variable = int.Parse(command.parameters[2]);
            var value = 0;
            //定数、変数の分岐
            switch (command.parameters[2])
            {
                case "0":
                    value = int.Parse(command.parameters[3]);
                    break;
                case "1":
                    var flagDataModel = DataManager.Self().GetFlags();
                    for (var i = 0; i < flagDataModel.variables.Count; i++)
                    {
                        if (flagDataModel.variables[i].id == command.parameters[3])
                        {
                            value = i;
                            break;
                        }
                    }
                    break;
            }
            _partyItem = new PartyItem();
            _partyItem.Init(itemId, type, variable, value);
            _partyItem.SetPartyItems();
            ProcessEndAction();
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}