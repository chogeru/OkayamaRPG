using System.Linq;
using System.Linq.Expressions;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using RPGMaker.Codebase.Addon;
using RPGMaker.Codebase.CoreSystem.Knowledge.JsonStructure;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.CoreSystem.Service.EventManagement;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement;
using SimpleJSON;

namespace RPGMaker.Codebase.Runtime.Addon
{
    public class AddonInstance {
        public static void ExchangeFunctionPointer(MethodInfo method0, MethodInfo method1) {

            unsafe

            {

                var functionPointer0 = method0.MethodHandle.Value.ToPointer();

                var functionPointer1 = method1.MethodHandle.Value.ToPointer();

                var tmpPointer = *((int*) new IntPtr(((int*) functionPointer0 + 1)).ToPointer());

                *((int*) new IntPtr(((int*) functionPointer0 + 1)).ToPointer()) = *((int*) new IntPtr(((int*) functionPointer1 + 1)).ToPointer());

                *((int*) new IntPtr(((int*) functionPointer1 + 1)).ToPointer()) = tmpPointer;

            }

        }

        object _instance;
        AddonDataModel _addonDataModel;
        Type[] _types;
        public class MethodTypeInfo {
#if ENABLE_IL2CPP
            public MethodInfo methodInfo;
#else
            public Action<object[]> method;
#endif
            public Type[] types;

#if ENABLE_IL2CPP
            public MethodTypeInfo(MethodInfo methodInfo, Type[] types) {
                this.methodInfo = methodInfo;
                this.types = types;
            }
#else
            public MethodTypeInfo(Action<object[]> method, Type[] types) {
                this.method = method;
                this.types = types;
            }
#endif
        }
        Dictionary<string, MethodTypeInfo> _methodTypeDic = new Dictionary<string, MethodTypeInfo>();

        public AddonInstance() {
            _instance = null;
            _addonDataModel = null;
            _types = null;
        }

        public AddonInstance(object instance, AddonDataModel adm, Type[] types) {
            _instance = instance;
            _addonDataModel = adm;
            _types = types;
        }
        public object GetInstance() {
            return _instance;
        }
        public void AddMethodInfo(string name, MethodInfo methodInfo, Type[] types) {
            // 引数はオブジェクトの配列
            var args = Expression.Parameter(typeof(object[]), "args");

            // メソッドに渡す引数はオブジェクト配列をインデクサでアクセス+キャスト => (cast)args[index]
            var parameters = methodInfo.GetParameters()
                .Select((x, index) =>
                    Expression.Convert(
                        Expression.ArrayIndex(args, Expression.Constant(index)),
                    x.ParameterType))
                .ToArray();

#if ENABLE_IL2CPP
            _methodTypeDic.Add(name, new MethodTypeInfo(methodInfo, types));
#else
            // 本体を作る。
            var lambda = Expression.Lambda<Action<object[]>>(
                Expression.Convert(
                    Expression.Call(Expression.Constant(_instance), methodInfo, parameters),
                    typeof(void)),
                args).Compile();
            _methodTypeDic.Add(name, new MethodTypeInfo(lambda, types));
#endif
        }
        public string GetName() {
            return (_addonDataModel != null) ? _addonDataModel.Name : null;
        }
        public MethodTypeInfo GetMethodTypeInfo(string methodName) {
            if (_methodTypeDic.ContainsKey(methodName))
            {
                return _methodTypeDic[methodName];
            }
            return null;
        }
    }
    public class AddonInstanceContainer : List<AddonInstance> {
        Dictionary<string, AddonInstance> _nameInstanceDic = new Dictionary<string, AddonInstance>();

        public void Add(AddonInstance addonInstance) {
            base.Add(addonInstance);
            var name = addonInstance.GetName();
            if (name != null)
            {
                if (!_nameInstanceDic.ContainsKey(name))
                {
                    _nameInstanceDic.Add(name, addonInstance);
                }
            }
        }
        public AddonInstance GetAddonInstance(string name) {
            if (_nameInstanceDic.ContainsKey(name))
            {
                return _nameInstanceDic[name];
            }
            return null;
        }
    }

    public enum ParamType {
        String,
        Number,
        Integer,
        Boolean,
        Select,
        Combo,
        MultilineString,
        Note,
        Struct,
        CommonEvent,
        MapEvent,
        Switch,
        Variable,
        Animation,
        Actor,
        Class,
        Skill,
        Item,
        Weapon,
        Armor,
        Enemy,
        Troop,
        State,
        Tileset,
        File,
    }

    public class AddonManager
    {
        public const string PlayerEventId = "-2";
        public const string ThisEventId = "-1";

        private const string _addonFolderPath = "Assets/RPGMaker/Codebase/Add-ons";
        private static AddonManager _instance;
        private AddonManagementService _addonRepositoryService;
        private List<AddonDataModel> _addonDataModels;
        private AddonInstanceContainer _addonInstances;
        delegate object ParameterTypeConvert(string value);
        static object GetRawString(string value) {
            var json = JSON.Parse(value);
            return json.Value;
        }
        private Dictionary<string, ParameterTypeConvert> _parameterTypeConvertDic = new Dictionary<string, ParameterTypeConvert>() {
            { "string", GetRawString },
            { "number", (value) => { return double.Parse(value); } },
            { "integer", (value) => { return int.Parse(value); } },
            { "boolean", (value) => { return bool.Parse(value); } },
            { "select", (value) => { return int.Parse(value); } },
            { "combo", (value) => { return value; } },
            { "multiline_string", GetRawString },
            { "note", GetRawString },
            { "struct", (value) => { return value; } },
            { "switch", (value) => { return value; } },
            { "variable", (value) => { return value; } },
            { "animation", (value) => { return value; } },
            { "actor", (value) => { return value; } },
            { "class", (value) => { return value; } },
            { "skill", (value) => { return value; } },
            { "item", (value) => { return value; } },
            { "weapon", (value) => { return value; } },
            { "armor", (value) => { return value; } },
            { "enemy", (value) => { return value; } },
            { "troop", (value) => { return value; } },
            { "state", (value) => { return value; } },
            { "tileset", (value) => { return value; } },
            { "file", (value) => { return value; } },
        };

        private string _eventId = null;
        private Action _commandCallback = null;
        private AddonInfoContainer _addonInfos;

        public static AddonManager Instance
        {
            get
            {
                return _instance ??= new AddonManager();
            }
        }

#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod]
        public static void InitializeOnLoad() {
            AddonManager.Instance.Refresh();
        }
#endif

        // Reload and initialize Add-ons.
        public void Refresh()
        {
            //Debug.Log("AddonManager.Refresh()");
            _addonRepositoryService = new AddonManagementService();
            _addonDataModels = _addonRepositoryService.LoadAddons();
#if UNITY_EDITOR
            _addonInfos = new AddonReader(_addonDataModels).ScanAndMergeAddons();
            _addonRepositoryService.SaveAddons(_addonDataModels);
            _addonRepositoryService.SaveAddonInfos(_addonInfos);
#else
            _addonInfos = _addonRepositoryService.LoadAddonInfos();
#endif
            InstantiateAddons();
            //var infos = GetAddonRuntimeInfos();
            //Debug.Log($"infos.Length: {infos.Length}");
        }

        public void CallAddonCommand(string addonName, string commandName, AddonParameterContainer parameters, string eventId, Action commandCallback) {
            //Debug.Log($"CallAddonCommand({addonName}, {commandName}, ...) called");
            var addonInfo = _addonInfos.GetAddonInfo(addonName);
            if (addonInfo == null)
            {
                Debug.LogWarning($"Addon {addonName} not registered.");
                commandCallback();
                return;
            }
            var commandInfo = addonInfo.commandInfos.GetCommandInfo(commandName);
            if (commandInfo == null)
            {
                Debug.LogWarning($"Addon {addonName}.{commandInfo} not registered.");
                commandCallback();
                return;
            }
            var addonInstance = _addonInstances.GetAddonInstance(addonName);
            if (addonInstance == null)
            {
                Debug.LogWarning($"Instance of Addon {addonName} not registered.");
                commandCallback();
                return;
            }
            var methodTypeInfo = addonInstance.GetMethodTypeInfo(commandName);
            if (methodTypeInfo == null)
            {
                Debug.LogWarning($"Method {commandInfo} of Addon {addonName} not registered.");
                commandCallback();
                return;
            }
            var params_ = new object[commandInfo.args.Count];
            for (int i = 0; i < params_.Length; i++)
            {
                var typeName = commandInfo.args[i].GetInfo("type");
                var value = GetInitialValue(parameters, commandInfo.args[i]);
                if (typeName != null && _parameterTypeConvertDic.ContainsKey(typeName.value))
                {
                    params_[i] = _parameterTypeConvertDic[typeName.value](value);
                }
                else
                {
                    params_[i] = value;
                }
            }
            try
            {
                _eventId = eventId;
                _commandCallback = commandCallback;
#if ENABLE_IL2CPP
                methodTypeInfo.methodInfo.Invoke(addonInstance.GetInstance(), params_);
#else
                methodTypeInfo.method(params_);
#endif
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                _eventId = null;
                var cb = _commandCallback;
                _commandCallback = null;
                if (cb != null)
                {
                    cb();
                }
            }
        }

        public string GetCurrentEventId() {
            return _eventId;
        }

        public Action TakeOutCommandCallback() {
            var cb = _commandCallback;
            _commandCallback = null;
            return cb;
        }

        public class AddonRuntimeInfo {
            public string addonName;
            public object instance;
            public class MethodParamInfo {
                public string methodName;
#if ENABLE_IL2CPP
                public MethodInfo methodInfo;
#else
                public Action<object[]> method;
#endif
                public class ParamInfo {
                    public string name;
                    public Type type;
                }
                public ParamInfo[] paramInfos;
            }
            public List<MethodParamInfo> methodParamInfos = new List<MethodParamInfo>();

            public AddonRuntimeInfo() { }
        }

