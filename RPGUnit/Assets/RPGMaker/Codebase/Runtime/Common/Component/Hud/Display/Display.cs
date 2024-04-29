using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.Runtime.Battle;
using RPGMaker.Codebase.Runtime.Map;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.ParticleSystem;

namespace RPGMaker.Codebase.Runtime.Common.Component.Hud.Display
{
    /// <summary>
    ///     画面演出の実行部分を管理するコンポーネント
    /// </summary>
    public class Display : MonoBehaviour
    {
        // const
        //--------------------------------------------------------------------------------------------------------------
        private const string PrefabPath =
            "Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/Display.prefab";
        private const string PrefabByScenePath =
            "Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/DisplayByScene.prefab";
        
        private const string TimerPrefabPath =
            "Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/GameTimer.prefab";


        // データプロパティ
        //--------------------------------------------------------------------------------------------------------------
        private                  GameObject _currentWeatherObject;
        [SerializeField] private Image      _displayImage;

        // 関数プロパティ
        //--------------------------------------------------------------------------------------------------------------
        private Action _endAction;
        private readonly Dictionary<string, FlashData> _flashTimeDictionary = new Dictionary<string, FlashData>();
        [SerializeField] private Image _fadeImage;
        [SerializeField] private Image _flashImage;
        [SerializeField] private ParticleSystem _rain;
        private IEnumerator _shakeEnumerator;
        private IEnumerator _shakeEnumeratorBattle;
        [SerializeField] private ParticleSystem _snow;
        [SerializeField] private GameObject _storm;
        [SerializeField] private ParticleSystem _stormRain1;
        [SerializeField] private ParticleSystem _stormRain2;

        // 表示要素プロパティ
        //--------------------------------------------------------------------------------------------------------------
        [SerializeField] private Canvas _targetCanvas;

        //画面の色調変更用の変数
        private Color _nowColor;
        private Color _targetColor;
        private float _nowGray;
        private float _targetGray;
        private float _time;
        private float _nowTime;
        private bool  _isInitialize = false;
        private bool  _isCover      = false;

        /// <summary>
        ///     画面演出用Prefabの生成
        /// </summary>
        /// <returns>生成されたPrefabにアタッチされているDisplayコンポーネント</returns>
        public static Display CreateDisplay() {
            var loadPrefab = UnityEditorWrapper.PrefabUtilityWrapper.LoadPrefabContents(PrefabPath);
            var obj = Instantiate(loadPrefab);
            UnityEditorWrapper.PrefabUtilityWrapper.UnloadPrefabContents(loadPrefab);
            return obj.GetComponent<Display>();
        }

        public static Display CreateDisplayByScene() {
            var loadPrefab = UnityEditorWrapper.PrefabUtilityWrapper.LoadPrefabContents(PrefabByScenePath);
            var obj = Instantiate(loadPrefab);
            UnityEditorWrapper.PrefabUtilityWrapper.UnloadPrefabContents(loadPrefab);
            return obj.GetComponent<Display>();
        }

        public static GameObject CreateTimerObject() {
            var loadPrefab = UnityEditorWrapper.PrefabUtilityWrapper.LoadPrefabContents(TimerPrefabPath);
            var obj = Instantiate(loadPrefab);
            UnityEditorWrapper.PrefabUtilityWrapper.UnloadPrefabContents(loadPrefab);
            
            return obj;
        }

        public void Init() {
            _targetCanvas.worldCamera = Camera.main;
            _displayImage.color = Color.clear;
            _fadeImage.color = Color.clear;
            _flashImage.color = Color.clear;
        }

        private void OnDestroy() {
            TimeHandler.Instance?.RemoveTimeAction(FadeOutCoroutine);
            TimeHandler.Instance?.RemoveTimeAction(FadeInCoroutine);
            TimeHandler.Instance?.RemoveTimeAction(ChangeImageColorProcess);
            TimeHandler.Instance?.RemoveTimeAction(FlashCoroutine);
        }

