using SPStudios.Drawers;
using UnityEditor;

namespace SPStudios.SerializableDictionary {
    //Custom property drawers for int keyed Inspectionaries.  Eliminates the need
    //to mark them with the [Inspectionary] attribute unless you want custom labels
    [CustomPropertyDrawer(typeof(IntStringDictionary))]
    public class IntStringInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(IntIntDictionary))]
    public class IntIntInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(IntFloatDictionary))]
    public class IntFloatInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(IntVector3Dictionary))]
    public class IntVector3InspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(IntGameObjectDictionary))]
    public class IntGameObjectInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(IntComponentDictionary))]
    public class IntComponentInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(IntBoolDictionary))]
    public class IntBoolInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(IntTransformDictionary))]
    public class IntTransformInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
}