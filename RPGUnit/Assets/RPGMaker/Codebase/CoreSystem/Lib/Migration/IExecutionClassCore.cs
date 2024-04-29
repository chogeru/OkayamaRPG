using System.Collections.Generic;

namespace RPGMaker.Codebase.CoreSystem.Lib.Migration
{
    internal interface IExecutionClassCore
    {
        // 当該マイグレーション処理を識別するための文字列を返す
        // 履歴と照合する際のキーとなるので、必ず一意となること
        // また、一度決めたら変更しないこと
        public string GetIdentifier();

        public void Execute();

        public void Rollback();

        public bool IsStorageUpdate();

        public List<string> ListStorageCopy();

        public List<string> ListStorageDelete();
    }
}