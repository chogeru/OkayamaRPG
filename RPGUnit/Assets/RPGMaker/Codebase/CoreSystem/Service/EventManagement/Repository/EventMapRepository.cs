using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.EventMap;
using System.Collections.Generic;
using System.IO;

namespace RPGMaker.Codebase.CoreSystem.Service.EventManagement.Repository
{
    public class EventMapRepository
    {
        private const string JsonFile = "Assets/RPGMaker/Storage/Event/JSON/eventMap.json";
        private const string SO_PATH  = "Assets/RPGMaker/Storage/Event/SO/eventMap.asset";
        private static List<EventMapDataModel> _eventMapDataModels;

        public List<EventMapDataModel> Load() {
            if (_eventMapDataModels != null) return _eventMapDataModels;
#if UNITY_EDITOR
            var jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JsonFile);
            _eventMapDataModels = JsonHelper.FromJsonArray<EventMapDataModel>(jsonString);
#else
            _eventMapDataModels = ScriptableObjectOperator.GetClass<EventMapDataModel>(JsonFile) as List<EventMapDataModel>;
#endif

            //ID重複のデータが存在した場合には、後勝ちとする
            var removedList = new List<int>();
            var dic = new Dictionary<KeyValuePair<string, string>, int>();
            for (int i = 0; i < _eventMapDataModels.Count; i++)
            {
                var key = new KeyValuePair<string, string>(_eventMapDataModels[i].mapId, _eventMapDataModels[i].eventId);
                if (dic.ContainsKey(key))
                {
                    removedList.Add(dic[key]);
                    dic.Remove(key);
                }
                dic.Add(key, i);
            }
            for (int i = removedList.Count - 1; i >= 0; i--)
            {
                _eventMapDataModels.RemoveAt(removedList[i]);
            }


            SetSerialNumbers();

            return _eventMapDataModels;
        }

        public void Save() {
            File.WriteAllText(JsonFile, JsonHelper.ToJsonArray(_eventMapDataModels));

            SetSerialNumbers();
        }

        public void Save(EventMapDataModel eventMapDataModel) {
            var eventMapLists = Load();

            var edited = false;
            for (var index = 0; index < eventMapLists.Count; index++)
            {
                if (eventMapLists[index].mapId != eventMapDataModel.mapId) continue;
                if (eventMapLists[index].eventId != eventMapDataModel.eventId) continue;

                eventMapLists[index] = eventMapDataModel;
                edited = true;
                break;
            }

            if (!edited) eventMapLists.Add(eventMapDataModel);

            File.WriteAllText(JsonFile, JsonHelper.ToJsonArray(eventMapLists));

            _eventMapDataModels = eventMapLists;

            SetSerialNumbers();
        }

        /**
         * マップに紐づくイベントを取得する
         */
        public List<EventMapDataModel> LoadEventMapEntitiesByMapId(string mapId) {
            Load();

            return _eventMapDataModels.FindAll(eventEntity => eventEntity.mapId == mapId);
        }

        public List<EventMapDataModel> LoadEventMapEntitiesByMapIdFromJson(string mapId) {
#if UNITY_EDITOR
            var jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JsonFile);
            var eventMapDataModels = JsonHelper.FromJsonArray<EventMapDataModel>(jsonString);
#else
            var eventMapDataModels = ScriptableObjectOperator.GetClass<EventMapDataModel>(JsonFile) as List<EventMapDataModel>;
#endif

            return eventMapDataModels.FindAll(eventMapDataModel => eventMapDataModel.mapId == mapId);
        }

        public void Delete(EventMapDataModel eventMapDataModel) {
            var eventMapLists = Load();

            eventMapLists.RemoveAll(eventMap =>
                eventMap.mapId == eventMapDataModel.mapId && eventMap.eventId == eventMapDataModel.eventId);

            File.WriteAllText(JsonFile, JsonHelper.ToJsonArray(eventMapLists));

            _eventMapDataModels = eventMapLists;

            SetSerialNumbers();
        }

        private void SetSerialNumbers() {
            // serial numberフィールドがあるモデルには連番を設定する
            for (var i = 0; i < _eventMapDataModels.Count; i++) _eventMapDataModels[i].SerialNumber = i + 1;
        }
    }
}