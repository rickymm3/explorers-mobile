using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace SPStudios.Drawers {
    /// <summary>
    /// Utility class for Drawer properties
    /// </summary>
    public static class DrawerUtil {
        //Spacing ratios
        public static float SINGLE_LINE_HEIGHT { get { return EditorGUIUtility.singleLineHeight; } }
        public static float FULL_BORDER { get { return SINGLE_LINE_HEIGHT / 4f; } }
        public static float HALF_BORDER { get { return SINGLE_LINE_HEIGHT / 8f; } }
        public static float ERROR_HEIGHT { get { return (SINGLE_LINE_HEIGHT * 2) + FULL_BORDER; } }
        public static float FOLDOUT_WIDTH { get { return SINGLE_LINE_HEIGHT - HALF_BORDER; } }
        public static float BUTTON_WIDTH { get { return SINGLE_LINE_HEIGHT * 1.5f; } }

        /// <summary>
        /// Displays the foldout toggle for the property.
        /// </summary>
        /// <param name="position">Position to draw the foldout</param>
        /// <param name="property">The property for the foldout</param>
        /// <param name="label">Label for the foldout</param>
        /// <param name="toggleOnLabelClick">Allows the foldout to be updated by clicking on the label</param>
        public static void ShowFoldout(Rect position, SerializedProperty property, GUIContent label, bool toggleOnLabelClick = true) {
            label = EditorGUI.BeginProperty(position, label, property);
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, toggleOnLabelClick);
            EditorGUI.EndProperty();
        }
        /// <summary>
        /// Displays the property
        /// </summary>
        /// <param name="position">Position to draw the property</param>
        /// <param name="property">Property being drawn</param>
        /// <param name="label">Label for the property field</param>
        /// <param name="includeChildren">Whether to include child objects or not</param>
        public static void ShowProperty(Rect position, SerializedProperty property, GUIContent label, bool includeChildren = true) {
            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, property, label, includeChildren);
            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Displays a button at the desired location with the given label
        /// </summary>
        /// <param name="position">The button's position</param>
        /// <param name="label">Label for the button</param>
        /// <param name="displayWhileRunning">Whether the button should display while the game is running</param>
        /// <returns>If the button is clicked or not</returns>
        public static bool DisplayButton(Rect position, string label, bool displayWhileRunning = true) {
            bool canDisplay = displayWhileRunning || !EditorApplication.isPlayingOrWillChangePlaymode;
            if (canDisplay) {
                return GUI.Button(position, label);
            }

            return false;
        }
        /// <summary>
        /// Displays a button at the desired location with the given label
        /// </summary>
        /// <param name="position">The button's position</param>
        /// <param name="label">Label for the button</param>
        /// <param name="displayWhileRunning">Whether the button should display while the game is running</param>
        /// <returns>If the button is clicked or not</returns>
        public static bool DisplayButton(Rect position, GUIContent label, bool displayWhileRunning = true) {
            bool canDisplay = displayWhileRunning || !EditorApplication.isPlayingOrWillChangePlaymode;
            if (canDisplay) {
                return GUI.Button(position, label);
            }

            return false;
        }

        /// <summary>
        /// Cleanly deletes an element from a property thats an array
        /// </summary>
        /// <param name="arrayProp">The array property</param>
        /// <param name="deleteIndex">The index to delete</param>
        public static void DeleteElementFromArray(SerializedProperty arrayProp, int deleteIndex) {
            if (arrayProp.isArray) {
                SerializedProperty propToDelete = arrayProp.GetArrayElementAtIndex(deleteIndex);
                //There's a bug with calling DeleteArrayElementAtIndex on a character array, so just copy
                // from the back.
                if (propToDelete.propertyType == SerializedPropertyType.Character) {
                    for (int j = deleteIndex; j < arrayProp.arraySize - 1; j++) {
                        arrayProp.GetArrayElementAtIndex(j).intValue = arrayProp.GetArrayElementAtIndex(j + 1).intValue;
                    }
                    arrayProp.arraySize--;
                } else {
                    //For some reason, isExpanded data doesn't travel with the property or something...
                    for (int k = deleteIndex; k < arrayProp.arraySize - 1; k++) {
                        arrayProp.GetArrayElementAtIndex(k).isExpanded = arrayProp.GetArrayElementAtIndex(k + 1).isExpanded;
                    }
                    int startSize = arrayProp.arraySize;
                    arrayProp.DeleteArrayElementAtIndex(deleteIndex);
                    //Sometimes a single delete only deletes the value and not the actual element
                    if (startSize == arrayProp.arraySize) {
                        arrayProp.DeleteArrayElementAtIndex(deleteIndex);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a property should be expanded or single line
        /// </summary>
        /// <param name="property"></param>
        /// <returns>True if property will need more than a single line based on type</returns>
        public static bool PropertyCanExpand(SerializedProperty property) {
            return property.hasChildren && PropertyTypeCanExpand(property.propertyType);
        }

        /// <summary>
        /// Returns whether the property type can be expanded with a foldout or not
        /// </summary>
        public static bool PropertyTypeCanExpand(SerializedPropertyType propType) {
            return !propType.Equals(SerializedPropertyType.Color) &&
                   !propType.Equals(SerializedPropertyType.LayerMask) &&
                   !propType.Equals(SerializedPropertyType.ObjectReference) &&
                   !propType.Equals(SerializedPropertyType.String) &&
                   !propType.Equals(SerializedPropertyType.Vector2) &&
                   !propType.Equals(SerializedPropertyType.Vector3);
        }

        /// <summary>
        /// Checks if a property must always be in expanded form or not
        /// </summary>
        public static bool PropertyMustExpand(SerializedProperty property) {
            return PropertyTypeMustExpand(property.propertyType);
        }
        /// <summary>
        /// Returns whether the property type should always be expanded
        /// </summary>
        public static bool PropertyTypeMustExpand(SerializedPropertyType propType) {
            return propType.Equals(SerializedPropertyType.AnimationCurve) ||
                   propType.Equals(SerializedPropertyType.Bounds) ||
                   propType.Equals(SerializedPropertyType.Rect);
        }

        /// <summary>
        /// Returns whether this is a standard property or one of the few special ones.
        /// </summary>
        public static bool IsStandardProperty(SerializedProperty property) {
            return !PropertyMustExpand(property);
        }
        /// <summary>
        /// Returns whether this is a standard property type or one of the few special ones.
        /// </summary>
        public static bool IsStandardProperty(SerializedPropertyType propType) {
            return !PropertyTypeMustExpand(propType);
        }

        /// <summary>
        /// Clears the value of a property to default value
        /// </summary>
        /// <param name="property"></param>
        public static void ClearPropValue(SerializedProperty property) {
            switch(property.propertyType) {
                case SerializedPropertyType.LayerMask:
                    property.intValue = 0;
                    break;
                case SerializedPropertyType.Integer:
                    property.intValue = 0;
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = 0;
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = "";
                    break;
                case SerializedPropertyType.Character:
                    property.intValue = 0;
                    break;
                case SerializedPropertyType.AnimationCurve:
                    property.animationCurveValue = new AnimationCurve();
                    break;
                case SerializedPropertyType.Bounds:
                    property.boundsValue = new Bounds();
                    break;
                case SerializedPropertyType.Color:
                    property.colorValue = Color.white;
                    break;
                case SerializedPropertyType.Rect:
                    property.rectValue = new Rect();
                    break;
                case SerializedPropertyType.Vector2:
                    property.vector2Value = Vector2.zero;
                    break;
                case SerializedPropertyType.Vector3:
                    property.vector3Value = Vector3.zero;
                    break;
                case SerializedPropertyType.Enum:
                    property.enumValueIndex = 0;
                    break;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceInstanceIDValue = 0;
                    property.objectReferenceValue = null;
                    break;
                case SerializedPropertyType.Quaternion:
                    property.quaternionValue = new Quaternion();
                    break;
                case SerializedPropertyType.Vector4:
                    property.vector4Value = Vector4.zero;
                    break;
            }
            if(!IsStandardProperty(property)) {
                property.isExpanded = true;
            }
        }

        /// <summary>
        /// Adds a new element to an array property.
        /// </summary>
        public static void AddNewElementToProp(SerializedProperty property) {
            if (property.isArray) {
                property.arraySize++;
                ClearPropValue(property.GetArrayElementAtIndex(property.arraySize - 1));
            }
        }

        /// <summary>
        /// Returns the property's fieldName that the SP represents
        /// </summary>
        public static string GetPropFieldName(SerializedProperty prop) {
            return GetPropFieldName(prop.propertyType);
        }
        /// <summary>
        /// Returns the property field name for the given prop type
        /// </summary>
        public static string GetPropFieldName(SerializedPropertyType propType) {
            if (_propTypesToFieldName.ContainsKey(propType)) {
                return _propTypesToFieldName[propType];
            }

            return string.Empty;
        }
        /// <summary>
        /// A dictionary of prop types to their corresponding property field name for value lookup
        /// </summary>
        private static Dictionary<SerializedPropertyType, string> _propTypesToFieldName = new Dictionary<SerializedPropertyType, string> {
            { SerializedPropertyType.AnimationCurve, "animationCurveValue" },
            { SerializedPropertyType.Boolean, "boolValue" },
            { SerializedPropertyType.Bounds, "boundsValue" },
            { SerializedPropertyType.Character, "intValue" },
            { SerializedPropertyType.Color, "colorValue" },
            { SerializedPropertyType.Enum, "enumValueIndex" },
            { SerializedPropertyType.Float, "floatValue" },
            { SerializedPropertyType.Integer, "intValue" },
            { SerializedPropertyType.ObjectReference, "objectReferenceValue" },
            { SerializedPropertyType.Rect, "rectValue" },
            { SerializedPropertyType.String, "stringValue" },
            { SerializedPropertyType.Vector2, "vector2Value" },
            { SerializedPropertyType.Vector3, "vector3Value" },
            { SerializedPropertyType.Quaternion, "quaternionValue" },
            { SerializedPropertyType.Vector4, "vector4Value" },
        };
    }
}