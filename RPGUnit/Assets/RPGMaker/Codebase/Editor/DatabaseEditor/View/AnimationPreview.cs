using Effekseer;
using Effekseer.Editor;
using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Animation;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using UI = UnityEngine.UI;

namespace RPGMaker.Codebase.Editor.DatabaseEditor.View.Preview
{
    public class AnimationPreview : AbstractPreview
    {
        private const string PreviewObj = "PreviewObjs.prefab";

        private Button _playButton;
        private Button _btnEnemy;
        private Button _btnBackgroundTop;
        private Button _btnBackgroundBottom;
        private string _imageNameEnemy = "";
        private string _imageNameBackground = "";


        private AnimationDataModel _animationDataModel;

        private bool        _animPlayFlag;
        private int         _frame;
        private int         _endFrame;

        private GameObject  _mainObject;
        private bool        _isEffekseer = false;
        private GameObject  _backGroundTop;
        private GameObject  _backGroundBottom;
        private GameObject  _targetImage;
        private GameObject  _effekseer;
        private GameObject  _particle; 
        private AudioSource _audioSource;

        private UI.Image    _flash;
        private UI.Image    _flashMask;
        private int         _flashFrame;
        private float       _flashAlpha;
        private float       _flashMaskAlpha;
        private int         _deleteFrame;

        public void InitUi(SceneWindow scene)
        {
            DestroyLocalData();

            _sceneWindow = scene;
            _sceneWindow.SetRenderingSize(_sceneWindow.GetRenderingSize().x, _sceneWindow.GetRenderingSize().x * 9 / 16);
            
            Create();
        }

        public void Create() {
            // プレビュー用オブジェクト作成
            GameObject previewObjs = AssetDatabase.LoadAssetAtPath<GameObject>(PathManager.SYSTEM_ANIMATION + PreviewObj);
            _mainObject = Object.Instantiate(previewObjs);
            _mainObject.transform.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
            _mainObject.transform.GetComponent<Canvas>().worldCamera = _sceneWindow.Camera;
            _mainObject.transform.GetComponent<Canvas>().sortingOrder = -100;

            _backGroundTop = _mainObject.transform.GetChild(0).GetChild(1).gameObject;
            _backGroundBottom = _mainObject.transform.GetChild(0).GetChild(0).gameObject;
            _targetImage = _mainObject.transform.GetChild(1).gameObject;
            _effekseer = _mainObject.transform.GetChild(3).gameObject;
            _flashMask = _targetImage.transform.GetChild(0).transform.GetComponent<UI.Image>();
            _flash = _mainObject.transform.GetChild(4).gameObject.GetComponent<UI.Image>();

            // サウンドの取得
            if (GameObject.FindWithTag("sound") == null)
            {
                var go = new GameObject();
                go.name = "sound";
                go.tag = "sound";
                _audioSource = go.AddComponent<AudioSource>();
            }
            else
                _audioSource = GameObject.FindWithTag("sound").transform.gameObject.GetComponent<AudioSource>();

            _sceneWindow.MoveGameObjectToPreviewScene(_mainObject);
            _mainObject.transform.localScale = Vector3.one;
            if (_playButton != null)
                _playButton.text = EditorLocalize.LocalizeText("WORD_0932");
            _animPlayFlag = false;
            SimulateDisable();
            StopAudio();
            SetDefault();
        }

        new public VisualElement CreateUi()
        {
            var container = new VisualElement();
            container.style.display = DisplayStyle.Flex;
            container.style.flexDirection = FlexDirection.Row;
            container.SendToBack();

            // 『再生』ボタン。
            _playButton = new Button() {text = EditorLocalize.LocalizeText("WORD_0932")};
            _playButton.clicked += () =>
            {
                _audioSource.Play();
                _audioSource.Stop();

                if (!_animPlayFlag)
                {
                    if ((_isEffekseer && _effekseer?.GetComponent<EffekseerEmitter>()?.effectAsset == null) ||
                        (!_isEffekseer && _particle?.GetComponentInChildren<ParticleSystem>(true) == null))
                        return;

                    _playButton.text = EditorLocalize.LocalizeText("WORD_0933");
                    _animPlayFlag = true;
                    PlayPaticle();
                }
                else
                {
                    _playButton.text = EditorLocalize.LocalizeText("WORD_0932");
                    _animPlayFlag = false;
                    SimulateDisable();
                    StopAudio();
                    SetDefault();
                }
            };
            container.Add(_playButton);

            // 『敵 変更』ボタン。
            _btnEnemy = new Button() { text = EditorLocalize.LocalizeText("WORD_0443") + " " + EditorLocalize.LocalizeText("WORD_0137") };
            _btnEnemy.clicked += () =>
            {
                var targetImageModalWindow = new ImageSelectModalWindow(PathManager.IMAGE_ENEMY);
                targetImageModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select TargetImage"), data =>
                {
                    _imageNameEnemy = (string) data;
                    ChangeTargetImage(PathManager.IMAGE_ENEMY + _imageNameEnemy + ".png");
                    _playButton.text = EditorLocalize.LocalizeText("WORD_0932");
                    _animPlayFlag = false;
                }, _imageNameEnemy);
            };
            container.Add(_btnEnemy);

