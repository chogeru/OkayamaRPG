using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Map;
using RPGMaker.Codebase.Runtime.Map.Item;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Common
{
    public class PartyWindow : WindowBase
    {
        public enum PartyType
        {
            Item,
            Skill
        }

        private List<CharacterItem> _characterItem;
        private GameAction _gameAction;
        private GameItem _gameItem;
        private PartyType _partyType = PartyType.Item;
        private string _useActorId = "";
        private string _useId      = "";
        Action _callback;
        private int _pattern;
        private bool isAll = false;

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="type"></param>
        /// <param name="useActorId"></param>
        /// <param name="useId"></param>
        /// <param name="gameItem"></param>
        /// <param name="callback"></param>
        public void Init(PartyType type, string useActorId, string useId, GameItem gameItem, Action callback) {
            _callback = callback;
            _partyType = type;
            _useActorId = useActorId;
            _useId = useId;
            _gameItem = gameItem;

            SystemSettingDataModel systemSettingDataModel = DataManager.Self().GetSystemDataModel();
            _pattern = int.Parse(systemSettingDataModel.uiPatternId) + 1;
            if (_pattern < 1 || _pattern > 6)
                _pattern = 1;

            var partyItems = transform.Find("MenuArea/PartyWindow/PartyItems").gameObject;
            var party = DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel;
            var actors = DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels;
            _characterItem = new List<CharacterItem>();

            //パーティ部分
            for (var i = 0; i < 4; i++)
            {
                if (i < party.actors.Count)
                {
                    for (var j = 0; j < actors.Count; j++)
                    {
                        if (actors[j].actorId == party.actors[i])
                        {
                            var characterItem = partyItems.transform.Find("Actor" + (i + 1)).gameObject
                                .GetComponent<CharacterItem>();
                            if (characterItem == null)
                            {
                                characterItem = partyItems.transform.Find("Actor" + (i + 1)).gameObject
                                    .AddComponent<CharacterItem>();
                            }
                            characterItem.Init(actors[j]);
                            _characterItem.Add(characterItem);
                            partyItems.transform.Find("Actor" + (i + 1)).gameObject.SetActive(true);
                            break;
                        }
                    }
                }
                else
                {
                    partyItems.transform.Find("Actor" + (i + 1)).gameObject.SetActive(false);
                }
            }
            

            //決定音やブザー音は、共通部品では鳴動しない
            foreach (var t in _characterItem)
            {
                t.GetComponent<WindowButtonBase>().SetSilentClick(true);
            }

            Init();
            SetFocusAft();
        }

        /// <summary>
        /// フォーカス設定
        /// </summary>
        private async void SetFocusAft(int index = 0) {
            await Task.Delay(10);
            isAll = false;
            //味方全員対象の場合
            if (_gameItem.Scope == 8 || _gameItem.Scope == 10 || _gameItem.Scope == 13 || _gameItem.Scope == 50)
            {
                isAll = true;
                foreach (var t in _characterItem)
                {
                    t.GetComponent<WindowButtonBase>().SetAlreadyHighlight(true);
                    t.GetComponent<WindowButtonBase>().SetAnimation(true);
                    t.GetComponent<WindowButtonBase>().SetDefaultClick();
                }

                for (var i = 0; i < _characterItem.Count; i++)
                {
                    var nav = _characterItem[i].GetComponent<Button>().navigation;
                    nav.mode = Navigation.Mode.None;
                    _characterItem[i].GetComponent<Button>().navigation = nav;
                }
                //先頭にフォーカスをあてる
                _characterItem[index].GetComponent<WindowButtonBase>().SetEnabled(true);
                _characterItem[index].GetComponent<Button>().Select();
            }
            else
            {
                foreach (var t in _characterItem)
                {
                    t.GetComponent<WindowButtonBase>().SetAlreadyHighlight(false);
                    t.GetComponent<WindowButtonBase>().SetAnimation(true);
                }

                //対象が「使用者への影響」にしか存在しない場合は、フォーカス移動を行わせない
                bool notFocusChange = false;
                if (_partyType == PartyType.Skill)
                    if (_gameItem.Scope == 0 && _gameItem.UserScope == 1)
                        notFocusChange = true;

                //十字キーでの操作登録
                for (var i = 0; i < _characterItem.Count; i++)
                {
                    var nav = _characterItem[i].GetComponent<Button>().navigation;
                    nav.mode = Navigation.Mode.Explicit;

                    //UIパターンに応じて十字キーを変更する
                    if (_pattern == 1 || _pattern == 2 || _pattern == 3 || _pattern == 4)
                    {
                        if (!notFocusChange)
                        {
                            nav.selectOnUp = _characterItem[i == 0 ? _characterItem.Count - 1 : i - 1].GetComponent<Button>();
                            nav.selectOnDown = _characterItem[(i + 1) % _characterItem.Count].GetComponent<Button>();
                        }
                        else
                        {
                            nav.selectOnUp = null;
                            nav.selectOnDown = null;
                        }
                    }
                    else
                    {
                        if (!notFocusChange)
                        {
                            nav.selectOnLeft = _characterItem[i == 0 ? _characterItem.Count - 1 : i - 1].GetComponent<Button>();
                            nav.selectOnRight = _characterItem[(i + 1) % _characterItem.Count].GetComponent<Button>();
                        }
                        else
                        {
                            nav.selectOnLeft = null;
                            nav.selectOnRight = null;
                        }
                    }

                    _characterItem[i].GetComponent<Button>().navigation = nav;
                    _characterItem[i].GetComponent<Button>().targetGraphic = _characterItem[i].GetComponent<Button>().transform.Find("Highlight").GetComponent<Image>();
                }
                //先頭にフォーカスをあてる
                if (!notFocusChange)
                {
                    _characterItem[index].GetComponent<WindowButtonBase>().SetEnabled(true);
                    _characterItem[index].GetComponent<Button>().Select();
                }
                else
                {
                    var party = DataManager.Self().GetGameParty();

                    //GameAction取得
                    for (int num = 0; num < party.Actors.Count; num++)
                    {
                        if (party.Actors[num].ActorId == _useActorId)
                        {
                            _characterItem[num].GetComponent<WindowButtonBase>().SetEnabled(true);
                            _characterItem[num].GetComponent<Button>().Select();
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// スキル使用処理
        /// </summary>
        /// <param name="index"></param>
        /// <param name="skillId"></param>
        private void UseSkill(int index, string skillId) {
            //パーティ情報取得
            var party = DataManager.Self().GetGameParty();

            //GameAction取得
            GameActor item = null;
            for (int i = 0; i < party.Actors.Count; i++)
                if (party.Actors[i].ActorId == _useActorId)
                {
                    item = party.Actors[i];
                    break;
                }

            _gameAction = new GameAction(item);

            //使用スキル設定
            var skill = DataManager.Self().GetSkillCustomDataModel(skillId);
            _gameAction.SetSkill(skill.basic.id);

            //対象者を取得
            var gameBattlers =
                isAll ? _gameAction.MakeTargets() : new List<GameBattler> {_characterItem[index].GameActor};
            foreach (var battler in gameBattlers)
            {
                battler.Result ??= new GameActionResult();
            }

            //コモンイベントの有無を判断
            bool isCommonEvent = false;
            bool isCommonEventForUser = false;
            foreach (var effect in _gameAction.Item.Effects)
            {
                if (isCommonEvent) break;
                isCommonEvent = _gameAction.IsEffectCommonEvent(effect);
            }

            //使用者への影響にチェックが入っている場合、使用者への影響側のコモンイベントも確認
            if (_gameAction.IsForUser())
                foreach (var effect in _gameAction.Item.EffectsMyself)
                {
                    if (isCommonEventForUser) break;
                    isCommonEventForUser = _gameAction.IsEffectCommonEvent(effect);
                }

            //ブザー音を鳴動するかどうか
            bool isSuccess = isCommonEvent || isCommonEventForUser;
            int hitCount = 0;

            //スキルを利用して効果があるかどうかの確認
            bool isEffect = false;
            foreach (var battler in gameBattlers)
            {
                if (_gameAction.TestApply(battler) && (_gameAction.IsForFriend() || _gameAction.IsForOpponentAndFriend()))
                {
                    isEffect = true;
                    break;
                }
            }

            if (_gameAction.IsForUser())
            {
                if (_gameAction.TestApplyMyself(item))
                {
                    isEffect = true;
                }
            }

            //コストの確認
            if ((isEffect || isCommonEvent || isCommonEventForUser) && item.CanPaySkillCost(_gameItem))
            {
                //対象者に対してアクションを実行
                item.UseItem(_gameItem);
                foreach (var battler in gameBattlers)
                {
                    //対象者へApply
                    if (_gameAction.TestApply(battler) && (_gameAction.IsForFriend() || _gameAction.IsForOpponentAndFriend()))
                    {
                        _gameAction.Apply(battler);
                        hitCount++;
                    }
                }

                //使用者への影響が設定されている場合、アクションを実行
                if (_gameAction.IsForUser())
                {
                    if (_gameAction.TestApplyMyself(item))
                    {
                        _gameAction.ApplyMyself(item);
                        hitCount++;
                    }
                }

                //コモンイベント実行
                if (isCommonEvent || isCommonEventForUser)
                    _gameAction.SetCommonEvent(isCommonEventForUser);

                if (hitCount > 0)
                {
                    isSuccess = true;
                    for (int i = 0; i < _characterItem.Count; i++)
                    {
                        _characterItem[i].UpdateData(_characterItem[i].RuntimeActorDataModel);
                    }
                }
            }

            //音声鳴動
            if (isSuccess || isCommonEvent || isCommonEventForUser)
            {
                _gameAction.UseItemPlaySe(1);
                MenuManager.MenuBase.AllUpdateStatus();
                SetFocusAft(index);
            }
            else
            {
                SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, systemSettingDataModel.soundSetting.buzzer);
                SoundManager.Self().PlaySe();
            }
        }

        private void UseItem(int index, string itemId) {
            //パーティ情報取得
            var item = _characterItem[index];
            var party = DataManager.Self().GetGameParty();

            //GameAction取得
            _gameAction = new GameAction(item.GameActor);
            _gameAction.SetItem(_gameItem.ItemId);

            //使用アイテム設定
            //アイテムを所持していない場合は終了
            if (!party.HasItem(_gameItem))
            {
                SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, systemSettingDataModel.soundSetting.buzzer);
                SoundManager.Self().PlaySe();
                return;
            }

            //対象者を取得
            var gameBattlers = isAll ? _gameAction.MakeTargets() : new List<GameBattler> {_characterItem[index].GameActor};
            foreach (var battler in gameBattlers)
            {
                battler.Result ??= new GameActionResult();
            }

            //コモンイベントの有無を判断
            bool isCommonEvent = false;
            bool isCommonEventForUser = false;
            foreach (var effect in _gameAction.Item.Effects)
            {
                if (isCommonEvent) break;
                isCommonEvent = _gameAction.IsEffectCommonEvent(effect);
            }

            //使用者への影響にチェックが入っている場合、使用者への影響側のコモンイベントも確認
            if (_gameAction.IsForUser())
                foreach (var effect in _gameAction.Item.EffectsMyself)
                {
                    if (isCommonEventForUser) break;
                    isCommonEventForUser = _gameAction.IsEffectCommonEvent(effect);
                }

            //スキルを利用して効果があるかどうかの確認
            bool isEffect = false;
            foreach (var battler in gameBattlers)
            {
                if (_gameAction.TestApply(battler) && (_gameAction.IsForFriend() || _gameAction.IsForOpponentAndFriend()))
                {
                    isEffect = true;
                    break;
                }
            }

            if (_gameAction.IsForUser())
            {
                if (_gameAction.TestApplyMyself(item.GameActor))
                {
                    isEffect = true;
                }
            }

            //ブザー音を鳴動するかどうか
            bool isSuccess = isCommonEvent || isCommonEventForUser;
            int hitCount = 0;

            if (isEffect || isCommonEvent || isCommonEventForUser)
            {
                //対象者に対してアクションを実行
                item.GameActor.UseItem(_gameItem);
                foreach (var battler in gameBattlers)
                {
                    //対象者へApply
                    if (_gameAction.TestApply(battler) && (_gameAction.IsForFriend() || _gameAction.IsForOpponentAndFriend()))
                    {
                        _gameAction.Apply(battler);
                        hitCount++;
                    }
                }

                //使用者への影響が設定されている場合、アクションを実行
                if (_gameAction.TestApplyMyself(item.GameActor))
                {
                    _gameAction.ApplyMyself(item.GameActor);
                    hitCount++;
                }

                //コモンイベント実行
                if (isCommonEvent || isCommonEventForUser)
                    _gameAction.SetCommonEvent(isCommonEventForUser);

                if (hitCount > 0)
                {
                    isSuccess = true;
                    for (int i = 0; i < _characterItem.Count; i++)
                    {
                        _characterItem[i].UpdateData(_characterItem[i].RuntimeActorDataModel);
                    }
                }
            }

            //音声鳴動
            if (isSuccess || isCommonEvent || isCommonEventForUser)
            {
                _gameAction.UseItemPlaySe(0);
                MenuManager.MenuBase.AllUpdateStatus();
                SetFocusAft(index);
            }
            else
            {
                SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, systemSettingDataModel.soundSetting.buzzer);
                SoundManager.Self().PlaySe();
            }
        }


        public void ButtonEvent(int index) {
            if (_partyType == PartyType.Item)
                UseItem(index, _useId);
            else
                UseSkill(index, _useId);
        }

        public new void Back() {
            gameObject.SetActive(false);
            if (_callback != null) _callback();
        }
    }
}