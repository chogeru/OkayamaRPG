using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventBattle;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Troop;
using RPGMaker.Codebase.Editor.Common;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Character.View.Component
{
    /**
     * 敵グループリストコンポーネント
     * (仕様上の表示不具合がある為ListViewからScrollViewに変更)
     */
    public class TroopListView : ScrollView
    {
        // データプロパティ
        private List<TroopDataModel> _troopDataModels;
        private List<EventBattleDataModel> _eventBattleDataModels;
        private BattleHierarchyView _battleHierarchyView;
        private string _updateData;

        // アクション
        private List<Action<int, string>> _action;
        private List<Action<int, string, int>> _actionEvent;

        /**
         * コンストラクタ
         */
        public TroopListView(
            List<TroopDataModel> troopDataModels,
            List<EventBattleDataModel> eventBattleDataModels,
            BattleHierarchyView battleHierarchyView
        ) {
            _troopDataModels = troopDataModels;
            _eventBattleDataModels = eventBattleDataModels;
            _battleHierarchyView = battleHierarchyView;
            style.flexDirection = FlexDirection.Column;
        }

        public void SetEventHandler(
            Action<int, string> onLeftClick, 
            Action<int, string> onRightClick, 
            Action<int, string, int> onLeftClickEvent,
            Action<int, string, int> onRightClickEvent
        ) {
            _action = new List<Action<int, string>>();
            _action.Add(onLeftClick);
            _action.Add(onRightClick);

            _actionEvent = new List<Action<int, string, int>>();
            _actionEvent.Add(onLeftClickEvent);
            _actionEvent.Add(onRightClickEvent);

            SetItem();
        }

        /**
         * データおよび表示を更新
         */
        public void Refresh(
            [CanBeNull] List<TroopDataModel> items = null,
            [CanBeNull] List<EventBattleDataModel> eventBattleDataModels = null,
            string updateData = null,
            int updateType = 0
        ) {
            _updateData = updateData;
            if (items != null) _troopDataModels = items;
            if (eventBattleDataModels != null) _eventBattleDataModels = eventBattleDataModels;
            if (updateData == null)
                SetItem();
            else
                UpdateItem(updateType);
        }

        /**
         * データの作成
         * （ScrollViewの仕様でRefreshがない為データ更新時に再作成する）
         */
        private void SetItem() {
            if(_troopDataModels != null)
            {
                contentViewport.Clear();
                for (int i = 0; i < _troopDataModels.Count; i++)
                {
                    int num = i;
                    VisualElement foldoutElement = new VisualElement();
                    var foldout = new Foldout();
                    foldout.name = _troopDataModels[i].id + "_foldout";

                    var foldoutLabel = new Label {text = _troopDataModels[num].name};
                    foldoutLabel.name = foldout.name + "_label";
                    foldoutLabel.style.position = Position.Absolute;
                    foldoutLabel.style.left = 35f;
                    foldoutLabel.style.right = 2f;
                    foldoutLabel.style.overflow = Overflow.Hidden;
                    foldoutLabel.style.paddingTop = 2f;
                    foldoutElement.Add(foldout);
                    foldoutElement.Add(foldoutLabel);

                    //Foldoutのクリックイベント
                    Editor.Hierarchy.Hierarchy.AddSelectableElementAndAction(foldout, () =>
                    {
                        _action[0].Invoke(num, _troopDataModels[num].name);
                    });
                    foldoutLabel.RegisterCallback<ClickEvent>(evt =>
                    {
                        Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(foldout);
                    }); 
                    
                    BaseClickHandler.ClickEvent(foldoutLabel, (evt) =>
                    {
                        if (evt == (int) MouseButton.RightMouse)
                        {
                            _action[1](num, _troopDataModels[num].name);
                        }
                    });
                    foldout.value = false;
                    contentViewport.Add(foldoutElement);
                    _battleHierarchyView.SetFoldout(_troopDataModels[i].id + "_foldout", foldout);

                    // 敵グループごとにバトルイベントページを追加
                    CreateFoldoutContents(foldout, i, 0);
                }
            }
        }

        private void UpdateItem(int updateType) {
            Foldout foldout = _battleHierarchyView.GetFoldout(_updateData + "_foldout");
            Label label = contentViewport.Query<Label>(_updateData + "_foldout_label");
            for (int i = 0; i < _troopDataModels.Count; i++)
            {
                if (_troopDataModels[i].id == _updateData)
                {
                    label.text = _troopDataModels[i].name;
                    CreateFoldoutContents(foldout, i, updateType);
                    break;
                }
            }
        }

        private void CreateFoldoutContents(Foldout foldout, int num, int updateType) {
            // 敵グループごとにバトルイベントページを追加
            foldout.Clear();
            var eventBattle = _eventBattleDataModels.Find(item => item.eventId == _troopDataModels[num].battleEventId);
            if (eventBattle == null) return;

            for (int pageNo = 0; pageNo < eventBattle.pages.Count; pageNo++)
            {
                Button btn = new Button();
                btn.text = EditorLocalize.LocalizeText("WORD_0565") + "#" + string.Format("{0:D4}", pageNo + 1);
                btn.name = eventBattle.eventId + "-" + pageNo;
                var troopIndex = num;
                var troopPageNo = pageNo;

                //各イベントページのクリックイベント
                BaseClickHandler.ClickEvent(btn, (evt) =>
                {
                    if (evt == (int) MouseButton.RightMouse)
                    {
                        _actionEvent[1].Invoke(troopIndex, _troopDataModels[troopIndex].name, troopPageNo);
                    }
                    else
                    {
                        _actionEvent[0].Invoke(troopIndex, _troopDataModels[troopIndex].name, troopPageNo);
                        RPGMaker.Codebase.Editor.Hierarchy.Hierarchy.ScrollTo(btn);
                    }
                });
                foldout.Add(btn);

                //新規にイベントを作成した場合には、そこまでフォーカスを移動する
                if (updateType == 1 && pageNo == eventBattle.pages.Count - 1)
                {
                    //この場合はFoldoutを開ける
                    foldout.value = true;
                    //クリックされた時の動作を行う
                    _actionEvent[0].Invoke(troopIndex, _troopDataModels[troopIndex].name, troopPageNo);
                    RPGMaker.Codebase.Editor.Hierarchy.Hierarchy.ScrollTo(btn);
                }
            }
        }
    }
}