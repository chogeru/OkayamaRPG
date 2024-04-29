using System;
using System.Collections.Generic;
using System.Linq;

namespace RPGMaker.Codebase.Runtime.Addon
{
    public class AddonParameterContainer : List<AddonParameter>
    {
        public AddonParameterContainer() {
        }

        public AddonParameterContainer(List<AddonParameter> parameters) {
            foreach (var parameter in parameters) Add(parameter);
        }

        public bool ContainsKey(string key) {
            var parameter = this.FirstOrDefault(x => x.key == key);
            return parameter != null;
        }

        public void SetParameterValue(string key, string value) {
            var param = this.FirstOrDefault(x => x.key == key);
            if (param == null)
            {
                // Create and add.
                param = new AddonParameter(key, value);
                Add(param);
                return;
            }

            param.value = value;
        }

        public string GetParameterValue(string key) {
            var parameter = this.FirstOrDefault(x => x.key == key);
            if (parameter == null) return null;
            return parameter.value;
        }
    }

    public class AddonParamInfo
    {
        public AddonParameterContainer infos = new AddonParameterContainer();
        public string                  name;
        public AddonParameterContainer options = new AddonParameterContainer();

        public AddonParamInfo(string name) {
            this.name = name;
        }

        public AddonParamInfo(AddonParamInfoJson json) {
            name = json.name;
            infos = new AddonParameterContainer(json.infos);
            options = new AddonParameterContainer(json.options);
        }

        public AddonParameter GetInfo(string key) {
            var info = infos.FirstOrDefault(x => x.key == key);
            return info;
        }

        public void AddInfo(string key, string value) {
            infos.Add(new AddonParameter(key, value));
        }

        public void RemoveInfo(string key) {
            var index = infos.FindIndex(x => { return x.key == key; });
            if (index >= 0) infos.RemoveAt(index);
        }
    }

    public class AddonParamInfoContainer : List<AddonParamInfo>
    {
        public AddonParamInfoContainer() {
        }

        public AddonParamInfoContainer(List<AddonParamInfoJson> jsons) {
            Clear();
            foreach (var json in jsons) Add(new AddonParamInfo(json));
        }

        public AddonParamInfo GetParamInfo(string name) {
            var paramInfo = this.FirstOrDefault(x => x.name == name);
            if (paramInfo == null) return null;
            return paramInfo;
        }
    }

    public class AddonCommandInfo
    {
        public AddonParamInfoContainer args  = new AddonParamInfoContainer();
        public AddonParameterContainer infos = new AddonParameterContainer();
        public string                  name;

        public AddonCommandInfo(string name) {
            this.name = name;
        }

        public AddonCommandInfo(AddonCommandInfoJson json) {
            name = json.name;
            infos = new AddonParameterContainer(json.infos);
            args = new AddonParamInfoContainer(json.args);
        }

        public AddonParameter GetParamInfo(string name) {
            var parameter = infos.FirstOrDefault(x => x.key == name);
            if (parameter == null) return null;
            return parameter;
        }
    }

    public class AddonStructInfo
    {
        public AddonParameterContainer infos = new AddonParameterContainer();
        public string                  name;
        public AddonParamInfoContainer params_ = new AddonParamInfoContainer();

        public AddonStructInfo(string name) {
            this.name = name;
        }

        public AddonStructInfo(AddonStructInfoJson json) {
            name = json.name;
            infos = new AddonParameterContainer(json.infos);
            params_ = new AddonParamInfoContainer(json.params_);
        }
    }

    public class AddonNoteParamInfoContainer : List<AddonParamInfo>
    {
        public AddonNoteParamInfoContainer() {
        }

        public AddonNoteParamInfoContainer(List<AddonParamInfoJson> jsons) {
            Clear();
            foreach (var json in jsons) Add(new AddonParamInfo(json));
        }

        public AddonParamInfo GetParamInfo(string name) {
            var paramInfo = this.FirstOrDefault(x => x.name == name);
            if (paramInfo != null) return paramInfo;
            return null;
        }
    }

    public class AddonComamndInfoContainer : List<AddonCommandInfo>
    {
        public AddonComamndInfoContainer() {
        }

        public AddonComamndInfoContainer(List<AddonCommandInfoJson> jsons) {
            Clear();
            foreach (var json in jsons) Add(new AddonCommandInfo(json));
        }

