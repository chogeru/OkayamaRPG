using RPGMaker.Codebase.CoreSystem.Helper;
using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Outline
{
    public class OutlineDataModel
    {
        // チャプターコードとセクションコードは、インデックス値+1とすることになった。
        private const int CodeOffset = 1;

        public OutlineDataModel(
            List<StartDataModel> starts,
            List<ChapterDataModel> chapters,
            List<SectionDataModel> sections,
            List<ConnectionDataModel> connections
        ) {
            Starts = starts;
            Chapters = chapters;
            Sections = sections;
            Connections = connections;
        }

        public List<StartDataModel> Starts { get; }
        public List<ChapterDataModel> Chapters { get; }
        public List<SectionDataModel> Sections { get; }
        public List<ConnectionDataModel> Connections { get; }

        public StartDataModel GetStartDataModel(string id) {
            return Starts.ForceSingleOrDefault(startDataModel => startDataModel.ID == id);
        }

        public ChapterDataModel GetChapterDataModel(string id) {
            return Chapters.ForceSingleOrDefault(chapterDataModel => chapterDataModel.ID == id);
        }

        public SectionDataModel GetSectionDataModel(string id) {
            return Sections.ForceSingleOrDefault(sectionDataModel => sectionDataModel.ID == id);
        }

        public ConnectionDataModel GetConnectionDataModel(string id) {
            return Connections.ForceSingleOrDefault(connectionDataModel => connectionDataModel.ID == id);
        }

        public ChapterDataModel GetChapterDataModel(int chapterCode) {
            return Chapters[chapterCode - CodeOffset];
        }

        public SectionDataModel GetSectionDataModel(int sectionCode) {
            return Sections[sectionCode - CodeOffset];
        }

        public int GetChapterCode(ChapterDataModel chapterDataModel) {
            return Chapters.IndexOf(chapterDataModel) + CodeOffset;
        }

        public int GetSectionCode(SectionDataModel sectionDataModel) {
            return Sections.IndexOf(sectionDataModel) + CodeOffset;
        }
    }
}