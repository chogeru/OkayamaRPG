using System;
using System.Collections.Generic;
using System.Linq;
using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using RPGMaker.Codebase.Editor.Hierarchy.Enum;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Common.View
{
    /// <summary>
    /// HierarchyViewの基底クラス
    /// </summary>
    public class AbstractHierarchyView : VisualElement
    {
        /// <summary>
        /// 初期化中かどうか
        /// </summary>
        protected bool isInitialize = false;
        /// <summary>
        /// 再読込中かどうか
        /// </summary>
        protected bool isRefresh = false;
        /// <summary>
        /// View内の状態を保持するための ScriptableSingleton
        /// </summary>
        public class HierarchyParams : ScriptableSingleton<HierarchyParams>
        {
            /// <summary>
            /// Foldoutの開閉状態を保持(名前)
            /// </summary>
            public List<string> FoldoutsName;
            /// <summary>
            /// Foldoutの開閉状態を保持(フラグ)
            /// </summary>
            public List<bool> FoldoutsSetting;
        }

        /// <summary>
        /// UIに配置するFoldout
        /// </summary>
        protected Dictionary<string, Foldout> foldout = new Dictionary<string, Foldout>();
        protected string ThisViewName;

        /// <summary>
        /// Hierarchy名
        /// </summary>
        public virtual string ViewName { 
            get
            {
                if (ThisViewName == null)
                    ThisViewName = this.GetType().Name;
                return ThisViewName;
            }
        }
        /// <summary>
        /// UXML定義
        /// </summary>
        protected virtual string MainUxml { get; }

        /// <summary>
        /// USS定義
        /// </summary>
        protected const string MainUssLight = "Assets/RPGMaker/Codebase/Editor/Hierarchy/Region/Base/Asset/hierarchyLight.uss";
        protected const string MainUssDark = "Assets/RPGMaker/Codebase/Editor/Hierarchy/Region/Base/Asset/hierarchyDark.uss";
        
        /// <summary>
        /// TOPのVisualElement
        /// </summary>
        protected VisualElement UxmlElement { get; set; }

        ///
        ///コンテキストメニュー用変数定義
        ///
        protected const string KeyNameActor = "actorFoldout";
        protected const string KeyNameNpc = "npcFoldout";
        protected const string KeyNameVehicle = "vehicleFoldout";
        protected const string KeyNameJob = "jobFoldout";
        protected const string KeyNameEnemy = "enemyCharacterFoldout";
        protected const string KeyNameTroop = "enemyGroupFoldout";
        protected const string KeyNameCustomSkill = "customSkillFoldout";
        protected const string KeyNameCustomState = "customStateFoldout";
        protected const string KeyNameWeapon = "weaponFoldout";
        protected const string KeyNameArmor = "armorFoldout";
        protected const string KeyNameItem = "itemFoldout";
        protected const string KeyNameBattleEffect = "battle_effect_foldout";
        protected const string KeyNameEventCommon = "eventCommonFoldout";
        protected const string KeyNameAssetWalking = "walkingCharacterFoldout";
        protected const string KeyNameAssetObject = "objectFoldout";
        protected const string KeyNameAssetPopupIcon = "popupIconFoldout";
        protected const string KeyNameAssetActor= "actorFoldout";
        protected const string KeyNameAssetWeapon = "weaponFoldout";
        protected const string KeyNameAssetState = "stateOverlapFoldout";
        protected const string KeyNameAssetBattleEffect = "battleEffectFoldout";
        protected const string KeyNameElementType = "element_type";
        protected const string KeyNameSkillType  = "skill_type";
        protected const string KeyNameWeaponType = "weapon_type";
        protected const string KeyNameArmorType  = "armor_type";
        protected const string KeyNameEquipType = "equip_type";
        protected const string KeyNameSwitchSetting = "switchSettingFoldout";
        protected const string KeyNameVariableSetting = "variableSettingFoldout";
        protected const string KeyNameTileGroupList = "tileGroupListFoldout";
        protected const string KeyNameMapList = "mapListFoldout";

        ///
        /// マップ、イベント更新時の種別を定義
        ///
        public const string RefreshTypeMapCreate       = "map-create";
        public const string RefreshTypeMapDuplicate    = "map-duplicate";
        public const string RefreshTypeMapDelete       = "map-delete";
        public const string RefreshTypeMapName         = "map-name";
        public const string RefreshTypeMapSize         = "map-size";
        public const string RefreshTypeEventCreate     = "event-create";
        public const string RefreshTypeEvenDuplicate   = "event-duplicate";
        public const string RefreshTypeEventDelete     = "event-delete";
        public const string RefreshTypeEventEvCreate   = "event-ev-create";
        public const string RefreshTypeEvenEvDuplicate = "event-ev-duplicate";
        public const string RefreshTypeEventEvDelete   = "event-ev-delete";
        public const string RefreshTypeEventName       = "event-name";

        ///
        /// Outline更新時の種別を定義
        ///
        public const string RefreshTypeTitle = "title-update";
        public const string RefreshTypeChapterCreate = "chapter-create";
        public const string RefreshTypeChapterDuplicate = "chapter-duplicate";
        public const string RefreshTypeChapterDelete    = "chapter-delete";
        public const string RefreshTypeChapterName      = "chapter-name";
        public const string RefreshTypeChapterEdit      = "chapter-edit";
        public const string RefreshTypeSectionCreate    = "section-create";
        public const string RefreshTypeSectionDuplicate = "section-duplicate";
        public const string RefreshTypeSectionDelete    = "section-delete";
        public const string RefreshTypeSectionName      = "section-name";
        public const string RefreshTypeSectionEdit      = "section-edit";
        public const string RefreshTypeSectionMapAdd    = "section-map-add";
        public const string RefreshTypeSectionMapRemove = "section-map-remove";
        public const string RefreshTypeSectionUpdate = "section-update";

        /// <summary>
        /// 種別、マップID、イベントID
        /// </summary>
        protected enum RefreshType
        {
            Type = 0,
            MapId,
        }

        
        /// <summary>
        /// 表示開始番号
        /// </summary>
        protected enum DisplayStartNum
        {
            None,
            One,
            Two,
            Three,
            Four,
            Five,
        }

        /// <summary>
        /// 登録上限数
        /// </summary>
        protected enum RegistrationLimit
        {
            None = -1,
            LIMITED_99 = 99,
            LIMITED_999 = 999,
            LIMITED_9999 = 9999,
        }
        
        protected RegistrationLimit              MaxType;
        protected Dictionary<string,string> SaveUuId = new Dictionary<string,string>();
        protected string                          UuId;
        protected Dictionary<string,int> NowDataCounts = new Dictionary<string, int>();
        
        protected class ContextMenuData
        {
            public string                 UuId;
            public List<string>           Names;
            public int                    SerialNumber;
            public DisplayStartNum DisplayStartNum;
        }
        
        /// <summary>
        /// UI初期化処理
        /// </summary>
        protected virtual void InitUI() {
            if (isInitialize) return;
            isInitialize = true;

            //Foldout用の初期化
            if (HierarchyParams.instance.FoldoutsName == null)
            {
                HierarchyParams.instance.FoldoutsName = new List<string>();
                HierarchyParams.instance.FoldoutsSetting = new List<bool>();
            }

            //初期化
            Clear();

            //UXMLの読込
            if (MainUxml != "")
            {
                UxmlElement = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MainUxml).CloneTree();
                EditorLocalize.LocalizeElements(UxmlElement);
                UxmlElement.style.flexGrow = 1;
                Add(UxmlElement);
            }
            else
            {
                UxmlElement = this;
            }
            
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(MainUssDark);
            if (!EditorGUIUtility.isProSkin)
            {
                styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(MainUssLight);
            }
            UxmlElement.styleSheets.Clear();
            UxmlElement.styleSheets.Add(styleSheet);
            //各コンテンツデータの初期化
            InitContentsData();

            isInitialize = false;
        }

        /// <summary>
        /// 各コンテンツデータの初期化
        /// </summary>
        protected virtual void InitContentsData() {}

        /// <summary>
        /// リフレッシュ処理
        /// </summary>
        protected bool Refresh() {
            if (isRefresh) return false;
            isRefresh = true;
            RefreshContents();
            isRefresh = false;
            return true;
        }

        /// <summary>
        /// コンテンツのリフレッシュ
        /// </summary>
        protected virtual void RefreshContents() {}

        /// <summary>
        /// Foldout部品の登録
        /// 既に開閉状態を保持していた場合には、そのデータを復元する
        /// </summary>
        /// <param name="foldout"></param>
        public void SetFoldout(string key, Foldout data = null) {
            //keyに対して、Viewの名称を付加する
            //これにより、各画面単位で一意に定まる名称に置き換える
            string keyWork = ViewName + "_" + key;

            if (foldout.ContainsKey(keyWork))
                foldout.Remove(keyWork);

            //ScriptableSingleton に値を保持していなければ初期化処理
            if (!HierarchyParams.instance.FoldoutsName.Contains(keyWork))
            {
                HierarchyParams.instance.FoldoutsName.Add(keyWork);
                HierarchyParams.instance.FoldoutsSetting.Add(false);
            }

            //対象のFoldout部品
            Foldout foldoutData;
            if (data != null)
                foldoutData = data;
            else
                foldoutData = UxmlElement.Query<Foldout>(key);
            int foldoutIndex = HierarchyParams.instance.FoldoutsName.IndexOf(keyWork);

            //Foldoutの開閉状態を取得し、最終の設定値を ScriptableSingleton に保持
            foldoutData.RegisterValueChangedCallback(evt =>
            {
                HierarchyParams.instance.FoldoutsSetting[foldoutIndex] = foldoutData.value;
            });

            //Foldoutを管理する
            foldout.Add(keyWork, foldoutData);

            //最終の設定値を、初期値として設定
            foldoutData.value = HierarchyParams.instance.FoldoutsSetting[foldoutIndex];
        }

        /// <summary>
        /// Foldout部品を返却
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Foldout GetFoldout(string key) {
            return foldout[ViewName + "_" + key];
        }

        /// <summary>
        /// コンテキストメニューの初期設定
        /// </summary>
        /// <param name="displayType"></param>
        /// <param name="maxType"></param>
        protected void InitContextMenu(RegistrationLimit maxType) {
            MaxType = maxType;
        }

        /// <summary>
        /// コンテキストメニューの「新規作成」「貼り付け」を表示する
        /// </summary>
        /// <param name="contextMenuDic"></param>
        protected virtual void SetParentContextMenu(Dictionary<string,List<string>> contextMenuDic) {

            foreach (var dic in contextMenuDic)
            {
                var keyName = dic.Key;
                var names = dic.Value;
                BaseClickHandler.ClickEvent(GetFoldout(keyName), evt =>
                {
                    var menu = new GenericMenu();
                    var nowDataCount = NowDataCounts.ContainsKey(keyName) ? NowDataCounts[keyName] : 0;
                    //上限超えているか
                    if ((int)MaxType > nowDataCount || MaxType == RegistrationLimit.None)
                    {
                        menu.AddItem(new GUIContent(names[0]), false,
                            ()=>Create(keyName));
                    }

                    var uuId = SaveUuId.ContainsKey(keyName) ? SaveUuId[keyName] : null;
                    
                    if (!string.IsNullOrEmpty(uuId))
                    {
                        menu.AddItem(new GUIContent(names[1]), false,
                            () => Duplicate(keyName, uuId));
                    }
                    else
                    {
                        menu.AddDisabledItem(new GUIContent(names[1]));
                    
                    }

                    menu.ShowAsContext();
                });

            }
        }

        /// <summary>
        /// コンテキストメニューの「コピー」「削除」を表示する
        /// </summary>
        /// <param name="contextMenuDic"></param>
        protected virtual void SetChildContextMenu(Dictionary<string, ContextMenuData> contextMenuDic) {
            foreach (var dic in contextMenuDic)
            {
                var keyName = dic.Key;
                var uuId = dic.Value.UuId;
                var names = dic.Value.Names;
                var serialNumber = dic.Value.SerialNumber;

                var menu = new GenericMenu();

                menu.AddItem(new GUIContent(names[0]), false,
                    () => CopyDataModel(keyName, uuId));

                //表示タイプ
                if ((int) dic.Value.DisplayStartNum <= serialNumber)
                {
                    menu.AddItem(new GUIContent(names[1]), false,
                        () => Delete(keyName, uuId));
                }

                menu.ShowAsContext();
            }
        }
        
        protected virtual void Create(string keyName) {
            var visualElement = CreateDataModel(keyName);
            if (KeyNameTileGroupList != keyName)
            {
                MapEditor.MapEditor.EventRefresh(true);
                RefreshContents();
            }
            if (visualElement != null) Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(visualElement);
        }

        /// <summary>
        /// 新規作成
        /// </summary>
        protected virtual VisualElement CreateDataModel(string keyName) {
            return null;
        }

        protected void Duplicate(string keyName, string uuId) {
            var visualElement = DuplicateDataModel(keyName, uuId);
            if (KeyNameTileGroupList != keyName)
            {
                //マップエディタ
                MapEditor.MapEditor.EventRefresh(true);
                RefreshContents();
            }
            if(visualElement != null) Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(visualElement);
        }

        /// <summary>
        /// 貼り付けするデータ取得
        /// </summary>
        protected virtual VisualElement DuplicateDataModel(string keyName, string uuId) {
            return null;
        }

        /// <summary>
        /// コピー
        /// </summary>
        protected void CopyDataModel(string keyName, string uuId) {

            if (SaveUuId.ContainsKey(keyName))
            {
                SaveUuId[keyName] = uuId;
            }
            else
            {
                SaveUuId.Add(keyName, uuId);
            }
            
        }

        protected void Delete(string keyName, string uuId) {
            var visualElement = DeleteDataModel(keyName, uuId);

            //削除使用しているIDをコピーしている場合、消す
            if (SaveUuId.ContainsKey(keyName))
            {
                if (SaveUuId[keyName] == uuId)
                {
                    SaveUuId.Remove(keyName);
                }
            }

            if (KeyNameTileGroupList != keyName)
            {
                //マップエディタ
                MapEditor.MapEditor.EventRefresh(true);
                RefreshContents();
            }
            if(visualElement != null) Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(visualElement);
        }

        /// <summary>
        /// 削除するデータ取得
        /// </summary>
        protected virtual VisualElement DeleteDataModel(string keyName, string uuId) {
            return null;
        }


        /// <summary>
        /// マップ、イベント更新時の種別をリストに変換
        /// </summary>
        /// <param name="upData"></param>
        /// <returns>[0] => 種別、[1] => マップID、[2] => イベントID</returns>
        protected List<string> GetRefreshType(string upData) {
            var data = upData.Split(',');
            return data.ToList();
        }
    }

}