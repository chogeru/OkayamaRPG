using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Common.Window;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.Canvas.QuickEvent.ModalWindow
{
    public class QuickeventCannibalChestModalwindow : BaseModalWindow
    {
        private Button _CANCEL_button;

        //データ系
        private DatabaseManagementService _databaseManagementService;
        private EventEditCanvas           _eventEditCanvas;
        private VisualElement             _image;
        private Button                    _imageButton;

        //returnする値
        private string _imageName;
        private int    _loseToggle; //敗走可能か

        //表示要素
        private Button        _OK_button;
        private int           _runAwayToggle; //逃亡可能か
        private string        _selectId; //直接指定、変数指定の時のID
        private int           _toggleNum; //選ばれたエンカウント種類の番号
        private Toggle        defeat_toggle;
        private VisualElement direct;
        private RadioButton        direct_toggle;
        private Toggle        escape_toggle;
        private RadioButton        random_toggle;
        private VisualElement variable;
        private RadioButton        variable_toggle;

        protected override string ModalUxml =>
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Component/Canvas/QuickEvent/ModalWindow/Uxml/quickevent_cannibal_chest_modalwindow.uxml";

        public void ShowWindow(
            EventEditCanvas eventEditCanvas,
            List<EventMapDataModel> eventList,
            MapDataModel mapDataModel,
            string modalTitle,
            CallBackWidow callBack
        ) {
            var wnd = GetWindow<QuickeventCannibalChestModalwindow>();

            _eventEditCanvas = eventEditCanvas;

            if (callBack != null) _callBackWindow = callBack;

            wnd.titleContent = new GUIContent(modalTitle);
            wnd.Init();
            //サイズ固定用
            var size = new Vector2(400, 180);
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

            // 基本データを取得する
            _databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;

            // 要素取得
            direct_toggle = labelFromUxml.Query<RadioButton>("radioButton-other-display5");
            variable_toggle = labelFromUxml.Query<RadioButton>("radioButton-other-display6");
            random_toggle = labelFromUxml.Query<RadioButton>("radioButton-other-display7");
            direct = labelFromUxml.Query<VisualElement>("direct");
            variable = labelFromUxml.Query<VisualElement>("variable");
            escape_toggle = labelFromUxml.Query<Toggle>("escape_toggle");
            defeat_toggle = labelFromUxml.Query<Toggle>("defeat_toggle");
            // 敵グループ設定の部分
            SetEnemy();


            // 画像選択
            _imageButton = labelFromUxml.Query<Button>("button");
            _image = labelFromUxml.Query<VisualElement>("image");

            var manageData = _databaseManagementService.LoadAssetManage();
            AssetManageDataModel defaultData = null;
            for (int i = 0; i < manageData.Count; i++)
                if (manageData[i].id == "1dd21a8b-d674-4788-a9ff-a83f252c8bb2")
                {
                    defaultData = manageData[i];
                    break;
                }
            if (defaultData != null)
            {
                _imageName = "1dd21a8b-d674-4788-a9ff-a83f252c8bb2";
                ImageSetting(ImageManager.LoadSvCharacter(_imageName));
            }

            _imageButton.clickable.clicked += () =>
            {
                var sdSelectModalWindow = new SdSelectModalWindow();
                sdSelectModalWindow.CharacterSdType = SdSelectModalWindow.CharacterType.Map;
                sdSelectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select IconImage"), data =>
                {
                    var imageName = (string) data;
                    var imageCharacter = ImageManager.LoadSvCharacter(imageName);
                    ImageSetting(imageCharacter);
                    _imageName = imageName;
                }, _imageName);
            };

            //OKボタン
            _OK_button = labelFromUxml.Query<Button>("OK_button");
            _OK_button.clicked += () =>
            {
                //決定されたのでポップアップのデータを渡す
                var returnValue = new List<object>
                {
                    _imageName,
                    _toggleNum,
                    _selectId,
                    _loseToggle,
                    _runAwayToggle
                };
                _callBackWindow(returnValue);
                _callBackWindow = null;
                Close();
            };

            //CANCELボタン
            _CANCEL_button = labelFromUxml.Query<Button>("CANCEL_button");
            _CANCEL_button.clicked += () =>
            {
                //入っている初期値で決定にするためこのままこのウィンドウを閉じる
                _callBackWindow(null);
                _callBackWindow = null;
                Close();
            };
        }

        //各項目の設定の実施
        private void SetEnemy() {
            //各リストデータの取得
            var troopDataModels = _databaseManagementService.LoadTroop();
            var flagDataModel = _databaseManagementService.LoadFlags();

            //直接指定の敵グループプルダウン
            var troopNameList = new List<string>();
            var troopIdList = new List<string>();
            var selectID = 0;
            for (var i = 0; i < troopDataModels.Count; i++)
            {
                troopNameList.Add(troopDataModels[i].name);
                troopIdList.Add(troopDataModels[i].id);
            }

            _selectId = troopIdList[0];
            var troopPopupField = new PopupFieldBase<string>(troopNameList, selectID);
            direct.Clear();
            direct.Add(troopPopupField);
            troopPopupField.RegisterValueChangedCallback(evt => { _selectId = troopIdList[troopPopupField.index]; });

            //変数指定の変数グループプルダウン
            var variableNameList = new List<string>();
            var variableIdNameList = new List<string>();
            selectID = 0;
            for (var i = 0; i < flagDataModel.variables.Count; i++)
            {
                if (flagDataModel.variables[i].name == "")
                    variableNameList.Add(EditorLocalize.LocalizeText("WORD_1518"));
                else
                    variableNameList.Add(flagDataModel.variables[i].name);

                variableIdNameList.Add(flagDataModel.variables[i].id);
            }

            var variablePopupField = new PopupFieldBase<string>(variableNameList, selectID);
            variable.Clear();
            variable.Add(variablePopupField);
            variablePopupField.RegisterValueChangedCallback(evt =>
            {
                _selectId = variableIdNameList[variablePopupField.index];
            });

            //初期値
            direct_toggle.value = true;
            _toggleNum = 0;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {direct_toggle, variable_toggle, random_toggle},
                0, new List<Action>
                {
                    //直性指定
                    () =>
                    {
                        toggleControl(0);
                    },
                    //変数指定
                    () =>
                    {
                        toggleControl(1);
                    },
                    //ランダムエンカウントと同じ
                    () =>
                    {
                        toggleControl(3);
                    }
                    
                });

            //押された時の処理まとめ
            void toggleControl(int num) {
                switch (num)
                {
                    case 0:
                        direct.SetEnabled(true);
                        variable.SetEnabled(false);
                        break;
                    case 1:
                        direct.SetEnabled(false);
                        variable.SetEnabled(true);
                        break;
                    case 2:
                        direct.SetEnabled(false);
                        variable.SetEnabled(false);
                        break;
                }

                //返り値ようにセット
                _toggleNum = num;
            }

            //敗北、逃走のトグル部分
            _loseToggle = 0;
            defeat_toggle.RegisterValueChangedCallback(o =>
            {
                if (defeat_toggle.value)
                    _loseToggle = 1;
                if (!defeat_toggle.value)
                    _loseToggle = 0;
            });
            _runAwayToggle = 0;
            escape_toggle.RegisterValueChangedCallback(o =>
            {
                if (escape_toggle.value)
                    _runAwayToggle = 1;
                if (!escape_toggle.value)
                    _runAwayToggle = 0;
            });
        }

        // 画像設定
        private void ImageSetting(Texture2D tex) {
            // ウィンドウサイズ取得
            var width = _imageButton.layout.width;
            var height = _imageButton.layout.height;
            if (float.IsNaN(width)) width = 65;
            if (float.IsNaN(height)) height = 80;

            BackgroundImageHelper.SetBackground(_image, new Vector2(width, height), tex);
        }

        private void OnDestroy() {
            // ウィンドウが閉じられた際にcallbackが呼ばれていなければ呼ぶ
            if (_callBackWindow != null) _callBackWindow(null);
        }
    }
}