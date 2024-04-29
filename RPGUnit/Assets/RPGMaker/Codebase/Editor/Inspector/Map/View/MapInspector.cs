// #define DEBUG_UTIL_TEST_LOG

using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.Enum;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.MapEditor.Window.MapEdit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.Editor.Common.View.SoundDataList;

namespace RPGMaker.Codebase.Editor.Inspector.Map.View
{
    /// <summary>
    /// [マップ設定]-[マップリスト]-[各マップ]-[マップ編集] Inspector
    /// </summary>
    public class MapInspector : AbstractInspectorElement
    {
        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/Map/Asset/MapInspector.uxml"; } }

        private readonly MapDataModel _mapDataModel;
        private readonly Action<MapDataModel> _onChange;
        private AudioClip _audioData;

        private AudioSource _audioSource;

        //音周り
        //BGM
        private Slider _bgmVolumeSlider;
        private Slider _bgmPitchSlider;
        private Slider _bgmPanSlider;
        private IntegerField _bgmVolumeInt;
        private IntegerField _bgmPitchInt;
        private IntegerField _bgmPanInt;
        private int _bgmVolumeValue;

        private Button _bgmButtonPlay;
        private Button _bgmButtonStop;
        private Button _bgmImport;

        private Toggle _mapBgmAutoPlayToggle;
        private GenericPopupFieldBase<SoundDataChoice> _mapBgmSelect;

        //BGS
        private Slider _bgsVolumeSlider;
        private Slider _bgsPitchSlider;
        private Slider _bgsPanSlider;
        private IntegerField _bgsVolumeInt;
        private IntegerField _bgsPitchInt;
        private IntegerField _bgsPanInt;
        private int _bgsVolumeValue;

        private Button _bgsButtonPlay;
        private Button _bgsButtonStop;
        private Button _bgsImport;

        private Toggle _mapBgsAutoPlayToggle;
        private GenericPopupFieldBase<SoundDataChoice> _mapBgsSelect;

        //MAPに関するデータ
        private Dictionary<string, string> _distantViewImageDictionary;
        private GameObject _gameObject;
        private ImTextField _mapDisplayNameText;
        private Toggle _mapForbidDashToggle;
        private Label _mapIdLabel;
        private ImTextField _mapMemo;
        private ImTextField _mapNameText;
        private PopupFieldBase<string> _mapScrollTypeSelect;

        private SliderInt _mapSizeWidthSlider;
        private IntegerField _mapSizeWidthText;
        private IntSliderField _mapWidthSliderField;

        private SliderInt _mapSizeHeightSlider;
        private IntegerField _mapSizeHeightText;
        private IntSliderField _mapHeightSliderField;

        private int _scrollIndex;
        private bool _isSampleMap;

        private Dictionary<string, string> _scrollTypeDictionary;

        public MapInspector(MapDataModel mapDataModel, bool isSampleMap, Action<MapDataModel> onChange) {
            _mapDataModel = mapDataModel;
            _onChange = onChange;
            _isSampleMap = isSampleMap;

            LoadDictionaries();
            Initialize();
        }

        protected override void RefreshContents() {
            base.RefreshContents();
            LoadDictionaries();
            Initialize();
        }

        private void LoadDictionaries() {
            _scrollTypeDictionary = EditorLocalize.LocalizeDictionaryValues(new Dictionary<string, string>
            {
                {MapDataModel.MapScrollType.NoLoop.ToString(), "WORD_0767"},
                {MapDataModel.MapScrollType.LoopVertical.ToString(), "WORD_0768"},
                {MapDataModel.MapScrollType.LoopHorizontal.ToString(), "WORD_0769"},
                {MapDataModel.MapScrollType.LoopBoth.ToString(), "WORD_0770"}
            });

            scrollIndexGet();
        }

