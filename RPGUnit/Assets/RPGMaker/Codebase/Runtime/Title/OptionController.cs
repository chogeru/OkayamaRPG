using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.CoreSystem.Service.RuntimeDataManagement;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map;
using RPGMaker.Codebase.Runtime.Map.Component.Character;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TextMP = TMPro.TextMeshProUGUI;


namespace RPGMaker.Codebase.Runtime.Title
{
    /// <summary>
    /// オプション画面処理
    /// </summary>
    public class OptionController : WindowBase
    {
        //キャラクターのダッシュ時の加速倍率
        [SerializeField] private Button _alwaysDashButton = null;

        [SerializeField] private Text   _alwaysDashItemName = null;
        [SerializeField] private TextMP _alwaysDashValue    = null;

        [SerializeField] private Text   _bgmVolumeItemName = null;
        [SerializeField] private TextMP _bgmVolumeValue    = null;

        [SerializeField] private Text   _bgsVolumeItemName     = null;
        [SerializeField] private TextMP _bgsVolumeValue        = null;

        [SerializeField] private Text   _commandRememberItemName = null;
        [SerializeField] private TextMP _commandRememberValue    = null;

        private MenuBase _menuBase;

        [SerializeField] private Text   _meVolumeItemName = null;
        [SerializeField] private TextMP _meVolumeValue    = null;

        //オプション用追加
        [SerializeField] private GameObject                   _optionRoot;
        private                  RuntimeConfigDataModel       _runtimeConfigDataModel;
        private                  RuntimeDataManagementService _runtimeDataManagementService;

        [SerializeField] private Text   _seVolumeItemName = null;
        [SerializeField] private TextMP _seVolumeValue    = null;

        //タイトル用
        [SerializeField] private TitleController _titleController;

#if UNITY_SWITCH
        bool _changed = false;  // some options changed but not saved.
#endif
        public override void Update() {
            base.Update();
            if (GameStateHandler.CurrentGameState() == GameStateHandler.GameState.TITLE && InputHandler.OnDown(Common.Enum.HandleType.Back) && _optionRoot.activeSelf) 
                Back();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="manager"></param>
        public void Init(WindowBase manager) {
            base.Init();

            _menuBase = manager as MenuBase;
            _runtimeDataManagementService = new RuntimeDataManagementService();
            _runtimeConfigDataModel = _runtimeDataManagementService.LoadConfig();

            _alwaysDashItemName.text = TextManager.alwaysDash;
            _commandRememberItemName.text = TextManager.commandRemember;
            _bgmVolumeItemName.text = TextManager.bgmVolume;
            _bgsVolumeItemName.text = TextManager.bgsVolume;
            _meVolumeItemName.text = TextManager.meVolume;
            _seVolumeItemName.text = TextManager.seVolume;
#if UNITY_SWITCH
            _changed = false;
#endif

            SoundManager.Self().Init();
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, systemSettingDataModel.soundSetting.ok);

            OpenOption();
        }

