using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXBehaviour : Tracer {

    private float timeOffset = 0;
    protected float timeNow = 0;

    // Use this for initialization
    void Start () {
        timeOffset = UnityEngine.Random.Range(0, 10);
    }
	
	// Update is called once per frame
	void Update () {
        timeNow = timeOffset + Time.time;
        UpdateFX();
    }

    public virtual void UpdateFX() {} //Override me
}
