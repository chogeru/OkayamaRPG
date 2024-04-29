using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventCommon;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Base.View.Component;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.CommonEvent.View
{
    /// <summary>
    /// コモンイベントのHierarchyView
    /// </summary>
    public class CommonEventHierarchyView : AbstractHierarchyView
    {
        // const
        //--------------------------------------------------------------------------------------------------------------
        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Hierarchy/Region/CommonEvent/Asset/database_common.uxml"; } }

        // ヒエラルキー本体クラス
        //--------------------------------------------------------------------------------------------------------------
        private readonly CommonEventHierarchy _commonEventHierarchy;

        // 利用するデータ
        //--------------------------------------------------------------------------------------------------------------
        private List<EventCommonDataModel> _eventCommonDataModels;

        // UI要素
        //--------------------------------------------------------------------------------------------------------------
        private HierarchyItemListView _eventCommonListView;
        private string _tagClassName;

        //--------------------------------------------------------------------------------------------------------------
        //
        // methods
        //
        //--------------------------------------------------------------------------------------------------------------

        // 初期化・更新系
        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="commonEventHierarchy"></param>
        public CommonEventHierarchyView(CommonEventHierarchy commonEventHierarchy) {
            _commonEventHierarchy = commonEventHierarchy;
            InitUI();
        }

        /// <summary>
        /// 各コンテンツデータの初期化
        /// </summary>
        override protected void InitContentsData() {
            SetFoldout("eventCommonFoldout");
            _eventCommonListView = new HierarchyItemListView(ViewName);
            ((VisualElement) UxmlElement.Query<VisualElement>("common_event_list")).Add(_eventCommonListView);

            InitEventHandlers();
        }

        /// <summary>
        /// イベントの初期設定
        /// </summary>
        private void InitEventHandlers() {
            InitContextMenu(RegistrationLimit.None);
            var dic = new Dictionary<string, List<string>>
            {
                {
                    KeyNameEventCommon,
                    new List<string>()
                    {
                        EditorLocalize.LocalizeText("WORD_0883"), EditorLocalize.LocalizeText("WORD_0884")
                    }
                }
            };
            SetParentContextMenu(dic);

            _eventCommonListView.SetEventHandler(
                (i, value) => { _commonEventHierarchy.OpenEventCommonInspector(_eventCommonDataModels[i]); },
                (i, value) =>
                {
                    var childDic = new Dictionary<string, ContextMenuData>
                    {
                        {
                            KeyNameEventCommon, new ContextMenuData()
                            {
                                UuId = _eventCommonDataModels[i].eventId,
                                Names =
                                    new List<string>()
                                    {
                                        EditorLocalize.LocalizeText("WORD_0015"),
                                        EditorLocalize.LocalizeText("WORD_0886")
                                    },
                                SerialNumber = i,
                                DisplayStartNum = DisplayStartNum.None
                            }
                        }
                    };
                    SetChildContextMenu(childDic);
                });
        }

        protected override VisualElement CreateDataModel(string keyName) {
            var visualElement = base.CreateDataModel(keyName);
            if (keyName == KeyNameEventCommon)
            {
                _commonEventHierarchy.CreateEventCommonDataModel();
                visualElement = LastCommonEventIndex();
            }

            return visualElement;
        }

        protected override VisualElement DuplicateDataModel(string keyName, string uuId) {
            var visualElement =  base.DuplicateDataModel(keyName, uuId);
            if (keyName == KeyNameEventCommon)
            {
                var commonDataModel = _eventCommonDataModels.FirstOrDefault(s => s.eventId == uuId);
                _commonEventHierarchy.DuplicateEventCommonDataModel(commonDataModel);
                visualElement = LastCommonEventIndex();
            }
            return visualElement;
            
        }

        protected override VisualElement DeleteDataModel(string keyName, string uuId) {
            var visualElement =  base.DeleteDataModel(keyName, uuId);
            if (keyName == KeyNameEventCommon)
            {
                EventCommonDataModel commonDataModel = null;
                int index = 0;
                for (int i = 0; i < _eventCommonDataModels.Count; i++)
                {
                    if (_eventCommonDataModels[i].eventId == uuId)
                    {
                        commonDataModel = _eventCommonDataModels[i];
                        index = i;
                        break;
                    }
                }
                _commonEventHierarchy.DeleteEventCommonDataModel(commonDataModel);
                var elements = new List<VisualElement>();
                _eventCommonListView.Query<Button>().ForEach(button => { elements.Add(button); });
                visualElement = elements.Count - 1 < index ? LastCommonEventIndex() : elements.FirstOrDefault(e => e.name == "CommonEventHierarchyView" + index);
            }
            return visualElement;
        }

        /// <summary>
        /// データ更新
        /// </summary>
        /// <param name="eventCommonDataModels"></param>
        /// <param name="tagClassName"></param>
        public void Refresh(
            [CanBeNull] List<EventCommonDataModel> eventCommonDataModels = null, string tagClassName = null)
        {
            _eventCommonDataModels = eventCommonDataModels ?? _eventCommonDataModels;
            _tagClassName = tagClassName;
            base.Refresh();
        }

        /// <summary>
        /// データ更新
        /// </summary>
        protected override void RefreshContents() {
            base.RefreshContents();
            _eventCommonListView.Refresh(_eventCommonDataModels.Select(item => item.name).ToList());

            // ボタンの種類判別用に、未定義のクラス名をタグとして追加する。
            if (_tagClassName != null)
            {
                _eventCommonListView.Query<Button>().ForEach(button => { button.AddToClassList(_tagClassName); });
            }
        }

        public VisualElement LastCommonEventIndex() {
            var elements = new List<VisualElement>();
            _eventCommonListView.Query<Button>().ForEach(button => { elements.Add(button); });

            //選択可能なVisualElementが存在しない場合には、Inspectorを初期化して終了する
            if (elements.Count == 0)
            {
                Inspector.Inspector.Clear();
                WindowLayoutManager.GetActiveWindow(WindowLayoutManager.WindowLayoutId.MapEventCommandSettingWindow)?.Close();
                WindowLayoutManager.GetActiveWindow(WindowLayoutManager.WindowLayoutId.MapEventExecutionContentsWindow)?.Close();
                return null;
            }

            return elements[elements.Count - 1];
        }
    }
}