        /// <summary>
        /// オプション表示処理
        /// </summary>
        public void OpenOption() {
            //オプションのメニューを表示する
            _optionRoot.SetActive(true);

            //コンフィグデータの反映部分
            _alwaysDashValue.text = _runtimeConfigDataModel.alwaysDash == 1 ? "ON" : "OFF";
            _commandRememberValue.text = _runtimeConfigDataModel.commandRemember == 1 ? "ON" : "OFF";

            _bgmVolumeValue.text = _runtimeConfigDataModel.bgmVolume + "%";
            _bgsVolumeValue.text = _runtimeConfigDataModel.bgsVolume + "%";
            _meVolumeValue.text = _runtimeConfigDataModel.meVolume + "%";
            _seVolumeValue.text = _runtimeConfigDataModel.seVolume + "%";

            SetButtonEnabled(_bgmVolumeValue.transform.parent.Find("Down").GetComponent<WindowButtonBase>(), false, _runtimeConfigDataModel.bgmVolume == 0);
            SetButtonEnabled(_bgmVolumeValue.transform.parent.Find("Up").GetComponent<WindowButtonBase>(), true, _runtimeConfigDataModel.bgmVolume == 100);
            SetButtonEnabled(_bgsVolumeValue.transform.parent.Find("Down").GetComponent<WindowButtonBase>(), false, _runtimeConfigDataModel.bgsVolume == 0);
            SetButtonEnabled(_bgsVolumeValue.transform.parent.Find("Up").GetComponent<WindowButtonBase>(), true, _runtimeConfigDataModel.bgsVolume == 100);
            SetButtonEnabled(_meVolumeValue.transform.parent.Find("Down").GetComponent<WindowButtonBase>(), false, _runtimeConfigDataModel.meVolume == 0);
            SetButtonEnabled(_meVolumeValue.transform.parent.Find("Up").GetComponent<WindowButtonBase>(), true, _runtimeConfigDataModel.meVolume == 100);
            SetButtonEnabled(_seVolumeValue.transform.parent.Find("Down").GetComponent<WindowButtonBase>(), false, _runtimeConfigDataModel.seVolume == 0);
            SetButtonEnabled(_seVolumeValue.transform.parent.Find("Up").GetComponent<WindowButtonBase>(), true, _runtimeConfigDataModel.seVolume == 100);

            // 先頭のボタンを選択しておく
            _alwaysDashButton.Select();
        }

        /// <summary>
        /// 常時ダッシュON/OFF
        /// </summary>
        public void AlwaysDashEvent() {
            var value = _alwaysDashValue.text;
            var toggleOff = value == "ON";
            _alwaysDashValue.text = toggleOff ? "OFF" : "ON";
            _runtimeConfigDataModel.alwaysDash = toggleOff ? 0 : 1;

            // マップから開いた場合（タイトル用のコンポーネントが格納されていない場合）はマップ用の処理を実施
            if (_titleController == null)
            {
                //常時ダッシュON/OFF設定による、全プレイヤーキャラクターのダッシュ設定
                MapManager.OperatingActor.GetComponent<CharacterOnMap>().SetDash(!toggleOff);
                for (var i = 0; i < MapManager.GetPartyMemberNum(); i++)
                {
                    MapManager.GetPartyGameObject(i).GetComponent<CharacterOnMap>().SetDash(!toggleOff);
                }
            }

            MapManager.SetRuntimeConfigDataModel(_runtimeConfigDataModel);
#if !UNITY_SWITCH
            _Save();
#else
            _changed = true;
#endif
        }

        /// <summary>
        /// コマンド記憶するのON/OFF
        /// </summary>
        public void CommandRememberEvent() {
            if (gameObject == null || gameObject.activeSelf == false) return;
            var value = _commandRememberValue.text;
            if (value == "ON")
            {
                _commandRememberValue.text = "OFF";
                _runtimeConfigDataModel.commandRemember = 0;
            }
            else if (value == "OFF")
            {
                _commandRememberValue.text = "ON";
                _runtimeConfigDataModel.commandRemember = 1;
            }
            MapManager.SetRuntimeConfigDataModel(_runtimeConfigDataModel);
            DataManager.Self().GetRuntimeConfigDataModel().commandRemember = _runtimeConfigDataModel.commandRemember;
#if !UNITY_SWITCH
            _Save();
#else
            _changed = true;
#endif
        }

        /// <summary>
        ///     BGMの音量調整ボタン押下時のコールバック
        /// </summary>
        /// <param name="_target">押下したボタン</param>
        public void OnChangeBgmVolumeClicked(Button _target) {
            var value = 0;
            if (_target.name == "Up")
                value = 20;
            else if (_target.name == "Down")
                value = -20;

            var volume = _runtimeConfigDataModel.bgmVolume;
            volume = Math.Max(0, Math.Min(volume + value, 100));

            _bgmVolumeValue.text = volume + "%";
            _runtimeConfigDataModel.bgmVolume = volume;
#if !UNITY_SWITCH
            _Save();
#else
            _changed = true;
#endif

            SoundManager.Self().SetRuntimeConfigDataModel(_runtimeConfigDataModel);
            SoundManager.Self().ChangeBgmState(_runtimeConfigDataModel.bgmVolume);

            SetButtonEnabled(_bgmVolumeValue.transform.parent.Find("Down").GetComponent<WindowButtonBase>(), false, volume == 0);
            SetButtonEnabled(_bgmVolumeValue.transform.parent.Find("Up").GetComponent<WindowButtonBase>(), true, volume == 100);
        }

