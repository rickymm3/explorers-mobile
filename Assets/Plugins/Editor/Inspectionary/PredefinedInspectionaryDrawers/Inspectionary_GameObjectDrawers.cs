using SPStudios.Drawers;
using UnityEditor;

namespace SPStudios.SerializableDictionary {
    //Custom property drawers for GameObject keyed Inspectionaries.  Eliminates the need
    //to mark them with the [Inspectionary] attribute unless you want custom labels
    [CustomPropertyDrawer(typeof(GameObjectStringDictionary))]
    public class GameObjectStringInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(GameObjectIntDictionary))]
    public class GameObjectIntInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(GameObjectFloatDictionary))]
    public class GameObjectFloatInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(GameObjectVector3Dictionary))]
    public class GameObjectVector3InspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(GameObjectGameObjectDictionary))]
    public class GameObjectGameObjectInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(GameObjectComponentDictionary))]
    public class GameObjectComponentInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(GameObjectBoolDictionary))]
    public class GameObjectBoolInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
    [CustomPropertyDrawer(typeof(GameObjectTransformDictionary))]
    public class GameObjectTransformInspectionaryDrawer : BaseCustomInspectionaryDrawer { }
}