        //スクロールのIndex取得
        private void scrollIndexGet() {
            var value = 0;
            foreach (var key in _scrollTypeDictionary.Keys)
            {
                if (key == _mapDataModel.scrollType.ToString())
                {
                    _scrollIndex = value;
                    break;
                }

                value++;
            }
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();

            _mapIdLabel = RootContainer.Query<Label>("map_id_label");
            _mapNameText = RootContainer.Query<ImTextField>("map_name_text");
            _mapDisplayNameText = RootContainer.Query<ImTextField>("map_display_name_text");

            // マップサムネイルプレビュー画像 (アウトラインエディターでも使用される)。
            var mapThumbnailPreviewImage = RootContainer.Q<Image>("map_thumbnail_preview");
            mapThumbnailPreviewImage.scaleMode = ScaleMode.ScaleToFit;
            SetMapThumbnailPreview(mapThumbnailPreviewImage);

            // 『サムネイルの更新』ボタン。
            var mapThumbnailUpdateButton = RootContainer.Q<Button>("map_thumbnail_update");
            mapThumbnailUpdateButton.clicked += () =>
            {
                var mapEditWindow =
                    WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.MapEditWindow) as
                        MapEditWindow;
                mapEditWindow.CaptureAndSaveMapThumbnail();
                SetMapThumbnailPreview(mapThumbnailPreviewImage);
            };

            // 『マップの画像出力』ボタン。
            var mapImageSaveButton = RootContainer.Q<Button>("map_image_save");
            mapImageSaveButton.clicked += () =>
            {
                // セーブファイルダイアログ。
                string title = EditorLocalize.LocalizeText("WORD_3061");
                string directory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);
                string extension = "png";
                string defaultName = $"{_mapDataModel.name}.{extension}";
                var filePath = EditorUtility.SaveFilePanel(title, directory, defaultName, extension);
                if (string.IsNullOrEmpty(filePath))
                {
                    return;
                }

