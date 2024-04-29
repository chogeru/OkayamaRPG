using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.CharacterActor;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Common.ControlCharacter
{
    public class ControlCharacter : MonoBehaviour
    {
        public Processer processer;

        private (GameObject parent,
            string messageText,
            string fontName,
            int fontSize,
            Color fontColor,
            GameObject goldWindow,
            bool isAllSkip) initializeParams;

        public bool IsWaitForButtonInput => processer.IsWaitForButtonInput;
        public bool IsNotWaitInput => processer.IsNotWaitInput || processer.IsNotWaitInputNextCommand;
        public bool IsNextPage => processer.IsNextPage;
        public bool IsNextPageControlCharacter => processer.IsNextPageControlCharacter;
        public bool IsEnd => processer.IsEnd;

        public void SetIsNotWaitCommand(bool flg) {
            processer.SetIsNotWaitInput(flg);
        }

        public void InitControl(
            GameObject parent,
            string messageText,
            string fontName,
            int fontSize,
            Color fontColor,
            GameObject goldWindow,
            bool isAllSkip
        ) {
            initializeParams =
                (parent,
                    messageText,
                    fontName,
                    fontSize,
                    fontColor,
                    goldWindow,
                    isAllSkip);

            NewProcessor();
        }

        public void ExecCharacter() {
            processer.ExecCharacter();
        }
        
        /// <summary>
        /// 名前の項目部分
        /// </summary>
        public void ExecCharacterByName() {
            processer.ExecCharacterByName();
        }
        public void ExecEditor() {
            NewProcessor();
            processer.ExecEditor();
        }

        /// <summary>
        /// 名前の項目部分
        /// </summary>
        public void ExecEditorByName() {
            NewProcessor();
            processer.ExecEditorByName();
        }

        public void Destroy() {
            if (processer != null)
            {
                processer.Destroy();
                processer = null;
            }
        }

        public void ResetForBattleMessageWindow(string text, bool isDeleted = true) {
            processer.ResetForBattleMessageWindow(text, isDeleted);
        }

        private void NewProcessor() {
            if (processer?.IsDirty == false)
            {
                return;
            }

            if (processer?.IsDirty == true)
            {
                Destroy();
            }

            processer = new Processer(this);
            processer.InitControl(
                initializeParams.parent,
                initializeParams.messageText,
                initializeParams.fontName,
                initializeParams.fontSize,
                initializeParams.fontColor,
                initializeParams.goldWindow,
                initializeParams.isAllSkip);
        }

        public class Processer
        {
            private const int RowSpacing = 8;
            private const float TextHeightCoefficient = 0.904f;

            private static readonly Color[] WindowColors =
            {
                new Color32(255, 255, 255, 255),
                new Color32(32, 160, 214, 255),
                new Color32(254, 120, 76, 255),
                new Color32(102, 204, 64, 255),
                new Color32(153, 204, 255, 255),
                new Color32(204, 192, 255, 255),
                new Color32(255, 255, 160, 255),
                new Color32(128, 128, 128, 255),
                new Color32(192, 192, 192, 255),
                new Color32(32, 128, 204, 255),
                new Color32(255, 56, 16, 255),
                new Color32(0, 160, 16, 255),
                new Color32(62, 154, 222, 255),
                new Color32(160, 152, 255, 255),
                new Color32(255, 204, 32, 255),
                new Color32(0, 0, 0, 255),
                new Color32(132, 170, 255, 255),
                new Color32(255, 255, 64, 255),
                new Color32(255, 32, 32, 255),
                new Color32(32, 32, 64, 255),
                new Color32(224, 128, 64, 255),
                new Color32(240, 192, 64, 255),
                new Color32(64, 128, 192, 255),
                new Color32(64, 192, 240, 255),
                new Color32(128, 255, 128, 255),
                new Color32(192, 128, 128, 255),
                new Color32(128, 128, 255, 255),
                new Color32(255, 128, 255, 255),
                new Color32(0, 160, 64, 255),
                new Color32(0, 224, 96, 255),
                new Color32(160, 96, 224, 255),
                new Color32(192, 128, 255, 255)
            };

            private static readonly Regex ControllCharRegex = new (
                @"^
                (?<Name>(V|N|PX|PY|P|C|I|FS))\[(?<Param>\d+)\]|     # パラメータありの制御文字。
                (?<Name>(G|{|}|\\|\$|\.|\||<|>|!|\^|f|[!-~A-Z]*))   # パラメータなしの制御文字。
                ",

            RegexOptions.IgnorePatternWhitespace);

            private readonly ControlCharacter controlCharacter;

            private        GameObject _rootObj;
            private        GameObject _goldWindow;
            private Text _originText;
            private static Font       _originFont;
            private        string     _messageText;
            private        string     _fontName;
            FontSize                  _fontSize;
            private Color             _fontColor = Color.white;

            // バトルの画面上部に表示するメッセージ表示。制御文字はエスケープするが、機能は働かない
            private bool _isAllSkip = false;

            private List<GameObject> _gameObjects = new ();
            private List<Text>       _texts       = new ();

            private int     _charIndex;
            private Vector2 position = Vector2.zero;
            private RowInfo rowInfo;

            // 同じ行の残りの文字を一瞬で表示中？
            private bool _isSkipToLineEnd = false;
            // 文字を表示中でも、待つがあるか？
            private bool _isSkipToLineWait = false;
            // 名前部分の文字列
            private bool _isNameToLine = false;


            // ボタン入力を待ち中？
            private bool _isWaitForButtonInput = false;

            // 文章表示後の入力待ちをしない。
            private bool _isNotWaitInput            = false;
            private bool _isNotWaitInputNextCommand = false;

            private bool _isNextPage = false;
            private bool _isNextPageControlCharacter = false;
            private bool _isEnd      = false;

            public Processer(ControlCharacter controlCharacter) {
                this.controlCharacter = controlCharacter;
            }

            public bool IsDirty { get; private set; }

            public bool IsWaitForButtonInput => _isWaitForButtonInput;
            public bool IsNotWaitInput => _isNotWaitInput;
            public bool IsNotWaitInputNextCommand => _isNotWaitInputNextCommand;
            public bool IsNextPage => _isNextPage;
            public bool IsNextPageControlCharacter => _isNextPageControlCharacter;
            public bool IsEnd => _isEnd;
            public int NowIndex => _charIndex;

            private Vector2 _windowRectSize => controlCharacter.GetComponent<RectTransform>().rect.size;

            public void InitControl(
                GameObject parent,
                string messageText,
                string fontName,
                int fontSize,
                Color fontColor,
                GameObject goldWindow,
                bool isAllSkip
            ) {
                DebugUtil.LogMethodParameters(parent, messageText, fontName, fontSize, fontColor, goldWindow,
                    isAllSkip);

                IsDirty = false;

                _rootObj = parent;
                _messageText = messageText.Replace("\r\n", "\n");
                _fontName = fontName;
                _fontSize = new FontSize(fontSize);
                _fontColor = fontColor;
                _goldWindow = goldWindow;
                _originText = parent.transform.parent.transform.Find("OriginText").GetComponent<Text>();
                _isAllSkip = isAllSkip;

                if (_originFont == null)
                {
                    _originFont = Font.CreateDynamicFontFromOSFont("message", _fontSize.ComponentFontSize);
                }

                _isEnd = false;
                _isWaitForButtonInput = false;

                _charIndex = 0;
            }

            public void ExecCharacter() {
                DebugUtil.LogMethod();

                IsDirty = true;

                _isEnd = false;
                _isWaitForButtonInput = false;
                _isNotWaitInput = false;
                _isNameToLine = false;
                TimeHandler.Instance.AddTimeActionEveryFrame(ExecFrame);
            }
            /// <summary>
            /// 名前の項目部分
            /// </summary>
            public void ExecCharacterByName() {
                DebugUtil.LogMethod();

                IsDirty = true;

                _isEnd = false;
                _isWaitForButtonInput = false;
                _isNotWaitInput = false;
                _isNameToLine = true;
                for (_charIndex = 0; _charIndex < _messageText.Length;)
                {
                    ExecParseChar(ref _messageText, ref _charIndex, _isAllSkip, isEditorPreview: false);
                }

            }
            private void ExecWaitFrame() {
                DebugUtil.LogMethod();

                if (_isSkipToLineWait)
                {
                    _isSkipToLineWait = false;
                    _isSkipToLineEnd = true;
                }
                TimeHandler.Instance.RemoveTimeAction(ExecWaitFrame);
                TimeHandler.Instance.AddTimeActionEveryFrame(ExecFrame);
            }

            private void ExecFrame() {
                DebugUtil.LogMethod();

                do
                {
                    if (_charIndex >= _messageText.Length)
                    {
                        TimeHandler.Instance.RemoveTimeAction(ExecFrame);
                        _isEnd = true;
                        return;
                    }

                    try
                    {
                        if (!(_rootObj != null && _rootObj.transform != null && _rootObj.transform.gameObject != null))
                        {
                            TimeHandler.Instance.RemoveTimeAction(ExecFrame);
                            return;
                        }
                    }
                    catch (System.Exception)
                    {
                        TimeHandler.Instance.RemoveTimeAction(ExecFrame);
                        return;
                    }

                    ExecParseChar(ref _messageText, ref _charIndex, _isAllSkip, isEditorPreview: false);
                } while (_isSkipToLineEnd);
            }

            public void ExecEditor() {
                DebugUtil.LogMethod();

                _isNameToLine = false;
                IsDirty = true;

                ResetForExecEditor();
                for (_charIndex = 0; _charIndex < _messageText.Length;)
                {
                    ExecParseChar(ref _messageText, ref _charIndex, _isAllSkip, isEditorPreview: true);
                }
            }

            /// <summary>
            /// 名前の項目部分
            /// </summary>
            public void ExecEditorByName() {
                DebugUtil.LogMethod();
                _isNameToLine = true;
                IsDirty = true;

                ResetForExecEditor();
                for (_charIndex = 0; _charIndex < _messageText.Length;)
                {
                    ExecParseChar(ref _messageText, ref _charIndex, _isAllSkip, isEditorPreview: true);
                }

            }

            /// <summary>
            /// 表示前に表示行1行分をパースして、改ページが必要かと表示縦幅を取得する。
            /// </summary>
            /// <param name="isEditorPreview">エディターのプレビュー表示の為に呼ばれたフラグ。</param>
            private void ParseRow(bool isSkipPxPyUntilPutObject, bool isEditorPreview) {
                DebugUtil.LogMethodParameters(isEditorPreview);

                // 使用しないものをnullに設定。
                _rootObj = null;
                _goldWindow = null;
                _gameObjects = null;
                _texts = null;

                rowInfo = new RowInfo(isSkipPxPyUntilPutObject);

                while (_charIndex < _messageText.Length && rowInfo.IsParsing)
                {
                    ExecParseChar(ref _messageText, ref _charIndex, _isAllSkip, isEditorPreview);
                }

                // 表示テキスト末端の表示行パース末端処理。
                if (rowInfo.IsParsing)
                {
                    ParseRowTermProcess();
                }
            }

            /// <summary>
            /// 1文字 (1制御文字列含む) の処理。
            /// </summary>
            /// <param name="text">表示テキスト。</param>
            /// <param name="charIndex">表示テキストの文字インデックス。</param>
            /// <param name="isAllSkip">制御文字をパースはするが処理は行わない。</param>
            /// <param name="isEditorPreview">エディタープレビュー表示から呼ばれた。</param>
            /// <remarks>
            /// 制御文字:
            /// 
            /// 【置換系】
            /// 
            /// \V[n]   変数n番の値に置き換えられます。
            /// \N[n]   アクターn番の名前に置き換えられます。
            /// \P[n]   パーティメンバーn番に置き換えられます。
            /// \I[n]   アイコンn番を描画します。
            /// \G      通貨単位に置き換えられます。
            /// \\      バックスラッシュに置き換えられます。
            /// 
            /// 【設定系】
            /// 
            /// \C[n]   以降の文字をn番の色で表示します。
            /// \PX[n]  ウィンドウの左上を原点にX座標を設定します。
            /// \PY[n]  ウィンドウの左上を原点にY座標を設定します。
            /// \FS[n]  文字サイズをnに変更します。
            /// \{      文字サイズを1段階大きくします。
            /// \}      文字サイズを1段階小さくします。
            /// 
            /// 【動作系】
            /// 
            /// \!      ボタン入力を待ちます。
            /// \.      1/4秒待ちます。
            /// \|      1秒待ちます。
            /// \>      同じ行の残りの文字を一瞬で表示します。
            /// \<      文字を一瞬で表示する効果を取り消します。
            /// \^      文章表示後の入力待ちをしません。
            ///
            /// \f      改ページをします (バトルの場合のみ)。Uniteで追加されたもの？
            /// 
            /// 【他】
            /// 
            /// \$      所持金のウィンドウを開きます。
            /// </remarks>
            private void ExecParseChar(
                ref string text,
                ref int charIndex,
                bool isAllSkip,
                bool isEditorPreview
            ) {
                // 処理するオブジェクト。
                object objectToProcess = null;

                do
                {
                    // 改ページでのリセット処理？
                    if (rowInfo?.RequestForPageBreak == RowInfo.Request.Reset && !isEditorPreview)
                    {
                        rowInfo.RequestForPageBreak = RowInfo.Request.None;

                        ResetForPageBreak();

                        // isSkipPxPyUntilPutObject = true の状態で、再度表示行パースを行う。
                        rowInfo = RowInfo.PreParseRow(this, isSkipPxPyUntilPutObject: true, isEditorPreview);
                        rowInfo.IsSkipPxPyUntilPutObject = true;
                    }

                    // 表示前に表示行の1行分をパースしてその結果を返す。
                    rowInfo ??= RowInfo.PreParseRow(this, isSkipPxPyUntilPutObject: false, isEditorPreview);

                    // 改ページ前の入力待ち？
                    if (rowInfo.RequestForPageBreak == RowInfo.Request.WaitButtonInput && !isEditorPreview)
                    {
                        rowInfo.RequestForPageBreak = RowInfo.Request.Reset;

                        TimeHandler.Instance.RemoveTimeAction(ExecFrame);
                        _isWaitForButtonInput = true;
                        objectToProcess = true;

                        //バトル中の場合には、次のページを表示することとする
                        if (GameStateHandler.IsBattle())
                        {
                            _isNextPage = true;
                        }

                        // 入力待ちを行わない場合は、次のシーケンスに進む
                        if (_isNotWaitInput)
                        {
                            ExecCharacter();
                        }

                        break;
                    }

                    switch (text[charIndex])
                    {
                        // 行末改行。
                        case '\n':
                        {
                            charIndex++;

                            if (LineBreakProcess())
                            {
                                return;
                            }

                            if (_isSkipToLineEnd && !isEditorPreview)
                            {
                                _isSkipToLineEnd = false;
                                ExecCharacter();
                            }

                            break;
                        }

                        // 制御文字。
                        case '\\':
                        {
                            charIndex++;

                            var match = ControllCharRegex.Match(text[charIndex..]);
                            DebugUtil.Assert(match.Success);
                            DebugUtil.Log($"#### [{charIndex - 1}]:制御文字【{GetControlCharacterString(match)}】");

                            charIndex += match.Value.Length;

                            // 置換系、設定系は無効か？
                            var disableReplacementOrSetting = isAllSkip && !isEditorPreview;

                            // その他は無効か？
                            var disableOther = isAllSkip && !isEditorPreview || rowInfo.IsParsing;


                            // 動作系は無効か？
                            var disableOperating = isAllSkip || isEditorPreview || rowInfo.IsParsing;
                            if (_isNameToLine)
                            {
                                disableOther = true;
                                disableOperating = true;
                            }

                            switch (match.Groups["Name"].Value)
                            {
                                // [置換系] \V[n] 変数n番の値に置き換えられます。
                                case "V":
                                {
                                    if (disableReplacementOrSetting)
                                    {
                                        break;
                                    }

                                    var values = DataManager.Self().GetRuntimeSaveDataModel()?.variables?.data;
                                    var valueIndex = GetParam(match) - 1;
                                    text = text.Insert(
                                        charIndex,
                                        valueIndex >= 0 && valueIndex < values?.Count
                                            ? values[valueIndex].ToString()
                                            : "0");
                                    break;
                                }

                                // [置換系] \N[n] アクターn番の名前に置き換えられます。
                                case "N":
                                {
                                    if (disableReplacementOrSetting)
                                    {
                                        break;
                                    }

                                    var serialNumber = GetParam(match);
                                    var characterActor =
                                        DataManager.Self().GetActorDataModels().Find(
                                            actor =>
                                                (ActorTypeEnum) actor.charaType == ActorTypeEnum.ACTOR &&
                                                actor.SerialNumber == serialNumber);
                                    var actorName = "?";

                                    // Runtimeの場合
                                    if (characterActor != null)
                                    {
                                        if (!isEditorPreview)
                                        {
                                            bool flg = false;
                                            var gameActor = DataManager.Self().GetRuntimeSaveDataModel();
                                            for (int i = 0; i < gameActor?.runtimeActorDataModels.Count; i++)
                                            {
                                                if (gameActor.runtimeActorDataModels[i].actorId == characterActor.uuId)
                                                {
                                                    actorName = gameActor.runtimeActorDataModels[i].name;
                                                    flg = true;
                                                    break;
                                                }
                                            }
                                            if (!flg)
                                            {
                                                //アクターIDが指定されているが、RuntimeActorDataModelには存在しない
                                                //このケースでは、マスタデータのものを使用する
                                                actorName = characterActor.basic.name;
                                            }
                                        }
                                        else
                                        {
                                            var actor = DataManager.Self().GetActorDataModel(characterActor.uuId);
                                            actorName = actor.basic.name;
                                        }
                                    }
                                    actorName = actorName.Replace("\\", "\\\\");
                                    
                                    text = text.Insert(charIndex, actorName);
                                    break;
                                }

                                // [置換系] \P[n]  パーティメンバーn番に置き換えられます。
                                case "P":
                                {
                                    if (disableReplacementOrSetting)
                                    {
                                        break;
                                    }

                                    var partyMemberIds = !isEditorPreview
                                        ? DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.actors
                                        : DataManager.Self().GetSystemDataModel().initialParty.party;
                                    var partyMenberIndex = GetParam(match) - 1;
                                    var s = "?";
                                    if (partyMenberIndex >= 0 && partyMenberIndex < partyMemberIds.Count)
                                    {
                                        // Runtimeの場合
                                        if (!isEditorPreview)
                                        {
                                            var gameActor = DataManager.Self().GetRuntimeSaveDataModel();
                                            for (int i = 0; i < gameActor?.runtimeActorDataModels.Count; i++)
                                            {
                                                if (gameActor.runtimeActorDataModels[i].actorId ==
                                                    partyMemberIds[partyMenberIndex])
                                                {
                                                    s = gameActor.runtimeActorDataModels[i].name;
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var actor = DataManager.Self().GetActorDataModel(partyMemberIds[partyMenberIndex]);
                                            s = actor.name;
                                        }
                                    }
                                    s = s.Replace("\\", "\\\\");


                                    text = text.Insert(charIndex, s);
                                    break;
                                }

                                // [置換系] \I[n] アイコンn番を描画します。
                                case "I":
                                {
                                    if (disableReplacementOrSetting)
                                    {
                                        break;
                                    }

                                    objectToProcess = LoadIcon(GetParam(match));
                                    if (objectToProcess == null)
                                    {
                                        objectToProcess = LoadIcon(0);
                                    }

                                    static Texture2D LoadIcon(int iconNumber) {
                                        return UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(
                                            $"Assets/RPGMaker/Storage/Images/System/IconSet/IconSet_{iconNumber:000}.png");
                                    }

                                    break;
                                }

                                // [置換系] \G 通貨単位に置き換えられます。
                                case "G":
                                {
                                    text = text.Insert(charIndex, GetMoneyWord());
                                    break;
                                }

                                // [置換系] \\ バックスラッシュに置き換えられます。
                                case "\\":
                                {
                                    objectToProcess = '\\';
                                    break;
                                }

                                // [設定系] \C[n] 以降の文字をn番の色で表示します。
                                case "C":
                                {
                                    if (disableReplacementOrSetting)
                                    {
                                        break;
                                    }

                                    var colorIndex = GetParam(match);
                                    _fontColor = colorIndex < WindowColors.Length
                                        ? WindowColors[colorIndex]
                                        : Color.black;
                                    break;
                                }

                                // [設定系] \PX[n] ウィンドウの左上を原点にX座標を設定します。
                                case "PX":
                                {
                                    if (disableReplacementOrSetting || rowInfo.IsSkipPxPyUntilPutObject)
                                    {
                                        break;
                                    }

                                    position.x = GetParam(match);
                                    break;
                                }

                                // [設定系] \PY[n] ウィンドウの左上を原点にY座標を設定します。
                                case "PY":
                                {
                                    if (disableReplacementOrSetting || rowInfo.IsSkipPxPyUntilPutObject)
                                    {
                                        break;
                                    }

                                    position.y = GetParam(match);
                                    break;
                                }

                                // [設定系] \FS[n] 文字サイズをnに変更します。
                                case "FS":
                                {
                                    if (disableReplacementOrSetting)
                                    {
                                        break;
                                    }

                                    _fontSize.UniteFontSize = GetParam(match);
                                    break;
                                }

                                // [設定系] \{ 文字サイズを1段階大きくします。
                                case "{":
                                {
                                    if (disableReplacementOrSetting)
                                    {
                                        break;
                                    }

                                    _fontSize.MakeLarger();
                                    break;
                                }

                                // [設定系] \} 文字サイズを1段階小さくします。
                                case "}":
                                {
                                    if (disableReplacementOrSetting)
                                    {
                                        break;
                                    }

                                    _fontSize.MakeSmaller();
                                    break;
                                }

                                // [動作系] \! ボタン入力を待ちます。
                                case "!":
                                {
                                    if (disableOperating)
                                    {
                                        break;
                                    }

                                    TimeHandler.Instance.RemoveTimeAction(ExecFrame);
                                    _isWaitForButtonInput = true;
                                    objectToProcess = true;
                                    break;
                                }

                                // [動作系] \. 1/4秒待ちます。
                                case ".":
                                {
                                    if (disableOperating)
                                    {
                                        break;
                                    }

                                    TimeHandler.Instance.RemoveTimeAction(ExecFrame);
                                    //一瞬表示が有効だった場合、待つ時だけ無効にする
                                    if (_isSkipToLineEnd)
                                    {
                                        _isSkipToLineEnd = false;
                                        _isSkipToLineWait = true;
                                    }
                                    TimeHandler.Instance.AddTimeAction(0.25f, ExecWaitFrame, false);
                                    objectToProcess = true;
                                    break;
                                }

                                // [動作系] \| 1秒待ちます。
                                case "|":
                                {
                                    if (disableOperating)
                                    {
                                        break;
                                    }

                                    TimeHandler.Instance.RemoveTimeAction(ExecFrame);
                                    //一瞬表示が有効だった場合、待つ時だけ無効にする
                                    if (_isSkipToLineEnd)
                                    {
                                        _isSkipToLineEnd = false;
                                        _isSkipToLineWait = true;
                                    }
                                    TimeHandler.Instance.AddTimeAction(1f, ExecWaitFrame, false);
                                    objectToProcess = true;
                                    break;
                                }

                                // [動作系] \> 同じ行の残りの文字を一瞬で表示します。
                                case ">":
                                {
                                    if (disableOperating)
                                    {
                                        break;
                                    }

                                    _isSkipToLineEnd = true;
                                    break;
                                }

                                // [動作系] \< 文字を一瞬で表示する効果を取り消します。
                                case "<":
                                {
                                    if (disableOperating)
                                    {
                                        break;
                                    }

                                    _isSkipToLineEnd = false;
                                    break;
                                }

                                // [動作系] \^ 文章表示後の入力待ちをしません。
                                case "^":
                                {
                                    if (disableOperating)
                                    {
                                        break;
                                    }

                                    _isNotWaitInput = true;
                                    break;
                                }

                                // [動作系] \f 改ページをします (バトルの場合のみ)
                                case "f":
                                {
                                    if (!GameStateHandler.IsBattle() || isEditorPreview)
                                    {
                                        break;
                                    }

                                    TimeHandler.Instance.RemoveTimeAction(ExecFrame);
                                    _isNextPage = true;
                                    _isNextPageControlCharacter = true;
                                    objectToProcess = true;
                                    break;
                                }

                                // [他] \$ 所持金のウィンドウを開きます。
                                case "$":
                                {
                                    if (disableOther)
                                    {
                                        break;
                                    }

                                    if (_goldWindow != null)
                                    {
                                        _goldWindow.SetActive(true);

                                        int gold = !isEditorPreview
                                            ? DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.gold
                                            : 0;
                                        _goldWindow.transform.Find("MenuItems/NowGold")
                                            .GetComponent<TMPro.TextMeshProUGUI>().text = gold.ToString();

                                        _goldWindow.transform.Find("MenuItems/Currency").GetComponent<Text>().text =
                                            GetMoneyWord();
                                    }

                                    break;
                                }

                                // [他] 未定義なもの。
                                default:
                                {
                                    break;
                                }
                            }

                            break;
                        }

                        // 通常の文字。
                        default:
                            objectToProcess = text[charIndex++];
                            DebugUtil.Log(
                                $"#### [{charIndex - 1}]:表示文字【{CSharpUtil.StringModifier(objectToProcess)}】, " +
                                $"rowInfo={{{rowInfo.IsParsing},{rowInfo.Height}}}");
                            break;
                    }

                    static string GetControlCharacterString(Match match) {
                        string s = $"\\{match.Groups["Name"].Value}";
                        var param = match.Groups["Param"].Value;
                        if (!string.IsNullOrEmpty(param))
                        {
                            s += $"[{param}]";
                        }

                        return s;
                    }

                    static int GetParam(Match match) {
                        int.TryParse(match.Groups["Param"].Value, out var val);
                        return val;
                    }

                    static string GetMoneyWord() {
                        var moneyWordItem = DataManager.Self().GetWordDefinitionDataModel().basicStatus.money;
                        return moneyWordItem.enabled == 0 ? moneyWordItem.initialValue : moneyWordItem.value;
                    }
                } while (charIndex < text.Length && objectToProcess == null);

                if (objectToProcess is char || objectToProcess is Texture2D)
                {
                    PutObject(objectToProcess);
                }
            }

            /// <summary>
            /// 文字またはアイコンを一つ配置する。
            /// </summary>
            /// <param name="objectToPut">配置するオブジェクト</param>
            private void PutObject(object objectToPut) {
                if (rowInfo.IsSkipPxPyUntilPutObject)
                {
                    rowInfo.IsSkipPxPyUntilPutObject = false;
                }

                var go = new GameObject(CSharpUtil.StringModifier(objectToPut).ToString());
                if (!rowInfo.IsParsing)
                {
                    _gameObjects.Add(go);
                }

                float yOffset = AddObjectComponent(go, objectToPut);
                var sizeDelta = go.GetComponent<RectTransform>().sizeDelta;

                bool prevRowInfoIsParsing = rowInfo.IsParsing;

                // 自動改行 (折り返し) ？
                if (position.x + sizeDelta.x > _windowRectSize.x)
                {
                    LineBreakProcess();
                }

                if (prevRowInfoIsParsing)
                {
                    rowInfo?.UpdateHeight(sizeDelta.y);
                    DestroyImmediate(go.GetComponent<Image>());
                    DestroyImmediate(go.GetComponent<Text>());
                    DestroyImmediate(go);
                }
                else
                {
                    var t = go.transform;
                    t.SetParent(_rootObj.transform);
                    t.localScale = Vector3.one;

                    var rt = go.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0, 1);
                    rt.anchorMax = new Vector2(0, 1);
                    rt.pivot = new Vector2(0, 1);
                    rt.anchoredPosition = new Vector3(position.x, -(position.y + yOffset), 0f);
                }

                position.x += sizeDelta.x;
            }

            private float AddObjectComponent(GameObject go, object objectToPut) {
                float yOffset = 0f;

                if (objectToPut is Texture2D iconTexture)
                {
                    var image = go.AddComponent<Image>();
                    image.sprite = Sprite.Create(
                        iconTexture,
                        new Rect(0, 0, iconTexture.width, iconTexture.height),
                        Vector2.zero);
                    image.SetNativeSize();

                    if (!rowInfo.IsParsing)
                    {
                        yOffset = (rowInfo.Height - image.preferredHeight) / 2f;
                    }
                }
                else
                {
                    var text = go.AddComponent<Text>();
                    if (!rowInfo.IsParsing)
                    {
                        _texts.Add(text);
                    }

                    SettingsTextComponent(text, objectToPut);

                    go.GetComponent<RectTransform>().sizeDelta =
                        new Vector2(text.preferredWidth, rowInfo.IsParsing ? (text.fontSize * TextHeightCoefficient) : rowInfo.Height);
                }

                return yOffset;
            }

            private void SettingsTextComponent(Text text, object objectToPut) {
                text.text = objectToPut.ToString();
                text.font = _originText.font;
                text.fontSize = _fontSize.ComponentFontSize;
                text.color = _fontColor;
                text.alignment = TextAnchor.MiddleCenter;
                text.horizontalOverflow = HorizontalWrapMode.Overflow;
                text.verticalOverflow = VerticalWrapMode.Overflow;
            }

            /// <summary>
            /// 改行処理。
            /// </summary>
            /// <returns>表示行事前パース中フラグ。</returns>
            private bool LineBreakProcess() {
                if (_isNameToLine) return true;
                var isRowInfoParsing = rowInfo.IsParsing;

                if (isRowInfoParsing)
                {
                    ParseRowTermProcess();
                }

                position.x = 0f;
                position.y += rowInfo.Height + RowSpacing;

                if (!isRowInfoParsing)
                {
                    rowInfo = null;
                }

                return isRowInfoParsing;
            }

            /// <summary>
            /// 表示行パース末端処理。
            /// </summary>
            private void ParseRowTermProcess() {
                rowInfo.IsParsing = false;

                // オブジェクトが1つも表示されておらず縦幅の設定がない場合は、全角スペース文字で縦幅を算出する。
                if (float.IsNaN(rowInfo.Height))
                {
                    object objectToPut = '　';
                    var go = new GameObject(CSharpUtil.StringModifier(objectToPut).ToString());
                    var text = go.AddComponent<Text>();
                    SettingsTextComponent(text, objectToPut);


                    rowInfo.UpdateHeight(text.fontSize * TextHeightCoefficient);

                    DestroyImmediate(text);
                    DestroyImmediate(go);
                }

                // 自動改ページ？
                if (position.y + rowInfo.Height > _windowRectSize.y)
                {
                    rowInfo.RequestForPageBreak = RowInfo.Request.WaitButtonInput;
                }

                DebugUtil.Log(
                    $"Processer.ParseRowTermProcess() : " +
                    $"Height={rowInfo.Height}, " +
                    $"Request={rowInfo.RequestForPageBreak}");
            }

            public void Destroy() {
                if (_goldWindow != null)
                {
                    _goldWindow = null;
                }

                if (_texts != null)
                {
                    foreach (var t in _texts)
                    {
                        DestroyImmediate(t);
                    }

                    _texts.Clear();
                }

                if (_gameObjects != null)
                {
                    foreach (var g in _gameObjects)
                    {
                        DestroyImmediate(g);
                    }

                    _gameObjects.Clear();
                }

                if (_rootObj != null)
                {
                    _rootObj = null;
                }

                _messageText = null;
                TimeHandler.Instance?.RemoveTimeAction(ExecFrame);
                TimeHandler.Instance?.RemoveTimeAction(ExecWaitFrame);
            }

            /// <summary>
            /// 改ページ用のリセット処理。
            /// </summary>
            private void ResetForPageBreak() {
                DebugUtil.LogMethodParameters();

                foreach (Object o in _texts.Select(t => t as Object).Concat(_gameObjects.Select(go => go as Object)))
                {
                    DestroyImmediate(o);
                }

                position = Vector2.zero;
                _fontSize = new FontSize(controlCharacter.initializeParams.fontSize);
                _fontColor = controlCharacter.initializeParams.fontColor;
            }

            /// <summary>
            /// Editorプレビュー専用
            /// </summary>
            private void ResetForExecEditor() {
                position = Vector2.zero;
                if (_texts != null)
                {
                    foreach (var t in _texts)
                    {
                        DestroyImmediate(t);
                    }
                }

                if (_gameObjects != null)
                {
                    foreach (var g in _gameObjects)
                    {
                        DestroyImmediate(g);
                    }
                }

                if (_rootObj.transform.childCount > 0)
                {
                    var child = _rootObj.GetComponentsInChildren<Text>();
                    foreach (var g in child)
                    {
                        DestroyImmediate(g.gameObject);
                    }
                }
            }

            /// <summary>
            /// バトル専用
            /// </summary>
            /// <param name="text"></param>
            /// <param name="isDeleted"></param>
            public void ResetForBattleMessageWindow(string text, bool isDeleted = true) {
                DebugUtil.LogMethodParameters(text, isDeleted);

                _isNextPage = false;
                _isNextPageControlCharacter = false;
                _messageText = text;
                if (isDeleted)
                {
                    _charIndex = 0;
                    position = Vector2.zero;

                    // バトルは何故か直接コンポーネントのフォントサイズを指定するので専用セッターで対応。
                    _fontSize.ComponentFontSize = 40;

                    if (_texts != null)
                    {
                        foreach (var t in _texts)
                        {
                            DestroyImmediate(t);
                        }

                        _texts.Clear();
                    }

                    if (_gameObjects != null)
                    {
                        foreach (var g in _gameObjects)
                        {
                            DestroyImmediate(g);
                        }

                        _gameObjects.Clear();
                    }
                }
            }

            /// <summary>
            /// 表示前に表示行1行分をパースして、改ページが必要かと表示縦幅を取得するクラス。
            /// </summary>
            private class RowInfo
            {
                public enum Request
                {
                    None,
                    WaitButtonInput,
                    Reset,
                }

                public float Height { get; private set; } = float.NaN;
                public Request RequestForPageBreak { get; set; }
                public bool IsSkipPxPyUntilPutObject { get; set; }
                public bool IsParsing { get; set; } = true;

                public RowInfo(bool isSkipPxPyUntilPutObject) {
                    IsSkipPxPyUntilPutObject = isSkipPxPyUntilPutObject;
                }

                /// <summary>
                /// 表示前に表示行の1行分をパースしてその結果を返す。
                /// </summary>
                /// <param name="processer">現在処理中のインスタンス。</param>
                /// <param name="isEditorPreview">エディターのプレビュー表示の為に呼ばれたフラグ。</param>
                /// <returns></returns>
                public static RowInfo PreParseRow(
                    Processer processer,
                    bool isSkipPxPyUntilPutObject,
                    bool isEditorPreview
                ) {
                    // シャローコピーでクローンを作成。
                    var clonedProcesser = (Processer) processer.MemberwiseClone();
                    clonedProcesser.ParseRow(isSkipPxPyUntilPutObject, isEditorPreview);
                    return clonedProcesser.rowInfo;
                }

                /// <summary>
                /// 縦幅を更新する。
                /// </summary>
                /// <param name="height">縦幅。</param>
                public void UpdateHeight(float height) {
                    Height = float.IsNaN(Height) ? height : System.Math.Max(Height, height);
                }
            }

            /// <summary>
            /// 次のコマンドが選択肢などのケースで、入力待ちを行わない場合に、外側から制御する
            /// </summary>
            public void SetIsNotWaitInput(bool flg) {
                _isNotWaitInputNextCommand = flg;
            }

            /// <summary>
            /// 前もってフォントを生成する
            /// </summary>
            [RuntimeInitializeOnLoadMethod]
            public static void InitializeOnLoad() {
#if UNITY_SWITCH
                RPGMaker.Codebase.Runtime.Switch.SwitchSupport.Initialize();
#endif
#if UNITY_PS4
                RPGMaker.Codebase.Runtime.PS4.PS4Support.Initialize();
#endif
                var fontsize = new FontSize(
                    DataManager.Self().GetUiSettingDataModel().commonMenus[0].menuFontSetting.size);
                _originFont = Font.CreateDynamicFontFromOSFont("message", fontsize.ComponentFontSize);
            }
        }
    }

    /// <summary>
    /// ツクール(MV, MZ)とそこそこ互換性のあるフォントサイズ管理構造体。
    /// </summary>
    /// <remarks>
    /// シャローコピーで別オブジェクトをなるようクラスではなく構造体としている。
    /// </remarks>
    public struct FontSize
    {
        private const int TkoolInitialFontSize         = 26;
        private const int TkoolStepFontSize            = 12;
        private const int TkoolLowerLimitSizeToCompare = 24;
        private const int TkoolUpperLimitSizeToCompare = 96;

        private const int UniteInitialFontSize = 100;

        private int UniteStepFontSize =>
            Raito(TkoolStepFontSize, TkoolInitialFontSize, UniteInitialFontSize);

        private int UniteLowerLimitSizeToCompare =>
            Raito(TkoolLowerLimitSizeToCompare, TkoolInitialFontSize, UniteInitialFontSize);

        private int UniteUpperLimitSizeToCompare =>
            Raito(TkoolUpperLimitSizeToCompare, TkoolInitialFontSize, UniteInitialFontSize);

        private const int ComponentInitialFontSize = 42;

        private int uniteFontSize;

        public FontSize(int uniteFontSize) {
            this.uniteFontSize = uniteFontSize;
        }

        private static int Raito(int value, int from, int to) {
            return (value * to + to / 2) / from;
        }

        public int UniteFontSize
        {
            set { uniteFontSize = value; }
        }

        public int ComponentFontSize
        {
            get { return Raito(uniteFontSize, UniteInitialFontSize, ComponentInitialFontSize); }
            set { uniteFontSize = Raito(value, ComponentInitialFontSize, UniteInitialFontSize); }
        }

        public void Reset() {
            uniteFontSize = UniteInitialFontSize;
        }

        public void MakeLarger() {
            if (uniteFontSize <= UniteUpperLimitSizeToCompare)
            {
                uniteFontSize += UniteStepFontSize;
            }
        }

        public void MakeSmaller() {
            if (uniteFontSize >= UniteLowerLimitSizeToCompare)
            {
                uniteFontSize -= UniteStepFontSize;
            }
        }
    }
}
