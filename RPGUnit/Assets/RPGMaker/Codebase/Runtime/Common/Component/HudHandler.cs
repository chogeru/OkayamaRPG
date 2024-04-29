using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Runtime.Common.Component.Hud;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Display;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.GameProgress;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Message;
using RPGMaker.Codebase.Runtime.Common.Enum;
using System;
using System.Collections.Generic;
using UnityEngine;
using Display = RPGMaker.Codebase.Runtime.Common.Component.Hud.Display.Display;
using Object = UnityEngine.Object;

namespace RPGMaker.Codebase.Runtime.Common.Component
{
    /**
     * HUD（ヘッドアップディスプレイ）系のUIを制御するクラス
     * たとえばメッセージウィンドウの処理などをこのクラスで行う
     */
    public class HudHandler
    {
        // データプロパティ
        //--------------------------------------------------------------------------------------------------------------
        DatabaseManagementService _databaseManagementService = new DatabaseManagementService();

        // 表示要素プロパティ
        //--------------------------------------------------------------------------------------------------------------
        private GameObject            _rootGameObject;
        private MessageWindow         _messageWindow;
        private int                   _messageWindowPosition;
        private MessageInputNumber    _inputNumWindow;
        private ItemWindow            _itemWindow;
        private MessageInputSelect    _inputSelectWindow;
        private MessageTextScroll     _messageTextScroll;
        private MapChangeName         _mapChangeName;
        
        private Picture               _picture;
        private Movie                 _movie;
        private Display               _display;
        
        private Display               _sceneDisplay;

        private GameTimer _gameTimer;
        private GameObject _timerObject;

        // methods
        //--------------------------------------------------------------------------------------------------------------
        /**
         * コンストラクタ
         */
        public HudHandler(GameObject rootGameObject) {
            _rootGameObject = rootGameObject;
        }
        
        //全てに対し削除の実施
        //rootとmessageを除く
        public void AllDestroy() {
            if(_inputNumWindow != null)
                Object.Destroy(_inputNumWindow.gameObject);
            if(_itemWindow != null)
                Object.Destroy(_itemWindow.gameObject);
            if (_inputSelectWindow != null)
            {
                Object.Destroy(_inputSelectWindow.gameObject);
                _inputSelectWindow = null;
            }
            if(_messageTextScroll != null)
                Object.Destroy(_messageTextScroll.gameObject);
            if(_mapChangeName != null)
                Object.Destroy(_mapChangeName.gameObject);
            if(_picture != null)
                Object.Destroy(_picture.gameObject);
            if(_movie != null)
                Object.Destroy(_movie.gameObject);
            if(_display != null)
                Object.Destroy(_display.gameObject);
        }

        // メッセージウィンドウ制御
        //--------------------------------------------------------------------------------------------------------------
        public bool IsMessageWindowActive() {
            return _messageWindow != null;
        }

        public void OpenMessageWindow() {
            if (IsMessageWindowActive())
            {
                CloseMessageWindow();
            }
            _messageWindow = (new GameObject()).AddComponent<MessageWindow>();
            _messageWindow.Init();
            _messageWindow.transform.position = new Vector3(0, 0, -9);
            _messageWindow.gameObject.transform.SetParent(_rootGameObject.transform);
        }

        public void CloseMessageWindow() {
            if (_messageWindow == null) return;
            _messageWindow.Destroy();
            Object.Destroy(_messageWindow.gameObject);
            _messageWindow = null;
        }
        
        public void ShowFaceIcon(string iconPath) {
            _messageWindow?.ShowFaceIcon(iconPath);
        }

        public void ShowName(string actorName) {
            _messageWindow?.ShowName(actorName);
        }

        public void ShowPicture(string pictureName) {
            _messageWindow?.ShowPicture(pictureName);
        }

        public void SetShowMessage(string message) {
            _messageWindow?.ShowMessage(message);
        }

        public void SetMessageWindowColor(int kind) {
            _messageWindow?.SetWindowColor(kind);
        }

        public void SetMessageWindowPos(int kind) {
            _messageWindowPosition = kind;
            _messageWindow?.SetWindowPos(kind);
        }

        public int GetMessageWindowPos() {
            return _messageWindowPosition;
        }

        public void Next() {
            _messageWindow.Next();
        }

        public void NextMessage(Action action) {
            if (!IsMessageWindowActive())
                return;

            if ( _messageWindow.NextMessage())
            {
                action.Invoke();
                CloseMessageWindow();
            }
        }