                var mapEditWindow =
                    WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.MapEditWindow) as
                        MapEditWindow;
                var texture2d = mapEditWindow.CaptureFullSizeMapFromCamera();
                ImageUtility.SaveAndDestroyTexture(filePath, texture2d);
            };

            //サウンドの取得
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

            //BGMの設定
            //▼音量
            //音量共通化
            _bgmVolumeSlider = RootContainer.Query<Slider>("bgm_volume_slider");
            _bgmVolumeInt = RootContainer.Query<IntegerField>("bgm_volume_text");

            //▼ピッチ
            _bgmPitchSlider = RootContainer.Query<Slider>("bgm_pitch_slider");
            _bgmPitchInt = RootContainer.Query<IntegerField>("bgm_pitch_text");

            //▼位相
            _bgmPanSlider = RootContainer.Query<Slider>("bgm_pan_slider");
            _bgmPanInt = RootContainer.Query<IntegerField>("bgm_pan_text");

            _bgmButtonPlay = RootContainer.Query<Button>("bgm_musicStart").AtIndex(0);
            _bgmButtonStop = RootContainer.Query<Button>("bgm_musicStop").AtIndex(0);

            //BGSの設定
            //▼音量
            //音量共通化
            _bgsVolumeSlider = RootContainer.Query<Slider>("bgs_volume_slider");
            _bgsVolumeInt = RootContainer.Query<IntegerField>("bgs_volume_text");

            //▼ピッチ
            _bgsPitchSlider = RootContainer.Query<Slider>("bgs_pitch_slider");
            _bgsPitchInt = RootContainer.Query<IntegerField>("bgs_pitch_text");

            //▼位相
            _bgsPanSlider = RootContainer.Query<Slider>("bgs_pan_slider");
            _bgsPanInt = RootContainer.Query<IntegerField>("bgs_pan_text");

            _bgsButtonPlay = RootContainer.Query<Button>("bgs_musicStart").AtIndex(0);
            _bgsButtonStop = RootContainer.Query<Button>("bgs_musicStop").AtIndex(0);

            _bgsButtonStop.clicked += () => { _audioSource.Stop(); };

            // マップ横サイズ。
            _mapSizeWidthSlider = RootContainer.Query<SliderInt>("map_size_width_slider");
            _mapSizeWidthText = RootContainer.Query<IntegerField>("map_size_width_text");

            // マップ縦サイズ。
            _mapSizeHeightSlider = RootContainer.Query<SliderInt>("map_size_height_slider");
            _mapSizeHeightText = RootContainer.Query<IntegerField>("map_size_height_text");

            _mapScrollTypeSelect = MakePopupField(_scrollTypeDictionary, _scrollIndex, RootContainer, "map_scroll_type_select_container");

            //BGM
            _bgmImport = RootContainer.Query<Button>("bgm_import");
            _mapBgmAutoPlayToggle = RootContainer.Query<Toggle>("map_bgm_auto_play_toggle");
            _mapBgmSelect = MakeSoundPopupField(SoundType.Bgm, _mapDataModel.bgmID, RootContainer, "map_bgm_select_container");

            //BGS
            _bgsImport = RootContainer.Query<Button>("bgs_import");
            _mapBgsAutoPlayToggle = RootContainer.Query<Toggle>("map_bgs_auto_play_toggle");
            _mapBgsSelect = MakeSoundPopupField(SoundType.Bgs, _mapDataModel.bgsID, RootContainer, "map_bgs_select_container");

            // 『ダッシュ禁止』トグル。
            _mapForbidDashToggle = RootContainer.Query<Toggle>("map_forbid_dash_toggle");

            _mapMemo = RootContainer.Query<ImTextField>("map_memo");

            SetEntityToUI();
            if (_isSampleMap)
                SetElementDisable();
        }

        private void SetMapThumbnailPreview(Image mapThumbnailPreviewImage) {
            var mapThumbnailImageFilePath =
                MapEditWindow.GetThumbnailImageFilePathThatExist(_mapDataModel.id);
            try
            {
                mapThumbnailPreviewImage.image = ImageUtility.LoadImageFileToTexture(mapThumbnailImageFilePath);
            }
            catch (Exception)
            {
            }
        }

        private static PopupFieldBase<string> MakePopupField(
            Dictionary<string, string> dictionary,
            int index,
            VisualElement parentContainer,
            string containerName
        ) {
            var RootContainer = (VisualElement) parentContainer.Query<VisualElement>(containerName);
            var popupField = new PopupFieldBase<string>(dictionary.Values.ToList(), index);
            RootContainer.Clear();
            RootContainer.Add(popupField);
            return popupField;
        }

        private static GenericPopupFieldBase<SoundDataChoice> MakeSoundPopupField(
            SoundType soundType,
            string filename,
            VisualElement parentContainer,
            string containerName
        ) {
            var RootContainer = (VisualElement) parentContainer.Query<VisualElement>(containerName);
            RootContainer.Clear();

            List<SoundType> soundTypes = new List<SoundType>();
            soundTypes.Add(soundType);
            var SoundsettingDropdownPopupField = GenericPopupFieldBase<SoundDataChoice>.Add(
                parentContainer,
                containerName,
                SoundDataList.GenerateChoices(soundTypes),
                SoundHelper.InitializeFileName(soundTypes, filename, false));

            return SoundsettingDropdownPopupField;
        }

        private void SetEntityToUI() {
            _mapIdLabel.text = _mapDataModel.SerialNumberString;
            _mapNameText.value = _mapDataModel.name;
            _mapNameText.RegisterCallback<FocusOutEvent>(evt =>
            {
                _mapDataModel.name = _mapNameText.value;
                MapEditor.MapEditor.SaveMap(_mapDataModel, CoreSystem.Service.MapManagement.Repository.MapRepository.SaveType.NO_PREFAB);
                MapEditor.MapEditor.ReloadMap(_mapDataModel, AbstractHierarchyView.RefreshTypeMapName + "," + _mapDataModel.id);
            });
            _mapDisplayNameText.value = _mapDataModel.displayName;
            _mapDisplayNameText.RegisterCallback<FocusOutEvent>(evt =>
            {
                _mapDataModel.displayName = _mapDisplayNameText.value;
                Save();
            });

            // マップ横サイズ。
            _mapWidthSliderField = new IntSliderField(
                _mapSizeWidthSlider,
                _mapSizeWidthText,
                MapDataModel.MinWidth,
                MapDataModel.MaxWidth,
                _mapDataModel.width,
                (width) => ChangeMapSize(width, _mapDataModel.height));

            // マップ縦サイズ。
            _mapHeightSliderField = new IntSliderField(
                _mapSizeHeightSlider,
                _mapSizeHeightText,
                MapDataModel.MinHeight,
                MapDataModel.MaxHeight,
                _mapDataModel.height,
                (height) => ChangeMapSize(_mapDataModel.width, height));

            _mapScrollTypeSelect.index =
                GetIndexOfDictionary(_scrollTypeDictionary, _mapDataModel.scrollType.ToString());
            _mapScrollTypeSelect.RegisterValueChangedCallback(evt =>
            {
                var selectedKey = _scrollTypeDictionary.FirstOrDefault(kv => kv.Value == _mapScrollTypeSelect.value)
                    .Key;
                _mapDataModel.scrollType =
                    (MapDataModel.MapScrollType) Enum.Parse(typeof(MapDataModel.MapScrollType), selectedKey);
                Save();
            });
            //BGMインポート
            _bgmImport.clickable.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("ogg", PathManager.SOUND_BGM);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileName(path);
                    _mapDataModel.bgmID = path;
                    Save();
                    _mapBgmSelect = MakeSoundPopupField(SoundType.Bgm, path, RootContainer, "map_bgm_select_container");
                }
            };
            new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                _mapBgmAutoPlayToggle,
                new List<VisualElement> {
                    RootContainer.Query<VisualElement>("map_bgm_auto_play_toggle_contents"),
                    RootContainer.Query<VisualElement>("map_bgm_auto_play_toggle_contents_pulldown"),
                },
                _mapDataModel.autoPlayBGM,
                () =>
                {
                    _mapDataModel.autoPlayBGM = _mapBgmAutoPlayToggle.value;
                    Save();
                }
            );
            _mapBgmSelect.RegisterValueChangedCallback(evt =>
            {
                _mapDataModel.bgmID = _mapBgmSelect.value.filename + "." + _mapBgmSelect.value.extention;
                Save();
            });
            //BGSインポート
            _bgsImport.clickable.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("ogg", PathManager.SOUND_BGS);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileName(path);
                    _mapDataModel.bgsID = path;
                    Save();
                    _mapBgsSelect = MakeSoundPopupField(SoundType.Bgs, path, RootContainer, "map_bgs_select_container");
                }
            };
            new CommonToggleSelector().SetToggleInVisualElementSelectorSingle(
                _mapBgsAutoPlayToggle,
                new List<VisualElement> {
                    RootContainer.Query<VisualElement>("map_bgs_auto_play_toggle_contents"),
                    RootContainer.Query<VisualElement>("map_bgs_auto_play_toggle_contents_pulldown"),
                },
                _mapDataModel.autoPlayBgs,
                () =>
                {
                    _mapDataModel.autoPlayBgs = _mapBgsAutoPlayToggle.value;
                    Save();
                }
            );
            _mapBgsSelect.RegisterValueChangedCallback(evt =>
            {
                _mapDataModel.bgsID = _mapBgsSelect.value.filename + "." + _mapBgsSelect.value.extention;
                Save();
            });

            // 『ダッシュ禁止』トグル。
            _mapForbidDashToggle.value = _mapDataModel.forbidDash;
            _mapForbidDashToggle.RegisterValueChangedCallback(evt =>
            {
                _mapDataModel.forbidDash = evt.newValue;
                Save();
            });

            _mapMemo.value = _mapDataModel.memo;
            _mapMemo.RegisterCallback<FocusOutEvent>(evt =>
            {
                _mapDataModel.memo = _mapMemo.value;
                Save();
            });

            //BGMの設定
            //▼音量
            //音量共通化
            _bgmVolumeInt.maxLength = 3;
            _bgmVolumeSlider.value = _mapDataModel.bgmState.volume;
            _bgmVolumeSlider.RegisterValueChangedCallback(evt =>
            {
                _bgmVolumeValue = (int) _bgmVolumeSlider.value;
                if (_bgmVolumeValue >= 100) _bgmVolumeValue = 100;

                _audioSource.volume = _bgmVolumeValue / 100f;
                if (_bgmVolumeValue >= 100) _bgmVolumeValue = 100;

                _bgmVolumeInt.value = _bgmVolumeValue;
                _mapDataModel.bgmState.volume =
                    _bgmVolumeInt.value;
                Save();
            });
            _bgmVolumeInt.value = _mapDataModel.bgmState.volume;
            _bgmVolumeInt.RegisterCallback<FocusOutEvent>(evt =>
            {
                _bgmVolumeValue = _bgmVolumeInt.value;
                if (_bgmVolumeValue >= 100) _bgmVolumeValue = 100;

                _bgmVolumeSlider.value = _bgmVolumeValue;
                _audioSource.volume = _bgmVolumeValue / 100f;
                if (_bgmVolumeValue >= 100) _bgmVolumeValue = 100;

                _mapDataModel.bgmState.volume = _bgmVolumeValue;
                Save();
            });

            //▼ピッチ
            _bgmPitchInt.maxLength = 3;
            _bgmPitchSlider.value = _mapDataModel.bgmState.pitch;
            _bgmPitchSlider.RegisterValueChangedCallback(evt =>
            {
                _audioSource.pitch = _bgmPitchSlider.value / 100f;
                _bgmPitchInt.value = (int) _bgmPitchSlider.value;
                _mapDataModel.bgmState.pitch = _bgmPitchInt.value;
                Save();
            });
            _bgmPitchInt.value = _mapDataModel.bgmState.pitch;
            _bgmPitchInt.RegisterCallback<FocusOutEvent>(evt =>
            {
                _bgmPitchSlider.value = _bgmPitchInt.value;
                _mapDataModel.bgmState.pitch = _bgmPitchInt.value;
                Save();
            });

            //▼位相
            _bgmPanSlider.value = _mapDataModel.bgmState.pan;
            _bgmPanSlider.RegisterValueChangedCallback(evt =>
            {
                _audioSource.panStereo = _bgmPanSlider.value / 100f;
                _bgmPanInt.value = (int) _bgmPanSlider.value;
                _mapDataModel.bgmState.pan = _bgmPanInt.value;
                Save();
            });
            _bgmPanInt.value = _mapDataModel.bgmState.pan;
            _bgmPanInt.RegisterCallback<FocusOutEvent>(evt =>
            {
                _bgmPanSlider.value = _bgmPanInt.value;
                _mapDataModel.bgmState.pan = _bgmPanInt.value;
                Save();
            });

            _bgmButtonPlay.clicked += () =>
            {
                List<SoundType> soundTypes = new List<SoundType>();
                soundTypes.Add(SoundType.Bgm);
                string filename = SoundHelper.InitializeFileName(soundTypes, _mapDataModel.bgmID, true);
                _audioData = AssetDatabase.LoadAssetAtPath<AudioClip>(filename);
                _audioSource.clip = _audioData;
                _audioSource.volume = _mapDataModel.bgmState.volume / 100f;
                _audioSource.pitch = _mapDataModel.bgmState.pitch / 100f;
                _audioSource.panStereo = _mapDataModel.bgmState.pan / 100f;
                _audioSource.Play();
            };

            _bgmButtonStop.clicked += () => { _audioSource.Stop(); };

            //BGSの設定
            //▼音量
            //音量共通化
            _bgsVolumeInt.maxLength = 3;
            _bgsVolumeSlider.value = _mapDataModel.bgsState.volume;
            _bgsVolumeSlider.RegisterValueChangedCallback(evt =>
            {
                _bgsVolumeValue = (int) _bgsVolumeSlider.value;
                if (_bgsVolumeValue >= 100) _bgsVolumeValue = 100;

                _audioSource.volume = _bgsVolumeValue / 100f;
                if (_bgsVolumeValue >= 100) _bgsVolumeValue = 100;

                _bgsVolumeInt.value = _bgsVolumeValue;
                _mapDataModel.bgsState.volume = _bgsVolumeInt.value;
                Save();
            });
            _bgsVolumeInt.value = _mapDataModel.bgsState.volume;
            _bgsVolumeInt.RegisterCallback<FocusOutEvent>(evt =>
            {
                _bgsVolumeValue = _bgsVolumeInt.value;
                if (_bgsVolumeValue >= 100) _bgsVolumeValue = 100;

                _bgsVolumeSlider.value = _bgsVolumeValue;
                _audioSource.volume = _bgsVolumeValue / 100f;
                if (_bgsVolumeValue >= 100) _bgsVolumeValue = 100;

                _mapDataModel.bgsState.volume = _bgsVolumeValue;
                Save();
            });

            //▼ピッチ
            _bgsPitchInt.maxLength = 3;
            _bgsPitchSlider.value = _mapDataModel.bgsState.pitch;
            _bgsPitchSlider.RegisterValueChangedCallback(evt =>
            {
                _audioSource.pitch = _bgsPitchSlider.value / 100f;
                _bgsPitchInt.value = (int) _bgsPitchSlider.value;
                _mapDataModel.bgsState.pitch = _bgsPitchInt.value;
                Save();
            });
            _bgsPitchInt.value = _mapDataModel.bgsState.pitch;
            _bgsPitchInt.RegisterCallback<FocusOutEvent>(evt =>
            {
                _bgsPitchSlider.value = _bgsPitchInt.value;
                _mapDataModel.bgsState.pitch = _bgsPitchInt.value;
                Save();
            });

            //▼位相
            _bgsPanSlider.value = _mapDataModel.bgsState.pan;
            _bgsPanSlider.RegisterValueChangedCallback(evt =>
            {
                _audioSource.panStereo = _bgsPanSlider.value / 100f;
                _bgsPanInt.value = (int) _bgsPanSlider.value;
                _mapDataModel.bgsState.pan = _bgsPanInt.value;
                Save();
            });
            _bgsPanInt.value = _mapDataModel.bgsState.pan;
            _bgsPanInt.RegisterCallback<FocusOutEvent>(evt =>
            {
                _bgsPanSlider.value = _bgsPanInt.value;
                _mapDataModel.bgsState.pan = _bgsPanInt.value;
                Save();
            });

            _bgsButtonPlay.clicked += () =>
            {
                _audioData = null;
                List<SoundType> soundTypes = new List<SoundType>();
                soundTypes.Add(SoundType.Bgs);
                string filename = SoundHelper.InitializeFileName(soundTypes, _mapDataModel.bgsID, true);
                _audioData = AssetDatabase.LoadAssetAtPath<AudioClip>(filename);
                _audioSource.clip = _audioData;
                _audioSource.volume = _mapDataModel.bgsState.volume / 100f;
                _audioSource.pitch = _mapDataModel.bgsState.pitch / 100f;
                _audioSource.panStereo = _mapDataModel.bgsState.pan / 100f;
                _audioSource.Play();
            };

            _bgsButtonStop.clicked += () => { _audioSource.Stop(); };
        }

        private void ChangeMapSize(int width, int height) {
            DebugUtil.TestLog($"ChangeMapSize({width}, {height})");

            if (width == _mapDataModel.width && height == _mapDataModel.height)
            {
                return;
            }

            var mapEditWindow =
                WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.MapEditWindow) as
                    MapEditWindow;

            // 以下で_mapDataModel.width, _mapDataModel.heightの更新も行われる。
            mapEditWindow.ChangeMapSize(width, height);
        }

        private void SetElementDisable() {
            _mapIdLabel.SetEnabled(false);
            _mapNameText.SetEnabled(false);
            _mapDisplayNameText.SetEnabled(false);
            _mapSizeHeightSlider.SetEnabled(false);
            _mapSizeHeightText.SetEnabled(false);
            _mapSizeWidthSlider.SetEnabled(false);
            _mapSizeWidthText.SetEnabled(false);
            _mapScrollTypeSelect.SetEnabled(false);
            _bgmImport.SetEnabled(false);
            _mapBgmAutoPlayToggle.SetEnabled(false);
            _mapBgmSelect.SetEnabled(false);
            _bgsImport.SetEnabled(false);
            _mapBgsAutoPlayToggle.SetEnabled(false);
            _mapBgsSelect.SetEnabled(false);
            _mapForbidDashToggle.SetEnabled(false);
            _mapMemo.SetEnabled(false);

            RootContainer.Q<Button>("map_thumbnail_update").SetEnabled(false);
            RootContainer.Q<Button>("map_image_save").SetEnabled(false);

            _bgmVolumeSlider.SetEnabled(false);
            _bgmVolumeInt.SetEnabled(false);
            _bgmPitchSlider.SetEnabled(false);
            _bgmPitchInt.SetEnabled(false);
            _bgmPanSlider.SetEnabled(false);
            _bgmPanInt.SetEnabled(false);
            _bgmButtonPlay.SetEnabled(false);
            _bgmButtonStop.SetEnabled(false);

            _bgsVolumeSlider.SetEnabled(false);
            _bgsVolumeInt.SetEnabled(false);
            _bgsPitchSlider.SetEnabled(false);
            _bgsPitchInt.SetEnabled(false);
            _bgsPanSlider.SetEnabled(false);
            _bgsPanInt.SetEnabled(false);
            _bgsButtonPlay.SetEnabled(false);
            _bgsButtonStop.SetEnabled(false);
        }

        private static int GetIndexOfDictionary(Dictionary<string, string> dictionary, string targetKey) {
            var index = dictionary.Keys.ToList().IndexOf(targetKey);
            return index > -1 ? index : 0;
        }

        protected override void SaveContents() {
            base.SaveContents();
            _onChange?.Invoke(_mapDataModel);
        }

        /// <summary>
        /// SliderIntとIntegerFieldで指定範囲の整数値を設定するUIのクラス。
        /// </summary>
        private class IntSliderField
        {
            private readonly SliderInt intSlider;
            private readonly IntegerField intField;

            public IntSliderField(
                SliderInt initialIintSlider,
                IntegerField initialIintField,
                int minValue,
                int maxhValue,
                int initialValue,
                Action<int> setter)
            {
                intSlider = initialIintSlider;
                intField = initialIintField;

                intSlider.lowValue = minValue;
                intSlider.highValue = maxhValue;

                // lowValue, highValue設定後に設定。
                intSlider.value = intField.value = initialValue;

                // 整数スライダー値変更時の処理。
                intSlider.RegisterValueChangedCallback(evt =>
                {
                    intField.value = intSlider.value;
                });

                // 整数スライダー値適用時の処理。
                intSlider.RegisterCallback<MouseCaptureOutEvent>(evt =>
                {
                    setter(intSlider.value);
                });

                // 整数フィールド値変更＆適用時の処理。
                intField.RegisterCallback<FocusOutEvent>(evt =>
                {
                    if (intField.value == intSlider.value) return;
                    intSlider.value = intField.value =
                        Math.Clamp(intField.value, intSlider.lowValue, intSlider.highValue);

                    setter(intField.value);
                });
            }
        }
    }
}