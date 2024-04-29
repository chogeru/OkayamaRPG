using UnityEngine;
using UnityEngine.Tilemaps;

namespace RPGMaker.Codebase.Runtime.Map
{
    /// <summary>
    /// 開始時にCollisionレイヤーを非表示にするだけのスクリプト
    /// </summary>
    public class BackgroundCollisionViewManager : MonoBehaviour
    {
        private void Start() {
            // TilemapRendererを非表示に設定
            transform.GetComponent<TilemapRenderer>().enabled = false;
        }
    }
}