using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Character;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Event.Character
{
    /// <summary>
    /// [キャラクター]-[アニメーションの表示]
    /// </summary>
    public class ShowAnimationProcessor : AbstractEventCommandProcessor
    {
        private CharacterAnimation _characterAnimation;
        private GameObject _characterObject;

        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            //0 イベントの番号
            //1 アニメーションの種類
            //2 ウェイト
            CharacterShowAnimation(
                command.parameters[0],
                command.parameters[1],
                command.parameters[2] == "1" ? true : false,
                eventID);
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }

        private void CharacterShowAnimation(string eventId, string animName, bool waitToggle, string currentEventID) {
            _characterObject = new GameObject {name = "AnimationObject"};
            _characterAnimation = _characterObject.AddComponent<CharacterAnimation>();
            _characterAnimation.Init();

            _characterAnimation.PlayAnimation(ProcessEndAction, CloseCharacterShowAnimation, eventId, animName,
                waitToggle, currentEventID);
        }

        private void CloseCharacterShowAnimation() {
            if (_characterAnimation == null) return;
            _characterObject = null;
            _characterAnimation = null;
            ProcessEndAction();
        }
    }
}