using System;
using System.Collections.Generic;
using UnityEngine;

namespace SPStudios.SerializableDictionary {
    //Predefined SerializedDictionaries for Component keyed dictionaries.
    //Used with predefined property drawer to eliminate the need to use the
    //[Inspectionary] attribute unless you want custom labels [Inspectionary("CustomKey", "CustomValue")]
    [Serializable]
    public class ComponentStringDictionary : SerializableDictionary<Component, string> { }
    [Serializable]
    public class ComponentIntDictionary : SerializableDictionary<Component, int> { }
    [Serializable]
    public class ComponentFloatDictionary : SerializableDictionary<Component, float> { }
    [Serializable]
    public class ComponentBoolDictionary : SerializableDictionary<Component, bool> { }
    [Serializable]
    public class ComponentVector3Dictionary : SerializableDictionary<Component, Vector3> { }
    [Serializable]
    public class ComponentGameObjectDictionary : SerializableDictionary<Component, GameObject> { }
    [Serializable]
    public class ComponentComponentDictionary : SerializableDictionary<Component, Component> { }
    [Serializable]
    public class ComponentTransformDictionary : SerializableDictionary<Component, Transform> { }
    [Serializable]
    public class ComponentDictionary<T> : SerializableDictionary<Component, T> { }
}