        public bool IsInputWait() {
            return _messageWindow?.IsWait() ?? false;
        }

        public bool IsInputEnd() {
            return _messageWindow?.IsEnd() ?? false;
        }

        public bool IsNotWaitInput() {
            return _messageWindow?.IsNotWaitInput() ?? false;
        }

        public void SetIsNotWaitInput(bool flg) {
            _messageWindow?.SetIsNotWaitInput(flg);
        }

        // 桁数入力ウィンドウ制御
        //--------------------------------------------------------------------------------------------------------------
        public bool IsInputNumWindowActive() {
            return _inputNumWindow != null;
        }

        public MessageInputNumber OpenInputNumWindow(string numDigits) {
            if (IsInputNumWindowActive())
            {
                CloseInputNumWindow();
            }

            _inputNumWindow = (new GameObject()).AddComponent<MessageInputNumber>();
            _inputNumWindow.Init(int.Parse(numDigits));
            _inputNumWindow.transform.position = new Vector3(0, 0, -9);
            _inputNumWindow.gameObject.transform.SetParent(_rootGameObject.transform);
            return _inputNumWindow;
        }

        public void CloseInputNumWindow() {
            if (_inputNumWindow == null) return;

            Object.Destroy(_inputNumWindow.gameObject);
            _inputNumWindow = null;
        }

        public int InputNumWindowOperation(HandleType type) {
            return _inputNumWindow.Process(type);
        }
        
        //数値入力の今の値
        public int InputNumNumber() {
            return _inputNumWindow.GetNowNumber();
        }

        // 所持アイテムウィンドウ制御
        //--------------------------------------------------------------------------------------------------------------
        public bool IsItemWindowActive() {
            return _itemWindow != null;
        }

        public void OpenItemWindow() {
            if (IsItemWindowActive())
            {
                CloseItemWindow();
            }

            _itemWindow = (new GameObject()).AddComponent<ItemWindow>();
            _itemWindow.Init();
            _itemWindow.transform.position = new Vector3(0, 0, -9);
            _itemWindow.gameObject.transform.SetParent(_rootGameObject.transform);
        }

        public void CloseItemWindow() {
            if (_itemWindow == null) return;

            Object.Destroy(_itemWindow.gameObject);
            _itemWindow = null;
        }

        // 所持アイテムウィンドウ制御
        //--------------------------------------------------------------------------------------------------------------
        public bool IsInputSelectWindowActive() {
            return _inputSelectWindow != null;
        }

        public MessageInputSelect OpenInputSelectWindow() {
            if (IsInputSelectWindowActive())
            {
                CloseInputSelectWindow();
            }

            _inputSelectWindow = (new GameObject()).AddComponent<MessageInputSelect>();
            _inputSelectWindow.Init();
            _inputSelectWindow.transform.position = new Vector3(0, 0, -9);
            _inputSelectWindow.gameObject.transform.SetParent(_rootGameObject.transform);
            return _inputSelectWindow;
        }

        public void CloseInputSelectWindow() {
            if (_inputSelectWindow == null) return;

            Object.Destroy(_inputSelectWindow.gameObject);
            _inputSelectWindow = null;
        }

        public void SetInputSelectWindowColor(int kind) {
            _inputSelectWindow.SetWindowColor(kind);
        }

        public void ActiveSelectFrame(string text) {
            _inputSelectWindow.ActiveSelectFrame(text);
        }

        public int GetSelectNum() {
            return _inputSelectWindow.GetSelectNum();
        }

        public void SetInputSelectWindowPos(int kind) {
            RectTransform Frame = _inputSelectWindow.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>();
            switch (kind)
            {
                //左
                case 0:
                    Frame.anchorMax = new Vector2(0.0f,0.5f);
                    Frame.anchorMin = new Vector2(0.0f,0.5f); 
                    Frame.pivot = new Vector2(0.0f, 1.0f);
                    break;
                //中
                case 1:
                    Frame.anchorMax = new Vector2(0.5f,0.5f);
                    Frame.anchorMin = new Vector2(0.5f,0.5f);
                    Frame.pivot = new Vector2(0.5f, 1.0f);
                    break;
                //右、その他
                case 2:
                default:
                    Frame.anchorMax = new Vector2(1.0f,0.5f);
                    Frame.anchorMin = new Vector2(1.0f,0.5f);
                    Frame.pivot = new Vector2(1.0f, 1.0f);
                    break;
            }

            int y = 195;
            // 文章表示位置によってY座標を設定
            if (HudDistributor.Instance.NowHudHandler().IsMessageWindowActive())
                if (HudDistributor.Instance.NowHudHandler().GetMessageWindowPos() == 0)
                    y = 195;
                else if (HudDistributor.Instance.NowHudHandler().GetMessageWindowPos() == 1)
                    y = -145;
                else
                {
                    y = -185;
                    Frame.pivot = new Vector2(Frame.pivot.x, 0.0f);
                }

            //positionが残るため更新
            _inputSelectWindow.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition = 
                new Vector3(
                    0f,
                    y
                );

            //以下はちらつき防止処理
            _inputSelectWindow.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>().localScale = new Vector3(0.0f, 0.0f, 0.0f);
            TimeHandler.Instance.AddTimeAction(0.1f, SetInputSelectWindowPosAft, false);
        }

