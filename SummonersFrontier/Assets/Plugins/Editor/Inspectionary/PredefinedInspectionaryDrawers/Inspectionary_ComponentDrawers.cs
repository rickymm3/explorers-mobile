using SPStudios.Drawers;
using UnityEditor;

namespace SPStudios.SerializableDictionary {
    //Custom property drawers for Component(e.g. MonoBehaviour) keyed Inspectionaries.  Eliminates the need
    //to mark them with the [Inspectionary] attribute unless you want custom labels
    [CustomPropertyDrawer(typeof(ComponentStringDictionary))]
    public class ComponentStringInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(ComponentIntDictionary))]
    public class ComponentIntInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(ComponentFloatDictionary))]
    public class ComponentFloatInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(ComponentVector3Dictionary))]
    public class ComponentVector3InspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(ComponentGameObjectDictionary))]
    public class ComponentGameObjectInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(ComponentComponentDictionary))]
    public class ComponentComponentInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(ComponentBoolDictionary))]
    public class ComponentBoolInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(ComponentTransformDictionary))]
    public class ComponentTransformInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
}