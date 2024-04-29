using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SkillCustom;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Common;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TextMP = TMPro.TextMeshProUGUI;

namespace RPGMaker.Codebase.Runtime.Battle.Window
{
    /// <summary>
    /// 戦闘シーンのアクターのコマンド( [攻撃][スキル][防御][アイテム] )のウィンドウ
    /// </summary>
    public class WindowActorCommand : WindowCommand
    {
        /// <summary>
        /// アクター
        /// </summary>
        private GameActor _actor;
        /// <summary>
        /// 最初に選択するボタン
        /// </summary>
        private GameObject _focusButton;
        /// <summary>
        /// ボタンリスト
        /// </summary>
        private readonly List<GameObject> _gameObjects = new List<GameObject>();

        /// <summary>
        /// 直前で追加したボタン
        /// </summary>
        private Button _beforeButton;
        /// <summary>
        /// 現在のボタン一覧
        /// </summary>
        private List<Button> _buttons;

        /// <summary>
        /// コマンド名称
        /// </summary>
        private List<string> _commandName;

        /// <summary>
        /// 初期化
        /// </summary>
        override public void Initialize() {
            base.Initialize();
            Openness = 0;
            Deactivate();
            _actor = null;

            //共通UIの適応を開始
            Init();

            // ボタン取得
            _focusButton = gameObject.transform.Find("WindowArea/Scroll View/Viewport/Content/Attack").gameObject;
        }

        /// <summary>
        /// メニューに全項目を追加。 個々の追加は addCommand で行っている
        /// </summary>
        public override void MakeCommandList() {
            for (int i = _gameObjects.Count - 1; i >= 0; i--)
            {
                DestroyImmediate(_gameObjects[i]);
                _gameObjects.RemoveAt(i);
            }
            DestroyChildren();
            if (_actor != null)
            {
                AddAttackCommand();
                AddSkillCommands();
                AddGuardCommand();
                AddItemCommand();
            }
        }

        /// <summary>
        /// [攻撃]コマンドを追加
        /// </summary>
        public void AddAttackCommand() {
            var skillId = GameBattlerBase.AttackSkillId;
            var skillData = DataManager.Self().GetSkillCustomDataModels();
            SkillCustomDataModel skill = null;
            for (int i = 0; i < skillData.Count; i++)
            {
                if (skillData[i].SerialNumber == _actor.AttackSkill() + 1)
                {
                    skill = skillData[i];
                    skillId = skill.basic.id;
                    break;
                }
            }
            if (BattleManager.InputtingAction() != null) BattleManager.InputtingAction().SetAttackSkill(skillId);
            AddCommand(TextManager.attack, "attack", _actor.CanAttack(skillId));
            var attackBtn = gameObject.transform.Find("WindowArea/Scroll View/Viewport/Content/Attack").gameObject
                .GetComponent<Button>();
            _beforeButton = gameObject.transform.Find("WindowArea/Scroll View/Viewport/Content/Attack").gameObject
                .GetComponent<Button>();
            
            _buttons = new List<Button>();
            _buttons.Add(attackBtn);
            attackBtn.transform.GetComponent<WindowButtonBase>().OnFocus = new UnityEngine.Events.UnityEvent();
            attackBtn.transform.GetComponent<WindowButtonBase>().OnFocus.AddListener(() =>
            {
                SelectSymbol("attack");
            });
            attackBtn.transform.GetComponent<WindowButtonBase>().OnClick = new Button.ButtonClickedEvent();
            attackBtn.transform.GetComponent<WindowButtonBase>().OnClick.AddListener(() =>
            {
                ProcessOk("attack", 0);
                OnClickCommand("attack");
            });
            
            if (skill != null)
            {
                var gameItem = new GameItem(skill.basic.id, GameItem.DataClassEnum.Skill);
                if (_actor.CanPaySkillCost(gameItem))
                {
                    var btnColorColors = attackBtn.colors;
                    btnColorColors.disabledColor = new Color(1f, 1f, 1f);
                                    btnColorColors.normalColor = new Color(1f, 1f, 1f);
                                    btnColorColors.selectedColor = new Color(1f, 1f, 1f, 0.5f);
                                    btnColorColors.pressedColor = new Color(1f, 1f, 1f);
                                    btnColorColors.highlightedColor = new Color(1f, 1f, 1f);
                    attackBtn.colors = btnColorColors;
                    attackBtn.GetComponent<WindowButtonBase>().SetGray();
                }
                else
                {
                    var btnColorColors = attackBtn.colors;
                    btnColorColors.disabledColor = new Color(0.5f, 0.5f, 0.5f);
                    btnColorColors.normalColor = new Color(0.5f, 0.5f, 0.5f);
                    btnColorColors.selectedColor = new Color(0.5f, 0.5f, 0.5f);
                    btnColorColors.pressedColor = new Color(0.5f, 0.5f, 0.5f);
                    btnColorColors.highlightedColor = new Color(0.5f, 0.5f, 0.5f);
                    attackBtn.colors = btnColorColors;
                    attackBtn.GetComponent<WindowButtonBase>().SetGray(true);
                }
            }
            
            

            _commandName = new List<string>();
            _commandName.Add("attack");
        }

