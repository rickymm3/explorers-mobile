using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollRectAlternateBehaviour : MonoBehaviour {

    public CampInterface campInterface;
    
    bool swiping = false;
    Vector3 startTouch;
    float timer = 0f;

    public void OnEndDrag() {
        Vector3 SwipeDirection = startTouch - Input.mousePosition;
        if ((Mathf.Abs(SwipeDirection.x) > Mathf.Abs(SwipeDirection.y)) && swiping) {
            if (SwipeDirection.x > (Screen.width * 0.25f)) {
                campInterface.ChangePanelIndex(campInterface.panelIndex + 1);
            } else if (Mathf.Abs(SwipeDirection.x) > (Screen.width * 0.3f)) {
                campInterface.ChangePanelIndex(campInterface.panelIndex - 1);
            }
        }
        swiping = false;
    }

    void Update() {
        timer += Time.deltaTime;

        if (timer > 0.7f)
            swiping = false;
    }

    public void OnBeginDrag() {
        startTouch = Input.mousePosition;
        timer = 0f;
        swiping = true;
    }
}