        private void SetInputSelectWindowPosAft() {
            if (_inputSelectWindow != null)
                _inputSelectWindow.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }

        public void MoveCorsor(HandleType type) {
            _inputSelectWindow?.Process(type);
        }

        // メッセージスクロール
        //--------------------------------------------------------------------------------------------------------------
        public bool IsMessageScrollWindowActive() {
            return _messageTextScroll != null;
        }

        public void OpenMessageScrollWindow() {
            if (IsMessageScrollWindowActive())
            {
                CloseMessageScrollWindow();
            }

            _messageTextScroll = (new GameObject()).AddComponent<MessageTextScroll>();
            _messageTextScroll.Init();
            _messageTextScroll.transform.position = new Vector3(0, 0, -9);
            _messageTextScroll.gameObject.transform.SetParent(_rootGameObject.transform);
        }

        public void CloseMessageScrollWindow() {
            if (_messageTextScroll == null) return;

            Object.Destroy(_messageTextScroll.gameObject);
            _messageTextScroll = null;
        }

        public void SetScrollSpeed(int speed) {
            _messageTextScroll?.SetSpeed(speed);
        }

        public void StartScroll(Action action) {
            _messageTextScroll.StartScroll(action);
        }

        public void SetScrollText(string text) {
            _messageTextScroll.SetScrollText(text);
        }
        public void SetScrollNoFast(bool flg) {
            _messageTextScroll.SetScrollNoFast(flg);
        }

        // ピクチャー関係の処理
        //--------------------------------------------------------------------------------------------------------------
        public bool IsPictureActive() {
            return _picture != null;
        }

        public void PictureInit() {
            if (!IsPictureActive())
            {
                _picture = (new GameObject()).AddComponent<Picture>();
                _picture.Init();
            }
        }

        public void AddPicture(int pictureNumber, string pictureName) {
            _picture.AddPicture(pictureNumber, pictureName);
        }

        public void AddPictureParameter(int pictureNumber, List<string> parameters) {
            _picture.AddPictureParameter(pictureNumber, parameters);
        }

        public UnityEngine.UI.Image GetPicture(int pictureNumber) {
            return _picture?.GetPicture(pictureNumber);
        }

        public void SetPivot(int pictureNumber, int pivot) {
            _picture.SetPivot(pictureNumber, pivot);
        }
        
        public void SetAnchor(int pictureNumber, int anchor) {
            _picture.SetAnchor(pictureNumber, anchor);
        }

        public void SetPosition(int pictureNumber, int type, string x, string y) {
            _picture.SetPosition(pictureNumber, type, x, y);
        }

        public void SetPictureSize(int pictureNumber, int widthDiameter, int heightDiameter) {
            _picture.SetPictureSize(pictureNumber, widthDiameter, heightDiameter);
        }

        public void PlayPictureSize(int pictureNumber, int frame, int widthDiameter, int heightDiameter) {
            _picture.StartChangeSize(pictureNumber, frame, widthDiameter, heightDiameter);
        }

        public void SetPictureOpacity(int pictureNumber, int opacity) {
            _picture.SetPictureOpacity(pictureNumber, opacity);
        }

        public void SetProcessingType(int pictureNumber, int processingType) {
            _picture.SetProcessingType(pictureNumber, processingType);
        }

        public void PlayMove(Action action, int pictureNumber, int moveType, int type, string x, string y, int flame, bool toggle) {
            _picture.StartMove(action, pictureNumber, moveType, type, x, y, flame, toggle);
        }

        public void PlayRotation(int pictureNumber, int rotation) {
            _picture.StartRotation(pictureNumber, rotation);
        }

        public void PlayChangeColor(Action action, Color color, int pictureNumber, float gray, int flame, bool toggle) {
            _picture.StartChangeColor(action, color, pictureNumber, gray, flame, toggle);
        }

