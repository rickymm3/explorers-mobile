using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PropertyInfo = System.Reflection.PropertyInfo;

namespace SPStudios.Drawers {
    /// <summary>
    /// Creates a base drawer for defining specified or even specialized drawers
    /// </summary>
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>))]
    public class BaseCustomInspectionaryDrawer : InspectionaryDrawer { }

    /// <summary>
    /// Editor display for the Inspectionary
    /// </summary>
    [CustomPropertyDrawer(typeof(InspectionaryAttribute))]
    public class InspectionaryDrawer : PropertyDrawer {
        #region Constants
        //Default labels for the Inspectionary
        protected const string DEFAULT_ENTRY_LABEL = "Entry";
        protected const string DEFAULT_KEY_LABEL = "Key";
        protected const string DEFAULT_VALUE_LABEL = "Value";

        //Error messages
        protected const string INVALID_OBJECT_MESSAGE = "Invalid object marked as [Inspectionary]: {0}";
        protected const string UNIQUE_KEY_ERROR_MESSAGE = "Not all keys are unique.";

        //Label Content with no text to display
        protected static readonly GUIContent _emptyContent = new GUIContent("");
        //Label Content for renaming individual entries
        private static readonly GUIContent _renameButtonContent = new GUIContent("Rename", "Rename this entry.");
        //Label Content for the delete button
        private static readonly GUIContent _deleteButtonContent = new GUIContent("X", "Delete");
        //Label Content for the add new element button
        private static readonly GUIContent _newElementButtonContent = new GUIContent("Add new element");
        #endregion

        #region Local Variables
        //The properties of the Inspectionary being processed and their field names.
        protected SerializedProperty _dictionaryProperty;
        //No field name, this is the property being worked on

        protected SerializedProperty _keys;
        private const string _keysName = "KeysList";

        protected SerializedProperty _values;
        private const string _valuesName = "ValuesList";

        protected SerializedProperty _entryLabels;
        private const string _entryLabelsName = "CustomLabels";

        protected SerializedProperty _labelBeingEdited;
        private const string _labelBeingEditedName = "LabelNum";

        protected SerializedProperty _isEditingLabelBool;
        private const string _isEditingLabelName = "EditingLabel";

        protected SerializedProperty _shouldUpdateInspector;
        private const string _shouldUpdateInspectorName = "UpdateInspectorValues";

        protected SerializedProperty _inspectorWasUpdated;
        private const string _inspectoredWasUpdatedName = "InspectorWasUpdated";

        //The labels for the key and value entries
        protected GUIContent _keyLabel;
        protected GUIContent _valueLabel;
        #endregion

        #region Unity callback overrides
        /// <summary>
        /// Returns the size of the inspectionary property for display in the editor
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (!InitializeInspectionaryProperties(property)) {
                //If the initialization fails, then only display an error
                return DrawerUtil.ERROR_HEIGHT;
            }

            float propertyHeight = DrawerUtil.SINGLE_LINE_HEIGHT + DrawerUtil.HALF_BORDER; //Foldout for inspectionary
            if (!UniqueKeyCheck()) {
                //Add height for an error box.
                propertyHeight += DrawerUtil.ERROR_HEIGHT + DrawerUtil.HALF_BORDER;
            }

            if (property.isExpanded) {
                //Add the height of the inspectionary
                if (_keys.arraySize > 0) {
                    if (UseSimpleDisplay()) {
                        propertyHeight += GetSimpleDisplayHeight();
                    } else {
                        propertyHeight += GetComplexDisplayHeight();
                    }
                }
                propertyHeight += DrawerUtil.SINGLE_LINE_HEIGHT; //Add height for the new element button
            }
            return propertyHeight;
        }

        /// <summary>
        /// Draw to the editor the Inspectionary for easy designer use
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label) {
            if (!InitializeInspectionaryProperties(prop)) {
                ShowHelpBox(position, string.Format(INVALID_OBJECT_MESSAGE, prop.name), MessageType.Error);
                return;
            }

            //Set the height to standard line height of the Inspectionary
            position.height = DrawerUtil.SINGLE_LINE_HEIGHT;
            //Foldout for the inspectionary
            DrawerUtil.ShowFoldout(position, _dictionaryProperty, label);
            position.y += DrawerUtil.SINGLE_LINE_HEIGHT;

            //Only force the inspectionary backing to update if the inspectionary is open
            _shouldUpdateInspector.boolValue = _dictionaryProperty.isExpanded;

            if (!UniqueKeyCheck()) {
                //Display an error message if the inspectionary does not pass the UniqueKey test
                ShowHelpBox(position, UNIQUE_KEY_ERROR_MESSAGE, MessageType.Error);
                position.y += DrawerUtil.ERROR_HEIGHT + DrawerUtil.HALF_BORDER;
            }

            #region Display the dictionary
            //If the inspectionary is open, draw it to the inspector window
            if (_dictionaryProperty.isExpanded) {
                EditorGUI.BeginChangeCheck(); //Start checking for any changes from the user
                position = TabRect(DrawerUtil.FULL_BORDER, position);

                //Display the inspectionary based on the types of the Keys and Values
                if (_keys.arraySize > 0) {
                    if (UseSimpleDisplay()) {
                        ShowSimpleElements(position);
                        position.y += GetSimpleDisplayHeight() + DrawerUtil.HALF_BORDER;
                    } else {
                        ShowComplexElements(position);
                        position.y += GetComplexDisplayHeight();
                    }
                }

                //New element button
                if (DrawerUtil.DisplayButton(TabRect(-DrawerUtil.FOLDOUT_WIDTH, position), _newElementButtonContent, true)) {
                    DrawerUtil.AddNewElementToProp(_keys);
                    DrawerUtil.AddNewElementToProp(_values);
                    DrawerUtil.AddNewElementToProp(_entryLabels);
                    SerializedProperty newLabel = _entryLabels.GetArrayElementAtIndex(_entryLabels.arraySize - 1);
                    newLabel.stringValue = DEFAULT_ENTRY_LABEL;
                    newLabel.isExpanded = false; // Start out collapsed
                }
                position.y += DrawerUtil.SINGLE_LINE_HEIGHT; //For the button

                //If there were any changes in the inspector, inform the object to update the dictionary
                //before using it again.  Only needs to be updated if the application is running.
                if (EditorGUI.EndChangeCheck() && EditorApplication.isPlayingOrWillChangePlaymode) {
                    _inspectorWasUpdated.boolValue = true;
                }
            }
            #endregion
        }
        #endregion

        /// <summary>
        /// Initialize the drawer with all the inspectionary's properties
        /// </summary>
        protected virtual bool InitializeInspectionaryProperties(SerializedProperty dictProperty) {
            _dictionaryProperty = dictProperty;
            _keys = _dictionaryProperty.FindPropertyRelative(_keysName);
            _values = _dictionaryProperty.FindPropertyRelative(_valuesName);

            //Check that the keys and values properties are valid
            if (_keys == null || _values == null ||
                !_keys.isArray || !_values.isArray) {
                return false;
            } else {
                //Check that values is the same size as keys
                if (_values.arraySize > _keys.arraySize) {
                    _values.arraySize = _keys.arraySize;
                } else if (_values.arraySize < _keys.arraySize) {
                    while (_values.arraySize < _keys.arraySize) {
                        DrawerUtil.AddNewElementToProp(_values);
                    }
                }
            }

            //Get customized label data
            _entryLabels = _dictionaryProperty.FindPropertyRelative(_entryLabelsName);
            _labelBeingEdited = _dictionaryProperty.FindPropertyRelative(_labelBeingEditedName);
            _isEditingLabelBool = _dictionaryProperty.FindPropertyRelative(_isEditingLabelName);

            //Check that the label properties are valid
            if (_entryLabels == null || !_entryLabels.isArray ||
                _isEditingLabelBool == null || _labelBeingEdited == null) {
                return false;
            } else {
                //Check that labels is the same size as keys
                if (_entryLabels.arraySize > _keys.arraySize) {
                    _entryLabels.arraySize = _keys.arraySize;
                } else if (_entryLabels.arraySize < _keys.arraySize) {
                    while (_entryLabels.arraySize < _keys.arraySize) {
                        _entryLabels.arraySize += 1;
                        _entryLabels.GetArrayElementAtIndex(_entryLabels.arraySize - 1).stringValue = DEFAULT_ENTRY_LABEL;
                    }
                }
            }

            //Get attribute labels
            InspectionaryAttribute attrib = this.attribute as InspectionaryAttribute;
            _keyLabel = new GUIContent((attrib != null ? attrib.KeyLabel : DEFAULT_KEY_LABEL));
            _valueLabel = new GUIContent((attrib != null ? attrib.ValueLabel : DEFAULT_VALUE_LABEL));

            //Properties for keeping the inspector and dictionary in sync.
            _shouldUpdateInspector = _dictionaryProperty.FindPropertyRelative(_shouldUpdateInspectorName);
            _inspectorWasUpdated = _dictionaryProperty.FindPropertyRelative(_inspectoredWasUpdatedName);
            if (_shouldUpdateInspector == null || _inspectorWasUpdated == null ||
                !_shouldUpdateInspector.propertyType.Equals(SerializedPropertyType.Boolean) ||
                !_inspectorWasUpdated.propertyType.Equals(SerializedPropertyType.Boolean)) {
                return false;
            }
            return true;
        }

        //($TODO): [Replace display code logic with an interface and factory pattern]
        //          Benefits: Specialized displays without needing to override all the things
        //                    Easier to read code
        //          public interface InspectionaryDisplayWindow {
        //              float GetDisplayHeight();
        //              void ShowDisplay(Rect position);
        //          }
        #region Display Code
        #region Simple Inspectionary Display
        /// <summary>
        /// Gets the height for the inspectionary as a SimpleDisplay
        /// </summary>
        protected virtual float GetSimpleDisplayHeight() {
            float simpleHeight = DrawerUtil.SINGLE_LINE_HEIGHT; //Table's labels
            simpleHeight += DrawerUtil.FULL_BORDER * 2; //Top and Bottom borders of the table
            simpleHeight += (DrawerUtil.SINGLE_LINE_HEIGHT * _keys.arraySize); //A line per entry
            simpleHeight += (DrawerUtil.HALF_BORDER * (_keys.arraySize - 1)); //a border between entries
            return simpleHeight;
        }

        /// <summary>
        /// Display each key value pair on a single line
        /// </summary>
        /// <returns>The change in height due to showing all elements in the inspectionary</returns>
        protected virtual void ShowSimpleElements(Rect position) {
            //Save the start indent level
            int guiIndent = EditorGUI.indentLevel;

            //Set up drawing settings
            EditorGUI.indentLevel = 0;
            position = TabRect(DrawerUtil.FOLDOUT_WIDTH, position); //Tab in the table key/value labels
            float halfWidth = position.width / 2;

            //Draw the key/value labels
            EditorGUI.LabelField(new Rect(position.x, position.y, halfWidth, position.height), _keyLabel);
            EditorGUI.LabelField(new Rect(position.x + halfWidth, position.y, halfWidth, position.height), _valueLabel);
            position.y += DrawerUtil.SINGLE_LINE_HEIGHT;

            //Draw the main box.
            float boxHeight = GetSimpleDisplayHeight() - DrawerUtil.SINGLE_LINE_HEIGHT; //box height fills entire field except the labels
            GUI.Box(new Rect(position.x, position.y, position.width, boxHeight), _emptyContent);
            position.y += DrawerUtil.FULL_BORDER;

            //Delete button is located to the left of the table
            Rect deleteButton = new Rect(position.x - DrawerUtil.BUTTON_WIDTH - DrawerUtil.FULL_BORDER, position.y,
                                                      DrawerUtil.BUTTON_WIDTH, position.height);

            //Set up position rect for displaying the entries
            float halfEntryWidth = (position.width - DrawerUtil.FULL_BORDER) / 2 - DrawerUtil.FULL_BORDER;
            float leftBorder = position.x + DrawerUtil.FULL_BORDER;
            position.x = leftBorder;
            position.width = halfEntryWidth;

            //Draw each element within the box including a delete button on the outside for each entry
            for (int i = 0; i < _keys.arraySize; i++) {
                //Draw delete button.
                if (DisplayDeleteButton(deleteButton, i)) {
                    break;
                }
                //Display Key
                DrawerUtil.ShowProperty(position, _keys.GetArrayElementAtIndex(i), _emptyContent);
                position.x += halfEntryWidth + DrawerUtil.FULL_BORDER;
                //Display Value
                DrawerUtil.ShowProperty(position, _values.GetArrayElementAtIndex(i), _emptyContent);

                //Set up rects for next entry
                position.y += DrawerUtil.SINGLE_LINE_HEIGHT + DrawerUtil.HALF_BORDER;
                position.x = leftBorder;
                deleteButton.y = position.y;
            }
            //Reset the indent level
            EditorGUI.indentLevel = guiIndent;
        }
        #endregion

        #region Complex Inspectionary Display
        /// <summary>
        /// Gets the height for the inspectionary as a ComplexDisplay
        /// </summary>
        /// <returns>The height of the entire inspectionary</returns>
        protected virtual float GetComplexDisplayHeight() {
            float complexHeight = 0;
            //Height is entirely based on the number of entries
            for (int i = 0; i < _keys.arraySize; i++) {
                complexHeight += DrawerUtil.SINGLE_LINE_HEIGHT + DrawerUtil.HALF_BORDER; //Entry Label and entry spacing
                if (_entryLabels.GetArrayElementAtIndex(i).isExpanded) {
                    //Box spacing - Above Key(FULL), Between KVP(FULL), Below Value(HALF)
                    complexHeight += DrawerUtil.FULL_BORDER * 3;
                    //Height for the entry's key and value
                    complexHeight += GetComplexEntryDisplayHeight(_keys.GetArrayElementAtIndex(i));
                    complexHeight += GetComplexEntryDisplayHeight(_values.GetArrayElementAtIndex(i));
                }
            }

            return complexHeight;
        }

        /// <summary>
        /// Displays all elements of the inspectionary in customized boxes
        /// </summary>
        /// <returns>The change in height due to showing all elements in the inspectionary</returns>
        protected virtual void ShowComplexElements(Rect position) {
            for (int i = 0; i < _keys.arraySize; i++) {
                SerializedProperty labelProp = _entryLabels.GetArrayElementAtIndex(i);
                if (DisplayLabelWithDeleteButton(position, labelProp, i)) {
                    break;
                }
                position.y += DrawerUtil.SINGLE_LINE_HEIGHT; //Height for label and buttons

                #region Draw This Entry
                if (labelProp.isExpanded) { //Use the label to determine if the entire entry should expand or not
                    //Save the current indentLevel
                    int guiIndent = EditorGUI.indentLevel;
                    //Setting indent level to 0 so that inspectionary will always stretch and look nice in the window.
                    EditorGUI.indentLevel = 0;

                    SerializedProperty key = _keys.GetArrayElementAtIndex(i);
                    SerializedProperty val = _values.GetArrayElementAtIndex(i);
                    float keyHeight = GetComplexEntryDisplayHeight(key);
                    float valueHeight = GetComplexEntryDisplayHeight(val);
                    float boxHeight = keyHeight + valueHeight + (DrawerUtil.FULL_BORDER * 2);
                    Rect entryRect = new Rect(position.x - DrawerUtil.FOLDOUT_WIDTH, position.y + DrawerUtil.HALF_BORDER,
                                              position.width + DrawerUtil.FOLDOUT_WIDTH, boxHeight);

                    GUI.Box(entryRect, _emptyContent);                          //Draw main container box
                    entryRect.y += DrawerUtil.FULL_BORDER;                      //Space between box and key
                    DisplayComplexEntry(entryRect, key, _keyLabel);             //Draw Key
                    entryRect.y += keyHeight + DrawerUtil.FULL_BORDER;          //Space between key and value
                    DisplayComplexEntry(entryRect, val, _valueLabel);           //Draw Value

                    //Reset for next entry
                    EditorGUI.indentLevel = guiIndent;
                    position.y += boxHeight + DrawerUtil.FULL_BORDER;
                }
                #endregion
                position.y += DrawerUtil.HALF_BORDER; //Spacing between entries
            }
        }

        /// <summary>
        /// Gets the display height for a property
        /// </summary>
        protected virtual float GetComplexEntryDisplayHeight(SerializedProperty property) {
            //If the property is closed up, just one line's height
            if (DrawerUtil.PropertyCanExpand(property)) {
                //NonStandard properties should be considered expanded at all times, otherwise they look stupid
                property.isExpanded = property.isExpanded || DrawerUtil.PropertyMustExpand(property);
                if (!property.isExpanded) {
                    return DrawerUtil.SINGLE_LINE_HEIGHT + DrawerUtil.HALF_BORDER;
                }
            }
            //Otherwise return the property's height
            return EditorGUI.GetPropertyHeight(property) + DrawerUtil.HALF_BORDER;
        }

        /// <summary>
        /// Displays a single key or value complex or simple
        /// </summary>
        /// <param name="position">The position to draw the property</param>
        /// <param name="property">The property to be drawn</param>
        /// <param name="label">The label for the property</param>
        /// <returns>The change in height due to showing the property</returns>
        protected virtual void DisplayComplexEntry(Rect position, SerializedProperty property, GUIContent label) {
            position.width -= DrawerUtil.FULL_BORDER; //Tab in from the right side
            float propHeight = EditorGUI.GetPropertyHeight(property);
            //Check if the property should be expanded or not.
            if (DrawerUtil.PropertyCanExpand(property)) {
                property.isExpanded = property.isExpanded || DrawerUtil.PropertyMustExpand(property);
                if (property.isExpanded) {
                    if (DrawerUtil.IsStandardProperty(property)) {
                        //If the property is expanded and is standard, draw a box for it.
                        position = TabRect(DrawerUtil.FOLDOUT_WIDTH, position); //Tab in the entry
                        float keyBoxHeight = propHeight - DrawerUtil.SINGLE_LINE_HEIGHT + DrawerUtil.HALF_BORDER;
                        GUI.Box(new Rect(position.x, position.y + DrawerUtil.SINGLE_LINE_HEIGHT, position.width + DrawerUtil.FULL_BORDER, keyBoxHeight), _emptyContent);
                    }
                    //Draw out the property
                    position.height = propHeight;
                    DrawerUtil.ShowProperty(position, property, label, true);
                } else {
                    //Just draw the foldout
                    position = TabRect(DrawerUtil.FOLDOUT_WIDTH, position); //Tab in the entry
                    position.height = DrawerUtil.SINGLE_LINE_HEIGHT;
                    DrawerUtil.ShowFoldout(position, property, label);
                }
            } else {
                //Draw out the simple property
                position.height = propHeight;
                DrawerUtil.ShowProperty(position, property, label, true);
            }
        }


        /// <summary>
        /// Displays an entry's label along with a rename button and a delete button.
        /// </summary>
        /// <returns>Returns whether or not the delete button was pressed</returns>
        private bool DisplayLabelWithDeleteButton(Rect position, SerializedProperty labelProp, int curIndex) {
            //Tab in to conform with general look of inspector properties
            position = TabRect(DrawerUtil.FOLDOUT_WIDTH, position);
            //Width -= Rename and Delete buttons
            position.width -= DrawerUtil.BUTTON_WIDTH * 4 + DrawerUtil.FULL_BORDER * 2;
            //Display the label for this entry.
            ShowEntryLabel(curIndex, labelProp, position);

            //Set up for rename button, size is 3 button widths
            position.x += position.width + DrawerUtil.FULL_BORDER;
            position.width = DrawerUtil.BUTTON_WIDTH * 3;
            //Display a button to allow the user to edit the name of the Inspectionary entry
            if (DrawerUtil.DisplayButton(position, _renameButtonContent, false)) {
                if (_labelBeingEdited.intValue != curIndex || _isEditingLabelBool.boolValue == false) {
                    //Turn on editing for an entry if it isn't already being edited
                    _isEditingLabelBool.boolValue = true;
                    _labelBeingEdited.intValue = curIndex;
                } else {
                    //Turn off editing if its the entry already being edited
                    _isEditingLabelBool.boolValue = false;
                }
            }

            //Set up the delete button
            position.x += DrawerUtil.BUTTON_WIDTH * 3 + DrawerUtil.FULL_BORDER;
            position.width = DrawerUtil.BUTTON_WIDTH;
            return DisplayDeleteButton(position, curIndex);
        }

        /// <summary>
        /// Displays a label and returns the position for the buttons
        /// </summary>
        /// <param name="curIndex">Current index being displayed</param>
        /// <param name="entryLabel">The property for saving the entry's customized name</param>
        /// <param name="position">Where the label will be placed</param>
        /// <returns>The end x position of the entry labels</returns>
        private void ShowEntryLabel(int curIndex, SerializedProperty entryLabel, Rect position) {
            //Construct the label
            string labelText;
            if (entryLabel.stringValue != DEFAULT_ENTRY_LABEL) {
                labelText = entryLabel.stringValue;
            } else {
                labelText = string.Format("{0} {1}", DEFAULT_ENTRY_LABEL, (curIndex + 1));
            }
            //If the label is being edited, don't show the foldout and instead display the property field
            if (_isEditingLabelBool.boolValue && _labelBeingEdited.intValue == curIndex) {
                DrawerUtil.ShowProperty(position, entryLabel, _emptyContent, false);
            } else {
                //Set entryLabel's .isExpanded as the foldout property for each entry.
                DrawerUtil.ShowFoldout(position, entryLabel, new GUIContent(labelText));
            }
        }
        #endregion

        /// <summary>
        /// Returns if the entry can be displayed on a single line or not and ergo should use SimpleDisplay
        /// </summary>
        protected virtual bool UseSimpleDisplay() {
            if (_keys.arraySize > 0) {
                SerializedProperty key = _keys.GetArrayElementAtIndex(0); //Just grab the first one.
                bool keySingleLine = EditorGUI.GetPropertyHeight(key) == DrawerUtil.SINGLE_LINE_HEIGHT && !DrawerUtil.PropertyCanExpand(key);
                if (keySingleLine) {
                    SerializedProperty val = _values.GetArrayElementAtIndex(0);
                    bool valueSingleLine = EditorGUI.GetPropertyHeight(val) == DrawerUtil.SINGLE_LINE_HEIGHT && !DrawerUtil.PropertyCanExpand(val);
                    return valueSingleLine;
                }
            }
            return false;
        }
        #endregion

        /// <summary>
        /// Adjusts the rectangle's position and width such that it tabs the rectangle,
        /// but doesn't change it's length within the inspector.
        /// </summary>
        protected static Rect TabRect(float dist, Rect rect) {
            rect.x += dist;
            rect.width -= dist;
            return rect;
        }

        /// <summary>
        /// Displays an error box with the selected message
        /// </summary>
        protected virtual void ShowHelpBox(Rect position, string message, MessageType messageType) {
            position.height = DrawerUtil.ERROR_HEIGHT;
            EditorGUI.HelpBox(position, message, messageType);
        }

        /// <summary>
        /// Displays a delete button that corresponds to an entry in the dictionary
        /// </summary>
        protected bool DisplayDeleteButton(Rect position, int deleteIndex) {
            if (DrawerUtil.DisplayButton(position, _deleteButtonContent)) {
                DrawerUtil.DeleteElementFromArray(_keys, deleteIndex);
                DrawerUtil.DeleteElementFromArray(_values, deleteIndex);
                DrawerUtil.DeleteElementFromArray(_entryLabels, deleteIndex);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if the entries in an array property has all unique entries
        /// </summary>
        protected virtual bool UniqueKeyCheck() {
            if (_keys.isArray && _keys.arraySize > 1) {
                string propName = DrawerUtil.GetPropFieldName(_keys.GetArrayElementAtIndex(0));
                if (!string.IsNullOrEmpty(propName)) {
                    return CheckArrayValuesAreUnique(propName);
                }
            }

            //Else, no way to check
            return true;
        }

        /// <summary>
        /// Checks that none of the elements in the arrayProp's propertyField's match
        /// </summary>
        /// <param name="propertyField">The field of the array to check against</param>
        private bool CheckArrayValuesAreUnique(string propertyField) {
            PropertyInfo property = typeof(SerializedProperty).GetProperty(propertyField);
            if (property != null) {
                HashSet<object> containedObjects = new HashSet<object>();
                for (int i = 0; i < _keys.arraySize; i++) {
                    object key = property.GetValue(_keys.GetArrayElementAtIndex(i), null);
                    if (containedObjects.Contains(key)) {
                        return false;
                    }
                    containedObjects.Add(key);
                }
            }
            return true;
        }
    }
}