using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

/// <summary>
/// C#ユーティリティークラス。
/// </summary>

namespace RPGMaker.Codebase.CoreSystem.Helper
{
    public static class CSharpUtil
    {
        /// <summary>
        /// 最大値、最小値を考慮した加算。
        /// </summary>
        /// <param name="lhs">被加算値</param>
        /// <param name="rhs">加算値</param>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <returns>結果値</returns>
        public static int AddValue(int lhs, int rhs, int min = int.MinValue, int max = int.MaxValue) {
            return (int) Clamp((long) lhs + rhs, min, max);
        }

        /// <summary>
        /// 最大値、最小値を考慮した減算。
        /// </summary>
        /// <param name="lhs">被減算値</param>
        /// <param name="rhs">減算値</param>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <returns>結果値</returns>
        public static int SubValue(int lhs, int rhs, int min = int.MinValue, int max = int.MaxValue) {
            return (int) Clamp((long) lhs - rhs, min, max);
        }

        /// <summary>
        /// 最大値、最小値を考慮した乗算。
        /// </summary>
        /// <param name="lhs">被乗算値</param>
        /// <param name="rhs">乗算値</param>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <returns>結果値</returns>
        public static int MulValue(int lhs, int rhs, int min = int.MinValue, int max = int.MaxValue) {
            return (int) Clamp((long) lhs * rhs, min, max);
        }

        /// <summary>
        /// 最大値、最小値、オーバーフローを考慮した加算。
        /// </summary>
        /// <param name="lhs">被加算値</param>
        /// <param name="rhs">加算値</param>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <returns>結果値</returns>
        public static long AddValue(long lhs, long rhs, long min = long.MinValue, long max = long.MaxValue) {
            DebugUtil.Assert(lhs >= min && lhs <= max && lhs != long.MinValue);
            return rhs < 0 ?
                SubValue(lhs, -rhs, min, max) :
                rhs < max - lhs ? lhs + rhs : max;
        }

        /// <summary>
        /// 最大値、最小値、オーバーフローを考慮した減算。
        /// </summary>
        /// <param name="lhs">被減算値</param>
        /// <param name="rhs">減算値</param>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <returns>結果値</returns>
        public static long SubValue(long lhs, long rhs, long min = long.MinValue, long max = long.MaxValue) {
            DebugUtil.Assert(lhs >= min && lhs <= max && lhs != long.MinValue);
            return rhs < 0 ?
                AddValue(lhs, -rhs, min, max) :
                rhs < lhs - min ? lhs - rhs : min;
        }

        /// <summary>
        /// foreachでインデックスも使用できるようにする拡張メソッド。
        /// </summary>
        /// <typeparam name="T">foreachで反復する型</typeparam>
        /// <param name="items">foreachで反復するオブジェクト</param>
        /// <returns>インデックス付き反復処理列挙子</returns>
        public static IEnumerable<(T item, int index)> Indexed<T>(this IEnumerable<T> items) {
            return Implement();

            IEnumerable<(T item, int index)> Implement() {
                var index = 0;
                foreach (var item in items)
                {
                    yield return (item, index++);
                }
            }
        }

        /// <summary>
        /// 指定型のコレクションの最初に条件が一致した位置のインデックス番号を返す拡張メソッド。
        /// </summary>
        /// <typeparam name="T">指定型</typeparam>
        /// <param name="collection">指定型のコレクション</param>
        /// <param name="match">条件</param>
        /// <returns>0から始まるインデックス。一致項目が見つからなかった場合は-1。</returns>
        /// <remarks>String、Array、List<T>などにあるIndexOfの汎用版。</remarks>
        public static int GenericIndexOf<T>(this IEnumerable<T> collection, Predicate<T> match) {
            return collection.
                Select((value, index) => new { value, index }).
                Where(vi => match(vi.value)).
                Select(vi => vi.index).
                DefaultIfEmpty(-1).
                FirstOrDefault();
        }

        /// <summary>
        /// コレクションのインデックスを範囲内に収まる値に変更します。
        /// </summary>
        /// <typeparam name="T">コレクションの型</typeparam>
        /// <param name="item">コレクション</param>
        /// <param name="index">インデックス</param>
        /// <returns>結果インデックス値</returns>
        /// <remarks>要素数が0の場合は-1を返します</remarks>
        public static int ClampIndex<T>(IEnumerable<T> item, int index) {
            return item.Count() == 0 ? -1 : Clamp(index, 0, item.Count() - 1);
        }

