// シンボルDEBUG_UTILを定義するとDebugUtilクラスの各メソッドが有効になります。
// 未定義の場合、Conditional属性の仕組みにより【呼び出し元】のコードが削除されます。
// 呼び出し元の DebugUtil.～() は埋め込んだままでも通常は問題とならない仕組みとなっています。
#if false
#define DEBUG_UTIL
#endif

using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

#if DEBUG_UTIL
#if UNITY_EDITOR
using UnityEditor;
using Unity.EditorCoroutines.Editor;
#endif
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#endif

// 以下は名前空間を明確にするために意図的にusingしません。
// UnityEngine.Debug
// System.Diagnostics

namespace RPGMaker.Codebase.CoreSystem.Helper
{
    /// <summary>
    /// デバッグユーティリティー クラス。
    /// </summary>
    public static class DebugUtil
    {
#if DEBUG_UTIL
        /// <summary>
        /// デバッグ用ログを出力をするフラグ。
        /// </summary>
        private static bool isLogOutput = true;

        /// <summary>
        /// デバッグ用ログをファイルに出力をするフラグ。
        /// </summary>
        private static bool isLogFileOutput = false;

        /// <summary>
        /// 古いログファイルをリネームして残すフラグ。
        /// </summary>
        private static bool isOldLogFileRename =
#if UNITY_EDITOR
            true;
#else
            false;
#endif

        /// <summary>
        /// ログファイルパス。
        /// </summary>
        public static string LogFilePath
        {
            get
            {
                return Path.Combine(GetFileDefaultDirectory(), nameof(DebugUtil) + "_Log.txt");
            }
        }

#endif

        /// <summary>
        /// リポジトリセーブ時にロード済みかチェックする。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o">ロード済みのはずのオブジェクト</param>
        /// <param name="filePath">対象ファイルパス</param>
#if !DEBUG_UTIL
        [Conditional("____DEBUG__UTIL__NEVER__DEFINED__SYMBOL__NAME____")]
#endif
        public static void CheckRepositorySave<T>(T o, string filePath)
        {
#if DEBUG_UTIL
            if (o == null)
            {
                LogWarning($"ロードせずにセーブしようとしました！セーブするべきものがありません。\"{filePath}\"");
            }
#endif
        }

        /// <summary>
        /// デバッグ用ログを出力をするフラグを設定する。
        /// </summary>
        /// <param name="enable">許可フラグ</param>
#if !DEBUG_UTIL
        [Conditional("____DEBUG__UTIL__NEVER__DEFINED__SYMBOL__NAME____")]
#endif
        public static void EnableLogOutput(bool enable)
        {
#if DEBUG_UTIL
            isLogOutput = enable;
#endif
        }

        /// <summary>
        /// デバッグ用ログをファイルに出力をするフラグを設定する。
        /// </summary>
        /// <param name="enable">許可フラグ</param>
        /// <remarks>
        /// 呼び元のスクリプトでシンボル DEBUG_UTIL_TEST_LOG 定義時のみ呼ばれる。
        /// </remarks>
        [Conditional("DEBUG_UTIL_TEST_LOG")]
#if !DEBUG_UTIL
        [Conditional("____DEBUG__UTIL__NEVER__DEFINED__SYMBOL__NAME____")]
#endif
        public static void TestEnableLogFileOutput(bool enable)
        {
#if DEBUG_UTIL
            EnableLogFileOutput(enable);
#endif
        }

        /// <summary>
        /// デバッグ用ログをファイルに出力をするフラグを設定する。
        /// </summary>
        /// <param name="enable">許可フラグ</param>
#if !DEBUG_UTIL
        [Conditional("____DEBUG__UTIL__NEVER__DEFINED__SYMBOL__NAME____")]
#endif
        public static void EnableLogFileOutput(bool enable)
        {
#if DEBUG_UTIL
            isLogFileOutput = enable;
#endif
        }

        /// <summary>
        /// 古いログファイルをリネームして残すフラグを設定する。
        /// </summary>
        /// <param name="enable">許可フラグ</param>
#if !DEBUG_UTIL
        [Conditional("____DEBUG__UTIL__NEVER__DEFINED__SYMBOL__NAME____")]
#endif
        public static void EnableOldLogFileRename(bool enable)
        {
#if DEBUG_UTIL
            isOldLogFileRename = enable;
#endif
        }