        public AddonRuntimeInfo[] GetAddonRuntimeInfos() {
            var runtimeInfos = new List<AddonRuntimeInfo>();
            foreach (var addonInstance in _addonInstances)
            {
                var runtimeInfo = new AddonRuntimeInfo();
                runtimeInfo.addonName = addonInstance.GetName();
                runtimeInfo.instance = addonInstance.GetInstance();
                var addonInfo = _addonInfos.GetAddonInfo(runtimeInfo.addonName);
                foreach (var commandInfo in addonInfo.commandInfos)
                {
                    var methodParamInfo = new AddonRuntimeInfo.MethodParamInfo();
                    methodParamInfo.methodName = commandInfo.name;
#if ENABLE_IL2CPP
                    methodParamInfo.methodInfo = addonInstance.GetMethodTypeInfo(methodParamInfo.methodName)?.methodInfo;
#else
                    methodParamInfo.method = addonInstance.GetMethodTypeInfo(methodParamInfo.methodName)?.method;
#endif
                    var paramInfos = new List<AddonRuntimeInfo.MethodParamInfo.ParamInfo>();
                    foreach (var arg in commandInfo.args)
                    {
                        var paramInfo = new AddonRuntimeInfo.MethodParamInfo.ParamInfo();
                        paramInfo.name = arg.name;
                        var typeInfo = arg.GetInfo("type");
                        var typeName = typeInfo != null ? typeInfo.value : "string";
                        if (!_parameterTypeConvertDic.ContainsKey(typeName))
                        {
                            typeName = "string";
                        }
                        paramInfo.type = GetParameterType(typeName);
                        paramInfos.Add(paramInfo);
                    }
                    methodParamInfo.paramInfos = paramInfos.ToArray();
                    runtimeInfo.methodParamInfos.Add(methodParamInfo);
                }
                runtimeInfos.Add(runtimeInfo);
            }
            return runtimeInfos.ToArray();
        }

        public Type GetParameterType(string typeName) {
            switch (typeName)
            {
                case "boolean":
                    return typeof(bool);
                case "number":
                    return typeof(double);
                case "integer":
                    return typeof(int);
                case "select":
                    return typeof(int);
                case "string":
                case "multiline_string":
                case "note":
                    return typeof(string);
                case "common_event":
                case "map_event":
                case "switch":
                case "variable":
                case "animation":
                case "actor":
                case "class":
                case "skill":
                case "item":
                case "weapon":
                case "armor":
                    return typeof(string);
                default:
                    return typeof(string);
            }

        }

        public static bool IsArray2Postfix(string typeName) {
            if (typeName.Length >= 4 && typeName.Substring(typeName.Length - 4) == "[][]")
            {
                return true;
            }
            return false;
        }

        public static bool IsArrayPostfix(string typeName) {
            if (typeName.Length >= 2 && typeName.Substring(typeName.Length - 2) == "[]")
            {
                return true;
            }
            return false;
        }

        private static Dictionary<string, ParamType> _typeNameParamTypeDic = new Dictionary<string, ParamType>(){
            {"string", ParamType.String},
            {"boolean", ParamType.Boolean},
            {"number", ParamType.Number},
            {"integer", ParamType.Integer},
            {"select", ParamType.Select},
            {"combo", ParamType.Combo},
            {"multiline_string", ParamType.MultilineString},
            {"note", ParamType.Note},
            {"common_event", ParamType.CommonEvent},
            {"map_event", ParamType.MapEvent},
            {"switch", ParamType.Switch},
            {"variable", ParamType.Variable},
            {"animation", ParamType.Animation},
            {"actor", ParamType.Actor},
            {"class", ParamType.Class},
            {"skill", ParamType.Skill},
            {"item", ParamType.Item},
            {"weapon", ParamType.Weapon},
            {"armor", ParamType.Armor},
            {"enemy", ParamType.Enemy},
            {"troop", ParamType.Troop},
            {"state", ParamType.State},
            {"tileset", ParamType.Tileset},
            {"file", ParamType.File},
        };
        public static ParamType GetParamType(string typeName, out int arrayDimension, out string structName) {
            arrayDimension = 0;
            structName = null;
            if (IsArray2Postfix(typeName))
            {
                arrayDimension = 2;
                typeName = typeName.Substring(0, typeName.Length - 4);
            }
            else if (IsArrayPostfix(typeName))
            {
                arrayDimension = 1;
                typeName = typeName.Substring(0, typeName.Length - 2);
            }
            if (typeName.Length >= 9 && typeName.Substring(0, 7) == "struct<" && typeName.Substring(typeName.Length - 1) == ">")
            {
                structName = typeName.Substring(7, typeName.Length - (7 + 1));
                return ParamType.Struct;
            }
            if (_typeNameParamTypeDic.ContainsKey(typeName))
            {
                return _typeNameParamTypeDic[typeName];
            }
            Debug.LogWarning($"Unknown type name: {typeName}");
            return ParamType.String;
        }

        public static string GetTypeName(ParamType paramType) {
            foreach (var item in _typeNameParamTypeDic)
            {
                if (item.Value == paramType)
                {
                    return item.Key;
                }
            }
            return null;
        }

#if UNITY_EDITOR
        static bool IsStringType(string typeName) {
            return (typeName == "string" || typeName == "multiline_string");
        }

        static bool _checkCircularReference = false;
        static HashSet<string> _validatingStructNameSet = new HashSet<string>();
        static HashSet<string> _errorReportedStructNameSet = new HashSet<string>();
        public static string ValidateValue(AddonInfo addonInfo, AddonParamInfo paramInfo, string typeName, string valueStr) {
            int arrayDimension = 0;
            string structName = null;
            var paramType = GetParamType(typeName, out arrayDimension, out structName);
            if (arrayDimension == 2)
            {
                var jsonNode = JSON.Parse(valueStr);
                if (!jsonNode.IsArray)
                {
                    return new JSONArray();
                }
                var jsonArray = jsonNode.AsArray;
                var baseTypeName = typeName.Substring(0, typeName.Length - 4);
                for (int i = 0; i < jsonArray.Count; i++)
                {
                    if (!jsonArray[i].IsArray)
                    {
                        jsonArray[i] = new JSONArray();
                        continue;
                    }
                    var jsonArray2 = jsonArray[i].AsArray;
                    for (int j = 0; j < jsonArray2.Count; j++)
                    {
                        jsonArray2[j] = GetJsonValue(addonInfo, paramInfo, baseTypeName, IsStringType(baseTypeName) ? jsonArray2[j].Value : jsonArray2[j].ToString());
                    }
                    jsonArray[i] = jsonArray2;
                }
                return jsonArray.ToString();
            }
            else if (arrayDimension == 1)
            {
                var jsonNode = JSON.Parse(valueStr);
                if (!jsonNode.IsArray)
                {
                    return new JSONArray();
                }
                var jsonArray = jsonNode.AsArray;
                var baseTypeName = typeName.Substring(0, typeName.Length - 2);
                for (int i = 0; i < jsonArray.Count; i++)
                {
                    jsonArray[i] = GetJsonValue(addonInfo, paramInfo, baseTypeName, IsStringType(baseTypeName) ? jsonArray[i].Value : jsonArray[i].ToString());
                }
                return jsonArray.ToString();
            }
            else if (structName != null)
            {
                var structInfo = addonInfo.structInfos.GetStructInfo(structName);
                if (structInfo == null)
                {
                    Debug.LogWarning($"struct not declared: {structName}");
                    return "{}";
                }
                if (_checkCircularReference && _validatingStructNameSet.Contains(structName))
                {
                    if (!_errorReportedStructNameSet.Contains(structName))
                    {
                        _errorReportedStructNameSet.Add(structName);
                        Debug.LogError($"There is a circular reference to an element of ~struct~{structName}>");
                    }
                    return "{}";
                }
                _validatingStructNameSet.Add(structName);
                var jsonNode = JSON.Parse(valueStr);
                if (!jsonNode.IsObject)
                {
                    jsonNode = new JSONObject();
                }
                var jsonObject = jsonNode.AsObject;
                foreach (var paramParamInfo in structInfo.params_)
                {
                    var key = paramParamInfo.name;
                    var tn = paramParamInfo.GetInfo("type")?.value;
                    jsonObject[key] = GetJsonValue(addonInfo, paramParamInfo, tn, (jsonObject.HasKey(key) ? IsStringType(tn) ? jsonObject[key].Value : jsonObject[key].ToString() : ""));
                }
                _validatingStructNameSet.Remove(structName);
                return jsonObject.ToString();
            }
            else
            {
                return ValidateValue(paramInfo, paramType, valueStr);
            }
        }