            // 『戦闘背景の変更（上）』ボタン。
            _btnBackgroundTop = new Button() { text = EditorLocalize.LocalizeText("WORD_1179") + " (" + EditorLocalize.LocalizeText("WORD_0297") + ")" };
            _btnBackgroundTop.clicked += () =>
            {
                var targetImageModalWindow = new ImageSelectModalWindow(PathManager.BATTLE_BACKGROUND_2);
                targetImageModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Background"), data =>
                {
                    _imageNameBackground = (string) data;
                    ChangeBattleBackGround(PathManager.BATTLE_BACKGROUND_2 + _imageNameBackground + ".png", true);
                    _playButton.text = EditorLocalize.LocalizeText("WORD_0932");
                    _animPlayFlag = false;
                }, _imageNameBackground);
            };
            container.Add(_btnBackgroundTop);

            // 『戦闘背景の変更（下）』ボタン。
            _btnBackgroundBottom = new Button() { text = EditorLocalize.LocalizeText("WORD_1179") + " (" + EditorLocalize.LocalizeText("WORD_0299") + ")" };
            _btnBackgroundBottom.clicked += () =>
            {
                var targetImageModalWindow = new ImageSelectModalWindow(PathManager.BATTLE_BACKGROUND_1);
                targetImageModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Background"), data =>
                {
                    _imageNameBackground = (string) data;
                    ChangeBattleBackGround(PathManager.BATTLE_BACKGROUND_1 + _imageNameBackground + ".png", false);
                    _playButton.text = EditorLocalize.LocalizeText("WORD_0932");
                    _animPlayFlag = false;
                }, _imageNameBackground);
            };
            container.Add(_btnBackgroundBottom);
            return container;
        }

        new public void Update() {
            if (_animPlayFlag)
            {
                // 各更新処理
                UpdateSE();
                UpdateFlash();

                if (IsPlay() == false && _endFrame <= _frame && _flashFrame <= _frame)
                {
                    _playButton.text = EditorLocalize.LocalizeText("WORD_0932");
                    _animPlayFlag = false;
                    SetDefault();
                    SimulateDisable();
                }

                _frame++;
            }
            _sceneWindow.Repaint();
            _sceneWindow.Render();
        }

        private void UpdateSE() {
            int max = 0;

            // SE再生
            for (int i = 0; i < _animationDataModel.seList.Count; i++)
            {
                if (_animationDataModel.seList[i].frame > max) max = _animationDataModel.seList[i].frame;

                if (_animationDataModel.seList[i].frame == _frame)
                {
                    PlayAudio(_animationDataModel.seList[i].seName);
                }
            }
        }

        private void UpdateFlash() {
            int max = 0;

            // α値の減算
            var color = _flash.color;
            _flash.color = new Color(color.r, color.g, color.b, color.a - _flashAlpha);
            color = _flashMask.color;
            _flashMask.color = new Color(color.r, color.g, color.b, color.a - _flashMaskAlpha);

            // 非表示の解除
            if (_deleteFrame == _frame)
                _targetImage.SetActive(true);

            // FLASH再生
            for (int i = 0; i < _animationDataModel.flashList.Count; i++)
            {
                if (_animationDataModel.flashList[i].frame > max) max = _animationDataModel.flashList[i].frame;

                if (_animationDataModel.flashList[i].frame == _frame)
                {
                    switch (_animationDataModel.flashList[i].flashType)
                    {
                        case 0: // 対象
                        case 1:
                            var colorStrMask = _animationDataModel.flashList[i].color.Split(",");
                            _flashMask.color = new Color(int.Parse(colorStrMask[0]) / 255f, int.Parse(colorStrMask[1]) / 255f, int.Parse(colorStrMask[2]) / 255f, 1);
                            _flashMaskAlpha = 1f / _animationDataModel.flashList[i].time;
                            break;

                        case 2: // 画面全体
                            var colorStr = _animationDataModel.flashList[i].color.Split(",");
                            _flash.color = new Color(int.Parse(colorStr[0]) / 255f, int.Parse(colorStr[1]) / 255f, int.Parse(colorStr[2]) / 255f, 1);
                            _flashAlpha = 1f / _animationDataModel.flashList[i].time;
                            break;

                        case 3: // 対象消去
                            _targetImage.SetActive(false);
                            _deleteFrame = _frame + _animationDataModel.flashList[i].time;
                            break;
                    }

                    if (_flashFrame < _frame + _animationDataModel.flashList[i].time)
                        _flashFrame = _frame + _animationDataModel.flashList[i].time;
                }
            }
        }

        private void PlayAudio(string se) {
            _audioSource.clip = LoadSe(se);
            _audioSource.PlayOneShot(_audioSource.clip);
        }

        void StopAudio() {
            _audioSource?.Stop();
        }

        private AudioClip LoadSe(string se) {
            AudioClip audioClip = null;

            if (System.IO.File.Exists("Assets/RPGMaker/Storage/Sounds/SE/" + se))
                audioClip =
                    AssetDatabase.LoadAssetAtPath<AudioClip>(
                        "Assets/RPGMaker/Storage/Sounds/SE/" + se);

            if (System.IO.File.Exists("Assets/RPGMaker/Storage/Sounds/SE/" + se))
                audioClip =
                    AssetDatabase.LoadAssetAtPath<AudioClip>(
                        "Assets/RPGMaker/Storage/Sounds/SE/" + se);

            return audioClip;
        }

        public void ChangeExpansionRate(float rate)
        {
            if (_isEffekseer)
                _effekseer.transform.localScale = Vector3.one * (rate * 0.01f);
            else if (_particle != null)
            {
                var main = _particle.transform.GetComponent<ParticleSystem>().main;
                main.startSizeMultiplier *=  rate * 0.01f;
                var particle = _particle.transform.GetComponentsInChildren<ParticleSystem>();
                for (int i = 0; i < particle.Length; i++)
                {
                    var mainModule = particle[i].main;
                    mainModule.startSizeMultiplier *= rate * 0.01f;
                }
            }
        }

        public void ChangeSpeed(float rate)
        {
            if (_isEffekseer)
                _effekseer.GetComponent<EffekseerEmitter>().speed = rate * 0.01f;
            else if (_particle != null)
            {
                var main = _particle.transform.GetComponent<ParticleSystem>().main;
                main.simulationSpeed = rate * 0.01f;
                var particle = _particle.transform.GetComponentsInChildren<ParticleSystem>();
                for (int i = 0; i < particle.Length; i++)
                {
                    var mainModule = particle[i].main;
                    mainModule.simulationSpeed = rate * 0.01f;
                }

            }
        }

        public void ChangeRotation(Quaternion rotation)
        {
            if (_isEffekseer)
                _effekseer.transform.rotation = rotation;
            else if (_particle != null)
            {
                _particle.transform.localRotation = rotation;
            }
        }

        public void ChangePos(Vector2 pos, int type) {
            ChangeDisplayPos(type);
            ChangeOffSet(pos);
        }

        private void ChangeOffSet(Vector2 pos)
        {
            if (_isEffekseer)
                _effekseer.transform.localPosition += new Vector3(pos.x, pos.y, 0);
            else if (_particle != null)
                _particle.transform.localPosition += new Vector3(pos.x, pos.y, 0);
        }

        private void ChangeDisplayPos(int type)
        {
            float pos = 0f;
            if (_animationDataModel.particleType != 3)
            {
                switch (type)
                {
                    case 0:
                    case 1:
                        pos = 0;
                        break;
                    case 2:
                        pos = _targetImage.transform.position.y - _targetImage.GetComponent<UI.Image>().sprite.texture.height / 2;
                        break;
                    case 3:
                        pos = _targetImage.transform.position.y + _targetImage.GetComponent<UI.Image>().sprite.texture.height / 2;
                        break;
                }
            }

            if (_isEffekseer)
                _effekseer.transform.localPosition = new Vector3(0, pos, 0);
            else if (_particle != null)
                _particle.transform.localPosition = new Vector3(0, pos, 0);
        }

        public void SetData(AnimationDataModel animation) {
            // 停止フレームを設定
            _endFrame = 0;
            foreach (var se in animation.seList)
                if (se.frame > _endFrame)
                    _endFrame = se.frame;
            foreach (var fl in animation.flashList)
                if (fl.frame > _endFrame)
                    _endFrame = fl.frame;

            Create();
            _animationDataModel = animation;
        }

        private void ChangeTargetImage(string path) {
            Sprite image = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            _targetImage.GetComponent<UI.Image>().sprite = image;
            _targetImage.GetComponent<RectTransform>().sizeDelta = image.rect.size;
            SaveParticle();
            Create();
        }

        private void ChangeBattleBackGround(string path, bool top) {
            Sprite image = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (top)
                _backGroundTop.GetComponent<UI.Image>().sprite = image;
            else
                _backGroundBottom.GetComponent<UI.Image>().sprite = image;
            SaveParticle();
            Create();
        }

        public void ChangeParticle<T>(T prefab, bool effekseer)
        {
            _animPlayFlag = false;
            SimulateDisable();
            StopAudio();
            SetDefault();
            

            if (_particle != null)
            {
                GameObject.DestroyImmediate(_particle);
                _particle = null;
            }

            if (effekseer == true)
            {
                SimulateDisable();
                _isEffekseer = true;
                // Effekseer初期化
                EffekseerEditor.instance.InitSystem();

                _effekseer.GetComponent<EffekseerEmitter>().effectAsset = prefab as EffekseerEffectAsset;
                Selection.activeGameObject = _effekseer;
            }
            else
            {
                SimulateDisable();
                _isEffekseer = false;
                _particle = PrefabUtility.InstantiatePrefab(prefab as GameObject) as GameObject;
                _sceneWindow.MoveGameObjectToPreviewScene(_particle);
                _particle.transform.SetParent(_mainObject.transform);
                _particle.transform.localScale = Vector3.one;
                _mainObject.transform.localScale = Vector3.one;
            }

            // 各値の設定
            ChangeExpansionRate(_animationDataModel.expansion);
            ChangeSpeed(_animationDataModel.playSpeed);
            var rotationArr = _animationDataModel.rotation.Split(';');
            var rotation = new Vector3Int(int.Parse(rotationArr[0]), int.Parse(rotationArr[1]),
                int.Parse(rotationArr[2]));
            ChangeRotation(Quaternion.Euler(rotation));
            var offsetArr = _animationDataModel.offset.Split(";");
            ChangePos(new Vector2(int.Parse(offsetArr[0]), int.Parse(offsetArr[1])),
                _animationDataModel.particlePos);
        }

        private bool IsPlay() {
            if (_isEffekseer == true && EffekseerEmitterEditor.GetInstance()?.IsExists() == false)
            {
                return false;
            }
            else if (_isEffekseer == false &&
                    (_particle?.GetComponentInChildren<ParticleSystem>(true)?.isPaused == true ||
                    _particle?.GetComponentInChildren<ParticleSystem>(true)?.isStopped == true))
            {
                return false;
            }
            return true;
        }

        private void PlayPaticle()
        {
            if (_isEffekseer == true)
            {
                if (EffekseerEmitterEditor.GetInstance() != null)
                    EffekseerEmitterEditor.GetInstance().Play();
            }
            else
            {
                SimulateDisable();
                SetSimulateMode();
                var particleSystem = _particle?.GetComponentInChildren<ParticleSystem>(true);
                if (particleSystem)
                {
                    particleSystem.Play();
                    ParticleSystemEditorUtilsReflect.editorIsScrubbing = false;
                }
            }
            _frame = 0;
        }

        private void SetSimulateMode() {
            SimulateDisable();
            var particleSystem = _particle?.GetComponentInChildren<ParticleSystem>(true);
            if (particleSystem)
                if (ParticleSystemEditorUtilsReflect.lockedParticleSystem != particleSystem)
                    ParticleSystemEditorUtilsReflect.lockedParticleSystem = particleSystem;
        }

        private void SimulateDisable()
        {
            if (_isEffekseer == true)
            {
                EffekseerEmitterEditor.GetInstance()?.Stop();
            }
            else
            {
                ParticleSystemEditorUtilsReflect.editorIsScrubbing = false;
                ParticleSystemEditorUtilsReflect.editorPlaybackTime = 0f;
                ParticleSystemEditorUtilsReflect.StopEffect();
            }
        }

        private void SaveParticle() {
            PrefabUtility.SaveAsPrefabAsset(_mainObject, PathManager.SYSTEM_ANIMATION + PreviewObj);
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
        }

        private void SetDefault() {
            _flash.color = Color.clear;
            _flashMask.color = Color.clear;
            _flashAlpha = 0;
            _flashMaskAlpha = 0;
            _flashFrame = 0;
            _frame = 0;

            _targetImage.SetActive(true);
        }

        public override void DestroyLocalData() {
            if (_backGroundTop != null)  
            {
                GameObject.DestroyImmediate(_backGroundTop);
                _backGroundTop = null;
            }
            if (_backGroundBottom != null)
            {
                GameObject.DestroyImmediate(_backGroundBottom);
                _backGroundBottom = null;
            }
            if (_effekseer != null) 
            {
                GameObject.DestroyImmediate(_effekseer);
                _effekseer = null;
            }
            if (_particle != null) 
            {
                GameObject.DestroyImmediate(_particle);
                _particle = null;
            }
            if (_mainObject != null) 
            {
                GameObject.DestroyImmediate(_mainObject);
                _mainObject = null;
            }
        }
    }
}