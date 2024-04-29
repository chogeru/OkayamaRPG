using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event.EventDataModel;

namespace RPGMaker.Codebase.Editor.MapEditor.Window.EventEdit
{
    /// <summary>
    ///     実行内容枠のウィンドウ
    /// </summary>
    public class ExecutionContentsWindow : BaseWindow
    {
        // イベントタイプ
        public enum EventType
        {
            Normal = 0,
            Common = 1,
            Battle = 2,
            None   = 3,
            Max
        }

        private const string ExecutionContetentUssDark =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/ExecutionContents/execution_contentsDark.uss";
        private const string ExecutionContetentUssLight =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/ExecutionContents/execution_contentsLight.uss";

        public bool IsSaveWait = false;
        
        // コマンドのコピー関係
        // (コモンイベント間でも保持する為にstaticとする)
        private static readonly List<EventCommand> _copyCommandList = new List<EventCommand>();
        private static readonly List<int>          _copyCodeList    = new List<int>();

        // 状態プロパティ
        private EventCommand _currentSelectingCommand;

        // データプロパティ
        private          EventDataModel         _eventDataModelEntity;
        private readonly EventManagementService _eventManagementService = new EventManagementService();
        private          EventType              _eventType;

        // アプリケーションユースケース
        private readonly GetEventCommandLabelText _getEventCommandLabelTextUseCase = new GetEventCommandLabelText();
        private          float                    _scrollIndex;

        private ScrollView _scrollView;

        private int _selectHeadIndex = -1;
        private int _selectTailIndex = -1;
        private int _selectFirstIndex = -1;
        private int _commandCount = -1;
        public List<EventDataModel> EventDataModels { get; private set; }
        public EventDataModel EventDataModel => _eventDataModelEntity;

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad() {
            //イベント編集中であった場合、閉じる
            WindowLayoutManager.CloseWindow(WindowLayoutManager.WindowLayoutId.MapEventCommandWindow);
        }

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="eventDataModelEntity"></param>
        /// <param name="initEvent"></param>
        /// <param name="eventType"></param>
        public void Init(
            EventDataModel eventDataModelEntity,
            bool initEvent = false,
            EventType eventType = EventType.None
        ) {
            if (eventType != EventType.None)
                _eventType = eventType;

            _eventDataModelEntity = eventDataModelEntity;
            EventDataModels = _eventManagementService.LoadEvent();

            // 更新処理内からも呼ばれているので初期化が必要なければExecutionContentsWindowParamは更新しない
            if (initEvent)
            {
                var instance = ExecutionContentsWindowParam.instance;
                instance.eventCommandIndex = 0;
                instance.eventId = _eventDataModelEntity.id;
                instance.page = _eventDataModelEntity.page;
            }

            if (IsSaveWait)
            {
                SaveWait(initEvent);
                return;
            }

            InitUI(initEvent);
        }

        /// <summary>
        /// データの保存を待ってから、イベントコマンド一覧を刷新する
        /// </summary>
        /// <param name="initEvent"></param>
        private async void SaveWait(bool initEvent) {
            await Task.Delay(1);
            if (IsSaveWait)
            {
                SaveWait(initEvent);
                return;
            }
            InitUI(initEvent);
        }

        /// <summary>
        /// UI初期化
        /// </summary>
        /// <param name="initEvent"></param>
        private void InitUI(bool initEvent) {
            var eventData = new List<EventCommand>();
            if (_eventDataModelEntity != null) eventData = _eventDataModelEntity.eventCommands;

            var last = eventData.FindLast(e => e.code == 0);
            if (last == null)
            {
                var initData = new EventCommand(0, new List<string>(), new List<EventCommandMoveRoute>());
                //なにもないデータを作り、ここをダブルクリックすることで新たにデータを追加する
                eventData.Add(initData);
                //このデータを保存する
                SetUpData();
            }

            if (eventData.Count > 0) _eventDataModelEntity.eventCommands = eventData;

            //コマンド一覧
            _scrollView = new ScrollView();
            _scrollView.style.unityTextAlign = TextAnchor.UpperLeft;
            _scrollView.verticalScroller.value = _scrollIndex;

            var commands = _eventDataModelEntity?.eventCommands;
            if (commands == null) return;

            //コマンド一覧に対してindentを設定する
            _eventManagementService.SetEventIndent(_eventDataModelEntity);

            for (var index = 0; index < commands.Count; index++)
            {
                var eventCommand = commands[index];
                CreateCommandButton(eventCommand, index);

                if (eventCommand.code == (int) EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG)
                    if (eventCommand.parameters[1] != "0")
                    {
                        //商品を1つ追加する
                        index++;
                        var param = new List<string>();
                        param.Add(eventCommand.parameters[0]);
                        param.Add(eventCommand.parameters[1]);
                        param.Add(eventCommand.parameters[2]);
                        param.Add(eventCommand.parameters[3]);

                        var shopOneLine = new EventCommand((int) EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG_LINE, param,
                            null);
                        shopOneLine.indent = eventCommand.indent + 1;
                        CreateCommandButton(shopOneLine, index);

                        //元のデータから1つ目の商品を消す
                        commands.Insert(index, shopOneLine);
                        eventCommand.parameters[0] = "0";
                        eventCommand.parameters[1] = "0";
                        eventCommand.parameters[2] = "0";
                        eventCommand.parameters[3] = "0";
                    }
            }

            //ウィンドウタイトル
            var windowTitleLabel = new Label {text = EditorLocalize.LocalizeText("WORD_0635")};

            var stylesheet =
                AssetDatabase.LoadAssetAtPath<StyleSheet>(
                    ExecutionContetentUssDark);

            if (!EditorGUIUtility.isProSkin)
            {
                stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ExecutionContetentUssLight);
            }

            
            // 要素配置
            rootVisualElement.Clear();
            rootVisualElement.styleSheets.Add(stylesheet);
            rootVisualElement.style.flexGrow = 1;
            rootVisualElement.Add(windowTitleLabel);
            rootVisualElement.Add(_scrollView);

