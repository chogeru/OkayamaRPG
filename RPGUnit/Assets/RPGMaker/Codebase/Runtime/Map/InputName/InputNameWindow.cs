using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Party;
using RPGMaker.Codebase.Runtime.Common.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Map.InputName
{
    public class InputNameWindow : WindowBase
    {
        /// <summary>
        /// モード変更時の動作指定
        /// </summary>
        private enum ModeChangeCode
        {
            Increment,// 次へ
            Decrement,// 戻る
            None,// なにもしない（初期化時など）
        }

        //顔画像のファイルパス
        private readonly string FacePath = "Assets/RPGMaker/Storage/Images/Faces/";
        private const int MAX_INPUT_LENGTH = 16;

        private readonly string[][] LATIN = new string[][]
        {
            new string[]{
                "A", "B", "C", "D", "E", "a", "b", "c", "d", "e",
                "F", "G", "H", "I", "J", "f", "g", "h", "i", "j",
                "K", "L", "M", "N", "O", "k", "l", "m", "n", "o",
                "P", "Q", "R", "S", "T", "p", "q", "r", "s", "t",
                "U", "V", "W", "X", "Y", "u", "v", "w", "x", "y",
                "Z", "[", "]", "^", "_", "z", "{", "}", "|", "~",
                "0", "1", "2", "3", "4", "!", "#", "$", "%", "&",
                "5", "6", "7", "8", "9", "(", ")", "*", "+", "-",
                "/", "=", "@", "<", ">", ":", ";", " ", "PG", "OK"
            },
            new string[]{
                "Á", "É", "Í", "Ó", "Ú", "á", "é", "í", "ó", "ú",
                "À", "È", "Ì", "Ò", "Ù", "à", "è", "ì", "ò", "ù",
                "Â", "Ê", "Î", "Ô", "Û", "â", "ê", "î", "ô", "û",
                "Ä", "Ë", "Ï", "Ö", "Ü", "ä", "ë", "ï", "ö", "ü",
                "Ā", "Ē", "Ī", "Ō", "Ū", "ā", "ē", "ī", "ō", "ū",
                "Ã", "Å", "Æ", "Ç", "Ð", "ã", "å", "æ", "ç", "ð",
                "Ñ", "Õ", "Ø", "Š", "Ŵ", "ñ", "õ", "ø", "š", "ŵ",
                "Ý", "Ŷ", "Ÿ", "Ž", "Þ", "ý", "ÿ", "ŷ", "ž", "þ",
                "Ĳ", "Œ", "ĳ", "œ", "ß", "«", "»", "BS", "PG", "OK"
            }
        };

        private readonly string[][] JAPAN = new string[][]
        {
            new string[]
            {
                "あ", "い", "う", "え", "お", "が", "ぎ", "ぐ", "げ", "ご",
                "か", "き", "く", "け", "こ", "ざ", "じ", "ず", "ぜ", "ぞ",
                "さ", "し", "す", "せ", "そ", "だ", "ぢ", "づ", "で", "ど",
                "た", "ち", "つ", "て", "と", "ば", "び", "ぶ", "べ", "ぼ",
                "な", "に", "ぬ", "ね", "の", "ぱ", "ぴ", "ぷ", "ぺ", "ぽ",
                "は", "ひ", "ふ", "へ", "ほ", "ぁ", "ぃ", "ぅ", "ぇ", "ぉ",
                "ま", "み", "む", "め", "も", "っ", "ゃ", "ゅ", "ょ", "ゎ",
                "や", "ゆ", "よ", "わ", "ん", "ー", "～", "・", "＝", "☆",
                "ら", "り", "る", "れ", "ろ", "ゔ", "を", "　", "PG", "OK"
            },
            new string[]
            {
                "ア", "イ", "ウ", "エ", "オ", "ガ", "ギ", "グ", "ゲ", "ゴ",
                "カ", "キ", "ク", "ケ", "コ", "ザ", "ジ", "ズ", "ゼ", "ゾ",
                "サ", "シ", "ス", "セ", "ソ", "ダ", "ヂ", "ヅ", "デ", "ド",
                "タ", "チ", "ツ", "テ", "ト", "バ", "ビ", "ブ", "ベ", "ボ",
                "ナ", "ニ", "ヌ", "ネ", "ノ", "パ", "ピ", "プ", "ペ", "ポ",
                "ハ", "ヒ", "フ", "ヘ", "ホ", "ァ", "ィ", "ゥ", "ェ", "ォ",
                "マ", "ミ", "ム", "メ", "モ", "ッ", "ャ", "ュ", "ョ", "ヮ",
                "ヤ", "ユ", "ヨ", "ワ", "ン", "ー", "～", "・", "＝", "☆",
                "ラ", "リ", "ル", "レ", "ロ", "ヴ", "ヲ", "　", "PG", "OK"
            },
            new string[]
            {
                "Ａ", "Ｂ", "Ｃ", "Ｄ", "Ｅ", "ａ", "ｂ", "ｃ", "ｄ", "ｅ",
                "Ｆ", "Ｇ", "Ｈ", "Ｉ", "Ｊ", "ｆ", "ｇ", "ｈ", "ｉ", "ｊ",
                "Ｋ", "Ｌ", "Ｍ", "Ｎ", "Ｏ", "ｋ", "ｌ", "ｍ", "ｎ", "ｏ",
                "Ｐ", "Ｑ", "Ｒ", "Ｓ", "Ｔ", "ｐ", "ｑ", "ｒ", "ｓ", "ｔ",
                "Ｕ", "Ｖ", "Ｗ", "Ｘ", "Ｙ", "ｕ", "ｖ", "ｗ", "ｘ", "ｙ",
                "Ｚ", "［", "］", "＾", "＿", "ｚ", "｛", "｝", "｜", "～",
                "０", "１", "２", "３", "４", "！", "＃", "＄", "％", "＆",
                "５", "６", "７", "８", "９", "（", "）", "＊", "＋", "－",
                "／", "＝", "＠", "＜", "＞", "：", "；", "BS", "PG", "OK"
            }
        };

        /// <summary>
        /// キーボタン管理の中間クラス
        /// </summary>
        class Key
        {
            public Text _textComponent;
            public Button _buttonComponent;
            public WindowButtonBase _wbbaseComponent;

            public Key(Text t, Button bt, WindowButtonBase wb) {
                _textComponent = t;
                _buttonComponent = bt;
                _wbbaseComponent = wb;
            }
        }

        [SerializeField] private GameObject _charaItems;
        [SerializeField] private GameObject _DecisionNameButton;
        [SerializeField] private GameObject _keyItems;
        [SerializeField] private GameObject Display;
        [SerializeField] private GameObject Face;
        [SerializeField] private GameObject KeyBoard;

        private Action _endAction;
        private RuntimeActorDataModel mActorModel = default;

        private string _actorName;
        private int _maxInputLength;
        private int _nowMode = 0;

        private List<Text> _charas;
        private List<Key> _keys = new List<Key>();

        public void Init(Action endAction, string actorid, int maxCount) {

            _endAction = endAction;

            //このタイミングで、まだセーブデータにアクターデータが存在しなかった場合、生成する
            mActorModel = DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels.FirstOrDefault(a => a.actorId == actorid);
            if (mActorModel == default)
            {
                //存在しないため新規作成
                var partyChange = new PartyChange();
                mActorModel = partyChange.SetActorData(actorid);
            }

            // 選択されたアクターの情報取得
            _maxInputLength = Math.Min(maxCount, MAX_INPUT_LENGTH);
            _actorName = mActorModel.name;
            if (_actorName.Length > _maxInputLength)
            {
                _actorName = _actorName.Substring(0, _maxInputLength);
            }
            // 顔の読み込み
            Face.GetComponent<Image>().sprite = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(FacePath + mActorModel.faceImage + ".png");

            // 入力した文字の表示部分
            _charas = _charaItems.GetComponentsInChildren<Text>().ToList();
            for (var i = 0; i < _charas.Count; i++)
            {
                _charas[i].gameObject.SetActive(i < _maxInputLength);
            }

            // キーボード部分
            var buttons = KeyBoard.GetComponentsInChildren<Button>().ToList();
            buttons.ForEach(bt =>
            {
                var text = bt.GetComponentInChildren<Text>();
                var wb = bt.GetComponent<WindowButtonBase>();
                _keys.Add(new Key(text, bt, wb));
            });

            Display.SetActive(true);
            KeyBoard.SetActive(true);

            UpdateCharaName();
            ModeChange(ModeChangeCode.None);

            //ナビゲーション設定
            for (var i = 0; i < _keys.Count; i++)
            {
                var select = _keys[i]._buttonComponent.GetComponent<Selectable>();
                var nav = _keys[i]._buttonComponent.navigation;
                nav.mode = Navigation.Mode.Explicit;
                var x = i % 10;
                var y = i / 10;
                nav.selectOnRight = _keys[y * 10 + (x + 1) % 10]._buttonComponent;
                nav.selectOnLeft = _keys[y * 10 + (x + 9) % 10]._buttonComponent;
                nav.selectOnDown = _keys[((y + 1) % 9) * 10 + x]._buttonComponent;
                nav.selectOnUp = _keys[((y + 8) % 9) * 10 + x]._buttonComponent;
                select.navigation = nav;
            }

            //フォーカス初期位置
            for (var i = 0; i < _keys.Count; i++)
            {
                _keys[i]._wbbaseComponent.SetEnabled(true);
                _keys[i]._wbbaseComponent.SetHighlight(false);
            }
            _keys[0]._buttonComponent.Select();

            // 処理登録
            InputDistributor.AddInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT, HandleType.Back, DeleteName);
            TimeHandler.Instance.AddTimeActionEveryFrame(KeyController);
        }

        /// <summary>
        /// 文字入力の受付
        /// </summary>
        /// <param name="obj"></param>
        public void ClickItemButton(GameObject obj) {

            // 88番ボタンにバックスペースを入れる
            // プレハブの更新をするとMigrationが必要になるためコードで対応
            var key = _keys.FirstOrDefault(k => k._buttonComponent.gameObject == obj);
            if (key._textComponent.text == "BS")
            {
                DeleteName();
            }
            else if (_maxInputLength > _actorName.Length)
            {
                _actorName += key._textComponent.text;
                UpdateCharaName();
            }
        }

        /// <summary>
        /// 最後の文字から削除
        /// </summary>
        private void DeleteName() {
            if (_actorName.Length > 0)
            {
                _actorName = _actorName.Substring(0, _actorName.Length - 1);
                UpdateCharaName();
            }
        }

        /// <summary>
        /// 決定キーの入力
        /// </summary>
        /// <param name="obj"></param>
        public void ClickDecisionNameButton(GameObject obj) {
            var saveData = DataManager.Self().GetRuntimeSaveDataModel();
            //空文字だったらデフォルトの名前に戻す
            if (_actorName == "")
            {
                _actorName = mActorModel.name;
                UpdateCharaName();
            }
            else
            {
                // 名前を更新して終了
                mActorModel.name = _actorName;
                Display.SetActive(false);
                KeyBoard.SetActive(false);
                InputDistributor.RemoveInputHandler(GameStateHandler.IsMap() ? GameStateHandler.GameState.EVENT : GameStateHandler.GameState.BATTLE_EVENT, HandleType.Back, DeleteName);
                TimeHandler.Instance.RemoveTimeAction(KeyController);
                _endAction.Invoke();
            }
        }

        /// <summary>
        /// 入力文字変更
        /// </summary>
        /// <param name="obj"></param>
        public void ClickModeChangeButton(GameObject obj) {
            InputModeChange();
        }

        /// <summary>
        /// キャラ名の表示部分更新
        /// </summary>
        private void UpdateCharaName() {
            // 一回クリア
            _charas.ForEach(c => c.text = "");
            // 再セット
            for (var i = 0; i < _actorName.Length; i++) _charas[i].text = _actorName.Substring(i, 1);
        }

        /// <summary>
        /// キーボードの切替
        /// </summary>
        /// <param name="change"></param>
        private void ModeChange(ModeChangeCode change) {

            string[][] languageIndex = null;

#if UNITY_EDITOR
            var systemLanguage = (SystemLanguage) typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.LocalizationDatabase").GetProperty("currentEditorLanguage").GetValue(null);
            if (systemLanguage == SystemLanguage.Japanese)
#else
            if (Application.systemLanguage == SystemLanguage.Japanese)
#endif
            {
                languageIndex = JAPAN;
            }
            else
            {
                languageIndex = LATIN;
            }

            var length = languageIndex.Length;
            if (change == ModeChangeCode.Increment)
            {
                _nowMode = ++_nowMode % length;
            }
            if (change == ModeChangeCode.Decrement)
            {
                // 加算して剰余を取ることでラップラウンドする
                _nowMode = (_nowMode + length - 1) % length;
            }
            //キーボードの文字盤の切り替え
            var str = languageIndex[_nowMode];
            for (var i = 0; i < _keys.Count; i++)
            {
                _keys[i]._textComponent.text = str[i];
            }
        }

        /// <summary>
        /// UIからのPageボタン入力処理
        /// デクリメント固定
        /// </summary>
        private void InputModeChange() {
            ModeChange(ModeChangeCode.Increment);
        }

        /// <summary>
        /// キーボードからのPage操作処理
        /// </summary>
        private void KeyController() {

            if (InputHandler.OnDown(HandleType.PageLeft))
            {
                ModeChange(ModeChangeCode.Decrement);
            }
            else if (InputHandler.OnDown(HandleType.PageRight))
            {
                ModeChange(ModeChangeCode.Increment);
            }
        }
    }
}