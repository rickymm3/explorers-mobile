using System;
using System.Collections.Generic;
using UnityEngine;

namespace SPStudios.SerializableDictionary {
    //Predefined SerializedDictionaries for int keyed dictionaries.
    //Used with predefined property drawer to eliminate the need to use the
    //[Inspectionary] attribute unless you want custom labels [Inspectionary("CustomKey", "CustomValue")]
    [Serializable]
    public class IntStringDictionary : SerializableDictionary<int, string> { }
    [Serializable]
    public class IntIntDictionary : SerializableDictionary<int, int> { }
    [Serializable]
    public class IntFloatDictionary : SerializableDictionary<int, float> { }
    [Serializable]
    public class IntBoolDictionary : SerializableDictionary<int, bool> { }
    [Serializable]
    public class IntVector3Dictionary : SerializableDictionary<int, Vector3> { }
    [Serializable]
    public class IntGameObjectDictionary : SerializableDictionary<int, GameObject> { }
    [Serializable]
    public class IntComponentDictionary : SerializableDictionary<int, Component> { }
    [Serializable]
    public class IntTransformDictionary : SerializableDictionary<int, Transform> { }
    [Serializable]
    public class IntDictionary<T> : SerializableDictionary<int, T> { }
}