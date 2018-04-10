using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingCombatTextInterface : Panel {
    public GameObject floatingCombatTextPrefab;

    List<GameObject> pool = new List<GameObject>();

    public void SpawnText(string message, Vector2 position, float textSize = 72f, CombatHitType type = CombatHitType.Normal, Color? textColor = null) {
        GameObject textObj = GetCombatTextObject();

        textObj.SetActive(true);
        textObj.transform.SetParent(this.transform);
        textObj.transform.localScale = Vector3.one;
        textObj.transform.SetAsLastSibling();

        // Init the UI
        textObj.GetComponent<FloatingCombatTextObject>().Initialize(message, position, textSize, type, textColor);
    }
    
    GameObject GetCombatTextObject() {
        foreach (GameObject go in pool) {
            if (go.activeSelf == false) {
                go.SetActive(true);
                return go;
            }
        }

        GameObject HeroPanel = (GameObject) Instantiate(floatingCombatTextPrefab);
        pool.Add(HeroPanel);
        return HeroPanel;
    }
}
