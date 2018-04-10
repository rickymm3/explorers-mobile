using System;
using System.Collections.Generic;
using UnityEngine;

namespace SPStudios.SerializableDictionary {
    //Predefined SerializedDictionaries for float keyed dictionaries.
    //Used with predefined property drawer to eliminate the need to use the
    //[Inspectionary] attribute unless you want custom labels [Inspectionary("CustomKey", "CustomValue")]
    [Serializable]
    public class FloatStringDictionary : SerializableDictionary<float, string> { }
    [Serializable]
    public class FloatIntDictionary : SerializableDictionary<float, int> { }
    [Serializable]
    public class FloatFloatDictionary : SerializableDictionary<float, float> { }
    [Serializable]
    public class FloatBoolDictionary : SerializableDictionary<float, bool> { }
    [Serializable]
    public class FloatVector3Dictionary : SerializableDictionary<float, Vector3> { }
    [Serializable]
    public class FloatGameObjectDictionary : SerializableDictionary<float, GameObject> { }
    [Serializable]
    public class FloatComponentDictionary : SerializableDictionary<float, Component> { }
    [Serializable]
    public class FloatTransformDictionary : SerializableDictionary<float, Transform> { }
    [Serializable]
    public class FloatDictionary<T> : SerializableDictionary<float, T> { }
}