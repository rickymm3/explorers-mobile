using UnityEngine;
using System.Collections;

public class DontDestroyOnLoad : MonoBehaviour {
	void Start () {
		DontDestroyOnLoad(this);
	}
}
