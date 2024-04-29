using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Battle.Window
{
    /// <summary>
    /// コマンドカーソルの移動やスクロールを行うウィンドウ
    /// </summary>
    public class WindowSelectable : WindowBattle
    {
        /// <summary>
        /// 入力ハンドラ
        /// </summary>
        protected Dictionary<string, Action> _handlers;
        /// <summary>
        /// ヘルプウィンドウ
        /// </summary>
        private WindowHelp _helpWindow;
        /// <summary>
        /// 選択されている項目の番号
        /// </summary>
        protected int _index;
        /// <summary>
        /// アイコン画像
        /// </summary>
        protected Image _iconImage = null;
        /// <summary>
        /// Selector
        /// </summary>
        [SerializeField] protected List<Selector> selectors;

        /// <summary>
        /// 初期化
        /// </summary>
        public override void Initialize() {
            base.Initialize();
            _index = -1;
            _helpWindow = null;
            _handlers = new Dictionary<string, Action>();
            Deactivate();
        }

        /// <summary>
        /// 選択中の項目の番号を返す
        /// </summary>
        /// <returns></returns>
        public int Index() {
            return _index;
        }

        /// <summary>
        /// ウィンドウが持つ最大項目数を返す
        /// </summary>
        /// <returns></returns>
        public virtual int MaxItems() {
            return 4;
        }

        /// <summary>
        /// ウィンドウをアクティブにする
        /// </summary>
        public new void Activate() {
            base.Activate();
            Reselect();
        }

        /// <summary>
        /// 非アクティブにする
        /// </summary>
        public new void Deactivate() {
            base.Deactivate();
            Reselect();
        }

        /// <summary>
        /// 指定した番号の項目を選択
        /// </summary>
        /// <param name="index"></param>
        public void Select(int index) {
            _index = index;
            CallUpdateHelp();
        }

        /// <summary>
        /// 全項目を非選択
        /// </summary>
        public void Deselect() {
            Select(-1);
        }

        /// <summary>
        /// 項目の再選択
        /// </summary>
        public void Reselect() {
            Select(_index);
        }

        /// <summary>
        /// ヘルプウィンドウを設定
        /// </summary>
        /// <param name="helpWindow"></param>
        public void SetHelpWindow(WindowHelp helpWindow) {
            _helpWindow = helpWindow;
            CallUpdateHelp();
        }

        /// <summary>
        /// ヘルプウィンドウを表示
        /// </summary>
        public void ShowHelpWindow() {
            if (_helpWindow) _helpWindow.Show();
        }

        /// <summary>
        /// ヘルプウィンドウを非表示
        /// </summary>
        public void HideHelpWindow() {
            if (_helpWindow) _helpWindow.Hide();
        }

        /// <summary>
        /// ハンドラを設定
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="method"></param>
        public void SetHandler(string symbol, Action method) {
            _handlers[symbol] = method;
        }

        /// <summary>
        /// 指定されたハンドラが利用可能か
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public bool IsHandled(string symbol) {
            return _handlers.ContainsKey(symbol);
        }

        /// <summary>
        /// 指定したハンドラを呼ぶ
        /// </summary>
        /// <param name="symbol"></param>
        public void CallHandler(string symbol) {
            if (IsHandled(symbol)) _handlers[symbol].Invoke();
        }

        /// <summary>
        /// Update処理
        /// </summary>
        public new void Update() {
            base.Update();
            if (InputHandler.OnUp(HandleType.RightClick) || InputHandler.OnUp(HandleType.Back) || SceneBattle.BackButton)
            {
                if (Active)
                {
                    if (IsCancelEnabled())
                    {
                        if (SceneBattle.IsUpdateSuppress) return;
                        SceneBattle.IsUpdateSuppress = true;
                        ProcessCancel();
                    }
                }
            }
        }

        /// <summary>
        /// キャンセルが可能か
        /// </summary>
        /// <returns></returns>
        public bool IsCancelEnabled() {
            return IsHandled("cancel");
        }

        /// <summary>
        /// OKの処理
        /// </summary>
        public virtual void ProcessOk() {
            Deactivate();
            CallOkHandler();
        }

        /// <summary>
        /// OKのハンドラを呼ぶ
        /// </summary>
        public virtual void CallOkHandler() {
            CallHandler("ok");
        }

        /// <summary>
        /// キャンセルを処理
        /// </summary>
        public void ProcessCancel() {
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE,
                DataManager.Self().GetSystemDataModel().soundSetting.cancel);
            SoundManager.Self().PlaySe();
            Deactivate();
            CallCancelHandler();
        }

        /// <summary>
        /// キャンセルのハンドラを呼ぶ
        /// </summary>
        public void CallCancelHandler() {
            CallHandler("cancel");
        }

        /// <summary>
        /// ヘルプのアップデートを呼ぶ
        /// </summary>
        public void CallUpdateHelp() {
            if (Active && _helpWindow) UpdateHelp();
        }

        /// <summary>
        /// ヘルプウィンドウをアップデート
        /// </summary>
        public virtual void UpdateHelp() {
            _helpWindow.Clear();
        }

        /// <summary>
        /// 指定項目をヘルプウィンドウに表示
        /// </summary>
        /// <param name="item"></param>
        public void SetHelpWindowItem(GameItem item) {
            if (_helpWindow) _helpWindow.SetItem(item);
        }

        /// <summary>
        /// 全項目を描画
        /// </summary>
        public virtual void DrawAllItems() {
            if (selectors != null && selectors.Any()) selectors.ForEach(selector => selector?.canvas?.SetActive(false));

            if (MaxItems() == 0) ClearItem();

            for (var i = 0; i < MaxItems(); i++)
            {
                DrawItem(i);
            }
        }

        /// <summary>
        /// 指定番号の項目を描画
        /// overrideして利用する
        /// </summary>
        /// <param name="index"></param>
        public virtual void DrawItem(int index) {
        }

        /// <summary>
        /// 指定した番号の項目を削除
        /// </summary>
        public virtual void ClearItem() {
            if (selectors != null)
                selectors.ForEach(selector => selector?.gameObject.SetActive(false));
        }

        /// <summary>
        /// コンテンツの再描画
        /// </summary>
        public virtual void Refresh() {
            DrawAllItems();
        }

        /// <summary>
        /// ボタン選択時処理
        /// </summary>
        /// <param name="index"></param>
        public void OnClickSelection(int index) {
            if (_index == index)
            {
                ProcessOk();
                return;
            }

            Select(index);
        }
        
        /// <summary>
        /// アイコン設定
        /// </summary>
        /// <param name="iconName"></param>
        /// <returns></returns>
        protected Sprite GetItemImage(string iconName) {
            var iconSetTexture =
                UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(
                    "Assets/RPGMaker/Storage/Images/System/IconSet/" + iconName + ".png");

            var iconTexture = iconSetTexture;
            if (iconTexture == null)
            {
                return null;
            }
            
            var sprite = Sprite.Create(
                iconTexture,
                new Rect(0, 0, iconTexture.width, iconTexture.height),
                new Vector2(0.5f, 0.5f)
            );
            
            var aspect = ImageManager.FixAspect( new Vector2(66f,66f), new Vector2(iconTexture.width, iconTexture.height));
            var aspectRatio = _iconImage.GetComponent<AspectRatioFitter>();
            if (aspectRatio == null)
            {
                aspectRatio = _iconImage.gameObject.AddComponent<AspectRatioFitter>();
                aspectRatio.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
            }
            aspectRatio.aspectRatio = aspect;
            
            return sprite;
        }
    }
}