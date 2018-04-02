using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadExplore : MonoBehaviour {
    
	void Start () {
        // Load relevant UI here
        MenuManager.Instance.ClearUI();
        //MenuManager.Instance.Load("Interface_Explore");
        MenuManager.Instance.Load("Interface_ActSelect");
    }
}
