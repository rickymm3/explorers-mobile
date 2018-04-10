using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class VineMovement : FXBehaviour {
    bool _isShow = false;
    bool _isTweening = false;

    public float wobbleAngleStart = 0;
    public float wobbleAngleRange = 10f;
    public float wobbleSpeed = 10f;
    public float stretchSpeedX = 5f;
    public float stretchSpeedY = 7f;
    public float stretchAmountX = 10f;
    public float stretchAmountY = 10f;
    private float _timeToShow = 0.3f;
    private float _timeToHide = 0.2f;

    private float _wobbleAngle = 0;
    private Vector2 _wobbleScale = Vector2.one;

    void Start() {
        transform.localScale = Vector2.zero;
    }

    // Update is called once per frame
    public override void UpdateFX() {
        if(!_isShow || _isTweening) return;
        base.UpdateFX();

        float slowDown = 0.1f;
        float stretchFactor = 0.01f;

        _wobbleAngle = wobbleAngleStart + Mathf.Sin(timeNow * wobbleSpeed * slowDown) * wobbleAngleRange;

        _wobbleScale.x = 1 + Mathf.Cos(timeNow * stretchSpeedX * slowDown) * stretchAmountX * stretchFactor;
        _wobbleScale.y = 1 + Mathf.Sin(timeNow * stretchSpeedY * slowDown) * stretchAmountY * stretchFactor;

        this.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, _wobbleAngle));
        this.transform.localScale = _wobbleScale;
    }

    [ContextMenu("Show Vines...")]
    public void Show() {
        _isShow = true;
        _isTweening = true;
        this.transform.DOKill();
        this.transform.DOScale(1.0f, _timeToShow)
            .SetEase(Ease.OutBack)
            .OnComplete(() => _isTweening = false);
    }

    [ContextMenu("Hide Vines...")]
    public void Hide() {
        _isShow = false;
        _isTweening = true;
        this.transform.DOKill();
        this.transform.DOScale(0f, _timeToHide)
            .SetEase(Ease.InBack)
            .OnComplete(() => _isTweening = false);
    }
}
