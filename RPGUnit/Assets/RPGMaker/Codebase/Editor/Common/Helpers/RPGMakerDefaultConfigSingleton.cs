using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RPGMaker.Codebase.Editor.Common
{

    [FilePath("ProjectSettings/RPGMakerDefaultConfigFlags.asset", FilePathAttribute.Location.ProjectFolder)]
    public class RPGMakerDefaultConfigSingleton : ScriptableSingleton<RPGMakerDefaultConfigSingleton>
    {
        public const string InitializeModeId            = "initialize";
        public const string RpgMakerUniteModeId         = "rpgmaker";
        public const string DefaultEditorModeId         = "default";
        public const string RpgMakerUniteWindowModeId   = "rpgmaker_window";

        [SerializeField] private bool m_defaultSettingsConfigured;
        [SerializeField] private string m_uniteMode = DefaultEditorModeId;
        [SerializeField] private bool m_revertLayoutSetting = true;

        private const string DIALOG_TITLE = "RPGMaker Unite";
        private const string CONFIG_OVERWRITE_TEXT = "WORD_5015"; //RPGMaker Unite requires to overwrite your project...
        private const string OVERWRITE_TEXT    = "WORD_5017"; //Overwrite
        private const string CANCEL_TEXT = "WORD_5020"; //Cancel

        private bool DefaultSettingsConfigured
        {
            get => m_defaultSettingsConfigured;
            set
            {
                var oldValue = m_defaultSettingsConfigured;
                m_defaultSettingsConfigured = value;
                if (oldValue != value)
                {
                    Save(true);
                }
            }
        }

        public string UniteMode {
            get => m_uniteMode;
            set {
                m_uniteMode = value;
                Save(true);
            }
        }

        public bool RevertLayoutSetting
        {
            get => m_revertLayoutSetting;
            set
            {
                var oldValue = m_revertLayoutSetting;
                m_revertLayoutSetting = value;
                if (oldValue != value)
                {
                    Save(true);
                }
            }
        }

        static internal int InitializeDefaultSettingsForRPGMakerUnite()
        {
            if (instance.DefaultSettingsConfigured)
            {
                //初期化済み
                return 1;
            }

            return ConfigureDefaultSettingsForRPGMakerUnite();
        }

        public static int ConfigureDefaultSettingsForRPGMakerUnite() {

            var message = EditorLocalize.LocalizeText(CONFIG_OVERWRITE_TEXT);
            var overwrite = EditorLocalize.LocalizeText(OVERWRITE_TEXT);
            var cancel = EditorLocalize.LocalizeText(CANCEL_TEXT);

            if (EditorUtility.DisplayDialog(DIALOG_TITLE, message, overwrite, cancel))
            {
                instance.DefaultSettingsConfigured = true;
                instance.UniteMode = InitializeModeId;

                SortingLayerHelper.ConfigureDefaultSettings();
                TagManagerHelper.ConfigureDefaultSettings();
                ProjectSettingsHelper.ConfigureDefaultSettings();
                GraphicsSettingsHelper.ConfigureDefaultSettings();
                EditorBuildSettingsHelper.ConfigureDefaultSettings();

                AssetDatabase.SaveAssets();

                // Uniteは、新InputSystemが有効ではないと動かないため、Activeにする
                var playerSettings = Resources.FindObjectsOfTypeAll<PlayerSettings>().FirstOrDefault();
                if (playerSettings != null)
                {
                    var playerSettingsObject = new SerializedObject(playerSettings);
                    var property = playerSettingsObject.FindProperty("activeInputHandler");
                    if (property.intValue != 2)
                    {
                        property.intValue = 2;
                        property.serializedObject.ApplyModifiedProperties();

                        //初期化、要再起動
                        return 3;
                    }
                }

                //初期化
                return 2;
            }

            //未初期化
            return 0;
        }
    }
}