using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalPositionAdjuster : MonoBehaviour {

    RectTransform localRectTransform = null;

    public RectTransform targetRectTransform;
    public bool yIsNegative = true;
    public float verticalOffset = 25f;

    Vector2 adjustedPosition = Vector2.zero;

    void Start() {
        localRectTransform = GetComponent<RectTransform>();
    }

	void Update () {
        AdjustPosition();
    }

    [ContextMenu("Adjust Position")]
    void AdjustPosition() {
        if (!localRectTransform) localRectTransform = GetComponent<RectTransform>();
        if (!localRectTransform) return;
        if (!targetRectTransform) return;

        adjustedPosition = localRectTransform.anchoredPosition;

        if (yIsNegative)
            adjustedPosition.y = (targetRectTransform.anchoredPosition.y - targetRectTransform.sizeDelta.y - verticalOffset);
        else
            adjustedPosition.y = (targetRectTransform.anchoredPosition.y + targetRectTransform.sizeDelta.y + verticalOffset);

        localRectTransform.anchoredPosition = adjustedPosition;
    }
}
