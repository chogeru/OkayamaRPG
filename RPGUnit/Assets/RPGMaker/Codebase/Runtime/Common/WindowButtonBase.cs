using RPGMaker.Codebase.Runtime.Common;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TextMP = TMPro.TextMeshProUGUI;

namespace RPGMaker.Codebase.CoreSystem.Helper
{
    public class WindowButtonBase : MonoBehaviour
    {
        [SerializeField] protected bool AutoChangeFocusdColor;

        [SerializeField] public GameObject ScrollView;
        [SerializeField] public GameObject Content;
        [SerializeField] public UnityEvent OnFocus;
        [SerializeField] public Button.ButtonClickedEvent OnClick;
        [SerializeField] protected float SetPaddingY = 20.0f;
        [SerializeField] public int MyIndex = 0;
        [SerializeField] public bool IsChildrenButton = false;

        public bool IsHorizontal = false;

        private bool isFocued;
        private Button _button;
        private Transform _highlightButton;
        private Image _hilightImage;
        private int clickCount = 0;
        private bool _isGray = false;
        private bool _isSilent = false;
        private bool _alreadyHilight = false;
        private bool _isAnimation = true;
        private float _orgHilightAlpha;
        private float _minHilightAlpha;
        private float _startAnimeTime;

        /// <summary>
        /// 既にDestroyされている状況の場合に、後続の処理を行わないようにするためのフラグ
        /// </summary>
        private bool _isDestroy = false;

        private void Start() {
            if (AutoChangeFocusdColor)
            {
                //初期化
                _button = this.GetComponent<Button>();
                _highlightButton = this.transform.Find("Highlight");

                //ハイライト部品をデフォルトでは無効にする
                if (_highlightButton == null)
                {
                    _highlightButton = this.transform.Find("Text/Highlight");
                }
                if (_highlightButton != null)
                {
                    if (_highlightButton.GetComponent<Image>() != null)
                    {
                        _hilightImage = _highlightButton.GetComponent<Image>();
                        _hilightImage.gameObject.SetActive(false);
                        isFocued = false;

                        _orgHilightAlpha = _hilightImage.color.a;
                        _minHilightAlpha = _orgHilightAlpha - 64.0f / 255.0f;
                        if (_minHilightAlpha < 0) _minHilightAlpha = 0;
                    }
                }

                //クリックイベントを登録
                transform.GetComponent<Button>().onClick.AddListener(OnClickBase);
            }
        }

        /// <summary>
        /// ボタンの有効,無効状態の切り替え
        /// </summary>
        /// <param name="isEnabled"></param>
        public void SetEnabled(bool isEnabled, bool removeHighlight = false) {
            if (_isDestroy) return;
            if (AutoChangeFocusdColor)
            {
                if (_button != null)
                {
                    _button.enabled = isEnabled;
                    if (removeHighlight)
                    {
                        //ハイライト部品を無効にする
                        if (_hilightImage != null)
                        {
                            _hilightImage.gameObject.SetActive(false);
                        }
                    }
                    if (!isEnabled && !removeHighlight && IsHighlight())
                    {
                        // ボタンが無効になり、ハイライトも残し、今ハイライト中の場合
                        _hilightImage.color = new Color(_hilightImage.color.r, _hilightImage.color.g, _hilightImage.color.b, _orgHilightAlpha);
                    }
                    else if (isEnabled && IsHighlight())
                    {
                        // ボタンが有効になり、今ハイライト中の場合
                        _startAnimeTime = Time.time;
                        _hilightImage.color = new Color(_hilightImage.color.r, _hilightImage.color.g, _hilightImage.color.b, _orgHilightAlpha);
                    }
                }
            }
        }

