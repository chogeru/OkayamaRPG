using Effekseer;
using Effekseer.Editor;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map.Component.Character;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.DatabaseEditor.View.Preview
{
    public class AssetManagePreview : AbstractPreview
    {
        private int          _frameCounter;
        private List<string> _imageNameList = new List<string>();
        private bool         _isEffekseer;
        private bool         _isParticle;

        private bool _isPlay;

        // アニメーション関連
        private GameObject _mainCanvas;
        private GameObject _imageObject;

        // パーティクル関連
        private GameObject         _particleObj;
        private string             _particlePath = "";
        private Button             _playButton;
        private int                _selectNum;
        private List<List<Sprite>> _spriteList = new List<List<Sprite>>();

        private int _type = 0;
        
        public void SetAssetId(string id, int type) {
            _assetId = id;
            _type = type;
        }

        public async void UpdateAssetId(string id) {
            await Task.Delay(50);
            _characterGraphic.ChangeAsset(id);
            _characterGraphic.Reset();
            _sceneWindow.Render();
        }


        public override void InitUi(SceneWindow sceneWindow, bool isChange = false) {
            DestroyLocalData();
            _sceneWindow = sceneWindow;
            if (_isParticle)
            {
                // Effekseer
                if (_isEffekseer)
                {
                    var effect =
                        AssetDatabase.LoadAssetAtPath<EffekseerEffectAsset>(_particlePath);
                    var obj = new GameObject();
                    obj.AddComponent<EffekseerEmitter>().effectAsset = effect;
                    _particleObj = obj;
                    EffekseerEmitterEditor.GetInstance()?.Stop();
                    EffekseerEditor.instance.InitSystem();
                    Selection.activeGameObject = _particleObj;
                }
                // 通常
                else
                {
                    // Prefabを読み込む
                    var obj = AssetDatabase.LoadAssetAtPath<GameObject>(_particlePath);
                    _particleObj = PrefabUtility.InstantiatePrefab(obj) as GameObject;
                }
                
                if (_particleObj != null)
                    sceneWindow.MoveGameObjectToPreviewScene(_particleObj);
            }
            else
            {
                
                // キャンバスを作成する
                _mainCanvas = new GameObject("Canvas");
                _actorOnMap = new GameObject();
                _actorOnMap.gameObject.transform.SetParent(_mainCanvas.transform);

                var charaObj = new GameObject();
                var canvas = charaObj.AddComponent<Canvas>();
                canvas.sortingLayerName = "Editor_Event";
                
                if (_type == (int) AssetCategoryEnum.SV_BATTLE_CHARACTER)
                {
                    _characterGraphic = charaObj.AddComponent<CharacterBattleGraphic>();
                }
                else if (_type == (int) AssetCategoryEnum.POPUP)
                {
                    _characterGraphic = charaObj.AddComponent<PopupGraphic>();
                }
                else if (_type == (int) AssetCategoryEnum.SV_WEAPON)
                {
                    _characterGraphic = charaObj.AddComponent<WeaponGraphic>();
                }
                else if (_type == (int) AssetCategoryEnum.SUPERPOSITION)
                {
                    _characterGraphic = charaObj.AddComponent<StateGraphic>();
                }
                else
                {
                    _characterGraphic = charaObj.AddComponent<CharacterGraphic>();
                }
                _characterGraphic.gameObject.transform.SetParent(_actorOnMap.transform);
                _characterGraphic.Init(_assetId);
                _sceneWindow.MoveGameObjectToPreviewScene(_mainCanvas);
                _sceneWindow.Render();
                SetAnimation(0);
            }
        }
        
        // 参照画像のパス（表示名）を設定する
        public void Setup(List<string> imageName = null) {
            _isParticle = false;
            _imageNameList = imageName;
        }

        // 更新処理
        public override void Update() {
            if (_isParticle == false)
            {
                _frameCounter += 1 / Runtime.Common.Commons.Fps;
            }
            else
            {
                if (_isPlay)
                {
                    if (_isEffekseer && EffekseerEmitterEditor.GetInstance()?.IsExists() == false)
                    {
                        EffekseerEmitterEditor.GetInstance().Stop();
                        _playButton.text = EditorLocalize.LocalizeText("WORD_0932");
                        _isPlay = !_isPlay;
                    }
                    else if (_isEffekseer == false &&
                             (_particleObj?.GetComponentInChildren<ParticleSystem>(true)?.isPaused == true ||
                              _particleObj?.GetComponentInChildren<ParticleSystem>(true)?.isStopped == true))
                    {
                        SimulateDisable();
                        _playButton.text = EditorLocalize.LocalizeText("WORD_0932");
                        _isPlay = !_isPlay;
                    }
                }
            }
        }

        // UI作成
        public override VisualElement CreateUi() {
            var container = new VisualElement();
            container.style.display = DisplayStyle.Flex;
            container.style.flexDirection = FlexDirection.Row;
            container.SendToBack();

            if (_isParticle)
            {
                // パーティクルの再生ボタン
                _playButton = new Button {text = EditorLocalize.LocalizeText("WORD_0932")};
                _playButton.clicked += () =>
                {
                    if (!_isPlay)
                    {
                        if (_isEffekseer)
                            EffekseerEmitterEditor.GetInstance().Play();
                        else
                            PlayPaticle();
                        _playButton.text = EditorLocalize.LocalizeText("WORD_0933");
                        _isPlay = !_isPlay;
                    }
                    else
                    {
                        if (_isEffekseer)
                            EffekseerEmitterEditor.GetInstance().Stop();
                        else
                            SimulateDisable();
                        _playButton.text = EditorLocalize.LocalizeText("WORD_0932");
                        _isPlay = !_isPlay;
                    }
                };
                container.Add(_playButton);
            }
            else
            {
                // 再生対象選択用のドロップダウン
                var choices = new List<string>(_imageNameList);
                var popupField =
                    new PopupField<string>(choices, 0);
                popupField.RegisterValueChangedCallback(evt =>
                {
                    _selectNum = popupField.index;
                    if (_type == (int) AssetCategoryEnum.MOVE_CHARACTER || _type == (int) AssetCategoryEnum.OBJECT)
                    {
                        SetDirectionType((Commons.Direction.Id) _selectNum + 2);
                    }
                    else
                    {
                        SetDirectionType((Commons.Direction.Id) _selectNum);
                    }

                    SetDirection();
                });
                container.Add(popupField);
            }

            return container;
        }
        
        protected override void SetDirection()
        {
            if (_type == (int) AssetCategoryEnum.MOVE_CHARACTER || _type == (int) AssetCategoryEnum.OBJECT)
            {
                base.SetDirection();
                return;
            }
            if (_characterGraphic != null) _characterGraphic.ChangeDirection((CharacterMoveDirectionEnum)_selectNum);
        }

        //====================================================================================================
        // パーティクル関連処理
        //====================================================================================================
        // エフェクトパスの設定
        public void SetEffectPath(string path, bool effekseer) {
            _isEffekseer = effekseer;
            _isParticle = true;
            _particlePath = path;
        }

        // エフェクト再生
        public void PlayPaticle() {
            if (_isEffekseer)
            {
                _particleObj.GetComponentInChildren<EffekseerEmitter>().Play();
            }
            else
            {
                SimulateDisable();
                SetSimulateMode();
                var particleSystem = _particleObj?.GetComponentInChildren<ParticleSystem>(true);
                if (particleSystem)
                {
                    particleSystem.Play();
                    ParticleSystemEditorUtilsReflect.editorIsScrubbing = false;
                }
            }
        }

        // パーティクルのシミュレート設定
        private void SetSimulateMode() {
            SimulateDisable();
            var particleSystem = _particleObj?.GetComponentInChildren<ParticleSystem>(true);
            if (particleSystem)
                if (ParticleSystemEditorUtilsReflect.lockedParticleSystem != particleSystem)
                    ParticleSystemEditorUtilsReflect.lockedParticleSystem = particleSystem;
        }

        // パーティクルのシミュレート設定を無効化
        public void SimulateDisable() {
            if (_isEffekseer)
            {
                _particleObj.GetComponentInChildren<EffekseerEmitter>().Stop();
            }
            else
            {
                ParticleSystemEditorUtilsReflect.editorIsScrubbing = false;
                ParticleSystemEditorUtilsReflect.editorPlaybackTime = 0f;
                ParticleSystemEditorUtilsReflect.StopEffect();
            }
        }

        public override void DestroyLocalData() {
            if (_mainCanvas != null)
            {
                Object.DestroyImmediate(_mainCanvas);
            	_mainCanvas = null;
            }

            if (_spriteList != null)
            {
                for (var i = 0; i < _spriteList.Count; i++)
                {
                    for (var j = 0; _spriteList[i] != null && j < _spriteList[i].Count; j++)
                    {
                        Object.DestroyImmediate(_spriteList[i][j]);
                        _spriteList[i][j] = null;
                    }
                }
            }

            _spriteList = new List<List<Sprite>>();

            if (_particleObj != null)
            {
                Object.DestroyImmediate(_particleObj);
            	_particleObj = null;
            }
        }
    }
}