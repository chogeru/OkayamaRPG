using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map.Menu;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Title
{
    /// <summary>
    /// タイトル画面処理
    /// </summary>
    public class TitleController : WindowBase
    {
        [SerializeField] private Animator   _backAnim = null;
        [SerializeField] private GameObject _menus = null;
        [SerializeField] private OptionController _optionController = null;
        [SerializeField] private SaveMenu _saveMenu  = null;
        [SerializeField] private Animator _titleAnim = null;
        [SerializeField] private GameObject _titleMenu = null;

        private SystemSettingDataModel system;

        //実行時のみ使用
        private static bool IsExcInit = true;
        
   
#if UNITY_EDITOR
        private static TitleController _controller = null;
        private static string _tabName = "";
        private static bool _isFocused = false;


        /// <summary>
        /// 現在開いているタブがGameビューだったら、フォーカス処理を実行する
        /// </summary>
        static void OnEditorUpdate() {
            if (UnityEditor.EditorWindow.focusedWindow != null &&
                UnityEditor.EditorWindow.focusedWindow.titleContent.text.Equals(_tabName))
            {
                // Gameビューにフォーカスが移動したときの処理
                _controller?.ChangeFocused();
            }
            else
            {
                _isFocused = false;
                _controller?.SaveFocused();
            }
        }

        void OnDestroy() {
            _controller = null;
            UnityEditor.EditorApplication.update -= OnEditorUpdate;
        }
#endif
        
        

        [RuntimeInitializeOnLoadMethod]
        public static void InitializeOnLoad() {
            IsExcInit = true;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update += OnEditorUpdate;
            var assembly = typeof(UnityEditor.EditorWindow).Assembly;
            var type = assembly.GetType("UnityEditor.GameView");
            var gameView = UnityEditor.EditorWindow.GetWindow(type);
            _tabName = gameView.titleContent.text;
#endif
        }

        protected void Start() {
#if UNITY_EDITOR
            if (Application.isPlaying)
                _titleMenu.GetComponent<Animator>().enabled = true;

            _controller = this;
#else
            _titleMenu.GetComponent<Animator>().enabled = true;
#endif

            SoundManager.Self().Init();
            //状態の更新
            GameStateHandler.SetGameState(GameStateHandler.GameState.TITLE);
            //TimeHandlerに登録されているものを全削除
            TimeHandler.Instance.ClearActions();

            //プレイ時間初期化
            TimeHandler.Instance.SetPlayTime(null);

            //今のオブジェクト全削除
            HudDistributor.Instance.AllDestroyHudHandler();

            _titleMenu.SetActive(false);
            _optionController.gameObject.SetActive(false);
            _saveMenu.gameObject.SetActive(false);

            //ゲーム中からタイトルへ戻ったかで取得する方法を変更
            var config = SoundManager.Self().GetRuntimeConfigDataModel() ?? DataManager.Self().GetRuntimeConfigDataModel();
            if (config == null)
            {
                config = DataManager.Self().CreateConfig();
                SoundManager.Self().SetRuntimeConfigDataModel(config);
            }

            system = DataManager.Self().GetSystemDataModel();
            PlayBgm(config);

            if (!IsExcInit)
            {
                HudDistributor.Instance.StaticHudHandler().DisplayInitByScene();
                HudDistributor.Instance.StaticHudHandler().FadeInFixedBlack(InitAfterFade, false, 0.5f, true);
            }
            else
            {
                HudDistributor.Instance.StaticHudHandler().DisplayInitByScene();
                HudDistributor.Instance.StaticHudHandler().FadeOut(() =>
                {
                    StartCoroutine(ExecInit());
                }, Color.black, 0, true);
            }

            Init();
        }

        private async void PlayBgm(RuntimeConfigDataModel config) {
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_BGM, system.bgm.title);
            await SoundManager.Self().PlayBgm();
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, system.soundSetting.ok);
            SoundManager.Self().ChangeBgmState(config.bgmVolume);
            SoundManager.Self().ChangeSeState(config.seVolume);
        }

        /// <summary>
        /// 起動時のみ実行する
        /// </summary>
        /// <returns></returns>
        private IEnumerator ExecInit() {
#if UNITY_SWITCH
            yield return new WaitForSeconds(2.0f);
#elif UNITY_PS4
            yield return new WaitForSeconds(4.0f);
#else
            yield return new WaitForSeconds(0.5f);
#endif
            IsExcInit = false;
            HudDistributor.Instance.StaticHudHandler().FadeInFixedBlack(InitAfterFade,   false, 0.5f, true);
        }

        public override void Init() {
            base.Init();

            // ロード可能なセーブデータが無い場合はグレーアウトする
            bool flg = _saveMenu.GetSaveQuantity() != 0;
            if (!flg)
            {
                _menus.transform.Find("Continue").GetComponent<WindowButtonBase>().SetGray(true);
            }

            if (_saveMenu.GetSaveQuantity() == 1 && _saveMenu.IsAutoSaveFile() && system.optionSetting.enabledAutoSave == 0)
            {
                _menus.transform.Find("Continue").GetComponent<WindowButtonBase>().SetGray(true);
            }
        }


        // Start is called before the first frame update
        public void InitAfterFade() {
            //入力ハンドリング初期化
            InputDistributor.RemoveInputHandlerWithGameState(GameStateHandler.GameState.TITLE);
            InputDistributor.RemoveInputHandlerWithGameState(GameStateHandler.GameState.MAP);
            InputDistributor.RemoveInputHandlerWithGameState(GameStateHandler.GameState.MENU);
            InputDistributor.RemoveInputHandlerWithGameState(GameStateHandler.GameState.BATTLE);
            InputDistributor.RemoveInputHandlerWithGameState(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT);
            InputDistributor.RemoveInputHandlerWithGameState(GameStateHandler.GameState.BATTLE_EVENT);

            _gameState = GameStateHandler.GameState.TITLE;

            var scales = transform.GetComponentsInChildren<CanvasScaler>();
            var displaySize = DataManager.Self().GetSystemDataModel()
                .DisplaySize[DataManager.Self().GetSystemDataModel().displaySize];
            foreach (var scale in scales)
            {
                scale.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                //scale.referenceResolution = displaySize;
            }

            MenuInitSetting();

            StartCoroutine(OpenTitleMenu());
        }

        public override void Update() {
            base.Update();
            InputHandler.Watch();
        }

        //オプションを閉じたときの処理0
        public void CloseOption() {
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, systemSettingDataModel.soundSetting.cancel);
            SoundManager.Self().PlaySe();
            StartCoroutine(StartSet());
        }

        private IEnumerator StartSet() {
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(OpenTitleMenu());
        }

        public void CloseContinue() {
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, systemSettingDataModel.soundSetting.cancel);
            SoundManager.Self().PlaySe();
            _saveMenu.gameObject.SetActive(false);
            StartCoroutine(StartSet());
        }


        /// <summary>
        ///     ニューゲーム
        /// </summary>
        public void NewGameEvent(GameObject obj) {
            NewGame();
        }

        private void NewGame() {
            StartCoroutine(CloseTitleMenu());

            //セーブデータ作成する
            DataManager.Self().CreateGame();
            DataManager.Self().CreateLoadGame();

            //装備関連の初期化処理
            //GameActorを利用するため、CoreSystemではなく、ここで実施
            var actorsWork = DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels;
            foreach (var actor in actorsWork)
            {
                //二刀流の場合に、装備枠を変更する
                ItemManager.SetActorEquipTypes(actor);

                //装備封印のチェックを行う
                //装備封印チェックの結果がtrueの場合には、メソッド内で装備も外される
                for (var i = 0; i < systemSettingDataModel.equipTypes.Count; i++)
                {
                    SystemSettingDataModel.EquipType equipTypeData = null;
                    for (int i2 = 0; i2 < systemSettingDataModel.equipTypes.Count; i2++)
                        if (systemSettingDataModel.equipTypes[i2].id == actor.equips[i].equipType)
                        {
                            equipTypeData = systemSettingDataModel.equipTypes[i2];
                            break;
                        }
                    ItemManager.CheckTraitEquipSea(actor, equipTypeData, i, true);
                }
            }
            
            DataManager.Self().ReloadGameParty();

            HudDistributor.Instance.StaticHudHandler().FadeOut(()=>{
                SceneManager.LoadScene("SceneMap");
                gameObject.SetActive(false);
            }, UnityEngine.Color.black, 0.5f, true);
        }

        /// <summary>
        ///     コンテニュー
        /// </summary>
        public void ContinueEvent(GameObject obj) {
            Continue();
        }

        /// <summary>
        ///     セーブ画面を開く。ロード可能なセーブデータが無い場合はブザーを鳴らす。
        /// </summary>
        /// <returns></returns>
        private async void Continue() {
            StartCoroutine(CloseTitleMenu());
            await Task.Delay(500);
            _saveMenu.gameObject.SetActive(true);
            _saveMenu.Init(this);
        }

        /// <summary>
        ///     オプション
        /// </summary>
        public void OptionEvent(GameObject obj) {
            Option();
        }

        private async void Option() {
            StartCoroutine(CloseTitleMenu());
            await Task.Delay(500);
            _optionController.gameObject.SetActive(true);
            _optionController.Init(this);
        }

        private IEnumerator OpenTitleMenu() {
            //タイトルメニューに画像の挿入
            _titleMenu.SetActive(true);
            yield return new WaitForAnimation(_titleAnim, 0);
            _menus.SetActive(true);

            //十字キーでの操作登録
            var selects = _titleMenu.GetComponentsInChildren<Button>();
            for (var i = 0; i < selects.Length; i++)
            {
                var nav = selects[i].navigation;
                nav.mode = Navigation.Mode.Explicit;
                nav.selectOnUp = selects[i == 0 ? selects.Length - 1 : i - 1];
                nav.selectOnDown = selects[(i + 1) % selects.Length];

                selects[i].navigation = nav;
            }

            selects[0].Select();
        }
