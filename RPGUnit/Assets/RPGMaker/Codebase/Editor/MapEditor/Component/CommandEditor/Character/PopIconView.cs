using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using static RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event.EventDataModel;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Character
{
    /// <summary>
    ///     [フキダシアイコンの表示]のコマンド設定枠の表示物
    /// </summary>
    public class PopIconView : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_popIcon_view.uxml";

        private GenericPopupFieldBase<TargetCharacterChoice> _targetCharacterPopupField;

        private EventCommand _targetCommand;

        public PopIconView(
            VisualElement rootElement,
            List<EventDataModel> eventDataModels,
            int eventIndex,
            int eventCommandIndex
        )
            : base(rootElement, eventDataModels, eventIndex, eventCommandIndex) {
        }

        public override void Invoke() {
            var commandTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SettingUxml);
            VisualElement commandFromUxml = commandTree.CloneTree();
            EditorLocalize.LocalizeElements(commandFromUxml);
            commandFromUxml.style.flexGrow = 1;
            RootElement.Add(commandFromUxml);

            UnityEditorWrapper.AssetDatabaseWrapper.Refresh();

            _targetCommand = EventDataModels[EventIndex].eventCommands[EventCommandIndex];
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

            var popIconChoices = _assetManageDatas.Select(assetManageData => assetManageData.name).ToList();

            // 初期値
            if (_targetCommand.parameters.Count == 0)
            {
                // キャラクター（-2から開始するインデックス）
                _targetCommand.parameters.Add("-2");
                // フキダシアイコン画像のパス
                _targetCommand.parameters.Add(_assetManageDatas.Count > 0 ? _assetManageDatas[0].id : "");
                // 完了までウェイト(0：OFF / 1：ON)
                _targetCommand.parameters.Add("0");

                EventManagementService.SaveEvent(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            // キャラクター。
            {
                int targetCharacterParameterIndex = 0;
                AddOrHideProvisionalMapAndAddTargetCharacterPopupField(
                    targetCharacterParameterIndex,
                    provisionalMapPopupField =>
                    {
                        _targetCharacterPopupField = AddTargetCharacterPopupField(
                            QRoot("character"),
                            targetCharacterParameterIndex,
                            forceMapId: provisionalMapPopupField?.value.MapDataModel?.id);
                    });
            }
            
            if (_assetManageDatas.Count == 0)
            {
                VisualElement popIconArea = RootElement.Query<VisualElement>("popIcon_area");
                popIconArea.style.display = DisplayStyle.None;
                _targetCommand.parameters[1] = "";
                Save(EventDataModels[EventIndex]);
                return;
            }

            // フキダシアイコン。

            VisualElement popIcon = RootElement.Query<VisualElement>("popIcon");
            VisualElement popIcon_image = RootElement.Query<VisualElement>("popIcon_image");
            VisualElement image = new VisualElement();
            image.style.width = popIcon_image.style.width;
            image.style.height = popIcon_image.style.height;
            popIcon_image.style.justifyContent = Justify.Center;
            popIcon_image.Add(image);

            var choiceAsset = _assetManageDatas.SingleOrDefault(asset => asset.id == _targetCommand.parameters[1]);
            var choiceIndex = choiceAsset != null ? _assetManageDatas.IndexOf(choiceAsset) : -1;
            if (_targetCommand.parameters[1] == "" && _assetManageDatas.Count > 0)
            {
                _targetCommand.parameters[1] = _assetManageDatas[0].id;
                Save(EventDataModels[EventIndex]);
            }

            bool isNone = false;

            if (choiceIndex == -1)
            {
                choiceIndex = 0;
                isNone = true;
            }


            var popIconPopupField = new PopupFieldBase<string>(popIconChoices, choiceIndex);
            popIcon.Clear();
            popIcon.Add(popIconPopupField);
            popIconPopupField.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[1] = _assetManageDatas[popIconPopupField.index].id;
                SetPopIconImage();
                Save(EventDataModels[EventIndex]);
            });

            //設定しているデータがなかった場合、「なし」を表示させる
            if (isNone)
            {
                popIconPopupField.ChangeButtonText(EditorLocalize.LocalizeText("WORD_0113"));
            }


            // プレビューボタン。
            Button previewButton = RootElement.Query<Button>("PreviewButton");
            previewButton.clicked += () =>
            {
                var popupIconAsset = _assetManageDatas.ForceSingle(a => a.name == popIconPopupField.value);
                Inspector.Inspector.AssetManageEditView(popupIconAsset);
            };

            Toggle weightToggle = RootElement.Query<Toggle>("weight_toggle");
            weightToggle.value = _targetCommand.parameters[2] == "1";
            weightToggle.RegisterValueChangedCallback(evt =>
            {
                _targetCommand.parameters[2] = weightToggle.value ? "1" : "0";
                ;
                Save(EventDataModels[EventIndex]);
            });


            if (!isNone) SetPopIconImage();

            void SetPopIconImage() {
                BackgroundImageHelper.SetBackground(image, new UnityEngine.Vector2(68, 68),
                    ImageManager.LoadPopIcon(
                        _assetManageDatas[popIconPopupField.index].imageSettings[0].path,
                        _assetManageDatas[popIconPopupField.index].imageSettings[0].animationFrame));
            }
        }
    }
}