        /// <summary>
        /// 条件を断定し、失敗するとエラーメッセージをUnityコンソールに表示する。
        /// </summary>
        /// <param name="condition">真であると期待する条件</param>
        /// <param name="sourceLineNumber">呼び出し元ソースファイルの行番号</param>
        /// <param name="sourceFilePath">呼び出し元のソースファイルのフルパス</param>
        /// <param name="memberName">呼び出し元のメソッド名またはプロパティ名</param>
#if !DEBUG_UTIL
        [Conditional("____DEBUG__UTIL__NEVER__DEFINED__SYMBOL__NAME____")]
#endif
        public static void Assert(
            bool condition,
            [CallerLineNumber] int sourceLineNumber = 0,
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string memberName = "")
        {
#if DEBUG_UTIL
            if (!condition)
            {
                // ブレークポイント設定用のif文。次の"}"の行に設定して、条件不成立の時に停止させる。
            }

            UnityEngine.Debug.Assert(condition);
#endif
        }

        /// <summary>
        /// 条件を断定し、失敗するとエラーメッセージをUnityコンソールに表示する。
        /// </summary>
        /// <param name="condition">真であると期待する条件</param>
        /// <param name="message">表示の際に文字列として変換対象となる文字列やオブジェクト</param>
        /// <param name="sourceLineNumber">呼び出し元ソースファイルの行番号</param>
        /// <param name="sourceFilePath">呼び出し元のソースファイルのフルパス</param>
        /// <param name="memberName">呼び出し元のメソッド名またはプロパティ名</param>
#if !DEBUG_UTIL
        [Conditional("____DEBUG__UTIL__NEVER__DEFINED__SYMBOL__NAME____")]
#endif
        public static void Assert(
            bool condition,
            object message,
            [CallerLineNumber] int sourceLineNumber = 0,
            [CallerFilePath] string sourceFilePath = "",
            [CallerMemberName] string memberName = "")
        {
#if DEBUG_UTIL
            if (!condition && isLogFileOutput)
            {
                LogFlie.Append("[Assert] " + message);
            }

            if (!condition)
            {
                // ブレークポイント設定用のif文。次の"}"の行に設定して、条件不成立の時に停止させる。
            }

            UnityEngine.Debug.Assert(condition, message);
#endif
        }

        /// <summary>
        /// 今フレームの最後に一時停止する。
        /// </summary>
#if !DEBUG_UTIL
        [Conditional("____DEBUG__UTIL__NEVER__DEFINED__SYMBOL__NAME____")]
#endif
        public static void Break()
        {
#if DEBUG_UTIL
            UnityEngine.Debug.Break();
#endif
        }

        /// <summary>
        /// ブレークポイント設定用メソッド。
        /// </summary>
        /// <remarks>
        /// 多くのブレークポイントを設定するとUnityやVisual Studioの動作が非常に遅くなる場合があるので、
        /// 本メソッドを用意。
        /// </remarks>
#if !DEBUG_UTIL
        [Conditional("____DEBUG__UTIL__NEVER__DEFINED__SYMBOL__NAME____")]
#endif
        public static void BreakPoint()
        {
#if DEBUG_UTIL
            Log("BreakPoint");
#endif
        }

        /// <summary>
        /// オブジェクトを文字列化してUnityコンソールに条件付き表示する。
        /// </summary>
        /// <param name="condition">表示する条件</param>
        /// <param name="message">文字列化して表示するオブジェクト</param>
#if !DEBUG_UTIL
        [Conditional("____DEBUG__UTIL__NEVER__DEFINED__SYMBOL__NAME____")]
#endif
        public static void LogIf(bool condition, object message)
        {
#if DEBUG_UTIL
            if (condition)
            {
                Log(message);
            }
#endif
        }

        /// <summary>
        /// オブジェクトを文字列化して警告メッセージをUnityコンソールに条件付き表示する。
        /// </summary>
        /// <param name="condition">表示する条件</param>
        /// <param name="message">文字列化して表示するオブジェクト</param>
#if !DEBUG_UTIL
        [Conditional("____DEBUG__UTIL__NEVER__DEFINED__SYMBOL__NAME____")]
#endif
        public static void LogWarningIf(bool condition, object message)
        {
#if DEBUG_UTIL
            if (condition)
            {
                LogWarning(message);
            }
#endif
        }

        /// <summary>
        /// メソッドと引数列をUnityコンソールに表示する。
        /// </summary>
        /// <param name="arguments">引数列</param>
#if !DEBUG_UTIL
        [Conditional("____DEBUG__UTIL__NEVER__DEFINED__SYMBOL__NAME____")]
#endif
        public static void LogMethod(params object[] arguments)
        {
#if DEBUG_UTIL
            Log(MethodToString(new System.Diagnostics.StackFrame(1, false).GetMethod(), arguments));
#endif
        }

