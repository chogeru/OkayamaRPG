using Effekseer;
using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Animation;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.DatabaseEditor.Window;
using RPGMaker.Codebase.Editor.Hierarchy.Enum;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.Animation.View
{
    /// <summary>
    /// [バトルエフェクト] Inspector
    /// </summary>
    public class AnimationInspectorElement : AbstractInspectorElement
    {
        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/Animation/Asset/inspector_animEdit.uxml"; } }

        private const string flashUxml =
            "Assets/RPGMaker/Codebase/Editor/Inspector/Animation/Asset/inspector_animEdit_flash.uxml";

        private const string soundUxml =
            "Assets/RPGMaker/Codebase/Editor/Inspector/Animation/Asset/inspector_animEdit_sound.uxml";

        private const int MAX_VALUE = 1000;
        private const int MIN_VALUE = 10;
        private const int ROTATION_MAX_VALUE = 360;
        private const int ROTATION_MIN_VALUE = 0;
        private const int OFFSET_X_MAX_VALUE = 1920;
        private const int OFFSET_Y_MAX_VALUE = 1080;
        private const int OFFSET_MIN_VALUE = 0;

        private List<AnimationDataModel> _animationDataModels;

        //ヒエラルキーの更新用
        private int _id;
        private readonly SceneWindow _sceneView;

        public AnimationInspectorElement(int id) {
            _id = id;

            _sceneView = WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.DatabaseSceneWindow) as SceneWindow;
            _sceneView.Create(SceneWindow.PreviewId.Animation);
            _sceneView.Init();

            Refresh();
        }

        /// <summary>
        /// データの更新
        /// </summary>
        override protected void RefreshContents() {
            base.RefreshContents();
            _animationDataModels = LoadAnimation();
            if (_animationDataModels.Count == 0)
            {
                _sceneView.Clear();
                Clear();
                return;
            }
            
            if (_animationDataModels.Count <= _id)
                _id = _animationDataModels.Count - 1;
            _sceneView.GetAnimationPreview().SetData(_animationDataModels[_id]);
            Initialize();
        }
        
        private List<AnimationDataModel> LoadAnimation()
        {
            var animationDataModels = databaseManagementService.LoadAnimation();
            //「なし」を抜く
            var animationDataModelsWork = new List<AnimationDataModel>();
            for (int i = 0; i < animationDataModels.Count; i++)
            {
                if (animationDataModels[i].id != "54b168ea-5141-48ed-9e42-4336ac58755c")
                {
                    animationDataModelsWork.Add(animationDataModels[i]);
                }
            }
            animationDataModels = animationDataModelsWork;
            return animationDataModels;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();
            var num = _id;

            _animationDataModels = LoadAnimation();
            if (_animationDataModels.Count - 1 < num)
            {
                Clear();
                return;
            }

            var soundDir = new DirectoryInfo(PathManager.SOUND_SE);
            var soundInfo = soundDir.GetFiles("*.ogg");
            var soundNames = new List<string>();
            foreach (var f in soundInfo)
                soundNames.Add(f.Name.Replace(".ogg", ""));
            soundInfo = soundDir.GetFiles("*.wav");
            foreach (var f in soundInfo)
                soundNames.Add(f.Name.Replace(".wav", ""));

            Label anim_ID = RootContainer.Query<Label>("anim_ID");
            anim_ID.text = _animationDataModels[num].SerialNumberString;

            ImTextField anim_name = RootContainer.Query<ImTextField>("anim_name");
            anim_name.value = _animationDataModels[num].particleName;
            anim_name.RegisterCallback<FocusOutEvent>(o =>
            {
                _animationDataModels[num].particleName = anim_name.value;
                _UpdateSceneView(true);
            });

            VisualElement anim_display_type = RootContainer.Query<VisualElement>("anim_display_type");
            VisualElement anim_display_pos = RootContainer.Query<VisualElement>("anim_display_pos");
            Label anim_display_pos_label = RootContainer.Query<Label>("anim_display_pos_label");

            if (_animationDataModels[num].particleType == 3)
            {
                anim_display_pos.style.display = DisplayStyle.None;
                anim_display_pos_label.style.display = DisplayStyle.None;
            }

            var anim_display_typeTextDropdownChoices =
                EditorLocalize.LocalizeTexts(new List<string> {"WORD_0709", "WORD_0710", "WORD_0711"});
            if (_animationDataModels[num].particleType <= 0)
                _animationDataModels[num].particleType = 1;
            var anim_display_typePopupField =
                new PopupFieldBase<string>(anim_display_typeTextDropdownChoices,
                    _animationDataModels[num].particleType - 1);
            anim_display_type.Add(anim_display_typePopupField);
            anim_display_typePopupField.RegisterValueChangedCallback(evt =>
            {
                _animationDataModels[num].particleType =
                    anim_display_typeTextDropdownChoices.IndexOf(anim_display_typePopupField.value) + 1;
                if (_animationDataModels[num].particleType == 3)
                {
                    anim_display_pos.style.display = DisplayStyle.None;
                    anim_display_pos_label.style.display = DisplayStyle.None;
                }
                else
                {
                    anim_display_pos.style.display = DisplayStyle.Flex;
                    anim_display_pos_label.style.display = DisplayStyle.Flex;
                }
                _sceneView.GetAnimationPreview().ChangePos(
                    new Vector2(int.Parse(_animationDataModels[num].offset.Split(";")[0]),
                    int.Parse(_animationDataModels[num].offset.Split(";")[1])),
                    _animationDataModels[num].particlePos);
                _UpdateSceneView();
            });

            var anim_display_posTextDropdownChoices =
                EditorLocalize.LocalizeTexts(new List<string> {"WORD_0712", "WORD_0592", "WORD_0593"});
            if (_animationDataModels[num].particlePos <= 0)
                _animationDataModels[num].particlePos = 1;
            var anim_display_posPopupField = new PopupFieldBase<string>(anim_display_posTextDropdownChoices,
                _animationDataModels[num].particlePos - 1);
            anim_display_pos.Add(anim_display_posPopupField);
            anim_display_posPopupField.RegisterValueChangedCallback(evt =>
            {
                _animationDataModels[num].particlePos =
                    anim_display_posTextDropdownChoices.IndexOf(anim_display_posPopupField.value) + 1;
                _sceneView.GetAnimationPreview().ChangePos(
                    new Vector2(int.Parse(_animationDataModels[num].offset.Split(";")[0]),
                    int.Parse(_animationDataModels[num].offset.Split(";")[1])), 
                    _animationDataModels[num].particlePos);
                _UpdateSceneView();
            });

            // 素材管理からデータ取得
            AssetManageRepository.OrderManager.OrderData orderData;
            var assetManageData = new List<AssetManageDataModel>();
            orderData = AssetManageRepository.OrderManager.Load();

            var manageDatas = databaseManagementService.LoadAssetManage();

            if (orderData.orderDataList[(int) AssetCategoryEnum.BATTLE_EFFECT].idList != null)
                for (var i = 0; i < orderData.orderDataList[(int) AssetCategoryEnum.BATTLE_EFFECT].idList.Count; i++)
                {
                    AssetManageDataModel data = null;
                    for (int i2 = 0; i2 < manageDatas.Count; i2++)
                        if (manageDatas[i2].id == orderData.orderDataList[(int) AssetCategoryEnum.BATTLE_EFFECT].idList[i])
                        {
                            data = manageDatas[i2];
                            break;
                        }
                    assetManageData.Add(data);
                }

            var particleNames = new List<string>();
            var index = 0;

            // 初期値を入れる
            if (_animationDataModels[num].particleId == "" &&
                assetManageData.Count > 0)
            {
                _animationDataModels[num].particleId = assetManageData[0].id;
                databaseManagementService.SaveAnimation(_animationDataModels);
            }

            for (var i = 0; i < assetManageData.Count; i++)
            {
                particleNames.Add(assetManageData[i].name);
                if (assetManageData[i].id == _animationDataModels[num].particleId) index = i;
            }

            //アニメーションの選択部分
            VisualElement anim_particle_select = RootContainer.Query<VisualElement>("anim_particle_select");
            var anim_particle_selectPopupField = new PopupFieldBase<string>(particleNames, index);
            if(assetManageData.Count > 0 && _animationDataModels.Count > 0) 
                EffectPreview(_sceneView.GetAnimationPreview(), manageDatas, assetManageData[anim_particle_selectPopupField.index]
                ,_animationDataModels[num]);
            anim_particle_select.Add(anim_particle_selectPopupField);
            anim_particle_selectPopupField.RegisterValueChangedCallback(evt =>
            {
                _animationDataModels[num].particleId =
                    assetManageData[anim_particle_selectPopupField.index].id;

                EffectPreview(_sceneView.GetAnimationPreview(), manageDatas,
                    assetManageData[anim_particle_selectPopupField.index],
                    _animationDataModels[num]);
                _UpdateSceneView();
            });

            int rateValue;
            var animExpansionRateSliderArea = RootContainer.Query<VisualElement>("anim_expansion_rate_sliderArea");
            _sceneView.GetAnimationPreview().ChangeExpansionRate(_animationDataModels[num].expansion);
            SliderAndFiledBase.IntegerSliderCallBack(animExpansionRateSliderArea, MIN_VALUE, MAX_VALUE, "%",
                _animationDataModels[num].expansion, evt =>
                {
                    rateValue = (int) evt;
                    _animationDataModels[num].expansion = rateValue;
                    _sceneView.GetAnimationPreview().ChangeExpansionRate(rateValue);
                    _UpdateSceneView();
                });

            int speedValue;
            var animPlaybackSpeedSliderArea = RootContainer.Query<VisualElement>("anim_playback_speed_sliderArea");
            _sceneView.GetAnimationPreview().ChangeSpeed(_animationDataModels[num].playSpeed);
            SliderAndFiledBase.IntegerSliderCallBack(animPlaybackSpeedSliderArea, MIN_VALUE, MAX_VALUE, "%",
                _animationDataModels[num].playSpeed, evt =>
                {
                    speedValue = (int) evt;
                    _animationDataModels[num].playSpeed = speedValue;
                    _sceneView.GetAnimationPreview().ChangeSpeed(speedValue);
                    _UpdateSceneView();
                });
            
            Vector3IntField animRotationSetting = RootContainer.Query<Vector3IntField>("anim_rotation_setting");
            animRotationSetting.style.flexGrow = 1;
            var rotationArr = _animationDataModels[num].rotation.Split(';');
            var rotation = new Vector3Int(int.Parse(rotationArr[0]), int.Parse(rotationArr[1]),
                int.Parse(rotationArr[2]));
            animRotationSetting.value = rotation;
            _sceneView.GetAnimationPreview().ChangeRotation(Quaternion.Euler(rotation));

            animRotationSetting.RegisterCallback<FocusOutEvent>(o =>
            {
                var x = animRotationSetting.value.x;
                var y = animRotationSetting.value.y;
                var z = animRotationSetting.value.z;
                
                if (x > ROTATION_MAX_VALUE) x = ROTATION_MAX_VALUE;
                if (x < ROTATION_MIN_VALUE) x = ROTATION_MIN_VALUE;
                if (y > ROTATION_MAX_VALUE) y = ROTATION_MAX_VALUE;
                if (y < ROTATION_MIN_VALUE) y = ROTATION_MIN_VALUE;
                if (z > ROTATION_MAX_VALUE) z = ROTATION_MAX_VALUE;
                if (z < ROTATION_MIN_VALUE) z = ROTATION_MIN_VALUE;

                animRotationSetting.value = new Vector3Int(x, y, z);
                
                _animationDataModels[num].rotation = x.ToString();
                _animationDataModels[num].rotation += ";" + y;
                _animationDataModels[num].rotation += ";" + z;
                _sceneView.GetAnimationPreview().ChangeRotation(Quaternion.Euler(animRotationSetting.value));
                _UpdateSceneView();
            });

            Vector2IntField anim_offset_setting = RootContainer.Query<Vector2IntField>("anim_offset_setting");
            anim_offset_setting.style.minWidth = 200;
            var offsetArr = _animationDataModels[num].offset.Split(';');
            var offset = new Vector2Int(int.Parse(offsetArr[0]), int.Parse(offsetArr[1]));
            anim_offset_setting.value = offset;
            _sceneView.GetAnimationPreview().ChangePos(offset, _animationDataModels[num].particlePos);
            anim_offset_setting.RegisterCallback<FocusOutEvent>(o =>
            {
                var x = anim_offset_setting.value.x;
                var y = anim_offset_setting.value.y;
                
                if (x > OFFSET_X_MAX_VALUE) x = OFFSET_X_MAX_VALUE;
                if (x < OFFSET_MIN_VALUE) x = OFFSET_MIN_VALUE;
                if (y > OFFSET_Y_MAX_VALUE) y = OFFSET_Y_MAX_VALUE;
                if (y < OFFSET_MIN_VALUE) y = OFFSET_MIN_VALUE;

                anim_offset_setting.value = new Vector2Int(x, y);
                
                _animationDataModels[num].offset = x.ToString();
                _animationDataModels[num].offset += ";" + y;
                _sceneView.GetAnimationPreview().ChangePos(
                    new Vector2Int(x, y),
                    _animationDataModels[num].particlePos);
                _UpdateSceneView();
            });

            for (var i = 0; i < _animationDataModels[num].seList.Count; i++)
            {
                SoundInspectorCreate(i);
                SoundInspectorSetting(_id, i, soundNames);
            }

            for (var i = 0; i < _animationDataModels[num].flashList.Count; i++)
            {
                FlashInspectorCreate(i);
                FlashInspectorSetting(_id, i);
            }

            Button anim_sound_add = RootContainer.Query<Button>("anim_sound_add");
            anim_sound_add.clickable.clicked += () =>
            {
                _animationDataModels = LoadAnimation();
                var max = _animationDataModels[num].seList.Count;
                var data = new AnimationDataModel.Se(0, "", 1);

                _animationDataModels[num].seList.Add(data);
                SoundInspectorCreate(max);
                SoundInspectorSetting(_id, max, soundNames);

                _UpdateSceneView();
            };


            Button anim_flash_add = RootContainer.Query<Button>("anim_flash_add");
            anim_flash_add.clickable.clicked += () =>
            {
                var max = _animationDataModels[num].flashList.Count;
                var data = new AnimationDataModel.Flash(0, 1, 1, "0,0,0", 0);
                _animationDataModels[num].flashList.Add(data);
                FlashInspectorCreate(max);
                FlashInspectorSetting(_id, max);
                _UpdateSceneView();
            };
        }

        private void SoundInspectorSetting(int id, int i, List<string> soundNames) {
            VisualElement anim_sound_foldout = RootContainer.Query<Foldout>("anim_sound_foldout" + (i + 1) + "_" + id);

            VisualElement anim_sound_type = anim_sound_foldout.Query<VisualElement>("anim_sound_type");
            var soundListNum =
                soundNames.IndexOf(_animationDataModels[id].seList[i].seName.Replace(".ogg", ""));
            if (soundListNum == -1)
                soundListNum = 0;
            var anim_sound_typePopupField = new PopupFieldBase<string>(soundNames, soundListNum);
            anim_sound_type.Add(anim_sound_typePopupField);
            anim_sound_typePopupField.RegisterValueChangedCallback(evt =>
            {
                _animationDataModels = LoadAnimation();
                if (File.Exists(PathManager.SOUND_SE + anim_sound_typePopupField.value + ".ogg"))
                    _animationDataModels[id].seList[i].seName = anim_sound_typePopupField.value + ".ogg";

                if (File.Exists(PathManager.SOUND_SE + anim_sound_typePopupField.value + ".wav"))
                    _animationDataModels[id].seList[i].seName = anim_sound_typePopupField.value + ".wav";

                _UpdateSceneView();
            });

            IntegerField anim_sound_flame = anim_sound_foldout.Query<IntegerField>("anim_sound_flame");
            anim_sound_flame.value = _animationDataModels[id].seList[i].frame;
            if (anim_sound_flame.value < 1)
            {
                anim_sound_flame.value = 1;
            }else if (anim_sound_flame.value > 999)
            {
                anim_sound_flame.value = 999;
            }
            BaseInputFieldHandler.IntegerFieldCallback(anim_sound_flame, evt =>
            {
                if (_animationDataModels[id].seList.Count == i)
                    _animationDataModels = LoadAnimation();

                _animationDataModels[id].seList[i].frame = anim_sound_flame.value;

                _UpdateSceneView();
                
            },1,999);
            
            Button anim_sound_delete = anim_sound_foldout.Query<Button>("anim_sound_delete");
            anim_sound_delete.clickable.clicked += () =>
            {
                //データの削除
                _animationDataModels[id].seList.RemoveAt(i);

                //効果音List再描画
                RootContainer.Q<VisualElement>("anim_sound_list").Clear();
                for (var i = 0; i < _animationDataModels[id].seList.Count; i++)
                {
                    SoundInspectorCreate(i);
                    SoundInspectorSetting(id, i, soundNames);
                }

                _UpdateSceneView();
            };
        }

        private void SoundInspectorCreate(int i) {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(soundUxml);
            VisualElement labelFromUXML = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromUXML);
            labelFromUXML.Q<Foldout>("anim_sound_foldout").text = "No." + (i + 1);
            VisualElement anim_sound_foldout = labelFromUXML.Query<Foldout>("anim_sound_foldout");
            anim_sound_foldout.name = "anim_sound_foldout" + (i + 1) + "_" + _id;

            RootContainer.Q<VisualElement>("anim_sound_list").Add(labelFromUXML);
        }

        private void FlashInspectorSetting(int id, int i) {
            VisualElement anim_flash_foldout = RootContainer.Query<Foldout>("anim_flash_foldout" + (i + 1) + "_" + _id);
            IntegerField anim_flash_flame = anim_flash_foldout.Query<IntegerField>("anim_flash_flame");

            anim_flash_flame.value = _animationDataModels[id].flashList[i].frame;
            if (anim_flash_flame.value < 1)
            {
                anim_flash_flame.value = 1;
            }else if (anim_flash_flame.value > 999)
            {
                anim_flash_flame.value = 999;
            }
            BaseInputFieldHandler.IntegerFieldCallback(anim_flash_flame, evt =>
            {
                if (_animationDataModels[id].flashList.Count == i)
                    _animationDataModels = LoadAnimation();

                _animationDataModels[id].flashList[i].frame = anim_flash_flame.value;
                _UpdateSceneView();

            }, 1, 999);

            IntegerField anim_flash_time = anim_flash_foldout.Query<IntegerField>("anim_flash_time");
            anim_flash_time.value = _animationDataModels[id].flashList[i].time;
            if (anim_flash_time.value < 1)
            {
                anim_flash_time.value = 1;
            }else if (anim_flash_time.value > 999)
            {
                anim_flash_time.value = 999;
            }
            BaseInputFieldHandler.IntegerFieldCallback(anim_flash_time, evt =>
            {
                if (_animationDataModels[id].flashList.Count == i)
                    _animationDataModels = LoadAnimation();

                _animationDataModels[id].flashList[i].time = anim_flash_time.value;

                _UpdateSceneView();

            }, 1, 999);

            Label anim_flash_color_text = anim_flash_foldout.Query<Label>("anim_flash_color_text");
            var colorArr = _animationDataModels[id].flashList[i].color.Split(',');
            if (colorArr.Length == 1) colorArr = new string[3] {"0", "0", "0"};

            anim_flash_color_text.text =
                string.Format("R : {0}, G : {1}, B : {2}", colorArr[0], colorArr[1], colorArr[2]);

            ColorFieldBase anim_flash_color = anim_flash_foldout.Query<ColorFieldBase>("anim_flash_color");
            anim_flash_color.value = new Color(int.Parse(colorArr[0]) / 255f,
                int.Parse(colorArr[1]) / 255f, int.Parse(colorArr[2]) / 255f);
            anim_flash_color.RegisterValueChangedCallback(evt =>
            {
                var co = anim_flash_color.value * 255;
                var r = co.r;
                var g = co.g;
                var b = co.b;
                var colors = new List<int>
                {
                    (int) Math.Ceiling(r),
                    (int) Math.Ceiling(g),
                    (int) Math.Ceiling(b)
                };
                _animationDataModels = LoadAnimation();
                var stringData = "";
                stringData = colors[0] + "," + colors[1] + "," + colors[2];
                _animationDataModels[id].flashList[i].color = stringData;
                anim_flash_color_text.text =
                    string.Format("R : {0}, G : {1}, B : {2}", colors[0], colors[1], colors[2]);
                var color = new Color(colors[0] / 255f, colors[1] / 255f, colors[2] / 255f);
                _UpdateSceneView();
            });


            VisualElement anim_flash_type = anim_flash_foldout.Query<VisualElement>("anim_flash_type");
            var anim_flash_typeTextDropdownChoices =
                EditorLocalize.LocalizeTexts(new List<string> {"WORD_0728", "WORD_0729", "WORD_0730"});
            if (_animationDataModels[id].flashList[i].flashType <= 0)
                _animationDataModels[id].flashList[i].flashType = 1;
            var anim_flash_typePopupField = new PopupFieldBase<string>(anim_flash_typeTextDropdownChoices,
                _animationDataModels[id].flashList[i].flashType - 1);
            anim_flash_type.Add(anim_flash_typePopupField);
            anim_flash_typePopupField.RegisterValueChangedCallback(evt =>
            {
                _animationDataModels = LoadAnimation();
                _animationDataModels[id].flashList[i].flashType =
                    anim_flash_typeTextDropdownChoices.IndexOf(anim_flash_typePopupField.value) + 1;
                _UpdateSceneView();
            });
            
            Button anim_flash_delete = anim_flash_foldout.Query<Button>("anim_flash_delete");
            anim_flash_delete.clickable.clicked += () =>
            {
                //データの削除
                _animationDataModels[id].flashList.RemoveAt(i);

                //フラシュList再描画
                RootContainer.Q<VisualElement>("anim_flash_list").Clear();
                for (var i = 0; i < _animationDataModels[id].flashList.Count; i++)
                {
                    FlashInspectorCreate(i);
                    FlashInspectorSetting(id, i);
                }
                _UpdateSceneView();
            };
        }

        public static void ShowAnimationPreview(AnimationDataModel particle) {
            var animationView =
                WindowLayoutManager.GetOrOpenWindow(
                    WindowLayoutManager.WindowLayoutId.DatabaseSceneWindow) as SceneWindow;
            animationView.Create(SceneWindow.PreviewId.Animation);
            animationView.Init();

            if (particle == null) return;

            var assetManageDataModels = Editor.Hierarchy.Hierarchy.databaseManagementService.LoadAssetManage();
            AssetManageDataModel data = null;
            for (int i = 0; i < assetManageDataModels.Count; i++)
                if (assetManageDataModels[i].id == particle.particleId)
                {
                    data = assetManageDataModels[i];
                    break;
                }

            EffectPreview(
                animationView.GetAnimationPreview(),
                assetManageDataModels,
                data,
                particle);
        }

        private static void EffectPreview(
            RPGMaker.Codebase.Editor.DatabaseEditor.View.Preview.AnimationPreview sceneView,
            List<AssetManageDataModel> manageDatas,
            AssetManageDataModel assetManageData,
            AnimationDataModel animationDataModel
        ) {
            if (EditorApplication.isPlaying)
            {
                return;
            }
            if (assetManageData == null) return;

            sceneView.SetData(animationDataModel);
            string path = "";
            for (int i = 0; i < manageDatas.Count; i++)
                if (manageDatas[i].id == assetManageData.id)
                {
                    path = manageDatas[i].imageSettings[0].path;
                    break;
                }
            var isEffekseer = Path.GetExtension(path) == ".asset";
            if (isEffekseer)
            {
                var effekseer = AssetDatabase.LoadAssetAtPath<EffekseerEffectAsset>(PathManager.ANIMATION_EFFEKSEER + path);
                if (effekseer != null)
                    sceneView.ChangeParticle(
                        effekseer,
                        isEffekseer);
            }
            else
            {
                var particle = AssetDatabase.LoadAssetAtPath<GameObject>(PathManager.ANIMATION_PREFAB + path);
                if (particle != null)
                    sceneView.ChangeParticle(
                        particle,
                        isEffekseer);
            }
        }

        private void FlashInspectorCreate(int i) {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(flashUxml);
            VisualElement labelFromUXML = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromUXML);
            labelFromUXML.Q<Foldout>("anim_flash_foldout").text = "No." + (i + 1);
            Foldout anim_flash_foldout = labelFromUXML.Query<Foldout>("anim_flash_foldout");
            anim_flash_foldout.name = "anim_flash_foldout" + (i + 1) + "_" + _id;

            RootContainer.Q<VisualElement>("anim_flash_list").Add(labelFromUXML);
        }

        private void _UpdateSceneView(bool isChangeName = false) {
            databaseManagementService.SaveAnimation(_animationDataModels);
            if (isChangeName)
                _ = Editor.Hierarchy.Hierarchy.Refresh(Region.Animation);
        }
    }
}