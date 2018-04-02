using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadCrates : MonoBehaviour {

	// Use this for initialization
	void Start () {
        MenuManager.Instance.ClearUI();
        MenuManager.Instance.Load("Interface_Crates");
    }
}
