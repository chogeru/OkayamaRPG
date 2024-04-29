//#define USE_TRACE_PRINT

#if USE_TRACE_PRINT
using RPGMaker.Codebase.CoreSystem.Helper;
#endif
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using GameState = RPGMaker.Codebase.Runtime.Common.GameStateHandler.GameState;

namespace RPGMaker.Codebase.Runtime.Common
{
    /// <summary>
    /// 時間経過に伴うActionの実行を管理するクラス
    /// </summary>
    public class TimeHandler : MonoBehaviour
    {
        /// <summary>
        /// シングルトンのオブジェクト
        /// </summary>
        private static TimeHandler _instance;
        private static GameObject _obj;
#if UNITY_EDITOR
        private static GameObject _instanceObject;
#endif

        /// <summary>
        /// 毎フレーム実行するActionリスト
        /// </summary>
        private List<TimeAction> _timeActionsEveryFrame;

        /// <summary>
        /// 時間経過で実行するActionリスト
        /// </summary>
        private List<TimeAction> _timeActions;

        /// <summary>
        /// プレイ時間
        /// </summary>
        private RuntimeSystemConfigDataModel _playTime;

        /// <summary>
        /// GameObject アタッチなしで StartCoroutine を使うための instance
        /// </summary>
        public static TimeHandler Instance
        {
            get
            {
                if (_instance == null)
                {
#if UNITY_EDITOR
                    //Runtime実行中
                    if (EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
                    {
                        if (_obj == null)
                        {
                            _obj = new GameObject(nameof(TimeHandler));
                            DontDestroyOnLoad(_obj);
                        }
                        _instance = _obj.AddComponent<TimeHandler>();
                        _instance.CreateTimeAction();
                    }
                    //Editor実行中
                    else if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
                    {
                        //本来、Editor実行中にTimeHandlerを利用することは無いが、
                        //プレビュー表示等で呼ばれてしまうケースがあるため、一時的にインスタンスを作成する
                        if (_instanceObject == null)
                        {
                            _instanceObject = new GameObject(nameof(TimeHandler));
                        }
                        _instance = _instanceObject.AddComponent<TimeHandler>();
                        _instance.CreateTimeAction();
                        EditorApplication.update -= EditorUpdate;
                        EditorApplication.update += EditorUpdate;
                    }

                    //CB登録
                    EditorApplication.playModeStateChanged -= ChangePlayMode;
                    EditorApplication.playModeStateChanged += ChangePlayMode;
#else
                    var obj = new GameObject(nameof(TimeHandler));
                    DontDestroyOnLoad(obj);
                    _instance = obj.AddComponent<TimeHandler>();
#endif
                }

                Application.targetFrameRate = 60;
                return _instance;
            }
        }

#if UNITY_EDITOR
        private static void EditorUpdate() {
            if (_instanceObject != null)
            {
                if (EditorApplication.isPlaying)
                    Destroy(_instanceObject);
                else
                    DestroyImmediate(_instanceObject);

                if (_obj == null)
                    _instance = null;
            }
            EditorApplication.update -= EditorUpdate;
        }
#endif

        public void CreateTimeAction() {
            if (_timeActions == null)
                _timeActions = new List<TimeAction>();
        }

#if UNITY_EDITOR
        private static void ChangePlayMode(PlayModeStateChange state) {
            //Runtime実行中
            if (EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (_obj == null)
                {
                    _obj = new GameObject(nameof(TimeHandler));
                    DontDestroyOnLoad(_obj);
                }
                if (_instance == null)
                {
                    _instance = _obj.AddComponent<TimeHandler>();
                }
            }
            //Editor実行中
            else if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                //必要時にインスタンスを生成するため、特に何もしない
            }
            //切り替え中
            else
            {
                if (_instance != null)
                    Destroy(_instance);
                _instance = null;
            }
        }
#endif

        public void ClearActions() {
            if (_timeActionsEveryFrame != null)
            {
                _timeActionsEveryFrame.Clear();
                _timeActionsEveryFrame = null;
            }

            if (_timeActions != null)
            {
                _timeActions.Clear();
            }
        }

