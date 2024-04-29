using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Armor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Weapon;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using RPGMaker.Codebase.Editor.Hierarchy.Enum;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Character.View;
using RPGMaker.Codebase.Editor.Inspector.Trait.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.Editor.Inspector.Trait.View.TraitsInspectorElement;

namespace RPGMaker.Codebase.Editor.Inspector.Character.View
{
    /// <summary>
    /// [キャラクター]-[アクター/NPC] Inspector
    /// </summary>
    public class CharacterInspectorElement : AbstractInspectorElement
    {
        private CharacterActorDataModel _actor;

        private          List<CharacterActorDataModel> _characterActorDataModels;
        private readonly CharacterHierarchyView        _element;

        private readonly string _id = "";

        //名前の文字数上限
        private readonly int _nameMax = 10;
        private readonly int           _type;

        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/Character/Asset/inspector_character_actor.uxml"; } }

        public CharacterInspectorElement(int type, string uuid, CharacterHierarchyView element) {
            _type = type;
            _id = uuid;
            _element = element;

            Refresh();
        }

        override protected void RefreshContents() {
            base.RefreshContents();
            Clear();
            _characterActorDataModels = databaseManagementService.LoadCharacterActor();

            if (_type == (int) ActorTypeEnum.ACTOR)
            {
                foreach (var actor in _characterActorDataModels)
                    if (actor.uuId == _id)
                    {
                        _actor = actor;
                        break;
                    }

                base.Initialize();
                CreateActor();
            }
            else if (_type == (int) ActorTypeEnum.NPC)
            {
                foreach (var actor in _characterActorDataModels)
                    if (actor.uuId == _id)
                    {
                        _actor = actor;
                        break;
                    }

                base.Initialize();
                CreateNpc();
            }
        }

        private void CreateActor() {
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();

            //AssetData
            var manageData = databaseManagementService.LoadAssetManage();

            VisualElement actorArea = RootContainer.Query<VisualElement>("actor_area");
            actorArea.style.display = DisplayStyle.Flex;
            VisualElement npcArea = RootContainer.Query<VisualElement>("npc_area");
            npcArea.style.display = DisplayStyle.None;

            Label actorIdText = RootContainer.Query<Label>("actor_id_text");
            actorIdText.text = _actor.SerialNumberString;

            ImTextField actorNameText = RootContainer.Query<ImTextField>("actor_name_text");
            actorNameText.value = _actor.basic.name;
            actorNameText.RegisterCallback<FocusOutEvent>(evt =>
            {
                //名前の文字数制限
                var name = actorNameText.value;

                if (name.Length > _nameMax)
                    name = name.Substring(0, _nameMax);

                _actor.basic.name = name;
                Save();
                UpdateData();
            });

            //職業から取得
            var classData = databaseManagementService.LoadClassCommon();
            //class_dropdown
            VisualElement classDropdown = RootContainer.Query<VisualElement>("class_dropdown");
            var classList = new List<string>();
            var num = 0;
            for (var i = 0; i < classData.Count; i++)
            {
                classList.Add(classData[i].basic.name);
                if (classData[i].id == _actor.basic.classId) num = i;
            }

            //初期装備リストを初期化
            SetInitialEquip(classData[num].id);

            //actor_secondName_text
            ImTextField actorSecondNameText = RootContainer.Query<ImTextField>("actor_secondName_text");
            actorSecondNameText.value = _actor.basic.secondName;
            actorSecondNameText.RegisterCallback<FocusOutEvent>(evt =>
            {
                //名前の文字数制限
                var name = actorSecondNameText.value;

                if (name.Length > _nameMax)
                    name = name.Substring(0, _nameMax);

                _actor.basic.secondName = name;
                Save();
            });

            //最大レベルの上限は、[職業の編集] - [共通設定]で指定されているレベルとなる
            //Inspector表示時に、初期レベル及び最大レベルが、この値を越えている場合には初期表示値を修正する
            var classDataModel = databaseManagementService.LoadClassCommon()[0];
            var maxLevel = classDataModel.maxLevel; //最大レベル
            if (_actor.basic.initialLevel > maxLevel)
            {
                _actor.basic.initialLevel = maxLevel;
            }
            if (_actor.basic.maxLevel > maxLevel)
            {
                _actor.basic.maxLevel = maxLevel;
            }

            //初期レベル
            IntegerField initialLevelText = RootContainer.Query<IntegerField>("initialLevel_text");
            initialLevelText.value = _actor.basic.initialLevel;
            BaseInputFieldHandler.IntegerFieldCallback(initialLevelText, evt =>
            {
                //初期レベルが最大レベルを越えている場合には、最大レベルで設定する
                if (initialLevelText.value > _actor.basic.maxLevel)
                {
                    initialLevelText.value = _actor.basic.maxLevel;
                }

                _actor.basic.initialLevel = initialLevelText.value;
                Save();
            }, 1, 99);

            //最大レベル
            IntegerField maxLevelText = RootContainer.Query<IntegerField>("maxLevel_text");
            maxLevelText.value = _actor.basic.maxLevel;
            BaseInputFieldHandler.IntegerFieldCallback(maxLevelText, evt =>
            {
                _actor.basic.maxLevel = maxLevelText.value;

                //初期レベルが最大レベルを越えている場合には、最大レベルで設定する
                if (initialLevelText.value > _actor.basic.maxLevel)
                {
                    initialLevelText.value = _actor.basic.maxLevel;
                    _actor.basic.initialLevel = initialLevelText.value;
                }

                Save();
            }, 1, maxLevel);

            //プロフィール
            ImTextField profileText = RootContainer.Query<ImTextField>("profile_text");
            profileText.value = _actor.basic.profile;
            profileText.RegisterCallback<FocusOutEvent>(evt =>
            {
                _actor.basic.profile = profileText.value;
                Save();
            });

            ImTextField memoText = RootContainer.Query<ImTextField>("memo_text");
            memoText.value = _actor.basic.memo;
            memoText.RegisterCallback<FocusOutEvent>(evt =>
            {
                _actor.basic.memo = memoText.value;
                Save();
            });

            //属性
            var elements = databaseManagementService.LoadSystem().elements;
            Label actorClassElementText = RootContainer.Query<Label>("actor_class_element_text");

            // 職業設定
            var classDropdownPopupField = new PopupFieldBase<string>(classList, num);
            classDropdown.Add(classDropdownPopupField);
            classDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                _actor.basic.classId = classData[classDropdownPopupField.index].id;
                Save();

                //職業を選択時、初期装備枠を更新
                SetInitialEquip(classData[classDropdownPopupField.index].id);
                //職業属性の表示更新
                UpdateClassElement();
            });

