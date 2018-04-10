using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FixScrollSensitivity :Tracer {

    ScrollRect scrollRect;

    // Use this for initialization
    void Start () {
        scrollRect = GetComponent<ScrollRect>();
        if(scrollRect==null) return;

        if (Application.platform == RuntimePlatform.WindowsEditor) {
            scrollRect.scrollSensitivity = 100;
        }
    }
}
