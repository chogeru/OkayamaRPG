using RPGMaker.Codebase.Runtime.Common;
using System.Collections.Generic;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Map.Menu
{
    public class SkillCommand : MonoBehaviour
    {
        private int _actorCount;
        private List<string> _actorIds = new List<string>();
        private SkillMenu _skillMenu;

        public void Init(SkillMenu skillMenu) {
            _skillMenu = skillMenu;
            _actorIds = DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.actors;
        }
        
        /// <summary>
        ///     ボタンのイベント入力
        /// </summary>
        /// <param name="obj"></param>
        public void ButtonEvent(GameObject obj) {
            for (var i = 0; i < _actorIds.Count; i++)
                if (_actorIds[i] == _skillMenu.ActorId())
                {
                    _actorCount = i;
                    break;
                }
            switch (obj.name)
            {
                // キャラ切り替え
                case "CharacterChangeRight":
                    _actorCount++;
                    if (_actorCount > _actorIds.Count - 1)
                    {
                        _actorCount = 0;
                    }

                    _skillMenu.Init(_skillMenu.MenuBase, _actorIds[_actorCount]);
                    break;
                case "CharacterChangeLeft":
                    _actorCount--;
                    if (_actorCount < 0)
                    {
                        _actorCount = _actorIds.Count - 1;
                    }

                    _skillMenu.Init(_skillMenu.MenuBase, _actorIds[_actorCount]);
                    break;
            }
        }

        /// <summary>
        ///     ボタンのイベント入力
        /// </summary>
        /// <param name="obj"></param>
        public void CharacterChange(bool isNext) {
            for (var i = 0; i < _actorIds.Count; i++)
                if (_actorIds[i] == _skillMenu.ActorId())
                {
                    _actorCount = i;
                    break;
                }
            if (isNext)
            {
                _actorCount++;
                if (_actorCount > _actorIds.Count - 1)
                {
                    _actorCount = 0;
                }

                _skillMenu.Init(_skillMenu.MenuBase, _actorIds[_actorCount]);
            }
            else
            {
                _actorCount--;
                if (_actorCount < 0)
                {
                    _actorCount = _actorIds.Count - 1;
                }

                _skillMenu.Init(_skillMenu.MenuBase, _actorIds[_actorCount]);
            }
        }
    }
}