        /// <summary>
        /// グレー設定を行う
        /// </summary>
        /// <param name="flg"></param>
        public void SetGray(bool flg = false) {
            if (_isDestroy) return;
            if (flg && !_isGray)
            {
                //グレー
                _isGray = true;
                var texts = this.GetComponentsInChildren<Text>();
                for (int i = 0; i < texts.Length; i++)
                {
                    texts[i].color = new Color(texts[i].color.r * 0.5f, texts[i].color.g * 0.5f, texts[i].color.b * 0.5f, texts[i].color.a);
                }
                var textMeshPro = this.GetComponentsInChildren<TextMP>();
                for (int i = 0; i < textMeshPro.Length; i++)
                {
                    textMeshPro[i].color = new Color(textMeshPro[i].color.r * 0.5f, textMeshPro[i].color.g * 0.5f, textMeshPro[i].color.b * 0.5f, textMeshPro[i].color.a);
                }

                var images = GetComponentsInChildren<Image>();
                for (int i = 0; i < images.Length; i++)
                {
                    if (images[i].name.StartsWith("Icon"))
                    {
                        images[i].color = new Color(images[i].color.r * 0.5f, images[i].color.g * 0.5f,
                            images[i].color.b * 0.5f, images[i].color.a);
                    }
                }                
            }
            else if (!flg && _isGray)
            {
                //元の色
                _isGray = false;
                var texts = this.GetComponentsInChildren<Text>();
                for (int i = 0; i < texts.Length; i++)
                {
                    texts[i].color = new Color(texts[i].color.r * 2f, texts[i].color.g * 2f, texts[i].color.b * 2f, texts[i].color.a);
                }
                var textMeshPro = this.GetComponentsInChildren<TextMP>();
                for (int i = 0; i < textMeshPro.Length; i++)
                {
                    textMeshPro[i].color = new Color(textMeshPro[i].color.r * 2f, textMeshPro[i].color.g * 2f, textMeshPro[i].color.b * 2f, textMeshPro[i].color.a);
                }
                
                var images = GetComponentsInChildren<Image>();
                for (int i = 0; i < images.Length; i++)
                {
                    if (images[i].name.StartsWith("Icon"))
                    {
                        images[i].color = new Color(images[i].color.r * 2f, images[i].color.g * 2f,
                            images[i].color.b * 2f, images[i].color.a);
                    }
                }
            }
        }

        /// <summary>
        /// 文字を透明にする 選択肢のちらつき防止用
        /// </summary>
        /// <param name="flg"></param>
        public void SetTransparent(bool flg) {
            if (flg)
            {
                var texts = this.GetComponentsInChildren<Text>();
                for (int i = 0; i < texts.Length; i++)
                {
                    texts[i].color = new Color(texts[i].color.r, texts[i].color.g, texts[i].color.b, texts[i].color.a * 0.01f);
                }
                var textMeshPro = this.GetComponentsInChildren<TextMP>();
                for (int i = 0; i < textMeshPro.Length; i++)
                {
                    textMeshPro[i].color = new Color(texts[i].color.r, texts[i].color.g, texts[i].color.b, texts[i].color.a * 0.01f);
                }
            }
            else
            {
                var texts = this.GetComponentsInChildren<Text>();
                for (int i = 0; i < texts.Length; i++)
                {
                    texts[i].color = new Color(texts[i].color.r, texts[i].color.g, texts[i].color.b, texts[i].color.a * 100f);
                }
                var textMeshPro = this.GetComponentsInChildren<TextMP>();
                for (int i = 0; i < textMeshPro.Length; i++)
                {
                    textMeshPro[i].color = new Color(texts[i].color.r, texts[i].color.g, texts[i].color.b, texts[i].color.a * 100f);
                }
            }
        }

        /// <summary>
        /// 決定音を鳴動させないようにするフラグ設定
        /// </summary>
        /// <param name="flg"></param>
        public void SetSilentClick(bool flg) {
            if (_isDestroy) return;
            _isSilent = flg;
        }
        
        /// <summary>
        /// ハイライトを強制的に設定する
        /// </summary>
        public void SetHighlight(bool flg = false) {
            //ハイライトを強制的に設定する
            if (_isDestroy) return;
            if (_hilightImage != null)
            {
                _hilightImage.gameObject.SetActive(flg);
                _startAnimeTime = Time.time;
                _hilightImage.color = new Color(_hilightImage.color.r, _hilightImage.color.g, _hilightImage.color.b, _orgHilightAlpha);
            }
        }

