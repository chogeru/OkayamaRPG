using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.Editor.Common.View.SoundDataList;
using Object = UnityEngine.Object;

namespace RPGMaker.Codebase.Editor.Inspector.Sound.View
{
    /// <summary>
    /// [サウンド設定] Inspector
    /// </summary>
    public class SoundInspectorElement : AbstractInspectorElement
    {
        public enum BgmList
        {
            Title = 0,
            BattleBgm
        }

        public enum SeList
        {
            VictoryMe = 0,
            DefeatMe,
            GameOverMe,
            Cursor,
            Ok,
            Cancel,
            Buzzer,
            Equip,
            Save,
            Load,
            BattleStart,
            Escape,
            EnemyAttack,
            EnemyDamage,
            EnemyCollapse,
            BossCollapse1,
            BossCollapse2,
            ActorDamage,
            ActorDied,
            Recovery,
            Miss,
            Evasion,
            MagicEvasion,
            MagicReflection,
            Shop,
            UseItem,
            UseSkill
        }

        private const string BgmType = "BGM";
        private const string SeType  = "SE";

        //各設定項目の下限上限
        private readonly Dictionary<SoundType, List<int>> _settingLimit = new Dictionary<SoundType, List<int>>
        {
            {SoundType.Volume, new List<int> {0, 100}},
            {SoundType.Pitch, new List<int> {100, 150}},
            {SoundType.Pan, new List<int> {-100, 100}}
        };

        private AudioClip _audioData;


        private AudioSource _audioSource;

        //テスト用のobjectの保持
        private          GameObject _gameObject;
        private          string     _nowPlaying;
        private readonly int        _num;

        //テスト用のobjectのタグ
        private readonly string _objectTag = "sound";

        private readonly string _path;

        private SystemSettingDataModel _systemSettingDataModel;

        private readonly string _type;
        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/Sound/Asset/inspector_sound.uxml"; } }

        private SoundCommonDataModel _saveSoundCommonDataModel;

        public SoundInspectorElement(string type, int num) {
            _type = type;
            _num = num;
            _path = _type switch
            {
                BgmType => PathManager.SOUND_BGM,
                SeType => PathManager.SOUND_SE,
                _ => ""
            };
            Refresh();
        }

        override protected void RefreshContents() {
            base.RefreshContents();
            _systemSettingDataModel = databaseManagementService.LoadSystem();
            Initialize();
        }

        //テスト時に作成したオブジェクトの削除
        public void ClearObject() {
            if (_gameObject != null) Object.DestroyImmediate(_gameObject);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();
            var sound = SetSoundData();

            //▼サウンドファイル選択
            VisualElement soundSettingDropdown = RootContainer.Query<VisualElement>("sound_setting_dropdown");
            List<RPGMaker.Codebase.Editor.Common.Enum.SoundType> soundTypes = new List<RPGMaker.Codebase.Editor.Common.Enum.SoundType>();
            if (_type == SeType)
            {
                soundTypes.Add(RPGMaker.Codebase.Editor.Common.Enum.SoundType.Se);
                soundTypes.Add(RPGMaker.Codebase.Editor.Common.Enum.SoundType.Me);
            }
            else
            {
                soundTypes.Add(RPGMaker.Codebase.Editor.Common.Enum.SoundType.Bgm);
                soundTypes.Add(RPGMaker.Codebase.Editor.Common.Enum.SoundType.Bgs);
            }
            SetBgmDropDownData(RootContainer, soundTypes);

            // インポート
            Button backgroundImport = RootContainer.Query<Button>("sound_import");
            backgroundImport.clickable.clicked += () =>
            {
                var path = AssetManageImporter.StartToFile("ogg", _path);
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetFileName(path);
                    sound.name = path;
                    SaveSound(sound);
                    SetBgmDropDownData(RootContainer, soundTypes);
                }
            };
            //▼ボリューム
            var volumeSliderArea = RootContainer.Query<VisualElement>("volume_sliderArea");
            SliderAndFiledBase.IntegerSliderCallBack(volumeSliderArea, 0, 100, "%",
                sound.volume, evt =>
                {
                    _audioSource.volume = evt / 100f;
                    sound.volume = evt;
                    SaveSound(sound);
                });