        //フェードイメージの透明度を上げて非表示にする
        public void HideFadeImage() {
            var beforeColor = _fadeImage.GetComponent<Image>().color;
            _fadeImage.GetComponent<Image>().color = new Color(beforeColor.r, beforeColor.g, beforeColor.b, 0);
        }

        public void StartFadeOut(Action action, Color fadeColor, float time) {
            if (_isCover)
            {
                action?.Invoke();
                return;
            }

            _endAction = action;
            _nowTime = 0;
            _time = time;

            //フェードアウトの色指定
            _fadeImage.GetComponent<Image>().color = fadeColor;

            //初期化処理
            _fadeImage.GetComponent<Image>().material.color = new Color(1, 1, 1, 1);
            var color = _fadeImage.color;
            color.a = 0f;

            //フレーム単位での処理
            TimeHandler.Instance.AddTimeActionEveryFrame(FadeOutCoroutine);
        }

        private void FadeOutCoroutine() {
            _nowTime += Time.deltaTime;
            _fadeImage.gameObject.SetActive(true);

            var color = _fadeImage.color;
            if (_nowTime >= _time)
            {
                color.a = 1f;
                _fadeImage.color = color;
                _isCover = true;
                TimeHandler.Instance.RemoveTimeAction(FadeOutCoroutine);
                TimeHandler.Instance.AddTimeActionFrame(1, EndFade, false);
            }
            else
            {
                color.a = (_nowTime / _time);
                _fadeImage.color = color;
            }
        }

        public void StartFadeIn(Action action, bool isInitialize, float time) {
            if (!_isCover && !isInitialize)
            {
                action?.Invoke();
                return;
            }

            _endAction = action;
            _nowTime = 0;
            _time = time;

            _fadeImage.GetComponent<Image>().material.shader = Shader.Find("UI/Default");

            //初期化処理
            _fadeImage.GetComponent<Image>().material.color = new Color(1, 1, 1, 1);
            var color = _fadeImage.color;
            color.a = 1f;
            _fadeImage.color = color;

            //フレーム単位での処理
            TimeHandler.Instance.AddTimeActionEveryFrame(FadeInCoroutine);
        }

        /// <summary>
        /// フェード画像を塗る
        /// </summary>
        /// <param name="fadeColor"></param>
        public void SetFadeImageColor(Color fadeColor) {
            if (!_isCover)
            {
                _fadeImage.GetComponent<Image>().color = fadeColor;
                _isCover = true;
            }
        }

        private void FadeInCoroutine() {
            _nowTime += Time.deltaTime;
            _fadeImage.gameObject.SetActive(true);

            var color = _fadeImage.color;
            if (_nowTime >= _time)
            {
                color.a = 0f;
                _fadeImage.color = color;
                _isCover = false;
                TimeHandler.Instance.RemoveTimeAction(FadeInCoroutine);
                _fadeImage.gameObject.SetActive(false);
                TimeHandler.Instance.AddTimeActionFrame(1, EndFade, false);
            }
            else
            {
                color.a = 1.0f - (_nowTime / _time);
                _fadeImage.color = color;
            }
        }

        private void EndFade() {
            var collBack = _endAction;
            _endAction = null;
            collBack?.Invoke();
        }

        public void DisplayChangeColor(Action action, Color color, float gray, float frame, bool wait) {
            _endAction = action;
            _displayImage.gameObject.SetActive(true);

            if (!_isInitialize)
            {
                _isInitialize = true;
                _nowColor = new Color(0f, 0f, 0f, 0f);
                _displayImage.color = new Color(0f, 0f, 0f, 0f);
                _nowGray = 0f;
            }

            _targetColor = color;
            _targetGray = gray;
            _time = frame / 60.0f;
            _nowTime = 0.0f;

            //フレーム単位での処理
            TimeHandler.Instance.AddTimeActionEveryFrame(ChangeImageColorProcess);

            if (!wait)
            {
                var collBack = _endAction;
                _endAction = null;
                collBack?.Invoke();
            }

            //セーブデータにも保存
            DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.tone = new List<int>();
            DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.tone.Add((int) color.r);
            DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.tone.Add((int) color.g);
            DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.tone.Add((int) color.b);
            DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.tone.Add((int) _targetGray);
        }

