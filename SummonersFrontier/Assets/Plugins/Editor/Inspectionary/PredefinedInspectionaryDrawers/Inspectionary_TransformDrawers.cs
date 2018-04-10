using SPStudios.Drawers;
using UnityEditor;

namespace SPStudios.SerializableDictionary {
    //Custom property drawers for Transform keyed Inspectionaries.  Eliminates the need
    //to mark them with the [Inspectionary] attribute unless you want custom labels
    [CustomPropertyDrawer(typeof(TransformStringDictionary))]
    public class TransformStringInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(TransformIntDictionary))]
    public class TransformIntInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(TransformFloatDictionary))]
    public class TransformFloatInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(TransformVector3Dictionary))]
    public class TransformVector3InspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(TransformGameObjectDictionary))]
    public class TransformGameObjectInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(TransformComponentDictionary))]
    public class TransformComponentInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(TransformBoolDictionary))]
    public class TransformBoolInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(TransformTransformDictionary))]
    public class TransformTransformInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
}