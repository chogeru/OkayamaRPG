using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.State;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Base.View.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.State.View
{
    /// <summary>
    /// ステートのHierarchyView
    /// </summary>
    public class StateHierarchyView : AbstractHierarchyView
    {
        // const
        //--------------------------------------------------------------------------------------------------------------
        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Hierarchy/Region/State/Asset/database_state.uxml"; } }

        private StateDataModel        _copyDataModel;
        private VisualElement         _customStateListContainer;
        private HierarchyItemListView _customStateListView;
        private Button                _defenseButton;

        // 利用するデータ
        //--------------------------------------------------------------------------------------------------------------
        private List<StateDataModel> _stateDataModels;

        // ヒエラルキー本体
        //--------------------------------------------------------------------------------------------------------------
        private readonly StateHierarchy _stateHierarchy;

        // UI要素
        //--------------------------------------------------------------------------------------------------------------
        private Button _unableToFightButton;

        // 状態
        //--------------------------------------------------------------------------------------------------------------
        private readonly int stateSkipIndex = 2;

        // その他
        //--------------------------------------------------------------------------------------------------------------
        private const int foldoutCount = 2;

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
        /// <param name="stateHierarchy"></param>
        public StateHierarchyView(StateHierarchy stateHierarchy) {
            _stateHierarchy = stateHierarchy;
            InitUI();
        }

        /// <summary>
        /// 各コンテンツデータの初期化
        /// </summary>
        override protected void InitContentsData() {
            _unableToFightButton = UxmlElement.Query<Button>("unable_fight_state_button");
            _defenseButton = UxmlElement.Query<Button>("defense_state_button");
            SetFoldout("customStateFoldout");
            _customStateListContainer = UxmlElement.Query<VisualElement>("state_custom_list");
            _customStateListView = new HierarchyItemListView(ViewName);
            _customStateListContainer.Add(_customStateListView);

            //Foldoutの開閉状態保持用
            for (int i = 0; i < foldoutCount; i++)
                SetFoldout("foldout_" + (i + 1));

            InitEventHandlers();
        }

        /// <summary>
        /// イベントの初期設定
        /// </summary>
        private void InitEventHandlers() {
            Editor.Hierarchy.Hierarchy.AddSelectableElementAndAction(_unableToFightButton,
                () => { _stateHierarchy.OpenStateInspector(_stateDataModels[0]); });
            _unableToFightButton.clickable.clicked += () =>
            {
                Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(_unableToFightButton);
            };

            Editor.Hierarchy.Hierarchy.AddSelectableElementAndAction(_defenseButton,
                () => { _stateHierarchy.OpenStateInspector(_stateDataModels[1]); });
            _defenseButton.clickable.clicked += () =>
            {
                Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(_defenseButton);
            };
            
            InitContextMenu(RegistrationLimit.None);
            // カスタムステートFoldout右クリック時
            var dic = new Dictionary<string, List<string>>
            {
                {
                    KeyNameCustomState,
                    new List<string>()
                    {
                        EditorLocalize.LocalizeText("WORD_0660"), EditorLocalize.LocalizeText("WORD_0661")
                    }
                }
            };
            SetParentContextMenu(dic);
            
            _customStateListView.SetEventHandler(
                (i, value) => { _stateHierarchy.OpenStateInspector(_stateDataModels[i + stateSkipIndex]); },
                (i, value) =>
                {
                    var childDic = new Dictionary<string, ContextMenuData>
                    {
                        {
                            KeyNameCustomState, new ContextMenuData()
                            {
                                UuId = _stateDataModels[i + stateSkipIndex].id,
                                Names =
                                    new List<string>()
                                    {
                                        EditorLocalize.LocalizeText("WORD_0662"),
                                        EditorLocalize.LocalizeText("WORD_0663")
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
            var visualElement =  base.CreateDataModel(keyName);
            if (keyName == KeyNameCustomState)
            {
                _stateHierarchy.CreateStateDataModel();
                visualElement = LastStateIndex();
            }

            return visualElement;
        }

        protected override VisualElement DuplicateDataModel(string keyName, string uuId) {
            var visualElement =  base.DuplicateDataModel(keyName, uuId);
            if (keyName == KeyNameCustomState)
            {
                var stateDataModel = _stateDataModels.FirstOrDefault(s => s.id == uuId);
                _stateHierarchy.DuplicateStateDataModel(stateDataModel);
                visualElement = LastStateIndex();
            }
            return visualElement;
        }

        protected override VisualElement DeleteDataModel(string keyName, string uuId) {
            var visualElement =  base.DeleteDataModel(keyName, uuId);
            if (keyName == KeyNameCustomState)
            {
                StateDataModel stateDataModel = null;
                int index = 0;
                for (int i = 0; i < _stateDataModels.Count; i++)
                {
                    if (_stateDataModels[i].id == uuId)
                    {
                        stateDataModel = _stateDataModels[i];
                        index = i;
                        break;
                    }
                }
                _stateHierarchy.DeleteStateDataModel(stateDataModel);
                var elements = new List<VisualElement>();
                _customStateListView.Query<Button>().ForEach(button => { elements.Add(button); });
                visualElement = elements.Count < index ? LastStateIndex() : elements.FirstOrDefault(e => e.name == "StateHierarchyView" + (index - stateSkipIndex));
            }
            return visualElement;
        }

        /// <summary>
        /// データ更新
        /// </summary>
        /// <param name="stateDataModels"></param>
        public void Refresh([CanBeNull] List<StateDataModel> stateDataModels = null) {
            if (stateDataModels != null) _stateDataModels = stateDataModels;
            base.Refresh();
        }

        /// <summary>
        /// データ更新
        /// </summary>
        protected override void RefreshContents() {
            base.RefreshContents();
            var customStateList = new ArraySegment<StateDataModel>(_stateDataModels.ToArray(), stateSkipIndex, _stateDataModels.Count - stateSkipIndex);
            _customStateListView.Refresh(customStateList.Select(item => item.name).ToList());
        }

        /// <summary>
        /// 最終選択していたステートを返却
        /// </summary>
        /// <returns></returns>
        public VisualElement LastStateIndex() {
            var elements = new List<VisualElement>();
            _customStateListView.Query<Button>().ForEach(button => { elements.Add(button); });

            //選択可能なVisualElementが存在しない場合には、Inspectorを初期化して終了する
            if (elements.Count == 0)
            {
                Inspector.Inspector.Clear();
                return null;
            }

            return elements[elements.Count - 1];
        }
    }
}