        /// <summary>
        /// [スキル]コマンドを追加
        /// </summary>
        public void AddSkillCommands() {
            var skillTypes = new List<int>();
            foreach (var data in _actor.Skills())
            {
                var skill = DataManager.Self().GetSkillCustomDataModel(data.basic.id);
                if (!skillTypes.Contains(skill.basic.skillType)) skillTypes.Add(skill.basic.skillType);
            }
            skillTypes.Sort((i, i1) => i - i1);

            var origin = gameObject.transform.Find("WindowArea/Scroll View/Viewport/Content/Skill").gameObject;
            origin.SetActive(true);
            for (var i = 0; i < skillTypes.Count; i++)
            {
                var obj = Instantiate(origin, origin.transform.parent, false);
                selectors.Add(obj.GetComponent<Selector>());
                var index = i;
                var name = systemSettingDataModel.skillTypes[skillTypes[i]];
                obj.transform.Find("Text").GetComponent<TextMP>().text = name.value;
                _gameObjects.Add(obj);

                AddCommand(name.value, "skill" + index, true);

                var btn = obj.GetComponent<Button>();
                btn.targetGraphic = obj.transform.Find("Highlight").GetComponent<Image>();
                btn.transform.GetComponent<WindowButtonBase>().OnFocus = new UnityEngine.Events.UnityEvent();
                btn.transform.GetComponent<WindowButtonBase>().OnFocus.AddListener(() =>
                {
                    SelectSymbol("skill" + index);
                });
                btn.transform.GetComponent<WindowButtonBase>().OnClick = new Button.ButtonClickedEvent();
                btn.transform.GetComponent<WindowButtonBase>().OnClick.AddListener(() =>
                {
                    SetExt(skillTypes[index]);
                    ProcessOk("skill", index);
                    OnClickCommand("skill");
                });

                //Navigationの登録
                //上への移動
                Navigation navigation = btn.navigation;
                navigation.mode = Navigation.Mode.Explicit;
                navigation.selectOnUp = _beforeButton;
                btn.navigation = navigation;

                //下への移動
                navigation = _beforeButton.navigation;
                navigation.mode = Navigation.Mode.Explicit;
                navigation.selectOnDown = btn;
                _beforeButton.navigation = navigation;

                //直前で編集したボタン
                _beforeButton = btn;
                _buttons.Add(btn);
                _commandName.Add("skill" + index);
            }

            origin.SetActive(false);
        }

        /// <summary>
        /// [防御]コマンドを追加
        /// </summary>
        public void AddGuardCommand() {
            var origin = gameObject.transform.Find("WindowArea/Scroll View/Viewport/Content/Guard").gameObject;
            origin.SetActive(true);
            var obj = Instantiate(origin, origin.transform.parent, false);
            obj.transform.Find("Text").GetComponent<TextMP>().text = TextManager.guard;
            _gameObjects.Add(obj);

            selectors.Add(obj.GetComponent<Selector>());
            origin.SetActive(false);

            obj.transform.GetComponent<WindowButtonBase>().OnFocus = new UnityEngine.Events.UnityEvent();
            obj.transform.GetComponent<WindowButtonBase>().OnFocus.AddListener(() =>
            {
                SelectSymbol("defence");
            });
            obj.transform.GetComponent<WindowButtonBase>().OnClick = new Button.ButtonClickedEvent();
            obj.transform.GetComponent<WindowButtonBase>().OnClick.AddListener(() =>
            {
                ProcessOk("defence", 0);
                OnClickCommand("defence");
            });

            AddCommand(TextManager.guard, "defence", _actor.CanGuard());

            //Navigationの登録
            Button btn = obj.GetComponent<Button>();
            btn.targetGraphic = obj.transform.Find("Highlight").GetComponent<Image>();
            //上への移動
            Navigation navigation = btn.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnUp = _beforeButton;
            btn.navigation = navigation;

            //下への移動
            navigation = _beforeButton.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnDown = btn;
            _beforeButton.navigation = navigation;

            //直前で編集したボタン
            _beforeButton = btn;
            _buttons.Add(btn);
            _commandName.Add("defence");
        }

