using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.SystemSetting;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map.Item;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TextMP = TMPro.TextMeshProUGUI;

namespace RPGMaker.Codebase.Runtime.Map.Menu
{
    public class MainMenu : WindowBase
    {
        private List<CharacterItem> _characterItem;

        private DisplayType _display = DisplayType.None;
        private TextMP      _endText;
        private TextMP      _equipText;
        private GameObject  _goldObject;
        private TextMP      _goldText;
        private Text        _goldUnitText;
        private TextMP      _itemText;

        private MenuBase    _menuBase;

        private GameObject  _menuObject;
        private TextMP      _optionText;
        private GameObject  _partyObject;
        private TextMP      _saveText;
        private TextMP      _skillText;
        private TextMP      _sortText;
        private TextMP      _statusText;
        private int _pattern;

        private MainMenuState _state;

        enum MainMenuState
        {
            COMMAND,
            SELECT_ACTOR
        }

        private void OnEnable() {
            UpdateStatus();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="manager"></param>
        public void Init(WindowBase manager) {
            _menuBase = manager as MenuBase;
            var saveData = DataManager.Self().GetRuntimeSaveDataModel();

            SystemSettingDataModel systemSettingDataModel = DataManager.Self().GetSystemDataModel();
            _pattern = int.Parse(systemSettingDataModel.uiPatternId) + 1;
            if (_pattern < 1 || _pattern > 6)
                _pattern = 1;

            //UI,用語設定のために保持しておく
            _menuObject = transform.Find("MenuArea/MenuWindow").gameObject;
            _itemText = _menuObject.transform.Find("MenuItems/Item/Text").GetComponent<TextMP>();
            _skillText = _menuObject.transform.Find("MenuItems/Skill/Text").GetComponent<TextMP>();
            _equipText = _menuObject.transform.Find("MenuItems/Equipment/Text").GetComponent<TextMP>();
            _statusText = _menuObject.transform.Find("MenuItems/Status/Text").GetComponent<TextMP>();
            _sortText = _menuObject.transform.Find("MenuItems/Sort/Text").GetComponent<TextMP>();
            _optionText = _menuObject.transform.Find("MenuItems/Option/Text").GetComponent<TextMP>();
            _saveText = _menuObject.transform.Find("MenuItems/Save/Text").GetComponent<TextMP>();
            _endText = _menuObject.transform.Find("MenuItems/End/Text").GetComponent<TextMP>();

            _goldObject = transform.Find("MenuArea/GoldWindow").gameObject;
            _goldText = _goldObject.transform.Find("MenuItems/NowGold").GetComponent<TextMP>();
            _goldUnitText = _goldObject.transform.Find("MenuItems/Currency").GetComponent<Text>();

            _partyObject = transform.Find("MenuArea/PartyWindow").gameObject;
            var partyItems = _partyObject.transform.Find("PartyItems").gameObject;

            _gameState = GameStateHandler.GameState.MENU;

            //十字キーでの操作登録
            var selects = _menuObject.GetComponentsInChildren<Button>();
            for (var i = 0; i < selects.Length; i++)
            {
                var nav = selects[i].navigation;
                nav.mode = Navigation.Mode.Explicit;

                //UIパターンに応じて十字キーを変更する
                if (_pattern == 1 || _pattern == 2)
                {
                    nav.selectOnUp = selects[i == 0 ? selects.Length - 1 : i - 1];
                    nav.selectOnDown = selects[(i + 1) % selects.Length];
                }
                else
                {
                    nav.selectOnLeft = selects[i == 0 ? selects.Length - 1 : i - 1];
                    nav.selectOnRight = selects[(i + 1) % selects.Length];
                }

                selects[i].navigation = nav;
                selects[i].targetGraphic = selects[i].transform.Find("Highlight").GetComponent<Image>();
            }

            if (selects.Length > 0) selects[0].Select();

            //セーブデータからの反映
            //現在の所持金
            _goldText.text = saveData?.runtimePartyDataModel.gold.ToString();

            _characterItem = new List<CharacterItem>();
            //パーティ部分
            for (var i = 0; i < 4; i++)
            {
                if (i < saveData?.runtimePartyDataModel.actors.Count)
                {
                    for (var j = 0; j < saveData.runtimeActorDataModels.Count; j++)
                    {
                        if (saveData.runtimeActorDataModels[j].actorId == saveData.runtimePartyDataModel.actors[i])
                        {
                            partyItems.transform.Find("Actor" + (i + 1)).gameObject.SetActive(true);
                            var characterItem = partyItems.transform.Find("Actor" + (i + 1)).gameObject
                                .GetComponent<CharacterItem>();
                            if (characterItem == null)
                            {
                                characterItem = partyItems.transform.Find("Actor" + (i + 1)).gameObject
                                    .AddComponent<CharacterItem>();
                            }
                            characterItem.Init(saveData.runtimeActorDataModels[j]);
                            _characterItem.Add(characterItem);
                            break;
                        }
                    }
                }
                else
                {
                    partyItems.transform.Find("Actor" + (i + 1)).gameObject.SetActive(false);
                }
            }

            //用語の適応
            _itemText.text = TextManager.item;
            _skillText.text = TextManager.skill;
            _equipText.text = TextManager.equip;
            _statusText.text = TextManager.status;
            _sortText.text = TextManager.formation;
            _optionText.text = TextManager.options;
            _saveText.text = TextManager.save;
            _endText.text = TextManager.gameEnd;
            _goldUnitText.text = TextManager.money;

            //共通のウィンドウの適応部分
            Init();

            //初期選択状態
            _state = MainMenuState.COMMAND;
        }

        /// <summary>
        /// セーブボタン押下可否の更新
        /// </summary>
        public void CanSave() {
            //セーブボタン押下有無設定
            WindowButtonBase button = _menuObject.transform.Find("MenuItems/Save").GetComponent<WindowButtonBase>();
            button.SetGray(DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.saveEnabled == 0);
        }

        /// <summary>
        /// ソートボタン押下可否の更新
        /// </summary>
        public void CanSort() {
            //ソートボタン押下有無設定
            WindowButtonBase button = _menuObject.transform.Find("MenuItems/Sort").GetComponent<WindowButtonBase>();
            button.SetGray(DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.formationEnabled == 0);
        }

        /// <summary>
        /// メニューを閉じて戻ってよいかどうかを返却する
        /// </summary>
        /// <returns></returns>
        public bool BackWindow() {
            if (_state == MainMenuState.SELECT_ACTOR)
            {
                _state = MainMenuState.COMMAND;
                ChangeFocusList();
                return false;
            }
            return true;
        }

        /// <summary>
        /// 他のメニューから戻ってきた場合の処理
        /// </summary>
        /// <returns></returns>
        public void BackMenu() {
            _state = MainMenuState.COMMAND;
            ChangeFocusList();
        }

        /// <summary>
        /// リストのフォーカス位置を変更する
        /// </summary>
        private void ChangeFocusList() {
            if (_state == MainMenuState.COMMAND)
            {
                //第一階層のメニューを選択可能とする
                int num = 0;
                var selects = _menuObject.GetComponentsInChildren<Button>();
                for (var i = 0; i < selects.Length; i++)
                {
                    selects[i].GetComponent<WindowButtonBase>().SetEnabled(true);
                    if (selects[i].GetComponent<WindowButtonBase>().IsHighlight())
                        num = i;
                }
                //第二階層のメニューは選択不可とする
                //ハイライト表示は初期化する
                for (var i = 0; i < _characterItem.Count; i++)
                {
                    var select = _characterItem[i].GetComponent<Button>();
                    select.GetComponent<WindowButtonBase>().SetEnabled(true, true);
                }
                //現在の状態に応じてフォーカス設定位置を変更する
                selects[num].GetComponent<Button>().Select();
            }
            else
            {
                //第一階層のメニューは選択不可とする
                var selects = _menuObject.GetComponentsInChildren<Button>();
                for (var i = 0; i < selects.Length; i++)
                {
                    selects[i].GetComponent<WindowButtonBase>().SetEnabled(false);
                }
                //第二階層のメニューを選択可能とする
                for (var i = 0; i < _characterItem.Count; i++)
                {
                    var select = _characterItem[i].GetComponent<Button>();
                    select.GetComponent<WindowButtonBase>().SetEnabled(true);
                }
                //最終選択したアクターを初期選択状態とする
                for (var i = 0; i < _characterItem.Count; i++)
                    if ((DataManager.Self().GetRuntimeConfigDataModel()?.commandRemember == 0 && i == 0) ||
                        (DataManager.Self().GetRuntimeConfigDataModel()?.commandRemember == 1 &&
                         i == DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.menuActorId))
                    {
                        _characterItem[i].GetComponent<Button>().Select();
                    }
            }
        }

        /// <summary>
        ///     ボタンのイベント入力
        /// </summary>
        /// <param name="obj"></param>
        public void ButtonEvent(GameObject obj) {
            _display = DisplayType.None;
            switch (obj.name)
            {
                case "Item":
                    _display = DisplayType.Item;
                    ItemOpen();
                    break;
                case "Skill":
                    _display = DisplayType.Skill;
                    SelectCharacter();
                    break;
                case "Equipment":
                    _display = DisplayType.Equip;
                    SelectCharacter();
                    break;
                case "Status":
                    _display = DisplayType.Status;
                    SelectCharacter();
                    break;
                case "Sort":
                    _display = DisplayType.Sort;
                    SortOpen();
                    break;
                case "Option":
                    _display = DisplayType.Option;
                    OptionOpen();
                    break;
                case "Save":
                    _display = DisplayType.Save;
                    SaveOpen();
                    break;
                case "End":
                    _display = DisplayType.End;
                    EndOpen();
                    break;
            }
        }

        /// <summary>
        /// メニュー更新
        /// </summary>
        public void UpdateStatus() {
            Init(_menuBase);
        }

        /// <summary>
        /// アイテムWindow表示処理
        /// </summary>
        private void ItemOpen() {
            _menuBase.ItemOpen();
        }

        /// <summary>
        /// アクター選択処理
        /// </summary>
        private void SelectCharacter() {
            _state = MainMenuState.SELECT_ACTOR;
            var selected = false;
            for (var i = 0; i < _characterItem.Count; i++)
            {
                var select = _characterItem[i].GetComponent<Button>();
                var nav = select.navigation;
                nav.mode = Navigation.Mode.Explicit;
                //UIパターンに応じて十字キーを変更する
                if (_pattern == 1 || _pattern == 2 || _pattern == 3 || _pattern == 4)
                {
                    nav.selectOnUp = _characterItem[i == 0 ? _characterItem.Count - 1 : i - 1].GetComponent<Selectable>();
                    nav.selectOnDown = _characterItem[(i + 1) % _characterItem.Count].GetComponent<Selectable>();
                }
                else
                {
                    nav.selectOnLeft = _characterItem[i == 0 ? _characterItem.Count - 1 : i - 1].GetComponent<Selectable>();
                    nav.selectOnRight = _characterItem[(i + 1) % _characterItem.Count].GetComponent<Selectable>();
                }
                select.navigation = nav;
                select.targetGraphic = select.transform.Find("Highlight").GetComponent<Image>();

                //最終選択したアクターを初期選択状態とする
                if ((DataManager.Self().GetRuntimeConfigDataModel()?.commandRemember == 0 && i == 0) ||
                    (DataManager.Self().GetRuntimeConfigDataModel()?.commandRemember == 1 && 
                     i == DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.menuActorId))
                {
                    _characterItem[i].GetComponent<Button>().Select();
                    selected = true;
                }
            }
            if (!selected && _characterItem.Count > 0)
            {
                _characterItem[0].GetComponent<Button>().Select();
            }
            ChangeFocusList();
        }

        /// <summary>
        /// 並べ替え選択時処理
        /// </summary>
        private void SortOpen() {
            _menuBase.SortOpen();
        }

        /// <summary>
        /// オプション表示処理
        /// </summary>
        private void OptionOpen() {
            _menuBase.OptionOpen();
        }

        /// <summary>
        /// セーブ表示処理
        /// </summary>
        private void SaveOpen() {
            _menuBase.SaveOpen();
        }

        /// <summary>
        /// ゲーム終了表示処理
        /// </summary>
        private void EndOpen() {
            _menuBase.EndOpen();
        }

        /// <summary>
        /// アクター選択時処理
        /// </summary>
        /// <param name="obj"></param>
        public void CharacterEvent(GameObject obj) {
            var actorId = obj.GetComponent<CharacterItem>().ActorId();
            switch (_display)
            {
                case DisplayType.Skill:
                    _menuBase.SkillOpen(actorId);
                    break;
                case DisplayType.Equip:
                    _menuBase.EquipmentOpen(actorId);
                    break;
                case DisplayType.Status:
                    _menuBase.StatusOpen(actorId);
                    break;
            }

            //最終選択したアクター情報を保持
            var saveData = DataManager.Self().GetRuntimeSaveDataModel();
            for (var i = 0; i < saveData.runtimeActorDataModels.Count; i++)
            {
                if (actorId == saveData.runtimePartyDataModel.actors[i])
                {
                    saveData.runtimePartyDataModel.menuActorId = i;
                    break;
                }
            }
        }

        private enum DisplayType
        {
            None,
            Skill,
            Equip,
            Status,
            //フォーカス用に追加
            Item,
            Sort,
            Option,
            Save,
            End
        }
    }
}