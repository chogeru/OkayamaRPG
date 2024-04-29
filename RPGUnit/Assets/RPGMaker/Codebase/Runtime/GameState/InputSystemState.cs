using RPGMaker.Codebase.Runtime.Common.Enum;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RPGMaker.Codebase.Runtime.Common
{
    public class InputSystemState : MonoBehaviour
    {
        private Vector2                      _move;
        private Dictionary<HandleType, bool> _inputSystemState = new Dictionary<HandleType, bool>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        InputSystemState() {
            //上下左右キー
            _inputSystemState.Add(HandleType.Left, false);
            _inputSystemState.Add(HandleType.Right, false);
            _inputSystemState.Add(HandleType.Up, false);
            _inputSystemState.Add(HandleType.Down, false);

            //決定キー、戻るキー
            _inputSystemState.Add(HandleType.Decide, false);
            _inputSystemState.Add(HandleType.Back, false);

            //上下左右のキーダウン
            _inputSystemState.Add(HandleType.LeftKeyDown, false);
            _inputSystemState.Add(HandleType.RightKeyDown, false);
            _inputSystemState.Add(HandleType.UpKeyDown, false);
            _inputSystemState.Add(HandleType.DownKeyDown, false);

            //シフトキー
            _inputSystemState.Add(HandleType.LeftShiftDown, false);
            _inputSystemState.Add(HandleType.LeftShiftUp, false);

            //メニュー操作
            _inputSystemState.Add(HandleType.RightClick, false);

            //ページ切り替え
            _inputSystemState.Add(HandleType.PageLeft, false);
            _inputSystemState.Add(HandleType.PageRight, false);

            //自分をInputHandlerに登録
            InputHandler.SetInputSystemState(this);
        }

        /// <summary>
        /// 十字キー
        /// </summary>
        /// <param name="context"></param>
        public void OnMove(InputAction.CallbackContext context)
        {
            //初期化
            _inputSystemState[HandleType.Left] = false;
            _inputSystemState[HandleType.Right] = false;
            _inputSystemState[HandleType.Up] = false;
            _inputSystemState[HandleType.Down] = false;

            //十字キーの大きさを取得
            _move = context.ReadValue<Vector2>();

            if(Mathf.Abs(_move.x) >= Mathf.Abs(_move.y))
            {
                //左右への移動
                if (_move.x <= -0.5)
                {
                    _inputSystemState[HandleType.Left] = true;
                    _inputSystemState[HandleType.LeftKeyDown] = true;
                }
                else if (_move.x >= 0.5)
                {
                    _inputSystemState[HandleType.Right] = true;
                    _inputSystemState[HandleType.RightKeyDown] = true;
                }
            }
            else
            {
                if (_move.y <= -0.5)
                {
                    _inputSystemState[HandleType.Down] = true;
                    _inputSystemState[HandleType.DownKeyDown] = true;
                }
                else if (_move.y >= 0.5)
                {
                    _inputSystemState[HandleType.Up] = true;
                    _inputSystemState[HandleType.UpKeyDown] = true;
                }
            }
        }

        /// <summary>
        /// 決定キー
        /// </summary>
        /// <param name="context"></param>
        public void OnFire(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                _inputSystemState[HandleType.Decide] = true;
            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                _inputSystemState[HandleType.Decide] = false;
            }
        }


        /// <summary>
        /// キャンセルキー
        /// </summary>
        /// <param name="context"></param>
        public void OnCancel(InputAction.CallbackContext context) {
            if (context.phase == InputActionPhase.Performed)
            {
                _inputSystemState[HandleType.Back] = true;
            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                _inputSystemState[HandleType.Back] = false;
            }
        }

        /// <summary>
        /// メニュー
        /// </summary>
        /// <param name="context"></param>
        public void OnMenu(InputAction.CallbackContext context) {
            if (context.phase == InputActionPhase.Performed)
            {
                _inputSystemState[HandleType.RightClick] = true;
            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                _inputSystemState[HandleType.RightClick] = false;
            }
        }

        public void OnLeftB(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                _inputSystemState[HandleType.PageLeft] = true;
            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                _inputSystemState[HandleType.PageLeft] = false;
            }
        }

        public void OnRightB(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
            {
                _inputSystemState[HandleType.PageRight] = true;
            }
            else if (context.phase == InputActionPhase.Canceled)
            {
                _inputSystemState[HandleType.PageRight] = false;
            }
        }

        /// <summary>
        /// 渡されたHandleTypeの、現在の状態を返却する
        /// </summary>
        /// <param name="handleType">HandleType</param>
        /// <returns>押下されている時true</returns>
        public bool CurrentInputSystemState(HandleType handleType) {
            bool value = false;
            switch (handleType) {
                //連続でキー入力を受け付けるもの
                case HandleType.Left:
                case HandleType.Right:
                case HandleType.Up:
                case HandleType.Down:
                    return _inputSystemState[handleType];
                //1回発火したら終了するもの
                case HandleType.Decide:
                case HandleType.Back:
                case HandleType.RightClick:
                case HandleType.LeftKeyDown:
                case HandleType.RightKeyDown:
                case HandleType.UpKeyDown:
                case HandleType.DownKeyDown:
                case HandleType.LeftShiftDown:
                case HandleType.LeftShiftUp:
                case HandleType.PageLeft:
                case HandleType.PageRight:
                    value = _inputSystemState[handleType];
                    _inputSystemState[handleType] = false;
                    return value;
            }

            return false;
        }

        /// <summary>
        /// 特定のキーがこのフレームで押されたかどうか（OnPress）
        /// </summary>
        /// <param name="handleType"></param>
        /// <returns></returns>
        public bool OnDown(HandleType handleType) {
            PlayerInput pInput = GetComponent<PlayerInput>();
            if (handleType == HandleType.Left || handleType == HandleType.Right || handleType == HandleType.Up || handleType == HandleType.Down)
            {
                return _inputSystemState[handleType];
            }

            InputAction action;
            if (handleType == HandleType.Decide) action = pInput.actions.FindAction("Fire");
            else if (handleType == HandleType.Back) action = pInput.actions.FindAction("Cancel");
            else if (handleType == HandleType.RightClick) action = pInput.actions.FindAction("Menu");
            else if (handleType == HandleType.LeftShiftDown) action = pInput.actions.FindAction("Dash");
            else if (handleType == HandleType.PageLeft && _inputSystemState[HandleType.PageLeft])
            {
                _inputSystemState[HandleType.PageLeft] = false;
                return true;
            }
            else if (handleType == HandleType.PageRight && _inputSystemState[HandleType.PageRight])
            {
                _inputSystemState[HandleType.PageRight] = false;
                return true;
            }
            else return false;
            return action.WasPressedThisFrame() && action.IsPressed();
        }

        /// <summary>
        /// 特定のキーがこのフレームで離されたかどうか（OnPress）
        /// </summary>
        /// <param name="handleType"></param>
        /// <returns></returns>
        public bool OnUp(HandleType handleType) {
            PlayerInput pInput = GetComponent<PlayerInput>();
            if (handleType == HandleType.Left || handleType == HandleType.Right || handleType == HandleType.Up || handleType == HandleType.Down)
            {
                return _inputSystemState[handleType];
            }

            InputAction action;
            if (handleType == HandleType.Decide) action = pInput.actions.FindAction("Fire");
            else if (handleType == HandleType.Back) action = pInput.actions.FindAction("Cancel");
            else if (handleType == HandleType.RightClick) action = pInput.actions.FindAction("Menu");
            else if (handleType == HandleType.LeftShiftDown) action = pInput.actions.FindAction("Dash");
            else return false;

            return action.WasReleasedThisFrame();
        }

        /// <summary>
        /// 特定のキーがこのフレームで押され続けているかどうか（OnPress）
        /// </summary>
        /// <param name="handleType"></param>
        /// <returns></returns>
        public bool OnPress(HandleType handleType) {
            PlayerInput pInput = GetComponent<PlayerInput>();
            if (handleType == HandleType.Left || handleType == HandleType.Right || handleType == HandleType.Up || handleType == HandleType.Down)
            {
                return _inputSystemState[handleType];
            }

            InputAction action;
            if (handleType == HandleType.Decide) action = pInput.actions.FindAction("Fire");
            else if (handleType == HandleType.Back) action = pInput.actions.FindAction("Cancel");
            else if (handleType == HandleType.RightClick) action = pInput.actions.FindAction("Menu");
            else if (handleType == HandleType.LeftShiftDown) action = pInput.actions.FindAction("Dash");
            else return false;
            return action.IsPressed();
        }
    }
}