        public void DeletePicture(int pictureNumber) {
            _picture.DeletePicture(pictureNumber);
        }

        public void SavePicture() {
            if (IsPictureActive())
            {
                _picture.SavePicture();
            }
        }

        public void LoadPicture() {
            PictureInit();
            _picture.LoadPicture();
        }

        // 画面関係の処理
        //--------------------------------------------------------------------------------------------------------------
        public bool IsDisplayActive() {
            return _display != null;
        }

        public void DisplayInit() {
            if (!IsDisplayActive())
            {
                _display = Display.CreateDisplay();
                _display.Init();
            }
        }

        public void DisplayInitByScene() {
            if (_sceneDisplay == null)
            {
                _sceneDisplay = Display.CreateDisplayByScene();
                _sceneDisplay.Init();
                UnityEngine.Object.DontDestroyOnLoad(_sceneDisplay);
            }
        }

        public GameTimer CreateGameTimer() {
            if (_gameTimer == null)
            {
                _gameTimer = new GameObject().AddComponent<GameTimer>();
                _gameTimer.Init();
            }
            return _gameTimer;
        }

        public GameTimer GetGameTimer() {
            return _gameTimer;
        }

        public GameObject TimerInitObject() {
            if (_timerObject == null)
            {
                _timerObject = Display.CreateTimerObject();
                _timerObject.name = "Timer";
                UnityEngine.Object.DontDestroyOnLoad(_timerObject);
            }
            return _timerObject;
        }
        
        public void HideFadeImage() {
            _display.HideFadeImage();
        }
        public void FadeOut(Action action, Color fadeColor, float time = 0.5f, bool isScene = false) {
            if (!isScene)
            {
                _display.StartFadeOut(action, fadeColor, time);
            }
            else
            {
                _sceneDisplay.StartFadeOut(action, fadeColor, time);
            }
        }
        public void FadeIn(Action action, bool isInitialize = false, float time = 0.5f, bool isScene = false) {
            if (!isScene)
            {
                _display.StartFadeIn(action, isInitialize, time);
            }
            else
            {
                _sceneDisplay.StartFadeIn(action, isInitialize, time);
            }
        }
        
        /// <summary>
        /// 主にシーン遷移専用。かならず画像を塗り潰してからフェードインする
        /// </summary>
        /// <param name="action"></param>
        /// <param name="isInitialize"></param>
        /// <param name="time"></param>
        /// <param name="isScene"></param>
        public void FadeInFixedBlack(Action action, bool isInitialize = false, float time = 0.5f, bool isScene = false) {
            if (!isScene)
            {
                _display.StartFadeIn(action, isInitialize, time);
            }
            else
            {
                _sceneDisplay.SetFadeImageColor(UnityEngine.Color.black);
                _sceneDisplay.StartFadeIn(action, isInitialize, time);
            }
        }
        
        
        public void ChangeColor(Action action, Color color, float gray, float flame, bool wait) {
            _display.DisplayChangeColor(action, color, gray, flame, wait);
        }
        public void Flash(Action action, Color color, int gray, int flame, bool wait, string evetId) {
            _display.DisplayFlash(action, color, gray, flame, wait, evetId);
        }
        public void Shake(Action action, int intensity, int speed, int flame, bool wait) {
            _display.DisplayShake(action, intensity, speed, flame, wait);
        }
        public void ChangeWeather(Action action, int type, int value, float flame, bool wait) {
            _display.DisplayWeather(action, type, value, flame, wait);
        }
        //マップ名表示/非表示
        //---------------------------------------------------------------------------
        public void PlayChangeMapName() {
            //表示前に前のものが残っていたら削除
            ClosePlayChangeMapName();
            
            _mapChangeName = new GameObject().AddComponent<MapChangeName>();
            _mapChangeName.GetComponent<MapChangeName>().Init();
        }
        
        public void ClosePlayChangeMapName() {
            if (_mapChangeName == null) return;

            Object.Destroy(_mapChangeName.gameObject);
            _mapChangeName = null;
        }

        // ムービー関係の処理
        //--------------------------------------------------------------------------------------------------------------
        public bool IsMovieActive() {
            return _movie != null;
        }

        public void MovieInit() {
            if (!IsMovieActive())
            {
                _movie = (new GameObject()).AddComponent<Movie>();
                _movie.Init();
            }
        }

        public void AddMovie(string movieName, Action callBack) {
            _movie.AddMovie(movieName, callBack);
        }
    }
}