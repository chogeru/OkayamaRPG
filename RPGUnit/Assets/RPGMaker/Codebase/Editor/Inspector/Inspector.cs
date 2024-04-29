using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Armor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventCommon;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Item;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCustom;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.State;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Weapon;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Character.View;
using RPGMaker.Codebase.Editor.Inspector.Animation.View;
using RPGMaker.Codebase.Editor.Inspector.Armor.View;
using RPGMaker.Codebase.Editor.Inspector.AssetManage.View;
using RPGMaker.Codebase.Editor.Inspector.BattleMenu.View;
using RPGMaker.Codebase.Editor.Inspector.BattleScene.View;
using RPGMaker.Codebase.Editor.Inspector.Character.View;
using RPGMaker.Codebase.Editor.Inspector.CharacterClass.View;
using RPGMaker.Codebase.Editor.Inspector.CharacterEarlyParty.View;
using RPGMaker.Codebase.Editor.Inspector.CharacterVehicle.View;
using RPGMaker.Codebase.Editor.Inspector.ClassCommon.View;
using RPGMaker.Codebase.Editor.Inspector.CommonEvent.View;
using RPGMaker.Codebase.Editor.Inspector.Encounter.View;
using RPGMaker.Codebase.Editor.Inspector.Enemy.View;
using RPGMaker.Codebase.Editor.Inspector.Environment.View;
using RPGMaker.Codebase.Editor.Inspector.GameMenu.View;
using RPGMaker.Codebase.Editor.Inspector.Item.View;
using RPGMaker.Codebase.Editor.Inspector.Map.View;
using RPGMaker.Codebase.Editor.Inspector.Option.View;
using RPGMaker.Codebase.Editor.Inspector.Outline.View;
using RPGMaker.Codebase.Editor.Inspector.SearchEvent.View;
using RPGMaker.Codebase.Editor.Inspector.SkillCommon.View;
using RPGMaker.Codebase.Editor.Inspector.SkillCustom.View;
using RPGMaker.Codebase.Editor.Inspector.Sound.View;
using RPGMaker.Codebase.Editor.Inspector.State.View;
using RPGMaker.Codebase.Editor.Inspector.Switch.View;
using RPGMaker.Codebase.Editor.Inspector.Talk.View;
using RPGMaker.Codebase.Editor.Inspector.Title.View;
using RPGMaker.Codebase.Editor.Inspector.Troop.View;
using RPGMaker.Codebase.Editor.Inspector.Type.View;
using RPGMaker.Codebase.Editor.Inspector.UIPattern.View;
using RPGMaker.Codebase.Editor.Inspector.Variable.View;
using RPGMaker.Codebase.Editor.Inspector.Weapon.View;
using RPGMaker.Codebase.Editor.Inspector.Word.View;
using RPGMaker.Codebase.Editor.MapEditor.Window.EventEdit;
using RPGMaker.Codebase.Editor.OutlineEditor.GTF;
using RPGMaker.Codebase.Editor.OutlineEditor.Window;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Display = RPGMaker.Codebase.Editor.Common.Enum.Display;

namespace RPGMaker.Codebase.Editor.Inspector
{
    public class InspectorParams : ScriptableSingleton<InspectorParams>
    {
        public int          displayIndex = (int) Display.None;
        public int          Number;
        public string       Type = "";
        public string       Uuid = "";
        public float        ScrollOffset = 0f;
    }

    /// <summary>
    ///     データベースエディター用インスペクターウィンドウ.
    /// </summary>
    public static class Inspector
    {
        private const string BaseUxml = "Assets/RPGMaker/Codebase/Editor/Inspector/inspector_base.uxml";
        private const string UssDark = "Assets/RPGMaker/Codebase/Editor/Inspector/inspectorDark.uss";
        private const string UssLight = "Assets/RPGMaker/Codebase/Editor/Inspector/inspectorLight.uss";

        // data properties
        //-----------------------------------------------------------------------

        // windows
        //-----------------------------------------------------------------------
        private static EditorWindow _inspectorWindow;
        

        // ui element
        //-----------------------------------------------------------------------
        private static AbstractInspectorElement _currentInspectorElement;
        private static InspectorParams _inspectorParams { get { return InspectorParams.instance; } }
        private static AbstractInspectorElement _nowInspectorElement;

