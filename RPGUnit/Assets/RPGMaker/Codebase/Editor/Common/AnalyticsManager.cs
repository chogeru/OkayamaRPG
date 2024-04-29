// Googleアナリティクス4を使用する。
// (未定義の場合は、2023年6月末日で終了予定のユニバーサルアナリティクスを使用する(開発テスト用))。

#define USE_GOOGLE_ANALYTICS_4

// Googleが用意したデバッグ用ホストを使用する。
// 通信テスト用で、情報は記録されない。
// #define USE_DEBUG_HOST

// テスト用に用意したGoogleアナリティクスのアカウント、プロパティを使用する。
// 開発中はこれを使用する想定。
#define USE_TEST_ACCOUNT_PROPERTY

// #define DEBUG_UTIL_TEST_LOG

using RPGMaker.Codebase.CoreSystem.Helper;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Common
{
    public class AnalyticsManager
    {
        // イベント名。
        public enum EventName
        {
            None,

            page_view,
            project,
            action,

            test_event_name
        }

        // イベントパラメータ。
        // C#の予約後と同じになるものは定義できないので、それについては末尾に'_'を追加しているが、
        // uxmlファイルへの記述では'_'は不要。
        public enum EventParameter
        {
            None,

            title,
            ui_setting,
            word,
            option,
            character,
            vehicle,
            job,
            battle_scene,
            enemy,
            troop,
            sound,
            se,
            skill_common,
            skill_basic,
            skill_custom,
            state_basic,
            state_custom,
            equipment_weapon,
            equipment_armor,
            equipment_item,
            type_element,
            type_skill,
            type_weapon,
            type_armor,
            type_equipment,
            battle_effect,
            common_event,
            resource_character,
            resource_ballon_icon,
            resource_sv_character,
            resource_battle_effect,
            environment,
            switch_,
            variable,
            map_tile,
            map_tilegroup,
            map_edit,
            map_battle_edit,
            map_event,
            event_search,
            outline,

            initialize,
            new_,
            open,
            close,
            save,
            deploy,
            testplay,
            help,

            test_event_parameter
        }

        private static AnalyticsManager instance;

        // VisualElementのクラス名として埋め込まれたアナリティクス用タグに一致する正規表現。
        private readonly Regex analyticsTagRegex;

        private bool isInitialized;

        private readonly HashSet<(EventName, EventParameter)> maybeReplaceToOutlineEvents;

        // シングルトンなのでprivateなコンストラクタ。
        private AnalyticsManager() {
            var eventNames =
                string.Join(
                    "|",
                    ((EventName[]) System.Enum.GetValues(typeof(EventName))).Select(enumName =>
                        EventNameOrParamater.ToString(enumName)));
            var eventParameters =
                string.Join(
                    "|",
                    ((EventParameter[]) System.Enum.GetValues(typeof(EventParameter))).Select(enumParameter =>
                        EventNameOrParamater.ToString(enumParameter)));

            analyticsTagRegex = new Regex(
                $"^AnalyticsTag__(?<NAME>({eventNames}))__(?<PARAMETER>({eventParameters}))$", RegexOptions.IgnoreCase);

            maybeReplaceToOutlineEvents = new HashSet<(EventName, EventParameter)>
            {
                (EventName.page_view, EventParameter.map_edit),
                (EventName.page_view, EventParameter.map_battle_edit),
                (EventName.page_view, EventParameter.map_event)
            };
        }

        public static AnalyticsManager Instance
        {
            get { return instance ??= new AnalyticsManager(); }
        }

        // "AnalyticsTag__{イベント名}__{イベントパラメータ}"という名のクラスが設定してあるVisualElementをHierarchyの
        // 親方向に探していき、最初に見つけたものの『イベント名』と『イベントパラメータ』を送信する。
        public void PostEventFromHierarchy(VisualElement ve) {
            var (eventName, eventParameter) = RecursiveSearch(ve);
            if (eventName != EventName.None && eventParameter != EventParameter.None)
                PostEvent(eventName, eventParameter);
            else
                DebugUtil.LogWarning("[Analytics] Hierarchy上のクリック操作で、送信すべきアナリティクス情報を特定できませんでした。");

            (EventName, EventParameter) RecursiveSearch(VisualElement ve) {
                DebugUtil.TestLog(
                    "[Analytics] " +
                    $"TryPostEvent(type={ve.GetType()}, " +
                    $"name={ve.name}, " +
                    $"classes={{{string.Join(", ", ve.GetClasses())}}})");

                foreach (var className in ve.GetClasses())
                {
                    var match = analyticsTagRegex.Match(className);
                    if (match.Success)
                        if (EventNameOrParamater.TryParse(match.Groups["NAME"].Value, out EventName eventName) &&
                            EventNameOrParamater.TryParse(match.Groups["PARAMETER"].Value,
                                out EventParameter eventParameter))
                        {
                            // 差し替えの可能性のあるイベントの場合、更に親方向に検索して差し替えを試行。
                            if (maybeReplaceToOutlineEvents.Contains((eventName, eventParameter)) && ve.parent != null)
                            {
                                var eventToReplace = (EventName.page_view, EventParameter.outline);
                                if (RecursiveSearch(ve.parent) == eventToReplace) return eventToReplace;
                            }

                            return (eventName, eventParameter);
                        }
                }

                if (ve.parent != null) return RecursiveSearch(ve.parent);

                return (EventName.None, EventParameter.None);
            }
        }

        public void PostEvent(EventName eventName, EventParameter eventParameter) {
#if USE_GOOGLE_ANALYTICS_4
#if USE_DEBUG_HOST
            const string host = "https://www.google-analytics.com/debug/mp/collect";
#else
            const string host = "https://www.google-analytics.com/mp/collect";
#endif
#else
    #if USE_DEBUG_HOST
            const string host = "https://www.google-analytics.com/debug/collect";
    #else
            const string host = "https://www.google-analytics.com/collect";
    #endif
#endif

#if USE_GOOGLE_ANALYTICS_4
#if USE_TEST_ACCOUNT_PROPERTY
            // 開発中に使用する送信先。
            //      アナリティクス アカウント    ARIT_UNITE
            //      プロパティ                   UNITE_TEST

            // 測定 ID。
            const string measurement_id = "G-BJ3YTQGYBT";

            // API シークレット。
            const string api_secret = "HCYUysvsQLedckmXH7dotg";
#else
            // リリース用の一般ユーザーが使用する送信先。
            //      アナリティクス アカウント    ？
            //      プロパティ                   ？

            // 測定 ID。
            const string measurement_id = 要設定！;

            // API シークレット。
            const string api_secret = 要設定！;
#endif
#else
            // プロトコルのバージョンです。この値は 1 にする必要があります。
            const string protocolVersion = "1";

            // データの送り先の Google アナリティクス プロパティを識別するための ID です。
            const string trackId = "UA-228688887-1";

            // 個々のユーザーについて収集された操作の種類です。RPG Maker Unite では event 固定です。
            const string hitType = "event";
#endif

            // 個々のユーザーに固有の ID です。RPG Maker Unite では 1 固定です。
            const string clientId = "1";

            // 初期化プログラムから呼ばれる場合があるので、
            // EventParameter.initialize前のEventParameter.initialize以外は無視。
            if (eventParameter == EventParameter.initialize)
                isInitialized = true;
            else if (!isInitialized) return;

            EditorCoroutineUtility.StartCoroutine(PostEventCoroutine(eventName, eventParameter), this);

            IEnumerator PostEventCoroutine(EventName eventName, EventParameter eventParameter) {
                DebugUtil.TestEnableLogFileOutput(true);

                var parameters = new List<string>
                {
#if USE_GOOGLE_ANALYTICS_4
                    $"measurement_id={measurement_id}",
                    $"api_secret={api_secret}",
#else
                    $"v={protocolVersion}",
                    $"tid={trackId}",
                    $"cid={clientId}",
                    $"t={hitType}",
                    $"ec={eventName}",
                    $"ea={eventParameter}",
#endif
                };

                var uri = $"{host}?{string.Join("&", parameters)}";

                using var webRequest = UnityWebRequest.Post(uri, new List<IMultipartFormSection>());
                webRequest.SetRequestHeader("User-Agent", $"Unity {Application.unityVersion}");

                var eventNameString = EventNameOrParamater.ToString(eventName);
                var eventParameterString = EventNameOrParamater.ToString(eventParameter);

#if USE_GOOGLE_ANALYTICS_4
                var json =
                    "{" +
                    $"    client_id: '{clientId}'," +
                    "    events: [{" +
                    $"        name: '{eventNameString}'," +
                    $"        params: {{ parameter: '{eventParameterString}' }}," +
                    "     }]" +
                    "}";
                var jsonBytes = Encoding.UTF8.GetBytes(json);
                webRequest.uploadHandler = new UploadHandlerRaw(jsonBytes);
                webRequest.SetRequestHeader("Content-Type", "application/json");
#endif

                const string logPrefix =
#if USE_DEBUG_HOST
                    "DEBUG_HOST: ";
#else
                    "";
#endif

                DebugUtil.TestLog($"[Analytics] {logPrefix}PostEvent({eventNameString}, {eventParameterString})");

                DebugUtil.TestEnableLogFileOutput(false);

                yield return webRequest.SendWebRequest();

                DebugUtil.TestEnableLogFileOutput(true);

                if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                    webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    DebugUtil.LogWarning("[Analytics] " + webRequest.error);
                }
                else
                {
                    DebugUtil.TestLog($"[Analytics] Response {webRequest.result} ({webRequest.responseCode})");
                    DebugUtil.TestLog($"[Analytics] Response text=\"{webRequest.downloadHandler.text}\"");
                }

                DebugUtil.TestEnableLogFileOutput(false);
            }
        }

        private static class EventNameOrParamater
        {
            public static bool TryParse(string name, out EventName outValue) {
                return CSharpUtil.TryParse(name, out outValue);
            }

            public static bool TryParse(string name, out EventParameter outValue) {
                return CSharpUtil.TryParse(name, out outValue) || CSharpUtil.TryParse(name + "_", out outValue);
            }

            public static string ToString(EventName name) {
                return name.ToString();
            }

            public static string ToString(EventParameter name) {
                return name.ToString().TrimEnd(new[] {'_'});
            }
        }
    }
}