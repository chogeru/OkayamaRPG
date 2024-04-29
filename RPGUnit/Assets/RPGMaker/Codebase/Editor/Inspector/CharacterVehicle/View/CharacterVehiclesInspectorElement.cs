using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Vehicle;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.Enum;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using RPGMaker.Codebase.Editor.Hierarchy.Enum;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.Editor.Common.View.SoundDataList;

namespace RPGMaker.Codebase.Editor.Inspector.CharacterVehicle.View
{
    /// <summary>
    /// [キャラクター]-[乗り物の編集] Inspector
    /// </summary>
    public class CharacterVehiclesInspectorElement : AbstractInspectorElement
    {
        private GameObject _gameObject;

        private readonly string _id;
        private          List<MapDataModel>   _mapData;

        private IntegerField _mapX;
        private IntegerField _mapY;

        private Vector2Int _initialPos;

        private readonly Dictionary<MoveTagEnum, string> _moveTagDictionary =
            EditorLocalize.LocalizeDictionaryValues(new Dictionary<MoveTagEnum, string>
            {
                {MoveTagEnum.NONE, "WORD_0113"},
                {MoveTagEnum.LOW_AIRSPACE, "WORD_1523"},
                {MoveTagEnum.HIGH_AIRSPACE, "WORD_1522"}
            });

        private VehiclesDataModel       _vehiclesData;
        private List<VehiclesDataModel> _vehiclesDataModels;

        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/CharacterVehicle/Asset/inspector_character_vehicles.uxml"; } }

        private int posX;
        private int posY;

        public CharacterVehiclesInspectorElement(string id) {
            _id = id;
            Refresh();
        }

        override protected void RefreshContents() {
            base.RefreshContents();
            _vehiclesDataModels = databaseManagementService.LoadCharacterVehicles();
            _mapData = mapManagementService.LoadMaps();

            for (var i = 0; i < _vehiclesDataModels.Count; i++)
                if (_vehiclesDataModels[i].id == _id)
                {
                    _vehiclesData = _vehiclesDataModels[i];
                    break;
                }

            Initialize();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();

            //AssetData
            var manageData = databaseManagementService.LoadAssetManage();

            //サウンドの取得
            AudioSource audioSource;
            AudioClip audioData;
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


            Label vehiclesId = RootContainer.Query<Label>("vehicles_id");
            vehiclesId.text = _vehiclesData.SerialNumberString;

            ImTextField vehiclesName = RootContainer.Query<ImTextField>("vehicles_name");
            vehiclesName.value = _vehiclesData.name;
            vehiclesName.RegisterCallback<FocusOutEvent>(evt =>
            {
                _vehiclesData.name = vehiclesName.value;
                Save();
                _UpdateSceneView();
            });


            //移動領域プルダウン
            VisualElement moveAriaDropdownDropdown = RootContainer.Query<VisualElement>("move_aria");
            var moveAriaDropdownDropdownChoices = _moveTagDictionary.Values.ToList();

            var selectmoveAriaDropdownPopupField =
                new PopupFieldBase<string>(moveAriaDropdownDropdownChoices, (int)_vehiclesData.MoveAria);
            moveAriaDropdownDropdown.Add(selectmoveAriaDropdownPopupField);
            selectmoveAriaDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                for (var i = 0; i < _vehiclesData.moveTags.Count; i++) _vehiclesData.moveTags[i] = 0;

                _vehiclesData.moveTags[selectmoveAriaDropdownPopupField.index] =
                    selectmoveAriaDropdownPopupField.index + 1;
                Save();
            });

            var vehiclesSpeedArea = RootContainer.Query<VisualElement>("vehicles_speedArea");
            SliderAndFiledBase.IntegerSliderCallBack(vehiclesSpeedArea, 0, 100, "",
                _vehiclesData.speed, evt =>
                {
                    _vehiclesData.speed = (int) evt;
                    Save();
                });

            //初期位置の座標表示部分
            MapDataModel mapDataModel = null;
            if (_vehiclesData.mapId != "")
            {
                mapDataModel = _mapData.Find(map =>
                    map.id == _vehiclesData.mapId);
            }

            if (mapDataModel != null)
            {
                MapEditor.MapEditor.LaunchCommonEventEditModeEnd(mapDataModel);
            }

