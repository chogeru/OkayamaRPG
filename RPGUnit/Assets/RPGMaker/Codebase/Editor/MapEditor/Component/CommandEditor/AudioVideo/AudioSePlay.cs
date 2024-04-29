using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.Enum;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event.EventDataModel;
using static RPGMaker.Codebase.Editor.Common.View.SoundDataList;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.AudioVideo
{
    /// <summary>
    ///     「SEの演奏」のコマンド設定枠の表示物
    /// </summary>
    public class AudioSePlay : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_audio_se_play.uxml";

        private AudioSource _audioSource;

        private GameObject   _gameObject;
        private EventCommand _targetCommand;

        public AudioSePlay(
            VisualElement rootElement,
            List<EventDataModel> eventDataModels,
            int eventIndex,
            int eventCommandIndex
        )
            : base(rootElement, eventDataModels, eventIndex, eventCommandIndex) {
        }

        public override void Invoke() {
            var commandTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SettingUxml);
            VisualElement commandFromUxml = commandTree.CloneTree();
            EditorLocalize.LocalizeElements(commandFromUxml);
            commandFromUxml.style.flexGrow = 1;
            RootElement.Add(commandFromUxml);

            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            AudioClip audioData;
            if (GameObject.FindWithTag("sound") == null)
            {
                _gameObject = new GameObject();
                _gameObject.name = "sound";
                _gameObject.tag = "sound";
                _audioSource = _gameObject.AddComponent<AudioSource>();
            }
            else
            {
                _audioSource = GameObject.FindWithTag("sound").transform.gameObject.GetComponent<AudioSource>();
            }

            _targetCommand = EventDataModels[EventIndex].eventCommands[EventCommandIndex];
            if (_targetCommand.parameters.Count == 0)
            {
                _targetCommand.parameters.Add("");
                _targetCommand.parameters.Add("90");
                _targetCommand.parameters.Add("100");
                _targetCommand.parameters.Add("0");
                EventManagementService.SaveEvent(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            //サウンドリスト
            SetBgmDropDownData(RootElement);

            // インポート
            Button backgroundImport = RootElement.Query<Button>("sound_import");
            backgroundImport.clickable.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("ogg", PathManager.SOUND_SE);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    _targetCommand.parameters[0] = path;
                    Save(EventDataModels[EventIndex]);
                    SetBgmDropDownData(RootElement);
                }
            };

            
            //▼ボリューム
            var volumeSliderArea = RootElement.Query<VisualElement>("volume_sliderArea");
            SliderAndFiledBase.IntegerSliderCallBack(volumeSliderArea, 0, 100, "%",
                int.Parse(_targetCommand.parameters[1]), evt =>
                {
                    _audioSource.volume = evt / 100f;
                    _targetCommand.parameters[1] = ((int) evt).ToString();
                    Save(EventDataModels[EventIndex]);
                });

            //▼ピッチ
            var pitchSliderArea = RootElement.Query<VisualElement>("pitch_sliderArea");
            SliderAndFiledBase.IntegerSliderCallBack(pitchSliderArea, 50, 150, "%",
                int.Parse(_targetCommand.parameters[2]), evt =>
                {
                    _audioSource.pitch = evt / 100f;
                    _targetCommand.parameters[2] = ((int) evt).ToString();
                    Save(EventDataModels[EventIndex]);

                });
            
            //▼位相
            var panSliderArea = RootElement.Query<VisualElement>("pan_sliderArea");
            SliderAndFiledBase.IntegerSliderCallBack(panSliderArea, -100, 100, "",
                int.Parse(_targetCommand.parameters[3]), evt =>
                {
                    _audioSource.panStereo = evt / 100f;
                    _targetCommand.parameters[3] = ((int) evt).ToString();
                    Save(EventDataModels[EventIndex]);

                });

            Button buttonPlay = RootElement.Query<Button>("play");
            Button buttonStop = RootElement.Query<Button>("stop");

            buttonPlay.text = "▶" + buttonPlay.text;
            buttonStop.text = "■" + buttonStop.text;

            buttonPlay.clicked += () =>
            {
                List<SoundType> soundTypes = new List<SoundType>();
                soundTypes.Add(SoundType.Se);

                string filename = SoundHelper.InitializeFileName(soundTypes, _targetCommand.parameters[0], true);
                audioData = AssetDatabase.LoadAssetAtPath<AudioClip>(filename);
                _audioSource.clip = audioData;

                //各パラメータの反映
                _audioSource.volume = int.Parse(_targetCommand.parameters[1]) / 100f;
                _audioSource.pitch = int.Parse(_targetCommand.parameters[2]) / 100f;
                _audioSource.panStereo = int.Parse(_targetCommand.parameters[3]) / 100f;

                _audioSource.Play();
            };
            buttonStop.clicked += () => { _audioSource.Stop(); };
        }

        private void SetBgmDropDownData(VisualElement Items) {
            VisualElement SoundsettingDropdown = Items.Query<VisualElement>("menu");
            SoundsettingDropdown.Clear();

            List<SoundType> soundTypes = new List<SoundType>();
            soundTypes.Add(SoundType.Se);
            var SoundsettingDropdownPopupField = GenericPopupFieldBase<SoundDataChoice>.Add(
                Items,
                "menu",
                SoundDataList.GenerateChoices(soundTypes),
                SoundHelper.InitializeFileName(soundTypes, _targetCommand.parameters[0], false));

            SoundsettingDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                //0番目は曲無し
                if (SoundsettingDropdownPopupField.index == 0)
                    _targetCommand.parameters[0] = "";
                else
                    _targetCommand.parameters[0] = SoundsettingDropdownPopupField.value.filename + "." + SoundsettingDropdownPopupField.value.extention;

                Save(EventDataModels[EventIndex]);
            });
        }
    }
}