using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ZoneMovementInterface : MonoBehaviour {
    public ExploreInterface explore;
    public RectTransform container;
    public int panels = 0;
    float target = 0f;
    
    public void NextZone() {
        //Debug.Log(target);
        if (target <= -(900f * (panels-1))) return;
        //Debug.Log("button hit");
        target -= 900f;
        MovePanel(target, container);

        explore.ZoneIndex++;
    }
    public void PreviousZone() {
        //Debug.Log(target);
        if (target >= 0f) return;
        //Debug.Log("button hit");
        target += 900f;
        MovePanel(target, container);

        explore.ZoneIndex--;
    }
    public void CancelHit() {
        explore.ZoneIndex=1;
        target = 0f;
        MovePanel(0f, container, 0f);
    }


    void MovePanel(float xPosition, RectTransform rectTrans, float duration = 1f) {
        DOTween.To(() => rectTrans.anchoredPosition, x => rectTrans.anchoredPosition = x, new Vector2(xPosition, rectTrans.anchoredPosition.y), duration);
    }
}
