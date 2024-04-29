using System;
using System.Threading.Tasks;
using UnityEditor;

namespace RPGMaker.Codebase.Editor.Common
{
    /// <summary>
    /// インポートなど、AssetDatabaseの更新を変更するためのAPIクラス
    /// </summary>
    public static class ApiManager
    {
        private static int _count;
        private static int _time = 5000;
        private static int timeFrom;
        private static bool _isAssetDatabaseStop = false;
        private static bool _isTileOrImage = false;
        private static bool _isTimeCount = false;

        //それ以外のインポートが実行しているとき
        public static bool IsAssetDatabaseStop
        {
            get => _isAssetDatabaseStop;
            set => _isAssetDatabaseStop = value;
        }

        //タイルや画像インポートが実行しているとき
        public static bool IsTileOrImageAssetDatabase
        {
            get => _isTileOrImage;
            set => _isTileOrImage = value;
        }

        /// <summary>
        /// AssetDatabaseの更新停止
        /// </summary>
        public static void AssetDatabaseStop() {
            if (_isTileOrImage) return;
            if (_count == 0)
            {
                AssetDatabase.StartAssetEditing();
                _isAssetDatabaseStop = true;
            }
            _count++;
        }
        
        /// <summary>
        /// AssetDatabaseの更新再開
        /// </summary>
        public static void AssetDatabaseRestart() {
            if (_isTileOrImage) return;
            _count--;
            if (_count == 0)
            {
                AssetDatabase.StopAssetEditing();
                _isAssetDatabaseStop = false;
            }
        }
        
        /// <summary>
        /// 読み込み（残しておく）
        /// </summary>
        public static async void AssetDatabaseStopToRestart() {
            AssetDatabase.StopAssetEditing();
            await Task.Delay(1);
            AssetDatabase.StartAssetEditing();
        }
        
        /// <summary>
        /// タイムアウト時間調整（残しておく）
        /// </summary>
        /// <param name="time"></param>
        public static void SetTimeOut(int time) {
            _time = time;
        }

        /// <summary>
        /// AssetDatabaseの更新停止状態取得
        /// </summary>
        /// <returns></returns>
        public static bool GetAssetDatabaseStatus() {
            return _isAssetDatabaseStop;
        }

        /// <summary>
        /// タイムアウトの時間を図る（残しておく）
        /// </summary>
        private static async void TimeCount() {
            if (_isTimeCount) return;
            _isTimeCount = true;
            while (_count != 0)
            {
                await Task.Delay(1);
                
                if (DateTime.Now.Hour * 60 * 60 * 1000 + DateTime.Now.Minute * 60 * 1000 + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond - timeFrom > _time)
                {
                    //AssetDatabaseの更新が止まっていたら、再開する
                    if (_isAssetDatabaseStop)
                    {
                        AssetDatabase.StartAssetEditing();
                        _count = 0;
                        _isAssetDatabaseStop = false;
                        break;
                    }
                }
            }
            _isTimeCount = false;
        }

    }
}
