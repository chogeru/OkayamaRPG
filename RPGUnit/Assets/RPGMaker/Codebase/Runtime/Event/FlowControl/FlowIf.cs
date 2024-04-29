using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Class;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Enum;
using RPGMaker.Codebase.Runtime.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Event.FlowControl
{
    /// <summary>
    /// [フロー制御]-[分岐設定]
    /// </summary>
    public class FlowIf
    {
        public bool FrowIf(string eventId, EventDataModel.EventCommand command) {
            var dm = new DatabaseManagementService();
            var data = DataManager.Self().GetRuntimeSaveDataModel();
            var sw = dm.LoadFlags().switches;
            var vari = dm.LoadFlags().variables;
            var actor = dm.LoadCharacterActor();
            var classes = dm.LoadCharacterActorClass();

            // 条件設定
            var flowType = 0;
            if (command.parameters[1] == "1") flowType += int.Parse(command.parameters[2]) + 1;

            // スイッチの判定
            if (command.parameters[3] == "1")
            {
                var num = 0;
                for (var i = 0; i < sw.Count; i++)
                    if (sw[i].id == command.parameters[4])
                    {
                        num = i;
                        break;
                    }

                if (data.switches.data[num] == !Convert.ToBoolean(int.Parse(command.parameters[5])))
                {
                    if (flowType != 1)
                        return true;
                }
                else if (flowType == 1)
                {
                    return false;
                }
            }

            // 変数
            if (command.parameters[6] == "1")
            {
                // 変数の値取得
                int GetVariableValue(string id) {
                    for (var i = 0; i < vari.Count; i++)
                        if (vari[i].id == id)
                            return int.Parse(data.variables.data[i]);
                    return 0;
                }

                ;

                var baseValue = GetVariableValue(command.parameters[7]);
                var compareValue = 0;

                // 定数か
                if (command.parameters[9] == "1")
                    compareValue = int.Parse(command.parameters[10]);
                else
                    compareValue = GetVariableValue(command.parameters[11]);

                // 計算
                switch (command.parameters[8])
                {
                    case "0":
                        if (baseValue == compareValue)
                        {
                            if (flowType != 1)
                                return true;
                        }
                        else if (flowType == 1)
                        {
                            return false;
                        }

                        break;
                    case "1":
                        if (baseValue >= compareValue)
                        {
                            if (flowType != 1)
                                return true;
                        }
                        else if (flowType == 1)
                        {
                            return false;
                        }

                        break;
                    case "2":
                        if (baseValue <= compareValue)
                        {
                            if (flowType != 1)
                                return true;
                        }
                        else if (flowType == 1)
                        {
                            return false;
                        }

                        break;
                    case "3":
                        if (baseValue > compareValue)
                        {
                            if (flowType != 1)
                                return true;
                        }
                        else if (flowType == 1)
                        {
                            return false;
                        }

                        break;
                    case "4":
                        if (baseValue < compareValue)
                        {
                            if (flowType != 1)
                                return true;
                        }
                        else if (flowType == 1)
                        {
                            return false;
                        }

                        break;
                    case "5":
                        if (baseValue != compareValue)
                        {
                            if (flowType != 1)
                                return true;
                        }
                        else if (flowType == 1)
                        {
                            return false;
                        }

                        break;
                }
            }

            // セルフスイッチ
            if (command.parameters[12] == "1")
            {
                // データを検索
                RuntimeSaveDataModel.SaveDataSelfSwitchesData swData = null;
                for (int i = 0; i < data.selfSwitches.Count; i++)
                    if (data.selfSwitches[i].id == eventId)
                    {
                        swData = data.selfSwitches[i];
                        break;
                    }

                // データがなければ追加
                if (swData == null)
                {
                    data.selfSwitches.Add(new RuntimeSaveDataModel.SaveDataSelfSwitchesData());
                    data.selfSwitches[data.selfSwitches.Count - 1].id = eventId;
                    data.selfSwitches[data.selfSwitches.Count - 1].data = new List<bool> {false, false, false, false};
                    swData = data.selfSwitches[data.selfSwitches.Count - 1];
                }

                if (swData.data[int.Parse(command.parameters[13])] ==
                    !Convert.ToBoolean(int.Parse(command.parameters[14])))
                {
                    if (flowType != 1)
                        return true;
                }
                else if (flowType == 1)
                {
                    return false;
                }
            }

            // タイマー
            if (command.parameters[15] == "1")
            {
                var sec = GameObject.FindWithTag("Second")?.GetComponent<Text>();
                var min = GameObject.FindWithTag("Minute")?.GetComponent<Text>();

                // タイマーが無い
                if (sec == null || min == null)
                {
                    if (flowType != 2)
                        return false;
                }
                else
                {
                    int time;

                    //タイミングによって、まだ正常に値を取得できない可能性がある
                    //その場合には条件を満たしていない
                    try
                    {
                        time = int.Parse(sec.text);
                        time += int.Parse(min.text) * 60;
                    }
                    catch (Exception)
                    {
                        return false;
                    }

                    if (command.parameters[16] == "0")
                        if (time >= int.Parse(command.parameters[17]) * 60 + int.Parse(command.parameters[18]))
                        {
                            if (flowType != 1)
                                return true;
                        }
                        else if (flowType == 1)
                        {
                            return false;
                        }

                    if (command.parameters[16] == "1")
                        if (time <= int.Parse(command.parameters[17]) * 60 + int.Parse(command.parameters[18]))
                        {
                            if (flowType != 1)
                                return true;
                        }
                        else if (flowType == 1)
                        {
                            return false;
                        }
                }
            }

            // アクター
            if (command.parameters[19] == "1")
            {
                RuntimeActorDataModel actorData = null;
                for (int i = 0; i < data.runtimeActorDataModels.Count; i++)
                    if (data.runtimeActorDataModels[i].actorId == command.parameters[20])
                    {
                        actorData = data.runtimeActorDataModels[i];
                        break;
                    }

                if (actorData == null)
                    if (flowType != 2)
                        return false;
                var isFound = false;

                switch (command.parameters[21])
                {
                    // パーティにいる
                    case "0":
                        string actorId = null;
                        for (int i = 0; i < data.runtimePartyDataModel.actors.Count; i++)
                            if (data.runtimePartyDataModel.actors[i] == actorData.actorId)
                            {
                                actorId = data.runtimePartyDataModel.actors[i];
                                break;
                            }

                        if (actorId != null)
                        {
                            if (flowType != 1)
                                return true;
                        }
                        else if (flowType == 1)
                        {
                            return false;
                        }

                        break;
                    // 名前一致
                    case "1":
                        if (actorData.name == command.parameters[22])
                        {
                            if (flowType != 1)
                                return true;
                        }
                        else if (flowType == 1)
                        {
                            return false;
                        }

                        break;
                    // 職業
                    case "2":
                        ClassDataModel classData = null;
                        for (int i = 0; i < classes.Count; i++)
                            if (classes[i].id == actorData.classId)
                            {
                                classData = classes[i];
                                break;
                            }

                        if (classData?.basic.id == command.parameters[23])
                        {
                            if (flowType != 1)
                                return true;
                        }
                        else if (flowType == 1)
                        {
                            return false;
                        }

                        break;
                    // スキルを覚えている
                    case "3":
                        var gameActor = DataManager.Self().GetGameActors().Actor(actorData);

                        for (var i = 0; i < gameActor.Skills().Count; i++)
                            if (gameActor.Skills()[i].basic.id == command.parameters[24])
                            {
                                if (flowType != 1)
                                    return true;
                                isFound = true;
                            }

                        // OR以外は失敗
                        if (flowType != 2 && isFound == false)
                            return false;
                        break;
                    // 武器装備
                    case "4":
                        for (var i = 0; i < actorData.equips.Count; i++)
                            if (actorData.equips[i].itemId == command.parameters[25])
                            {
                                if (flowType != 1)
                                    return true;
                                isFound = true;
                            }

                        // OR以外は失敗
                        if (flowType != 2 && isFound == false)
                            return false;
                        break;
                    // 防具装備
                    case "5":
                        for (var i = 0; i < actorData.equips.Count; i++)
                            if (actorData.equips[i].itemId == command.parameters[26])
                            {
                                if (flowType != 1)
                                    return true;
                                isFound = true;
                            }

                        // OR以外は失敗
                        if (flowType != 2 && isFound == false)
                            return false;
                        break;
                    // ステートが付与されている
                    case "6":
                        var party = DataManager.Self().GetGameParty();
                        for (int i = 0; i < party.Actors.Count; i++)
                        {
                            if (party.Actors[i].ActorId == actorData.actorId)
                            {
                                for (var j = 0; j < party.Actors[i].States.Count; j++)
                                    if (party.Actors[i].States[j].id == command.parameters[27])
                                    {
                                        if (flowType != 1)
                                            return true;
                                        isFound = true;
                                    }
                                break;
                            }
                        }

                        // OR以外は失敗
                        if (flowType != 2 && isFound == false)
                            return false;
                        break;
                }
            }

            // 敵キャラ（Mapで処理しない）
            if (command.parameters[28] == "1")
            {
                // バトル中のみ判定
                if (GameStateHandler.IsBattle())
                {
                    // 敵キャラのIndex取得
                    int.TryParse(command.parameters[29], out var memberIndex);
                    if (DataManager.Self().GetGameTroop().Members().Count > memberIndex)
                    {
                        // 敵データ取得
                        var enemy = DataManager.Self().GetGameTroop().Members().ElementAtOrDefault(memberIndex);
                        if (command.parameters[30] == "0")
                        {
                            // 敵が出現しているかどうか
                            if (enemy.IsAppeared())
                            {
                                // AND以外なら条件を満たす
                                if (flowType != 1) return true;
                            }
                            // OR以外なら条件を満たさない
                            else if (flowType != 2)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            // ステートが付与されているか
                            bool isFound = false;
                            for (var j = 0; j < enemy.States.Count; j++)
                            {
                                // ステートIDが一致している
                                if (enemy.States[j].id == command.parameters[31])
                                {
                                    // AND以外なら条件を満たす
                                    if (flowType != 1) return true;
                                    isFound = true;
                                }
                            }

                            // ステートが付与されておらず、条件がOR以外なら条件を満たさない
                            if (flowType != 2 && isFound == false)
                                return false;
                        }
                    }
                    else
                    {
                        // OR以外なら条件を満たさない
                        if (flowType != 2)
                            return false;
                    }
                }
                else
                {
                    // OR以外なら条件を満たさない
                    if (flowType != 2)
                        return false;
                }
            }

            // キャラクター
            if (command.parameters[32] == "1")
            {
                // 向き
                var direction = CharacterMoveDirectionEnum.Down;
                switch (command.parameters[34])
                {
                    case "0":
                        direction = CharacterMoveDirectionEnum.Down;
                        break;
                    case "1":
                        direction = CharacterMoveDirectionEnum.Left;
                        break;
                    case "2":
                        direction = CharacterMoveDirectionEnum.Right;
                        break;
                    case "3":
                        direction = CharacterMoveDirectionEnum.Up;
                        break;
                }

                var events = MapEventExecutionController.Instance.GetEventOnMap(eventId);
                var isFound = false;

                // 対象
                switch (command.parameters[33])
                {
                    // プレイヤー
                    case "-2":
                        if (MapManager.OperatingCharacter.GetCurrentDirection() == direction)
                        {
                            if (flowType != 1)
                                return true;
                        }
                        else if (flowType == 1)
                        {
                            return false;
                        }

                        break;
                    // 自身
                    case "-1":
                        if (events != null && events.GetCurrentDirection() == direction)
                        {
                            if (flowType != 1)
                                return true;
                            isFound = true;
                        }

                        // OR以外は失敗
                        if (flowType != 2 && isFound == false)
                            return false;
                        break;
                    // 指定
                    default:
                        events = MapEventExecutionController.Instance.GetEventOnMap(command.parameters[33]);
                        if (events != null && events.GetCurrentDirection() == direction)
                        {
                            if (flowType != 1)
                                return true;
                            isFound = true;
                        }

                        // OR以外は失敗
                        if (flowType != 2 && isFound == false)
                            return false;
                        break;
                }
            }

            // 乗り物
            if (command.parameters[35] == "1")
            {
                if (MapManager.CurrentVehicleId == command.parameters[36])
                {
                    if (flowType != 1)
                        return true;
                }
                else if (flowType == 1)
                {
                    return false;
                }
            }

            // 所持金
            if (command.parameters[37] == "1")
                switch (command.parameters[38])
                {
                    // 所持金 >= 金額
                    case "0":
                        if (data.runtimePartyDataModel.gold >= int.Parse(command.parameters[39]))
                        {
                            if (flowType != 1)
                                return true;
                        }
                        else if (flowType == 1)
                        {
                            return false;
                        }

                        break;
                    // 所持金 <= 金額
                    case "1":
                        if (data.runtimePartyDataModel.gold <= int.Parse(command.parameters[39]))
                        {
                            if (flowType != 1)
                                return true;
                        }
                        else if (flowType == 1)
                        {
                            return false;
                        }

                        break;
                    // 所持金 < 金額
                    case "2":
                        if (data.runtimePartyDataModel.gold < int.Parse(command.parameters[39]))
                        {
                            if (flowType != 1)
                                return true;
                        }
                        else if (flowType == 1)
                        {
                            return false;
                        }

                        break;
                }

            // アイテム
            if (command.parameters[40] == "1")
            {
                var isFound = false;
                for (var i = 0; i < data.runtimePartyDataModel.items.Count; i++)
                    if (data.runtimePartyDataModel.items[i].itemId == command.parameters[41] &&
                        data.runtimePartyDataModel.items[i].value > 0)
                    {
                        if (flowType != 1)
                            return true;
                        isFound = true;
                    }

                // OR以外は失敗
                if (flowType != 2 && isFound == false)
                    return false;
            }

            // 武器
            if (command.parameters[42] == "1")
            {
                var isFound = false;
                for (var i = 0; i < data.runtimePartyDataModel.weapons.Count; i++)
                    if (data.runtimePartyDataModel.weapons[i].weaponId == command.parameters[43] &&
                        data.runtimePartyDataModel.weapons[i].value > 0)
                    {
                        if (flowType != 1)
                            return true;
                        isFound = true;
                    }

                if (command.parameters[44] == "1")
                    for (var i = 0; i < data.runtimeActorDataModels.Count; i++)
                    for (var i2 = 0; i2 < data.runtimeActorDataModels[i].equips.Count; i2++)
                        if (data.runtimeActorDataModels[i].equips[i2].itemId == command.parameters[43])
                        {
                            if (flowType != 1)
                                return true;
                            isFound = true;
                        }

                // OR以外は失敗
                if (flowType != 2 && isFound == false)
                    return false;
            }

            // 防具
            if (command.parameters[45] == "1")
            {
                var isFound = false;
                for (var i = 0; i < data.runtimePartyDataModel.armors.Count; i++)
                    if (data.runtimePartyDataModel.armors[i].armorId == command.parameters[46] &&
                        data.runtimePartyDataModel.armors[i].value > 0)
                    {
                        if (flowType != 1)
                            return true;
                        isFound = true;
                    }

                if (command.parameters[47] == "1")
                    for (var i = 0; i < data.runtimeActorDataModels.Count; i++)
                    for (var i2 = 0; i2 < data.runtimeActorDataModels[i].equips.Count; i2++)
                        if (data.runtimeActorDataModels[i].equips[i2].itemId == command.parameters[46])
                        {
                            if (flowType != 1)
                                return true;
                            isFound = true;
                        }

                // OR以外は失敗
                if (flowType != 2 && isFound == false)
                    return false;
            }

            // ボタン
            if (command.parameters[48] == "1")
            {
                var keyCode = KeyCode.A;
                var inputType = InputType.Down;

                switch (command.parameters[49])
                {
                    case "0":
                        keyCode = KeyCode.Return;
                        break;
                    case "1":
                        keyCode = KeyCode.Escape;
                        break;
                    case "2":
                        keyCode = KeyCode.LeftShift;
                        break;
                    case "3":
                        keyCode = KeyCode.DownArrow;
                        break;
                    case "4":
                        keyCode = KeyCode.LeftArrow;
                        break;
                    case "5":
                        keyCode = KeyCode.RightArrow;
                        break;
                    case "6":
                        keyCode = KeyCode.UpArrow;
                        break;
                    case "7":
                        keyCode = KeyCode.PageUp;
                        break;
                    case "8":
                        keyCode = KeyCode.PageDown;
                        break;
                }

                switch (command.parameters[50])
                {
                    case "0":
                        inputType = InputType.Press;
                        break;
                    case "1":
                        inputType = InputType.Down;
                        break;
                    case "2":
                        inputType = InputType.Repeat;
                        break;
                }

                if (InputHandler.GetInputKey(keyCode, inputType))
                {
                    if (flowType != 1)
                        return true;
                }
                else if (flowType == 1)
                {
                    return false;
                }
            }

            // AND条件はtrueを返す
            if (flowType == 1)
                return true;
            return false;
        }
    }
}