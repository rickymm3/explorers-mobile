using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SnapToRightOfBar : MonoBehaviour {

    public RectTransform icon;
    public RectTransform bar;
    public Vector2 offset = new Vector2(50f, 0f);

    void Update() {
        icon.anchoredPosition = new Vector2(bar.anchoredPosition.x + bar.localScale.x * bar.rect.width, bar.anchoredPosition.y) + offset;
        //print(bar.sizeDelta.x);
    }
}