        /// <summary>
        /// メソッドと引数列をUnityコンソールに表示する。
        /// </summary>
        /// <param name="paramerterValues">引数値列</param>
#if !DEBUG_UTIL
        [Conditional("____DEBUG__UTIL__NEVER__DEFINED__SYMBOL__NAME____")]
#endif
        public static void LogMethodParameters(params object[] parameterValues)
        {
#if DEBUG_UTIL
            var methodBase = new System.Diagnostics.StackFrame(1, false).GetMethod();
            var parameters = string.Join(
                ", ",
                methodBase.
                    GetParameters().
                    Zip(parameterValues, (p, v) => (p,v)).
                    Select(zip => $"{zip.p.Name}={CSharpUtil.StringModifier(zip.v)}"));
            Log($"{methodBase.DeclaringType.Name}.{methodBase.Name}({parameters})");
#endif
        }

#if DEBUG_UTIL
        /// <summary>
        /// メソッドと引数列を文字列化する。
        /// </summary>
        /// <param name="methodBase">メソッド情報</param>
        /// <param name="arguments">引数列</param>
        /// <returns>文字列</returns>
        private static string MethodToString(MethodBase methodBase, params object[] arguments)
        {
            arguments = arguments.Select(a => a is string ? $"\"{a}\"" : a).ToArray();
            return $"{methodBase.DeclaringType.Name}.{methodBase.Name}() {string.Join(", ", arguments)}";
        }
#endif

        /// <summary>
        /// オブジェクトを文字列化してUnityコンソールに表示する。
        /// </summary>
        /// <param name="message">文字列化して表示するオブジェクト</param>
        /// <param name="forceIndent">強制インデント値</param>
        /// <remarks>
        /// 呼び元のスクリプトでシンボル DEBUG_UTIL_TEST_LOG 定義時のみ呼ばれる。
        /// </remarks>
        [Conditional("DEBUG_UTIL_TEST_LOG")]
#if !DEBUG_UTIL
        [Conditional("____DEBUG__UTIL__NEVER__DEFINED__SYMBOL__NAME____")]
#endif
        public static void TestLog(object message, int forceIndent = -1)
        {
#if DEBUG_UTIL
            Log(message, forceIndent);
#endif
        }

        /// <summary>
        /// オブジェクトを文字列化してUnityコンソールに表示する。
        /// </summary>
        /// <param name="message">文字列化して表示するオブジェクト</param>
        /// <param name="forceIndent">強制インデント値</param>
#if !DEBUG_UTIL
        [Conditional("____DEBUG__UTIL__NEVER__DEFINED__SYMBOL__NAME____")]
#endif
        public static void Log(object message, int forceIndent = -1)
        {
#if DEBUG_UTIL
            var match = message as System.Text.RegularExpressions.Match;
            if (match != null)
            {
                System.Text.StringBuilder sb = new();
                sb.AppendLine($"Match.Success = \"{match.Success}\"");
                if (match.Success)
                {
                    sb.AppendLine($"  Match.Value = \"{match.Value}\"");
                    foreach (var (group, groupIndex) in match.Groups.Indexed())
                    {
                        sb.AppendLine($"  Group[{groupIndex}] : Index = {group.Index}, Name = \"{group.Name}\", Value = \"{group.Value}\"");
                        foreach (var (capture, captureIndex) in group.Captures.Indexed())
                        {
                            sb.AppendLine($"    Capture[{captureIndex}] : Index = {capture.Index}, Value = \"{capture.Value}\"");
                        }
                    }
                }

                message = sb.ToString();
            }

            if (forceIndent < 0)
            {
                IndentLog.StockLog();
            }

            if (isLogOutput)
            {
                UnityEngine.Debug.Log(message);
            }

            if (isLogFileOutput)
            {
                LogFlie.Append(message, forceIndent);
            }
#endif
        }

        /// <summary>
        /// オブジェクトを文字列化して警告メッセージをUnityコンソールに表示する。
        /// </summary>
        /// <param name="message">文字列化して表示するオブジェクト</param>
        /// <param name="forceIndent">強制インデント値</param>
#if !DEBUG_UTIL
        [Conditional("____DEBUG__UTIL__NEVER__DEFINED__SYMBOL__NAME____")]
#endif
        public static void LogWarning(object message, int forceIndent = -1)
        {
#if DEBUG_UTIL
            if (forceIndent < 0)
            {
                IndentLog.StockLog();
            }

            if (isLogOutput)
            {
                UnityEngine.Debug.LogWarning(message);
            }

            if (isLogFileOutput)
            {
                LogFlie.Append("[Warning] " + message);
            }
#endif
        }

