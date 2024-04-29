using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Vehicle;
using RPGMaker.Codebase.Runtime.Common;
using System;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Map.Component.Character
{
    public class VehicleOnMap : CharacterOnMap
    {
        // データプロパティ
        //--------------------------------------------------------------------------------------------------------------
        private VehiclesDataModel _vehiclesDataModelCache;

        // 状態プロパティ
        //--------------------------------------------------------------------------------------------------------------
        protected SpriteRenderer _spriteRenderer;

        // 表示要素プロパティ
        //--------------------------------------------------------------------------------------------------------------
        protected VehicleGraphic _vehicleGraphic;

        // 関数プロパティ
        //--------------------------------------------------------------------------------------------------------------

        protected VehiclesDataModel _vehiclesDataModel
        {
            get
            {
                return _vehiclesDataModelCache ??= DataManager.Self().GetVehicleDataModel(CharacterId);
            }
        }
        
        // initialize methods
        //--------------------------------------------------------------------------------------------------------------
        /**
         * 目的位置へ毎フレーム移動していく
         */
        override protected void MoveToPositionOnTileByFrame(Vector2 destinationPositionOnWorld, Action callBack) {
            Vector2 currentPos = gameObject.transform.position;
            if (currentPos != destinationPositionOnWorld)
            {
                //メニュー開き中は進まないようにしておく
                if (MenuManager.IsMenuActive) return;

                var newPos = Vector2.MoveTowards(
                    currentPos,
                    destinationPositionOnWorld,
                    Time.deltaTime * _moveSpeed);

                SetGameObjectPositionWithRenderingOrder(newPos);
            }
            else
            {
                callBack();
            }
        }

        public SpriteRenderer GetVehicleShadow() {
            return _spriteRenderer;
        }

        public void SetVehicleShadow(SpriteRenderer spriteRenderer) {
            _spriteRenderer = spriteRenderer;
        }

        // 乗降状態を設定。
        public void SetRide(bool isRide) {
            SetSortingLayer(isRide ?
                _vehiclesDataModel.MoveAria != VehiclesDataModel.MoveAriaType.None:
                false);
            SetCharacterRide(isRide, _moveSpeed);
        }

        private void Start() {
            //フレーム単位での処理
            TimeHandler.Instance.AddTimeActionEveryFrame(UpdateTimeHandler);
        }

        private void OnDestroy() {
            TimeHandler.Instance?.RemoveTimeAction(UpdateTimeHandler);
        }
    }
}