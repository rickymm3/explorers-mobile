using System;
using System.Collections.Generic;
using UnityEngine;

namespace SPStudios.SerializableDictionary {
    //Predefined SerializedDictionaries for string keyed dictionaries.
    //Used with predefined property drawer to eliminate the need to use the
    //[Inspectionary] attribute unless you want custom labels [Inspectionary("CustomKey", "CustomValue")]
    [Serializable]
    public class StringStringDictionary : SerializableDictionary<string, string> { }
    [Serializable]
    public class StringIntDictionary : SerializableDictionary<string, int> { }
    [Serializable]
    public class StringFloatDictionary : SerializableDictionary<string, float> { }
    [Serializable]
    public class StringBoolDictionary : SerializableDictionary<string, bool> { }
    [Serializable]
    public class StringVector3Dictionary : SerializableDictionary<string, Vector3> { }
    [Serializable]
    public class StringGameObjectDictionary : SerializableDictionary<string, GameObject> { }
    [Serializable]
    public class StringComponentDictionary : SerializableDictionary<string, Component> { }
    [Serializable]
    public class StringTransformDictionary : SerializableDictionary<string, Transform> { }
    [Serializable]
    public class StringDictionary<T> : SerializableDictionary<string, T> { }
}