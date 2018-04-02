using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class PulseTextColor : MonoBehaviour {

    public Color textColor = Color.white;
    public float speed = 1f;
    public RangedFloat alphaRange;
    TextMeshProUGUI textObj;

	void Start () {
        textObj = GetComponent<TextMeshProUGUI>();
        if (textObj != null)
            StartCoroutine(PulseAlphaToFull());
        else
            Debug.LogError("Missing text component for pulse");
	}

    IEnumerator PulseAlphaToFull() {
        while (true) {
            textColor.a = alphaRange.maxValue;
            yield return textObj.DOColor(textColor, speed).WaitForCompletion();
            textColor.a = alphaRange.minValue;
            yield return textObj.DOColor(textColor, speed).WaitForCompletion();
        }
    }
}
