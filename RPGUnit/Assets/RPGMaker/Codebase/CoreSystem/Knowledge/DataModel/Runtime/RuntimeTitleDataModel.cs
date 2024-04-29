using System;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime
{
    [Serializable]
    public class RuntimeTitleDataModel
    {
        public Bgm             bgm;
        public string          gameTitle;
        public GameTitleCommon gameTitleCommon;
        public GameTitleImage  gameTitleImage;
        public GameTitleText   gameTitleText;
        public string          note;
        public StartMenu       startMenu;
        public string          titleBackgroundImage;
        public TitleFront      titleFront;

	    public bool isEqual(RuntimeTitleDataModel data)
	    {
	        return bgm.isEqual(data.bgm) &&
	               gameTitle == data.gameTitle &&
	               gameTitleCommon.isEqual(data.gameTitleCommon) &&
	               gameTitleImage.isEqual(data.gameTitleImage) &&
	               gameTitleText.isEqual(data.gameTitleText) &&
	               note == data.note &&
	               startMenu.isEqual(data.startMenu) &&
	               titleBackgroundImage == data.titleBackgroundImage &&
	               titleFront.isEqual(data.titleFront);
	    }
    
        [Serializable]
        public class TitleFront
        {
            public string image;
            public int[]  position;
            public float  scale;

            public bool isEqual(TitleFront data) {
                if (image != data.image ||
                    scale != data.scale ||
                    position.Length != data.position.Length)
                    return false;

                for (int i = 0; i < position.Length; i++)
                    if (position[i] != data.position[i])
                        return false;

                return true;
            }
        }

        [Serializable]
        public class GameTitleCommon
        {
            public int   gameTitleType;
            public int[] position;

            public bool isEqual(GameTitleCommon data) {
                if (gameTitleType != data.gameTitleType ||
                	position.Length != data.position.Length)
                    return false;

                for (int i = 0; i < position.Length; i++)
                    if (position[i] != data.position[i])
                        return false;

                return true;
            }
        }

        [Serializable]
        public class GameTitleText
        {
            public int[]  color;
            public string font;
            public int    size;

            public bool isEqual(GameTitleText data) {
                if (font != data.font ||
                    size != data.size ||
                    color.Length != data.color.Length)
                    return false;

                for (int i = 0; i < color.Length; i++)
                    if (color[i] != data.color[i])
                        return false;

                return true;
            }
        }

        [Serializable]
        public class GameTitleImage
        {
            public string image;
            public float  scale;

		    public bool isEqual(GameTitleImage data)
		    {
		        return image == data.image &&
		               scale == data.scale;
		    }
        }

        [Serializable]
        public class Bgm
        {
            public string name;
            public int    pan;
            public int    pitch;
            public int    volume;

		    public bool isEqual(Bgm data)
		    {
		        return name == data.name &&
		               pan == data.pan &&
		               pitch == data.pitch &&
		               volume == data.volume;
		    }
        }

        [Serializable]
        public class StartMenu
        {
            public MenuContinue    menuContinue;
            public MenuFontSetting menuFontSetting;
            public MenuNewGame     menuNewGame;
            public MenuOption      menuOption;
            public MenuUiSetting   menuUiSetting;

		    public bool isEqual(StartMenu data)
		    {
		        return menuContinue.isEqual(data.menuContinue) &&
		               menuFontSetting.isEqual(data.menuFontSetting) &&
		               menuNewGame.isEqual(data.menuNewGame) &&
		               menuOption.isEqual(data.menuOption) &&
		               menuUiSetting.isEqual(data.menuUiSetting);
		    }
        }

        [Serializable]
        public class MenuNewGame
        {
            public bool   enabled;
            public string value;

		    public bool isEqual(MenuNewGame data)
		    {
		        return enabled == data.enabled &&
		        	   value == data.value;
		    }
        }

        [Serializable]
        public class MenuContinue
        {
            public bool   enabled;
            public string value;

		    public bool isEqual(MenuContinue data)
		    {
		        return enabled == data.enabled && 
		        	   value == data.value;
		    }
        }

        [Serializable]
        public class MenuOption
        {
            public bool   enabled;
            public string value;

		    public bool isEqual(MenuOption data)
		    {
		        return enabled == data.enabled && 
		        	   value == data.value;
		    }
        }

        [Serializable]
        public class MenuFontSetting
        {
            public List<int> color;
            public string    font;
            public int       size;

            public bool isEqual(MenuFontSetting data) {
                if (color.Count != data.color.Count)
                    return false;

                for (int i = 0; i < color.Count; i++)
                    if (color[i] != data.color[i])
                        return false;

                if (font != data.font || 
                    size != data.size)
                    return false;

                return true;
            }
        }

        [Serializable]
        public class MenuUiSetting
        {
            public List<int> color;
            public string    frame;
            public List<int> position;
            public float     scale;
            public string    window;

            public bool isEqual(MenuUiSetting data) {
                if (frame != data.frame ||
                    scale != data.scale || 
                    window != data.window ||
                    position.Count != data.position.Count ||
                    color.Count != data.color.Count)
                    return false;

                for (int i = 0; i < color.Count; i++)
                    if (color[i] != data.color[i])
                        return false;

                for (int i = 0; i < position.Count; i++)
                    if (position[i] != data.position[i])
                        return false;

                return true;
            }
        }
    }
}