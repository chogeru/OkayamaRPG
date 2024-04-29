using RPGMaker.Codebase.CoreSystem.Helper;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Battle.BattleSceneTest
{
    public class BattleSceneTest : MonoBehaviour
    {
        [SerializeField] private int duration;

        [SerializeField] private Text text;

        public static event Action<string> ScenePlayEndEvent;

        // Start is called before the first frame update
        private void Start() {
        }

        // Update is called once per frame
        private void Update() {
            var count = Math.Max((int) Math.Ceiling((double) duration - Time.time), 0);
            if (text.text != count.ToString())
            {
                text.text = count.ToString();
                DebugUtil.Log($"BattleSceneTest countdown {count}.");
            }

            if (count == 0) ScenePlayEndEvent?.Invoke(SceneManager.GetActiveScene().name);
        }
    }
}
