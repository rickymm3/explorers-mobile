using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FloatingCrystal : MonoBehaviour {

    RectTransform crystalRect;

	void Start () {
        crystalRect = GetComponent<RectTransform>();
        StartCoroutine(FloatingMovement());
	}
	
	IEnumerator FloatingMovement() {
        while (true) {
            yield return crystalRect.DOAnchorPosY(-10f, 3f).WaitForCompletion();
            yield return crystalRect.DOAnchorPosY(0f, 3f).WaitForCompletion();
        }
    }
}
