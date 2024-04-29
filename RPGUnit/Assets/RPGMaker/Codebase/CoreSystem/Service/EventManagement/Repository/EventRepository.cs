using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.JsonStructure;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace RPGMaker.Codebase.CoreSystem.Service.EventManagement.Repository
{
    public class EventRepository
    {
        private const string JsonPath    = "Assets/RPGMaker/Storage/Event/JSON/Event/";
        private static List<EventDataModel> _eventDataModels;

        private string SavePath(EventDataModel eventDataModel) {
            return JsonPath + eventDataModel.id + "-" + eventDataModel.page + ".json";
        }

        private string LoadPath(string eventId, int page) {
            return JsonPath + eventId + "-" + page + ".json";
        }

        private string DeletePath(string eventId, int page) {
            return JsonPath + eventId + "-" + page + ".meta";
        }

        public void Save(EventDataModel eventDataModel) {
#if UNITY_EDITOR
            var eventLists = Load();

            var edited = false;
            for (var index = 0; index < eventLists.Count; index++)
            {
                if (eventLists[index].id != eventDataModel.id) continue;
                if (eventLists[index].page != eventDataModel.page) continue;

                eventLists[index] = eventDataModel;
                edited = true;
                break;
            }

            if (!edited) eventLists.Add(eventDataModel);

            File.WriteAllText(SavePath(eventDataModel), JsonHelper.ToJson(eventDataModel));

            _eventDataModels = eventLists;
#else
            //UnityEngine.Debug.LogError("Dont call");
#endif
        }

        public List<EventDataModel> Load() {
#if UNITY_EDITOR
            if (_eventDataModels != null) return _eventDataModels;

            _eventDataModels = GetJsons().Select(ConvertJsonToEntity).ToList();
            return _eventDataModels;
#else
            //UnityEngine.Debug.LogError("Dont call");
            return null;
#endif
        }

        public EventDataModel LoadEventById(string eventId, int page = 0) {
#if UNITY_EDITOR
            return ConvertJsonToEntity(JsonHelper.FromJson<EventJson>(
                UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(LoadPath(eventId, page))));
#else
            return ScriptableObjectOperator.GetClass<EventDataModel>(LoadPath(eventId, page)) as EventDataModel;
#endif
        }

#if UNITY_EDITOR
        public void Clear() {
            _eventDataModels = null;
        }
#endif

        public void Delete(EventDataModel eventDataModel) {
            File.Delete(LoadPath(eventDataModel.id, eventDataModel.page));
            File.Delete(DeletePath(eventDataModel.id, eventDataModel.page));
            UnityEditorWrapper.AssetDatabaseWrapper.Refresh2();

            if (_eventDataModels != null)
                _eventDataModels.Remove(eventDataModel);
        }

        public void PageStuffing(EventDataModel eventDataModel) {
            eventDataModel.page--;
            File.Move(LoadPath(eventDataModel.id, eventDataModel.page + 1),
                LoadPath(eventDataModel.id, eventDataModel.page));
            if (_eventDataModels != null)
                _eventDataModels.Remove(eventDataModel);
            Save(eventDataModel);
        }

#if UNITY_EDITOR
        private List<EventJson> GetJsons() {
            var eventJsons = new List<EventJson>();

            if (Directory.Exists(JsonPath) == false)
                Directory.CreateDirectory(JsonPath);

            var files = Directory.GetFiles(JsonPath, "*.json", SearchOption.TopDirectoryOnly);

            foreach (var file in files)
            {
                eventJsons.Add(
                    JsonHelper.FromJson<EventJson>(UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(file)));
            }

            return eventJsons;
        }
#endif

        private EventDataModel ConvertJsonToEntity(EventJson json) {
            return new EventDataModel(
                json.id,
                json.page,
                json.type,
                json.eventCommands == null
                    ? null
                    : json.eventCommands.Select(eventCommandJson =>
                        new EventDataModel.EventCommand(
                            eventCommandJson.code,
                            eventCommandJson.parameters.ToList(),
                            eventCommandJson.route == null
                                ? null
                                : eventCommandJson.route.Select(eventCommandRoute =>
                                    new EventDataModel.EventCommandMoveRoute(eventCommandRoute.code,
                                        eventCommandRoute.parameters.ToList(), eventCommandRoute.codeIndex)).ToList()
                        )).ToList()
            );
        }
    }
}