        /// <summary>
        /// オブジェクトを文字列化してエラーメッセージをUnityコンソールに表示する。
        /// </summary>
        /// <param name="message">文字列化して表示するオブジェクト</param>
        /// <param name="forceIndent">強制インデント値</param>
#if !DEBUG_UTIL
        [Conditional("____DEBUG__UTIL__NEVER__DEFINED__SYMBOL__NAME____")]
#endif
        public static void LogError(object message, int forceIndent = -1)
        {
#if DEBUG_UTIL
            if (forceIndent < 0)
            {
                IndentLog.StockLog();
            }

            if (isLogOutput)
            {
                UnityEngine.Debug.LogError(message);
            }

            if (isLogFileOutput)
            {
                LogFlie.Append("[Error] " + message);
            }
#endif
        }

        /// <summary>
        /// ログ出力のインデント指定をするクラス。
        /// </summary>
        public class IndentLog : System.IDisposable
        {
#if DEBUG_UTIL
            private static readonly LinkedList<IndentLog> stockIndentLogs = new();

            private string stockMessage;
            private readonly int stockIndent;
#endif

            /// <summary>
            /// コンストラクタ。
            /// </summary>
            /// <param name="stockMessage">インデント内で他のログ表示がある場合、事前に表示するメッセージ</param>
            public IndentLog(string stockMessage = null)
            {
#if DEBUG_UTIL
                this.stockMessage = stockMessage;
                stockIndent = LogFlie.AddIndent(+1);
                stockIndentLogs.AddLast(this);
#endif
            }

            /// <summary>
            /// メソッド表示用コンストラクタ。
            /// </summary>
            /// <param name="methodBase">メソッド情報</param>
            /// <param name="arguments">引数列</param>
            public IndentLog(MethodBase methodBase, params object[] arguments)
            {
#if DEBUG_UTIL
                stockMessage = MethodToString(methodBase, arguments);
                stockIndent = LogFlie.AddIndent(+1);
                stockIndentLogs.AddLast(this);
#endif
            }

#if DEBUG_UTIL
            /// <summary>
            /// ストックされたログを表示する。
            /// </summary>
            public static void StockLog()
            {
                foreach (IndentLog stockIndentLog in stockIndentLogs)
                {
                    if (stockIndentLog.stockMessage != null)
                    {
                        Log(stockIndentLog.stockMessage, stockIndentLog.stockIndent);
                        stockIndentLog.stockMessage = null;
                    }
                }
            }
#endif

            /// <summary>
            /// 廃棄。
            /// </summary>
            public void Dispose()
            {
#if DEBUG_UTIL
                LogFlie.AddIndent(-1);
                stockIndentLogs.Remove(this);
#endif
            }
        }

#if DEBUG_UTIL
        /// <summary>
        /// デバッグ用ログをファイルに出力をするクラス。
        /// </summary>
        private static class LogFlie
        {
            private const int IndentWidth = 4;

            private static string filePath;
            private static int indent = 0;

            /// <summary>
            /// ログをファイルに追加。
            /// </summary>
            /// <param name="message">ログメッセージ</param>
            /// <param name="forceIndent">強制インデント値</param>
            public static void Append(object message, int forceIndent = -1)
            {
                if (filePath == null)
                {
                    filePath = LogFilePath;

                    if (isOldLogFileRename)
                    {
                        RenameFileByLastWriteTime(filePath);
                    }

                    DeleteFile(filePath);
                }
               
                message = string.Concat(
                    $"[{System.DateTime.Now:HH:mm:ss.fff} ({Time.frameCount, 5})] ",
                    new string(' ', (forceIndent >= 0 ? forceIndent : indent) * IndentWidth),
                    message);

                AppendTextFile(filePath, message.ToString() + System.Environment.NewLine);
            }

            /// <summary>
            /// インデント追加。
            /// </summary>
            /// <param name="addaValue">追加インデント値</param>
            /// <returns>結果インデント値</returns>
            public static int AddIndent(int addaValue)
            {
                int result = indent;
                indent += addaValue;
                return result;
            }
        }
#endif

