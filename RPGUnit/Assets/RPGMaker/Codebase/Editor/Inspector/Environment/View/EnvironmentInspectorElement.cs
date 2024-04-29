using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.Environment.View
{
    /// <summary>
    /// [初期設定]-[環境設定] Inspector
    /// </summary>
    public class EnvironmentInspectorElement : AbstractInspectorElement
    {
        //スタンドアロン（WindowsPC、MacPC）
        //iPhone
        //Android
        //WebGL
        //Windows ストア アプリ
        //tvOS
        //Nintendo Switch
        //PlayStation 4
        //PlayStation 5
        //Xbox One

        private readonly List<string> _displaySizeList = new List<string>
        {
            "1920 x 1080",
            "1280 x 720"
        };


        private readonly List<string> _platformList = new List<string>
        {
            "WORD_2585",
            "Android",
            "iPhone",
            "WebGL",
            "WORD_2586",
            "tvOS",
            "Nintendo Switch",
            "PlayStation 4",
            "PlayStation 5",
            "Xbox One"
        };

        private SystemSettingDataModel _systemSettingDataModel;

        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/Environment/Asset/inspector_environment.uxml"; } }

        public EnvironmentInspectorElement() {
            Initialize();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();

            _systemSettingDataModel = databaseManagementService.LoadSystem();

            var sizeList = new List<string>
            {
                _displaySizeList[0]
#if UNITY_SWITCH
                , _displaySizeList[1]
#endif
            };
            var commonDropdownPopupField =
                new PopupFieldBase<string>(sizeList, _systemSettingDataModel.displaySize);


            //プラットフォームのプルダウン設定
            VisualElement platform = RootContainer.Query<VisualElement>("platform");

            var nowPlatform = GetNowPlatform();
            var dropdownPopupField =
                new PopupFieldBase<string>(EditorLocalize.LocalizeTexts(_platformList), (int) nowPlatform);

            dropdownPopupField.RegisterValueChangedCallback(callback =>
            {
                var platform = (Platform) dropdownPopupField.index;
                var updateInputSystem = (nowPlatform == Platform.NintendoSwitch || platform == Platform.NintendoSwitch);
                if (SelectedPlatform(platform) && updateInputSystem)
                {
                    SaveInputSystem(platform);
                }
            });
            platform.Add(dropdownPopupField);

            //画面サイズ
            VisualElement displaySize = RootContainer.Query<VisualElement>("display_size");
            commonDropdownPopupField.RegisterValueChangedCallback(callback =>
            {
                _systemSettingDataModel.displaySize = commonDropdownPopupField.index;
                Save();
            });
            displaySize.Add(commonDropdownPopupField);

            //コントローラー
            RadioButton controllerToggle0 = RootContainer.Query<RadioButton>("radioButton-environment-display1");
            RadioButton controllerToggle1 = RootContainer.Query<RadioButton>("radioButton-environment-display2");
            controllerToggle0.value = _systemSettingDataModel.isController == 0;
            controllerToggle1.value = _systemSettingDataModel.isController == 1;

            var targetTeamActions = new List<Action>
            {
                //使用する
                () =>
                {
                    _systemSettingDataModel.isController = 0;
                    Save();
                    SaveInputSystem(GetNowPlatform());
                },
                //使用しない
                () =>
                {
                    _systemSettingDataModel.isController = 1;
                    Save();
                    SaveInputSystem(GetNowPlatform());
                }
            };
            
            new CommonToggleSelector().SetRadioSelector(new List<RadioButton>(){controllerToggle0, controllerToggle1}, 
                _systemSettingDataModel.isController, targetTeamActions);
        }

        /// <summary>
        ///     プラットフォームの切り替え
        /// </summary>
        /// <param name="platform"></param>
        private bool SelectedPlatform(Platform platform) {
            var isCheck = true;
            switch (platform)
            {
                case Platform.Standalone:
                    isCheck = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone,
                        BuildTarget.StandaloneWindows);
                    break;
                case Platform.Android:
                    isCheck = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android,
                        BuildTarget.Android);
                    break;
                case Platform.Ios:
                    isCheck = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
                    break;
                case Platform.Webgl:
                    isCheck = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL,
                        BuildTarget.WebGL);
                    break;
                case Platform.WindowsStore:
                    isCheck = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WSA,
                        BuildTarget.WSAPlayer);
                    break;
                case Platform.TvOs:
                    isCheck = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.tvOS, BuildTarget.tvOS);
                    break;
                case Platform.NintendoSwitch:
                    isCheck = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Switch,
                        BuildTarget.Switch);
                    break;
                case Platform.PlayStation4:
                    isCheck = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.PS4, BuildTarget.PS4);
                    break;
                case Platform.PlayStation5:
                    isCheck = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.PS5, BuildTarget.PS5);
                    break;
                case Platform.XboxOne:
                    isCheck = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.XboxOne,
                        BuildTarget.XboxOne);
                    break;
            }

            if (!isCheck) EditorUtility.DisplayDialog("プラットフォーム切り替え", "指定されたプラットフォームのパッケージがありません。", "閉じる");
            return isCheck;
        }

        /// <summary>
        ///     現在のプラットフォーム
        /// </summary>
        /// <returns></returns>
        private Platform GetNowPlatform() {
            var platform = Platform.Standalone;
#if UNITY_STANDALONE
            platform = Platform.Standalone;
#elif UNITY_STANDALONE_WIN
			platform = Platform.Standalone;
#elif UNITY_ANDROID
			platform = Platform.Android;
#elif UNITY_IOS
			platform = Platform.Ios;
#elif UNITY_WEBGL
			platform = Platform.Webgl;
#elif UNITY_SWITCH
            platform = Platform.NintendoSwitch;
#elif UNITY_PS4
            platform = Platform.PlayStation4;
#endif
            return platform;
        }

        override protected void SaveContents() {
            databaseManagementService.SaveSystem(_systemSettingDataModel);
        }

        private enum Platform
        {
            Standalone,
            Android,
            Ios,
            Webgl,
            WindowsStore,
            TvOs,
            NintendoSwitch,
            PlayStation4,
            PlayStation5,
            XboxOne
        }

        /// <summary>
        /// プレハブを更新
        /// </summary>
        private void SaveInputSystem(Platform platform) {
            var inputSystemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/InputSystem.prefab");
            var inputSystemObj = UnityEngine.Object.Instantiate(inputSystemPrefab);
            var defaultInput = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                    platform == Platform.NintendoSwitch ?
                        "Assets/RPGMaker/InputSystem/rpgmaker_switch.inputactions" :
                    platform == Platform.PlayStation4 ?
                        "Assets/RPGMaker/InputSystem/rpgmaker_ps4.inputactions" :
                        "Assets/RPGMaker/InputSystem/rpgmaker.inputactions"
                );
            var nonControllerInput = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/RPGMaker/InputSystem/rpgmakerNonController.inputactions");

            var playerInput = inputSystemObj.GetComponent<PlayerInput>();
            if (_systemSettingDataModel.isController == 0)
            {
                playerInput.actions = defaultInput;
                //EventSystemは設定情報があるため、プレハブごと更新
                var eventSystemDefaultPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                        platform == Platform.NintendoSwitch ?
                            "Assets/Resources/EventSystemDefaultSwitch.prefab" :
                        platform == Platform.PlayStation4 ?
                            "Assets/Resources/EventSystemDefaultPs4.prefab" :
                            "Assets/Resources/EventSystemDefault.prefab"
                    );
                var eventSystemDefaultObj = UnityEngine.Object.Instantiate(eventSystemDefaultPrefab);
                PrefabUtility.SaveAsPrefabAsset(eventSystemDefaultObj, "Assets/Resources/EventSystem.prefab");
                UnityEngine.Object.DestroyImmediate(eventSystemDefaultObj);
            }
            else
            {
                playerInput.actions = nonControllerInput;
                //EventSystemは設定情報があるため、プレハブごと更新
                var eventSystemNonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/EventSystemNon.prefab");
                var eventSystemNonObj = UnityEngine.Object.Instantiate(eventSystemNonPrefab);
                PrefabUtility.SaveAsPrefabAsset(eventSystemNonObj, "Assets/Resources/EventSystem.prefab");
                UnityEngine.Object.DestroyImmediate(eventSystemNonObj);
            }

            // 保存
            PrefabUtility.SaveAsPrefabAsset(inputSystemObj, "Assets/Resources/InputSystem.prefab");
            UnityEngine.Object.DestroyImmediate(inputSystemObj);
        }
    }
}