        //-----------------------------------------------------------------------
        //
        // methods
        //
        //-----------------------------------------------------------------------
        // 初期化・更新系
        //-----------------------------------------------------------------------
        public static void Init() {
            _inspectorWindow = WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.DatabaseInspectorWindow);
            if (_inspectorWindow == null)
                throw new Exception("cannot instantiate inspector window.");

            _inspectorWindow.titleContent = new GUIContent(EditorLocalize.LocalizeText("WORD_1562"));

            //サイズ固定用
            var size = new Vector2(520f, _inspectorWindow.minSize.y);
            _inspectorWindow.minSize = size;
            _baseElement = null;
            Clear();
        }

        public static bool IsCached() {
            return _nowInspectorElement != null;
        }

        public static void ClearCached() {
            _nowInspectorElement = null;
        }

        public static void Clear(bool clearCache = false) {
            if (_nowInspectorElement is TroopInspectorElement)
            {
                ((TroopInspectorElement) _nowInspectorElement).ClearSceneWindow();
            }
            else if (_nowInspectorElement is AssetManageInspectorElement)
            {
                ((AssetManageInspectorElement) _nowInspectorElement).ClearSceneWindow();
            }

            _inspectorWindow.rootVisualElement.Clear();

            if (clearCache)
                _nowInspectorElement = null;
        }

        public static bool Refresh() {
            if (_currentInspectorElement == null)
                return false;

            _currentInspectorElement.Refresh();
            return true;
        }


        private static VisualElement _baseElement;
        
        private static void Render(AbstractInspectorElement inspectorElement) {
            //音楽の再生を行っていた場合止める
            var gameObject = GameObject.FindWithTag("sound");
            if (gameObject != null)
                gameObject.transform.gameObject.GetComponent<AudioSource>().Stop();

            if (_inspectorParams == null || _inspectorWindow == null) Init();
            _currentInspectorElement = inspectorElement;

            VisualElement baseUxml = AssetDatabase
                .LoadAssetAtPath<VisualTreeAsset>(BaseUxml)
                .CloneTree();
            
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssDark);
            if (!EditorGUIUtility.isProSkin)
            {
                styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(UssLight);
            }
            baseUxml.styleSheets.Clear();
            baseUxml.styleSheets.Add(styleSheet);
            _baseElement = baseUxml.Query<VisualElement>("base");
            _baseElement.Add(_currentInspectorElement);

            _inspectorWindow.rootVisualElement.Clear();
            _inspectorWindow.rootVisualElement.Add(baseUxml);

