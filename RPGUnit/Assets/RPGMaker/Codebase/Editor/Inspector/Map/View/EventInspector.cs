using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Armor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Item;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Weapon;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using RPGMaker.Codebase.Editor.MapEditor.Window.EventEdit;
using RPGMaker.Codebase.Runtime.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;
using UnityEngine.UIElements;
using EnabledType = RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap.EventMapDataModel.EventMapPageConditionImage.EnabledType;

namespace RPGMaker.Codebase.Editor.Inspector.Map.View
{
    /// <summary>
    /// [マップ設定]-[マップリスト]-[各マップ]-[イベント] Inspector
    /// </summary>
    public class EventInspector : AbstractInspectorElement
    {
        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/Map/Asset/inspector_mapEvent_event.uxml"; } }

        private List<string> _actorDropdownChoices;
        private List<string> _actorNameDropdownChoices;
        private readonly List<CharacterActorDataModel> _actors;
        private List<string> _autoMoveTextDropdownChoices;
        private PopupFieldBase<string> _chapterPopupField;
        private List<EventMoveEnum> _codeList = new List<EventMoveEnum>();

        // UI要素プロパティ
        private List<string> _directionTextDropdownChoices;
        private readonly EventEditWindow _element;
        private VisualElement _eventActorContainer;
        private PopupFieldBase<string> _eventActorPopupField;
        private Toggle _eventActorToggle;
        private VisualElement _eventAutoMoveAnimationContainer;
        private PopupFieldBase<string> _eventAutoMoveAnimationPopupField;
        private VisualElement _eventAutoMoveContainer;
        private VisualElement _eventAutoMoveDirectionContainer;
        private Toggle _eventAutoMoveDirectionFix;
        private PopupFieldBase<string> _eventAutoMoveDirectionPopupField;
        private VisualElement _eventAutoMovePassingContainer;
        private PopupFieldBase<string> _eventAutoMovePassingPopupField;
        private PopupFieldBase<string> _eventAutoMovePopupField;
        private VisualElement _eventAutoMoveRateContainer;
        private PopupFieldBase<string> _eventAutoMoveRatePopupField;
        private VisualElement _eventAutoMoveSpeedContainer;
        private PopupFieldBase<string> _eventAutoMoveSpeedPopupField;
        private VisualElement _eventAutoMoveStepAnimationContainer;
        private PopupFieldBase<string> _eventAutoMoveStepAnimationPopupField;
        private VisualElement _eventCharacterImagePopupFieldContainer;
        private PopupFieldBase<string> _eventCharacterImagePopupField;
        private Toggle _eventCharacterImageToggle;
        private VisualElement _eventCharacterImageArea;
        private Button _eventExportButton;
        private VisualElement _eventCharacterImageContainer;
        private VisualElement _eventCharacterImage;
        private Label _eventIdText;
        private Toggle _eventSelectImageToggle;
        private VisualElement _eventSelectImageArea;
        private Button _eventImportButton;
        private VisualElement _eventItemContainer;
        private PopupFieldBase<string> _eventItemPopupField;
        private Toggle _eventItemToggle;

        private readonly EventManagementService _eventManagementService;
        private EventMapDataModel _eventMapDataModel;
        private MapDataModel _mapDataModel;

        // データプロパティ
        private List<EventMapDataModel> _eventMapDataModels;
        private IntegerField _eventMapX;
        private IntegerField _eventMapY;
        private ImTextField _eventMemoText;
        private ImTextField _eventNameText;
        private Label _eventPictureName;
        private VisualElement _eventPriorityContainer;
        private PopupFieldBase<string> _eventPriorityPopupField;
        private VisualElement _eventSdImageContainer;
        private VisualElement _eventSdImage;
        private VisualElement _eventSelfSwitch;
        private PopupFieldBase<string> _eventSelfSwitchPopupField;
        private Toggle _eventSelfToggle;
        private VisualElement _eventSwitch1Container;
        private Toggle _eventSwitch1Toggle;
        private VisualElement _eventSwitch2Container;
        private Toggle _eventSwitch2Toggle;
        private VisualElement _eventSwitchItem;
        private PopupFieldBase<string> _eventSwitchItemPopupField;
        private Toggle _eventSwitchItemToggle;
        private PopupFieldBase<string> _eventSwitchPopupField1;
        private PopupFieldBase<string> _eventSwitchPopupField2;
        private VisualElement _eventTriggerContainer;
        private PopupFieldBase<string> _eventTriggerPopupField;
        private VisualElement _eventVariableContainer;
        private IntegerField _eventVariableInteger;
        private PopupFieldBase<string> _eventVariablePopupField;
        private Toggle _eventVariableToggle;
        private readonly FlagDataModel _flags;
        private List<string> _itemDropdownChoices;
        private List<string> _itemNameDropdownChoices;
        private readonly List<ItemDataModel> _items;
        private readonly List<WeaponDataModel> _weapons;
        private readonly List<ArmorDataModel> _armors;
        private List<string> _moveAnimationTextDropdownChoices;
        private List<string> _moveRateTextDropdownChoices;
        private Toggle _moveSkipToggle;
        private List<string> _moveSpeedTextDropdownChoices;
        private Vector2 _nowPos = Vector2.one;
        private List<string> _passingTextDropdownChoices;
        private Button _previewButton;
        private List<string> _priorityTextDropdownChoices;
        private Toggle _repeatOperationToggle;
        private Button _routeInitButton;
        private Button _routeSettingButton;
        private PopupFieldBase<string> _sectionPopupField;

        // dictionary
        private Dictionary<SelfSwitchEnum, string> _selfSwitchDictionary;
        private List<string> _stepAnimationTextDropdownChoices;
        private List<string> _switchDropdownChoices;
        private List<string> _switchItemDropdownChoices;
        private List<string> _switchItemNameDropdownChoices;
        private List<string> _switchNameDropdownChoices;
        private readonly EventMapDataModel.EventMapPage _targetEventMapPage;
        private List<string> _triggerTextDropdownChoices;
        private List<string> _variableDropdownChoices;
        private List<string> _variableNameDropdownChoices;

        /**
         * コンストラクタ
         */
        public EventInspector(
            EventMapDataModel eventMapDataModel,
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
            _eventManagementService = new EventManagementService();

            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();

            _eventMapDataModels = _eventManagementService.LoadEventMap();

            _eventMapDataModel = eventMapDataModel;
            _targetEventMapPage = eventMapDataModel.pages.Single(eventMapPage => eventMapPage.page == pageNum);
            _mapDataModel = mapDataModel;

            _flags = flags;
            _items = items;
            _weapons = weapons;
            _armors = armors;
            _actors = actors;
            _element = element;

            InitChangeDirection();
            InitDictionaries();
            Initialize();
        }

        protected override void RefreshContents() {
            base.RefreshContents();
            InitChangeDirection();
            InitDictionaries();
            Initialize();
        }

        private void InitChangeDirection() {
            // 元々 directionFix に、移動方向、プレイヤー、右固定、左固定、下固定、上固定 というデータが入っている
            // 最新では directionFix には 固定かどうか しか設定しないため、2以上のデータの場合には、変換処理を行う
            if (_targetEventMapPage.walk.directionFix >= 2)
            {
                // 指定されていた向きを direction に設定
                _targetEventMapPage.walk.direction = _targetEventMapPage.walk.directionFix;
                // 固定
                _targetEventMapPage.walk.directionFix = 1;
            }
            // direction が -1 の場合は、初期化されていない = 古いデータとみなし、変換処理を行う
            if (_targetEventMapPage.walk.direction == -1)
            {
                // 指定されていた向きを direction に設定
                _targetEventMapPage.walk.direction = _targetEventMapPage.walk.directionFix;
                // ここに来るケースは、向きが 移動方向又はプレイヤーの場合で、向きは固定にできないため、0を指定
                _targetEventMapPage.walk.directionFix = 0;
            }
        }

