using System;

namespace RPGMaker.Codebase.CoreSystem.Lib.Migration
{
    [Serializable]
    public class MigrationHistoryDataModelCore
    {
        public string executedAt;
        public string executionClassIdentifier;
        public string id;
    }
}