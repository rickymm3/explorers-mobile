#define SPEED_OVER_GARBAGE

using System;
using System.Collections;
using UnityEngine;

namespace System.Collections.Generic {
    /// <summary>
    /// Dictionary capable of displaying within the inspector.
    /// Implements same as System.Collections.Generic.Dictionary
    /// </summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue> {
        //Error messages
        private const string ERROR_INVALID_ARRAY_SIZES = "Cannot have a different number of values and keys.";
        private const string ERROR_NON_UNIQUE_KEY_LIST = "Cannot have duplicate keys";

        //The actual dictionary
        protected Dictionary<TKey, TValue> _cachedDict = null;

        //List backing that makes serialization possible!
        [SerializeField]
        protected List<TKey> KeysList = new List<TKey>();
        [SerializeField]
        protected List<TValue> ValuesList = new List<TValue>();

        [NonSerialized]
        protected bool _isValidated = false;
        /// <summary>
        /// Ensures the dictionary is in a valid state before performing any operations
        /// Throws ArgumentException if validation fails.  Because Validation is called
        /// when the object is first accessed.  It is safe to zip up the dictionary from
        /// the KeysList and ValuesList here.
        /// </summary>
        protected virtual void BeforeDictionaryOperation() {
            if (!_isValidated || InspectorWasUpdated) {
                _isValidated = true;
                InspectorWasUpdated = false;

                //Step 1: Ensure the length of the keys and values lists are the same length
                if (KeysList.Count != ValuesList.Count) {
                    //Shouldn't be possible under proper usage with the Inspectionary attribute
                    throw new ArgumentException(ERROR_INVALID_ARRAY_SIZES);
                }
                //Step 2: Ensure all keys are unique
                if (!AreKeysUnique()) {
                    throw new ArgumentException(ERROR_NON_UNIQUE_KEY_LIST);
                }
                //Step 3: Set up the dictionary
                if (_cachedDict == null) {
                    _cachedDict = new Dictionary<TKey, TValue>();
                }
                _cachedDict.Clear();
                //Step 4: Copy values to the dictionary
                for (int i = 0; i < KeysList.Count; i++) {
                    _cachedDict.Add(KeysList[i], ValuesList[i]);
                }
            }
        }

        /// <summary>
        /// Make any updates to the state of the object here.  Called in a finally
        /// block after each dictionary operation.
        /// </summary>
        protected virtual void AfterDictionaryOperation() {
#if UNITY_EDITOR
            //Doesn't need to run unless in the editor and the application is running
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode && _isValidated && UpdateInspectorValues) {
                UpdateInspectorValues = false;
                KeysList.Clear();
                KeysList.AddRange(_cachedDict.Keys);
                ValuesList.Clear();
                ValuesList.AddRange(_cachedDict.Values);
            }
#endif
        }

        /// <summary>
        /// Checks if all entries in the KeysList are unique
        /// </summary>
        private bool AreKeysUnique() {
            //If speed is more important than the initial garbage created by the hashset, comment out the define at
            //the start of the file.  Shouldn't matter either way as this is only called on initial validation
#if SPEED_OVER_GARBAGE
            //Speedy solution is to copy the list to a HashSet and check that the Set's size is the same as the list
            HashSet<TKey> keyHashSet = new HashSet<TKey>(KeysList);
            return keyHashSet.Count == KeysList.Count;
#else
            //Slow non garbage producing method is O(n^2) and must do a full sweep of the entire KeysList
            for (int i = 0; i < KeysList.Count - 1; i++) {
                for (int j = i + 1; j < KeysList.Count; j++) {
                    if (KeysList[i].Equals(KeysList[j])) {
                        return false;
                    }
                }
            }
            return true;
#endif
        }

        //Inspectionary members used only for editor purposes
        #region Inspector only properties
        //Used by the InspectionaryDrawer to mark the object for a dictionary update.
        //This one is marked serialized instead and _isValidated as Nonserialized
        //to avoid any possibility of serializing _isValidated as true
        [SerializeField]
        private bool InspectorWasUpdated = false;

        //Used to know if the editor backing needs to be updated or not.
        [SerializeField]
        private bool UpdateInspectorValues = false;

        //Turn off the warning for unused variables as these are all obtained through
        //the editor script.
#pragma warning disable 0414 
        //Entry labels for display in the editor.  Makes life easier for designers
        [SerializeField]
        private List<string> CustomLabels = new List<string>();
        // True if a label is being edited.
        [SerializeField]
        private bool EditingLabel = false;
        //The index of the label being edited.
        [SerializeField]
        private int LabelNum = 0;
