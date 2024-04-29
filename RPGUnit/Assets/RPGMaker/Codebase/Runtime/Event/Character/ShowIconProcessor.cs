using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Character;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Event.Character
{
    /// <summary>
    /// [キャラクター]-[フキダシアイコンの表示]
    /// </summary>
    public class ShowIconProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            //0 イベントの番号
            //1 フキダシアイコンアセットid
            //2 ウェイト

            CharacterShowIcon(
                command.parameters[0],
                command.parameters[1],
                command.parameters[2] == "1" ? true : false,
                eventID);
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }

        private void CharacterShowIcon(
            string eventId,
            string popupIconAssetId,
            bool waitToggle,
            string currentEventID
        ) {
            ShowIcon showIcon = new GameObject("ShowIcon").AddComponent<ShowIcon>();
            showIcon.Init();
            showIcon.PlayAnimation(ProcessEndAction, CloseCharacterShowIcon, eventId, popupIconAssetId, waitToggle,
                currentEventID);
        }

        private void CloseCharacterShowIcon(ShowIcon showIcon) {
            Object.Destroy(showIcon.gameObject);
        }
    }
}