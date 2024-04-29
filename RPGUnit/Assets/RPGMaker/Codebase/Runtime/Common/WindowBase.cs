using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Common
{
    public class WindowBase : MonoBehaviour
    {
        /// <summary>
        /// Window表示時のゲームの状態
        /// キー入力
        /// </summary>
        protected GameStateHandler.GameState _gameState = GameStateHandler.GameState.TITLE;
        public delegate void OnComplete();

        public const   string                 UI_PATH = "Assets/RPGMaker/Storage/Images/Ui/";
        protected      GameObject             _selectedItem;
        public         GameObject[]           commonWindowObject;
        protected      SystemSettingDataModel systemSettingDataModel;
        private RuntimeSystemConfigDataModel _dataModel;

        public virtual void Init() {
            systemSettingDataModel = DataManager.Self().GetSystemDataModel();
            _dataModel = DataManager.Self().GetRuntimeSaveDataModel()?.runtimeSystemConfig;
        }

        public virtual void Update() {
            _dataModel ??= DataManager.Self().GetRuntimeSaveDataModel()?.runtimeSystemConfig;

            if (_dataModel?.windowTone.Count > 0)
            {
                foreach (var window in commonWindowObject)
                {
                    var obj = window.transform.Find("Image");
                    var image = obj?.GetComponent<Image>();
                    if (image != null)
                    {
                        if (image.color != new Color(_dataModel.windowTone[0] / 255f, _dataModel.windowTone[1] / 255f,
                            _dataModel.windowTone[2] / 255f))
                        {
                            image.color = new Color(_dataModel.windowTone[0] / 255f, _dataModel.windowTone[1] / 255f,
                                _dataModel.windowTone[2] / 255f);
                        }
                    }
                }
            }
        }

        virtual public void Back() {
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, systemSettingDataModel.soundSetting.cancel);
            SoundManager.Self().PlaySe();
        }
    }
}