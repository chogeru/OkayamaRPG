using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement.Repository;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Hierarchy.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Common.Window.ModalWindow
{
    public class MapCreateForSampleMapModalWindow : BaseModalWindow
    {
        // テキスト
        private const string PROCESS_TEXT = "WORD_0025";
        private const string COPY_NAME    = "WORD_1462";
        private       Button _CANCEL_button;

        //表示要素
        private VisualElement _mapSelect;
        private Button        _OK_button;

        protected override string ModalUxml =>
            "Assets/RPGMaker/Codebase/Editor/Common/Window/ModalWindow/Uxml/map_create_for_samplemap.uxml";

        public void ShowWindow() {
            var wnd = GetWindow<MapCreateForSampleMapModalWindow>();

            // 処理タイトル名適用
            wnd.titleContent = new GUIContent(EditorLocalize.LocalizeText(PROCESS_TEXT));
            wnd.Init();
            //サイズ固定用
            var size = new Vector2(280, 105);
            wnd.minSize = size;
            wnd.maxSize = size;
            wnd.maximized = false;
        }

        public override void Init() {
            var root = rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ModalUxml);
            VisualElement labelFromUxml = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromUxml);
            root.Add(labelFromUxml);

            // element取得
            _mapSelect = labelFromUxml.Query<VisualElement>("mapSelect");

            var mapManagementService = Editor.Hierarchy.Hierarchy.mapManagementService;
            var sampleMapDatas = mapManagementService.LoadMapSamples();

            // マップ名リスト
            var mapNameList = new List<string>();
            foreach (var data in sampleMapDatas)
                mapNameList.Add(data.name);

            var mapSelectPopupField = new PopupFieldBase<string>(mapNameList, 0);
            _mapSelect.Clear();
            _mapSelect.Add(mapSelectPopupField);

            //OKボタン
            _OK_button = labelFromUxml.Query<Button>("OK_button");
            _OK_button.clicked += () =>
            {
                // 指定のマップデータのIDのみ変更&Prefab複製
                var mapData = MapDataModel.CopyData(sampleMapDatas[mapSelectPopupField.index]);
                mapData.id = Guid.NewGuid().ToString();

                if (mapManagementService.LoadMaps().Count > 0)
                    mapData.index = mapManagementService.LoadMaps()[mapManagementService.LoadMaps().Count - 1].index++;
                else
                    mapData.index = 0;

                MapDataModel.CopyMapPrefabForEditor(sampleMapDatas[mapSelectPopupField.index], mapData.id, true);

                // 名前設定
                mapData.name += " " + EditorLocalize.LocalizeText(COPY_NAME);
                mapData.name = CreateDuplicateName(mapManagementService.LoadMaps().Select(m => m.name).ToList(), mapData.name);
                
                string CreateDuplicateName(List<string> names, string output) {
                    string createDuplicateName = output;
                    while (names.Contains(createDuplicateName))
                    {
                        createDuplicateName += " " + EditorLocalize.LocalizeText(COPY_NAME);
                    }
                    return createDuplicateName;
                }

                // データ更新
                // マップ新規作成時は、強制的にPrefabを保存する
                mapManagementService.SaveMap(mapData, MapRepository.SaveType.SAVE_PREFAB_FORCE);
                _ = Hierarchy.Hierarchy.Refresh(Region.Map, AbstractHierarchyView.RefreshTypeMapDuplicate + "," + mapData.id);
                MapEditor.MapEditor.LaunchMapEditMode(mapData);
                Hierarchy.Hierarchy.MapLastSelect();
                Close();
            };

            //CANCELボタン
            _CANCEL_button = labelFromUxml.Query<Button>("CANCEL_button");
            _CANCEL_button.clicked += () => { Close(); };
        }
    }
}