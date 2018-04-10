using UnityEngine;
using System.Collections;

public class ForceSceneLoad : MonoBehaviour {

	public string scene = "";
    public bool RequireClick = false;

	void Start () {
        if (!RequireClick)
		    UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
	}

    void Update() {
        if (RequireClick)
            if (Input.GetMouseButtonDown(0))
                UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }
}