        public AddonCommandInfo GetCommandInfo(string name) {
            var commandInfo = this.FirstOrDefault(x => x.name == name);
            if (commandInfo != null) return commandInfo;
            return null;
        }
    }

    public class AddonStructInfoContainer : List<AddonStructInfo>
    {
        public AddonStructInfoContainer() {
        }

        public AddonStructInfoContainer(List<AddonStructInfoJson> jsons) {
            Clear();
            foreach (var json in jsons) Add(new AddonStructInfo(json));
        }

        public AddonStructInfo GetStructInfo(string name) {
            var structInfo = this.FirstOrDefault(x => x.name == name);
            if (structInfo != null) return structInfo;
            return null;
        }
    }

    public class AddonInfo
    {
        public string                      addondesc      = "";
        public string                      author         = "";
        public List<string>                base_          = new List<string>();
        public AddonComamndInfoContainer   commandInfos   = new AddonComamndInfoContainer();
        public string                      help           = "";
        public string                      name           = "";
        public AddonNoteParamInfoContainer noteParamInfos = new AddonNoteParamInfoContainer();
        public List<string>                orderAfter     = new List<string>();

        public List<string> orderBefore = new List<string>();

        //public List<string> requiredAssets = new List<string>();
        public AddonParamInfoContainer  paramInfos  = new AddonParamInfoContainer();
        public AddonStructInfoContainer structInfos = new AddonStructInfoContainer();
        public string                   url         = "";

        public AddonInfo() {
        }

        public AddonInfo(AddonInfoJson addonInfoJson) {
            name = addonInfoJson.name;
            addondesc = addonInfoJson.addondesc;
            author = addonInfoJson.author;
            help = addonInfoJson.help;
            url = addonInfoJson.url;
            base_ = addonInfoJson.base_;
            orderAfter = addonInfoJson.orderAfter;
            orderBefore = addonInfoJson.orderBefore;
            //requiredAssets = addonInfoJson.requiredAssets;
            paramInfos = new AddonParamInfoContainer(addonInfoJson.paramInfos);
            noteParamInfos = new AddonNoteParamInfoContainer(addonInfoJson.noteParamInfos);
            commandInfos = new AddonComamndInfoContainer(addonInfoJson.commandInfos);
            structInfos = new AddonStructInfoContainer(addonInfoJson.structInfos);
        }
    }

    public class AddonInfoContainer : List<AddonInfo>
    {
        public AddonInfoContainer() {
        }

        public AddonInfoContainer(List<AddonInfo> addonInfos) {
            foreach (var addonInfo in addonInfos) Add(addonInfo);
        }

        public AddonInfo GetAddonInfo(string name) {
            var addonInfo = this.FirstOrDefault(x => x.name == name);
            if (addonInfo != null) return addonInfo;
            return null;
        }
    }

    [Serializable]
    public class AddonDataModel : IAddonDataModel
    {
        public AddonDataModel(
            string name,
            bool status,
            string description,
            AddonParameterContainer parameters = null
        ) {
            Name = name;
            Status = status;
            Description = description;
            Parameters = parameters;
        }

        public string Name { get; set; }
        public bool Status { get; set; }
        public string Description { get; set; }
        public AddonParameterContainer Parameters { get; set; }

        public string GetParameterValue(string key) {
            var param = Parameters.FirstOrDefault(x => x.key == key);
            if (param == null) return null;
            return param.value;
        }

        public void SetParameterValue(string key, string value) {
            var param = Parameters.FirstOrDefault(x => x.key == key);
            if (param == null)
            {
                // Create and add.
                param = new AddonParameter(key, value);
                Parameters.Add(param);
                return;
            }

            param.value = value;
        }

        public void AddParameter(string key, string value) {
            var param = Parameters.FirstOrDefault(x => x.key == key);
            if (param == null)
            {
            }

            param.value = value;
        }

        public static AddonDataModel Create() {
            return new AddonDataModel(
                "",
                false,
                "",
                new AddonParameterContainer()
            );
        }

        public void CopyTo(AddonDataModel addonDataMode) {
            var json = DataConverter.ConvertAddonToJson(this);
            var addon = DataConverter.ConvertAddonToObject(json);
            addonDataMode.Name = Name;
            addonDataMode.Status = Status;
            addonDataMode.Description = Description;
            addonDataMode.Parameters = addon.Parameters;
        }
    }
}
