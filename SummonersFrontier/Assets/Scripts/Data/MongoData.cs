using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MongoData<T> : IMongoData where T : BaseData {
    int _MongoID = 0;
    public T data;
    public string className = "Unknown";

    public static GameAPIManager API { get { return GameAPIManager.API; } }
    public static DataManager dataMan { get { return DataManager.Instance; } }

    public string DebugID { get { return (data==null ? className : data.FullIdentity) + " #" + MongoID; } }
    public int MongoID {
        get { return _MongoID; }
        set {
            if(value<1) {
                Tracer.trace("SETTING MONGO_ID TO < 1: " + DebugID + " = " + value);
            }
            _MongoID = value;
        }
    }
}

public interface IMongoData {
    int MongoID { get; set; }
    string DebugID { get; }
}