            //▼ピッチ
            var pitchSliderArea = RootContainer.Query<VisualElement>("pitch_sliderArea");
            SliderAndFiledBase.IntegerSliderCallBack(pitchSliderArea, 50, 150, "%",
                sound.pitch, evt =>
                {
                    _audioSource.pitch = evt / 100f;
                    sound.pitch = evt; 
                    SaveSound(sound);
                });
            
            //▼位相
            var panSliderArea = RootContainer.Query<VisualElement>("pan_sliderArea");
            SliderAndFiledBase.IntegerSliderCallBack(panSliderArea, -100, 100, "",
                sound.pan, evt =>
                {
                    _audioSource.panStereo = evt / 100f;
                    sound.pan = evt;
                    SaveSound(sound);
                });

            //サウンドテスト処理

            var buttonPlay = RootContainer.Query<Button>("musicStart").AtIndex(0);
            var buttonStop = RootContainer.Query<Button>("musicStop").AtIndex(0);

            //テストで音を鳴らすオブジェクトの準備
            if (GameObject.FindWithTag(_objectTag) != null)
            {
                //既に存在する場合はリサイクルする
                _gameObject = GameObject.FindWithTag(_objectTag).transform.gameObject;
                _audioSource = _gameObject.GetComponent<AudioSource>();
            }
            else
            {
                _gameObject = new GameObject();
                _gameObject.name = _objectTag;
                _gameObject.tag = _objectTag;
                _audioSource = _gameObject.AddComponent<AudioSource>();
            }


            //再生
            buttonPlay.clicked += () =>
            {
                if (_type == BgmType)
                {
                    //同じBGMが再生されていた際には再生をしない
                    if (!_audioSource.isPlaying || _nowPlaying != sound.name) play();
                }
                else
                    play();

                void play() {
                    string filename = SoundHelper.InitializeFileName(soundTypes, sound.name, true);
                    _audioData = AssetDatabase.LoadAssetAtPath<AudioClip>(filename);

                    //なしを選択した時
                    if (_audioData == null) 
                    {
                        //BGMを再生していたら止める
                        if(_audioSource.isPlaying) _audioSource.Stop();
                        return;
                    }

                    _audioSource.clip = _audioData;
                    _nowPlaying = _audioData.name;

                    //各パラメータの反映
                    _audioSource.volume = sound.volume / 100f;
                    _audioSource.pitch = sound.pitch / 100f;
                    _audioSource.panStereo = sound.pan / 100f;

                    _audioSource.Play();
                }
            };

            //停止
            buttonStop.clicked += () => { _audioSource.Stop(); };
        }

        /// <summary>
        /// サウンドのセレクトボックス表示
        /// </summary>
        /// <param name="RootContainer"></param>
        private void SetBgmDropDownData(VisualElement RootContainer, List<RPGMaker.Codebase.Editor.Common.Enum.SoundType> soundTypes) {
            var sound = SetSoundData();

            VisualElement SoundsettingDropdown = RootContainer.Query<VisualElement>("sound_setting_dropdown");
            SoundsettingDropdown.Clear();

            var SoundsettingDropdownPopupField = GenericPopupFieldBase<SoundDataChoice>.Add(
                RootContainer,
                "sound_setting_dropdown",
                SoundDataList.GenerateChoices(soundTypes),
                SoundHelper.InitializeFileName(soundTypes, sound.name, false));

            SoundsettingDropdownPopupField.RegisterValueChangedCallback(evt =>
            {
                if (SoundsettingDropdownPopupField.index == 0)
                    sound.name = "";
                else
                    sound.name = SoundsettingDropdownPopupField.value.filename + "." + SoundsettingDropdownPopupField.value.extention;
                SaveSound(sound);
            });
        }

        //サウンド設定の値の制限
        //「volume」「pitch」「pan」のいずれかと入力された値が入ってくる
        private int SettingLimit(SoundType type, int value) {
            value = Math.Min(value, _settingLimit[type][1]);
            value = Math.Max(value, _settingLimit[type][0]);
            return value;
        }


