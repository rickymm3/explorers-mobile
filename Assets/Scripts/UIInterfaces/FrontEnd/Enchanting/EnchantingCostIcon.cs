using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnchantingCostIcon : MonoBehaviour {

    public TextMeshProUGUI PriceLabel;
    public Image icon;

    public void Init(string cost, Sprite icon) {
        PriceLabel.text = cost;
        this.icon.sprite = icon;
    }

    public void SetColor(Color color) {
        PriceLabel.color = color;
        icon.color = color;
    }
    public void ShowNoCost() {
        PriceLabel.text = "-/-";
    }
}
