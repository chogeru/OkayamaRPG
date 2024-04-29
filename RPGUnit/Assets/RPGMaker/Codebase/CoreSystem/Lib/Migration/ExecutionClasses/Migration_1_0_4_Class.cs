using System.Collections.Generic;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement.Repository;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.CoreSystem.Knowledge.Misc;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;

namespace RPGMaker.Codebase.CoreSystem.Lib.Migration.ExecutionClasses
{
    /// <summary>
    /// Version1.0.4へのMigration用クラス
    /// </summary>
    internal class Migration_1_0_4_Class : IExecutionClassCore
    {
        public string GetIdentifier()
        {
            return "Migration104Class";
        }

        public void Execute()
        {
            EventWord();
            EventDirectionFix();
        }

        public void Rollback()
        {
        }

        public bool IsStorageUpdate()
        {
            return false;
        }

        public List<string> ListStorageCopy()
        {
            return null;
        }

        public List<string> ListStorageDelete()
        {
            return null;
        }

        void EventDirectionFix() {
            var eventManage = new EventMapRepository();
            var eventMapData = eventManage.Load();

            // ページ番号2の向きを修正
            for (int i = 0; i < eventMapData.Count; i++)
            {
                if (eventMapData[i].pages.Count > 1)
                {
                    if (eventMapData[i].pages[1].walk.direction == -1)
                        eventMapData[i].pages[1].walk.direction = 2;
                    if (eventMapData[i].pages[1].walk.directionFix == 2)
                        eventMapData[i].pages[1].walk.directionFix = 1;
                    eventManage.Save(eventMapData[i]);
                }
            }
        }

        /// <summary>
        /// 簡単イベント(宝箱)の文言変更
        /// </summary>
        private void EventWord() {
#if UNITY_EDITOR
            EventRepository eventRepository = new EventRepository();
            List<EventDataModel> DataModels = eventRepository.Load();

            for (int i = 0; i < DataModels.Count; i++)
            {
                var eventData = eventRepository.LoadEventById(DataModels[i].id);
                var flg = false;
                for (int j = 0; j < eventData.eventCommands.Count; j++)
                {
                    if (eventData.eventCommands[j].code == 401)
                    {
                        if (!eventData.eventCommands[j].parameters[0].EndsWith("\\G" + CoreSystemLocalize.LocalizeText("WORD_3104")) && 
                            eventData.eventCommands[j].parameters[0].EndsWith(CoreSystemLocalize.LocalizeText("WORD_3104")))
                        {
                            eventData.eventCommands[j].parameters[0] = eventData.eventCommands[j].parameters[0].Replace("G" + CoreSystemLocalize.LocalizeText("WORD_3104"), "\\G " + CoreSystemLocalize.LocalizeText("WORD_3104"));
                            flg = true;
                        }
                    }
                }

                if (flg)
                {
                    eventRepository.Save(eventData);
                }
            }
#endif
        }
    }
}