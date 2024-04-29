using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.Runtime.Common.Enum;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Common.Component.Hud.Message
{
    public class MessageInputNumber : MonoBehaviour
    {
        // const
        //--------------------------------------------------------------------------------------------------------------
        private const string PrefabPath =
            "Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/MessageInputNum.prefab";

        private RectTransform _backGround;
        private RectTransform _flame;

        // データプロパティ
        //--------------------------------------------------------------------------------------------------------------
        private int              _lastTimeDigitsCount;
        private int              _nowDigitsCount;
        private List<int>        _nowNumber;
        private int              _numDigits;
        private List<GameObject> _panel;

        // 表示要素プロパティ
        //--------------------------------------------------------------------------------------------------------------
        private GameObject _prefab;
        private List<Text> _text;
        public Button DownButton { private set; get; } 
        public Button UpButton { private set; get; }
        public Button DecideButton { private set; get; }
        public List<Button> NumbersButton { private set; get; }

        // initialize methods
        //--------------------------------------------------------------------------------------------------------------
        /**
         * 初期化
         */
        public void Init(int numDigits) {
            _numDigits = numDigits;
            var loadPrefab = UnityEditorWrapper.PrefabUtilityWrapper.LoadPrefabContents(PrefabPath);
            _prefab = Instantiate(
                loadPrefab,
                gameObject.transform,
                true
            );
            UnityEditorWrapper.PrefabUtilityWrapper.UnloadPrefabContents(loadPrefab);


            _text = new List<Text>();
            _panel = new List<GameObject>();
            _nowNumber = new List<int>();
            NumbersButton = new List<Button>();
            var textParent = _prefab.transform.Find("Canvas/Frame/TextParent");

            for (var i = 0; i < textParent.childCount; i++)
                _text.Add(textParent.GetChild(i).transform.GetComponent<Text>());
            foreach (var data in _text)
            {
                data.transform.GetChild(0).gameObject.SetActive(true);
                _panel.Add(data.transform.GetChild(0).gameObject);
            }

            for (var i = 0; i < _numDigits; i++)
                _nowNumber.Add(0);

            for (var i = 0; i < _text.Count; i++)
            {
                NumbersButton.Add(_text[i].transform.GetComponent<Button>());
            }

            _backGround = _prefab.transform.Find("Canvas/Image").GetComponent<RectTransform>();
            _flame = _prefab.transform.Find("Canvas/Frame").GetComponent<RectTransform>();

            DownButton = _flame.transform.Find("Down").GetComponent<Button>();
            UpButton = _flame.transform.Find("Up").GetComponent<Button>();
            DecideButton = _flame.transform.Find("Decide").GetComponent<Button>();

            _lastTimeDigitsCount = 0;
            _nowDigitsCount = 0;
            
            ShowInputWindow();
            for (var i = 0;i < _panel.Count;i++)
            {
                if(i != 0) _panel[i].GetComponent<Animator>().enabled = false;
            }
        }
        

        private void ShowInputWindow() {
            //今回表示される桁数分回して表示している
            for (var i = 0; i < _numDigits; i++)
                _text[i].gameObject.SetActive(true);


            //表示する数分幅を広げる
            float widthLength = 66 * (_numDigits - 1);
            _backGround.offsetMax = new Vector2(_backGround.offsetMax.x + widthLength, _backGround.offsetMax.y);
            _flame.offsetMax = new Vector2(_flame.offsetMax.x + widthLength, _flame.offsetMax.y);
            
            int y = 0;
            // 文章表示位置によってY座標を設定
            if (HudDistributor.Instance.NowHudHandler().IsMessageWindowActive())
                if (HudDistributor.Instance.NowHudHandler().GetMessageWindowPos() == 1)
                    y = -270;

            _backGround.transform.localPosition = new Vector3(0f, y);
            _flame.transform.localPosition = new Vector3(0f, y);

            PanelActive();
        }

        public int Process(HandleType type) {
            var num = 0;
            switch (type)
            {
                case HandleType.Left:
                    _lastTimeDigitsCount = _nowDigitsCount;
                    _nowDigitsCount--;
                    _nowDigitsCount = _nowDigitsCount < 0 ? _numDigits - 1 : _nowDigitsCount;
                    PanelActive();
                    break;
                case HandleType.Right:
                    _lastTimeDigitsCount = _nowDigitsCount;
                    _nowDigitsCount++;
                    _nowDigitsCount = _nowDigitsCount > _numDigits - 1 ? 0 : _nowDigitsCount;
                    PanelActive();
                    break;
                case HandleType.Up:
                    num = int.Parse(_text[_nowDigitsCount].text) + 1;
                    num = num % 10;
                    _text[_nowDigitsCount].text = num.ToString();
                    _nowNumber[_nowDigitsCount] = num;
                    break;
                case HandleType.Down:
                    num = int.Parse(_text[_nowDigitsCount].text) - 1;
                    num = num + 10;
                    num = num % 10;
                    _text[_nowDigitsCount].text = num.ToString();
                    _nowNumber[_nowDigitsCount] = num;
                    break;
            }

            var Number = 0;
            for (var i = 0; i < _numDigits; i++)
            {
                var n = int.Parse(_text[i].text);
                for (var i2 = 0; i < i2; i2++)
                    n = n * 10;
                Number += n;
            }

            return Number;
        }

        public void PanelActive() {
            _panel[_lastTimeDigitsCount].GetComponent<Animator>().enabled = false;
            _panel[_lastTimeDigitsCount].GetComponent<Image>().color = new Color(1f,1f,1f,0.01f);
            _panel[_nowDigitsCount].GetComponent<Animator>().enabled = true;
            _panel[_nowDigitsCount].GetComponent<Image>().color = new Color(1f,1f,1f,1f);
        }

        public void ClickCursor(int index) {
            _lastTimeDigitsCount = _nowDigitsCount;
            _nowDigitsCount = index;
            _panel[_lastTimeDigitsCount].GetComponent<Animator>().enabled = false;
            _panel[_lastTimeDigitsCount].GetComponent<Image>().color = new Color(1f,1f,1f,0.01f);
            _panel[_nowDigitsCount].GetComponent<Animator>().enabled = true;
            _panel[_nowDigitsCount].GetComponent<Image>().color = new Color(1f,1f,1f,1f);
        }

        public int GetNowNumber() {
            var returnNumber = "0";
            foreach (var number in _nowNumber)
                returnNumber += number.ToString();

            return int.Parse(returnNumber);
        }
    }
}