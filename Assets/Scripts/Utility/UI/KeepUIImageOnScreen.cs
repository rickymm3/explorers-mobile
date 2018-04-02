using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeepUIImageOnScreen : MonoBehaviour {
    // This script requires a constant width, cannot have a RectTransform that stretches
    // TODO implement height locks as well

    RectTransform reference;
    float offset = 0f, left = 0f, right = 0f, scale = 1f;
    Canvas c;

    void Start() {
        // Get the image
        reference = GetComponent<RectTransform>();
        scale = GameObject.Find("UICanvas").GetComponent<CanvasScaler>().scaleFactor;
        c = GameObject.Find("UICanvas").GetComponent<Canvas>();
    }

	void Update () {
        if (!reference) reference = GetComponent<RectTransform>();
        if (!reference) return;
        if (reference.localScale.magnitude < 0.1f) return;

        reference.position = new Vector3(reference.position.x - offset, reference.position.y, reference.position.z);
        offset = 0f;

        //float width = RectTransformUtility.PixelAdjustRect(reference, c).width;

        // Check to see if the left side of the image is within bounds
        left = reference.position.x - (reference.rect.width * 0.5f * scale);
        if (HorizontalPointOutOfCameraBounds(left)) {
            // The point is out of bounds, adjust the position
            offset = -left;
        } else {
            // The left is good check the right
            right = reference.position.x * scale + (reference.rect.width * 0.5f * scale);
            if (HorizontalPointOutOfCameraBounds(right)) {
                // The point is out of bounds, adjust the position
                offset = Screen.width - right;
            }
        }

        // Move the object if there is an offset
        if (offset != 0f)
            reference.position += new Vector3(offset, 0f, 0f);

        // TODO Fix the weird descrepancy with the width not being screen space
        /*if (reference.localScale.magnitude > 0.5f)
            Debug.Log(" <<< [R: " + right + "][L: " + left + "][S: " + Screen.width + "][Offset: " + offset + "][M: " + Input.mousePosition +
                "]\n[SDx: " + reference.sizeDelta.x + "][W: " + reference.rect.width + "][NW: " + width + "][P: " + reference.position.x +
                "][SF: " + GameObject.Find("UICanvas").GetComponent<CanvasScaler>().scaleFactor + "]");*/
    }

    bool HorizontalPointOutOfCameraBounds(float point) {
        if (point < 0f || point > Screen.width)
            return true;
        return false;
    }
}