        /// <summary>
        ///     フラッシュ演出の実施
        /// </summary>
        /// <param name="callback">実行後のコールバック</param>
        /// <param name="color">フラッシュの色</param>
        /// <param name="alpha">フラッシュの透明度</param>
        /// <param name="frame">フラッシュが消えるまでの時間</param>
        /// <param name="wait">callback実行をフラッシュ完了まで待つかどうか</param>
        /// <param name="eventId">実行したときのイベントID</param>
        public void DisplayFlash(Action callback, Color color, float alpha, int frame, bool wait, string eventId) {
            _flashImage.gameObject.SetActive(true);
            _flashImage.material.shader = Shader.Find("UI/Default");
            var flashColor = new Color(color.r / 255, color.g / 255, color.b / 255, alpha / 255);
            _flashImage.color = flashColor;

            if (_flashTimeDictionary.ContainsKey(eventId))
            {
                _flashTimeDictionary[eventId].Action = callback;
                _flashTimeDictionary[eventId].Frame = frame / 60.0f;
                _flashTimeDictionary[eventId].NowTime = 0f;
                _flashTimeDictionary[eventId].Color = flashColor;
            }
            else
            {
                _flashTimeDictionary.Add(eventId, new FlashData(callback, 0f, frame / 60f, flashColor));
            }
            
            //フレーム単位での処理
            TimeHandler.Instance.AddTimeActionEveryFrame(FlashCoroutine);

            if (!wait)
            {
                if (_flashTimeDictionary.ContainsKey(eventId))
                {
                    Action action = _flashTimeDictionary[eventId].Action;
                    _flashTimeDictionary[eventId].Action = null;
                    action?.Invoke();
                }
            }
        }

        /// <summary>
        ///     フラッシュ演出の実行部分
        /// </summary>
        private void FlashCoroutine() {
            var timeKeys = _flashTimeDictionary.Keys.ToList();
            for (int i = 0; i < timeKeys.Count; i++)
            {
                var key = timeKeys[i];
                if (_flashTimeDictionary[key].NowTime >= _flashTimeDictionary[key].Frame)
                {
                    _flashImage.gameObject.SetActive(false);
                    Action action = _flashTimeDictionary[key].Action;
                    _flashTimeDictionary.Remove(key);
                    action?.Invoke();
                }
                else
                {
                    _flashTimeDictionary[key].NowTime += Time.deltaTime;
                    _flashImage.color = Color.Lerp(_flashTimeDictionary[key].Color, Color.clear, _flashTimeDictionary[key].NowTime / _flashTimeDictionary[key].Frame);
                }
            }

            if (_flashTimeDictionary.Count == 0)
            {
                TimeHandler.Instance.RemoveTimeAction(FlashCoroutine);
            }
        }

        /// <summary>
        ///     シェイク演出の実施
        /// </summary>
        /// <param name="callback">実行後のコールバック</param>
        /// <param name="intensity">揺れの強さ</param>
        /// <param name="speed">揺れの速度</param>
        /// <param name="flame">揺れが収まるまでの時間</param>
        /// <param name="wait">callback実行を演出完了まで待つかどうか</param>
        public void DisplayShake(Action callback, int intensity, int speed, int flame, bool wait) {
            if (GameStateHandler.IsMap())
            {
                if (_shakeEnumerator != null)
                {
                    StopCoroutine(_shakeEnumerator);
                    _shakeEnumerator = null;
                }

                _shakeEnumerator = ShakeCoroutine(callback, intensity, speed, flame, wait);
                StartCoroutine(_shakeEnumerator);
            }
            else
            {
                if (_shakeEnumeratorBattle != null)
                {
                    StopCoroutine(_shakeEnumeratorBattle);
                    _shakeEnumeratorBattle = null;
                }

                _shakeEnumeratorBattle = ShakeCoroutineBattle(callback, intensity, speed, flame, wait);
                StartCoroutine(_shakeEnumeratorBattle);
            }
        }

