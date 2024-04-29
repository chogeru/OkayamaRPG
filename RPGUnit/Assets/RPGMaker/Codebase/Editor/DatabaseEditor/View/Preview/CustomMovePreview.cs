using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map.Component.Character;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Editor.DatabaseEditor.View.Preview
{
    public class CustomMovePreview : AbstractPreview
    {
        public enum PosId {
            Turn = 9999,
            TurnAndNext,
            Wait
        }
        protected GameObject _charaObj;
        List<Vector2Int> _posList = null;
        float _waitSeconds;
        List<FootprintGraphic> _footprintGraphicList = new List<FootprintGraphic>();
        Vector2Int _lastPos;
        class UpdateLoop
        {
            public void Stop() {
                _working = false;
            }
            public bool Working() {
                return _working;
            }
            bool _working = true;
        }

        UpdateLoop _updateLoop = null;

        public void CreateMap(MapDataModel mapDataModel, Vector2 pos, List<Vector2Int> posList, string eventId) {
            {
                //前回のリソースを破棄。
                if (_updateLoop != null)
                {
                    _updateLoop.Stop();
                    _updateLoop = null;
                }
                if (_charaObj != null) { Object.DestroyImmediate(_charaObj); }
                _charaObj = null;
                if (_actorOnMap != null) try { Object.DestroyImmediate(_actorOnMap); } catch (System.Exception) { }
                _actorOnMap = null;
                _posList?.Clear();
                _posList = null;
                foreach (var footprintGraphic in _footprintGraphicList)
                {
                    if (footprintGraphic != null) { Object.DestroyImmediate(footprintGraphic.gameObject); }
                }
                _footprintGraphicList.Clear();
            }
            _isAnimation = false;
            MapDataModel = mapDataModel;
            _eventId = eventId;
            _nowPos = pos;
            //_nextPos = nextPos;
            _posList = new List<Vector2Int>(posList);
            if (_mapPrefab == null)
            {
                _mapPrefab = MapDataModel.EditorDirectLoadMapPrefab();
            }
        }

        public void SetTargetId(string targetId) {
            _targetId = targetId;
            _characterGraphic?.ChangeAsset(GetAssetId());
        }

        // 対象ID設定
        protected override string GetAssetId() {
            var database = Editor.Hierarchy.Hierarchy.databaseManagementService;
            var eventManage = new EventManagementService();

            var assetId = "";
            // プレイヤー
            if (_targetId == "-2" &&
                database.LoadSystem().initialParty.party.Count > 0 &&
                database.LoadCharacterActor().Find(data => data.uuId == database.LoadSystem().initialParty.party[0]) != null)
                assetId = database.LoadCharacterActor().Find(data => data.uuId == database.LoadSystem().initialParty.party[0]).image.character;
            // このイベント
            else if (_targetId == "-1" &&
                eventManage.LoadEventMap().Find(data => data.eventId == _eventId) != null)
                if (eventManage.LoadEventMap().Find(data => data.eventId == _eventId).pages[0].condition.image.enabled == 1)
                    assetId = eventManage.LoadEventMap().Find(data => data.eventId == _eventId).pages[0].image.sdName;
                else
                    assetId = database.LoadCharacterActor().Find(data => data.uuId == eventManage.LoadEventMap().Find(data => data.eventId == _eventId).pages[0].condition.image.imageName).image.character;
            // 指定イベント
            else if (eventManage.LoadEventMap().Find(data => data.eventId == _targetId) != null)
                if (eventManage.LoadEventMap().Find(data => data.eventId == _targetId).pages[0].condition.image.enabled == 1)
                    assetId = eventManage.LoadEventMap().Find(data => data.eventId == _targetId).pages[0].image.sdName;
                else
                    assetId = database.LoadCharacterActor().Find(data => data.uuId == eventManage.LoadEventMap().Find(data => data.eventId == _targetId).pages[0].condition.image.imageName).image.character;

            return assetId;
        }

        /// <summary>
        ///     初期状態のUI設定
        /// </summary>
        public override void InitUi(SceneWindow scene, bool isChange = false) {
            if (_map == null)
            {
                _map = MapDataModel.InstantiateMapPrefab(MapDataModel, _mapPrefab);
                _map.transform.localPosition = new Vector3(0f, 0f, 0f);
                _map.transform.localScale = Vector3.one * 1f;
            }
            _sceneWindow = scene;
            SetUp();
            // プレビューシーンに移動
            scene.MoveGameObjectToPreviewScene(_map);
            SetCamera();
        }
        
        protected override void SetUp() {
            if (_actorOnMap == null)
            {
                _actorOnMap = new GameObject();
                _actorOnMap.gameObject.transform.SetParent(_map.transform);
                _map.transform.position = new Vector3(-0.25f, 0f, 10f);
            }

            if (_charaObj == null)
            {
                _charaObj = new GameObject();
                var canvas = _charaObj.AddComponent<Canvas>();
                canvas.sortingLayerName = "Editor_Event";
                _characterGraphic = _charaObj.AddComponent<CharacterGraphic>();
                _characterGraphic.gameObject.transform.SetParent(_actorOnMap.transform);
                _characterGraphic.Init(GetAssetId());
                _characterGraphic.gameObject.transform.position = new Vector3(0f, 0f, 0f);
            }
            //マップの裏に表示されるため、-3にしておく
            _actorOnMap.transform.position = new Vector3(_nowPos.x, _nowPos.y, -3f);
            _sceneWindow.Render();

            index = 0;
            _nextPos = currentPos;
            _moveFrequencyWaitStartTime = System.DateTime.Now;
        }

        FootprintGraphic PutFootprint(GameObject actorOnMap, Vector2Int pos, FootprintGraphic.TextureId textureId) {
            var footprintObj = new GameObject();
            var canvas = footprintObj.AddComponent<Canvas>();
            canvas.sortingLayerName = "Editor_Event";
            canvas.sortingOrder = -(600 + 1) + _footprintGraphicList.Count;
            var footprintGraphic = footprintObj.AddComponent<FootprintGraphic>();
            footprintObj.transform.SetParent(actorOnMap.transform.parent);
            footprintGraphic.Init(((int) textureId).ToString());
            footprintObj.transform.position = new Vector3(pos.x, pos.y, actorOnMap.transform.position.z);
            return footprintGraphic;
        }

        public override void Update() {
            if (_updateLoop == null)
            {
                _updateLoop = new UpdateLoop();
                UpdateAsync(_updateLoop);
            }
            base.Update();
        }

        async void UpdateAsync(UpdateLoop updateLoop) {
            while (updateLoop.Working())
            {
                if (_waitSeconds > 0)
                {
                    _waitSeconds -= 4.0f / 15;
                }
                if (_waitSeconds <= 0 && _posList != null && _posList.Count > 0)
                {
                    while (true)
                    {
                        if (index == 0)
                        {
                            foreach (var footprintGraphic in _footprintGraphicList)
                            {
                                Object.DestroyImmediate(footprintGraphic.gameObject);
                            }
                            _footprintGraphicList.Clear();
                        }
                        var pos = _posList[index];
                        if (pos.x == (int) CustomMovePreview.PosId.Wait)
                        {
                            var footprintGraphic = PutFootprint(_actorOnMap, _lastPos, FootprintGraphic.TextureId.Red);
                            _footprintGraphicList.Add(footprintGraphic);
                        }
                        else if (pos.x == (int) CustomMovePreview.PosId.TurnAndNext)
                        {
                            SetDirectionType((Commons.Direction.Id) pos.y);
                            SetDirection();
                            index = (index + 1) % _posList.Count;
                            continue;
                        }
                        else if (pos.x == (int) CustomMovePreview.PosId.Turn)
                        {
                            SetDirectionType((Commons.Direction.Id) pos.y);
                            SetDirection();
                            var footprintGraphic = PutFootprint(_actorOnMap, _lastPos, FootprintGraphic.TextureId.Blue);
                            _footprintGraphicList.Add(footprintGraphic);
                        }
                        else if (pos.x >= 0)
                        {
                            _actorOnMap.transform.position = new Vector3(pos.x, pos.y, _actorOnMap.transform.position.z);
                            var footprintGraphic = PutFootprint(_actorOnMap, pos, FootprintGraphic.TextureId.Blue);
                            _footprintGraphicList.Add(footprintGraphic);
                            _lastPos = pos;
                        }
                        break;
                    }
                    index = (index + 1) % _posList.Count;
                    _waitSeconds += 0.5f;
                }

                foreach (var footprintGraphic in _footprintGraphicList)
                {
                    footprintGraphic.Update();
                }
                await Task.Delay(1000 / 15);
            }
        }

        // 隣のタイルへの移動を設定。
        public override void DestroyLocalData() {
            //以下の破棄処理は、Unity側で既に破棄済みである可能性があるため try catch で括る
            if (_scrollChanvas != null) try { Object.DestroyImmediate(_scrollChanvas); } catch (System.Exception) { }
            if (_scrollText != null) try { Object.DestroyImmediate(_scrollText); } catch (System.Exception) { }
            if (_map != null) try { Object.DestroyImmediate(_map); } catch (System.Exception) { }
            if (_updateLoop != null)
            {
                _updateLoop.Stop();
                _updateLoop = null;
            }
            foreach (var footprintGraphic in _footprintGraphicList)
            {
                try { Object.DestroyImmediate(footprintGraphic.gameObject); } catch (System.Exception) { }
            }
            _footprintGraphicList.Clear();
            if (_charaObj != null) { Object.DestroyImmediate(_charaObj); }
            if (_actorOnMap != null) try { Object.DestroyImmediate(_actorOnMap); } catch (System.Exception) { }

            _characterGraphic = null;
            _scrollChanvas = null;
            _scrollText = null;
            _mapPrefab = null;
            _map = null;
            _charaObj = null;
            _actorOnMap = null;
            _posList?.Clear();
            _posList = null;
        }

        public class FootprintGraphic : MonoBehaviour
        {
            static Sprite[] _defaultSpriteArr = null;
            static string[] _defaultSpriteNameArr = new string[] { "IconSet_165", "IconSet_162" };
            public enum TextureId
            {
                Blue,
                Red
            };
            protected readonly float TILE_SIZE = 96;
            protected Sprite _currentSprite;

            // 状態プロパティ
            //--------------------------------------------------------------------------------------------------------------
            protected float _growSeconds = 0;

            // 表示要素プロパティ
            //--------------------------------------------------------------------------------------------------------------
            protected Image _image;

            // データプロパティ
            //--------------------------------------------------------------------------------------------------------------
            protected Material _material;

            // 関数プロパティ
            //--------------------------------------------------------------------------------------------------------------
            /// <summary>
            /// 各画面でSDキャラ画像の1枚目を表示するためのプロパティ
            /// </summary>
            /// <returns></returns>
            public virtual Sprite GetCurrentSprite() {
                return _currentSprite;
            }

            /// <summary>
            /// 各画面でSDキャラ画像の1枚目を表示するためのプロパティ
            /// </summary>
            /// <returns></returns>
            public virtual Material GetMaterial() {
                return _material;
            }

            /// <summary>
            /// 各画面でSDキャラ画像の1枚目を表示するためのプロパティ
            /// </summary>
            /// <returns></returns>
            public virtual Vector2 GetSize() {
                return _image.transform.localScale;
            }

            static float _offsetX = 0.15f;
            static float _offsetY = -0.1f;
            // initialize methods
            //--------------------------------------------------------------------------------------------------------------
            /**
             * 初期化
             */
            public virtual void Init(string assetId) {
                if (assetId == "") return;
                SetUpSprites(assetId);

                // コンポーネント設定
                gameObject.GetOrAddComponent<Canvas>();
                gameObject.name = "actor";
                _image = gameObject.AddComponent<Image>();
                _material = new Material(_image.material.shader);
                _image.material = _material;

                // サイズ、位置設定
                transform.GetComponent<RectTransform>().pivot = new Vector2(0, 0);
                transform.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
                transform.position = new Vector3(0, 0, 0);
                var id = int.Parse(assetId);
                _currentSprite = _defaultSpriteArr[id];
                _image.sprite = _currentSprite;
                if (_currentSprite?.texture != null)
                {
                    SetSize(new Vector2(
                        _currentSprite.texture.width / TILE_SIZE,
                        _currentSprite.texture.height / TILE_SIZE));
                    transform.GetComponent<RectTransform>().pivot = new Vector2(_offsetX, _offsetY);
                    transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(_offsetX, _offsetY);
                }

                _growSeconds = 0.5f;
                SetSize(new Vector2(
                    _currentSprite.texture.width / TILE_SIZE * 0.6f,
                    _currentSprite.texture.height / TILE_SIZE * 0.6f));
            }

            private void FixedUpdate() {
            }

            public void Update() {
                if (_growSeconds > 0)
                {
                    _growSeconds = Mathf.Max(0, _growSeconds - 20.0f / Runtime.Common.Commons.Fps);
                    var scale = 1.0f - _growSeconds * (0.4f / 0.5f);
                    SetSize(new Vector2(
                        _currentSprite.texture.width / TILE_SIZE * scale,
                        _currentSprite.texture.height / TILE_SIZE * scale));
                }
            }

            public virtual void ChangeAsset(string assetId) {
                // 画像が設定されていなければ初期化
                if (_image == null)
                {
                    Init(assetId);
                }

                if (_image != null && _image.enabled == false)
                    _image.enabled = true;

                SetUpSprites(assetId);
                var id = int.Parse(assetId);
                _currentSprite = _defaultSpriteArr[id];
                _image.sprite = _currentSprite;
                ChangeTextureSize();
                Render();
            }


            public virtual void Reset() {
                ChangeTextureSize();
                Render();
            }

            protected virtual void ChangeTextureSize() {
                var spriteData = _currentSprite;

                if (spriteData?.texture != null)
                    SetSize(new Vector2(
                        spriteData.texture.width / TILE_SIZE,
                        spriteData.texture.height / TILE_SIZE));
            }

            protected virtual void Render() {
                if (_image == null) return;
                // テクスチャがnullであればImageを無効化する
                if (_currentSprite == null)
                {
                    _image.enabled = false;
                    return;
                }

                _image.sprite = _currentSprite;
            }

            protected virtual void SetUpSprites(string assetId) {
                if (_defaultSpriteArr == null)
                {
                    _defaultSpriteArr = new Sprite[_defaultSpriteNameArr.Length];
                    for (int i = 0; i < _defaultSpriteNameArr.Length; i++)
                    {
                        var texture2D = ImageUtility.LoadImageFileToTexture($"Assets/RPGMaker/Storage/Images/System/IconSet/{_defaultSpriteNameArr[i]}.png");
                        var sprite = Sprite.Create(
                            texture2D,
                            new Rect(0, 0, texture2D.width, texture2D.height),
                            new Vector2(0.5f, 0.5f)
                        );
                        _defaultSpriteArr[i] = sprite;

                    }
                }
            }

            // テクスチャUVの設定
            // start:開始UV位置
            // end:終了UV位置
            protected virtual void SetTextureUV(Vector2 start, Vector2 end) {
                // 数値調整して代入
                if (_material == null)
                {
                    _material = new Material(_image.material.shader);
                }
                _material.mainTextureOffset = new Vector2(start.x, start.y);
                _material.SetTextureScale("_MainTex", new Vector2(end.x - start.x, end.y));
            }

            // サイズ設定(1.0が100%)
            protected virtual void SetSize(Vector2 size) {
                _image.transform.localScale = size;
            }

            //キャラクターの画像を読み込み直す部分
            //読み込み直すassetsIDが入ります
            public virtual void ReloadCharacterImage(string id) {
                //処理が重複してしまったため「SetUpSprites」を呼び出しています
                SetUpSprites(id);
            }

            /// <summary>
            ///     画像の表示を有効にするかどうかの切り替え
            /// </summary>
            /// <param name="enable">有効か無効か</param>
            public virtual void SetImageEnable(bool enable) {
                if (_currentSprite != null && _image != null)
                    _image.enabled = enable;
                else if (_image != null)
                    _image.enabled = false;
            }

            /// <summary>
            ///     透明状態の設定
            /// </summary>
            /// <param name="transparent">trueで透明、falseで不透明</param>
            public virtual void SetTransparent(bool transparent) {
                if (_image == null)
                    return;

                var alpha = transparent ? 0f : 1f;
                _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, alpha);
            }

            /// <summary>
            ///     キャラクター用画像の読込
            /// </summary>
            /// <param name="fileName">画像のファイル名</param>
            /// <returns>読み込まれた画像のSprite</returns>
            protected virtual Sprite LoadCharacterSprite(string fileName) {
                if (string.IsNullOrEmpty(fileName)) return null;

                return UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                    "Assets/RPGMaker/Storage/Images/Characters/" + fileName);
            }

            /// <summary>
            ///     オブジェクト用画像の読込
            /// </summary>
            /// <param name="fileName">画像のファイル名</param>
            /// <returns>読み込まれた画像のSprite</returns>
            protected virtual Sprite LoadObjectSprite(string fileName) {
                if (string.IsNullOrEmpty(fileName)) return null;

                return UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                    "Assets/RPGMaker/Storage/Images/Objects/" + fileName);
            }

            // enum / interface / local class
            //--------------------------------------------------------------------------------------------------------------
            protected class SpriteDataModel
            {
                public SpriteDataModel(Sprite sprite, int animationFrame, int animationSpeed) {
                    this.sprite = sprite;
                    this.animationFrame = animationFrame;
                    this.animationSpeed = animationSpeed;
                }

                public Sprite sprite { get; set; }
                public int animationFrame { get; }
                public int animationSpeed { get; }
            }
        }
    }
}