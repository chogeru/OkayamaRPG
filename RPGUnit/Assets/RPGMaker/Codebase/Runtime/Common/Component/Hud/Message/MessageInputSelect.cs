using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.Runtime.Common.Enum;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Common.Component.Hud.Message
{
    public class MessageInputSelect : MonoBehaviour
    {
        public static float StartY = -459f;
        public static int SelectIndex = 0;
        // const
        //--------------------------------------------------------------------------------------------------------------
        private readonly string PrefabPath =
            "Assets/RPGMaker/Codebase/Runtime/Map/Asset/Prefab/MessageInputSelect.prefab";

        private List<Button> _buttons;
        private int          _nowDigitsCount;
        private int          _numDigits;

        // 表示要素プロパティ
        //--------------------------------------------------------------------------------------------------------------
        private GameObject                  _prefab;
        private List<HorizontalLayoutGroup> _ruckList;

        private GameObject _maskObj;
        private VerticalLayoutGroup _maskVerticalLayoutGroup;
        private ContentSizeFitter _maskContentSizeFitter;
        private Image _arrowUp;
        private Image _arrowDown;
        private Button _arrowUpButton;
        private Button _arrowDownButton;
        private bool _isArrowEnabled = false;

        private Image _backgroundImage;
        private Image _frameImage;
        private Color _backgroundColor;
        private Color _frameColor;
        

        private int        _selectNum;
        private List<Text> _text;

        // データプロパティ
        //--------------------------------------------------------------------------------------------------------------
        private int activeFrameCount;

        // initialize methods
        //--------------------------------------------------------------------------------------------------------------
        /**
         * 初期化
         */
        public void Init() {
            var loadPrefab = UnityEditorWrapper.PrefabUtilityWrapper.LoadPrefabContents(PrefabPath);
            _prefab = Instantiate(
                loadPrefab,
                gameObject.transform,
                true
            );
            UnityEditorWrapper.PrefabUtilityWrapper.UnloadPrefabContents(loadPrefab);
            StartY = -459f;
            SelectIndex = 0;

            // 選択肢取得
            _ruckList = new List<HorizontalLayoutGroup>();
            _ruckList.Add(_prefab.transform.Find("Margin/BackImage/Frame/Mask/Highlight/Ruck_1").GetComponent<HorizontalLayoutGroup>());
            _ruckList.Add(_prefab.transform.Find("Margin/BackImage/Frame/Mask/Highlight/Ruck_2").GetComponent<HorizontalLayoutGroup>());
            _ruckList.Add(_prefab.transform.Find("Margin/BackImage/Frame/Mask/Highlight/Ruck_3").GetComponent<HorizontalLayoutGroup>());
            _ruckList.Add(_prefab.transform.Find("Margin/BackImage/Frame/Mask/Highlight/Ruck_4").GetComponent<HorizontalLayoutGroup>());
            _ruckList.Add(_prefab.transform.Find("Margin/BackImage/Frame/Mask/Highlight/Ruck_5").GetComponent<HorizontalLayoutGroup>());
            _ruckList.Add(_prefab.transform.Find("Margin/BackImage/Frame/Mask/Highlight/Ruck_6").GetComponent<HorizontalLayoutGroup>());

            _arrowUp = _prefab.transform.Find("Margin/ArrowUp").GetComponent<Image>();
            _arrowDown = _prefab.transform.Find("Margin/ArrowDown").GetComponent<Image>();
            _arrowUpButton = _prefab.transform.Find("Margin/ArrowUp").GetComponent<Button>();
            _arrowDownButton = _prefab.transform.Find("Margin/ArrowDown").GetComponent<Button>();

            _maskObj = _prefab.transform.Find("Margin/BackImage/Frame/Mask").gameObject;
            _maskVerticalLayoutGroup = _maskObj.GetComponent<VerticalLayoutGroup>();
            _maskContentSizeFitter = _maskObj.GetComponent<ContentSizeFitter>();

            _backgroundImage = _prefab.transform.Find("Margin/BackImage").GetComponent<Image>();
            _frameImage = _prefab.transform.Find("Margin/BackImage/Frame").GetComponent<Image>();
            _backgroundColor = _backgroundImage.color;
            _frameColor = _frameImage.color;


            // 選択肢のテキスト取得
            _text = new List<Text>();
            for (var i = 0; i < _ruckList.Count; i++)
            {
                _text.Add(_ruckList[i].transform.Find("Box/Text").GetComponent<Text>());
                //一時的にサイズ自動調節をOFFにする
                _text[i].resizeTextForBestFit = false;
            }

            // 選択肢のボタン取得
            _buttons = new List<Button>();
            for (var i = 0; i < _text.Count; i++)
            {
                _buttons.Add(_text[i].transform.parent.GetComponent<Button>());
            }

            activeFrameCount = 0;
            _selectNum = -1;

            //少し待ってからActiveにして、サイズ等が反映するようにする
            _prefab.transform.GetComponent<RectTransform>().localScale = new Vector3(0.0f, 0.0f, 0.0f);
            TimeHandler.Instance.AddTimeAction(0.01f, MessageInputSelectActive, false);
        }

        private void MessageInputSelectActive() {
            //サイズ調整
            _prefab.transform.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);

            //Textの中で最も長い文字列を取得
            float maxWidth = 0.0f;
            float maxHeight = 0.0f;
            for (var i = 0; i < _text.Count; i++)
            {
                float width = _text[i].preferredWidth;
                float height = _text[i].preferredHeight;
                if (width > maxWidth) maxWidth = width;
                if (height > maxHeight) maxHeight = height;
            }

            //Textのサイズを、全ての選択肢の中で、最も長いものに変更する
            for (var i = 0; i < _text.Count; i++)
            {
                _text[i].rectTransform.sizeDelta = new Vector2(maxWidth, maxHeight);
            }

            //Box（Button）
            for (var i = 0; i < _buttons.Count; i++)
            {
                _buttons[i].GetComponent<HorizontalLayoutGroup>().CalculateLayoutInputVertical();
                _buttons[i].GetComponent<HorizontalLayoutGroup>().CalculateLayoutInputHorizontal();
                _buttons[i].GetComponent<HorizontalLayoutGroup>().SetLayoutVertical();
                _buttons[i].GetComponent<HorizontalLayoutGroup>().SetLayoutHorizontal();
                LayoutRebuilder.ForceRebuildLayoutImmediate(_buttons[i].GetComponent<RectTransform>());
                _buttons[i].GetComponent<WindowButtonBase>().ScrollView =_prefab.transform.Find("Margin/BackImage/Frame").gameObject;
                _buttons[i].GetComponent<WindowButtonBase>().Content = _prefab.transform.Find("Margin/BackImage/Frame/Mask/Highlight").gameObject;
                _buttons[i].GetComponent<WindowButtonBase>().IsChildrenButton = true;
                _buttons[i].GetComponent<WindowButtonBase>().MyIndex = i;

                //ボタンのクリック音を、共通部品で鳴動させない
                _buttons[i].GetComponent<WindowButtonBase>().SetSilentClick(true);
            }

            //Ruck
            for (int i = 0; i < _ruckList.Count; i++)
            {
                _ruckList[i].CalculateLayoutInputVertical();
                _ruckList[i].CalculateLayoutInputHorizontal();
                _ruckList[i].SetLayoutVertical();
                _ruckList[i].SetLayoutHorizontal();
                LayoutRebuilder.ForceRebuildLayoutImmediate(_ruckList[i].GetComponent<RectTransform>());
            }

            //Highlight
            _prefab.transform.Find("Margin/BackImage/Frame/Mask/Highlight").GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
            _prefab.transform.Find("Margin/BackImage/Frame/Mask/Highlight").GetComponent<VerticalLayoutGroup>().CalculateLayoutInputHorizontal();
            _prefab.transform.Find("Margin/BackImage/Frame/Mask/Highlight").GetComponent<VerticalLayoutGroup>().SetLayoutVertical();
            _prefab.transform.Find("Margin/BackImage/Frame/Mask/Highlight").GetComponent<VerticalLayoutGroup>().SetLayoutHorizontal();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_prefab.transform.Find("Margin/BackImage/Frame/Mask/Highlight").GetComponent<RectTransform>());

            _prefab.transform.Find("Margin/BackImage/Frame/Mask/Highlight").GetComponent<ContentSizeFitter>().enabled =
                false;

            //Frame
            _prefab.transform.Find("Margin/BackImage/Frame").GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
            _prefab.transform.Find("Margin/BackImage/Frame").GetComponent<VerticalLayoutGroup>().CalculateLayoutInputHorizontal();
            _prefab.transform.Find("Margin/BackImage/Frame").GetComponent<VerticalLayoutGroup>().SetLayoutVertical();
            _prefab.transform.Find("Margin/BackImage/Frame").GetComponent<VerticalLayoutGroup>().SetLayoutHorizontal();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_prefab.transform.Find("Margin/BackImage/Frame").GetComponent<RectTransform>());

            if (activeFrameCount > 4 && (HudDistributor.Instance.NowHudHandler().GetMessageWindowPos() == 1 && HudDistributor.Instance.NowHudHandler().IsMessageWindowActive()))
            {
                _isArrowEnabled = true;
                 var sizeDelta = _maskObj.GetComponent<RectTransform>().sizeDelta;
                _maskContentSizeFitter.enabled = false;
                _maskObj.GetComponent<RectTransform>().sizeDelta = new Vector2(sizeDelta.x, 300f);

                var backImage = _prefab.transform.Find("Margin/BackImage").GetComponent<RectTransform>();
                _arrowUp.transform.localPosition = new Vector2(backImage.transform.localPosition.x - sizeDelta.x / 2, backImage.transform.localPosition.y);
                _arrowDown.transform.localPosition = new Vector2(backImage.transform.localPosition.x - sizeDelta.x / 2, backImage.transform.localPosition.y - 300f);
                
                _arrowUpButton.onClick.AddListener(ArrowUp);
                _arrowDownButton.onClick.AddListener(ArrowDown);
                _arrowUp.gameObject.SetActive(false);
                _arrowDown.gameObject.SetActive(true);

            }
            else
            {
                _isArrowEnabled = false;
                _arrowUp.gameObject.SetActive(false);
                _arrowDown.gameObject.SetActive(false);
            }
            
        }

        public List<Button> GetButtons() {
            return _buttons;
        }

        public int GetActiveButtonsCount() {
            return activeFrameCount;
        }


        public void SetWindowColor(int kind) {
            switch (kind)
            {
                //ウィンドウ
                case 0:
                    _backgroundImage.color = _backgroundColor;
                    _frameImage.color = _frameColor;
                    break;
                //暗くする
                case 1:
                    var rectMask2D = _backgroundImage.gameObject.AddComponent<RectMask2D>();
                    rectMask2D.softness = new Vector2Int(0,60);
                    _frameImage.sprite = null;
                    _frameImage.color = new Color(0, 0, 0, 96f/255f);
                    break;
                //透明
                case 2:
                    _backgroundImage.color = new Color(_backgroundColor.r,_backgroundColor.g,_backgroundColor.b, 0.01f);
                    _frameImage.color = new Color(_frameColor.r,_frameColor.g,_frameColor.b, 0.01f);
                    break;

            }
        }
        
        public void ActiveSelectFrame(string text) {
            if (activeFrameCount < _ruckList.Count)
            {
                _ruckList[activeFrameCount].gameObject.SetActive(true);
                _text[activeFrameCount].gameObject.SetActive(true);
                _text[activeFrameCount].text = text;
                activeFrameCount++;
            }
        }

        public void Process(HandleType type) {
            switch (type)
            {
                case HandleType.Up:
                    _nowDigitsCount--;
                    _nowDigitsCount = _nowDigitsCount < 0 ? _numDigits - 1 : _nowDigitsCount;
                    break;
                case HandleType.Down:
                    _nowDigitsCount++;
                    _nowDigitsCount = _nowDigitsCount > _numDigits - 1 ? 0 : _nowDigitsCount;
                    break;
            }
        }

        // ボタンの選択切替
        private void ButtonInteractableChange() {
            var selects = _buttons;
            for (var i = 0; i < selects.Count; i++)
            {
                var nav = selects[i].navigation;
                nav.mode = Navigation.Mode.Explicit;
                nav.selectOnUp = selects[i == 0 ? activeFrameCount - 1 : i - 1];
                nav.selectOnDown = selects[(i + 1) % activeFrameCount];
                nav.selectOnLeft = selects[i == 0 ? activeFrameCount - 1 : i - 1];
                nav.selectOnRight = selects[(i + 1) % activeFrameCount];
                selects[i].navigation = nav;
                
                if (i == _selectNum)
                {
                    selects[i].Select();
                }
            }
        }

        // 選択番号の取得
        public int GetSelectNum() {
            return _selectNum;
        }
        
        public void SetSelectNum(int num) {
            _selectNum = num;
            if (_isArrowEnabled)
            {
                _arrowUp.gameObject.SetActive(_selectNum != 0);
                _arrowDown.gameObject.SetActive(_selectNum + 1 != activeFrameCount);
            }

            ButtonInteractableChange();
        }

        private void ArrowUp() {
            //カーソル移動のSE鳴動
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, DataManager.Self().GetSystemDataModel().soundSetting.cursor);
            SoundManager.Self().PlaySe();
            SetSelectNum(GetSelectNum() - 1);
        }
        private void ArrowDown() {
            //カーソル移動のSE鳴動
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, DataManager.Self().GetSystemDataModel().soundSetting.cursor);
            SoundManager.Self().PlaySe();
            SetSelectNum(GetSelectNum() + 1);
        }
        
        private void ButtonClick() {
            // 別の場所で決定処理を行っているため処理は特に無し
        }
    }
}