using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WavyPosition : FXBehaviour {

    public float speedChase = 3f;
    public float waveSpeedX = 5f;
    public float waveSpeedY = 7f;
    public float waveScaleX = 10f;
    public float waveScaleY = 10f;

    private Vector2 _wavyOffset = Vector2.zero;
    
    private Vector2 _targetPosition = Vector2.zero;
    public Vector2 targetPosition {
        get { return _targetPosition; }
        set { _targetPosition = value; }
    }
	
	// Update is called once per frame
	public override void UpdateFX() {
        base.UpdateFX();
        _wavyOffset.x = Mathf.Cos(timeNow * waveSpeedX) * waveScaleX;
        _wavyOffset.y = Mathf.Sin(timeNow * waveSpeedY) * waveScaleY;

        Vector2 target = _wavyOffset + _targetPosition;
        Vector2 lerp = Vector2.Lerp(transform.localPosition, target, speedChase * Time.deltaTime);
		this.transform.localPosition = lerp;
    }
}
