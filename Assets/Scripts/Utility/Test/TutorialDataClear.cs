using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialDataClear : MonoBehaviour {

    public int CurrentState = 0;
    public int TutorialState = 0;

    void Update() {
        if (PlayerPrefs.HasKey("tutorial_step")) CurrentState = PlayerPrefs.GetInt("tutorial_step");
    }

    [ContextMenu("Set Tutorial Stage")]
    void SetTutorialState() {
        PlayerPrefs.SetInt("tutorial_step", TutorialState);
    }

    [ContextMenu("Clear Tutorial Data")]
    void ClearTutorialData() {
        PlayerPrefs.DeleteKey("tutorial_step");
        PlayerPrefs.DeleteKey("ftue_intro_story");
        PlayerPrefs.DeleteKey("tutorial_hero_upgrades");
        PlayerPrefs.DeleteKey("tutorial_shop");
        PlayerPrefs.DeleteKey("tutorial_boss_battle");
        PlayerPrefs.DeleteKey("tutorial_tap_battle");
        PlayerPrefs.DeleteKey("tutorial_boss_battle_castable_skill");

        Debug.Log("[Tutorial Data Cleared]");
    }
}
