using RPGMaker.Codebase.Runtime.Common;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Map
{
    /// <summary>
    /// マップのScene制御
    /// </summary>
    public class SceneMap : SceneBase
    {
        [SerializeField] public GameObject menuGameObject;
        [SerializeField] public GameObject rootGameObject;

        protected override void Start() {
            HudDistributor.Instance.StaticHudHandler().DisplayInitByScene();
            HudDistributor.Instance.StaticHudHandler().FadeInFixedBlack(Init,   false, 0f, true);
            MenuManager.IsEndGameToTitle = false;
        }

        protected override void Init() {
            base.Init();
            MapManager.InitManager(rootGameObject, Camera.main, menuGameObject);

            // パーティメンバーの隊列歩行の反映
            // follow = -1 の場合は、旧セーブデータで、隊列歩行状態を持っていないもののため、マスタデータの値を反映する
            if (DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.follow == -1)
            {
                DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.follow = DataManager.Self().GetSystemDataModel().optionSetting.optFollowers;
            }

            MapManager.ChangePlayerFollowers(DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.follow == 1);

            //カメラ設定、画面サイズによって変更する。アスペクト比はベース維持
            var baseWidth = 9.0f;
            var baseHeight = 16.0f;

            var displaySize = DataManager.Self().GetSystemDataModel()
                .DisplaySize[DataManager.Self().GetSystemDataModel().displaySize];
            var scaleWidth = displaySize.y / baseHeight * (baseWidth / displaySize.x);
            var scaleRatio = Mathf.Max(scaleWidth, 1.0f);
            Camera.main.orthographicSize *= scaleRatio;

            //Canvas大きさ設定
            var scales = rootGameObject.transform.GetComponentsInChildren<CanvasScaler>();
            foreach (var scale in scales)
            {
                scale.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                //scale.referenceResolution = displaySize;
            }

            // ループマップなどで主人公が下に移動し続けると表示されなくなるので、
            // ひとまず表示される範囲で極端に小さな値を設定。
            Camera.main.nearClipPlane = -10000000000f;

            gameObject.AddComponent<SelectedGameObjectManager>();

            TimeHandler.Instance.AddTimeActionEveryFrame(UpdateTimeHandler);
        }

        private void OnDestroy() {
            TimeHandler.Instance?.RemoveTimeAction(UpdateTimeHandler);
        }

        public void UpdateTimeHandler() {
            //Map更新処理
            MapManager.UpdateEventWatch();

            //GameOverへの遷移中でなければ、キーイベントを受け付ける
            if (!MapManager.IsMovingGameOver)
                InputHandler.Watch();
        }
    }
}