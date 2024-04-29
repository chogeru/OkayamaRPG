using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using RPGMaker.Codebase.Editor.DatabaseEditor.View.Preview;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Message
{
    public class TextWindow : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_text_window.uxml";

        public TextWindow(
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

            var characterActorDataModels =
                DatabaseManagementService.LoadCharacterActor();


            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Count < 10)
            {
                var uiSetting = DatabaseManagementService.LoadUiSettingDataModel();

                // 共通設定を反映
                var pic = "0";
                var face = "0";
                if (uiSetting.talkMenu.characterMenu.characterEnabled == 0)
                    face = "1";
                else if (uiSetting.talkMenu.characterMenu.characterEnabled == 1)
                    pic = "1";

                // キャラクターの表示(0:非表示 1:表示)
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters
                    .Add(uiSetting.talkMenu.characterMenu.nameEnabled.ToString());
                // 顔の表示(0:非表示 1:表示)
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add(face);
                // ディスプレイの表示(0:非表示 1:表示)
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add(pic);
                // 背景(0:ウィンドウ 1:暗くする 2:透明)
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("0");
                // ポジション(-1:初期設定 0:上 1:中 2:下)
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("-1");
                // テキスト文字
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("");
                // キャラクター名
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("");
                // 顔グラ
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("");
                // 立ち絵
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("");
                // アクターID
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("");

                var data = EventDataModels[EventIndex].eventCommands.ToList();

                // 表示するテキストがなければ追加
                if (EventDataModels[EventIndex].eventCommands[EventCommandIndex + 1].code !=
                    (int) EventEnum.EVENT_CODE_MESSAGE_TEXT_ONE_LINE)
                {
                    var eventDatas = new EventDataModel.EventCommand(0, new List<string>(),
                        new List<EventDataModel.EventCommandMoveRoute>());
                    eventDatas.code = (int) EventEnum.EVENT_CODE_MESSAGE_TEXT_ONE_LINE;
                    eventDatas.parameters = new List<string>
                    {
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[5],
                        EventIndex.ToString()
                    };
                    data.Insert(EventCommandIndex + 1, eventDatas);
                }

                EventDataModels[EventIndex].eventCommands = data;

                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            // 顔画像選択
            //------------------------------------------------------------------------------------------------------------------------------
            // プレビュー画像
            Image faceImage = RootElement.Query<Image>("face_image");
            faceImage.scaleMode = ScaleMode.ScaleToFit;
            faceImage.image = AssetDatabase.LoadAssetAtPath<Texture2D>(
                PathManager.IMAGE_FACE +
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[7] + ".png"
            );

            // 画像名
            Label faceImageName = RootElement.Query<Label>("face_image_name");
            faceImageName.text = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[7];

            // 画像変更ボタン
            Button faceChangeBtn = RootElement.Query<Button>("face_image_change_btn");
            faceChangeBtn.clicked += () =>
            {
                var selectModalWindow = new ImageSelectModalWindow(PathManager.IMAGE_FACE, true);
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Image"), data =>
                {
                    var imageName = (string) data;
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[7] = imageName;
                    var path = PathManager.IMAGE_FACE + imageName + ".png";
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    faceImage.image = tex;
                    faceImageName.text = imageName;
                    Save(EventDataModels[EventIndex]);
                }, EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[7]);
            };

            // 画像インポートボタン
            Button faceImportBtn = RootElement.Query<Button>("face_image_import_btn");
            faceImportBtn.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("png", PathManager.IMAGE_FACE);
                if (!string.IsNullOrEmpty(path))
                {
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    faceImage.image = tex;
                    path = Path.GetFileNameWithoutExtension(path);
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[7] = path;
                    faceImageName.text = path;
                    Save(EventDataModels[EventIndex]);
                }
            };

            // ピクチャ画像選択
            //------------------------------------------------------------------------------------------------------------------------------
            // プレビュー画像
            Image pictureImage = RootElement.Query<Image>("picture_image");
            pictureImage.scaleMode = ScaleMode.ScaleToFit;
            pictureImage.image = AssetDatabase.LoadAssetAtPath<Texture2D>(
                PathManager.IMAGE_ADV +
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[8] + ".png"
            );

            // 画像名
            Label pictureImageName = RootElement.Query<Label>("picture_image_name");
            pictureImageName.text = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[8];

            // 画像変更ボタン
            Button pictureChangeBtn = RootElement.Query<Button>("picture_image_change_btn");
            pictureChangeBtn.clicked += () =>
            {
                var selectModalWindow = new ImageSelectModalWindow(PathManager.IMAGE_ADV, true);
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Image"), data =>
                {
                    var imageName = (string) data;
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[8] = imageName;
                    var path = PathManager.IMAGE_ADV + imageName + ".png";
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    pictureImage.image = tex;
                    pictureImageName.text = imageName;
                    Save(EventDataModels[EventIndex]);
                }, EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[8]);
            };

            // 画像インポートボタン
            Button pictureImportBtn = RootElement.Query<Button>("picture_image_import_btn");
            pictureImportBtn.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("png", PathManager.IMAGE_ADV);
                if (!string.IsNullOrEmpty(path))
                {
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    pictureImage.image = tex;
                    path = Path.GetFileNameWithoutExtension(path);
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[8] = path;
                    pictureImageName.text = path;
                    Save(EventDataModels[EventIndex]);
                }
            };

            ImTextField nameText = RootElement.Query<ImTextField>("nameText");
            nameText.SetEnabled(false);

            var actorDropdownChoices = new List<CharacterActorDataModel>
                {CharacterActorDataModel.CreateDefault("-1", "", 0)};
            var actorNameDropdownChoices = EditorLocalize.LocalizeTexts(new List<string> {"WORD_0113"});
            for (var i = 0; i < characterActorDataModels.Count; i++)
            {
                actorDropdownChoices.Add(characterActorDataModels[i]);
                actorNameDropdownChoices.Add(characterActorDataModels[i].basic.name);
            }

            VisualElement character = RootElement.Query<VisualElement>("character");
            var nameIndex = 0;
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[9] != "")
            {
                for (int i = 0; i < characterActorDataModels.Count; i++)
                {
                    if (characterActorDataModels[i].uuId == EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[9])
                    {
                        nameIndex = i + 1;
                        break;
                    }
                }
            }
            if (nameIndex == -1)
            {
                nameIndex = 0;
            }
            if (nameIndex == 0)
            {
                nameText.SetEnabled(true);
                faceChangeBtn.SetEnabled(true);
                pictureChangeBtn.SetEnabled(true);
            }
            else
            {
                nameText.SetEnabled(false);
                faceChangeBtn.SetEnabled(false);
                pictureChangeBtn.SetEnabled(false);

                //キャラクターが存在した場合、名前、顔画像、ピクチャ画像を更新
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[6] =
                    characterActorDataModels[nameIndex - 1].basic.name;
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[7] =
                    characterActorDataModels[nameIndex - 1].image.face;
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[8] =
                    characterActorDataModels[nameIndex - 1].image.adv;
            }

            var characterPopupField = new PopupFieldBase<string>(actorNameDropdownChoices, nameIndex);
            nameText.value = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[6];
            nameText.RegisterCallback<FocusOutEvent>(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[6] = nameText.value;
                Save(EventDataModels[EventIndex]);
            });
            character.Clear();
            character.Add(characterPopupField);
            characterPopupField.RegisterValueChangedCallback(evt =>
            {
                var num = characterPopupField.index;

                // 名前・画像設定
                if (num == 0)
                {
                    nameText.SetEnabled(true);
                    faceChangeBtn.SetEnabled(true);
                    pictureChangeBtn.SetEnabled(true);

                    //項目が無しなので名前画像を空白に設定しなおす
                    Texture2D tex = null;
                    faceImage.image = tex;
                    faceImageName.text = "";
                    pictureImage.image = tex;
                    pictureImageName.text = "";

                    nameText.value = "";

                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[6] = nameText.value;
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[7] = "";
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[8] = "";
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[9] = "";
                }
                else
                {
                    nameText.SetEnabled(false);
                    faceChangeBtn.SetEnabled(false);
                    pictureChangeBtn.SetEnabled(false);

                    var path = PathManager.IMAGE_FACE + actorDropdownChoices[num].image.face + ".png";
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    faceImage.image = tex;
                    faceImageName.text = actorDropdownChoices[num].image.face;

                    path = PathManager.IMAGE_ADV + actorDropdownChoices[num].image.adv + ".png";
                    tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    pictureImage.image = tex;
                    pictureImageName.text = actorDropdownChoices[num].image.adv;

                    nameText.value = characterPopupField.value;

                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[6] =
                        characterPopupField.value;
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[7] =
                        actorDropdownChoices[num].image.face;
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[8] =
                        actorDropdownChoices[num].image.adv;
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[9] =
                        actorDropdownChoices[num].uuId;
                }

                Save(EventDataModels[EventIndex]);
            });

            VisualElement backGround = RootElement.Query<VisualElement>("backGround");
            var backGroundTextDropdownChoices =
                EditorLocalize.LocalizeTexts(new List<string> {"WORD_1196", "WORD_1197", "WORD_1198"});
            var backGroundIndex = -1;
            try
            {
                if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] != "")
                    backGroundIndex = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3]);
            }
            catch (Exception)
            {
                backGroundIndex = 0;
            }
            if (backGroundIndex == -1) backGroundIndex = 0;
            var backGroundPopupField = new PopupFieldBase<string>(backGroundTextDropdownChoices, backGroundIndex);
            backGround.Clear();
            backGround.Add(backGroundPopupField);
            backGroundPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] =
                    backGroundTextDropdownChoices.IndexOf(backGroundPopupField.value).ToString();
                Save(EventDataModels[EventIndex]);
            });

            VisualElement windowPosition = RootElement
                .Query<VisualElement>("windowPosition");
            var windowPositionTextDropdownChoices =
                EditorLocalize.LocalizeTexts(new List<string> {"WORD_0062", "WORD_0297", "WORD_0298", "WORD_0299"});
            var popupNum = 0;
            int parse;
            if (int.TryParse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4], out parse))
                popupNum = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4]) + 1;
            var windowPositionPopupField = new PopupFieldBase<string>(windowPositionTextDropdownChoices, popupNum);
            windowPosition.Clear();
            windowPosition.Add(windowPositionPopupField);
            windowPositionPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] =
                    (windowPositionTextDropdownChoices.IndexOf(windowPositionPopupField.value) - 1).ToString();
                Save(EventDataModels[EventIndex]);
            });

            RadioButton character_toggle_yes = RootElement.Query<RadioButton>("radioButton-eventCommand-display1");
            RadioButton character_toggle_no = RootElement.Query<RadioButton>("radioButton-eventCommand-display2");
            RadioButton faceImage_toggle_yes = RootElement.Query<RadioButton>("radioButton-eventCommand-display3");
            RadioButton faceImage_toggle_no = RootElement.Query<RadioButton>("radioButton-eventCommand-display4");
            RadioButton display_toggle_yes = RootElement.Query<RadioButton>("radioButton-eventCommand-display5");
            RadioButton display_toggle_no = RootElement.Query<RadioButton>("radioButton-eventCommand-display6");

            var characterDefaultSelect = 0;
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] == "1")
                characterDefaultSelect = 0;
            else
                characterDefaultSelect = 1;
            
            var characterNameActions = new List<Action>
            {
                () =>
                {
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "1";
                    Save(EventDataModels[EventIndex]);
                },
                () =>
                {
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "0";
                    Save(EventDataModels[EventIndex]);
                }
            };

            new CommonToggleSelector().SetRadioSelector(new List<RadioButton>(){character_toggle_yes, character_toggle_no},
                characterDefaultSelect,
                characterNameActions);

            var faceDefaultSelect = 0;

            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] == "1")
            {
                faceDefaultSelect = 0;
                //顔が選択状態の場合、ピクチャは選択不可とする
                display_toggle_yes.SetEnabled(false);
            }
            else
            {
                faceDefaultSelect = 1;
                //顔が非選択状態の場合、ピクチャは選択可とする
                display_toggle_yes.SetEnabled(true);
            }
            
            
            var faceActions = new List<Action>
            {
                () =>
                {
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = "1";
                    Save(EventDataModels[EventIndex]);
                    display_toggle_yes.SetEnabled(false);
                },
                () =>
                {
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = "0";
                    Save(EventDataModels[EventIndex]);
                    display_toggle_yes.SetEnabled(true);
                }
            };

            new CommonToggleSelector().SetRadioSelector(new List<RadioButton>(){faceImage_toggle_yes, faceImage_toggle_no},
                faceDefaultSelect,
                faceActions);
            
            var pictureDefaultSelect = 0;
            
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] == "1")
            {
                pictureDefaultSelect = 0;
                //ピクチャが選択状態の場合、顔は選択不可とする
                faceImage_toggle_yes.SetEnabled(false);
            }
            else
            {
                pictureDefaultSelect = 1;
                //ピクチャが非選択状態の場合、顔は選択可とする
                faceImage_toggle_yes.SetEnabled(true);
            }
            
            var pictureActions = new List<Action>
            {
                () =>
                {
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] = "1";
                    Save(EventDataModels[EventIndex]);
                    faceImage_toggle_yes.SetEnabled(false);
                },
                () =>
                {
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] = "0";
                    Save(EventDataModels[EventIndex]);
                    faceImage_toggle_yes.SetEnabled(true);
                }
            };

            new CommonToggleSelector().SetRadioSelector(new List<RadioButton>(){display_toggle_yes, display_toggle_no},
                pictureDefaultSelect,
                pictureActions);
            


            Button buttonPreview = RootElement.Query<Button>("ButtonPreview");

            buttonPreview.clickable.clicked += () =>
            {
                var _databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;

                var uiSettingDataModel = _databaseManagementService.LoadUiSettingDataModel();
                var _sceneWindow =
                    WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.DatabaseSceneWindow) as
                        SceneWindow;
                var num = actorNameDropdownChoices.IndexOf(characterPopupField.value);
                var image = CharacterActorDataModel.Image.CreateDefault();
                if (num == 0)
                {
                    image.face = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[7];
                    image.adv = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[8];
                }
                else
                {
                    image = characterActorDataModels[num - 1].image;
                }

                var toggles = new List<int>();
                toggles.Add(character_toggle_yes.value ? 1 : 0);
                toggles.Add(faceImage_toggle_yes.value ? 1 : 0);
                toggles.Add(display_toggle_yes.value ? 1 : 0);
                _sceneWindow.Create(SceneWindow.PreviewId.TalkWindow);
                _sceneWindow.GetTalkWindowPreview().SetWindowType(TalkWindowPreview.TalkWindowType.Event);
                _sceneWindow.GetTalkWindowPreview().SetUiData(uiSettingDataModel);
                //メッセージの本文はこのイベントコード次に設定されるため
                int background = 0;
                int position = 0;
                try
                {
                    background = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3]);
                }
                catch (Exception) { }
                try
                {
                    position = int.Parse(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4]);
                }
                catch (Exception) { }
                var message = EventDataModels[EventIndex].eventCommands[EventCommandIndex + 1].parameters[0];
                _sceneWindow.GetTalkWindowPreview().SetEventData(toggles,
                    image,
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[6],
                    message,
                    background,
                    position);
                _sceneWindow.Init();
                _sceneWindow.SetRenderingSize(2400, 1080);
                _sceneWindow.Render();
            };
        }
    }
}