            //職業属性の更新
            UpdateClassElement();

            void UpdateClassElement() {
                if (classData.Count > 0)
                {
                    var elementStr = EditorLocalize.LocalizeText("WORD_0113");
                    foreach (var e in elements)
                        if (e.id == classData[classDropdownPopupField.index].element)
                        {
                            elementStr = e.value;
                            break;
                        }

                    actorClassElementText.text = elementStr;
                }
                else
                {
                    actorClassElementText.text = "";
                }
            }


            //個別属性
            VisualElement actorClassDropdown = RootContainer.Query<VisualElement>("actor_class_dropdown");
            var elementList = new List<string>();
            foreach (var e in elements) elementList.Add(e.value);

            var actorClassDropdownPopupField = new PopupFieldBase<string>(elementList, _actor.element);
            actorClassDropdown.Add(actorClassDropdownPopupField);
            actorClassDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                _actor.element = actorClassDropdownPopupField.index;
                Save();
            });

            // 顔画像
            //------------------------------------------------------------------------------------------------------------------------------
            // プレビュー画像
            Image actorFaceImage = RootContainer.Query<Image>("actor_face_image");
            actorFaceImage.scaleMode = ScaleMode.ScaleToFit;
            actorFaceImage.image = ImageManager.LoadFace(_actor.image.face);

            // 画像名
            Label actorFaceImageName = RootContainer.Query<Label>("actor_face_image_name");
            actorFaceImageName.text = _actor.image.face;

            // 画像変更ボタン
            Button actorFaceChangeBtn = RootContainer.Query<Button>("actor_face_change_btn");
            actorFaceChangeBtn.clicked += () =>
            {
                var selectModalWindow = new ImageSelectModalWindow(PathManager.IMAGE_FACE, true);
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Image"), data =>
                {
                    var imageName = (string) data;
                    actorFaceImage.image = ImageManager.LoadFace(imageName);
                    _actor.image.face = imageName;
                    actorFaceImageName.text = imageName;
                    Save();
                }, _actor.image.face);
            };

            // インポートボタン
            Button actorFaceImportBtn = RootContainer.Query<Button>("actor_face_import_btn");
            actorFaceImportBtn.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("png", PathManager.IMAGE_FACE);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    actorFaceImage.image = ImageManager.LoadFace(path);
                    _actor.image.face = path;
                    actorFaceImageName.text = path;
                    Save();
                }
            };

            // マップでの画像
            //------------------------------------------------------------------------------------------------------------------------------
            // プレビュー画像
            Image actorMapImage = RootContainer.Query<Image>("actor_map_image");
            actorMapImage.scaleMode = ScaleMode.ScaleToFit;

            // 画像名
            Label actorMapImageName = RootContainer.Query<Label>("actor_map_image_name");

            string imageNameWork = "";
            string assetId = "";
            for (int i = 0; i < manageData.Count; i++)
            {
                if (manageData[i].id == _actor.image.character)
                {
                    imageNameWork = manageData[i].name;
                    assetId = manageData[i].id;
                    break;
                }
            }
            actorMapImageName.text = imageNameWork;
            actorMapImage.image = ImageManager.LoadSvCharacter(assetId);

            // 画像変更ボタン
            Button actorMapChangeBtn = RootContainer.Query<Button>("actor_map_change_btn");
            actorMapChangeBtn.clicked += () =>
            {
                var selectModalWindow = new SdSelectModalWindow();
                selectModalWindow.CharacterSdType = SdSelectModalWindow.CharacterType.Map;
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Image"), data =>
                {
                    var imageName = (string) data;
                    _actor.image.character = imageName;
                    actorMapImage.image = ImageManager.LoadSvCharacter(_actor.image.character);

                    string imageNameWork = "";
                    for (int i = 0; i < manageData.Count; i++)
                    {
                        if (manageData[i].id == _actor.image.character)
                        {
                            imageNameWork = manageData[i].name;
                            break;
                        }
                    }
                    actorMapImageName.text = imageNameWork;

                    Save();
                }, _actor.image.character);
            };

            // 画像インポートボタン
            // 素材管理からしかインポートできないためボタンなし

            // バトルでの画像
            //------------------------------------------------------------------------------------------------------------------------------
            // プレビュー画像
            Image actorBattleImage = RootContainer.Query<Image>("actor_battle_image");
            actorBattleImage.scaleMode = ScaleMode.ScaleToFit;
            

            // 画像名
            Label actorBattleImageName = RootContainer.Query<Label>("actor_battle_image_name");

            imageNameWork = "";
            assetId = "";
            for (int i = 0; i < manageData.Count; i++)
            {
                if (manageData[i].id == _actor.image.battler)
                {
                    imageNameWork = manageData[i].name;
                    assetId = manageData[i].id;
                    break;
                }
            }
            actorBattleImageName.text = imageNameWork;
            actorBattleImage.image = ImageManager.LoadSvCharacter(assetId);

            // 画像変更ボタン
            Button actorBattleChangeBtn = RootContainer.Query<Button>("actor_battle_change_btn");
            actorBattleChangeBtn.clicked += () =>
            {
                var selectModalWindow = new SdSelectModalWindow();
                selectModalWindow.CharacterSdType = SdSelectModalWindow.CharacterType.Battle;
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Image"), data =>
                {
                    var imageName = (string) data;
                    _actor.image.battler = imageName;
                    actorBattleImage.image = ImageManager.LoadSvCharacter(_actor.image.battler);

                    string imageNameWork = "";
                    for (int i = 0; i < manageData.Count; i++)
                    {
                        if (manageData[i].id == _actor.image.battler)
                        {
                            imageNameWork = manageData[i].name;
                            break;
                        }
                    }
                    actorBattleImageName.text = imageNameWork;

                    Save();
                }, _actor.image.battler);
            };

            // 画像インポートボタン
            // 素材管理からしかインポートできないためボタンなし

            // ピクチャ
            //------------------------------------------------------------------------------------------------------------------------------
            // プレビュー画像
            Image actorAdvImage = RootContainer.Query<Image>("actor_adv_image");
            actorAdvImage.scaleMode = ScaleMode.ScaleToFit;
            actorAdvImage.image = ImageManager.LoadPicture(_actor.image.adv);

            // 画像名
            Label actorAdvImageName = RootContainer.Query<Label>("actor_adv_image_name");
            actorAdvImageName.text = _actor.image.adv;

            // 画像変更ボタン
            Button actorAdvChangeBtn = RootContainer.Query<Button>("actor_adv_change_btn");
            actorAdvChangeBtn.clicked += () =>
            {
                var selectModalWindow = new ImageSelectModalWindow(PathManager.IMAGE_ADV, true);
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Image"), data =>
                {
                    var imageName = (string) data;
                    _actor.image.adv = imageName;
                    actorAdvImage.image = ImageManager.LoadPicture(_actor.image.adv);
                    actorAdvImageName.text = imageName;
                    Save();
                }, actorAdvImageName.text);
            };

            // 画像インポートボタン
            Button actorAdvImportBtn = RootContainer.Query<Button>("actor_adv_import_btn");
            actorAdvImportBtn.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("png", PathManager.IMAGE_ADV);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    _actor.image.adv = path;
                    actorAdvImage.image = ImageManager.LoadPicture(_actor.image.adv);
                    actorAdvImageName.text = path;
                    Save();
                }
            };

            //class_traits_new
            //class_traits_area
            VisualElement classTraitsArea = RootContainer.Query<VisualElement>("class_traits_area");
            var traitWindow = new TraitsInspectorElement();
            classTraitsArea.Add(traitWindow);
            traitWindow.Init(_actor.traits, TraitsType.TRAITS_TYPE_ACTOR, evt =>
            {
                _actor.traits = (List<TraitCommonDataModel>) evt;
                Save();
            });
        }

        private void CreateNpc() {
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();

            //AssetData
            var manageData = databaseManagementService.LoadAssetManage();

            VisualElement actorArea = RootContainer.Query<VisualElement>("actor_area");
            actorArea.style.display = DisplayStyle.None;
            VisualElement npcArea = RootContainer.Query<VisualElement>("npc_area");
            npcArea.style.display = DisplayStyle.Flex;

            //npc_name_text
            Label npcIdText = RootContainer.Query<Label>("npc_id_text");
            npcIdText.text = _actor.SerialNumberString;

            ImTextField npcNameText = RootContainer.Query<ImTextField>("npc_name_text");
            npcNameText.value = _actor.basic.name;
            npcNameText.RegisterCallback<FocusOutEvent>(evt =>
            {
                _actor.basic.name = npcNameText.value;
                Save();
                UpdateData();
            });

            ImTextField memoText = RootContainer.Query<ImTextField>("npc_memo_text");
            memoText.value = _actor.basic.memo;
            memoText.RegisterCallback<FocusOutEvent>(evt =>
            {
                _actor.basic.memo = memoText.value;
                Save();
            });

            // 顔画像
            //------------------------------------------------------------------------------------------------------------------------------
            // プレビュー画像
            Image npcFaceImage = RootContainer.Query<Image>("npc_face_image");
            npcFaceImage.scaleMode = ScaleMode.ScaleToFit;
            npcFaceImage.image = ImageManager.LoadFace(_actor.image.face);

            // 画像名
            Label npcFaceImageName = RootContainer.Query<Label>("npc_face_image_name");
            npcFaceImageName.text = _actor.image.face;

            // 画像変更ボタン
            Button npcFaceChangeBtn = RootContainer.Query<Button>("npc_face_change_btn");
            npcFaceChangeBtn.clicked += () =>
            {
                var selectModalWindow = new ImageSelectModalWindow(PathManager.IMAGE_FACE, true);
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Image"), data =>
                {
                    var imageName = (string) data;
                    _actor.image.face = imageName;
                    npcFaceImage.image = ImageManager.LoadFace(_actor.image.face);
                    npcFaceImageName.text = imageName;
                    Save();
                }, _actor.image.face);
            };

            // 画像インポートボタン
            Button npcFaceImportBtn = RootContainer.Query<Button>("npc_face_import_btn");
            npcFaceImportBtn.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("png", PathManager.IMAGE_FACE);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    _actor.image.face = path;
                    npcFaceImage.image = ImageManager.LoadFace(_actor.image.face);
                    npcFaceImageName.text = path;
                    Save();
                }
            };

            // マップ上の画像
            //------------------------------------------------------------------------------------------------------------------------------
            // プレビュー画像
            Image npcMapImage = RootContainer.Query<Image>("npc_map_image");
            npcMapImage.scaleMode = ScaleMode.ScaleToFit;
            

            // 画像名
            Label npcMapImageName = RootContainer.Query<Label>("npc_map_image_name");

            string imageNameWork = "";
            string assetId = "";
            for (int i = 0; i < manageData.Count; i++)
            {
                if (manageData[i].id == _actor.image.character)
                {
                    imageNameWork = manageData[i].name;
                    assetId = manageData[i].id;
                    break;
                }
            }
            npcMapImageName.text = imageNameWork;
            npcMapImage.image = ImageManager.LoadSvCharacter(assetId);

            // 画像変更ボタン
            Button npcMapChangeBtn = RootContainer.Query<Button>("npc_map_change_btn");
            npcMapChangeBtn.clicked += () =>
            {
                var selectModalWindow = new SdSelectModalWindow();
                selectModalWindow.CharacterSdType = SdSelectModalWindow.CharacterType.Map;
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Image"), data =>
                {
                    var imageName = (string) data;
                    _actor.image.character = imageName;
                    npcMapImage.image = ImageManager.LoadSvCharacter(_actor.image.character);

                    string imageNameWork = "";
                    for (int i = 0; i < manageData.Count; i++)
                    {
                        if (manageData[i].id == _actor.image.character)
                        {
                            imageNameWork = manageData[i].name;
                            break;
                        }
                    }
                    npcMapImageName.text = imageNameWork;

                    Save();
                }, _actor.image.character);
            };

            // 画像インポートボタン
            // 素材管理からしかインポートできないためボタンなし

            // ピクチャ
            //------------------------------------------------------------------------------------------------------------------------------
            // プレビュー画像
            Image npcAdvImage = RootContainer.Query<Image>("npc_adv_image");
            npcAdvImage.scaleMode = ScaleMode.ScaleToFit;
            npcAdvImage.image = ImageManager.LoadPicture(_actor.image.adv);

            // 画像名
            Label npcAdvImageName = RootContainer.Query<Label>("npc_adv_image_name");
            npcAdvImageName.text = _actor.image.adv;

            // 画像変更ボタン
            Button npcAdvChangeBtn = RootContainer.Query<Button>("npc_adv_change_btn");
            npcAdvChangeBtn.clicked += () =>
            {
                var selectModalWindow = new ImageSelectModalWindow(PathManager.IMAGE_ADV, true);
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Image"), data =>
                {
                    var imageName = (string) data;
                    _actor.image.adv = imageName;
                    npcAdvImage.image = ImageManager.LoadPicture(_actor.image.adv);
                    npcAdvImageName.text = imageName;
                    Save();
                }, _actor.image.adv);
            };

            // 画像インポートボタン
            Button npcAdvImportBtn = RootContainer.Query<Button>("npc_adv_import_btn");
            npcAdvImportBtn.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("png", PathManager.IMAGE_ADV);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    _actor.image.adv = path;
                    npcAdvImage.image = ImageManager.LoadPicture(_actor.image.adv);
                    npcAdvImageName.text = path;
                    Save();
                }
            };
        }

        override protected void SaveContents() {
            //セーブ部位の作成
            databaseManagementService.SaveCharacterActor(_characterActorDataModels);
        }

        private void UpdateData() {
            _ = Editor.Hierarchy.Hierarchy.Refresh(Region.Character, _actor.uuId);
        }

        private void SetInitialEquip(string classId) {
            var classData = databaseManagementService.LoadClassCommon();
            var classIndex = classData.IndexOf(classData.FirstOrDefault(data => data.id == classId));

            //初期装備
            //職業の防具タイプを取得
            //防具を取得
            //そのうえで装備タイプが一緒だった箇所に初期装備として表示させる
            VisualElement initialEquips = RootContainer.Query<VisualElement>("initial_equips");
            initialEquips.Clear();

            var equipTypes = databaseManagementService.LoadSystem().equipTypes;
            var weaponTypes = databaseManagementService.LoadSystem().weaponTypes;
            var armorTypes = databaseManagementService.LoadSystem().armorTypes;
            var wList = databaseManagementService.LoadWeapon();
            var weaponList = new List<WeaponDataModel>();

            foreach (var cw in classData[classIndex].weaponTypes)
            foreach (var w in wList)
                if (w.basic.weaponTypeId == cw)
                    weaponList.Add(w);


            var aList = databaseManagementService.LoadArmor();
            var armorList = new List<ArmorDataModel>();
            foreach (var ca in classData[classIndex].armorTypes)
            foreach (var l in aList)
                if (l.basic.armorTypeId == ca)
                    armorList.Add(l);

            if (_actor.equips.Count == 0) _actor.equips = new List<CharacterActorDataModel.Equipment>();

            var data = new List<CharacterActorDataModel.Equipment>();
            for (var i = 0; i < equipTypes.Count; i++)
            {
                var dataWork = new CharacterActorDataModel.Equipment(equipTypes[i].id, "");
                for (var j = 0; j < _actor.equips.Count; j++)
                    if (equipTypes[i].id == _actor.equips[j].type)
                    {
                        dataWork.value = _actor.equips[j].value;
                        break;
                    }

                data.Add(dataWork);
            }

            _actor.equips = data;

            //初期武器設定
            VisualElement weaponElement = new InspectorItemUnit();
            var la = new Label(equipTypes[0].name);
            weaponElement.Add(la);
            var strList = new List<string>();
            strList.Add(EditorLocalize.LocalizeText("WORD_0113"));
            var defaultIndex = -1;
            for (var j = 0; j < weaponList.Count; j++) strList.Add(weaponList[j].basic.name);
            WeaponDataModel weaponData = null;
            try
            {
                for (int i = 0; i < weaponList.Count; i++)
                    if (weaponList[i].basic.id == _actor.equips[0].value)
                    {
                        weaponData = weaponList[i];
                        break;
                    }
                defaultIndex = strList.IndexOf(weaponData.basic.name);
            }
            catch (Exception)
            {
            }

            if (defaultIndex < 0)
            {
                //このケースでは、職業を変更した等の理由で、装備不可能なものが選択されているため、初期化する
                defaultIndex = 0;
                _actor.equips[0].value = "";
            }

            var weaponPopupField = new PopupFieldBase<string>(strList, defaultIndex);
            weaponElement.Add(weaponPopupField);
            weaponPopupField.RegisterValueChangedCallback(evt =>
            {
                WeaponDataModel weaponData = null;
                for (int i = 0; i < weaponList.Count; i++)
                    if (weaponList[i].basic.name == weaponPopupField.value)
                    {
                        weaponData = weaponList[i];
                        break;
                    }

                if (_actor.equips.Count > 1)
                {
                    if (weaponData != null)
                    {
                        _actor.equips[0].type = weaponData.basic.equipmentTypeId;
                        _actor.equips[0].value = weaponData.basic.id;
                    }
                    else
                    {
                        //初期装備(武器)無しの時
                        _actor.equips[0].type = "";
                        _actor.equips[0].value = "";
                    }
                }
                else
                {
                    _actor.equips.Add(new CharacterActorDataModel.Equipment(weaponData.basic.weaponTypeId,
                        weaponData.basic.id));
                }

                Save();
            });
            initialEquips.Add(weaponElement);

            //初期防具設定
            for (var i = 1; i < equipTypes.Count; i++)
            {
                VisualElement element = new InspectorItemUnit();
                la = new Label(equipTypes[i].name);
                element.Add(la);
                strList = new List<string>();
                strList.Add(EditorLocalize.LocalizeText("WORD_0113"));
                for (var j = 0; j < armorList.Count; j++)
                    if (equipTypes[i].id == armorList[j].basic.equipmentTypeId)
                        strList.Add(armorList[j].basic.name);

                ArmorDataModel armorData = null;
                defaultIndex = -1;
                try
                {
                    for (int i2 = 0; i2 < armorList.Count; i2++)
                        if (armorList[i2].basic.id == _actor.equips[i].value)
                        {
                            armorData = armorList[i2];
                            break;
                        }

                    defaultIndex = strList.IndexOf(armorData.basic.name);
                }
                catch (Exception)
                {
                }

                if (defaultIndex < 0)
                {
                    //このケースでは、職業を変更した等の理由で、装備不可能なものが選択されているため、初期化する
                    defaultIndex = 0;
                    _actor.equips[i].value = "";
                }

                var armorPopupField = new PopupFieldBase<string>(strList, defaultIndex);
                element.Add(armorPopupField);
                var index = i;
                armorPopupField.RegisterValueChangedCallback(evt =>
                {
                    ArmorDataModel armorDataWork = null;
                    for (int i2 = 0; i2 < armorList.Count; i2++)
                        if (armorList[i2].basic.name == armorPopupField.value)
                        {
                            armorDataWork = armorList[i2];
                            break;
                        }

                    if (_actor.equips.Count > index)
                    {
                        if (armorDataWork != null)
                        {
                            _actor.equips[index].type = armorDataWork.basic.equipmentTypeId;
                            _actor.equips[index].value = armorDataWork.basic.id;
                        }
                        else
                        {
                            //初期装備(防具)無しの時
                            _actor.equips[index].type = "";
                            _actor.equips[index].value = "";
                        }
                    }
                    else
                    {
                        _actor.equips.Add(new CharacterActorDataModel.Equipment(armorDataWork.basic.armorTypeId,
                            armorDataWork.basic.id));
                    }

                    Save();
                });
                initialEquips.Add(element);
            }
        }
    }
}