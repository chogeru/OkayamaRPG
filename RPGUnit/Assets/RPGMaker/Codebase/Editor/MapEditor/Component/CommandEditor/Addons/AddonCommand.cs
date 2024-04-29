using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.AddonUIUtil;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using RPGMaker.Codebase.Runtime.Addon;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.CommandEditor.Addons
{
    public class AddonCommand : AbstractCommandEditor
    {
        private const string SettingUxml =
            "Assets/RPGMaker/Codebase/Editor/MapEditor/Asset/Uxml/EventCommand/inspector_mapEvent_addon_command.uxml";

        public static readonly string               mapIdKey = "\nmapId\n";
        private                AddonDataModel       _addon   = AddonDataModel.Create();
        private                AddonInfo            _addonInfo;
        private                AddonInfoContainer   _addonInfos;
        private                List<AddonDataModel> _addons;
        private                AddonCommandInfo     _commandInfo;

        private GameObject              _gameObject;
        private VisualElement           _listWindow;
        private AddonParameterContainer _parameters;

        public AddonCommand(
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

            //UnityEditorWrapper.AssetDatabaseWrapper.Refresh();

            if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Count == 0)
            {
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("");
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters.Add("[]");
                Save(EventDataModels[EventIndex]);
                UnityEditorWrapper.AssetDatabaseWrapper.Refresh();
            }

            var addonManagementService = new AddonManagementService();
            _addons = addonManagementService.LoadAddons();
            _addon.Name = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0];
            _addonInfos = AddonManager.Instance.GetAddonInfos();
            _addonInfo = _addon.Name.Length > 0 ? _addonInfos.GetAddonInfo(_addon.Name) : null;
            _commandInfo = null;
            _parameters = new AddonParameterContainer(
                JsonHelper.FromJsonArray<AddonParameter>(EventDataModels[EventIndex].eventCommands[EventCommandIndex]
                    .parameters[2]));

            _listWindow = RootElement.Query<VisualElement>("system_window_lowerwindow").AtIndex(0);

            var addonWindow = RootElement.Query<VisualElement>("system_window_addonwindow").AtIndex(0);
            var commandWindow = RootElement.Query<VisualElement>("system_window_commandwindow").AtIndex(0);
            var descriptionWindow = RootElement.Query<VisualElement>("system_window_descriptionwindow").AtIndex(0);

            // Add-on Name
            var label = new Label(EditorLocalize.LocalizeText("WORD_2511"));
            label.style.height = 16;
            addonWindow.Add(label);

            var nameList = new List<string>();
            var defaultIndex = 0;
            nameList.Add(EditorLocalize.LocalizeText("WORD_2505"));
            var i = 0;
            foreach (var addonInfo in _addonInfos)
            {
                if (addonInfo.commandInfos.Count == 0) continue;
                var name = addonInfo.name;
                nameList.Add(name);
                if (name == _addon.Name) defaultIndex = i + 1;
                i++;
            }

            var popupField = new PopupFieldBase<string>(nameList, defaultIndex);
            popupField.RegisterValueChangedCallback(evt =>
            {
                var popupField = evt.currentTarget as PopupFieldBase<string>;
                //Debug.Log($"popupField: {popupField.index}, {popupField.value}");
                if (popupField.value != _addon.Name)
                {
                    //_renew = true;
                    if (popupField.index == 0)
                    {
                        _addon = AddonDataModel.Create();
                        _addonInfo = null;
                        _commandInfo = null;
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] = "";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = "";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] = "[]";
                        ChangeCommandAndDescription(_commandInfo, commandWindow, descriptionWindow);
                        Save(EventDataModels[EventIndex]);
                    }
                    else
                    {
                        //var addonInfo = _addonInfos.GetAddonInfo(popupField.value);
                        //_addon = AddonDataModel.Create();
                        //AddonManager.Instance.ApplyAddonInfoToModel(_addon, addonInfo);
                        _addon.Name = EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[0] =
                            popupField.value;
                        _addonInfo = _addon.Name.Length > 0 ? _addonInfos.GetAddonInfo(_addon.Name) : null;
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = "";
                        EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] = "[]";
                        _commandInfo = null;
                        if (_addonInfo != null && _addonInfo.commandInfos.Count > 0)
                            _commandInfo = _addonInfo.commandInfos[0];
                        ChangeCommandAndDescription(_commandInfo, commandWindow, descriptionWindow);
                        Save(EventDataModels[EventIndex]);
                    }
                }

                ;
            });
            popupField.AddToClassList("addon_name_popup_field");
            AddonUIUtil.AddStyleEllipsisToPopupField(popupField);
            addonWindow.Add(popupField);

            // Command Name, Desctiption
            RefreshCommandAndDescription(commandWindow, descriptionWindow);


            // Parameters
            RefreshParameter(_listWindow);
        }

        private void ChangeCommandAndDescription(
            AddonCommandInfo commandInfo,
            VisualElement commandWindow,
            VisualElement descriptionWindow
        ) {
            if (commandInfo != null)
                EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] = commandInfo.name;
            RefreshCommandAndDescription(commandWindow, descriptionWindow);
            RefreshParameter(_listWindow);
        }

        private void RefreshCommandAndDescription(VisualElement commandWindow, VisualElement descriptionWindow) {
            commandWindow.Clear();
            descriptionWindow.Clear();
            var clear = false;
            if (_addonInfo == null || _addonInfo.commandInfos.Count == 0) clear = true;

            // Command Name
            var label = new Label(EditorLocalize.LocalizeText("WORD_2512"));
            label.style.height = 16;
            commandWindow.Add(label);
            TextElement desctiptionTextElement = null;
            var descriptinText = "";
            if (clear)
            {
                var button = new Button();
                button.style.flexGrow = 1;
                commandWindow.Add(button);
            }
            else
            {
                var nameList = new List<string>();
                var defaultIndex = 0;
                var i = 0;
                foreach (var commandInfo in _addonInfo.commandInfos)
                {
                    var text = commandInfo.infos.GetParameterValue("text");
                    //
                    nameList.Add(text != null ? text : commandInfo.name);
                    if (EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] == commandInfo.name)
                    {
                        defaultIndex = i;
                        _commandInfo = commandInfo;
                    }

                    i++;
                }

                var popupField = new PopupFieldBase<string>(nameList, defaultIndex);
                popupField.RegisterValueChangedCallback(evt =>
                {
                    var popupField = evt.currentTarget as PopupFieldBase<string>;
                    //Debug.Log($"popupField: {popupField.index}, {popupField.value}");
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[1] =
                        _addonInfo.commandInfos[popupField.index].name;
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] = "[]";
                    _commandInfo = _addonInfo.commandInfos.GetCommandInfo(EventDataModels[EventIndex]
                        .eventCommands[EventCommandIndex].parameters[1]);
                    var description = _commandInfo.GetParamInfo("desc");
                    desctiptionTextElement.text = description != null
                        ? description.value
                        : _addonInfo.commandInfos[popupField.index].name;
                    ChangeCommandAndDescription(_commandInfo, commandWindow, descriptionWindow);
                    Save(EventDataModels[EventIndex]);
                });
                popupField.AddToClassList("addon_command_popup_field");
                commandWindow.Add(popupField);
                var description = _commandInfo?.GetParamInfo("desc");
                descriptinText = description != null ? description.value :
                    _addonInfo != null ? _addonInfo.commandInfos[popupField.index].name : "";
            }

            // Command Description
            desctiptionTextElement = new TextElement();
            desctiptionTextElement.text = descriptinText;
            desctiptionTextElement.style.flexGrow = 1;
            desctiptionTextElement.AddToClassList("text_ellipsis");
            descriptionWindow.Add(desctiptionTextElement);
        }

        private void RefreshParameter(VisualElement listWindow) {
            listWindow.Clear();
            listWindow.Add(CreateListView());
        }

        // リストの要素作成
        private UnityEngine.UIElements.ListView CreateListView() {
            StripedListView<string> listView = null;
            Action<VisualElement, int> bindItem = (e, i) =>
            {
                e.Clear();
                if (_commandInfo == null) return;
                {
                    var key = (listView.itemsSource as List<string>)[i];
                    VisualElement visualElement = new KeyVisualElement<string>(key);
                    visualElement.style.flexDirection = FlexDirection.Row;

                    listView.SetVisualElementStriped(visualElement, i);
                    if (i == listView.itemsSource.Count - 1) listView.AddVisualElementStriped(e);

                    var paramInfo = _commandInfo.args.GetParamInfo(key);
                    var paramKey = paramInfo.name;

                    // Name
                    var nameInfo = paramInfo.GetInfo("text");
                    var nameLabel = new Label(nameInfo != null ? nameInfo.value : paramKey);
                    nameLabel.AddToClassList("list_view_item_name_label");
                    nameLabel.AddToClassList("text_ellipsis");

                    // Value
                    var type = paramInfo.GetInfo("type")?.value;
                    var value = AddonManager.GetInitialValue(_parameters, paramInfo);
                    value = AddonUIUtil.GetEasyReadableText(_addonInfo, paramInfo, null, value);
                    var valueLabel = new Label(value);
                    valueLabel.AddToClassList("list_view_item_value_label");
                    valueLabel.AddToClassList("text_ellipsis");
                    valueLabel.style.flexGrow = 1;

                    visualElement.Add(nameLabel);
                    visualElement.Add(valueLabel);
                    e.Add(visualElement);
                }
            };

            Func<VisualElement> makeItem = () => new Label();
            var args = _commandInfo != null ? _commandInfo.args : new AddonParamInfoContainer();
            listView = new StripedListView<string>(new string[args.Count], 16, makeItem, bindItem);
            listView.AddToClassList("list_view");
            listView.SolidQuad();
            listView.name = "list";
            //listView.selectionType = SelectionType.Multiple;
            listView.reorderable = false;
            var list = new List<string>();
            foreach (var paramInfo in args) list.Add(paramInfo.name);
            listView.itemsSource = list;

            if (list.Count > 0) listView.selectedIndex = 0;

            listView.RegisterCallback<KeyDownEvent>(e =>
            {
                //Debug.Log($"pressed: '{e.character}'");
                switch (e.keyCode)
                {
                    case KeyCode.KeypadEnter:
                    case KeyCode.Return:
                        var key = (listView.itemsSource as List<string>)[listView.selectedIndex];
                        ShowAddonParameterEditWindow(_addonInfo, _commandInfo.args.GetParamInfo(key));
                        break;
                }
            });
            listView.onItemsChosen += objects =>
            {
                var list = objects.ToList();
                if (list.Count == 0) return;
                //Debug.Log($"list: {list[0]}");
                var key = list[0].ToString();
                ShowAddonParameterEditWindow(_addonInfo, _commandInfo.args.GetParamInfo(key));
            };

            return listView;
        }

        private void ShowAddonParameterEditWindow(AddonInfo addonInfo, AddonParamInfo paramInfo) {
            var arr = new AddonParameter[_parameters.Count];
            _parameters.CopyTo(arr);
            var parameters = new AddonParameterContainer(arr.ToList());
            var mapEvent = new EventManagementService().LoadEventMap()
                .FirstOrDefault(x => x.eventId == EventDataModels[EventIndex].id);
            var mapId = mapEvent?.mapId;
            var addedParameter = new AddonParameter(mapIdKey, mapId);
            parameters.Add(addedParameter);
            AddonParameterModalWindow.ShowAddonParameterEditWindow(addonInfo, paramInfo, false, parameters, obj =>
            {
                //Debug.Log($"AddonParameterEditModalWindow return: {obj}");
                var refresh = false;
                var value = obj as string;
                var listView = _listWindow[0] as UnityEngine.UIElements.ListView;
                if (obj is bool && (bool) obj)
                {
                    refresh = true;
                    parameters.Remove(addedParameter);
                    _parameters.Clear();
                    foreach (var parameter in parameters) _parameters.Add(parameter);
                    EventDataModels[EventIndex].eventCommands[EventCommandIndex].parameters[2] =
                        JsonHelper.ToJsonArray(_parameters);
                    Save(EventDataModels[EventIndex]);
                }

                if (refresh) listView.Rebuild();
            });
        }
    }
}