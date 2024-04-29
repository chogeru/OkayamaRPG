using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Character;
using RPGMaker.Codebase.Runtime.Map;
using UnityEngine; //バトルでは本コマンドは利用しない

namespace RPGMaker.Codebase.Runtime.Event.Character
{
    /// <summary>
    /// [キャラクター]-[場所移動]
    /// </summary>
    public class MovePlaceProcessor : AbstractEventCommandProcessor
    {
        private MovePlace _movePlace;

        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            //移動処理実施
            MovePlace(eventID, command);
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }

        private void MovePlace(string eventId, EventDataModel.EventCommand command) {
            if (_movePlace == null)
            {
                _movePlace = new GameObject().AddComponent<MovePlace>();
                _movePlace.Init();
            }

            _movePlace.MovePlaceProcess(eventId, command, CloseMovePlace);
        }

        private void CloseMovePlace() {
            if (_movePlace == null) return;
            Object.Destroy(_movePlace.gameObject);
            _movePlace = null;

            //マップ変更時にOFFにする
            MapManager.IsDisplayName = 0;
            //場所移動成功時にマップ名の表示を行う
            HudDistributor.Instance.NowHudHandler().PlayChangeMapName();

            //移動前にデータを破棄
            //MapEventExecuteController.Instance.RemoveCarryEventOnMap();

            //次のイベント実行
            ProcessEndAction();
        }
    }
}