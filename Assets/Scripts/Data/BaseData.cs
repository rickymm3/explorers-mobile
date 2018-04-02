using UnityEngine;

[System.Serializable]
public class BaseData {
    public static DataManager dataMan { get { return DataManager.Instance; } }

    [SerializeField]
    string _identity = "";
    public string Identity {
        get { return _identity; }
        set { _identity = value; }
    }

    [SerializeField]
    int _id = -1;
    public int ID {
        get { return _id; }
        set { _id = value; }
    }
    
    public string FullIdentity {
        get { return ID + "_" + Identity; }
    }

    public override string ToString() {
        return Identity;
    }
}
