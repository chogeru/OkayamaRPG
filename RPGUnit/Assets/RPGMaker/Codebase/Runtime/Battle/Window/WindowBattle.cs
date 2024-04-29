using RPGMaker.Codebase.Runtime.Common;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Battle.Window
{
    /// <summary>
    /// メッセージやステータスなどを描くためのメソッドを多く持つ、ウィンドウオブジェクト
    /// MV の Winow_Base
    /// Unite では Prefab を利用するため、ほとんどの処理が不要
    /// </summary>
    public class WindowBattle : WindowBase
    {
        /// <summary>
        /// [static] 基本のアイコン幅 (規定値 : 32)
        /// </summary>
        protected int _iconWidth = 32;
        /// <summary>
        /// [static] 基本のアイコン高さ (規定値 : 32)
        /// </summary>
        protected int _iconHeight = 32;
        /// <summary>
        /// [static] 基本の顔画像の幅 (規定値 : 144)
        /// </summary>
        protected int _faceWidth = 144;
        /// <summary>
        /// [static] 基本の顔画像の高さ (規定値 : 144)
        /// </summary>
        protected int _faceHeight = 144;
        /// <summary>
        /// ウィンドウが開いている途中か
        /// </summary>
        private bool _opening;
        /// <summary>
        /// ウィンドウが閉じている途中か
        /// </summary>
        private bool _closing;
        /// <summary>
        /// 透過度
        /// Unite固有
        /// </summary>
        private int _opacity;
        /// <summary>
        /// 表示状態
        /// Unite固有
        /// </summary>
        private bool _visible;
        /// <summary>
        /// 開放度(0 〜 255)
        /// </summary>
        public int Openness;
        /// <summary>
        /// ポーズサインが表示中か
        /// </summary>
        public bool Pause;

        [SerializeField] protected new GameObject gameObject;
        [SerializeField] protected     GameObject GraphicLineObject;
        [SerializeField] protected     GameObject GraphicObject;



        /// <summary>
        /// 表示状態
        /// </summary>
        public bool Visible
        {
            get => _visible;
            set
            {
                _visible = value;
                Opacity = value ? 255 : 0;
                gameObject.transform.localScale = value ? new Vector3(1, 1, 1) : new Vector3(0, 0, 0);
            }
        }

        /// <summary>
        /// Activeかどうか
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// 透過度
        /// </summary>
        public int Opacity
        {
            get => _opacity;
            set
            {
                _opacity = value;
                if (gameObject.GetComponent<CanvasGroup>() == null) gameObject.AddComponent<CanvasGroup>();

                gameObject.GetComponent<CanvasGroup>().alpha = value / 255;
            }
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public virtual void Initialize() {
            //CanvasGroupを追加
            if (gameObject.GetComponent<CanvasGroup>() == null) gameObject.AddComponent<CanvasGroup>();

            //GameObjectをActiveにする
            gameObject.SetActive(true);

            //各フラグ初期化
            _opening = false;
            _closing = false;
            Visible = false;

            Init();
        }

        /// <summary>
        /// Update処理
        /// </summary>
        virtual public void UpdateTimeHandler() {
            UpdateOpen();
            UpdateClose();
        }

        /// <summary>
        /// ウィンドウを開いている状態をアップデート
        /// </summary>
        public void UpdateOpen() {
            if (_opening)
            {
                Openness += 32;
                if (IsOpen()) _opening = false;
            }
        }

        /// <summary>
        /// ウィンドウが完全に開いているか
        /// </summary>
        /// <returns></returns>
        public bool IsOpen() {
            return Openness >= 255;
        }

        /// <summary>
        /// ウィンドウが完全に閉じているか
        /// </summary>
        /// <returns></returns>
        public bool IsClosed() {
            return Openness <= 0;
        }

        /// <summary>
        /// ウィンドウを閉じている状態をアップデート
        /// </summary>
        public void UpdateClose() {
            if (_closing)
            {
                Openness -= 32;
                if (IsClosed())
                {
                    Deactivate();
                    Visible = false;
                    _closing = false;
                }
            }
        }

        /// <summary>
        /// ウィンドウを開く
        /// </summary>
        public virtual void Open() {
            Activate();
            Visible = true;
            if (!IsOpen()) _opening = true;

            _closing = false;
        }

        /// <summary>
        /// ウィンドウを閉じる
        /// </summary>
        public virtual void Close() {
            if (!IsClosed())
            {
                _closing = true;
            }

            _opening = false;
            
        }

        /// <summary>
        /// ウィンドウが開いている途中か
        /// </summary>
        /// <returns></returns>
        virtual public bool IsOpening() {
            return _opening;
        }

        /// <summary>
        /// ウィンドウが閉じている途中か
        /// </summary>
        /// <returns></returns>
        virtual public bool IsClosing() {
            return _closing;
        }

        /// <summary>
        /// ウィンドウを表示
        /// </summary>
        public virtual void Show() {
            Visible = true;
        }

        /// <summary>
        /// ウィンドウを非表示(閉じるわけではない)
        /// </summary>
        public virtual void Hide() {
            Visible = false;
        }

        /// <summary>
        /// ウィンドウをアクティブにする
        /// </summary>
        public void Activate() {
            Active = true;
        }

        /// <summary>
        /// 非アクティブにする
        /// </summary>
        protected void Deactivate() {
            Active = false;
        }
    }

    public class TextState
    {
        public int    height;
        public int    index;
        public int    left;
        public string text;
        public int    x;
        public int    y;
    }
}