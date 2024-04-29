#define USE_PARTIAL_LOOP
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.CoreSystem.Service.RuntimeDataManagement;
using UnityEngine;
using UnityEngine.Video;
#if USE_PARTIAL_LOOP
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Sound;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#else
using RPGMaker.Codebase.CoreSystem.Helper.SO;
#endif
#endif

namespace RPGMaker.Codebase.Runtime.Common
{
    public class SoundManager : MonoBehaviour
    {
        private enum SoundType
        {
            BGM,
            BGS,
            SE,
            ME,
        }

        public enum SoundBgmNum
        {
            Title = 0,
            Battle,
            Victory,
            Defeat,
            GameOver,
            Boat,
            Ship,
            Airship
        }

        public enum SoundSeNum
        {
            Cursor,
            OK,
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
            ActorCollapse,
            Recovery,
            Miss,
            Evasion,
            MagicEvasion,
            MagicReflection,
            Shop,
            UseItem,
            UseSkill
        }

        public const int SYSTEM_AUDIO_BGM = 100;
        public const int SYSTEM_AUDIO_BGS = 101;
        public const int SYSTEM_AUDIO_ME  = 102;
        public const int SYSTEM_AUDIO_SE  = 103;

        private static SoundManager _self;

#if USE_PARTIAL_LOOP
        private PlayControl _bgmClip;
#else
        private AudioSource _bgmClip;
#endif

        private SoundCommonDataModel _bgmSoundCommon;

        private SavedSoundData _bgmSavedSoundData;

#if USE_PARTIAL_LOOP
        private PlayControl _bgsClip;
#else
        private AudioSource          _bgsClip;
#endif
        private SoundCommonDataModel _bgsSoundCommon;

        private Coroutine _bgmFadeCoroutine;
        private Coroutine _bgsFadeCoroutine;

        private readonly List<string> _listSoundSe = new List<string>
        {
            "Cursor",
            "OK",
            "Cancel",
            "Buzzer",
            "Equip",
            "Save",
            "Load",
            "BattleStart",
            "Escape",
            "EnemyAttack",
            "EnemyDamage",
            "EnemyCollapse",
            "BossCollapse1",
            "BossCollapse2",
            "ActorDamage",
            "ActorCollapse",
            "Recovery",
            "Miss",
            "Evasion",
            "MagicEvasion",
            "MagicReflection",
            "Shop",
            "UseItem",
            "UseSkill"
        };

        private AudioSource            _meClip;
        private SoundCommonDataModel   _meSoundCommon;
        private RuntimeConfigDataModel _runtimeConfigDataModel;

        private RuntimeDataManagementService _runtimeDataManagementService;
        private List<AudioSource>            _seSources;
        private SoundCommonDataModel         _seSoundCommon;

        private readonly Dictionary<SoundBgmNum, SoundCommonDataModel> _systemsBgmSound =
            new Dictionary<SoundBgmNum, SoundCommonDataModel>();

        private SystemSettingDataModel _systemSettingDataModel;

        private readonly Dictionary<SoundSeNum, SoundCommonDataModel> _systemsSeSound =
            new Dictionary<SoundSeNum, SoundCommonDataModel>();

        private int fadetime;

        private bool  _isPlayingMe = false;
        private float _fadeSec     = 0f;
        private float _nowBgmVal   = 0f;

#if USE_PARTIAL_LOOP
        static Dictionary<string, LoopInfo> _bgmFilenameLoopInfoDic = null;
        static Dictionary<string, LoopInfo> _bgsFilenameLoopInfoDic = null;
        public class LoopInfo
        {
            public LoopInfo(int loopStartSamples, int loopEndSamples) {
                this.loopStartSamples = loopStartSamples;
                this.loopEndSamples = loopEndSamples;
            }
            public int loopStartSamples;
            public int loopEndSamples;
        }
        string _bgmLoopInfoFilename = "Assets/RPGMaker/Storage/Sounds/bgmLoopInfo.json";
        string _bgsLoopInfoFilename = "Assets/RPGMaker/Storage/Sounds/bgsLoopInfo.json";
#endif

        public static SoundManager Self() {
            if (_self == null)
            {
#if USE_PARTIAL_LOOP
                if (TforuUtility.Instance == null) return null;
#endif
                var sm = TforuUtility.Instance.gameObject.GetComponent<SoundManager>();
                if (!sm) sm = TforuUtility.Instance.gameObject.AddComponent<SoundManager>();

                _self = sm;
            }

            return _self;
        }

#if USE_PARTIAL_LOOP
        private void Start() {
            InitLoopInfo();
        }

        private void InitLoopInfo() {
            if (_bgmFilenameLoopInfoDic == null)
            {
                LoadLoopInfo(0);
            }
            if (_bgsFilenameLoopInfoDic == null)
            {
                LoadLoopInfo(1);
            }
        }
#endif

        public SoundCommonDataModel GetBgmSound() {
            return _bgmSoundCommon;
        }

        public SoundCommonDataModel GetBgsSound() {
            return _bgsSoundCommon;
        }

        public void SetData(int type, SoundCommonDataModel work) {
            switch (type)
            {
                case SYSTEM_AUDIO_BGM:
                    _bgmSoundCommon = work;
                    break;
                case SYSTEM_AUDIO_SE:
                    _seSoundCommon = work;
                    break;
                case SYSTEM_AUDIO_BGS:
                    _bgsSoundCommon = work;
                    break;
                case SYSTEM_AUDIO_ME:
                    _meSoundCommon = work;
                    break;
            }
        }

        public void SetRuntimeConfigDataModel(RuntimeConfigDataModel runtimeConfigDataModel) {
            _runtimeConfigDataModel = runtimeConfigDataModel;
        }
        public RuntimeConfigDataModel GetRuntimeConfigDataModel() {
            return _runtimeConfigDataModel;
        }

