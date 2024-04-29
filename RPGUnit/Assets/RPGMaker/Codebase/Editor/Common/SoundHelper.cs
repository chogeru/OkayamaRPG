using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.Editor.Common.Enum;
using System.Collections.Generic;
using System.IO;

namespace RPGMaker.Codebase.Editor.Common
{
    /// <summary>
    /// サウンド関連の処理
    /// </summary>
    public static class SoundHelper
    {
        /// <summary>
        /// 拡張子が保存されていないデータ用に、拡張子をつけてサウンドファイル名を返却する
        /// </summary>
        /// <param name="soundType"></param>
        /// <param name="soundFileName"></param>
        /// <returns></returns>
        public static string InitializeFileName(List<SoundType> soundTypes, string soundFileName, bool isPath) {
            DirectoryInfo dir;
            string dirName;
            for (int i = 0; i < soundTypes.Count; i++)
            {
                if (soundTypes[i] == SoundType.Bgm)
                    dirName = PathManager.SOUND_BGM;
                else if (soundTypes[i] == SoundType.Bgs)
                    dirName = PathManager.SOUND_BGS;
                else if (soundTypes[i] == SoundType.Me)
                    dirName = PathManager.SOUND_ME;
                else if (soundTypes[i] == SoundType.Se)
                    dirName = PathManager.SOUND_SE;
                else
                    return "";

                dir = new DirectoryInfo(dirName);

                var info = dir.GetFiles("*.ogg");
                foreach (var f in info)
                {
                    if (soundFileName.EndsWith(".ogg"))
                    {
                        if (f.Name == soundFileName)
                        {
                            if (!isPath)
                                return soundFileName;
                            else
                                return dirName + soundFileName;
                        }
                    }
                    else
                    {
                        if (f.Name.Replace(".ogg", "") == soundFileName)
                        {
                            if (!isPath)
                                return soundFileName + ".ogg";
                            else
                                return dirName + soundFileName + ".ogg";
                        }
                    }
                }

                info = dir.GetFiles("*.wav");
                foreach (var f in info)
                {
                    if (soundFileName.EndsWith(".wav"))
                    {
                        if (f.Name == soundFileName)
                        {
                            if (!isPath)
                                return soundFileName;
                            else
                                return dirName + soundFileName;
                        }
                    }
                    else
                    {
                        if (f.Name.Replace(".wav", "") == soundFileName)
                        {
                            if (!isPath)
                                return soundFileName + ".wav";
                            else
                                return dirName + soundFileName + ".wav";
                        }
                    }
                }
            }

            return "";
        }
        /// <summary>
        /// 表示用に、拡張子の無い文字を返却する
        /// </summary>
        /// <param name="soundType"></param>
        /// <param name="soundFileName"></param>
        /// <returns></returns>
        public static string RemoveExtention(string soundFileName) {
            return soundFileName.Replace(".ogg", "").Replace(".wav","");
        }
    }
}