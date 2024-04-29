using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.Window;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow
{
    public class SdSelectModalWindow : BaseModalWindow
    {
        public enum CharacterType
        {
            Map = 0,
            Battle
        }

        private readonly List<string> _assetPath = new List<string>
        {
            PathManager.IMAGE_CHARACTER,
            PathManager.IMAGE_SV_CHARACTER,
            PathManager.IMAGE_OBJECT,
        };

        private VisualElement rightWindow;
        
        private bool isObjectOnly = false;

        public CharacterType CharacterSdType { get; set; } = CharacterType.Map;

        protected override string ModalUxml =>
            "Assets/RPGMaker/Codebase/Editor/DatabaseEditor/ModalWindow/Uxml/select_image_modal_window.uxml";

        protected override string ModalUss => "";

        private List<AssetManageDataModel> _assetManageData;
        private ListView _typeView;
        private List<string> _fileIds;
        private string _currentSelect;

        public SdSelectModalWindow(bool isObjectOnly = false)
        {
            this.isObjectOnly = isObjectOnly;
        }

        public void ShowWindow(string modalTitle, CallBackWidow callBack, string currentSelect) {
            ShowWindow(modalTitle, callBack);
            _currentSelect = currentSelect;
            //描画を待ってから選択状態を更新する
            SelectImage();
        }

        private async void SelectImage() {
            await Task.Delay(1);
            if (!string.IsNullOrEmpty(_currentSelect))
            {
                int index = _fileIds.FindIndex(item =>
                {
                    return item == _currentSelect;
                });

                if (index < 0) index = 0;

                _typeView.SetSelection(index);
                _typeView.ScrollToId(index);

                string imgName = "";
                int type = 0;
                foreach (var asset in _assetManageData)
                {
                    if (asset == null) continue;
                    if (asset.id == _fileIds[_typeView.selectedIndex])
                    {
                        imgName = asset.imageSettings[0].path;
                        type = asset.assetTypeId;
                        break;
                    }
                }

                _UpdateImage(imgName, type);
            }
        }

        public override void ShowWindow(string modalTitle, CallBackWidow callBack) {
            var wnd = GetWindow<SdSelectModalWindow>();

            if (callBack != null)
            {
                _callBackWindow = callBack;
            }

            wnd.titleContent = new GUIContent(EditorLocalize.LocalizeText("WORD_1611"));
            wnd.Init();
        }

        public override void Init() {
            var _orderData = AssetManageRepository.OrderManager.Load();
            _assetManageData = new List<AssetManageDataModel>();
            var _databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;
            var manageData = _databaseManagementService.LoadAssetManage();
            List<AssetCategoryEnum> categorys =
                isObjectOnly ?
                    new() { AssetCategoryEnum.OBJECT } :
                CharacterSdType == CharacterType.Map ?
                    new() { AssetCategoryEnum.MOVE_CHARACTER, AssetCategoryEnum.OBJECT } :
                    new() { AssetCategoryEnum.SV_BATTLE_CHARACTER };
            for (var i = 0; i < _orderData.orderDataList.Length; i++)
            {
                if (_orderData.orderDataList[i].idList == null)
                    continue;
                if (categorys.Contains((AssetCategoryEnum)_orderData.orderDataList[i].assetTypeId))
                {
                    for (var i2 = 0; i2 < _orderData.orderDataList[i].idList.Count; i2++)
                    {
                        AssetManageDataModel data = null;
                        for (int i3 = 0; i3 < manageData.Count; i3++)
                            if (manageData[i3].id == _orderData.orderDataList[i].idList[i2])
                            {
                                data = manageData[i3];
                                break;
                            }
                        var count = 0;
                        if (data == null)
                            count++;
                        _assetManageData.Add(data);
                    }
                }
            }

            var root = rootVisualElement;
            root.style.flexGrow = 1;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ModalUxml);
            VisualElement labelFromUxml = visualTree.CloneTree();
            labelFromUxml.style.flexGrow = 1;
            EditorLocalize.LocalizeElements(labelFromUxml);
            root.Add(labelFromUxml);
            var leftWindow = labelFromUxml.Query<VisualElement>("system_window_leftwindow").AtIndex(0);
            rightWindow = labelFromUxml.Query<VisualElement>("system_window_rightwindow").AtIndex(0);
            rightWindow.style.flexGrow = 1;

            var fileNames = new List<string>();
            _fileIds = new List<string>();

            // 先頭に「なし」の選択肢を追加
            fileNames.Add(EditorLocalize.LocalizeText("WORD_0113"));
            _fileIds.Add("");
            // ファイルの情報をアセットデータから格納
            foreach (var asset in _assetManageData)
            {
                if (asset == null) continue;
                fileNames.Add(asset.name);
                _fileIds.Add(asset.id);
            }

            Action<VisualElement, int> bindType = (e, i) =>
            {
                e.Clear();
                var l = new Label(fileNames[i]);
                e.Add(l);
            };
            Func<VisualElement> makeType = () => new Label();
            _typeView = new ListView(new string[fileNames.Count], 16, makeType, bindType);
            _typeView.name = "list";
            _typeView.selectionType = SelectionType.Multiple;
            _typeView.style.flexGrow = 1.0f;
            leftWindow.Add(_typeView);

            var path = _assetPath[(int) CharacterSdType];
            var imgName = "";
            var type = 0;
            var indexFile = 0;
            _typeView.selectedIndex = indexFile;
            _currentSelect = _fileIds[_typeView.selectedIndex];
            foreach (var asset in _assetManageData)
            {
                if (asset == null) continue;
                if (asset.id == _fileIds[_typeView.selectedIndex])
                {
                    imgName = asset.imageSettings[0].path;
                    type = asset.assetTypeId;
                }
            }

            _typeView.onSelectionChange += objects =>
            {
                imgName = "";
                
                foreach (var asset in _assetManageData)
                {
                    if (asset == null) continue;
                    if (asset.id == _fileIds[_typeView.selectedIndex])
                    {
                        imgName = asset.imageSettings[0].path;
                        type = asset.assetTypeId;
                        break;
                    }
                }

                _UpdateImage(imgName, type);
                if (_typeView.selectedIndex < 0)
                    _typeView.selectedIndex = 0;
                if (_fileIds.Count > _typeView.selectedIndex)
                    _currentSelect = _fileIds[_typeView.selectedIndex];
            };

            _UpdateImage(imgName, type);

            var buttonOk = labelFromUxml.Query<Button>("Common_Button_Ok").AtIndex(0);
            var buttonCancel = labelFromUxml.Query<Button>("Common_Button_Cancel").AtIndex(0);

            buttonOk.clicked += () =>
            {
                _callBackWindow(_currentSelect);
                Close();
            };

            buttonCancel.clicked += () => { Close(); };

            //ListViewにフォーカスをあてる
            _typeView.Focus();
        }


        private void _UpdateImage(string imageName, int type) {
            //画像の読み込み
            var path = type == (int) AssetCategoryEnum.MOVE_CHARACTER ?
                _assetPath[0] :
                type == (int) AssetCategoryEnum.OBJECT ?
                _assetPath[2] : 
                _assetPath[1];

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path + imageName);

            VisualElement image = rightWindow.Query<VisualElement>("image");

            // テクスチャが見つからなかった場合はプレビュー表示を空にする
            if (tex == null)
            {
                image.style.backgroundImage = null;
                return;
            }

            image.style.backgroundImage = tex;
            if (tex != null)
            {
                image.style.width = tex.width;
                image.style.height = tex.height;
            }
        }
    }
}