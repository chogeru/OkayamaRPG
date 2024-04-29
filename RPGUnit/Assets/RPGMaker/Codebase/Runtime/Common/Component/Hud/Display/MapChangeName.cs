using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.Runtime.Map;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

//バトルでは本コマンドは利用しない

namespace RPGMaker.Codebase.Runtime.Common.Component.Hud.Display
{
    public class MapChangeName : MonoBehaviour
    {
        // const
        //--------------------------------------------------------------------------------------------------------------
        private const string PrefabPath =
            "Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/MapChangeName.prefab";

        // 表示要素プロパティ
        //--------------------------------------------------------------------------------------------------------------
        private GameObject _prefab;

        // データ
        //--------------------------------------------------------------------------------------------------------------
        private RuntimePlayerDataModel _runtimePlayerDataModel;
        private TextMeshProUGUI        _text;


        public void Init() {
            //データ取得
            _runtimePlayerDataModel = DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel;

            //マップ名の表示がONになっているか
            if (_runtimePlayerDataModel.map.nameDisplay == 1 && MapManager.IsDisplayName == 0)
            {
                //既にある場合は作成しない
                if (_prefab == null)
                {
                    var loadPrefab = UnityEditorWrapper.PrefabUtilityWrapper.LoadPrefabContents(PrefabPath);
                    //マップ名表示ウィンドウの生成
                    _prefab = Instantiate(
                        loadPrefab,
                        gameObject.transform,
                        true
                    );
                    _prefab.name = loadPrefab.name;
                    UnityEditorWrapper.PrefabUtilityWrapper.UnloadPrefabContents(loadPrefab);

                }

                _prefab.SetActive(true);
                _text = _prefab.transform.GetComponentInChildren<TextMeshProUGUI>();

                //マップ名の表示
                var mapNama = MapManager.CurrentMapDataModel.displayName;
                _text.text = mapNama;

                //一定時間表示を行う
                CloseMapNameDisplay();

                //このマップでマップ名が表示されたことを入れる
                MapManager.IsDisplayName = 1;
            }
        }

        //マップ名の表示削除用
        private async void CloseMapNameDisplay() {
            await Task.Delay(1000);
            //オブジェクトの削除
            HudDistributor.Instance.NowHudHandler().ClosePlayChangeMapName();
        }
    }
}