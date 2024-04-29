//#define ENABLE_LOG

#if ENABLE_LOG
using RPGMaker.Codebase.CoreSystem.Helper;
#endif
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Common.View
{
    /// <summary>
    /// UIElementsのTextFieldを、IMGUIのUIをIMGUIContainerで内包したVisualElementに差し替えるためのクラス。
    /// </summary>
    /// <remarks>
    /// 
    /// ■機能と名前
    /// 
    /// VisualElementのもの以外で使用可能な本クラスのプロパティとuxmlの属性は以下の通り。
    /// 
    ///     プロパティ  uxml属性    優先順位
    ///     ----------- ----------- --------  
    ///     value       value       (機能外)
    ///     multiline   multiline   高
    ///     maxLength   max-length  ↑
    ///     isReadOnly  readonly
    ///     isDelayed   is-delayed  ↓
    ///     label       label       低
    ///     
    /// 名前はUIElementsのTextFieldと互換。
    /// 機能はUIElementsのTextFieldとだいたい互換。
    ///     
    /// ■同時に組み合わせて使用可能な機能の制限
    /// 
    /// 同時に組み合わせて使用可能な機能 (対応するuxml属性での設定も含む)。
    ///
    ///     multiline と maxLength
    ///     maxLength のみ
    ///     isReadOnly のみ
    ///     isDelayed と label
    ///     label のみ
    /// 
    /// IMGUIは機能ごとにUIが分かれており、自由に機能のオンオフができない為、上記の制限となる。
    /// また、組み合わせて使用不可な機能を同時に設定した場合、優先順位の高い機能が適用され、
    /// そうでない機能の設定は無視される。
    /// 設定時に無視される機能があった場合、コンソールに警告ログが表示される。
    /// 
    /// ■本クラスを用意した理由
    /// 
    /// UIElementsのTextFieldは、以下などの問題がある為。
    /// 
    /// * 入力中のテキストの表示がおかしい (折り返し位置など)。
    /// * テキスト入力中のカーソル移動がおかしい。
    /// * IME変換中の文字列が表示されない。
    /// 
    /// ■注意
    /// 
    /// value に null を代入しようとした場合、実際は string.Empty が代入されます。
    /// 
    /// </remarks>
    public class ImTextField : VisualElement, INotifyValueChanged<string>
    {
        /// <summary>
        /// 組む合わせ可能な機能名列リスト (優先順位順)。
        /// </summary>
        string[][] combinableFunctionNamesList = new string[][]
        {
            new string[]{ nameof(multiline), nameof(maxLength) },
            new string[]{ nameof(maxLength) },
            new string[]{ nameof(isReadOnly) },
            new string[]{ nameof(isDelayed),  nameof(label) },
            new string[]{ nameof(label) },
        };

        private string _value = string.Empty;

        private bool _multiline = false;
        private int _maxLength = -1;
        private bool _isReadOnly = false;
        private bool _isDelayed = false;
        private string _label = string.Empty;

        private bool inFocus = false;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="label">ラベルテキスト。</param>
        public ImTextField(string label)
            : this()
        {
            this.label = label;
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public ImTextField()
            : base()
        {
            Add(new IMGUIContainer(() =>
            {
                var imGuiStyle =
                    new GUIStyle(!multiline ? EditorStyles.textField : EditorStyles.textArea) { wordWrap = true };

                var newValue = value;

                // 優先順位の順に実行。combinableFunctionNamesListに優先順位順に機能の可能な組み合わせを記述している。
                if (multiline)
                {
                    // maxLengthには対応。
                    newValue = maxLength >= 0 ?
                        GUILayout.TextArea(value, maxLength, imGuiStyle) :
                        EditorGUILayout.TextArea(value, imGuiStyle);
                }
                else if (maxLength >= 0)
                {
                    newValue = GUILayout.TextField(value, maxLength, imGuiStyle);
                }
                else if (isReadOnly)
                {
                    EditorGUILayout.SelectableLabel(value, imGuiStyle, GUILayout.Height(18));
                }
                else if (isDelayed)
                {
                    // labelには対応。
                    newValue = !string.IsNullOrEmpty(label) ?
                        EditorGUILayout.DelayedTextField(label, value, imGuiStyle) :
                        EditorGUILayout.DelayedTextField(value, imGuiStyle);
                }
                else
                {
                    // labelには対応。
                    newValue = !string.IsNullOrEmpty(label) ?
                        EditorGUILayout.TextField(label, value, imGuiStyle) :
                        EditorGUILayout.TextField(value, imGuiStyle);
                }

                value = newValue;
            }));

            RegisterCallback<FocusInEvent>(focusIntEvent =>
            {
                inFocus = true;
            });

            RegisterCallback<FocusOutEvent>(focusOutEvent =>
            {
                inFocus = false;
            });

#if ENABLE_LOG
            RegisterCallback<FocusEvent>(focusEvent =>
            {
                DebugUtil.Log($"RegisterCallback<FocusEvent>({value})");
            });

            RegisterCallback<FocusInEvent>(focusIntEvent =>
            {
                DebugUtil.Log($"RegisterCallback<FocusInEvent>({value})");
            });

            RegisterCallback<FocusOutEvent>(focusOutEvent =>
            {
                DebugUtil.Log($"RegisterCallback<FocusOutEvent>({value})");
            });

            RegisterCallback<BlurEvent>(blurEvent =>
            {
                DebugUtil.Log($"RegisterCallback<blurEvent>({value})");
            });

            this.RegisterValueChangedCallback(changeEvent =>
            {
                DebugUtil.Log($"RegisterValueChangedCallback({changeEvent.previousValue}→{changeEvent.newValue})");
            });
#endif

            style.flexGrow = 1f;
        }

        public virtual string value
        {
            get
            {
                return _value;
            }

            set
            {
                var newValue = value ?? string.Empty;
                if (newValue == _value)
                {
                    return;
                }

                // 値を変更し、RegisterValueChangedCallbackメソッドで登録したメソッドを呼ぶ。
                using ChangeEvent<string> changeEvent = ChangeEvent<string>.GetPooled(_value, newValue);
                changeEvent.target = this;
                SetValueWithoutNotify(newValue);
                SendEvent(changeEvent);
            }
        }

        public bool multiline
        {
            get
            {
                return _multiline;
            }

            set
            {
                if (value == _multiline)
                {
                    return;
                }

                _multiline = value;
                WarnningIfNecessary(nameof(multiline));
            }
        }

        // from class TextInputBaseField<TValueType> 

        public int maxLength
        {
            get
            {
                return _maxLength;
            }

            set
            {
                if (value == _maxLength)
                {
                    return;
                }

                _maxLength = value;
                WarnningIfNecessary(nameof(maxLength));
            }
        }

        public bool isReadOnly
        {
            get
            {
                return _isReadOnly;
            }

            set
            {
                if (value == _isReadOnly)
                {
                    return;
                }

                _isReadOnly = value;
                WarnningIfNecessary(nameof(isReadOnly));
            }
        }

        public bool isDelayed
        {
            get
            {
                return _isDelayed;
            }

            set
            {
                if (value == _isDelayed)
                {
                    return;
                }

                _isDelayed = value;
                WarnningIfNecessary(nameof(isDelayed));
            }
        }

        // from class BaseField<TValueType>

        public string label
        {
            get
            {
                return _label;
            }

            set
            {
                if (value == _label)
                {
                    return;
                }

                _label = value;
                WarnningIfNecessary(nameof(label));
            }
        }

        public new class UxmlFactory : UxmlFactory<ImTextField, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription valueAttribute = new() { name = "value" };

            // from class TextField

            private readonly UxmlBoolAttributeDescription multilineAttribute = new() { name = "multiline" };

            // from class TextInputBaseField<TValueType>

            private UxmlIntAttributeDescription maxLengthAttribute = new()
            {
                name = "max-length",
                obsoleteNames = new string[] { "maxLength" },
                defaultValue = -1
            };

            private UxmlBoolAttributeDescription isReadOnlyAttribute = new() { name = "readonly" };
            private UxmlBoolAttributeDescription isDelayedAttribute = new() { name = "is-delayed" };

            // from class BaseField<TValueType>

            private UxmlStringAttributeDescription labelAttribute = new() { name = "label" };


            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var imTextField = (ImTextField)ve;
                imTextField._value = valueAttribute.GetValueFromBag(bag, cc);
                imTextField.multiline = multilineAttribute.GetValueFromBag(bag, cc);
                imTextField.maxLength = maxLengthAttribute.GetValueFromBag(bag, cc);
                imTextField.isReadOnly = isReadOnlyAttribute.GetValueFromBag(bag, cc);
                imTextField.isDelayed = isDelayedAttribute.GetValueFromBag(bag, cc);
                imTextField.label = labelAttribute.GetValueFromBag(bag, cc);
            }
        }

        /// <summary>
        /// INotifyValueChanged<string>インターフェイスに必要なメソッド。
        /// </summary>
        /// <param name="newValue">新しい値。</param>
        public virtual void SetValueWithoutNotify(string newValue)
        {
            _value = newValue;
        }

        /// <summary>
        /// このUIを保持しているウィンドウがキーボードフォーカスを失ったときに呼び出されます。
        /// </summary>
        /// <remarks>
        /// VisualElementが、他のEditorWindowもしくは他のアプリのウィンドウにフォーカスが移ってもFocusOutEventが
        /// 発生しないため、このメソッドでFocusOutEventを送信する。
        /// </remarks>
        public void OnOwnerWindowLostFocus()
        {
            if (!inFocus)
            {
                return;
            }

            inFocus = false;

            Focusable focusable = this;
            Focusable willGiveFocusTo = null;
            FocusChangeDirection direction = FocusChangeDirection.unspecified;
            FocusController focusController = focusable.focusController;

            using FocusOutEvent e = FocusOutEvent.GetPooled(focusable, willGiveFocusTo, direction, focusController);
            focusable.SendEvent(e);
        }

        /// <summary>
        /// 機能の組み合わせで無効な設定があれば警告を表示する。
        /// </summary>
        private void WarnningIfNecessary(string functionName)
        {
            var functions = new Function[]
            {
                new Function { name = nameof(multiline), enabled = multiline },
                new Function { name = nameof(maxLength), enabled = maxLength >= 0 },
                new Function { name = nameof(isReadOnly), enabled = isReadOnly },
                new Function { name = nameof(isDelayed), enabled = isDelayed },
                new Function { name = nameof(label), enabled = !string.IsNullOrEmpty(label) },
            };

            var function = functions.Single(function => function.name == functionName);

            if (!function.enabled)
            {
                return;
            }

            // 有効設定な機能名列。
            var enabledFunctionNames = functions.Where(function => function.enabled).Select(function => function.name);

            // 組む合わせ可能な機能名列。
            var combinableFunctionNames = combinableFunctionNamesList.First(
                pairableFunctionNames => enabledFunctionNames.Contains(pairableFunctionNames.First()));

            // 無視される機能名列。
            var ignoredFunctionNames = enabledFunctionNames.Where(
                enabledFunctionName => !combinableFunctionNames.Contains(enabledFunctionName));

            if (!ignoredFunctionNames.Any())
            {
                return;
            }

            Debug.LogWarning(
                $"{function.name}が有効値に設定され、" +
                $"{string.Join("と", enabledFunctionNames)}が有効な状態ですが、" +
                $"同時適用できない低優先順位な機能{string.Join("と", ignoredFunctionNames)}は無視されます。");
        }

        private class Function
        {
            public string name;
            public bool enabled;
        }
    }
}