#pragma warning restore 0414
        #endregion

        #region Conversions
        public static implicit operator Dictionary<TKey, TValue>(SerializableDictionary<TKey, TValue> serialized) {
            return serialized._cachedDict;
        }
        public static explicit operator SerializableDictionary<TKey, TValue>(Dictionary<TKey, TValue> regDict) {
            SerializableDictionary<TKey, TValue> newDict = new SerializableDictionary<TKey, TValue>();
            newDict._cachedDict = regDict;
            return newDict;
        }
        #endregion

        //Straight up copy paste functionality for IDictionary<TKey, TValue> with try/finally wrapper
        /* public ($return) ($Method)(($Parameters)) {
         *      try {
         *          BeforeDictionaryOperation();
         *          return _cachedDict.($Method)(($Parameters));
         *      } finally {
         *          AfterDictionaryOperation();
         *      }
         * }
         */
        #region Dictionary function remapping
        #region Properties
        public IEqualityComparer<TKey> Comparer {
            get {
                try {
                    BeforeDictionaryOperation();
                    return _cachedDict.Comparer;
                } finally {
                    AfterDictionaryOperation();
                }
            }
        }
        public int Count {
            get {
                try {
                    BeforeDictionaryOperation();
                    return _cachedDict.Count;
                } finally {
                    AfterDictionaryOperation();
                }
            }
        }
        public Dictionary<TKey, TValue>.KeyCollection Keys {
            get {
                try {
                    BeforeDictionaryOperation(); return _cachedDict.Keys;
                } finally {
                    AfterDictionaryOperation();
                }
            }
        }
        public Dictionary<TKey, TValue>.ValueCollection Values {
            get {
                try {
                    BeforeDictionaryOperation();
                    return _cachedDict.Values;
                } finally {
                    AfterDictionaryOperation();
                }
            }
        }

        public TValue this[TKey key] {
            get {
                try {
                    BeforeDictionaryOperation();
                    return _cachedDict[key];
                } finally {
                    AfterDictionaryOperation();
                }
            }
            set {
                try {
                    BeforeDictionaryOperation();
                    _cachedDict[key] = value;
                } finally {
                    AfterDictionaryOperation();
                }
            }
        }
        #endregion

        #region Dictionary<TKey,TValue> Methods
        public void Add(TKey key, TValue value) {
            try {
                BeforeDictionaryOperation();
                _cachedDict.Add(key, value);
            } finally {
                AfterDictionaryOperation();
            }
        }
        public void Clear() {
            try {
                BeforeDictionaryOperation();
                _cachedDict.Clear();
            } finally {
                AfterDictionaryOperation();
            }
        }
        public bool ContainsKey(TKey key) {
            try {
                BeforeDictionaryOperation();
                return _cachedDict.ContainsKey(key);
            } finally {
                AfterDictionaryOperation();
            }
        }
        public bool ContainsValue(TValue value) {
            try {
                BeforeDictionaryOperation();
                return _cachedDict.ContainsValue(value);
            } finally {
                AfterDictionaryOperation();
            }
        }
        public Dictionary<TKey, TValue>.Enumerator GetEnumerator() {
            try {
                BeforeDictionaryOperation();
                return _cachedDict.GetEnumerator();
            } finally {
                AfterDictionaryOperation();
            }
        }
        public bool Remove(TKey key) {
            try {
                BeforeDictionaryOperation();
                return _cachedDict.Remove(key);
            } finally {
                AfterDictionaryOperation();
            }
        }
        public bool TryGetValue(TKey key, out TValue value) {
            try {
                BeforeDictionaryOperation();
                return _cachedDict.TryGetValue(key, out value);
            } finally {
                AfterDictionaryOperation();
            }
        }
        #endregion

        #region Explicit Implementations (Same as Dictionary<TKey, TValue>)
        #region Properties
        ICollection<TKey> IDictionary<TKey, TValue>.Keys { get { return Keys; } }
        ICollection<TValue> IDictionary<TKey, TValue>.Values { get { return Values; } }
        public bool IsReadOnly {
            get {
                try {
                    BeforeDictionaryOperation();
                    return ((IDictionary)_cachedDict).IsReadOnly;
                } finally {
                    AfterDictionaryOperation();
                }
            }
        }
        #endregion

        #region IDictionary<TKey, TValue> Methods
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() {
            try {
                BeforeDictionaryOperation();
                return _cachedDict.GetEnumerator();
            } finally {
                AfterDictionaryOperation();
            }
        }
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) {
            try {
                BeforeDictionaryOperation();
                _cachedDict[item.Key] = item.Value;
            } finally {
                AfterDictionaryOperation();
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) {
            try {
                BeforeDictionaryOperation();
                return ((IDictionary)_cachedDict).Contains(item);
            } finally {
                AfterDictionaryOperation();
            }
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            try {
                BeforeDictionaryOperation();
                ((IDictionary)_cachedDict).CopyTo(array, arrayIndex);
            } finally {
                AfterDictionaryOperation();
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) {
            try {
                BeforeDictionaryOperation();
                return _cachedDict.Remove(item.Key);
            } finally {
                AfterDictionaryOperation();
            }
        }
        #endregion
        #endregion
        #endregion
    }
}