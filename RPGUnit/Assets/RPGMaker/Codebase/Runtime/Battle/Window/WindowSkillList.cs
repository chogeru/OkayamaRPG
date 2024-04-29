using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPGMaker.Codebase.Runtime.Battle.Window
{
    /// <summary>
    /// [スキル]の選択ウィンドウ
    /// </summary>
    public class WindowSkillList : WindowSelectable
    {
        /// <summary>
        /// アクター
        /// </summary>
        private GameActor _actor;
        /// <summary>
        /// スキルのGameObjectのオリジナル
        /// </summary>
        private GameObject _battleSkillSelector;
        /// <summary>
        /// スキルの配列
        /// </summary>
        private List<GameItem> _data;
        /// <summary>
        /// アイテム総数
        /// </summary>
        private int _itemCount;
        /// <summary>
        /// スキル総数（Refreshで初期化されるため保持する）
        /// </summary>
        private int _skillCount;
        /// <summary>
        /// スキルタイプID
        /// </summary>
        private int _stypeId;

        /// <summary>
        /// 初期化
        /// </summary>
        public override void Initialize() {
            base.Initialize();
            _actor = null;
            _stypeId = 0;
            _data = new List<GameItem>();
            _battleSkillSelector = gameObject.transform.Find("WindowArea/Scroll View/Viewport/List/SkillSelector1")
                .gameObject;
            _itemCount = 1; // 総数（複製元の分で初期値は1）
            _skillCount = 0;
        }

        /// <summary>
        /// 対象となるアクターを設定
        /// </summary>
        /// <param name="actor"></param>
        public void SetActor(GameActor actor) {
            if (_actor != actor)
            {
                _actor = actor;
            }
            Refresh();
        }

        /// <summary>
        /// スキルタイプIDを設定
        /// </summary>
        /// <param name="stypeId"></param>
        public void SetStypeId(int stypeId) {
            if (_stypeId != stypeId)
            {
                _stypeId = stypeId;
            }
            Refresh();
        }

        /// <summary>
        /// ウィンドウが持つ最大項目数を返す
        /// </summary>
        /// <returns></returns>
        public override int MaxItems() {
            return _data?.Count ?? 1;
        }

        /// <summary>
        /// 現在選択中のスキルを返す
        /// </summary>
        /// <returns></returns>
        public GameItem Item() {
            return _data != null && Index() >= 0 ? _data.ElementAtOrDefault(Index()) : null;
        }

        /// <summary>
        /// 指定したスキルが含まれるか
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Includes(GameItem item) {
            return item != null && item.STypeId == _stypeId;
        }

        /// <summary>
        /// 指定したスキルが利用可能か
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool IsEnabled(GameItem item) {
            return _actor != null && _actor.CanUse(item);
        }

        /// <summary>
        /// 項目のリストを作成
        /// </summary>
        public void MakeItemList() {
            _data = new List<GameItem>();

            if (_actor != null)
                _data = _actor.Skills().Aggregate(new List<GameItem>(), (l, skillData) =>
                {
                    var item = new GameItem(skillData.basic.id, GameItem.DataClassEnum.Skill);
                    if (Includes(item)) l.Add(item);

                    return l;
                });
            else
                _data = new List<GameItem>();
        }

        /// <summary>
        /// 指定した番号の項目を削除
        /// </summary>
        public override void ClearItem() {
            base.ClearItem();
            _itemCount = 1;
        }

        /// <summary>
        /// 前に選択したものを選択
        /// </summary>
        public void SelectLast() {
            GameItem skill = null;
            if (DataManager.Self().GetGameParty().InBattle())
                skill = _actor.LastBattleSkill();
            else
                skill = _actor.LastMenuSkill();

            var index = _data.IndexOf(skill);
            Select(index >= 0 ? index : 0);
        }

        /// <summary>
        /// 指定番号の項目を描画
        /// </summary>
        /// <param name="index"></param>
        public override void DrawItem(int index) {
            // アイテム作成
            MakeItem();

            if (selectors.Count <= index)
                return;

            if (_data.ElementAtOrDefault(index) != null)
            {
                var skill = _data[index];

                _iconImage = selectors[index].gameObject.transform.Find("Icon").GetComponent<Image>();
                var tex = GetItemImage(_data[index].Icon);
                _iconImage.gameObject.SetActive(tex != null);
                _iconImage.sprite = tex;

                selectors[index].SetUp(index, _data[index].Name, Select, OnClickSelection);

                var skillValue = selectors[index].gameObject.transform.Find("SkillValue").gameObject
                    .GetComponent<TextMeshProUGUI>();

                var btnColor = selectors[index].GetComponent<Button>();

                var btnColorColors = btnColor.colors;
                btnColorColors.disabledColor = new Color(1f, 1f, 1f);
                btnColorColors.normalColor = new Color(1f, 1f, 1f);
                btnColorColors.selectedColor = new Color(1f, 1f, 1f, 0.5f);
                btnColorColors.pressedColor = new Color(1f, 1f, 1f);
                btnColorColors.highlightedColor = new Color(1f, 1f, 1f);
                btnColor.colors = btnColorColors;


                if (_actor.SkillTpCost(skill) == 0 && _actor.SkillMpCost(skill) == 0)
                {
                    skillValue.text = "0";
                    skillValue.color = new Color(1f, 1f, 1f);
                }
                else if (_actor.SkillTpCost(skill) > 0)
                {
                    skillValue.text = _data[index].TpCost.ToString();
                    // コスト設定
                    skillValue.color = new Color(0f, 1f, 0f);
                }
                else if (_actor.SkillMpCost(skill) > 0)
                {
                    skillValue.text = _actor.SkillMpCost(skill).ToString();
                    skillValue.color = new Color(0f, 0f, 1f);
                }

                if (!IsEnabled(_data[index]))
                {
                    btnColorColors = btnColor.colors;
                    btnColorColors.disabledColor = new Color(0.5f, 0.5f, 0.5f);
                    btnColorColors.normalColor = new Color(0.5f, 0.5f, 0.5f);
                    btnColorColors.selectedColor = new Color(0.5f, 0.5f, 0.5f);
                    btnColorColors.pressedColor = new Color(0.5f, 0.5f, 0.5f);
                    btnColorColors.highlightedColor = new Color(0.5f, 0.5f, 0.5f);
                    btnColor.colors = btnColorColors;
                }
            }
        }

        /// <summary>
        /// ヘルプウィンドウをアップデート
        /// </summary>
        public override void UpdateHelp() {
            SetHelpWindowItem(Item());
        }

        /// <summary>
        /// コンテンツの再描画
        /// </summary>
        public new void Refresh() {
            //古いアイテムを全て消す
            for (int i = 0; i < selectors.Count; i++)
            {
                if (selectors[i].gameObject != _battleSkillSelector)
                {
                    DestroyImmediate(selectors[i].gameObject);
                    selectors.RemoveAt(i);
                    i--;
                }
                else
                {
                    //一旦グレーを解除
                    selectors[i].GetComponent<WindowButtonBase>().SetGray(false);
                }

                if (selectors.Count == 1)
                    break;
            }

            _itemCount = 1;

            //リスト生成
            MakeItemList();
            DrawAllItems();
            // アイテム表示切替
            SetActiveWindow();
            //ハイライト初期化
            for (int i = 0; i < selectors.Count; i++)
            {
                selectors[i].GetComponent<WindowButtonBase>().SetHighlight(i == 0);
            }

            //グレー表示への対応
            for (int i = 0; i < _data.Count; i++)
            {
                bool flg = IsEnabled(_data[i]);
                if (!flg)
                {
                    selectors[i].GetComponent<WindowButtonBase>().SetGray(true);
                }
                else
                {
                    selectors[i].GetComponent<WindowButtonBase>().SetGray(false);
                }
            }

            //件数が1件しかない場合の処理
            if (_data == null || _data.Count == 0)
            {
                selectors[0].SetUp(0, "", null, null);
                var skillValue = selectors[0].gameObject.transform.Find("SkillValue").gameObject
                    .GetComponent<TextMeshProUGUI>();
                skillValue.text = "";
                selectors[0].gameObject.transform.Find("Icon").gameObject.SetActive(false);
            }

            // ナビゲーションの設定
            SetNavigation();
            RefreshAft();
        }

        /// <summary>
        /// コンテンツの再描画の後、若干待ってから実行する処理
        /// </summary>
        private async void RefreshAft() {
            await Task.Delay(100);

            //ボタンの有効状態切り替え
            //最終選択したスキルを選択状態とする
            GameItem skill;
            if (DataManager.Self().GetGameParty().InBattle())
                skill = _actor.LastBattleSkill();
            else
                skill = _actor.LastMenuSkill();

            var index = 0;
            if (DataManager.Self().GetRuntimeConfigDataModel()?.commandRemember != null && DataManager.Self().GetRuntimeConfigDataModel()?.commandRemember == 1)
            {
                for (int i = 0; i < _data.Count; i++)
                {
                    if (_data[i].ItemId == skill?.ItemId)
                    {
                        index = i;
                        break;
                    }
                }
            }

            bool flg = false;
            for (int i = 0; i < selectors.Count; i++)
            {
                selectors[i].GetComponent<WindowButtonBase>().SetEnabled(true);
                //元々フォーカスが当たっていた場所に、フォーカスしなおし
                if (i == index)
                {
                    flg = true;
                    selectors[i].GetComponent<Button>().Select();
                }
                else
                {
                    selectors[i].GetComponent<WindowButtonBase>().SetHighlight(false);
                }
            }

            if (!flg)
            {
                selectors[0].GetComponent<Button>().Select();
            }
        }

        /// <summary>
        /// アイテムの配列( _data )を生成
        /// </summary>
        private void MakeItem() {
            // スキル数を取得
            _skillCount = _data.Count;

            // 現在のアイテム数が超えていれば
            while (_itemCount <= _skillCount)
            {
                _itemCount++;
                var gameObject = Instantiate(_battleSkillSelector, _battleSkillSelector.transform.parent, false);
                gameObject.name = "SkillSelector" + _itemCount;
                selectors.Add(gameObject.GetComponent<Selector>());
            }
        }

        /// <summary>
        /// アイテム表示切替
        /// </summary>

        private void SetActiveWindow() {
            for (var i = 0; i < _itemCount; i++)
                if (i < _skillCount)
                    selectors[i].gameObject.SetActive(true);
                else
                    selectors[i].gameObject.SetActive(false);

            // ボタンを選択状態にする
            if (_battleSkillSelector != null)
                EventSystem.current.SetSelectedGameObject(_battleSkillSelector);
        }

        /// <summary>
        /// ナビゲーションを設定する
        /// </summary>
        public void SetNavigation() {
            var selects = gameObject.transform.Find("WindowArea/Scroll View/Viewport/List").gameObject
                .GetComponentsInChildren<Selectable>();
            for (var i = 0; i < selects.Length; i++)
            {
                var nav = selects[i].navigation;
                nav.mode = Navigation.Mode.Explicit;

                nav.selectOnRight = selects[i < selects.Length - 1 ? i + 1 : i];
                nav.selectOnLeft = selects[i == 0 ? i : i - 1];
                nav.selectOnDown = selects[i + 2 < selects.Length ? i + 2 : i + 1 < selects.Length ? i + 1 : i];
                nav.selectOnUp = selects[i - 2 <= -1 ? i : i - 2];
                selects[i].navigation = nav;
            }
        }

        /// <summary>
        /// ウィンドウを表示
        /// </summary>
        public override void Show() {
            base.Show();

            //ボタンの有効状態切り替え
            //最終選択したスキルを選択状態とする
            GameItem skill;
            if (DataManager.Self().GetGameParty().InBattle())
                skill = _actor.LastBattleSkill();
            else
                skill = _actor.LastMenuSkill();

            var index = 0;
            if (DataManager.Self().GetRuntimeConfigDataModel()?.commandRemember != null && DataManager.Self().GetRuntimeConfigDataModel()?.commandRemember == 1)
            {
                for (int i = 0; i < _data.Count; i++)
                {
                    if (_data[i].ItemId == skill?.ItemId)
                    {
                        index = i;
                        break;
                    }
                }
            }

            bool flg = false;
            for (int i = 0; i < selectors.Count; i++)
            {
                selectors[i].GetComponent<WindowButtonBase>().SetEnabled(true);
                //元々フォーカスが当たっていた場所に、フォーカスしなおし
                if (i == index)
                {
                    flg = true;
                    selectors[i].GetComponent<Button>().Select();
                }
                else
                {
                    selectors[i].GetComponent<WindowButtonBase>().SetHighlight(false);
                }
            }

            if (!flg)
            {
                selectors[0].GetComponent<Button>().Select();
            }
        }

        /// <summary>
        /// ウィンドウを非表示(閉じるわけではない)
        /// </summary>
        public override void Hide() {
            //ボタンの有効状態切り替え
            for (int i = 0; i < selectors.Count; i++)
            {
                selectors[i].GetComponent<WindowButtonBase>().SetEnabled(false);
            }

            base.Hide();
        }

    }
}