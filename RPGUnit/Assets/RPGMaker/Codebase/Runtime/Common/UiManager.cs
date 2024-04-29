using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.UiSetting;
using RPGMaker.Codebase.Runtime.Battle.Window;

namespace RPGMaker.Codebase.Runtime.Common
{
    public class UiManager : WindowBattle
    {
        private static UiSettingDataModel.CommonMenu _commonMenu;
        private static SystemSettingDataModel        _systemSettingDataModel;
        private static int                           _uiPattern;

        public static string partyCommandWindow => _commonMenu.windowBackgroundImage.image;
        public static string partyCommandLine => _commonMenu.windowFrameImage.image;
        public static string actorCommandWindow => _commonMenu.windowBackgroundImage.image;
        public static string actorCommandLine => _commonMenu.windowFrameImage.image;
        public static string battleStatusWindow => _commonMenu.windowBackgroundImage.image;
        public static string battleStatusLine => _commonMenu.windowFrameImage.image;
        public static string battleActorWindow => _commonMenu.windowBackgroundImage.image;
        public static string battleActorLine => _commonMenu.windowFrameImage.image;
        public static string messageWindow => _commonMenu.windowBackgroundImage.image;
        public static string messageLine => _commonMenu.windowFrameImage.image;
        public static string battleEnemyWindow => _commonMenu.windowBackgroundImage.image;
        public static string battleEnemyLine => _commonMenu.windowFrameImage.image;
        public static string battleSkillWindow => _commonMenu.windowBackgroundImage.image;
        public static string battleSkillLine => _commonMenu.windowFrameImage.image;
        public static string battleItemWindow => _commonMenu.windowBackgroundImage.image;
        public static string battleItemLine => _commonMenu.windowFrameImage.image;
    }
}