            _mapX = RootContainer.Query<IntegerField>("map_X");
            _mapY = RootContainer.Query<IntegerField>("map_Y");
            _mapX.value = _vehiclesData.initialPos[0];
            _mapY.value = _vehiclesData.initialPos[1] * -1;
            _mapX.RegisterCallback<FocusOutEvent>(evt =>
            {
                // 指定座標に配置可能か
                if (CanPutPos(_mapData.Find(map => map.id == _vehiclesData.mapId),
                    new Vector2Int(_mapX.value,
                    _vehiclesData.initialPos[1])) == true)
                {
                    _vehiclesData.initialPos[0] = _mapX.value;
                    Save();
                    MapEditor.MapEditor.LaunchCommonEventEditModeEnd(_mapData.Find(map =>
                        map.id == _vehiclesData.mapId));
                }
                else
                {
                    _mapX.value = _vehiclesData.initialPos[0];
                }
            });

            _mapY.RegisterCallback<FocusOutEvent>(evt =>
            {
                // 指定座標に配置可能か
                if (CanPutPos(_mapData.Find(map => map.id == _vehiclesData.mapId),
                    new Vector2Int(_vehiclesData.initialPos[0],
                    _mapY.value * -1)) == true)
                {
                    _vehiclesData.initialPos[1] = _mapY.value * -1;
                    Save();
                    MapEditor.MapEditor.LaunchCommonEventEditModeEnd(_mapData.Find(map =>
                        map.id == _vehiclesData.mapId));
                }
                else
                {
                    _mapY.value = _vehiclesData.initialPos[1] * -1;
                }
            });

            VisualElement selectMapDropdownDropdown = RootContainer.Query<VisualElement>("selectMap_dropdown");
            var selectMapDropdownDropdownChoices = MapNameList();

            var mapIndex = 0;
            for (var i = 0; i < _mapData.Count; i++)
                if (_mapData[i].id == _vehiclesData.mapId)
                {
                    mapIndex = i + 1;
                    break;
                }

            Button designationButton = RootContainer.Query<Button>("designation_button");
            designationButton.SetEnabled(mapIndex != 0);
            _mapX.SetEnabled(mapIndex != 0);
            _mapY.SetEnabled(mapIndex != 0);
            var selectMapDropdownDropdownPopupField =
                new PopupFieldBase<string>(selectMapDropdownDropdownChoices, mapIndex);
            selectMapDropdownDropdown.Add(selectMapDropdownDropdownPopupField);
            selectMapDropdownDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                //なしの時は空
                if (selectMapDropdownDropdownPopupField.index == 0)
                {
                    mapIndex = selectMapDropdownDropdownPopupField.index;
                    if (_vehiclesData.mapId != "")
                        MapEditor.MapEditor.LaunchCommonEventEditModeEnd(_mapData.Find(map =>
                            map.id == _vehiclesData.mapId));
                    _vehiclesData.mapId = "";
                }
                // 配置可能か
                else if (CanPutVehicle(_mapData[selectMapDropdownDropdownPopupField.index - 1]) == true)
                {
                    // 現在の座標に配置可能か
                    if (CanPutPos(_mapData[selectMapDropdownDropdownPopupField.index - 1],
                        new Vector2Int(_vehiclesData.initialPos[0],
                        _vehiclesData.initialPos[1])) == false)
                    {
                        _mapX.value = _initialPos.x;
                        _mapY.value = _initialPos.y * -1;
                        _vehiclesData.initialPos[0] = _initialPos.x;
                        _vehiclesData.initialPos[1] = _initialPos.y;
                    }

                    _vehiclesData.mapId = _mapData[selectMapDropdownDropdownPopupField.index - 1].id;
                    MapEditor.MapEditor.LaunchCommonEventEditModeEnd(_mapData.Find(map =>
                        map.id == _vehiclesData.mapId));

                    mapIndex = selectMapDropdownDropdownPopupField.index;
                }
                else
                {
                    // 配置不可時は元に戻す
                    selectMapDropdownDropdownPopupField.index = mapIndex;
                    selectMapDropdownDropdownPopupField.ChangeButtonText(mapIndex);
                }

