using UnityEngine;

namespace System.Collections.Generic {
    /// <summary>
    /// Attribute for labeling a SerializedDictionary to display in the inspector
    /// </summary>
    public class InspectionaryAttribute : PropertyAttribute {
        //Default label values
        protected const string DEFAULT_KEY_LABEL = "Key";
        protected const string DEFAULT_VALUE_LABEL = "Value";

        //The label to show in the Unity inspector for an entry's key.
        public readonly string KeyLabel;
        //The label to show in the Unity inspector for an entry's value.
        public readonly string ValueLabel;

        public InspectionaryAttribute(string keyLabel = DEFAULT_KEY_LABEL, string valueLabel = DEFAULT_VALUE_LABEL) {
            this.KeyLabel = keyLabel;
            this.ValueLabel = valueLabel;
        }
    }
}