using RPGMaker.Codebase.Runtime.Battle.Objects;
using TMPro;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Battle.Window
{
    /// <summary>
    /// 項目が選択された際の解説などを表示するウィンドウ
    /// </summary>
    public class WindowHelp : WindowBattle
    {
        /// <summary>
        /// 表示される文
        /// </summary>
        private string _text;
        [SerializeField] protected TextMeshProUGUI textField;
        [SerializeField] protected TextMeshProUGUI textField2;

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="numLines"></param>
        public void Initialize(int? numLines = null) {
            base.Initialize();
            _text = "";

            //共通UIの適応を開始
            Init();

            Refresh();
        }

        /// <summary>
        /// 指定文字を表示
        /// </summary>
        /// <param name="text"></param>
        public void SetText(string text) {
            if (_text != text)
            {
                _text = text;
                Refresh();
            }
        }

        /// <summary>
        /// 文字の消去
        /// </summary>
        public void Clear() {
            SetText("");
        }

        /// <summary>
        /// 指定アイテムの説明を表示
        /// </summary>
        /// <param name="item"></param>
        public void SetItem(GameItem item) {
            SetText(item != null ? item.Description : "");
        }

        /// <summary>
        /// 再描画
        /// </summary>
        public void Refresh() {
            textField.text = _text;

            var width = textField.transform.parent.GetComponent<RectTransform>().sizeDelta +
                        textField.GetComponent<RectTransform>().sizeDelta;
            textField.text = "";
            textField2.text = "";
            if (_text.Contains("\\n"))
            {
                var textList = _text.Split("\\n");
                textField.text = textList[0];
                if (width.x >= textField.preferredWidth)
                {
                    textField2.text = textList[1];
                    return;
                }
                _text = textList[0];
            }
            var isNextLine = false;
            textField.text = "";
            textField2.text = "";
            for (int i = 0; i < _text.Length; i++)
            {
                if (!isNextLine)
                {
                    textField.text += _text[i];
                    if (width.x <= textField.preferredWidth)
                    {
                        var lastChara = textField.text.Substring(textField.text.Length - 1);
                        textField2.text += lastChara;
                        textField.text = textField.text.Remove(textField.text.Length - 1);
                        isNextLine = true;
                    }
                }
                else
                {
                    textField2.text += _text[i];
                    if (width.x <= textField2.preferredWidth)
                    {
                        textField2.text = textField2.text.Remove(textField2.text.Length - 1);
                        break;
                    }
                }
            }
        }
    }
}