                if (_vehiclesData.mapId != "")
                {
                    designationButton.SetEnabled(true);
                    _mapX.SetEnabled(true);
                    _mapY.SetEnabled(true);
                }
                else
                {
                    designationButton.SetEnabled(false);
                    _mapX.SetEnabled(false);
                    _mapY.SetEnabled(false);
                }
            });

            //マップ指定ボタンが押されたらセーブされる
            var isEdit = false;
            designationButton.text = EditorLocalize.LocalizeText("WORD_1583");
            designationButton.clickable.clicked += () =>
            {
                if (isEdit)
                {
                    designationButton.text = EditorLocalize.LocalizeText("WORD_1583");
                    EndMapPosition(_vehiclesData.mapId);
                    selectMapDropdownDropdownPopupField.SetEnabled(true);
                }
                else
                {
                    designationButton.text = EditorLocalize.LocalizeText("WORD_1584");
                    SetMapPosition(_vehiclesData.mapId);
                    selectMapDropdownDropdownPopupField.SetEnabled(false);
                }

                isEdit = !isEdit;
            };

            //BGMの設定
            //▼サウンドファイル選択
            List<SoundType> soundTypes = new List<SoundType>();
            soundTypes.Add(SoundType.Bgm);
            var SoundsettingDropdownPopupField = GenericPopupFieldBase<SoundDataChoice>.Add(
                RootContainer,
                "Soundsetting_dropdown",
                SoundDataList.GenerateChoices(soundTypes),
                SoundHelper.InitializeFileName(soundTypes, _vehiclesData.bgm.name, false));

            SoundsettingDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                //0番目は曲無し
                if (SoundsettingDropdownPopupField.index == 0)
                    _vehiclesData.bgm.name = "";
                else
                    _vehiclesData.bgm.name = SoundsettingDropdownPopupField.value.filename + "." + SoundsettingDropdownPopupField.value.extention;

                Save();
            });

            //▼音量
            //音量共通化
            int volumeValue;
            var volumeSliderArea = RootContainer.Query<VisualElement>("volume_sliderArea");
            SliderAndFiledBase.IntegerSliderCallBack(volumeSliderArea, 0, 100, "%",
                _vehiclesData.bgm.volume, evt =>
                {
                    volumeValue = (int) evt;
                    audioSource.volume = volumeValue / 100f;
                    _vehiclesData.bgm.volume = volumeValue;
                    Save();
                });

            //▼ピッチ
            var pitchSliderArea = RootContainer.Query<VisualElement>("pitch_sliderArea");
            SliderAndFiledBase.IntegerSliderCallBack(pitchSliderArea, 50, 150, "%",
                _vehiclesData.bgm.pitch, evt =>
                {
                    audioSource.pitch = evt / 100f;
                    _vehiclesData.bgm.pitch = evt; 
                    Save();
                });
            
            //▼位相
            var panSliderArea = RootContainer.Query<VisualElement>("pan_sliderArea");
            SliderAndFiledBase.IntegerSliderCallBack(panSliderArea, -100, 100, "",
                _vehiclesData.bgm.pan, evt =>
                {
                    audioSource.panStereo = evt / 100f;
                    _vehiclesData.bgm.pan = evt;
                    Save();
                });

            var buttonPlay = RootContainer.Query<Button>("musicStart")
                .AtIndex(0);
            var buttonStop = RootContainer.Query<Button>("musicStop")
                .AtIndex(0);
            buttonPlay.clicked += () =>
            {
                string filename = SoundHelper.InitializeFileName(soundTypes, _vehiclesData.bgm.name, true);
                audioData = AssetDatabase.LoadAssetAtPath<AudioClip>(filename);
                audioSource.clip = audioData;
                audioSource.volume = _vehiclesData.bgm.volume / 100f;
                audioSource.pitch = _vehiclesData.bgm.pitch / 100f;
                audioSource.panStereo = _vehiclesData.bgm.pan / 100f;
                audioSource.Play();
            };

            buttonStop.clicked += () => { audioSource.Stop(); };

            // 乗り物の画像
            //------------------------------------------------------------------------------------------------------------------------------
            // プレビュー画像
            Image vehicleImage = RootContainer.Query<Image>("vehicle_image_preview");
            vehicleImage.scaleMode = ScaleMode.ScaleToFit;

            // 画像名
            string imageNameWork = "";
            string assetId = "";
            for (int i = 0; i < manageData.Count; i++)
            {
                if (manageData[i].id == _vehiclesData.images)
                {
                    imageNameWork = manageData[i].name;
                    assetId = manageData[i].id;
                    break;
                }
            }
            vehicleImage.image = ImageManager.LoadSvCharacter(assetId);

            Label vehicleImageName = RootContainer.Query<Label>("vehicle_image_name");
            vehicleImageName.text = imageNameWork; //_vehiclesData.images;

            // 画像変更ボタン
            Button vehicleChangeBtn = RootContainer.Query<Button>("vehicle_image_change_btn");
            vehicleChangeBtn.clicked += () =>
            {
                var selectModalWindow = new SdSelectModalWindow(isObjectOnly: true);
                selectModalWindow.CharacterSdType = SdSelectModalWindow.CharacterType.Map;
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Image"), data =>
                {
                    var imageName = (string) data;
                    _vehiclesData.images = imageName;
                    var svCharacter = ImageManager.LoadSvCharacter(imageName);
                    vehicleImage.image = svCharacter;

                    for (int i = 0; i < manageData.Count; i++)
                    {
                        if (manageData[i].id == _vehiclesData.images)
                        {
                            imageNameWork = manageData[i].name;
                            break;
                        }
                    }

                    vehicleImageName.text = imageNameWork;
                    Save();
                }, _vehiclesData.images);
            };

            // 画像インポートボタン
            // 素材管理からしかインポート不可のためボタンなし
        }


        public void SetMapPosition(string mapId) {
            posX = -1;
            posY = -1;

            var mapDataModel = _mapData.Find(map => map.id == mapId);
            MapEditor.MapEditor.LaunchCommonEventEditMode(mapDataModel, 0,
                v =>
                {
                    posX = v.x;
                    posY = v.y;
                    _mapX.value = v.x;
                    _mapY.value = v.y * -1;
                },
                _vehiclesData.id,
                true);
        }

        public void EndMapPosition(string mapId) {
            var mapDataModel = _mapData.Find(map =>
                map.id == mapId);
            MapEditor.MapEditor.LaunchCommonEventEditModeEnd(mapDataModel);

            if (posX == -1 && posY == -1)
                return;

            _vehiclesData.initialPos[0] = posX;
            _vehiclesData.initialPos[1] = posY;

            Save();
        }

        override protected void SaveContents() {
            databaseManagementService.SaveCharacterVehicles(_vehiclesDataModels);
        }

        private void _UpdateSceneView() {
            _ = Editor.Hierarchy.Hierarchy.Refresh(Region.Character, _vehiclesData.id);
        }


        private List<string> MapNameList() {
            var returnList = new List<string>();

            //事前に「なし」だけ入れておく
            returnList.Add(EditorLocalize.LocalizeText("WORD_0113"));

            foreach (var data in _mapData) returnList.Add(data.name);

            return returnList;
        }

        // 配置できるか
        private bool CanPutVehicle(MapDataModel mapDataModel) {
            // 配置物の取得
            var vehicles = databaseManagementService.LoadCharacterVehicles().FindAll(vehicle => vehicle.mapId == mapDataModel.id);
            var events = new EventManagementService().LoadEventMap();
            var mapEvents = events.FindAll(item => item.mapId == mapDataModel.id);
            var stMap = databaseManagementService.LoadSystem().initialParty.startMap;

            // 配置チェック
            for (int y = 0; y < mapDataModel.height; y++)
            {
                for (int x = 0; x < mapDataModel.width; x++)
                {
                    bool found = false;
                    foreach (var e in mapEvents)
                    {
                        if (e.x == x && e.y * -1 == y)
                        {
                            found = true;
                            break;
                        }
                    }

                    foreach (var v in vehicles)
                    {
                        if (v.id != _vehiclesData.id && v.initialPos[0] == x && v.initialPos[1] * -1 == y)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (stMap.mapId == _vehiclesData.mapId && stMap.position[0] == x && stMap.position[1] * -1 == y)
                    {
                        found = true;
                        break;
                    }

                    if (found == false)
                    {
                        _initialPos = new Vector2Int(x, y * -1);
                        return true;
                    }
                }
            }

            // メッセージ表示
            EditorUtility.DisplayDialog(EditorLocalize.LocalizeText("WORD_0295"),
                        EditorLocalize.LocalizeText("WORD_3063"),
                        EditorLocalize.LocalizeText("WORD_3051"));

            return false;
        }

        // 指定位置に配置できるか
        private bool CanPutPos(MapDataModel mapDataModel, Vector2Int pos) {

            if (mapDataModel == null) return false;

            if (mapDataModel.width - 1 < pos.x || mapDataModel.height - 1 < pos.y * -1 || 0 > pos.x || 0 > pos.y * -1)
                return false;

            // 配置物の取得
            var vehicles = databaseManagementService.LoadCharacterVehicles().FindAll(vehicle => vehicle.mapId == mapDataModel.id);
            var events = new EventManagementService().LoadEventMap();
            var mapEvents = events.FindAll(item => item.mapId == mapDataModel.id);
            var stMap = databaseManagementService.LoadSystem().initialParty.startMap;

            // 配置チェック
            foreach (var e in mapEvents)
                if (e.x == pos.x && e.y == pos.y)
                    return false;

            foreach (var v in vehicles)
                if (_vehiclesData.id != v.id && v.mapId == mapDataModel.id && v.initialPos[0] == pos.x && v.initialPos[1] == pos.y)
                    return false;

            if (stMap.mapId == _vehiclesData.mapId && stMap.position[0] == pos.x && stMap.position[1] * -1 == pos.y)
                return false;

            return true;
        }
    }
}