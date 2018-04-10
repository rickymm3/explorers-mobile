using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadTapBattle : MonoBehaviour {
	void Start () {
        MenuManager.Instance.ClearUI();
        UnityEngine.SceneManagement.SceneManager.LoadScene("BattleRoot", UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }
}