#if UNITY_EDITOR
        Button _saveButton;

        public void SaveFocused() {
            if (_optionController.gameObject.activeSelf)
            {
                _optionController.SaveFocused();
            }
            else if (_saveMenu.gameObject.activeSelf)
            {
                _saveMenu.SaveFocused();
            }
            else
            {
                SaveFocusedTitle();
            }
        }

        private void SaveFocusedTitle() {
            var selects = _titleMenu.GetComponentsInChildren<Button>();
            for (var i = 0; i < selects.Length; i++)
            {
                var button = selects[i].GetComponent<WindowButtonBase>();
                if (button.IsHighlight())
                {
                    _saveButton = selects[i].GetComponent<Button>();
                    break;
                }
            }
        }

        private void ChangeFocused() {
            if (_isFocused) return;
            _isFocused = true;

            if (_optionController.gameObject.activeSelf)
            {
                _optionController.ChangeFocused();
            }
            else if (_saveMenu.gameObject.activeSelf)
            {
                _saveMenu.ChangeFocused();
            }
            else
            {
                ChangeFocusedTitle();
            }
        }

        private async void ChangeFocusedTitle() {
            //少し待たないとフォーカスが移らないため、待つ
            await Task.Delay(10);

            //フォーカス再設定処理
            _saveButton?.Select();
        }
#endif
        // メニューの配置設定
        private void MenuInitSetting() {
            void NewObj() {
                var obj = new GameObject();
                obj.transform.SetParent(_menus.transform);
                obj.transform.gameObject.AddComponent<RectTransform>();
                obj.transform.gameObject.AddComponent<Image>().sprite = 
                    _menus.transform.Find("NewGame").transform.gameObject.GetComponent<Image>().sprite;
            }

            if (!_menus.transform.Find("NewGame").transform.gameObject.activeSelf)
                NewObj();
            if (!_menus.transform.Find("Continue").transform.gameObject.activeSelf)
                NewObj();
            if (!_menus.transform.Find("Option").transform.gameObject.activeSelf)
                NewObj();

            var vlg = _menus.GetComponent<VerticalLayoutGroup>();
            vlg.childForceExpandHeight = true;
            vlg.childControlHeight = true;
            vlg.childScaleHeight = false;
        }

        public IEnumerator CloseTitleMenu() {
            _menus.SetActive(false);
            _titleAnim.SetBool("close", true);
            _backAnim.SetBool("close", true);

            yield return new WaitForSeconds(0.5f);

            _titleMenu.SetActive(false);
        }
    }
}