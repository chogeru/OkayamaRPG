using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement.Repository;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.MapEditor.Component.EventText;
using RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Actor;
using RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Addons;
using RPGMaker.Codebase.Editor.MapEditor.Component.EventText.AudioVideo;
using RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Battle;
using RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Character;
using RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Character.Action;
using RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Character.Direction;
using RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Character.Move;
using RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Character.Vehicle;
using RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Display;
using RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Event;
using RPGMaker.Codebase.Editor.MapEditor.Component.EventText.FlowControl;
using RPGMaker.Codebase.Editor.MapEditor.Component.EventText.GameProgress;
using RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Map;
using RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Message;
using RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Party;
using RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Picture;
using RPGMaker.Codebase.Editor.MapEditor.Component.EventText.SceneControl;
using RPGMaker.Codebase.Editor.MapEditor.Component.EventText.SystemSetting;
using RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Timing;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.CoreSystem.Service.EventManagement
{
    public class GetEventCommandLabelText
    {
        private readonly EventRepository _eventRepository;

        private readonly Dictionary<EventEnum, Type> _eventCommandView = new Dictionary<EventEnum, Type>
        {
            //メッセージ
            {EventEnum.EVENT_CODE_MESSAGE_TEXT, typeof(MessageText)},
            {EventEnum.EVENT_CODE_MESSAGE_TEXT_ONE_LINE, typeof(MessageTextOneLine)},
            {EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT, typeof(MessageInputSelect)},
            {EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_SELECTED, typeof(MessageInputSelectSelected)},
            {EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_CANCELED, typeof(MessageInputSelectCanceled)},
            {EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_END, typeof(MessageInputSelectEnd)},
            {EventEnum.EVENT_CODE_MESSAGE_INPUT_NUMBER, typeof(MessageInputNumber)},
            {EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_ITEM, typeof(MessageInputSelectItem)},
            {EventEnum.EVENT_CODE_MESSAGE_TEXT_SCROLL, typeof(MessageTextScroll)},
            {EventEnum.EVENT_CODE_MESSAGE_TEXT_SCROLL_ONE_LINE, typeof(MessageTextScrollOnLine)},

            //キャラクター
            {EventEnum.EVENT_CODE_CHARACTER_SHOW_ANIMATION, typeof(CharacterShowAnimation)},
            {EventEnum.EVENT_CODE_CHARACTER_SHOW_ICON, typeof(CharacterShowIcon)},
            {EventEnum.MOVEMENT_WALKING_ANIMATION_ON, typeof(MovementWalkingAnimationOn)},
            {EventEnum.MOVEMENT_WALKING_ANIMATION_OFF, typeof(MovementWalkingAnimationOn)},
            {EventEnum.MOVEMENT_STEPPING_ANIMATION_ON, typeof(MovementWalkingAnimationOn)},
            {EventEnum.MOVEMENT_STEPPING_ANIMATION_OFF, typeof(MovementWalkingAnimationOn)},
            {EventEnum.MOVEMENT_CHANGE_IMAGE, typeof(MovementChangeImage)},

            {EventEnum.EVENT_CODE_MOVE_PLACE, typeof(MovePlace)},

            {EventEnum.EVENT_CODE_MOVE_SET_MOVE_POINT, typeof(MoveSetMovePoint)},
            {EventEnum.MOVEMENT_MOVE_AT_RANDOM, typeof(MoveSetMovePoint)},
            {EventEnum.MOVEMENT_MOVE_TOWARD_PLAYER, typeof(MoveSetMovePoint)},
            {EventEnum.MOVEMENT_MOVE_AWAY_FROM_PLAYER, typeof(MoveSetMovePoint)},
            {EventEnum.MOVEMENT_ONE_STEP_FORWARD, typeof(OneStepForward)},
            {EventEnum.MOVEMENT_ONE_STEP_BACKWARD, typeof(OneStepBackward)},
            {EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE, typeof(FlowCustomMove)},
            {EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE_END, typeof(FlowCustomMoveEnd)},
            {EventEnum.EVENT_CODE_STEP_MOVE, typeof(StepMove)},
            {EventEnum.EVENT_CODE_CHANGE_MOVE_SPEED, typeof(ChangeMoveSpeed)},
            {EventEnum.EVENT_CODE_CHANGE_MOVE_FREQUENCY, typeof(ChangeMoveFrequency)},
            {EventEnum.EVENT_CODE_PASS_THROUGH, typeof(PassThrough)},

            {EventEnum.MOVEMENT_JUMP, typeof(Jump)},

            //向き
            {EventEnum.MOVEMENT_TURN_DOWN, typeof(Direction)},
            {EventEnum.MOVEMENT_TURN_LEFT, typeof(Direction)},
            {EventEnum.MOVEMENT_TURN_RIGHT, typeof(Direction)},
            {EventEnum.MOVEMENT_TURN_UP, typeof(Direction)},
            {EventEnum.MOVEMENT_TURN_90_RIGHT, typeof(Direction)},
            {EventEnum.MOVEMENT_TURN_90_LEFT, typeof(Direction)},
            {EventEnum.MOVEMENT_TURN_180, typeof(Direction)},
            {EventEnum.MOVEMENT_TURN_90_RIGHT_OR_LEFT, typeof(Direction)},
            {EventEnum.MOVEMENT_TURN_AT_RANDOM, typeof(Direction)},
            {EventEnum.MOVEMENT_TURN_TOWARD_PLAYER, typeof(Direction)},
            {EventEnum.MOVEMENT_TURN_AWAY_FROM_PLAYER, typeof(Direction)},

            {EventEnum.EVENT_CODE_MOVE_PLACE_SHIP, typeof(MovePlaceShip)},

            {EventEnum.EVENT_CODE_MOVE_RIDE_SHIP, typeof(MoveRideShip)},

            //タイミング
            {EventEnum.EVENT_CODE_TIMING_WAIT, typeof(TimingWait)},

            //パーティ
            {EventEnum.EVENT_CODE_PARTY_CHANGE, typeof(PartyChange)},
            {EventEnum.EVENT_CODE_CHARACTER_CHANGE_ALPHA, typeof(CharacterChangeAlpha)},
            {EventEnum.EVENT_CODE_CHARACTER_CHANGE_WALK, typeof(CharacterChangeWalk)},
            {EventEnum.EVENT_CODE_CHARACTER_CHANGE_PARTY, typeof(CharacterChangeParty)},
            {EventEnum.EVENT_CODE_PARTY_GOLD, typeof(PartyGold)},
            {EventEnum.EVENT_CODE_PARTY_ITEM, typeof(PartyItem)},
            {EventEnum.EVENT_CODE_PARTY_WEAPON, typeof(PartyWeapon)},
            {EventEnum.EVENT_CODE_PARTY_ARMS, typeof(PartyArms)},

            //アクター
            {EventEnum.EVENT_CODE_ACTOR_CHANGE_HP, typeof(ActorChangeHP)},
            {EventEnum.EVENT_CODE_ACTOR_CHANGE_MP, typeof(ActorChangeMP)},
            {EventEnum.EVENT_CODE_ACTOR_CHANGE_TP, typeof(ActorChangeTP)},
            {EventEnum.EVENT_CODE_ACTOR_CHANGE_STATE, typeof(ActorChangeState)},
            {EventEnum.EVENT_CODE_ACTOR_HEAL, typeof(ActorHeal)},
            {EventEnum.EVENT_CODE_ACTOR_CHANGE_EXP, typeof(ActorChangeExp)},
            {EventEnum.EVENT_CODE_ACTOR_CHANGE_LEVEL, typeof(ActorChangeLevel)},
            {EventEnum.EVENT_CODE_ACTOR_CHANGE_PARAMETER, typeof(ActorChangeParametar)},
            {EventEnum.EVENT_CODE_ACTOR_CHANGE_SKILL, typeof(ActorChangeSkill)},
            {EventEnum.EVENT_CODE_ACTOR_CHANGE_EQUIPMENT, typeof(ActorChangeEquipment)},
            {EventEnum.EVENT_CODE_ACTOR_CHANGE_CLASS, typeof(ActorChangeClass)},
            {EventEnum.EVENT_CODE_ACTOR_CHANGE_NAME, typeof(ActorChangeName)},
            {EventEnum.EVENT_CODE_ACTOR_CHANGE_NICKNAME, typeof(ActorChangeNickName)},
            {EventEnum.EVENT_CODE_ACTOR_CHANGE_PROFILE, typeof(ActorChangeProfile)},

            //マップ
            {EventEnum.EVENT_CODE_MOVE_MAP_SCROLL, typeof(MoveMapScroll)},
            {EventEnum.EVENT_CODE_MAP_CHANGE_NAME, typeof(MapChangeName)},
            {EventEnum.EVENT_CODE_MAP_CHANGE_TILE_SET, typeof(MapChangeTileset)},
            {EventEnum.EVENT_CODE_MAP_CHANGE_BATTLE_BACKGROUND, typeof(MapChangeBattleBackGround)},
            {EventEnum.EVENT_CODE_MAP_CHANGE_DISTANT_VIEW, typeof(MapChangeDistantView)},
            {EventEnum.EVENT_CODE_MAP_GET_POINT, typeof(MapGetPoint)},

            //ゲーム進行
            {EventEnum.EVENT_CODE_GAME_SWITCH, typeof(GameSwitch)},
            {EventEnum.EVENT_CODE_GAME_VAL, typeof(GameVal)},
            {EventEnum.EVENT_CODE_GAME_SELF_SWITCH, typeof(GameSelfSwitch)},
            {EventEnum.EVENT_CODE_GAME_TIMER, typeof(GameTimer)},

            //フロー制御
            {EventEnum.EVENT_CODE_FLOW_IF, typeof(FlowIf)},
            {EventEnum.EVENT_CODE_FLOW_AND, typeof(FlowAnd)},
            {EventEnum.EVENT_CODE_FLOW_OR, typeof(FlowOr)},
            {EventEnum.EVENT_CODE_FLOW_ELSE, typeof(FlowIfElse)},
            {EventEnum.EVENT_CODE_FLOW_ENDIF, typeof(FlowEndIf)},
            {EventEnum.EVENT_CODE_FLOW_LOOP, typeof(FlowLoop)},
            {EventEnum.EVENT_CODE_FLOW_LOOP_BREAK, typeof(FlowLoopBreak)},
            {EventEnum.EVENT_CODE_FLOW_LOOP_END, typeof(FlowLoopEnd)},
            {EventEnum.EVENT_CODE_FLOW_EVENT_BREAK, typeof(FlowEventBreak)},
            {EventEnum.EVENT_CODE_FLOW_JUMP_COMMON, typeof(FlowJumpCommon)},
            {EventEnum.EVENT_CODE_FLOW_LABEL, typeof(FlowLabel)},
            {EventEnum.EVENT_CODE_FLOW_JUMP_LABEL, typeof(FlowJumpLabel)},
            {EventEnum.EVENT_CODE_FLOW_ANNOTATION, typeof(FlowAnnotation)},

            //イベント
            {EventEnum.EVENT_CODE_CHARACTER_IS_EVENT, typeof(CharacterIsEvent)},
            {EventEnum.EVENT_CODE_MOVE_SET_EVENT_POINT, typeof(MoveSetEventPoint)},

            //ピクチャ
            {EventEnum.EVENT_CODE_PICTURE_SHOW, typeof(PictureShow)},
            {EventEnum.EVENT_CODE_PICTURE_MOVE, typeof(PictureMove)},
            {EventEnum.EVENT_CODE_PICTURE_ROTATE, typeof(PictureRotate)},
            {EventEnum.EVENT_CODE_PICTURE_CHANGE_COLOR, typeof(PictureChangeColor)},
            {EventEnum.EVENT_CODE_PICTURE_ERASE, typeof(PictureErase)},

            //画面
            {EventEnum.EVENT_CODE_DISPLAY_FADEOUT, typeof(DisplayFadeOut)},
            {EventEnum.EVENT_CODE_DISPLAY_FADEIN, typeof(DisplayFadeIn)},
            {EventEnum.EVENT_CODE_DISPLAY_CHANGE_COLOR, typeof(DisplayChangeColor)},
            {EventEnum.EVENT_CODE_DISPLAY_FLASH, typeof(DisplayFlash)},
            {EventEnum.EVENT_CODE_DISPLAY_SHAKE, typeof(DisplayShake)},
            {EventEnum.EVENT_CODE_DISPLAY_WEATHER, typeof(DisplayWeather)},

            //オーディオ、ビデオ
            {EventEnum.EVENT_CODE_AUDIO_BGM_PLAY, typeof(AudioBgmPlay)},
            {EventEnum.EVENT_CODE_AUDIO_BGM_FADEOUT, typeof(AudioBgmFadeOut)},
            {EventEnum.EVENT_CODE_AUDIO_BGM_SAVE, typeof(AudioBgmSave)},
            {EventEnum.EVENT_CODE_AUDIO_BGM_CONTINUE, typeof(AudioBgmContinue)},
            {EventEnum.EVENT_CODE_AUDIO_BGS_PLAY, typeof(AudioBgsPlay)},
            {EventEnum.EVENT_CODE_AUDIO_BGS_FADEOUT, typeof(AudioBgsFadeOut)},
            {EventEnum.EVENT_CODE_AUDIO_ME_PLAY, typeof(AudioMePlay)},
            {EventEnum.EVENT_CODE_AUDIO_SE_PLAY, typeof(AudioSePlay)},
            {EventEnum.EVENT_CODE_AUDIO_SE_STOP, typeof(AudioSeStop)},
            {EventEnum.EVENT_CODE_AUDIO_MOVIE_PLAY, typeof(AudioMoviePlay)},

            //シーンコントロール
            {EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG, typeof(SceneSetBattleConfig)},
            {EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_WIN, typeof(SceneSetBattleConfigWin)},
            {EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_ESCAPE, typeof(SceneSetBattleConfigEscape)},
            {EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_LOSE, typeof(SceneSetBattleConfigLose)},
            {EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_END, typeof(SceneSetBattleConfigEnd)},

            {EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG, typeof(SceneSetShopConfig)},
            {EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG_LINE, typeof(SceneSetShopConfig)},
            {EventEnum.EVENT_CODE_SCENE_INPUT_NAME, typeof(SceneInputName)},
            {EventEnum.EVENT_CODE_SCENE_MENU_OPEN, typeof(SceneMenuOpen)},
            {EventEnum.EVENT_CODE_SCENE_SAVE_OPEN, typeof(SceneSaveOpen)},
            {EventEnum.EVENT_CODE_SCENE_GAME_OVER, typeof(SceneGameOver)},
            {EventEnum.EVENT_CODE_SCENE_GOTO_TITLE, typeof(SceneGotoTitle)},

            //バトル
            {EventEnum.EVENT_CODE_BATTLE_CHANGE_STATUS, typeof(BattleChangeStatus)},
            {EventEnum.EVENT_CODE_BATTLE_CHANGE_MP, typeof(BattleChangeStatus)},
            {EventEnum.EVENT_CODE_BATTLE_CHANGE_TP, typeof(BattleChangeStatus)},
            {EventEnum.EVENT_CODE_BATTLE_CHANGE_STATE, typeof(BattleChangeState)},
            {EventEnum.EVENT_CODE_BATTLE_APPEAR, typeof(BattleAppear)},
            {EventEnum.EVENT_CODE_BATTLE_TRANSFORM, typeof(BattleTransform)},
            {EventEnum.EVENT_CODE_BATTLE_SHOW_ANIMATION, typeof(BattleShowAnimation)},
            {EventEnum.EVENT_CODE_BATTLE_EXEC_COMMAND, typeof(BattleExecCommand)},
            {EventEnum.EVENT_CODE_BATTLE_STOP, typeof(BattleStop)},

            //システムセッティング
            {EventEnum.EVENT_CODE_SYSTEM_BATTLE_BGM, typeof(SystemBattleBgm)},
            {EventEnum.EVENT_CODE_SYSTEM_BATTLE_WIN, typeof(SystemBattleWin)},
            {EventEnum.EVENT_CODE_SYSTEM_BATTLE_LOSE, typeof(SystemBattleLose)},
            {EventEnum.EVENT_CODE_SYSTEM_SHIP_BGM, typeof(SystemShipBgm)},
            {EventEnum.EVENT_CODE_SYSTEM_IS_SAVE, typeof(SystemIsSave)},
            {EventEnum.EVENT_CODE_SYSTEM_IS_MENU, typeof(SystemIsMenu)},
            {EventEnum.EVENT_CODE_SYSTEM_IS_ENCOUNT, typeof(SystemIsEncount)},
            {EventEnum.EVENT_CODE_SYSTEM_IS_SORT, typeof(SystemIsSort)},
            {EventEnum.EVENT_CODE_SYSTEM_WINDOW_COLOR, typeof(SystemWindowColor)},
            {EventEnum.EVENT_CODE_SYSTEM_CHANGE_ACTOR_IMAGE, typeof(SystemChangeActorImage)},
            {EventEnum.EVENT_CODE_SYSTEM_CHANGE_SHIP_IMAGE, typeof(SystemChangeShipImage)},

            //アドオン
            {EventEnum.EVENT_CODE_ADDON_COMMAND, typeof(AddonCommand)}
        };

        public GetEventCommandLabelText() {
            _eventRepository = new EventRepository();
        }

        public VisualElement Invoke(EventDataModel.EventCommand eventCommand) {
            var ret = "";
            var code = eventCommand.code;

            //インデントの数分、スペースを入れる
            for (var j = 0; j < eventCommand.indent; j++) ret += "    ";

            var element = new VisualElement();
            if (code != 0)
            {
                var commandViewInstance =
                    (IEventCommandView) Activator.CreateInstance(_eventCommandView[(EventEnum) code]);
                element = commandViewInstance.Invoke(ret, eventCommand);
            }


            else if (code == 0)
            {
                ret += "◆";
                element = new Label(ret);
            }


            else if (code == (int) EventEnum.EVENT_CODE_MESSAGE_TEXT_ONE_LINE)
            {
                ret += "◆" + EditorLocalize.LocalizeText("WORD_1069") + " : ";
            }

            else if (code == (int) EventEnum.EVENT_CODE_BATTLE_CHANGE_MP)
            {
                var enemyid = int.Parse(eventCommand.parameters[0]);
                if (enemyid == -1)
                    ret += "Change Enemy MP : Entire Troop, ";
                else
                    ret += "Change Enemy MP : #" + (enemyid + 1) + ", ";

                if (int.Parse(eventCommand.parameters[1]) == 0)
                    ret += " + ";
                else
                    ret += " - ";

                if (int.Parse(eventCommand.parameters[2]) == 0)
                    ret += eventCommand.parameters[3];
                else
                    ret +=
                        "#" + (int.Parse(eventCommand.parameters[3]) + 1).ToString("0000");
            }
            else if (code == (int) EventEnum.EVENT_CODE_BATTLE_CHANGE_TP)
            {
                var enemyid = int.Parse(eventCommand.parameters[0]);
                if (enemyid == -1)
                    ret += "Change Enemy TP : Entire Troop, ";
                else
                    ret += "Change Enemy TP : #" + (enemyid + 1) + ", ";

                if (int.Parse(eventCommand.parameters[1]) == 0)
                    ret += " + ";
                else
                    ret += " - ";

                if (int.Parse(eventCommand.parameters[2]) == 0)
                    ret += eventCommand.parameters[3];
                else
                    ret +=
                        "#" + (int.Parse(eventCommand.parameters[3]) + 1).ToString("0000");
            }

            else if (code == (int) EventEnum.EVENT_CODE_BATTLE_HEAL)
            {
                var enemyid = int.Parse(eventCommand.parameters[0]);
                if (enemyid == -1)
                    ret += "Enemy Recover All : Entire Troop";
                else
                    ret += "Enemy Recover All : #" + (enemyid + 1) + ", ";
            }
            else if (code == (int) EventEnum.EVENT_CODE_BATTLE_APPEAR)
            {
                var enemyid = int.Parse(eventCommand.parameters[0]);
                ret += EditorLocalize.LocalizeText("WORD_1107") + " : #" + (enemyid + 1) + ", ";
            }

            else if (code == (int) EventEnum.EVENT_CODE_BATTLE_STOP)
            {
            }

            else if (code == (int) EventMoveEnum.MOVEMENT_MOVE_DOWN)
            {
                ret = "Move Down";
            }
            else if (code == (int) EventMoveEnum.MOVEMENT_MOVE_LEFT)
            {
                ret = "Move Left";
            }
            else if (code == (int) EventMoveEnum.MOVEMENT_MOVE_RIGHT)
            {
                ret = "Move Right";
            }
            else if (code == (int) EventMoveEnum.MOVEMENT_MOVE_UP)
            {
                ret = "Move Up";
            }
            else if (code == (int) EventMoveEnum.MOVEMENT_MOVE_LOWER_LEFT)
            {
                ret = "Move Lower Left";
            }
            else if (code == (int) EventMoveEnum.MOVEMENT_MOVE_LOWER_RIGHT)
            {
                ret = "Move Lower Right";
            }
            else if (code == (int) EventMoveEnum.MOVEMENT_MOVE_UPPER_LEFT)
            {
                ret = "Move Upper Left";
            }
            else if (code == (int) EventMoveEnum.MOVEMENT_MOVE_UPPER_RIGHT)
            {
                ret = "Move Upper Right";
            }

            else if (code == (int) EventEnum.MOVEMENT_TURN_LEFT)
            {
                var name = "";
                if (eventCommand.parameters[0] == "-1")
                    name = EditorLocalize.LocalizeText("WORD_0920");
                else if (eventCommand.parameters[0] == "-2")
                    name = EditorLocalize.LocalizeText("WORD_0860");
                else
                    try
                    {
                        name = "EV" + int.Parse(eventCommand.parameters[0]).ToString("000");
                    }
                    catch (Exception)
                    {
                    }

                var toggle = "OFF";
                if (eventCommand.parameters[2] == "1")
                    toggle = "ON";

                ret = "◆" + EditorLocalize.LocalizeText("WORD_0858") + " : " +
                      EditorLocalize.LocalizeText("WORD_0014") + " : " + name +
                      EditorLocalize.LocalizeText("WORD_0858") + " : " + EditorLocalize.LocalizeText("WORD_0956") + " " +
                      EditorLocalize.LocalizeText("WORD_0844") + " : " + toggle;
            }
            else if (code == (int) EventEnum.MOVEMENT_TURN_RIGHT)
            {
                var name = "";
                if (eventCommand.parameters[0] == "-1")
                    name = EditorLocalize.LocalizeText("WORD_0920");
                else if (eventCommand.parameters[0] == "-2")
                    name = EditorLocalize.LocalizeText("WORD_0860");
                else
                    try
                    {
                        name = "EV" + int.Parse(eventCommand.parameters[0]).ToString("000");
                    }
                    catch (Exception)
                    {
                    }

                var toggle = "OFF";
                if (eventCommand.parameters[2] == "1")
                    toggle = "ON";

                ret = "◆" + EditorLocalize.LocalizeText("WORD_0858") + " : " +
                      EditorLocalize.LocalizeText("WORD_0014") + " : " + name +
                      EditorLocalize.LocalizeText("WORD_0957") + " : " + EditorLocalize.LocalizeText("WORD_0956") + " " +
                      EditorLocalize.LocalizeText("WORD_0844") + " : " + toggle;
            }
            else if (code == (int) EventEnum.MOVEMENT_TURN_UP)
            {
                var name = "";
                if (eventCommand.parameters[0] == "-1")
                    name = EditorLocalize.LocalizeText("WORD_0920");
                else if (eventCommand.parameters[0] == "-2")
                    name = EditorLocalize.LocalizeText("WORD_0860");
                else
                    try
                    {
                        name = "EV" + int.Parse(eventCommand.parameters[0]).ToString("000");
                    }
                    catch (Exception)
                    {
                    }

                var toggle = "OFF";
                if (eventCommand.parameters[2] == "1")
                    toggle = "ON";

                ret = "◆" + EditorLocalize.LocalizeText("WORD_0858") + " : " +
                      EditorLocalize.LocalizeText("WORD_0014") + " : " + name +
                      EditorLocalize.LocalizeText("WORD_0957") + " : " + EditorLocalize.LocalizeText("WORD_0958") + " " +
                      EditorLocalize.LocalizeText("WORD_0844") + " : " + toggle;
            }
            else if (code == (int) EventMoveEnum.MOVEMENT_TURN_90_RIGHT)
            {
                var name = "";
                if (eventCommand.parameters[0] == "-1")
                    name = EditorLocalize.LocalizeText("WORD_0920");
                else if (eventCommand.parameters[0] == "-2")
                    name = EditorLocalize.LocalizeText("WORD_0860");
                else
                    try
                    {
                        name = "EV" + int.Parse(eventCommand.parameters[0]).ToString("000");
                    }
                    catch (Exception)
                    {
                    }

                var toggle = "OFF";
                if (eventCommand.parameters[2] == "1")
                    toggle = "ON";

                ret = "◆" + EditorLocalize.LocalizeText("WORD_0858") + " : " +
                      EditorLocalize.LocalizeText("WORD_0014") + " : " + name +
                      EditorLocalize.LocalizeText("WORD_0957") + " : " + EditorLocalize.LocalizeText("WORD_0959") + " " +
                      EditorLocalize.LocalizeText("WORD_0844") + " : " + toggle;
            }
            else if (code == (int) EventMoveEnum.MOVEMENT_TURN_90_LEFT)
            {
                var name = "";
                if (eventCommand.parameters[0] == "-1")
                    name = EditorLocalize.LocalizeText("WORD_0920");
                else if (eventCommand.parameters[0] == "-2")
                    name = EditorLocalize.LocalizeText("WORD_0860");
                else
                    try
                    {
                        name = "EV" + int.Parse(eventCommand.parameters[0]).ToString("000");
                    }
                    catch (Exception)
                    {
                    }

                var toggle = "OFF";
                if (eventCommand.parameters[2] == "1")
                    toggle = "ON";

                ret = "◆" + EditorLocalize.LocalizeText("WORD_0858") + " : " +
                      EditorLocalize.LocalizeText("WORD_0014") + " : " + name +
                      EditorLocalize.LocalizeText("WORD_0957") + " : " + EditorLocalize.LocalizeText("WORD_0960") + " " +
                      EditorLocalize.LocalizeText("WORD_0844") + " : " + toggle;
            }
            else if (code == (int) EventMoveEnum.MOVEMENT_TURN_180)
            {
                var name = "";
                if (eventCommand.parameters[0] == "-1")
                    name = EditorLocalize.LocalizeText("WORD_0920");
                else if (eventCommand.parameters[0] == "-2")
                    name = EditorLocalize.LocalizeText("WORD_0860");
                else
                    try
                    {
                        name = "EV" + int.Parse(eventCommand.parameters[0]).ToString("000");
                    }
                    catch (Exception)
                    {
                    }

                var toggle = "OFF";
                if (eventCommand.parameters[2] == "1")
                    toggle = "ON";

                ret = "◆" + EditorLocalize.LocalizeText("WORD_0858") + " : " +
                      EditorLocalize.LocalizeText("WORD_0014") + " : " + name +
                      EditorLocalize.LocalizeText("WORD_0957") + " : " + EditorLocalize.LocalizeText("WORD_0961") + " " +
                      EditorLocalize.LocalizeText("WORD_0844") + " : " + toggle;
            }
            else if (code == (int) EventMoveEnum.MOVEMENT_TURN_90_RIGHT_OR_LEFT)
            {
                var name = "";
                if (eventCommand.parameters[0] == "-1")
                    name = EditorLocalize.LocalizeText("WORD_0920");
                else if (eventCommand.parameters[0] == "-2")
                    name = EditorLocalize.LocalizeText("WORD_0860");
                else
                    try
                    {
                        name = "EV" + int.Parse(eventCommand.parameters[0]).ToString("000");
                    }
                    catch (Exception)
                    {
                    }

                var toggle = "OFF";
                if (eventCommand.parameters[2] == "1")
                    toggle = "ON";

                ret = "◆" + EditorLocalize.LocalizeText("WORD_0858") + " : " +
                      EditorLocalize.LocalizeText("WORD_0014") + " : " + name +
                      EditorLocalize.LocalizeText("WORD_0957") + " : " + EditorLocalize.LocalizeText("WORD_0962") + " " +
                      EditorLocalize.LocalizeText("WORD_0844") + " : " + toggle;
            }
            else if (code == (int) EventMoveEnum.MOVEMENT_TURN_AT_RANDOM)
            {
                var name = "";
                if (eventCommand.parameters[0] == "-1")
                    name = EditorLocalize.LocalizeText("WORD_0920");
                else if (eventCommand.parameters[0] == "-2")
                    name = EditorLocalize.LocalizeText("WORD_0860");
                else
                    try
                    {
                        name = "EV" + int.Parse(eventCommand.parameters[0]).ToString("000");
                    }
                    catch (Exception)
                    {
                    }

                var toggle = "OFF";
                if (eventCommand.parameters[2] == "1")
                    toggle = "ON";

                ret = "◆" + EditorLocalize.LocalizeText("WORD_0858") + " : " +
                      EditorLocalize.LocalizeText("WORD_0014") + " : " + name +
                      EditorLocalize.LocalizeText("WORD_0957") + " : " + EditorLocalize.LocalizeText("WORD_0963") + " " +
                      EditorLocalize.LocalizeText("WORD_0844") + " : " + toggle;
            }
            else if (code == (int) EventMoveEnum.MOVEMENT_TURN_TOWARD_PLAYER)
            {
                var name = "";
                if (eventCommand.parameters[0] == "-1")
                    name = EditorLocalize.LocalizeText("WORD_0920");
                else if (eventCommand.parameters[0] == "-2")
                    name = EditorLocalize.LocalizeText("WORD_0860");
                else
                    try
                    {
                        name = "EV" + int.Parse(eventCommand.parameters[0]).ToString("000");
                    }
                    catch (Exception)
                    {
                    }

                var toggle = "OFF";
                if (eventCommand.parameters[2] == "1")
                    toggle = "ON";

                ret = "◆" + EditorLocalize.LocalizeText("WORD_0858") + " : " +
                      EditorLocalize.LocalizeText("WORD_0014") + " : " + name +
                      EditorLocalize.LocalizeText("WORD_0957") + " : " + EditorLocalize.LocalizeText("WORD_0964") + " " +
                      EditorLocalize.LocalizeText("WORD_0844") + " : " + toggle;
            }
            else if (code == (int) EventMoveEnum.MOVEMENT_TURN_AWAY_FROM_PLAYER)
            {
                var name = "";
                if (eventCommand.parameters[0] == "-1")
                    name = EditorLocalize.LocalizeText("WORD_0920");
                else if (eventCommand.parameters[0] == "-2")
                    name = EditorLocalize.LocalizeText("WORD_0860");
                else
                    try
                    {
                        name = "EV" + int.Parse(eventCommand.parameters[0]).ToString("000");
                    }
                    catch (Exception)
                    {
                    }

                var toggle = "OFF";
                if (eventCommand.parameters[2] == "1")
                    toggle = "ON";

                ret = "◆" + EditorLocalize.LocalizeText("WORD_0858") + " : " +
                      EditorLocalize.LocalizeText("WORD_0014") + " : " + name +
                      EditorLocalize.LocalizeText("WORD_0957") + " : " + EditorLocalize.LocalizeText("WORD_0965") + " " +
                      EditorLocalize.LocalizeText("WORD_0844") + " : " + toggle;
            }
            else if (code == (int) EventEnum.MOVEMENT_WALKING_ANIMATION_ON)
            {
                var name = "";
                if (eventCommand.parameters[0] == "-1")
                    name = EditorLocalize.LocalizeText("WORD_0920");
                else if (eventCommand.parameters[0] == "-2")
                    name = EditorLocalize.LocalizeText("WORD_0860");
                else
                    try
                    {
                        name = "EV" + int.Parse(eventCommand.parameters[0]).ToString("000");
                    }
                    catch (Exception)
                    {
                    }

                ret = "◆" + EditorLocalize.LocalizeText("WORD_0967") + " : " + name + " " +
                      EditorLocalize.LocalizeText("WORD_0968");
            }
            else if (code == (int) EventEnum.MOVEMENT_WALKING_ANIMATION_OFF)
            {
                var name = "";
                if (eventCommand.parameters[0] == "-1")
                    name = EditorLocalize.LocalizeText("WORD_0920");
                else if (eventCommand.parameters[0] == "-2")
                    name = EditorLocalize.LocalizeText("WORD_0860");
                else
                    try
                    {
                        name = "EV" + int.Parse(eventCommand.parameters[0]).ToString("000");
                    }
                    catch (Exception)
                    {
                    }

                ret = "◆" + EditorLocalize.LocalizeText("WORD_0967") + " : " + name + " " +
                      EditorLocalize.LocalizeText("WORD_0969");
            }
            else if (code == (int) EventEnum.MOVEMENT_STEPPING_ANIMATION_ON)
            {
                var name = "";
                if (eventCommand.parameters[0] == "-1")
                    name = EditorLocalize.LocalizeText("WORD_0920");
                else if (eventCommand.parameters[0] == "-2")
                    name = EditorLocalize.LocalizeText("WORD_0860");
                else
                    try
                    {
                        name = "EV" + int.Parse(eventCommand.parameters[0]).ToString("000");
                    }
                    catch (Exception)
                    {
                    }

                ret = "◆" + EditorLocalize.LocalizeText("WORD_0967") + " : " + name + " " +
                      EditorLocalize.LocalizeText("WORD_0970");
            }
            else if (code == (int) EventEnum.MOVEMENT_STEPPING_ANIMATION_OFF)
            {
                var name = "";
                if (eventCommand.parameters[0] == "-1")
                    name = EditorLocalize.LocalizeText("WORD_0920");
                else if (eventCommand.parameters[0] == "-2")
                    name = EditorLocalize.LocalizeText("WORD_0860");
                else
                    try
                    {
                        name = "EV" + int.Parse(eventCommand.parameters[0]).ToString("000");
                    }
                    catch (Exception)
                    {
                    }

                ret = "◆" + EditorLocalize.LocalizeText("WORD_0967") + " : " + name + " " +
                      EditorLocalize.LocalizeText("WORD_0971");
            }
            else if (code == (int) EventMoveEnum.MOVEMENT_DIRECTION_FIX_ON)
            {
                ret = EditorLocalize.LocalizeText("WORD_0966") + " ON";
            }
            else if (code == (int) EventMoveEnum.MOVEMENT_DIRECTION_FIX_OFF)
            {
                ret = EditorLocalize.LocalizeText("WORD_0966") + " OFF";
            }
            else if (code == (int) EventMoveEnum.MOVEMENT_THROUGH_ON)
            {
                ret = EditorLocalize.LocalizeText("WORD_0867") + " ON";
            }
            else if (code == (int) EventMoveEnum.MOVEMENT_THROUGH_OFF)
            {
                ret = EditorLocalize.LocalizeText("WORD_0867") + " OFF";
            }
            else if (code == (int) EventMoveEnum.MOVEMENT_TRANSPARENT_ON)
            {
                ret = EditorLocalize.LocalizeText("WORD_1198") + " ON";
            }
            else if (code == (int) EventMoveEnum.MOVEMENT_TRANSPARENT_OFF)
            {
                ret = EditorLocalize.LocalizeText("WORD_1198") + " OFF";
            }
            else if (code == (int) EventEnum.MOVEMENT_CHANGE_IMAGE)
            {
            }
            else if (code == (int) EventEnum.EVENT_CODE_BATTLE_STOP)
            {
                ret += "◆" + EditorLocalize.LocalizeText("WORD_1115");
            }
            else if (code == (int) EventMoveEnum.MOVEMENT_SCRIPT)
            {
            }
            else if (code == 0)
            {
                ret += "◆";
            }
            else
            {
                ret = eventCommand.code.ToString();
                for (var j = 0; j < eventCommand.parameters.Count; j++)
                {
                    ret += ",";
                    ret += eventCommand.parameters[j];
                }
            }

            var work = ret;
            ret = work;

            return element;
        }
    }
}