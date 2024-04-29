using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCustom;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Common.Component.Hud.Actor;

namespace RPGMaker.Codebase.Runtime.Event.Actor
{
    /// <summary>
    /// [アクター]-[スキルの増減]
    /// </summary>
    public class ActorChangeSkillProcessor : AbstractEventCommandProcessor
    {
        private ActorChangeSkill _actor;

        protected override void Process(string eventID, EventDataModel.EventCommand command) {
            if (_actor == null)
            {
                _actor = new ActorChangeSkill();
                _actor.Init(
                    DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels,
                    DataManager.Self().GetRuntimeSaveDataModel().variables,
                    DataManager.Self().GetActorDataModels()
                );
            }

            var skills = DataManager.Self().GetSkillCustomDataModels();
            SkillCustomDataModel skillsData = null;
            for (int i = 0; i < skills.Count; i++)
                if (skills[i].basic.id == command.parameters[3])
                {
                    skillsData = skills[i];
                    break;
                }
            _actor.ChangeSkill(skillsData, command);
            ProcessEndAction();
        }

        private void ProcessEndAction() {
            _actor = null;
            SendBackToLauncher.Invoke();
        }
    }
}