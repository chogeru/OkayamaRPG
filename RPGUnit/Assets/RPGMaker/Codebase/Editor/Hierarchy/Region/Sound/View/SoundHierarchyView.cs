using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.Inspector.Sound.View;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Sound.View
{
    /// <summary>
    ///     サウンド設定
    /// </summary>
    public class SoundHierarchyView : AbstractHierarchyView
    {
        // const
        //--------------------------------------------------------------------------------------------------------------
        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Hierarchy/Region/Sound/Asset/database_sounds.uxml"; } }

        // UI要素
        //--------------------------------------------------------------------------------------------------------------
        private List<Button> _bgmButtons;
        private List<Button> _seButtons;
        private const int foldoutCount = 3;

        // ヒエラルキー本体クラス
        //--------------------------------------------------------------------------------------------------------------
        private readonly SoundHierarchy _soundHierarchy;

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
        /// <param name="soundHierarchy"></param>
        public SoundHierarchyView(SoundHierarchy soundHierarchy) {
            _soundHierarchy = soundHierarchy;
            InitUI();
            InitEventHandlers();
        }

        /// <summary>
        /// 各コンテンツデータの初期化
        /// </summary>
        override protected void InitContentsData() {
            _bgmButtons = new List<Button>();
            _seButtons = new List<Button>();

            for (var i = 0; i < System.Enum.GetValues(typeof(SoundInspectorElement.BgmList)).Length; i++)
            {
                Button button = UxmlElement.Query<Button>("BGM_" + i);
                button.name = "BGM_" + i.ToString();
                _bgmButtons.Add(button);
            }

            for (var i = 0; i < System.Enum.GetValues(typeof(SoundInspectorElement.SeList)).Length; i++)
            {
                Button button = UxmlElement.Query<Button>("SE_" + i);
                button.name = "SE_" + i.ToString();
                _seButtons.Add(button);
            }

            //Foldoutの開閉状態保持用
            for (int i = 0; i < foldoutCount; i++)
                SetFoldout("foldout_" + (i + 1));
        }

        /// <summary>
        /// イベントの初期設定
        /// </summary>
        private void InitEventHandlers() {
            _bgmButtons.ForEach(button =>
            {
                Editor.Hierarchy.Hierarchy
                    .AddSelectableElementAndAction(button,
                        () => { _soundHierarchy.OpenBgmInspector(int.Parse(button.name.Substring(4))); });
                button.clickable.clicked += () => { Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(button); };
            });
            _seButtons.ForEach(button =>
            {
                Editor.Hierarchy.Hierarchy
                    .AddSelectableElementAndAction(button,
                        () => { _soundHierarchy.OpenSeInspector(int.Parse(button.name.Substring(3))); });
                button.clickable.clicked += () => { Editor.Hierarchy.Hierarchy.InvokeSelectableElementAction(button); };
            });
        }
    }
}