        /// <summary>
        /// 実行する。
        /// </summary>
        /// <param name="action">実行する処理</param>
        /// <param name="name">実行名</param>
#if !DEBUG_UTIL
        [Conditional("____DEBUG__UTIL__NEVER__DEFINED__SYMBOL__NAME____")]
#endif
        public static void Execution(System.Action action, string name = null)
        {
#if DEBUG_UTIL
            string callerMethodName = MethodBase.GetCurrentMethod().Name;

            if (name != null)
            {
                Log($"{callerMethodName}: \"{name}\" 実行開始。");
            }

            action();

            if (name != null)
            {
                Log($"{callerMethodName}: {name} 実行終了。");
            }
#endif
        }

        /// <summary>
        /// 繰り返し実行する。
        /// </summary>
        /// <param name="action">実行する処理</param>
        /// <param name="name">繰り返し実行名</param>
        /// <param name="count">実行する回数</param>
        /// <param name="intervalSeconds">実行間隔秒</param>
#if !(DEBUG_UTIL && UNITY_EDITOR)
        [Conditional("____DEBUG__UTIL__NEVER__DEFINED__SYMBOL__NAME____")]
#endif
        public static void EditorRepeatExecution(
            System.Action action,
            string name = null,
            int count = 1,
            float intervalSeconds = 0.0f)
        {
#if DEBUG_UTIL && UNITY_EDITOR
            string callerMethodName = MethodBase.GetCurrentMethod().Name;
            bool isBreak = false;

            // StartCoroutineOwnerlessは自動でコルーチンは停止しないので、手動で停止させる必要がある。
            // できれば修正したほうが良いかも。
            EditorCoroutineUtility.StartCoroutineOwnerless(RepeatExecutionCoroutine());

            IEnumerator RepeatExecutionCoroutine()
            {
                foreach (var countIndex in Enumerable.Range(0, count))
                {
                    if (name != null)
                    {
                        Log($"{callerMethodName}: \"{name}\" 実行 {countIndex + 1}/{count} 回目。");
                    }

                    action();

                    yield return new EditorWaitForSeconds(intervalSeconds);
                    
#if UNITY_EDITOR_WIN
                    if (Win32Api.GetKeyState(Win32Api.VirtualKey.Escape) < 0)
                    {
                        break;
                    }
#endif

                    // デバッガでisBreakをtrueに変更してbreakさせる為のもの。
                    if (isBreak)
                    {
                        break;
                    }
                }

                if (name != null)
                {
                    Log($"{callerMethodName}: {name} 実行終了。");
                }
            }
#endif
        }

        /// <summary>
        /// Unityエディタ起動時にRPG Makerはウィンドウ状態で開始するか。
        /// </summary>
#if !(DEBUG_UTIL && UNITY_EDITOR)
        [Conditional("____DEBUG__UTIL__NEVER__DEFINED__SYMBOL__NAME____")]
#endif
        public static void IsUniteWindowStart(ref bool isUniteWindowStart)
        {
#if DEBUG_UTIL && UNITY_EDITOR
            if (EditorUserSettings.GetConfigValue("IsUniteWindowStart") == "true")
            {
                isUniteWindowStart = true;
            }
#endif
        }

        /// <summary>
        /// ファイルの既定ディレクトリにテクスチャをPNGファイルとしてセーブする。
        /// </summary>
#if !DEBUG_UTIL
        [Conditional("____DEBUG__UTIL__NEVER__DEFINED__SYMBOL__NAME____")]
#endif
        public static void SaveTextureToPng(UnityEngine.Texture2D texture, string fileName)
        {
#if DEBUG_UTIL
            File.WriteAllBytes(
                Path.Combine(GetFileDefaultDirectory(), fileName + ".png"),
                texture.EncodeToPNG());
#endif
        }

#if DEBUG_UTIL
        /// <summary>
        /// ファイルの既定ディレクトリを取得。
        /// </summary>
        /// <returns>既定ディレクトリ</returns>
        /// <remarks>UnityプロジェクトのAssetsフォルダの親の親のディレクトリ</remarks>
        private static string GetFileDefaultDirectory()
        {
            string path =
#if UNITY_EDITOR
                Path.Combine(UnityEngine.Application.dataPath, "..", "..", "..", "..");
#elif UNITY_STANDALONE
                UnityEngine.Application.dataPath;
#else
                UnityEngine.Application.temporaryCachePath;
#endif
            return new FileInfo(Path.Combine(path, "$DebugFiles")).FullName;
        }

