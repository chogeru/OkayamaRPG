using System;
using System.Linq;
using UnityEngine;

namespace RPGMaker.Codebase.CoreSystem.Helper
{
    /// <summary>
    /// Unityユーティリティークラス。
    /// </summary>
    public static class UnityUtil
    {
        public static void SetPositionXY(this Transform transform, Vector2 position)
        {
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }

        public static void SetLocalPositionY(this Transform transform, float y)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, y, transform.localPosition.z);
        }

        public static void SetLocalPositionZ(this Transform transform, float z)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, z);
        }

        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject.TryGetComponent(out T component))
            {
                return component;
            }

            return gameObject.AddComponent<T>();
        }

        /// <summary>
        /// Unityのソートレイヤーの設定を列挙型SortingLayerIndexで参照するクラス。
        /// </summary>
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoad]
#endif
        public static class SortingLayerManager
        {
            static SortingLayerManager()
            {
#if UNITY_EDITOR
                // Unityエディタープロジェクトに設定されているソートレイヤー情報と列挙型SortingLayerIndexが
                // 一致しているか確認。
                var sortingLayerIndexes = (SortingLayerIndex[]) Enum.GetValues(typeof(SortingLayerIndex));
                var zipLayers = SortingLayer.layers.Zip(sortingLayerIndexes,
                    (unitySortingLayer, sortingLayerIndex) =>
                        new { UnitySortingLayer = unitySortingLayer, SortingLayerIndex = sortingLayerIndex });

                var isCheckOk = zipLayers.All(
                    zipLayer => zipLayer.UnitySortingLayer.name == zipLayer.SortingLayerIndex.ToString());
                DebugUtil.Log($"-------------------------------------------------");
                DebugUtil.Log($"SortingLayer Check {(isCheckOk ? "Ok": "Error")}");
                DebugUtil.Log($"-------------------------------------------------");

                if (!isCheckOk)
                {
                    DebugUtil.Log($"UnityEngine.SortingLayer.Name : SortingLayerIndex");
                    DebugUtil.Log($"-------------------------------------------------");

                    foreach (var zipLayer in zipLayers)
                    {
                        var same = zipLayer.UnitySortingLayer.name == zipLayer.SortingLayerIndex.ToString();
                        DebugUtil.Log(
                            $"{zipLayer.UnitySortingLayer.name} " +
                            $"{(same ? "==" : "!=")} " +
                            $"{zipLayer.SortingLayerIndex}");
                    }

                    DebugUtil.Log($"-------------------------------------------------");
                }

                DebugUtil.Assert(
                    isCheckOk,
                    $"Unityのソートレイヤーの設定と列挙型{nameof(SortingLayerIndex)}の名称列が一致していません。");
#endif
            }

            /// <summary>
            /// 本プロジェクトで設定されているソートレイヤーを列挙型で定義。
            /// </summary>
            /// <remarks>
            /// 文字列ではなくこの列挙型でソートレイヤーを指定する目的で用意されたもの。
            /// Unityエディターの以下の場所の設定と内容が一致している必要がある。
            ///   メインメニュー/設定/プロジェクト設定/タグとレイヤー/ソートレイヤー
            /// </remarks>
            public enum SortingLayerIndex
            {
                Default,
                MapLayerDistantView,                        // マップ遠景レイヤー。
                MapLayerBackground,                         // マップ背景レイヤー。
                MapLayerBackgroundCollision,                // マップ背景衝突判定用レイヤー (背景レイヤーが1枚絵の場合に衝突判定用に設定される。非表示)。
                MapLayerA,                                  // マップレイヤーA。
                MapLayerA_Effect,                           // マップレイヤーAエフェクト。
                MapLayerB,                                  // マップレイヤーB。
                MapLayerB_Effect,                           // マップレイヤーBエフェクト。
                MapLayerShadow,                             // マップ影レイヤー。
                MapLayerC,                                  // マップレイヤーC。
                MapLayerC_Effect,                           // マップレイヤーCエフェクト。
                MapLayerD,                                  // マップレイヤーD。
                MapLayerD_Effect,                           // マップレイヤーDエフェクト。
                MapLayerForRoute,                           // 使用しなくなったレイヤー (削除しても問題ないようだが変更が各所に及ぶので現状は非表示のみ)。
                MapLayerRegion,                             // マップリージョンレイヤー (マップタイルごとにリージョン番号が設定されている。非表示)。
                Runtime_MapCharacterPriorityUnder,          // キャラクター プライオリティ 下。
                Runtime_MapCharacterPriorityNormal,         // キャラクター プライオリティ 通常。
                Runtime_MapCharacterPriorityUpper,          // キャラクター プライオリティ 上。
                Runtime_MapLayerA_Upper,                    // マップレイヤーAの上層用レイヤー (通り抜けタイル表示用)。
                Runtime_MapLayerB_Upper,                    // マップレイヤーBの上層用レイヤー (通り抜けタイル表示用)。
                Runtime_MapLayerC_Upper,                    // マップレイヤーCの上層用レイヤー (通り抜けタイル表示用)。
                Runtime_MapLayerD_Upper,                    // マップレイヤーDの上層用レイヤー (通り抜けタイル表示用)。
                Runtime_MapCharacterInFlyingPriorityUnder,  // 飛行中のキャラクター プライオリティ 下。
                Runtime_MapCharacterInFlyingPriorityNormal, // 飛行中のキャラクター プライオリティ 通常。
                Runtime_MapCharacterInFlyingPriorityUpper,  // 飛行中のキャラクター プライオリティ 上。
                Runtime_MapBalloon,                         // フキダシ。
                Runtime_Weather,                            // 天候
                Editor_Event,                               // イベント半透明矩形。
                Editor_Image,                               // イベントに設定した画像。
                Editor_LineGrid,                            // タイルグリッド線。
                Editor_Cursor,                              // カーソル矩形。
            }

            public static int GetId(SortingLayerIndex sortingLayerIndex)
            {
                return SortingLayer.layers[(int)sortingLayerIndex].id;
            }
        }
    }
}
