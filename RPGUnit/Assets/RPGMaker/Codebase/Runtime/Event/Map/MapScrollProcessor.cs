using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Event.Map
{
    public class MapScrollProcessor : AbstractEventCommandProcessor
    {
        private Camera  _camera;
        private int     _moveSpeed;
        private float   _speed = 6f;
        private string  _mapId;
        private int     _direction;
        private float   _amount;
        private float   _value;
        private bool    _isWait = false;

        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            //速度を初期化
            _speed = 6f;

            //現在のマップIDを保持
            _mapId = MapManager.CurrentMapDataModel.id;

            //方向(0:下 1:左 2:右 3:上)
            //距離
            //速度(0:1/8倍速　1:1/4倍速 2:1/2倍速 3:標準速 4:2倍速 5:4倍速")
            //ウェイト(0:待たない 1:待つ)
            _camera = MapManager.GetCamera();

            _direction = int.Parse(command.parameters[0]);
            _amount = int.Parse(command.parameters[1]);
            _value = 0;
            _moveSpeed = int.Parse(command.parameters[2]);
            MoveSettings();

            TimeHandler.Instance.RemoveTimeAction(StartScroll);
            TimeHandler.Instance.AddTimeActionEveryFrame(StartScroll);

            if (command.parameters[3] == "0")
            {
                _isWait = false;
                ProcessEndAction();
            }
            else
            {
                _isWait = true;
            }
        }

        /// <summary>
        ///     速度の設定
        /// </summary>
        private void MoveSettings() {
            switch (_moveSpeed)
            {
                case 0:
                    _speed = _speed / 8;
                    break;
                case 1:
                    _speed = _speed / 4;
                    break;
                case 2:
                    _speed = _speed / 2;
                    break;
                case 4:
                    _speed = _speed * 2;
                    break;
                case 5:
                    _speed = _speed * 4;
                    break;
            }
        }

        /// <summary>
        ///     スクロールアニメーション
        /// </summary>
        /// <param name="flg"></param>
        /// <returns></returns>
        private void StartScroll() {
            float sub = Time.deltaTime * _speed;
            if (_value >= _amount || _mapId != MapManager.CurrentMapDataModel.id)
            {
                // カメラ位置を保存
                DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.cameraX =
                    (int) MapManager.GetCameraPosition().x;
                DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.cameraY =
                    (int) MapManager.GetCameraPosition().y;

                TimeHandler.Instance.RemoveTimeAction(StartScroll);
                if (_isWait)
                {
                    _isWait = false;
                    ProcessEndAction();
                }
            }
            else
            {
                if (_value + sub > _amount)
                {
                    sub = _amount - _value;
                    _value = _amount;
                }
                else
                {
                    _value += sub;
                }

                // MapManagerのカメラ位置を更新
                var cameraPos = MapManager.GetCameraPosition();
                if (_direction == 0)
                    cameraPos += new Vector3(0, -sub, 0);
                else if (_direction == 1)
                    cameraPos += new Vector3(-sub, 0, 0);
                else if (_direction == 2)
                    cameraPos += new Vector3(sub, 0, 0);
                else
                    cameraPos += new Vector3(0, sub, 0);
                MapManager.SetCameraPosition(cameraPos);
            }
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}