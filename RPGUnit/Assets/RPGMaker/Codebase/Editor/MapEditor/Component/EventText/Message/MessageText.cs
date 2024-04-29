using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Message
{
    /// <summary>
    ///     [文章の表示]コマンドの実行内容枠の表示物
    /// </summary>
    public class MessageText : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            var characterActorDataModels =
                DatabaseManagementService.LoadCharacterActor();

            var nameIndex = 0;
            if (eventCommand.parameters.Count > 9 && eventCommand.parameters[9] != "")
            {
                for (int i = 0; i < characterActorDataModels.Count; i++)
                {
                    if (eventCommand.parameters.Count > 9 && characterActorDataModels[i].uuId == eventCommand.parameters[9])
                    {
                        nameIndex = i + 1;
                        break;
                    }
                }
            }
            if (nameIndex > 0)
            {
                //キャラクターが存在した場合、名前、顔画像、ピクチャ画像を更新
                eventCommand.parameters[6] =
                    characterActorDataModels[nameIndex - 1].basic.name;
                eventCommand.parameters[7] =
                    characterActorDataModels[nameIndex - 1].image.face;
                eventCommand.parameters[8] =
                    characterActorDataModels[nameIndex - 1].image.adv;
            }

            // 名前
            var name = eventCommand.parameters[6];

            // 画像名
            var faceIconName = eventCommand.parameters[7];
            var pictorName = eventCommand.parameters[8];

            // 背景
            var backGround = eventCommand.parameters[3] switch
            {
                "0" => EditorLocalize.LocalizeText("WORD_1196"), // ウィンドウ
                "1" => EditorLocalize.LocalizeText("WORD_1197"), // 暗くする
                "2" => EditorLocalize.LocalizeText("WORD_1198"), // 透明
                _ => EditorLocalize.LocalizeText("WORD_1196") // 初期値
            };

            // ウィンドウ位置
            var windowPosition = eventCommand.parameters[4] switch
            {
                "-1" => EditorLocalize.LocalizeText("WORD_0062"), // 初期設定
                "0" => EditorLocalize.LocalizeText("WORD_0297"), // 上
                "1" => EditorLocalize.LocalizeText("WORD_0298"), // 中
                "2" => EditorLocalize.LocalizeText("WORD_0299"), // 下
                _ => EditorLocalize.LocalizeText("WORD_0062") // 初期値
            };

            // MV準拠で「画像名（仕様上顔アイコンとキャラ画像の2つ）, 背景, ウィンドウ位置」を実行内容枠に表示
            ret = indent + $"◆ {EditorLocalize.LocalizeText("WORD_1202")}: " +
                  $"{name}, {faceIconName}, {pictorName}, {backGround}, {windowPosition}";

            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }

        public virtual MessageText Clone() {
            var eventData = new MessageText();
            eventData.Element = Element;
            eventData.LabelElement = LabelElement;
            eventData.ret = ret;
            return eventData;
        }
    }
}