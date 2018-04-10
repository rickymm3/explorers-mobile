using System;
using System.Collections.Generic;
using UnityEngine;

namespace SPStudios.SerializableDictionary {
    //Predefined SerializedDictionaries for Vector3 keyed dictionaries.
    //Used with predefined property drawer to eliminate the need to use the
    //[Inspectionary] attribute unless you want custom labels [Inspectionary("CustomKey", "CustomValue")]
    [Serializable]
    public class Vector3StringDictionary : SerializableDictionary<Vector3, string> { }
    [Serializable]
    public class Vector3IntDictionary : SerializableDictionary<Vector3, int> { }
    [Serializable]
    public class Vector3FloatDictionary : SerializableDictionary<Vector3, float> { }
    [Serializable]
    public class Vector3BoolDictionary : SerializableDictionary<Vector3, bool> { }
    [Serializable]
    public class Vector3Vector3Dictionary : SerializableDictionary<Vector3, Vector3> { }
    [Serializable]
    public class Vector3GameObjectDictionary : SerializableDictionary<Vector3, GameObject> { }
    [Serializable]
    public class Vector3ComponentDictionary : SerializableDictionary<Vector3, Component> { }
    [Serializable]
    public class Vector3TransformDictionary : SerializableDictionary<Vector3, Transform> { }
    [Serializable]
    public class Vector3Dictionary<T> : SerializableDictionary<Vector3, T> { }
}