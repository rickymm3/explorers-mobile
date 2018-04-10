using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

[Serializable]
public class ManagerPrefabLoader : Tracer {

    public List<GameObject> prefabs = new List<GameObject>();

	// Use this for initialization
	void Start () {
		foreach(GameObject prefab in prefabs) {
            if (prefab == null) {
                traceWarn("ManagerPrefabLoader - prefab is null in GameObject: " + gameObject.name);
                continue;
            }

            GameObject inst = Instantiate(prefab);
            this.AddChild(inst);
        }
	}
}
