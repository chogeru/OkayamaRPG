#if UNITY_EDITOR
#define USE_SCRIPTABLE_SINGLETON
using UnityEditor;
#endif
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Encounter;
using System;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.Misc
{
    /// <summary>
    /// マップとバトル間でのデータ受渡用クラス
    /// </summary>
    [Serializable]
    public class BattleSceneTransition
    {
        /// <summary>
        /// 選択されたTroopId
        /// </summary>
        private string selectTroopId = string.Empty;
        /// <summary>
        /// バトル背景1
        /// </summary>
        private string backgroundImage1 = string.Empty;
        /// <summary>
        /// バトル背景2
        /// </summary>
        private string backgroundImage2 = string.Empty;
        /// <summary>
        /// イベントのマップからバトル背景1設定
        /// </summary>
        private string _eventMapBackgroundImage1 = string.Empty;
        /// <summary>
        /// イベントのマップからバトル背景2設定
        /// </summary>
        private string _eventMapBackgroundImage2 = string.Empty;
        /// <summary>
        /// 逃走可否
        /// </summary>
        private bool canEscape;
        /// <summary>
        /// 敗北可否
        /// </summary>
        private bool canLose;
        /// <summary>
        /// シングルトン
        /// </summary>
        public static BattleSceneTransition Instance => Singleton.Instance;

        /// <summary>
        /// 逃走可否設定
        /// </summary>
        public bool CanEscape
        {
            get => canEscape;
            set => canEscape = value;
        }
        /// <summary>
        /// 敗北可否設定
        /// </summary>
        public bool CanLose
        {
            get => canLose;
            set => canLose = value;
        }
        /// <summary>
        /// 敵グループID
        /// </summary>
        public string SelectTroopId
        {
            set => selectTroopId = value;
            get => selectTroopId;
        }
        /// <summary>
        /// マップのタイルごとに設定されているリージョンidに紐付けられた
        /// エンカウントデータに設定されているバトル背景画像1
        /// </summary>
        public string EncounterDataBackgroundImage1
        {
            set => backgroundImage1 = value;
            get => backgroundImage1;
        }
        /// <summary>
        /// マップのタイルごとに設定されているリージョンidに紐付けられた
        /// エンカウントデータに設定されているバトル背景画像2
        /// </summary>
        public string EncounterDataBackgroundImage2
        {
            set => backgroundImage2 = value;
            get => backgroundImage2;
        }

        public string EventMapBackgroundImage1
        {
            get
            {
                return _eventMapBackgroundImage1;
            }
            set
            {
                _eventMapBackgroundImage1 = value;
            }
        }

        public string EventMapBackgroundImage2
        {
            get => _eventMapBackgroundImage2;
            set => _eventMapBackgroundImage2 = value;
        }

        /// <summary>
        /// マップのエンカウントデータモデル
        /// </summary>
        public EncounterDataModel EncounterDataModel { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private BattleSceneTransition() {
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public static void Initialize() {
            Singleton.Initialize();
        }

        /// <summary>
        /// 戦闘テストでセーブデータを作成する為のもの
        /// </summary>
        public Actor[] Actors { get; set; }

        /// <summary>
        /// 戦闘テストで利用するアクター情報
        /// </summary>
        [Serializable]
        public class Actor
        {
            public string[] equipIds;
            public string   id;
            public int      level;
        }

#if USE_SCRIPTABLE_SINGLETON
        /// <summary>
        /// シングルトン
        /// </summary>
        private class Singleton : ScriptableSingleton<Singleton>
        {
            private BattleSceneTransition battleSceneTransition;

            public static BattleSceneTransition Instance
            {
                get
                {
                    instance.battleSceneTransition ??= new BattleSceneTransition();
                    return instance.battleSceneTransition;
                }
            }

            public static void Initialize() {
                instance.battleSceneTransition = null;
            }
        }
#else
        /// <summary>
        /// シングルトン
        /// </summary>
        public class Singleton
        {
            private static BattleSceneTransition battleSceneTransition;

            public static BattleSceneTransition Instance
            {
                get
                {
                    battleSceneTransition ??= new BattleSceneTransition();
                    return battleSceneTransition;
                }
            }

            public static void Initialize()
            {
                battleSceneTransition = null;
            }
        }
#endif
    }
}