using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Map;
using RPGMaker.Codebase.Runtime.Map.Component.Character;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Event.Character
{
    /// <summary>
    /// [キャラクター]-[アニメーション指定]
    /// </summary>
    public class AnimationSettingsProcessor : AbstractEventCommandProcessor
    {
        private CharacterGraphic _characterGraphic;
        private CharacterOnMap   _characterOnMap;

        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            //対象のオブジェクト
            GameObject targetObj;
            //歩行用のオブジェクト
            //CharacterGraphic _characterGraphic;

            eventID = (command.parameters[0] == "-1") ? eventID : command.parameters[0];
            //-2:プレイヤー
            //-1：このイベント
            //その他：指定のイベントID
            if (eventID == "-2")
            {
                targetObj = MapManager.GetOperatingCharacterGameObject();
            }
            else
            {
                targetObj = MapEventExecutionController.Instance.GetEventMapGameObject(eventID);
            }

            if (targetObj.GetComponent<CharacterOnMap>() == null)
            {
                _characterOnMap = targetObj.AddComponent<CharacterOnMap>();
            }
            else
            {
                _characterOnMap = targetObj.GetComponent<CharacterOnMap>();
            }

            switch (command.parameters[1])
            {
                case "0":
                    _characterOnMap.SetAnimationSettings(true, _characterOnMap.GetStepAnimation());
                    break;
                case "1":
                    _characterOnMap.SetAnimationSettings(false, _characterOnMap.GetStepAnimation());
                    break;
                case "2":
                    _characterOnMap.SetAnimationSettings(_characterOnMap.GetAnimation(), true);
                    break;
                case "3":
                    _characterOnMap.SetAnimationSettings(_characterOnMap.GetAnimation(), false);
                    break;
            }

            // プレイヤーの場合はパーティにも反映
            if (eventID == "-2")
            {
                for (var i = 0; i < MapManager.GetPartyMemberNum(); i++)
                {
                    var party = MapManager.GetPartyGameObject(i).GetComponent<CharacterOnMap>();
                    switch (command.parameters[1])
                    {
                        case "0":
                            party.SetAnimationSettings(true, party.GetStepAnimation());
                            break;
                        case "1":
                            party.SetAnimationSettings(false, party.GetStepAnimation());
                            break;
                        case "2":
                            party.SetAnimationSettings(party.GetAnimation(), true);
                            break;
                        case "3":
                            party.SetAnimationSettings(party.GetAnimation(), false);
                            break;
                    }
                }
            }

            ProcessEndAction();
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}