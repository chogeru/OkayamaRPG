using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common.View;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.Editor.MapEditor.Window.EventEdit.ExecutionContentsWindow;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Message
{
    /// <summary>
    ///     [文章の表示]コマンドによって実行内容枠に追加されるテキスト入力行の表示物。
    ///     <para>parameters[0]：実行時に表示する文字列</para>
    ///     <para>parameters[1]：値が入っていないコマンドがあるので使用禁止</para>
    /// </summary>
    public class MessageTextOneLine : AbstractEventText, IEventCommandView
    {
        /// <summary>行数の制限</summary>
        private const int maxLineNum = 4;

        /// <summary>1行あたりの文字数</summary>
        private const int charNumPerLine = 40;

        private const int charNumMax = 10000;

        /// <summary>表示されている入力欄の親VisualElementのリスト</summary>
        private readonly List<VisualElement> _textFieldParentList = new List<VisualElement>();

        private Action<bool> _updateEventMessageTextAndSaveEventAction;

        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            Element.style.flexDirection = FlexDirection.Column;

            //イベントを検索
            var instance = ExecutionContentsWindowParam.instance;
            var eventDataModels = EventManagementService.LoadEvent();
            var dataModelsIndex = instance.eventId;
            var page = instance.page;
            var eventIndex = -1;
            for (var i = 0; i < eventDataModels.Count; i++)
            {
                if (eventDataModels[i].id == dataModelsIndex && eventDataModels[i].page == page)
                {
                    eventIndex = i;
                    break;
                }
            }


            Element.RegisterCallback<FocusOutEvent>(evt => { UpdateEventMessageTextAndSaveEvent(false); });
            _updateEventMessageTextAndSaveEventAction = UpdateEventMessageTextAndSaveEvent;
            void UpdateEventMessageTextAndSaveEvent(bool saveOnly) {
                //編集中のコマンドに保存
                var eventDataModels = EventManagementService.LoadEvent();
                var result = new List<string>();
                for (var i = 0; i < _textFieldParentList.Count; i++)
                {
                    var textField = _textFieldParentList[i].Query<ImTextField>().First();
                    textField.style.whiteSpace = WhiteSpace.Normal;
                    result.Add(textField.value);
                }

                eventCommand.parameters[0] = string.Join("\n", result.ToArray());

                if (!saveOnly)
                {
                    ExecutionContentsWindow.SetUpData();
                }
                else
                {
                    EventManagementService.SaveEvent(eventDataModels[eventIndex]);
                }
            }

            // 各VisualElementを設定
            LabelElement.text = indent + "◇#";
            SetupContainer(0, eventCommand.parameters[0]);
            return Element;
        }

        /// <summary>
        ///     テキスト入力欄の初期化
        /// </summary>
        /// <param name="startIndex">初期化の開始地点。これより手前の入力欄はそのままの状態で残る</param>
        /// <param name="text">使用する文字列</param>
        private void SetupContainer(int startIndex, string text) {
            // テキストが空の場合は入力欄を1つ作って抜ける
            if (string.IsNullOrEmpty(text))
            {
                var element = GenerateTextArea(text);
                _textFieldParentList.Add(element);
                Element.Add(element);
                return;
            }

            // インデックスが指定されている場合はそのインデックス以降の入力欄を一旦削除して設定し直す
            if (startIndex > 0)
                for (var i = _textFieldParentList.Count - 1; i >= startIndex; i--)
                {
                    var element = _textFieldParentList[i];
                    Element.Remove(element);
                    _textFieldParentList.Remove(element);
                }

            var lines = text.Split('\n');
            var adjustedText = lines.ToList();

            // 4行ごとに1つの入力欄に収める
            for (var i = 0; i < adjustedText.Count; i += maxLineNum)
            {
                var lineText = "";
                var count = adjustedText.Count - i < maxLineNum ? adjustedText.Count - i : maxLineNum;

                lineText = string.Join("\n", adjustedText.ToArray(), i, count);
                var element = GenerateTextArea(lineText);
                _textFieldParentList.Add(element);
                Element.Add(element);
            }
        }

        /// <summary>
        ///     1行あたりの文字数と行数の制限をかける形で編集する
        /// </summary>
        /// <returns>編集後の文字列</returns>
        private void EditInputText(ImTextField textField) {
            var lines = textField.value.Split('\n');
            var lineNum = lines.Length < maxLineNum ? lines.Length : maxLineNum;
            textField.value = string.Join("\n", lines, 0, lineNum);

            // 入力欄の桁数上限以上入力された場合は別の入力欄に渡す
            if (lines.Length > maxLineNum)
            {
                var curretElementIndex = _textFieldParentList.FindIndex(v => v.Query<ImTextField>().First() == textField);
                var result = new List<string>();
            
                // 改行されただけの場合は先頭に改行コードを仕込む
                var surplusText = string.Join("\n", lines, maxLineNum, lines.Length - maxLineNum);
                result.Add(surplusText);
            
                // 自分より後ろにあるテキスト入力欄のテキストを収集
                for (var i = curretElementIndex + 1; i < _textFieldParentList.Count; i++)
                {
                    var tmpTextField = _textFieldParentList[i].Query<ImTextField>().First();
                    result.Add(tmpTextField.value);
                }
            
                // 自分より後ろにあるテキスト入力欄を生成し直す
                SetupContainer(curretElementIndex + 1, string.Join("\n", result.ToArray()));
            
                // フォーカスを追加された末尾の入力欄に合わせる
                var countOffset = result.Count(v => v.Contains("\n")) / 4 + 1;
                _textFieldParentList[curretElementIndex + countOffset].Query<ImTextField>().First().Focus();
            }
        }

        /// <summary>
        ///     入力欄を持つVisualElementを生成する
        /// </summary>
        /// <param name="text">初期値としてテキストボックスに表示する文字列</param>
        private VisualElement GenerateTextArea(string text) {
            var container = new VisualElement();
            var textField = new ImTextField();
            var label = new Label();

            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;
            label.text = LabelElement.text;

            textField.multiline = true;
            textField.style.width = 400;
            textField.value = text;

            // 値が変更された場合には、セーブのみ実施
            textField.RegisterValueChangedCallback(v =>
            {
                _updateEventMessageTextAndSaveEventAction(true);
            });

            // FocusOutした場合には、部品を作り直す
            textField.RegisterCallback<FocusOutEvent>(v =>
            {

                ExecutionContentsWindow.IsSaveWait = true;
                //一度に入力できる文字数の上限を設ける
                var textFieldValue = textField.value;
                if (textFieldValue.Length > charNumMax)
                {
                    textField.value = textFieldValue.Replace(textFieldValue.Substring(charNumMax),"");
                }

                
                EditInputText(textField);

                // 登録されている入力欄が丸々空白の場合は削除する
                var singleLine = textField.value.Replace("\r", "").Replace("\n", "");
                if (singleLine.Length == 0 && _textFieldParentList.Count > 1 &&
                    _textFieldParentList.Any(v => v == container))
                {
                    Element.Remove(container);
                    _textFieldParentList.Remove(container);
                }

                _updateEventMessageTextAndSaveEventAction(false);
            });

            container.Add(label);
            container.Add(textField);
            return container;
        }
    }
}