        /// <summary>
        ///     BGSの音量調整ボタン押下時のコールバック
        /// </summary>
        /// <param name="_target">押下したボタン</param>
        public void OnChangeBgsVolumeClicked(Button _target) {
            var value = 0;
            if (_target.name == "Up")
                value = 20;
            else if (_target.name == "Down")
                value = -20;

            var volume = _runtimeConfigDataModel.bgsVolume;
            volume = Math.Max(0, Math.Min(volume + value, 100));

            _bgsVolumeValue.text = volume + "%";
            _runtimeConfigDataModel.bgsVolume = volume;
#if !UNITY_SWITCH
            _Save();
#else
            _changed = true;
#endif

            SoundManager.Self().SetRuntimeConfigDataModel(_runtimeConfigDataModel);
            SoundManager.Self().ChangeBgsState(_runtimeConfigDataModel.bgsVolume);

            SetButtonEnabled(_bgsVolumeValue.transform.parent.Find("Down").GetComponent<WindowButtonBase>(), false, volume == 0);
            SetButtonEnabled(_bgsVolumeValue.transform.parent.Find("Up").GetComponent<WindowButtonBase>(), true, volume == 100);
        }

        /// <summary>
        ///     MEの音量調整ボタン押下時のコールバック
        /// </summary>
        /// <param name="_target">押下したボタン</param>
        public void OnChangeMeVolumeClicked(Button _target) {
            var value = 0;
            if (_target.name == "Up")
                value = 20;
            else if (_target.name == "Down")
                value = -20;

            var volume = _runtimeConfigDataModel.meVolume;
            volume = Math.Max(0, Math.Min(volume + value, 100));

            _meVolumeValue.text = volume + "%";
            _runtimeConfigDataModel.meVolume = volume;
#if !UNITY_SWITCH
            _Save();
#else
            _changed = true;
#endif

            SoundManager.Self().SetRuntimeConfigDataModel(_runtimeConfigDataModel);
            SoundManager.Self().ChangeSeState(_runtimeConfigDataModel.meVolume);

            SetButtonEnabled(_meVolumeValue.transform.parent.Find("Down").GetComponent<WindowButtonBase>(), false, volume == 0);
            SetButtonEnabled(_meVolumeValue.transform.parent.Find("Up").GetComponent<WindowButtonBase>(), true, volume == 100);
        }

        /// <summary>
        ///     SEの音量調整ボタン押下時のコールバック
        /// </summary>
        /// <param name="_target">押下したボタン</param>
        public void OnChangeSeVolumeClicked(Button _target) {
            var value = 0;
            if (_target.name == "Up")
                value = 20;
            else if (_target.name == "Down")
                value = -20;

            var volume = _runtimeConfigDataModel.seVolume;
            volume = Math.Max(0, Math.Min(volume + value, 100));

            _seVolumeValue.text = volume + "%";
            _runtimeConfigDataModel.seVolume = volume;
#if !UNITY_SWITCH
            _Save();
#else
            _changed = true;
#endif

            SoundManager.Self().SetRuntimeConfigDataModel(_runtimeConfigDataModel);
            SoundManager.Self().ChangeSeState(_runtimeConfigDataModel.seVolume);

            SetButtonEnabled(_seVolumeValue.transform.parent.Find("Down").GetComponent<WindowButtonBase>(), false, volume == 0);
            SetButtonEnabled(_seVolumeValue.transform.parent.Find("Up").GetComponent<WindowButtonBase>(), true, volume == 100);
        }

        /// <summary>
        /// 保存処理
        /// </summary>
        private void _Save() {
            _runtimeDataManagementService.SaveConfig();
        }

