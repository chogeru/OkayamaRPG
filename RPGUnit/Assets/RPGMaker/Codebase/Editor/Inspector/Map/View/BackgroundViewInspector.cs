using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.MapEditor.Component.Canvas;
using RPGMaker.Codebase.Editor.MapEditor.Window.MapEdit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.Map.View
{
    /// <summary>
    /// [マップ設定]-[マップリスト]-[各マップ]-[マップ編集]-[背景] Inspector
    /// </summary>
    public class BackgroundViewInspector : AbstractInspectorElement
    {
        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/Map/Asset/background_view.uxml"; } }

        private readonly MapDataModel         _mapEntity;
        private readonly Action<MapDataModel> _onChange;

        //背景の画像リストの保持
        private List<string> _backgroundPicture;

        //画像設定のプルダウン
        private PopupFieldBase<string> _backgroundViewDropdown;

        private Dictionary<string, string> _backgroundViewImageDictionary;

        // "エディター表示"トグル。
        private Toggle _displayToggle;

        //更新用
        private MapEditWindow _mapEditWindow =
            WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.MapEditWindow) as MapEditWindow;

        //画像の拡大の部分
        private          RadioButton _zoom0Toggle;
        private          RadioButton _zoom2Toggle;
        private          RadioButton _zoom4Toggle;

        public BackgroundViewInspector(MapDataModel mapEntity, Action<MapDataModel> onChange) {
            _mapEntity = mapEntity;
            _onChange = onChange;

            LoadDictionaries();
            Initialize();
        }

        protected override void RefreshContents() {
            base.RefreshContents();
            LoadDictionaries();
            Initialize();
        }

        private void LoadDictionaries() {
            _backgroundViewImageDictionary = new Dictionary<string, string>();
            _backgroundViewImageDictionary.Add("", EditorLocalize.LocalizeText("WORD_1594"));
            GetBackgroundPicture();
            //背景ファイル内の画像取得
            foreach (var backgroundName in _backgroundPicture)
                _backgroundViewImageDictionary.Add(Guid.NewGuid().ToString(), backgroundName);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();

            _backgroundViewDropdown =
                MakePopupField(_backgroundViewImageDictionary, RootContainer, "background_view_dropdown");

            _displayToggle = RootContainer.Query<Toggle>("display_toggle");

            _zoom0Toggle = RootContainer.Query<RadioButton>("radioButton-mapEdit-display14");
            _zoom2Toggle = RootContainer.Query<RadioButton>("radioButton-mapEdit-display15");
            _zoom4Toggle = RootContainer.Query<RadioButton>("radioButton-mapEdit-display16");

            SetEntityToUI();
        }

        private PopupFieldBase<string> MakePopupField(
            Dictionary<string, string> dictionary,
            VisualElement parentContainer,
            string containerName
        ) {
            var imageFileName = Path.ChangeExtension(_mapEntity.background.imageName, ".png");
            var RootContainer = (VisualElement) parentContainer.Query<VisualElement>(containerName);
            var popupField = new PopupFieldBase<string>(dictionary.Values.ToList(), BackgroundNameToIndex(imageFileName));
            RootContainer.Add(popupField);
            return popupField;
        }

        private void SetEntityToUI() {
            _backgroundViewDropdown.RegisterValueChangedCallback(evt =>
            {
                var imageFileName = _backgroundViewDropdown.value;
                var imageFilePath = PathManager.MAP_BACKGROUND + imageFileName;

                _mapEntity.background.imageName = Path.GetFileNameWithoutExtension(imageFileName);
                _onChange?.Invoke(_mapEntity);

                var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(imageFilePath);
                var spr = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(imageFilePath);

                //Mapのリロード
                _mapEditWindow =
                    WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.MapEditWindow) as
                        MapEditWindow;
                _mapEditWindow.UpdateBackgroundView(_mapEntity, spr, tex);
            });

            // マウス侵入時に選択肢を更新しinspector枠が開かれたのちの画像データの更新に対応する
            _backgroundViewDropdown.RegisterCallback<MouseEnterEvent>(evt =>
            {
                LoadDictionaries();
                _backgroundViewDropdown.RefreshChoices(_backgroundViewImageDictionary.Values.ToList());
            });

            // エディター表示トグル。
            var backgroundTransform = _mapEntity.GetLayerTransformForEditor(MapDataModel.Layer.LayerType.Background);
            var backgroundSpriteRenderer = backgroundTransform.GetComponent<SpriteRenderer>();
            backgroundSpriteRenderer.enabled = _displayToggle.value = _mapEntity.background.showInEditor;
            _displayToggle.RegisterValueChangedCallback(evt =>
            {
                backgroundSpriteRenderer.enabled =
                _mapEntity.background.showInEditor = evt.newValue;
                _onChange?.Invoke(_mapEntity);
            });

            if (_mapEntity.background.imageZoomIndex == MapDataModel.ImageZoomIndex.Zoom1)
                _zoom0Toggle.value = true;
            else if (_mapEntity.background.imageZoomIndex == MapDataModel.ImageZoomIndex.Zoom2)
                _zoom2Toggle.value = true;
            else if (_mapEntity.background.imageZoomIndex == MapDataModel.ImageZoomIndex.Zoom4)
                _zoom4Toggle.value = true;

            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {_zoom0Toggle, _zoom2Toggle, _zoom4Toggle},
                (int) _mapEntity.background.imageZoomIndex, new List<Action>
                {
                    // 等倍表示トグル。
                    () =>
                    {
                        _mapEntity.background.imageZoomIndex = MapDataModel.ImageZoomIndex.Zoom1;
                        MapCanvas.UpdateBackgroundDisplayedAndScaleForEditor(_mapEntity);

                        _onChange?.Invoke(_mapEntity);
                    },
                    // 2倍表示トグル。
                    () =>
                    {
                        _mapEntity.background.imageZoomIndex = MapDataModel.ImageZoomIndex.Zoom2;
                        MapCanvas.UpdateBackgroundDisplayedAndScaleForEditor(_mapEntity);

                        _onChange?.Invoke(_mapEntity);
                    },
                    // 4倍表示トグル。
                    () =>
                    {
                        _mapEntity.background.imageZoomIndex = MapDataModel.ImageZoomIndex.Zoom4;
                        MapCanvas.UpdateBackgroundDisplayedAndScaleForEditor(_mapEntity);

                        _onChange?.Invoke(_mapEntity);
                    }
                });
        }

        //背景の名前をプルダウンのIndexに変更する
        private int BackgroundNameToIndex(string mapEntityName) {
            var index = 0;
            if (mapEntityName == null) return index;

            for (var i = 0; i < _backgroundPicture.Count; i++)
                if (_backgroundPicture[i] == mapEntityName)
                {
                    index = i + 1;
                    break;
                }

            return index;
        }

        //今の背景のフォルダ内の画像名を取得してきてListに込める部分
        private void GetBackgroundPicture() {
            //フォルダを指定してそれ以下のファイルを取得
            var names = Directory.GetFiles(
                PathManager.MAP_BACKGROUND, "*.png", SearchOption.AllDirectories);
            //ファイル名のみを入れる配列変数
            var substitutionName = new string[names.Length];
            for (var i = 0; i < names.Length; i++)
                //フォルダパスが含まれているのでファイル名のみに変換して詰め直す
                substitutionName[i] = names[i].Replace(PathManager.MAP_BACKGROUND, "");

            //背景フォルダ内の画像を保持させる
            _backgroundPicture = substitutionName.ToList();
        }
    }
}