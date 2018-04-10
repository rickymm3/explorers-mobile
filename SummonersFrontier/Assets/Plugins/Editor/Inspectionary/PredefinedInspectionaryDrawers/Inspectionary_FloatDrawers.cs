using SPStudios.Drawers;
using UnityEditor;

namespace SPStudios.SerializableDictionary {
    //Custom property drawers for float keyed Inspectionaries.  Eliminates the need
    //to mark them with the [Inspectionary] attribute unless you want custom labels
    [CustomPropertyDrawer(typeof(FloatStringDictionary))]
    public class floatStringInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(FloatIntDictionary))]
    public class floatIntInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(FloatFloatDictionary))]
    public class floatFloatInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(FloatVector3Dictionary))]
    public class floatVector3InspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(FloatGameObjectDictionary))]
    public class floatGameObjectInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(FloatComponentDictionary))]
    public class floatComponentInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(FloatBoolDictionary))]
    public class floatBoolInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(FloatTransformDictionary))]
    public class floatTransformInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
}