        public void Init() {
            _runtimeDataManagementService = new RuntimeDataManagementService();
            _runtimeConfigDataModel = _runtimeDataManagementService.LoadConfig();


#if USE_PARTIAL_LOOP
            if (_bgmClip == null) _bgmClip = new PlayControl(gameObject);
#else
            if (_bgmClip == null) _bgmClip = gameObject.AddComponent<AudioSource>();
#endif

#if USE_PARTIAL_LOOP
            if (_bgsClip == null) _bgsClip = new PlayControl(gameObject);
#else
            if (_bgsClip == null) _bgsClip = gameObject.AddComponent<AudioSource>();
#endif

            if (_seSources == null) _seSources = new List<AudioSource>();

            if (_meClip == null) _meClip = gameObject.AddComponent<AudioSource>();

            try
            {
                _systemsBgmSound.Add(SoundBgmNum.Title,
                    _systemSettingDataModel.bgm.title);
            }
            catch (Exception)
            {
            }

            try
            {
                _systemsBgmSound.Add(SoundBgmNum.Battle,
                    new SoundCommonDataModel(
                        DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.battleBgm.name,
                        DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.battleBgm.pan,
                        DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.battleBgm.pitch,
                        DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.battleBgm.volume));
            }
            catch (Exception)
            {
            }

            try
            {
                _systemsBgmSound.Add(SoundBgmNum.Victory,
                    new SoundCommonDataModel(
                        DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.victoryMe.name,
                        DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.victoryMe.pan,
                        DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.victoryMe.pitch,
                        DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.victoryMe.volume));
            }
            catch (Exception)
            {
            }

            try
            {
                _systemsBgmSound.Add(SoundBgmNum.Defeat,
                    new SoundCommonDataModel(
                        DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.defeatMe.name,
                        DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.defeatMe.pan,
                        DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.defeatMe.pitch,
                        DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.defeatMe.volume));
            }
            catch (Exception)
            {
            }

            try
            {
                _systemsBgmSound.Add(SoundBgmNum.GameOver,
                    _systemSettingDataModel.bgm.gameOverMe);
            }
            catch (Exception)
            {
            }

            for (var i = 0; i < _listSoundSe.Count; i++)
                try
                {
                    _systemsSeSound.Add((SoundSeNum) i,
                        _systemSettingDataModel.soundSetting.GetData(_listSoundSe[i]));
                }
                catch (Exception)
                {
                }
        }

        public async Task PlayBgm(Action action) {
            await PlayBgm();
            action();
        }

