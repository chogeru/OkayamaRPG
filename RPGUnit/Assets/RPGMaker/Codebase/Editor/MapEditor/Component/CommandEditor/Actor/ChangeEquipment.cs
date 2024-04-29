using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Armor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Class;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Weapon;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Actor
{
    public class ChangeEquipment : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_change_equipment.uxml";

        private string _nowActorId       = "";
        private string _nowEquipmentId   = "";
        private int    _nowEquipmentType = -1;

        public ChangeEquipment(
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

            //各種データ読込
            var characterActorDataModels = DatabaseManagementService.LoadCharacterActor().FindAll(actor => actor.charaType == (int) ActorTypeEnum.ACTOR);
            var systemSettingDataModel = DatabaseManagementService.LoadSystem();

            //装備タイプ = 武器、盾、頭、体、その他ユーザーが設定したリスト
            var equipTypes = DatabaseManagementService.LoadSystem().equipTypes;

            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            //初期値設定
            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Count == 0)
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters
                    .Add(characterActorDataModels[0].uuId);
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add(equipTypes[0].id);
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("-1");
                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            //リスト表示する内容生成
            VisualElement actor = RootElement.Query<VisualElement>("actor");
            var characterActorNameList = EditorLocalize.LocalizeTexts(new List<string>());
            var characterActorIDList = EditorLocalize.LocalizeTexts(new List<string>());

            //アクターのリスト生成
            for (var i = 0; i < characterActorDataModels.Count; i++)
            {
                if(characterActorDataModels[i].charaType == (int)ActorTypeEnum.NPC) continue;
                characterActorNameList.Add(characterActorDataModels[i].basic.name);
                characterActorIDList.Add(characterActorDataModels[i].uuId);
            }

            //初期index
            _nowActorId = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0];
            var selectID = characterActorIDList.IndexOf(_nowActorId);
            if (selectID < 0) selectID = 0;

            //アクターのPU生成
            var actorPopupField = new PopupFieldBase<string>(characterActorNameList, selectID);

            //装備タイプ
            VisualElement equipmentType = RootElement.Query<VisualElement>("equipmentType");
            var equipmentNameList = EditorLocalize.LocalizeTexts(new List<string>());
            var equipmentIDList = EditorLocalize.LocalizeTexts(new List<string>());

            //装備タイプのリスト生成
            for (var i = 0; i < equipTypes.Count; i++)
            {
                equipmentNameList.Add(equipTypes[i].name);
                equipmentIDList.Add(equipTypes[i].id);
            }

            //初期index
            _nowEquipmentType =
                equipmentIDList.IndexOf(EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1]);
            if (_nowEquipmentType < 0) _nowEquipmentType = 0;

            //装備タイプのPU生成
            var equipmentPopupField = new PopupFieldBase<string>(equipmentNameList, _nowEquipmentType);
            equipmentType.Clear();
            equipmentType.Add(equipmentPopupField);
            equipmentPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                    equipmentIDList[equipmentPopupField.index];
                Save(EventDataModels[EventIndex]);
                //装備一覧生成しなおし
                SetItemList(_nowActorId, equipmentPopupField.index, _nowEquipmentId);
            });

            //装備一覧生成
            _nowEquipmentId = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2];
            SetItemList(_nowActorId, _nowEquipmentType, _nowEquipmentId, true);
            
            //アクターのコールバック設定
            actor.Clear();
            actor.Add(actorPopupField);
            actorPopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] =
                    characterActorIDList[actorPopupField.index];
                //装備タイプのプルダウンを先頭に変更
                equipmentPopupField.ChangeButtonText(0);
                equipmentPopupField.index = 0;
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = equipmentIDList[0];
                //装備を「なし」に
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] = "-1";
                //現在の装備タイプ、装備を初期化
                _nowEquipmentType = 0;
                _nowEquipmentId = "-1";
                Save(EventDataModels[EventIndex]);
                //装備一覧生成しなおし
                SetItemList(characterActorIDList[actorPopupField.index], _nowEquipmentType, _nowEquipmentId, true);
            });

        }

        private void SetItemList(string actorId, int equipmentIndex, string equipId, bool flg = false) {
            if (_nowActorId == actorId && _nowEquipmentType == equipmentIndex && !flg) return;
            _nowActorId = actorId;

            //装備タイプ = 武器、盾、頭、体、その他ユーザーが設定したリスト
            var equipTypes = DatabaseManagementService.LoadSystem().equipTypes;
            //武器リスト
            var weaponTypes = DatabaseManagementService.LoadSystem().weaponTypes;
            //防具リスト
            var armorTypes = DatabaseManagementService.LoadSystem().armorTypes;
            //職業リスト
            var classData = DatabaseManagementService.LoadClassCommon();

            //装備を表示する領域
            VisualElement equipment = RootElement.Query<VisualElement>("equipment");

            //アクターの職業取得
            var actorData = DatabaseManagementService.LoadCharacterActor();
            CharacterActorDataModel actor = null;
            ClassDataModel actorClass = null;
            foreach (var actorWork in actorData)
                if (actorWork.uuId == _nowActorId)
                {
                    actor = actorWork;
                    break;
                }

            foreach (var classWork in classData)
                if (classWork.id == actor.basic.classId)
                {
                    actorClass = classWork;
                    break;
                }

            //装備タイプが武器なのか、防具なのかによって処理を振り分け
            _nowEquipmentType = equipmentIndex;
            var names = new List<string>();
            var ids = new List<string>();

            //最初に「なし」を入れる
            names.Add(EditorLocalize.LocalizeText("WORD_0113"));
            ids.Add("-1");

            var index = 0;
            if (_nowEquipmentType == 0)
            {
                //武器
                var wList = DatabaseManagementService.LoadWeapon();
                var weaponList = new List<WeaponDataModel>();

                //現在選択中のアクターの、職業が装備可能な武器を追加する
                foreach (var cw in actorClass.weaponTypes)
                foreach (var w in wList)
                    if (w.basic.weaponTypeId == cw)
                    {
                        weaponList.Add(w);
                        names.Add(w.basic.name);
                        ids.Add(w.basic.id);
                        if (equipId == w.basic.id) index = names.Count - 1;
                    }
            }
            else
            {
                //防具
                var aList = DatabaseManagementService.LoadArmor();
                var armorList = new List<ArmorDataModel>();

                foreach (var ca in actorClass.armorTypes)
                foreach (var l in aList)
                    if (l.basic.armorTypeId == ca)
                        if (l.basic.equipmentTypeId == equipTypes[_nowEquipmentType].id)
                        {
                            armorList.Add(l);
                            names.Add(l.basic.name);
                            ids.Add(l.basic.id);
                            if (equipId == l.basic.id) index = names.Count - 1;
                        }
            }

            if (index == -1) index = 0;

            //ポップアップを表示
            var equipmentTypePopupField = new PopupFieldBase<string>(names, index);
            equipment.Clear();
            equipment.Add(equipmentTypePopupField);
            equipmentTypePopupField.RegisterValueChangedCallback(evt =>
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] =
                    ids[equipmentTypePopupField.index];
                Save(EventDataModels[EventIndex]);
            });
        }
    }
}