        /// <summary>
        ///     マップイベントページ情報によりマップタイルに表示するテクスチャーを取得する。
        /// </summary>
        /// <param name="eventMapPage">マップイペントページ情報</param>
        /// <returns>テクスチャー</returns>
        public static Texture2D LoadMapTileTexture(EventMapDataModel.EventMapPage eventMapPage) {
            switch (eventMapPage.condition.image.Enabled)
            {
                case EnabledType.Character:
                {
                    var actor = Editor.Hierarchy.Hierarchy.databaseManagementService.LoadCharacterActor();
                    string assetId = "";
                    for (int i = 0; i < actor.Count; i++)
                        if (actor[i].uuId == eventMapPage.condition.image.imageName)
                        {
                            assetId = actor[i].image.character;
                            break;
                        }
                    return assetId != null ? ImageManager.LoadSvCharacter(assetId) : null;
                }

                case EnabledType.SelectedImage:
                    return LoadSdCharacterTexture(eventMapPage);
            }

            return null;
        }

        public static Texture2D LoadSdCharacterTexture(EventMapDataModel.EventMapPage eventMapPage) {
            Texture2D imageCharacter;
            {
                var sdName = eventMapPage.image.sdName;
                if (string.IsNullOrEmpty(sdName)) return null;

                imageCharacter = ImageManager.LoadSvCharacter(sdName);
            }

            return imageCharacter;
        }

        /**
         * dictionaryデータ初期化
         */
        private void InitDictionaries() {
            _selfSwitchDictionary = new Dictionary<SelfSwitchEnum, string>
            {
                {SelfSwitchEnum.A, "A"},
                {SelfSwitchEnum.B, "B"},
                {SelfSwitchEnum.C, "C"},
                {SelfSwitchEnum.D, "D"}
            };
            _switchDropdownChoices = _flags.switches.Select(sw => sw.id).ToList();
            _switchNameDropdownChoices = new List<string>();
            for (var i = 0; i < _flags.switches.Count; i++) _switchNameDropdownChoices.Add(_flags.switches[i].name);

            
            var noName = EditorLocalize.LocalizeText("WORD_1518");
            
            _variableDropdownChoices = _flags.variables.Select(v => v.id).ToList();
            _variableNameDropdownChoices = new List<string>();
            for (var i = 0; i < _flags.variables.Count; i++)
            {
                var name = "";
                name = _flags.variables[i].name == "" ? noName : _flags.variables[i].name;
                _variableNameDropdownChoices.Add(name);
            }

            _switchItemDropdownChoices = new List<string>();
            _switchItemNameDropdownChoices = new List<string>();
            for (var i = 0; i < _items.Count; i++)
                if (_items[i].basic.switchItem == 0)
                {
                    _switchItemDropdownChoices.Add(_items[i].basic.id);
                    _switchItemNameDropdownChoices.Add(_items[i].basic.name);
                }
            for (var i = 0; i < _weapons.Count; i++)
                if (_weapons[i].basic.switchItem == 0)
                {
                    _switchItemDropdownChoices.Add(_weapons[i].basic.id);
                    _switchItemNameDropdownChoices.Add(_weapons[i].basic.name);
                }
            for (var i = 0; i < _armors.Count; i++)
                if (_armors[i].basic.switchItem == 0)
                {
                    _switchItemDropdownChoices.Add(_armors[i].basic.id);
                    _switchItemNameDropdownChoices.Add(_armors[i].basic.name);
                }


            if (_switchItemDropdownChoices.Count == 0)
            {
                _switchItemDropdownChoices.Add(EditorLocalize.LocalizeText("WORD_0113"));
                _switchItemNameDropdownChoices.Add(EditorLocalize.LocalizeText("WORD_0113"));
            }


            _itemDropdownChoices = _items.Select(item => item.basic.id).ToList();
            _itemNameDropdownChoices = _items.Select(item => item.basic.name).ToList();
            _actorDropdownChoices = _actors.Select(actor => actor.uuId).ToList();
            _actorNameDropdownChoices = _actors.Select(actor => actor.basic.name).ToList();

            _autoMoveTextDropdownChoices =
                EditorLocalize.LocalizeTexts(new List<string> {"WORD_0844", "WORD_0447", "WORD_0845", "WORD_0846"});
            // "1.1/8倍速", "2.1/4倍速", "3.1/2倍速", "4.標準", "5.2倍速", "6.4倍速"
            _moveSpeedTextDropdownChoices =
                EditorLocalize.LocalizeTexts(new List<string>
                    {"WORD_0847", "WORD_0848", "WORD_0849", "WORD_0850", "WORD_0851", "WORD_0852"});

            // 移動方向、プレイヤー、右、左、下、上、ダメージ
            _directionTextDropdownChoices =
                EditorLocalize.LocalizeTexts(new List<string>
                    {"WORD_0859", "WORD_0860", "WORD_0814", "WORD_0813", "WORD_0815", "WORD_0812", "WORD_0509"});
            _moveAnimationTextDropdownChoices =
                EditorLocalize.LocalizeTexts(new List<string> {"WORD_0052", "WORD_0533"});
            _stepAnimationTextDropdownChoices =
                EditorLocalize.LocalizeTexts(new List<string> {"WORD_0052", "WORD_0533"});
            _passingTextDropdownChoices =
                EditorLocalize.LocalizeTexts(new List<string> {"WORD_0052", "WORD_0533"});
            // "1.最低", "2.低", "3.標準", "4.高", "5.最高"
            _moveRateTextDropdownChoices =
                EditorLocalize.LocalizeTexts(new List<string>
                    {"WORD_0854", "WORD_0855", "WORD_0850", "WORD_0856", "WORD_0857"});

            _priorityTextDropdownChoices =
                EditorLocalize.LocalizeTexts(new List<string> {"WORD_0870", "WORD_0871", "WORD_0872"});
            _triggerTextDropdownChoices =
                EditorLocalize.LocalizeTexts(new List<string>
                    {"WORD_0874", "WORD_0875", "WORD_0876", "WORD_0877", "WORD_0878"});
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();

            // 基本設定
            //----------------------------------------------------------------------------------------------
            // イベントID
            _eventIdText = RootContainer.Query<Label>("event_id");

            // イベント名
            _eventNameText = RootContainer.Query<ImTextField>("event_name");

            // 座標
            _eventMapX = RootContainer.Query<IntegerField>("map_X");
            _eventMapY = RootContainer.Query<IntegerField>("map_Y");

            // メモ
            _eventMemoText = RootContainer.Query<ImTextField>("event_memo");

            // ページ設定
            //----------------------------------------------------------------------------------------------

            // アウトラインエディター関連。
            {
                AdjustChapterIdBySectionBelong(_targetEventMapPage);
                SetChapterPopupField();
                SetSectionPopupField();

                // チャプター
                void SetChapterPopupField() {
                    
                    var chapters = OutlineEditor.OutlineEditor.OutlineDataModel.Chapters;
                    var choices = chapters.Select(data => data.Name.ToString()).ToList();
                    //名前が空だった場合、「名称未設定」を入れる
                    for (int i = 0; i < choices.Count; i++)
                    {
                        if (choices[i] == "")
                        {
                            choices[i] = EditorLocalize.LocalizeText("WORD_1518");
                        }
                    }
                    choices.Insert(0, EditorLocalize.LocalizeText("WORD_1518"));

                    var chapterDataModel = GetChapterDataModel(_targetEventMapPage);
                    var index = chapterDataModel != null
                        ? OutlineEditor.OutlineEditor.OutlineDataModel.GetChapterCode(chapterDataModel)
                        : 0;

                    _chapterPopupField = new PopupFieldBase<string>(choices, index);
                    var parentVe = RootContainer.Q<VisualElement>("chapter");
                    parentVe.Clear();
                    parentVe.Add(_chapterPopupField);

                    _chapterPopupField.RegisterValueChangedCallback(o =>
                    {
                        var chapterCode = _chapterPopupField.index;
                        _targetEventMapPage.chapterId = chapterCode != 0
                            ? OutlineEditor.OutlineEditor.OutlineDataModel.GetChapterDataModel(chapterCode).ID
                            : string.Empty;
                        MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);

                        // 選択中のチャプターに属するセクションのみ選択できるようにする。
                        SetSectionPopupField();
                    });
                }

                // セクション
                void SetSectionPopupField() {
                    var chapterDataModel = GetChapterDataModel(_targetEventMapPage);
                    var sections = OutlineEditor.OutlineEditor.OutlineDataModel.Sections.Where(section =>
                        chapterDataModel == null || section.ChapterID == chapterDataModel.ID).ToList();
                    var choices = new List<string>();
                    var choicesId = new List<string>();
                    for (int i = 0; i < sections.Count; i++)
                    {
                        choices.Add(sections[i].Name);
                        choicesId.Add(sections[i].ID);
                    }
                    
                    //名前が空だった場合、「名称未設定」を入れる
                    for (int i = 0; i < choices.Count; i++)
                    {
                        if (choices[i] == "")
                        {
                            choices[i] = EditorLocalize.LocalizeText("WORD_1518");
                        }
                    }
                    choices.Insert(0, EditorLocalize.LocalizeText("WORD_1518"));
                    choicesId.Insert(0, "-1");

                    var sectionDataModel = GetSectionDataModel(_targetEventMapPage);
                    var index = sections.IndexOf(sectionDataModel) + 1;
                    if (sectionDataModel != null && index == 0)
                    {
                        _targetEventMapPage.sectionId = string.Empty;
                        MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
                    }

                    _sectionPopupField = new PopupFieldBase<string>(choices, index);
                    var parentVe = RootContainer.Q<VisualElement>("section");
                    parentVe.Clear();
                    parentVe.Add(_sectionPopupField);

                    _sectionPopupField.RegisterValueChangedCallback(o =>
                    {
                        var sectionCode = _sectionPopupField.index;
                        _targetEventMapPage.sectionId = choicesId[sectionCode];
                        MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);

                        if (AdjustChapterIdBySectionBelong(_targetEventMapPage))
                        {
                            MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
                            SetChapterPopupField();
                        }
                    });
                }

