using RPGMaker.Codebase.CoreSystem.Helper;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RPGMaker.Codebase.Runtime.Common
{
    /// <summary>
    ///     UI選択で期待外の動作を補正するクラス。
    /// </summary>
    public class SelectedGameObjectManager : MonoBehaviour
    {
        private GameObject _latestSelectedGameObject;

        public void Clear() {
            DebugUtil.TestLog($"{MethodBase.GetCurrentMethod().Name}()");
            _latestSelectedGameObject = null;
        }

        public void Update() {
            // 選択中のGameObject(EventSystem.current.currentSelectedGameObjectの値)がnullになったら、
            // 保存しておいた最新のGameObjectを選択状態にする。
            // ボタン以外の部分をクリックすると選択中のGameObjectを失いキー操作ができなくなり
            // 場合によってはカーソル表示も消えるのでその対処。
            var currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
            if (currentSelectedGameObject != null)
            {
                if (_latestSelectedGameObject != currentSelectedGameObject)
                {
                    DebugUtil.TestLog($"{_latestSelectedGameObject} = {currentSelectedGameObject}");
                    _latestSelectedGameObject = currentSelectedGameObject;
                }
            }
            else if (_latestSelectedGameObject != null)
            {
                DebugUtil.TestLog($"EventSystem.current.SetSelectedGameObject({_latestSelectedGameObject})");
                EventSystem.current.SetSelectedGameObject(_latestSelectedGameObject);
            }
        }
    }
}