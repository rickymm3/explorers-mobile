using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SimpleJSON;
using ExtensionMethods;

public class GlobalsData {
    
    public string currentGlobalPreset = "preset-1";

    GlobalsDictionary _globalData;

    public GlobalsData() {
        _globalData = new GlobalsDictionary();
    }

    public void LoadGlobals(JSONNode sheet) {
        // The globals sheet is a bit more intricate, we CAN have more presets or "pages" if you will of globals / default values.
        // First, lets iterate through the [headers] to find how many "preset-#" we have.
        JSONArray headers = sheet["headers"].AsArray;
        JSONArray datas = sheet["data"].AsArray;

        //Assigns the first (top-left) cell content, basically the property-header loopup to check on each data rows.
        string propertyHeader = headers[0];

        for (int g = 1; g < headers.Count; g++) {
            string header = headers[g].AsDecodedURL();
            GlobalsValuesDictionary globalValues = new GlobalsValuesDictionary();

            foreach (JSONNode globalJSON in datas) {
                string strProp = globalJSON[propertyHeader].AsDecodedURL();
                string strValue = globalJSON[header].AsDecodedURL();
                GlobalProps globProp = strProp.AsEnum<GlobalProps>();
                globalValues.AddOrSet(globProp, strValue);
            }

            _globalData.AddOrSet(header, globalValues);
        }
    }

    public int GetGlobalAsInt(GlobalProps globalProp, int defaultValue = -1, string preset = null) {
        string str = GetGlobalAsString(globalProp, preset);
        if (str == null) return defaultValue;
        return int.Parse(str);
    }

    public float GetGlobalAsFloat(GlobalProps globalProp, float defaultValue = -1f, string preset = null) {
        string str = GetGlobalAsString(globalProp, preset);
        if (str == null) return defaultValue;
        return float.Parse(str);
    }

    public bool GetGlobalAsBool(GlobalProps globalProp, bool defaultValue = false, string preset = null) {
        string str = GetGlobalAsString(globalProp, preset);
        if (str == null) return defaultValue;
        str = str.Trim().ToLower();
        return str == "true" || str == "yes" || str == "y" || int.Parse(str) > 0;
    }

    public string GetGlobalAsString(GlobalProps globalProp, string defaultValue = null, string preset = null) {
        if (preset == null) preset = currentGlobalPreset;

        if (!_globalData.ContainsKey(preset)) {
            Tracer.traceError("Incorrect currentGlobalPreset selected: " + preset + " try one of these: " + _globalData.Keys.ToArray().Join(", "));
            return defaultValue;
        }
        if (!_globalData[preset].ContainsKey(globalProp)) {
            Tracer.traceError("Missing global-property on global preset: " + globalProp + " on " + preset);
            return defaultValue;
        }
        return _globalData[preset][globalProp];
    }
}

public static class GlobalExtensions {
    public static int GetInt(this GlobalProps prop, int defaultValue = -1, string preset = null) {
        return DataManager.globals.GetGlobalAsInt(prop, defaultValue, preset);
    }

    public static float GetFloat(this GlobalProps prop, float defaultValue = -1, string preset = null) {
        return DataManager.globals.GetGlobalAsFloat(prop, defaultValue, preset);
    }

    public static bool GetBool(this GlobalProps prop, bool defaultValue = false, string preset = null) {
        return DataManager.globals.GetGlobalAsBool(prop, defaultValue, preset);
    }

    public static string GetString(this GlobalProps prop, string defaultValue = null, string preset = null) {
        return DataManager.globals.GetGlobalAsString(prop, defaultValue, preset);
    }
}
