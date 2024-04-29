using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Vehicle;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.Map.View
{
    /// <summary>
    /// [マップ設定] 内で利用するタイル用のInspector
    /// </summary>
    public class TileInspector : AbstractInspectorElement
    {
        // 呼び出し元のタイプによってInspectorを変更する為の定義
        public enum TYPE
        {
            NORMAL,
            TILEGROUP,
            IMAGE
        }

        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/Map/Asset/TileInspector.uxml"; } }
        private const string vehicleTrafficUxml =
            "Assets/RPGMaker/Codebase/Editor/Inspector/Map/Asset/VehiclePass.uxml";

        private Foldout                    _animationFoldout;
        private Label                      _animationFrameAmountLabel;
        private IntegerField               _animationFrameAmountText;
        private IntegerField               _animationSpeedText;
        private Label                      _animationToggleLabel;
        private VisualElement              _animationToggleElem;
        private Toggle                     _animationToggle;
        private FloatField                 _damagefloatValue;
        private Dictionary<string, string> _damageFloorTypeDictionary;
        private IntegerField               _damageIntValue;
        private IntegerField               _terrainTagIntValue;
        private int                        _terrainTagMax = 7;
        private Button                     _terrainTagMinusBtn;
        private Button                     _terrainTagPlusBtn;

        private readonly DatabaseManagementService  _databaseManagementService;
        private          Dictionary<string, string> _imageAdjustTypeDictionary;
        private          Foldout                    _imageAdjustTypeFoldout;
        private          List<RadioButton>               _imageAdjustTypeToggleList;

        private readonly TYPE                       _inspectorType;
        private          RadioButton                     _isBushToggleOff;
        private          RadioButton                     _isBushToggleOn;
        private RadioButton _isCounterToggleOff;
        private RadioButton _isCounterToggleOn;
        private          RadioButton                     _isDamageFixedToggle;
        private RadioButton _isDamageFloorToggleOff;
        private RadioButton _isDamageFloorToggleOn;
        private          RadioButton                     _isDamageRatioToggle;
        private          RadioButton                     _isLadderToggleOff;
        private          RadioButton                     _isLadderToggleOn;
        private          Toggle                     _isPassBottomToggle;
        private          Toggle                     _isPassLeftToggle;
        private          Toggle                     _isPassRightToggle;
        private          Toggle                     _isPassToggle;
        private          Toggle                     _isPassTopToggle;
        private          Toggle                     _isPassUnderToggle;
        private readonly MapManagementService       _mapManagementService;
        private readonly Action                     _onClickRegisterBtn;
        private          Dictionary<string, string> _passTypeDictionary;
        private          Button                     _registerBtn;
        private          bool                       _isFindTile;

        private readonly TileDataModel _tileDataModel;

        private Label              _tileIdLabel;
        private VisualElement      _tileImage;
        private TileImageDataModel _tileImageDataModel;
        private ImTextField          _tileNameText;

        private VisualElement _tileSetting;

        private Dictionary<string, string> _tileTypeDictionary;
        private PopupFieldBase<string>     _tileTypeSelect;
        private Button                     _vehicleAddButton;

        //乗り物部分UI
        private VisualElement              _vehicleAria;
        private Button                     _vehicleDelete;
        private VisualElement              _vehicleFoldDown;
        private Dictionary<string, string> _vehiclePassTypeDictionary;

        private readonly List<VehiclesDataModel> _vehiclesDataModels;

        public TileInspector(TileDataModel tileDataModel, Action onClickRegisterBtn, TYPE inspectorType = TYPE.NORMAL) {
            _tileDataModel = tileDataModel;
            _onClickRegisterBtn = onClickRegisterBtn;
            _databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;
            _mapManagementService = Editor.Hierarchy.Hierarchy.mapManagementService;
            _vehiclesDataModels = _databaseManagementService.LoadCharacterVehicles();

            _inspectorType = inspectorType;

            _isFindTile = false;
            var tiles = _mapManagementService.LoadTileTable();
            for (int i = 0; i < tiles.Count; i++)
                if (tiles[i].id == _tileDataModel.id)
                {
                    _isFindTile = true;
                    break;
                }

            LoadDictionaries();
            Initialize();
        }

        protected override void RefreshContents() {
            base.RefreshContents();
            LoadDictionaries();
            Initialize();
        }

        private void LoadDictionaries() {
            _tileTypeDictionary = EditorLocalize.LocalizeDictionaryValues(new Dictionary<string, string>
            {
                {TileDataModel.Type.AutoTileA.ToString(), "WORD_1524"},
                {TileDataModel.Type.AutoTileB.ToString(), "WORD_1525"},
                {TileDataModel.Type.AutoTileC.ToString(), "WORD_1526"},
                {TileDataModel.Type.NormalTile.ToString(), "WORD_1527"},
                {TileDataModel.Type.LargeParts.ToString(), "WORD_0744"},
                {TileDataModel.Type.Effect.ToString(), "WORD_0745"}
            });
            _imageAdjustTypeDictionary = EditorLocalize.LocalizeDictionaryValues(new Dictionary<string, string>
            {
                {TileDataModel.ImageAdjustType.None.ToString(), "WORD_0113"},
                {TileDataModel.ImageAdjustType.Scale.ToString(), "WORD_0758"},
                {TileDataModel.ImageAdjustType.Split.ToString(), "WORD_0759"}
            });
            _passTypeDictionary = EditorLocalize.LocalizeDictionaryValues(new Dictionary<string, string>
            {
                {TileDataModel.PassType.CanPassNormally.ToString(), "WORD_0808"},
                {TileDataModel.PassType.CanPassUnder.ToString(), "WORD_0809"},
                {TileDataModel.PassType.CannotPass.ToString(), "WORD_0810"}
            });
            _vehiclePassTypeDictionary = EditorLocalize.LocalizeDictionaryValues(new Dictionary<string, string>
            {
                {TileDataModel.PassType.CanPassNormally.ToString(), "WORD_0808"},
                {TileDataModel.PassType.CanPassUnder.ToString(), "WORD_0809"},
                {TileDataModel.PassType.CannotPass.ToString(), "WORD_0810"}
            });
            _damageFloorTypeDictionary = EditorLocalize.LocalizeDictionaryValues(new Dictionary<string, string>
            {
                {TileDataModel.DamageFloorType.Fix.ToString(), "WORD_0820"},
                {TileDataModel.DamageFloorType.Rate.ToString(), "WORD_0821"},
                {TileDataModel.DamageFloorType.None.ToString(), "WORD_0113"}
            });
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();

            _tileSetting = RootContainer.Query<VisualElement>("tile_setting");

            _tileIdLabel = RootContainer.Query<Label>("tile_id_label");
            _tileNameText = RootContainer.Query<ImTextField>("tile_name_text");
            _tileTypeSelect = MakePopupField(_tileTypeDictionary, RootContainer, "tile_type_select_container",
                GetIndexOfDictionary(_tileTypeDictionary, _tileDataModel.type.ToString()));
            _imageAdjustTypeFoldout = RootContainer.Query<Foldout>("image_adjust_type_select_foldout");
            _imageAdjustTypeToggleList = RootContainer.Query<RadioButton>("radioButton-mapEdit-display-tile_adjust").ToList();
            _animationFoldout = RootContainer.Query<Foldout>("animation_foldout");
            _animationToggleLabel = RootContainer.Query<Label>("animation_toggle_label");
            _animationToggleElem = RootContainer.Query<VisualElement>("multiple_item_in_row");
            _animationToggle = RootContainer.Query<Toggle>("animation_toggle");
            _animationFrameAmountLabel = RootContainer.Query<Label>("animation_frame_amount_label");
            _animationFrameAmountText = RootContainer.Query<IntegerField>("animation_frame_amount_text");
            _animationSpeedText = RootContainer.Query<IntegerField>("animation_speed_text");
            _isPassToggle = RootContainer.Query<Toggle>("is_pass_toggle");
            _isPassUnderToggle = RootContainer.Query<Toggle>("is_pass_under_toggle");
            _isPassTopToggle = RootContainer.Query<Toggle>("is_pass_top_toggle");
            _isPassBottomToggle = RootContainer.Query<Toggle>("is_pass_bottom_toggle");
            _isPassLeftToggle = RootContainer.Query<Toggle>("is_pass_left_toggle");
            _isPassRightToggle = RootContainer.Query<Toggle>("is_pass_right_toggle");
            _isLadderToggleOn = RootContainer.Query<RadioButton>("radioButton-mapEdit-display1");
            _isLadderToggleOff = RootContainer.Query<RadioButton>("radioButton-mapEdit-display2");
            _isBushToggleOn = RootContainer.Query<RadioButton>("radioButton-mapEdit-display3");
            _isBushToggleOff = RootContainer.Query<RadioButton>("radioButton-mapEdit-display4");
            _isCounterToggleOn = RootContainer.Query<RadioButton>("radioButton-mapEdit-display5");
            _isCounterToggleOff = RootContainer.Query<RadioButton>("radioButton-mapEdit-display6");
            _isDamageFloorToggleOn = RootContainer.Query<RadioButton>("radioButton-mapEdit-display7");
            _isDamageFloorToggleOff = RootContainer.Query<RadioButton>("radioButton-mapEdit-display8");
            _isDamageFixedToggle = RootContainer.Query<RadioButton>("radioButton-mapEdit-display9");
            _isDamageRatioToggle = RootContainer.Query<RadioButton>("radioButton-mapEdit-display10");
            _damageIntValue = RootContainer.Query<IntegerField>("damage_int_value_text");
            _damagefloatValue = RootContainer.Query<FloatField>("damage_float_value_text");
            _terrainTagIntValue = RootContainer.Query<IntegerField>("terrain_tag_int_value_text");
            _terrainTagMinusBtn = RootContainer.Query<Button>("terrain_tag_minus_btn");
            _terrainTagPlusBtn = RootContainer.Query<Button>("terrain_tag_plus_btn");
            _registerBtn = RootContainer.Query<Button>("register_btn");
            _tileImage = RootContainer.Query<VisualElement>("tile_image");
            _vehicleAria = RootContainer.Query<VisualElement>("vehicle_aria");
            _vehicleAddButton = RootContainer.Query<Button>("vehicle_add_button");

            SetEntityToUI();
        }

        private static PopupFieldBase<string> MakePopupField(
            Dictionary<string, string> dictionary,
            VisualElement parentContainer,
            string containerName,
            int index
        ) {
            var RootContainer = (VisualElement) parentContainer.Query<VisualElement>(containerName);
            var popupField = new PopupFieldBase<string>(dictionary.Values.ToList(), index);
            RootContainer.Add(popupField);
            return popupField;
        }

        private void SetEntityToUI() {
            _tileIdLabel.text = _tileDataModel.SerialNumberString;
            _tileNameText.value = _tileDataModel.name;
            _tileNameText.RegisterCallback<FocusOutEvent>(evt =>
            {
                _tileDataModel.name = _tileNameText.value;
                Save();
            });
            _tileTypeSelect.index = GetIndexOfDictionary(_tileTypeDictionary, _tileDataModel.type.ToString());
            _tileTypeSelect.RegisterValueChangedCallback(evt =>
            {
                var selectedKey = _tileTypeDictionary.FirstOrDefault(kv => kv.Value == _tileTypeSelect.value).Key;
                _tileDataModel.type = (TileDataModel.Type) Enum.Parse(typeof(TileDataModel.Type), selectedKey);
                ChangeDisplayRules(_tileDataModel.type);
                Save();
            });
            _tileTypeSelect.SetEnabled(_inspectorType == TYPE.IMAGE);
            // タイルの表示設定
            // 空データの追加
            _imageAdjustTypeToggleList.Add(new RadioButton());
            new CommonToggleSelector().SetRadioSelector(_imageAdjustTypeToggleList,
                GetIndexOfDictionary(_imageAdjustTypeDictionary, _tileDataModel.imageAdjustType.ToString()),
                new List<Action>
                {
                    () =>
                    {
                        _tileDataModel.imageAdjustType = TileDataModel.ImageAdjustType.Scale;
                    },
                    () =>
                    {
                        _tileDataModel.imageAdjustType = TileDataModel.ImageAdjustType.Split;
                    },
                    () =>
                    {
                        _tileDataModel.imageAdjustType = TileDataModel.ImageAdjustType.None;
                    }
                });
            ChangeDisplayRules(_tileDataModel.type);

            // アニメーションフレーム
            _animationToggle.value = _tileDataModel.hasAnimation;
            _animationToggle.RegisterValueChangedCallback(evt =>
            {
                _tileDataModel.hasAnimation = evt.newValue;
                Save();
            });
            _animationFrameAmountText.value = _tileDataModel.animationFrame;
            if (_animationFrameAmountText.value < 1)
            {
                _animationFrameAmountText.value = 1;
            }else if (_animationFrameAmountText.value > 999)
            {
                _animationFrameAmountText.value = 999;
            }
            BaseInputFieldHandler.IntegerFieldCallback(_animationFrameAmountText, evt =>
            {
                 _tileDataModel.animationFrame = _animationFrameAmountText.value;
                                if (_isFindTile == true) 
                                    _onClickRegisterBtn?.Invoke();
                                Save();
            },1, 999);

            // アニメーションスピード
            _animationSpeedText.value = _tileDataModel.animationSpeed;
            if (_animationSpeedText.value < 1)
            {
                _animationSpeedText.value = 1;
            }else if (_animationSpeedText.value > 999)
            {
                _animationSpeedText.value = 999;
            }

            BaseInputFieldHandler.IntegerFieldCallback(_animationSpeedText, evt =>
            {
                _tileDataModel.animationSpeed = _animationSpeedText.value;
                // TilingRulesに即反映されるよう呼ぶ
                if (_isFindTile == true)
                    _onClickRegisterBtn?.Invoke();
                Save();
            }, 1, 999);

            // 通行設定
            switch (_tileDataModel.passType)
            {
                case TileDataModel.PassType.CanPassNormally:
                    _isPassToggle.value = true;
                    break;
                case TileDataModel.PassType.CanPassUnder:
                    _isPassUnderToggle.value = true;
                    break;
                case TileDataModel.PassType.CannotPass:
                    break;
            }

            // 『通れる』トグル。
            _isPassToggle.RegisterValueChangedCallback(evt => 
            {
                OnPassToggleChanged(
                    _isPassToggle,
                    _isPassUnderToggle,
                    ref _tileDataModel.passType,
                    TileDataModel.PassType.CanPassNormally,
                    true);

                Save();
            });

            // 『下を潜って通れる』トグル。
            _isPassUnderToggle.RegisterValueChangedCallback(evt =>
            {
                OnPassToggleChanged(
                    _isPassUnderToggle,
                    _isPassToggle,
                    ref _tileDataModel.passType,
                    TileDataModel.PassType.CanPassUnder,
                    true);

                Save();
            });

            // 『上』トグル。
            _isPassTopToggle.value = _tileDataModel.isPassTop;
            _isPassTopToggle.RegisterValueChangedCallback(evt =>
            {
                _tileDataModel.isPassTop = evt.newValue;
                Save();
            });

            // 『下』トグル。
            _isPassBottomToggle.value = _tileDataModel.isPassBottom;
            _isPassBottomToggle.RegisterValueChangedCallback(evt =>
            {
                _tileDataModel.isPassBottom = evt.newValue;
                Save();
            });

            // 『左』トグル。
            _isPassLeftToggle.value = _tileDataModel.isPassLeft;
            _isPassLeftToggle.RegisterValueChangedCallback(evt =>
            {
                _tileDataModel.isPassLeft = evt.newValue;
                Save();
            });

            // 『右』トグル。
            _isPassRightToggle.value = _tileDataModel.isPassRight;
            _isPassRightToggle.RegisterValueChangedCallback(evt =>
            {
                _tileDataModel.isPassRight = evt.newValue;
                Save();
            });

            // 梯子
            new CommonToggleSelector().SetRadioSelector(new List<RadioButton> {_isLadderToggleOn, _isLadderToggleOff},
                Convert.ToInt32(!_tileDataModel.isLadder),
                new List<Action>
                {
                    () =>
                    {
                        _tileDataModel.isLadder = true;
                        Save();
                    },
                    () =>
                    {
                        _tileDataModel.isLadder = false;
                        Save();
                    }
                });
            // 茂み
            new CommonToggleSelector().SetRadioSelector(new List<RadioButton> {_isBushToggleOn, _isBushToggleOff},
                Convert.ToInt32(!_tileDataModel.isBush),
                new List<Action>
                {
                    () =>
                    {
                        _tileDataModel.isBush = true;
                        Save();
                    },
                    () =>
                    {
                        _tileDataModel.isBush = false;
                        Save();
                    }
                });

            if (_tileDataModel.type == TileDataModel.Type.AutoTileC)
            {
                _isCounterToggleOff.SetEnabled(false);
            }
            // カウンター属性
            new CommonToggleSelector().SetRadioSelector(new List<RadioButton> {_isCounterToggleOn, _isCounterToggleOff},
                Convert.ToInt32(!_tileDataModel.isCounter),
                new List<Action>
                {
                    () =>
                    {
                        _tileDataModel.isCounter = true;
                        Save();
                    },
                    () =>
                    {
                        _tileDataModel.isCounter = false;
                        Save();
                    }
                });
            // カウンター属性
            new CommonToggleSelector().SetRadioSelector(new List<RadioButton> {_isCounterToggleOn, _isCounterToggleOff},
                Convert.ToInt32(!_tileDataModel.isCounter),
                new List<Action>
                {
                    () =>
                    {
                        _tileDataModel.isCounter = true;
                        Save();
                    },
                    () =>
                    {
                        _tileDataModel.isCounter = false;
                        Save();
                    }
                });
            // ダメージタイプ用の一時データ
            var toggle = new RadioButton();
            // ダメージ属性
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {_isDamageFloorToggleOn, _isDamageFloorToggleOff},
                Convert.ToInt32(!_tileDataModel.isDamageFloor),
                new List<Action>
                {
                    () =>
                    {
                        _tileDataModel.isDamageFloor = true;
                        _isDamageFixedToggle.SetEnabled(true);
                        _isDamageRatioToggle.SetEnabled(true);
                        if (_tileDataModel.damageFloorType == TileDataModel.DamageFloorType.Fix)
                        {
                            _damageIntValue.SetEnabled(true);
                            _damagefloatValue.SetEnabled(false);
                        }
                        else if (_tileDataModel.damageFloorType == TileDataModel.DamageFloorType.Rate)
                        {
                            _damageIntValue.SetEnabled(false);
                            _damagefloatValue.SetEnabled(true);
                        }
                        else
                        {
                            _damageIntValue.SetEnabled(false);
                            _damagefloatValue.SetEnabled(false);
                        }
                        Save();
                    },
                    () =>
                    {
                        _tileDataModel.isDamageFloor = false;
                        _isDamageFixedToggle.SetEnabled(false);
                        _isDamageRatioToggle.SetEnabled(false);
                        _damageIntValue.SetEnabled(false);
                        _damagefloatValue.SetEnabled(false);
                        toggle.value = false;
                        Save();
                    }
                });

            var defaultSelect  = (int) _tileDataModel.damageFloorType;
            if (defaultSelect  > 1) defaultSelect  = 0;
            
            // ダメージタイプ
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {_isDamageFixedToggle, _isDamageRatioToggle},
                defaultSelect,
                new List<Action>
                {
                    () =>
                    {
                        _tileDataModel.damageFloorType = TileDataModel.DamageFloorType.Fix;
                        _damageIntValue.SetEnabled(true);
                        _damagefloatValue.SetEnabled(false);
                        Save();
                    },
                    () =>
                    {
                        _tileDataModel.damageFloorType = TileDataModel.DamageFloorType.Rate;
                        _damageIntValue.SetEnabled(false);
                        _damagefloatValue.SetEnabled(true);
                        Save();
                    }
                });
            _damageIntValue.value = (int) _tileDataModel.damageFloorValue;
            BaseInputFieldHandler.IntegerFieldCallback(_damageIntValue, evt =>
            {
                _tileDataModel.damageFloorValue = _damageIntValue.value;
                Save();
            }, 0, 9999999);
            _damagefloatValue.value = _tileDataModel.damageFloorValue;
            BaseInputFieldHandler.FloatFieldCallback(_damagefloatValue, evt =>
            {
                _tileDataModel.damageFloorValue = _damagefloatValue.value;
                Save();
            }, 0, 200, 2);

            if (!_tileDataModel.isDamageFloor)
            {
                _isDamageFixedToggle.SetEnabled(false);
                _isDamageRatioToggle.SetEnabled(false);
                _damageIntValue.SetEnabled(false);
                _damagefloatValue.SetEnabled(false);
            }
            else
            {
                _isDamageFixedToggle.SetEnabled(true);
                _isDamageRatioToggle.SetEnabled(true);
                if (_tileDataModel.damageFloorType == TileDataModel.DamageFloorType.Fix)
                {
                    _damageIntValue.SetEnabled(true);
                    _damagefloatValue.SetEnabled(false);
                }
                else if (_tileDataModel.damageFloorType == TileDataModel.DamageFloorType.Rate)
                {
                    _damageIntValue.SetEnabled(false);
                    _damagefloatValue.SetEnabled(true);
                }
                else
                {
                    _damageIntValue.SetEnabled(false);
                    _damagefloatValue.SetEnabled(false);
                }
            }

            // 地形タグ
            _terrainTagIntValue.value = (int) _tileDataModel.terrainTagValue;
            BaseInputFieldHandler.IntegerFieldCallback(_terrainTagIntValue, evt =>
            {
                _tileDataModel.terrainTagValue = _terrainTagIntValue.value;
                Save();
            }, 0, _terrainTagMax);
            _terrainTagMinusBtn.clicked += () =>
            {
                _terrainTagIntValue.value = Math.Max(0, _terrainTagIntValue.value - 1);
                _tileDataModel.terrainTagValue = _terrainTagIntValue.value;
                Save();
            };
            _terrainTagPlusBtn.clicked += () =>
            {
                _terrainTagIntValue.value = Math.Min(_terrainTagMax, _terrainTagIntValue.value + 1);
                _tileDataModel.terrainTagValue = _terrainTagIntValue.value;
                Save();
            };

            // 画像プレビューの設定
            //------------------------------------------------------------------------------
            Texture2D texture;
            if (_tileDataModel.tileImageDataModel.texture != null || _tileDataModel.m_DefaultSprite != null)
            {
                //Textureが設定済み
                if (_tileDataModel.tileImageDataModel.texture == null)
                    texture = _tileDataModel.m_DefaultSprite.texture;
                else
                    texture = _tileDataModel.tileImageDataModel.texture;
            }
            else
            {
                //Texture未設定の場合は、ここでロードする
                _tileDataModel.tileImageDataModel.texture = _mapManagementService.ReadImage(_tileDataModel.tileImageDataModel.filename);
                texture = _tileDataModel.tileImageDataModel.texture;
            }
            _tileImage.style.backgroundImage = texture;

            // サイズ調整
            var imageSize = texture.width;
            if (texture.width < texture.height)
                imageSize = texture.height;

            // 最大サイズの割合から大きさを設定する
            var ratio = 248.0f / imageSize;
            // 元画像が表示できるサイズならそのまま設定
            if (ratio > 1.0f)
            {
                _tileImage.style.width = texture.width;
                _tileImage.style.height = texture.height;
            }
            else
            {
                _tileImage.style.width = texture.width * ratio;
                _tileImage.style.height = texture.height * ratio;
            }

            _tileImage.style.alignSelf = Align.Center;
            _tileImage.style.marginTop = (250 - _tileImage.style.height.value.value) / 2;
            //------------------------------------------------------------------------------

            var index = 0;
            //乗り物部分の初期値
            foreach (var vehicleType in _tileDataModel.vehicleTypes)
            {
                AddVehicle(vehicleType, 0, index);
                index++;
            }

            //乗り物部分の追加
            _vehicleAddButton.clicked += () =>
            {
                AddVehicle(new VehicleType(0, _vehiclesDataModels[0].id), 1, _tileDataModel.vehicleTypes.Count);
                Save();
            };

            // 要素の表示、非表示設定
            if (_inspectorType == TYPE.NORMAL || _inspectorType == TYPE.IMAGE)
            {
                if (_isFindTile == false)
                {
                    // "タイルデータの設定" を "タイルデータの登録" に変更。
                    this.Q<Label>("setting_name").text = EditorLocalize.LocalizeText("WORD_0733");

                    // タイルの『登録』ボタンの処理。

                    // 未登録タイル
                    _tileSetting.style.display = DisplayStyle.None;

                    // ボタンの処理設定(タイル追加)
                    _registerBtn.clicked += () =>
                    {
                        _onClickRegisterBtn?.Invoke();
                        Save();
                    };
                }
                else
                {
                    // タイルの『削除』ボタンの処理。

                    // ボタンの処理設定(タイル削除)
                    _registerBtn.text = EditorLocalize.LocalizeText("WORD_1521");
                    _registerBtn.clicked += () =>
                    {
                        // 注意ダイアログ表示
                        if (EditorUtility.DisplayDialog(EditorLocalize.LocalizeText("WORD_4011"),
                            EditorLocalize.LocalizeText("WORD_4012"),
                            EditorLocalize.LocalizeText("WORD_3025"), EditorLocalize.LocalizeText("WORD_1530")))
                        {
                            if (_tileDataModel.type == TileDataModel.Type.LargeParts)
                            {
                                var tiles = GetLargePartsTileDataModels(_tileDataModel);
                                foreach (var tile in tiles)
                                {
                                    Delete(tile);
                                }
                            }
                            else
                            {
                                Delete(_tileDataModel);
                            }

                            MapEditor.MapEditor.ReloadTiles();
                        }
                    };

                    if (_tileDataModel.hasAnimation)
                    {
                        _animationToggleLabel.style.display = DisplayStyle.None;
                        _animationToggleElem.style.display = DisplayStyle.None;
                        _animationToggle.style.display = DisplayStyle.None;
                        _animationFrameAmountLabel.style.display = DisplayStyle.None;
                        _animationFrameAmountText.style.display = DisplayStyle.None;
                        _animationToggle.SetEnabled(false);
                    }
                    else
                        _animationFoldout.style.display = DisplayStyle.None;
                }
            }
            else
            {
                _registerBtn.style.display = DisplayStyle.None;
            }
        }

        // 表示規則
        // タイルタイプによる画像の補正、アニメーション表示設定
        private void ChangeDisplayRules(TileDataModel.Type tileType) {
            switch (tileType)
            {
                case TileDataModel.Type.AutoTileA:
                case TileDataModel.Type.AutoTileB:
                    for (var i = 0; i < _imageAdjustTypeToggleList.Count; i++)
                    {
                        _imageAdjustTypeToggleList[i].SetEnabled(false);
                        _imageAdjustTypeToggleList[i].value = false;
                    }

                    _imageAdjustTypeToggleList[2].value = true;
                    _animationSpeedText.SetEnabled(true);
                    _animationFrameAmountText.SetEnabled(true);
                    _animationToggle.SetEnabled(true);
                    break;

                case TileDataModel.Type.AutoTileC:
                    for (var i = 0; i < _imageAdjustTypeToggleList.Count; i++)
                    {
                        _imageAdjustTypeToggleList[i].SetEnabled(false);
                        _imageAdjustTypeToggleList[i].value = false;
                    }

                    _imageAdjustTypeToggleList[2].value = true;
                    _animationSpeedText.SetEnabled(false);
                    _animationFrameAmountText.SetEnabled(false);
                    _animationToggle.value = false;
                    _animationToggle.SetEnabled(false);
                    break;

                case TileDataModel.Type.Effect:
                    for (var i = 0; i < _imageAdjustTypeToggleList.Count; i++)
                    {
                        _imageAdjustTypeToggleList[i].SetEnabled(false);
                        _imageAdjustTypeToggleList[i].value = false;
                    }

                    _imageAdjustTypeToggleList[2].value = true;
                    _animationSpeedText.SetEnabled(true);
                    _animationFrameAmountText.SetEnabled(true);
                    _animationToggle.value = true;
                    _animationToggle.SetEnabled(false);
                    break;

                case TileDataModel.Type.NormalTile:
                    for (var i = 0; i < _imageAdjustTypeToggleList.Count; i++)
                    {
                        _imageAdjustTypeToggleList[i].SetEnabled(true);
                        _imageAdjustTypeToggleList[i].value = false;
                    }

                    _imageAdjustTypeToggleList[1].value = true;
                    _imageAdjustTypeToggleList[2].SetEnabled(false);
                    _animationSpeedText.SetEnabled(true);
                    _animationFrameAmountText.SetEnabled(true);
                    _animationToggle.value = false;
                    _animationToggle.SetEnabled(true);
                    break;

                case TileDataModel.Type.LargeParts:
                    for (var i = 0; i < _imageAdjustTypeToggleList.Count; i++)
                    {
                        _imageAdjustTypeToggleList[i].SetEnabled(false);
                        _imageAdjustTypeToggleList[i].value = false;
                    }

                    _imageAdjustTypeToggleList[2].value = true;
                    _animationSpeedText.SetEnabled(true);
                    _animationFrameAmountText.SetEnabled(true);
                    _animationToggle.value = false;
                    _animationToggle.SetEnabled(true);
                    break;
            }
        }

        //乗り物の項目の追加
        //乗り物タイプ、新規追加は「1」初期値は「0」
        private void AddVehicle(VehicleType type, int add, int index) {
            var vehicleVisualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(vehicleTrafficUxml);
            var vehicleContainer = vehicleVisualTree.CloneTree();
            EditorLocalize.LocalizeElements(vehicleContainer);
            vehicleContainer.style.flexGrow = 1;
            var vehicleIndex = index;

            _vehicleFoldDown = vehicleContainer.Query<VisualElement>("vehicle_fold_down");
            Toggle vehicleIsPassToggle = vehicleContainer.Query<Toggle>("is_pass_toggle");
            Toggle vehicleIsPassUnderToggle = vehicleContainer.Query<Toggle>("is_pass_under_toggle");
            _vehicleDelete = vehicleContainer.Query<Button>("vehicle_delete");

            //乗り物選択
            var VehicleDropdownPopupField =
                new PopupFieldBase<string>(VehicleList(), VehicleIdToIndex(type.vehicleId));
            _vehicleFoldDown.Clear();
            _vehicleFoldDown.Add(VehicleDropdownPopupField);
            VehicleDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                type.vehicleId = _vehiclesDataModels[VehicleDropdownPopupField.index].id;
                Save();
            });

            // 通行設定
            switch (type.vehiclePassType)
            {
                case TileDataModel.PassType.CanPassNormally:
                    vehicleIsPassToggle.value = true;
                    break;
                case TileDataModel.PassType.CanPassUnder:
                    vehicleIsPassUnderToggle.value = true;
                    break;
                case TileDataModel.PassType.CannotPass:
                    break;
            }

            vehicleIsPassToggle.RegisterValueChangedCallback(evt =>
            {
                OnPassToggleChanged(
                    vehicleIsPassToggle,
                    vehicleIsPassUnderToggle,
                    ref type.vehiclePassType,
                    TileDataModel.PassType.CanPassNormally,
                    false);

                _tileDataModel.vehicleTypes[vehicleIndex] = type;

                Save();
            });

            vehicleIsPassUnderToggle.RegisterValueChangedCallback(evt =>
            {
                OnPassToggleChanged(
                    vehicleIsPassUnderToggle,
                    vehicleIsPassToggle,
                    ref type.vehiclePassType,
                    TileDataModel.PassType.CanPassUnder,
                    false);

                _tileDataModel.vehicleTypes[vehicleIndex] = type;

                Save();
            });

            //削除
            _vehicleDelete.clicked += () =>
            {
                _tileDataModel.vehicleTypes.Remove(type);
                Save();
                _vehicleAria.Clear();
                var index = 0;
                //乗り物部分の初期値
                foreach (var vehicleType in _tileDataModel.vehicleTypes)
                {
                    AddVehicle(vehicleType, 0, index);
                    index++;
                }

            };

            //新規追加の場合はデータにも追加する
            if (add == 1) _tileDataModel.vehicleTypes.Add(type);

            _vehicleAria.Add(vehicleContainer);
        }

        //乗り物の選択
        private List<string> VehicleList() {
            var returnList = new List<string>();
            foreach (var vehiclesDataModel in _vehiclesDataModels) returnList.Add(vehiclesDataModel.name);

            return returnList;
        }

        //乗り物IDをIndexに
        private int VehicleIdToIndex(string id) {
            var returnindex = 0;
            for (var i = 0; i < _vehiclesDataModels.Count; i++)
                if (_vehiclesDataModels[i].id == id)
                {
                    returnindex = i;
                    break;
                }

            return returnindex;
        }

        /// <summary>
        /// 『通れる』トルグまたは『下を潜って通れる』トルグの値変更時。
        /// </summary>
        /// <param name="changedToggle">値が変更されたトルグ。</param>
        /// <param name="otherToggle">値が変更されたトルグではないトルグ。</param>
        /// <param name="pathTypeToSet">設定する通行タイプ変数。</param>
        /// <param name="pathTypeWhenTrue">変更された値がtrueだった場合の通行タイプ。</param>
        /// <param name="includeChangePassDirection">通行可能な向きの変更を含む。</param>
        private void OnPassToggleChanged(
            Toggle changedToggle,
            Toggle otherToggle, 
            ref TileDataModel.PassType pathTypeToSet,
            TileDataModel.PassType pathTypeWhenTrue,
            bool includeChangePassDirection)
        {
            if (changedToggle.value)
            {
                pathTypeToSet = pathTypeWhenTrue;
                otherToggle.value = false;

                if (includeChangePassDirection)
                {
                    // 全向きオフなら全向きオンに。
                    List<Toggle> toggles =
                        new(){ _isPassTopToggle, _isPassBottomToggle, _isPassLeftToggle, _isPassRightToggle };
                    if (toggles.All(toggle => !toggle.value))
                    {
                        toggles.ForEach(toggle => toggle.value = true);
                    }
                }
            }
            else if (!otherToggle.value)
            {
                // 『通れる』と『下を潜って通れる』のどちらもfalseなら『通れない』を設定。
                pathTypeToSet = TileDataModel.PassType.CannotPass;
            }
        }

        private static int GetIndexOfDictionary(Dictionary<string, string> dictionary, string targetKey) {
            var index = dictionary.Keys.ToList().IndexOf(targetKey);
            return index > -1 ? index : 0;
        }

        override protected void SaveContents() {
            base.SaveContents();
            if (_isFindTile == false) return;
            MapEditor.MapEditor.SaveInspectorTile(_tileDataModel);
        }

        private List<TileDataModel> GetLargePartsTileDataModels(TileDataModel tileDataModel) {
            var parentId = tileDataModel.largePartsDataModel.parentId;
            var tiles = _mapManagementService.LoadTileTable();
            var largePartsList = new List<TileDataModel>();
            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i].type == TileDataModel.Type.LargeParts &&
                    tiles[i].largePartsDataModel.parentId == parentId)
                    largePartsList.Add(tiles[i].TileDataModel);
            }
            return largePartsList;
        }

        private void Delete(TileDataModel tileDataModel) {
            var path = mapManagementService.GetAssetPath(tileDataModel.tileDataModelInfo, true);

            string id = tileDataModel.id;
            // ファイルとフォルダを削除
            if (File.Exists(path + id + ".asset"))
                File.Delete(path + id + ".asset");
            if (File.Exists(path + id + ".asset.meta"))
                File.Delete(path + id + ".asset.meta");
            if (File.Exists(path + id + ".meta"))
                File.Delete(path + id + ".meta");
            if (Directory.Exists(path + id))
                Directory.Delete(path + id, true);
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh2();
            _mapManagementService.RemoveTile(id);
        }
    }
}