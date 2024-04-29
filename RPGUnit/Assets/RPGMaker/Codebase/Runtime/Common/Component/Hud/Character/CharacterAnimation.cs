using Effekseer;
using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Animation;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Runtime.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Common.Component.Hud.Character
{
    public class CharacterAnimation : MonoBehaviour
    {
        // データプロパティ
        //--------------------------------------------------------------------------------------------------------------
        private List<string> _animationNameList;
        private GameObject _animationPrefab;

        // 関数プロパティ
        //--------------------------------------------------------------------------------------------------------------
        private Action _closeAction;

        // コアシステムサービス
        //--------------------------------------------------------------------------------------------------------------
        private DatabaseManagementService _databaseManagementService;
        private EffekseerHandle _effekseerHandle;

        private ParticleSystem _particleSystem = null;
        private List<ParticleSystem> particles = new List<ParticleSystem>();

        // 状態プロパティ
        //--------------------------------------------------------------------------------------------------------------
        private bool _isMap;

        // 表示要素プロパティ
        //--------------------------------------------------------------------------------------------------------------
        private GameObject _prefab;
        private AnimationDataModel _animationDataModel;

        private bool _waitToggle;
        private int  _frame;
        private bool _isPlay;
        private bool _isPlaySound;
        private bool _isPlayFlash;

        private static GameObject _flashObject;

        private GameObject _targetImage;
        private GameObject _targetObj;

        private Image _flash;
        private Image _flashMask;
        private int _flashFrame;
        private float _flashAlpha;
        private float _flashMaskAlpha;
        private int _deleteFrame;

        public string animationId;
        public float delay;

        public bool mirror;
        private bool isBattle;
        private bool isEffekseer;
        private int _seMax;
        private int _flashMax;

        /// <summary>
        /// アニメーション無し
        /// </summary>
        private static string AnimationNone = "54b168ea-5141-48ed-9e42-4336ac58755c";

        // const
        //--------------------------------------------------------------------------------------------------------------

        // initialize methods
        //--------------------------------------------------------------------------------------------------------------
        /**
         * 初期化
         */
        public void Init() {
            MapManager.AddAnimation(this);

            if (_prefab != null)
                return;

            _databaseManagementService = new DatabaseManagementService();

            _prefab = new GameObject("CharacterAnimation");
            _prefab.transform.SetParent(gameObject.transform);
            var animationDataModels = _databaseManagementService.LoadAnimation();

            _animationNameList = new List<string>();
            for (var i = 0; i < animationDataModels.Count; i++)
                _animationNameList.Add(animationDataModels[i].particleName);

            _frame = 0;
            _isPlay = true;

            _isMap = GameStateHandler.IsMap();
        }

        private void InitForBattle() {
            if (_prefab != null)
                return;

            _databaseManagementService = new DatabaseManagementService();

            _prefab = gameObject;
            _prefab.transform.SetParent(gameObject.transform);
            var animationDataModels = _databaseManagementService.LoadAnimation();

            _animationNameList = new List<string>();

            for (var i = 0; i < animationDataModels.Count; i++)
                _animationNameList.Add(animationDataModels[i].particleName);

            _frame = 0;
            _isPlay = true;
        }

        public void PlayAnimation(
            Action endAction,
            Action closeAction,
            string eventId,
            string animationDataId,
            bool waitToggle,
            string currentEventID
        ) {
            Init();
            _closeAction = closeAction;
            _waitToggle = waitToggle;
            var animation = _databaseManagementService.LoadAnimation();
            _animationDataModel = null;
            for (int i = 0; i < animation.Count; i++)
                if (animation[i].id == animationDataId)
                {
                    _animationDataModel = animation[i];
                    break;
                }
            
            //アニメーションのデータを参照し、再生速度が0以下だった場合は、再生不可能のため、即 closeAction を実行して終了する
            if (_animationDataModel == null || _animationDataModel.id == AnimationNone || _animationDataModel.playSpeed <= 0)
            {
                _closeAction.Invoke();
                StopAnimation();
                return;
            }


            var evId = (eventId == "-1") ? currentEventID : eventId;
            if (evId == "-2") //プレイヤーの座標に出す
                _targetObj = MapManager.GetOperatingCharacterGameObject();
            else
                _targetObj = MapEventExecutionController.Instance.GetEventMapGameObject(evId);

            //Editor側でターゲット指定していたイベントが消去されている場合、ターゲットの取得が出来ないため
            //そのケースでは初期値であるプレイヤーの座標を指定しなおす
            if (_targetObj == null)
                _targetObj = MapManager.GetOperatingCharacterGameObject();

            EffectSet(_animationDataModel.particleId);

            //SE初期化
            InitSE();

            //対象を取得し、Flash初期化
            _targetImage = _targetObj.transform.Find("actor")?.gameObject;
            InitFlash();

            if (!waitToggle)
                endAction.Invoke();
        }

        /// <summary>
        ///     バトル用に使用する
        /// </summary>
        /// <param name="endAction"></param>
        /// <param name="closeAction"></param>
        /// <param name="animationDataId"></param>
        public void PlayAnimationForBattle(
            Action endAction,
            Action closeAction,
            string animationDataId,
            bool isActor
        ) {
            InitForBattle();
            _waitToggle = true;
            _closeAction = closeAction;
            var animation = _databaseManagementService.LoadAnimation();
            _animationDataModel = null;
            for (int i = 0; i < animation.Count; i++)
                if (animation[i].id == animationDataId)
                {
                    _animationDataModel = animation[i];
                    break;
                }

            //アニメーションのデータを参照し、再生速度が0以下だった場合は、再生不可能のため、即 closeAction を実行して終了する
            if (_animationDataModel == null || _animationDataModel.id == AnimationNone || _animationDataModel.playSpeed <= 0)
            {
                _closeAction.Invoke();
                return;
            }

            if (isActor)
                _targetObj = transform.parent.Find("Sprite").gameObject;
            else
                _targetObj = transform.parent.gameObject;

            EffectSet(_animationDataModel.particleId, true);

            //SE初期化
            InitSE();

            //対象を取得し、Flash初期化
            _targetImage = _targetObj.gameObject;
            InitFlash();
        }

        private void OnParticleSystemStopped() {
            if (_waitToggle)
                _closeAction.Invoke();
            _isPlay = false;
        }

        private void Update() {
            // モードが変わった際に破棄
            if (_isMap != GameStateHandler.IsMap())
            {
                StopAnimation();
                return;
            }

            // メニュー表示中
            if (GameStateHandler.IsMenu())
            {
                // 再生停止
                if (isEffekseer)
                {
                    if (_effekseerHandle.enabled && _effekseerHandle.exists == true &&
                        _effekseerHandle.paused == false)
                    {
                        _effekseerHandle.paused = true;
                    }
                }
                else
                {
                    if (_particleSystem != null)
                    {
                        if (!_particleSystem.isPaused)
                        {
                            _particleSystem.Pause();
                        }
                    }
                    if (particles.Count > 0)
                    {
                        for (int i = 0; i < particles.Count; i++)
                        {
                            if (!particles[i].isPaused)
                            {
                                particles[i].Pause();
                            }
                        }
                    }
                }
                return;
            }
            else
            {
                // 停止中であれば再開
                if (isEffekseer)
                {
                    if (_effekseerHandle.enabled && _effekseerHandle.exists == true &&
                        _effekseerHandle.paused == true)
                    {
                        _effekseerHandle.paused = false;
                    }
                }
                else
                {
                    if (_particleSystem != null)
                    {
                        if (_particleSystem.isPaused)
                        {
                            _particleSystem.Play();
                        }
                    }
                    if (particles.Count > 0)
                    {
                        for (int i = 0; i < particles.Count; i++)
                        {
                            if (particles[i].isPaused)
                            {
                                particles[i].Play();
                            }
                        }
                    }
                }
            }

            if (isEffekseer)
            {
                if (_effekseerHandle.enabled && _effekseerHandle.exists == false)
                {
                    if (_waitToggle)
                        _closeAction.Invoke();
                    _isPlay = false;
                }
            }
            else
            {
                if (_particleSystem != null)
                {
                    if (!_particleSystem.isPlaying)
                    {
                        var flg = false;
                        if (particles.Count > 0)
                        {
                            for (int i = 0; i < particles.Count; i++)
                            {
                                if (particles[i].isPlaying)
                                {
                                    flg = true;
                                    break;
                                }
                            }
                        }

                        if (!flg)
                        {
                            if (_waitToggle)
                                _closeAction.Invoke();
                            _isPlay = false;
                        }
                    }
                }
            }

            // SE更新
            UpdatePlaySE();

            // フラッシュ更新
            UpdatePlayFlash();

            // 座標更新
            if (_isPlay && !isBattle)
                SetPosition(false);

            _frame++;

            // 再生終了
            if (_isPlay == false && _isPlaySound == false && _isPlayFlash == false)
                StopAnimation();
        }

        /// <summary>
        /// サウンド初期化処理
        /// </summary>
        private void InitSE() {
            _seMax = -1;

            for (int i = 0; i < _animationDataModel.seList.Count; i++)
                if (_animationDataModel.seList[i].frame > _seMax) _seMax = _animationDataModel.seList[i].frame;

            if (_seMax >= 0)
                _isPlaySound = true;
            else
                _isPlaySound = false;
        }

        /// <summary>
        /// サウンド更新
        /// </summary>
        private void UpdatePlaySE() {
            if (_isPlaySound == false) return;

            for (int i = 0; i < _animationDataModel.seList.Count; i++)
            {
                if (_animationDataModel.seList[i].frame == _frame)
                {
                    //サウンドデータの生成
                    var sound = new SoundCommonDataModel(_animationDataModel.seList[i].seName.Split(".")[0], 0, 100, 90);
                    SoundManager.Self().Init();
                    //データのセット
                    SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, sound);
                    //サウンドの再生
                    SoundManager.Self().PlaySe();
                }
            }

            // 再生終了
            if (_seMax <= _frame || !_isPlay)
                _isPlaySound = false;
        }

        /// <summary>
        /// フラッシュ初期化
        /// </summary>
        private void InitFlash() {
            if (_targetImage != null)
            {
                if (_targetImage.transform.Find("FlashMask") != null)
                    _flashMask = _targetImage.transform.Find("FlashMask").GetComponent<Image>();
                else
                {
                    var flash = new GameObject();
                    flash.name = "FlashMask";
                    flash.transform.SetParent(_targetImage.transform);
                    _flashMask = flash.AddComponent<Image>();
                    _flashMask.color = Color.clear;
                }
            }

            if (_flashObject == null)
            {
                _flashObject = new GameObject();
                _flashObject.name = "Flash";
                var canvas = _flashObject.AddComponent<Canvas>();
                canvas.sortingLayerID = UnityUtil.SortingLayerManager.GetId(UnityUtil.SortingLayerManager.SortingLayerIndex.Runtime_MapBalloon);
                _flashObject.GetComponent<RectTransform>().sizeDelta = new Vector2(2000, 1200);
                _flash = _flashObject.AddComponent<Image>();
                _flash.color = Color.clear;
                DontDestroyOnLoad(_flashObject);
            }
            else
                _flash = _flashObject.GetComponent<Image>();

            //flashの最大フレームを設定
            _flashMax = -1;

            for (int i = 0; i < _animationDataModel.flashList.Count; i++)
                if (_animationDataModel.flashList[i].frame > _flashMax)
                    _flashMax = _animationDataModel.flashList[i].frame;

            if (_flashMax >= 0)
                _isPlayFlash = true;
            else
                _isPlayFlash = false;
            
        }

        /// <summary>
        /// フラッシュ更新
        /// </summary>
        private void UpdatePlayFlash() {
            if (_isPlayFlash == false)
                return;

            if (_flash == null || _flashMask == null) return;

            // α値の減算
            var color = _flash.color;
            _flash.color = new Color(color.r, color.g, color.b, color.a - _flashAlpha);
            color = _flashMask.color;
            _flashMask.color = new Color(color.r, color.g, color.b, color.a - _flashMaskAlpha);

            // 非表示の解除
            if (_deleteFrame == _frame)
            {
                if (!GameStateHandler.IsBattle())
                {
                    _targetImage.SetActive(true);
                }
                else
                {
                    _targetImage.GetComponent<Image>().enabled = true;
                }
            }

            for (int i = 0; i < _animationDataModel.flashList.Count; i++)
            {
                if (_animationDataModel.flashList[i].frame == _frame)
                {
                    switch (_animationDataModel.flashList[i].flashType)
                    {
                        case 0: // 対象
                        case 1:
                            if (_targetImage.GetComponent<Mask>() == null)
                                _targetImage.AddComponent<Mask>();
                            else
                                _targetImage.GetComponent<Mask>().enabled = true;
                            

                            //対象のImageがアクティブだったら
                            if (_targetImage.GetComponent<Image>().enabled)
                            {
                                _flashMask.enabled = true;
                                var colorStrMask = _animationDataModel.flashList[i].color.Split(",");
                                _flashMask.color = new Color(int.Parse(colorStrMask[0]) / 255f,
                                    int.Parse(colorStrMask[1]) / 255f, int.Parse(colorStrMask[2]) / 255f, 1);
                            }

                            _flashMaskAlpha = 1f / _animationDataModel.flashList[i].time;
                            break;

                        case 2: // 画面全体
                            var colorStr = _animationDataModel.flashList[i].color.Split(",");
                            _flash.color = new Color(int.Parse(colorStr[0]) / 255f, int.Parse(colorStr[1]) / 255f, int.Parse(colorStr[2]) / 255f, 1);
                            _flashAlpha = 1f / _animationDataModel.flashList[i].time;
                            break;

                        case 3: // 対象消去
                            if (!GameStateHandler.IsBattle())
                            {
                                _targetImage.SetActive(false);
                            }
                            else
                            {
                                _targetImage.GetComponent<Image>().enabled = false;
                            }
                            _deleteFrame = _frame + _animationDataModel.flashList[i].time;
                            break;
                    }

                    if (_flashFrame < _frame + _animationDataModel.flashList[i].time)
                        _flashFrame = _frame + _animationDataModel.flashList[i].time;
                }

                //対象のイベントと、マスクのコンポーネントのEnabledを同じにしておく
                if (_animationDataModel.flashList[i].flashType == 1)
                {
                    if (_targetImage.GetComponent<Image>())
                    {
                        _flashMask.enabled = _targetImage.GetComponent<Image>().enabled;
                    }
                }
            }

            // 再生終了
            if (_flashMax <= _frame && _flashFrame <= _frame || !_isPlay)
                _isPlayFlash = false;
        }

        /// <summary>
        ///     パーティクルの設定（Effekseerか、Unityか）
        /// </summary>
        /// <param name="manageData"></param>
        /// <param name="id"></param>
        private void EffectSet(
            string id,
            bool isBattle = false
        ) {
            if (id == "")
            {
                return;
            }
            if (_animationDataModel == null)
            {
                return;
            }

            this.isBattle = isBattle;

#if UNITY_EDITOR
            var inputString =
                UnityEditorWrapper.AssetDatabaseWrapper
                    .LoadAssetAtPath<TextAsset>(
                        "Assets/RPGMaker/Storage/AssetManage/JSON/Assets/" + id + ".json");
            var path = JsonHelper.FromJson<AssetManageDataModel>(inputString.text).imageSettings[0].path;
#else
            string path =
 (ScriptableObjectOperator.GetClass<AssetManageDataModel>("SO/" + id + ".asset") as AssetManageDataModel).imageSettings[0].path;
#endif

            // 各値の設定
            var offsetArr = _animationDataModel.offset.Split(";");
            var rotationArr = _animationDataModel.rotation.Split(';');
            var rotation = new Vector3Int(int.Parse(rotationArr[0]),
                int.Parse(rotationArr[1]),
                int.Parse(rotationArr[2]));

            // 座標設定
            var pos = SetPosition(true);

            var effekseer = path.EndsWith(".asset");
            if (effekseer)
            {
                isEffekseer = true;

                var rate = _animationDataModel.expansion * 0.01f;
                var particle =
                    UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<EffekseerEffectAsset>(
                        PathManager.ANIMATION_EFFEKSEER + path);

                _effekseerHandle = EffekseerSystem.PlayEffect(particle, pos);

                _effekseerHandle.SetRotation(Quaternion.Euler(rotation));
                _effekseerHandle.SetScale(new Vector3(rate, rate, rate));
                _effekseerHandle.speed = _animationDataModel.playSpeed * 0.01f;
            }
            else
            {

                isEffekseer = false;

                var particle =
                    UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<GameObject>(PathManager.ANIMATION_PREFAB + path);
                _animationPrefab = Instantiate(particle);
                var localScale = _animationPrefab.transform.localScale;
                _animationPrefab.transform.SetParent(_prefab.transform);
                _animationPrefab.transform.localScale = localScale;
                _animationPrefab.transform.localPosition =
                    new Vector3(_targetObj.transform.position.x, _targetObj.transform.position.y, -9) + pos;
                _animationPrefab.transform.localRotation = Quaternion.Euler(rotation);
                var main = _animationPrefab.GetComponent<ParticleSystem>().main;
                main.startSizeMultiplier = _animationDataModel.expansion * 0.01f;
                main.stopAction = ParticleSystemStopAction.Callback;
                main.simulationSpeed = _animationDataModel.playSpeed * 0.01f;

                particles = _animationPrefab.GetComponentsInChildren<ParticleSystem>().ToList();
                for (int i = 0; i < particles.Count; i++)
                {
                    Renderer pRenderer = particles[i].GetComponent<Renderer>();
                    pRenderer.sortingLayerID =
                        UnityUtil.SortingLayerManager.GetId(UnityUtil.SortingLayerManager.SortingLayerIndex
                            .Runtime_MapBalloon);
                    var mainModule = particles[i].main;
                    mainModule.startSizeMultiplier *= _animationDataModel.expansion * 0.01f;
                    mainModule.stopAction = ParticleSystemStopAction.Callback;
                    mainModule.simulationSpeed = _animationDataModel.playSpeed * 0.01f;
                }

                _particleSystem = _animationPrefab.GetComponent<ParticleSystem>();
                _particleSystem.Play();
                Renderer psRenderer = _particleSystem.GetComponent<Renderer>();
                psRenderer.sortingLayerID = UnityUtil.SortingLayerManager.GetId(UnityUtil.SortingLayerManager.SortingLayerIndex.Runtime_MapBalloon);

                for (int i = 0; i < particles.Count; i++)
                {
                    particles[i].Play();
                }
            }
        }

        /// <summary>
        /// アニメーション座標を返却
        /// </summary>
        /// <param name="ret">座標を返却するのみ=true, 設定まで行う=false</param>
        /// <returns></returns>
        private Vector3 SetPosition(bool retOnly) {

            var pos = Vector3.zero;
            if (!isBattle)
            {
                // 左下原点
                pos = _targetObj.transform.localPosition;
                pos += _targetObj.transform.localScale / 2;
                if (_animationDataModel.particleType != 3)
                {
                    switch (_animationDataModel.particlePos)
                    {
                        case 0:
                        case 1:
                            break;
                        case 2:
                            pos.y -= _targetObj.transform.localScale.y / 2;
                            break;
                        case 3:
                            pos.y = _targetObj.transform.localScale.y;
                            break;
                    }
                }
                else
                    Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            }
            else
            {
                float posY = 0f;
                SystemSettingDataModel systemdata = DataManager.Self().GetSystemDataModel();
                const float per = 100.0f;

                if (_animationDataModel.particleType < 2)
                {
                    //敵の座標を基準に出す
                    switch (_animationDataModel.particlePos)
                    {
                        case 0:
                        case 1:
                            break;
                        case 2:
                            posY = -1.0f * _targetObj.transform.parent.GetComponent<RectTransform>().sizeDelta.y / 2.0f / per;
                            break;
                        case 3:
                            posY = _targetObj.transform.parent.GetComponent<RectTransform>().sizeDelta.y / 2.0f / per;
                            break;
                    }
                    pos = _targetObj.transform.position + new Vector3(0f, posY, 0);
                }
                else if (_animationDataModel.particleType == 2)
                {
                    //敵の表示領域の真ん中に出す
                    if (systemdata.battleScene.viewType == 1)
                        pos = new Vector3(-350.0f / per, 250.0f / per, 0);
                    else
                        pos = new Vector3(0, 250.0f / per, 0);
                }
                else
                {
                    //画面の真ん中に出す
                    pos = new Vector3(0, 250.0f / per, 0);
                }
            }

            if (!retOnly)
            {
                if (isEffekseer)
                    _effekseerHandle.SetLocation(pos);
                else
                    _animationPrefab.transform.localPosition = pos;
            }

            return pos;
        }

        public void StopAnimation() {
            _isPlay = false;
            _isPlaySound = false;
            _isPlayFlash = false;
            if (_flash != null)
            {
                _flash.color = Color.clear;
            }

            if (_flashMask != null)
            {
                _flashMask.color = Color.clear;
            }

            if (_targetImage != null)
            {
                if (_targetImage.GetComponent<Mask>() != null)
                    _targetImage.GetComponent<Mask>().enabled = false;

                if (!GameStateHandler.IsBattle())
                {
                    _targetImage.SetActive(true);
                }
                else
                {
                    _targetImage.GetComponent<Image>().enabled = true;
                }
            }

            _effekseerHandle.Stop();

            try
            {
                DestroyImmediate(this.gameObject);
            }
            catch
            {
                //すでに消されてる
            }
        }
        
        void OnDestroy(){
            //シーンを跨ぐように用意
            //マップ遷移時にStopAnimationが呼ばれているのでDestroyでは、Effekseerが再生中だった場合停止する
            if (_effekseerHandle.enabled)
            {
                if (_effekseerHandle.exists)
                {
                    _effekseerHandle.Stop();
                }
            }
        }

    }
    

    public class CharacterAnimationActor
    {
        public string animationId;
        public float delay;
        public bool mirror;
    }
}