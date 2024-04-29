using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map.Component.Character;
using RPGMaker.Codebase.Runtime.Map.Menu;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Map.Item
{
    /// <summary>
    ///     セーブデータ一覧の各項目を制御するコンポーネント
    /// </summary>
    public class SaveItem : MonoBehaviour, ISelectHandler
    {
        [SerializeField] private TextMeshProUGUI _file       = null;
        [SerializeField] private List<Image>     _images     = null;
        [SerializeField] private Button          _itemButton = null;
        [SerializeField] private TextMeshProUGUI _number     = null;
        [SerializeField] private TextMeshProUGUI _playTime   = null;

        public Button ItemButton => _itemButton;
        public SelectEvent OnItemSelect { get; } = new SelectEvent();
        public int SaveFileNo { get; private set; } = -1;
        private List<GameObject> characterGameObjects = new List<GameObject>();

        public void OnSelect(BaseEventData eventData) {
            ((ISelectHandler) _itemButton).OnSelect(eventData);
            OnItemSelect?.Invoke(this);
        }

        /// <summary>
        ///     生成時の初期化処理
        /// </summary>
        /// <param name="runtimeSaveDataModel">参照するセーブデータ</param>
        /// <param name="number">セーブデータの番号、0でオートセーブ用</param>
        /// <param name="operation">どの操作を行っているか</param>
        public void Init(RuntimeSaveDataModel runtimeSaveDataModel, int number, SaveMenu.Operation operation) {
            gameObject.name = "SaveItem : " + (number > 0 ? number.ToString() : "AutoSave");

            _file.text = number > 0 ? TextManager.file : "AutoSave";
            _number.text = number > 0 ? number.ToString() : "";
            _playTime.text = "";
            SaveFileNo = number;

            var hasSaveData = runtimeSaveDataModel != null;
            Refresh(runtimeSaveDataModel);
            switch (operation)
            {
                case SaveMenu.Operation.Save:
                    // マップから開いた場合はオートセーブを無効にする
                    SetInteractable(number != 0, hasSaveData);
                    break;
                case SaveMenu.Operation.Load:
                    // タイトルから開いた場合はデータが格納されていない項目を無効にする
                    SetInteractable(hasSaveData, hasSaveData);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        ///     表示内容の更新
        /// </summary>
        /// <param name="saveData">参照するセーブデータ</param>
        public void Refresh(RuntimeSaveDataModel saveData) {
            if (saveData == null) return;

            float playTime = saveData.runtimeSystemConfig.playTime;
            int characterType = DataManager.Self().GetUiSettingDataModel().commonMenus[0].characterType;
            _playTime.text = 
                Mathf.Floor(playTime / 3600).ToString() + ":" + 
                Mathf.Floor(playTime % 3600 / 60).ToString("00") + ":" + 
                Mathf.Floor(playTime % 60).ToString("00");

            //現在パーティにいるメンバーを表示する
            for (var actorNum = 0; actorNum < saveData.runtimePartyDataModel.actors.Count; actorNum++)
            {
                for (var i = 0; i < saveData.runtimeActorDataModels.Count; i++)
                {
                    //素材がなくなっているなどの理由で表示できない場合は処理を飛ばすため、try catch でくくる
                    try
                    {
                        if (saveData.runtimeActorDataModels[i].actorId == saveData.runtimePartyDataModel.actors[actorNum])
                        {
                            // 顔アイコンを読み込む。セーブロードのUIパターンは顔画像のみ
                            if (_images.Count > actorNum)
                            {
                                if (characterType == (int) MenuIconTypeEnum.FACE)
                                {
                                    //顔アイコン
                                    var sprite = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(
                                        "Assets/RPGMaker/Storage/Images/Faces/" +
                                        saveData.runtimeActorDataModels[i].faceImage + ".png");
                                    _images[actorNum].enabled = true;
                                    _images[actorNum].sprite = sprite;
                                    _images[actorNum].color = Color.white;
                                    _images[actorNum].preserveAspect = true;
                                }
                                else if (characterType == (int) MenuIconTypeEnum.SD)
                                {
                                    //SDキャラ
                                    var assetId = DataManager.Self().GetActorDataModel(saveData.runtimeActorDataModels[i].actorId).image.character;
                                    CharacterGraphic characterGraphic = new GameObject().AddComponent<CharacterGraphic>();
                                    characterGraphic.Init(assetId);

                                    _images[actorNum].enabled = true;
                                    _images[actorNum].sprite = characterGraphic.GetCurrentSprite();
                                    _images[actorNum].color = Color.white;
                                    _images[actorNum].material = characterGraphic.GetMaterial();
                                    _images[actorNum].transform.localScale = characterGraphic.GetSize();

                                    if (_images[actorNum].transform.localScale.x > 1.0f || _images[actorNum].transform.localScale.y > 1.0f)
                                    {
                                        if (_images[actorNum].transform.localScale.y > 1.0f)
                                        {
                                            _images[actorNum].transform.localScale = new Vector2(_images[actorNum].transform.localScale.x / _images[actorNum].transform.localScale.y, 1.0f);
                                        }
                                        else
                                        {
                                            _images[actorNum].transform.localScale = new Vector2(1.0f, _images[actorNum].transform.localScale.y / _images[actorNum].transform.localScale.x);
                                        }
                                    }

                                    _images[actorNum].rectTransform.pivot = new Vector2(0.5f, 1);
                                    characterGraphic.gameObject.SetActive(false);
                                    characterGameObjects.Add(characterGraphic.gameObject);
                                }
                                else if (characterType == (int) MenuIconTypeEnum.PICTURE)
                                {
                                    //立ち絵
                                    var imageName = saveData.runtimeActorDataModels[i].advImage.Contains(".png")
                                        ? saveData.runtimeActorDataModels[i].advImage
                                        : saveData.runtimeActorDataModels[i].advImage + ".png";
                                    var path = "Assets/RPGMaker/Storage/Images/Pictures/" + imageName;
                                    var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(path);

                                    _images[actorNum].enabled = true;
                                    _images[actorNum].sprite = tex;
                                    _images[actorNum].color = Color.white;
                                    _images[actorNum].preserveAspect = true;
                                }
                                else
                                {
                                    _images[actorNum].enabled = false;
                                    _images[actorNum].sprite = null;
                                }
                            }
                        }
                    }
                    catch (Exception) { }
                }
            }
            for (var actorNum = saveData.runtimePartyDataModel.actors.Count; actorNum < _images.Count; actorNum++)
            {
                _images[actorNum].enabled = false;
                _images[actorNum].sprite = null;
            }
        }

        /// <summary>
        ///     押下可能かどうかを設定する
        /// </summary>
        /// <param name="interactable">押下可能か</param>
        /// <param name="hasSaveData">対応したセーブデータを表示しているか</param>
        private void SetInteractable(bool interactable, bool hasSaveData) {
            var tmpColor = interactable ? Color.white : Color.gray;

            _file.color = tmpColor;
            _number.color = tmpColor;
            _playTime.color = tmpColor;

            if (hasSaveData)
                _images.ForEach(v => v.color = tmpColor);
        }

        private void OnDestroy() {
            _itemButton.onClick.RemoveAllListeners();
            OnItemSelect.RemoveAllListeners();
            characterGameObjects.ForEach(data => DestroyImmediate(data));
            characterGameObjects.Clear();
        }


        public class SelectEvent : UnityEvent<SaveItem>
        {
        }
    }
}