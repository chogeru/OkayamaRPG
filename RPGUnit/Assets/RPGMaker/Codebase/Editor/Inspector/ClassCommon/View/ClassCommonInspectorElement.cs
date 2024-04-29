using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Class;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.ClassCommon.View
{
    /// <summary>
    /// [キャラクター]-[職業の編集]-[共通設定] Inspector
    /// </summary>
    public class ClassCommonInspectorElement : AbstractInspectorElement
    {
        private const int MAX_LEVEL_MIN     = 2;
        private const int MAX_LEVEL_MAX     = 99;

        private const int CLEAR_LEVEL_MIN     = 3;
        private const int CLEAR_LEVEL_MAX     = MAX_LEVEL_MAX - 1;

        private const int MAX_EXP_MIN     = 1000000;
        private const int MAX_EXP_MAX     = 9999999;

        private const int MAX_HP_MIN     = 200;
        private const int MAX_HP_MAX     = 9999;    //仕様によりHP上限を9999にする

        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/ClassCommon/Asset/inspector_job_common.uxml"; } }
        private          List<ClassDataModel>      _classDataModels;

        public ClassCommonInspectorElement() {
            Refresh();
        }

        override protected void RefreshContents() {
            base.RefreshContents();
            _classDataModels = databaseManagementService.LoadClassCommon();
            Initialize();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();

            //以下ロード処理
            for (var i = 0; i < _classDataModels.Count; i++)
            {
                var num = i;
                IntegerField maxLevelIntField = RootContainer.Query<IntegerField>("max_level");
                maxLevelIntField.value = _classDataModels[i].maxLevel;
                BaseInputFieldHandler.IntegerFieldCallback(maxLevelIntField, o =>
                {
                    IntegerField clearLevelIntField = RootContainer.Query<IntegerField>("clear_level");
                    if (clearLevelIntField.value >= maxLevelIntField.value)
                        clearLevelIntField.value = maxLevelIntField.value - 1;

                    _classDataModels[num].maxLevel = maxLevelIntField.value;
                    databaseManagementService.SaveClassCommon(_classDataModels);
                }, MAX_LEVEL_MIN, MAX_LEVEL_MAX);

                IntegerField clearLevelIntField = RootContainer.Query<IntegerField>("clear_level");
                clearLevelIntField.value = _classDataModels[i].clearLevel;
                BaseInputFieldHandler.IntegerFieldCallback(clearLevelIntField, o =>
                {
                    if (clearLevelIntField.value >= _classDataModels[num].maxLevel - 1)
                        clearLevelIntField.value = _classDataModels[num].maxLevel - 1;
                    _classDataModels[num].clearLevel = clearLevelIntField.value;
                    databaseManagementService.SaveClassCommon(_classDataModels);
                }, CLEAR_LEVEL_MIN, CLEAR_LEVEL_MAX);

                IntegerField expGainIncreaseIntField = RootContainer.Query<IntegerField>("exp_gain_increase_value");
                expGainIncreaseIntField.value = _classDataModels[i].expGainIncreaseValue;
                BaseInputFieldHandler.IntegerFieldCallback(expGainIncreaseIntField, o =>
                {
                    _classDataModels[num].expGainIncreaseValue = expGainIncreaseIntField.value;
                    databaseManagementService.SaveClassCommon(_classDataModels);
                }, MAX_EXP_MIN, MAX_EXP_MAX);

                IntegerField baseHpMaxIntField = RootContainer.Query<IntegerField>("base_hp_max_value");
                baseHpMaxIntField.value = _classDataModels[i].baseHpMaxValue;
                BaseInputFieldHandler.IntegerFieldCallback(baseHpMaxIntField, o =>
                {
                    _classDataModels[num].baseHpMaxValue = baseHpMaxIntField.value;
                    databaseManagementService.SaveClassCommon(_classDataModels);
                }, MAX_HP_MIN, MAX_HP_MAX);

                //ステータスの有効化
                //各種データの順番
                var statusEnabled = new List<int>
                {
                    _classDataModels[num].basic.abilityEnabled.mp,
                    _classDataModels[num].basic.abilityEnabled.tp,
                    _classDataModels[num].basic.abilityEnabled.magicAttack,
                    _classDataModels[num].basic.abilityEnabled.magicDefense,
                    _classDataModels[num].basic.abilityEnabled.speed,
                    _classDataModels[num].basic.abilityEnabled.luck
                };
                for (var j = 0; j < RootContainer.Query<Toggle>("status_toggle").ToList().Count; j++)
                {
                    var count = j;

                    //各トグルの取得
                    var statusToggle = RootContainer.Query<Toggle>("status_toggle").AtIndex(count);
                    //初期値
                    statusToggle.value = statusEnabled[count] == 1;
                    statusToggle.RegisterValueChangedCallback(evt =>
                    {
                        switch (count)
                        {
                            case 0:
                                _classDataModels[num].basic.abilityEnabled.mp = statusToggle.value ? 1 : 0;
                                break;
                            case 1:
                                _classDataModels[num].basic.abilityEnabled.tp = statusToggle.value ? 1 : 0;
                                break;
                            case 2:
                                _classDataModels[num].basic.abilityEnabled.magicAttack = statusToggle.value ? 1 : 0;
                                break;
                            case 3:
                                _classDataModels[num].basic.abilityEnabled.magicDefense = statusToggle.value ? 1 : 0;
                                break;
                            case 4:
                                _classDataModels[num].basic.abilityEnabled.speed = statusToggle.value ? 1 : 0;
                                break;
                            case 5:
                                _classDataModels[num].basic.abilityEnabled.luck = statusToggle.value ? 1 : 0;
                                break;
                        }

                        statusEnabled[count] = statusToggle.value ? 1 : 0;

                        databaseManagementService.SaveClassCommon(_classDataModels);
                    });
                }
            }
        }
    }
}