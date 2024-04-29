using RPGMaker.Codebase.CoreSystem.Helper;
using System;
using UnityEngine;
using UnityEngine.UI;
using TextMP = TMPro.TextMeshProUGUI;

namespace RPGMaker.Codebase.Runtime.Battle.Objects
{
    public class Selector : MonoBehaviour
    {
        private                 int        _index;
        [SerializeField] public GameObject canvas;
        [SerializeField] public TextMP     label;

        public Button button { get; private set; }

        private void Start() {
        }

        public void SetUp(int index, string name, Action<int> Select, Action<int> OnClick) {
            _index = index;
            label.text = name;
            label.raycastTarget = false;

            button = canvas.GetComponent<Button>();
            if (button == null)
            {
                button = canvas.AddComponent<Button>();
                if (canvas.transform.Find("Highlight") != null)
                {
                    if (canvas.transform.Find("Highlight").GetComponent<Image>() != null)
                        button.targetGraphic = canvas.transform.Find("Highlight").GetComponent<Image>();
                }

                var colorBlock = new ColorBlock();
                //元々の値の代入
                colorBlock = button.colors;
                //ココで「normalColor」のaが0じゃないとハイライトがうまくいかない為「0」へ
                colorBlock.normalColor = new Color(
                    button.colors.normalColor.r,
                    button.colors.normalColor.g,
                    button.colors.normalColor.b,
                    0
                );
                //変更した値を戻す
                button.colors = colorBlock;
            }
            WindowButtonBase buttonBase = canvas.GetComponent<WindowButtonBase>();
            if (buttonBase == null)
            {
                buttonBase = canvas.AddComponent<WindowButtonBase>();
            }
            else
            {
                buttonBase.OnFocus.RemoveAllListeners();
                buttonBase.OnClick.RemoveAllListeners();
            }
            buttonBase.OnFocus = new UnityEngine.Events.UnityEvent();
            buttonBase.OnFocus.AddListener(() =>
            {
                if (Select != null)
                    Select(_index);
            });
            buttonBase.OnClick = new Button.ButtonClickedEvent();
            buttonBase.OnClick.AddListener(() =>
            {
                if (OnClick != null)
                    OnClick(_index);
            });
            canvas.SetActive(true);
        }
    }
}