                // チャプターが、セクションが所属するチャプターでなければ、 所属するチャプターに変更する。
                static bool AdjustChapterIdBySectionBelong(EventMapDataModel.EventMapPage eventMapPage) {
                    var sectionDataModel = GetSectionDataModel(eventMapPage);
                    if (sectionDataModel != null && sectionDataModel.ChapterID != eventMapPage.chapterId)
                    {
                        eventMapPage.chapterId = sectionDataModel.ChapterID;
                        return true;
                    }

                    return false;
                }

                static ChapterDataModel GetChapterDataModel(EventMapDataModel.EventMapPage eventMapPage) {
                    return OutlineEditor.OutlineEditor.OutlineDataModel.Chapters.SingleOrDefault(chapter =>
                        chapter.ID == eventMapPage.chapterId);
                }

                static SectionDataModel GetSectionDataModel(EventMapDataModel.EventMapPage eventMapPage) {
                    return OutlineEditor.OutlineEditor.OutlineDataModel.Sections.SingleOrDefault(section =>
                        section.ID == eventMapPage.sectionId);
                }
            }

            // テンプレート
            //----------------------------------------------------------------------------------------------
            // インポート
            _eventImportButton = RootContainer.Query<Button>("event_templete_inport");

            // エクスポート
            _eventExportButton = RootContainer.Query<Button>("event_templete_export");

            // 出現条件
            //----------------------------------------------------------------------------------------------
            // スイッチ1のON/OFF
            _eventSwitch1Container = RootContainer.Query<VisualElement>("event_switch1");
            _eventSwitch1Toggle = RootContainer.Query<Toggle>("event_switch1_toggle");

            // スイッチ1の対象リスト
            _eventSwitchPopupField1 = new PopupFieldBase<string>(_switchNameDropdownChoices,
                SwitchIdToIndex(_targetEventMapPage.condition.switchOne.switchId));
            _eventSwitch1Container.Add(_eventSwitchPopupField1);

            // スイッチ2のON/OFF
            _eventSwitch2Container = RootContainer.Query<VisualElement>("event_switch2");
            _eventSwitch2Toggle = RootContainer.Query<Toggle>("event_switch2_toggle");

            // スイッチ2の対象リスト
            _eventSwitchPopupField2 = new PopupFieldBase<string>(_switchNameDropdownChoices,
                SwitchIdToIndex(_targetEventMapPage.condition.switchTwo.switchId));
            _eventSwitch2Container.Add(_eventSwitchPopupField2);

            // 変数関連
            _eventVariableContainer = RootContainer.Query<VisualElement>("event_variable");
            _eventVariableToggle = RootContainer.Query<Toggle>("event_variable_toggle");
            _eventVariableInteger = RootContainer.Query<IntegerField>("event_variable_field");
            _eventVariablePopupField = new PopupFieldBase<string>(_variableNameDropdownChoices,
                VariableIdToIndex(_targetEventMapPage.condition.variables.variableId));
            _eventVariableContainer.Add(_eventVariablePopupField);

            // アイテム関連
            _eventItemContainer = RootContainer.Query<VisualElement>("event_item");
            _eventItemToggle = RootContainer.Query<Toggle>("event_item_toggle");
            _eventItemPopupField = new PopupFieldBase<string>(_itemNameDropdownChoices,
                ItemIdToIndex(_targetEventMapPage.condition.item.itemId));
            _eventItemContainer.Add(_eventItemPopupField);

            // アクター関連
            _eventActorContainer = RootContainer.Query<VisualElement>("event_actor");
            _eventActorToggle = RootContainer.Query<Toggle>("event_actor_toggle");
            _eventActorPopupField = new PopupFieldBase<string>(_actorNameDropdownChoices,
                ActorIdToIndex(_targetEventMapPage.condition.actor.actorId));
            _eventActorContainer.Add(_eventActorPopupField);

            // セルフスイッチ関連
            _eventSelfSwitch = RootContainer.Query<VisualElement>("event_self_switch");
            _eventSelfToggle = RootContainer.Query<Toggle>("event_self_toggle");
            _eventSelfSwitchPopupField = new PopupFieldBase<string>(
                _selfSwitchDictionary.Values.ToList(),
                selfSwitchIndex()
            );

