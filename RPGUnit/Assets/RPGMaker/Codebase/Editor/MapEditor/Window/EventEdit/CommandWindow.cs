using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.Window;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Window.EventEdit
{
    public class CommandWindow : BaseModalWindow
    {
        private VisualElement _root;

        /// <summary>
        /// ボタン名とイベントコードの紐づけ設定
        /// </summary>
        private static Dictionary<string, EventEnum> EventCommandList = new Dictionary<string, EventEnum>
        {
            {"command_text_button", EventEnum.EVENT_CODE_MESSAGE_TEXT},
            {"command_inputSelect_button", EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT},
            {"command_inbutNumber_button", EventEnum.EVENT_CODE_MESSAGE_INPUT_NUMBER},
            {"command_input_selectItem_button", EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_ITEM},
            {"command_textScroll_button", EventEnum.EVENT_CODE_MESSAGE_TEXT_SCROLL},
            {"CharacterShowAnimation", EventEnum.EVENT_CODE_CHARACTER_SHOW_ANIMATION},
            {"CharacterShowIcon", EventEnum.EVENT_CODE_CHARACTER_SHOW_ICON},
            {"AnimationSetting", EventEnum.MOVEMENT_WALKING_ANIMATION_ON},
            {"CharacterImageSetting", EventEnum.MOVEMENT_CHANGE_IMAGE},
            {"PlaceMove", EventEnum.EVENT_CODE_MOVE_PLACE},
            {"MoveRouteSetting", EventEnum.EVENT_CODE_MOVE_SET_MOVE_POINT},
            {"OneStepForward", EventEnum.MOVEMENT_ONE_STEP_FORWARD},
            {"OneStepBack", EventEnum.MOVEMENT_ONE_STEP_BACKWARD},
            {"Jump", EventEnum.MOVEMENT_JUMP},
            {"CustomMove", EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE},
            {"StepMove", EventEnum.EVENT_CODE_STEP_MOVE},
            {"ChangeMoveSpeed", EventEnum.EVENT_CODE_CHANGE_MOVE_SPEED},
            {"ChangeMoveFrequency", EventEnum.EVENT_CODE_CHANGE_MOVE_FREQUENCY},
            {"PassThrough", EventEnum.EVENT_CODE_PASS_THROUGH},
            {"Direction", EventEnum.MOVEMENT_TURN_DOWN},
            {"Vehicle", EventEnum.EVENT_CODE_MOVE_PLACE_SHIP},
            {"RideVehicle", EventEnum.EVENT_CODE_MOVE_RIDE_SHIP},
            {"SwitchControl", EventEnum.EVENT_CODE_GAME_SWITCH},
            {"VariableControl", EventEnum.EVENT_CODE_GAME_VAL},
            {"SelfSwitchControl", EventEnum.EVENT_CODE_GAME_SELF_SWITCH},
            {"TimerControl", EventEnum.EVENT_CODE_GAME_TIMER},
            {"ShowPicture", EventEnum.EVENT_CODE_PICTURE_SHOW},
            {"MovePicture", EventEnum.EVENT_CODE_PICTURE_MOVE},
            {"RoutePicture", EventEnum.EVENT_CODE_PICTURE_ROTATE},
            {"ChangeColorPicture", EventEnum.EVENT_CODE_PICTURE_CHANGE_COLOR},
            {"ErasurePicture", EventEnum.EVENT_CODE_PICTURE_ERASE},
            {"ConditionalBranch", EventEnum.EVENT_CODE_FLOW_IF},
            {"Loop", EventEnum.EVENT_CODE_FLOW_LOOP},
            {"LoopBreak", EventEnum.EVENT_CODE_FLOW_LOOP_BREAK},
            {"EventBreak", EventEnum.EVENT_CODE_FLOW_EVENT_BREAK},
            {"CommonEvent", EventEnum.EVENT_CODE_FLOW_JUMP_COMMON},
            {"Label", EventEnum.EVENT_CODE_FLOW_LABEL},
            {"LabelJump", EventEnum.EVENT_CODE_FLOW_JUMP_LABEL},
            {"Annotation", EventEnum.EVENT_CODE_FLOW_ANNOTATION},
            {"ChangePartyMenber", EventEnum.EVENT_CODE_PARTY_CHANGE},
            {"ChangeAlpha", EventEnum.EVENT_CODE_CHARACTER_CHANGE_ALPHA},
            {"ChangeWalk", EventEnum.EVENT_CODE_CHARACTER_CHANGE_WALK},
            {"ChangeParty", EventEnum.EVENT_CODE_CHARACTER_CHANGE_PARTY},
            {"PartyGold", EventEnum.EVENT_CODE_PARTY_GOLD},
            {"PartyItem", EventEnum.EVENT_CODE_PARTY_ITEM},
            {"PartyWeapon", EventEnum.EVENT_CODE_PARTY_WEAPON},
            {"PartyArmor", EventEnum.EVENT_CODE_PARTY_ARMS},
            {"Timing", EventEnum.EVENT_CODE_TIMING_WAIT},
            {"ChangeBattleBGM", EventEnum.EVENT_CODE_SYSTEM_BATTLE_BGM},
            {"ChangeVictoryME", EventEnum.EVENT_CODE_SYSTEM_BATTLE_WIN},
            {"ChangeLoseME", EventEnum.EVENT_CODE_SYSTEM_BATTLE_LOSE},
            {"ChangeVehicleBGM", EventEnum.EVENT_CODE_SYSTEM_SHIP_BGM},
            {"ChangeProhibitedSave", EventEnum.EVENT_CODE_SYSTEM_IS_SAVE},
            {"ChangeProhibitedMenu", EventEnum.EVENT_CODE_SYSTEM_IS_MENU},
            {"ChangeProhibitedEncounter", EventEnum.EVENT_CODE_SYSTEM_IS_ENCOUNT},
            {"ChangeProhibitedSort", EventEnum.EVENT_CODE_SYSTEM_IS_SORT},
            {"ChangeWindowColor", EventEnum.EVENT_CODE_SYSTEM_WINDOW_COLOR},
            {"ChangeActorPicture", EventEnum.EVENT_CODE_SYSTEM_CHANGE_ACTOR_IMAGE},
            {"ChangeVehiclePicture", EventEnum.EVENT_CODE_SYSTEM_CHANGE_SHIP_IMAGE},
            {"hpButton", EventEnum.EVENT_CODE_ACTOR_CHANGE_HP},
            {"mpButton", EventEnum.EVENT_CODE_ACTOR_CHANGE_MP},
            {"tpButton", EventEnum.EVENT_CODE_ACTOR_CHANGE_TP},
            {"stateChange", EventEnum.EVENT_CODE_ACTOR_CHANGE_STATE},
            {"fullRecovery", EventEnum.EVENT_CODE_ACTOR_HEAL},
            {"expButton", EventEnum.EVENT_CODE_ACTOR_CHANGE_EXP},
            {"levelButton", EventEnum.EVENT_CODE_ACTOR_CHANGE_LEVEL},
            {"statusButton", EventEnum.EVENT_CODE_ACTOR_CHANGE_PARAMETER},
            {"skillButton", EventEnum.EVENT_CODE_ACTOR_CHANGE_SKILL},
            {"equipButton", EventEnum.EVENT_CODE_ACTOR_CHANGE_EQUIPMENT},
            {"classButton", EventEnum.EVENT_CODE_ACTOR_CHANGE_CLASS},
            {"actorSetting", EventEnum.EVENT_CODE_ACTOR_CHANGE_NAME},
            {"ScreenFadeOut", EventEnum.EVENT_CODE_DISPLAY_FADEOUT},
            {"ScreenFadeIn", EventEnum.EVENT_CODE_DISPLAY_FADEIN},
            {"ScreenChangeColor", EventEnum.EVENT_CODE_DISPLAY_CHANGE_COLOR},
            {"ScreenFlash", EventEnum.EVENT_CODE_DISPLAY_FLASH},
            {"ScreenShake", EventEnum.EVENT_CODE_DISPLAY_SHAKE},
            {"WeatherSetting", EventEnum.EVENT_CODE_DISPLAY_WEATHER},
            {"BGMPerformance", EventEnum.EVENT_CODE_AUDIO_BGM_PLAY},
            {"BGMFeadOut", EventEnum.EVENT_CODE_AUDIO_BGM_FADEOUT},
            {"BGMSave", EventEnum.EVENT_CODE_AUDIO_BGM_SAVE},
            {"BGMPlay", EventEnum.EVENT_CODE_AUDIO_BGM_CONTINUE},
            {"BGSPerformance", EventEnum.EVENT_CODE_AUDIO_BGS_PLAY},
            {"BGSFeadOut", EventEnum.EVENT_CODE_AUDIO_BGS_FADEOUT},
            {"MEPerformance", EventEnum.EVENT_CODE_AUDIO_ME_PLAY},
            {"SEPerformance", EventEnum.EVENT_CODE_AUDIO_SE_PLAY},
            {"SEStop", EventEnum.EVENT_CODE_AUDIO_SE_STOP},
            {"MoviePlay", EventEnum.EVENT_CODE_AUDIO_MOVIE_PLAY},
            {"battle_enemy_status", EventEnum.EVENT_CODE_BATTLE_CHANGE_STATUS},
            {"battle_enemy_state", EventEnum.EVENT_CODE_BATTLE_CHANGE_STATE},
            {"battle_enemy_appearance", EventEnum.EVENT_CODE_BATTLE_APPEAR},
            {"battle_enemy_transform", EventEnum.EVENT_CODE_BATTLE_TRANSFORM},
            {"battle_enemy_animation", EventEnum.EVENT_CODE_BATTLE_SHOW_ANIMATION},
            {"battle_enemy_battleForced", EventEnum.EVENT_CODE_BATTLE_EXEC_COMMAND},
            {"battle_enemy_stop", EventEnum.EVENT_CODE_BATTLE_STOP},
            {"MapScroll", EventEnum.EVENT_CODE_MOVE_MAP_SCROLL},
            {"MapNameChange", EventEnum.EVENT_CODE_MAP_CHANGE_NAME},
            {"BattleBackGroundChange", EventEnum.EVENT_CODE_MAP_CHANGE_BATTLE_BACKGROUND},
            {"DistantViewChange", EventEnum.EVENT_CODE_MAP_CHANGE_DISTANT_VIEW},
            {"MapGetPoint", EventEnum.EVENT_CODE_MAP_GET_POINT},
            {"BattleProcess", EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG},
            {"ShopProcess", EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG},
            {"NameInput", EventEnum.EVENT_CODE_SCENE_INPUT_NAME},
            {"OpenMenu", EventEnum.EVENT_CODE_SCENE_MENU_OPEN},
            {"OpenSave", EventEnum.EVENT_CODE_SCENE_SAVE_OPEN},
            {"GameOver", EventEnum.EVENT_CODE_SCENE_GAME_OVER},
            {"GOTOTitle", EventEnum.EVENT_CODE_SCENE_GOTO_TITLE},
            {"CharacterIsEvent", EventEnum.EVENT_CODE_CHARACTER_IS_EVENT},
            {"MoveSetEventPoint", EventEnum.EVENT_CODE_MOVE_SET_EVENT_POINT},
            {"AddonCommands", EventEnum.EVENT_CODE_ADDON_COMMAND}
        };

        private readonly string commandUxml =
            "Assets/RPGMaker/Codebase/Editor/Inspector/Map/Asset/inspector_mapEvent_command.uxml";

        public override void ShowWindow(string modalTitle, CallBackWidow callBack) {
            //CB登録
            if (callBack != null) _callBackWindow = callBack;

            //Window表示
            var w = (CommandWindow)WindowLayoutManager.GetActiveWindow(
                WindowLayoutManager.WindowLayoutId.MapEventCommandWindow);

            //タイトル設定
            w.titleContent = new GUIContent(EditorLocalize.LocalizeText("WORD_1570"));
            _root = rootVisualElement;
            _root.Clear();

            //サイズ指定
            w.minSize = new Vector2(360, 600);

            //UI描画
            var commandTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(commandUxml);
            VisualElement commandFromUxml = commandTree.CloneTree();
            EditorLocalize.LocalizeElements(commandFromUxml);
            commandFromUxml.style.flexGrow = 1;
            _root.Add(commandFromUxml);

            ButtonSetting();
        }

        /// <summary>
        /// 各ボタン押下時のCB登録
        /// </summary>
        private void ButtonSetting() {
            foreach(var data in EventCommandList)
            {
                Button button = _root.Query<Button>(data.Key);
                button.clickable.clicked += () =>
                {
                    var code = (int) data.Value;
                    _callBackWindow(code.ToString());
                    Close();
                };
            }
        }

        /// <summary>
        /// マップイベント、バトルイベントに応じてボタンの有効状態の切り替えを行う
        /// </summary>
        /// <param name="enabled"></param>
        public void SetBattleOrMapEnabled(EventCodeList.EventType eventType) {
            foreach (var data in EventCommandList)
            {
                Button button = _root.Query<Button>(data.Key);
                if (!EventCodeList.CheckEventCodeExecute((int)data.Value, eventType, false))
                    button.SetEnabled(false);
                else
                    button.SetEnabled(true);
            }
        }

        /// <summary>
        ///     カスタム移動コマンド内で使えないコマンドを無効化。
        /// </summary>
        public void SetCustomMoveEnabled() {
            foreach (var name in new List<string>() { "MoveRouteSetting", "CustomMove", "ConditionalBranch", "Loop", "LoopBreak", "EventBreak", "CommonEvent", "Label", "LabelJump", "Annotation" })
            {
                Button button = _root.Query<Button>(name);
                button?.SetEnabled(false);
            }
        }
    }
}