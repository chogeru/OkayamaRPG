using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.CoreSystem.Knowledge.Misc;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository;
using RPGMaker.Codebase.CoreSystem.Service.RuntimeDataManagement.Repository;

namespace RPGMaker.Codebase.CoreSystem.Service.RuntimeDataManagement
{
    public class RuntimeDataManagementService
    {
        private readonly NewGameRepository _newGameRepository;
        private readonly TitleRepository   _titleRepository;

        public RuntimeDataManagementService() {
            _newGameRepository = new NewGameRepository();
            _titleRepository = new TitleRepository();
        }

        public RuntimeConfigDataModel LoadConfig() {
            return _newGameRepository.LoadConfig();
        }

        public RuntimeSaveDataModel LoadSaveData(int id) {
            return _newGameRepository.LoadData(id);
        }

        public RuntimeConfigDataModel NewConfig() {
            return _newGameRepository.NewConfig();
        }

        public void SaveConfig() {
            _newGameRepository.SaveConfig();
        }

        /// <summary>
        ///     ゲームデータのセーブの実施
        /// </summary>
        /// <param name="runtimeSaveDataModel">参照するセーブデータ</param>
        public void SaveSaveData(RuntimeSaveDataModel runtimeSaveDataModel, int id) {
            _newGameRepository.SaveData(runtimeSaveDataModel, id);
        }

        /// <summary>
        ///     ゲームデータのオートセーブの実施
        /// </summary>
        /// <param name="runtimeSaveDataModel">参照するセーブデータ</param>
        public void SaveAutoSaveData(RuntimeSaveDataModel runtimeSaveDataModel) {
            _newGameRepository.SaveData(runtimeSaveDataModel, 0);
        }

        public void StartNewGame(BattleSceneTransition battleTest = null) {
            _newGameRepository.CreateGame(battleTest);
        }

        public RuntimeSaveDataModel StartLoadGame() {
            return _newGameRepository.CreateLoadGame();
        }

        /// <summary>
        /// ActorData生成
        /// </summary>
        /// <param name="actorData"></param>
        /// <returns></returns>
        public RuntimeActorDataModel CreateActorData(BattleSceneTransition.Actor actorData) {
            return _newGameRepository.CreateActorData(actorData);
        }

        public int GetSaveFileCount() {
            return _newGameRepository.GetSaveFileCount();
        }

        public bool IsAutoSaveFile() {
            return _newGameRepository.IsAutoSaveFile();
        }

        public string LoadSaveData(string filename) {
            return _newGameRepository.LoadSaveFile(filename);
        }
    }
}