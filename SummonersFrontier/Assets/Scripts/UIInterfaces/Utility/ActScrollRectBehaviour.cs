using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class ActScrollRectBehaviour : MonoBehaviour {
    float[] points;
    int screens = 0;
    float stepSize;

    int screenIndex = 0;
    float positionStart = 0f;
    
    ScrollRect scroll;
    float targetH;

    public System.Action<int> onActSelect;

    // Use this for initialization
    void Start() {
        if (!DataManager.Instance.isLoaded) return;
        
        screens = DataManager.Instance.actDataList.Count;

        scroll = gameObject.GetComponent<ScrollRect>();
        scroll.inertia = false;

        if (screens > 0) {
            points = new float[screens];
            stepSize = 1 / (float) (screens - 1);

            for (int i = 0; i < screens; i++) {
                points[i] = i * stepSize;
            }
        } else {
            points[0] = 0;
        }
    }

    void Update() {
        if (scroll==null) return;
        
        scroll.horizontalNormalizedPosition = Mathf.Lerp(scroll.horizontalNormalizedPosition, targetH, 50 * scroll.elasticity * Time.deltaTime);
    }

    public void DragEnd() {
        targetH = points[FindNearest(scroll.horizontalNormalizedPosition, points)];
        
        if ((Input.mousePosition.x - positionStart) > 10f) {
            screenIndex--;
            if (screenIndex < 0) screenIndex = 0;
        } else if ((Input.mousePosition.x - positionStart) < -10f) {
            screenIndex++;
            if (screenIndex >= screens) screenIndex = screens - 1;
        } else if (Mathf.Abs(Input.mousePosition.x - positionStart) <= 1f) {
            //print("[ActScrollSnap] Select Act " + (screenIndex + 1));
            if (onActSelect != null) onActSelect(screenIndex + 1);
        }
        
        targetH = points[screenIndex];
    }
    
    public void OnDrag() {
        positionStart = Input.mousePosition.x;
    }

    int FindNearest(float f, float[] array) {
        float distance = Mathf.Infinity;
        int output = 0;
        for (int index = 0; index < array.Length; index++) {
            if (Mathf.Abs(array[index] - f) < distance) {
                distance = Mathf.Abs(array[index] - f);
                output = index;
            }
        }
        return output;
    }
}
