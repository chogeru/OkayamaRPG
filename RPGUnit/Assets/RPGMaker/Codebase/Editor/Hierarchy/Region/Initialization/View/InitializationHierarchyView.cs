// #define TEST_PREVIE_SCENE_AGING

using RPGMaker.Codebase.Editor.Common.View;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Initialization.View
{
    /// <summary>
    /// 初期設定のHierarchyView
    /// </summary>
    public class InitializationHierarchyView : AbstractHierarchyView
    {
        // const
        //--------------------------------------------------------------------------------------------------------------
        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Hierarchy/Region/Initialization/Asset/database_initializations.uxml"; } }

        // UI要素
        //--------------------------------------------------------------------------------------------------------------
        private Button _battleMenuMainButton;
        private Button _commonSettingButton;
        private Button _environmentButton;
        private Button _gameMenuEquipmentButton;
        private Button _gameMenuItemButton;
        private Button _gameMenuMainButton;
        private Button _gameMenuOptionButton;
        private Button _gameMenuQuitGameButton;
        private Button _gameMenuSaveButton;
        private Button _gameMenuSkillButton;
        private Button _gameMenuSortButton;
        private Button _gameMenuStatusButton;

        private Button _titleButton;
        private Button _optionButton;
        private Button _talkCharacterSelectButton;
        private Button _talkInputNumberButton;
        private Button _talkItemSelectButton;
        private Button _talkSelectionButton;
        private Button _termAbilityParameterButton;
        private Button _termBasicStatusButton;
        private Button _termBattleMessageButton;
        private Button _termCommandButton;
        private Button _termNormalMessageButton;

        private const int foldoutCount = 6;

        //--------------------------------------------------------------------------------------------------------------
        //
        // methods
        //
        //--------------------------------------------------------------------------------------------------------------

        // 初期化・更新系
        //--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="initializationHierarchy"></param>
        public InitializationHierarchyView(InitializationHierarchy initializationHierarchy) {
            InitUI();
        }

        /// <summary>
        /// 各コンテンツデータの初期化
        /// </summary>
        override protected void InitContentsData() {
            _titleButton = UxmlElement.Query<Button>("title_button");
            _commonSettingButton = UxmlElement.Query<Button>("common_setting");
            _gameMenuMainButton = UxmlElement.Query<Button>("game_menu_0");
            _gameMenuItemButton = UxmlElement.Query<Button>("game_menu_1");
            _gameMenuSkillButton = UxmlElement.Query<Button>("game_menu_2");
            _gameMenuEquipmentButton = UxmlElement.Query<Button>("game_menu_3");
            _gameMenuStatusButton = UxmlElement.Query<Button>("game_menu_4");
            _gameMenuSortButton = UxmlElement.Query<Button>("game_menu_5");
            _gameMenuOptionButton = UxmlElement.Query<Button>("game_menu_6");
            _gameMenuSaveButton = UxmlElement.Query<Button>("game_menu_7");
            _gameMenuQuitGameButton = UxmlElement.Query<Button>("game_menu_8");
            _battleMenuMainButton = UxmlElement.Query<Button>("battle_menu_0");
            _talkCharacterSelectButton = UxmlElement.Query<Button>("talk_window_0");
            _talkSelectionButton = UxmlElement.Query<Button>("talk_window_1");
            _talkInputNumberButton = UxmlElement.Query<Button>("talk_window_2");
            _talkItemSelectButton = UxmlElement.Query<Button>("talk_window_3");
            _termBasicStatusButton = UxmlElement.Query<Button>("term_0");
            _termAbilityParameterButton = UxmlElement.Query<Button>("term_1");
            _termCommandButton = UxmlElement.Query<Button>("term_2");
            _termNormalMessageButton = UxmlElement.Query<Button>("term_3");
            _termBattleMessageButton = UxmlElement.Query<Button>("term_4");
            _optionButton = UxmlElement.Query<Button>("option");
            _environmentButton = UxmlElement.Query<Button>("environment");

            //Foldoutの開閉状態保持用
            for (int i = 0; i < foldoutCount; i++)
                SetFoldout("foldout_" + (i + 1));

            InitEventHandlers();
        }

        /// <summary>
        /// イベントの初期設定
        /// </summary>
        private void InitEventHandlers() {
            var buttonAndActions = new List<KeyValuePair<Button, Action>>
            {
                new KeyValuePair<Button, Action>(_titleButton, () => { Inspector.Inspector.TitleView(); }),
                new KeyValuePair<Button, Action>(_commonSettingButton, () => { Inspector.Inspector.UiCommonEditView(); }),
                new KeyValuePair<Button, Action>(_gameMenuMainButton, () => { Inspector.Inspector.GameMenuView(0); }),
                new KeyValuePair<Button, Action>(_gameMenuItemButton, () => { Inspector.Inspector.GameMenuView(1); }),
                new KeyValuePair<Button, Action>(_gameMenuSkillButton, () => { Inspector.Inspector.GameMenuView(2); }),
                new KeyValuePair<Button, Action>(_gameMenuEquipmentButton, () => { Inspector.Inspector.GameMenuView(3); }),
                new KeyValuePair<Button, Action>(_gameMenuStatusButton, () => { Inspector.Inspector.GameMenuView(4); }),
                new KeyValuePair<Button, Action>(_gameMenuSortButton, () => { Inspector.Inspector.GameMenuView(5); }),
                new KeyValuePair<Button, Action>(_gameMenuOptionButton, () => { Inspector.Inspector.GameMenuView(6); }),
                new KeyValuePair<Button, Action>(_gameMenuSaveButton, () => { Inspector.Inspector.GameMenuView(7); }),
                new KeyValuePair<Button, Action>(_gameMenuQuitGameButton, () => { Inspector.Inspector.GameMenuView(8); }),
                new KeyValuePair<Button, Action>(_battleMenuMainButton, () => { Inspector.Inspector.BattleMenuView(); }),
                new KeyValuePair<Button, Action>(_talkCharacterSelectButton, () => { Inspector.Inspector.UiTalkEditView(0); }),
                new KeyValuePair<Button, Action>(_talkSelectionButton, () => { Inspector.Inspector.UiTalkEditView(1); }),
                new KeyValuePair<Button, Action>(_talkInputNumberButton, () => { Inspector.Inspector.UiTalkEditView(2); }),
                new KeyValuePair<Button, Action>(_talkItemSelectButton, () => { Inspector.Inspector.UiTalkEditView(3); }),
                new KeyValuePair<Button, Action>(_termBasicStatusButton, () => { Inspector.Inspector.WordView(0); }),
                new KeyValuePair<Button, Action>(_termAbilityParameterButton, () => { Inspector.Inspector.WordView(1); }),
                new KeyValuePair<Button, Action>(_termCommandButton, () => { Inspector.Inspector.WordView(2); }),
                new KeyValuePair<Button, Action>(_termNormalMessageButton, () => { Inspector.Inspector.WordView(3); }),
                new KeyValuePair<Button, Action>(_termBattleMessageButton, () => { Inspector.Inspector.WordView(4); }),
                new KeyValuePair<Button, Action>(_optionButton, () => { Inspector.Inspector.OptionView(); }),
                new KeyValuePair<Button, Action>(_environmentButton, () => { Inspector.Inspector.EnvironmentEditView(); })
            };

            foreach (var buttonAndAction in buttonAndActions)
            {
                Editor.Hierarchy.Hierarchy.AddSelectableElementAndAction(buttonAndAction.Key, buttonAndAction.Value);
                buttonAndAction.Key.clickable.clicked += () =>
                {
                    Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(buttonAndAction.Key);
                };
            }

#if TEST_PREVIE_SCENE_AGING
            CoreSystem.Helper.DebugUtil.Execution(() =>
            {
                _titleButton.clicked += () =>
                {
                    CoreSystem.Helper.DebugUtil.EditorRepeatExecution(
                   () => { Inspector.Inspector.TitleView(); },
                   "タイトルプレビュー",
                   100,
                   0.1f);
                };
            });
#endif
        }

        // イベントハンドラ
        //--------------------------------------------------------------------------------------------------------------
    }
}