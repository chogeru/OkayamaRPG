using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.SystemSetting
{
    /// <summary>
    /// アクターの画像変更
    /// </summary>
    public class ChangeActorImage : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_change_actor_image.uxml";
        
        private PopupFieldBase<string> _characterMapPopupField;
        private PopupFieldBase<string> _characterBattlePopupField;

        public ChangeActorImage(
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

            var charactorDropdownChoices = _GetCharacterList();

            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();

            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Count != 5)
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters
                    .Add(charactorDropdownChoices[0].uuId);
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters
                    .Add(charactorDropdownChoices[0].image.face);
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters
                    .Add(charactorDropdownChoices[0].image.character);
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters
                    .Add(charactorDropdownChoices[0].image.battler);
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters
                    .Add(charactorDropdownChoices[0].image.adv);
                EventManagementService.SaveEvent(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            ////最初の画像表示
            //キャラクタードロップダウン
            VisualElement actorDropdown = RootElement.Q<VisualElement>("systemSetting_changeActorPicture")
                .Query<VisualElement>("actor_dropdown");
            VisualElement charactorDropdown = RootElement.Q<VisualElement>("systemSetting_changeActorPicture")
                .Query<VisualElement>("walk_dropdown");
            VisualElement battleDropdown = RootElement.Q<VisualElement>("systemSetting_changeActorPicture")
                .Query<VisualElement>("battle_dropdown");
            
            var actorListId = -1;
            var faceListId = -1;
            var characterListId = -1;
            var battleListId = -1;
            var pictureListId = -1;
            for (var i = 0; i < charactorDropdownChoices.Count; i++)
            {
                if (actorListId == -1 && charactorDropdownChoices[i].uuId ==
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0])
                    actorListId = i;

                if (faceListId == -1 && charactorDropdownChoices[i].image.face ==
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1])
                    faceListId = i;
                
                if (pictureListId == -1 && charactorDropdownChoices[i].image.adv ==
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4])
                    pictureListId = i;
            }
            
            //選択肢に名前を表示売る際に一時的に使用するList
            var characterName = new List<string>();
            for (var i = 0; i < charactorDropdownChoices.Count; i++)
                characterName.Add(charactorDropdownChoices[i].basic.name);

            var face = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1];
            // プレビュー画像
            Image actorFaceImage = RootElement.Query<Image>("actor_face_image");
            actorFaceImage.scaleMode = ScaleMode.ScaleToFit;
            actorFaceImage.image = ImageManager.LoadFace(face);

            // 画像名
            Label actorFaceImageName = RootElement.Query<Label>("actor_face_image_name");
            actorFaceImageName.text = face;

            // 画像変更ボタン
            Button actorFaceChangeBtn = RootElement.Query<Button>("actor_face_change_btn");
            actorFaceChangeBtn.clicked += () =>
            {
                var selectModalWindow = new ImageSelectModalWindow(PathManager.IMAGE_FACE, true);
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Image"), data =>
                {
                    var imageName = (string) data;
                    actorFaceImage.image = ImageManager.LoadFace(imageName);
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = imageName;
                    actorFaceImageName.text = imageName;
                    Save(EventDataModels[EventIndex]);
                }, EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1]);
            };

            // インポートボタン
            Button actorFaceImportBtn = RootElement.Query<Button>("actor_face_import_btn");
            actorFaceImportBtn.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("png", PathManager.IMAGE_FACE);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    actorFaceImage.image = ImageManager.LoadFace(path);
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = path;
                    actorFaceImageName.text = path;
                    Save(EventDataModels[EventIndex]);
                }
            };

            // マップでの画像
            //------------------------------------------------------------------------------------------------------------------------------

            Image actorMapImage = RootElement.Query<Image>("walk_preview");
            
            var characterMapAssets = GetAssetManageList(SdSelectModalWindow.CharacterType.Map);
            for (int i = 0; i < characterMapAssets[1].Count; i++)
            {
                if (characterMapAssets[1][i] == EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2])
                {
                    characterListId = i;
                    actorMapImage.scaleMode = ScaleMode.ScaleToFit;
                    actorMapImage.image = ImageManager.LoadSvCharacter(
                        characterMapAssets[1][i]);
                    break;
                }
            }

            if (characterListId == -1 && characterMapAssets[1].Count > 0)
            {
                characterListId = 0;
            }

            if (characterMapAssets.Count > 0 && characterListId != -1)
            {
                _characterMapPopupField = new PopupFieldBase<string>(characterMapAssets[0], characterListId);
                charactorDropdown.Clear();
                charactorDropdown.Add(_characterMapPopupField);
                _characterMapPopupField.RegisterValueChangedCallback(evt =>
                {
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] =
                        characterMapAssets[1][_characterMapPopupField.index];
                    actorMapImage.scaleMode = ScaleMode.ScaleToFit;
                    actorMapImage.image = ImageManager.LoadSvCharacter(
                        characterMapAssets[1][_characterMapPopupField.index]);
                    Save(EventDataModels[EventIndex]);
                });
            }

            // バトルでの画像
            //------------------------------------------------------------------------------------------------------------------------------
            
            Image actorBattleImage = RootElement.Query<Image>("battle_preview");
            var characterBattleAssets = GetAssetManageList(SdSelectModalWindow.CharacterType.Battle);
            for (int i = 0; i < characterBattleAssets[1].Count; i++)
            {
                if (characterBattleAssets[1][i] == EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3])
                {
                    battleListId = i;
                    actorBattleImage.scaleMode = ScaleMode.ScaleToFit;
                    actorBattleImage.image = ImageManager.LoadSvCharacter(
                        characterBattleAssets[1][i]);
                    Save(EventDataModels[EventIndex]);
                    break;
                }
            }
            if (battleListId == -1 && characterBattleAssets[1].Count > 0)
            {
                battleListId = 0;
            }

            if (characterBattleAssets.Count > 0 && battleListId != -1)
            {
                _characterBattlePopupField = new PopupFieldBase<string>(characterBattleAssets[0], battleListId);
                battleDropdown.Clear();
                battleDropdown.Add(_characterBattlePopupField);
                _characterBattlePopupField.RegisterValueChangedCallback(evt =>
                {
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] =
                        characterBattleAssets[1][_characterBattlePopupField.index];

                    actorBattleImage.scaleMode = ScaleMode.ScaleToFit;
                    actorBattleImage.image = ImageManager.LoadSvCharacter(
                        characterBattleAssets[1][_characterBattlePopupField.index]);
                    Save(EventDataModels[EventIndex]);
                });
            }

            // ピクチャ
            //------------------------------------------------------------------------------------------------------------------------------
            // プレビュー画像
            Image actorAdvImage = RootElement.Query<Image>("actor_adv_image");
            actorAdvImage.scaleMode = ScaleMode.ScaleToFit;
            actorAdvImage.image =
                ImageManager.LoadPicture(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4]);

            // 画像名
            Label actorAdvImageName = RootElement.Query<Label>("actor_adv_image_name");
            actorAdvImageName.text = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4];

            // 画像変更ボタン
            Button actorAdvChangeBtn = RootElement.Query<Button>("actor_adv_change_btn");
            actorAdvChangeBtn.clicked += () =>
            {
                var selectModalWindow = new ImageSelectModalWindow(PathManager.IMAGE_ADV, true);
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Image"), data =>
                {
                    var imageName = (string) data;
                    actorAdvImage.image =ImageManager.LoadPicture(imageName);
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] = imageName;
                    actorAdvImageName.text = imageName;
                    Save(EventDataModels[EventIndex]);
                }, EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4]);
            };
            
            // インポートボタン
            Button actorPictureImportBtn = RootElement.Query<Button>("actor_adv_import_btn");
            actorPictureImportBtn.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("png", PathManager.IMAGE_ADV);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    actorAdvImage.image = ImageManager.LoadPicture(path);
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] = path;
                    actorAdvImageName.text = path;
                    Save(EventDataModels[EventIndex]);
                }
            };

            //ValueChangeを考慮するため一番下に
            DropdownSettings(actorDropdown, characterName, actorListId, charactorDropdownChoices);
        }

        //キャラクターListの取得
        private List<CharacterActorDataModel> _GetCharacterList() {
            var characterActorDataModels =
                DatabaseManagementService.LoadCharacterActor();
            var fileNames = new List<CharacterActorDataModel>();
            for (var i = 0; i < characterActorDataModels.Count; i++)
            {
                if (characterActorDataModels[i].charaType == (int) ActorTypeEnum.ACTOR)
                {
                    fileNames.Add(characterActorDataModels[i]);
                }
            }

            return fileNames;
        }

        //各ドロップダウンの作成
        private void DropdownSettings(
            VisualElement dropdown,
            List<string> nameList,
            int id,
            List<CharacterActorDataModel> charaData
        ) {
            var characterDropdownPopupField = new PopupFieldBase<string>(nameList, id);
            dropdown.Clear();
            dropdown.Add(characterDropdownPopupField);
            characterDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] =
                    charaData[characterDropdownPopupField.index].uuId;
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                    charaData[characterDropdownPopupField.index].image.face;
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] =
                    charaData[characterDropdownPopupField.index].image.character;
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3] =
                    charaData[characterDropdownPopupField.index].image.battler;
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4] =
                    charaData[characterDropdownPopupField.index].image.adv;

                //画像の表示
                ActorImages();

                Save(EventDataModels[EventIndex]);

            });
        }

        //画像表示用
        private void ActorImages() {
            //画像読み込み
            var face = AssetDatabase.LoadAssetAtPath<Texture2D>(
                PathManager.IMAGE_FACE + EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] +
                ".png");
            var picture =
                ImageManager.LoadPicture(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4]);

            Image actorMapImage = RootElement.Query<Image>("walk_preview");
            var characterMapAssets = GetAssetManageList(SdSelectModalWindow.CharacterType.Map);
            for (int i = 0; i < characterMapAssets[1].Count; i++)
            {
                if (characterMapAssets[1][i] == EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2])
                {
                    _characterMapPopupField.ChangeButtonText(i);
                    _characterMapPopupField.index = i;
                    actorMapImage.scaleMode = ScaleMode.ScaleToFit;
                    actorMapImage.image = ImageManager.LoadSvCharacter(characterMapAssets[1][i]);
                    break;
                }
            }

            Image actorBattleImage = RootElement.Query<Image>("battle_preview");
            var characterBattleAssets = GetAssetManageList(SdSelectModalWindow.CharacterType.Battle);
            for (int i = 0; i < characterBattleAssets[1].Count; i++)
            {
                if (characterBattleAssets[1][i] ==
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[3])
                {
                    _characterBattlePopupField.ChangeButtonText(i);
                    _characterBattlePopupField.index = i;
                    actorBattleImage.scaleMode = ScaleMode.ScaleToFit;
                    actorBattleImage.image = ImageManager.LoadSvCharacter(characterBattleAssets[1][i]);
                    break;
                }
            }

            //画像の表示
            RootElement.Query<Image>("actor_face_image").AtIndex(0).image = face;
            Label actorFaceImageName = RootElement.Query<Label>("actor_face_image_name");
            actorFaceImageName.text = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1];
            RootElement.Query<Image>("actor_adv_image").AtIndex(0).image = picture;
            Label actorAdvImageName = RootElement.Query<Label>("actor_adv_image_name");
            actorAdvImageName.text = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[4];
        }
        

        /// <summary>
        /// 素材管理から取得
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private List<List<string>> GetAssetManageList(SdSelectModalWindow.CharacterType type) {
            var orderData = AssetManageRepository.OrderManager.Load();
            var assetManageData = new List<AssetManageDataModel>();
            var databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;
            var manageData = databaseManagementService.LoadAssetManage();
            var category = type == SdSelectModalWindow.CharacterType.Map
                ? AssetCategoryEnum.MOVE_CHARACTER
                : AssetCategoryEnum.SV_BATTLE_CHARACTER;
            var category2 = category == AssetCategoryEnum.MOVE_CHARACTER
                ? AssetCategoryEnum.OBJECT
                : AssetCategoryEnum.SV_BATTLE_CHARACTER;

            for (var i = 0; i < orderData.orderDataList.Length; i++)
            {
                if (orderData.orderDataList[i].idList == null)
                    continue;
                if (orderData.orderDataList[i].assetTypeId == (int) category ||
                    orderData.orderDataList[i].assetTypeId == (int) category2)
                {
                    for (var i2 = 0; i2 < orderData.orderDataList[i].idList.Count; i2++)
                    {
                        for (int i3 = 0; i3 < manageData.Count; i3++)
                            if (manageData[i3].id == orderData.orderDataList[i].idList[i2])
                            {
                                assetManageData.Add(manageData[i3]);
                                break;
                            }
                    }
                }
            }
            
            var fileNames = new List<string>();
            var fileIds = new List<string>();

            // 先頭に「なし」の選択肢を追加
            fileNames.Add(EditorLocalize.LocalizeText("WORD_0113"));
            fileIds.Add("");
            // ファイルの情報をアセットデータから格納
            foreach (var asset in assetManageData)
            {
                if (asset == null) continue;
                fileNames.Add(asset.name);
                fileIds.Add(asset.id);
            }
            return new List<List<string>>(){fileNames,fileIds};
        }
    }
}