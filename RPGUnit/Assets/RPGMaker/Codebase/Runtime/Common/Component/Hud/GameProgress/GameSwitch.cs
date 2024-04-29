namespace RPGMaker.Codebase.Runtime.Common.Component.Hud.GameProgress
{
    public class GameSwitch
    {
        public void SetGameSwitch(bool isMultiple, int selectIndex, int max, bool toggle) {
            var runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();
            if (!isMultiple)
                runtimeSaveDataModel.switches.data[selectIndex] = toggle;
            else
                for (var i = selectIndex; i <= max; i++)
                    runtimeSaveDataModel.switches.data[i] = toggle;
        }
    }
}