using System;
using System.Collections.Generic;
using RPGMaker.Codebase.CoreSystem.Knowledge.JsonStructure;

namespace RPGMaker.Codebase.Runtime.Addon
{
    [Serializable]
    public class AddonJson : IJsonStructure
    {
        public string           description;
        public string           name;
        public AddonParameter[] parameters;
        public bool             status;

        public AddonJson(
            string name,
            bool status,
            string description,
            AddonParameter[] parameters
        ) {
            this.name = name;
            this.status = status;
            this.description = description;
            this.parameters = parameters;
        }

        public string GetID() {
            return name;
        }
    }

    [Serializable]
    public class AddonParameter
    {
        public string key;
        public string value;

        public AddonParameter(
            string key,
            string value
        ) {
            this.key = key;
            this.value = value;
        }
    }

    [Serializable]
    public class AddonParamInfoJson
    {
        public List<AddonParameter> infos = new List<AddonParameter>();
        public string               name;
        public List<AddonParameter> options = new List<AddonParameter>();

        public AddonParamInfoJson(AddonParamInfo addonParamInfo) {
            name = addonParamInfo.name;
            foreach (var info in addonParamInfo.infos)
            {
                infos.Add(info);
            }

            foreach (var option in addonParamInfo.options)
            {
                options.Add(option);
            }
        }
    }

    [Serializable]
    public class AddonCommandInfoJson
    {
        public List<AddonParamInfoJson> args  = new List<AddonParamInfoJson>();
        public List<AddonParameter>     infos = new List<AddonParameter>();
        public string                   name;

        public AddonCommandInfoJson(AddonCommandInfo addonCommandInfo) {
            name = addonCommandInfo.name;
            foreach (var info in addonCommandInfo.infos)
            {
                infos.Add(info);
            }

            foreach (var arg in addonCommandInfo.args)
            {
                args.Add(new AddonParamInfoJson(arg));
            }
        }
    }

    [Serializable]
    public class AddonStructInfoJson
    {
        public List<AddonParameter>     infos = new List<AddonParameter>();
        public string                   name;
        public List<AddonParamInfoJson> params_ = new List<AddonParamInfoJson>();

        public AddonStructInfoJson(AddonStructInfo addonStructInfo) {
            name = addonStructInfo.name;
            foreach (var info in addonStructInfo.infos)
            {
                infos.Add(info);
            }

            foreach (var param in addonStructInfo.params_)
            {
                params_.Add(new AddonParamInfoJson(param));
            }
        }
    }

    [Serializable]
    public class AddonInfoJson
    {
        public string                     addondesc      = "";
        public string                     author         = "";
        public List<string>               base_          = new List<string>();
        public List<AddonCommandInfoJson> commandInfos   = new List<AddonCommandInfoJson>();
        public string                     help           = "";
        public string                     name           = "";
        public List<AddonParamInfoJson>   noteParamInfos = new List<AddonParamInfoJson>();
        public List<string>               orderAfter     = new List<string>();

        public List<string> orderBefore = new List<string>();

        //public List<string> requiredAssets = new List<string>();
        public List<AddonParamInfoJson>  paramInfos  = new List<AddonParamInfoJson>();
        public List<AddonStructInfoJson> structInfos = new List<AddonStructInfoJson>();
        public string                    url         = "";

        public AddonInfoJson(AddonInfo addonInfo) {
            name = addonInfo.name;
            addondesc = addonInfo.addondesc;
            author = addonInfo.author;
            help = addonInfo.help;
            url = addonInfo.url;
            base_ = addonInfo.base_;
            orderAfter = addonInfo.orderAfter;
            orderBefore = addonInfo.orderBefore;
            //requiredAssets = addonInfo.requiredAssets;
            foreach (var paramInfo in addonInfo.paramInfos)
            {
                paramInfos.Add(new AddonParamInfoJson(paramInfo));
            }

            foreach (var noteParamInfo in addonInfo.noteParamInfos)
            {
                noteParamInfos.Add(new AddonParamInfoJson(noteParamInfo));
            }

            foreach (var commandInfo in addonInfo.commandInfos)
            {
                commandInfos.Add(new AddonCommandInfoJson(commandInfo));
            }

            foreach (var structInfo in addonInfo.structInfos)
            {
                structInfos.Add(new AddonStructInfoJson(structInfo));
            }
        }
    }

    public class AddonNoteJson
    {
        public string str;

        public AddonNoteJson(string str) {
            this.str = str;
        }
    }

    public class AddonArrayJson
    {
        public string[] arr;

        public AddonArrayJson(string[] arr) {
            this.arr = arr;
        }
    }
}