        /// <summary>
        /// [アイテム]コマンドを追加
        /// </summary>
        public void AddItemCommand() {
            var origin = gameObject.transform.Find("WindowArea/Scroll View/Viewport/Content/Item").gameObject;
            origin.SetActive(true);
            var obj = Instantiate(origin, origin.transform.parent, false);
            selectors.Add(obj.GetComponent<Selector>());
            _gameObjects.Add(obj);
            obj.transform.Find("Text").GetComponent<TextMP>().text = TextManager.item;
            origin.SetActive(false);

            obj.transform.GetComponent<WindowButtonBase>().OnFocus = new UnityEngine.Events.UnityEvent();
            obj.transform.GetComponent<WindowButtonBase>().OnFocus.AddListener(() =>
            {
                SelectSymbol("item");
            });
            obj.transform.GetComponent<WindowButtonBase>().OnClick = new Button.ButtonClickedEvent();
            obj.transform.GetComponent<WindowButtonBase>().OnClick.AddListener(() =>
            {
                ProcessOk("item", 0);
                OnClickCommand("item");
            });

            AddCommand(TextManager.item, "item");

            //Navigationの登録
            Button btn = obj.GetComponent<Button>();
            btn.targetGraphic = obj.transform.Find("Highlight").GetComponent<Image>();
            Button attackBtn = gameObject.transform.Find("WindowArea/Scroll View/Viewport/Content/Attack").gameObject.GetComponent<Button>();

            //上への移動
            Navigation navigation = btn.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnUp = _beforeButton;
            navigation.selectOnDown = attackBtn;
            btn.navigation = navigation;

            //下への移動
            navigation = _beforeButton.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnDown = btn;
            _beforeButton.navigation = navigation;

            //ループ用の処理
            navigation = attackBtn.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnUp = btn;
            attackBtn.navigation = navigation;

            //直前で編集したボタン
            _beforeButton = btn;
            _buttons.Add(btn);
            _commandName.Add("item");
        }

        /// <summary>
        /// 指定アクターのコマンドを設定
        /// </summary>
        /// <param name="actor"></param>
        public void Setup(GameActor actor) {
            // ボタンを選択状態にする
            if (_focusButton != null)
                EventSystem.current.SetSelectedGameObject(_focusButton);

            _actor = actor;
            ClearCommandList();
            MakeCommandList();

            // 選択UIのナビゲーションを設定する
            var selects = gameObject.transform.Find("WindowArea/Scroll View/Viewport/Content").gameObject
                .GetComponentsInChildren<Selectable>();
            for (var i = 0; i < selects.Length; i++)
            {
                selects[i].targetGraphic = selects[i].transform.Find("Highlight").GetComponent<Image>();
                var nav = selects[i].navigation;
                nav.mode = Navigation.Mode.Explicit;
                nav.selectOnUp = selects[i == 0 ? selects.Length - 1 : i - 1];
                nav.selectOnDown = selects[(i + 1) % selects.Length];
                selects[i].navigation = nav;
            }

            var scrollRect = gameObject.transform.Find("WindowArea/Scroll View").GetComponent<ScrollRect>();
            scrollRect.enabled = true;
            scrollRect.horizontal = false;


            Refresh();
            SelectLast();
            Activate();
            Open();
            ActorCommandWords();
        }

        /// <summary>
        /// OKの処理
        /// </summary>
        public void ProcessOk(string symbol, int stypeid) {
            if (_actor != null)
            {
                if (DataManager.Self().GetRuntimeConfigDataModel()?.commandRemember == 1)
                {
                    _actor.LastCommandSymbol = symbol;
                    _actor.LastCommandSymbolStypeId = stypeid;
                }
                else
                {
                    _actor.LastCommandSymbol = "";
                    _actor.LastCommandSymbolStypeId = -1;
                }
            }
        }