        /// <summary>
        ///     BGMorSE
        /// </summary>
        /// <returns></returns>
        private SoundCommonDataModel SetSoundData() {
            if (_type == BgmType)
                return _GetBgm();
            return _GetSe();
        }

        /// <summary>
        ///     BGM種類取得
        /// </summary>
        /// <returns></returns>
        private SoundCommonDataModel _GetBgm() {
            var
                sound = new SoundCommonDataModel("", 100, 90, 90);

            switch ((BgmList) _num)
            {
                case BgmList.Title:
                    sound = _systemSettingDataModel.bgm.title;
                    break;
                case BgmList.BattleBgm:
                    sound = _systemSettingDataModel.bgm.battleBgm;
                    break;
            }

            return sound;
        }

        /// <summary>
        ///     SE種類取得
        /// </summary>
        /// <returns></returns>
        private SoundCommonDataModel _GetSe() {
            var
                sound = new SoundCommonDataModel("", 100, 90, 90);

            switch ((SeList) _num)
            {
                case SeList.VictoryMe:
                    sound = _systemSettingDataModel.bgm.victoryMe;
                    break;
                case SeList.DefeatMe:
                    sound = _systemSettingDataModel.bgm.defeatMe;
                    break;
                case SeList.GameOverMe:
                    sound = _systemSettingDataModel.bgm.gameOverMe;
                    break;
                case SeList.Cursor:
                    sound = _systemSettingDataModel.soundSetting.cursor;
                    break;
                case SeList.Ok:
                    sound = _systemSettingDataModel.soundSetting.ok;
                    break;
                case SeList.Cancel:
                    sound = _systemSettingDataModel.soundSetting.cancel;
                    break;
                case SeList.Buzzer:
                    sound = _systemSettingDataModel.soundSetting.buzzer;
                    break;
                case SeList.Equip:
                    sound = _systemSettingDataModel.soundSetting.equip;
                    break;
                case SeList.Save:
                    sound = _systemSettingDataModel.soundSetting.save;
                    break;
                case SeList.Load:
                    sound = _systemSettingDataModel.soundSetting.load;
                    break;
                case SeList.BattleStart:
                    sound = _systemSettingDataModel.soundSetting.battleStart;
                    break;
                case SeList.Escape:
                    sound = _systemSettingDataModel.soundSetting.escape;
                    break;
                case SeList.EnemyAttack:
                    sound = _systemSettingDataModel.soundSetting.enemyAttack;
                    break;
                case SeList.EnemyDamage:
                    sound = _systemSettingDataModel.soundSetting.enemyDamage;
                    break;
                case SeList.EnemyCollapse:
                    sound = _systemSettingDataModel.soundSetting.enemyCollapse;
                    break;
                case SeList.BossCollapse1:
                    sound = _systemSettingDataModel.soundSetting.bossCollapse1;
                    break;
                case SeList.BossCollapse2:
                    sound = _systemSettingDataModel.soundSetting.bossCollapse2;
                    break;
                case SeList.ActorDamage:
                    sound = _systemSettingDataModel.soundSetting.actorDamage;
                    break;
                case SeList.ActorDied:
                    sound = _systemSettingDataModel.soundSetting.actorDied;
                    break;
                case SeList.Recovery:
                    sound = _systemSettingDataModel.soundSetting.recovery;
                    break;
                case SeList.Miss:
                    sound = _systemSettingDataModel.soundSetting.miss;
                    break;
                case SeList.Evasion:
                    sound = _systemSettingDataModel.soundSetting.evasion;
                    break;
                case SeList.MagicEvasion:
                    sound = _systemSettingDataModel.soundSetting.magicEvasion;
                    break;
                case SeList.MagicReflection:
                    sound = _systemSettingDataModel.soundSetting.magicReflection;
                    break;
                case SeList.Shop:
                    sound = _systemSettingDataModel.soundSetting.shop;
                    break;
                case SeList.UseItem:
                    sound = _systemSettingDataModel.soundSetting.useItem;
                    break;
                case SeList.UseSkill:
                    sound = _systemSettingDataModel.soundSetting.useSkill;
                    break;
            }

            return sound;
        }

