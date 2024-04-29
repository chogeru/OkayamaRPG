using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventBattle;
using System.Collections.Generic;
using System.IO;

namespace RPGMaker.Codebase.CoreSystem.Service.EventManagement.Repository
{
    public class EventBattleRepository
    {
        private const string JsonPath = "Assets/RPGMaker/Storage/Event/JSON/eventBattle.json";

        private static List<EventBattleDataModel> _eventBattleDataModels;

        public void Save(List<EventBattleDataModel> eventBattleDataModels) {
            File.WriteAllText(JsonPath, JsonHelper.ToJsonArray(eventBattleDataModels));
            _eventBattleDataModels = eventBattleDataModels;
        }

        public List<EventBattleDataModel> Load() {
            if (_eventBattleDataModels != null) return _eventBattleDataModels;
#if UNITY_EDITOR
            var jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JsonPath);
            _eventBattleDataModels = JsonHelper.FromJsonArray<EventBattleDataModel>(jsonString);
#else
            _eventBattleDataModels =
 ScriptableObjectOperator.GetClass<EventBattleDataModel>(JsonPath) as List<EventBattleDataModel>;
#endif
            return _eventBattleDataModels;
        }

        public void Reset() {
            _eventBattleDataModels = null;
        }
    }
}