        /// <summary>
        /// 古い同名のファイルをリネームして残してテキストファイルをセーブ。
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <param name="saveText">セーブテキスト</param>
        private static void SaveTextFileLeaveOldFile(string filePath, string saveText)
        {
            if (File.Exists(filePath))
            {
                string existsFileText = LoadTextFile(filePath);
                if (existsFileText == saveText)
                {
                    return;
                }

                string movePath =
                    Path.Combine(
                        Path.GetDirectoryName(filePath),
                        string.Concat(
                            Path.GetFileNameWithoutExtension(filePath),
                            File.GetLastWriteTime(filePath).ToString("_yyMMdd_HHmmss"),
                            Path.GetExtension(filePath)));

                File.Move(filePath, movePath);
            }

            SaveTextFile(filePath, saveText);
        }

        /// <summary>
        /// 指定ファイルが存在したら、削除する。
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        private static void DeleteFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            File.Delete(filePath);
        }

        /// <summary>
        /// 指定ファイルが存在したら、最終更新日時を追加したファイル名にリネームする。
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        private static void RenameFileByLastWriteTime(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            string movePath =
                Path.Combine(
                    Path.GetDirectoryName(filePath),
                    string.Concat(
                        Path.GetFileNameWithoutExtension(filePath),
                        File.GetLastWriteTime(filePath).ToString("_yyMMdd_HHmmss"),
                        Path.GetExtension(filePath)));

            File.Move(filePath, movePath);
        }

        /// <summary>
        /// テキストファイルをセーブ。
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <param name="saveText">セーブテキスト</param>
        private static void SaveTextFile(string filePath, string saveText)
        {
            CreatePathDirectory(filePath);

            // UTF8 BOMなしファイルとしてセーブ。
            File.WriteAllText(filePath, saveText);
        }

        /// <summary>
        /// テキストファイルにテキストを追加。
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <param name="appendText">追加テキスト</param>
        private static void AppendTextFile(string filePath, string appendText)
        {
            CreatePathDirectory(filePath);

            // UTF8 BOMなしとして追加。
            File.AppendAllText(filePath, appendText);
        }

        /// <summary>
        /// ファイルパスのディレクトリが存在しなければ作成する。
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        private static void CreatePathDirectory(string filePath)
        {
            string directoryName = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
        }

        /// <summary>
        /// テキストファイルをロードを試行。
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>ロードしたファイルの文字列 (ファイルが存在しなければnull)</returns>
        private static string TryLoadTextFile(string filePath)
        {
            return !File.Exists(filePath) ? null : LoadTextFile(filePath);
        }

        /// <summary>
        /// テキストファイルをロード。
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>ロードしたファイルの文字列</returns>
        private static string LoadTextFile(string filePath)
        {
            // UTF8 BOMなしファイルとしてロード。
            return File.ReadAllText(filePath);
        }

#if UNITY_EDITOR
        /*
         * uxmlファイルのロードをフックして、ロードしたファイルをログファイルに書き出し、処理を解析する。
         * 
         * 対象のプログラムを以下のように事前に置換して使用する。
         * 【元】AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
         * 【先】RPGMaker.Codebase.Editor.Common.SekikawaHelper.LoadVisualTreeAssetAtPath(
         */
        public static UnityEngine.UIElements.VisualTreeAsset LoadVisualTreeAssetAtPath(string assetPath)
        {
            var lines = new List<string> { assetPath };
            string saveFilePath = Path.Combine(GetFileDefaultDirectory(), "LoadVisualTreeAssetAtPath_log.txt");
            File.AppendAllLines(saveFilePath, lines);

            // 事前の置換の対象にならないよう、非ジェネリックメソッドを使用している。
            return (UnityEngine.UIElements.VisualTreeAsset)
                AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.UIElements.VisualTreeAsset));
        }
