using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Class;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.Editor.Common.AutoguideHelper;

namespace RPGMaker.Codebase.Editor.Inspector.StatusAutoGuide.View
{
    /// <summary>
    /// [キャラクター]-[職業の編集] のInspector内の、[能力値]-[オートガイド]
    /// </summary>
    public class StatusAutoGuideElement : VisualElement
    {
        // 定数
        private const    int          LIMIT_MIN_HP_MP     = -5;
        private const    int          LIMIT_MAX_HP_MP     = 5;
        private const    int          MAX_AUTO_GUID_POINT = 30;
        private const    int          LIMIT_MIN_POINT     = 0;
        private const    int          LIMIT_MAX_POINT     = 15;
        private readonly IntegerField _integer_hp_mp;

        // パラメータの残り値
        private readonly IntegerField _integer_max_param;

        // スライダーリストの値表示箇所
        private readonly List<IntegerField> _integer_param = new List<IntegerField>();

        // HP/MP
        private readonly Slider _slider_hp_mp;

        // スライダーのリスト
        // 順番は攻撃、防御、魔力、魔防、敏捷、運の順
        private readonly List<Slider> _slider_param = new List<Slider>();
        private readonly int[]        _auto_guid_param;

        private          List<AutoGuideDataModel>  _autoGuideDataModels;
        private readonly List<ClassDataModel>      _classDataModels;
        private readonly DatabaseManagementService _databaseManagementService;

        private ClassDataModel.AutoGuide levelMaxParameter = ClassDataModel.AutoGuide.CreateDefault();
        private ClassDataModel.AutoGuide levelMinParameter = ClassDataModel.AutoGuide.CreateDefault();

        public int statusPoint;

        public StatusAutoGuideElement() {
            var treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Assets/RPGMaker/Codebase/Editor/Inspector/StatusAutoGuide/Asset/StatusAutoGuideElement.uxml");
            var container = treeAsset.Instantiate();
            hierarchy.Add(container);
            _databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;

            // データロード
            _classDataModels = _databaseManagementService.LoadClassCommon();
            _autoGuideDataModels = _databaseManagementService.LoadAutoGuide();

            // 割振の残値初期化
            _auto_guid_param = new int[container.Query<Slider>("auto_guid_slider").ToList().Count];

            // 割振の残値
            _integer_max_param = container.Query<IntegerField>("max_integer");
            _integer_max_param.value = MAX_AUTO_GUID_POINT;
            _integer_max_param.isReadOnly = true;

            // HP/MPのパラメータ
            _slider_hp_mp = container.Query<Slider>("hp_mp_slider");
            _integer_hp_mp = container.Query<IntegerField>("hp_mp_integer");

            _slider_hp_mp.lowValue = LIMIT_MIN_HP_MP;
            _slider_hp_mp.highValue = LIMIT_MAX_HP_MP;
            _slider_hp_mp.value = 0;
            _integer_hp_mp.value = 0;

            _slider_hp_mp.RegisterValueChangedCallback(evt => { _integer_hp_mp.value = (int) _slider_hp_mp.value; });
            _integer_hp_mp.RegisterValueChangedCallback(evt =>
            {
                _slider_hp_mp.value = _integer_hp_mp.value;
            });

            // オートガイドパラメータのelement取得（攻撃～運）
            for (var i = 0; i < container.Query<Slider>("auto_guid_slider").ToList().Count; i++)
            {
                var count = i;

                // スライダー取得
                _slider_param.Add(container.Query<Slider>("auto_guid_slider").AtIndex(i));
                // スライダーの値表示箇所取得
                _integer_param.Add(container.Query<IntegerField>("auto_guid_integer").AtIndex(i));

                // 値設定
                _slider_param[i].lowValue = LIMIT_MIN_POINT;
                _slider_param[i].highValue = LIMIT_MAX_POINT;
                _slider_param[i].value = LIMIT_MIN_POINT;

                _integer_param[i].value = int.Parse(_slider_param[i].value.ToString());
                _integer_param[i].maxLength = LIMIT_MAX_POINT;
                _auto_guid_param[i] = _integer_param[i].value;

                // コールバック登録(スライダー)
                _slider_param[i].RegisterValueChangedCallback(evt =>
                {
                    if (CheckMaxStatusPoint())
                    {
                        _slider_param[count].value = _integer_param[count].value + _integer_max_param.value;
                        _integer_param[count].value = _integer_param[count].value + _integer_max_param.value;
                        _integer_max_param.value = MAX_AUTO_GUID_POINT - TotalScore();
                        return;
                    }

                    _integer_param[count].value = (int) float.Parse(_slider_param[count].value.ToString());
                    _auto_guid_param[count] = _integer_param[count].value;
                    _integer_max_param.value = MAX_AUTO_GUID_POINT - TotalScore();
                });

                // コールバック登録(直接入力)
                _integer_param[i].RegisterValueChangedCallback(evt =>
                {
                    if (_integer_param[count].value > LIMIT_MAX_POINT) _integer_param[count].value = LIMIT_MAX_POINT;

                    if (CheckMaxStatusPoint())
                    {
                        _integer_param[count].value = (int) _slider_param[count].value + _integer_max_param.value;
                        _slider_param[count].value = _slider_param[count].value + _integer_max_param.value;
                        _integer_max_param.value = MAX_AUTO_GUID_POINT - TotalScore();
                        return;
                    }

                    _slider_param[count].value = _integer_param[count].value;
                    _auto_guid_param[count] = (int) _slider_param[count].value;
                    _integer_max_param.value = MAX_AUTO_GUID_POINT - TotalScore();
                });
            }
        }