            //セルフスイッチ初期表示で見つからなかった場合
            int selfSwitchIndex() {
                if (_selfSwitchDictionary.Values.IndexOf(_targetEventMapPage.condition.selfSwitch.selfSwitch) == -1)
                    return 0;
                return _selfSwitchDictionary.Values.IndexOf(_targetEventMapPage.condition.selfSwitch.selfSwitch);
            }

            _eventSelfSwitch.Add(_eventSelfSwitchPopupField);


            // スイッチアイテム関連
            _eventSwitchItem = RootContainer.Query<VisualElement>("event_switch_item");
            _eventSwitchItemToggle = RootContainer.Query<Toggle>("event_switch_item_toggle");
            var index = _switchItemDropdownChoices.IndexOf(_targetEventMapPage.condition.switchItem.switchItemId);
            if (index < 0) index = 0;
            _eventSwitchItemPopupField = new PopupFieldBase<string>(_switchItemNameDropdownChoices, index);
            _eventSwitchItem.Add(_eventSwitchItemPopupField);

            // 画像
            //----------------------------------------------------------------------------------------------
            _eventCharacterImageToggle = RootContainer.Query<Toggle>("event_chatacter_image_Toggle");
            _eventCharacterImageArea = RootContainer.Query<VisualElement>("event_character_image_area");
            _eventCharacterImagePopupFieldContainer =
                RootContainer.Query<VisualElement>("event_character_image_popupfield_container");

            index = _actors.FindIndex(actor => actor.uuId == _targetEventMapPage.condition.image.imageName);
            if (index < 0) index = 0;
            _eventCharacterImagePopupField = new PopupFieldBase<string>(_actorNameDropdownChoices, index);
            _eventCharacterImagePopupFieldContainer.Add(_eventCharacterImagePopupField);

            _eventSelectImageToggle = RootContainer.Query<Toggle>("event_select_image_toggle");
            _eventSelectImageArea = RootContainer.Query<VisualElement>("event_select_image_area");

            _eventCharacterImageContainer = RootContainer.Query<VisualElement>("event_character_image");
            _eventSdImageContainer = RootContainer.Query<VisualElement>("event_sd_image");

            _eventPictureName = RootContainer.Query<Label>("event_picture_name");


            // 自律移動
            //----------------------------------------------------------------------------------------------

            _eventAutoMoveContainer = RootContainer.Query<VisualElement>("event_auto_move");
            _eventAutoMovePopupField =
                new PopupFieldBase<string>(_autoMoveTextDropdownChoices, _targetEventMapPage.move.moveType);
            _eventAutoMoveContainer.Add(_eventAutoMovePopupField);
            _routeSettingButton = RootContainer.Query<Button>("route_setting_button");
            _routeInitButton = RootContainer.Query<Button>("route_init_button");
            _previewButton = RootContainer.Query<Button>("preview_button");
            _repeatOperationToggle = RootContainer.Query<Toggle>("repeatOperation_toggle");
            _moveSkipToggle = RootContainer.Query<Toggle>("moveSkip_toggle");


            // 移動速度
            _eventAutoMoveSpeedContainer = RootContainer.Query<VisualElement>("event_move_speed");
            _eventAutoMoveSpeedPopupField =
                new PopupFieldBase<string>(_moveSpeedTextDropdownChoices, _targetEventMapPage.move.speed);
            _eventAutoMoveSpeedContainer.Add(_eventAutoMoveSpeedPopupField);

            // 向き
            _eventAutoMoveDirectionContainer = RootContainer.Query<VisualElement>("event_direction");
            _eventAutoMoveDirectionPopupField = new PopupFieldBase<string>(_directionTextDropdownChoices,
                _targetEventMapPage.walk.direction % _directionTextDropdownChoices.Count);
            _eventAutoMoveDirectionContainer.Add(_eventAutoMoveDirectionPopupField);

            // 向きを固定する
            _eventAutoMoveDirectionFix = RootContainer.Query<Toggle>("event_direction_fix");

            // 歩行アニメ
            _eventAutoMoveAnimationContainer = RootContainer.Query<VisualElement>("event_move_animation");
            _eventAutoMoveAnimationPopupField =
                new PopupFieldBase<string>(_moveAnimationTextDropdownChoices, _targetEventMapPage.walk.walking);
            _eventAutoMoveAnimationContainer.Add(_eventAutoMoveAnimationPopupField);

            // 足踏みアニメ
            _eventAutoMoveStepAnimationContainer = RootContainer.Query<VisualElement>("event_step_animation");
            _eventAutoMoveStepAnimationPopupField = new PopupFieldBase<string>(_stepAnimationTextDropdownChoices,
                _targetEventMapPage.walk.stepping);
            _eventAutoMoveStepAnimationContainer.Add(_eventAutoMoveStepAnimationPopupField);

            // すりぬけ
            _eventAutoMovePassingContainer = RootContainer.Query<VisualElement>("event_passing");
            _eventAutoMovePassingPopupField =
                new PopupFieldBase<string>(_passingTextDropdownChoices, _targetEventMapPage.walk.through);
            _eventAutoMovePassingContainer.Add(_eventAutoMovePassingPopupField);

            // 行動頻度
            _eventAutoMoveRateContainer = RootContainer.Query<VisualElement>("event_move_Rate");
            _eventAutoMoveRatePopupField =
                new PopupFieldBase<string>(_moveRateTextDropdownChoices, _targetEventMapPage.move.frequency);
            _eventAutoMoveRateContainer.Add(_eventAutoMoveRatePopupField);

            // プライオリティ
            //----------------------------------------------------------------------------------------------
            _eventPriorityContainer = RootContainer.Query<VisualElement>("event_priority");
            _eventPriorityPopupField =
                new PopupFieldBase<string>(_priorityTextDropdownChoices, _targetEventMapPage.priority);
            _eventPriorityContainer.Add(_eventPriorityPopupField);

            // トリガー
            //----------------------------------------------------------------------------------------------
            _eventTriggerContainer = RootContainer.Query<VisualElement>("event_trigger");
            _eventTriggerPopupField =
                new PopupFieldBase<string>(_triggerTextDropdownChoices, _targetEventMapPage.eventTrigger);
            _eventTriggerContainer.Add(_eventTriggerPopupField);

