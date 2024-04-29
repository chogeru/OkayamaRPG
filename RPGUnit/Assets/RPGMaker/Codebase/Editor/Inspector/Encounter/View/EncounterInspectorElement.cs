using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Encounter;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Enemy;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Troop;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.Enum;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using RPGMaker.Codebase.Editor.Inspector.Map.View;
using RPGMaker.Codebase.Runtime.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.Editor.Common.View.SoundDataList;

namespace RPGMaker.Codebase.Editor.Inspector.Encounter.View
{
    /// <summary>
    /// [マップ設定]-[マップリスト]-[各マップ]-[バトル編集] Inspector
    /// </summary>
    public class EncounterInspectorElement : AbstractInspectorElement
    {
        private const int EncounterWalkMinCount         = 1;
        private const int MinWeight                     = 1;
        private const int MaxWeight                     = 9;
        private const int EnemyLeastMaxCount            = 1;
        private const int FrontViewEnemyHighestMaxCount = 8;
        private const int SideViewEnemyHighestMaxCount  = 6;
        private const int TroopAppearanceMinRatio       = 0;
        private const int TroopAppearanceMaxRatio       = 100;

        private          List<EncounterDataModel>  _encounterDataModels;

        private VisualElement _encounterEnemyArea;

        private VisualElement      _encounterEnemyGroupArea;
        private EncounterDataModel _encounterDataModel;

        private List<EnemyDataModel> _enemyDataModels;

        //敵キャラ上限数
        private int _enemyMax;

        private GameObject _gameObject;

        //ヒエラルキーを触るための各々の保持
        private readonly string               _mapId;
        private readonly int                  _regionId;

        private List<TroopDataModel> _troopDataModels;

        //敵グループ上限数
        private int _troopMax;

        //インスペクター枠のUXML
        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/Encounter/Asset/inspector_encounter.uxml"; } }

        public EncounterInspectorElement(string mapId, int regionId) {
            _mapId = mapId;
            _regionId = regionId;
            Initialize();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();

            var systemData = databaseManagementService.LoadSystem();
            _enemyDataModels = databaseManagementService.LoadEnemy();
            _troopDataModels = databaseManagementService.LoadTroop();
            _encounterDataModels = databaseManagementService.LoadEncounter();

            // マップidとリージョンidに基づいたエンカウンターデータの管理方法でデータの生成または取得。
            var targetEncounterDataModels =
                _encounterDataModels.Where(e => e.mapId == _mapId && e.region == _regionId);
            if (targetEncounterDataModels.Count() == 0)
            {
                _encounterDataModel = EncounterDataModel.CreateDefault(Guid.NewGuid().ToString());
                _encounterDataModel.name = "#" + string.Format("{0:D4}", _encounterDataModels.Count + 1);

                if (_mapId != "-1")
                {
                    _encounterDataModel.mapId = _mapId;
                    _encounterDataModel.region = _regionId;
                }

                _encounterDataModels.Add(_encounterDataModel);
            }
            else
            {
                DebugUtil.Assert(targetEncounterDataModels.Count() < 2);
                _encounterDataModel = targetEncounterDataModels.First();
            }

            SetData();
        }

