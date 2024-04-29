using System;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Map
{
    /// <summary>
    ///     メニューの呼び出し管理
    /// </summary>
    public class MenuManager : MonoBehaviour
    {
        /// <summary>
        ///     メニュー開き中か？
        /// </summary>
        private static bool _isMenuActive = false;
        public static bool IsMenuActive
        {
            get { return _isMenuActive; }
            set
            {
                var last = _isMenuActive;
                _isMenuActive = value;
                if (last != _isMenuActive && _isMenuActive)
                {
                    MenuActiveEvent?.Invoke();
                }
            }
        }

        public static event Action MenuActiveEvent;

        /// <summary>
        ///     ショップ開き中か？
        /// </summary>
        public static bool IsShopActive = false;

        public static bool IsEndGameToTitle = false;

        public static MenuBase MenuBase = null;
        public static DateTime dateTime = new DateTime();

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod()]
        static void Init() {
            IsMenuActive = false;
            IsShopActive = false;
            IsEndGameToTitle = false;
            MenuBase = null;
            dateTime = new DateTime();
        }
#endif
    }
}