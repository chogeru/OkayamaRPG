using Assets.RPGMaker.Codebase.Editor.Common.Asset;
using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Actor;
using RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Addons;
using RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.AudioVideo;
using RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Battle;
using RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Character;
using RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Character.Action;
using RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Character.Animation;
using RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Character.Direction;
using RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Character.Image;
using RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Character.Move;
using RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Character.Vehicle;
using RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Event;
using RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.FlowControl;
using RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.GameProgress;
using RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Map;
using RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Message;
using RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Party;
using RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Picture;
using RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Scene;
using RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Screen;
using RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.SystemSetting;
using RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Timing;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Label = RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.FlowControl.Label;

namespace RPGMaker.Codebase.Editor.MapEditor.Window.EventEdit
{
    public class CommandSettingWindow : BaseWindow
    {
        private VisualElement _commandSetting;

        // データプロパティ
        private List<EventDataModel> _eventDataModels;

        private EventManagementService _eventManagementService;

        // UI要素プロパティ
        private VisualElement _root;

        private ExecutionContentsWindow executionContentsWindow => 
            (ExecutionContentsWindow)WindowLayoutManager.GetActiveWindow(
                WindowLayoutManager.WindowLayoutId.MapEventExecutionContentsWindow);

        /**
         * 初期化
         */
        private void InitUI() {
            var stylesheet =
                AssetDatabase.LoadAssetAtPath<StyleSheet>(
                    "Assets/RPGMaker/Codebase/Editor/Inspector/inspectorDark.uss");

            if (!EditorGUIUtility.isProSkin)
            {
                stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/RPGMaker/Codebase/Editor/Inspector/inspectorLight.uss");
            }


            if (stylesheet != null)
                rootVisualElement.styleSheets.Add(stylesheet);
        }

        protected void Awake() {
            InitUI();
            _root = rootVisualElement;
            _root.Clear();
            var scrollView = new ScrollView();
            _commandSetting = new VisualElement();
            scrollView.Add(_commandSetting);
            _root.Add(scrollView);
        }