        /// <summary>
        ///     基本データ
        /// </summary>
        /// <param name="RootContainer"></param>
        private void SetData() {
            // ペン類のボタン群。
            RootContainer.Q<VisualElement>("pen_buttons_root").Add(
                new PenButtonMenu(PenButtonMenu.EditTarget.Battle).GetPenButtonMenuElement());

            //サウンドの取得
            var SoundsettingDropdownChoices = _GetSoundList();
            AudioSource audioSource;
            AudioClip audioData;
            var nowPlaying = "";

            if (GameObject.FindWithTag("sound") == null)
            {
                _gameObject = new GameObject();
                _gameObject.name = "sound";
                _gameObject.tag = "sound";
                audioSource = _gameObject.AddComponent<AudioSource>();
            }
            else
            {
                audioSource = GameObject.FindWithTag("sound").transform.gameObject.GetComponent<AudioSource>();
            }

            var encounterFoldout = RootContainer.Q<Foldout>("encounter_foldout");
            encounterFoldout.text = _regionId == 0
                ? EditorLocalize.LocalizeText("WORD_0732")
                : EditorLocalize.LocalizeText("WORD_0833");

            var encounterRegionIdLabel = RootContainer.Q<Label>("encounter_region_id");
            if (_regionId == 0)
                encounterRegionIdLabel.RemoveFromHierarchy();
            else
                encounterRegionIdLabel.text =
                    EditorLocalize.LocalizeText("WORD_0834") + " " + $"[{_encounterDataModel.region}]";

            // ID設定
            // mapデータ取得
            var mapDataModels = Editor.Hierarchy.Hierarchy.mapManagementService.LoadMaps();

            var mapIdList = new List<string>();
            var mapIdNum = -1;
            var count = 0;
            foreach (var e in mapDataModels)
            {
                mapIdList.Add(e.id);
                if (e.id == _encounterDataModel.mapId)
                    mapIdNum = count;
                count++;
            }

            if (mapIdNum == -1)
            {
                _encounterDataModel.mapId = mapDataModels[0].id;
                mapIdNum = 0;
            }

            var mapDataModel = Editor.Hierarchy.Hierarchy.mapManagementService.LoadMapById(_mapId);
            var encounterMapIdLabel = RootContainer.Q<Label>("encounter_map_id");
            encounterMapIdLabel.text = EditorLocalize.LocalizeText("WORD_0995") + "  [" +
                                       mapDataModel?.SerialNumberString + "]";

            // マップ名前
            RootContainer.Q<Label>("map_name").text = mapDataModel.name;

            // 敵出現歩数。
            IntegerField encounterWalkCount = RootContainer.Query<IntegerField>("encounter_walk_count");
            encounterWalkCount.value = _encounterDataModel.step;
            encounterWalkCount.isDelayed = true;
            encounterWalkCount.RegisterValueChangedCallback(evt =>
            {
                encounterWalkCount.value = Math.Max(EncounterWalkMinCount, encounterWalkCount.value);
                _encounterDataModel.step = encounterWalkCount.value;
                Save();
            });

            // 背景画像（下）設定
            //------------------------------------------------------------------------------------------------------------------------------            
            // プレビュー画像
            Image previewImage1 = RootContainer.Query<Image>("battle_scene_bg_top_image");
            previewImage1.scaleMode = ScaleMode.ScaleToFit;
            previewImage1.image = ImageManager.LoadBattleback1(_encounterDataModel.backImage1)?.texture;

            // 画像名
            Label imageNameLabel1 = RootContainer.Query<Label>("battle_scene_bg_top_image_name");
            imageNameLabel1.text = ImageManager.GetBattlebackName(_encounterDataModel.backImage1, 1) + ".png";

            // 画像変更ボタン
            Button changeButton1 = RootContainer.Query<Button>("battle_scene_bg_top_image_change");
            changeButton1.clicked += () =>
            {
                var selectModalWindow = new ImageSelectModalWindow(PathManager.BATTLE_BACKGROUND_1, true);
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Image"), data =>
                {
                    var imageName = (string) data;
                    _encounterDataModel.backImage1 = imageName;
                    imageNameLabel1.text = ImageManager.GetBattlebackName(imageName, 1) + ".png";
                    previewImage1.image = ImageManager.LoadBattleback1(_encounterDataModel.backImage1).texture;
                    Save();
                }, _encounterDataModel.backImage1);
            };

