using RPGMaker.Codebase.CoreSystem.Helper;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace RPGMaker.Codebase.Runtime.Common.Component.Hud
{
    public class Movie : MonoBehaviour
    {
        // const
        //--------------------------------------------------------------------------------------------------------------
        private const string PrefabPath       = "Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/Movie.prefab";
        private const string MoviePath        = "Assets/RPGMaker/Storage/Movies/";
        private const string ParentObjectName = "Canvas";

        // データプロパティ
        //--------------------------------------------------------------------------------------------------------------

        // 状態プロパティ
        //--------------------------------------------------------------------------------------------------------------

        // 表示要素プロパティ
        //--------------------------------------------------------------------------------------------------------------
        private GameObject    _prefab;
        private RawImage      _rawImage;
        private RenderTexture _renderTexture;
        private VideoPlayer   _videoPlayer;
        private Action _callBack;

        // 関数プロパティ
        //--------------------------------------------------------------------------------------------------------------

        // enum / interface / local class
        //--------------------------------------------------------------------------------------------------------------

        // initialize methods
        //--------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 初期化
        /// </summary>
        public void Init() {
            //描画用のプレハブが無い場合に生成
            if (_prefab == null)
            {
                var loadPrefab = UnityEditorWrapper.PrefabUtilityWrapper.LoadPrefabContents(PrefabPath);
                _prefab = Instantiate(
                    loadPrefab,
                    gameObject.transform,
                    true
                );
                UnityEditorWrapper.PrefabUtilityWrapper.UnloadPrefabContents(loadPrefab);
            }

            _prefab.SetActive(true);
        }

        /// <summary>
        /// ムービーファイル名を渡して、読み込みの実施
        /// </summary>
        /// <param name="pictureName"></param>
        /// <param name="callBack"></param>
        public void AddMovie(string pictureName, Action callBack) {
            
            //動画ファイルの読み込み
            var acquiredMovie =
                UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<VideoClip>(MoviePath + pictureName + ".mp4");
            
            //再生用のオブジェクトの生成と設定
            var obj = new GameObject();
            obj.transform.SetParent(_prefab.transform.Find(ParentObjectName).transform);
            obj.AddComponent<VideoPlayer>();
            obj.AddComponent<RectTransform>();
            obj.AddComponent<RawImage>();
            obj.GetComponent<RectTransform>().sizeDelta = new Vector2(acquiredMovie.width, acquiredMovie.height);
            obj.GetComponent<RectTransform>().localPosition = Vector3.zero;
            obj.transform.localScale = new Vector3(1, 1, 1);

            //描画用「RenderTexture」の設定
            _renderTexture = new RenderTexture((int) acquiredMovie.width, (int) acquiredMovie.height, 24);

            //描画用「RawImage」の設定
            _rawImage = obj.GetComponent<RawImage>();
            _rawImage.texture = _renderTexture;

            //描画用「VideoPlayer」の設定
            _videoPlayer = obj.GetComponent<VideoPlayer>();
            _videoPlayer.targetTexture = _renderTexture;
            _videoPlayer.loopPointReached += EndReached;
            _videoPlayer.clip = acquiredMovie;
            _callBack = callBack;
            //実際に再生していく
            _videoPlayer.Prepare();
            TimeHandler.Instance.AddTimeActionEveryFrame(Prepared);
        }

        private void EndReached(VideoPlayer yuhrr) {
            _videoPlayer.frame = 0;
        }

        private void Prepared() {
            if (_videoPlayer.isPrepared)
            {
                TimeHandler.Instance.RemoveTimeAction(Prepared);
                //再生の開始
                _videoPlayer.Play();
                TimeHandler.Instance.AddTimeActionEveryFrame(PlayMovie);
            }
        }

        /// <summary>
        /// 実際に再生から停止までを司る部分
        /// </summary>
        private void PlayMovie() {
            if(_videoPlayer.isPlaying) return;

            TimeHandler.Instance.RemoveTimeAction(PlayMovie);
            //再生の停止
            _videoPlayer.Stop();
            _videoPlayer.gameObject.SetActive(false);
            _callBack?.Invoke();
        }
    }
}