        /// <summary>
        ///     シェイク演出の実行部分
        /// </summary>
        private static float currentRad;
        private IEnumerator ShakeCoroutine(Action callback, int intensity, int speed, int flame, bool wait) {
            // 速度5（中央値）の場合に10フレームで中心→端→中心のサイクルを1回行うことを基準に計算（MVがだいたいそれくらいだった）
            const int defaultSpeed = 5, defaultIntensity = 5;
            var shackCount = Mathf.CeilToInt(flame / (10f * (defaultSpeed / (float) speed)));
            var rad = (shackCount * 180f + currentRad / (Mathf.PI * 2)) * Mathf.Deg2Rad;
            currentRad = currentRad % (Mathf.PI * 2);
            // 基準の計算の条件をもとに速度を出す
            var moveSpeed = 180f * Mathf.Deg2Rad / 10f * ((float) speed / defaultSpeed);
            Vector3 startPos;
            startPos = MapManager.GetCameraPosition() + new Vector3(0.5f, 0.5f, 0);
            var shookPosX = 0f;

            if (!wait) callback?.Invoke();
            while (!Mathf.Approximately(rad, currentRad))
            {
                //ステートがMAP、イベントでは無い場合は、処理しない
                if (!(GameStateHandler.CurrentGameState() == GameStateHandler.GameState.MAP ||
                    GameStateHandler.CurrentGameState() == GameStateHandler.GameState.EVENT))
                {
                    yield return null;
                    continue;
                }

                bool flg = false;
                try
                {
                    if (Camera.main == null) 
                        flg = true;
                } catch (Exception) { 
                    flg = true; 
                }
                if (flg)
                {
                    yield return null;
                    continue;
                }

                var sin = Mathf.Sin(currentRad);
                shookPosX = startPos.x + intensity / (float) defaultIntensity * sin;

                Camera.main.transform.localPosition = new Vector3(shookPosX, startPos.y, startPos.z);
                currentRad = Mathf.Min(rad, currentRad + (moveSpeed * Time.deltaTime * 60f));
                startPos = MapManager.GetCameraPosition() + new Vector3(0.5f, 0.5f, 0);

                yield return null;
            }

            if (wait) callback?.Invoke();

            Camera.main.transform.localPosition = startPos;
        }

        /// <summary>
        ///     シェイク演出の実行部分（バトル用）
        /// </summary>
        private static float currentRadBattle;
        private IEnumerator ShakeCoroutineBattle(Action callback, int intensity, int speed, int flame, bool wait) {
            // 速度5（中央値）の場合に10フレームで中心→端→中心のサイクルを1回行うことを基準に計算（MVがだいたいそれくらいだった）
            const int defaultSpeed = 5, defaultIntensity = 5;
            var shackCount = Mathf.CeilToInt(flame / (10f * (defaultSpeed / (float) speed)));
            var rad = (shackCount * 180f + currentRadBattle / (Mathf.PI * 2)) * Mathf.Deg2Rad;
            currentRadBattle = currentRadBattle % (Mathf.PI * 2);
            // 基準の計算の条件をもとに速度を出す
            var moveSpeed = 180f * Mathf.Deg2Rad / 10f * ((float) speed / defaultSpeed);
            Vector3 startPos;
            BattleManager.GetCanvas().renderMode = RenderMode.WorldSpace;
            startPos = BattleManager.GetCanvas().worldCamera.transform.position;
            var shookPosX = 0f;

            if (!wait) callback?.Invoke();
            while (!Mathf.Approximately(rad, currentRadBattle))
            {
                //ステートがバトルではなければ処理を終了する
                if (!GameStateHandler.IsBattle())
                {
                    yield break;
                }

                var cos = Mathf.Cos(currentRadBattle);
                shookPosX = startPos.x + (float) intensity / (float) defaultIntensity / (float) 10.0f * cos;
                BattleManager.GetCanvas().worldCamera.transform.position = new Vector3(shookPosX, startPos.y, startPos.z);

                currentRadBattle = Mathf.Min(rad, currentRadBattle + (moveSpeed * Time.deltaTime * 60f));
                startPos = BattleManager.GetCanvas().worldCamera.transform.position;

                yield return null;
            }

            BattleManager.GetCanvas().renderMode = RenderMode.ScreenSpaceCamera;
            BattleManager.GetCanvas().worldCamera.transform.position = new Vector3(0, startPos.y, startPos.z);

            if (wait)
            {
                yield return null;
                callback?.Invoke();
            }
        }

