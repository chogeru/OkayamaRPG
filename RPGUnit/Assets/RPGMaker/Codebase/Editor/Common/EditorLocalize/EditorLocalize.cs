// 日本語への変換が失敗したら、表示テキストを元のまま表示する（エラーテキストを追加しない） 。
#define IF_CONVET_TO_JAPANESE_FAILS_DISPLAY_ORIGINAL

// 非公開クラスから設定言語を取得する。
#define GET_LANGUAGE_FROM_PRIVATE_CLASS

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Common
{
    /**
     * エディターローカライズ クラス。
     * 
     * ◆ローカライズする方針は大きく分けて2つ。
     * 
     *  1. uxml に直接記載しているテキスト文字の場合。
     *  
     *      uxml を読み込んだ直後に、uxml 内に含まれている text 文字列を、ローカライズ対象として置換する。
     *      
     *      対象クラスは以下 (追加の可能性あり)。
     *          Label
     *          Button
     *		
     *		使用メソッドは以下。
     *		    public static VisualElement LocalizeElements(VisualElement visualElement)
     *
     *
     *  2. プログラム内で直接埋め込んでいるテキストの場合。
     *  
     *	    たとえば、イベント一覧等では、設定しているパラメータに応じて表示する文字を変更する必要がある。
     *      そのようなケースは、プログラム内部でローカライズを直接行う。
     *
     *		使用メソッドは以下。
     *          public static string LocalizeWindowTitle(string text);
     *          public static string LocalizeText(string text)
     *          public static List<string> LocalizeTexts(List<string> texts)
     *          public static Dictionary<T, string> LocalizeDictionaryValues<T>(Dictionary<T, string> dictionary)
     *          
     * ◆ローカライズ用データ。
     * 
     * ローカライズデータは、クラス EditorLocalizeData で定義している。
     * キーが重複している場合、キーの末尾に " (番号)" を追加して重複が回避される。
     * よって重複したキーでローカライズしたい場合は、上記ように変更されたキーを指定する必要がある。
     *
     * ◆シンボル DEBUG 定義時の動作。
     * 
     * 指定キーが存在せずローカライズが失敗した場合、表示テキストは指定キーにエラーを示す以下のいずれかの文字列が追加されたものになる。
     *   " (翻訳キー重複)"
     *   " (翻訳キーなし)"
     * ただし日本語へのローカライズの場合、開発効率を考慮し上記文字列は追加されず、指定キーがそのまま表示テキストとなる。
     * 日本語へのローカライズの場合もエラーを示す文字列を追加したい場合は、本クラス中の以下のシンボルを未定義することで実現できる。
     *      IF_CONVET_TO_JAPANESE_FAILS_DISPLAY_ORIGINAL
     * 
     * キーが重複している場合やローカライズが失敗した場合は、コンソールウィンドウに警告ログが表示される。
     * 
     * ◆設定言語の取得。
     * 
     * Unityエディターの設定言語は非公開クラス LocalizationDatabase のプロパティから取得している。
     *      UnityEditor.LocalizationDatabase.currentEditorLanguageProperty
     * 以下のシンボルを未定義にすれば非公開クラスの参照を行わなくなるが、取得した言語情報はOSの設定言語となる。
     *      GET_LANGUAGE_FROM_PRIVATE_CLASS
     * 本クラス中の定数 ForceSystemLanguage の値を変更することにより、強制的に取得設定言語情報を変更しテストできる。
     * 
     */
    public static class EditorLocalize
    {
        // テスト用の強制言語設定。
        private const SystemLanguage ForceSystemLanguage =
#if DEBUG && false
            SystemLanguage.English;
#else
            SystemLanguage.Unknown;
#endif

        // 重複キーカウント。
        private static readonly Dictionary<string, int> DuplicateKeyCount = new Dictionary<string, int>();

        // ローカライズ辞書。
        private static readonly Dictionary<string, Dictionary<SystemLanguage, string>> LocaliseDictionary =
            new Dictionary<string, Dictionary<SystemLanguage, string>>();

        // ローカライズ対象外の文字列。
        private static readonly HashSet<string> NonLocalizableStrings = new HashSet<string> { "+", "-","X","Y","%","％",":",";" };

        /**
         * 静的コンストラクタ。
         */
        static EditorLocalize() {
            // 内容が重複した行を除いた、行と列の2次元ジャグ配列を作成。
            var localiseData = new List<List<string>>();
            {
                var lineValueHashSet = new HashSet<string>();
                for (var lineIndex = 0; lineIndex < EditorLocalizeData.LocaliseData.GetLength(0); lineIndex++)
                {
                    var localiseLineData = new List<string>();
                    for (var columnIndex = 0; columnIndex < (int) EditorLocalizeData.DataType.ValueBottom + 1; columnIndex++) localiseLineData.Add(EditorLocalizeData.LocaliseData[lineIndex, columnIndex]);

                    var lineValue = string.Join("\t", localiseLineData);

                    // 全ての値が同じ行は追加しない。
                    if (lineValueHashSet.Contains(lineValue)) continue;

                    lineValueHashSet.Add(lineValue);
                    localiseData.Add(localiseLineData);
                }
            }

            // 重複キーを考慮した翻訳用辞書を作成。
            foreach (var lineValues in localiseData)
            {
                var key = lineValues[(int) EditorLocalizeData.DataType.Key];

                if (!DuplicateKeyCount.ContainsKey(key)) DuplicateKeyCount.Add(key, 0);

                DuplicateKeyCount[key]++;

                // 最初のキー重複時は、1つ目の要素のキーを重複用のものに変更。
                if (DuplicateKeyCount[key] == 2)
                    continue;

                // 重複キーを重複用のものに変更。
                if (DuplicateKeyCount[key] >= 2)
                {
                    var newKey = key + DuplicateKeySuffix(DuplicateKeyCount[key]);
                    InitalizeDuplicateKeyLog(
                        key,
                        newKey,
                        lineValues.Skip((int) EditorLocalizeData.DataType.ValueTop).Take(EditorLocalizeData.ValueCount).ToList());
                    key = newKey;
                }

                LocaliseDictionary.Add(
                    key,
                    new Dictionary<SystemLanguage, string>
                    {
                        { SystemLanguage.Japanese, lineValues[(int)EditorLocalizeData.DataType.Japanese] },
                        { SystemLanguage.English, lineValues[(int)EditorLocalizeData.DataType.English] },
                        { SystemLanguage.Chinese, lineValues[(int)EditorLocalizeData.DataType.Chinese] }
                    });
            }
        }

        /**
         * ウィンドウタイトルをローカライズしたテキストに変換する。
         * 
         * ウィンドウタイトルは特別な処理をする可能性を考慮し、
         * 変換用専用メソッドを用意。
         */
        public static string LocalizeWindowTitle(string text) {
            return LocalizeText(text);
        }

        /**
         * テキストリストをローカライズしたテキストリストに変換する。
         */
        public static List<string> LocalizeTexts(List<string> texts) {
            foreach (var index in Enumerable.Range(0, texts.Count)) texts[index] = LocalizeText(texts[index]);

            return texts;
        }

        /**
         * ディクショナリ内の値テキストをローカライズしたディクショナリに変換する。
         */
        public static Dictionary<T, string> LocalizeDictionaryValues<T>(Dictionary<T, string> dictionary) {
            var keys = new List<T>(dictionary.Keys);
            foreach (var key in keys) dictionary[key] = LocalizeText(dictionary[key]);

            return dictionary;
        }

        /**
         * テキストをローカライズしたテキストに変換する。
         */
        public static string LocalizeText(string text) {
            var trimmedText = text.Trim();

            // null、空白、数値に解釈できる文字列、ローカライズ対象外の文字列は変換しない。
            if (string.IsNullOrWhiteSpace(text) ||
                decimal.TryParse(trimmedText, out _) ||
                NonLocalizableStrings.Contains(trimmedText))
                return text;

            if (LocaliseDictionary.TryGetValue(text, out var textByLanguages))
                text = textByLanguages[GetKeyLanguage()];
            else
                LocalizeFailureLog(ref text);

            return text;
        }


        /// <summary>
        /// テキストをローカライズしたテキストをフォーマットを変換してフォーマットを通す。
        /// </summary>
        /// <param name="text">テキスト</param>
        /// <param name="arg">フォーマットの引数</param>
        /// <returns>フォーマットを通した後の文字列</returns>
        public static string LocalizeTextFormat(string text, params string[] arg) {
            var format = LocalizeText(text);

            return string.Format(format, arg);
        }

        /**
         * 指定VisualElement以下のヒエラルキーの対象となる型に設定されたテキストをローカライズしたテキストに変換する。
         */
        public static VisualElement LocalizeElements(VisualElement visualElement) {
            if (visualElement == null) return visualElement;

            LocalizeTypedText<Label>(visualElement);
            LocalizeTypedText<Button>(visualElement);
            return visualElement;
        }

        /**
         * 指定VisualElement以下のヒエラルキーの指定の型に設定されたテキストをローカライズしたテキストに変換する。
         */
        private static VisualElement LocalizeTypedText<T>(VisualElement visualElement) where T : VisualElement {
            foreach (var typedVisualElement in visualElement.Query<T>().ToList()) Localize(typedVisualElement);

            return visualElement;
        }

        /**
         * VisualElementの指定の型に設定されたテキストをローカライズしたテキストに変換する。
         */
        private static VisualElement Localize(VisualElement visualElement) {
            switch (visualElement)
            {
                case Label label:
                    label.text = LocalizeText(label.text);
                    break;

                case Button button:
                    button.text = LocalizeText(button.text);
                    break;

                default:
#if DEBUG
#endif
                    break;
            }

            return visualElement;
        }

        /**
         * ローカライズ翻訳値リストを取得。
         */
        private static List<string> GetLocaliseValues(string key) {
            return new List<string>
                {
                    LocaliseDictionary[key][SystemLanguage.Japanese],
                    LocaliseDictionary[key][SystemLanguage.English],
                    LocaliseDictionary[key][SystemLanguage.Chinese]
                };
        }

        /**
         * キー重複時のキーに追加する接尾辞を取得。
         */
        private static string DuplicateKeySuffix(int count) {
            return $" ({count})";
        }


        /**
         * 初期化時のキー重複ログ出力。
         */
        [Conditional("DEBUG")]
        private static void InitalizeDuplicateKeyLog(string key, string newKey, List<string> values) {
            var valuesString = string.Join(", ", values.Select(v => $"\"{v}\""));
        }

        /**
         * ローカライズ失敗ログ出力。
         */
        [Conditional("DEBUG")]
        private static void LocalizeFailureLog(ref string text) {
//対応時に開ける
#if false
            if (DuplicateKeyCount.ContainsKey(text) && DuplicateKeyCount[text] >= 2)
            {
                string keyBase = text;
                var duplicateKeyLocalizes =
                    string.Join(
                        "\n",
                        Enumerable.Range(1, DuplicateKeyCount[text]).
                            Select(c => keyBase + DuplicateKeySuffix(c)).
                            Select(key => $"    \"{key}\" → {{{string.Join(", ", GetLocaliseValues(key).Select(v => $"\"{v}\""))}}}"));
                
#if IF_CONVET_TO_JAPANESE_FAILS_DISPLAY_ORIGINAL
                if (GetKeyLanguage() != SystemLanguage.Japanese)
#endif
                {
                    text = $"{text} (翻訳キー重複)";
                }
            }
            else
            {

#if IF_CONVET_TO_JAPANESE_FAILS_DISPLAY_ORIGINAL
                if (GetKeyLanguage() != SystemLanguage.Japanese)
#endif
                {
                    text = $"{text} (翻訳キーなし)";
                }
            }
