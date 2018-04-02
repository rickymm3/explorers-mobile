using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerSingleton<T> : Singleton<T> where T : Tracer {

    public static GameAPIManager API { get { return GameAPIManager.Instance; } }
    public static BuildManager buildMan { get { return BuildManager.Instance; } }
    public static GameManager gameMan { get { return GameManager.Instance; } }
    public static DataManager dataMan { get { return DataManager.Instance; } }
    public static MenuManager menuMan { get { return MenuManager.Instance; } }
    public static AudioManager audioMan { get { return AudioManager.Instance; } }
    public static PlayerManager playerMan { get { return PlayerManager.Instance; } }
    public static PlayerSignals signals { get { return PlayerManager.Instance.Signals; } }
    public static GlobalsData globals { get { return dataMan.globalData; } }
    public static CurrencyManager coinMan { get { return CurrencyManager.Instance; } }

    public static GameObject GetDynamics() {
        return GameObject.Find("_Dynamics");
    }

    public static GameObject GetDynamics(string nestedName) {
        GameObject dynamics = GetDynamics();
        GameObject nestedGO = null;
        Transform nestedTrans = dynamics.transform.Find(nestedName);

        if (nestedTrans == null) {
            nestedGO = new GameObject(nestedName);
            nestedTrans = nestedGO.transform;
            nestedTrans.parent = dynamics.transform;
        } else {
            nestedGO = nestedTrans.gameObject;
        }

        return nestedGO;
    }
}
