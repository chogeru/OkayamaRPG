using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using System;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Message
{
    /// <summary>
    /// [選択肢]コマンドの実行内容枠の表示物
    /// </summary>
    public class MessageInputSelect : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            string backGround = "";
            string windowPosition = "";
            string defaultChoice = "";
            string cancelChoice = "";
            
            // 背景
            backGround  = eventCommand.parameters[1] switch
            {
                "0" => EditorLocalize.LocalizeText("WORD_1196"),    // ウィンドウ
                "1" => EditorLocalize.LocalizeText("WORD_1197"),    // 暗くする
                "2" => EditorLocalize.LocalizeText("WORD_1198"),    // 透明
                _ => EditorLocalize.LocalizeText("WORD_1196")       // 初期値
            };

            // ウィンドウ位置
            windowPosition  = eventCommand.parameters[2] switch
            {
                "-1" => EditorLocalize.LocalizeText("WORD_0366"),   // 初期設定
                "0" => EditorLocalize.LocalizeText("WORD_0813"),    // 上
                "1" => EditorLocalize.LocalizeText("WORD_0298"),    // 中
                "2" => EditorLocalize.LocalizeText("WORD_0814"),    // 下
                _ => EditorLocalize.LocalizeText("WORD_0366"),      // 初期値
            };

            // デフォルト選択肢
            defaultChoice = (Convert.ToInt32(eventCommand.parameters[3])+1).ToString() switch
            {
                "1" => EditorLocalize.LocalizeText("WORD_1538"),    // 選択肢1
                "2" => EditorLocalize.LocalizeText("WORD_1539"),    // 選択肢2
                "3" => EditorLocalize.LocalizeText("WORD_1540"),    // 選択肢3
                "4" => EditorLocalize.LocalizeText("WORD_1541"),    // 選択肢4
                "5" => EditorLocalize.LocalizeText("WORD_1542"),    // 選択肢5
                "6" => EditorLocalize.LocalizeText("WORD_1543"),    // 選択肢6
                _ => "-",                                           // なし
            };

            // キャンセル選択肢
            cancelChoice = (Convert.ToInt32(eventCommand.parameters[4])+1).ToString() switch
            {
                "1" => EditorLocalize.LocalizeText("WORD_1538"),    // 選択肢1
                "2" => EditorLocalize.LocalizeText("WORD_1539"),    // 選択肢2
                "3" => EditorLocalize.LocalizeText("WORD_1540"),    // 選択肢3
                "4" => EditorLocalize.LocalizeText("WORD_1541"),    // 選択肢4
                "5" => EditorLocalize.LocalizeText("WORD_1542"),    // 選択肢5
                "6" => EditorLocalize.LocalizeText("WORD_1543"),    // 選択肢6
                _ => "-",                                           // なし
            };

            ret = indent + $"◆{EditorLocalize.LocalizeText("WORD_0080")}: " +
                $"({backGround}, {windowPosition}, {defaultChoice}, {cancelChoice})";
            LabelElement.text = ret;

            Element.Add(LabelElement);
            return Element;
        }
    }
}