#endif
        }

        /**
         * キーで使用する言語を取得。
         */
        private static SystemLanguage GetKeyLanguage() {
#if DEBUG
            //if (ForceSystemLanguage != SystemLanguage.Unknown)
            //{
            //    return ForceSystemLanguage;
            //}
#endif

            var language =
#if GET_LANGUAGE_FROM_PRIVATE_CLASS
                GetCurrentEditorLanguage();
#else
                GetOsLanguage();
#endif

            switch (language)
            {
                case SystemLanguage.Japanese:
                    return SystemLanguage.Japanese;

                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                case SystemLanguage.ChineseTraditional:
                    return SystemLanguage.Chinese;

                default:
                    return SystemLanguage.English;
            }
        }

        /**
         * オペレーティングシステムの設定言語を取得。
         */
        private static SystemLanguage GetOsLanguage() {

            return Application.systemLanguage;
        }

        public static SystemLanguage GetNowLanguage() {
            var language =
#if GET_LANGUAGE_FROM_PRIVATE_CLASS
                GetCurrentEditorLanguage();
#else
                GetOsLanguage();
#endif

            switch (language)
            {
                case SystemLanguage.Japanese:
                    return SystemLanguage.Japanese;

                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                case SystemLanguage.ChineseTraditional:
                    return SystemLanguage.Chinese;

                default:
                    return SystemLanguage.English;
            }
        }

#if GET_LANGUAGE_FROM_PRIVATE_CLASS
        /**
         * Unityエディターの設定言語を取得。
         * 
         * 非公開クラス UnityEditor.LocalizationDatabase のプロパティ currentEditorLanguage の値を取得する。
         */
        private static SystemLanguage GetCurrentEditorLanguage() {
            var assembly = typeof(EditorWindow).Assembly;
            var localizationDatabaseType = assembly.GetType("UnityEditor.LocalizationDatabase");
            var currentEditorLanguageProperty = localizationDatabaseType.GetProperty("currentEditorLanguage");
            return (SystemLanguage) currentEditorLanguageProperty.GetValue(null);
        }
#endif
    }
}
