using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Troop;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using RPGMaker.Codebase.Runtime.Battle.Sprites;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace RPGMaker.Codebase.Editor.DatabaseEditor.View.Preview
{
    /// <summary>
    ///     バトルシーン用のプレビュー
    /// </summary>
    public class BattleScenePreview
    {
        private const string PrefabPath =
            "Assets/RPGMaker/Codebase/Runtime/Battle/Asset/Prefab/BattleScenePreviewCanvas.prefab";

        private GameObject _canvasGo;
        private SpritesetBattle _spritesetBattle;
        private TroopDataModel _troopDataModel;
        private SceneWindow _sceneWindow;

        /// <summary>
        ///     初期設定
        /// </summary>
        public void InitUi(SceneWindow sceneWindow, TroopDataModel troopDataModel) {
            _troopDataModel = troopDataModel;

            //ゲーム実行中はプレビューを表示しない
            if (EditorApplication.isPlaying)
            {
                return;
            }

            _canvasGo = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            sceneWindow.MoveGameObjectToPreviewScene(_canvasGo);
            _canvasGo.gameObject.GetComponent<Canvas>().worldCamera = sceneWindow.Camera;

            _spritesetBattle =
                _canvasGo.transform.Find("SpriteSetBattle").GetComponent<SpritesetBattle>();

            _sceneWindow = sceneWindow;
        }

        public async void Render() {
            //ゲーム実行中はプレビューを表示しない
            if (EditorApplication.isPlaying)
            {
                return;
            }
            _canvasGo.SetActive(true);
            _spritesetBattle.RenderBattleScenePreview(_troopDataModel.id);

            //少し待ってからSceneWindowの再描画を行う
            await Task.Delay(1);
            _sceneWindow.ReRender(1);

            _canvasGo.gameObject.GetComponent<Canvas>().worldCamera.Render();
            _canvasGo.SetActive(false);
        }

#if DEBUG && false
        /// <summary>
        /// 他のプレビューの動作確認用メソッド。
        /// </summary>
        /// <param name="sceneWindow">プレビュー(多分)シーン用ウィンドウ</param>
        /// <remarks>
        /// 他のプレビューのInitUi()、Render()メソッドの代わりに呼び出して他の部分に問題がないかを確認する、
        /// という使用方法を想定している。
        /// </remarks>
        public static void StaticInit(SceneWindow sceneWindow) {
            var _databaseManagementService = new CoreSystem.Service.DatabaseManagement.DatabaseManagementService();
            var troopDataModels = _databaseManagementService.LoadTroop().dataModels;
            var firstTroopDataModel = troopDataModels[0];

            var canvasGo = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
            sceneWindow.AddPreview(canvasGo);
            canvasGo.gameObject.GetComponent<Canvas>().worldCamera = sceneWindow.Camera;

            var spritesetBattle =
                canvasGo.transform.Find("SpriteSetBattle").GetComponent<Runtime.Battle.Sprites.SpritesetBattle>();
            spritesetBattle.RenderBattleScenePreview(firstTroopDataModel.id);
        }
#endif
    }
}