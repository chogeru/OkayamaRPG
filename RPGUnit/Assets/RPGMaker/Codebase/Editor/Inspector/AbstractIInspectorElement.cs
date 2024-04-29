using Assets.RPGMaker.Codebase.Editor.Common.Asset;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement;
using RPGMaker.Codebase.CoreSystem.Service.OutlineManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector
{
    public abstract class AbstractInspectorElement : VisualElement
    {
        /// <summary>
        /// DatabaseManagementService取得
        /// </summary>
        /// <returns></returns>
        protected DatabaseManagementService databaseManagementService
        {
            get
            {
                return Editor.Hierarchy.Hierarchy.databaseManagementService;
            }
        }
        /// <summary>
        /// EventManagementService取得
        /// </summary>
        /// <returns></returns>
        protected EventManagementService eventManagementService
        {
            get
            {
                return Editor.Hierarchy.Hierarchy.eventManagementService;
            }
        }
        /// <summary>
        /// MapManagementService取得
        /// </summary>
        /// <returns></returns>
        protected MapManagementService mapManagementService
        {
            get
            {
                return Editor.Hierarchy.Hierarchy.mapManagementService;
            }
        }
        /// <summary>
        /// OutlineManagementService取得
        /// </summary>
        /// <returns></returns>
        protected OutlineManagementService outlineManagementService
        {
            get
            {
                return Editor.Hierarchy.Hierarchy.outlineManagementService;
            }
        }

        /// <summary>
        /// View内の状態を保持するための ScriptableSingleton
        /// </summary>
        public class InspectorParams : ScriptableSingleton<InspectorParams>
        {
            /// <summary>
            /// Foldoutの開閉状態を保持(名前)
            /// </summary>
            public List<string> FoldoutsName;
            /// <summary>
            /// Foldoutの開閉状態を保持(フラグ)
            /// </summary>
            public List<bool> FoldoutsSetting;
            /// <summary>
            /// スクロール位置の保持
            /// </summary>
            public float ScrollOffset;
        }

        /// <summary>
        /// UIに配置するFoldout
        /// </summary>
        protected Dictionary<string, Foldout> foldout = new Dictionary<string, Foldout>();
        protected string ThisViewName;
        
        

        /// <summary>
        /// Hierarchy名
        /// </summary>
        public virtual string ViewName
        {
            get
            {
                if (ThisViewName == null)
                    ThisViewName = this.GetType().Name;
                return ThisViewName;
            }
            set 
            {
                ViewName = value;
            }
        }

        /// <summary>
        /// UXML定義
        /// </summary>
        protected virtual string MainUxml { get; set; }
        /// <summary>
        /// Root VisualElement
        /// </summary>
        protected VisualElement RootContainer;

        protected bool _canInitialize = true;
        protected bool _canUpdate = true;
        protected bool _canRefresh = true;
        protected bool _canSave = true;

        protected bool _forceScroll = false;

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Initialize() {
            if (!_canInitialize) return;
            _canInitialize = false;
            InitializeContents();
            CanInitializeOn();
            InitializeButtonLabel();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        protected virtual void InitializeContents() {
            Clear();
            RootContainer = new VisualElement();
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MainUxml);
            VisualElement labelFromXml = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromXml);
            labelFromXml.style.flexGrow = 1;
            RootContainer.Add(labelFromXml);
            Add(RootContainer);
        }

        /// <summary>
        /// 初期化を再度行えるようにする
        /// </summary>
        protected async void CanInitializeOn() {
            //InitializeContentsで、コンテンツを読み込み後にFoldoutの状態を更新
            List<Foldout> data = new List<Foldout>();
            GetAllFoldout(data, this);

            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].name == "")
                {
                    data[i].name = "foldout_autoset_" + i;
                }
                SetFoldout(data[i]);
            }

            //スクロール位置を設定
            ScrollView scrollView = this.Query<ScrollView>().First();
            if (scrollView != null)
                scrollView.scrollOffset = new UnityEngine.Vector2(0f, InspectorParams.instance.ScrollOffset);

            await Task.Delay(1);
            _canInitialize = true;
        }



        /// <summary>
        /// ボタンの幅で、ラベルに3点リーダーをつける
        /// </summary>
        protected void InitializeButtonLabel() {
            List<Button> data = new List<Button>();
            GetAllButton(data, this);

            List<string> strWork = new List<string>();
            List<int> strWidth = new List<int>();


            for (int i = 0; i < data.Count; i++)
            {
                strWork.Add(data[i].text);
                strWidth.Add((int) data[i].contentRect.width);
            }

            this.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                EditorUpdate();
            });

            void EditorUpdate() {
                for (int i = 0; i < data.Count; i++)
                {
                    if (strWidth[i] == (int) data[i].contentRect.width) continue;
                    for (int j = strWork[i].Length; j > 0; j--)
                    {
                        var s = strWork[i].Substring(0, j);
                        if (j != 1 && GetTextWidth(s) > data[i].contentRect.width)
                        {
                            continue;
                        }

                        if (j == strWork[i].Length)
                        {
                            data[i].text = s;
                            data[i].tooltip = "";
                            break;
                        }

                        if (j - 2 <= 1)
                        {
                            j = 3;
                        }

                        data[i].text = s.Substring(0, j - 2) + "...";
                        data[i].tooltip = strWork[i];
                        break;
                    }

                    strWidth[i] = (int) data[i].contentRect.width;
                }
            }
        }

        /// <summary>
        /// テキストの長さからWidthを取得
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        protected float GetTextWidth(string text) {
            try
            {
                var size = EditorStyles.label.CalcSize(text);
                return size.x;
            }
            catch (Exception)
            {
                //
            }
            return 0f;
        }

        /// <summary>
        /// 画面内に存在する全てのFoldoutを取得する
        /// </summary>
        /// <param name="data"></param>
        /// <param name="me"></param>
        protected void GetAllFoldout(List<Foldout> data, VisualElement me) {
            if (me is Foldout)
                data.Add((Foldout) me);

            foreach (VisualElement child in me.Children())
                GetAllFoldout(data, child);
        }

        /// <summary>
        /// 画面内に存在する全てのButtonを取得する
        /// </summary>
        /// <param name="data"></param>
        /// <param name="me"></param>
        protected void GetAllButton(List<Button> data, VisualElement me) {
            if (me is Button)
            {
                var parent = me.parent;
                if(!(parent is PopupFieldBase<string>))
                { 
                    data.Add((Button) me);
                }
            }

            foreach (VisualElement child in me.Children())
                GetAllButton(data, child);
        }

        /// <summary>
        /// データの更新
        /// </summary>
        public void Refresh() {
            if (!_canRefresh) return;
            _canRefresh = false;
            RefreshContents();
            CanRefreshOn();
        }
        
        public void RefreshScroll() {
            _forceScroll = true;
            SaveScroll(true);
            Refresh();
        }

        /// <summary>
        /// データの更新を再度行えるようにする
        /// </summary>
        protected async void CanRefreshOn() {
            await Task.Delay(1);
            _canRefresh = true;
            if (_forceScroll)
            {
                ScrollView scrollView = this.Query<ScrollView>().First();
                if (scrollView != null)
                    scrollView.scrollOffset = new UnityEngine.Vector2(0f, InspectorParams.instance.ScrollOffset);
                _forceScroll = false;
            }
        }

        /// <summary>
        /// データの更新
        /// </summary>
        protected virtual void RefreshContents() { }

        /// <summary>
        /// セーブ処理
        /// </summary>
        protected void Save() {
            if (!_canSave) return;

            //初期化中は処理しない
            if (!_canInitialize) return;

            //アップデート中も処理しない
            if (!_canRefresh) return;

            _canSave = false;
            CanSaveOn();
        }

        /// <summary>
        /// セーブ処理
        /// </summary>
        protected virtual void SaveContents() { }

        /// <summary>
        /// セーブを再度行えるようにする
        /// </summary>
        protected async void CanSaveOn() {
            await Task.Delay(1);
            //実際のセーブタイミングを、Wait後とし、複数回セーブを実行されたとしても1回のみとする
            SaveContents();
            _canSave = true;
        }

        /// <summary>
        /// Foldout部品の登録
        /// 既に開閉状態を保持していた場合には、そのデータを復元する
        /// </summary>
        /// <param name="foldout"></param>
        public void SetFoldout(Foldout foldoutData) {
            //keyに対して、Viewの名称を付加する
            //これにより、各画面単位で一意に定まる名称に置き換える
            string keyWork = ViewName + "_" + foldoutData.name;

            if (foldout.ContainsKey(keyWork))
                foldout.Remove(keyWork);

            //ScriptableSingleton に値を保持していなければ初期化処理
            //Foldout用の初期化
            if (InspectorParams.instance.FoldoutsName == null)
            {
                InspectorParams.instance.FoldoutsName = new List<string>();
                InspectorParams.instance.FoldoutsSetting = new List<bool>();
            }

            //初期化時の値は、画面描画直後の開閉状態
            if (!InspectorParams.instance.FoldoutsName.Contains(keyWork))
            {
                InspectorParams.instance.FoldoutsName.Add(keyWork);
                InspectorParams.instance.FoldoutsSetting.Add(foldoutData.value);
            }

            int foldoutIndex = InspectorParams.instance.FoldoutsName.IndexOf(keyWork);

            //Foldoutの開閉状態を取得し、最終の設定値を ScriptableSingleton に保持
            foldoutData.RegisterValueChangedCallback(evt =>
            {
                InspectorParams.instance.FoldoutsSetting[foldoutIndex] = foldoutData.value;
            });

            //Foldoutを管理する
            foldout.Add(keyWork, foldoutData);

            //最終の設定値を、初期値として設定
            foldoutData.value = InspectorParams.instance.FoldoutsSetting[foldoutIndex];
        }

        public void SaveScroll(bool saved) {
            ScrollView scrollView = this.Query<ScrollView>().First();

            if (scrollView == null)
            {
                InspectorParams.instance.ScrollOffset = 0;
                return;
            }

            if (saved)
                InspectorParams.instance.ScrollOffset = scrollView.scrollOffset.y;
            else
                InspectorParams.instance.ScrollOffset = 0;
        }
    }
}