        public static string ValidateValue(AddonParamInfo paramInfo, ParamType paramType, string valueStr) {
            var minParam = paramInfo.GetInfo("min");
            var maxParam = paramInfo.GetInfo("max");
            int arrayDimension = 0;
            string structName = null;
            GetParamType(paramInfo.GetInfo("type")?.value ?? "string", out arrayDimension, out structName);
            AddonParameter defaultInfo = null;
            if (arrayDimension == 0)
            {
                defaultInfo = paramInfo.GetInfo("default");
            }
            switch (paramType)
            {
                case ParamType.Boolean:
                    {
                        int intValue = 0;
                        bool boolValue = false;
                        bool result = false;
                        if (defaultInfo != null)
                        {
                            result = bool.Parse(defaultInfo.value);
                        }
                        if (valueStr == "ON")
                        {
                            result = true;

                        }
                        else
                        if (valueStr == "OFF")
                        {
                            result = false;

                        }
                        var onInfo = paramInfo.GetInfo("on");
                        if (onInfo != null && onInfo.value == valueStr)
                        {
                            result = true;
                        }
                        var offInfo = paramInfo.GetInfo("off");
                        if (offInfo != null && offInfo.value == valueStr)
                        {
                            result = false;
                        }
                        else if (int.TryParse(valueStr, out intValue))
                        {
                            result = ((intValue == 0) ? false : true);
                        }
                        else
                        if (bool.TryParse(valueStr, out boolValue))
                        {
                            result = boolValue;
                        }
                        return result.ToString();
                    }

                case ParamType.Integer:
                    {
                        int intValue = 0;
                        if (defaultInfo != null)
                        {
                            intValue = int.Parse(defaultInfo.value);
                        }
                        int intValue2 = 0;
                        if (!int.TryParse(valueStr, out intValue2))
                        {
                            return intValue.ToString();
                        }
                        intValue = intValue2;
                        if (minParam != null)
                        {
                            var minValue = int.Parse(minParam.value);
                            if (intValue < minValue)
                            {
                                intValue = minValue;
                            }
                        }
                        if (maxParam != null)
                        {
                            var maxValue = int.Parse(maxParam.value);
                            if (intValue > maxValue)
                            {
                                intValue = maxValue;
                            }
                        }
                        return intValue.ToString();
                    }

                case ParamType.Number:
                    {
                        double doubleValue = 0;
                        if (defaultInfo != null)
                        {
                            doubleValue = double.Parse(defaultInfo.value);
                        }
                        double doubleValue2 = 0;
                        if (!double.TryParse(valueStr, out doubleValue2))
                        {
                            return doubleValue.ToString();
                        }
                        doubleValue = doubleValue2;
                        if (minParam != null)
                        {
                            var minValue = double.Parse(minParam.value);
                            if (doubleValue < minValue)
                            {
                                doubleValue = minValue;
                            }
                        }
                        if (maxParam != null)
                        {
                            var maxValue = double.Parse(maxParam.value);
                            if (doubleValue > maxValue)
                            {
                                doubleValue = maxValue;
                            }
                        }
                        var dstr = doubleValue.ToString();
                        var decimalsParam = paramInfo.GetInfo("decimals");
                        if (decimalsParam != null)
                        {
                            var pointIndex = dstr.IndexOf('.');
                            if (pointIndex >= 0)
                            {
                                if (dstr.Length - pointIndex > int.Parse(decimalsParam.value) + 1)
                                {
                                    var format = "{" + $"0:N{decimalsParam.value}" + "}";
                                    //Debug.Log($"format: {format}");
                                    return string.Format(format, doubleValue);
                                }
                            }
                        }
                        return dstr;
                    }

                case ParamType.Select:
                    {
                        string result = "0";
                        if (defaultInfo != null)
                        {
                            result = defaultInfo.value;
                        }
                        bool boolValue = false;
                        if (bool.TryParse(valueStr, out boolValue))
                        {
                            valueStr = boolValue ? "1" : "0";
                        }
                        foreach (var option in paramInfo.options)
                        {
                            if (option.key == valueStr || option.value == valueStr)
                            {
                                result = option.value;
                                break;
                            }
                        }
                        return result;
                    }

                case ParamType.Combo:
                    return valueStr;

                case ParamType.String:
                case ParamType.MultilineString:
                    return valueStr;

                case ParamType.Note:
                    {
                        var result = "";
                        if (defaultInfo != null)
                        {
                            result = defaultInfo.value;
                        }
                        if (valueStr == null)
                        {
                            return result;
                        }
                        return valueStr;
                    }

                case ParamType.CommonEvent:
                    {
                        var result = "";
                        if (defaultInfo != null)
                        {
                            result = defaultInfo.value;
                        }
                        if (valueStr == null)
                        {
                            return result;
                        }
                        var eventCommonDataModels = new EventManagementService().LoadEventCommon();
                        int serialNumber = -1;
                        if (!int.TryParse(valueStr, out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var eventCommonDataModel = eventCommonDataModels.FirstOrDefault(x => x.SerialNumber == serialNumber || x.name == valueStr);
                        if (eventCommonDataModel != null)
                        {
                            valueStr = eventCommonDataModel.eventId;
                        }
                        return valueStr;
                    }

                case ParamType.MapEvent:
                    {
                        var result = "[\"\",\"\"]";
                        if (defaultInfo != null)
                        {
                            var arr = DataConverter.GetStringArrayFromJson(defaultInfo.value);
                            if (arr.Length >= 2)
                            {
                                result = DataConverter.GetJsonStringArray(arr);
                            }
                        }
                        var list = DataConverter.GetStringArrayFromJson(valueStr)?.ToList();
                        if (list == null)
                        {
                            return result;
                        }
                        while (list.Count < 2)
                        {
                            list.Add("");
                        }
                        if (list.Count > 2)
                        {
                            list.RemoveRange(2, list.Count - 2);
                        }
                        var mapEntities = new MapManagementService().LoadMaps();
                        int serialNumber = -1;
                        if (!int.TryParse(list[0], out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var mapEntity = mapEntities.FirstOrDefault(x => x.SerialNumber == serialNumber || x.name == list[0]);
                        if (mapEntity != null)
                        {
                            list[0] = mapEntity.id;
                        }
                        var eventMapDataModels = new EventManagementService().LoadEventMap();
                        var eventMapEntities = eventMapDataModels.FindAll(x => x.mapId == list[0]);
                        if (!int.TryParse(list[1], out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        if (list[1] != AddonManager.PlayerEventId && list[1] != AddonManager.ThisEventId)
                        {
                            var eventMapEntity = eventMapEntities.FirstOrDefault(x => x.SerialNumber == serialNumber || (list[1].Length > 0 && x.name == list[1]));
                            if (eventMapEntity != null)
                            {
                                list[1] = eventMapEntity.eventId;
                            }
                        }
                        return DataConverter.GetJsonStringArray(list.ToArray());
                    }

                case ParamType.Switch:
                    {
                        var flags = new DatabaseManagementService().LoadFlags();
                        int serialNumber = -1;
                        if (!int.TryParse(valueStr, out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var sw = flags.switches.FirstOrDefault(x => x.SerialNumber == serialNumber || x.name == valueStr);
                        if (sw != null)
                        {
                            valueStr = sw.id;
                        }
                        return valueStr;
                    }

                case ParamType.Variable:
                    {
                        var flags = new DatabaseManagementService().LoadFlags();
                        int serialNumber = -1;
                        if (!int.TryParse(valueStr, out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var v = flags.variables.FirstOrDefault(x => x.SerialNumber == serialNumber || x.name == valueStr);
                        if (v != null)
                        {
                            valueStr = v.id;
                        }
                        return valueStr;
                    }

                case ParamType.Animation:
                    {
                        var animations = new DatabaseManagementService().LoadAnimation();
                        int serialNumber = -1;
                        if (!int.TryParse(valueStr, out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var animation = animations.FirstOrDefault(x => x.SerialNumber == serialNumber || x.particleName == valueStr);
                        if (animation != null)
                        {
                            valueStr = animation.id;
                        }
                        return valueStr;
                    }

                case ParamType.Actor:
                    {
                        var actors = new DatabaseManagementService().LoadCharacterActor();
                        int serialNumber = -1;
                        if (!int.TryParse(valueStr, out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var actor = actors.FirstOrDefault(x => x.SerialNumber == serialNumber || x.basic.name == valueStr);
                        if (actor != null)
                        {
                            valueStr = actor.uuId;
                        }
                        return valueStr;
                    }

                case ParamType.Class:
                    {
                        var classes = new DatabaseManagementService().LoadCharacterActorClass();
                        int serialNumber = -1;
                        if (!int.TryParse(valueStr, out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var class_ = classes.FirstOrDefault(x => x.SerialNumber == serialNumber || x.basic.name == valueStr);
                        if (class_ != null)
                        {
                            valueStr = class_.id;
                        }
                        return valueStr;
                    }

                case ParamType.Skill:
                    {
                        var skills = new DatabaseManagementService().LoadSkillCustom();
                        int serialNumber = -1;
                        if (!int.TryParse(valueStr, out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var skill = skills.FirstOrDefault(x => (x.basic.id != "1" && x.basic.id != "2") && (x.SerialNumber == serialNumber || x.basic.name == valueStr));
                        if (skill != null)
                        {
                            valueStr = skill.basic.id;
                        }
                        return valueStr;
                    }

                case ParamType.Item:
                    {
                        var items = new DatabaseManagementService().LoadItem();
                        int serialNumber = -1;
                        if (!int.TryParse(valueStr, out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var item = items.FirstOrDefault(x => x.SerialNumber == serialNumber || x.basic.name == valueStr);
                        if (item != null)
                        {
                            valueStr = item.basic.id;
                        }
                        return valueStr;
                    }

                case ParamType.Weapon:
                    {
                        var weapons = new DatabaseManagementService().LoadWeapon();
                        int serialNumber = -1;
                        if (!int.TryParse(valueStr, out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var weapon = weapons.FirstOrDefault(x => x.SerialNumber == serialNumber || x.basic.name == valueStr);
                        if (weapon != null)
                        {
                            valueStr = weapon.basic.id;
                        }
                        return valueStr;
                    }

                case ParamType.Armor        :
                    {
                        var armors = new DatabaseManagementService().LoadArmor();
                        int serialNumber = -1;
                        if (!int.TryParse(valueStr, out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var armor = armors.FirstOrDefault(x => x.SerialNumber == serialNumber || x.basic.name == valueStr);
                        if (armor != null)
                        {
                            valueStr = armor.basic.id;
                        }
                        return valueStr;
                    }

                case ParamType.Enemy:
                    {
                        var enemies = new DatabaseManagementService().LoadEnemy();
                        int serialNumber = -1;
                        if (!int.TryParse(valueStr, out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var enemy = enemies.FirstOrDefault(x => x.SerialNumber == serialNumber || x.name == valueStr);
                        if (enemy != null)
                        {
                            valueStr = enemy.id;
                        }
                        return valueStr;
                    }

                case ParamType.Troop:
                    {
                        var troops = new DatabaseManagementService().LoadTroop();
                        int serialNumber = -1;
                        if (!int.TryParse(valueStr, out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var troop = troops.FirstOrDefault(x => x.SerialNumber == serialNumber || x.name == valueStr);
                        if (troop != null)
                        {
                            valueStr = troop.id;
                        }
                        return valueStr;
                    }

                case ParamType.State:
                    {
                        var states = new DatabaseManagementService().LoadStateEdit();
                        int serialNumber = -1;
                        if (!int.TryParse(valueStr, out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var state = states.FirstOrDefault(x => x.SerialNumber == serialNumber || x.name == valueStr);
                        if (state != null)
                        {
                            valueStr = state.id;
                        }
                        return valueStr;
                    }

                case ParamType.Tileset:
                    {
                        var tilesets = new MapManagementService().LoadTileGroups();
                        int serialNumber = -1;
                        if (!int.TryParse(valueStr, out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var tileset = tilesets.FirstOrDefault(x => x.SerialNumber == serialNumber || x.name == valueStr);
                        if (tileset != null)
                        {
                            valueStr = tileset.id;
                        }
                        return valueStr;
                    }

                case ParamType.File:
                    return valueStr;

                default:
                    Debug.LogError($"Validate not implemented: {paramType.ToString()}");
                    return valueStr;
            }
        }

#endif

        public AddonInfo GetAddonInfo(string addonName) {
            var addonInfo = _addonInfos.FirstOrDefault(x => x.name == addonName);
            return addonInfo;
        }

        public AddonInfoContainer GetAddonInfos() {
            return _addonInfos;
        }

        public static string GetInitialValue(AddonParameterContainer parameters, AddonParamInfo paramInfo) {
            var value = parameters.GetParameterValue(paramInfo.name);
            if (value == null)
            {
                var defaultInfo = paramInfo.GetInfo("default");
                if (defaultInfo != null)
                {
                    value = defaultInfo.value;
                }
            }
            return value;
        }

        public static bool ApplyAddonInfoToModel(AddonDataModel addonDataModel, AddonInfo addonInfo) {
            var modified = false;
            if (addonInfo.name != addonDataModel.Name)
            {
                addonDataModel.Name = addonInfo.name;
                modified = true;
            }
            if (addonInfo.addondesc != addonDataModel.Description)
            {
                addonDataModel.Description = addonInfo.addondesc;
                modified = true;
            }
            var i = 0;
            while (i < addonDataModel.Parameters.Count)
            {
                var key = addonDataModel.Parameters[i].key;
                if (addonInfo.paramInfos.GetParamInfo(key) != null)
                {
                    i++;
                    continue;
                }
                addonDataModel.Parameters.RemoveAt(i);
                modified = true;
            }
            /*foreach (var paramInfo in addonInfo.paramInfos){
                var parameter = addonDataModel.Parameters.FirstOrDefault(x => x.key == paramInfo.name);
                if (parameter != null){
                    continue;
                }
                addonDataModel.Parameters.Add(new AddonParameter(paramInfo.name, null));
                modified = true;
            }*/
            return modified;
        }


        private Type[] GetTypes(AddonParamInfoContainer paramInfos) {
            var types = new Type[paramInfos.Count];
            for (int i = 0; i < paramInfos.Count; i++)
            {
                var typeInfo = paramInfos[i].GetInfo("type");
                var typeName = typeInfo != null ? typeInfo.value : "string";
                if (!_parameterTypeConvertDic.ContainsKey(typeName))
                {
                    typeName = "string";
                }
                types[i] = GetParameterType(typeName);
            }
            return types;
        }

#if UNITY_EDITOR
        public static AddonParameterContainer GetStructParameters(AddonStructInfo structInfo, string structValue) {
            var parameters = new AddonParameterContainer();
            if (structValue == null)
            {
                return parameters;
            }
            var jsonNode = JSON.Parse(structValue);
            JSONObject jsonObj = (jsonNode.IsObject ? jsonNode.AsObject : null);
            foreach (var param in structInfo.params_)
            {
                string v = null;
                if (jsonObj != null)
                {
                    v = jsonObj[param.name].ToString();
                }
                parameters.SetParameterValue(param.name, v);
            }
            return parameters;
        }

        public static string GetStructValue(AddonStructInfo structInfo, AddonParameterContainer parameters) {
            var jo = new JSONObject();
            foreach (var parameter in parameters)
            {
                jo.Add(parameter.key, JSON.Parse(parameter.value??""));
            }
            return jo.ToString();
        }
        
        public static List<string> GetStringListFromJson(string jsonStr) {
            var list = new List<string>();
            var jsonNode = JSON.Parse(jsonStr);
            if (!jsonNode.IsArray)
            {
                return list;
            }
            foreach (var node in jsonNode.AsArray)
            {
                list.Add(node.Value.ToString());
            }
            return list;
        }

        public static string GetJsonFromStringList(List<string> list) {
            var jsonArray = new JSONArray();
            foreach (var str in list)
            {
                jsonArray.Add(JSON.Parse(str));
            }
            return jsonArray.ToString();
        }

        public static JSONNode GetJsonValue(AddonInfo addonInfo, AddonParamInfo paramInfo, string typeName, string valueStr) {
            int arrayDimension = 0;
            string structName = null;
            var paramType = GetParamType(typeName, out arrayDimension, out structName);
            if (arrayDimension == 2)
            {
                var jsonNode = JSON.Parse(valueStr);
                if (!jsonNode.IsArray)
                {
                    return new JSONArray();
                }
                var jsonArray = jsonNode.AsArray;
                var baseTypeName = typeName.Substring(0, typeName.Length - 4);
                for (int i = 0; i < jsonArray.Count; i++)
                {
                    if (!jsonArray[i].IsArray)
                    {
                        jsonArray[i] = new JSONArray();
                        continue;
                    }
                    var jsonArray2 = jsonArray[i].AsArray;
                    for (int j = 0; j < jsonArray2.Count; j++)
                    {
                        jsonArray2[j] = GetJsonValue(addonInfo, paramInfo, baseTypeName, IsStringType(baseTypeName) ? jsonArray2[j].Value : jsonArray2[j].ToString());
                    }
                    jsonArray[i] = jsonArray2;
                }
                return jsonArray;
            }
            else if (arrayDimension == 1)
            {
                var jsonNode = JSON.Parse(valueStr);
                if (!jsonNode.IsArray)
                {
                    return new JSONArray();
                }
                var jsonArray = jsonNode.AsArray;
                var baseTypeName = typeName.Substring(0, typeName.Length - 2);
                for (int i = 0; i < jsonArray.Count; i++)
                {
                    jsonArray[i] = GetJsonValue(addonInfo, paramInfo, baseTypeName, IsStringType(baseTypeName) ? jsonArray[i].Value : jsonArray[i].ToString());
                }
                return jsonArray;
            }
            else if (structName != null)
            {
                var structInfo = addonInfo.structInfos.GetStructInfo(structName);
                if (structInfo == null)
                {
                    Debug.LogWarning($"struct not declared: {structName}");
                    return new JSONObject();
                }
                if (_checkCircularReference && _validatingStructNameSet.Contains(structName))
                {
                    if (!_errorReportedStructNameSet.Contains(structName))
                    {
                        _errorReportedStructNameSet.Add(structName);
                        Debug.LogError($"There is a circular reference to an element of ~struct~{structName}>");
                    }
                    return new JSONObject();
                }
                _validatingStructNameSet.Add(structName);
                var jsonNode = JSON.Parse(valueStr);
                if (!jsonNode.IsObject)
                {
                    jsonNode = new JSONObject();
                }
                var jsonObject = jsonNode.AsObject;
                var keys = new List<string>();
                foreach (var key in jsonObject.Keys)
                {
                    keys.Add(key);
                }
                foreach (var key in keys)
                {
                    if (jsonObject.HasKey(key))
                    {
                        var paramParamInfo = structInfo.params_.GetParamInfo(key);
                        var tn = paramParamInfo.GetInfo("type")?.value;
                        jsonObject[key] = GetJsonValue(addonInfo, paramParamInfo, tn, IsStringType(tn) ? jsonObject[key].Value : jsonObject[key].ToString());
                    }
                }
                _validatingStructNameSet.Remove(structName);
                return jsonObject;
            }
            else
            {
                return GetJsonValue(paramInfo, paramType, valueStr);
            }
        }

        public static JSONNode GetJsonValue(AddonParamInfo paramInfo, ParamType paramType, string valueStr) {
            var minParam = paramInfo.GetInfo("min");
            var maxParam = paramInfo.GetInfo("max");
            int arrayDimension = 0;
            string structName = null;
            GetParamType(paramInfo.GetInfo("type")?.value ?? "string", out arrayDimension, out structName);
            AddonParameter defaultInfo = null;
            if (arrayDimension == 0)
            {
                defaultInfo = paramInfo.GetInfo("default");
            }
            switch (paramType)
            {
                case ParamType.Boolean:
                    {
                        int intValue = 0;
                        bool boolValue = false;
                        bool result = false;
                        if (defaultInfo != null)
                        {
                            result = bool.Parse(defaultInfo.value);
                        }
                        if (valueStr == "ON")
                        {
                            result = true;

                        }
                        else
                        if (valueStr == "OFF")
                        {
                            result = false;

                        }
                        var onInfo = paramInfo.GetInfo("on");
                        if (onInfo != null && onInfo.value == valueStr)
                        {
                            result = true;
                        }
                        var offInfo = paramInfo.GetInfo("off");
                        if (offInfo != null && offInfo.value == valueStr)
                        {
                            result = false;
                        }
                        else if (int.TryParse(valueStr, out intValue))
                        {
                            result = ((intValue == 0) ? false : true);
                        }
                        else
                        if (bool.TryParse(valueStr, out boolValue))
                        {
                            result = boolValue;
                        }
                        return new JSONBool(result);
                    }

                case ParamType.Integer:
                    {
                        int intValue = 0;
                        if (defaultInfo != null)
                        {
                            intValue = int.Parse(defaultInfo.value);
                        }
                        int intValue2 = 0;
                        if (!int.TryParse(valueStr, out intValue2))
                        {
                            return new JSONNumber(intValue);
                        }
                        intValue = intValue2;
                        if (minParam != null)
                        {
                            var minValue = int.Parse(minParam.value);
                            if (intValue < minValue)
                            {
                                intValue = minValue;
                            }
                        }
                        if (maxParam != null)
                        {
                            var maxValue = int.Parse(maxParam.value);
                            if (intValue > maxValue)
                            {
                                intValue = maxValue;
                            }
                        }
                        return new JSONNumber(intValue);
                    }

                case ParamType.Number:
                    {
                        double doubleValue = 0;
                        if (defaultInfo != null)
                        {
                            doubleValue = double.Parse(defaultInfo.value);
                        }
                        double doubleValue2 = 0;
                        if (!double.TryParse(valueStr, out doubleValue2))
                        {
                            return new JSONNumber(doubleValue);
                        }
                        doubleValue = doubleValue2;
                        if (minParam != null)
                        {
                            var minValue = double.Parse(minParam.value);
                            if (doubleValue < minValue)
                            {
                                doubleValue = minValue;
                            }
                        }
                        if (maxParam != null)
                        {
                            var maxValue = double.Parse(maxParam.value);
                            if (doubleValue > maxValue)
                            {
                                doubleValue = maxValue;
                            }
                        }
                        var decimalsParam = paramInfo.GetInfo("decimals");
                        if (decimalsParam != null)
                        {
                            var dstr = doubleValue.ToString();
                            var pointIndex = dstr.IndexOf('.');
                            if (pointIndex >= 0)
                            {
                                if (dstr.Length - pointIndex > int.Parse(decimalsParam.value) + 1)
                                {
                                    var format = "{" + $"0:N{decimalsParam.value}" + "}";
                                    //Debug.Log($"format: {format}");
                                    return new JSONNumber(double.Parse(string.Format(format, doubleValue)));
                                }
                            }
                        }
                        return new JSONNumber(doubleValue);
                    }

                case ParamType.Select:
                    {
                        string result = "0";
                        if (defaultInfo != null)
                        {
                            result = defaultInfo.value;
                        }
                        bool boolValue = false;
                        if (bool.TryParse(valueStr, out boolValue))
                        {
                            valueStr = boolValue ? "1" : "0";
                        }
                        foreach (var option in paramInfo.options)
                        {
                            if (option.key == valueStr || option.value == valueStr)
                            {
                                result = option.value;
                                break;
                            }
                        }
                        return new JSONNumber(int.Parse(result));
                    }

                case ParamType.Combo:
                    return new JSONString(valueStr);

                case ParamType.String:
                case ParamType.MultilineString:
                    return new JSONString(valueStr);

                case ParamType.Note:
                    {
                        var result = "";
                        if (defaultInfo != null)
                        {
                            result = defaultInfo.value;
                        }
                        var jsonNode = JSON.Parse(valueStr);
                        if (jsonNode == null)
                        {
                            return new JSONString(result);
                        }
                        return jsonNode;
                    }

                case ParamType.CommonEvent:
                    {
                        var result = "";
                        if (defaultInfo != null)
                        {
                            result = defaultInfo.value;
                        }
                        if (valueStr == null)
                        {
                            return new JSONString(result);
                        }
                        var eventCommonDataModels = new EventManagementService().LoadEventCommon();
                        int serialNumber = -1;
                        if (!int.TryParse(valueStr, out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var eventCommonDataModel = eventCommonDataModels.FirstOrDefault(x => x.SerialNumber == serialNumber || x.name == valueStr);
                        if (eventCommonDataModel != null)
                        {
                            valueStr = eventCommonDataModel.eventId;
                        }
                        return new JSONString(valueStr);
                    }

                case ParamType.MapEvent:
                    {
                        var result = "[\"\",\"\"]";
                        if (defaultInfo != null)
                        {
                            var arr = DataConverter.GetStringArrayFromJson(defaultInfo.value);
                            if (arr.Length >= 2)
                            {
                                result = DataConverter.GetJsonStringArray(arr);
                            }
                        }
                        var jsonNode = JSON.Parse(valueStr);
                        if (!jsonNode.IsArray)
                        {
                            return JSON.Parse(result);
                        }
                        var jsonArray = jsonNode.AsArray;
                        while (jsonArray.Count < 2)
                        {
                            jsonArray.Add(new JSONString(""));
                        }
                        while (jsonArray.Count > 2)
                        {
                            jsonArray.Remove(jsonArray.Count - 1);
                        }
                        var mapEntities = new MapManagementService().LoadMaps();
                        int serialNumber = -1;
                        if (jsonArray[0].IsNumber)
                        {
                            serialNumber = jsonArray[0].AsInt;
                        }
                        var mapEntity = mapEntities.FirstOrDefault(x => x.SerialNumber == serialNumber || x.name == jsonArray[0].Value);
                        if (mapEntity != null)
                        {
                            jsonArray[0] = new JSONString(mapEntity.id);
                        }
                        var eventMapDataModels = new EventManagementService().LoadEventMap();
                        var eventMapEntities = eventMapDataModels.FindAll(x => x.mapId == jsonArray[0].Value);
                        if (jsonArray[1].IsNumber)
                        {
                            serialNumber = jsonArray[1].AsInt;
                        }
                        if (jsonArray[1].Value != AddonManager.PlayerEventId && jsonArray[1].Value != AddonManager.ThisEventId)
                        {
                            var eventMapEntity = eventMapEntities.FirstOrDefault(x => x.SerialNumber == serialNumber || x.name == jsonArray[1].Value);
                            if (eventMapEntity != null)
                            {
                                jsonArray[1] = new JSONString(eventMapEntity.eventId);
                            }
                        }
                        return jsonArray;
                    }

                case ParamType.Switch:
                    {
                        var flags = new DatabaseManagementService().LoadFlags();
                        int serialNumber = -1;
                        if (!int.TryParse(valueStr, out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var sw = flags.switches.FirstOrDefault(x => x.SerialNumber == serialNumber || x.name == valueStr);
                        if (sw != null)
                        {
                            valueStr = sw.id;
                        }
                        return new JSONString(valueStr);
                    }

                case ParamType.Variable:
                    {
                        var flags = new DatabaseManagementService().LoadFlags();
                        int serialNumber = -1;
                        if (!int.TryParse(valueStr, out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var v = flags.variables.FirstOrDefault(x => x.SerialNumber == serialNumber || x.name == valueStr);
                        if (v != null)
                        {
                            valueStr = v.id;
                        }
                        return new JSONString(valueStr);
                    }

                case ParamType.Animation:
                    {
                        var animations = new DatabaseManagementService().LoadAnimation();
                        int serialNumber = -1;
                        if (!int.TryParse(valueStr, out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var animation = animations.FirstOrDefault(x => x.SerialNumber == serialNumber || x.particleName == valueStr);
                        if (animation != null)
                        {
                            valueStr = animation.id;
                        }
                        return valueStr;
                    }

                case ParamType.Actor:
                    {
                        var actors = new DatabaseManagementService().LoadCharacterActor();
                        int serialNumber = -1;
                        if (!int.TryParse(valueStr, out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var actor = actors.FirstOrDefault(x => x.SerialNumber == serialNumber || x.basic.name == valueStr);
                        if (actor != null)
                        {
                            valueStr = actor.uuId;
                        }
                        return new JSONString(valueStr);
                    }

                case ParamType.Class:
                    {
                        var classes = new DatabaseManagementService().LoadCharacterActorClass();
                        int serialNumber = -1;
                        if (!int.TryParse(valueStr, out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var class_ = classes.FirstOrDefault(x => x.SerialNumber == serialNumber || x.basic.name == valueStr);
                        if (class_ != null)
                        {
                            valueStr = class_.id;
                        }
                        return new JSONString(valueStr);
                    }

                case ParamType.Skill:
                    {
                        var skills = new DatabaseManagementService().LoadSkillCustom();
                        int serialNumber = -1;
                        if (!int.TryParse(valueStr, out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var skill = skills.FirstOrDefault(x => (x.basic.id != "1" && x.basic.id != "2") && (x.SerialNumber == serialNumber || x.basic.name == valueStr));
                        if (skill != null)
                        {
                            valueStr = skill.basic.id;
                        }
                        return new JSONString(valueStr);
                    }

                case ParamType.Item:
                    {
                        var items = new DatabaseManagementService().LoadItem();
                        int serialNumber = -1;
                        if (!int.TryParse(valueStr, out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var item = items.FirstOrDefault(x => x.SerialNumber == serialNumber || x.basic.name == valueStr);
                        if (item != null)
                        {
                            valueStr = item.basic.id;
                        }
                        return new JSONString(valueStr);
                    }

                case ParamType.Weapon:
                    {
                        var weapons = new DatabaseManagementService().LoadWeapon();
                        int serialNumber = -1;
                        if (!int.TryParse(valueStr, out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var weapon = weapons.FirstOrDefault(x => x.SerialNumber == serialNumber || x.basic.name == valueStr);
                        if (weapon != null)
                        {
                            valueStr = weapon.basic.id;
                        }
                        return new JSONString(valueStr);
                    }

                case ParamType.Armor:
                    {
                        var armors = new DatabaseManagementService().LoadArmor();
                        int serialNumber = -1;
                        if (!int.TryParse(valueStr, out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var armor = armors.FirstOrDefault(x => x.SerialNumber == serialNumber || x.basic.name == valueStr);
                        if (armor != null)
                        {
                            valueStr = armor.basic.id;
                        }
                        return new JSONString(valueStr);
                    }

                case ParamType.Enemy:
                    {
                        var enemies = new DatabaseManagementService().LoadEnemy();
                        int serialNumber = -1;
                        if (!int.TryParse(valueStr, out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var enemy = enemies.FirstOrDefault(x => x.SerialNumber == serialNumber || x.name == valueStr);
                        if (enemy != null)
                        {
                            valueStr = enemy.id;
                        }
                        return valueStr;
                    }

                case ParamType.Troop:
                    {
                        var troops = new DatabaseManagementService().LoadTroop();
                        int serialNumber = -1;
                        if (!int.TryParse(valueStr, out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var troop = troops.FirstOrDefault(x => x.SerialNumber == serialNumber || x.name == valueStr);
                        if (troop != null)
                        {
                            valueStr = troop.id;
                        }
                        return new JSONString(valueStr);
                    }

                case ParamType.State:
                    {
                        var states = new DatabaseManagementService().LoadStateEdit();
                        int serialNumber = -1;
                        if (!int.TryParse(valueStr, out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var state = states.FirstOrDefault(x => x.SerialNumber == serialNumber || x.name == valueStr);
                        if (state != null)
                        {
                            valueStr = state.id;
                        }
                        return new JSONString(valueStr);
                    }

                case ParamType.Tileset:
                    {
                        var tilesets = new MapManagementService().LoadTileGroups();
                        int serialNumber = -1;
                        if (!int.TryParse(valueStr, out serialNumber))
                        {
                            serialNumber = -1;
                        }
                        var tileset = tilesets.FirstOrDefault(x => x.SerialNumber == serialNumber || x.name == valueStr);
                        if (tileset != null)
                        {
                            valueStr = tileset.id;
                        }
                        return new JSONString(valueStr);
                    }

                case ParamType.File:
                    return new JSONString(valueStr);

                default:
                    Debug.LogError($"GetJsonValue not implemented: {paramType.ToString()}");
                    return new JSONString(valueStr);
            }
        }
#endif

        private void InstantiateAddons() {
            _addonInstances = new AddonInstanceContainer();
            var assembly = typeof(AddonManager).Assembly;
            foreach (var admOrg in _addonDataModels)
            {
                if (!admOrg.Status)
                {
                    //_addonInstances.Add(new AddonInstance(null, adm, types));
                    continue;
                }
                var addonInfo = _addonInfos.GetAddonInfo(admOrg.Name);
                var adm = AddonDataModel.Create();
                admOrg.CopyTo(adm);

                var type = assembly.GetType($"RPGMaker.Codebase.Addon.{adm.Name}");
                //Debug.Log($"type:{type}");
                var types = GetTypes(addonInfo.paramInfos);
                var ctor = type.GetConstructor(types);
                if (ctor == null)
                {
                    Debug.LogError($"Add-on constructor {adm.Name}({string.Join(", ", types.Select(x => x.Name).ToList())}) not found.");
                    _addonInstances.Add(new AddonInstance(null, adm, types));
                    continue;
                }
                var params_ = new object[addonInfo.paramInfos.Count];
                for (int i = 0; i < params_.Length; i++)
                {
                    var typeName = addonInfo.paramInfos[i].GetInfo("type");
                    var value = GetInitialValue(adm.Parameters, addonInfo.paramInfos[i]);
                    if (typeName != null && _parameterTypeConvertDic.ContainsKey(typeName.value))
                    {
                        params_[i] = _parameterTypeConvertDic[typeName.value](value);
                    }
                    else
                    {
                        params_[i] = value;
                    }
                    adm.SetParameterValue(addonInfo.paramInfos[i].name, value);
                }
                object instance = null;
                try
                {
                    instance = ctor.Invoke(params_);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                if (instance == null)
                {
                    _addonInstances.Add(new AddonInstance(null, adm, types));
                    continue;
                }
                //Debug.Log($"{adm.Name}: {instance}");
                var addonInstance = new AddonInstance(instance, adm, types);
                _addonInstances.Add(addonInstance);

                foreach (var commandInfo in addonInfo.commandInfos)
                {
                    types = GetTypes(commandInfo.args);
                    var methodInfo = type.GetMethod(commandInfo.name, types);
                    //Debug.Log($"methodInfo: {methodInfo}, {commandInfo.name}");
                    if (methodInfo != null)
                    {
                        addonInstance.AddMethodInfo(commandInfo.name, methodInfo, types);
                    }
                    else
                    {
                        var list = new List<string>();
                        foreach (var t in types)
                        {
                            list.Add(t.Name);
                        }
                        Debug.LogError($"Addon command {addonInfo.name}.{commandInfo.name}({String.Join(", ", list)}) not found");
                    }
                }
            }
        }

#if UNITY_EDITOR
        /**
         * Unityエディターの設定言語を取得。
         * 
         * 非公開クラス UnityEditor.LocalizationDatabase のプロパティ currentEditorLanguage の値を取得する。
         */
        public static SystemLanguage GetCurrentEditorLanguage() {
            var assembly = typeof(UnityEditor.EditorWindow).Assembly;
            var localizationDatabaseType = assembly.GetType("UnityEditor.LocalizationDatabase");
            var currentEditorLanguageProperty = localizationDatabaseType.GetProperty("currentEditorLanguage");
            return (SystemLanguage) currentEditorLanguageProperty.GetValue(null);
        }
#endif

#if UNITY_EDITOR
        public class AddonReader {
            Dictionary<string, string> _langComments = null;
            Dictionary<string, Dictionary<string, string>> _structLangComments = null;
            AddonInfo _addonInfo = null;
            string _currentFilename = null;
            private AddonInfoContainer _addonInfos;
            private List<AddonDataModel> _addonDataModels;
            private AddonManagementService _addonRepositoryService;

            public AddonReader(List<AddonDataModel> addonDataModels) {
                _addonDataModels = addonDataModels;
                _addonRepositoryService = new AddonManagementService();

            }

            private bool ReadAddonCommentsRecur(string text) {
                //MyDebug.Log($"ReadAddonCommentsRecur: {text}");
                var rePat = @"\/\*(~struct~([a-zA-Z_][a-zA-Z0-9_]*))?\:([a-zA-Z_]*)([\s\S ]*)\*\/";
                var rePat2 = @"\*\/";

                var options = RegexOptions.Multiline;
                var match = Regex.Match(text, rePat, options);
                if (match.Success)
                {
                    var structName = match.Groups[2].Value;
                    var lang = match.Groups[3].Value;
                    var comments = match.Groups[4].Value;
                    var restComments = "";
                    if (lang.Length == 0)
                    {
                        lang = "en";
                    }
                    var match2 = Regex.Match(comments, rePat2, options);
                    if (match2.Success)
                    {
                        //MyDebug.Log($"match2.Index: {match2.Index}");
                        restComments = comments.Substring(match2.Index + 2) + "*/";
                        comments = comments.Substring(0, match2.Index);
                    }
                    {
                        if (structName.Length == 0)
                        {
                            if (!_langComments.ContainsKey(lang))
                            {
                                _langComments.Add(lang, comments);
                            }
                            //MyDebug.Log($"_langComments.Add: {lang}, {comments}");
                        }
                        else
                        {
                            if (!_structLangComments.ContainsKey(structName))
                            {
                                _structLangComments.Add(structName, new Dictionary<string, string>());
                            }
                            if (!_structLangComments[structName].ContainsKey(lang))
                            {
                                _structLangComments[structName].Add(lang, comments);
                            }
                            //MyDebug.Log($"_structLangComments.Add: {structName}, {lang}, {comments}");
                        }
                    }
                    if (restComments.Length > 0)
                    {
                        ReadAddonCommentsRecur(restComments);
                    }
                }
                return true;
            }

            private string GetCommentsForLang(Dictionary<string, string> langComments, string lang) {
                string comments = null;
                if (langComments.ContainsKey(lang))
                {
                    comments = langComments[lang];
                }
                else
                {
                    foreach (var key in langComments.Keys)
                    {
                        if (key.Substring(0, 2) == lang.Substring(0, 2))
                        {
                            comments = langComments[key];
                            break;
                        }
                    }
                    if (comments == null)
                    {
                        comments = langComments.FirstOrDefault().Value;
                    }
                }
                return comments;
            }

            private void ProcessLangComment(string comments) {
                ProcessComment(comments, null);
            }
            private void ProcessStructComment(string comments, AddonStructInfo structInfo) {
                ProcessComment(comments, structInfo);
            }

            private void ProcessComment(string comments, AddonStructInfo structInfo) {
                var currentInfos = new AddonParameterContainer();  //読み捨て用ダミーを設定。
                var currentArgs = new AddonParamInfoContainer();  //読み捨て用ダミーを設定。
                var currentOptions = new AddonParameterContainer();  //読み捨て用ダミーを設定。
                var currentOption = new AddonParameter(null, null);   //読み捨て用ダミーを設定。

                if (structInfo != null)
                {
                    currentInfos = structInfo.infos;
                }
                var rePat = @"@([a-zA-Z]+)([^@]*)";
                var options = RegexOptions.Multiline;
                var matches = Regex.Matches(comments, rePat, options);
                int index = 0;
                foreach (Match match in matches)
                {
                    var name = match.Groups[1].Value;
                    var text = match.Groups[2].Value;
                    //MyDebug.Log($"{index}: {name}, {text}");
                    text = Regex.Replace(text, @"[  ]*\n[  ]*\*?[  ]?", "\n");
                    if (string.Compare(name, "help") == 0)
                    {
                        text = Regex.Replace(text, @"\n[  ]*", "\n");
                    }
                    text = text.Trim();
                    //MyDebug.Log($"text: {text}");
                    AddonParamInfo pi = null;
                    switch (name)
                    {
                        case "addondesc":
                            _addonInfo.addondesc = text;
                            break;
                        case "author":
                            _addonInfo.author = text;
                            break;
                        case "help":
                            _addonInfo.help = text;
                            break;
                        case "url":
                            _addonInfo.url = text;
                            break;
                        case "base":
                            _addonInfo.base_.Add(text);
                            break;
                        case "orderAfter":
                            _addonInfo.orderAfter.Add(text);
                            break;
                        case "orderBefore":
                            _addonInfo.orderBefore.Add(text);
                            break;
                        //case "requiredAssets":
                        //_addonInfo.requiredAssets.Add(text);
                        //break;

                        case "param":
                            var paramInfos = (structInfo != null) ? structInfo.params_ : _addonInfo.paramInfos;
                            pi = paramInfos.GetParamInfo(text);
                            if (pi == null)
                            {
                                pi = new AddonParamInfo(text);
                                paramInfos.Add(pi);
                            }
                            currentInfos = pi.infos;
                            currentOptions = pi.options;
                            break;
                        case "text":
                        case "desc":
                        case "default":
                        case "type":
                        case "parent":
                        case "max":
                        case "min":
                        case "decimals":
                        case "dir":
                        case "on":
                        case "off":
                            currentInfos.Add(new AddonParameter(name, text));
                            break;

                        case "option":
                            currentOptions.Add(new AddonParameter(text, null));
                            currentOption = currentOptions.FirstOrDefault(x => x.key == text);
                            break;
                        case "value":
                            currentOption.value = text;
                            break;

                        case "command":
                            var ci = _addonInfo.commandInfos.GetCommandInfo(text);
                            if (ci == null)
                            {
                                ci = new AddonCommandInfo(text);
                                _addonInfo.commandInfos.Add(ci);
                            }
                            currentInfos = ci.infos;
                            currentArgs = ci.args;
                            break;
                        case "arg":
                            pi = currentArgs.GetParamInfo(text);
                            if (pi == null)
                            {
                                pi = new AddonParamInfo(text);
                                currentArgs.Add(pi);
                            }

                            currentInfos = pi.infos;
                            currentOptions = pi.options;
                            break;

                        case "noteParam":
                            pi = _addonInfo.noteParamInfos.GetParamInfo(text);
                            if (pi == null)
                            {
                                pi = new AddonParamInfo(text);
                                _addonInfo.noteParamInfos.Add(pi);
                            }
                            currentInfos = pi.infos;
                            currentOptions = pi.options;
                            break;
                        case "noteType":
                        case "noteDir":
                        case "noteData":
                            currentInfos.Add(new AddonParameter(name, text));
                            break;

                        default:
                            Debug.LogWarning($"Unknown annotation: @{name} {text} in {_currentFilename}");
                            break;
                    }
                    index++;
                }

            }
            private AddonInfo ReadAddon(string addonName) {
                _addonInfo = new AddonInfo();
                _langComments = new Dictionary<string, string>();
                _structLangComments = new Dictionary<string, Dictionary<string, string>>();
                var addonFile = $"{_addonFolderPath}/{addonName}/{addonName}.cs";
                _currentFilename = addonFile;
                try
                {
                    var text = File.ReadAllText(addonFile);
                    //MyDebug.Log(text);
                    ReadAddonCommentsRecur(text);
                    var lang = "en";
                    //var language = Application.systemLanguage;
                    var language = GetCurrentEditorLanguage();
                    //var language = SystemLanguage.English;
                    switch (language)
                    {
                        case SystemLanguage.Japanese:
                            lang = "ja";
                            break;
                        case SystemLanguage.ChineseSimplified:
                            lang = "zh_CN";
                            break;
                    }
                    string comments = GetCommentsForLang(_langComments, lang);
                    _addonInfo.name = addonName;
                    if (comments != null)
                    {
                        ProcessLangComment(comments);
                    }

                    foreach (var structName in _structLangComments.Keys)
                    {
                        comments = GetCommentsForLang(_structLangComments[structName], lang);
                        if (_addonInfo.structInfos.GetStructInfo(structName) != null)
                        {
                            continue;
                        }
                        var si = new AddonStructInfo(structName);
                        _addonInfo.structInfos.Add(si);
                        if (comments != null)
                        {
                            ProcessStructComment(comments, si);
                        }
                    }

                    //MyDebug.DumpAddonInfo(_addonInfo);

                    return _addonInfo;
                }
                catch (IOException)
                {
                    return null;
                }
                finally
                {
                    _currentFilename = null;
                }
            }

            public AddonInfoContainer ScanAndMergeAddons() {
                var modified = false;
                _addonInfos = new AddonInfoContainer();
                try
                {
                    // ディレクトリ内のファイル全取得
                    var dataPath = Directory.GetDirectories(_addonFolderPath, "*", SearchOption.TopDirectoryOnly);
                    for (int i2 = 0; i2 < dataPath.Length; i2++)
                    {
                        var addonName = dataPath[i2].Substring(_addonFolderPath.Length + 1);
                        //MyDebug.Log($"{i2}: {dataPath[i2]}, {addonName}");
                        var addonInfo = ReadAddon(addonName);
                        if (addonInfo != null)
                        {
                            AdjustInvalidParameter(addonInfo);
                            _addonInfos.Add(addonInfo);
                        }
                    }
                }
                catch (IOException)
                {
                }

                int i = 0;
                while (i < _addonDataModels.Count)
                {
                    var addonName = _addonDataModels[i].Name;
                    var addonInfo = _addonInfos.GetAddonInfo(addonName);
                    if (addonInfo != null)
                    {
                        if (AddonManager.ApplyAddonInfoToModel(_addonDataModels[i], addonInfo))
                        {
                            modified = true;
                        }
                        i++;
                        continue;
                    }
                    RemoveAt(i);
                    modified = true;
                }
                foreach (var addonInfo in _addonInfos)
                {
                    var addonName = addonInfo.name;
                    var addonDataModel = _addonDataModels.FirstOrDefault(x => x.Name == addonName);
                    if (addonDataModel != null) {
                        continue;
                    }
                    Append(addonInfo);
                }
                SortAddonsSimple();

                if (modified)
                {
                    _addonRepositoryService.SaveAddons(_addonDataModels);
                }
                return _addonInfos;
            }

            private void SortAddonsSimple() {
                if (_addonDataModels.Count == 0) return;
                //_addonInfos
                //_addonDataModels
                var list = new List<AddonDataModel>();
                foreach (var addon in _addonDataModels)
                {
                    list.Add(addon);
                }
                int countdown = list.Count();
                bool changed = true;
                while (changed && countdown-- > 0)
                {
                    changed = false;
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (SortAddonSimple(list, i))
                        {
                            changed = true;
                            break;
                        }
                    }
                }
                if (changed)
                {
                    //安定しなかった。
                    Debug.LogWarning("Add-on order is not stable.");
                }
                _addonDataModels.Clear();
                _addonDataModels.AddRange(list);
            }

            private bool MoveBefore(List<AddonDataModel> list, int target, int anchor) {
                if (target <= anchor) return false;
                while (true)
                {
                    var addon = list[target];
                    list.RemoveAt(target);
                    list.Insert(target - 1, addon);
                    target--;
                    if (anchor == target)
                    {
                        break;
                    }
                }
                return true;
            }

            private bool MoveAfter(List<AddonDataModel> list, int target, int anchor) {
                if (target >= anchor) return false;
                while (true)
                {
                    var addon = list[target];
                    list.RemoveAt(target);
                    list.Insert(target + 1, addon);
                    target++;
                    if (anchor == target)
                    {
                        break;
                    }
                }
                return true;
            }

            private bool SortAddonSimple(List<AddonDataModel> list, int index) {
                var addon = _addonInfos.GetAddonInfo(list[index].Name);
                bool changed = false;
                int bottomAnchor = -1;
                foreach (var name in addon.orderAfter)
                {
                    var anchor = list.FindIndex(x => x.Name == name);
                    if (anchor < 0) continue;
                    changed |= MoveAfter(list, index, anchor);
                    index = list.FindIndex(x => x.Name == addon.name);
                    if (index > bottomAnchor)
                    {
                        bottomAnchor = index;
                    }
                }
                foreach (var name in addon.orderBefore)
                {
                    var anchor = list.FindIndex(x => x.Name == name);
                    if (anchor < 0) continue;
                    if (anchor < bottomAnchor)
                    {
                        changed |= MoveAfter(list, anchor, index);
                        index = list.FindIndex(x => x.Name == addon.name);
                        continue;
                    }
                    changed |= MoveBefore(list, index, anchor);
                    index = list.FindIndex(x => x.Name == addon.name);
                }
                return changed;
            }


            private void Append(AddonInfo addonInfo) {
                //MyDebug.Log($"Append({addonInfo.name})");
                var addonDataModel = AddonDataModel.Create();
                addonDataModel.Name = addonInfo.name;
                addonDataModel.Description = addonInfo.addondesc;
                _addonDataModels.Add(addonDataModel);
            }

            private void RemoveAt(int index) {
                //MyDebug.Log($"RemoveAt({index})");
                _addonDataModels.RemoveAt(index);
            }

            private void AdjustInvalidParameter(AddonInfo addonInfo) {
                _checkCircularReference = true;
                foreach (var paramInfo in addonInfo.paramInfos)
                {
                    AdjustInvalidParameter(addonInfo, paramInfo);
                }
                foreach (var commandInfo in addonInfo.commandInfos)
                {
                    foreach (var arg in commandInfo.args)
                    {
                        AdjustInvalidParameter(addonInfo, arg);
                    }
                }
                foreach (var structInfo in addonInfo.structInfos)
                {
                    foreach (var param in structInfo.params_)
                    {
                        AdjustInvalidParameter(addonInfo, param);
                    }
                }
                _checkCircularReference = false;
            }


            private void AdjustInvalidParameter(AddonInfo addonInfo, AddonParamInfo paramInfo, AddonParameter typeInfo = null, AddonParameter defaultInfo = null) {
                if (defaultInfo == null)
                {
                    defaultInfo = paramInfo.GetInfo("default");
                }
                if (typeInfo == null)
                {
                    typeInfo = paramInfo.GetInfo("type");
                    if (typeInfo == null) return;
                    if (IsArray2Postfix(typeInfo.value))
                    {
                        var newTypeInfo = new AddonParameter("type", typeInfo.value.Substring(0, typeInfo.value.Length - 4));
                        if (defaultInfo == null)
                        {
                            defaultInfo = new AddonParameter("default", "[]");
                            paramInfo.infos.Add(defaultInfo);
                        }
                        var newDefaultInfo = new AddonParameter("default", null);
                        var jsonNode = JSON.Parse(defaultInfo.value);
                        if (!jsonNode.IsArray)
                        {
                            defaultInfo.value = "[]";
                        } else {
                            var jsonArray = jsonNode.AsArray;
                            for (int i = 0; i < jsonArray.Count; i++)
                            {
                                if (!jsonArray[i].IsArray)
                                {
                                    jsonArray[i] = new JSONArray();
                                    continue;
                                }
                                var jsonArray2 = jsonArray[i].AsArray;
                                for (int j = 0; j < jsonArray2.Count; j++)
                                {
                                    newDefaultInfo.value = jsonArray2[j].ToString();
                                    AdjustInvalidParameter(addonInfo, paramInfo, newTypeInfo, newDefaultInfo);
                                    jsonArray2[j] = JSON.Parse(newDefaultInfo.value);
                                }
                                jsonArray[i] = jsonArray2;
                            }
                            defaultInfo.value = jsonArray.ToString();
                        }
                        return;
                    }
                    else if (IsArrayPostfix(typeInfo.value))
                    {
                        var newTypeInfo = new AddonParameter("type", typeInfo.value.Substring(0, typeInfo.value.Length - 2));
                        if (defaultInfo == null)
                        {
                            defaultInfo = new AddonParameter("default", "[]");
                            paramInfo.infos.Add(defaultInfo);
                        }
                        var newDefaultInfo = new AddonParameter("default", null);
                        var jsonNode = JSON.Parse(defaultInfo.value);
                        if (!jsonNode.IsArray)
                        {
                            defaultInfo.value = "[]";
                            jsonNode = JSON.Parse(defaultInfo.value);
                        }
                        {
                            var jsonArray = jsonNode.AsArray;
                            for (int i = 0; i < jsonArray.Count; i++)
                            {
                                newDefaultInfo.value = jsonArray[i].ToString();
                                AdjustInvalidParameter(addonInfo, paramInfo, newTypeInfo, newDefaultInfo);
                                jsonArray[i] = JSON.Parse(newDefaultInfo.value);
                            }
                            defaultInfo.value = jsonArray.ToString();
                            if (jsonArray.Count == 0)
                            {
                                // force call
                                newDefaultInfo.value = "";
                                int arrayDimension2 = 0;
                                string structName2 = null;
                                var paramType2 = GetParamType(typeInfo.value, out arrayDimension2, out structName2);
                                if (paramType2 != ParamType.Struct)
                                {
                                    newDefaultInfo.value = ValidateValue(paramInfo, paramType2, "");
                                }
                                AdjustInvalidParameter(addonInfo, paramInfo, newTypeInfo, newDefaultInfo);
                            }
                        }
                        return;
                    }
                }
                var minInfo = paramInfo.GetInfo("min");
                var maxInfo = paramInfo.GetInfo("max");
                int intValue = 0;

                int arrayDimension = 0;
                string structName = null;
                var paramType = GetParamType(typeInfo.value, out arrayDimension, out structName);
                AddonStructInfo structInfo = null;
                if (paramType == ParamType.Struct)
                {
                    structInfo = addonInfo.structInfos.GetStructInfo(structName);
                    if (structInfo == null)
                    {
                        Debug.LogWarning($"Struct name not found: {structName}");
                        typeInfo.value = "string";
                        paramType = ParamType.String;
                    }
                }
                if (typeInfo.value == "text")
                {
                    typeInfo.value = "string";
                    paramType = ParamType.String;
                }
                switch (paramType)
                {
                    case ParamType.Number:
                        {
                            if (defaultInfo == null)
                            {
                                paramInfo.AddInfo("default", "0");
                            }
                            AdjustInvalidParameter<double>(paramInfo, minInfo, maxInfo, defaultInfo, double.TryParse, double.Parse);
                            var decimalsParam = paramInfo.GetInfo("decimals");
                            if (decimalsParam != null)
                            {
                                if (!int.TryParse(decimalsParam.value, out intValue))
                                {
                                    paramInfo.RemoveInfo(minInfo.key);
                                }
                                else if (intValue < 0)
                                {
                                    decimalsParam.value = "0";
                                }
                            }
                            break;
                        }
                    case ParamType.Integer:
                        {
                            if (defaultInfo == null)
                            {
                                paramInfo.AddInfo("default", "0");
                            }
                            AdjustInvalidParameter<int>(paramInfo, minInfo, maxInfo, defaultInfo, int.TryParse, int.Parse);
                            break;
                        }
                    /*case ParamType.String:
                    case ParamType.MultilineString:
                        {
                            if (defaultInfo == null)
                            {
                                paramInfo.AddInfo("default", "\"\"");
                            }
                            else
                            {
                                if (defaultInfo.value.Length < 2 || defaultInfo.value[0] != '"' || defaultInfo.value[defaultInfo.value.Length - 1] != '"')
                                {
                                    defaultInfo.value = DataConverter.GetJsonString(defaultInfo.value);
                                }
                            }
                            break;
                        }*/
                    case ParamType.Combo:
                    case ParamType.CommonEvent:
                        {
                            if (defaultInfo == null)
                            {
                                paramInfo.AddInfo("default", "");
                            }
                            break;
                        }
                    case ParamType.Select:
                        {
                            bool boolValue = false;
                            if (defaultInfo != null)
                            {
                                if (bool.TryParse(defaultInfo.value, out boolValue))
                                {
                                    defaultInfo.value = boolValue ? "1" : "0";
                                }
                            }
                            else
                            {
                                defaultInfo = new AddonParameter("default", "0");
                                paramInfo.infos.Add(defaultInfo);
                            }
                            var dic = new Dictionary<int, AddonParameter>();
                            var valueSpecifiedKeyList = new List<string>();
                            foreach (var option in paramInfo.options)
                            {
                                //Debug.Log($"option: {option.key}, {option.value}");
                                if (bool.TryParse(option.value, out boolValue))
                                {
                                    option.value = boolValue ? "1" : "0";
                                }
                                if (option.value != null)
                                {
                                    valueSpecifiedKeyList.Add(option.key);
                                    if (!int.TryParse(option.value, out intValue))
                                    {
                                        Debug.LogWarning($"option value must be a integer: {option.value}");
                                        intValue = 100000 + dic.Count; // dummy
                                        option.value = intValue.ToString();
                                    }
                                    if (dic.ContainsKey(intValue))
                                    {
                                        Debug.LogWarning($"Duplicate option values: {option.value}");
                                        continue;
                                    }
                                    dic.Add(int.Parse(option.value), option);
                                }
                            }
                            paramInfo.AddInfo("valueSpecifiedKeys", string.Join(',', valueSpecifiedKeyList));
                            //Debug.Log($"valueSpecifiedKeys: {string.Join(',', valueSpecifiedKeyList)}, name: {paramInfo.name}, text: {paramInfo.GetInfo("text")?.value}, desc: {paramInfo.GetInfo("desc")?.value}");
                            int index = 0;
                            foreach (var option in paramInfo.options)
                            {
                                while (dic.ContainsKey(index))
                                {
                                    index++;
                                }
                                if (option.value == null)
                                {
                                    option.value = index.ToString();
                                    index++;
                                }
                                else
                                {
                                    index = int.Parse(option.value) + 1;
                                }
                            }
                            var found = false;
                            AddonParameter first = null;
                            foreach (var option in paramInfo.options)
                            {
                                if (first == null)
                                {
                                    first = option;
                                }
                                if (option.key == defaultInfo.value)
                                {
                                    defaultInfo.value = option.value;
                                    found = true;
                                    break;
                                }
                                else if (option.value == defaultInfo.value)
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (!found && first != null)
                            {
                                defaultInfo.value = first.value;
                            }
                            break;
                        }
                    case ParamType.Boolean:
                        {
                            if (defaultInfo == null)
                            {
                                defaultInfo = new AddonParameter("default", "false");
                                paramInfo.infos.Add(defaultInfo);
                            }
                            var boolValue = false;
                            if (defaultInfo.value == "ON")
                            {
                                defaultInfo.value = "true";

                            }
                            else
                            if (defaultInfo.value == "OFF")
                            {
                                defaultInfo.value = "false";

                            }
                            else if (int.TryParse(defaultInfo.value, out intValue))
                            {
                                defaultInfo.value = ((intValue == 0) ? "false" : "true");
                            }
                            else
                            if (!bool.TryParse(defaultInfo.value, out boolValue))
                            {
                                defaultInfo.value = "false";
                            }
                            break;
                        }
                    case ParamType.String:
                    case ParamType.MultilineString:
                    case ParamType.Note:
                        {
                            if (defaultInfo == null)
                            {
                                defaultInfo = new AddonParameter("default", "");
                                paramInfo.infos.Add(defaultInfo);
                            }
                            var jsonNode = JSON.Parse(defaultInfo.value);
                            defaultInfo.value = jsonNode.ToString();
                            break;
                        }
                    case ParamType.Struct:
                        {
                            if (defaultInfo == null)
                            {
                                defaultInfo = new AddonParameter("default", "{}");
                                paramInfo.infos.Add(defaultInfo);
                            }
                            defaultInfo.value = ValidateValue(addonInfo, paramInfo, $"struct<{structName}>", defaultInfo.value);
                            break;
                        }

                    case ParamType.MapEvent:
                        {
                            if (defaultInfo == null)
                            {
                                defaultInfo = new AddonParameter("default", "[\"\",\"\"]");
                                paramInfo.infos.Add(defaultInfo);
                            }
                            var list = DataConverter.GetStringArrayFromJson(defaultInfo.value)?.ToList();
                            if (list == null)
                            {
                                list = new List<string>() { "", "" };
                            }
                            while (list.Count < 2)
                            {
                                list.Add("");
                            }
                            if (list.Count > 2)
                            {
                                list.RemoveRange(2, list.Count - 2);
                            }
                            defaultInfo.value = DataConverter.GetJsonStringArray(list.ToArray());
                            break;
                        }

                    case ParamType.Switch:
                    case ParamType.Variable:
                    case ParamType.Animation:
                    case ParamType.Actor:
                    case ParamType.Class:
                    case ParamType.Skill:
                    case ParamType.Item:
                    case ParamType.Weapon:
                    case ParamType.Armor:
                    case ParamType.Enemy:
                    case ParamType.Troop:
                    case ParamType.State:
                    case ParamType.Tileset:
                    case ParamType.File:
                        {
                            if (defaultInfo == null)
                            {
                                paramInfo.AddInfo("default", "");
                            }
                            break;
                        }

                    default:
                        Debug.LogWarning($"Unknown type {typeInfo.value}, treated as string");
                        typeInfo.value = "string";
                        if (defaultInfo == null)
                        {
                            paramInfo.AddInfo("default", "");
                        }
                        break;
                }
            }

            delegate bool TryParseDelegate<T>(string s, out T result);
            delegate T ParseDelegate<T>(string s);
            private void AdjustInvalidParameter<T>(AddonParamInfo paramInfo, AddonParameter minInfo, AddonParameter maxInfo, AddonParameter defaultInfo, TryParseDelegate<T> TryParse, ParseDelegate<T> Parse) where T: IComparable {
                T value = default;
                if (minInfo != null)
                {
                    if (!TryParse(minInfo.value, out value))
                    {
                        Debug.LogWarning($"@min value is not valid as {typeof(T).Name}: {minInfo.value}");
                        paramInfo.RemoveInfo(minInfo.key);
                        minInfo = null;
                    }
                }
                if (maxInfo != null)
                {
                    if (!TryParse(maxInfo.value, out value))
                    {
                        Debug.LogWarning($"@max value is not valid as {typeof(T).Name}: {maxInfo.value}");
                        paramInfo.RemoveInfo(maxInfo.key);
                        maxInfo = null;
                    }
                }
                if (defaultInfo != null)
                {
                    if (!TryParse(defaultInfo.value, out value))
                    {
                        Debug.LogWarning($"@default value is not valid as {typeof(T).Name}: {defaultInfo.value}");
                        paramInfo.RemoveInfo(defaultInfo.key);
                    }
                    else
                    {
                        if (minInfo != null)
                        {
                            var minValue = Parse(minInfo.value);
                            value = (value.CompareTo(minValue) >= 0 ? value : minValue);
                        }
                        if (maxInfo != null)
                        {
                            var maxValue = Parse(maxInfo.value);
                            value = (value.CompareTo(maxValue) <= 0 ? value : maxValue);
                        }
                        defaultInfo.value = value.ToString();
                    }
                }
            }
        }
#endif
    }

#if UNITY_EDITOR
    public class MyDebug
    {
        static public void Log(string line) {
            var logFilename = "E:/_git/AddonManager.log";
            var streamWriter = File.AppendText(logFilename);
            streamWriter.WriteLine(line);
            streamWriter.Close();
        }

        static public void DumpAddonParameters(AddonParameterContainer addonParameters, string prefix) {
            int num = 1;
            foreach (var param in addonParameters)
            {
                Log($"{prefix}{num}: {param.key}: {param.value}");
                num++;
            }
        }

        static public void DumpAddonParamInfo(AddonParamInfo addonParamInfo, string prefix) {
            DumpAddonParameters(addonParamInfo.infos, prefix);
            DumpAddonParameters(addonParamInfo.options, prefix);
        }

        static public void DumpAddonInfo(AddonInfo addonInfo) {
            Log("---------------------------------------------");
            Log($"name: {addonInfo.name}");
            Log($"addondesc: {((addonInfo.addondesc.Length > 0) ? addonInfo.addondesc : "-")}");
            Log($"author: {((addonInfo.author.Length > 0) ? addonInfo.author : "-")}");
            Log($"help: {((addonInfo.help.Length > 0) ? addonInfo.help : "-")}");
            Log($"url: {((addonInfo.url.Length > 0) ? addonInfo.url : "-")}");
            Log($"base: {((addonInfo.base_.Count > 0) ? string.Join(",", addonInfo.base_.ToArray()) : "-")}");
            Log($"orderAfter: {((addonInfo.orderAfter.Count > 0) ? string.Join(",", addonInfo.orderAfter.ToArray()) : "-")}");
            Log($"orderBefore: {((addonInfo.orderBefore.Count > 0) ? string.Join(",", addonInfo.orderBefore.ToArray()) : "-")}");
            //Log($"requiredAssets: {((addonInfo.requiredAssets.Count > 0) ? string.Join(",", addonInfo.requiredAssets.ToArray()) : "-")}");
            Log("");
            foreach (var paramInfo in addonInfo.paramInfos)
            {
                Log($"param: {paramInfo.name}");
                DumpAddonParamInfo(paramInfo, "  ");
            }
            if (addonInfo.paramInfos.Count > 0)
            {
                Log("");
            }

            foreach (var ci in addonInfo.commandInfos)
            {
                Log($"command: {ci.name}");
                DumpAddonParameters(ci.infos, "  ");
                var args = ci.args;
                foreach (var pi in args)
                {
                    Log($"  arg: {pi.name}");
                    DumpAddonParamInfo(pi, "    ");
                }
            }
            if (addonInfo.commandInfos.Count > 0)
            {
                Log("");
            }

            foreach (var si in addonInfo.structInfos)
            {
                Log($"struct: {si.name}");
                DumpAddonParameters(si.infos, "  ");
                var params_ = si.params_;
                foreach (var pi in params_)
                {
                    Log($"  param: {pi.name}");
                    DumpAddonParamInfo(pi, "    ");
                }
            }
            if (addonInfo.structInfos.Count > 0)
            {
                Log("");
            }

            foreach (var paramInfo in addonInfo.noteParamInfos)
            {
                Log($"noteParam: {paramInfo.name}");
                DumpAddonParamInfo(paramInfo, "  ");
            }
            if (addonInfo.noteParamInfos.Count > 0)
            {
                Log("");
            }
        }
    }
#endif
}

//ダミー
namespace RPGMaker.Codebase.Addon {
}
