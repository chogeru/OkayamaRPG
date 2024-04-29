using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Troop;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.DatabaseEditor.View;
using RPGMaker.Codebase.Editor.DatabaseEditor.View.Preview;
using System.Threading.Tasks;
using System.Collections;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using UIE = UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.DatabaseEditor.Window
{
    /// <summary>
    ///     データベースエディター用シーンウィンドウ.
    /// </summary>
    public class SceneWindow : BaseWindow
    {
        // プレビュー定義
        public enum PreviewId
        {
            None        = 0,
            Title       = 1,
            AssetManage = 2,
            Menu        = 3,
            Battle      = 4,
            BattleScene = 5,
            TalkWindow  = 6,
            Scroll      = 7,
            Jump        = 8,
            Route       = 9,
            Animation   = 10,
            CustomMove  = 11,
            Max
        }

        // プレビュー変数
        private TitlePreview        _titlePreview;
        private AssetManagePreview  _assetManagePreview;
        private MenuPreview         _menuPreview;
        private BattlePreview       _battlePreview;
        private BattleScenePreview  _battleScenePreview;
        private TalkWindowPreview   _talkWindowPreview;
        private ScrollPreview       _scrollPreview;
        private JumpPreview         _jumpPreview;
        private RoutePreview        _routePreview;
        private AnimationPreview    _animationPreview;
        private CustomMovePreview   _customMovePreview;

        private PreviewSceneElement _previewSceneElement;

        // 更新処理
        private double    _lastTime;
        private PreviewId _previewId = 0;
        private bool      _isReRendering = false;

        private const float WAIT_TIME = 2f / 60f;
        private EditorCoroutine _renderingSizeRefreshCoroutine;

        public Camera Camera => _previewSceneElement.Camera;

        private void OnEnable() {
            titleContent = new GUIContent(EditorLocalize.LocalizeText("WORD_1563") + " " +
                                          EditorLocalize.LocalizeText("WORD_1581"));
        }

        public TitlePreview GetTitlePreview() {
            return _titlePreview;
        }

        public AssetManagePreview GetManagePreview() {
            return _assetManagePreview;
        }

        public MenuPreview GetMenuPreview() {
            return _menuPreview;
        }

        public BattlePreview GetBattlePreview() {
            return _battlePreview;
        }

        public BattleScenePreview GetBattleScenePreview() {
            return _battleScenePreview;
        }

        public TalkWindowPreview GetTalkWindowPreview() {
            return _talkWindowPreview;
        }

        public ScrollPreview GetScrollPreview() {
            return _scrollPreview;
        }

        public JumpPreview GetJumpPreview() {
            return _jumpPreview;
        }

        public RoutePreview GetRoutePreview() {
            return _routePreview;
        }

        public AnimationPreview GetAnimationPreview() {
            return _animationPreview;
        }
        public CustomMovePreview GetCustomMovePreview() {
            return _customMovePreview;
        }


        // プレビューの作成
        public void Create(PreviewId id = PreviewId.None) {
            _previewId = id;

            switch (_previewId)
            {
                // タイトル
                case PreviewId.Title:
                    if (_titlePreview == null)
                    {
                        var databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;
                        _titlePreview =
                            new TitlePreview(databaseManagementService.LoadTitle());
                    }

                    break;

                // 素材管理
                case PreviewId.AssetManage:
                    if (_assetManagePreview == null) _assetManagePreview = new AssetManagePreview();

                    break;

                // メニュー
                case PreviewId.Menu:
                    if (_menuPreview == null) _menuPreview = new MenuPreview();

                    break;

                // バトル
                case PreviewId.Battle:
                    if (_battlePreview == null) _battlePreview = new BattlePreview();

                    break;

                // バトルシーン (背景と敵)
                case PreviewId.BattleScene:
                    if (_battleScenePreview == null) _battleScenePreview = new BattleScenePreview();

                    break;

                // 会話ウィンドウ
                case PreviewId.TalkWindow:
                    if (_talkWindowPreview == null) _talkWindowPreview = new TalkWindowPreview();

                    break;

                case PreviewId.Scroll:
                    if (_scrollPreview == null) _scrollPreview = new ScrollPreview();

                    break;

                case PreviewId.Jump:
                    if (_jumpPreview == null) _jumpPreview = new JumpPreview();

                    break;

                case PreviewId.Route:
                    if (_routePreview == null) _routePreview = new RoutePreview();

                    break;

                case PreviewId.Animation:
                    if (_animationPreview == null)　_animationPreview = new AnimationPreview();

                    break;

                case PreviewId.CustomMove:
                    if (_customMovePreview == null) _customMovePreview = new CustomMovePreview();

                    break;
            }
        }

        public void Init(object parameter = null) {
            Clear();
            if (_previewSceneElement != null) _previewSceneElement.Dispose();
            _previewSceneElement = new PreviewSceneElement();
            _previewSceneElement.RenderTextureSize = new Vector2Int((int) position.width, (int) position.height);

            // ルートサイズが変わったらレンダリングサイズを更新
            _previewSceneElement.RegisterCallback<UIE.GeometryChangedEvent>(evt =>
            {
                if (_renderingSizeRefreshCoroutine != null) EditorCoroutineUtility.StopCoroutine(_renderingSizeRefreshCoroutine);
                _renderingSizeRefreshCoroutine = null;
                _renderingSizeRefreshCoroutine = EditorCoroutineUtility.StartCoroutine(RenderingSizeRefresh(), this);
            });

            switch (_previewId)
            {
                // タイトル
                case PreviewId.Title:
                    _titlePreview.InitUi(this);
                    break;

                // 素材管理
                case PreviewId.AssetManage:
                    _assetManagePreview.InitUi(this);
                    rootVisualElement.Add(_assetManagePreview.CreateUi());
                    break;

                // ゲームメニュー
                case PreviewId.Menu:
                    if (parameter != null)
                        _menuPreview.InitUi(this, (MenuPreview.UI_CHANGE_TYPE) parameter);
                    else
                        _menuPreview.InitUi(this);
                    break;

                // バトル
                case PreviewId.Battle:
                    _battlePreview.InitUi(this);
                    break;

                // バトルシーン (背景と敵)
                case PreviewId.BattleScene:
                    _battleScenePreview.InitUi(this, parameter as TroopDataModel);
                    break;

                // 会話ウィンドウ
                case PreviewId.TalkWindow:
                    _talkWindowPreview.InitUi(this);
                    break;

                // スクロールウィンドウ
                case PreviewId.Scroll:
                    _scrollPreview.InitUi(this);
                    break;

                // ジャンプ
                case PreviewId.Jump:
                    _jumpPreview.InitUi(this);
                    rootVisualElement.Add(_jumpPreview.CreateUi());
                    break;

                //ルート指定
                case PreviewId.Route:
                    _routePreview.InitUi(this);
                    rootVisualElement.Add(_routePreview.CreateUi());
                    break;

                // アニメーション
                case PreviewId.Animation:
                    _animationPreview.InitUi(this);
                    rootVisualElement.Add(_animationPreview.CreateUi());
                    break;

                // カスタム移動
                case PreviewId.CustomMove:
                    _customMovePreview.InitUi(this);
                    rootVisualElement.Add(_customMovePreview.CreateUi());
                    break;
            }
            _lastTime = EditorApplication.timeSinceStartup;
        }

        private void Update() {
            var elapsedTime = EditorApplication.timeSinceStartup - _lastTime;
            if (elapsedTime < 1f / Runtime.Common.Commons.Fps)
            {
                return;
            }

            int count = 1 + (int)(elapsedTime / (1f / Runtime.Common.Commons.Fps) - 1);
            _lastTime = EditorApplication.timeSinceStartup;

            for (int i = 0; i < count; i++)
            {
                switch (_previewId)
                {
                    // タイトル
                    case PreviewId.Title:
                        break;

                    // 素材管理
                    case PreviewId.AssetManage:
                        if (_previewSceneElement == null) break;
                        if (EditorApplication.isPlaying)
                        {
                            _previewSceneElement.SetEnabled(false);
                        }
                        else
                        {
                            _previewSceneElement.SetEnabled(true);
                            _assetManagePreview.Update();
                            Render();
                            Repaint();
                        }
                        break;

                    // メニュー
                    case PreviewId.Menu:
                        break;

                    // バトル
                    case PreviewId.Battle:
                        break;

                    // バトルシーン (背景と敵)
                    case PreviewId.BattleScene:
                        break;

                    // 会話ウィンドウ
                    case PreviewId.TalkWindow:
                        break;

                    // ジャンプ
                    case PreviewId.Jump:
                        _jumpPreview?.Update();
                        break;

                    //ルート指定
                    case PreviewId.Route:
                        _routePreview?.Update();
                        break;

                    // アニメーション
                    case PreviewId.Animation:
                        _animationPreview?.Update();
                        Render();
                        Repaint();
                        break;

                    // カスタム移動
                    case PreviewId.CustomMove:
                        _customMovePreview?.Update();
                        Render();
                        Repaint();
                        break;
                }
            }
        }

        public void Clear() {
            if (_previewSceneElement != null) _previewSceneElement.Dispose();

            rootVisualElement.Clear();
            titleContent = new GUIContent(EditorLocalize.LocalizeText("WORD_1563") + " " +
                                          EditorLocalize.LocalizeText("WORD_1581"));

            if (_previewId != PreviewId.Title && _titlePreview != null)
            {
                _titlePreview.DestroyLocalData();
                _titlePreview = null;
            }

            if (_previewId != PreviewId.AssetManage && _assetManagePreview != null)
            {
                _assetManagePreview.DestroyLocalData();
                _assetManagePreview = null;
            }

            if (_previewId != PreviewId.Menu && _menuPreview != null)
            {
                _menuPreview.DestroyLocalData();
                _menuPreview = null;
            }

            if (_previewId != PreviewId.Battle && _battlePreview != null)
            {
                _battlePreview.DestroyLocalData();
                _battlePreview = null;
            }

            if (_previewId != PreviewId.TalkWindow && _talkWindowPreview != null)
            {
                _talkWindowPreview.DestroyLocalData();
                _talkWindowPreview = null;
            }

            if (_previewId != PreviewId.Scroll && _scrollPreview != null)
            {
                _scrollPreview.DestroyLocalData();
                _scrollPreview = null;
            }

            if (_previewId != PreviewId.Jump && _jumpPreview != null)
            {
                _jumpPreview.DestroyLocalData();
                _jumpPreview = null;
            }

            if (_previewId != PreviewId.Route && _routePreview != null)
            {
                _routePreview.DestroyLocalData();
                _routePreview = null;
            }

            if (_previewId != PreviewId.Animation && _animationPreview != null)
            {
                _animationPreview.DestroyLocalData();
                _animationPreview = null;
            }

            if (_previewId != PreviewId.CustomMove && _customMovePreview != null)
            {
                _customMovePreview.DestroyLocalData();
                _customMovePreview = null;
            }
        }

        public void SetRenderingSize(int x, int y) {
            _previewSceneElement.RenderTextureSize = new Vector2Int(x, y);
        }

        public Vector2Int GetRenderingSize() {
            return _previewSceneElement.RenderTextureSize;
        }

        /// <summary>
        /// ゲームオブジェクトを現在属しているシーンからプレビューシーンに移動させる。
        /// </summary>
        /// <param name="go">ゲームオブジェクト</param>
        public void MoveGameObjectToPreviewScene(GameObject go) {
            _previewSceneElement.MoveGameObjectToPreviewScene(go);
        }

        public void Render() {
            switch (_previewId)
            {
                // タイトル
                case PreviewId.Title:
                    // タイトルの場合、一定時間後に再描画する
                    if (!_isReRendering)
                    {
                        ReRender();
                        _isReRendering = true;
                    }
                    _titlePreview.Render();
                    break;

                // 素材管理
                case PreviewId.AssetManage:
                    break;

                // ゲームメニュー
                case PreviewId.Menu:
                    _menuPreview.Render();
                    break;

                // バトル
                case PreviewId.Battle:
                    _battlePreview.Render();
                    break;

                // バトルシーン (背景と敵)
                case PreviewId.BattleScene:
                    _battleScenePreview.Render();
                    break;

                // 会話ウィンドウ
                case PreviewId.TalkWindow:
                    _talkWindowPreview.Render();
                    break;

                // アニメーション
                case PreviewId.Animation:
                    break;
            }

            rootVisualElement.Add(_previewSceneElement);
            _previewSceneElement?.Render();
        }

        /// <summary>
        /// 一定時間後に再描画を行う
        /// タイトル画面のメニュー部分が、稀に崩れたまま表示されてしまう問題への対応
        /// </summary>
        public async void ReRender(int time = 500) {
            await Task.Delay(time);
            if (_titlePreview != null)
            {
                _titlePreview.Render();
                _previewSceneElement?.Render();
                _isReRendering = false;
            }
        }

        /**
         * 表示刷新処理
         */
        private IEnumerator RenderingSizeRefresh() {
            if (rootVisualElement == null || rootVisualElement.layout.width == float.NaN)
                yield return null;

            yield return new WaitForSeconds(WAIT_TIME);

            // Yの方が大きい
            if (rootVisualElement.layout.width / 16 < rootVisualElement.layout.height / 9)
                SetRenderingSize((int) rootVisualElement.layout.width,
                    (int) rootVisualElement.layout.width * 9 / 16);
            else
                SetRenderingSize((int) rootVisualElement.layout.height / 9 * 16,
                    (int) rootVisualElement.layout.height);
            Render();
        }
    }
}