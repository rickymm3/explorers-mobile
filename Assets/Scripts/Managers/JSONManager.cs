using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using ExtensionMethods;
using System.Text.RegularExpressions;
using System.IO;
using RSG;

public class JSONManager : Singleton<JSONManager> {
    static char[] _BAD_CHARS = new char[] { '[', ']', '{', '}', ',', '\n', '\r' };

    Promise<JSONNode> _promise;

    string _lastJSONContent = "";


    public IPromise<JSONNode> Load(string url) {
        this._promise = new Promise<JSONNode>();
        StartCoroutine(__Load(url));
        return this._promise;
    }

    private void NotifyError(string err) {
        traceError(err);

        _promise.Reject(new Exception("JSONManage Error: " + err));
    }

    private IEnumerator __Load(string url) {
        yield return new WaitForEndOfFrame();

        WWW www = new WWW(url);
        yield return www;
        
        if(www.error.Exists()) {
            NotifyError("Could not load JSON: " + url);
            yield break;
        }

        if(!www.text.Exists() || !www.text.StartsWith("{")) {
            NotifyError("Data doesn't appear to be JSON formatted: " + www.text);
            yield break;
        }

        _lastJSONContent = www.text;
        JSONNode json = JSON.Parse(_lastJSONContent);
        _promise.Resolve(json);
    }
    
    internal void SaveLastFileTo(string localJsonURL) {
        if(!_lastJSONContent.Exists()) {
            traceError("No previous JSON data loaded, so no content to write locally!");
            return;
        }

        File.WriteAllText(localJsonURL, _lastJSONContent);

        trace("LOADING", "Local JSON File Successfully written to...\n"+localJsonURL);
    }

    //////////////////////////////////////// Utility Functions to process JSONNodes:

    public static List<T> ForEach<T>(JSONNode jsonData, Func<JSONNode, int, T> cbForEach, string fieldName = "data") {
        return ForEach(jsonData, new List<T>(), cbForEach, fieldName);
    }

    public static List<T> ForEach<T>(JSONNode jsonData, List<T> list, Func<JSONNode, int, T> cbForEach, string fieldName="data") {
        if(jsonData==null) return list;

        JSONArray jsonArray = jsonData[fieldName].AsArray;
        
        int id = 0;
        foreach(JSONNode jsonItem in jsonArray) {
            T item = cbForEach(jsonItem, id++);

            if(item==null) continue;

            list.Add(item);
        }

        return list;
    }

    public static string[] SplitArray(JSONNode jsonData) {
        string str = jsonData == null || !jsonData.Exists() ? null : jsonData.AsDecodedURL();

        if (string.IsNullOrEmpty(str)) return null;

        Regex badChars = new Regex(@"[^\w_\-,]*", RegexOptions.IgnoreCase);
        str = badChars.Replace(str, "");

        string[] strSplit = str.Split(",");

        return strSplit;
    }

    public static string[] SplitArray(JSONNode jsonData, char character, bool clearChars = true) {
        string str = jsonData == null || !jsonData.Exists() ? null : jsonData.AsDecodedURL();

        if (string.IsNullOrEmpty(str)) return null;

        if (clearChars) {
            Regex badChars = new Regex(@"[^\w_\-,]*", RegexOptions.IgnoreCase);
            str = badChars.Replace(str, "");
        }
        string[] strSplit = str.Split(character);

        return strSplit;
    }

    public static Dictionary<string, string> SplitKVStrings(JSONNode jsonData, string delimLines = "\n", string delimKV = "=", bool trimBadChars=true) {
        string str = jsonData.AsDecodedURL();

        return SplitKVStrings(str, delimLines, delimKV, trimBadChars);
    }

    public static Dictionary<string, string> SplitKVStrings(string str, string delimLines = "\n", string delimKV = "=", bool trimBadChars = true) {
        if (trimBadChars) {
            str = str.Trim(_BAD_CHARS);
        }

        string[] lines = str.Split(delimLines);

        var result = new Dictionary<string, string>();

        foreach (string line in lines) {
            string[] lineSplit = line.Split(delimKV);
            string key = lineSplit[0];
            string value = lineSplit[1];

            if (trimBadChars) {
                result.Add(key.Trim(_BAD_CHARS), value.Trim(_BAD_CHARS));
            } else {
                result.Add(key.Trim(), value.Trim());
            }
        }

        return result;
    }

    public static Dictionary<string, int> SplitKVInts(JSONNode jsonData, string delimLines = "\n", string delimKV = "=", bool trimBadChars = true) {
        var resultStrings = SplitKVStrings(jsonData, delimLines, delimKV, trimBadChars);
        var result = new Dictionary<string, int>();

        foreach (KeyValuePair<string, string> kv in resultStrings) {
            result.Add(kv.Key, int.Parse(kv.Value));
        }

        return result;
    }

    public static Dictionary<string, float> SplitKVFloats(JSONNode jsonData, string delimLines = "\n", string delimKV = "=", bool trimBadChars = true) {
        var resultStrings = SplitKVStrings(jsonData, delimLines, delimKV, trimBadChars);
        var result = new Dictionary<string, float>();

        foreach (KeyValuePair<string, string> kv in resultStrings) {
            result.Add(kv.Key, float.Parse(kv.Value));
        }

        return result;
    }
}
