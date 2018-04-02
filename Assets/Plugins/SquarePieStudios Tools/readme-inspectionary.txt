First of all, thank you for downloading Inspectionary.

To create a dictionary that displays in the inspector, you must follow two easy steps.
1. First create the dictionary class, Unity requires a concrete version of the class in order
   to serialize it.
        [Serializable] //Required for any class to serialize and display in the inspector
        public class CustomDictionary : SerializableDictionary<KeyType, ValueType> { }
        
2. And to use the dictionary, declare it as you would any other variable, you will need
   to include the [Inspectionary] attribute as well.  The attribute can be used to create
   customized inspector labels as well.
        Default:
            [Inspectionary]
            public CustomDictionary TestDictionary1;
        Customized:
            [Inspectionary("KeyLabel", "ValueLabel")]
            public CustomDictionary TestDictionary2;
            
It really is that simple!

To use the dictionary in your scripts, use it exactly as you would a Dictionary<KeyType, ValueType>

If you have any questions, contact me at SquarePieStudios@gmail.com