        private void OnDestroy() {
            if (_timeActionsEveryFrame != null)
            {
                _timeActionsEveryFrame.Clear();
                _timeActionsEveryFrame = null;
            }

            if (_timeActions != null)
            {
                _timeActions.Clear();
            }

            if (_obj != null)
            {
#if UNITY_EDITOR
                if (EditorApplication.isPlaying)
                    Destroy(_obj);
                else
                    DestroyImmediate(_obj);
#else
                Destroy(_obj);
#endif
                _obj = null;
            }
#if UNITY_EDITOR
            if (_instanceObject != null)
            {
                if (EditorApplication.isPlaying)
                    Destroy(_instanceObject);
                else
                    DestroyImmediate(_instanceObject);
            }
#endif
        }

        /// <summary>
        /// 実行するActionの登録（毎フレーム）
        /// </summary>
        /// <param name="action"></param>
        public void AddTimeActionEveryFrame(Action action) {
            AddTimeAction(0, action, true);
        }

        /// <summary>
        /// 実行するActionの登録（フレーム指定）
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="action"></param>
        /// <param name="loop"></param>
        public void AddTimeActionFrame(int frame, Action action, bool loop) {
            AddTimeAction(frame / 60.0f, action, loop);
        }

        /// <summary>
        /// 1msだけ処理を待ち、Actionを実行する
        /// スタックオーバーフロー対策であり、実際に待ちたいわけではない場合にのみ利用する
        /// </summary>
        /// <param name="action"></param>
        public async void WaitMillisec(Action action) {
            await Task.Delay(1);
#if UNITY_EDITOR
            if (EditorApplication.isPlaying != EditorApplication.isPlayingOrWillChangePlaymode)
            {
                //再生又は停止ボタンを押してから、実際に状態が変わるまでの間は、CB実行しない
                return;
            }
#endif
            action();
        }

        /// <summary>
        /// 実行するActionの登録
        /// </summary>
        /// <param name="executeTime"></param>
        /// <param name="action"></param>
        /// <param name="loop"></param>
        /// <param name="forceAdd">登録済みでも強制的に登録するフラグ。</param>
        public void AddTimeAction(float executeTime, Action action, bool loop, bool forceAdd = false) {
            //既にリストに登録済みのActionの場合は、登録しない
            CreateTimeAction();
            if (!forceAdd && _timeActions.Any(timeAction => timeAction.GetAction() == action))
            {
                return;
            }

            TimeAction timeAction =
                new(GameStateHandler.IsBattle() ? GameState.BATTLE : GameStateHandler.IsMenu() ? GameState.MENU : GameState.MAP, action, executeTime, 0, loop);

            //リストへ登録
            _timeActions.Add(timeAction);
            TracePrint("Add", timeAction);
        }

        /// <summary>
        /// Actionの削除
        /// </summary>
        /// <param name="action"></param>
        public void RemoveTimeAction(Action action) {
            CreateTimeAction();
            var timeAction = _timeActions.FirstOrDefault(timeAction => timeAction.GetAction() == action);
            if (timeAction == null)
            {
                return;
            }

            _timeActions.Remove(timeAction);
            TracePrint("Del", timeAction);
        }

        /// <summary>
        /// Update処理
        /// </summary>
        private void Update() {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying != EditorApplication.isPlayingOrWillChangePlaymode)
            {
                //再生又は停止ボタンを押してから、実際に状態が変わるまでの間は、CB実行しない
                return;
            }
#endif
            CreateTimeAction();

            bool ret;
            var timeActions = new List<TimeAction>(_timeActions);
            for (int i = 0; i < timeActions.Count; i++)

            {
                if (!_timeActions.Contains(timeActions[i])) continue;

                //登録されているActionに、経過した時間を通知する
                //時間経過済みの場合には、Actionを実行する
                ret = timeActions[i].UpdateTime(Time.deltaTime);

                if (ret)
                {
                    if (timeActions[i].IsEnd())

                    {
                        //Actionを実行した場合に、破棄して良い場合は、配列からデータを破棄する
                        //先に破棄しないと、ExecuteAction内で、同一のActionを再登録できないため、配列から一時変数に避けてから処理する
                        TimeAction action = timeActions[i];
                        RemoveTimeAction(timeActions[i].GetAction());

                        action.ExecuteAction();
                    }
                    else
                    {
                        timeActions[i].ExecuteAction();
                    }
                }

                if (_timeActions.Count <= i)
                    continue;
            }

            //プレイ時間を加算する
            if (_playTime != null)
                _playTime.playTime += Time.deltaTime;
        }