#endif

        /// <summary>
        /// static版の処理時間計測。
        /// </summary>
        public partial class Stopwatch
        {
#if DEBUG_UTIL
            private static readonly Dictionary<string, System.Diagnostics.Stopwatch> _stopWatch = new();
#endif

#if !DEBUG_UTIL
            [System.Diagnostics.Conditional("____DEBUG__UTIL__NEVER__DEFINED__SYMBOL__NAME____")]
#endif
            public static void Start(string stopWatchName)
            {
#if DEBUG_UTIL
                _stopWatch[stopWatchName] = new System.Diagnostics.Stopwatch();
                _stopWatch[stopWatchName].Start();
                Log($"Stopwatch \"{stopWatchName}\" Start.");
#endif
            }

#if !DEBUG_UTIL
            [System.Diagnostics.Conditional("____DEBUG__UTIL__NEVER__DEFINED__SYMBOL__NAME____")]
#endif
            public static void End(string stopWatchName)
            {
#if DEBUG_UTIL
                _stopWatch[stopWatchName].Stop();
                var elapsedString = _stopWatch[stopWatchName].Elapsed.ToString(@"mm\:ss\.fff");
                Log($"Stopwatch \"{stopWatchName}\" End. Elapsed time [{elapsedString}]");
                _stopWatch.Remove(stopWatchName);
#endif
            }
        }

        /// <summary>
        /// using用の処理時間計測。
        /// </summary>
        /// <example>
        /// using (new Stopwatch("計測名")) { 計測処理 }
        /// </example>
        public partial class Stopwatch : System.IDisposable
        {
#if DEBUG_UTIL
            private readonly string _stopWatchName;
#endif

            public Stopwatch(string stopWatchName)
            {
#if DEBUG_UTIL
                _stopWatchName = stopWatchName;
                Start(stopWatchName);
#endif
            }

            public void Dispose()
            {
#if DEBUG_UTIL
                End(_stopWatchName);
#endif
            }
        }

#endif

        /// <summary>
        /// 変化を表示するログ。
        /// </summary>
        public static class ChangeLog
        {
#if DEBUG_UTIL
            private static readonly Dictionary<string, string> logs = new();
#endif

#if !DEBUG_UTIL
            [Conditional("____DEBUG__UTIL__NEVER__DEFINED__SYMBOL__NAME____")]
#endif
            public static void Log(string key, string value)
            {
#if DEBUG_UTIL
                if (logs.TryGetValue(key, out var oldValue) && value == oldValue)
                {
                    return;
                }

                logs[key] = value;
                DebugUtil.Log($"{key}{value}");
#endif
            }
        }

