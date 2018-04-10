using SPStudios.Drawers;
using UnityEditor;

namespace SPStudios.SerializableDictionary {
    //Custom property drawers for Vector3 keyed Inspectionaries.  Eliminates the need
    //to mark them with the [Inspectionary] attribute unless you want custom labels
    [CustomPropertyDrawer(typeof(Vector3StringDictionary))]
    public class Vector3StringInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(Vector3IntDictionary))]
    public class Vector3IntInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(Vector3FloatDictionary))]
    public class Vector3FloatInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(Vector3Vector3Dictionary))]
    public class Vector3Vector3InspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(Vector3GameObjectDictionary))]
    public class Vector3GameObjectInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(Vector3ComponentDictionary))]
    public class Vector3ComponentInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(Vector3BoolDictionary))]
    public class Vector3BoolInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(Vector3TransformDictionary))]
    public class Vector3TransformInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
}