        public IEnumerator ChangeColorProcess(Color targetColor, float gray, float flame, bool wait, Image image) {
            float r = 255;
            float g = 255;
            float b = 255;
            image.GetComponent<Image>().material.color = new Color(r / 255, g / 255, b / 255, 1);
            var distanceA = Vector3.Distance(new Vector3(r, g, b),
                new Vector3(targetColor.r, targetColor.g, targetColor.b));
            var distanceG = Vector3.Distance(new Vector3(r, g, b),
                new Vector3(targetColor.r, targetColor.g, targetColor.b));
            var distanceB = Vector3.Distance(new Vector3(r, g, b),
                new Vector3(targetColor.r, targetColor.g, targetColor.b));

            var valueA = distanceA / flame * 0.1f;
            var valueG = distanceG / flame * 0.1f;
            var valueB = distanceB / flame * 0.1f;

            if (!wait)
            {
                var collBack = _endAction;
                _endAction = null;
                collBack?.Invoke();
            }
            while (true)
            {
                r = Mathf.Lerp(image.color.r, targetColor.r, valueA);
                g = Mathf.Lerp(image.color.g, targetColor.g, valueG);
                b = Mathf.Lerp(image.color.b, targetColor.b, valueB);
                image.GetComponent<Image>().material.color = new Color(r / 255, g / 255, b / 255, 1);
                image.color = new Color(r, g, b);
                if (Vector3.Distance(new Vector3(r, g, b), new Vector3(targetColor.r, targetColor.g, targetColor.b)) <
                    0.5f)
                {
                    image.color = targetColor;
                    image.GetComponent<Image>().material.color = new Color(r / 255, g / 255, b / 255, gray / 255.0f);
                    var collBack = _endAction;
                    _endAction = null;
                    collBack?.Invoke();
                    break;
                }

                yield return null;
            }
        }

        public void ChangeImageColorProcess() {
            Color color = _displayImage.color;

            if (_nowTime >= _time)
            {
                _displayImage.color = new Color(_targetColor.r / 255, _targetColor.g / 255, _targetColor.b / 255, _targetGray / 255.0f);
                _nowColor = new Color(_targetColor.r, _targetColor.g, _targetColor.b, _targetGray);
                _nowGray = _targetGray;
                TimeHandler.Instance.RemoveTimeAction(ChangeImageColorProcess);
                var collBack = _endAction;
                _endAction = null;
                collBack?.Invoke();
            }
            else
            {
                _nowTime += Time.deltaTime;
                _displayImage.color = new Color(
                    ((_targetColor.r - _nowColor.r) * (_nowTime / _time) + _nowColor.r) / 255,
                    ((_targetColor.g - _nowColor.g) * (_nowTime / _time) + _nowColor.g) / 255,
                    ((_targetColor.b - _nowColor.b) * (_nowTime / _time) + _nowColor.b) / 255,
                    ((_targetGray - _nowGray) * (_nowTime / _time) + _nowGray) / 255.0f);
            }
        }


