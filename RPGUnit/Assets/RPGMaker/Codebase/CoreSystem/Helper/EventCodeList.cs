using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Helper
{
    /// <summary>
    /// 各画面（マップ、バトル）で利用できるイベントコマンド一覧をまとめたクラス
    /// </summary>
    public static class EventCodeList
    {
        public enum EventType
        {
            Map,
            Battle,
            Common
        }

        /// <summary>
        /// マップで実行可能なイベント一覧
        /// </summary>
        public static List<EventEnum> Map = new List<EventEnum> { 
                // 空白
                0,

                //メッセージ,
                EventEnum.EVENT_CODE_MESSAGE_TEXT,
                EventEnum.EVENT_CODE_MESSAGE_TEXT_ONE_LINE,
                EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT,
                EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_SELECTED,
                EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_CANCELED,
                EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_END,
                EventEnum.EVENT_CODE_MESSAGE_INPUT_NUMBER,
                EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_ITEM,
                EventEnum.EVENT_CODE_MESSAGE_TEXT_SCROLL,
                EventEnum.EVENT_CODE_MESSAGE_TEXT_SCROLL_ONE_LINE,

                //キャラクター,
                EventEnum.EVENT_CODE_CHARACTER_SHOW_ANIMATION,
                EventEnum.EVENT_CODE_CHARACTER_SHOW_ICON,
                EventEnum.EVENT_CODE_CHARACTER_IS_EVENT,
                EventEnum.MOVEMENT_WALKING_ANIMATION_ON,
                EventEnum.MOVEMENT_WALKING_ANIMATION_OFF,
                EventEnum.MOVEMENT_STEPPING_ANIMATION_ON,
                EventEnum.MOVEMENT_STEPPING_ANIMATION_OFF,
                EventEnum.MOVEMENT_CHANGE_IMAGE,

                EventEnum.EVENT_CODE_MOVE_PLACE,
                EventEnum.EVENT_CODE_MOVE_SET_MOVE_POINT,
                EventEnum.EVENT_CODE_MOVE_SET_EVENT_POINT,
                EventEnum.MOVEMENT_MOVE_AT_RANDOM,
                EventEnum.MOVEMENT_MOVE_TOWARD_PLAYER,
                EventEnum.MOVEMENT_MOVE_AWAY_FROM_PLAYER,
                EventEnum.MOVEMENT_ONE_STEP_FORWARD,
                EventEnum.MOVEMENT_ONE_STEP_BACKWARD,
                EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE,
                EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE_END,
                EventEnum.EVENT_CODE_STEP_MOVE,
                EventEnum.EVENT_CODE_CHANGE_MOVE_SPEED,
                EventEnum.EVENT_CODE_CHANGE_MOVE_FREQUENCY,
                EventEnum.EVENT_CODE_PASS_THROUGH,
                EventEnum.MOVEMENT_JUMP,
                EventEnum.MOVEMENT_TURN_DOWN,
                EventEnum.MOVEMENT_TURN_LEFT,
                EventEnum.MOVEMENT_TURN_RIGHT,
                EventEnum.MOVEMENT_TURN_UP,
                EventEnum.MOVEMENT_TURN_90_RIGHT,
                EventEnum.MOVEMENT_TURN_90_LEFT,
                EventEnum.MOVEMENT_TURN_180,
                EventEnum.MOVEMENT_TURN_90_RIGHT_OR_LEFT,
                EventEnum.MOVEMENT_TURN_AT_RANDOM,
                EventEnum.MOVEMENT_TURN_TOWARD_PLAYER,
                EventEnum.MOVEMENT_TURN_AWAY_FROM_PLAYER,
                EventEnum.EVENT_CODE_MOVE_PLACE_SHIP,
                EventEnum.EVENT_CODE_MOVE_RIDE_SHIP,

                //タイミング,
                EventEnum.EVENT_CODE_TIMING_WAIT,

                //パーティ,
                EventEnum.EVENT_CODE_PARTY_CHANGE,
                EventEnum.EVENT_CODE_CHARACTER_CHANGE_ALPHA,
                EventEnum.EVENT_CODE_CHARACTER_CHANGE_WALK,
                EventEnum.EVENT_CODE_CHARACTER_CHANGE_PARTY,
                EventEnum.EVENT_CODE_PARTY_GOLD,
                EventEnum.EVENT_CODE_PARTY_ITEM,
                EventEnum.EVENT_CODE_PARTY_WEAPON,
                EventEnum.EVENT_CODE_PARTY_ARMS,

                //アクター,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_HP,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_MP,
                //EventEnum.EVENT_CODE_ACTOR_CHANGE_TP,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_STATE,
                EventEnum.EVENT_CODE_ACTOR_HEAL,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_EXP,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_LEVEL,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_PARAMETER,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_SKILL,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_EQUIPMENT,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_CLASS,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_NAME,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_NICKNAME,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_PROFILE,

                //ゲーム進行,
                EventEnum.EVENT_CODE_GAME_SWITCH,
                EventEnum.EVENT_CODE_GAME_VAL,
                EventEnum.EVENT_CODE_GAME_SELF_SWITCH,
                EventEnum.EVENT_CODE_GAME_TIMER,

                //フロー制御
                EventEnum.EVENT_CODE_FLOW_IF,
                EventEnum.EVENT_CODE_FLOW_ELSE,
                EventEnum.EVENT_CODE_FLOW_ENDIF,
                EventEnum.EVENT_CODE_FLOW_AND,
                EventEnum.EVENT_CODE_FLOW_OR,
                EventEnum.EVENT_CODE_FLOW_LOOP,
                EventEnum.EVENT_CODE_FLOW_LOOP_BREAK,
                EventEnum.EVENT_CODE_FLOW_LOOP_END,
                EventEnum.EVENT_CODE_FLOW_EVENT_BREAK,
                EventEnum.EVENT_CODE_FLOW_JUMP_COMMON,
                EventEnum.EVENT_CODE_FLOW_LABEL,
                EventEnum.EVENT_CODE_FLOW_JUMP_LABEL,
                EventEnum.EVENT_CODE_FLOW_ANNOTATION,

                //ピクチャ,
                EventEnum.EVENT_CODE_PICTURE_SHOW,
                EventEnum.EVENT_CODE_PICTURE_MOVE,
                EventEnum.EVENT_CODE_PICTURE_ROTATE,
                EventEnum.EVENT_CODE_PICTURE_CHANGE_COLOR,
                EventEnum.EVENT_CODE_PICTURE_ERASE,

                //画面,
                EventEnum.EVENT_CODE_DISPLAY_FADEOUT,
                EventEnum.EVENT_CODE_DISPLAY_FADEIN,
                EventEnum.EVENT_CODE_DISPLAY_CHANGE_COLOR,
                EventEnum.EVENT_CODE_DISPLAY_FLASH,
                EventEnum.EVENT_CODE_DISPLAY_SHAKE,
                EventEnum.EVENT_CODE_DISPLAY_WEATHER,

                //マップ,
                EventEnum.EVENT_CODE_MAP_CHANGE_NAME,
                EventEnum.EVENT_CODE_MAP_CHANGE_BATTLE_BACKGROUND,
                EventEnum.EVENT_CODE_MAP_GET_POINT,
                EventEnum.EVENT_CODE_MAP_CHANGE_DISTANT_VIEW,
                EventEnum.EVENT_CODE_MOVE_MAP_SCROLL,

                //システム,
                EventEnum.EVENT_CODE_SYSTEM_BATTLE_BGM,
                EventEnum.EVENT_CODE_SYSTEM_BATTLE_WIN,
                EventEnum.EVENT_CODE_SYSTEM_BATTLE_LOSE,
                EventEnum.EVENT_CODE_SYSTEM_SHIP_BGM,
                EventEnum.EVENT_CODE_SYSTEM_CHANGE_ACTOR_IMAGE,
                EventEnum.EVENT_CODE_SYSTEM_CHANGE_SHIP_IMAGE,
                EventEnum.EVENT_CODE_SYSTEM_IS_MENU,
                EventEnum.EVENT_CODE_SYSTEM_IS_SAVE,
                EventEnum.EVENT_CODE_SYSTEM_IS_ENCOUNT,
                EventEnum.EVENT_CODE_SYSTEM_IS_SORT,
                EventEnum.EVENT_CODE_SYSTEM_WINDOW_COLOR,

                //オーディオ・ビデオ,
                EventEnum.EVENT_CODE_AUDIO_BGM_PLAY,
                EventEnum.EVENT_CODE_AUDIO_BGS_PLAY,
                EventEnum.EVENT_CODE_AUDIO_ME_PLAY,
                EventEnum.EVENT_CODE_AUDIO_SE_PLAY,
                EventEnum.EVENT_CODE_AUDIO_SE_STOP,
                EventEnum.EVENT_CODE_AUDIO_BGM_FADEOUT,
                EventEnum.EVENT_CODE_AUDIO_BGS_FADEOUT,
                EventEnum.EVENT_CODE_AUDIO_BGM_SAVE,
                EventEnum.EVENT_CODE_AUDIO_BGM_CONTINUE,
                EventEnum.EVENT_CODE_AUDIO_MOVIE_PLAY,

                //シーン制御,
                EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG,
                EventEnum.EVENT_CODE_SCENE_GAME_OVER,
                EventEnum.EVENT_CODE_SCENE_GOTO_TITLE,
                EventEnum.EVENT_CODE_SCENE_INPUT_NAME,
                EventEnum.EVENT_CODE_SCENE_MENU_OPEN,
                EventEnum.EVENT_CODE_SCENE_SAVE_OPEN,
                EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG,
                //EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG_LINE,
                EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_WIN,
                EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_ESCAPE,
                EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_LOSE,
                EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_END,

                //バトル,
                //EventEnum.EVENT_CODE_BATTLE_CHANGE_STATUS,
                //EventEnum.EVENT_CODE_BATTLE_CHANGE_STATE,
                //EventEnum.EVENT_CODE_BATTLE_APPEAR,
                //EventEnum.EVENT_CODE_BATTLE_TRANSFORM,
                //EventEnum.EVENT_CODE_BATTLE_SHOW_ANIMATION,
                //EventEnum.EVENT_CODE_BATTLE_EXEC_COMMAND,
                //EventEnum.EVENT_CODE_BATTLE_STOP,

                //アドオン,
                EventEnum.EVENT_CODE_ADDON_COMMAND
        };

        /// <summary>
        /// バトルで実行可能なイベント一覧
        /// </summary>
        public static List<EventEnum> Battle = new List<EventEnum> { 
                // 空白
                0,

                //メッセージ,
                EventEnum.EVENT_CODE_MESSAGE_TEXT,
                EventEnum.EVENT_CODE_MESSAGE_TEXT_ONE_LINE,
                EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT,
                EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_SELECTED,
                EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_CANCELED,
                EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_END,
                EventEnum.EVENT_CODE_MESSAGE_INPUT_NUMBER,
                EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_ITEM,
                EventEnum.EVENT_CODE_MESSAGE_TEXT_SCROLL,
                EventEnum.EVENT_CODE_MESSAGE_TEXT_SCROLL_ONE_LINE,

                //キャラクター,
                EventEnum.EVENT_CODE_CHARACTER_SHOW_ANIMATION,
                EventEnum.EVENT_CODE_CHARACTER_SHOW_ICON,
                //EventEnum.EVENT_CODE_CHARACTER_IS_EVENT,
                EventEnum.MOVEMENT_WALKING_ANIMATION_ON,
                EventEnum.MOVEMENT_WALKING_ANIMATION_OFF,
                EventEnum.MOVEMENT_STEPPING_ANIMATION_ON,
                EventEnum.MOVEMENT_STEPPING_ANIMATION_OFF,
                EventEnum.MOVEMENT_CHANGE_IMAGE,

                //EventEnum.EVENT_CODE_MOVE_PLACE,
                //EventEnum.EVENT_CODE_MOVE_SET_MOVE_POINT,
                //EventEnum.EVENT_CODE_MOVE_SET_EVENT_POINT,
                //EventEnum.MOVEMENT_MOVE_AT_RANDOM,
                //EventEnum.MOVEMENT_MOVE_TOWARD_PLAYER,
                //EventEnum.MOVEMENT_MOVE_AWAY_FROM_PLAYER,
                //EventEnum.MOVEMENT_ONE_STEP_FORWARD,
                //EventEnum.MOVEMENT_ONE_STEP_BACKWARD,
                //EventEnum.MOVEMENT_JUMP,
                //EventEnum.MOVEMENT_TURN_DOWN,
                //EventEnum.MOVEMENT_TURN_LEFT,
                //EventEnum.MOVEMENT_TURN_RIGHT,
                //EventEnum.MOVEMENT_TURN_UP,
                //EventEnum.MOVEMENT_TURN_90_RIGHT,
                //EventEnum.MOVEMENT_TURN_90_LEFT,
                //EventEnum.MOVEMENT_TURN_180,
                //EventEnum.MOVEMENT_TURN_90_RIGHT_OR_LEFT,
                //EventEnum.MOVEMENT_TURN_AT_RANDOM,
                //EventEnum.MOVEMENT_TURN_TOWARD_PLAYER,
                //EventEnum.MOVEMENT_TURN_AWAY_FROM_PLAYER,
                //EventEnum.EVENT_CODE_MOVE_PLACE_SHIP,
                //EventEnum.EVENT_CODE_MOVE_RIDE_SHIP,

                //タイミング,
                EventEnum.EVENT_CODE_TIMING_WAIT,

                //パーティ,
                EventEnum.EVENT_CODE_PARTY_CHANGE,
                EventEnum.EVENT_CODE_CHARACTER_CHANGE_ALPHA,
                //EventEnum.EVENT_CODE_CHARACTER_CHANGE_WALK,
                //EventEnum.EVENT_CODE_CHARACTER_CHANGE_PARTY,
                EventEnum.EVENT_CODE_PARTY_GOLD,
                EventEnum.EVENT_CODE_PARTY_ITEM,
                EventEnum.EVENT_CODE_PARTY_WEAPON,
                EventEnum.EVENT_CODE_PARTY_ARMS,

                //アクター,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_HP,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_MP,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_TP,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_STATE,
                EventEnum.EVENT_CODE_ACTOR_HEAL,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_EXP,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_LEVEL,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_PARAMETER,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_SKILL,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_EQUIPMENT,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_CLASS,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_NAME,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_NICKNAME,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_PROFILE,

                //ゲーム進行,
                EventEnum.EVENT_CODE_GAME_SWITCH,
                EventEnum.EVENT_CODE_GAME_VAL,
                EventEnum.EVENT_CODE_GAME_SELF_SWITCH,
                EventEnum.EVENT_CODE_GAME_TIMER,

                //フロー制御
                EventEnum.EVENT_CODE_FLOW_IF,
                EventEnum.EVENT_CODE_FLOW_ELSE,
                EventEnum.EVENT_CODE_FLOW_ENDIF,
                EventEnum.EVENT_CODE_FLOW_AND,
                EventEnum.EVENT_CODE_FLOW_OR,
                EventEnum.EVENT_CODE_FLOW_LOOP,
                EventEnum.EVENT_CODE_FLOW_LOOP_BREAK,
                EventEnum.EVENT_CODE_FLOW_LOOP_END,
                EventEnum.EVENT_CODE_FLOW_EVENT_BREAK,
                EventEnum.EVENT_CODE_FLOW_JUMP_COMMON,
                EventEnum.EVENT_CODE_FLOW_LABEL,
                EventEnum.EVENT_CODE_FLOW_JUMP_LABEL,
                EventEnum.EVENT_CODE_FLOW_ANNOTATION,

                //ピクチャ,
                EventEnum.EVENT_CODE_PICTURE_SHOW,
                EventEnum.EVENT_CODE_PICTURE_MOVE,
                EventEnum.EVENT_CODE_PICTURE_ROTATE,
                EventEnum.EVENT_CODE_PICTURE_CHANGE_COLOR,
                EventEnum.EVENT_CODE_PICTURE_ERASE,

                //画面,
                EventEnum.EVENT_CODE_DISPLAY_FADEOUT,
                EventEnum.EVENT_CODE_DISPLAY_FADEIN,
                EventEnum.EVENT_CODE_DISPLAY_CHANGE_COLOR,
                EventEnum.EVENT_CODE_DISPLAY_FLASH,
                EventEnum.EVENT_CODE_DISPLAY_SHAKE,
                //EventEnum.EVENT_CODE_DISPLAY_WEATHER,

                //マップ,
                //EventEnum.EVENT_CODE_MAP_CHANGE_NAME,
                EventEnum.EVENT_CODE_MAP_CHANGE_BATTLE_BACKGROUND,
                //EventEnum.EVENT_CODE_MAP_GET_POINT,
                //EventEnum.EVENT_CODE_MAP_CHANGE_DISTANT_VIEW,
                //EventEnum.EVENT_CODE_MOVE_MAP_SCROLL,

                //システム,
                EventEnum.EVENT_CODE_SYSTEM_BATTLE_BGM,
                EventEnum.EVENT_CODE_SYSTEM_BATTLE_WIN,
                EventEnum.EVENT_CODE_SYSTEM_BATTLE_LOSE,
                EventEnum.EVENT_CODE_SYSTEM_SHIP_BGM,
                EventEnum.EVENT_CODE_SYSTEM_CHANGE_ACTOR_IMAGE,
                EventEnum.EVENT_CODE_SYSTEM_CHANGE_SHIP_IMAGE,
                EventEnum.EVENT_CODE_SYSTEM_IS_MENU,
                EventEnum.EVENT_CODE_SYSTEM_IS_SAVE,
                EventEnum.EVENT_CODE_SYSTEM_IS_ENCOUNT,
                EventEnum.EVENT_CODE_SYSTEM_IS_SORT,
                EventEnum.EVENT_CODE_SYSTEM_WINDOW_COLOR,

                //オーディオ・ビデオ,
                EventEnum.EVENT_CODE_AUDIO_BGM_PLAY,
                EventEnum.EVENT_CODE_AUDIO_BGS_PLAY,
                EventEnum.EVENT_CODE_AUDIO_ME_PLAY,
                EventEnum.EVENT_CODE_AUDIO_SE_PLAY,
                EventEnum.EVENT_CODE_AUDIO_SE_STOP,
                EventEnum.EVENT_CODE_AUDIO_BGM_FADEOUT,
                EventEnum.EVENT_CODE_AUDIO_BGS_FADEOUT,
                EventEnum.EVENT_CODE_AUDIO_BGM_SAVE,
                EventEnum.EVENT_CODE_AUDIO_BGM_CONTINUE,
                EventEnum.EVENT_CODE_AUDIO_MOVIE_PLAY,

                //シーン制御,
                EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG,
                EventEnum.EVENT_CODE_SCENE_GAME_OVER,
                EventEnum.EVENT_CODE_SCENE_GOTO_TITLE,
                //EventEnum.EVENT_CODE_SCENE_INPUT_NAME,
                //EventEnum.EVENT_CODE_SCENE_MENU_OPEN,
                //EventEnum.EVENT_CODE_SCENE_SAVE_OPEN,
                //EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG,
                //EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG_LINE,
                EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_WIN,
                EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_ESCAPE,
                EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_LOSE,
                EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_END,

                //バトル,
                EventEnum.EVENT_CODE_BATTLE_CHANGE_STATUS,
                EventEnum.EVENT_CODE_BATTLE_CHANGE_STATE,
                EventEnum.EVENT_CODE_BATTLE_APPEAR,
                EventEnum.EVENT_CODE_BATTLE_TRANSFORM,
                EventEnum.EVENT_CODE_BATTLE_SHOW_ANIMATION,
                EventEnum.EVENT_CODE_BATTLE_EXEC_COMMAND,
                EventEnum.EVENT_CODE_BATTLE_STOP,

                //アドオン,
                EventEnum.EVENT_CODE_ADDON_COMMAND
        };

        /// <summary>
        /// コモンイベントで実行可能なイベント一覧
        /// </summary>
        public static List<EventEnum> Common = new List<EventEnum> { 
                // 空白
                0,

                //メッセージ,
                EventEnum.EVENT_CODE_MESSAGE_TEXT,
                EventEnum.EVENT_CODE_MESSAGE_TEXT_ONE_LINE,
                EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT,
                EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_SELECTED,
                EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_CANCELED,
                EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_END,
                EventEnum.EVENT_CODE_MESSAGE_INPUT_NUMBER,
                EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_ITEM,
                EventEnum.EVENT_CODE_MESSAGE_TEXT_SCROLL,
                EventEnum.EVENT_CODE_MESSAGE_TEXT_SCROLL_ONE_LINE,

                //キャラクター,
                EventEnum.EVENT_CODE_CHARACTER_SHOW_ANIMATION,
                EventEnum.EVENT_CODE_CHARACTER_SHOW_ICON,
                EventEnum.EVENT_CODE_CHARACTER_IS_EVENT,
                EventEnum.MOVEMENT_WALKING_ANIMATION_ON,
                EventEnum.MOVEMENT_WALKING_ANIMATION_OFF,
                EventEnum.MOVEMENT_STEPPING_ANIMATION_ON,
                EventEnum.MOVEMENT_STEPPING_ANIMATION_OFF,
                EventEnum.MOVEMENT_CHANGE_IMAGE,

                EventEnum.EVENT_CODE_MOVE_PLACE,
                EventEnum.EVENT_CODE_MOVE_SET_MOVE_POINT,
                EventEnum.EVENT_CODE_MOVE_SET_EVENT_POINT,
                EventEnum.MOVEMENT_MOVE_AT_RANDOM,
                EventEnum.MOVEMENT_MOVE_TOWARD_PLAYER,
                EventEnum.MOVEMENT_MOVE_AWAY_FROM_PLAYER,
                EventEnum.MOVEMENT_ONE_STEP_FORWARD,
                EventEnum.MOVEMENT_ONE_STEP_BACKWARD,
                EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE,
                EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE_END,
                EventEnum.EVENT_CODE_STEP_MOVE,
                EventEnum.EVENT_CODE_CHANGE_MOVE_SPEED,
                EventEnum.EVENT_CODE_CHANGE_MOVE_FREQUENCY,
                EventEnum.EVENT_CODE_PASS_THROUGH,
                EventEnum.MOVEMENT_JUMP,
                EventEnum.MOVEMENT_TURN_DOWN,
                EventEnum.MOVEMENT_TURN_LEFT,
                EventEnum.MOVEMENT_TURN_RIGHT,
                EventEnum.MOVEMENT_TURN_UP,
                EventEnum.MOVEMENT_TURN_90_RIGHT,
                EventEnum.MOVEMENT_TURN_90_LEFT,
                EventEnum.MOVEMENT_TURN_180,
                EventEnum.MOVEMENT_TURN_90_RIGHT_OR_LEFT,
                EventEnum.MOVEMENT_TURN_AT_RANDOM,
                EventEnum.MOVEMENT_TURN_TOWARD_PLAYER,
                EventEnum.MOVEMENT_TURN_AWAY_FROM_PLAYER,
                EventEnum.EVENT_CODE_MOVE_PLACE_SHIP,
                EventEnum.EVENT_CODE_MOVE_RIDE_SHIP,

                //タイミング,
                EventEnum.EVENT_CODE_TIMING_WAIT,

                //パーティ,
                EventEnum.EVENT_CODE_PARTY_CHANGE,
                EventEnum.EVENT_CODE_CHARACTER_CHANGE_ALPHA,
                EventEnum.EVENT_CODE_CHARACTER_CHANGE_WALK,
                EventEnum.EVENT_CODE_CHARACTER_CHANGE_PARTY,
                EventEnum.EVENT_CODE_PARTY_GOLD,
                EventEnum.EVENT_CODE_PARTY_ITEM,
                EventEnum.EVENT_CODE_PARTY_WEAPON,
                EventEnum.EVENT_CODE_PARTY_ARMS,

                //アクター,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_HP,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_MP,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_TP,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_STATE,
                EventEnum.EVENT_CODE_ACTOR_HEAL,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_EXP,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_LEVEL,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_PARAMETER,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_SKILL,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_EQUIPMENT,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_CLASS,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_NAME,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_NICKNAME,
                EventEnum.EVENT_CODE_ACTOR_CHANGE_PROFILE,

                //ゲーム進行,
                EventEnum.EVENT_CODE_GAME_SWITCH,
                EventEnum.EVENT_CODE_GAME_VAL,
                EventEnum.EVENT_CODE_GAME_SELF_SWITCH,
                EventEnum.EVENT_CODE_GAME_TIMER,

                //フロー制御
                EventEnum.EVENT_CODE_FLOW_IF,
                EventEnum.EVENT_CODE_FLOW_ELSE,
                EventEnum.EVENT_CODE_FLOW_ENDIF,
                EventEnum.EVENT_CODE_FLOW_AND,
                EventEnum.EVENT_CODE_FLOW_OR,
                EventEnum.EVENT_CODE_FLOW_LOOP,
                EventEnum.EVENT_CODE_FLOW_LOOP_BREAK,
                EventEnum.EVENT_CODE_FLOW_LOOP_END,
                EventEnum.EVENT_CODE_FLOW_EVENT_BREAK,
                EventEnum.EVENT_CODE_FLOW_JUMP_COMMON,
                EventEnum.EVENT_CODE_FLOW_LABEL,
                EventEnum.EVENT_CODE_FLOW_JUMP_LABEL,
                EventEnum.EVENT_CODE_FLOW_ANNOTATION,

                //ピクチャ,
                EventEnum.EVENT_CODE_PICTURE_SHOW,
                EventEnum.EVENT_CODE_PICTURE_MOVE,
                EventEnum.EVENT_CODE_PICTURE_ROTATE,
                EventEnum.EVENT_CODE_PICTURE_CHANGE_COLOR,
                EventEnum.EVENT_CODE_PICTURE_ERASE,

                //画面,
                EventEnum.EVENT_CODE_DISPLAY_FADEOUT,
                EventEnum.EVENT_CODE_DISPLAY_FADEIN,
                EventEnum.EVENT_CODE_DISPLAY_CHANGE_COLOR,
                EventEnum.EVENT_CODE_DISPLAY_FLASH,
                EventEnum.EVENT_CODE_DISPLAY_SHAKE,
                EventEnum.EVENT_CODE_DISPLAY_WEATHER,

                //マップ,
                EventEnum.EVENT_CODE_MAP_CHANGE_NAME,
                EventEnum.EVENT_CODE_MAP_CHANGE_BATTLE_BACKGROUND,
                EventEnum.EVENT_CODE_MAP_GET_POINT,
                EventEnum.EVENT_CODE_MAP_CHANGE_DISTANT_VIEW,
                EventEnum.EVENT_CODE_MOVE_MAP_SCROLL,

                //システム,
                EventEnum.EVENT_CODE_SYSTEM_BATTLE_BGM,
                EventEnum.EVENT_CODE_SYSTEM_BATTLE_WIN,
                EventEnum.EVENT_CODE_SYSTEM_BATTLE_LOSE,
                EventEnum.EVENT_CODE_SYSTEM_SHIP_BGM,
                EventEnum.EVENT_CODE_SYSTEM_CHANGE_ACTOR_IMAGE,
                EventEnum.EVENT_CODE_SYSTEM_CHANGE_SHIP_IMAGE,
                EventEnum.EVENT_CODE_SYSTEM_IS_MENU,
                EventEnum.EVENT_CODE_SYSTEM_IS_SAVE,
                EventEnum.EVENT_CODE_SYSTEM_IS_ENCOUNT,
                EventEnum.EVENT_CODE_SYSTEM_IS_SORT,
                EventEnum.EVENT_CODE_SYSTEM_WINDOW_COLOR,

                //オーディオ・ビデオ,
                EventEnum.EVENT_CODE_AUDIO_BGM_PLAY,
                EventEnum.EVENT_CODE_AUDIO_BGS_PLAY,
                EventEnum.EVENT_CODE_AUDIO_ME_PLAY,
                EventEnum.EVENT_CODE_AUDIO_SE_PLAY,
                EventEnum.EVENT_CODE_AUDIO_SE_STOP,
                EventEnum.EVENT_CODE_AUDIO_BGM_FADEOUT,
                EventEnum.EVENT_CODE_AUDIO_BGS_FADEOUT,
                EventEnum.EVENT_CODE_AUDIO_BGM_SAVE,
                EventEnum.EVENT_CODE_AUDIO_BGM_CONTINUE,
                EventEnum.EVENT_CODE_AUDIO_MOVIE_PLAY,

                //シーン制御,
                EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG,
                EventEnum.EVENT_CODE_SCENE_GAME_OVER,
                EventEnum.EVENT_CODE_SCENE_GOTO_TITLE,
                EventEnum.EVENT_CODE_SCENE_INPUT_NAME,
                EventEnum.EVENT_CODE_SCENE_MENU_OPEN,
                EventEnum.EVENT_CODE_SCENE_SAVE_OPEN,
                EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG,
                //EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG_LINE,
                EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_WIN,
                EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_ESCAPE,
                EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_LOSE,
                EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_END,

                //バトル,
                EventEnum.EVENT_CODE_BATTLE_CHANGE_STATUS,
                EventEnum.EVENT_CODE_BATTLE_CHANGE_STATE,
                EventEnum.EVENT_CODE_BATTLE_APPEAR,
                EventEnum.EVENT_CODE_BATTLE_TRANSFORM,
                EventEnum.EVENT_CODE_BATTLE_SHOW_ANIMATION,
                EventEnum.EVENT_CODE_BATTLE_EXEC_COMMAND,
                EventEnum.EVENT_CODE_BATTLE_STOP,

                //アドオン,
                EventEnum.EVENT_CODE_ADDON_COMMAND
        };


        /// <summary>
        /// イベントが実行可能かどうかの返却
        /// </summary>
        /// <param name="eventCode">イベントコード</param>
        /// <param name="isBattle">バトルかどうか</param>
        /// <param name="isRuntime">Runtimeかどうか</param>
        /// <returns></returns>
        public static bool CheckEventCodeExecute(int eventCode, EventType eventType, bool isRuntime) {
            if (eventType == EventType.Map)
            {
                var ret = Map.FindAll(data => (int) data == eventCode);
                if (ret == null || ret.Count == 0) return false;
            }
            else if (eventType == EventType.Battle)
            {
                var ret = Battle.FindAll(data => (int) data == eventCode);
                if (ret == null || ret.Count == 0) return false;

                //イベントの登録自体は行えるものの、Runtimeでは無動作というイベントが存在する（MV準拠）
                //そのためRuntimeの場合には、更に追加でチェックを行う
                if (isRuntime)
                {
                    if (eventCode == (int) EventEnum.EVENT_CODE_CHARACTER_SHOW_ANIMATION ||
                        eventCode == (int) EventEnum.EVENT_CODE_CHARACTER_SHOW_ICON ||
                        eventCode == (int) EventEnum.MOVEMENT_CHANGE_IMAGE ||
                        eventCode == (int) EventEnum.MOVEMENT_WALKING_ANIMATION_ON || eventCode == (int) EventEnum.MOVEMENT_WALKING_ANIMATION_OFF ||
                        eventCode == (int) EventEnum.MOVEMENT_STEPPING_ANIMATION_ON || eventCode == (int) EventEnum.MOVEMENT_STEPPING_ANIMATION_OFF ||
                        eventCode == (int) EventEnum.EVENT_CODE_CHARACTER_CHANGE_ALPHA ||
                        eventCode == (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}