            if (initEvent && commands.Count > 0)
            {
                _currentSelectingCommand = commands[0];
                InitSelect();
            }

            _commandCount = commands.Count;
        }

        private async void InitSelect() {
            //イベントコマンドによっては、即反映不可能なため、若干待つ
            await Task.Delay(10);
            //最初のイベントコマンドの選択範囲を取得
            var (head, tail) = GetRelevanceLinesIndex(0);
            _selectHeadIndex = head;
            _selectTailIndex = tail;
            _selectFirstIndex = head;
            //イベントコマンドの編集Window表示
            ShowEventCommand();
        }

        /// <summary>
        /// コマンドボタン生成
        /// </summary>
        /// <param name="eventCommand"></param>
        /// <param name="index"></param>
        private void CreateCommandButton(EventCommand eventCommand, int index) {
            var btn = new Button();
            btn.Add(_getEventCommandLabelTextUseCase.Invoke(eventCommand));
            var currentIndex = index;

            // 下記の処理が無いとButtonに対してMouseDownEventが効かない
            // https://forum.unity.com/threads/registercallback-mousedownevent-registers-only-right-mouse-button.950653/#post-6725344
            btn.clickable.activators.Clear();
            btn.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == (int) MouseButton.RightMouse)
                    // 選択範囲内を右クリックした場合はコンテキストメニューの表示のみを行う
                    if (currentIndex >= _selectHeadIndex && currentIndex <= _selectTailIndex)
                        return;

                var instance = ExecutionContentsWindowParam.instance;
                int currentIndexWork = currentIndex;
                bool isHideEvntCommand = false;
                if (Event.current.shift)
                {
                    // シフトを押しながらクリックで複数行選択とする
                    _selectHeadIndex = Math.Min(currentIndex, _selectFirstIndex);
                    _selectTailIndex = Math.Max(currentIndex, _selectFirstIndex);

                    var (head, tail) = GetRelevanceLinesIndexInRange(_selectHeadIndex, _selectTailIndex);
                    _selectHeadIndex = head;
                    _selectTailIndex = tail;
                    _currentSelectingCommand = eventCommand;

                    //シフトを押しながらクリックされた場合、かつ異なる行だった時
                    //イベントコマンドWindowを消去する
                    //他の処理を通した後、最後に消去するため、フラグを立てる
                    if (currentIndex != _selectFirstIndex)
                    {
                        isHideEvntCommand = true;
                    }
                }
                else
                {
                    // 選択した行のコマンドに関係性のあるコマンドの範囲を取得
                    var (head, tail) = GetRelevanceLinesIndex(currentIndex);
                    _selectHeadIndex = head;
                    _selectTailIndex = tail;
                    _selectFirstIndex = head;
                    currentIndexWork = _selectHeadIndex;
                    _currentSelectingCommand = _eventDataModelEntity.eventCommands[_selectHeadIndex];
                }

                _scrollIndex = _scrollView.verticalScroller.value;
                ShowEventCommand(currentIndexWork);

