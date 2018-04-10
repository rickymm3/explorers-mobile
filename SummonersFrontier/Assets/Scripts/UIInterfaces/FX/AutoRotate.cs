using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotate : MonoBehaviour {
    public enum AutoRotateAxis { X, Y, Z }

    public float rotateSpeed = 100f;
    public AutoRotateAxis axis = AutoRotateAxis.Z;
	
	void Update () {
        float angle = rotateSpeed * Time.deltaTime;

        switch(axis) {
            case AutoRotateAxis.X: this.transform.Rotate(new Vector3(angle, 0, 0)); break;
            case AutoRotateAxis.Y: this.transform.Rotate(new Vector3(0, angle, 0)); break;
            case AutoRotateAxis.Z: this.transform.Rotate(new Vector3(0, 0, angle)); break;
        }

    }
}
