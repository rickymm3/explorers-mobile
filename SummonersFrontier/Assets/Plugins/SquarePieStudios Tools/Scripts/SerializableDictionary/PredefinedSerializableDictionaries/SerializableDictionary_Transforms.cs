using System;
using System.Collections.Generic;
using UnityEngine;

namespace SPStudios.SerializableDictionary {
    //Predefined SerializedDictionaries for Transform keyed dictionaries.
    //Used with predefined property drawer to eliminate the need to use the
    //[Inspectionary] attribute unless you want custom labels [Inspectionary("CustomKey", "CustomValue")]
    [Serializable]
    public class TransformStringDictionary : SerializableDictionary<Transform, string> { }
    [Serializable]
    public class TransformIntDictionary : SerializableDictionary<Transform, int> { }
    [Serializable]
    public class TransformFloatDictionary : SerializableDictionary<Transform, float> { }
    [Serializable]
    public class TransformBoolDictionary : SerializableDictionary<Transform, bool> { }
    [Serializable]
    public class TransformVector3Dictionary : SerializableDictionary<Transform, Vector3> { }
    [Serializable]
    public class TransformGameObjectDictionary : SerializableDictionary<Transform, GameObject> { }
    [Serializable]
    public class TransformComponentDictionary : SerializableDictionary<Transform, Component> { }
    [Serializable]
    public class TransformTransformDictionary : SerializableDictionary<Transform, Transform> { }
    [Serializable]
    public class TransformDictionary<T> : SerializableDictionary<Transform, T> { }
}