                //イベントコマンドWindowを非表示にする場合
                if (isHideEvntCommand)
                {
                    HideEventCommand();
                }
            });

            btn.RegisterCallback<MouseUpEvent>(evt =>
            {
                // 右クリックであったならコンテキストメニューを表示する
                if (evt.button == (int) MouseButton.RightMouse) OnRightClickEvent(currentIndex, eventCommand.code);
            });

            _scrollView.Add(btn);
        }

        /// <summary>
        /// イベントデータ保存
        /// </summary>
        public void SetUpData() {
            _eventManagementService.SaveEvent(_eventDataModelEntity);
            if(IsSaveWait) SaveEnd();
        }

        /// <summary>
        /// データ保存終了時処理
        /// </summary>
        private async void SaveEnd() {
            await Task.Delay(1);
            IsSaveWait = false;
        }

        /// <summary>
        /// 内部のデータの再取得を行い、描画内容の更新を行う
        /// </summary>
        /// <param name="eventDataModelEntity"></param>
        /// <param name="flg"></param>
        public void Refresh([CanBeNull] EventDataModel eventDataModelEntity = null, bool flg = false) {
            if (eventDataModelEntity != null) _eventDataModelEntity = eventDataModelEntity;

            _scrollView.Clear();
            Init(_eventDataModelEntity);
            SetCurrentEventCommandActiveStyle();

            if (flg)
            {
                var instance = ExecutionContentsWindowParam.instance;
                ShowEventCommand(instance.eventCommandIndex);
            }
        }

        /// <summary>
        /// コマンド一覧表示
        /// </summary>
        public void ProcessTextSetting() {
            //選択しなおす必要がある可能性がある
            //そのため、このタイミングで、選択開始行を変更することなく、選択終了行を取得しなおす
            if (_commandCount != _eventDataModelEntity.eventCommands.Count)
            {
                //コマンドの行数が変わっている場合は、選択しなおす
                if (_selectFirstIndex < _selectTailIndex)
                {
                    int sub = _eventDataModelEntity.eventCommands.Count - _commandCount;
                    var (head, tail) = GetRelevanceLinesIndex(_selectTailIndex + sub);
                    _selectHeadIndex = head;
                    _selectTailIndex = tail;
                }
                else
                {
                    var (head, tail) = GetRelevanceLinesIndex(_selectHeadIndex);
                    _selectHeadIndex = head;
                    _selectTailIndex = tail;
                }
            }

            //マップのイベントをRefresh
            MapEditor.EventRefresh();
        }

        /// <summary>
        /// 現在選択中のイベントコマンドを返却
        /// </summary>
        /// <returns></returns>
        public EventCommand GetCurrentSelectingCommand() {
            return _currentSelectingCommand;
        }

        /// <summary>
        /// 選択したイベントコマンドをコマンド設定枠に表示する
        /// </summary>
        /// <param name="selectedIndex">選択中の行のインデックス</param>
        private void ShowEventCommand(int selectedIndex = 0) {
            if (_currentSelectingCommand == null) return;
            var instance = ExecutionContentsWindowParam.instance;
            
            // 選択中の行を行をアクティブな表示にする
            instance.eventCommandIndex = selectedIndex;
            instance.page = _eventDataModelEntity.page;
            SetCurrentEventCommandActiveStyle();

            // コマンド設定枠に表示する
            var eventIndex = EventDataModels.FindIndex(evt =>
                evt.id == _eventDataModelEntity.id && evt.page == _eventDataModelEntity.page);

            var commandSettingWindow = (CommandSettingWindow)WindowLayoutManager.GetActiveWindow(
                    WindowLayoutManager.WindowLayoutId.MapEventCommandSettingWindow);
            commandSettingWindow.SetEventCommand(
                _currentSelectingCommand.code,
                eventIndex,
                instance.eventCommandIndex
            );

            // 『アニメーションの表示』イベント以外は、マップイベント編集ウィンドウにフォーカスする。
            if ((EventEnum) _currentSelectingCommand.code != EventEnum.EVENT_CODE_CHARACTER_SHOW_ANIMATION &&
                _eventType != EventType.Battle)
                WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.MapEventEditWindow);
        }

        /// <summary>
        /// コマンド設定枠を消去する
        /// </summary>
        private void HideEventCommand() {
            var commandSettingWindow = (CommandSettingWindow) WindowLayoutManager.GetActiveWindow(
                    WindowLayoutManager.WindowLayoutId.MapEventCommandSettingWindow);
            commandSettingWindow.HideScreen();
        }

        /// <summary>
        /// 選択中のコマンドの行をアクティブな表示にする
        /// </summary>
        private void SetCurrentEventCommandActiveStyle() {
            var instance = ExecutionContentsWindowParam.instance;
            _scrollView.Query<Button>(null, "unity-button").ForEach(item => { item.RemoveFromClassList("active"); });

            // 選択中の行をアクティブな表示にする
            for (var i = _selectHeadIndex; i <= _selectTailIndex; i++)
            {
                var target = _scrollView.Query<Button>(null, "unity-button").AtIndex(i);
                target?.AddToClassList("active");
            }
        }

        /// <summary>
        /// 右クリック時処理
        /// </summary>
        /// <param name="index"></param>
        /// <param name="code"></param>
        private void OnRightClickEvent(int index, int code) {
            var menu = new GenericMenu();
            if (code == 0)
            {
                menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_0010")), false,
                    () => CreateEventCommand(index));

                //マップイベント、バトルイベント、コモンイベントで、それぞれ貼り付け可能なコマンドが異なる
                //コピーしたデータ内に、貼り付けが出来ないものが含まれていた場合には、貼り付けを選択不可とする
                bool isActivePaste = true;

                if (_copyCommandList == null || _copyCommandList.Count <= 0)
                    isActivePaste = false;
                else
                {
                    EventCodeList.EventType type = EventCodeList.EventType.Map;
                    if (_eventType == EventType.Battle)
                        type = EventCodeList.EventType.Battle;
                    else if (_eventType == EventType.Common)
                        type = EventCodeList.EventType.Common;

                    foreach (var data in _copyCommandList)
                        if (!EventCodeList.CheckEventCodeExecute(data.code, type, false))
                            isActivePaste = false;
                }

                if (isActivePaste)
                    menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_0011")), false, PasteEventCommand(index));
                else
                    menu.AddDisabledItem(new GUIContent(EditorLocalize.LocalizeText("WORD_0011")), false);

                // 複数行選択されていればコピー、削除を有効化
                if (_selectHeadIndex != -1 && _selectTailIndex != -1 && _selectTailIndex > _selectHeadIndex)
                {
                    menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_0015")), false, CopyEventCommand());
                    menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_0383")), false, DeleteEventCommand());
                }
            }
            else if (code == (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_SELECTED || //選択肢の分岐
                     code == (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_CANCELED || //選択肢の分岐（キャンセル）
                     code == (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_END || //選択肢の終了
                     code == (int) EventEnum.EVENT_CODE_FLOW_ELSE || //条件文の、else文
                     code == (int) EventEnum.EVENT_CODE_FLOW_ENDIF || //条件文終了
                     code == (int) EventEnum.EVENT_CODE_FLOW_AND || //条件文のAND条件
                     code == (int) EventEnum.EVENT_CODE_FLOW_OR || //条件文のOR条件
                     code == (int) EventEnum.EVENT_CODE_FLOW_LOOP_END || //ループ終了
                     code == (int) EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE_END || //カスタム移動終了
                     code == (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_WIN || //バトル勝利
                     code == (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_LOSE || //バトル敗北
                     code == (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_ESCAPE || //バトル逃走
                     code == (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_END || //バトル終了
                     code == (int) EventEnum.EVENT_CODE_MESSAGE_TEXT_SCROLL_ONE_LINE || //メッセージのスクロールの本文
                     code == (int) EventEnum.EVENT_CODE_MESSAGE_TEXT_ONE_LINE || //文章表示の本文
                     code == (int) EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG_LINE //ショップの購入物
            )
            {
                //間にイベントを差し込まれると困るため、削除かコピーのみを有効とする
                menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_0015")), false, CopyEventCommand());
                menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_0383")), false, DeleteEventCommand());
            }
            else
            {
                menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_0010")), false,
                    () => CreateEventCommand(index));
                menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_0015")), false, CopyEventCommand());

                //マップイベント、バトルイベント、コモンイベントで、それぞれ貼り付け可能なコマンドが異なる
                //コピーしたデータ内に、貼り付けが出来ないものが含まれていた場合には、貼り付けを選択不可とする
                bool isActivePaste = true;

                if (_copyCommandList == null || _copyCommandList.Count <= 0)
                    isActivePaste = false;
                else
                {
                    EventCodeList.EventType type = EventCodeList.EventType.Map;
                    if (_eventType == EventType.Battle)
                        type = EventCodeList.EventType.Battle;
                    else if (_eventType == EventType.Common)
                        type = EventCodeList.EventType.Common;

                    foreach (var data in _copyCommandList)
                        if (!EventCodeList.CheckEventCodeExecute(data.code, type, false))
                            isActivePaste = false;
                }

                if (isActivePaste)
                    menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_0011")), false, PasteEventCommand(index));
                else
                    menu.AddDisabledItem(new GUIContent(EditorLocalize.LocalizeText("WORD_0011")), false);

                menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_0383")), false, DeleteEventCommand());
            }

            menu.ShowAsContext();
        }

        /// <summary>
        /// イベントの追加
        /// </summary>
        /// <param name="selectedIndex">選択中の行のインデックス</param>
        private void CreateEventCommand(int selectedIndex) {
            var commonWindow = (CommandWindow)WindowLayoutManager.GetOrOpenWindow(
                WindowLayoutManager.WindowLayoutId.MapEventCommandWindow);
            commonWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("MapEditor CommandWindow"), data =>
            {
                //選択された行から、選択範囲を再取得
                var (headNow, tailNow) = GetRelevanceLinesIndex(selectedIndex);

                var window = (CommandSettingWindow)WindowLayoutManager.GetActiveWindow(
                    WindowLayoutManager.WindowLayoutId.MapEventCommandSettingWindow);

                window.SetEventId(_eventDataModelEntity.id);
                window.SetEventPage(_eventDataModelEntity.page);
                window.InsertEventCommand((string) data, headNow, null);

                //選択しなおす
                var (head, tail) = GetRelevanceLinesIndex(headNow);
                _selectHeadIndex = head;
                _selectTailIndex = tail;
                _selectFirstIndex = head;
                _currentSelectingCommand = _eventDataModelEntity.eventCommands[_selectHeadIndex];
                ShowEventCommand(_selectHeadIndex);
            });
            if (IsInCustomMoveCommand(_eventDataModelEntity.eventCommands, selectedIndex))
            {
                commonWindow.SetCustomMoveEnabled();

            } else
            {
                EventCodeList.EventType type = EventCodeList.EventType.Map;
                if (_eventType == EventType.Battle)
                    type = EventCodeList.EventType.Battle;
                else if (_eventType == EventType.Common)
                    type = EventCodeList.EventType.Common;
                commonWindow.SetBattleOrMapEnabled(type);
            }
        }

        /// <summary>
        /// selectedIndex位置がカスタム移動コマンド内のときtrueを返す。ただし、selectedIndexがカスタム移動コマンドの開始位置の場合はfalseを返す。
        /// </summary>
        private bool IsInCustomMoveCommand(List<EventCommand> eventCommands, int selectedIndex) {
            if (eventCommands[selectedIndex].code == (int)EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE) return false;

            var indent = eventCommands[selectedIndex].indent - 1;
            for (int i = selectedIndex - 1; i >= 0; i--) {
                if (eventCommands[i].indent != indent) continue;
                if (eventCommands[i].code == (int) EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE_END) return false;
                if (eventCommands[i].code == (int)EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE) return true;
            }
            return false;
        }

        /// <summary>
        /// イベントの削除
        /// </summary>
        private GenericMenu.MenuFunction DeleteEventCommand() {
            if (_selectHeadIndex < 0 || _selectTailIndex < 0) return null;

            return () =>
            {
                var window = (CommandSettingWindow)WindowLayoutManager.GetActiveWindow(
                    WindowLayoutManager.WindowLayoutId.MapEventCommandSettingWindow);

                window.SetEventId(_eventDataModelEntity.id);
                window.SetEventPage(_eventDataModelEntity.page);

                // _selectHeadIndex と _selectTailIndex は、window.DeleteEvent() を呼んだ先で変更されるので、
                // 直接使用せず、別インスタンス (IEnumerable<int>) に設定して使用する。
                bool first = true;
                foreach (var eventCommandIndex in
                    Enumerable.Range(_selectHeadIndex, _selectTailIndex - _selectHeadIndex + 1).Reverse())
                {
                    window.DeleteEvent(eventCommandIndex, first);
                    first = false;
                }

                // 削除後に削除範囲の先頭の行を選択する
                var (head, tail) = GetRelevanceLinesIndex(_selectHeadIndex);
                _selectHeadIndex = head;
                _selectTailIndex = tail;
                _selectFirstIndex = head;
                _currentSelectingCommand = _eventDataModelEntity.eventCommands[_selectHeadIndex];
                ShowEventCommand(_selectHeadIndex);
            };
        }

        /// <summary>
        /// イベントのコピー
        /// </summary>
        private GenericMenu.MenuFunction CopyEventCommand() {
            if (_selectHeadIndex < 0 || _selectTailIndex < 0) return null;

            return () =>
            {
                _copyCodeList.Clear();
                _copyCommandList.Clear();

                for (var i = _selectHeadIndex; i <= _selectTailIndex; i++)
                {
                    var command = _eventDataModelEntity.eventCommands[i];
                    _copyCommandList.Add(command);
                    _copyCodeList.Add(command.code);
                }
            };
        }

        /// <summary>
        /// イベントの貼り付け
        /// </summary>
        /// <param name="selectedIndex">選択中の行のインデックス</param>
        /// <returns>貼り付け時の挙動</returns>
        private GenericMenu.MenuFunction PasteEventCommand(int selectedIndex) {
            if (_copyCommandList.Count < 1 || _selectHeadIndex < 0 || _selectTailIndex < 0) return null;

            return () =>
            {
                var window = (CommandSettingWindow)WindowLayoutManager.GetActiveWindow(
                WindowLayoutManager.WindowLayoutId.MapEventCommandSettingWindow);

                window.SetEventId(_eventDataModelEntity.id);
                window.SetEventPage(_eventDataModelEntity.page);

                var copyCommandList = new List<EventCommand>(_copyCommandList);
                var copyCodeList    = new List<int>(_copyCodeList);
                if (IsInCustomMoveCommand(_eventDataModelEntity.eventCommands, selectedIndex))
                {
                    RemoveNgCommandsForCustomMoveCommand(copyCommandList, copyCodeList);
                }

                //選択された行から、選択範囲を再取得
                var (headNow, tailNow) = GetRelevanceLinesIndex(selectedIndex);
                for (var i = copyCommandList.Count - 1; i >= 0; i--)
                {
                    if (i != 0)
                        window.InsertEventCommand(copyCodeList[i].ToString(), headNow, copyCommandList[i], false);
                    else
                        window.InsertEventCommand(copyCodeList[i].ToString(), headNow, copyCommandList[i], true);
                }

                // 貼り付け行の先頭を選択し、貼り付けた一連のコマンド群及び関連性のある行をアクティブな表示にする
                var (head, tail) =
                    GetRelevanceLinesIndexInRange(headNow, headNow + (copyCodeList.Count - 1));
                _selectHeadIndex = head;
                _selectTailIndex = tail;
                _selectFirstIndex = head;
                _currentSelectingCommand = _eventDataModelEntity.eventCommands[_selectHeadIndex];
                ShowEventCommand(_selectHeadIndex);
            };
        }

        private Dictionary<int, int> _ngCommandInfoForCustomMoveCommand = new Dictionary<int, int>()
        {
            {(int)EventEnum.EVENT_CODE_MOVE_SET_MOVE_POINT, (int)EventEnum.EVENT_CODE_MOVE_SET_MOVE_POINT },
            {(int)EventEnum.MOVEMENT_MOVE_AT_RANDOM, (int)EventEnum.MOVEMENT_MOVE_AT_RANDOM },
            {(int)EventEnum.MOVEMENT_MOVE_TOWARD_PLAYER, (int)EventEnum.MOVEMENT_MOVE_TOWARD_PLAYER },
            {(int)EventEnum.MOVEMENT_MOVE_AWAY_FROM_PLAYER, (int)EventEnum.MOVEMENT_MOVE_AWAY_FROM_PLAYER },
            {(int)EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE, (int)EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE_END},
            {(int)EventEnum.EVENT_CODE_FLOW_IF, (int)EventEnum.EVENT_CODE_FLOW_ENDIF},
            {(int)EventEnum.EVENT_CODE_FLOW_LOOP, (int)EventEnum.EVENT_CODE_FLOW_LOOP_END},
            {(int)EventEnum.EVENT_CODE_FLOW_LOOP_BREAK, (int)EventEnum.EVENT_CODE_FLOW_LOOP_BREAK},
            {(int)EventEnum.EVENT_CODE_FLOW_EVENT_BREAK, (int)EventEnum.EVENT_CODE_FLOW_EVENT_BREAK},
            {(int)EventEnum.EVENT_CODE_FLOW_COMMON_START, (int)EventEnum.EVENT_CODE_FLOW_COMMON_END},
            {(int)EventEnum.EVENT_CODE_FLOW_LABEL, (int)EventEnum.EVENT_CODE_FLOW_LABEL},
            {(int)EventEnum.EVENT_CODE_FLOW_JUMP_LABEL, (int)EventEnum.EVENT_CODE_FLOW_JUMP_LABEL},
            {(int)EventEnum.EVENT_CODE_FLOW_ANNOTATION, (int)EventEnum.EVENT_CODE_FLOW_ANNOTATION},
        };
        private void RemoveNgCommandsForCustomMoveCommand(List<EventCommand> commandList, List<int> codeList) {
            var indent = commandList[0].indent;
            int i = 0;
            while (i < commandList.Count)
            {
                if (_ngCommandInfoForCustomMoveCommand.ContainsKey(commandList[i].code))
                {
                    var encCommandCode = _ngCommandInfoForCustomMoveCommand[commandList[i].code];
                    var end = i;
                    while (end < commandList.Count - 1)
                    {
                        var command = commandList[end];
                        if (command.indent == indent && command.code == encCommandCode)
                        {
                            break;
                        }
                        end++;
                    }
                    commandList.RemoveRange(i, end - i + 1);
                    codeList.RemoveRange(i, end - i + 1);
                } else
                {
                    i++;
                }
            }
        }


        /// <summary>
        /// 指定された行に関連性のある行が無いかをチェックし、関連性のある行の範囲を返す
        /// </summary>
        /// <param name="index">行のインデックス</param>
        /// <returns>範囲の先頭のインデックス、末尾のインデックス。関連性が無い場合は引数の値が両方に入る</returns>
        private (int, int) GetRelevanceLinesIndex(int index) {
            if (_eventDataModelEntity.eventCommands.Count <= index)
                index = _eventDataModelEntity.eventCommands.Count - 1;
            
            if (index < 0)
                index = 0;
            
            var currentCommand = _eventDataModelEntity.eventCommands[index];
            var headIndex = index;
            var tailIndex = index;
            var tmpIndex = -1;

            switch ((EventEnum) Enum.ToObject(typeof(EventEnum), currentCommand.code))
            {
                // [選択肢]コマンド関係
                case EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT: //選択肢の開始
                case EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_SELECTED: //選択肢の分岐
                case EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_CANCELED: //選択肢の分岐（キャンセル）
                case EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_END: //選択肢の終了
                    // 先頭にあたる「選択肢の開始」の行を選択された行から上方向に探索する
                    for (var i = index; i >= 0; i--)
                        if (_eventDataModelEntity.eventCommands[i].code ==
                            (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT &&
                            _eventDataModelEntity.eventCommands[i].indent == currentCommand.indent)
                        {
                            tmpIndex = i;
                            headIndex = Math.Min(i, headIndex);
                            break;
                        }

                    // 「選択肢の開始」の行から下方向に「選択肢の終了」の行を探索する
                    for (var i = tmpIndex + 1; i < _eventDataModelEntity.eventCommands.Count; i++)
                        if (_eventDataModelEntity.eventCommands[i].code ==
                            (int) EventEnum.EVENT_CODE_MESSAGE_INPUT_SELECT_END &&
                            _eventDataModelEntity.eventCommands[i].indent == currentCommand.indent)
                        {
                            tailIndex = Math.Max(i, tailIndex);
                            break;
                        }

                    break;

                // [分岐設定]コマンド関係
                case EventEnum.EVENT_CODE_FLOW_IF: //条件文の開始
                case EventEnum.EVENT_CODE_FLOW_AND: //条件文 AND
                case EventEnum.EVENT_CODE_FLOW_OR: //条件文 OR
                case EventEnum.EVENT_CODE_FLOW_ELSE: //条件文の分岐
                case EventEnum.EVENT_CODE_FLOW_ENDIF: //条件文の終了
                    // 先頭にあたる「条件文の開始」の行を選択された行から上方向に探索する
                    int indentWork = currentCommand.indent;
                    if ((EventEnum) Enum.ToObject(typeof(EventEnum), currentCommand.code) == EventEnum.EVENT_CODE_FLOW_AND ||
                        (EventEnum) Enum.ToObject(typeof(EventEnum), currentCommand.code) == EventEnum.EVENT_CODE_FLOW_OR)
                    {
                        indentWork--;
                    }
                    for (var i = index; i >= 0; i--)
                        if (_eventDataModelEntity.eventCommands[i].code == (int) EventEnum.EVENT_CODE_FLOW_IF &&
                            _eventDataModelEntity.eventCommands[i].indent == indentWork)
                        {
                            tmpIndex = i;
                            headIndex = Math.Min(i, headIndex);
                            break;
                        }

                    // 「条件文の開始」の行から下方向に「条件文の終了」の行を探索する
                    for (var i = tmpIndex + 1; i < _eventDataModelEntity.eventCommands.Count; i++)
                        if (_eventDataModelEntity.eventCommands[i].code == (int) EventEnum.EVENT_CODE_FLOW_ENDIF &&
                            _eventDataModelEntity.eventCommands[i].indent == indentWork)
                        {
                            tailIndex = Math.Max(i, tailIndex);
                            break;
                        }

                    break;

                // [ループ]コマンド関係
                case EventEnum.EVENT_CODE_FLOW_LOOP: //ループ文の開始
                case EventEnum.EVENT_CODE_FLOW_LOOP_END: //ループ文の終了
                    // 先頭にあたる「ループ文の開始」の行を選択された行から上方向に探索する
                    for (var i = index; i >= 0; i--)
                        if (_eventDataModelEntity.eventCommands[i].code == (int) EventEnum.EVENT_CODE_FLOW_LOOP &&
                            _eventDataModelEntity.eventCommands[i].indent == currentCommand.indent)
                        {
                            tmpIndex = i;
                            headIndex = Math.Min(i, headIndex);
                            break;
                        }

                    // 「ループ文の開始」の行から下方向に「ループ文の終了」の行を探索する
                    for (var i = tmpIndex + 1; i < _eventDataModelEntity.eventCommands.Count; i++)
                        if (_eventDataModelEntity.eventCommands[i].code == (int) EventEnum.EVENT_CODE_FLOW_LOOP_END &&
                            _eventDataModelEntity.eventCommands[i].indent == currentCommand.indent)
                        {
                            tailIndex = Math.Max(i, tailIndex);
                            break;
                        }

                    break;

                // [カスタム移動]コマンド関係
                case EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE: //カスタム移動の開始
                case EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE_END: //カスタム移動の終了
                    // 先頭にあたる「コマンド移動の開始」の行を選択された行から上方向に探索する
                    for (var i = index; i >= 0; i--)
                        if (_eventDataModelEntity.eventCommands[i].code == (int) EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE &&
                            _eventDataModelEntity.eventCommands[i].indent == currentCommand.indent)
                        {
                            tmpIndex = i;
                            headIndex = Math.Min(i, headIndex);
                            break;
                        }

                    // 「ループ文の開始」の行から下方向に「ループ文の終了」の行を探索する
                    for (var i = tmpIndex + 1; i < _eventDataModelEntity.eventCommands.Count; i++)
                        if (_eventDataModelEntity.eventCommands[i].code == (int) EventEnum.EVENT_CODE_FLOW_CUSTOM_MOVE_END &&
                            _eventDataModelEntity.eventCommands[i].indent == currentCommand.indent)
                        {
                            tailIndex = Math.Max(i, tailIndex);
                            break;
                        }

                    break;

                // [戦闘の処理]コマンド関係
                case EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG: //バトル開始
                case EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_WIN: //バトル勝利
                case EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_LOSE: //バトル敗北
                case EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_ESCAPE: //バトル逃走
                case EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_END: //バトル終了
                    // 先頭にあたる「バトル開始」の行を選択された行から上方向に探索する
                    for (var i = index; i >= 0; i--)
                        if (_eventDataModelEntity.eventCommands[i].code ==
                            (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG &&
                            _eventDataModelEntity.eventCommands[i].indent == currentCommand.indent)
                        {
                            tmpIndex = i;
                            headIndex = Math.Min(i, headIndex);
                            break;
                        }

                    //これらのイベントの場合は、終了が「バトル終了」
                    //または、「バトル勝利」がない場合は1行のみ削除
                    if (_eventDataModelEntity.eventCommands[tmpIndex + 1].code != (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_WIN)
                    {
                        tailIndex = Math.Max(tmpIndex, tailIndex);
                    }
                    else
                        //現在のindexから下方向に探す
                        for (var i = tmpIndex + 1; i < _eventDataModelEntity.eventCommands.Count; i++)
                            if (_eventDataModelEntity.eventCommands[i].code ==
                                (int) EventEnum.EVENT_CODE_SCENE_SET_BATTLE_CONFIG_END &&
                                _eventDataModelEntity.eventCommands[i].indent == currentCommand.indent)
                            {
                                tailIndex = Math.Max(i, tailIndex);
                                break;
                            }

                    break;

                // [文章の表示]コマンド関係
                case EventEnum.EVENT_CODE_MESSAGE_TEXT: //文章表示
                case EventEnum.EVENT_CODE_MESSAGE_TEXT_ONE_LINE: //文章表示のテキスト部分
                    // 先頭にあたる「文章表示」の行を選択された行から上方向に探索する
                    for (var i = index; i >= 0; i--)
                        if (_eventDataModelEntity.eventCommands[i].code == (int) EventEnum.EVENT_CODE_MESSAGE_TEXT)
                        {
                            tmpIndex = i;
                            headIndex = Math.Min(i, headIndex);
                            break;
                        }

                    // 「文章表示」の行から下方向に「インデントが同じ」の行を探索する
                    for (var i = tmpIndex + 1; i < _eventDataModelEntity.eventCommands.Count; i++)
                        if (_eventDataModelEntity.eventCommands[i].indent == currentCommand.indent)
                        {
                            tailIndex = Math.Max(i - 1, tailIndex);
                            break;
                        }

                    break;

                // [文章のスクロール]コマンド関係
                case EventEnum.EVENT_CODE_MESSAGE_TEXT_SCROLL: //文章のスクロール
                case EventEnum.EVENT_CODE_MESSAGE_TEXT_SCROLL_ONE_LINE: //文章のスクロールのテキスト部分
                    // 先頭にあたる「文章のスクロール」の行を選択された行から上方向に探索する
                    for (var i = index; i >= 0; i--)
                        if (_eventDataModelEntity.eventCommands[i].code ==
                            (int) EventEnum.EVENT_CODE_MESSAGE_TEXT_SCROLL)
                        {
                            tmpIndex = i;
                            headIndex = Math.Min(i, headIndex);
                            break;
                        }

                    // 「文章のスクロール」の行から下方向に「インデントが同じ」の行を探索する
                    for (var i = tmpIndex + 1; i < _eventDataModelEntity.eventCommands.Count; i++)
                        if (_eventDataModelEntity.eventCommands[i].indent == currentCommand.indent)
                        {
                            tailIndex = Math.Max(i - 1, tailIndex);
                            break;
                        }

                    break;

                // [ショップ]コマンド関係
                case EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG:
                case EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG_LINE:
                    // 先頭にあたる「ショップの表示」の行を選択された行から上方向に探索する
                    for (var i = index; i >= 0; i--)
                        if (_eventDataModelEntity.eventCommands[i].code == (int) EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG)
                        {
                            tmpIndex = i;
                            headIndex = Math.Min(i, headIndex);
                            break;
                        }

                    // 「ショップの表示」の行から下方向に「ショップの購入物ではなくなるまで」の行を探索する
                    for (var i = tmpIndex + 1; i < _eventDataModelEntity.eventCommands.Count; i++)
                        if (_eventDataModelEntity.eventCommands[i].code != (int) EventEnum.EVENT_CODE_SCENE_SET_SHOP_CONFIG_LINE)
                        {
                            tailIndex = Math.Max(i - 1, tailIndex);
                            break;
                        }

                    break;

                // [敵キャラのステータス増減]コマンド関係
                case EventEnum.EVENT_CODE_BATTLE_CHANGE_STATUS: // [敵キャラのステータス増減]本体
                case EventEnum.EVENT_CODE_BATTLE_CHANGE_MP: // [敵キャラのステータス増減]のMP増減
                case EventEnum.EVENT_CODE_BATTLE_CHANGE_TP: // [敵キャラのステータス増減]のTP増減
                    // 先頭にあたる[敵キャラのステータス増減]本体の行を選択された行から上方向に探索する
                    for (var i = index; i >= 0; i--)
                        if (currentCommand.code == (int) EventEnum.EVENT_CODE_BATTLE_CHANGE_STATUS &&
                            _eventDataModelEntity.eventCommands[i].indent == currentCommand.indent)
                        {
                            tmpIndex = i;
                            headIndex = Math.Min(i, headIndex);
                            break;
                        }

                    // 本体の行から下方向に「現在のインデックスを含む3行以内の関連コマンドが見つからなくなるまで」の行を探索する
                    for (var i = headIndex + 1; i <= headIndex + 3; i++)
                    {
                        var tmpCommand = _eventDataModelEntity.eventCommands[i];

                        if (!(tmpCommand.code == (int) EventEnum.EVENT_CODE_BATTLE_CHANGE_MP &&
                              _eventDataModelEntity.eventCommands[i].indent == currentCommand.indent) &&
                            !(tmpCommand.code == (int) EventEnum.EVENT_CODE_BATTLE_CHANGE_TP &&
                              _eventDataModelEntity.eventCommands[i].indent == currentCommand.indent))
                        {
                            // 見つからなくなった = 1つ手前の行が[敵キャラのステータス増減]コマンドの末尾
                            tailIndex = Math.Max(i - 1, tailIndex);
                            break;
                        }
                    }

                    break;

                // [アクター設定の変更]コマンド関係
                case EventEnum.EVENT_CODE_ACTOR_CHANGE_NAME: // [アクター設定の変更]の名前変更コマンド
                case EventEnum.EVENT_CODE_ACTOR_CHANGE_NICKNAME: // [アクター設定の変更]の二つ名変更コマンド
                case EventEnum.EVENT_CODE_ACTOR_CHANGE_PROFILE: // [アクター設定の変更]のプロフィール変更コマンド
                    // 先頭にあたる行を取得
                    tmpIndex = ChangeName.GetBaseCommandIndex(_eventDataModelEntity.eventCommands, index);
                    headIndex = Math.Min(tmpIndex, headIndex);
                    // 本体の行から下方向に「現在のインデックスを含む3行以内の関連コマンドが見つからなくなるまで」の行を探索する
                    for (var i = headIndex + 1; i <= headIndex + 3; i++)
                    {
                        var tmpCommand = _eventDataModelEntity.eventCommands[i];

                        if (!(tmpCommand.code == (int) EventEnum.EVENT_CODE_ACTOR_CHANGE_NICKNAME &&
                              _eventDataModelEntity.eventCommands[i].indent == currentCommand.indent) &&
                            !(tmpCommand.code == (int) EventEnum.EVENT_CODE_ACTOR_CHANGE_PROFILE &&
                              _eventDataModelEntity.eventCommands[i].indent == currentCommand.indent))
                        {
                            // 見つからなくなった = 1つ手前の行が[アクター設定の変更]コマンドの末尾
                            tailIndex = Math.Max(i - 1, tailIndex);
                            break;
                        }
                    }

                    break;
            }

            return (headIndex, tailIndex);
        }

        /// <summary>
        /// 指定された範囲内の行に関連性のある行が無いかをチェックし、関連性のある行の範囲を返す
        /// </summary>
        /// <param name="index">行のインデックス</param>
        /// <returns>関連性のある行を含めた範囲の先頭のインデックス、末尾のインデックス。関連性が無い場合は引数の値がそれぞれに入る</returns>
        private (int, int) GetRelevanceLinesIndexInRange(int head, int tail) {
            var retHead = head;
            var retTail = tail;

            // 選択範囲内に関連性のある行が無いかチェック
            for (var i = head; i <= tail; i++)
            {
                var (tmpHead, tmpTail) = GetRelevanceLinesIndex(i);
                retHead = Math.Min(retHead, tmpHead);
                retTail = Math.Max(retTail, tmpTail);

                // 下方向への探索が進んだ場合はその分だけ行数を進める
                i = Math.Max(i, tmpTail);
            }

            return (retHead, retTail);
        }

        public class ExecutionContentsWindowParam : ScriptableSingleton<ExecutionContentsWindowParam>
        {
            // コマンドのインデックス
            public int eventCommandIndex;

            // イベントのID
            public string eventId;

            // ページ番号
            public int page;
        }
    }
}