using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnOrderHeroController : MonoBehaviour {

    public Image Border;
    public Image BorderBkg;
    public Image Portrait;
    public TextMeshProUGUI Label;

    public void Initialize(Sprite sprite, bool isMonster = false) {
        Portrait.sprite = sprite;

        if (isMonster) {
            Border.color = new Color(0.7f, 0f, 0f, 1f);
            BorderBkg.color = new Color(0.7f, 0f, 0f, 1f);
        } else {
            Border.color = Color.white;
            BorderBkg.color = Color.white;
        }
    }

    public void Casting() {
        Label.gameObject.SetActive(true);
    }
    public void NotCasting() {
        Label.gameObject.SetActive(false);
    }
}
