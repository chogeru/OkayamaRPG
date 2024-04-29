using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.CoreSystem.Service.RuntimeDataManagement;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Enum;
using RPGMaker.Codebase.Runtime.Map.Component.Character;
using RPGMaker.Codebase.Runtime.Map.Item;
using RPGMaker.Codebase.Runtime.Title;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Map.Menu
{
    /// <summary>
    ///     タイトル画面またはゲームのメインメニューから開くセーブロード画面
    /// </summary>
    public class SaveMenu : WindowBase
    {
        /// <summary>
        ///     セーブロード画面で行う操作
        /// </summary>
        public enum Operation
        {
            /// <summary>ゲームデータのセーブ</summary>
            Save,

            /// <summary>セーブデータのロード</summary>
            Load
        }

        public const            int             SAVE_DATA_NUM_MAX = 20;
        [SerializeField] private TextMeshProUGUI _descriptionTitle = null;
        [SerializeField] private Button          _downArrowButton  = null;
        private                  RectTransform   _itemPrefabRect;
        private                  MenuBase        _menuBase;
        private                  Operation       _operation      = Operation.Load;
        [SerializeField] private SaveItem        _saveItem       = null;
        [SerializeField] private RectTransform   _saveItemParent = null;

        private readonly List<SaveItem> _saveItems = new List<SaveItem>();

        [SerializeField] private ScrollRect    _scrollRect     = null;
        [SerializeField] private Button        _upArrowButton  = null;

        public override void Update() {
            base.Update();

            if (InputHandler.OnDown(Common.Enum.HandleType.Back) && gameObject.activeSelf)
            {
                SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, systemSettingDataModel.soundSetting.cancel);
                SoundManager.Self().PlaySe();

                // タイトル画面で開いている場合の処理
                if (_operation == Operation.Load)
                {
                    var titleController = GetComponentInParent<TitleController>();
                    titleController.CloseContinue();
                }
            }
        }

        public void Init(WindowBase manager) {
            base.Init();

            switch (SceneManager.GetActiveScene().name)
            {
                //タイトル画面の場合はロード画面を表示
                case "Title":
                    _descriptionTitle.text = TextManager.loadMessage;
                    _operation = Operation.Load;
                    break;
                //その他ケースではセーブ画面を表示
                default:
                    _descriptionTitle.text = TextManager.saveMessage;
                    _operation = Operation.Save;
                    break;
            }

            _menuBase = manager as MenuBase;
            _itemPrefabRect = _saveItem.GetComponent<RectTransform>();

            //キー登録
            _gameState = GameStateHandler.GameState.MENU;

            if (MapEventExecutionController.Instance.IsPauseEvent())
            {
                InputDistributor.AddInputHandler(GameStateHandler.GameState.EVENT, HandleType.Back, Back, "SaveMenuBack");
                InputDistributor.AddInputHandler(GameStateHandler.GameState.EVENT, HandleType.RightClick, Back, "SaveMenuBack");
            }

            //画面が生成されるまで1フレーム待ち
            TimeHandler.Instance.AddTimeActionFrame(1, SetupSaveDataItem, false);
        }

        public new void Back() {
            if (MapEventExecutionController.Instance.IsPauseEvent() && MenuBase.IsEventToSave)
            {
                InputDistributor.RemoveInputHandler(GameStateHandler.GameState.EVENT, HandleType.Back, Back, "SaveMenuBack");
                InputDistributor.RemoveInputHandler(GameStateHandler.GameState.EVENT, HandleType.RightClick, Back, "SaveMenuBack");
                gameObject.SetActive(false);

                if (_menuBase != null)
                    _menuBase?.AllClose();
                else
                    ResumeEvent();

                MenuBase.IsEventToSave = false;
                return;
            }

            _menuBase?.BackMenu();
        }

        private void ResumeEvent() {
            MapEventExecutionController.Instance.ResumeEvent();
        }

        /// <summary>
        ///     セーブデータの総数を取得する
        /// </summary>
        /// <returns>パスにfileが含まれるjsonファイルの個数</returns>
        public int GetSaveQuantity() {
            var runtimeDataManagementService = new RuntimeDataManagementService();
            return runtimeDataManagementService.GetSaveFileCount();
        }
        
        public bool IsAutoSaveFile() {
            var runtimeDataManagementService = new RuntimeDataManagementService();
            return runtimeDataManagementService.IsAutoSaveFile();
        }

        /// <summary>
        ///     セーブデータの配置
        /// </summary>
        private void SetupSaveDataItem() {
            _saveItems.ForEach(v => Destroy(v.gameObject));
            _saveItems.Clear();
            transform.Find("MenuArea/FileLoad/Scroll View").gameObject.SetActive(true);

            // オートセーブ用の項目を作成
            if (systemSettingDataModel.optionSetting.enabledAutoSave == 1)
            {
                var saveData = GetSaveData("file0");
                var saveItem = CreateSaveItem(saveData, 0);
                _saveItems.Add(saveItem);
            }

            // ファイル1~20のセーブデータ項目を作成
            for (var i = 1; i <= SAVE_DATA_NUM_MAX; i++)
            {
                var saveData = GetSaveData($"file{i}");
                var saveItem = CreateSaveItem(saveData, i);
                _saveItems.Add(saveItem);
            }

            //十字キーでの操作登録
            for (var i = 0; i < _saveItems.Count; i++)
            {
                var nav = _saveItems[i].ItemButton.navigation;
                nav.mode = Navigation.Mode.Explicit;
                nav.selectOnUp = _saveItems[i == 0 ? _saveItems.Count - 1 : i - 1].ItemButton;
                nav.selectOnDown = _saveItems[(i + 1) % _saveItems.Count].ItemButton;
                _saveItems[i].ItemButton.navigation = nav;
            }

            _saveItemParent.sizeDelta =
                new Vector2(_saveItemParent.sizeDelta.x, (_itemPrefabRect.rect.height + 20) * _saveItems.Count);
            _saveItems[0].ItemButton.Select();
        }

        /// <summary>
        ///     セーブデータ項目の作成
        /// </summary>
        /// <param name="runtimeSaveDataModel">参照するセーブデータ</param>
        /// <param name="number">セーブデータの番号、0でオートセーブ用</param>
        private SaveItem CreateSaveItem(RuntimeSaveDataModel saveData, int number) {
            var saveItem = Instantiate(_saveItem, _saveItemParent.transform);
            saveItem.Init(saveData, number, _operation);

            //OnFocus、OnClick追加
            saveItem.GetComponent<WindowButtonBase>().OnFocus = new UnityEngine.Events.UnityEvent();
            saveItem.GetComponent<WindowButtonBase>().OnFocus.AddListener(() =>
            {
                _selectedItem = saveItem.gameObject;
                CheckArrowButton();
            });
            saveItem.GetComponent<WindowButtonBase>().OnClick = new Button.ButtonClickedEvent();
            saveItem.GetComponent<WindowButtonBase>().OnClick.AddListener(() =>
            {
                OnSaveItemClicked(saveItem);
            });

            //スクロール制御の追加
            saveItem.GetComponent<WindowButtonBase>().ScrollView = transform.Find("MenuArea/FileLoad/Scroll View").gameObject;
            saveItem.GetComponent<WindowButtonBase>().Content = transform.Find("MenuArea/FileLoad/Scroll View/Viewport/Content").gameObject;

            //ロード、セーブについてはブザー音は本クラス内で鳴動させる
            saveItem.GetComponent<WindowButtonBase>().SetSilentClick(true);

            saveItem.gameObject.SetActive(true);
            return saveItem;
        }

        /// <summary>
        ///     ローカルに保存されているセーブデータの取得
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <returns>取得成功でセーブデータを返す。失敗でnull</returns>
        private RuntimeSaveDataModel GetSaveData(string fileName) {
            if (string.IsNullOrEmpty(fileName)) return null;

            //セーブデータを手動で編集されたなどの理由で、JSONとして不正な状態になっているセーブデータが
            //存在した場合に読み飛ばすため、try catch で括る
            RuntimeSaveDataModel retData = null;
            try
            {
                var runtimeDataManagementService = new RuntimeDataManagementService();
                var str = runtimeDataManagementService.LoadSaveData(fileName);
                if (str == null) return null;

                // 型を合わせる
                var inputString = new TextAsset(str);
                retData = JsonUtility.FromJson<RuntimeSaveDataModel>(inputString.text);
            } catch (Exception) {}

            return retData;
        }

        /// <summary>
        ///     セーブデータ項目をクリックした際に呼び出すコールバック
        /// </summary>
        private void OnSaveItemClicked(SaveItem clickedItem) {
            ButtonProcessing(null, clickedItem.gameObject);

            if (_operation == Operation.Save)
            {
                if (clickedItem.SaveFileNo != 0)
                {
                    //セーブした回数をインクリメント
                    DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.saveCount++;

                    //現在再生されているBGMを保存する
                    if (SoundManager.Self().IsNowBgmPlaying() && SoundManager.Self().GetBgmSound() != null)
                    {
                        var sound = SoundManager.Self().GetBgmSound();
                        DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.bgmOnSave.name = sound.name;
                        DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.bgmOnSave.volume = sound.volume;
                        DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.bgmOnSave.pitch = sound.pitch;
                        DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.bgmOnSave.pan = sound.pan;
                    }
                    else
                    {
                        DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.bgmOnSave.name = "";
                    }
                    
                    //現在再生されているBGSを保存する
                    if (SoundManager.Self().IsNowBgsPlaying() && SoundManager.Self().GetBgsSound() != null)
                    {
                        var sound = SoundManager.Self().GetBgsSound();
                        DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.bgsOnSave.name = sound.name;
                        DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.bgsOnSave.volume = sound.volume;
                        DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.bgsOnSave.pitch = sound.pitch;
                        DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.bgsOnSave.pan = sound.pan;
                    }
                    else
                    {
                        DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.bgsOnSave.name = "";
                    }

                    // 現在のマップ配置データを保存
                    {
                        RuntimeOnMapDataModel RuntimeOnMapDataModel = new RuntimeOnMapDataModel();
                        // 現在のマップイベント
                        foreach (var ev in MapEventExecutionController.Instance.GetEvents())
                            RuntimeOnMapDataModel.onMapDatas.Add(new RuntimeOnMapDataModel.OnMapData().Create(ev));
                        // パーティ
                        for (int i = 0; i < MapManager.PartyOnMap?.Count; i++)
                            RuntimeOnMapDataModel.onMapDatas.Add(new RuntimeOnMapDataModel.OnMapData().Create(MapManager.GetPartyGameObject(i).GetComponent<ActorOnMap>(), i + 1));
                        // プレイヤー
                        RuntimeOnMapDataModel.onMapDatas.Add(
                            new RuntimeOnMapDataModel.OnMapData().Create(MapManager.GetCharacterGameObject().GetComponent<ActorOnMap>(), 0));

                        DataManager.Self().GetRuntimeSaveDataModel().RuntimeOnMapDataModel = RuntimeOnMapDataModel;
                    }

                    // 現在のマップ配置データを保持（イベント外）
                    {
                        //タイマー
                        float timer = -1;
                        var obj = HudDistributor.Instance.NowHudHandler().GetGameTimer();
                        if (obj != null)
                        {
                            timer = obj.GetGameTimer();
                        }
                        DataManager.Self().GetRuntimeSaveDataModel().runtimeScreenDataModel.timer = timer;

                        //ピクチャ
                        HudDistributor.Instance.NowHudHandler().SavePicture();
                    }

                    // 有効な項目にセーブを実施
                    var runtimeDataManagementService = new RuntimeDataManagementService();
                    var data = DataManager.Self().GetRuntimeSaveDataModel();
                    runtimeDataManagementService.SaveSaveData(data, clickedItem.SaveFileNo);
                    clickedItem.Refresh(data);

                    //セーブ音
                    SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, systemSettingDataModel.soundSetting.save);
                    SoundManager.Self().PlaySe();
                }
                else
                {
                    //オートセーブの項目にはセーブさせない
                    //ブザー音
                    SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, systemSettingDataModel.soundSetting.buzzer);
                    SoundManager.Self().PlaySe();
                }
            }
        }

        public void ButtonProcessing(OnComplete callback, GameObject obj) {
            SaveItem saveItem = null;
            for (int i = 0; i < _saveItems.Count; i++)
                if (_saveItems[i].gameObject == _selectedItem)
                {
                    saveItem = _saveItems[i];
                    break;
                }

            if (_operation == Operation.Load)
            {
                var data = DataManager.Self().LoadSaveData(saveItem.SaveFileNo);
                if (data != null)
                {
                    DataManager.Self().ReloadGameParty();
                    // セーブデータの読込成功
                    SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, systemSettingDataModel.soundSetting.load);
                    SoundManager.Self().PlaySe();
                    SceneManager.LoadScene("SceneMap");
                    transform.parent.gameObject.SetActive(false);

                    //ロード音
                    SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, systemSettingDataModel.soundSetting.load);
                    SoundManager.Self().PlaySe();
                }
                else
                {
                    //セーブデータの読込失敗
                    //ブザー音
                    SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, systemSettingDataModel.soundSetting.buzzer);
                    SoundManager.Self().PlaySe();
                }
            }
        }

        /// <summary>
        ///     下向き矢印ボタン押下時のコールバック
        /// </summary>
        public void OnDownArrowClicked() {
            if (_selectedItem == null)
                return;

            var index = _saveItems.FindIndex(v => _selectedItem == v.gameObject);
            if (index == -1) return;
            index = Math.Min(index + 1, _saveItems.Count);

            var saveItem = _saveItems[index];
            saveItem.ItemButton.Select();
        }

        /// <summary>
        ///     上向き矢印ボタン押下時のコールバック
        /// </summary>
        public void OnUpArrowClicked() {
            if (_selectedItem == null) return;

            var index = _saveItems.FindIndex(v => _selectedItem == v.gameObject);
            if (index == -1) return;
            index = Math.Max(index - 1, 0);

            var saveItem = _saveItems[index];
            saveItem.ItemButton.Select();
        }

        /// <summary>
        ///     矢印ボタンの表示について確認を行う
        /// </summary>
        /// <param name="index">選択中の項目のインデックス。選択中の項目が無ければ-1を格納する</param>
        private void CheckArrowButton() {
            _upArrowButton.gameObject.SetActive(!Mathf.Approximately(_scrollRect.verticalNormalizedPosition, 1f));
            _downArrowButton.gameObject.SetActive(!Mathf.Approximately(1f - _scrollRect.verticalNormalizedPosition,
                1f));
        }

#if UNITY_EDITOR
        Button _saveButton;

        public void SaveFocused() {
            for (var i = 0; i < _saveItems.Count; i++)
            {
                var button = _saveItems[i].ItemButton.GetComponent<WindowButtonBase>();
                if (button.IsHighlight())
                {
                    _saveButton = _saveItems[i].ItemButton.GetComponent<Button>();
                    break;
                }
            }
        }

        public async void ChangeFocused() {
            //少し待たないとフォーカスが移らないため、待つ
            await Task.Delay(10);

            //フォーカス再設定処理
            _saveButton?.Select();
        }
#endif
    }
}