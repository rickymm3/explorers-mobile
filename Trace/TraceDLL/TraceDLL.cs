using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tracer : MonoBehaviour {
    static bool _IS_ERROR = false;
    static List<string> _TAG_FILTERS = new List<string>();
    static Dictionary<string, int> _traceCounters = new Dictionary<string, int>();

    public static bool IS_TRACE_ENABLED = true;
    public static Action<object> OnTrace;
    public static Action<object> OnTraceWarn;
    public static Action<object> OnTraceError;
    
    public static void traceSetTagFilters(params string[] tags) {
        //Convert to UPPER case first:
        for(int t=tags.Length; --t>=0;) {
            tags[t] = tags[t].ToUpper();
        }

        _TAG_FILTERS.Clear();
        _TAG_FILTERS.AddRange(tags);
    }

    public static void CheckCounters() {
        foreach(var kv in _traceCounters) {
            trace(string.Format("({0}) {1}", kv.Value, kv.Key));
        }

        _traceCounters.Clear();
    }

    public static void releaseErrorLock() {
        _IS_ERROR = false;
    }

    public static void trace(string tag, object msg) {
        tag = tag.ToUpper();
        if (!_TAG_FILTERS.Contains(tag)) return;

        trace("[" + tag + "] " + msg);
    }

    public static void trace(object msg) {
        if (!IS_TRACE_ENABLED) return;
        Debug.Log(msg);
        if (OnTrace != null) OnTrace(msg);
    }

    public static void traceError(string tag, object msg) {
        tag = tag.ToUpper();
        if (!_TAG_FILTERS.Contains(tag)) return;

        traceError("[" + tag + "] " + msg);
    }

    public static void traceError(object msg) {
        Debug.LogError(msg);

        //Prevent infinite recursion in case the callbacks triggers more traceError()...
        if (_IS_ERROR) return;
        _IS_ERROR = true;
        if (OnTraceError != null) OnTraceError(msg);
        _IS_ERROR = false;
    }

    public static void traceWarn(string tag, object msg) {
        tag = tag.ToUpper();
        if (!_TAG_FILTERS.Contains(tag)) return;

        traceWarn("[" + tag + "] " + msg);
    }

    public static void traceWarn(object msg) {
        Debug.LogWarning(msg);
        if (OnTraceWarn != null) OnTraceWarn(msg);
    }

    public static void traceCounter(object msg) {
        string msgStr = msg.ToString();
        if (_traceCounters.ContainsKey(msgStr)) {
            _traceCounters[msgStr] += 1;
        } else {
            _traceCounters.Add(msgStr, 1);
        }
    }
}
