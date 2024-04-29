using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[FilePath("RMUPreferences.asset", FilePathAttribute.Location.PreferencesFolder)]
public class RMUPreferences : ScriptableSingleton<RMUPreferences>
{
    private const string lastNoticeToppageUrl = "https://notice.rpgmakerofficial.com/toppage/";

    [SerializeField]
    public bool displayNotifs;
    [SerializeField]
    public string lastNoticeDate;
    [SerializeField]
    public string noticeToppageUrl;

    private RMUPreferences() {
        if (instance.noticeToppageUrl == null || instance.noticeToppageUrl.Length == 0)
        {
            instance.displayNotifs = true;
            instance.noticeToppageUrl = lastNoticeToppageUrl;
        }
    }

    public void Save() {
        Save(true);
    }
}
