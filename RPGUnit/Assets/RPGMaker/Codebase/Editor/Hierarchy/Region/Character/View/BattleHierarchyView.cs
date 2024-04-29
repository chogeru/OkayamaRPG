using JetBrains.Annotations;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Enemy;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventBattle;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Troop;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Base.View.Component;
using RPGMaker.Codebase.Editor.Hierarchy.Region.Character.View.Component;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Character.View
{
    /// <summary>
    /// キャラクターのHierarchyView
    /// </summary>
    public class BattleHierarchyView : AbstractHierarchyView
    {
        // const
        //--------------------------------------------------------------------------------------------------------------
        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Hierarchy/Region/Character/Asset/database_battle.uxml"; } }

        // 利用するデータ
        //--------------------------------------------------------------------------------------------------------------
        private List<EnemyDataModel> _enemyDataModels;
        private List<EventBattleDataModel> _eventBattleDataModels;
        private List<TroopDataModel> _troopDataModels;
        private string _updateData;

        // ヒエラルキー本体クラス
        //--------------------------------------------------------------------------------------------------------------
        private readonly BattleHierarchy _battleHierarchy;

        // UI要素
        //--------------------------------------------------------------------------------------------------------------
        private HierarchyItemListView _enemyCharacterListView;
        private TroopListView _enemyGroupListView;
        private Button _battleSceneButton;
        private int _updateType = 0;

        private Dictionary<string, TroopDataModel> _troopDataModelWork = new Dictionary<string, TroopDataModel>();
        private int _eventNum = 0;

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
        /// <param name="battleHierarchy"></param>
        public BattleHierarchyView(BattleHierarchy battleHierarchy) {
            _battleHierarchy = battleHierarchy;
            InitUI();
        }

        /// <summary>
        /// 各コンテンツデータの初期化
        /// </summary>
        override protected void InitContentsData() {
            //戦闘シーン
            _battleSceneButton = UxmlElement.Query<Button>("battleSceneButton");

            //敵
            SetFoldout("enemyCharacterFoldout");
            _enemyCharacterListView = new HierarchyItemListView(ViewName);
            ((VisualElement) UxmlElement.Query<VisualElement>("enemyCharacterListContainer")).Add(_enemyCharacterListView);

            //敵グループ
            SetFoldout("enemyGroupFoldout");
            _enemyGroupListView = new TroopListView(
                _troopDataModels,
                _eventBattleDataModels,
                this
            );
            ((VisualElement) UxmlElement.Query<VisualElement>("enemyGroupListContainer")).Add(_enemyGroupListView);

            //バトルの編集
            SetFoldout("battleSettingFoldout");
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
                    KeyNameEnemy,
                    new List<string>()
                    {
                        EditorLocalize.LocalizeText("WORD_0560"), EditorLocalize.LocalizeText("WORD_0561")
                    }
                },
                {
                    KeyNameTroop,
                    new List<string>()
                    {
                        EditorLocalize.LocalizeText("WORD_0566"), EditorLocalize.LocalizeText("WORD_0567")
                    }
                }
            };
            SetParentContextMenu(dic);

            // 戦闘シーンボタンクリック時
            Editor.Hierarchy.Hierarchy.AddSelectableElementAndAction(_battleSceneButton,
                () => { _battleHierarchy.OpenBattleSceneInspector(); });
            _battleSceneButton.clickable.clicked += () =>
            {
                Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(_battleSceneButton);
            };
            
            // 敵キャラリストアイテムクリック時
            _enemyCharacterListView.SetEventHandler(
                (i, value) => { _battleHierarchy.OpenEnemyInspector(_enemyDataModels[i], this); },
                (i, value) =>
                {
                    var childDic = new Dictionary<string, ContextMenuData>
                    {
                        {
                            KeyNameEnemy, new ContextMenuData()
                            {
                                UuId = _enemyDataModels[i].id,
                                Names =
                                    new List<string>()
                                    {
                                        EditorLocalize.LocalizeText("WORD_0562"),
                                        EditorLocalize.LocalizeText("WORD_0563")
                                    },
                                SerialNumber = i,
                                DisplayStartNum = DisplayStartNum.None
                            }
                        }
                    };
                    SetChildContextMenu(childDic);
                });
            
            // 敵グループアイテムクリック時
            _enemyGroupListView.SetEventHandler(
                (i, value) => { _battleHierarchy.OpenTroopInspector(_troopDataModels[i], this); },
                (i, value) =>
                {
                    var childDic = new Dictionary<string, ContextMenuData>
                    {
                        {
                            KeyNameTroop, new ContextMenuData()
                            {
                                UuId = _troopDataModels[i].id,
                                Names =
                                    new List<string>()
                                    {
                                        EditorLocalize.LocalizeText("WORD_0568"),
                                        EditorLocalize.LocalizeText("WORD_0569"),
                                        EditorLocalize.LocalizeText("WORD_0570"),
                                        EditorLocalize.LocalizeText("WORD_0571"),
                                    },
                                SerialNumber = i,
                                DisplayStartNum = DisplayStartNum.None
                            }
                        }
                    };
                    SetChildContextMenu(childDic);
                },
                (i, value, num) =>
                {
                    _battleHierarchy.OpenTroopInspector(_troopDataModels[i], this, num);
                },
                (i, value, num) =>
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_0572")), false, () =>
                    {
                        if (_troopDataModelWork.ContainsKey(_troopDataModels[i].id + "_" + num))
                        {
                            _troopDataModelWork[_troopDataModels[i].id + "_" + num] = _troopDataModels[i];
                        }
                        else
                        {
                            _troopDataModelWork.Add(_troopDataModels[i].id + "_" + num, _troopDataModels[i]);
                        }

                        _eventNum = num;
                    });
                    menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_0573")), false, () =>
                    {
                        if (_troopDataModelWork.ContainsKey(_troopDataModels[i].id + "_" + num))
                        {
                            _troopDataModelWork.Remove(_troopDataModels[i].id + "_" + num);
                        }
                        _battleHierarchy.DeleteTroopEventDataModel(_troopDataModels[i], num);
                        //削除後バトルイベントの最後尾を選択。numが-1になれば通常の敵グループを選択
                        _battleHierarchy.OpenTroopInspector(_troopDataModels[i], this, num-1);
                        // 更新時に開閉状態が初期化されるので修正予定
                        _battleHierarchy.Refresh();
                    });
                    menu.ShowAsContext();
                });
        }

        protected override void SetChildContextMenu(Dictionary<string, ContextMenuData> contextMenuDic) {
            foreach (var dic in contextMenuDic)
            {
                var keyName = dic.Key;
                var uuId = dic.Value.UuId;
                var names = dic.Value.Names;
                var serialNumber = dic.Value.SerialNumber;

                if (keyName == KeyNameTroop)
                {
                    var troopDataModel = _troopDataModels.FirstOrDefault(t => t.id == uuId);
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent(names[0]), false,
                        () => CopyDataModel(keyName, uuId));
                    //表示タイプ
                    if ((int) dic.Value.DisplayStartNum <= serialNumber)
                    {
                        menu.AddItem(new GUIContent(names[1]), false,
                            () => Delete(keyName, uuId));
                    }

                    menu.AddItem(new GUIContent(names[2]), false,
                        () =>
                        {
                            _battleHierarchy.CreateTroopEventDataModel(troopDataModel);
                            _battleHierarchy.Refresh(troopDataModel?.id);
                        });
                    if (_troopDataModelWork.ContainsKey(troopDataModel?.id + "_" + _eventNum))
                    {
                        menu.AddItem(new GUIContent(names[3]), false,
                            () =>
                            {
                                _battleHierarchy.PasteTroopEventDataModel(_troopDataModelWork[troopDataModel?.id + "_" + _eventNum], troopDataModel,
                                    _eventNum);
                                _battleHierarchy.Refresh(troopDataModel?.id);
                            });
                    }
                    else
                    {
                        menu.AddDisabledItem(new GUIContent(names[3]));
                    }
                    
                    menu.ShowAsContext();
                }
                else
                {
                    base.SetChildContextMenu(contextMenuDic);
                }
            }
        }

        protected override VisualElement CreateDataModel(string keyName) {
            var visualElement = base.CreateDataModel(keyName);
            switch (keyName)
            {
                case KeyNameEnemy:
                    _battleHierarchy.CreateEnemyDataModel(this);
                    visualElement = LastEnemyIndex();
                    break;
                case KeyNameTroop:
                    _battleHierarchy.CreateTroopDataModel(this);
                    visualElement = LastTroopIndex();
                    break;
            }

            return visualElement;
        }

        protected override VisualElement DuplicateDataModel(string keyName, string uuId) {
            var visualElement = base.DuplicateDataModel(keyName, uuId);
            switch (keyName)
            {
                case KeyNameEnemy:
                    var enemyDataModel = _enemyDataModels.FirstOrDefault(e => e.id == uuId);
                    _battleHierarchy.PasteEnemyDataModel(this, enemyDataModel);
                    visualElement = LastEnemyIndex();
                    break;
                case KeyNameTroop:
                    var troopDataModel = _troopDataModels.FirstOrDefault(t => t.id == uuId);
                    _battleHierarchy.PasteTroopDataModel(this, troopDataModel);
                    visualElement = LastTroopIndex();
                    break;
            }
            return visualElement;
        }

        protected override VisualElement DeleteDataModel(string keyName, string uuId) {
            var visualElement =  base.DeleteDataModel(keyName, uuId);
            int index = 0;
            List<VisualElement> elements;
            switch (keyName)
            {
                case KeyNameEnemy:
                    EnemyDataModel enemyDataModel = null;
                    index = 0;
                    for (int i = 0; i < _enemyDataModels.Count; i++)
                    {
                        if (_enemyDataModels[i].id == uuId)
                        {
                            enemyDataModel = _enemyDataModels[i];
                            index = i;
                            break;
                        }
                    }
                    _battleHierarchy.DeleteEnemyDataModel(enemyDataModel);
                    elements = new List<VisualElement>();
                    _enemyCharacterListView.Query<Button>().ForEach(button => { elements.Add(button); });
                    visualElement = elements.Count - 1 < index
                        ? LastEnemyIndex()
                        : elements.FirstOrDefault(e => e.name == "BattleHierarchyView" + index);
                    break;
                case KeyNameTroop:
                    TroopDataModel troopDataModel = null;
                    index = 0;
                    for (int i = 0; i < _troopDataModels.Count; i++)
                    {
                        if (_troopDataModels[i].id == uuId)
                        {
                            troopDataModel = _troopDataModels[i];
                            index = i;
                            break;
                        }
                    }
                    _battleHierarchy.DeleteTroopDataModel(troopDataModel);
                    elements = new List<VisualElement>();
                    _enemyGroupListView.Query<Foldout>().ForEach(button => { elements.Add(button); });

                    if (elements.Count - 1 < index)
                    {
                        visualElement = LastTroopIndex();

                    }
                    else
                    {
                        var id = _troopDataModels[index].id;
                        visualElement = elements.FirstOrDefault(e => e.name == id + "_foldout");
                    }

                    break;
            }
            return visualElement;
        }

        // データ更新
        //--------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// データ更新
        /// </summary>
        /// <param name="characterActorDataModels"></param>
        /// <param name="npcCharacterActorDataModels"></param>
        /// <param name="vehiclesDataModels"></param>
        /// <param name="classDataModels"></param>
        /// <param name="enemyDataModels"></param>
        /// <param name="troopDataModels"></param>
        /// <param name="eventBattleDataModels"></param>
        /// <param name="encounterDataModels"></param>
        public void Refresh(
            string updateData = null,
            [CanBeNull] List<EnemyDataModel> enemyDataModels = null,
            [CanBeNull] List<TroopDataModel> troopDataModels = null,
            [CanBeNull] List<EventBattleDataModel> eventBattleDataModels = null,
            int updateType = 0
        ) {
            _updateData = updateData;
            _updateType = updateType;
            if (enemyDataModels != null) _enemyDataModels = enemyDataModels;
            if (troopDataModels != null) _troopDataModels = troopDataModels;
            if (eventBattleDataModels != null) _eventBattleDataModels = eventBattleDataModels;
            base.Refresh();
        }

        /// <summary>
        /// データ更新
        /// </summary>
        protected override void RefreshContents() {
            base.RefreshContents();
            if (_updateData == null)
            {
                SetEnemy();
                SetTroop();
            }
            else
            {
                UpdateEnemy();
                UpdateTroop();
            }
        }

        /// <summary>
        /// 敵の更新
        /// </summary>
        private void SetEnemy() {
            //特徴の先頭が、
            //[0]=命中率
            //[1]=回避率
            //[2]=攻撃時属性
            //となっていない場合には不正データのため、ここで補正する
            foreach (var _enemy in _enemyDataModels)
            {
                TraitCommonDataModel traitWork;
                bool flg = false;
                if (_enemy.traits == null)
                {
                    _enemy.traits = new List<TraitCommonDataModel>();
                }
                if (_enemy.traits.Count < 1 || _enemy.traits[0].categoryId != 2 || _enemy.traits[0].traitsId != 2 || _enemy.traits[0].effectId != 0)
                {
                    //0番目の特徴が命中率ではない
                    flg = false;
                    for (int i = 0; i < _enemy.traits.Count; i++)
                    {
                        if (_enemy.traits[i].categoryId == 2 && _enemy.traits[i].traitsId == 2 && _enemy.traits[i].effectId == 0)
                        {
                            //他のところに命中率があった場合、それを0番目に移動する
                            traitWork = _enemy.traits[i];
                            _enemy.traits.RemoveAt(i);
                            _enemy.traits.Insert(0, traitWork);

                            if (traitWork.value == 0)
                                traitWork.value = 950;

                            flg = true;
                            break;
                        }
                    }
                    if (!flg)
                    {
                        //他のところにも命中率がなかったので追加
                        _enemy.traits.Insert(0, new TraitCommonDataModel(2, 2, 0, 950));
                    }
                }
                if (_enemy.traits.Count < 2 || _enemy.traits[1].categoryId != 2 || _enemy.traits[1].traitsId != 2 || _enemy.traits[1].effectId != 1)
                {
                    //2番目の特徴が回避率ではない
                    flg = false;
                    for (int i = 0; i < _enemy.traits.Count; i++)
                    {
                        if (_enemy.traits[i].categoryId == 2 && _enemy.traits[i].traitsId == 2 && _enemy.traits[i].effectId == 1)
                        {
                            //他のところに回避率があった場合、それを1番目に移動する
                            traitWork = _enemy.traits[i];
                            _enemy.traits.RemoveAt(i);
                            _enemy.traits.Insert(1, traitWork);
                            flg = true;
                            break;
                        }
                    }
                    if (!flg)
                    {
                        //他のところにも回避率がなかったので追加
                        _enemy.traits.Insert(1, new TraitCommonDataModel(2, 2, 1, 50));
                    }
                }
                if (_enemy.traits.Count < 3 || _enemy.traits[2].categoryId != 3 || _enemy.traits[2].traitsId != 1)
                {
                    //3番目の特徴が攻撃時属性ではない
                    flg = false;
                    for (int i = 0; i < _enemy.traits.Count; i++)
                    {
                        if (_enemy.traits[i].categoryId == 3 && _enemy.traits[i].traitsId == 1)
                        {
                            //他のところに攻撃時属性があった場合、それを2番目に移動する
                            traitWork = _enemy.traits[i];
                            _enemy.traits.RemoveAt(i);
                            _enemy.traits.Insert(2, traitWork);
                            flg = true;
                            break;
                        }
                    }
                    if (!flg)
                    {
                        //他のところにも攻撃時属性がなかったので追加
                        _enemy.traits.Insert(2, new TraitCommonDataModel(3, 1, 2, 0));
                    }
                }
            }

            _enemyCharacterListView.Refresh(_enemyDataModels.Select(item => item.name).ToList());
        }

        private void UpdateEnemy() {
            for (int i = 0; i < _enemyDataModels.Count; i++)
            {
                if (_enemyDataModels[i].id == _updateData)
                {
                    _enemyCharacterListView.Refresh(_enemyDataModels.Select(item => item.name).ToList(), i);
                    break;
                }
            }
        }

        /// <summary>
        /// 敵グループの更新
        /// </summary>
        private void SetTroop() {
            _enemyGroupListView.Refresh(_troopDataModels, _eventBattleDataModels);
        }

        private void UpdateTroop() {
            for (int i = 0; i < _troopDataModels.Count; i++)
            {
                if (_troopDataModels[i].id == _updateData)
                {
                    _enemyGroupListView.Refresh(_troopDataModels, _eventBattleDataModels, _updateData, _updateType);
                    break;
                }
            }
        }

        /// <summary>
        /// 最終選択していた敵を返却
        /// </summary>
        /// <returns></returns>
        public VisualElement LastEnemyIndex() {
            var elements = new List<VisualElement>();
            _enemyCharacterListView.Query<Button>().ForEach(button => { elements.Add(button); });

            //選択可能なVisualElementが存在しない場合には、Inspectorを初期化して終了する
            if (elements.Count == 0)
            {
                Inspector.Inspector.Clear();
                return null;
            }

            return elements[elements.Count - 1];
        }

        /// <summary>
        /// 最終選択していた敵グループを返却
        /// </summary>
        /// <returns></returns>
        public VisualElement LastTroopIndex() {
            var elements = new List<VisualElement>();
            _enemyGroupListView.Query<Foldout>().ForEach(button => { elements.Add(button); });

            //選択可能なVisualElementが存在しない場合には、Inspectorを初期化して終了する
            if (elements.Count == 0)
            {
                Inspector.Inspector.Clear();
                return null;
            }

            return elements[elements.Count - 1];
        }

        // イベントハンドラ
        //--------------------------------------------------------------------------------------------------------------
    }
}