            // 背景画像インポート
            Button importButton1 = RootContainer.Query<Button>("battle_scene_bg_top_image_import");
            importButton1.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("png", PathManager.BATTLE_BACKGROUND_1);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    _encounterDataModel.backImage1 = path;
                    imageNameLabel1.text = ImageManager.GetBattlebackName(path, 1) + ".png";
                    previewImage1.image = _encounterDataModel.backImage1 != "" ?
                        ImageManager.LoadBattleback1(_encounterDataModel.backImage1).texture : null;
                    Save();
                    Refresh();
                }
            };

            // 背景画像（上）設定
            //------------------------------------------------------------------------------------------------------------------------------            
            // プレビュー画像
            Image previewImage2 = RootContainer.Query<Image>("battle_scene_bg_bottom_image");
            previewImage2.scaleMode = ScaleMode.ScaleToFit;
            previewImage2.image = ImageManager.LoadBattleback2(_encounterDataModel.backImage2)?.texture;

            // 画像名
            Label imageNameLabel2 = RootContainer.Query<Label>("battle_scene_bg_bottom_image_name");
            imageNameLabel2.text = ImageManager.GetBattlebackName(_encounterDataModel.backImage2, 2) + ".png";

            // 画像変更ボタン
            Button changeButton2 = RootContainer.Query<Button>("battle_scene_bg_bottom_image_change");
            changeButton2.clicked += () =>
            {
                var selectModalWindow = new ImageSelectModalWindow(PathManager.BATTLE_BACKGROUND_2, true);
                selectModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Image"), data =>
                {
                    var imageName = (string) data;
                    _encounterDataModel.backImage2 = imageName;
                    imageNameLabel2.text = ImageManager.GetBattlebackName(imageName, 2) + ".png";
                    previewImage2.image = ImageManager.LoadBattleback2(_encounterDataModel.backImage2).texture;
                    Save();
                }, _encounterDataModel.backImage2);
            };

            // 背景画像インポート
            Button importButton2 = RootContainer.Query<Button>("battle_scene_bg_bottom_image_import");
            importButton2.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("png", PathManager.BATTLE_BACKGROUND_2);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    _encounterDataModel.backImage2 = path;
                    imageNameLabel2.text = ImageManager.GetBattlebackName(path, 2) + ".png";
                    previewImage2.image = _encounterDataModel.backImage2 != "" ?
                        ImageManager.LoadBattleback2(_encounterDataModel.backImage2).texture : null;
                    Save();
                    Refresh();
                }
            };

            //BGMの設定
            //▼サウンドファイル選択
            List<SoundType> soundTypes = new List<SoundType>();
            soundTypes.Add(SoundType.Bgm);
            SetBgmDropDownData(RootContainer);

            // インポート
            Button backgroundImport = RootContainer.Query<Button>("sound_import");
            backgroundImport.clickable.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("ogg", PathManager.SOUND_BGM);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileNameWithoutExtension(path);
                    _encounterDataModel.bgm.name = path;
                    Save();
                    SetBgmDropDownData(RootContainer);
                }
            };


            //▼音量
            //音量共通化
            int volumeValue;
            Slider volumeSlider = RootContainer
                .Query<Slider>("volume_slider");
            IntegerField volumeInt = RootContainer
                .Query<IntegerField>("volume_text");
            volumeInt.maxLength = 3;
            volumeSlider.value =
                _encounterDataModel.bgm.volume;
            volumeSlider.RegisterValueChangedCallback(evt =>
            {
                volumeValue = (int) volumeSlider.value;
                if (volumeValue >= 100) volumeValue = 100;

                audioSource.volume = volumeValue / 100f;
                if (volumeValue >= 100) volumeValue = 100;

                volumeInt.value = volumeValue;
                _encounterDataModel.bgm.volume = volumeInt.value;
                Save();
            });
            volumeInt.value =
                _encounterDataModel.bgm.volume;
            volumeInt.RegisterCallback<FocusOutEvent>(evt =>
            {
                volumeValue = volumeInt.value;
                if (volumeValue >= 100) volumeValue = 100;

                volumeSlider.value = volumeValue;
                audioSource.volume = volumeValue / 100f;
                if (volumeValue >= 100) volumeValue = 100;

                _encounterDataModel.bgm.volume = volumeValue;
                Save();
            });

            //▼ピッチ
            Slider pitchSlider = RootContainer
                .Query<Slider>("pitch_slider");
            IntegerField pitchInt = RootContainer
                .Query<IntegerField>("pitch_text");
            pitchInt.maxLength = 3;
            pitchSlider.value =
                _encounterDataModel.bgm.pitch;
            pitchSlider.RegisterValueChangedCallback(evt =>
            {
                audioSource.pitch = pitchSlider.value / 100f;
                pitchInt.value = (int) pitchSlider.value;
                _encounterDataModel.bgm.pitch = pitchInt.value;
                Save();
            });
            pitchInt.value =
                _encounterDataModel.bgm.pitch;
            pitchInt.RegisterCallback<FocusOutEvent>(evt =>
            {
                pitchSlider.value = pitchInt.value;
                _encounterDataModel.bgm.pitch = pitchInt.value;
                Save();
            });

            //▼位相
            Slider panSlider = RootContainer.Query<Slider>("pan_slider");
            IntegerField panInt = RootContainer
                .Query<IntegerField>("pan_text");
            panSlider.value =
                _encounterDataModel.bgm.pan;
            panSlider.RegisterValueChangedCallback(evt =>
            {
                audioSource.panStereo = panSlider.value / 100f;
                panInt.value = (int) panSlider.value;
                _encounterDataModel.bgm.pan = panInt.value;
                Save();
            });
            panInt.value =
                _encounterDataModel.bgm.pan;
            panInt.RegisterCallback<FocusOutEvent>(evt =>
            {
                panSlider.value = panInt.value;
                _encounterDataModel.bgm.pan = panInt.value;
                Save();
            });

            var buttonPlay = RootContainer.Query<Button>("musicStart")
                .AtIndex(0);
            var buttonStop = RootContainer.Query<Button>("musicStop")
                .AtIndex(0);
            buttonPlay.clicked += () =>
            {
                if (!audioSource.isPlaying || nowPlaying != _encounterDataModel.bgm.name)
                {
                    string filename = SoundHelper.InitializeFileName(soundTypes, _encounterDataModel.bgm.name, true);
                    audioData = AssetDatabase.LoadAssetAtPath<AudioClip>(filename);

                    //なしを選択した時
                    if (audioData == null)
                    {
                        //BGMを再生していたら止める
                        if (audioSource.isPlaying) audioSource.Stop();
                        return;
                    }

                    audioSource.clip = audioData;
                    nowPlaying = audioData.name;
                    //各パラメータの反映
                    audioSource.volume = _encounterDataModel.bgm.volume / 100f;
                    audioSource.pitch = _encounterDataModel.bgm.pitch / 100f;
                    audioSource.panStereo = _encounterDataModel.bgm.pan / 100f;
                    audioSource.Play();
                }
            };

            buttonStop.clicked += () => { audioSource.Stop(); };
            // モンスター出現最大数。
            IntegerField encounterEnemyMax = RootContainer.Query<IntegerField>("encounter_enemy_max");
            encounterEnemyMax.value = _encounterDataModel.enemyMax;
            encounterEnemyMax.isDelayed = true;
            encounterEnemyMax.RegisterValueChangedCallback(evt =>
            {
                encounterEnemyMax.value = ClampEnemyMaxCount(encounterEnemyMax.value);
                _encounterDataModel.enemyMax = encounterEnemyMax.value;
                Save();
            });

            // 敵グループ出現比率。
            IntegerField encounterEnemyParsent = RootContainer.Query<IntegerField>("encounter_enemy_parsent");
            encounterEnemyParsent.value = _encounterDataModel.troopPer;
            encounterEnemyParsent.isDelayed = true;
            encounterEnemyParsent.RegisterValueChangedCallback(evt =>
            {
                encounterEnemyParsent.value =
                    CSharpUtil.Clamp(encounterEnemyParsent.value, TroopAppearanceMinRatio, TroopAppearanceMaxRatio);
                _encounterDataModel.troopPer = encounterEnemyParsent.value;
                Save();
            });

            Button encounterEnemyAdd = RootContainer.Query<Button>("encounter_enemy_add");
            _encounterEnemyArea = RootContainer.Query<VisualElement>("encounter_enemy_area");

            for (var i = 0; i < _encounterDataModel.enemyList.Count; i++) _SetEnemy(i);

            //敵キャラの上限値
            _enemyMax = GetEnemyHighestMaxCount();

            //上限値を超えていた場合上限値に合わせる
            if (_encounterDataModel.enemyList.Count > _enemyMax)
                for (var i = _encounterDataModel.enemyList.Count; i > _enemyMax; i--)
                    DeleteEnemy(i - 1);

            encounterEnemyAdd.clicked += () =>
            {
                if (_enemyMax > _encounterDataModel.enemyList.Count)
                {
                    _encounterDataModel.enemyList.Add(new EncounterDataModel.Enemy(_enemyDataModels[0].id, 1, 1));
                    _SetEnemy(_encounterDataModel.enemyList.Count - 1);
                    Save();
                }
            };

            Button encounterEnemyGroupAdd = RootContainer.Query<Button>("encounter_enemy_group_add");
            _encounterEnemyGroupArea = RootContainer.Query<VisualElement>("encounter_enemy_group_area");

            for (var i = 0; i < _encounterDataModel.troopList.Count; i++) _SetTroop(i);

            //敵グループの上限値
            _troopMax = GetEnemyHighestMaxCount();

            //上限値を超えていた場合上限値に合わせる
            if (_encounterDataModel.troopList.Count > _troopMax)
                for (var i = _encounterDataModel.troopList.Count; i > _troopMax; i--)
                    DeleteTroop(i - 1);

            encounterEnemyGroupAdd.clicked += () =>
            {
                if (_troopMax > _encounterDataModel.troopList.Count)
                {
                    _encounterDataModel.troopList.Add(new EncounterDataModel.Troop(_troopDataModels[0].id, 1));
                    _SetTroop(_encounterDataModel.troopList.Count - 1);
                    Save();
                }
            };
        }

        private void SetBgmDropDownData(VisualElement RootContainer) {
            VisualElement SoundsettingDropdown = RootContainer.Query<VisualElement>("Soundsetting_dropdown");
            SoundsettingDropdown.Clear();

            List<SoundType> soundTypes = new List<SoundType>();
            soundTypes.Add(SoundType.Bgm);
            var SoundsettingDropdownPopupField = GenericPopupFieldBase<SoundDataChoice>.Add(
                RootContainer,
                "Soundsetting_dropdown",
                SoundDataList.GenerateChoices(soundTypes),
                SoundHelper.InitializeFileName(soundTypes, _encounterDataModel.bgm.name, false));

            SoundsettingDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                //0番目は曲無し
                if (SoundsettingDropdownPopupField.index == 0)
                    _encounterDataModel.bgm.name = "";
                else
                    _encounterDataModel.bgm.name = SoundsettingDropdownPopupField.value.filename + "." + SoundsettingDropdownPopupField.value.extention;

                Save();
            });
        }

        // 1つの敵キャラUIを追加。
        private void _SetEnemy(int index) {
            var enemyDataModel = _enemyDataModels.Single(
                enemyDataModel => enemyDataModel.id == _encounterDataModel.enemyList[index].enemyId);

            // ルート。
            var root = new VisualElement();

            // 敵キャラのFoldout。
            var foldout = new Foldout();
            root.Add(foldout);
            BaseClickHandler.ClickEvent(
                foldout,
                evt =>
                {
                    if (evt != (int) MouseButton.RightMouse) return;

                    _OnEnemyRightClickEventDelete(index);
                });
            foldout.AddToClassList("encounter_foldout");
            foldout.text = $"#{index + 1:000}";
            foldout.name = "foldout_setenemy_" + _encounterDataModel.mapId + "_" + _encounterDataModel.region + "_" + $"#{index + 1:000}";
            SetFoldout(foldout);

            // 敵キャラの名前の行。
            {
                var nameRowVe = new InspectorItemUnit();
                foldout.Add(nameRowVe);

                // 敵キャラの"名前"ラベル。
                nameRowVe.Add(new Label(EditorLocalize.LocalizeText("WORD_0039")));

                // 敵キャラ名のPopupField。
                var enemyPopupField =
                    new PopupFieldBase<string>(
                        _enemyDataModels.Select(enemyDataModel => enemyDataModel.name).ToList(),
                        EnemyIdToIndex(_encounterDataModel.enemyList[index].enemyId));
                nameRowVe.Add(enemyPopupField);
                enemyPopupField.style.flexGrow = 1.0f;

                enemyPopupField.RegisterValueChangedCallback(evt =>
                {
                    _encounterDataModel.enemyList[index].enemyId = _enemyDataModels[enemyPopupField.index].id;
                    Save();
                    //kindは必要か微妙
                });
            }

            // 敵キャラのレベル。
            var ve3 = new InspectorItemUnit();
            
            
            foldout.Add(ve3);
            var levelLabel = new Label(EditorLocalize.LocalizeText("WORD_0139"));
            ve3.Add(levelLabel);
            var level = new Label(enemyDataModel.level.ToString());
            level.style.height = 16;
            level.style.marginLeft = 4;
            ve3.Add(level);

            // 敵キャラの重み。
            var ve4 = new InspectorItemUnit();
            foldout.Add(ve4);
            var weight = new IntegerField();
            var weightLabel = new Label(EditorLocalize.LocalizeText("WORD_0830"));
            weight.maxLength = 3;
            weight.value = _encounterDataModel.enemyList[index].weight;
            weight.isDelayed = true;
            weight.AddToClassList("input_field_3");
            weight.RegisterValueChangedCallback(evt =>
            {
                weight.value = CSharpUtil.Clamp(weight.value, MinWeight, MaxWeight);
                _encounterDataModel.enemyList[index].weight = weight.value;
                Save();
            });
            ve4.Add(weightLabel);
            ve4.Add(weight);

            // 敵キャラの同時出現最大数。
            var ve5 = new InspectorItemUnit();
            foldout.Add(ve5);
            var maxAppearancesLabel = new Label(EditorLocalize.LocalizeText("WORD_0831"));
            ve5.Add(maxAppearancesLabel);
            var maxAppearances = new IntegerField
            {
                maxLength = 3, value = _encounterDataModel.enemyList[index].maxAppearances, isDelayed = true
            };
            maxAppearances.AddToClassList("input_field_3");
            maxAppearances.RegisterValueChangedCallback(evt =>
            {
                maxAppearances.value = ClampEnemyMaxCount(maxAppearances.value);
                _encounterDataModel.enemyList[index].maxAppearances = maxAppearances.value;
                Save();
            });
            ve5.Add(maxAppearances);

            // 削除ボタン。
            {
                var button = new Button(() => DeleteEnemy(index)) {text = EditorLocalize.LocalizeText("WORD_0383")};
                button.AddToClassList("small");
                foldout.Add(button);
            }
            _encounterEnemyArea.Add(root);
        }

        // 敵キャラの最大出現数を調整。
        private static int ClampEnemyMaxCount(int value) {
            return CSharpUtil.Clamp(value, EnemyLeastMaxCount, GetEnemyHighestMaxCount());
        }

        // 敵キャラの最大出現数の最大値を取得。
        private static int GetEnemyHighestMaxCount() {
            return DataManager.Self().GetSystemDataModel().battleScene.viewType == 0
                ?
                //フロントビューの場合。
                FrontViewEnemyHighestMaxCount
                :
                //サイドビューの場合。
                SideViewEnemyHighestMaxCount;
        }

        // 1つの敵グループUIを追加。
        private void _SetTroop(int index) {
            // ルート。
            var root = new VisualElement();

            // 敵グループのFoldout。
            var foldout = new Foldout();
            root.Add(foldout);
            foldout.AddToClassList("encounter_foldout");
            BaseClickHandler.ClickEvent(
                foldout,
                evt =>
                {
                    if (evt != (int) MouseButton.RightMouse) return;

                    _OnTroopRightClickEventDelete(index);
                });
            foldout.text = $"#{index + 1:000}";

            // 敵グループの名前の行。
            {
                var nameRowVe = new InspectorItemUnit();
                foldout.Add(nameRowVe);

                // 敵グループの"名前"ラベル。
                nameRowVe.Add(new Label(EditorLocalize.LocalizeText("WORD_0039")));

                // 敵グループ名のPopupField。
                var troopPopupField =
                    new PopupFieldBase<string>(
                        _troopDataModels.Select(troopDataModel => troopDataModel.name).ToList(),
                        TroopIdToIndex(_encounterDataModel.troopList[index].troopId));
                nameRowVe.Add(troopPopupField);
                troopPopupField.style.flexGrow = 1.0f;

                troopPopupField.RegisterValueChangedCallback(evt =>
                {
                    _encounterDataModel.troopList[index].troopId = _troopDataModels[troopPopupField.index].id;
                    Save();
                });
            }

            // 敵グループの重み。
            var ve3 = new InspectorItemUnit();
            ve3.AddToClassList("enemy_list_area");
            foldout.Add(ve3);
            var weightLabel = new Label(EditorLocalize.LocalizeText("WORD_0830"));
            ve3.Add(weightLabel);
            var weight = new IntegerField();
            weight.value = _encounterDataModel.troopList[index].weight;
            weight.isDelayed = true;
            weight.AddToClassList("input_field_3");
            weight.RegisterValueChangedCallback(evt =>
            {
                weight.value = CSharpUtil.Clamp(weight.value, MinWeight, MaxWeight);
                _encounterDataModel.troopList[index].weight = weight.value;
                Save();
            });
            ve3.Add(weight);

            // 削除ボタン。
            {
                var button = new Button(() => DeleteTroop(index)) {text = EditorLocalize.LocalizeText("WORD_0383")};
                button.AddToClassList("small");
                foldout.Add(button);
            }
            _encounterEnemyGroupArea.Add(root);
        }

        override protected void SaveContents() {
            //セーブ部位の作成
            databaseManagementService.SaveEncounter(_encounterDataModels);
        }

        // 敵キャラFoldoutを右ボタンクリック時の処理。
        private void _OnEnemyRightClickEventDelete(int index) {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_0383")), false,
                () => { DeleteEnemy(index); });

            menu.ShowAsContext();
        }

        // 敵キャラ削除。
        private void DeleteEnemy(int index) {
            if (_encounterDataModel.enemyList.Count >= index + 1)
                _encounterDataModel.enemyList.RemoveAt(index);
            _encounterEnemyArea.Clear();

            foreach (var i in Enumerable.Range(0, _encounterDataModel.enemyList.Count)) _SetEnemy(i);

            Save();
        }

        // 敵グループFoldoutを右ボタンクリック時の処理。
        private void _OnTroopRightClickEventDelete(int index) {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_0383")), false,
                () => { DeleteTroop(index); });

            menu.ShowAsContext();
        }

        // 敵グループ削除。
        private void DeleteTroop(int index) {
            if (_encounterDataModel.troopList.Count >= index + 1)
                _encounterDataModel.troopList.RemoveAt(index);
            _encounterEnemyGroupArea.Clear();

            foreach (var i in Enumerable.Range(0, _encounterDataModel.troopList.Count)) _SetTroop(i);

            Save();
        }

        private int EnemyIdToIndex(string id) {
            var returnIndex = 0;
            for (var i = 0; i < _enemyDataModels.Count; i++)
                if (_enemyDataModels[i].id == id)
                {
                    returnIndex = i;
                    break;
                }

            return returnIndex;
        }

        private int TroopIdToIndex(string id) {
            var returnIndex = 0;
            for (var i = 0; i < _troopDataModels.Count; i++)
                if (_troopDataModels[i].id == id)
                {
                    returnIndex = i;
                    break;
                }

            return returnIndex;
        }


        /// <summary>
        ///     サウンドのリスト取得
        /// </summary>
        /// <returns></returns>
        private List<string> _GetSoundList() {
            var dir = new DirectoryInfo("Assets/RPGMaker/Storage/Sounds/BGM/");
            var info = dir.GetFiles("*.ogg");
            var fileNames = new List<string> {EditorLocalize.LocalizeText("WORD_0113")};
            foreach (var f in info) fileNames.Add(f.Name.Replace(".ogg", ""));
            info = dir.GetFiles("*.wav");
            foreach (var f in info) fileNames.Add(f.Name.Replace(".wav", ""));

            return fileNames;
        }

        private enum CurrentType
        {
            Enemy = 0,
            Troop
        }
    }
}
