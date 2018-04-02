using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ActorSelector : MonoBehaviour {

    public float StaticZ = 1.5f;

    public void Hide() {
        transform.DOScale(0f, 1f);
    }
    
    public void GoToTarget(Vector3 Position) {
        Position.z = StaticZ;
        transform.DOScaleX(1f, 0.15f);
        transform.DOScaleY(0.75f, 0.15f);
        transform.DOScaleZ(1f, 0.15f);
        transform.DOMove(Position, 0.15f);
    }
}
