using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Class;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.CharacterEarlyParty.View
{
    /// <summary>
    /// [キャラクター]-[初期パーティ設定] Inspector
    /// </summary>
    public class CharacterEarlyPartyInspectorElement : AbstractInspectorElement
    {
        private readonly List<string>                  _charaName = new List<string>();
        private          List<CharacterActorDataModel> _characterActorDataModels;
        private          List<ClassDataModel>          _classDataModels;

        //MAPの情報が入るList
        private          List<MapDataModel>   _mapData;
        private SystemSettingDataModel _systemSettingDataModel;

        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/CharacterEarlyParty/Asset/inspector_character_earlyParty.uxml"; } }

        private IntegerField mapX;
        private IntegerField mapY;

        private int posX;
        private int posY;

        private Vector2Int _initialPos;

        public CharacterEarlyPartyInspectorElement() {
            Refresh();
        }

        override protected void RefreshContents() {
            base.RefreshContents();
            _systemSettingDataModel = databaseManagementService.LoadSystem();
            _characterActorDataModels = databaseManagementService.LoadCharacterActor();
            _classDataModels = databaseManagementService.LoadCharacterActorClass();
            _mapData = mapManagementService.LoadMaps();

            Initialize();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();
            var actors = _characterActorDataModels.ToList();

            Foldout member1Foldout = RootContainer.Query<Foldout>("member1Foldout");
            Foldout member2Foldout = RootContainer.Query<Foldout>("member2Foldout");
            Foldout member3Foldout = RootContainer.Query<Foldout>("member3Foldout");
            Foldout member4Foldout = RootContainer.Query<Foldout>("member4Foldout");
            switch (_systemSettingDataModel.initialParty.partyMax)
            {
                case 1:
                    member1Foldout.style.display = DisplayStyle.Flex;
                    member2Foldout.style.display = DisplayStyle.None;
                    member3Foldout.style.display = DisplayStyle.None;
                    member4Foldout.style.display = DisplayStyle.None;
                    break;
                case 2:
                    member1Foldout.style.display = DisplayStyle.Flex;
                    member2Foldout.style.display = DisplayStyle.Flex;
                    member3Foldout.style.display = DisplayStyle.None;
                    member4Foldout.style.display = DisplayStyle.None;
                    break;
                case 3:
                    member1Foldout.style.display = DisplayStyle.Flex;
                    member2Foldout.style.display = DisplayStyle.Flex;
                    member3Foldout.style.display = DisplayStyle.Flex;
                    member4Foldout.style.display = DisplayStyle.None;
                    break;
                case 4:
                    member1Foldout.style.display = DisplayStyle.Flex;
                    member2Foldout.style.display = DisplayStyle.Flex;
                    member3Foldout.style.display = DisplayStyle.Flex;
                    member4Foldout.style.display = DisplayStyle.Flex;
                    break;
            }

            //初期パーティメンバー数の設定ドロップダウン
            VisualElement earlyPartyMaxMemberDropdown = RootContainer.Query<VisualElement>("maxMember_dropdown");
            List<string> earlyPartyMaxMemberDropdownChoices;
            switch (_characterActorDataModels.Count)
            {
                case 1:
                    earlyPartyMaxMemberDropdownChoices = new List<string> {"1"};
                    break;
                case 2:
                    earlyPartyMaxMemberDropdownChoices = new List<string> {"1", "2"};
                    break;
                case 3:
                    earlyPartyMaxMemberDropdownChoices = new List<string> {"1", "2", "3"};
                    break;
                default:
                    earlyPartyMaxMemberDropdownChoices = new List<string> {"1", "2", "3", "4"};
                    break;
            }

            var earlyPartyMaxMemberDropdownPopupField =
                new PopupFieldBase<string>(earlyPartyMaxMemberDropdownChoices,
                    _systemSettingDataModel.initialParty.partyMax - 1);
            earlyPartyMaxMemberDropdown.Add(earlyPartyMaxMemberDropdownPopupField);
            earlyPartyMaxMemberDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                // パーティにデータ追加
                var max = _systemSettingDataModel.initialParty.party.Count;
                var partyNum = earlyPartyMaxMemberDropdownPopupField.index + 1 -
                               _systemSettingDataModel.initialParty.party.Count;

                var isAdd = true;
                if (partyNum < 0)
                {
                    isAdd = false;
                    partyNum = partyNum * -1;
                }

                for (var i = 0; i < partyNum; i++)
                {
                    var uuId = "";
                    if (isAdd)
                    {
                        foreach (var actor in actors)
                            //Actorの選別
                            if (actor.charaType == (int) ActorTypeEnum.ACTOR)
                                if (!_systemSettingDataModel.initialParty.party.Contains(actor.uuId))
                                {
                                    uuId = actor.uuId;
                                    break;
                                }

                        _systemSettingDataModel.initialParty.party.Add(uuId);
                    }
                    else
                    {
                        _systemSettingDataModel.initialParty.party.RemoveAt(max - 1 - i);
                    }
                }


                _systemSettingDataModel.initialParty.partyMax =
                    int.Parse(earlyPartyMaxMemberDropdownPopupField.value);
                Save();

                switch (int.Parse(earlyPartyMaxMemberDropdownPopupField.value))
                {
                    case 1:
                        member1Foldout.style.display = DisplayStyle.Flex;
                        member2Foldout.style.display = DisplayStyle.None;
                        member3Foldout.style.display = DisplayStyle.None;
                        member4Foldout.style.display = DisplayStyle.None;
                        break;
                    case 2:
                        member1Foldout.style.display = DisplayStyle.Flex;
                        member2Foldout.style.display = DisplayStyle.Flex;
                        member3Foldout.style.display = DisplayStyle.None;
                        member4Foldout.style.display = DisplayStyle.None;
                        break;
                    case 3:
                        member1Foldout.style.display = DisplayStyle.Flex;
                        member2Foldout.style.display = DisplayStyle.Flex;
                        member3Foldout.style.display = DisplayStyle.Flex;
                        member4Foldout.style.display = DisplayStyle.None;
                        break;
                    case 4:
                        member1Foldout.style.display = DisplayStyle.Flex;
                        member2Foldout.style.display = DisplayStyle.Flex;
                        member3Foldout.style.display = DisplayStyle.Flex;
                        member4Foldout.style.display = DisplayStyle.Flex;
                        break;
                }

                // 要素更新
                Clear();
                Initialize();
            });

            for (var i = 0; i < _characterActorDataModels.Count; i++)
                //Actorの選別
                if (_characterActorDataModels[i].charaType == 0)
                {
                    if (_charaName.Contains(_characterActorDataModels[i].uuId)) continue;
                    _charaName.Add(_characterActorDataModels[i].uuId);
                }

            //初期位置の座標表示部分
            var launchMapDataModel =
                _mapData.Find(map => map.id == _systemSettingDataModel.initialParty.startMap.mapId);
            if (launchMapDataModel != null)
            {
                MapEditor.MapEditor.LaunchCommonEventEditModeEnd(launchMapDataModel);
            }

            mapX = RootContainer.Query<IntegerField>("map_X");
            mapY = RootContainer.Query<IntegerField>("map_Y");
            mapX.value = _systemSettingDataModel.initialParty.startMap.position[0];
            mapY.value = _systemSettingDataModel.initialParty.startMap.position[1] * -1;
            mapX.RegisterCallback<FocusOutEvent>(evt =>
            {
                // 指定座標に配置可能か
                if (CanPutPos(_mapData.Find(map => map.id == _systemSettingDataModel.initialParty.startMap.mapId),
                    new Vector2Int(mapX.value,
                    _systemSettingDataModel.initialParty.startMap.position[1])) == true)
                {
                    _systemSettingDataModel.initialParty.startMap.position[0] = mapX.value;
                    Save();
                    MapEditor.MapEditor.LaunchCommonEventEditModeEnd(_mapData.Find(map =>
                        map.id == _systemSettingDataModel.initialParty.startMap.mapId));
                }
                else
                {
                    mapX.value = _systemSettingDataModel.initialParty.startMap.position[0];
                }
            });

            mapY.RegisterCallback<FocusOutEvent>(evt =>
            {
                // 指定座標に配置可能か
                if (CanPutPos(_mapData.Find(map => map.id == _systemSettingDataModel.initialParty.startMap.mapId),
                    new Vector2Int(_systemSettingDataModel.initialParty.startMap.position[0],
                    mapY.value * -1)) == true)
                {
                    _systemSettingDataModel.initialParty.startMap.position[1] = mapY.value * -1;
                    Save();
                    MapEditor.MapEditor.LaunchCommonEventEditModeEnd(_mapData.Find(map =>
                        map.id == _systemSettingDataModel.initialParty.startMap.mapId));
                }
                else
                { 
                    mapY.value = _systemSettingDataModel.initialParty.startMap.position[1] * -1;
                }
            });

            Button designationButton = RootContainer.Query<Button>("designation_button");

            var selectMapPopupField = GenericPopupFieldBase<MapDataChoice>.Add(
                RootContainer,
                "selectMap_dropdown",
                MapDataChoice.GenerateChoices(),
                _systemSettingDataModel.initialParty.startMap.mapId);

            // selectMapPopupFieldの選択値変更時の処理。
            selectMapPopupField.RegisterValueChangedCallback(
                changeEvent =>
                {
                    var selectedMapDataModel = changeEvent.newValue.MapDataModel;
                    if (selectedMapDataModel == null)
                    {
                        ChangedSelectMapPopupFieldValueProcess(changeEvent.newValue);
                    }
                    // マップに配置可能か
                    else if (CanPutPlayer(selectedMapDataModel))
                    {
                        // 現在の座標に配置可能か
                        if (!CanPutPos(
                                selectedMapDataModel,
                                new Vector2Int(_systemSettingDataModel.initialParty.startMap.position[0],
                                _systemSettingDataModel.initialParty.startMap.position[1] * -1)))
                        {
                            mapX.value = _initialPos.x;
                            mapY.value = _initialPos.y * -1;
                            _systemSettingDataModel.initialParty.startMap.position[0] = _initialPos.x;
                            _systemSettingDataModel.initialParty.startMap.position[1] = _initialPos.y;
                        }

                        ChangedSelectMapPopupFieldValueProcess(changeEvent.newValue);

                        MapEditor.MapEditor.LaunchCommonEventEditModeEnd(selectedMapDataModel);
                    }
                    else
                    {
                        // 配置不可時は元に戻す
                        selectMapPopupField.ForceSetValue(changeEvent.previousValue);
                    }
                });

            UpdateFromSelectMapPopupField();

            // 設定値変更時の処理。
            void ChangedSelectMapPopupFieldValueProcess(MapDataChoice newValue)
            {
                _systemSettingDataModel.initialParty.startMap.mapId = newValue.Id;
                Save();

                UpdateFromSelectMapPopupField();
            }

            void UpdateFromSelectMapPopupField()
            {
                designationButton.SetEnabled(selectMapPopupField.value.MapDataModel != null);
            }

            //マップ指定ボタンが押されたらセーブされる
            var isEdit = false;
            // 『設定開始』ボタン。
            designationButton.text = EditorLocalize.LocalizeText("WORD_1583");
            designationButton.clickable.clicked += () =>
            {
                if (isEdit)
                {
                    designationButton.text = EditorLocalize.LocalizeText("WORD_1583");
                    EndMapPosition(_systemSettingDataModel.initialParty.startMap.mapId);
                    selectMapPopupField.SetEnabled(true);
                }
                else
                {
                    designationButton.text = EditorLocalize.LocalizeText("WORD_1584");
                    SetMapPosition(_systemSettingDataModel.initialParty.startMap.mapId);
                    selectMapPopupField.SetEnabled(false);
                }

                isEdit = !isEdit;
            };

            //キャラクター選択
            for (var i = 0; i < _systemSettingDataModel.initialParty.partyMax; i++) _SetCharacter(RootContainer, i);
        }


        public void SetMapPosition(string mapId) {
            posX = -1;
            posY = -1;

            var mapDataModel = _mapData.Find(map =>
                map.id == mapId);
            MapEditor.MapEditor.LaunchCommonEventEditMode(mapDataModel, 0,
                v =>
                {
                    posX = v.x;
                    posY = v.y;
                    mapX.value = v.x;
                    mapY.value = v.y * -1;
                },
                _systemSettingDataModel.initialParty.startMap.mapId,
                true);
        }

        public void EndMapPosition(string mapId) {
            var mapDataModel = _mapData.Find(map =>
                map.id == mapId);
            MapEditor.MapEditor.LaunchCommonEventEditModeEnd(mapDataModel);

            if (posX == -1 && posY == -1)
                return;

            _systemSettingDataModel.initialParty.startMap.position[0] = posX;
            _systemSettingDataModel.initialParty.startMap.position[1] = posY;
            Save();
        }

        private void _SetCharacter(VisualElement root, int index) {
            var actors = _characterActorDataModels.FindAll(chara => chara.charaType == (int) ActorTypeEnum.ACTOR)
                .ToList();
            var ids = _systemSettingDataModel.initialParty.party;

            VisualElement dropdown = root.Query<VisualElement>("characterNameDropdown" + (index + 1));
            //職業の表示
            Label memberClass = root.Query<Label>("member_class" + (index + 1));
            //初期レベル
            Label memberLevel = root.Query<Label>("member_level" + (index + 1));

            VisualElement memberIcon = root.Query<VisualElement>("member_icon" + (index + 1));

            dropdown.Clear();

            var selectedIndex = 0;
            var choices = new List<string>();
            var choicesId = new List<string>();
            if (ids.Count > index)
                for (var i = 0; i < actors.Count; i++)
                    if (actors[i].uuId == ids[index])
                    {
                        selectedIndex = i;
                        break;
                    }

            var nonSelectedList = new List<string>();

            //キャラクターの全体数回す
            foreach (var actor in actors)
            {
                var partyIn = true;
                foreach (var partyMember in _systemSettingDataModel.initialParty.party)
                    //既にパーティに含まれているか
                    if (partyMember == actor.uuId)
                    {
                        partyIn = false;
                        break;
                    }

                //パーティの全体回す
                //ただし既にパーティに含まれているものは除く
                if (!partyIn) nonSelectedList.Add(actor.basic.name);

                choices.Add(actor.basic.name);
                choicesId.Add(actor.uuId);
            }

            var popupField = new PopupFieldBase<string>(choices, selectedIndex, null, null, 0, nonSelectedList);
            dropdown.Add(popupField);

            for (var i = 0; i < actors.Count; i++)
                // idが空の場合は先頭の値を入れる
                if (popupField.value == actors[i].basic.name ||
                    _systemSettingDataModel.initialParty.party[index] == "")
                {
                    _systemSettingDataModel.initialParty.party[index] = actors[i].uuId;
                    memberClass.text = _GetClass(actors[i].basic.classId);
                    memberLevel.text = actors[i].basic.initialLevel.ToString();
                    var tex = ImageManager.LoadSvCharacter(actors[i].image.character);
                    memberIcon.style.backgroundImage = tex;
                    if (tex != null)
                    {
                        memberIcon.style.width = tex.width / 2;
                        memberIcon.style.height = tex.height / 2;
                    }

                    break;
                }

            popupField.RegisterValueChangedCallback(evt =>
            {
                _systemSettingDataModel.initialParty.party[index] = choicesId[popupField.index];

                var actor = actors.Find(a => a.uuId == choicesId[popupField.index]);

                memberClass.text = _GetClass(actor.basic.classId);
                memberLevel.text = actor.basic.initialLevel.ToString();
                var tex = ImageManager.LoadFace(actor.image.face);
                if (tex != null)
                {
                    memberIcon.style.backgroundImage = tex;
                    memberIcon.style.width = tex.width / 2;
                    memberIcon.style.height = tex.height / 2;
                }


                //キャラクター選択プルダウン更新用
                Save();
                for (var i = 0; i < _systemSettingDataModel.initialParty.party.Count; i++) _SetCharacter(root, i);
            });
        }

        private string _GetClass(string id) {
            for (var i = 0; i < _classDataModels.Count; i++)
                if (_classDataModels[i].id == id)
                    return _classDataModels[i].basic.name;

            return "";
        }

        //セーブをする
        override protected void SaveContents() {
            databaseManagementService.SaveSystem(_systemSettingDataModel);
        }

        // 配置できるか
        private bool CanPutPlayer(MapDataModel mapDataModel) {
            // 配置物の取得
            var vehicles = databaseManagementService.LoadCharacterVehicles().FindAll(vehicle => vehicle.mapId == mapDataModel.id);
            var events = new EventManagementService().LoadEventMap();
            var mapEvents = events.FindAll(item => item.mapId == mapDataModel.id);

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
                        if (v.initialPos[0] == x && v.initialPos[1] * -1 == y)
                        {
                            found = true;
                            break;
                        }
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
            if (mapDataModel.width - 1 < pos.x || mapDataModel.height - 1 < pos.y * -1 || 0 > pos.x || 0 > pos.y * -1)
                return false;

            // 配置物の取得
            var vehicles = databaseManagementService.LoadCharacterVehicles().FindAll(vehicle => vehicle.mapId == mapDataModel.id);
            var events = new EventManagementService().LoadEventMap();
            var mapEvents = events.FindAll(item => item.mapId == mapDataModel.id);

            // 配置チェック
            foreach (var e in mapEvents)
                if (e.x == pos.x && e.y == pos.y)
                    return false;

            foreach (var v in vehicles)
                if (v.mapId == mapDataModel.id && v.initialPos[0] == pos.x && v.initialPos[1] == pos.y)
                    return false;

            return true;
        }
    }
}