using System;
using System.Collections.Generic;
using UnityEngine;

namespace SPStudios.SerializableDictionary {
    //Predefined SerializedDictionaries for GameObject keyed dictionaries.
    //Used with predefined property drawer to eliminate the need to use the
    //[Inspectionary] attribute unless you want custom labels [Inspectionary("CustomKey", "CustomValue")]
    [Serializable]
    public class GameObjectStringDictionary : SerializableDictionary<GameObject, string> { }
    [Serializable]
    public class GameObjectIntDictionary : SerializableDictionary<GameObject, int> { }
    [Serializable]
    public class GameObjectFloatDictionary : SerializableDictionary<GameObject, float> { }
    [Serializable]
    public class GameObjectBoolDictionary : SerializableDictionary<GameObject, bool> { }
    [Serializable]
    public class GameObjectVector3Dictionary : SerializableDictionary<GameObject, Vector3> { }
    [Serializable]
    public class GameObjectGameObjectDictionary : SerializableDictionary<GameObject, GameObject> { }
    [Serializable]
    public class GameObjectComponentDictionary : SerializableDictionary<GameObject, Component> { }
    [Serializable]
    public class GameObjectTransformDictionary : SerializableDictionary<GameObject, Transform> { }
    [Serializable]
    public class GameObjectDictionary<T> : SerializableDictionary<GameObject, T> { }
}