        public async Task PlayBgm() {
            if (_bgmClip.isPlaying) StopBgm();

            TryStopBgmFadeCoroutine();

#if USE_PARTIAL_LOOP
            int loopStartSamples = -1;
            int loopEndSamples = -1;
            (_bgmClip.clip, loopStartSamples, loopEndSamples) = await GetClip(_bgmSoundCommon?.name, SoundType.BGM);
            _bgmClip.SetLoopInfo(loopStartSamples, loopEndSamples);
#else
            _bgmClip.clip = await GetClip(_bgmSoundCommon?.name, SoundType.BGM);
#endif

            if (_bgmClip.clip == null) return;

            await Task.Delay(1);

            float volume = 100.0f;
            if (_runtimeConfigDataModel != null)
            {
                volume = _runtimeConfigDataModel.bgmVolume;
            }

            _bgmClip.volume = _bgmSoundCommon.volume / 100f * volume / 100f;
            _bgmClip.pitch = _bgmSoundCommon.pitch / 100f;
            _bgmClip.panStereo = _bgmSoundCommon.pan / 100f;
            _bgmClip.time = 0f;
            _bgmClip.Play();
            _bgmClip.loop = true;
        }

#if USE_PARTIAL_LOOP
        private async Task<(AudioClip, int, int)> GetClip(string clipName, SoundType soundType) {

#else
        private static async Task<AudioClip> GetClip(string clipName, SoundType soundType) {
#endif
#if UNITY_EDITOR
            string path = "";
            string path2 = "";
#endif
            string extension = ".ogg";
            string extension2 = ".wav";
#if !UNITY_EDITOR
            string type = "";
            string type2 = "";
#endif
#if USE_PARTIAL_LOOP
            int loopStartSamples = -1;
            int loopEndSamples = -1;

            InitLoopInfo();
#endif

            switch (soundType)
            {
                case SoundType.BGM:
                case SoundType.BGS:
#if UNITY_EDITOR
                    path = PathManager.SOUND_BGM;
                    path2 = PathManager.SOUND_BGS;
#endif
#if !UNITY_EDITOR
                    type = "BGM";
                    type2 = "BGS";
#endif
                    break;
                case SoundType.SE:
                case SoundType.ME:
#if UNITY_EDITOR
                    path = PathManager.SOUND_SE;
                    path2 = PathManager.SOUND_ME;
#endif
#if !UNITY_EDITOR
                    type = "SE";
                    type2 = "ME";
#endif
                    break;
            }

#if USE_PARTIAL_LOOP
            var needLoopPoints = (soundType == SoundType.BGM || soundType == SoundType.BGS);
#endif
#if UNITY_EDITOR
            //".ogg"か".wav"かある方で読み込みを行う
            AudioClip sounddata = null;
            if (File.Exists(path + clipName))
            {
                sounddata = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<AudioClip>(
                    path + clipName);
#if USE_PARTIAL_LOOP
                if (needLoopPoints) GetLoopPoints(SoundType.BGM, clipName, out loopStartSamples, out loopEndSamples);
#endif
            }
            else if (File.Exists(path + clipName + extension))
            {
                sounddata = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<AudioClip>(
                    path + clipName + extension);
#if USE_PARTIAL_LOOP
                if (needLoopPoints) GetLoopPoints(SoundType.BGM, clipName + extension, out loopStartSamples, out loopEndSamples);
#endif
            }
            else if (File.Exists(path + clipName + extension2))
            {
                sounddata = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<AudioClip>(
                    path + clipName + extension2);
#if USE_PARTIAL_LOOP
                if (needLoopPoints) GetLoopPoints(SoundType.BGM, clipName + extension2, out loopStartSamples, out loopEndSamples);
#endif
            }
            //pathに無ければpath2から取得する
            else if (File.Exists(path2 + clipName) && sounddata == null)
            {
                sounddata = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<AudioClip>(
                    path2 + clipName);
#if USE_PARTIAL_LOOP
                if (needLoopPoints) GetLoopPoints(SoundType.BGS, clipName, out loopStartSamples, out loopEndSamples);
#endif
            }
            else if (File.Exists(path2 + clipName + extension) && sounddata == null)
            {
                sounddata = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<AudioClip>(
                    path2 + clipName + extension);
#if USE_PARTIAL_LOOP
                if (needLoopPoints) GetLoopPoints(SoundType.BGS, clipName + extension, out loopStartSamples, out loopEndSamples);
#endif
            }
            else if (File.Exists(path2 + clipName + extension2) && sounddata == null)
            {
                sounddata = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<AudioClip>(
                    path2 + clipName + extension2);
#if USE_PARTIAL_LOOP
                if (needLoopPoints) GetLoopPoints(SoundType.BGS, clipName + extension2, out loopStartSamples, out loopEndSamples);
#endif
            }
#if USE_PARTIAL_LOOP
            return (sounddata, loopStartSamples, loopEndSamples);
#else
            return sounddata;
#endif
#else
            AudioClip sounddata = null;
            if (clipName == null || clipName == "") {
#if USE_PARTIAL_LOOP
                return (sounddata, loopStartSamples, loopEndSamples);
#else
                return sounddata;
#endif
            }
            //type読込
            if (clipName.EndsWith(extension) || clipName.EndsWith(extension2))
#if USE_PARTIAL_LOOP
                (sounddata, loopStartSamples, loopEndSamples) = await LoadAudioClip(type, clipName, clipName.EndsWith(extension) ? "ogg" : "wav");
#else
                sounddata = await LoadAudioClip(type, clipName, clipName.EndsWith(extension) ? "ogg" : "wav");
#endif
#if USE_PARTIAL_LOOP
            if (needLoopPoints && sounddata != null){
                needLoopPoints = false;
                GetLoopPoints(SoundType.BGM, clipName, out loopStartSamples, out loopEndSamples);
            }
#endif
            if (sounddata == null)
#if USE_PARTIAL_LOOP
                (sounddata, loopStartSamples, loopEndSamples) = await LoadAudioClip(type, clipName + extension, "ogg");
#else
                sounddata = await LoadAudioClip(type, clipName + extension, "ogg");
#endif
#if USE_PARTIAL_LOOP
            if (needLoopPoints && sounddata != null){
                needLoopPoints = false;
                GetLoopPoints(SoundType.BGM, clipName + extension, out loopStartSamples, out loopEndSamples);
            }
#endif
            if (sounddata == null)
#if USE_PARTIAL_LOOP
                (sounddata, loopStartSamples, loopEndSamples) = await LoadAudioClip(type, clipName + extension2, "wav");
#else
                sounddata = await LoadAudioClip(type, clipName + extension2, "wav");
#endif
#if USE_PARTIAL_LOOP
            if (needLoopPoints && sounddata != null){
                needLoopPoints = false;
                GetLoopPoints(SoundType.BGM, clipName + extension2, out loopStartSamples, out loopEndSamples);
            }
#endif

            //typeで読み込めなければ、type2を読込
            if (sounddata == null && (clipName.EndsWith(extension) || clipName.EndsWith(extension2)))
#if USE_PARTIAL_LOOP
                (sounddata, loopStartSamples, loopEndSamples) = await LoadAudioClip(type2, clipName, clipName.EndsWith(extension) ? "ogg" : "wav");
#else
                sounddata = await LoadAudioClip(type2, clipName, clipName.EndsWith(extension) ? "ogg" : "wav");
#endif
#if USE_PARTIAL_LOOP
            if (needLoopPoints && sounddata != null){
                needLoopPoints = false;
                GetLoopPoints(SoundType.BGS, clipName, out loopStartSamples, out loopEndSamples);
            }
#endif
            if (sounddata == null)
#if USE_PARTIAL_LOOP
                (sounddata, loopStartSamples, loopEndSamples) = await LoadAudioClip(type2, clipName + extension, "ogg");
#else
                sounddata = await LoadAudioClip(type2, clipName + extension, "ogg");
#endif
#if USE_PARTIAL_LOOP
            if (needLoopPoints && sounddata != null){
                needLoopPoints = false;
                GetLoopPoints(SoundType.BGS, clipName + extension, out loopStartSamples, out loopEndSamples);
            }
#endif
            if (sounddata == null)
#if USE_PARTIAL_LOOP
                (sounddata, loopStartSamples, loopEndSamples) = await LoadAudioClip(type2, clipName + extension2, "wav");
#else
                sounddata = await LoadAudioClip(type2, clipName + extension2, "wav");
#endif
#if USE_PARTIAL_LOOP
            if (needLoopPoints && sounddata != null){
                needLoopPoints = false;
                GetLoopPoints(SoundType.BGS, clipName + extension2, out loopStartSamples, out loopEndSamples);
            }
#endif

#if USE_PARTIAL_LOOP
            return (sounddata, loopStartSamples, loopEndSamples);
#else
            return sounddata;
#endif
#endif
        }

        public void ChangeBgmState(int volume) {
            // BGMデータがnull
            if (_bgmSoundCommon == null)
                _bgmClip.volume = volume / 100f;
            else
                _bgmClip.volume = _bgmSoundCommon.volume / 100f * (volume / 100f);
        }

        public void StopBgm() {
            if (_bgmClip.isPlaying) _bgmClip.Stop();
        }

        // BGMの保存。
        public void SaveBgm() {
            // 非再生中？
            if (_bgmClip?.isPlaying != true)
            {
                return;
            }

#if USE_PARTIAL_LOOP
            _bgmSavedSoundData = new SavedSoundData(_bgmSoundCommon.name, _bgmClip.GetCurrentSource());
#else
            _bgmSavedSoundData = new SavedSoundData(_bgmSoundCommon.name, _bgmClip);
#endif
        }

        // BGMの再開。
        public async void ContinueBgm() {
            if (_bgmClip == null || _bgmSavedSoundData == null || IsBgmPlaying(_bgmSavedSoundData.clipName))
            {
                return;
            }

            TryStopBgmFadeCoroutine();

#if USE_PARTIAL_LOOP
            int loopStartSamples = -1;
            int loopEndSamples = -1;
            (_bgmClip.clip, loopStartSamples, loopEndSamples) = await GetClip(_bgmSoundCommon.name = _bgmSavedSoundData.clipName, SoundType.BGM);
            _bgmClip.SetLoopInfo(loopStartSamples, loopEndSamples);
#else
            _bgmClip.clip = await GetClip(_bgmSoundCommon.name = _bgmSavedSoundData.clipName, SoundType.BGM);
#endif

            _bgmClip.pitch = _bgmSavedSoundData.pitch;
            _bgmClip.panStereo = _bgmSavedSoundData.panStereo;
            _bgmClip.volume = _bgmSavedSoundData.volume;
            _bgmClip.loop = _bgmSavedSoundData.loop;
            _bgmClip.time = _bgmSavedSoundData.time;

            _bgmClip.Play();
        }

        public bool IsBgmPlaying(string soundName) {
            return _bgmClip?.isPlaying == true && _bgmSoundCommon?.name == soundName && _bgmFadeCoroutine == null;
        }

        public bool IsNowBgmPlaying() {
            return _bgmClip.isPlaying && _bgmFadeCoroutine == null;
        }

        public async Task PlayBgs() {
            if (_bgsClip.isPlaying) StopBgs();

            TryStopBgsFadeCoroutine();

#if USE_PARTIAL_LOOP
            int loopStartSamples = -1;
            int loopEndSamples = -1;
            (_bgsClip.clip, loopStartSamples, loopEndSamples) = await GetClip(_bgsSoundCommon?.name, SoundType.BGS);
            _bgsClip.SetLoopInfo(loopStartSamples, loopEndSamples);
#else
            _bgsClip.clip = await GetClip(_bgsSoundCommon?.name, SoundType.BGS);
#endif

            if (_bgsClip.clip == null) return;

            float volume = 100.0f;
            if (_runtimeConfigDataModel != null)
            {
                volume = _runtimeConfigDataModel.bgsVolume;
            }

            _bgsClip.volume = _bgsSoundCommon.volume / 100f * volume / 100f;
            _bgsClip.pitch = _bgsSoundCommon.pitch / 100f;
            _bgsClip.panStereo = _bgsSoundCommon.pan / 100f;
            _bgsClip.loop = true;
            _bgsClip.Play();
        }

        public void ChangeBgsState(int volume) {
            if (_bgsSoundCommon == null) return;
            _bgsClip.volume = _bgsSoundCommon.volume / 100f * (volume / 100f);
        }

        public void StopBgs() {
            if (_bgsClip.isPlaying) _bgsClip.Stop();
        }
        
        public bool IsNowBgsPlaying() {
            return _bgsClip.isPlaying && _bgsFadeCoroutine == null;
        }


        public async Task PlayMe() {
#if USE_PARTIAL_LOOP
            (_meClip.clip, _, _) = await GetClip(_meSoundCommon?.name, SoundType.ME);
#else
            _meClip.clip = await GetClip(_meSoundCommon?.name, SoundType.ME);
#endif

            if (_meClip.clip == null) return;

            float volume = 100.0f;
            if (_runtimeConfigDataModel != null)
            {
                volume = _runtimeConfigDataModel.meVolume;
            }

            _meClip.volume = _meSoundCommon.volume / 100f * volume / 100f;
            _meClip.pitch = _meSoundCommon.pitch / 100f;
            _meClip.panStereo = _meSoundCommon.pan / 100f;
            if (_bgmClip != null && _bgmClip.isPlaying)
            {
                _bgmClip.Pause();
                //現在の値を保持してBGMをフェードイン再生するときに保持した値を使用する
                _fadeSec = _bgmClip.volume / 1;
                _nowBgmVal = _bgmClip.volume;
                _bgmClip.volume = 0f;
            }
            
            _meClip.PlayOneShot(_meClip.clip);
            _isPlayingMe = true;
        }
        
        void Update(){
            // ME再生後にBGMを再生
            if (_isPlayingMe)
            {
                if (!_meClip.isPlaying)
                {
                    if (_bgmClip != null)
                    {
                        FadeInBgm();
                    }
                }
            }
#if USE_PARTIAL_LOOP
            _bgmClip.Update();
            _bgsClip.Update();
#endif
        }

        private void FadeInBgm() {
            if (!_bgmClip.isPlaying) _bgmClip.Play();
            if(_nowBgmVal >= _bgmClip.volume)
            {
                _bgmClip.volume += Time.deltaTime * _fadeSec;
            }else
            {
                _isPlayingMe = false;
            }
        }

        public void ChangeMeState(int volume) {
            if (_meSoundCommon == null) return;
            _meClip.volume = _meSoundCommon.volume / 100f * (volume / 100f);
        }

        public void StopMe(bool initFlg = true) {
            if (_meClip.isPlaying) _meClip.Stop();
            if (initFlg) _isPlayingMe = false;
        }

        public async void PlaySe() {
            if (_seSoundCommon == null)
                return;

#if USE_PARTIAL_LOOP
            AudioClip sounddata;
            (sounddata, _, _) = await GetClip(_seSoundCommon?.name, SoundType.SE);
#else
            AudioClip sounddata = await GetClip(_seSoundCommon?.name, SoundType.SE);
#endif
            try
            {
                if (sounddata == null || gameObject == null) return;
            } catch (Exception)
            {
                return;
            }

            float volume = 100.0f;
            if (_runtimeConfigDataModel != null)
            {
                volume = _runtimeConfigDataModel.seVolume;
            }

            // ソース生成して設定（同一ソースから再生すると設定値が共有される）
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            _seSources.Add(audioSource);
            audioSource.clip = sounddata;
            audioSource.volume = _seSoundCommon.volume / 100f * volume / 100f;
            audioSource.pitch = _seSoundCommon.pitch / 100f;
            audioSource.panStereo = _seSoundCommon.pan / 100f;
            audioSource.PlayOneShot(audioSource.clip);

            try
            {
                // 音声ファイルの再生終了後に破棄する
                while (audioSource.isPlaying)
                {
                    await Task.Delay(1);
                }
                Destroy(audioSource, audioSource.clip.length);
            }
            catch (Exception) { 
            }
        }

#if !UNITY_EDITOR
#if USE_PARTIAL_LOOP
        private static async Task<(AudioClip, int, int)> LoadAudioClip(string folderName, string filename, string extention) {
#else
        private static async Task<AudioClip> LoadAudioClip(string folderName, string filename, string extention) {
#endif
            //ファイルの存在有無チェック
            AddressableExt ext = new AddressableExt();
            ext.folderName = folderName;
            ext.extention = extention;
#if USE_PARTIAL_LOOP
            int loopStartSamples = -1;
            int loopEndSamples = -1;
#endif

            if (await AddressableManager.Load.CheckResourceExistence("Assets/RPGMaker/Storage/Sounds/" + folderName + "/" + filename))
            {
                //存在した場合には、読み込みを試みる
                try
                {
                    AudioClip sounddata = null;
                    sounddata = AddressableManager.Load.LoadAssetSync<AudioClip>("Assets/RPGMaker/Storage/Sounds/" + folderName + "/" + filename);
#if USE_PARTIAL_LOOP
	                if (folderName == "BGM" || folderName == "BGS"){
	                    LoopInfo loopInfo = null;
	                    if (folderName == "BGM"){
	                        if (_bgmFilenameLoopInfoDic.ContainsKey(filename)){
	                            loopInfo = _bgmFilenameLoopInfoDic[filename];
	                        }
	                    } else {
	                        if (_bgsFilenameLoopInfoDic.ContainsKey(filename)){
	                            loopInfo = _bgsFilenameLoopInfoDic[filename];
	                        }
	                    }
	                }
                    return (sounddata, loopStartSamples, loopEndSamples);
#else
                    return sounddata;
#endif
                }
                catch (Exception) { }
            }
#if USE_PARTIAL_LOOP
            return (null, loopStartSamples, loopEndSamples);
#else
            return null;
#endif
        }
#endif

        public void ChangeSeState(int volume) {
            for (int i = 0; i < _seSources.Count; i++)
                if (_seSources[i] == null)
                {
                    _seSources.RemoveAt(i);
                    i--;
                }
                else
                    _seSources[i].volume = _seSoundCommon.volume / 100f * (volume / 100f);
        }

        public void StopSe() {
            for (int i = 0; i < _seSources.Count; i++)
                if (_seSources[i] == null)
                {
                    _seSources.RemoveAt(i);
                    i--;
                }
                else
                    _seSources[i].Stop();
        }

        public void FadeoutBgm(int type, int sec) {
            fadetime = sec;
            switch (type)
            {
                case SYSTEM_AUDIO_BGM:
                    _bgmFadeCoroutine = TforuUtility.Instance.StartCoroutine(_FadeoutBgm());
                    break;
                case SYSTEM_AUDIO_BGS:
                    _bgsFadeCoroutine = TforuUtility.Instance.StartCoroutine(_FadeoutBgs());
                    break;
            }
        }

        private IEnumerator _FadeoutBgm() {
            var fadepersec = _bgmClip.volume / fadetime;

            float timework = 0;

            float work;
            float work2;
            while (_bgmClip.volume > 0)
            {
                work = timework * fadepersec;
                work2 = _bgmClip.volume - work;
                if (work2 < 0) work2 = 0;
                _bgmClip.volume = work2;
                yield return null;
                timework = Time.deltaTime;
            }

            //フェードアウトしたタイミングで音楽を止める
            _bgmClip.Stop();

            _bgmFadeCoroutine = null;

            yield return null;
        }
        
        private void TryStopBgmFadeCoroutine() {
            if (_bgmFadeCoroutine != null)
            {
                TforuUtility.Instance.StopCoroutine(_bgmFadeCoroutine);
                _bgmFadeCoroutine = null;
            }
        }

        private IEnumerator _FadeoutBgs() {
            var fadepersec = _bgsClip.volume / fadetime;

            float timework = 0;

            float work;
            float work2;
            while (_bgsClip.volume > 0)
            {
                work = timework * fadepersec;
                work2 = _bgsClip.volume - work;
                if (work2 < 0) work2 = 0;
                _bgsClip.volume = work2;
                yield return null;
                timework = Time.deltaTime;
            }

            //フェードアウトしたタイミングで音楽を止める
            _bgsClip.Stop();

            _bgsFadeCoroutine = null;

            yield return null;
        }

        private void TryStopBgsFadeCoroutine() {
            if (_bgsFadeCoroutine != null)
            {
                TforuUtility.Instance.StopCoroutine(_bgsFadeCoroutine);
                _bgsFadeCoroutine = null;
            }
        }
        
        public bool IsMovie(string name) {
            bool flag = false;
            var moviePath = PathManager.MOVIES + name + ".mp4";
#if UNITY_EDITOR
            VideoClip movies = null;
            if (File.Exists(moviePath))
                movies = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<VideoClip>(
                    moviePath);
            flag = movies != null;
#else
            var movies = AddressableManager.Load.LoadAssetSync<VideoClip>(PathManager.MOVIES + name + ".mp4");
            flag = movies != null;
#endif
            return flag;
        }

        // 『演奏を保存』イベント用データのクラス。
        private class SavedSoundData
        {
            public string clipName;
            public float  volume;
            public float  pitch;
            public float  panStereo;
            public bool   loop;
            public float  time;

            public SavedSoundData(string clipName, AudioSource audioSource) {
                this.clipName = clipName;

                this.volume = audioSource.volume;
                this.pitch = audioSource.pitch;
                this.panStereo = audioSource.panStereo;
#if USE_PARTIAL_LOOP
                this.loop = true;
#else
                this.loop = audioSource.loop;
#endif
                this.time = audioSource.time;
            }
        }

#if USE_PARTIAL_LOOP
        public void LoadLoopInfo(int index) {
            if (index == 0 && _bgmFilenameLoopInfoDic == null)
            {
                _bgmFilenameLoopInfoDic = new Dictionary<string, LoopInfo>();
            }
            else if (index == 1 && _bgsFilenameLoopInfoDic == null)
            {
                _bgsFilenameLoopInfoDic = new Dictionary<string, LoopInfo>();
            }
            var dic = (index == 0) ? _bgmFilenameLoopInfoDic : _bgsFilenameLoopInfoDic;
            dic.Clear();
            var filename = (index == 0) ? _bgmLoopInfoFilename : _bgsLoopInfoFilename;
            List<LoopInfoModel> loopInfoModels = null;
#if UNITY_EDITOR
            string jsonStr = File.ReadAllText(filename);
            if (jsonStr.Length > 0 && jsonStr[0] == '{')    //旧フォーマット。
            {
                jsonStr = String.Empty;
            }
            if (jsonStr.Length == 0)
            {
                var path = (index == 0 ? PathManager.SOUND_BGM : PathManager.SOUND_BGS);
                for (int i = 0; i < 2; i++)
                {
                    var searchPattern = (i == 0) ? "*.ogg" : "*.wav";
                    var files = Directory.GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
                    foreach (var file in files)
                    {
                        int loopStartSamples;
                        int loopEndSamples;
                        SimpleSoundFormat.GetLoopPoints(file, out loopStartSamples, out loopEndSamples);
                        LoopInfo loopInfo = null;
                        if (loopStartSamples >= 0 && loopEndSamples >= 0)
                        {
                            loopInfo = new LoopInfo(loopStartSamples, loopEndSamples);
                        }
                        dic.Add(file.Substring(file.LastIndexOf('/') + 1), loopInfo);
                    }
                }
                SaveLoopInfo(index);
                return;
            }
            loopInfoModels = JsonHelper.FromJsonArray<LoopInfoModel>(jsonStr);
#else
            loopInfoModels = AddressableManager.Load.LoadAssetSync<LoopInfoSO>(filename).dataModels;
#endif
            foreach (var loopInfoModel in loopInfoModels)
            {
                LoopInfo loopInfo = null;
                loopInfo = new LoopInfo(loopInfoModel.start, loopInfoModel.end);
                dic.Add(loopInfoModel.name, loopInfo);
            }
            return;
        }

#if UNITY_EDITOR
        void SaveLoopInfo(int index) {
            var dic = (index == 0) ? _bgmFilenameLoopInfoDic : _bgsFilenameLoopInfoDic;
            var filename = (index == 0) ? _bgmLoopInfoFilename : _bgsLoopInfoFilename;
            var loopInfoDataModels = new List<LoopInfoModel>();

            foreach (var item in dic)
            {
                LoopInfoModel loopInfoDataModel = null;

                if (item.Value == null)
                {
                    loopInfoDataModel = new LoopInfoModel(item.Key, -1, -1);
                }
                else
                {
                    loopInfoDataModel = new LoopInfoModel(item.Key, item.Value.loopStartSamples, item.Value.loopEndSamples);
                }
                loopInfoDataModels.Add(loopInfoDataModel);
            }
            File.WriteAllText(filename, JsonHelper.ToJsonArray(loopInfoDataModels));
        }
#endif

        void GetLoopPoints(SoundType soundType, string filename, out int loopStartSamples, out int loopEndSamples) {
            loopStartSamples = -1;
            loopEndSamples = -1;
            var dic = (soundType == SoundType.BGM) ? _bgmFilenameLoopInfoDic : _bgsFilenameLoopInfoDic;
            LoopInfo loopInfo = null;
            if (!dic.ContainsKey(filename))
            {
#if UNITY_EDITOR
                var fn = $"{PathManager.SOUND_BGM}{filename}";
                SimpleSoundFormat.GetLoopPoints(fn, out loopStartSamples, out loopEndSamples);
                if (loopStartSamples >= 0 && loopEndSamples >= 0)
                {
                    loopInfo = new LoopInfo(loopStartSamples, loopEndSamples);
                }
                dic.Add(filename, loopInfo);
                SaveLoopInfo((soundType == SoundType.BGM) ? 0 : 1);
#endif
                return;
            }
            loopInfo = dic[filename];
            if (loopInfo == null) return;
            loopStartSamples = loopInfo.loopStartSamples;
            loopEndSamples = loopInfo.loopEndSamples;
        }

#if UNITY_EDITOR
        public void UpdateLoopInfo(int index, List<string> deletedFilenames, List<string> importedFilenames) {
            InitLoopInfo();
            var dic = (index == 0) ? _bgmFilenameLoopInfoDic : _bgsFilenameLoopInfoDic;
            foreach (var filename in deletedFilenames)
            {
                if (dic.ContainsKey(filename))
                {
                    dic.Remove(filename);
                }
            }
            foreach (var filename in importedFilenames)
            {
                var path = (index == 0 ? PathManager.SOUND_BGM : PathManager.SOUND_BGS);
                int loopStartSamples;
                int loopEndSamples;
                SimpleSoundFormat.GetLoopPoints($"{path}{filename}", out loopStartSamples, out loopEndSamples);
                LoopInfo loopInfo = null;
                if (loopStartSamples >= 0 && loopEndSamples >= 0)
                {
                    loopInfo = new LoopInfo(loopStartSamples, loopEndSamples);
                }
                if (dic.ContainsKey(filename))
                {
                    dic[filename] = loopInfo;
                }
                else
                {
                    dic.Add(filename, loopInfo);
                }
            }
            if (deletedFilenames.Count > 0 || importedFilenames.Count > 0)
            {
                SaveLoopInfo(index);
            }
        }
#endif
#endif
        /// <summary>
        /// BGMを変更する
        /// </summary>
        /// <param name="nextBgm"></param>
        public async void ChangeBGM(SoundCommonDataModel nextBgm)
        {
            // 現在流れているBGMと比較
            if (_bgmSoundCommon != null && _bgmSoundCommon.IsEqualsSoundInfo(nextBgm))
            {
                // 同じBGMであればボリュームとパンのみ変更
                _bgmClip.volume = nextBgm.volume / 100f * _runtimeConfigDataModel.bgmVolume / 100f;
                _bgmClip.panStereo = nextBgm.pan / 100f;
            }
            else
            {
                // 異なるBGMであればBGMを変更する
                Init();
                //データのセット
                SetData(SoundManager.SYSTEM_AUDIO_BGM, nextBgm);
                //サウンドの再生
                await PlayBgm();
            }
        }

        /// <summary>
        /// BGSを変更する
        /// </summary>
        /// <param name="nextBgs"></param>
        public async void ChangeBGS(SoundCommonDataModel nextBgs) {
            // 現在流れているBGSと比較
            if (_bgsSoundCommon != null && _bgsSoundCommon.IsEqualsSoundInfo(nextBgs)) {
                // 同じBGSであればボリュームとパンのみ変更
                _bgsClip.volume = nextBgs.volume / 100f * _runtimeConfigDataModel.bgsVolume / 100f;
                _bgsClip.panStereo = nextBgs.pan / 100f;
            } else {
                // 異なるBGSであればBGSを変更する
                Init();
                //データのセット
                SetData(SoundManager.SYSTEM_AUDIO_BGS, nextBgs);
                //サウンドの再生
                await PlayBgs();
            }
        }
    }

#if USE_PARTIAL_LOOP
    public class PlayControl
    {
        AudioSource[] _audioSources = new AudioSource[2];
        AudioClip _audioClip;
        public AudioClip audioClip { get { return _audioClip; } }
        double _startEventTime;
        double _accTime;
        double _lastStartTime;
        int _lastIndex = 0;
        bool _playing = false;
        int _loopStartSamples = -1;
        float _volume = 1.0f;
        float _pitch = 1.0f;
        float _panStereo = 0.0f;
        int _timeSamples = 0;
        public int loopStartSamples { get { return _loopStartSamples; } }
        int _loopEndSamples = -1;
        public int loopEndSamples { get { return _loopEndSamples; } }
        public bool isPlaying { get { return _playing; } }
        public float volume
        {
            get
            {
                return _volume;
            }
            set
            {
                _volume = value;
                for (int i = 0; i < _audioSources.Length; i++)
                {
                    _audioSources[i].volume = _volume;
                }
            }
        }
        public float pitch
        {
            get
            {
                return _pitch;
            }
            set
            {
                _pitch = value;
            }
        }
        public float panStereo
        {
            get
            {
                return _panStereo;
            }
            set
            {
                _panStereo = value;
                for (int i = 0; i < _audioSources.Length; i++)
                {
                    _audioSources[i].panStereo = _panStereo;
                }
            }
        }
        public float time
        {
            get
            {
                var audioSource = GetCurrentSource();
                return audioSource.time;
            }
            set
            {
                _timeSamples = (int)(value * _audioClip.frequency);
            }
        }
        public AudioClip clip
        {
            get
            {
                return _audioClip;
            }
            set
            {
                SetAudioClip(value);
            }
        }
        public bool loop
        {
            get
            {
                return true;
            }
            set
            {
                if (!value)
                {
                    Debug.LogError("loop must be true.");
                }
            }
        }

        public PlayControl(GameObject gameObject) {
            for (int i = 0; i < _audioSources.Length; i++)
            {
                _audioSources[i] = gameObject.AddComponent<AudioSource>();
            }
        }

        public void SetAudioClip(AudioClip audioClip) {
            for (int i = 0; i < _audioSources.Length; i++)
            {
                _audioSources[i].Stop();
            }
            _timeSamples = 0;
            _playing = false;

            _playing = false;
            _audioClip = audioClip;
        }

        public void SetLoopInfo(int loopStartSamples, int loopEndSamples) {
            _loopStartSamples = loopStartSamples;
            _loopEndSamples = loopEndSamples;
        }

        public void Play() {
            SetupFirstPlayRange();
            if (_loopEndSamples >= 0)
            {
                SetupNextPlayRange();
            }
            _playing = true;
        }

        public void Stop() {
            for (int i = 0; i < _audioSources.Length; i++)
            {
                _audioSources[i].Stop();
            }
            _timeSamples = 0;
            _playing = false;
        }

        public void Pause() {
            _timeSamples = GetCurrentSource().timeSamples;
            for (int i = 0; i < _audioSources.Length; i++)
            {
                _audioSources[i].Stop();
            }
            _playing = false;
        }

        public double GetPlayTime() {
            if (!_playing) return -1;
            return AudioSettings.dspTime - _startEventTime;
        }

        public AudioSource GetCurrentSource() {
            if (_audioSources[_lastIndex].isPlaying)
            {
                return _audioSources[_lastIndex];
            }
            return _audioSources[1 - _lastIndex];
        }

        void SetupFirstPlayRange() {
            _startEventTime = AudioSettings.dspTime;
            _lastStartTime = _startEventTime;
            _lastIndex = 0;
            _audioSources[_lastIndex].clip = _audioClip;
            _audioSources[_lastIndex].volume = _volume;
            _audioSources[_lastIndex].pitch = _pitch;
            _audioSources[_lastIndex].panStereo = _panStereo;
            if (_loopEndSamples < 0)
            {
                //全体ループ。
                _audioSources[_lastIndex].timeSamples = _timeSamples;
                _audioSources[_lastIndex].loop = true;
                _audioSources[_lastIndex].PlayScheduled(_lastStartTime);
                _audioSources[_lastIndex].SetScheduledEndTime(_lastStartTime + 60 * 60 * 24 * 365 *10);
                _lastIndex = 1 - _lastIndex;
                return;
            }
            if (_timeSamples > _loopEndSamples)
            {
                //ループ区間を過ぎていたら、ループ開始位置から再生開始させる。
                _timeSamples = _loopStartSamples;
            }
            _accTime = 0;
            double duration;
            if (_timeSamples >= _loopStartSamples)
            {
                duration = (double) (_loopEndSamples - _timeSamples + 1) / _audioClip.frequency;
            }
            else
            {
                duration = (double) (_loopStartSamples - _timeSamples) / _audioClip.frequency;
            }
            duration /= _pitch;
            _audioSources[_lastIndex].timeSamples = _timeSamples;
            _audioSources[_lastIndex].loop = false;
            _audioSources[_lastIndex].PlayScheduled(_lastStartTime);
            _audioSources[_lastIndex].SetScheduledEndTime(_lastStartTime + duration);
            _accTime += duration;
        }

        void SetupNextPlayRange() {
            var duration = (double) (_loopEndSamples - _loopStartSamples + 1) / _audioClip.frequency;
            duration /= _pitch;
            _lastStartTime = _startEventTime + _accTime;
            var nextIndex = 1 - _lastIndex;
            _audioSources[nextIndex].clip = _audioClip;
            _audioSources[nextIndex].timeSamples = _loopStartSamples;
            _audioSources[nextIndex].volume = _volume;
            _audioSources[nextIndex].pitch = _pitch;
            _audioSources[nextIndex].panStereo = _panStereo;
            _audioSources[nextIndex].loop = false;
            _audioSources[nextIndex].PlayScheduled(_lastStartTime);
            _audioSources[nextIndex].SetScheduledEndTime(_lastStartTime + duration);
            _accTime += duration;
        }

        public void Update() {
            if (_playing && _audioClip != null && _loopEndSamples >= 0)
            {
                var time = AudioSettings.dspTime;
                if (time >= _lastStartTime)
                {
                    _lastIndex = 1 - _lastIndex;
                    SetupNextPlayRange();
                }
            }
        }
    }

#if UNITY_EDITOR
    public class SimpleSoundFormat
    {
        static int GetInt32(byte[] bytes, int start) {
            var int32 = bytes[start + 0] | (bytes[start + 1] << 8) | (bytes[start + 2] << 16) | (bytes[start + 3] << 24);
            return int32;
        }

        static int GetSegmentSize(byte[] segmentTable) {
            foreach (var b in segmentTable)
            {
                if (b != 0xff)
                {
                    return b;
                }
            }
            return -1;
        }
        public static bool GetLoopPoints(string filename, out int loopStartSamples, out int loopEndSamples) {
            loopStartSamples = -1;
            loopEndSamples = -1;
            if (!File.Exists(filename)) return false;
            using (var fs = File.OpenRead(filename))
            {
                var bytes = new byte[0x2c];
                if (fs.Read(bytes, 0, 12) != 12) return false;
                if (Encoding.ASCII.GetString(bytes, 0, 4) == "OggS")
                {
                    // OGG
                    if (fs.Read(bytes, 0, 14) != 14) return false;
                    if (fs.Read(bytes, 0, 1) != 1) return false;
                    var segments = (int) bytes[0];
                    var segmentTable = new byte[segments];
                    if (fs.Read(segmentTable, 0, segments) != segments) return false;
                    if (fs.Read(bytes, 0, 7) != 7) return false;
                    if (bytes[0] == 0x01 && Encoding.ASCII.GetString(bytes, 1, 6) == "vorbis")
                    {
                        var ss = GetSegmentSize(segmentTable);
                        if (ss < 0)
                        {
                            return false;
                        }
                        fs.Position += ss - 7;
                        if (fs.Read(bytes, 0, 16) != 16) return false;
                        if (Encoding.ASCII.GetString(bytes, 0, 4) == "OggS")
                        {
                            if (fs.Read(bytes, 0, 10) != 10) return false;
                            if (fs.Read(bytes, 0, 1) != 1) return false;
                            segments = (int) bytes[0];
                            segmentTable = new byte[segments];
                            if (fs.Read(segmentTable, 0, segments) != segments) return false;
                            if (fs.Read(bytes, 0, 7) != 7) return false;
                            if (bytes[0] == 0x03 && Encoding.ASCII.GetString(bytes, 1, 6) == "vorbis")
                            {
                                if (fs.Read(bytes, 0, 4) != 4) return false;
                                var venderLen = GetInt32(bytes, 0);
                                fs.Position += venderLen;
                                if (fs.Read(bytes, 0, 4) != 4) return false;
                                var comments = GetInt32(bytes, 0);
                                int loopLength = -1;
                                for (int i = 0; i < comments; i++)
                                {
                                    if (fs.Read(bytes, 0, 4) != 4) return false;
                                    var commentLen = GetInt32(bytes, 0);
                                    var comment = new byte[commentLen];
                                    if (fs.Read(comment, 0, commentLen) != commentLen) return false;
                                    if (commentLen >= 10 && Encoding.ASCII.GetString(comment, 0, 10) == "LOOPSTART=")
                                    {
                                        if (!int.TryParse(Encoding.ASCII.GetString(comment, 10, commentLen - 10), out loopStartSamples))
                                        {
                                            return false;
                                        }
                                    }
                                    else if (commentLen >= 11 && Encoding.ASCII.GetString(comment, 0, 11) == "LOOPLENGTH=")
                                    {
                                        int samples;
                                        if (!int.TryParse(Encoding.ASCII.GetString(comment, 11, commentLen - 11), out samples))
                                        {
                                            return false;
                                        }
                                        if (samples <= 0) return false;
                                        loopLength = samples;
                                    }
                                    if (loopStartSamples >= 0 && loopLength >= 0)
                                    {
                                        loopEndSamples = loopStartSamples + loopLength - 1;
                                    }
                                    if (loopStartSamples >= 0 && loopEndSamples > loopStartSamples)
                                    {
                                        return true;
                                    }
                                }
                                return false;
                            }
                        }
                    }
                    return false;
                }
                if (Encoding.ASCII.GetString(bytes, 0, 4) == "RIFF" && Encoding.ASCII.GetString(bytes, 8, 4) == "WAVE")
                {
                    // WAV
                    while (true)
                    {
                        if (fs.Read(bytes, 0, 8) != 8) break;
                        if (Encoding.ASCII.GetString(bytes, 0, 4) == "smpl")
                        {
                            if (fs.Read(bytes, 0, 0x2c - 8) != 0x2c - 8) return false;
                            var sampleLoops = GetInt32(bytes, 0x24 - 8);
                            if (sampleLoops > 0)
                            {
                                if (fs.Read(bytes, 0, 0x18) != 0x18) return false;
                                loopStartSamples = GetInt32(bytes, 8);
                                loopEndSamples = GetInt32(bytes, 12);
                                return true;
                            }
                            return false;
                        }
                        fs.Position += GetInt32(bytes, 4);
                    }
                    return false;
                }
                return false;
            }

        }
    }

    class LoopInfoChecker : AssetPostprocessor {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload) {
            var typeImportedFilenames = new List<string>[] { new List<string>(), new List<string>() };
            var typeDeletedFilenames = new List<string>[] { new List<string>(), new List<string>() };
            foreach (string str in deletedAssets)
            {
                if (str.StartsWith(PathManager.SOUND_BGM))
                {
                    typeDeletedFilenames[0].Add(str.Substring(PathManager.SOUND_BGM.Length));
                }
                else if (str.StartsWith(PathManager.SOUND_BGS))
                {
                    typeDeletedFilenames[1].Add(str.Substring(PathManager.SOUND_BGS.Length));
                }
            }
            foreach (string str in movedFromAssetPaths)
            {
                if (str.StartsWith(PathManager.SOUND_BGM))
                {
                    typeDeletedFilenames[0].Add(str.Substring(PathManager.SOUND_BGM.Length));
                }
                else if (str.StartsWith(PathManager.SOUND_BGS))
                {
                    typeDeletedFilenames[1].Add(str.Substring(PathManager.SOUND_BGS.Length));
                }
            }

            foreach (string str in importedAssets)
            {
                if (str.StartsWith(PathManager.SOUND_BGM))
                {
                    typeImportedFilenames[0].Add(str.Substring(PathManager.SOUND_BGM.Length));
                }
                else if (str.StartsWith(PathManager.SOUND_BGS))
                {
                    typeImportedFilenames[1].Add(str.Substring(PathManager.SOUND_BGS.Length));
                }
            }
            foreach (string str in movedAssets)
            {
                if (str.StartsWith(PathManager.SOUND_BGM))
                {
                    typeImportedFilenames[0].Add(str.Substring(PathManager.SOUND_BGM.Length));
                }
                else if (str.StartsWith(PathManager.SOUND_BGS))
                {
                    typeImportedFilenames[1].Add(str.Substring(PathManager.SOUND_BGS.Length));
                }
            }
            if (typeDeletedFilenames[0].Count > 0 || typeDeletedFilenames[1].Count > 0 || typeImportedFilenames[0].Count > 0 || typeImportedFilenames[1].Count > 0)
            {
                var soundManager = SoundManager.Self();
                if (soundManager != null)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        soundManager.UpdateLoopInfo(i, typeDeletedFilenames[i], typeImportedFilenames[i]);
                    }
                }
            }
        }
    }
#endif

#endif
}