#if DEBUG_UTIL && UNITY_EDITOR_WIN
        private static class Win32Api
        {
            /// <summary>
            /// WindowsAPIのインポート
            /// </summary>
            [System.Runtime.InteropServices.DllImport(
                "user32.dll",
                CharSet = System.Runtime.InteropServices.CharSet.Auto,
                CallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall)]

            /// <summary>
            /// キー押下確認。
            /// </summary>
            /// <param name="nVirtKey">仮想キーコード</param>
            /// <returns>負値で押下中</returns>
            public static extern short GetKeyState(VirtualKey nVirtKey);

            /// <summary>
            /// Enumeration for virtual keys.
            /// </summary>
            public enum VirtualKey : ushort
            {
                LeftButton = 0x01,
                RightButton = 0x02,
                Cancel = 0x03,
                MiddleButton = 0x04,
                ExtraButton1 = 0x05,
                ExtraButton2 = 0x06,
                Back = 0x08,
                Tab = 0x09,
                Clear = 0x0C,
                Return = 0x0D,
                Shift = 0x10,
                Control = 0x11,
                Menu = 0x12,
                Pause = 0x13,
                CapsLock = 0x14,
                Kana = 0x15,
                Hangeul = 0x15,
                Hangul = 0x15,
                Junja = 0x17,
                Final = 0x18,
                Hanja = 0x19,
                Kanji = 0x19,
                Escape = 0x1B,
                Convert = 0x1C,
                NonConvert = 0x1D,
                Accept = 0x1E,
                ModeChange = 0x1F,
                Space = 0x20,
                Prior = 0x21,
                Next = 0x22,
                End = 0x23,
                Home = 0x24,
                Left = 0x25,
                Up = 0x26,
                Right = 0x27,
                Down = 0x28,
                Select = 0x29,
                Print = 0x2A,
                Execute = 0x2B,
                Snapshot = 0x2C,
                Insert = 0x2D,
                Delete = 0x2E,
                Help = 0x2F,
                N0 = 0x30,
                N1 = 0x31,
                N2 = 0x32,
                N3 = 0x33,
                N4 = 0x34,
                N5 = 0x35,
                N6 = 0x36,
                N7 = 0x37,
                N8 = 0x38,
                N9 = 0x39,
                A = 0x41,
                B = 0x42,
                C = 0x43,
                D = 0x44,
                E = 0x45,
                F = 0x46,
                G = 0x47,
                H = 0x48,
                I = 0x49,
                J = 0x4A,
                K = 0x4B,
                L = 0x4C,
                M = 0x4D,
                N = 0x4E,
                O = 0x4F,
                P = 0x50,
                Q = 0x51,
                R = 0x52,
                S = 0x53,
                T = 0x54,
                U = 0x55,
                V = 0x56,
                W = 0x57,
                X = 0x58,
                Y = 0x59,
                Z = 0x5A,
                LeftWindows = 0x5B,
                RightWindows = 0x5C,
                Application = 0x5D,
                Sleep = 0x5F,
                Numpad0 = 0x60,
                Numpad1 = 0x61,
                Numpad2 = 0x62,
                Numpad3 = 0x63,
                Numpad4 = 0x64,
                Numpad5 = 0x65,
                Numpad6 = 0x66,
                Numpad7 = 0x67,
                Numpad8 = 0x68,
                Numpad9 = 0x69,
                Multiply = 0x6A,
                Add = 0x6B,
                Separator = 0x6C,
                Subtract = 0x6D,
                Decimal = 0x6E,
                Divide = 0x6F,
                F1 = 0x70,
                F2 = 0x71,
                F3 = 0x72,
                F4 = 0x73,
                F5 = 0x74,
                F6 = 0x75,
                F7 = 0x76,
                F8 = 0x77,
                F9 = 0x78,
                F10 = 0x79,
                F11 = 0x7A,
                F12 = 0x7B,
                F13 = 0x7C,
                F14 = 0x7D,
                F15 = 0x7E,
                F16 = 0x7F,
                F17 = 0x80,
                F18 = 0x81,
                F19 = 0x82,
                F20 = 0x83,
                F21 = 0x84,
                F22 = 0x85,
                F23 = 0x86,
                F24 = 0x87,
                NumLock = 0x90,
                ScrollLock = 0x91,
                NEC_Equal = 0x92,
                Fujitsu_Jisho = 0x92,
                Fujitsu_Masshou = 0x93,
                Fujitsu_Touroku = 0x94,
                Fujitsu_Loya = 0x95,
                Fujitsu_Roya = 0x96,
                LeftShift = 0xA0,
                RightShift = 0xA1,
                LeftControl = 0xA2,
                RightControl = 0xA3,
                LeftMenu = 0xA4,
                RightMenu = 0xA5,
                BrowserBack = 0xA6,
                BrowserForward = 0xA7,
                BrowserRefresh = 0xA8,
                BrowserStop = 0xA9,
                BrowserSearch = 0xAA,
                BrowserFavorites = 0xAB,
                BrowserHome = 0xAC,
                VolumeMute = 0xAD,
                VolumeDown = 0xAE,
                VolumeUp = 0xAF,
                MediaNextTrack = 0xB0,
                MediaPrevTrack = 0xB1,
                MediaStop = 0xB2,
                MediaPlayPause = 0xB3,
                LaunchMail = 0xB4,
                LaunchMediaSelect = 0xB5,
                LaunchApplication1 = 0xB6,
                LaunchApplication2 = 0xB7,
                OEM1 = 0xBA,
                OEMPlus = 0xBB,
                OEMComma = 0xBC,
                OEMMinus = 0xBD,
                OEMPeriod = 0xBE,
                OEM2 = 0xBF,
                OEM3 = 0xC0,
                OEM4 = 0xDB,
                OEM5 = 0xDC,
                OEM6 = 0xDD,
                OEM7 = 0xDE,
                OEM8 = 0xDF,
                OEMAX = 0xE1,
                OEM102 = 0xE2,
                ICOHelp = 0xE3,
                ICO00 = 0xE4,
                ProcessKey = 0xE5,
                ICOClear = 0xE6,
                Packet = 0xE7,
                OEMReset = 0xE9,
                OEMJump = 0xEA,
                OEMPA1 = 0xEB,
                OEMPA2 = 0xEC,
                OEMPA3 = 0xED,
                OEMWSCtrl = 0xEE,
                OEMCUSel = 0xEF,
                OEMATTN = 0xF0,
                OEMFinish = 0xF1,
                OEMCopy = 0xF2,
                OEMAuto = 0xF3,
                OEMENLW = 0xF4,
                OEMBackTab = 0xF5,
                ATTN = 0xF6,
                CRSel = 0xF7,
                EXSel = 0xF8,
                EREOF = 0xF9,
                Play = 0xFA,
                Zoom = 0xFB,
                Noname = 0xFC,
                PA1 = 0xFD,
                OEMClear = 0xFE
            }
        }
#endif
    }
}