        public void InitStatusGuide(
            ClassDataModel.AutoGuide autoGuide,
            ClassDataModel.Parameter parameters
        ) {
            // 代入数するパラメータ
            int[] param =
            {
                autoGuide.attack,
                autoGuide.defense,
                autoGuide.magicAttack,
                autoGuide.magicDefense,
                autoGuide.speed,
                autoGuide.luck
            };

            _slider_hp_mp.value = autoGuide.maxHp;
            _integer_hp_mp.value = autoGuide.maxHp;

            // パラメータ初期化
            for (var i = 0; i < _slider_param.Count; i++)
            {
                _slider_param[i].value = param[i];
                _integer_param[i].value = param[i];
                _auto_guid_param[i] = param[i];
            }

            // 割り振り最大値を超えていたらデフォルト値を入れる
            if (CheckMaxStatusPoint())
                for (var i = 0; i < _slider_param.Count; i++)
                {
                    _slider_param[i].value = 0;
                    _integer_param[i].value = 0;
                    _auto_guid_param[i] = 0;
                }

            // 割り振りパラメータ
            _integer_max_param.value = MAX_AUTO_GUID_POINT - TotalScore();
            SetBaseParameter();
        }

        // オートガイドを実行してパラメーターを返す
        public int[,] CreateParameter() {
            //オートガイドが参照する項目の取得
            var classDataModel = _databaseManagementService.LoadClassCommon()[0];
            var clearLevel = classDataModel.clearLevel; //クリアレベル
            var maxLevel = classDataModel.maxLevel; //最大レベル
            var expGainIncreaseValue = classDataModel.expGainIncreaseValue; //経験値の上限
            var maxHp = classDataModel.baseHpMaxValue;

            //オートガイド共通計算用の、標準モデル
            var standardModel = CalcClassModel(
                maxLevel,
                clearLevel,
                expGainIncreaseValue,
                maxHp,
                (int) float.Parse(_slider_hp_mp.value.ToString()),
                (int) float.Parse(_slider_param[0].value.ToString()),
                (int) float.Parse(_slider_param[1].value.ToString()),
                (int) float.Parse(_slider_param[2].value.ToString()),
                (int) float.Parse(_slider_param[3].value.ToString()),
                (int) float.Parse(_slider_param[4].value.ToString()),
                (int) float.Parse(_slider_param[5].value.ToString())
            );

            // パラメータ分作成（最大値、最小値）
            var param = new int[8, 2];
            param[0, 0] = standardModel.maxHp;
            param[0, 1] = standardModel.minHp;
            param[1, 0] = standardModel.maxMp;
            param[1, 1] = standardModel.minMp;
            param[2, 0] = standardModel.maxAttack;
            param[2, 1] = standardModel.minAttack;
            param[3, 0] = standardModel.maxDefense;
            param[3, 1] = standardModel.minDefense;
            param[4, 0] = standardModel.maxMagic;
            param[4, 1] = standardModel.minMagic;
            param[5, 0] = standardModel.maxMagicDefense;
            param[5, 1] = standardModel.minMagicDefense;
            param[6, 0] = standardModel.maxSpeed;
            param[6, 1] = standardModel.minSpeed;
            param[7, 0] = standardModel.maxLuck;
            param[7, 1] = standardModel.minLuck;

            return param;
        }

        private void SetBaseParameter() {
            levelMinParameter = AutoGuideRepository.GetLevelParameter(
                _classDataModels[0].baseHpMaxValue, _classDataModels[0].clearLevel,
                1);

            levelMaxParameter = AutoGuideRepository.GetLevelParameter(
                _classDataModels[0].baseHpMaxValue, _classDataModels[0].clearLevel,
                _classDataModels[0].maxLevel);
        }

        // パラメータ最大値チェック
        private bool CheckMaxStatusPoint() {
            return TotalScore() > MAX_AUTO_GUID_POINT;
        }

        private int TotalScore() {
            var totalScore = 0;

            for (var i = 0; i < _slider_param.Count; i++) totalScore += (int) _slider_param[i].value;

            return totalScore;
        }

        public int[] GetAutoGuideParams() {
            return new int[]
            {
                (int) float.Parse(_slider_hp_mp.value.ToString()),
                (int) float.Parse(_slider_param[0].value.ToString()),
                (int) float.Parse(_slider_param[1].value.ToString()),
                (int) float.Parse(_slider_param[2].value.ToString()),
                (int) float.Parse(_slider_param[3].value.ToString()),
                (int) float.Parse(_slider_param[4].value.ToString()),
                (int) float.Parse(_slider_param[5].value.ToString())
            };
        }

        public new class UxmlFactory : UxmlFactory<StatusAutoGuideElement, UxmlTraits>
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