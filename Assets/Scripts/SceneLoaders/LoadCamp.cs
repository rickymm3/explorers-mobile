using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadCamp : MonoBehaviour {

    [HideInInspector] public CampInterface campInterface = null;

	void Start () {
        MenuManager.Instance.ClearUI();
        Invoke("LoadCampInterface", 0.01f);
	}

    void LoadCampInterface() {
        campInterface = (CampInterface) MenuManager.Instance.Load("Interface_Camp");
        campInterface.Initialize((HeaderBarInterface) MenuManager.Instance.Load("Interface_HeaderBar"));
    }
}
