using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Class;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.Editor.Inspector.CharacterClass.View;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.StatusGraph.View
{
    /// <summary>
    /// [キャラクター]-[職業の編集] のInspector内の、[能力値]グラフ
    /// </summary>
    public class StatusGraphElement : VisualElement
    {
        // 能力タイプ
        public enum Type
        {
            Hp,
            Mp,
            Attack,
            Defense,
            MagicAttack,
            MagicDefence,
            Speed,
            Luck,
            Max
        }

        // グラフカラー配列（並びはTypeに準拠）
        private static readonly Color[] _graphColors =
        {
            new Color(1, 0.8f, 0.4f),
            new Color(0.3f, 0.6f, 1.0f),
            new Color(1.0f, 0.3f, 0.4f),
            new Color(0.3f, 0.8f, 0.5f),
            new Color(1.0f, 0.3f, 1.0f),
            new Color(0.0f, 0.7f, 0.0f),
            new Color(0.0f, 0.8f, 1.0f),
            new Color(1.0f, 0.9f, 0.3f)
        };

        private readonly Button _graph;
        private readonly int    _paramMaxLv;

        private readonly List<ClassDataModel> _classDataModels;

        private readonly DatabaseManagementService _databaseManagementService;
        private          int                       _paramGrow;
        private          int                       _paramMax;

        // 計算に使用する値
        private int _paramOne;
        private int _paramPeakLv;

        // タイプ
        private Type                     _tabType;
        private ClassDataModel.Parameter parameters;

        public StatusGraphElement() {
            _databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;
            var treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/RPGMaker/Codebase/Editor/Inspector/StatusGraph/Asset/StatusGraphElement.uxml");
            var container = treeAsset.Instantiate();
            hierarchy.Add(container);


            _classDataModels = _databaseManagementService.LoadClassCommon();

            // 最大レベル設定読み出し
            for (var i = 0; i < _classDataModels.Count; i++) _paramMaxLv = _classDataModels[i].maxLevel;

            _graph = container.Query<Button>("graph");
        }

        public void SetParamOne(int param) {
            _paramOne = param;
        }

        public void SetParamMax(int param) {
            _paramMax = param;
        }

        public void SetParamPeakLv(int param) {
            _paramPeakLv = param;
        }

        public void SetParamGrow(int param) {
            _paramGrow = param;
        }

        // グラフ初期化
        public void InitStatusGraph(
            ClassDataModel.Parameter parameters,
            Type type
        ) {
            // タブを設定
            _tabType = type;

            this.parameters = parameters;
            UpDateGraph(parameters);
        }

        // 値の設定、グラフ更新
        private void SetStatusValue(List<int> pram, VisualElement element, Color graphColor) {
            element.Clear();

            // グラフの計算
            var clearParam = 0;

            for (var i = 1; i <= _paramMaxLv; i++)
            {
                if (_paramOne == _paramMax)
                {
                    pram[i] = _paramOne;
                    continue;
                }

                double x;

                // ピークレベルで計算を分ける
                if (i > _paramPeakLv)
                {
                    // オートガイド資料に記載
                    var a = clearParam;
                    var b = _paramMax;

                    var n1 = a + (b - a) * (i - _paramPeakLv) / (_paramMaxLv - _paramPeakLv);
                    var n2 = a + (b - a) * (i - _paramPeakLv) * (i - _paramPeakLv) / (_paramMaxLv - _paramPeakLv) /
                        (_paramMaxLv - _paramPeakLv);
                    x = Math.Ceiling((double) (n2 * _paramGrow + n1 * (10 - _paramGrow)) / 10);
                }
                else
                {
                    // オートガイド資料に記載
                    var a = _paramOne;
                    var b = _paramMax * (_paramPeakLv / _paramMaxLv * 0.15 + 0.85);
                    var n1 = a + (b - a) * (i - 1) / (_paramPeakLv - 1);
                    var n2 = a + (b - a) * (i - 1) * (i - 1) / (_paramPeakLv - 1) / (_paramPeakLv - 1);
                    x = (n2 * _paramGrow + n1 * (10 - _paramGrow)) / 10;

                    if (i == _paramPeakLv)
                        clearParam = (int) x;
                }

                pram[i] = (int) x;
            }

            var expandRate = (float) CharacterClassInspectorElement.PARAM_MAX_VALUE[(int) _tabType] /
                             pram[pram.Count - 1] /
                             ((float) CharacterClassInspectorElement.PARAM_MAX_VALUE[(int) _tabType] / 100);

            for (var i = 0; i < _classDataModels[0].maxLevel - 1; i++)
            {
                var bar = new VisualElement();
                bar.style.backgroundColor = new StyleColor(graphColor);
                bar.style.height = new StyleLength(new Length(pram[i] * expandRate, LengthUnit.Percent));
                bar.style.width = new StyleLength(new Length(1, LengthUnit.Percent));
                element.Add(bar);
            }

            _databaseManagementService.SaveClassCommon(_classDataModels);
        }

        // グラフの更新
        public void UpDateGraph(
            ClassDataModel.Parameter parameters
        ) {
            switch (_tabType)
            {
                case Type.Hp:
                    SetStatusValue(parameters.maxHp, _graph, _graphColors[(int) _tabType]);
                    break;

                case Type.Mp:
                    SetStatusValue(parameters.maxMp, _graph, _graphColors[(int) _tabType]);
                    break;

                case Type.Attack:
                    SetStatusValue(parameters.attack, _graph, _graphColors[(int) _tabType]);
                    break;

                case Type.Defense:
                    SetStatusValue(parameters.defense, _graph, _graphColors[(int) _tabType]);
                    break;

                case Type.MagicAttack:
                    SetStatusValue(parameters.magicAttack, _graph, _graphColors[(int) _tabType]);
                    break;

                case Type.MagicDefence:
                    SetStatusValue(parameters.magicDefense, _graph, _graphColors[(int) _tabType]);
                    break;

                case Type.Speed:
                    SetStatusValue(parameters.speed, _graph, _graphColors[(int) _tabType]);
                    break;

                case Type.Luck:
                    SetStatusValue(parameters.luck, _graph, _graphColors[(int) _tabType]);
                    break;
            }
        }

        public new class UxmlFactory : UxmlFactory<StatusGraphElement, UxmlTraits>
        {
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc) {
                base.Init(ve, bag, cc);
            }
        }
    }
}