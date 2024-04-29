using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Animation;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using RPGMaker.Codebase.Editor.Hierarchy.Enum;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.AssetManage.View
{
    /// <summary>
    /// [素材管理] Inspector
    /// </summary>
    public class AssetManageInspectorElement : AbstractInspectorElement
    {
        private readonly int MIN_FRAME_SPEED = 1;
        private readonly int MAX_FRAME       = 1024;
        private readonly int MAX_SPEED       = 8192;

        // 各素材毎の参照先
        private readonly string[] assetPath =
        {
            PathManager.IMAGE_CHARACTER,
            PathManager.IMAGE_OBJECT,
            PathManager.IMAGE_BALLOON,
            PathManager.IMAGE_SV_CHARACTER,
            PathManager.IMAGE_WEAPON,
            PathManager.IMAGE_OVERLAP,
            PathManager.IMAGE_ANIMATION,
            PathManager.SOUND_SE,
            PathManager.ANIMATION_PREFAB,
            PathManager.ANIMATION_EFFEKSEER,
        };

        private          AssetManageDataModel                         _assetManageDataModel;
        private          AssetManageRepository.OrderManager.OrderData _orderData;

        private SceneWindow _sceneView;

        private readonly string[] mainUxml =
        {
            "Assets/RPGMaker/Codebase/Editor/Inspector/AssetManage/Asset/inspector_asset_character_move.uxml",
            "Assets/RPGMaker/Codebase/Editor/Inspector/AssetManage/Asset/inspector_asset_object_move.uxml",
            "Assets/RPGMaker/Codebase/Editor/Inspector/AssetManage/Asset/inspector_asset_popup.uxml",
            "Assets/RPGMaker/Codebase/Editor/Inspector/AssetManage/Asset/inspector_asset_sv_battle_character.uxml",
            "Assets/RPGMaker/Codebase/Editor/Inspector/AssetManage/Asset/inspector_asset_sv_weapon.uxml",
            "Assets/RPGMaker/Codebase/Editor/Inspector/AssetManage/Asset/inspector_asset_superposition.uxml",
            "Assets/RPGMaker/Codebase/Editor/Inspector/AssetManage/Asset/inspector_asset_battle_effect.uxml"
        };

        public AssetManageInspectorElement(AssetManageDataModel assetManageDataModel) {
            _assetManageDataModel = assetManageDataModel;
            Refresh();
        }

        override protected void RefreshContents() {
            base.RefreshContents();
            _orderData = AssetManageRepository.OrderManager.Load();
            if (_assetManageDataModel != null)
                _assetManageDataModel = databaseManagementService.LoadAssetManage()
                    .Find(item => item.id == _assetManageDataModel.id);

            if (_assetManageDataModel == null)
            {
                _sceneView =
                    WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.DatabaseSceneWindow) as
                        SceneWindow;
                _sceneView?.Clear();
                Clear();
                return;
            }

            Initialize();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            MainUxml = mainUxml[_assetManageDataModel.assetTypeId];
            base.InitializeContents();

            // シーン設定
            _sceneView =
                WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.DatabaseSceneWindow) as
                    SceneWindow;
            _sceneView.Create(SceneWindow.PreviewId.AssetManage);
            if (_assetManageDataModel.assetTypeId == (int) AssetCategoryEnum.BATTLE_EFFECT)
            {
                var path = "";
                var effekseer = false;
                if (_assetManageDataModel.imageSettings[0].path.Contains(".prefab"))
                {
                    path = assetPath[_assetManageDataModel.assetTypeId + 2];
                }
                else
                {
                    path = assetPath[_assetManageDataModel.assetTypeId + 3];
                    effekseer = true;
                }

                _sceneView.GetManagePreview()
                    .SetEffectPath(path + _assetManageDataModel.imageSettings[0].path, effekseer);
                _sceneView.Init();
                _sceneView.Render();
            }
            else
            {
                // 画像のパスを取得する
                var pathList = new List<string>();
                for (var i = 0; i < _assetManageDataModel.imageSettings.Count; i++)
                    pathList.Add(assetPath[_assetManageDataModel.assetTypeId] +
                                 _assetManageDataModel.imageSettings[i].path);
                // 表示名を取得する
                var nameList = new List<string>();
                var foldout = RootContainer.Query<Foldout>(className: "top_text").ToList();
                for (var i = 0; i < foldout.Count; i++)
                    nameList.Add(foldout[i].text);
                // コマサイズ、スピードを取得
                var frameList = new List<int>();
                var speedList = new List<int>();
                for (var i = 0; i < _assetManageDataModel.imageSettings.Count; i++)
                {
                    frameList.Add(_assetManageDataModel.imageSettings[i].animationFrame);
                    speedList.Add(_assetManageDataModel.imageSettings[i].animationSpeed);
                }
                
                
                _sceneView.GetManagePreview().SetAssetId(_assetManageDataModel.id, _assetManageDataModel.assetTypeId);

                _sceneView.GetManagePreview().Setup(nameList);
                _sceneView.Init();
                _sceneView.Render();
            }

            // ID、名前設定
            Label asset_character_move_ID = RootContainer.Query<Label>("asset_name");
            asset_character_move_ID.text = _assetManageDataModel.SerialNumberString;

            ImTextField asset_character_move_name = RootContainer.Query<ImTextField>("asset_description");
            asset_character_move_name.value = _assetManageDataModel.name;
            asset_character_move_name.RegisterCallback<FocusOutEvent>(o =>
            {
                _assetManageDataModel.name = asset_character_move_name.value;
                _UpdateSceneView();
                UpdateData();
            });

            // 画像のセレクタ
            var data_path = new List<string>();
            try
            {
                // ディレクトリ内のファイル全取得
                if (_assetManageDataModel.assetTypeId == (int) AssetCategoryEnum.BATTLE_EFFECT)
                {
                    data_path.AddRange(Directory.GetFiles(assetPath[_assetManageDataModel.assetTypeId + 2], "*.prefab",
                        SearchOption.TopDirectoryOnly));
                    data_path.AddRange(Directory.GetFiles(assetPath[_assetManageDataModel.assetTypeId + 3], "*.asset",
                        SearchOption.TopDirectoryOnly));

                    for (var i = 0; i < data_path.Count; i++)
                    {
                        data_path[i] = data_path[i].Replace("\\", "/");
                        data_path[i] = data_path[i].Replace(assetPath[_assetManageDataModel.assetTypeId + 2], "");
                        data_path[i] = data_path[i].Replace(assetPath[_assetManageDataModel.assetTypeId + 3], "");
                    }
                }
            }
            catch (IOException)
            {
            }

            // 画像用
            for (var i = 0; i < RootContainer.Query<Button>("image_button").ToList().Count; i++)
            {
                var num = i;
                var asset_image = RootContainer.Query<Button>("image_button").AtIndex(num);
                asset_image.text = _assetManageDataModel.imageSettings[num].path;

                asset_image.clicked += () =>
                {
                    new ImageSelectModalWindow(assetPath[_assetManageDataModel.assetTypeId]).ShowWindow("",
                    data => 
                    {
                        var imageName = (string) data + ".png";
                        _assetManageDataModel.imageSettings[num].path = imageName;
                        _assetManageDataModel.imageSettings[num].animationFrame = MIN_FRAME_SPEED;
                        _assetManageDataModel.imageSettings[num].animationSpeed = MIN_FRAME_SPEED;
                        _UpdateSceneView();
                        Refresh();
                    }, _assetManageDataModel.imageSettings[num].path);
                };
            }

            // エフェクト用
            for (var i = 0; i < RootContainer.Query<VisualElement>("asset_select").ToList().Count; i++)
            {
                var num = i;
                var asset_character_move_image = RootContainer.Query<VisualElement>("asset_select").AtIndex(num);
                var imageTextDropdownChoices = data_path;
                imageTextDropdownChoices.Add("");

                var image_num = -1;
                for (var i2 = 0; i2 < data_path.Count; i2++)
                    if (data_path[i2] == _assetManageDataModel.imageSettings[num].path)
                        image_num = i2;

                PopupFieldBase<string> asset_imagePopupField;
                if (image_num == -1)
                    asset_imagePopupField =
                        new PopupFieldBase<string>(imageTextDropdownChoices, imageTextDropdownChoices.Count - 1);
                else
                    asset_imagePopupField = new PopupFieldBase<string>(imageTextDropdownChoices, image_num);

                asset_character_move_image.Add(asset_imagePopupField);
                asset_imagePopupField.RegisterValueChangedCallback(evt =>
                {
                    _assetManageDataModel.imageSettings[num].path = asset_imagePopupField.value;
                    _UpdateSceneView();
                    Refresh();
                });
            }

            // インポートボタン
            var buttonImport = RootContainer.Query<Button>("asset_import").ToList();
            for (var i = 0; i < buttonImport.Count; i++)
            {
                var num = i;
                buttonImport[i].clicked += () =>
                {
                    // エフェクト以外
                    if ((int) AssetCategoryEnum.BATTLE_EFFECT != _assetManageDataModel.assetTypeId)
                    {
                        var path = "";
                        if ((int) AssetCategoryEnum.MOVE_CHARACTER == _assetManageDataModel.assetTypeId)
                        {
                            path = AssetManageImporter.StartToFile("png", assetPath[_assetManageDataModel.assetTypeId],
                                null, true, true);
                        }
                        else
                        {
                            path = AssetManageImporter.StartToFile("png", assetPath[_assetManageDataModel.assetTypeId]);
                        }

                        if (!string.IsNullOrEmpty(path))
                        {
                            path = Path.GetFileName(path);
                            _assetManageDataModel.imageSettings[num].path = path;
                            _assetManageDataModel.imageSettings[num].animationFrame = MIN_FRAME_SPEED;
                            _assetManageDataModel.imageSettings[num].animationSpeed = MIN_FRAME_SPEED;
                            _UpdateSceneView();
                            Refresh();
                        }
                    }
                    // エフェクト
                    else
                    {
                        var animationList =
                            AssetManageImporter.StartToZip_Effect<AnimationDataModel>(
                                new List<string> { "png", "ogg", "wav","png.meta", "ogg.meta", "wav.meta" },
                                new List<string>
                                {
                                    assetPath[_assetManageDataModel.assetTypeId],
                                    assetPath[_assetManageDataModel.assetTypeId + 1],
                                    assetPath[_assetManageDataModel.assetTypeId + 1],
                                    assetPath[_assetManageDataModel.assetTypeId],
                                    assetPath[_assetManageDataModel.assetTypeId + 1],
                                    assetPath[_assetManageDataModel.assetTypeId + 1]
                                },
                                new List<string>
                                {
                                    assetPath[_assetManageDataModel.assetTypeId + 2],
                                    assetPath[_assetManageDataModel.assetTypeId + 3]
                                });

                        // データがある
                        if (animationList != null && animationList.Count > 0)
                        {
                            // 読み込んだJSONデータを適用する
                            var animationDataModels = databaseManagementService.LoadAnimation();

                            for (var i2 = 0; i2 < animationList.Count; i2++)
                            {
                                // IDのみ新規設定
                                animationList[i2].id = Guid.NewGuid().ToString();
                                animationDataModels.Add(animationList[i2]);
                                databaseManagementService.SaveAnimation(animationDataModels);
                            }
                        }
                        //インスペクター側の更新
                        Refresh();
                        RpgMakerEditor.IsImportEffekseer = true;
                    }
                };
            }

            //複数存在することもあるので検索を回す
            for (var i = 0; i < RootContainer.Query<VisualElement>("asset_image_select").ToList().Count; i++)
            {
                // 画像表示
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(
                    assetPath[_assetManageDataModel.assetTypeId] + _assetManageDataModel.imageSettings[i].path);
                //画像の部分
                var asset_image_select_area = RootContainer.Query<VisualElement>("asset_image_select_area").AtIndex(i);
                var asset_image_select = RootContainer.Query<VisualElement>("asset_image_select").AtIndex(i);

                var width = asset_image_select_area.layout.width;
                var height = asset_image_select_area.layout.height;
                if (float.IsNaN(width)) width = 256;
                if (float.IsNaN(height)) height = 72;

                if (tex != null)
                    BackgroundImageHelper.SetBackground(asset_image_select, new Vector2(width, height), tex);
            }

            // コマサイズ
            for (var i = 0; i < RootContainer.Query<Vector2IntField>("asset_image_size").ToList().Count; i++)
            {
                var num = i;
                var asset_character_move_size =
                    RootContainer.Query<Vector2IntField>("asset_image_size").AtIndex(num);
                asset_character_move_size.SetEnabled(false);

                // 画像サイズ取得
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(
                    assetPath[_assetManageDataModel.assetTypeId] + _assetManageDataModel.imageSettings[i].path);
                var size = new Vector2Int(0, 0);
                if (tex != null)　size = new Vector2Int(tex.width, tex.height);

                if (_assetManageDataModel.imageSettings[num].animationFrame == 0)
                    _assetManageDataModel.imageSettings[num].animationFrame = 1;
                _assetManageDataModel.imageSettings[num].sizeX =
                    size.x / _assetManageDataModel.imageSettings[num].animationFrame;
                _assetManageDataModel.imageSettings[num].sizeY = size.y;
                var offset = new Vector2Int(_assetManageDataModel.imageSettings[num].sizeX,
                    _assetManageDataModel.imageSettings[num].sizeY);
                asset_character_move_size.value = offset;
                asset_character_move_size.RegisterCallback<FocusOutEvent>(o =>
                {
                    _assetManageDataModel.imageSettings[num].sizeX = asset_character_move_size.value.x;
                    _assetManageDataModel.imageSettings[num].sizeY = asset_character_move_size.value.y;
                    _UpdateSceneView();
                });
            }

            // コマ数
            for (var i = 0; i < RootContainer.Query<IntegerField>("asset_animation_frame").ToList().Count; i++)
            {
                var num = i;
                var asset_animation_frame = RootContainer.Query<IntegerField>("asset_animation_frame").AtIndex(num);
                asset_animation_frame.value = _assetManageDataModel.imageSettings[num].animationFrame;
                asset_animation_frame.RegisterCallback<FocusOutEvent>(o =>
                {
                    if (MIN_FRAME_SPEED > asset_animation_frame.value)
                        asset_animation_frame.value = MIN_FRAME_SPEED;
                    else if (MAX_FRAME < asset_animation_frame.value)
                        asset_animation_frame.value = MAX_FRAME;

                    _assetManageDataModel.imageSettings[num].animationFrame = asset_animation_frame.value;
                    _sceneView.GetManagePreview().UpdateAssetId(_assetManageDataModel.id);
                    _UpdateSceneView();

                    // 画像サイズ取得
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        assetPath[_assetManageDataModel.assetTypeId] + _assetManageDataModel.imageSettings[num].path);
                    var size = new Vector2Int(0, 0);
                    if (tex != null) size = new Vector2Int(tex.width, tex.height);

                    // 画像サイズ設定
                    _assetManageDataModel.imageSettings[num].sizeX =
                        size.x / _assetManageDataModel.imageSettings[num].animationFrame;
                    _assetManageDataModel.imageSettings[num].sizeY = size.y;
                    var offset = new Vector2Int(_assetManageDataModel.imageSettings[num].sizeX,
                        _assetManageDataModel.imageSettings[num].sizeY);
                    var asset_character_move_size =
                        RootContainer.Query<Vector2IntField>("asset_image_size").AtIndex(num);
                    asset_character_move_size.value = offset;
                });
            }

            // 再生速度
            for (var i = 0; i < RootContainer.Query<IntegerField>("asset_animation_speed").ToList().Count; i++)
            {
                var num = i;
                var asset_animation_speed = RootContainer.Query<IntegerField>("asset_animation_speed").AtIndex(num);
                asset_animation_speed.value = _assetManageDataModel.imageSettings[num].animationSpeed;
                asset_animation_speed.RegisterCallback<FocusOutEvent>(o =>
                {
                    if (MIN_FRAME_SPEED > asset_animation_speed.value)
                        asset_animation_speed.value = MIN_FRAME_SPEED;
                    else if (MAX_SPEED < asset_animation_speed.value)
                        asset_animation_speed.value = MAX_SPEED;

                    _assetManageDataModel.imageSettings[num].animationSpeed = asset_animation_speed.value;
                    _sceneView.GetManagePreview().UpdateAssetId(_assetManageDataModel.id);
                    _UpdateSceneView();
                });
            }
        }

        // データ更新
        private void _UpdateSceneView() {
            AssetManageRepository.OrderManager.Save(_orderData);
            databaseManagementService.SaveAssetManage(_assetManageDataModel);
        }

        //ヒエラルキー側のアップデート
        private void UpdateData() {
            _ = Editor.Hierarchy.Hierarchy.Refresh(Region.AssetManage, _assetManageDataModel.id);
        }

        public void ClearSceneWindow() {
            _sceneView?.Clear();
        }
    }
}