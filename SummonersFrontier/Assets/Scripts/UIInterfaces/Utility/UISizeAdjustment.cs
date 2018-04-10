using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum UIScaleType { Height, Width }
public class UISizeAdjustment : MonoBehaviour {
    // Warning this wont work yet
    public UIScaleType type = UIScaleType.Height;
    [Range(0f, 1f)] public float percent = 1f;

    float adjustedSize = 0f;

    RectTransform rectTrans;
    CanvasScaler scaler;

    void Start() {
        rectTrans = GetComponent<RectTransform>();
        scaler = GetComponentInParent<CanvasScaler>();
    }

	void Update () {
        //if (Input.GetKeyDown(KeyCode.A))
        UpdateUI();
    }

    void UpdateUI() {
        float resolutionAdjustment = 0f;
        switch (type) {
            case UIScaleType.Height:
                // TODO get the calculation correct with teh Canvas resizer
                resolutionAdjustment = Screen.height / scaler.referenceResolution.y;

                adjustedSize = scaler.referenceResolution.y * resolutionAdjustment * percent;

                rectTrans.sizeDelta = new Vector2(rectTrans.sizeDelta.x, adjustedSize);
                break;
            case UIScaleType.Width:
                // TODO get the calculation correct with teh Canvas resizer
                resolutionAdjustment = Screen.width / scaler.referenceResolution.x;

                adjustedSize = Screen.width * resolutionAdjustment * percent;

                rectTrans.sizeDelta = new Vector2(adjustedSize, rectTrans.sizeDelta.y);
                break;
        }
    }
}