        /// <summary>
        /// 値を指定範囲内の値にして返します。
        /// </summary>
        /// <typeparam name="T">値の型</typeparam>
        /// <param name="value">元値</param>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <returns>結果値</returns>
        public static T Clamp<T>(T value, T min, T max) where T : IComparable<T> {
            return
                value.CompareTo(min) < 0 ? min :
                value.CompareTo(max) > 0 ? max :
                value;
        }

        /// <summary>
        /// テキストの表示幅をUNICODの文字コードから簡易的に取得。
        /// </summary>
        /// <param name="text">テキスト</param>
        /// <returns>半角文字換算の文字数</returns>
        public static int GetTextUnicodeHalfwidthCount(string text) {
            return text.Sum(c => c >= '\u0000' && c <= '\u0080' || c >= '\uFF61' && c <= '\uFF9F' ? 1 : 2);
        }

        /// <summary>
        /// 文字列から列挙型の値を取得する。
        /// </summary>
        /// <typeparam name="TEnum">列挙型の型</typeparam>
        /// <param name="inString">変換元の文字列</param>
        /// <param name="outEnumValue">変換先の列挙型の値</param>
        /// <returns>成功フラグ</returns>
        public static bool TryParse<TEnum>(string inString, out TEnum outEnumValue) where TEnum : struct {
            return Enum.TryParse(inString, out outEnumValue) && Enum.IsDefined(typeof(TEnum), outEnumValue);
        }

        /// <summary>
        /// テキストを分割する。
        /// </summary>
        /// <param name="text">分割対象テキスト。</param>
        /// <param name="splitLength">分割する長さ。</param>
        /// <param name="demiliterString">デミリタ文字列。</param>
        /// <param name="isMuchPossibleNewLine">できるだけ改行位置で分割する。</param>
        /// <returns>分割後テキスト列。</returns>
        /// <remarks>
        /// まずデミリタ文字列で分割し、分割後の各テキストを長さで分割する。
        /// </remarks>
        public static List<string> SplitText(
            string text, int splitLength, string demiliterString, bool isMuchPossibleNewLine) {
            List<string> demiliterSplitedText = null;
            if (demiliterString != null)
            {
                // デミリタで分割 (デミリタ自身は分割した文字列の末尾に含める)。
                demiliterSplitedText =
                    Regex.Split(text, demiliterString).Select(s => s + demiliterString).ToList();
                demiliterSplitedText[demiliterSplitedText.Count - 1] =
                    demiliterSplitedText[demiliterSplitedText.Count - 1][0..^demiliterString.Length];
            }
            else
            {
                demiliterSplitedText = new List<string>() { text };
            }

            // 更に長さで分割。
            List<string> texts = new List<string>();
            foreach (var splitedText in demiliterSplitedText)
            {
                texts.AddRange(SplitText(splitedText, splitLength, isMuchPossibleNewLine));
            }

            return texts;
        }

        /// <summary>
        /// テキストを指定の長さで分割する。
        /// </summary>
        /// <param name="text">分割対象テキスト。</param>
        /// <param name="splitLength">分割する長さ。</param>
        /// <param name="isMuchPossibleNewLine">できるだけ改行位置で分割する。</param>
        /// <returns>分割後テキスト列。</returns>
        public static IEnumerable<string> SplitText(string text, int splitLength, bool isMuchPossibleNewLine) {
            if (string.IsNullOrEmpty(text))
            {
                yield return text;
            }
            else
            {
                for (var index = 0; index < text.Length;)
                {
                    //　文字数で分割する
                    var length = Math.Min(splitLength, text.Length - index);
                    if (isMuchPossibleNewLine)
                    {
                        //文字数の上限を超えていたら改行判定を行う
                        if (length > splitLength)
                        {
                            // 分割候補文字列中に改行があれば、できるだけ末尾に近い改行位置を分割位置とする。
                            var demiliterIndex = text.LastIndexOf('\n', index + length - 1, length);
                            if (demiliterIndex > 0)
                            {
                                length = demiliterIndex - index + 1;
                            }
                        }
                    }
                    yield return text.Substring(index, length);

                    index += length;
                }
            }
        }

