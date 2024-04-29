using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.Editor.Common.View.SoundDataList;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.SystemSetting
{
    /// <summary>
    /// 戦闘BGMの変更
    /// 勝利時MEの変更
    /// 敗北時MEの変更
    /// 乗り物BGMの変更
    /// </summary>
    public class ChangeSound : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_change_sound.uxml";

        private const string bgmPath = "Assets/RPGMaker/Storage/Sounds/BGM/";
        private const string mePath  = "Assets/RPGMaker/Storage/Sounds/ME/";

        private GameObject _gameObject;

        private string _path;

        public ChangeSound(
            VisualElement rootElement,
            List<EventDataModel> eventDataModels,
            int eventIndex,
            int eventCommandIndex
        )
            : base(rootElement, eventDataModels, eventIndex, eventCommandIndex) {
        }

        public void Invoke(int id) {
            var soundType = id;
            _path = (soundType == 0 || soundType == 3) ? bgmPath : mePath;
            var commandTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SettingUxml);
            VisualElement commandFromUxml = commandTree.CloneTree();
            EditorLocalize.LocalizeElements(commandFromUxml);
            commandFromUxml.style.flexGrow = 1;
            RootElement.Add(commandFromUxml);
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();

            //乗り物用データ取得
            var VehicleDropdownChoices = DatabaseManagementService.LoadCharacterVehicles();

            AudioSource audioSource;
            AudioClip audioData;
            var nowPlaying = "";
            if (GameObject.FindWithTag("sound") == null)
            {
                _gameObject = new GameObject();
                _gameObject.name = "sound";
                _gameObject.tag = "sound";
                audioSource = _gameObject.AddComponent<AudioSource>();
            }
            else
            {
                audioSource = GameObject.FindWithTag("sound").transform.gameObject.GetComponent<AudioSource>();
            }

            VisualElement soundVehicle = RootElement.Query<VisualElement>("sound_vehicle");
            Label soundTypeLabel = RootElement.Query<Label>("sound_type");
            List<RPGMaker.Codebase.Editor.Common.Enum.SoundType> soundTypes = new List<Common.Enum.SoundType>();
            Label soundTypeName = RootElement.Query<Label>("sound_type_name");

            // 戦闘BGMの変更 = 0
            // 勝利時MEの変更 = 1
            // 敗北時MEの変更 = 2
            // 乗り物BGMの変更 = 3
            switch (soundType)
            {
                case 0:
                    soundTypeLabel.text = EditorLocalize.LocalizeText("WORD_0931");
                    soundTypeName.text = EditorLocalize.LocalizeText("WORD_1071");
                    soundVehicle.style.display = DisplayStyle.None;
                    soundTypes.Add(RPGMaker.Codebase.Editor.Common.Enum.SoundType.Bgm);
                    break;
                case 1:
                    soundTypeLabel.text = EditorLocalize.LocalizeText("WORD_0945");
                    soundTypeName.text = EditorLocalize.LocalizeText("WORD_1072");
                    soundVehicle.style.display = DisplayStyle.None;
                    soundTypes.Add(RPGMaker.Codebase.Editor.Common.Enum.SoundType.Me);
                    break;
                case 2:
                    soundTypeLabel.text = EditorLocalize.LocalizeText("WORD_0945");
                    soundTypeName.text = EditorLocalize.LocalizeText("WORD_1073");
                    soundVehicle.style.display = DisplayStyle.None;
                    soundTypes.Add(RPGMaker.Codebase.Editor.Common.Enum.SoundType.Me);
                    break;
                case 3:
                    soundTypeLabel.text = EditorLocalize.LocalizeText("WORD_0931");
                    soundTypeName.text = EditorLocalize.LocalizeText("WORD_1074");
                    soundVehicle.style.display = DisplayStyle.Flex;
                    soundTypes.Add(RPGMaker.Codebase.Editor.Common.Enum.SoundType.Bgm);
                    break;
            }
            
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Count != 5)
            {
                //音楽名
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("");
                //音量
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("90");
                //ピッチ
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("100");
                //位相
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                //乗り物名(乗り物専用)
                if (VehicleDropdownChoices != null && VehicleDropdownChoices.Count > 0)
                {
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters
                        .Add(VehicleDropdownChoices[0].id);
                }
                else
                {
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters
                        .Add("");
                }

                EventManagementService.SaveEvent(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            if (soundType == 3)
            {
                if (VehicleDropdownChoices != null && VehicleDropdownChoices.Count == 0)
                {
                    VisualElement soundArea = RootElement.Query<VisualElement>("sound_area");
                    soundArea.style.display = DisplayStyle.None;
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] = "";
                    return;
                }
            }

            //乗り物ドロップダウン
            VisualElement VehicleDropdown = RootElement.Query<VisualElement>("vehicle_dropdown");
            var VheicleListId = 0;
            for (var i = 0; i < VehicleDropdownChoices.Count; i++)
                if (VehicleDropdownChoices[i].id ==
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4])
                {
                    VheicleListId = i;
                    break;
                }

            //乗り物がひとつもない状態から追加した際に、初期値を代入する
            if (VehicleDropdownChoices.Count > 0 &&
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] == "")
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] =
                    VehicleDropdownChoices[0].id;
                Save(EventDataModels[EventIndex]);
            }

            //選択肢に名前を表示売る際に一時的に使用するList
            var VehicleName = new List<string>();
            for (var i = 0; i < VehicleDropdownChoices.Count; i++) VehicleName.Add(VehicleDropdownChoices[i].name);

            var VehicleDropdownPopupField = new PopupFieldBase<string>(VehicleName, VheicleListId);
            VehicleDropdown.Clear();
            VehicleDropdown.Add(VehicleDropdownPopupField);
            VehicleDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] =
                    VehicleDropdownChoices[VehicleDropdownPopupField.index].id;
                Save(EventDataModels[EventIndex]);
            });

            //▼サウンドファイル選択
            SetBgmDropDownData(RootElement, soundTypes);

            // インポート
            Button backgroundImport = RootElement.Query<Button>("sound_import");
            backgroundImport.clickable.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("ogg", _path);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = path;
                    Save(EventDataModels[EventIndex]);
                    SetBgmDropDownData(RootElement, soundTypes);
                }
            };

            //▼音量
            //音量共通化
            int volumeValue;
            Slider volumeSlider = RootElement.Query<Slider>("volume_slider");
            IntegerField volumeInt = RootElement.Query<IntegerField>("volume_text");
            volumeInt.maxLength = 3;
            volumeSlider.value = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1]);
            volumeSlider.RegisterValueChangedCallback(evt =>
            {
                volumeValue = (int) volumeSlider.value;
                if (volumeValue >= 100) volumeValue = 100;

                audioSource.volume = volumeValue / 100f;
                if (volumeValue >= 100) volumeValue = 100;

                volumeInt.value = volumeValue;
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                    volumeInt.value.ToString();
                Save(EventDataModels[EventIndex]);
            });

            volumeInt.value = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1]);
            BaseInputFieldHandler.IntegerFieldCallback(volumeInt, evt =>
            {
                volumeValue = volumeInt.value;
                volumeSlider.value = volumeValue;
                audioSource.volume = volumeValue / 100f;
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = volumeValue.ToString();
                Save(EventDataModels[EventIndex]);
            }, 0, 100);

            //▼ピッチ
            Slider pitchSlider = RootElement.Query<Slider>("pitch_slider");
            IntegerField pitchInt = RootElement.Query<IntegerField>("pitch_text");
            pitchInt.maxLength = 3;
            pitchSlider.value = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2]);
            pitchSlider.RegisterValueChangedCallback(evt =>
            {
                audioSource.pitch = pitchSlider.value / 100f;
                pitchInt.value = (int) pitchSlider.value;
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] =
                    pitchInt.value.ToString();
                Save(EventDataModels[EventIndex]);
            });
            pitchInt.value = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2]);

            BaseInputFieldHandler.IntegerFieldCallback(pitchInt, evt =>
            {
                pitchSlider.value = pitchInt.value;
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] = pitchInt.value.ToString();
                Save(EventDataModels[EventIndex]);
            }, 50, 150);

            //▼位相
            Slider panSlider = RootElement.Q<VisualElement>("systemSetting_change_sound").Query<Slider>("pan_slider");
            IntegerField panInt = RootElement.Query<IntegerField>("pan_text");
            panSlider.value =
                int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3]);
            panSlider.RegisterValueChangedCallback(evt =>
            {
                audioSource.panStereo = panSlider.value / 100f;
                panInt.value = (int) panSlider.value;
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] =
                    panInt.value.ToString();
                Save(EventDataModels[EventIndex]);
            });
            panInt.value = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3]);

            BaseInputFieldHandler.IntegerFieldCallback(panInt, evt =>
            {
                panSlider.value = panInt.value;
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] = panSlider.value.ToString();
                Save(EventDataModels[EventIndex]);
            }, -100, 100);

            var buttonPlay = RootElement.Q<VisualElement>("systemSetting_change_sound").Query<Button>("musicStart").AtIndex(0);
            var buttonStop = RootElement.Q<VisualElement>("systemSetting_change_sound").Query<Button>("musicStop").AtIndex(0);

            buttonPlay.text = "▶" + buttonPlay.text;
            buttonStop.text = "■" + buttonStop.text;

            buttonPlay.clicked += () =>
            {
                if (!audioSource.isPlaying ||
                    nowPlaying != EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0])
                {
                    string filename = SoundHelper.InitializeFileName(soundTypes, EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0], true);
                    audioData = AssetDatabase.LoadAssetAtPath<AudioClip>(filename);

                    //なしを選択した時
                    if (audioData == null)
                    {
                        //BGMを再生していたら止める
                        if (audioSource.isPlaying) audioSource.Stop();
                        return;
                    }

                    audioSource.clip = audioData;
                    nowPlaying = audioData.name;

                    //各パラメータの反映
                    audioSource.volume = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1]) / 100f;
                    audioSource.pitch = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2]) / 100f;
                    audioSource.panStereo = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3]) / 100f;

                    audioSource.Play();
                }
            };

            buttonStop.clicked += () => { audioSource.Stop(); };
        }

        /// <summary>
        /// サウンドのセレクトボックス表示
        /// </summary>
        /// <param name="Items"></param>
        private void SetBgmDropDownData(VisualElement Items, List<RPGMaker.Codebase.Editor.Common.Enum.SoundType> soundTypes) {
            VisualElement SoundsettingDropdown = Items.Query<VisualElement>("Soundsetting_dropdown");
            SoundsettingDropdown.Clear();

            var SoundsettingDropdownPopupField = GenericPopupFieldBase<SoundDataChoice>.Add(
                Items,
                "Soundsetting_dropdown",
                SoundDataList.GenerateChoices(soundTypes),
                SoundHelper.InitializeFileName(soundTypes, EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0], false));

            SoundsettingDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                if (SoundsettingDropdownPopupField.index == 0)
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "";
                else
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = SoundsettingDropdownPopupField.value.filename + "." + SoundsettingDropdownPopupField.value.extention;
                Save(EventDataModels[EventIndex]);
            });
        }
    }
}