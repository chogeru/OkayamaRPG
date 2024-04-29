using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using System;

namespace RPGMaker.Codebase.Runtime.Common.Component.Hud.Party
{
    public class PartyItem
    {
        // 状態プロパティ
        //--------------------------------------------------------------------------------------------------------------
        //所持数の上限下限
        private readonly int _maxValue = 9999;
        private readonly int _minValue = 0;

        // データプロパティ
        //--------------------------------------------------------------------------------------------------------------
        private string _itemId;
        private int    _type;
        private int    _value;
        private int    _variable;

        // initialize methods
        //--------------------------------------------------------------------------------------------------------------
        /**
         * 初期化
         */
        public void Init(string itemId, int type, int variable, int value) {
            _itemId = itemId;
            _type = type;
            _variable = variable;
            _value = value;
        }

        public void SetPartyItems() {
            if (_variable == 1)
            {
                var variablesNum = int.Parse(DataManager.Self().GetRuntimeSaveDataModel().variables.data[_value]);
                _value = variablesNum;
            }

            var items = DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.items;
            var index = 0;
            var haveItem = false;
            for (var i = 0; i < items.Count; i++)
                if (_itemId == items[i].itemId)
                {
                    haveItem = true;
                    index = i;
                    break;
                }

            if (_type == 0)
            {
                if (!haveItem)
                {
                    var work = new RuntimePartyDataModel.Item();
                    work.itemId = _itemId;
                    work.value = Math.Min(Math.Max(_value, _minValue), _maxValue);
                    items.Add(work);
                }
                else
                {
                    items[index].value += _value;
                    items[index].value = Math.Min(Math.Max(items[index].value, _minValue), _maxValue);
                }
            }
            else
            {
                if (haveItem)
                {
                    items[index].value -= _value;
                    items[index].value = Math.Min(Math.Max(items[index].value, _minValue), _maxValue);
                }
            }

            //バトルでの実行時の場合には、バトル画面のDataModelにも反映する
            if (GameStateHandler.IsBattle())
                DataManager.Self().GetGameParty().ResetPartyItems();
        }
    }
}