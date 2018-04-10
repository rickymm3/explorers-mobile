using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScrollRectSnap : MonoBehaviour {
    public enum EnumSelectionMethod { Instant, Move, Fade }

    float[] points;
    [Tooltip("how many screens or pages are there within the content (steps)")]
    public int screens = 1;
    float stepSize;

    ScrollRect scroll;
    bool LerpH;
    float targetH;
    [Tooltip("Snap horizontally")]
    public bool snapInH = true;

    bool LerpV;
    float targetV;
    [Tooltip("Snap vertically")]
    public bool snapInV = true;

    public GameObject dot;
    public List<GameObject> dotPositionList = new List<GameObject>();

    public System.Action<int> onIndexChanged;
    public EnumSelectionMethod selectionMethod = EnumSelectionMethod.Fade;

    // Use this for initialization
    void Start() {
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
        if (LerpH) {
            /*scroll.horizontalNormalizedPosition = Mathf.Lerp(scroll.horizontalNormalizedPosition, targetH, 100 * scroll.elasticity * Time.deltaTime);
            //Debug.Log("HNP: " + scroll.horizontalNormalizedPosition);
            if (Mathf.Approximately(scroll.horizontalNormalizedPosition, targetH)) LerpH = false;
            */

            // Fix for screen switching
            scroll.horizontalNormalizedPosition = targetH;
            LerpH = false;
        }
        if (LerpV) {
            scroll.verticalNormalizedPosition = Mathf.Lerp(scroll.verticalNormalizedPosition, targetV, 100 * scroll.elasticity * Time.deltaTime);
            if (Mathf.Approximately(scroll.verticalNormalizedPosition, targetV)) LerpV = false;
        }
    }

    public void DragEnd() {
        if (scroll.horizontal && snapInH) {
            targetH = points[FindNearest(scroll.horizontalNormalizedPosition, points)];
            LerpH = true;
        }
        if (scroll.vertical && snapInV) {
            targetV = points[FindNearest(scroll.verticalNormalizedPosition, points)];
            LerpV = true;
        }
        UpdateDot();
    }

    public void ForceChange(int index) {
        // This only supports Horizontal ATM
        if (index >= screens) return;

        targetH = index / (float) (screens - 1);
        LerpH = true;

        UpdateDot();
    }

    int lastIndex = 0;
    void UpdateDot() {
        if (dot != null && dotPositionList.Count > 0) {
            int index = Mathf.FloorToInt( (dotPositionList.Count-1) * targetH );
            
            if (lastIndex != index)
                if (onIndexChanged != null) onIndexChanged(index);

            lastIndex = index;

            Vector3 target = dotPositionList[index].transform.position;

            switch (selectionMethod) {
                case EnumSelectionMethod.Move:
                    dot.transform.DOMoveX(target.x, 0f);
                    break;
                case EnumSelectionMethod.Fade:
                    dot.transform.position = target;
                    var g = dot.GetComponent<Graphic>();
                    g.color = new Color(1, 1, 1, 0);
                    g.DOKill();
                    g.DOFade(1, 0.5f);
                    break;
                default:
                    dot.transform.position = target;
                    break;

            }

        }
    }

    public void OnDrag() {
        LerpH = false;
        LerpV = false;
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