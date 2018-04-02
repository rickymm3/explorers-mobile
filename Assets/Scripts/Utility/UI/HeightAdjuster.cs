using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightAdjuster : MonoBehaviour {

    RectTransform localRectTransform = null;

    public float minHeight = 0f;
    public List<RectTransform> targetRectTransforms;
    public float AdditionalHeight = 0f;
    public float SectionSpacing = 25f;

    Vector2 adjustedSize = Vector2.zero;
    float listAddedHeight = 0f;

    void Start() {
        localRectTransform = GetComponent<RectTransform>();
    }

    void Update() {
        AdjustHeight();
    }

    [ContextMenu("Adjust Height")]
    void AdjustHeight() {
        if (!localRectTransform) localRectTransform = GetComponent<RectTransform>();
        if (!localRectTransform) return;
        if (targetRectTransforms.Count < 1) return;

        listAddedHeight = 0f;
        foreach (RectTransform rect in targetRectTransforms) {
            listAddedHeight += rect.sizeDelta.y;
        }

        adjustedSize = localRectTransform.sizeDelta;
        adjustedSize.y = Mathf.Max((listAddedHeight + AdditionalHeight + (SectionSpacing * targetRectTransforms.Count)), minHeight);
        localRectTransform.sizeDelta = adjustedSize;
    }
}
