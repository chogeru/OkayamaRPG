using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map;
using RPGMaker.Codebase.Runtime.Map.Component.Character;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Event.Character
{
    /// <summary>
    /// [キャラクター]-[キャラ画像設定]
    /// </summary>
    public class ChangeImageProcessor : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            //変更するプレイヤー、イベントのオブジェクト取得
            GameObject targetObj;
            //プレイヤーかイベントか
            var isEvent = true;

            eventID = (command.parameters[0] == "-1") ? eventID : command.parameters[0];
            if (eventID == "-2") //プレイヤーの座標に出す
            {
                targetObj = MapManager.OperatingActor.gameObject;
                isEvent = false;
            }
            else
            {
                targetObj = MapEventExecutionController.Instance.GetEventMapGameObject(eventID);
            }

            //画像の変更
            if (command.parameters[2] == "1")
                if (isEvent)
                    targetObj.GetComponent<EventOnMap>().ChangeAsset(command.parameters[1]);
                else
                    targetObj.GetComponent<ActorOnMap>().ChangeAsset(command.parameters[1]);

            //画像の不透明度
            if (command.parameters[4] == "1")
                if (isEvent)
                    targetObj.GetComponent<EventOnMap>().SetOpacityToNpc((float) (int.Parse(command.parameters[3]) / 255.0));
                else
                {
                    foreach (var actor in MapManager.GetAllActorOnMap())
                    {
                        DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.opacity = (float) (int.Parse(command.parameters[3]) / 255.0);
                        actor.SetOpacity((float) (int.Parse(command.parameters[3]) / 255.0));
                    }
                }

            //画像の合成方法
            if (command.parameters[6] == "1")
                if (isEvent)
                    targetObj.GetComponent<EventOnMap>().SetChangeBlendMode(int.Parse(command.parameters[5]));
                else
                    targetObj.GetComponent<ActorOnMap>().SetChangeBlendMode(int.Parse(command.parameters[5]));
            
            //向きが指定されている場合は、強制的に向きを変更する
            if (command.parameters.Count >= 8 && command.parameters[7] != "0")
            {
                CharacterMoveDirectionEnum direction = CharacterMoveDirectionEnum.Up;
                if (command.parameters[7] == "1") direction = CharacterMoveDirectionEnum.Down;
                else if (command.parameters[7] == "2") direction = CharacterMoveDirectionEnum.Left;
                else if (command.parameters[7] == "3") direction = CharacterMoveDirectionEnum.Right;
                else if (command.parameters[7] == "4") direction = CharacterMoveDirectionEnum.Up;
                else if (command.parameters[7] == "5") direction = CharacterMoveDirectionEnum.Damage;

                if (isEvent)
                    targetObj.GetComponent<EventOnMap>().ChangeCharacterDirection(direction);
                else
                    targetObj.GetComponent<ActorOnMap>().ChangeCharacterDirection(direction);
            }

            //乗り物に搭乗している場合は、改めて不透明度を0にする
            if (!isEvent && MapManager.CurrentVehicleId != "")
                foreach (var actor in MapManager.GetAllActorOnMap())
                    actor.SetCharacterEnable(false);

            ProcessEndAction();
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}