using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline;
using RPGMaker.Codebase.CoreSystem.Knowledge.JsonStructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RPGMaker.Codebase.CoreSystem.Service.OutlineManagement.Repository
{
    public class OutlineRepository
    {
        private const string JsonFileOfStart      = "Assets/RPGMaker/Storage/Outline/start.json";
        private const string JsonFileOfChapter    = "Assets/RPGMaker/Storage/Outline/chapter.json";
        private const string JsonFileOfSection    = "Assets/RPGMaker/Storage/Outline/section.json";
        private const string JsonFileOfConnection = "Assets/RPGMaker/Storage/Outline/connection.json";
        private const string JsonFileOfMap        = "Assets/RPGMaker/Storage/Map/JSON/Map";
        private const string JsonFileOfEventMap   = "Assets/RPGMaker/Storage/Event/JSON/eventMap.json";
        private const string JsonFileOfFlags      = "Assets/RPGMaker/Storage/Flags/JSON/flags.json";
        private List<StartJson>      _startsJson;
        private List<ChapterJson>    _chaptersJson;
        private List<SectionJson>    _sectionsJson;
        private List<ConnectionJson> _connectionsJson;
        private List<MapJson>        _mapsJson;
        private List<EventMapJson>   _eventMapsJson;
        private FlagsJson            _flagsJson;

        /**
         * アウトラインデータをJSONから読み出す.
         */
        public OutlineDataModel GetOutline() {
            // 各種JSONデータ読み出し
            _startsJson = JsonHelper.FromJsonArray<StartJson>(
                UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JsonFileOfStart)
            );
            _chaptersJson = JsonHelper.FromJsonArray<ChapterJson>(
                UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JsonFileOfChapter)
            );
            _sectionsJson = JsonHelper.FromJsonArray<SectionJson>(
                UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JsonFileOfSection)
            );
            _connectionsJson = JsonHelper.FromJsonArray<ConnectionJson>(
                UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JsonFileOfConnection)
            );

            _mapsJson = Directory.GetFiles(JsonFileOfMap, "*.json", SearchOption.TopDirectoryOnly)
                .Select(item=>{ return JsonHelper.FromJson<MapJson>(UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(item)); }).ToList();

            _eventMapsJson = JsonHelper.FromJsonArray<EventMapJson>(
                UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JsonFileOfEventMap)
            );
            _flagsJson = JsonHelper.FromJson<FlagsJson>(
                UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JsonFileOfFlags)
            );

            // JSONをEntityにコンバート
            var eventEntities = _eventMapsJson
                .Select(eventMapJson =>
                    DataConverter.ConvertEventToObject(
                        eventMapJson))
                .ToList();
            var mapEntities = _mapsJson
                .Select(mapJson =>
                {
                    List<EventSubDataModel> events = new List<EventSubDataModel>();
                    for (int i = 0; i < eventEntities.Count; i++)
                        if (eventEntities[i].MapId == mapJson.mapId)
                            events.Add(eventEntities[i]);
                    
                    return DataConverter.ConvertMapToObject(
                        mapJson,
                        events);
                })
                .ToList();
            var sectionEntities = _sectionsJson
                .Select(sectionJson =>
                {
                    List<MapSubDataModel> maps = new List<MapSubDataModel>();
                    for (int i = 0; i < mapEntities.Count; i++)
                        if (sectionJson.mapIds.Contains(mapEntities[i].ID))
                            maps.Add(mapEntities[i]);

                    return DataConverter.ConvertSectionToObject(
                        sectionJson,
                        maps,
                        GetBelongingSwitchesBySectionJson(sectionJson),
                        GetReferringSwitchesBySectionJson(sectionJson),
                        GetRelatedBySwitchSectionIdsBySectionJson(sectionJson)
                    );
                })
                .ToList();
            var chapterEntities = _chaptersJson
                .Select(chapterJson =>
                {
                    MapSubDataModel map = null;
                    for (int i = 0; i < mapEntities.Count; i++)
                        if (mapEntities[i].ID == chapterJson.mapId)
                        {
                            map = mapEntities[i];
                            break;
                        }

                    return DataConverter.ConvertChapterToObject(
                        chapterJson,
                        map);
                })
                .ToList();
            var startEntities = _startsJson
                .Select(startJson =>
                {
                    return DataConverter.ConvertStartToObject(startJson);
                })
                .ToList();
            var connectionEntities = _connectionsJson
                .Select(DataConverter.ConvertConnectionToObject)
                .ToList();

            var ret = new OutlineDataModel(startEntities, chapterEntities, sectionEntities, connectionEntities);

            return SetSerialNumbers(ret);
        }

        /**
         * アウトラインデータをJSONに保存する.
         * （保存対象はChapterとSectionのみ(Startを追加)）
         */
        public void StoreOutline(OutlineDataModel outlineDataModel) {
            outlineDataModel = SetSerialNumbers(outlineDataModel);

            var startJson =
                outlineDataModel.Starts.Select(DataConverter
                    .ConvertStartToJson);
            var chaptersJson =
                outlineDataModel.Chapters.Select(DataConverter
                    .ConvertChapterToJson);
            var sectionsJson =
                outlineDataModel.Sections.Select(DataConverter
                    .ConvertSectionToJson);
            var connectionsJson =
                outlineDataModel.Connections.Select(DataConverter
                    .ConvertConnectionToJson);

            File.WriteAllText(JsonFileOfStart, JsonHelper.ToJsonArray(startJson));
            File.WriteAllText(JsonFileOfChapter, JsonHelper.ToJsonArray(chaptersJson));
            File.WriteAllText(JsonFileOfSection, JsonHelper.ToJsonArray(sectionsJson));
            File.WriteAllText(JsonFileOfConnection, JsonHelper.ToJsonArray(connectionsJson));
        }

        /**
         * セクションデータをjsonに変換
         */
        public SectionJson SectionToJson(SectionDataModel sectionData) {
            return
                DataConverter
                    .ConvertSectionToJson(sectionData);
        }

        /**
         * jsonデータをセクションデータに変換
         */
        public SectionDataModel JsonToSection(SectionJson sectionJson) {
            // JSONをEntityにコンバート
            var eventEntities = _eventMapsJson
                .Select(eventMapJson =>
                    DataConverter.ConvertEventToObject(
                        eventMapJson))
                .ToList();

            var mapEntities = _mapsJson
                .Select(mapJson =>
                {
                    return DataConverter.ConvertMapToObject(
                        mapJson,
                        eventEntities.FindAll(ev => ev.MapId == mapJson.mapId));
                })
                .ToList();

            return DataConverter.ConvertSectionToObject(
                        sectionJson,
                        mapEntities.FindAll(map => sectionJson.mapIds.Contains(map.ID)),
                        GetBelongingSwitchesBySectionJson(sectionJson),
                        GetReferringSwitchesBySectionJson(sectionJson),
                        GetRelatedBySwitchSectionIdsBySectionJson(sectionJson)
                    );
       }

        /**
         * 当該セクションに「属している（セクション内でON/OFFする）」スイッチ一覧を取得する.
         */
        private List<SwitchSubDataModel> GetBelongingSwitchesBySectionJson(SectionJson sectionJson) {
            // まずセクションに属するマップ内に配置されたイベントIDを全取得する
            var eventIds = _eventMapsJson
                .FindAll(eventMapJson => sectionJson.mapIds.Contains(eventMapJson.mapId))
                .Select(eventMapJson => eventMapJson.eventId);

            // flags.jsonから、belongingEventsにeventIdsを含むスイッチを抽出する
            var switchJsons = _flagsJson.switches.Where(switchJson =>
                    switchJson.belongingEvents.Exists(belongingEvent => eventIds.Contains(belongingEvent.eventId)))
                .ToList();

            return GetSwitchEntitiesBySwitchJsons(switchJsons);
        }

        /**
         * 当該セクション内のイベント等に対する「条件となっている」スイッチ一覧を取得する.
         */
        private List<SwitchSubDataModel> GetReferringSwitchesBySectionJson(SectionJson sectionJson) {
            // まずセクションに属するマップ内に配置されたイベントIDを全取得する
            var eventIds = _eventMapsJson
                .FindAll(eventMapJson => sectionJson.mapIds.Contains(eventMapJson.mapId))
                .Select(eventMapJson => eventMapJson.eventId);

            // flags.jsonから、referringEventsにeventIdsを含むスイッチを抽出する
            var switchJsons = _flagsJson.switches.Where(switchJson =>
                    switchJson.referringEvents.Exists(belongingEvent => eventIds.Contains(belongingEvent.eventId)))
                .ToList();

            return GetSwitchEntitiesBySwitchJsons(switchJsons);
        }

        /**
         * スイッチごとにイベントデータを紐づけて（Entity化して）返す.
         */
        private List<SwitchSubDataModel> GetSwitchEntitiesBySwitchJsons(IEnumerable<SwitchJson> switchJsons) {
            return switchJsons.Select(switchJson =>
            {
                var belongingEvents = switchJson.belongingEvents.Select(belongingFlagEvent =>
                {
                    var mapName = _mapsJson
                        .FirstOrDefault(mapJson => mapJson.mapId == belongingFlagEvent.mapId)?
                        .displayName;

                    EventMapJson eventMapJson = null;
                    for (int i = 0; i < _eventMapsJson.Count; i++)
                        if (_eventMapsJson[i].eventId == belongingFlagEvent.eventId)
                        {
                            eventMapJson = _eventMapsJson[i];
                            break;
                        }

                    return DataConverter.ConvertEventToObject(
                        eventMapJson, mapName);
                }).ToList();

                var referringEvents = switchJson.referringEvents.Select(referringFlagEvent =>
                {
                    var mapName = _mapsJson
                        .FirstOrDefault(mapJson => mapJson.mapId == referringFlagEvent.mapId)?
                        .displayName;

                    EventMapJson eventMapJson = null;
                    for (int i = 0; i < _eventMapsJson.Count; i++)
                        if (_eventMapsJson[i].eventId == referringFlagEvent.eventId)
                        {
                            eventMapJson = _eventMapsJson[i];
                            break;
                        }

                    return DataConverter.ConvertEventToObject(
                        eventMapJson, mapName);
                }).ToList();

                return DataConverter.ConvertSwitchToObject(
                    switchJson, belongingEvents, referringEvents);
            }).ToList();
        }

        private List<string> GetRelatedBySwitchSectionIdsBySectionJson(SectionJson sectionJson) {
            var belongingSwitches = GetBelongingSwitchesBySectionJson(sectionJson);

            var ret = new List<string>();

            foreach (var belongingSwitch in belongingSwitches)
            {
                foreach (var referringEvent in belongingSwitch.ReferringEventEntities)
                {
                    MapJson map = null;
                    for (int i = 0; i < _mapsJson.Count; i++)
                        if (_mapsJson[i].mapId == referringEvent.MapId)
                        {
                            map = _mapsJson[i];
                            break;
                        }
                    if (map == null) continue;

                    List<SectionJson> targetSections = new List<SectionJson>();
                    for (int i = 0; i < _sectionsJson.Count; i++)
                        if (_sectionsJson[i].mapIds.Contains(map.mapId))
                            targetSections.Add(_sectionsJson[i]);

                    ret.AddRange(targetSections.Select(sectionJson3 => sectionJson3.sectionId));
                }
            }

            return ret;
        }
        
        private OutlineDataModel SetSerialNumbers(OutlineDataModel outlineDataModel) {
            // serial numberフィールドがあるモデルには連番を設定する
            for (var i = 0; i < outlineDataModel.Chapters.Count; i++)
            {
                outlineDataModel.Chapters[i].SerialNumber = i + 1;
            }
            for (var i = 0; i < outlineDataModel.Sections.Count; i++)
            {
                outlineDataModel.Sections[i].SerialNumber = i + 1;
            }

            return outlineDataModel;
        }
    }
}