        /// <summary>
        /// プレイ時間を登録する
        /// </summary>
        public void SetPlayTime(RuntimeSystemConfigDataModel playTime) {
            _playTime = playTime;
        }

        [Conditional("USE_TRACE_PRINT")]
        private void TracePrint(string operateName, TimeAction operateTimerAction) {
#if USE_TRACE_PRINT
            DebugUtil.Log("");
            DebugUtil.Log($"## {operateName} {operateTimerAction.ToDetailString()}");
            foreach (var (timeAction, index) in _timeActions.Indexed())
            {
                DebugUtil.Log($"_timeActions[{index,2}] = {timeAction.ToDetailString()}");
            }
#endif
        }

        /// <summary>
        /// 現在、処理を実行して良いかどうかを返却する
        /// 本来TimeHandlerの管轄としたいものの、理由があって出来なかった箇所からの確認時に実行すること
        /// </summary>
        /// <returns></returns>
        public bool CanExecute() {
#if UNITY_EDITOR
            if (!(EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode))
            {
                //Runtime実行中以外は処理しない
                return false;
            }
#endif
            return true;
        }

        /// <summary>
        /// 時間経過で実行するActionを管理するクラス
        /// </summary>
        private class TimeAction
        {
            /// <summary>
            /// 対象のGameState
            /// </summary>
            GameState gameState;
            /// <summary>
            /// 実行するAction
            /// </summary>
            Action action;
            /// <summary>
            /// 実行するまでの時間
            /// </summary>
            float executeTime;
            /// <summary>
            /// 経過した時間
            /// </summary>
            float elapsedTime;
            /// <summary>
            /// 処理を繰り返すかどうか
            /// </summary>
            bool loop;
            /// <summary>
            /// 破棄して良いかどうか
            /// </summary>
            bool isEnd;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="gameState"></param>
            /// <param name="action"></param>
            /// <param name="executeTime"></param>
            /// <param name="elapsedTime"></param>
            /// <param name="loop"></param>
            public TimeAction(GameState gameState, Action action, float executeTime, float elapsedTime, bool loop) {
                this.gameState = gameState;
                this.action = action;
                this.executeTime = executeTime;
                this.elapsedTime = elapsedTime;
                this.loop = loop;
                this.isEnd = false;
            }

            /// <summary>
            /// 時間経過による更新処理
            /// </summary>
            /// <param name="deltaTime">経過時間。</param>
            /// <returns>アクション実行フラグ。</returns>
            public bool UpdateTime(float deltaTime) {
                //現在のGameStateが一致していなければ処理しない
                if (GameStateHandler.IsMap() && !GameStateHandler.IsMenu() && gameState != GameState.MAP ||
                    GameStateHandler.IsMenu() && gameState != GameState.MENU ||
                    GameStateHandler.IsBattle() && gameState != GameState.BATTLE)
                {
                    return false;
                }

                //毎フレーム実行するActionの場合
                if (executeTime == 0)
                {
                    //Actionを実行
                    return true;
                }

                //経過した時間を加算する
                elapsedTime += deltaTime;

                //経過した時間が、Action実行迄の時間を越えたかどうかの確認
                if (executeTime <= elapsedTime)
                {
                    //ループする場合には、経過した時間を減算する
                    if (loop)
                        elapsedTime -= executeTime;
                    else
                        isEnd = true;

                    //Actionを実行したことを通知する
                    return true;
                }

                return false;
            }

            /// <summary>
            /// Actionを実行する
            /// </summary>
            public void ExecuteAction() {
                //Actionを実行
                action();
            }

            /// <summary>
            /// Actionの返却
            /// </summary>
            /// <returns></returns>
            public Action GetAction() {
                return action;
            }

            /// <summary>
            /// 破棄していいかどうかを返却
            /// </summary>
            /// <returns></returns>
            public bool IsEnd() {
                return isEnd;
            }

            /// <summary>
            /// ループするかどうか
            /// </summary>
            /// <returns></returns>
            public bool IsLoop() {
                return loop;
            }

#if USE_TRACE_PRINT
            public string ToDetailString()
            {
                return
                    $"{{" +
                    $"{gameState}, " +
                    $"{action.GetTargetClassMethodName()}, " +
                    $"{executeTime:F2}, " +
                    $"{elapsedTime:F2}, " +
                    $"{loop}, " +
                    $"{isEnd}}}";
            }
#endif
        }
    }
}