        public void SetAlreadyHighlight(bool flg) {
            if (_isDestroy) return;
            _alreadyHilight = flg;
            SetHighlight(flg);
        }

        /// <summary>
        /// フォーカスが当たっている間、アニメーションするかどうか
        /// </summary>
        /// <param name="isAnimation"></param>
        public void SetAnimation(bool isAnimation) {
            if (_isDestroy) return;
            _isAnimation = isAnimation;
        }

        /// <summary>
        /// 現在このボタンがハイライト状態かどうかの返却
        /// </summary>
        /// <returns></returns>
        public bool IsHighlight() {
            //ハイライト部品が有効かどうかを返却する
            if (_isDestroy) return false;
            if (AutoChangeFocusdColor)
            {
                if (_hilightImage != null)
                {
                    return _hilightImage.gameObject.activeSelf;
                }
            }
            return false;
        }

        /// <summary>
        /// 最初からフォーカス状態にさせるために使用する
        /// </summary>
        public void SetDefaultClick() {
            if (_isDestroy) return;
            clickCount = 1;
        }

        /// <summary>
        /// フォーカスが当たっている場合に、Highlightを有効にする
        /// フォーカスが外れている場合に、Highlightを無効にする
        /// 本設定は、GUIで AutoChangeFocusdColor にチェックがついている部品のみを対象として動作する
        /// また、ボタンが有効な場合にのみ動作する
        /// </summary>
        private void Update() {
            //GUIで、対象と設定した部品のみを対象とする
            if (_isDestroy) return;
            if (AutoChangeFocusdColor)
            {
                //ボタンが有効状態の場合にのみ処理する
                //デフォルトは有効状態
                if (_button != null && _button.enabled)
                {
                    //現在、この部品にフォーカスが当たっている場合の処理
                    if (EventSystem.current.currentSelectedGameObject == this.gameObject && !isFocued)
                    {
                        //ハイライト部品を有効にする
                        isFocued = true;
                        if (_hilightImage != null)
                        {
                            _hilightImage.gameObject.SetActive(true);
                            _startAnimeTime = Time.time;
                            _hilightImage.color = new Color(_hilightImage.color.r, _hilightImage.color.g, _hilightImage.color.b, _orgHilightAlpha);

                            if (ScrollView != null && Content != null)
                            {
                                if (IsChildrenButton)
                                {
                                    AdjustScrollByEventSelect();
                                }
                                else
                                {
                                    if (IsHorizontal)
                                        AdjustScrollByHorizontal();
                                    else
                                        AdjustScroll();
                                }
                            }
                            if (OnFocus != null)
                            {
                                OnFocus.Invoke();
                            }

                            clickCount = 0;
                            if (!(Input.GetMouseButtonDown(0)))
                            {
                                //マウスクリックによるフォーカス移動ではない場合、clickCountを1進める
                                clickCount = 1;

                                if (InputHandler.OnDown(Runtime.Common.Enum.HandleType.Up) || InputHandler.OnDown(Runtime.Common.Enum.HandleType.Down) ||
                                    InputHandler.OnDown(Runtime.Common.Enum.HandleType.Left) || InputHandler.OnDown(Runtime.Common.Enum.HandleType.Right))
                                {
                                    //上下左右キーが押されたことによるフォーカス移動の場合、音声を鳴動する
                                    SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, DataManager.Self().GetSystemDataModel().soundSetting.cursor);
                                    SoundManager.Self().PlaySe();
                                }
                            }
                        }
                    }
                    //現在、この部品にフォーカスが当たっていない場合の処理
                    else if (EventSystem.current.currentSelectedGameObject != this.gameObject && isFocued)
                    {
                        //ハイライト部品を無効にする
                        isFocued = false;
                        if (_hilightImage != null && !_alreadyHilight)
                        {
                            _hilightImage.gameObject.SetActive(false);
                        }
                        clickCount = 0;
                    }
                    //既にフォーカスがあたっていた場合の処理
                    else if (IsHighlight())
                    {
                        if (_hilightImage != null && _isAnimation)
                        {
                            //ハイライトのアニメーション
                            float phase = (Time.time - _startAnimeTime) * Mathf.PI / 1.5f;
                            var alpha = _orgHilightAlpha - Mathf.Abs(Mathf.Sin(phase) * (_orgHilightAlpha - _minHilightAlpha));
                            _hilightImage.color = new Color(_hilightImage.color.r, _hilightImage.color.g, _hilightImage.color.b, alpha);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// フォーカスがこの部品に移動した場合に、スクロール位置を調節する
        /// スクロール位置を調節するためのパラメーターが指定されている場合にのみ動作する
        /// </summary>
        private void AdjustScroll() {
            if (_isDestroy) return;
            //表示可能な高さ
            float showHeight = ScrollView.GetComponent<RectTransform>().rect.height;
            //スクロール対象のコンテンツの高さ
            float contentHeight = Content.GetComponent<RectTransform>().rect.height;
            //現在のスクロール位置
            float nowY = Content.GetComponent<RectTransform>().anchoredPosition.y;
            //自分のY座標
            float myPositionY = this.GetComponent<RectTransform>().anchoredPosition.y;
            //自分の高さ
            float myHeight = this.GetComponent<RectTransform>().rect.height;
            //パディング
            float paddingY = SetPaddingY;
            try
            {
                paddingY = Content.GetComponent<VerticalLayoutGroup>().padding.top;
            }
            catch (Exception) { }
            //自分の座標調整（スクロール量と方向が逆なため）
            myPositionY *= -1;

            //現在のスクロール位置 + 表示可能な高さより、自分のY座標 + 自分の高さが大きい場合は、自分のY座標位置まで下にスクロールする
            if (nowY + showHeight < myPositionY + myHeight)
            {
                //自分が完全に見える位置まで移動
                float y = myPositionY + myHeight - showHeight - paddingY * 2;
                if (y < 0)
                    y = 0;
                else if (y > contentHeight)
                    y = contentHeight - showHeight;
                
                Content.GetComponent<RectTransform>().anchoredPosition = new Vector2(Content.GetComponent<RectTransform>().anchoredPosition.x, y);
            }
            //現在のスクロール位置より、自分のY座標が小さい場合は、自分のY座標位置まで上にスクロールする
            else if (nowY > myPositionY - paddingY * 2)
            {
                //自分が完全に見える位置まで移動
                float y = myPositionY - paddingY * 2;
                if (y < 0)
                    y = 0;
                else if (y > contentHeight)
                    y = contentHeight - showHeight;
                
                Content.GetComponent<RectTransform>().anchoredPosition = new Vector2(Content.GetComponent<RectTransform>().anchoredPosition.x, y);
            }
        }
        
        public void AdjustScrollByHorizontal() {
            if (_isDestroy) return;
            //表示可能な高さ
            float showWidth = ScrollView.GetComponent<RectTransform>().rect.width;
            //スクロール対象のコンテンツの高さ
            float contentWidth = Content.GetComponent<RectTransform>().rect.width;
            //現在のスクロール位置
            float nowX = Content.GetComponent<RectTransform>().anchoredPosition.x;
            //自分のY座標
            float myPositionX = this.GetComponent<RectTransform>().anchoredPosition.x;
            //自分の高さ
            float myWidth = this.GetComponent<RectTransform>().rect.width;
            
            if (nowX + showWidth < myPositionX + myWidth)
            {
                //自分が完全に見える位置まで移動
                float x = -(myPositionX + myWidth - showWidth);
                if (x > 0) x = 0;

                Content.GetComponent<RectTransform>().anchoredPosition =
                    new Vector2(x, Content.GetComponent<RectTransform>().anchoredPosition.y);
            }
            else if (nowX < myPositionX)
            {
                //自分が完全に見える位置まで移動
                float x = myPositionX;
                if (x > 0)
                    x = 0;
                
                Content.GetComponent<RectTransform>().anchoredPosition =
                    new Vector2(x, Content.GetComponent<RectTransform>().anchoredPosition.y);
            }
        }
        
        /// <summary>
        //イベントコマンド「選択肢」専用
        // </summary>
        public void AdjustScrollByEventSelect() {
            if (_isDestroy) return;
            //表示可能な高さ
            float showHeight = ScrollView.GetComponent<RectTransform>().rect.height;
            //スクロール対象のコンテンツの高さ
            float contentHeight = Content.GetComponent<RectTransform>().rect.height;
            //現在のスクロール位置
            float nowY = Content.GetComponent<RectTransform>().anchoredPosition.y;
            //自分の高さ
            float myHeight = this.GetComponent<RectTransform>().rect.height;
            //パディング
            float paddingY = SetPaddingY;
            try
            {
                paddingY = Content.GetComponent<VerticalLayoutGroup>().padding.top;
            }
            catch (Exception) { }
            //選択肢用の補正値
            paddingY = paddingY * 2;
            //自分のY座標
            float myPositionY = MyIndex * (myHeight + 2);
            myPositionY = myPositionY - contentHeight;

            //現在のスクロール位置 + 表示可能な高さより、自分のY座標 + 自分の高さが大きい場合は、自分のY座標位置まで下にスクロールする
            if (nowY + showHeight < myPositionY + myHeight + paddingY)
            {
                //自分が完全に見える位置まで移動
                float y = myPositionY + myHeight + paddingY - showHeight;
                Content.GetComponent<RectTransform>().anchoredPosition = new Vector2(Content.GetComponent<RectTransform>().anchoredPosition.x, y);
            }
            //現在のスクロール位置より、自分のY座標が小さい場合は、自分のY座標位置まで上にスクロールする
            else if (nowY > myPositionY + 8)
            {
                //自分が完全に見える位置まで移動
                float y = myPositionY + 8;
                Content.GetComponent<RectTransform>().anchoredPosition = new Vector2(Content.GetComponent<RectTransform>().anchoredPosition.x, y);
            }
        }

        /// <summary>
        /// クリックイベント
        /// ボタンの挙動を、マウスクリック時には1回目でフォーカス移動、2回目で発火
        /// Enterの場合には1回目で発火
        /// と統一するためのWrapper
        /// </summary>
        private void OnClickBase() {
            if (_isDestroy) return;
            //ボタンが有効状態の場合にのみ処理する
            //デフォルトは有効状態
            if (_button != null && _button.enabled)
            {
                //クリック、Enter共にカウントを加える
                clickCount++;

                if (IsHighlight() && OnClick != null && clickCount >= 2)
                {
                    if (!_isGray)
                    {
                        //OK時のSE鳴動
                        if (!_isSilent)
                        {
                            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, DataManager.Self().GetSystemDataModel().soundSetting.ok);
                            SoundManager.Self().PlaySe();
                        }

                        //クリックイベント発火
                        OnClickWaitFrame();
                    }
                    else
                    {
                        //ブザー音鳴動し、クリックイベントは発火させない
                        SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, DataManager.Self().GetSystemDataModel().soundSetting.buzzer);
                        SoundManager.Self().PlaySe();
                    }
                }
                else
                {
                    //カーソル移動のSE鳴動
                    SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, DataManager.Self().GetSystemDataModel().soundSetting.cursor);
                    SoundManager.Self().PlaySe();
                }
            }
        }

        private void OnDestroy() {
            _isDestroy = true;
        }

        private async void OnClickWaitFrame() {
            await Task.Delay(1);
            OnClick.Invoke();
        }

        public void SetRaycastTarget(bool flg) {
            if (GetComponent<Image>() != null)
            { 
                GetComponent<Image>().raycastTarget = flg;
            }
        }

        public void RemoveFocus() {
            isFocued = false;
            if (_hilightImage != null && !_alreadyHilight)
            {
                _hilightImage.gameObject.SetActive(false);
            }
            clickCount = 0;
        }
    }
}