            SetEntityToUi();
            SetCallbackToUi();
        }

        /**
         * UIにデータを反映
         */
        private void SetEntityToUi() {
            // イベントID
            _eventIdText.text = _eventMapDataModel.SerialNumberString;
            // イベント名
            _eventNameText.value = _eventMapDataModel.name;
            // 座標
            _eventMapX.value = _eventMapDataModel.x;
            _eventMapY.value = _eventMapDataModel.y * -1;
            // メモ
            _eventMemoText.value = _eventMapDataModel.note;

            // チャプターとセクションはInitUi()にまとめた。

            // スイッチ1のON/OFF
            _eventSwitch1Toggle.value = Convert.ToBoolean(_targetEventMapPage.condition.switchOne.enabled);
            _eventSwitch1Container.SetEnabled(_eventSwitch1Toggle.value);
            // スイッチ1の対象リスト
            _eventSwitchPopupField1.index = SwitchIdToIndex(_targetEventMapPage.condition.switchOne.switchId);
            // スイッチ2のON/OFF
            _eventSwitch2Toggle.value = Convert.ToBoolean(_targetEventMapPage.condition.switchTwo.enabled);
            _eventSwitch2Container.SetEnabled(_eventSwitch2Toggle.value);
            // スイッチ2の対象リスト
            _eventSwitchPopupField2.index = SwitchIdToIndex(_targetEventMapPage.condition.switchTwo.switchId);
            // 変数関連
            _eventVariableToggle.value = Convert.ToBoolean(_targetEventMapPage.condition.variables.enabled);
            _eventVariableInteger.value = _targetEventMapPage.condition.variables.value;
            _eventVariableContainer.SetEnabled(_eventVariableToggle.value);
            _eventVariableInteger.SetEnabled(_eventVariableToggle.value);
            //変数の対象リスト
            _eventVariablePopupField.index = VariableIdToIndex(_targetEventMapPage.condition.variables.variableId);
            // アイテム関連
            _eventItemToggle.value = Convert.ToBoolean(_targetEventMapPage.condition.item.enabled);
            _eventItemPopupField.index = ItemIdToIndex(_targetEventMapPage.condition.item.itemId);
            _eventItemContainer.SetEnabled(_eventItemToggle.value);

            // アクター関連
            _eventActorToggle.value = Convert.ToBoolean(_targetEventMapPage.condition.actor.enabled);
            _eventActorContainer.SetEnabled(_eventActorToggle.value);
            _eventActorPopupField.index = ActorIdToIndex(_targetEventMapPage.condition.actor.actorId);

            // セルフスイッチ関連
            _eventSelfToggle.value = Convert.ToBoolean(_targetEventMapPage.condition.selfSwitch.enabled);
            _eventSelfSwitch.SetEnabled(_eventSelfToggle.value);

            //スィッチアイテム関連
            _eventSwitchItemToggle.value = Convert.ToBoolean(_targetEventMapPage.condition.switchItem.enabled);
            _eventSwitchItem.SetEnabled(_eventSwitchItemToggle.value);

            // 画像関連
            var enabledType = _targetEventMapPage.condition.image.Enabled;
            _eventCharacterImageToggle.value = enabledType == EnabledType.Character;
            _eventSelectImageToggle.value = enabledType == EnabledType.SelectedImage;
            _eventCharacterImageArea.SetEnabled(_eventCharacterImageToggle.value);
            _eventSelectImageArea.SetEnabled(_eventSelectImageToggle.value);

            //向き固定
            _eventAutoMoveDirectionFix.value = Convert.ToBoolean(_targetEventMapPage.walk.directionFix);
        }

        /**
         * UIにコールバック処理を登録
         */
        private void SetCallbackToUi() {
            // イベント名
            _eventNameText.RegisterCallback<FocusOutEvent>(o =>
            {
                _eventMapDataModel.name = _eventNameText.value;
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
                UpdateData();
            });

            // 座標
            _eventMapX.RegisterCallback<FocusOutEvent>(o =>
            {
                if (_eventMapX.value > _mapDataModel.width - 1 || _eventMapX.value < 0 || 
                    CheckOverlap(_eventMapX.value, _eventMapDataModel.y))
                {
                    _eventMapX.value = _eventMapDataModel.x;
                    _element.Refresh();
                }
                else
                {
                    _eventMapDataModel.x = _eventMapX.value;
                    MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
                    _element.Refresh();
                }
            });
            _eventMapY.RegisterCallback<FocusOutEvent>(o =>
            {
                if (_eventMapY.value > _mapDataModel.height - 1 || _eventMapY.value < 0 ||
                    CheckOverlap(_eventMapDataModel.x, _eventMapY.value * -1))
                {
                    _eventMapY.value = _eventMapDataModel.y * -1;
                    _element.Refresh();
                }
                else
                {
                    _eventMapDataModel.y = _eventMapY.value * -1;
                    MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
                    _element.Refresh();
                }
            });

            // メモ
            _eventMemoText.RegisterCallback<FocusOutEvent>(o =>
            {
                _eventMapDataModel.note = _eventMemoText.value;
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
            });

            // チャプターとセクションはInitUi()にまとめた。

            // インポート
            _eventImportButton.clicked += () =>
            {
                //[0] EventMapDataModel
                //[1] EventDataModel 存在する場合のみ
                //のファイルパス配列を取得
                bool success = false;
                List<string> files = AssetManageImporter.ImportMapEventJsons();
                if (files.Count >= 2)
                {
                    var eventMapDataModel = AssetManageImporter.ReadJsonToDataModel<EventMapDataModel>(files[0]);
                    // データが正常に取得できれば反映
                    if (eventMapDataModel != null)
                    {
                        // 変更する情報のみを設定する
                        List<EventMapDataModel> eventMapDataModelList = _eventManagementService.LoadEventMap();
                        EventMapDataModel eventMapDataModelWork = null;
                        for (int i = 0; i < eventMapDataModelList.Count; i++)
                            if (eventMapDataModelList[i].mapId == _eventMapDataModel.mapId && eventMapDataModelList[i].eventId == _eventMapDataModel.eventId)
                            {
                                eventMapDataModelWork = eventMapDataModelList[i];
                                break;
                            }
                        eventMapDataModelWork.name = eventMapDataModel.name;
                        eventMapDataModelWork.note = eventMapDataModel.note;

                        //EventDataModel のJSONデータも存在していた場合は、それも読み込む
                        EventDataModel eventDataModel = _eventManagementService.LoadEventById(_eventMapDataModel.eventId, _targetEventMapPage.page);
                        var eventDataModelWork = AssetManageImporter.ReadJsonToDataModel<EventDataModel>(files[1]);
                        if (eventDataModelWork != null)
                        {
                            //エクスポートした時のページ番号の設定を、インポートした時のページ番号の設定に上書き
                            eventMapDataModelWork.pages[_targetEventMapPage.page] = eventMapDataModel.pages[eventDataModelWork.page];
                            //ページ番号だけは挿げ替える
                            eventMapDataModelWork.pages[_targetEventMapPage.page].page = _targetEventMapPage.page;
                            _eventManagementService.SaveEventMap(eventMapDataModelWork);

                            //エクスポートした時のイベントコマンドを、インポートした時のページ番号の設定に上書き
                            eventDataModel.eventCommands = eventDataModelWork.eventCommands;
                            _eventManagementService.SaveEvent(eventDataModel);

                            _Save();
                            UpdateData();
                            UpdateDataImport();

                            success = true;
                        }
                    }
                }

                //PU表示
                if (success)
                    AssetManageImporter.ShowDialog(success);
            };

            // エクスポート
            _eventExportButton.clicked += () =>
            {
                //編集中のページのみエクスポート
                EventDataModel eventDataModel = new EventManagementService().LoadEventById(_eventMapDataModel.eventId, _targetEventMapPage.page);
                AssetManageExporter.ExportMapEventJsons(_eventMapDataModel, eventDataModel, _eventMapDataModel.eventId);
            };

            // スイッチ1のON/OFF
            _eventSwitch1Toggle.RegisterValueChangedCallback(o =>
            {
                // nullであれば初期値を入れる
                if (_targetEventMapPage.condition.switchOne.switchId == "" && _switchDropdownChoices.Count > 0)
                    _targetEventMapPage.condition.switchOne.switchId = _switchDropdownChoices[0];
                _targetEventMapPage.condition.switchOne.enabled = Convert.ToInt32(_eventSwitch1Toggle.value);
                _eventSwitch1Container.SetEnabled(_eventSwitch1Toggle.value);
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
            });

            // スイッチ1の対象リスト
            _eventSwitchPopupField1.RegisterValueChangedCallback(evt =>
            {
                _targetEventMapPage.condition.switchOne.switchId =
                    _switchDropdownChoices[_eventSwitchPopupField1.index];
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
            });

            // スイッチ2のON/OFF
            _eventSwitch2Toggle.RegisterValueChangedCallback(o =>
            {
                // nullであれば初期値を入れる
                if (_targetEventMapPage.condition.switchTwo.switchId == "" && _switchDropdownChoices.Count > 0)
                    _targetEventMapPage.condition.switchTwo.switchId = _switchDropdownChoices[0];
                _targetEventMapPage.condition.switchTwo.enabled = Convert.ToInt32(_eventSwitch2Toggle.value);
                _eventSwitch2Container.SetEnabled(_eventSwitch2Toggle.value);
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
            });

            // スイッチ2の対象リスト
            _eventSwitchPopupField2.RegisterValueChangedCallback(evt =>
            {
                _targetEventMapPage.condition.switchTwo.switchId =
                    _switchDropdownChoices[_eventSwitchPopupField2.index];
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
            });

            // 変数関連
            _eventVariableToggle.RegisterValueChangedCallback(o =>
            {
                // nullであれば初期値を入れる
                if (_targetEventMapPage.condition.variables.variableId == "" && _variableDropdownChoices.Count > 0)
                    _targetEventMapPage.condition.variables.variableId = _variableDropdownChoices[0];
                _targetEventMapPage.condition.variables.enabled = Convert.ToInt32(_eventVariableToggle.value);
                _eventVariableContainer.SetEnabled(_eventVariableToggle.value);
                _eventVariableInteger.SetEnabled(_eventVariableToggle.value);
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
            });
            _eventVariableInteger.RegisterCallback<FocusOutEvent>(o =>
            {
                _targetEventMapPage.condition.variables.value =
                    _eventVariableInteger.value;
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
            });
            _eventVariablePopupField.RegisterValueChangedCallback(evt =>
            {
                _targetEventMapPage.condition.variables.variableId =
                    _variableDropdownChoices[_eventVariablePopupField.index];
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
            });

            // アイテム関連
            _eventItemToggle.RegisterValueChangedCallback(o =>
            {
                // nullであれば初期値を入れる
                if (_targetEventMapPage.condition.item.itemId == "" && _itemDropdownChoices.Count > 0)
                    _targetEventMapPage.condition.item.itemId = _itemDropdownChoices[0];
                _targetEventMapPage.condition.item.enabled = Convert.ToInt32(_eventItemToggle.value);
                _eventItemContainer.SetEnabled(_eventItemToggle.value);
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
            });
            _eventItemPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetEventMapPage.condition.item.itemId =
                    _itemDropdownChoices[_itemNameDropdownChoices.IndexOf(_eventItemPopupField.value)];
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
            });

            // アクター関連
            _eventActorToggle.RegisterValueChangedCallback(o =>
            {
                // nullであれば初期値を入れる
                if (_targetEventMapPage.condition.actor.actorId == "" && _actorDropdownChoices.Count > 0)
                    _targetEventMapPage.condition.actor.actorId = _actorDropdownChoices[0];
                _targetEventMapPage.condition.actor.enabled = Convert.ToInt32(_eventActorToggle.value);
                _eventActorContainer.SetEnabled(_eventActorToggle.value);
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
            });
            _eventActorPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetEventMapPage.condition.actor.actorId =
                    _actorDropdownChoices[_actorNameDropdownChoices.IndexOf(_eventActorPopupField.value)];
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
            });

            // セルフスイッチ関連
            _eventSelfToggle.RegisterValueChangedCallback(o =>
            {
                _targetEventMapPage.condition.selfSwitch.enabled = Convert.ToInt32(_eventSelfToggle.value);
                _eventSelfSwitch.SetEnabled(_eventSelfToggle.value);
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
            });
            _eventSelfSwitchPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetEventMapPage.condition.selfSwitch.selfSwitch = _eventSelfSwitchPopupField.value;
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
            });

            // スィッチアイテム関連
            _eventSwitchItemToggle.RegisterValueChangedCallback(o =>
            {
                // nullであれば初期値を入れる
                if (_targetEventMapPage.condition.switchItem.switchItemId == "" && _switchItemDropdownChoices.Count > 0)
                    _targetEventMapPage.condition.switchItem.switchItemId = _switchItemDropdownChoices[0];
                _targetEventMapPage.condition.switchItem.enabled = Convert.ToInt32(_eventSwitchItemToggle.value);
                _eventSwitchItem.SetEnabled(_eventSwitchItemToggle.value);
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
            });
            _eventSwitchItemPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetEventMapPage.condition.switchItem.switchItemId = _switchItemDropdownChoices[_eventSwitchItemPopupField.index];
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
            });

            // 画像関連

            // 『キャラクター』トグル。
            _eventCharacterImageToggle.RegisterValueChangedCallback(o =>
            {
                _eventCharacterImageArea.SetEnabled(o.newValue);
                if (o.newValue)
                {
                    _eventSelectImageToggle.value = false;
                }
                if (_eventSelectImageToggle.value) return;
                _targetEventMapPage.condition.image.Enabled =
                    o.newValue ? EnabledType.Character : EnabledType.None;
                var actor = _actors[_eventCharacterImagePopupField.index];
                if (o.newValue)
                {
                    _targetEventMapPage.condition.image.imageName = actor.uuId;
                    _targetEventMapPage.image.name = actor.basic.name;
                    _eventPictureName.text = actor.basic.name;
                }
                else
                {
                    _targetEventMapPage.image.name = "";
                    _eventPictureName.text = "";
                }
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
                _element.Refresh();
                
                
            });

            // 『画像を選択』トグル。
            _eventSelectImageToggle.RegisterValueChangedCallback(o =>
            {
                _eventSelectImageArea.SetEnabled(o.newValue);
                if (o.newValue)
                {
                    _eventCharacterImageToggle.value = false;
                    _targetEventMapPage.image.name = "";
                    _eventPictureName.text = "";
                }
                if (_eventCharacterImageToggle.value) return;
                _targetEventMapPage.condition.image.Enabled =
                    o.newValue ? EnabledType.SelectedImage : EnabledType.None;
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
                _element.Refresh();
            });

            // 『キャラクター』プルダウン
            _eventCharacterImagePopupField.RegisterValueChangedCallback(evt =>
            {
                var actor = _actors[_eventCharacterImagePopupField.index];
                _targetEventMapPage.condition.image.imageName = actor.uuId;
                _targetEventMapPage.image.name = actor.basic.name;
                _eventPictureName.text = actor.basic.name;

                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);

                _element.Refresh();

                BackgroundImageHelper.SetBackground(
                    _eventCharacterImage,
                    new Vector2(66, 76),
                    ImageManager.LoadSvCharacter(actor.image.character),
                    LengthUnit.Pixel);
            });

            // キャラクター画像ボタン。
            {
                var faceImageButton = _eventCharacterImageContainer as Button;
                _eventCharacterImage = new VisualElement();
                _eventCharacterImage.style.alignSelf = Align.Center;
                _eventCharacterImageContainer.Add(_eventCharacterImage);
                var actor = _actors[_eventCharacterImagePopupField.index];
                if (actor.image.character != "")
                {
                    _eventCharacterImageContainer.style.justifyContent = Justify.Center;
                    BackgroundImageHelper.SetBackground(
                        _eventCharacterImage,
                        new Vector2(66, 76),
                        ImageManager.LoadSvCharacter(actor.image.character),
                        LengthUnit.Pixel);
                }
                // ボタンとしては機能させない。
                faceImageButton.focusable = false;
            }

            // SDキャラクター画像ボタン。
            {
                var manageData = databaseManagementService.LoadAssetManage();
                var sdImageButton = _eventSdImageContainer as Button;
                _eventSdImage = new VisualElement();
                _eventSdImage.style.alignSelf = Align.Center;
                _eventSdImageContainer.Add(_eventSdImage);

                string assetId = "";
                for (int i = 0; i < manageData.Count; i++)
                {
                    if (manageData[i].id == _targetEventMapPage.image.sdName)
                    {
                        assetId = manageData[i].id;
                        break;
                    }
                }


                if (assetId != "")
                {
                    _eventSdImageContainer.style.justifyContent = Justify.Center;
                    BackgroundImageHelper.SetBackground(
                        _eventSdImage,
                        new Vector2(66, 76),
                        LoadSdCharacterTexture(_targetEventMapPage),
                        LengthUnit.Pixel);
                }

                sdImageButton.clickable.clicked += () => { SelectedSd(); };
            }

            if (_eventCharacterImageToggle.value)
            {
                var actor = _actors[_eventCharacterImagePopupField.index];
                _eventPictureName.text = actor.basic.name;
            }


            // 自律移動関連
            _eventAutoMovePopupField.RegisterValueChangedCallback(evt => { OnChangeAutoMovePopupField(); });

            // 移動速度
            _eventAutoMoveSpeedPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetEventMapPage.move.speed =
                    _moveSpeedTextDropdownChoices.IndexOf(_eventAutoMoveSpeedPopupField.value);
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
            });

            // 向き
            _eventAutoMoveDirectionPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetEventMapPage.walk.direction = _eventAutoMoveDirectionPopupField.index;
                if (_eventAutoMoveDirectionPopupField.index <= 1)
                {
                    //向きが移動方向又はプレイヤーの場合、向きは固定にできない
                    _targetEventMapPage.walk.directionFix = 0;
                    _eventAutoMoveDirectionFix.value = false;
                    _eventAutoMoveDirectionFix.SetEnabled(false);
                }
                else
                {
                    //向きを固定にできる
                    _eventAutoMoveDirectionFix.SetEnabled(true);
                }
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
            });

            // 向き固定
            _eventAutoMoveDirectionFix.RegisterValueChangedCallback(o =>
            {
                _targetEventMapPage.walk.directionFix = Convert.ToInt32(_eventAutoMoveDirectionFix.value);
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
            });
            
            if (_eventAutoMoveDirectionPopupField.index <= 1)
            {
                //向きが移動方向又はプレイヤーの場合、向きは固定にできない
                _targetEventMapPage.walk.directionFix = 0;
                _eventAutoMoveDirectionFix.value = false;
                _eventAutoMoveDirectionFix.SetEnabled(false);
            }
            else
            {
                //向きを固定にできる
                _eventAutoMoveDirectionFix.SetEnabled(true);
            }

            // 歩行アニメ
            _eventAutoMoveAnimationPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetEventMapPage.walk.walking =
                    _moveAnimationTextDropdownChoices.IndexOf(_eventAutoMoveAnimationPopupField.value);
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
            });

            // 足踏みアニメ
            _eventAutoMoveStepAnimationPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetEventMapPage.walk.stepping =
                    _stepAnimationTextDropdownChoices.IndexOf(_eventAutoMoveStepAnimationPopupField.value);
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
            });

            // すり抜け
            _eventAutoMovePassingPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetEventMapPage.walk.through =
                    _passingTextDropdownChoices.IndexOf(_eventAutoMovePassingPopupField.value);
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
            });

            // 行動頻度
            _eventAutoMoveRatePopupField.RegisterValueChangedCallback(evt =>
            {
                _targetEventMapPage.move.frequency =
                    _moveRateTextDropdownChoices.IndexOf(_eventAutoMoveRatePopupField.value);
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
            });

            // プライオリティ
            _eventPriorityPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetEventMapPage.priority = _priorityTextDropdownChoices.IndexOf(_eventPriorityPopupField.value);
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
            });

            // トリガー
            _eventTriggerPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetEventMapPage.eventTrigger = _triggerTextDropdownChoices.IndexOf(_eventTriggerPopupField.value);
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
            });

            var pos = new List<Vector3Int>();
            var codeList = new List<EventMoveEnum>();
            var indexList = new List<int>();
            var textList = new List<string>();
            pos.Add(new Vector3Int(0, 0, 0));
            foreach (var data in _targetEventMapPage.move.route) codeList.Add((EventMoveEnum) data.code);

            _codeList = codeList;

            //開始位置の設定
            InitializeStartPosition(pos, indexList, textList);

            //ルート指定開始/終了
            var isEdit = false;
            _routeSettingButton.text = EditorLocalize.LocalizeText("WORD_1583");
            _routeSettingButton.clickable.clicked +=
                () =>
                {
                    if (isEdit)
                    {
                        _routeSettingButton.text = EditorLocalize.LocalizeText("WORD_1583");
                        MapEditor.MapEditor.LaunchRouteDrawingModeEnd();
                    }
                    else
                    {
                        //開始位置の設定
                        InitializeStartPosition(pos, indexList, textList);

                        _routeSettingButton.text = EditorLocalize.LocalizeText("WORD_1584");
                        MapEditor.MapEditor.LaunchRouteDrawingMode(new Vector3Int(_eventMapDataModel.x, _eventMapDataModel.y, 0));
                    }
                    // プレビューのボタンを押下可能状態
                    _previewButton.SetEnabled(isEdit);

                    isEdit = !isEdit;
                };

            //ルート初期化
            _routeInitButton.clickable.clicked += () =>
            {
                var pos = new List<Vector3Int>();
                var codeList = new List<EventDataModel.EventCommandMoveRoute>();
                var indexList = new List<int>();
                var textList = new List<string>();
                pos.Add(new Vector3Int(0, 0, 0));

                //開始位置の設定
                pos[0] = new Vector3Int(_eventMapDataModel.x, _eventMapDataModel.y, 0);
                _nowPos = new Vector2(_eventMapDataModel.x, _eventMapDataModel.y);

                MapEditor.MapEditor.LaunchRouteEditMode(
                    pos, indexList, codeList, textList, CallBackCode, -1, -1);
            };

            //プレビューボタン
            _previewButton.clickable.clicked += Preview;

            //オプション
            //繰り返し
            _repeatOperationToggle.value = _targetEventMapPage.move.repeat == 1;
            _repeatOperationToggle.RegisterValueChangedCallback(o =>
            {
                _targetEventMapPage.move.repeat = _repeatOperationToggle.value ? 1 : 0;
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
            });
            //とばす
            _moveSkipToggle.value = _targetEventMapPage.move.skip == 1;
            _moveSkipToggle.RegisterValueChangedCallback(o =>
            {
                _targetEventMapPage.move.skip = _moveSkipToggle.value ? 1 : 0;
                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
            });
        }

        private void InitializeStartPosition(List<Vector3Int> pos, List<int> indexList, List<string> textList) {
            //開始位置の設定
            pos[0] = new Vector3Int(_eventMapDataModel.x, _eventMapDataModel.y, 0);
            MapEditor.MapEditor.LaunchRouteEditMode(
                pos, indexList, _targetEventMapPage.move.route, textList, CallBackCode, -1, -1);
            _nowPos = new Vector2(_eventMapDataModel.x, _eventMapDataModel.y);
            if (_targetEventMapPage.move.moveType != 3)
            {
                _routeSettingButton.SetEnabled(false);
                _routeInitButton.SetEnabled(false);
                _previewButton.SetEnabled(false);
                _repeatOperationToggle.SetEnabled(false);
                _moveSkipToggle.SetEnabled(false);
            }
        }

        /**
         * 自律移動タイプ変更時のイベントハンドラ
         */
        private void OnChangeAutoMovePopupField() {
            _targetEventMapPage.move.moveType = _eventAutoMovePopupField.index;

            MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
            _routeSettingButton.SetEnabled(false);
            _routeInitButton.SetEnabled(false);
            _previewButton.SetEnabled(false);
            _repeatOperationToggle.SetEnabled(false);
            _moveSkipToggle.SetEnabled(false);


            // 「カスタム」が選択された場合
            if (_eventAutoMovePopupField.index == 3)
            {
                _routeSettingButton.SetEnabled(true);
                _routeInitButton.SetEnabled(true);
                _previewButton.SetEnabled(true);
                _repeatOperationToggle.SetEnabled(true);
                _moveSkipToggle.SetEnabled(true);
            }
        }

        /// <summary>
        ///     マップで入力したルート指定が入った配列を返却
        /// </summary>
        /// <param name="callBackCodeList"></param>
        private void CallBackCode(List<EventDataModel.EventCommandMoveRoute> callBackCodeList) {
            var moveRoutes = new List<EventDataModel.EventCommandMoveRoute>();
            foreach (var moveRoute in callBackCodeList)
            {
                var route = new EventDataModel.EventCommandMoveRoute(moveRoute.code, new List<string>(),
                    moveRoute.codeIndex);
                moveRoutes.Add(route);
            }

            _targetEventMapPage.move.route = moveRoutes;
            _codeList = new List<EventMoveEnum>();
            foreach (var data in moveRoutes) _codeList.Add((EventMoveEnum) data.code);
            MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);
        }

        private void SelectedSd() {
            var sdSelectModalWindow = new SdSelectModalWindow();
            sdSelectModalWindow.CharacterSdType = SdSelectModalWindow.CharacterType.Map;
            sdSelectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select IconImage"), data =>
            {
                var imageName = (string) data;
                _targetEventMapPage.image.sdName = imageName;
                
                _Save();

                BackgroundImageHelper.SetBackground(
                    _eventSdImage,
                    new Vector2(66, 76),
                    LoadSdCharacterTexture(_targetEventMapPage),
                    LengthUnit.Pixel);

                MapEditor.MapEditor.SaveEventMap(_eventMapDataModel);

                _element.Refresh();
            }, _targetEventMapPage.image.sdName);
        }

        //アクターの初期値用
        private int ActorIdToIndex(string id) {
            var returnIndex = 0;
            for (var i = 0; i < _actors.Count; i++)
                if (id == _actors[i].uuId)
                {
                    returnIndex = i;
                    break;
                }

            return returnIndex;
        }

        //アイテムの初期値用
        private int ItemIdToIndex(string id) {
            var returnIndex = 0;
            for (var i = 0; i < _items.Count; i++)
                if (id == _items[i].basic.id)
                {
                    returnIndex = i;
                    break;
                }

            return returnIndex;
        }

        //スイッチの初期値用
        private int SwitchIdToIndex(string id) {
            var returnIndex = 0;
            for (var i = 0; i < _flags.switches.Count; i++)
                if (id == _flags.switches[i].id)
                {
                    returnIndex = i;
                    break;
                }

            return returnIndex;
        }

        //変数の初期値用
        private int VariableIdToIndex(string id) {
            var returnIndex = 0;
            for (var i = 0; i < _flags.variables.Count; i++)
                if (id == _flags.variables[i].id)
                {
                    returnIndex = i;
                    break;
                }

            return returnIndex;
        }

        private void _Save() {
            _eventMapDataModels =
                _eventManagementService.LoadEventMap();
            for (var i = 0; i < _eventMapDataModels.Count; i++)
                if (_eventMapDataModels[i].eventId == _eventMapDataModel.eventId)
                {
                    _eventMapDataModels[i] = _eventMapDataModel;
                    break;
                }
        }

        //hierarchy更新
        private void UpdateData() {
            _ = Editor.Hierarchy.Hierarchy.Refresh(Editor.Hierarchy.Enum.Region.Map, AbstractHierarchyView.RefreshTypeEventName + "," + _mapDataModel.id);
            _element.Refresh(null, _eventMapDataModels, _eventMapDataModel);
        }

        private async void UpdateDataImport() {
            await Task.Delay(10);
            MapEditor.MapEditor.LaunchEventEditMode(_mapDataModel, _eventMapDataModel, _targetEventMapPage.page);
        }

        private void Preview() {
            var sceneWindow =
                WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.DatabaseSceneWindow) as
                    SceneWindow;
            var mapId = _eventMapDataModel.mapId;
            var mapDataModel = Editor.Hierarchy.Hierarchy.mapManagementService.LoadMapById(mapId);
            if (sceneWindow != null)
            {
                sceneWindow.Clear();
                sceneWindow.Create(SceneWindow.PreviewId.Route);
                sceneWindow.GetRoutePreview().CreateMap(mapDataModel, _nowPos, _codeList, _eventMapDataModel.eventId);
                sceneWindow.Init();

                var routePreview = sceneWindow.GetRoutePreview();
                routePreview.SetTargetId("-1");
                routePreview.SetSpeed((Commons.SpeedMultiple.Id)_targetEventMapPage.move.speed);
                routePreview.SetMoveFrequencyWaitTime(_targetEventMapPage.move.frequency);

                // 向き
                var dir = _targetEventMapPage.walk.direction;
                //補正
                if (dir == 2) dir = 4;
                else if (dir == 4) dir = 2;
                routePreview.SetDirectionType((Commons.Direction.Id)dir);

                var value =
                    _targetEventMapPage.walk.walking == 1 && _targetEventMapPage.walk.stepping == 1 ? 2 :
                    _targetEventMapPage.walk.walking == 0 && _targetEventMapPage.walk.stepping == 0 ? 3 :
                    _targetEventMapPage.walk.walking == 1 && _targetEventMapPage.walk.stepping == 0 ? 1 : 0;
                routePreview.SetAnimation(value);

                sceneWindow.SetRenderingSize(2400, 1080);
                sceneWindow.Render();
            }
        }

        // 重複座標判定処理
        bool CheckOverlap(int x, int y) {
            // 配置物の取得
            var database = Editor.Hierarchy.Hierarchy.databaseManagementService;
            var vehicles = database.LoadCharacterVehicles().FindAll(vehicle => vehicle.mapId == _eventMapDataModel.mapId);
            var stMap = database.LoadSystem().initialParty.startMap;

            foreach (var data in _eventMapDataModels)
                if (data.mapId == _eventMapDataModel.mapId && data.x == x && data.y == y)
                    return true;

            foreach (var v in vehicles)
                if (v.initialPos[0] == x && v.initialPos[1] == y)
                    return true;

            if (stMap.mapId == _eventMapDataModel.mapId && stMap.position[0] == x && stMap.position[1] * -1 == y)
                return true;

            return false;
        }
    }
}