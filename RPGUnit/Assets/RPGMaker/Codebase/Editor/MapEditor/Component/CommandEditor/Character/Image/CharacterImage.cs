using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event.EventDataModel;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Character.Image
{
    /// <summary>
    ///     [キャラ画像設定]コマンドのコマンド設定枠の表示物
    /// </summary>
    public class CharacterImage : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_character_image.uxml";

        private GenericPopupFieldBase<TargetCharacterChoice> _targetCharacterPopupField;
        private EventCommand _targetCommand;

        private UnityEngine.UIElements.Image characterImagePreview;
        private List<AssetManageDataModel> _assetManageData;
        private List<AssetManageDataModel> _assetManageObjectData;
        private bool _isCharacter = true;
        private int _num = 0;


        public CharacterImage(
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
            _targetCommand = EventDataModels[EventIndex].eventCommands[EventCommandIndex];
            RootElement.Add(commandFromUxml);

            if (_targetCommand.parameters.Count == 0)
            {
                // キャラクター名
                _targetCommand.parameters.Add("-2");
                // 画像名
                _targetCommand.parameters.Add("");
                // キャラクター画像の変更(0:変更しない 1:変更する)
                _targetCommand.parameters.Add("0");
                // 透明度
                _targetCommand.parameters.Add("255");
                // 透明度の変更(0:変更しない 1:変更する)
                _targetCommand.parameters.Add("0");
                // 合成方法の選択肢のインデックス
                _targetCommand.parameters.Add("0");
                // 合成方法の指定(0:指定しない 1:指定する)
                _targetCommand.parameters.Add("0");
                // 向き
                _targetCommand.parameters.Add("0");
                Save(EventDataModels[EventIndex]);
            }

            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();

            //キャラクター
            {
                int targetCharacterParameterIndex = 0;
                AddOrHideProvisionalMapAndAddTargetCharacterPopupField(
                    targetCharacterParameterIndex,
                    provisionalMapPopupField =>
                    {
                        _targetCharacterPopupField = AddTargetCharacterPopupField(
                            QRoot("character"),
                            targetCharacterParameterIndex,
                            forceMapId: provisionalMapPopupField?.value.MapDataModel?.id);
                    });
            }

            //選択肢のindex初期化
            _num = 0;

            //キャラクター画像の設定
            var assetDataModels = DatabaseManagementService.LoadAssetManage();

            //キャラクターとオブジェクトの配列を作成し、初期値も保持
            var _orderData = AssetManageRepository.OrderManager.Load();
            _assetManageData = new List<AssetManageDataModel>();
            _assetManageObjectData = new List<AssetManageDataModel>();

            _isCharacter = true;
            for (var i = 0; i < _orderData.orderDataList.Length; i++)
            {
                if (_orderData.orderDataList[i].idList == null)
                    continue;
                if (_orderData.orderDataList[i].assetTypeId == (int) AssetCategoryEnum.MOVE_CHARACTER)
                    for (var i2 = 0; i2 < _orderData.orderDataList[i].idList.Count; i2++)
                    {
                        AssetManageDataModel data = null;
                        for (int j = 0; j < assetDataModels.Count; j++)
                        {
                            if (assetDataModels[j].id == _orderData.orderDataList[i].idList[i2])
                            {
                                data = assetDataModels[j];
                                break;
                            }
                        }

                        if (data != null)
                            _assetManageData.Add(data);

                        if (data != null && _targetCommand.parameters[1] == data.id)
                        {
                            _isCharacter = true;
                            _num = _assetManageData.Count;
                        }
                    }
                if (_orderData.orderDataList[i].assetTypeId == (int) AssetCategoryEnum.OBJECT)
                    for (var i2 = 0; i2 < _orderData.orderDataList[i].idList.Count; i2++)
                    {
                        AssetManageDataModel data = null;
                        for (int j = 0; j < assetDataModels.Count; j++)
                        {
                            if (assetDataModels[j].id == _orderData.orderDataList[i].idList[i2])
                            {
                                data = assetDataModels[j];
                                break;
                            }
                        }

                        if (data != null)
                            _assetManageObjectData.Add(data);

                        if (data != null && _targetCommand.parameters[1] == data.id)
                        {
                            _isCharacter = false;
                            _num = _assetManageObjectData.Count;
                        }
                    }
            }

            //初期のセレクトボックスを生成
            CreatePopupField();

            //画像種別選択
            characterImagePreview = RootElement.Query<UnityEngine.UIElements.Image>("character_image_preview");

            var characterType = EditorLocalize.LocalizeTexts(new List<string> { "WORD_0350", "WORD_1326" });
            VisualElement characterTypeSelectort = RootElement.Query<VisualElement>("character_image_type");
            var characterTypePopupField = new PopupFieldBase<string>(characterType, _isCharacter ? 0 : 1);
            characterTypeSelectort.Clear();
            characterTypeSelectort.Add(characterTypePopupField);
            characterTypePopupField.RegisterValueChangedCallback(evt =>
            {
                if (characterTypePopupField.index == 0 && !_isCharacter)
                {
                    _isCharacter = true;
                }
                else if (characterTypePopupField.index == 1 && _isCharacter)
                {
                    _isCharacter = false;
                }
                else
                {
                    return;
                }

                //リストが変わったため、num を初期化
                _num = 0;
                _targetCommand.parameters[1] = "";
                characterImagePreview.image = LoadTexture(_targetCommand.parameters[1]);

                //向きのプルダウンメニューを更新（オブジェクトにはダメージ絵が無いため）
                List<string> directionTextDropdownChoices;
                directionTextDropdownChoices = EditorLocalize.LocalizeTexts(new List<string> {"WORD_0815", "WORD_0813", "WORD_0814", "WORD_0812"});
                if (_isCharacter) directionTextDropdownChoices.Add(EditorLocalize.LocalizeText("WORD_0509"));
                
                if (!_isCharacter && _targetCommand.parameters[7] == "5")
                    _targetCommand.parameters[7] = "1";

                int directionIndex = 0;
                if (int.Parse(_targetCommand.parameters[7]) >= 2)
                {
                    directionIndex = int.Parse(_targetCommand.parameters[7]) - 1;
                }

                VisualElement direction = RootElement.Query<VisualElement>("direction");
                direction.Clear();

                PopupFieldBase<string> directionPopupField = new PopupFieldBase<string>(directionTextDropdownChoices, directionIndex);
                direction.Add(directionPopupField);

                //向きのプルダウン変更時
                directionPopupField.RegisterValueChangedCallback(evt =>
                {
                    _targetCommand.parameters[7] = (directionPopupField.index + 1).ToString();
                    Save(EventDataModels[EventIndex]);
                });

                //リスト再生成
                CreatePopupField();

                //データを保存
                Save(EventDataModels[EventIndex]);
            });

            //画像選択のプロパティ設定
            characterImagePreview.scaleMode = ScaleMode.ScaleToFit;
            characterImagePreview.image = LoadTexture(_targetCommand.parameters[1]);

            if (_num == -1) _num = 0;

            // 透明度の入力欄の設定
            IntegerField opacity_num = RootElement.Query<IntegerField>("opacity_num");
            opacity_num.value = int.Parse(_targetCommand.parameters[3]);
            opacity_num.RegisterCallback<FocusOutEvent>(evt =>
            {
                _targetCommand.parameters[3] = opacity_num.value.ToString();
                Save(EventDataModels[EventIndex]);
            });
            opacity_num.RegisterValueChangedCallback(evt =>
            {
                opacity_num.value = Math.Min(255, opacity_num.value);
                opacity_num.value = Math.Max(0, opacity_num.value);
            });

            // 透明度のトグルの設定
            Toggle opacity_toggle = RootElement.Query<Toggle>("opacity_toggle");
            opacity_toggle.value = _targetCommand.parameters[4] == "1";
            opacity_toggle.RegisterValueChangedCallback(evt =>
            {
                opacity_num.SetEnabled(opacity_toggle.value);
                _targetCommand.parameters[4] = Convert.ToInt32(opacity_toggle.value).ToString();
                Save(EventDataModels[EventIndex]);
            });
            opacity_num.SetEnabled(opacity_toggle.value);

            // 合成方法の設定
            VisualElement synthetic = RootElement.Query<VisualElement>("synthetic");
            var syntheticTextDropdownChoices = EditorLocalize.LocalizeTexts(new List<string> {"WORD_2594", "WORD_0976", "WORD_0977", "WORD_0978"});
            var syntheticNum = 0;
            if (_targetCommand.parameters[5] != null)
                syntheticNum = int.Parse(_targetCommand.parameters[5]);
            if (syntheticNum == -1)
                syntheticNum = 0;
            var syntheticPopupField = new PopupFieldBase<string>(syntheticTextDropdownChoices, syntheticNum);
            synthetic.Clear();
            synthetic.Add(syntheticPopupField);
            syntheticPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[5] = syntheticPopupField.index.ToString();
                Save(EventDataModels[EventIndex]);
            });
            Toggle synthetic_toggle = RootElement.Query<Toggle>("synthetic_toggle");
            if (_targetCommand.parameters[6] == "1")
            {
                synthetic_toggle.value = true;
                synthetic.SetEnabled(true);
            }
            else
            {
                synthetic.SetEnabled(false);
            }

            synthetic_toggle.RegisterValueChangedCallback(evt =>
            {
                var num = 0;
                if (synthetic_toggle.value)
                {
                    synthetic.SetEnabled(true);
                    num = 1;
                }
                else
                {
                    synthetic.SetEnabled(false);
                }

                _targetCommand.parameters[6] = num.ToString();
                Save(EventDataModels[EventIndex]);
            });

            //向き
            if (_targetCommand.parameters.Count < 8)
            {
                _targetCommand.parameters.Add("0");
            }
            if (!_isCharacter && _targetCommand.parameters[7] == "5")
                _targetCommand.parameters[7] = "1";

            int directionIndex = 0;
            if (int.Parse(_targetCommand.parameters[7]) >= 2)
            {
                directionIndex = int.Parse(_targetCommand.parameters[7]) - 1;
            }

            //向きのトグル
            Toggle direction_toggle = RootElement.Query<Toggle>("direction_toggle");

            //向きのプルダウンメニュー
            List<string> directionTextDropdownChoices;
            directionTextDropdownChoices = EditorLocalize.LocalizeTexts(new List<string> {"WORD_0815", "WORD_0813", "WORD_0814", "WORD_0812"});
            if (_isCharacter) directionTextDropdownChoices.Add(EditorLocalize.LocalizeText("WORD_0509"));

            VisualElement direction = RootElement.Query<VisualElement>("direction");
            PopupFieldBase<string> directionPopupField = new PopupFieldBase<string>(directionTextDropdownChoices, directionIndex);
            direction.Add(directionPopupField);

            //向きのトグル変更時
            direction_toggle.RegisterValueChangedCallback(o =>
            {
                if (direction_toggle.value)
                {
                    directionPopupField.SetEnabled(true);
                    _targetCommand.parameters[7] = (directionPopupField.index + 1).ToString();
                }
                else
                {
                    directionPopupField.SetEnabled(false);
                    _targetCommand.parameters[7] = "0";
                }
                Save(EventDataModels[EventIndex]);
            });

            //向きのプルダウン変更時
            directionPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[7] = (directionPopupField.index + 1).ToString();
                Save(EventDataModels[EventIndex]);
            });

            //初期設定
            if (_targetCommand.parameters[7] != "0")
            {
                direction_toggle.value = true;
                directionPopupField.SetEnabled(true);
                _targetCommand.parameters[7] = (directionPopupField.index + 1).ToString();
            }
            else
            {
                direction_toggle.value = false;
                directionPopupField.SetEnabled(false);
            }
        }

        private void CreatePopupField() {
            //セレクトボックスの領域を初期化
            VisualElement characterImageSelector = RootElement.Query<VisualElement>("character_image_selector");
            characterImageSelector.Clear();

            //なしを追加
            var characterImageSelectorChoices = new List<string>() { EditorLocalize.LocalizeText("WORD_0113") };
            var characterIdSelectorChoices = new List<string>() { "-1" };

            //キャラクター
            if (_isCharacter)
            {
                characterImageSelectorChoices.Clear();
                characterImageSelectorChoices.Add(EditorLocalize.LocalizeText("WORD_0113"));
                characterIdSelectorChoices.Add("-1");
                foreach (var dataModel in _assetManageData)
                {
                    if (dataModel == null) continue;
                    characterImageSelectorChoices.Add(dataModel.name);
                    characterIdSelectorChoices.Add(dataModel.id);
                }
            }
            //オブジェクト
            else
            {
                characterImageSelectorChoices.Clear();
                characterImageSelectorChoices.Add(EditorLocalize.LocalizeText("WORD_0113"));
                characterIdSelectorChoices.Add("-1");
                foreach (var dataModel in _assetManageObjectData)
                {
                    if (dataModel == null) continue;
                    characterImageSelectorChoices.Add(dataModel.name);
                    characterIdSelectorChoices.Add(dataModel.id);
                }
            }

            //セレクトボックス作成
            var characterImagePopupField = new PopupFieldBase<string>(characterImageSelectorChoices, _num);
            characterImageSelector.Add(characterImagePopupField);
            characterImagePopupField.RegisterValueChangedCallback(evt =>
            {
                _num = characterImagePopupField.index;
                AssetManageDataModel targetAsset = null;

                if (_num != 0)
                {
                    if (_isCharacter)
                    {
                        targetAsset = _assetManageData[_num - 1];
                    }
                    else
                    {
                        targetAsset = _assetManageObjectData[_num - 1];
                    }
                    _targetCommand.parameters[1] = targetAsset?.id;
                    characterImagePreview.image = LoadTexture(_targetCommand.parameters[1]);
                }
                else
                {
                    _targetCommand.parameters[1] = "";
                    characterImagePreview.image = null;
                }

                Save(EventDataModels[EventIndex]);

                //再描画
                CreatePopupField();
            });

            // 画像選択のトグルの設定
            Toggle image_toggle = RootElement.Query<Toggle>("image_toggle");
            image_toggle.value = _targetCommand.parameters[2] == "1";
            image_toggle.RegisterValueChangedCallback(evt =>
            {
                characterImagePopupField.SetEnabled(image_toggle.value);
                _targetCommand.parameters[2] = Convert.ToInt32(image_toggle.value).ToString();
                Save(EventDataModels[EventIndex]);
            });
            characterImagePopupField.SetEnabled(image_toggle.value);
        }

        private Texture2D LoadTexture(string fileName) {
            var imageCharacter = ImageManager.LoadSvCharacter(fileName);

            return imageCharacter;
        }
    }
}