        /// <summary>
        /// 戻る処理
        /// </summary>
        override public void Back() {
            if (GameStateHandler.CurrentGameState() != GameStateHandler.GameState.TITLE)
            {
                // 選択を解除
                EventSystem.current.SetSelectedGameObject(null);
                _selectedItem = null;
                _menuBase.BackMenu();
                return;
            }
#if UNITY_SWITCH
            if (_changed)
            {
                _Save();
            }
#endif

            // 選択を解除
            EventSystem.current.SetSelectedGameObject(null);
            _selectedItem = null;

            _titleController.CloseOption();
            _optionRoot.SetActive(false);

            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, systemSettingDataModel.soundSetting.cancel);
            SoundManager.Self().PlaySe();
        }

        private void SetButtonEnabled(WindowButtonBase btn, bool isUp, bool isGray) {
            if (isUp)
            {
                if (isGray)
                {
                    var path = "Assets/RPGMaker/Storage/Images/Ui/uigw_opbutt_up_g.png";
                    var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(path);
                    btn.GetComponent<Image>().sprite = tex;
                    btn.SetGray(true);
                }
                else
                {
                    var path = "Assets/RPGMaker/Storage/Images/Ui/uigw_opbutt_up_dark.png";
                    var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(path);
                    btn.GetComponent<Image>().sprite = tex;
                    btn.SetGray(false);
                }
            }
            else
            {
                if (isGray)
                {
                    var path = "Assets/RPGMaker/Storage/Images/Ui/uigw_opbutt_down_g.png";
                    var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(path);
                    btn.GetComponent<Image>().sprite = tex;
                    btn.SetGray(true);
                }
                else
                {
                    var path = "Assets/RPGMaker/Storage/Images/Ui/uigw_opbutt_down_dark.png";
                    var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(path);
                    btn.GetComponent<Image>().sprite = tex;
                    btn.SetGray(false);
                }
            }
        }

#if UNITY_EDITOR
        Button _saveButton;

        public void SaveFocused() {
            if (_alwaysDashButton.GetComponent<WindowButtonBase>().IsHighlight()) _saveButton = _alwaysDashButton;
            else if (_commandRememberValue.transform.parent.GetComponent<WindowButtonBase>().IsHighlight()) _saveButton = _commandRememberValue.transform.parent.GetComponent<Button>();
            else if (_bgmVolumeValue.transform.parent.Find("Down").GetComponent<WindowButtonBase>().IsHighlight()) _saveButton = _bgmVolumeValue.transform.parent.Find("Down").GetComponent<Button>();
            else if (_bgmVolumeValue.transform.parent.Find("Up").GetComponent<WindowButtonBase>().IsHighlight()) _saveButton = _bgmVolumeValue.transform.parent.Find("Up").GetComponent<Button>();
            else if (_bgsVolumeValue.transform.parent.Find("Down").GetComponent<WindowButtonBase>().IsHighlight()) _saveButton = _bgsVolumeValue.transform.parent.Find("Down").GetComponent<Button>();
            else if (_bgsVolumeValue.transform.parent.Find("Up").GetComponent<WindowButtonBase>().IsHighlight()) _saveButton = _bgsVolumeValue.transform.parent.Find("Up").GetComponent<Button>();
            else if (_meVolumeValue.transform.parent.Find("Down").GetComponent<WindowButtonBase>().IsHighlight()) _saveButton = _meVolumeValue.transform.parent.Find("Down").GetComponent<Button>();
            else if (_meVolumeValue.transform.parent.Find("Up").GetComponent<WindowButtonBase>().IsHighlight()) _saveButton = _meVolumeValue.transform.parent.Find("Up").GetComponent<Button>();
            else if (_seVolumeValue.transform.parent.Find("Down").GetComponent<WindowButtonBase>().IsHighlight()) _saveButton = _seVolumeValue.transform.parent.Find("Down").GetComponent<Button>();
            else if (_seVolumeValue.transform.parent.Find("Up").GetComponent<WindowButtonBase>().IsHighlight()) _saveButton = _seVolumeValue.transform.parent.Find("Up").GetComponent<Button>();
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