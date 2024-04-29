using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Helper.SO;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using JsonHelper = RPGMaker.Codebase.CoreSystem.Helper.JsonHelper;

namespace RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository
{
    public class AssetManageRepository
    {
        private static readonly string JSON_PATH = "Assets/RPGMaker/Storage/AssetManage/JSON";
#if !UNITY_EDITOR
        private static readonly string SO_PATH = "Assets/RPGMaker/Storage/AssetManage/SO";
#endif
        private static List<AssetManageDataModel> _assetManageDataModels;

        /// <summary>
        ///     JSONファイルに保存する
        /// </summary>
        public void Save(AssetManageDataModel dataModel) {
            if (dataModel == null)
                throw new Exception("Tried to save null data model.");

            File.WriteAllText(JSON_PATH + "/Assets/" + dataModel.id + ".json", JsonUtility.ToJson(dataModel));

            // キャッシュを更新
            var targetIndex = _assetManageDataModels.FindIndex(item => item.id == dataModel.id);
            if (targetIndex != -1)
                _assetManageDataModels[targetIndex] = dataModel;

            UnityEditorWrapper.AssetDatabaseWrapper.Refresh2();
        }

        /// <summary>
        ///     Jsonデータ読み込み
        /// </summary>
        public List<AssetManageDataModel> Load() {
#if UNITY_EDITOR
            if (_assetManageDataModels != null)
                // キャッシュがあればそれを返す
                return _assetManageDataModels;

            _assetManageDataModels = new List<AssetManageDataModel>();
            try
            {
                // ディレクトリ内のファイル全取得
                var dataPath = Directory.GetFiles(JSON_PATH + "/Assets/", "*.json", SearchOption.AllDirectories);
                for (var i = 0; i < dataPath.Length; i++)
                {
                    dataPath[i] = dataPath[i].Replace("\\", "/");

                    // 取得したJSONデータを読み込む
                    var jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(dataPath[i]);
                    var json = JsonHelper.FromJson<AssetManageDataModel>(jsonString);
                    _assetManageDataModels.Add(json);
                }

                //暫定対応（Runtime実行にまとめたJSON用意しておく）
                File.WriteAllText(JSON_PATH + "/assetsData.json", JsonHelper.ToJsonArray(_assetManageDataModels));
            }
            catch (IOException)
            {
            }

            return _assetManageDataModels;
#else
            List<AssetManageDataModel> assetsData = AddressableManager.Load.LoadAssetSync<AssetManagesSO>(SO_PATH + "/assetsData.asset").dataModels;
            _assetManageDataModels = assetsData;
            //SetSerialNumbers();
            return _assetManageDataModels;
#endif
        }

        // 順番管理用
        public class OrderManager
        {
            private static readonly string JSON_NAME = "/orderManager.json";

            private static OrderData _orderData;

            // ロード
            public static OrderData Load() {
                if (_orderData != null)
                    // キャッシュがあればそれを返す
                    return _orderData;

                // 取得したJSONデータを読み込む
                var inputString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JSON_PATH + JSON_NAME);
                _orderData = new OrderData();

                // null
                if (inputString == null)
                {
                    // 初期データ作成
                    _orderData.orderDataList =
                        new OrderDataList[Enum.GetValues(typeof(AssetCategoryEnum)).Length];
                    for (var i = 0; i < Enum.GetValues(typeof(AssetCategoryEnum)).Length; i++)
                        _orderData.orderDataList[i].assetTypeId = i;
                }
                else
                {
                    try
                    {
                        _orderData = JsonUtility.FromJson<OrderData>(inputString);
                    }
                    catch (Exception)
                    {
                        _orderData = new OrderData();
                    }
                }

                return _orderData;
            }

            // セーブ
            public static void Save(OrderData data) {
                File.WriteAllText(JSON_PATH + JSON_NAME, JsonUtility.ToJson(data));

                // キャッシュを更新
                _orderData = data;
            }

            // 順番管理用クラス
            [Serializable]
            public struct OrderDataList
            {
                public int          assetTypeId;
                public List<string> idList;
            }

            [Serializable]
            public class OrderData
            {
                public OrderDataList[] orderDataList;
            }
        }
    }
}