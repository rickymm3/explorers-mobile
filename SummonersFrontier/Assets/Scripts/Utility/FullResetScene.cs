using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class FullResetScene : MonoBehaviour {
    public string nextScene;

    void Start() {
        StartCoroutine(__WaitUntilAllScenesUnloaded());
    }

    private IEnumerator __WaitUntilAllScenesUnloaded() {
        yield return new WaitForSeconds(1);

        var scenes = SceneManager.GetAllScenes();

        foreach (Scene scene in scenes) {
            if (scene.name == "RootReset") continue;

            SceneManager.UnloadSceneAsync(scene);
        }

        var managers = GameObject.Find("Managers");
        Destroy(managers);

        var cameras = Camera.allCameras;
        foreach (Camera cam in cameras) {
            Destroy(cam.gameObject);
        }

        while (SceneManager.sceneCount>1) yield return new WaitForEndOfFrame();

        yield return new WaitForSeconds(1);

        SceneManager.LoadSceneAsync(nextScene);
    }
}