        /// <summary>
        /// テキストを指定の長さで分割する。
        /// </summary>
        /// <param name="text">分割対象テキスト。</param>
        /// <param name="splitLength">分割する長さ。</param>
        /// <returns>分割後テキスト列。</returns>
        public static IEnumerable<string> SplitText(string text, int splitLength) {
            return
                Enumerable.Range(0, (text.Length + splitLength - 1) / splitLength).
                    Select(index =>
                        text.Substring(
                            index * splitLength,
                            Math.Min(splitLength, text.Length - index * splitLength)));
        }

        /// <summary>
        /// objectが文字列または文字なら引用符で囲む。
        /// </summary>
        /// <param name="o">オブジェクト。</param>
        /// <returns>結果のオブジェクト。</returns>
        public static object StringModifier(object o) {
            return o is string ? $"\"{o}\"" : o is char ? $"'{o}'" : o;
        }

        /// <summary>
        /// 入力シーケンスから条件を満たした1の要素もしくは既定値を返します。
        /// </summary>
        /// <typeparam name="T">要素の型</typeparam>
        /// <param name="source">入力シーケンス</param>
        /// <param name="filter">条件</param>
        /// <returns>条件を満たした要素</returns>
        /// <remarks>
        /// 複数の要素が見つかった場合は、最初の要素を返します。
        /// 更にDebugUtilが有効だった場合は、警告メッセージを表示します。
        /// 要素が見つからない場合は、既定値を返します。
        /// </remarks>
        public static T ForceSingleOrDefault<T>(this IEnumerable<T> source, Func<T, bool> filter) where T : class {
            var e = source.Where(filter);
            DebugUtil.LogWarningIf(e.Count() > 1,
                $"{MethodBase.GetCurrentMethod().Name} : {e.Count()}個の要素が見つかりましたが、最初の要素を返します。");
            return e.Count() > 0 ? e.ElementAt(0) : default;
        }

        /// <summary>
        /// 入力シーケンスから条件を満たした1の要素もしくは既定値を返します。
        /// </summary>
        /// <typeparam name="T">要素の型</typeparam>
        /// <param name="source">入力シーケンス</param>
        /// <param name="filter">条件</param>
        /// <returns>条件を満たした要素</returns>
        /// <remarks>
        /// 複数の要素が見つかった場合は、最初の要素を返します。
        /// 更にDebugUtilが有効だった場合は、警告メッセージを表示します。
        /// 要素が見つからない場合は、既定値を返します。
        /// 更にDebugUtilが有効だった場合は、警告メッセージを表示します。
        /// </remarks>
        public static T ForceSingle<T>(this IEnumerable<T> source, Func<T, bool> filter) where T : class {
            var e = source.Where(filter);
            DebugUtil.LogWarningIf(e.Count() > 1,
                $"{MethodBase.GetCurrentMethod().Name} : {e.Count()}個の要素が見つかりましたが、最初の要素を返します。");
            DebugUtil.LogWarningIf(e.Count() == 0, $"{MethodBase.GetCurrentMethod().Name} : 要素が見つかりませんでしたので、既定値を返します。");
            return e.Count() > 0 ? e.ElementAt(0) : default;
        }

        /// <summary>
        /// DataModelをクローンする
        /// </summary>
        public static T DataClone<T>(this T src) where T : class {
            //クローン不可ならnullを返す
            if (!HasAttribute<T, SerializableAttribute>())
            {
                return null;
            }

            using var ms = new MemoryStream();
            var bf = new BinaryFormatter();
            bf.Serialize(ms, src);
            ms.Position = 0;
            return (T) bf.Deserialize(ms);
        }

        /// <summary>
        /// クラスに指定の属性が存在するかをチェックする。
        /// </summary>
        public static bool HasAttribute<T, Attr>()
            where T : class
            where Attr : Attribute {
            //Serializable属性が付いていなければfalseを返す
            return Attribute.GetCustomAttribute(typeof(T), typeof(Attr)) != null;
        }

        /// <summary>
        /// Actionのターゲットのクラス名.メソッド名を取得する。
        /// </summary>
        /// <param name="action">Action</param>
        /// <returns>クラス名.メソッド名。</returns>
        public static string GetTargetClassMethodName(this Action action) {
            string className = action.Method.DeclaringType.Name;
            if (action.Target != null)
            {
                className += $"({action.Target.GetHashCode():X})";
            }

            return className + ":" + action.Method.ToString();
        }
    }
}
