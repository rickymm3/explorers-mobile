using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempExploreCleanup : MonoBehaviour {
    
    void Start() {
        // Clean up the current selected explore
        PlayerManager.Instance.CompleteCurrentExplore();
    }
	
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Camp");
        }
	}
}
