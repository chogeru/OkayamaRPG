using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCustom;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map.Menu;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Map.Item
{
    public class SkillItem : MonoBehaviour
    {
        private Button               _button;
        private SkillCustomDataModel _customDataModel;
        private Image                _icon;

        private TextMeshProUGUI _name;
        private SkillMenu       _skillMenu;
        private TextMeshProUGUI _value;
        public GameItem GameItem { get; private set; }

        private GameActor _actor;
        private Action _openAction;
        private Action _closeAction;

        private bool isUse = false;

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="root"></param>
        /// <param name="skillId"></param>
        /// <param name="openAction"></param>
        /// <param name="closeAction"></param>
        public void Init(SkillMenu root, string skillId, Action openAction, Action closeAction) {
            _customDataModel = DataManager.Self().GetSkillCustomDataModel(skillId);
            if (_customDataModel == null)
            {
                transform.Find("Icon").gameObject.SetActive(false);
                return;
            }
            if (GameItem == null) GameItem = new GameItem(skillId, GameItem.DataClassEnum.Skill);
            _skillMenu = root;
            _name = transform.Find("Name").GetComponent<TextMeshProUGUI>();
            _icon = transform.Find("Icon").GetComponent<Image>();
            _value = transform.Find("Value").GetComponent<TextMeshProUGUI>();

            _button = GetComponent<Button>();
            if (transform.GetComponent<WindowButtonBase>() != null)
            {
                transform.GetComponent<WindowButtonBase>().OnFocus = new UnityEngine.Events.UnityEvent();
                transform.GetComponent<WindowButtonBase>().OnFocus.AddListener(OnFocus);
                transform.GetComponent<WindowButtonBase>().OnClick = new Button.ButtonClickedEvent();
                transform.GetComponent<WindowButtonBase>().OnClick.AddListener(ButtonEvent);
            }

            _actor = DataManager.Self().GetGameActors().Actor(_skillMenu.GetActor());
            _openAction = openAction;
            _closeAction = closeAction;
            
            _icon.sprite = GetItemImage(_customDataModel.basic.iconId);

            UpdateStatus();
        }

        /// <summary>
        /// ステータス更新
        /// </summary>
        public void UpdateStatus() {
            _name.text = _customDataModel.basic.name;
            isUse = true;
            _value.enabled = true;
            if (_customDataModel.basic.costTp > 0)
            {
                _value.text = _customDataModel.basic.costTp.ToString();
                _value.color = Color.green;
            }
            else if (_customDataModel.basic.costMp > 0)
            {
                _value.text = _actor.SkillMpCost(GameItem).ToString();
                _value.color = Color.blue;
            }
            else
            {
                //MP,TPが0の場合、非表示
                _value.enabled = false;
            }

            switch ((ItemEnums.ItemCanUseTiming) _customDataModel.basic.canUseTiming)
            {
                case ItemEnums.ItemCanUseTiming.ALL:
                case ItemEnums.ItemCanUseTiming.MENU:
                    isUse = true;
                    _button.interactable = true;
                    _name.color = Color.white;
                    _icon.color = Color.white;
                    break;
                case ItemEnums.ItemCanUseTiming.BATTLE:
                case ItemEnums.ItemCanUseTiming.NONE:
                    isUse = false;
                    _name.color = Color.gray;
                    _icon.color = Color.gray;
                    break;
            }

            //スキルタイプ封印、スキル封印
            if (_actor.IsSkillSealed(_customDataModel.SerialNumber - 1) || _actor.IsSkillTypeSealed(GameItem.STypeId))
            {
                isUse = false;
                _name.color = Color.gray;
            }

            if (!isUse)
            {
                transform.GetComponent<WindowButtonBase>().SetGray(true);
            }
        }

        /// <summary>
        /// OnFocus時処理
        /// </summary>
        private void OnFocus() {
            _skillMenu.SetDescription(_customDataModel.basic.description);
        }

        /// <summary>
        /// スキル選択時処理
        /// </summary>
        public void ButtonEvent() {
            var gameAction = new GameAction(_actor);
            gameAction.SetSkill(_customDataModel.basic.id);

            //敵が対象で、使用者への影響も設定されていない場合
            if ((gameAction.IsForOpponent() || GameItem.Scope == 0) && !gameAction.IsForUser())
            {
                //このケースであっても、コモンイベントが設定されており、かつ個数が足りている場合は、
                //コモンイベントだけを実行する
                var gameBattlers = gameAction.MakeTargets();
                foreach (var battler in gameBattlers)
                {
                    battler.Result ??= new GameActionResult();
                }

                bool isCommonEvent = false;
                bool isCommonEventForUser = false;

                //コモンイベントが設定されているか
                foreach (var effect in GameItem.Effects)
                {
                    if (isCommonEvent) break;
                    isCommonEvent = gameAction.IsEffectCommonEvent(effect);
                }

                //使用者への影響にチェックが入っている場合、使用者への影響側のコモンイベントも確認
                if (gameAction.IsForUser())
                    foreach (var effect in gameAction.Item.EffectsMyself)
                    {
                        if (isCommonEventForUser) break;
                        isCommonEventForUser = gameAction.IsEffectCommonEvent(effect);
                    }

                if (isCommonEvent || isCommonEventForUser)
                {
                    //コモンイベントが設定されている場合、個数が足りているか
                    if (_actor.CanPaySkillCost(GameItem))
                    {
                        //コモンイベントを実行して終了する
                        _actor.UseItem(GameItem);
                        gameAction.SetCommonEvent(isCommonEventForUser);

                        //スキル使用時のSE鳴動
                        gameAction.UseItemPlaySe(1);
                        MenuManager.MenuBase.AllUpdateStatus();
                        return;
                    }
                }
            }
            else
            {
                //対象が味方なので、選択Windowを表示して終了する
                _skillMenu.MenuBase.OpenPartyWindow(PartyWindow.PartyType.Skill, _skillMenu.ActorId(), _customDataModel.basic.id, GameItem, _closeAction);
                _openAction?.Invoke();

                //最終選択したスキルのIDを保持
                var runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();
                for (int i = 0; i < runtimeSaveDataModel.runtimeActorDataModels.Count; i++)
                    if (runtimeSaveDataModel.runtimeActorDataModels[i].actorId == _skillMenu.ActorId())
                    {
                        runtimeSaveDataModel.runtimeActorDataModels[i].lastMenuSkill.dataClass = "skill";
                        runtimeSaveDataModel.runtimeActorDataModels[i].lastMenuSkill.itemId = _customDataModel.basic.id;
                        break;
                    }

                return;
            }

            //ブザー音鳴動
            SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, DataManager.Self().GetSystemDataModel().soundSetting.buzzer);
            SoundManager.Self().PlaySe();
        }

        private Sprite GetItemImage(string iconName) {
            var iconSetTexture =
                UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(
                    "Assets/RPGMaker/Storage/Images/System/IconSet/" + iconName + ".png");

            var iconTexture = iconSetTexture;

            if (iconTexture == null)
            {
                _icon.gameObject.SetActive(false);
                return null;
            }


            var sprite = Sprite.Create(
                iconTexture,
                new Rect(0, 0, iconTexture.width, iconTexture.height),
                new Vector2(0.5f, 0.5f)
            );

            var aspect =
                ImageManager.FixAspect(new Vector2(66f, 66f), new Vector2(iconTexture.width, iconTexture.height));
            var aspectRatio = _icon.GetComponent<AspectRatioFitter>();
            if (aspectRatio == null)
            {
                aspectRatio = _icon.gameObject.AddComponent<AspectRatioFitter>();
                aspectRatio.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
            }
            aspectRatio.aspectRatio = aspect;


            return sprite;
        }
    }
}