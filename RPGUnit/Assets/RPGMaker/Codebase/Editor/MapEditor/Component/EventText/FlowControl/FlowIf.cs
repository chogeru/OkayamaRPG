using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Enemy;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.FlowControl
{
    public class FlowIf : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            // メモ
            // [0] 条件を満たさない分岐(0:しない 1:する)
            // [1] 複数選択の可否(0:しない 1:する)
            // [2] 処理の方法(0:AND 1:OR)
            // [3] 有効化(0 or 1)
            // [4] スイッチ選択ID
            // [5] 操作選択(0 or 1)
            // [6] 変数有効化(0 or 1)
            // [7] 変数ID
            // [8] 条件式選択(0:＝ 1:≧ 2:≦ 3:＞ 4:＜ 5:≠)
            // [9] 定数有効化(0 or 1)
            // [10] 定数の値
            // [11] 変数ID
            // [12] セルフスイッチの有効化(0 or 1)
            // [13] セルフスイッチの選択(0:A 1:B 2:C 3:D)
            // [14] セルフスイッチ操作選択(0:ON 1:OFF)
            // [15] タイマー有効化(0 or 1)
            // [16] タイマー条件式選択(0:≧ 1:≦)
            // [17] 分
            // [18] 秒
            // [19] アクター有効化(0 or 1)
            // [20] アクターID
            // [21] 有効化(0:パーティーに居る 1:名前 2:職業 3:スキル 4:武器 5:防具 6:ステート)
            // [22] 名前入力
            // [23] 職業ID
            // [24] スキルID
            // [25] 武器ID
            // [26] 防具ID
            // [27] ステートID
            // [28] 敵キャラ有効化(0 or 1)
            // [29] 敵キャラID
            // [30] 出現orステート
            // [31] 敵キャラステートID
            // [32] キャラクター有効化(0 or 1)
            // [33] イベント選択(-2:プレイヤー -1:このイベント 0…イベント)
            // [34] 向き(0:下 1:左 2:右 3:上)
            // [35] 乗り物有効化(0 or 1)
            // [36] 乗り物ID
            // [37] 所持金有効化(0 or 1)
            // [38] 所持金条件式(0:≧ 1:≦ 2:＜)
            // [39] 所持金
            // [40] アイテム有効化(0 or 1)
            // [41] アイテムのID
            // [42] 武器の有効化(0 or 1)
            // [43] 武器のID
            // [44] 装備含む(0 or 1)
            // [45] 防具の有効化(0 or 1)
            // [46] 防具のID
            // [47] 装備含む(0 or 1)
            // [48] ボタン有効化(0 or 1)
            // [49] ボタン(0:決定 1:キャンセル 2:シフト 3:ダウン 4:レフト 5:ライト 6:アップ 7:ページアップ 8:ページダウン)
            // [50] アクション選択(0:が押されている 1:がトリガーされている 2:がリピートされている)				

            ret = indent;

            ret += "◆" + EditorLocalize.LocalizeText("WORD_1139") + " : ";
            if (int.Parse(eventCommand.parameters[1]) == 1)
            {
                LabelElement.text = ret;
                Element.Add(LabelElement);
                return Element;
            }

            if (int.Parse(eventCommand.parameters[3]) == 1)
            {
                var switchs = _GetSwitchList();
                var data = switchs.FirstOrDefault(c => c.id == eventCommand.parameters[4]);
                var name = "";
                if (data != null)
                    name = data.name;
                if (name == "") name = EditorLocalize.LocalizeText("WORD_1518");

                ret += "#" + (switchs.IndexOf(data) + 1).ToString("0000") + " " + name + " = ";

                if (int.Parse(eventCommand.parameters[5]) == 0)
                    ret += "ON";
                else
                    ret += "OFF";
            }

            else if (int.Parse(eventCommand.parameters[6]) == 1)
            {
                var variables = _GetVariablesList();
                var data = variables.FirstOrDefault(c => c.id == eventCommand.parameters[7]);
                var name = data.name;
                if (name == "") name = EditorLocalize.LocalizeText("WORD_1518");

                ret += "#" + (variables.IndexOf(data) + 1).ToString("0000") + " " + name;
                var formula = new List<string> {"=", "≧", "≦", "＞", "＜", "≠"};
                ret +=
                    " " + formula[int.Parse(eventCommand.parameters[8])] + " ";
                if (int.Parse(eventCommand.parameters[9]) == 1)
                {
                    ret += eventCommand.parameters[10];
                }
                else
                {
                    var data2 = variables.FirstOrDefault(c => c.id == eventCommand.parameters[11]);
                    if (data2 == null) data2 = variables[0];

                    var name2 = data2.name;
                    if (name2 == "") name2 = EditorLocalize.LocalizeText("WORD_1518");

                    ret += "#" + (variables.IndexOf(data2) + 1).ToString("0000") + " " + name2;
                }
            }

            else if (int.Parse(eventCommand.parameters[12]) == 1)
            {
                var selfSwitchList = new List<string> {"A", "B", "C", "D"};
                ret += EditorLocalize.LocalizeText("WORD_0840") + " " +
                       selfSwitchList[int.Parse(eventCommand.parameters[13])] + " = ";
                if (int.Parse(eventCommand.parameters[14]) == 0)
                    ret += "ON";
                else
                    ret += "OFF";
            }

            else if (int.Parse(eventCommand.parameters[15]) == 1)
            {
                ret += EditorLocalize.LocalizeText("WORD_1043");
                var timer = new List<string> {"≧", "≦"};
                ret +=
                    " " + timer[int.Parse(eventCommand.parameters[16])] + " ";
                ret +=
                    int.Parse(eventCommand.parameters[17]) + EditorLocalize.LocalizeText("WORD_1051") + " " +
                    int.Parse(eventCommand.parameters[18]) + EditorLocalize.LocalizeText("WORD_0938");
            }

            else if (int.Parse(eventCommand.parameters[19]) == 1)
            {
                ret += DatabaseManagementService.LoadCharacterActor()
                    .FirstOrDefault(c => c.uuId == eventCommand.parameters[20])
                    ?.basic
                    .name + " ";
                if (int.Parse(eventCommand.parameters[21]) == 0)
                    ret += EditorLocalize.LocalizeText("WORD_1600");
                else if (int.Parse(eventCommand.parameters[21]) == 1)
                    ret += EditorLocalize.LocalizeText("WORD_0039") + " = " + eventCommand.parameters[22];
                else if (int.Parse(eventCommand.parameters[21]) == 2)
                    ret += EditorLocalize.LocalizeText("WORD_0336") + " = " + DatabaseManagementService
                        .LoadCharacterActorClass().FirstOrDefault(c => c.basic.id == eventCommand.parameters[23])
                        ?.basic
                        .name;
                else if (int.Parse(eventCommand.parameters[21]) == 3)
                    ret += DatabaseManagementService.LoadSkillCustom()
                               .FirstOrDefault(c => c.basic.id == eventCommand.parameters[24])
                               ?.basic.name +
                           EditorLocalize.LocalizeText("WORD_1145");
                else if (int.Parse(eventCommand.parameters[21]) == 4)
                    ret += DatabaseManagementService.LoadWeapon()
                               .FirstOrDefault(c => c.basic.id == eventCommand.parameters[25])
                               ?.basic.name +
                           EditorLocalize.LocalizeText("WORD_1146");
                else if (int.Parse(eventCommand.parameters[21]) == 5)
                    ret += DatabaseManagementService.LoadArmor()
                               .FirstOrDefault(c => c.basic.id == eventCommand.parameters[26])
                               ?.basic.name +
                           EditorLocalize.LocalizeText("WORD_1146");
                else if (int.Parse(eventCommand.parameters[21]) == 6)
                {
                    var stateName = EditorLocalize.LocalizeText("WORD_0113");
                    var state = DatabaseManagementService.LoadStateEdit()
                               .FirstOrDefault(c => c.id == eventCommand.parameters[27]);

                    if (state != null)
                    {
                        stateName = state.name;
                    }


                    ret += stateName + EditorLocalize.LocalizeText("WORD_1147");
                }

            }

            // 敵キャラ有効化
            else if (int.Parse(eventCommand.parameters[28]) == 1)
            {
                int.TryParse(eventCommand.parameters[29], out var memberIndex);
                if (GetEnemyNameList().Count <= memberIndex)
                {
                    memberIndex = 0;
                }

                string data = "";
                if (GetEnemyNameList().Count > memberIndex)
                    data = GetEnemyNameList()[memberIndex];
                ret += $" : #{memberIndex + 1} {data}";

                if (int.Parse(eventCommand.parameters[30]) == 0)
                    ret += EditorLocalize.LocalizeText("WORD_1148");
                else
                    ret += DatabaseManagementService.LoadStateEdit()
                               .FirstOrDefault(c => c.id == eventCommand.parameters[31])?.name +
                           EditorLocalize.LocalizeText("WORD_1147");
            }

            else if (int.Parse(eventCommand.parameters[32]) == 1)
            {
                var name = "";
                if (eventCommand.parameters[33] == "-2")
                    name = EditorLocalize.LocalizeText("WORD_0860");
                else if (eventCommand.parameters[33] == "-1")
                    name = EditorLocalize.LocalizeText("WORD_0920");
                else
                    name = GetEventDisplayName(eventCommand.parameters[33]);

                name += " ";

                var direct = new List<string> {"WORD_0299", "WORD_0813", "WORD_0814", "WORD_0297"};
                ret += name + EditorLocalize.LocalizeText("WORD_1149") + " " +
                       EditorLocalize.LocalizeText(direct[int.Parse(eventCommand.parameters[34])]);
            }

            // 乗り物
            else if (int.Parse(eventCommand.parameters[35]) == 1)
            {
                ret += DatabaseManagementService.LoadCharacterVehicles()
                           .FirstOrDefault(c => c.id == eventCommand.parameters[36])?.name + " " +
                       EditorLocalize.LocalizeText("WORD_1150");
            }

            // 所持金
            else if (int.Parse(eventCommand.parameters[37]) == 1)
            {
                ret += EditorLocalize.LocalizeText("WORD_1544");
                var gold = EditorLocalize.LocalizeTexts(new List<string> {"WORD_1509", "WORD_1510", "WORD_1512"});
                ret += EditorLocalize.LocalizeText("WORD_0581") + " " + gold[int.Parse(eventCommand.parameters[38])] +
                       " " + int.Parse(eventCommand.parameters[39]);
            }

            // アイテム
            else if (int.Parse(eventCommand.parameters[40]) == 1)
            {
                ret += DatabaseManagementService.LoadItem()
                           .FirstOrDefault(c => c.basic.id == eventCommand.parameters[41])?.basic.name +
                       EditorLocalize.LocalizeText("WORD_1151");
            }

            // 武器
            else if (int.Parse(eventCommand.parameters[42]) == 1)
            {
                ret += DatabaseManagementService.LoadWeapon()
                           .FirstOrDefault(c => c.basic.id == eventCommand.parameters[43])?.basic.name +
                       EditorLocalize.LocalizeText("WORD_1151");
            }

            // 防具
            else if (int.Parse(eventCommand.parameters[45]) == 1)
            {
                ret += DatabaseManagementService.LoadArmor()
                           .FirstOrDefault(c => c.basic.id == eventCommand.parameters[46])?.basic.name +
                       EditorLocalize.LocalizeText("WORD_1151");
            }

            else if (int.Parse(eventCommand.parameters[48]) == 1)
            {
                var buttons = new List<string>
                {
                    "WORD_1154",
                    "WORD_1155",
                    "WORD_1156",
                    "WORD_1157",
                    "WORD_1158",
                    "WORD_1159",
                    "WORD_1160",
                    "WORD_1161",
                    "WORD_1162"
                };
                var pushd = new List<string>
                {
                    "WORD_1163",
                    "WORD_1164",
                    "WORD_1165"
                };
                ret += EditorLocalize.LocalizeText("WORD_1153") + " [" + EditorLocalize.LocalizeText(buttons[int.Parse(eventCommand.parameters[49])]) + "] " +
                       EditorLocalize.LocalizeText(pushd[int.Parse(eventCommand.parameters[50])]);
            }

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }

        private List<FlagDataModel.Switch> _GetSwitchList() {
            var flagDataModel = DatabaseManagementService.LoadFlags();
            var fileNames = new List<FlagDataModel.Switch>();
            for (var i = 0; i < flagDataModel.switches.Count; i++) fileNames.Add(flagDataModel.switches[i]);

            return fileNames;
        }

        private List<FlagDataModel.Variable> _GetVariablesList() {
            var flagDataModel = DatabaseManagementService.LoadFlags();
            var fileNames = new List<FlagDataModel.Variable>();
            for (var i = 0; i < flagDataModel.variables.Count; i++) fileNames.Add(flagDataModel.variables[i]);

            return fileNames;
        }

        private List<EnemyDataModel> _GetEnemyList() {
            var enemyDataModels =
                DatabaseManagementService.LoadEnemy();
            var fileNames = new List<EnemyDataModel>();
            for (var i = 0; i < enemyDataModels.Count; i++)
                if (enemyDataModels[i].deleted == 0)
                    fileNames.Add(enemyDataModels[i]);

            return fileNames;
        }
    }
}