        /// <summary>
        ///     コマンドの追加
        /// </summary>
        /// <param name="commandCode">コマンドコード</param>
        /// <param name="index">コマンドを追加/挿入するインデックス</param>
        /// <param name="parameters">追加時に適用するパラメータ</param>
        public void InsertEventCommand(string commandCode, int index, EventDataModel.EventCommand eventCommand, bool show = true) {

            _eventManagementService ??= Hierarchy.Hierarchy.eventManagementService;
            var instance = CommandSettingWindowParam.instance;

            HideScreen();
            instance.eventCommandIndex = 0;
            var split = commandCode.Split('_');
            var code = int.Parse(split[0]);
            _eventDataModels = executionContentsWindow.EventDataModels;

            instance.eventIndex =
                _eventDataModels.FindIndex(v => instance.eventID == v.id && instance.eventPage == v.page);
            if (instance.eventIndex == -1)
            {
                instance.eventIndex = 0;
                _eventDataModels.Insert(
                    instance.eventIndex,
                    new EventDataModel(
                        instance.eventID,
                        0,
                        1,
                        new List<EventDataModel.EventCommand>
                        {
                            new EventDataModel.EventCommand(
                                code,
                                new List<string>(),
                                new List<EventDataModel.EventCommandMoveRoute>())
                        }));
            }
            else
            {
                var eventCommands = _eventDataModels[instance.eventIndex].eventCommands;

                eventCommands.Insert(
                    index,
                    eventCommand != null ?
                        JsonHelper.Clone(eventCommand) :
                        new EventDataModel.EventCommand(
                            code,
                            new List<string>(),
                            new List<EventDataModel.EventCommandMoveRoute>()));

                if (code == (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT && eventCommand == null)
                {
                    eventCommands.Insert(
                        index + 1, 
                        new EventDataModel.EventCommand(
                            (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_END,
                            new List<string>(),
                            new List<EventDataModel.EventCommandMoveRoute>()));
                }

                instance.eventCommandIndex = index;
            }

            _eventManagementService.SaveEvent(_eventDataModels[instance.eventIndex]);

            if (show)
            {
                ShowSettingWindow(code);
                executionContentsWindow.ProcessTextSetting();
            }
        }

        public void SetEventCommand(int code, int eventIndex, int eventCommandIndex) {
            var instance = CommandSettingWindowParam.instance;
            _eventDataModels = executionContentsWindow.EventDataModels;

            instance.eventIndex = eventIndex;
            instance.eventCommandIndex = eventCommandIndex;

            MapEditor.WhenEventClosed();

            HideScreen();
            ShowSettingWindow(code);
        }
        
        private void ShowSettingWindow(int code) {
            if (code == -1) return;

            //テストプレイ実行後、インスタンスが破棄されている可能性があるため、取得しなおす
            if (_commandSetting == null)
                //明示的に初期化しなおし
                Awake();

            //音楽の再生を行っていた場合止める
            var gameObject = GameObject.FindWithTag("sound");
            if (gameObject != null)
                gameObject.transform.gameObject.GetComponent<AudioSource>().Stop();

            var instance = CommandSettingWindowParam.instance;
            instance.code = code;
            switch ((EventEnum) code)
            {
                case EventEnum.EVENT_CODE_MESSAGE_TEXT:
                    new TextWindow(_commandSetting, _eventDataModels, instance.eventIndex, instance.eventCommandIndex)
                        .Invoke();
                    break;
                case EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT:
                    new InputSelect(_commandSetting, _eventDataModels, instance.eventIndex, instance.eventCommandIndex)
                        .Invoke();
                    break;
                case EventEnum.EVENT_CODE_MESSAGE_INPUT_NUMBER:
                    new InputNumber(_commandSetting, _eventDataModels, instance.eventIndex, instance.eventCommandIndex)
                        .Invoke();
                    break;
                case EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_ITEM:
                    new SelectItem(_commandSetting, _eventDataModels, instance.eventIndex, instance.eventCommandIndex)
                        .Invoke();
                    break;
                case EventEnum.EVENT_CODE_MESSAGE_TEXT_SCROLL:
                    new CommandTextScroll(_commandSetting, _eventDataModels, instance.eventIndex,
                            instance.eventCommandIndex)
                        .Invoke();
                    break;
                case EventEnum.EVENT_CODE_CHARACTER_SHOW_ANIMATION:
                    new AnimationView(_commandSetting,
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_CHARACTER_SHOW_ICON:
                    new PopIconView(_commandSetting, _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_MOVE_PLACE:
                    new PlaceMove(_commandSetting, _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_MOVE_PLACE_SHIP:
                    new Vehicle(_commandSetting, _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_MOVE_RIDE_SHIP:
                    new RideShip(_commandSetting, _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_MOVE_SET_MOVE_POINT:
                case EventEnum.MOVEMENT_MOVE_AT_RANDOM:
                case EventEnum.MOVEMENT_MOVE_TOWARD_PLAYER:
                case EventEnum.MOVEMENT_MOVE_AWAY_FROM_PLAYER:
                    new MoveRoute(_commandSetting, _eventDataModels, instance.eventIndex, instance.eventCommandIndex)
                        .Invoke();
                    break;
                case EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE:
                    new CustomMove(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_STEP_MOVE:
                    new StepMove(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_CHANGE_MOVE_SPEED:
                    new ChangeMoveSpeed(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_CHANGE_MOVE_FREQUENCY:
                    new ChangeMoveFrequency(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_PASS_THROUGH:
                    new PassThrough(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_GAME_SWITCH:
                    new GameSwitch(_commandSetting, _eventDataModels, instance.eventIndex, instance.eventCommandIndex)
                        .Invoke();
                    break;
                case EventEnum.EVENT_CODE_GAME_VAL:
                    new GameVariable(_commandSetting, _eventDataModels, instance.eventIndex, instance.eventCommandIndex)
                        .Invoke();
                    break;
                case EventEnum.EVENT_CODE_GAME_SELF_SWITCH:
                    new GameSelfSwitch(_commandSetting,
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_GAME_TIMER:
                    new GameTimer(_commandSetting, _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_PICTURE_SHOW:
                    new PictureShow(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_PICTURE_MOVE:
                    new PictureMove(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_PICTURE_ROTATE:
                    new PictureRotate(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_PICTURE_CHANGE_COLOR:
                    new PictureChangeColor(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_PICTURE_ERASE:
                    new PictureErase(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_FLOW_IF:
                    new ConditionalBranch(_commandSetting.Q<VisualElement>(""), _eventDataModels, instance.eventIndex,
                        instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_FLOW_LOOP:
                    new Loop(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_FLOW_LOOP_BREAK:
                    new LoopEnd(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_FLOW_EVENT_BREAK:
                    new EventEnd(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_FLOW_JUMP_COMMON:
                    new CommonEvent(
                        _commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_FLOW_LABEL:
                    new Label(
                        _commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_FLOW_JUMP_LABEL:
                    new LabelJump(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_FLOW_ANNOTATION:
                    new Annotation(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;

                case EventEnum.EVENT_CODE_PARTY_CHANGE:
                    new PartyChange(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_CHARACTER_CHANGE_ALPHA:
                    new CharacterChangeAlpha(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_CHARACTER_CHANGE_WALK:
                    new CharacterChangeWalk(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_CHARACTER_CHANGE_PARTY:
                    new CharacterChangeParty(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_PARTY_GOLD:
                    new PartyGold(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_PARTY_ITEM:
                    new PartyItem(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_PARTY_WEAPON:
                    new PartyWeapon(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_PARTY_ARMS:
                    new PartyArms(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_TIMING_WAIT:
                    new Timing(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_SYSTEM_BATTLE_BGM:
                    new ChangeSound(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke(0);
                    break;
                case EventEnum.EVENT_CODE_SYSTEM_BATTLE_WIN:
                    new ChangeSound(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke(1);
                    break;
                case EventEnum.EVENT_CODE_SYSTEM_BATTLE_LOSE:
                    new ChangeSound(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke(2);
                    break;
                case EventEnum.EVENT_CODE_SYSTEM_SHIP_BGM:
                    new ChangeSound(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke(3);
                    break;
                case EventEnum.EVENT_CODE_SYSTEM_IS_SAVE:
                    new TwoToggle(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_SYSTEM_IS_MENU:
                    new TwoToggle(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_SYSTEM_IS_ENCOUNT:
                    new TwoToggle(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_SYSTEM_IS_SORT:
                    new TwoToggle(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_SYSTEM_WINDOW_COLOR:
                    new ChangeWindowColor(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_SYSTEM_CHANGE_ACTOR_IMAGE:
                    new ChangeActorImage(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_SYSTEM_CHANGE_SHIP_IMAGE:
                    new ChangeShipImage(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;

                case EventEnum.EVENT_CODE_ACTOR_CHANGE_HP:
                    new ChangeHp(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_ACTOR_CHANGE_MP:
                    new ChangeMp(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_ACTOR_CHANGE_TP:
                    new ChangeTp(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_ACTOR_CHANGE_STATE:
                    new ChangeState(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_ACTOR_HEAL:
                    new ActorHeal(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_ACTOR_CHANGE_EXP:
                    new ChangeExp(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_ACTOR_CHANGE_LEVEL:
                    new ChangeLevel(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_ACTOR_CHANGE_PARAMETER:
                    new ChangeParameter(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_ACTOR_CHANGE_SKILL:
                    new ChangeSkill(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_ACTOR_CHANGE_EQUIPMENT:
                    new ChangeEquipment(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_ACTOR_CHANGE_CLASS:
                    new ChangeClass(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_ACTOR_CHANGE_NAME:
                case EventEnum.EVENT_CODE_ACTOR_CHANGE_NICKNAME:
                case EventEnum.EVENT_CODE_ACTOR_CHANGE_PROFILE:
                    new ChangeName(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_DISPLAY_FADEOUT:
                    new DisplayFadeOut(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_DISPLAY_FADEIN:
                    new DisplayFadeIn(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_DISPLAY_CHANGE_COLOR:
                    new DisplayChangeColor(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_DISPLAY_FLASH:
                    new DisplayFlash(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_DISPLAY_SHAKE:
                    new DisplayShake(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_DISPLAY_WEATHER:
                    new DisplayWeather(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_AUDIO_BGM_PLAY:
                    new AudioBgmPlay(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_AUDIO_BGM_FADEOUT:
                    new AudioBgmFadeOut(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_AUDIO_BGM_SAVE:
                    new AudioBgmSave(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_AUDIO_BGM_CONTINUE:
                    new AudioBgmContinue(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_AUDIO_BGS_PLAY:
                    new AudioBgsPlay(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_AUDIO_BGS_FADEOUT:
                    new AudioBgsFadeOut(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_AUDIO_ME_PLAY:
                    new AudioMePlay(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_AUDIO_SE_PLAY:
                    new AudioSePlay(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_AUDIO_SE_STOP:
                    new AudioSeStop(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_AUDIO_MOVIE_PLAY:
                    new AudioMoviePlay(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_BATTLE_CHANGE_STATUS:
                case EventEnum.EVENT_CODE_BATTLE_CHANGE_MP:
                case EventEnum.EVENT_CODE_BATTLE_CHANGE_TP:
                    new BattleEnemyStatus(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_BATTLE_CHANGE_STATE:
                    new BattleEnemyState(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_BATTLE_APPEAR:
                    new BattleEnemyAppearance(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_BATTLE_TRANSFORM:
                    new BattleEnemyTransform(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_BATTLE_SHOW_ANIMATION:
                    new BattleEnemyAnimation(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_BATTLE_EXEC_COMMAND:
                    new BattleEnemyForced(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_BATTLE_STOP:
                    new BattleSuspension(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_MOVE_MAP_SCROLL:
                    new MapScroll(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_MAP_CHANGE_NAME:
                    new MapShowNameChange(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_MAP_CHANGE_TILE_SET:
                    new TileSetChange(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_MAP_CHANGE_BATTLE_BACKGROUND:
                    new BattleBackGroundChange(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_MAP_CHANGE_DISTANT_VIEW:
                    new DistantViewChange(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_MAP_GET_POINT:
                    new DesignatedLocationObtain(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;

                case EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG:
                    new BattleProcess(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG:
                    new ShopProcess(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG_LINE:
                    for (var i = instance.eventCommandIndex; i >= 0; i--)
                        if (_eventDataModels[instance.eventIndex].eventCommands[i].code ==
                            (int) EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG)
                        {
                            new ShopProcess(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                                instance.eventIndex, i).Invoke();
                            break;
                        }

                    break;
                case EventEnum.EVENT_CODE_SCENE_INPUT_NAME:
                    new NameInputProcess(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_SCENE_MENU_OPEN:
                    new OpenMenuWindow(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_SCENE_SAVE_OPEN:
                    new SaveMenuWindow(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_SCENE_GAME_OVER:
                    new GameOver(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_SCENE_GOTO_TITLE:
                    new GoBackTitleWindow(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_CHARACTER_IS_EVENT:
                    new CharacterIsEvent(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.EVENT_CODE_MOVE_SET_EVENT_POINT:
                    new MoveSetEventPoint(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;

                case EventEnum.MOVEMENT_WALKING_ANIMATION_ON:
                case EventEnum.MOVEMENT_WALKING_ANIMATION_OFF:
                case EventEnum.MOVEMENT_STEPPING_ANIMATION_ON:
                case EventEnum.MOVEMENT_STEPPING_ANIMATION_OFF:
                    new AnimationSetting(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.MOVEMENT_CHANGE_IMAGE:
                    new CharacterImage(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;

                case EventEnum.MOVEMENT_ONE_STEP_FORWARD:
                    new OneStepForward(_commandSetting.Q<VisualElement>(""),
                        _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.MOVEMENT_ONE_STEP_BACKWARD:
                    new OneStepBack(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;


                case EventEnum.MOVEMENT_TURN_DOWN:
                case EventEnum.MOVEMENT_TURN_LEFT:
                case EventEnum.MOVEMENT_TURN_RIGHT:
                case EventEnum.MOVEMENT_TURN_UP:
                case EventEnum.MOVEMENT_TURN_90_RIGHT:
                case EventEnum.MOVEMENT_TURN_90_LEFT:
                case EventEnum.MOVEMENT_TURN_180:
                case EventEnum.MOVEMENT_TURN_90_RIGHT_OR_LEFT:
                case EventEnum.MOVEMENT_TURN_AT_RANDOM:
                case EventEnum.MOVEMENT_TURN_TOWARD_PLAYER:
                case EventEnum.MOVEMENT_TURN_AWAY_FROM_PLAYER:
                    new Direction(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
                case EventEnum.MOVEMENT_JUMP:
                    new Jump(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;

                case EventEnum.EVENT_CODE_ADDON_COMMAND:
                    new AddonCommand(_commandSetting.Q<VisualElement>(""), _eventDataModels,
                        instance.eventIndex, instance.eventCommandIndex).Invoke();
                    break;
            }

            InitializeButtonLabel();
        }
        
        /// <summary>
        /// ボタンの幅で、ラベルに3点リーダーをつける
        /// </summary>
        protected void InitializeButtonLabel() {
            List<Button> data = new List<Button>();
            GetAllButton(data, _commandSetting);

            List<string> strWork = new List<string>();
            List<int> strWidth = new List<int>();



            for (int i = 0; i < data.Count; i++)
            {
                strWork.Add(data[i].text);
                strWidth.Add((int) data[i].contentRect.width);
            }

            _commandSetting.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                EditorUpdate();
            });

            void EditorUpdate() {
                for (int i = 0; i < data.Count; i++)
                {
                    if (strWidth[i] == (int) data[i].contentRect.width) continue;
                    for (int j = strWork[i].Length; j > 0; j--)
                    {
                        var s = strWork[i].Substring(0, j);
                        if (j != 1 && GetTextWidth(s) > data[i].contentRect.width)
                        {
                            continue;
                        }

                        if (j == strWork[i].Length)
                        {
                            data[i].text = s;
                            data[i].tooltip = "";
                            break;
                        }

                        if (j - 2 <= 1)
                        {
                            j = 3;
                        }

                        data[i].text = s.Substring(0, j - 2) + "...";
                        data[i].tooltip = strWork[i];
                        break;
                    }

                    strWidth[i] = (int) data[i].contentRect.width;
                }
            }
        }
        
        /// <summary>
        /// テキストの長さからWidthを取得
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        protected float GetTextWidth(string text) {
            try
            {
                var size = EditorStyles.label.CalcSize(text);
                return size.x;
            }
            catch (Exception)
            {
                //
            }
            return 0f;
        }
        
        /// <summary>
        /// 画面内に存在する全てのButtonを取得する
        /// </summary>
        /// <param name="data"></param>
        /// <param name="me"></param>
        protected void GetAllButton(List<Button> data, VisualElement me) {
            if (me is Button)
            {
                var parent = me.parent;
                if (!(parent is PopupFieldBase<string>))
                {
                    data.Add((Button) me);
                }
            }

            foreach (VisualElement child in me.Children())
                GetAllButton(data, child);
        }

        public void DeleteEvent(int index, bool last) {
            //デバッグ実行後に変数がnullになる可能性がある対処
            _eventManagementService ??= Hierarchy.Hierarchy.eventManagementService;

            var instance = CommandSettingWindowParam.instance;

            _eventDataModels = _eventManagementService.LoadEvent();

            for (var i = 0; i < _eventDataModels.Count; i++)
                if (instance.eventID == _eventDataModels[i].id)
                    if (instance.eventPage == _eventDataModels[i].page)
                    {
                        instance.eventIndex = i;
                        break;
                    }

            if (_eventDataModels[instance.eventIndex].eventCommands.Count <= index) return;
            if (_eventDataModels[instance.eventIndex].eventCommands[index].code == 0 && last == true) return;

            //削除
            _eventDataModels[instance.eventIndex].eventCommands.RemoveAt(index);

            _eventManagementService.SaveEvent(_eventDataModels[instance.eventIndex]);
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            executionContentsWindow.ProcessTextSetting();
        }

        public void SetEventId(string eventID) {
            CommandSettingWindowParam.instance.eventID = eventID;
        }

        public void SetEventPage(int eventPage) {
            CommandSettingWindowParam.instance.eventPage = eventPage;
        }

        public void HideScreen() {
            try
            {
                _commandSetting.Clear();
            }
            catch (Exception)
            {
            }
        }

        public class CommandSettingWindowParam : ScriptableSingleton<CommandSettingWindowParam>
        {
            public int    code = -1;
            public int    eventCommandIndex;
            public string eventID;
            public int    eventIndex = -1;
            public int    eventPage;
        }
    }
}