using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository;
using RPGMaker.Codebase.Editor.Common;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Character
{
    /// <summary>
    ///     [フキダシアイコンの表示]の実行内容枠の表示物
    /// </summary>
    public class CharacterShowIcon : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            ret += "◆" + EditorLocalize.LocalizeText("WORD_0953") + " : ";

            ret += GetTargetCharacterName(eventCommand.parameters[0]) + ", ";

            var _orderData = AssetManageRepository.OrderManager.Load();
            var _assetManageDatas = new List<AssetManageDataModel>();
            var manageData = Editor.Hierarchy.Hierarchy.databaseManagementService.LoadAssetManage();
            var category = AssetCategoryEnum.POPUP;
            for (var i = 0; i < _orderData.orderDataList.Length; i++)
            {
                if (_orderData.orderDataList[i].idList == null)
                    continue;
                if (_orderData.orderDataList[i].assetTypeId == (int) category)
                    for (var i2 = 0; i2 < _orderData.orderDataList[i].idList.Count; i2++)
                    {
                        for (int i3 = 0; i3 < manageData.Count; i3++)
                            if (manageData[i3].id == _orderData.orderDataList[i].idList[i2])
                            {
                                _assetManageDatas.Add(manageData[i3]);
                                break;
                            }
                    }
            }

            bool isNone = true;

            if (_assetManageDatas.Count > 0)
            {
                for (int i = 0; i < _assetManageDatas.Count; i++)
                {
                    if (_assetManageDatas[i].id == eventCommand.parameters[1])
                    {
                        ret += _assetManageDatas[i].name;
                        isNone = false;
                        break;
                    }
                }
            }

            //設定しているデータがなかった場合、「なし」を表示させる
            if (isNone)
            {
                ret += EditorLocalize.LocalizeText("WORD_0113");
            }


            if (eventCommand.parameters[2] == "1") ret += " (" + EditorLocalize.LocalizeText("WORD_1087") + ")";


            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}