            // インスペクターがアウトラインエディターのチャプターまたはセクションの場合のみ、
            // アウトラインエディターウィンドウの セクション追加 ボタンを有効状態にする。
            foreach (var window in Resources.FindObjectsOfTypeAll<SceneWindow>())
            {
                var button = window.rootVisualElement.Q<Button>("add-section-button");
                if (button == null || inspectorElement == null)
                    continue;
                button.SetEnabled(
                    inspectorElement is InspectorViewForChapter ||
                    inspectorElement is InspectorViewForSection);
                button.userData = inspectorElement;
            }
        }

        // 引数のOeGraphViewを保持するウィンドウのセクション追加ボタンからカレントチャプターidを取得。
        public static string GetCurrentChapterId(OeGraphView graphView) {
            var window = Resources.FindObjectsOfTypeAll<SceneWindow>()
                .ForceSingleOrDefault(sw => sw.GraphView == graphView);
            return window != null
                ? GetCurrentChapterId(window.rootVisualElement.Q<Button>("add-section-button"))
                : null;
        }

        // セクション追加ボタンが保持するカレントチャプターidを取得。
        // (Inspectorに表示されているチャプターまたは表示されているセクションの属するチャプター)。
        public static string GetCurrentChapterId(Button button) {
            if (_nowInspectorElement == null) return null;
            
            switch (button.userData)
            {
                case InspectorViewForChapter chapterInspector:
                    return chapterInspector.ChapterDataModel.ID;
                case InspectorViewForSection sectionInspector:
                    return sectionInspector.SectionDataModel.ChapterID;
            }

            return null;
        }

        // 各種インスペクター起動メソッド
        //-----------------------------------------------------------------------
        // タイトル
        public static void TitleView(bool notSwitchDisplaySceneWindow = false) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.Title)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(false);
                _nowInspectorElement = new TitleInspectorElement(notSwitchDisplaySceneWindow);
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.Title;
            Render(_nowInspectorElement);
        }

        //オプション        
        public static void OptionView() {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.Option)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(false);
                _nowInspectorElement = new OptionInspectorElement();
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.Option;
            Render(_nowInspectorElement);
        }

        //ダメージ計算        
        public static void JobCommonView() {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.Job)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(false);
                _nowInspectorElement = new ClassCommonInspectorElement();
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.Job;
            Render(_nowInspectorElement);
        }

        //バトルメニュー
        public static void BattleMenuView() {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.BattleMenu)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(false);
                _nowInspectorElement = new UIBattleMenuInspectorElement();
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.BattleMenu;
            Render(_nowInspectorElement);
        }

        //ゲームメニュー
        public static void GameMenuView(int num) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.BattleMenu || _inspectorParams.Number != num)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.BattleMenu);
                _nowInspectorElement = new UIGameMenuInspectorElement(num);
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.GameMenu;
            _inspectorParams.Number = num;
            Render(_nowInspectorElement);
        }

        //用語
        public static void WordView(int num) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.Word || _inspectorParams.Number != num)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.Word);
                _nowInspectorElement = new WordInspectorElement(num);
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.Word;
            _inspectorParams.Number = num;
            Render(_nowInspectorElement);
        }

        public static void SoundView(string type, int num) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.Sound || _inspectorParams.Type != type || _inspectorParams.Number != num)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.Sound && _inspectorParams.Type != type);
                _nowInspectorElement = new SoundInspectorElement(type, num);
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.Sound;
            _inspectorParams.Type = type;
            _inspectorParams.Number = num;
            Render(_nowInspectorElement);
        }

        public static void CharacterView(int type, string uuid, CharacterHierarchyView element) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.Character || _inspectorParams.Uuid != uuid || _inspectorParams.Number != type)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.Character && _inspectorParams.Number == type);
                _nowInspectorElement = new CharacterInspectorElement(type, uuid, element);
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.Character;
            _inspectorParams.Number = type;
            _inspectorParams.Uuid = uuid;
            Render(_nowInspectorElement);
        }

        //キャラクター設定        
        public static void CharacterEarlyPartyView() {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.CharacterEarlyParty)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(false);
                _nowInspectorElement = new CharacterEarlyPartyInspectorElement();
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.CharacterEarlyParty;
            Render(_nowInspectorElement);
        }

        //乗り物部分
        public static void VehiclesView(string uuid) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.CharacterVehicles || _inspectorParams.Uuid != uuid)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.CharacterVehicles);
                _nowInspectorElement = new CharacterVehiclesInspectorElement(uuid);
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.CharacterVehicles;
            _inspectorParams.Uuid = uuid;
            Render(_nowInspectorElement);
        }

        //職業部分
        public static void ClassView(string uuid, CharacterHierarchyView element) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.CharacterClass || _inspectorParams.Uuid != uuid)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.CharacterClass);
                _nowInspectorElement = new CharacterClassInspectorElement(uuid);
            }

            _inspectorParams.displayIndex = (int) Display.CharacterClass;
            _inspectorParams.Uuid = uuid;

            Render(_nowInspectorElement);
        }

        public static void SkillCommonView() {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.SkillCommon)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(false);
                _nowInspectorElement = new SkillCommonInspectorElement();
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.SkillCommon;
            Render(_nowInspectorElement);
        }

        public static void SkillCustomView(SkillCustomDataModel skillCustomDataModel) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.SkillCustom || _inspectorParams.Uuid != skillCustomDataModel.basic.id)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.SkillCustom);
                _nowInspectorElement = new SkillCustomInspectorElement(skillCustomDataModel);
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.SkillCustom;
            _inspectorParams.Uuid = skillCustomDataModel.basic.id;

            Render(_nowInspectorElement);
        }

        public static void CharacterEnemyView(string id, BattleHierarchyView element) {
            _nowInspectorElement = null;

            _inspectorParams.displayIndex = (int) Display.Enemy;
            _inspectorParams.Uuid = id;
            Render(new EnemyInspectorElement(id, element));
        }

        public static void BattleSceneView() {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.BattleScene)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(false);
                _nowInspectorElement = new BattleSceneInspectorElement();
            }
            else
                return;

            _nowInspectorElement.Initialize();
            _inspectorParams.displayIndex = (int) Display.BattleScene;
            Render(_nowInspectorElement);
        }

        public static void TroopSceneView(string id, BattleHierarchyView element, int eventNum = -1) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.Troop || _inspectorParams.Uuid != id || _inspectorParams.Number != eventNum)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.Troop);
                _nowInspectorElement = new TroopInspectorElement(id, element, eventNum);
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.Troop;
            _inspectorParams.Uuid = id;
            _inspectorParams.Number = eventNum;
            Render(_nowInspectorElement);
        }

        public static void EncounterSceneView(string mapId, int regionId, BattleHierarchyView element) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.Encounter || _inspectorParams.Uuid != mapId || _inspectorParams.Number != regionId)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.Encounter);
                _nowInspectorElement = new EncounterInspectorElement(mapId, regionId);
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.Encounter;
            _inspectorParams.Uuid = mapId;
            _inspectorParams.Number = regionId;
            Render(_nowInspectorElement);
        }

        public static void StateEditView(StateDataModel stateDataModel) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.StateEdit || _inspectorParams.Uuid != stateDataModel.id)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.StateEdit);
                _nowInspectorElement = new StateEditInspectorElement(stateDataModel);
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.StateEdit;
            _inspectorParams.Uuid = stateDataModel.id;
            Render(_nowInspectorElement);
        }

        public static void WeaponEditView(WeaponDataModel weaponDataModel) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.Weapon || _inspectorParams.Uuid != weaponDataModel.basic.id)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.Weapon);
                _nowInspectorElement = new WeaponInspectorElement(weaponDataModel);
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.Weapon;
            _inspectorParams.Uuid = weaponDataModel.basic.id;
            Render(_nowInspectorElement);
        }

        public static void ArmorEditView(ArmorDataModel armorDataModel) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.Armor || _inspectorParams.Uuid != armorDataModel.basic.id)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.Armor);
                _nowInspectorElement = new ArmorInspectorElement(armorDataModel);
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.Armor;
            _inspectorParams.Uuid = armorDataModel.basic.id;
            Render(_nowInspectorElement);
        }

        public static void UiCommonEditView() {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.UiCommon)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(false);
                _nowInspectorElement = new UiCommonInspectorElement();
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.UiCommon;
            Render(_nowInspectorElement);
        }

        public static void UiTalkEditView(int num) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.UiTalk || _inspectorParams.Number != num)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.UiTalk);
                _nowInspectorElement = new UiTalkInspectorElement(num);
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.UiTalk;
            _inspectorParams.Number = num;
            Render(_nowInspectorElement);
        }

        public static void ItemEditView(ItemDataModel itemDataModel) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.Item || _inspectorParams.Uuid != itemDataModel.basic.id)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.Item);
                _nowInspectorElement = new ItemEditInspectorElement(itemDataModel);
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.Item;
            _inspectorParams.Uuid = itemDataModel.basic.id;
            Render(_nowInspectorElement);
        }

        public static void AttributeTypeEditView(SystemSettingDataModel.Element element) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.AttributeTypeEdit || _inspectorParams.Uuid != element.id)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.AttributeTypeEdit);
                _nowInspectorElement = new AttributeTypeEditInspectorElement(element);
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.AttributeTypeEdit;
            _inspectorParams.Uuid = element.id;
            Render(_nowInspectorElement);
        }

        public static void SkillTypeEditView(SystemSettingDataModel.SkillType skillType) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.SkillTypeEdit || _inspectorParams.Uuid != skillType.id)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.SkillTypeEdit);
                _nowInspectorElement = new SkillTypeEditInspectorElement(skillType);
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.SkillTypeEdit;
            _inspectorParams.Uuid = skillType.id;
            Render(_nowInspectorElement);
        }

        public static void WeaponTypeEditView(SystemSettingDataModel.WeaponType weaponType) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.WeaponTypeEdit || _inspectorParams.Uuid != weaponType.id)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.WeaponTypeEdit);
                _nowInspectorElement = new WeaponTypeEditInspectorElement(weaponType);
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.WeaponTypeEdit;
            _inspectorParams.Uuid = weaponType.id;
            Render(_nowInspectorElement);
        }

        public static void ArmorTypeEditView(SystemSettingDataModel.ArmorType armorType) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.ArmorTypeEdit || _inspectorParams.Uuid != armorType.id)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.ArmorTypeEdit);
                _nowInspectorElement = new ArmorTypeEditInspectorElement(armorType);
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.ArmorTypeEdit;
            _inspectorParams.Uuid = armorType.id;
            Render(_nowInspectorElement);
        }

        public static void EquipmentTypeEditView(SystemSettingDataModel.EquipType equipType) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.EquipmentTypeEdit || _inspectorParams.Uuid != equipType.id)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.EquipmentTypeEdit);
                _nowInspectorElement = new EquipmentTypeEditInspectorElement(equipType);
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.EquipmentTypeEdit;
            _inspectorParams.Uuid = equipType.id;
            Render(_nowInspectorElement);
        }

        public static void AnimEditView(int id) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.Animation || _inspectorParams.Number != id)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.Animation);
                _nowInspectorElement = new AnimationInspectorElement(id);
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.SkillTypeEdit;
            _inspectorParams.Number = id;
            Render(_nowInspectorElement);
        }

        // コモンイベント用のインスペクタ
        public static void CommonEventEditView(EventCommonDataModel eventCommonDataModel) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.CommonEvent || _inspectorParams.Uuid != eventCommonDataModel.eventId)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.CommonEvent);
                _nowInspectorElement = new CommonEventInspectorElement(eventCommonDataModel);
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.CommonEvent;
            _inspectorParams.Uuid = eventCommonDataModel.eventId;
            Render(_nowInspectorElement);
        }

        // アウトライン
        public static void OutlineView(IOutlineDataModel dataModel) {
            switch (dataModel)
            {
                case StartDataModel:
                    break;
                case ChapterDataModel:
                    _nowInspectorElement = null;
                    break;
                case SectionDataModel:
                    _nowInspectorElement = null;
                    break;
                default:
                    throw new Exception();
            }

            _inspectorParams.displayIndex = (int) Display.Outline;
            switch (dataModel)
            {
                case StartDataModel _:
                    TitleView(true);
                    break;
                case ChapterDataModel chapter:
                    _inspectorParams.Uuid = chapter.ID;
                    _inspectorParams.Number = 0;
                    _nowInspectorElement = new InspectorViewForChapter(chapter);
                    Render(_nowInspectorElement);
                    break;
                case SectionDataModel section:
                    _inspectorParams.Uuid = section.ID;
                    _inspectorParams.Number = 1;
                    _nowInspectorElement = new InspectorViewForSection(section);
                    Render(_nowInspectorElement);
                    break;
                default:
                    throw new Exception();
            }
        }

        // 素材管理用インスペクタ
        public static void AssetManageEditView(AssetManageDataModel assetManageDataModel) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.AssetManage || _inspectorParams.Uuid != assetManageDataModel.id)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.AssetManage);
                _nowInspectorElement = new AssetManageInspectorElement(assetManageDataModel);
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.AssetManage;
            _inspectorParams.Uuid = assetManageDataModel.id;

            Render(_nowInspectorElement);
        }

        public static void EnvironmentEditView() {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.EnvironmentEdit)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(false);
                _nowInspectorElement = new EnvironmentInspectorElement();
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.EnvironmentEdit;
            Render(_nowInspectorElement);
        }

        public static void SwitchEditView(FlagDataModel.Switch sw) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.SwitchEdit || _inspectorParams.Uuid != sw.id)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.SwitchEdit);
                _nowInspectorElement = new SwitchEditInspectorElement(sw);
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.SwitchEdit;
            _inspectorParams.Uuid = sw.id;
            Render(_nowInspectorElement);
        }

        public static void VariableEditView(FlagDataModel.Variable variable) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.SwitchEdit || _inspectorParams.Uuid != variable.id)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.SwitchEdit);
                _nowInspectorElement = new VariableEditInspectorElement(variable);
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.VariableEdit;
            _inspectorParams.Uuid = variable.id;
            Render(_nowInspectorElement);
        }

        // タイルグループ
        public static void TileGroupEditView(TileGroupDataModel tileGroupDataModel) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.TileGroup || _inspectorParams.Uuid != tileGroupDataModel.id)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.TileGroup);
                _nowInspectorElement = new TileGroupInspector(tileGroupDataModel);
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.TileGroup;
            _inspectorParams.Uuid = tileGroupDataModel.id;
            Render(_nowInspectorElement);
        }

        // マップ
        public static void MapView(MapDataModel mapDataModel, bool isSampleMap) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.MapEdit || _inspectorParams.Uuid != mapDataModel.id)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.MapEdit);

                var mapInspectorView = new MapInspectorView();
                mapInspectorView.SetMapEntity(mapDataModel, isSampleMap);
                _nowInspectorElement = mapInspectorView;
            }
            else
                return;

            if (isSampleMap)
                _inspectorParams.displayIndex = (int) Display.MapPreview;
            else
                _inspectorParams.displayIndex = (int) Display.MapEdit;
            _inspectorParams.Uuid = mapDataModel.id;

            Render(_nowInspectorElement);
        }

        // 遠景
        public static void MapDistantView(MapDataModel mapEntity) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.MapDistant || _inspectorParams.Uuid != mapEntity.id)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.MapDistant);

                var mapInspectorView = new MapInspectorView();
                mapInspectorView.SetDistantView(mapEntity);
                _nowInspectorElement = mapInspectorView;
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.MapDistant;
            _inspectorParams.Uuid = mapEntity.id;

            Render(_nowInspectorElement);
        }

        // 背景
        public static void MapBackgroundView(MapDataModel mapEntity) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.MapBackground || _inspectorParams.Uuid != mapEntity.id)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.MapBackground);

                var mapInspectorView = new MapInspectorView();
                mapInspectorView.SetBackgroundView(mapEntity);
                _nowInspectorElement = mapInspectorView;
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.MapBackground;
            _inspectorParams.Uuid = mapEntity.id;

            Render(_nowInspectorElement);
        }

        public static void MapBackgroundCollisionView(TileDataModel tileDataModel) {
            if (_nowInspectorElement != null)
            {
                _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.MapBackgroundCol);
            }

            _inspectorParams.displayIndex = (int) Display.MapBackgroundCol;
            var mapInspectorView = new MapInspectorView();
            if (tileDataModel != null)
            {
                _inspectorParams.Uuid = tileDataModel.id;
                mapInspectorView.SetBackgroundCollisionView(tileDataModel);
            }
            _nowInspectorElement = mapInspectorView;

            Render(_nowInspectorElement);
        }

        public static void MapTileView(
            TileDataModel tileDataModel,
            TileInspector.TYPE inspectorType = TileInspector.TYPE.NORMAL
        ) {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.MapTile || _inspectorParams.Uuid != tileDataModel.id)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.MapTile);

                var mapInspectorView = new MapInspectorView();
                mapInspectorView.SetTileEntity(tileDataModel, inspectorType);
                _nowInspectorElement = mapInspectorView;
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.MapTile;
            _inspectorParams.Uuid = tileDataModel.id;

            Render(_nowInspectorElement);
        }

        public static void MapEventView(
            EventMapDataModel eventMapDataModelList,
            FlagDataModel flags,
            List<ItemDataModel> items,
            List<WeaponDataModel> weapons,
            List<ArmorDataModel> armors,
            List<CharacterActorDataModel> actors,
            int pageNum,
            EventEditWindow element,
            MapDataModel mapDataModel
        )
        {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.MapEvent || _inspectorParams.Uuid != eventMapDataModelList.mapId || _inspectorParams.Type != eventMapDataModelList.eventId || _inspectorParams.Number != pageNum)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(_inspectorParams.displayIndex == (int) Display.MapEvent);

                var mapInspectorView = new MapInspectorView();
                mapInspectorView.SetEventEntity(eventMapDataModelList, flags, items, weapons, armors, actors, pageNum, element, mapDataModel);
                _nowInspectorElement = mapInspectorView;
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.MapEvent;
            _inspectorParams.Uuid = eventMapDataModelList.mapId;
            _inspectorParams.Type = eventMapDataModelList.eventId;
            _inspectorParams.Number = pageNum;

            Render(_nowInspectorElement);
        }

        public static PenButtonMenu GetPenButtonMenu() {
            return (_nowInspectorElement as MapInspectorView)?.GetPenButtonMenu();
        }

        public static void SearchEventView() {
            if (_nowInspectorElement == null || _inspectorParams.displayIndex != (int) Display.Search)
            {
                if (_nowInspectorElement != null)
                    _nowInspectorElement.SaveScroll(false);
                _nowInspectorElement = new SearchEventInspector();
            }
            else
                return;

            _inspectorParams.displayIndex = (int) Display.Search;
            Render(_nowInspectorElement);
        }
    }
}