        private void SaveSound(SoundCommonDataModel sound) {
            _saveSoundCommonDataModel = sound;
            base.Save();
        }

        protected override void SaveContents() {
            base.SaveContents();

            if (_type == BgmType)
                _SaveBgm(_saveSoundCommonDataModel);
            else
                _SaveSe(_saveSoundCommonDataModel);

            databaseManagementService.SaveSystem(_systemSettingDataModel);
        }

        private void _SaveBgm(SoundCommonDataModel sound) {
            switch ((BgmList) _num)
            {
                case BgmList.Title:
                    _systemSettingDataModel.bgm.title = sound;
                    break;
                case BgmList.BattleBgm:
                    _systemSettingDataModel.bgm.battleBgm = sound;
                    break;
            }
        }

        private void _SaveSe(SoundCommonDataModel sound) {
            switch ((SeList) _num)
            {
                case SeList.VictoryMe:
                    _systemSettingDataModel.bgm.victoryMe = sound;
                    break;
                case SeList.DefeatMe:
                    _systemSettingDataModel.bgm.defeatMe = sound;
                    break;
                case SeList.GameOverMe:
                    _systemSettingDataModel.bgm.gameOverMe = sound;
                    break;
                case SeList.Cursor:
                    _systemSettingDataModel.soundSetting.cursor = sound;
                    break;
                case SeList.Ok:
                    _systemSettingDataModel.soundSetting.ok = sound;
                    break;
                case SeList.Cancel:
                    _systemSettingDataModel.soundSetting.cancel = sound;
                    break;
                case SeList.Buzzer:
                    _systemSettingDataModel.soundSetting.buzzer = sound;
                    break;
                case SeList.Equip:
                    _systemSettingDataModel.soundSetting.equip = sound;
                    break;
                case SeList.Save:
                    _systemSettingDataModel.soundSetting.save = sound;
                    break;
                case SeList.Load:
                    _systemSettingDataModel.soundSetting.load = sound;
                    break;
                case SeList.BattleStart:
                    _systemSettingDataModel.soundSetting.battleStart = sound;
                    break;
                case SeList.Escape:
                    _systemSettingDataModel.soundSetting.escape = sound;
                    break;
                case SeList.EnemyAttack:
                    _systemSettingDataModel.soundSetting.enemyAttack = sound;
                    break;
                case SeList.EnemyDamage:
                    _systemSettingDataModel.soundSetting.enemyDamage = sound;
                    break;
                case SeList.EnemyCollapse:
                    _systemSettingDataModel.soundSetting.enemyCollapse = sound;
                    break;
                case SeList.BossCollapse1:
                    _systemSettingDataModel.soundSetting.bossCollapse1 = sound;
                    break;
                case SeList.BossCollapse2:
                    _systemSettingDataModel.soundSetting.bossCollapse2 = sound;
                    break;
                case SeList.ActorDamage:
                    _systemSettingDataModel.soundSetting.actorDamage = sound;
                    break;
                case SeList.ActorDied:
                    _systemSettingDataModel.soundSetting.actorDied = sound;
                    break;
                case SeList.Recovery:
                    _systemSettingDataModel.soundSetting.recovery = sound;
                    break;
                case SeList.Miss:
                    _systemSettingDataModel.soundSetting.miss = sound;
                    break;
                case SeList.Evasion:
                    _systemSettingDataModel.soundSetting.evasion = sound;
                    break;
                case SeList.MagicEvasion:
                    _systemSettingDataModel.soundSetting.magicEvasion = sound;
                    break;
                case SeList.MagicReflection:
                    _systemSettingDataModel.soundSetting.magicReflection = sound;
                    break;
                case SeList.Shop:
                    _systemSettingDataModel.soundSetting.shop = sound;
                    break;
                case SeList.UseItem:
                    _systemSettingDataModel.soundSetting.useItem = sound;
                    break;
                case SeList.UseSkill:
                    _systemSettingDataModel.soundSetting.useSkill = sound;
                    break;
            }
        }

        //各種設定項目
        private enum SoundType
        {
            Volume = 0,
            Pitch,
            Pan
        }
    }
}