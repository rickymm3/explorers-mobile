using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ActNavigation : MonoBehaviour {
    
    public ActSelectInterface selectInterface;
    public RectTransform container;
    public int panels = 0;
    float target = 0f;

    void Update() {

    }

    [ContextMenu("next")]
    public void Next() {
        //Debug.Log(target);
        if (target <= -(900f * (panels - 1))) return;
        //Debug.Log("button hit");
        target -= 900f;
        MovePanel(target, container);

        selectInterface.ActIndex++;
    }
    [ContextMenu("prev")]
    public void Previous() {
        //Debug.Log(target);
        if (target >= 0f) return;
        //Debug.Log("button hit");
        target += 900f;
        MovePanel(target, container);

        selectInterface.ActIndex--;
    }
    public void CancelHit() {
        selectInterface.ActIndex = 1;
        target = 0f;
        MovePanel(0f, container, 0f);
    }


    void MovePanel(float xPosition, RectTransform rectTrans, float duration = 1f) {
        DOTween.To(() => rectTrans.anchoredPosition, x => rectTrans.anchoredPosition = x, new Vector2(xPosition, rectTrans.anchoredPosition.y), duration);
    }
}