        /// <summary>
        ///     天候演出の実施
        /// </summary>
        /// <param name="callback">実行後のコールバック</param>
        /// <param name="type">天候の種類</param>
        /// <param name="value">天候の勢い</param>
        /// <param name="flame">完了まで待つ場合の待機フレーム数</param>
        /// <param name="wait">callback実行を演出完了まで待つかどうか</param>
        public void DisplayWeather(Action callback, int type, int value, float frame, bool wait) {
            _currentWeatherObject = null;

            // パーティクルの生成量を設定。直前まで非アクティブだった場合はパーティクルの生成を一旦止める
            EmissionModule emission;
            switch (type)
            {
                case 1:
                    emission = _rain.emission;
                    emission.rateOverTime = new MinMaxCurve(10 * value);

                    _currentWeatherObject = _rain.gameObject;
                    if (!_rain.gameObject.activeSelf)
                        _rain.Stop();
                    break;
                case 2:
                    emission = _stormRain1.emission;
                    emission.rateOverTime = new MinMaxCurve(10 * value);
                    emission = _stormRain2.emission;
                    emission.rateOverTime = new MinMaxCurve(10 * value);

                    _currentWeatherObject = _storm.gameObject;
                    if (!_storm.gameObject.activeSelf)
                    {
                        _stormRain1.Stop();
                        _stormRain2.Stop();
                    }

                    break;
                case 3:
                    emission = _snow.emission;
                    emission.rateOverTime = new MinMaxCurve(10 * value);

                    _currentWeatherObject = _snow.gameObject;
                    if (!_snow.gameObject.activeSelf)
                        _snow.Stop();
                    break;
            }

            // 天気の種類に対応したオブジェクトを有効にする
            _rain.gameObject.SetActive(type == 1);
            _storm.gameObject.SetActive(type == 2);
            _snow.gameObject.SetActive(type == 3);
            
            _rain.GetComponent<ParticleSystemRenderer>().sortingLayerID = UnityUtil.SortingLayerManager.GetId(UnityUtil.SortingLayerManager.SortingLayerIndex.Runtime_Weather);
            _stormRain1.GetComponent<ParticleSystemRenderer>().sortingLayerID = UnityUtil.SortingLayerManager.GetId(UnityUtil.SortingLayerManager.SortingLayerIndex.Runtime_Weather);
            _stormRain2.GetComponent<ParticleSystemRenderer>().sortingLayerID = UnityUtil.SortingLayerManager.GetId(UnityUtil.SortingLayerManager.SortingLayerIndex.Runtime_Weather);
            _snow.GetComponent<ParticleSystemRenderer>().sortingLayerID = UnityUtil.SortingLayerManager.GetId(UnityUtil.SortingLayerManager.SortingLayerIndex.Runtime_Weather);
            _time = frame / 60.0f;
            _nowTime = 0.0f;

            //フレーム単位での処理
            if (!wait || frame == 0)
            {
                callback?.Invoke();
            }
            else
            {
                TimeHandler.Instance.AddTimeAction(_time, callback, false);
            }

            //セーブデータにも保存
            DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.weather.type = type;
            DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.weather.power = value;
        }

        private void LateUpdate() {
            if (_currentWeatherObject != null)
            {
                var pos = Camera.main.transform.position;
                pos.y += 5;
                pos.z = _currentWeatherObject.transform.position.z;
                _currentWeatherObject.transform.position = pos;
            }
        }
        
        public class FlashData
        {
            public Action Action { get; set; }

            public float NowTime { get; set; }

            public float Frame { get; set; }

            public Color Color { get; set; }

            public FlashData(Action action, float nowTime, float frame, Color color) {
                Action = action;
                NowTime = nowTime;
                Frame = frame;
                Color = color;
            }

        }
    }
}