        /// <summary>
        /// 前に選択した項目を選択
        /// </summary>
        public void SelectLast() {
            if (_actor != null && DataManager.Self().GetRuntimeConfigDataModel()?.commandRemember != null)
            {
                var symbol = _actor.LastCommandSymbol;
                if (symbol == "skill")
                {
                    var skill = _actor.LastBattleSkill();
                    if (skill != null)
                        SelectExt(skill.STypeId);
                }
            }
        }

        /// <summary>
        /// 各コマンドのローカライズ
        /// </summary>
        private void ActorCommandWords() {
            TextMP attak = transform.Find("WindowArea/Scroll View/Viewport/Content/Attack/Text").GetComponent<TextMP>();
            TextMP skill = transform.Find("WindowArea/Scroll View/Viewport/Content/Skill/Text").GetComponent<TextMP>();
            TextMP guard = transform.Find("WindowArea/Scroll View/Viewport/Content/Guard/Text").GetComponent<TextMP>();
            TextMP item = transform.Find("WindowArea/Scroll View/Viewport/Content/Item/Text").GetComponent<TextMP>();

            attak.text = TextManager.attack;
            skill.text = TextManager.skill;
            guard.text = TextManager.guard;
            item.text = TextManager.item;
        }

        /// <summary>
        /// ウィンドウを表示
        /// </summary>
        public override void Show() {
            base.Show();
            //ボタンの有効状態切り替え
            for (int i = 0; _buttons != null && i < _buttons.Count; i++)
            {
                if (_buttons[i] != null)
                    _buttons[i].GetComponent<WindowButtonBase>().SetEnabled(true);
            }

            if (_buttons != null)
            {
                string symbol = _actor.LastCommandSymbol;
                if (symbol == "skill")
                {
                    var skill = _actor.LastBattleSkill();
                    if (skill != null)
                        SelectExt(skill.STypeId);

                    if (_commandName.IndexOf(_actor.LastCommandSymbol + (_actor.LastCommandSymbolStypeId)) >= 0)
                    {
                        _buttons[(_commandName.IndexOf(_actor.LastCommandSymbol + (_actor.LastCommandSymbolStypeId)))].GetComponent<Button>().Select();
                    }
                    else
                    {
                        _buttons[0].GetComponent<Button>().Select();
                    }
                }
                else
                {
                    if (_commandName.IndexOf(_actor.LastCommandSymbol) >= 0)
                    {
                        _buttons[(_commandName.IndexOf(_actor.LastCommandSymbol))].GetComponent<Button>().Select();
                    }
                    else
                    {
                        _buttons[0].GetComponent<Button>().Select();
                    }
                }
            }
        }

        /// <summary>
        /// ウィンドウを非表示(閉じるわけではない)
        /// </summary>
        public override void Hide() {
            //ボタンの有効状態切り替え
            for (int i = 0; _buttons != null && i < _buttons.Count; i++)
            {
                _buttons[i].GetComponent<WindowButtonBase>().SetEnabled(false);
            }
            base.Hide();
        }

        public void SetBattlePreviewMode(GameActor actor) {
            _actor = actor;
            ClearCommandList();
            MakeCommandList();
            ActorCommandWords();
            _buttons[0].GetComponent<WindowButtonBase>().SetHighlight(true);
            _buttons[0].transform.Find("Highlight").gameObject.SetActive(false);
            _buttons[0].transform.Find("Highlight").gameObject.SetActive(true);
        }
        
        public void DestroyChildren() {
            var gameObjects = gameObject.transform.Find("WindowArea/Scroll View/Viewport/Content")
                .GetComponentsInChildren<Button>();
            for (int i = 0; i < gameObjects.Length; i++)
            {
                if(gameObjects[i].gameObject.name == "Attack") continue;
                if(gameObjects[i].gameObject.name == "Skill") continue;
                if(gameObjects[i].gameObject.name == "Guard") continue;
                if(gameObjects[i].gameObject.name == "Item") continue;
                DestroyImmediate(gameObjects[i].gameObject);
            }
        }
    }
}