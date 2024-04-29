using RPGMaker.Codebase.Runtime.Common;

namespace RPGMaker.Codebase.Runtime.Map.Component.Character
{
    public class ActorOnMap : CharacterOnMap
    {
        public void ExecUpdateTimeHandler() {
            UpdateTimeHandler();
        }
    }
}