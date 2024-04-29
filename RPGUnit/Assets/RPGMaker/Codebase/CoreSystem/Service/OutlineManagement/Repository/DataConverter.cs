using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline;
using RPGMaker.Codebase.CoreSystem.Knowledge.JsonStructure;
using System.Collections.Generic;
using System.Linq;

namespace RPGMaker.Codebase.CoreSystem.Service.OutlineManagement.Repository
{
    public static class DataConverter
    {
        public static StartDataModel ConvertStartToObject(
            StartJson jsonStartJson
        ) {
            return new StartDataModel(
                jsonStartJson.startId,
                jsonStartJson.title,
                jsonStartJson.description,
                jsonStartJson.position[0],
                jsonStartJson.position[1]
            );
        }

        public static ChapterDataModel ConvertChapterToObject(
            ChapterJson jsonChapterJson,
            MapSubDataModel mapSubDataModel
        ) {
            return new ChapterDataModel(
                jsonChapterJson.chapterId,
                jsonChapterJson.title,
                jsonChapterJson.levelLow,
                jsonChapterJson.levelHigh,
                mapSubDataModel,
                jsonChapterJson.description,
                jsonChapterJson.position[0],
                jsonChapterJson.position[1]
            );
        }

        public static SectionDataModel ConvertSectionToObject(
            SectionJson jsonSectionJson,
            List<MapSubDataModel> maps,
            List<SwitchSubDataModel> belongingSwitches,
            List<SwitchSubDataModel> referringSwitches,
            List<string> relatedBySwitchSectionIds
        ) {
            return new SectionDataModel(
                jsonSectionJson.sectionId,
                jsonSectionJson.chapterId,
                jsonSectionJson.title,
                maps,
                belongingSwitches,
                referringSwitches,
                relatedBySwitchSectionIds,
                jsonSectionJson.memo,
                jsonSectionJson.position[0],
                jsonSectionJson.position[1]
            );
        }

        public static ConnectionDataModel ConvertConnectionToObject(ConnectionJson jsonConnectionJson) {
            return new ConnectionDataModel(
                jsonConnectionJson.id,
                jsonConnectionJson.lUuid,
                jsonConnectionJson.lPortDirection,
                jsonConnectionJson.lPortOrientation,
                jsonConnectionJson.rUuid,
                jsonConnectionJson.rPortDirection,
                jsonConnectionJson.rPortOrientation
            );
        }

        public static MapSubDataModel ConvertMapToObject(MapJson jsonMapJson, List<EventSubDataModel> events) {
            return new MapSubDataModel(
                jsonMapJson.mapId,
                jsonMapJson.name,
                events,
                jsonMapJson.encounter.Select(encounter => new Region(encounter.region)).ToList()
            );
        }

        public static EventSubDataModel ConvertEventToObject(EventMapJson jsonEventMapJson, string mapName = null) {
            return new EventSubDataModel(
                jsonEventMapJson.eventId,
                jsonEventMapJson.mapId,
                jsonEventMapJson.name,
                mapName
            );
        }

        public static SwitchSubDataModel ConvertSwitchToObject(
            SwitchJson jsonSwitchJson,
            List<EventSubDataModel> belongingEventEntities,
            List<EventSubDataModel> referringEventEntities
        ) {
            return new SwitchSubDataModel(
                jsonSwitchJson.switchId,
                jsonSwitchJson.name,
                belongingEventEntities,
                referringEventEntities
            );
        }

        public static StartJson ConvertStartToJson(StartDataModel startDataModel) {
            return new StartJson(
                startDataModel.ID,
                startDataModel.Name,
                startDataModel.Memo,
                0,
                0,
                new List<float> {startDataModel.PosX, startDataModel.PosY},
                0
            );
        }

        public static ChapterJson ConvertChapterToJson(ChapterDataModel chapterDataModel) {
            return new ChapterJson(
                chapterDataModel.ID,
                chapterDataModel.Name,
                chapterDataModel.SupposedLevelMin,
                chapterDataModel.SupposedLevelMax,
                chapterDataModel.Memo,
                null,
                chapterDataModel.FieldMapSubDataModel?.ID ?? "",
                0,
                0,
                new List<float> {chapterDataModel.PosX, chapterDataModel.PosY},
                0
            );
        }

        public static SectionJson ConvertSectionToJson(SectionDataModel sectionDataModel) {
            return new SectionJson(
                sectionDataModel.ID,
                sectionDataModel.ChapterID,
                sectionDataModel.Name,
                "",
                sectionDataModel.Memo,
                null,
                sectionDataModel.Maps.Select(map => map.ID).ToList(),
                new List<float> {sectionDataModel.PosX, sectionDataModel.PosY},
                null,
                null,
                0
            );
        }

        public static ConnectionJson ConvertConnectionToJson(ConnectionDataModel connectionDataModel) {
            return new ConnectionJson(
                connectionDataModel.ID,
                connectionDataModel.LUuid,
                connectionDataModel.LPortDirection,
                connectionDataModel.LPortOrientation,
                connectionDataModel.RUuid,
                connectionDataModel.RPortDirection,
                connectionDataModel.RPortOrientation
            );
        }
    }
}