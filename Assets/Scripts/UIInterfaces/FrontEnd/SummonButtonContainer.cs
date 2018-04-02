using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SummonButtonContainer :MonoBehaviour {

    public Button btn;
    public Image icon;
    public Image bg;
    public TextMeshProUGUI label;
    public TextMeshProUGUI counter;

    [HideInInspector] public CurrencyTypes currency;
    [HideInInspector] public SummonType summonType;

    public void UpdateCounter() {
        int amount = currency.GetAmount();
        counter.text = amount.ToString();

        if (amount <= 0) {
            bg.color = icon.color = new Color(0.5f, 0.5f, 0.5f, 1);
        } else {
            bg.color = icon.color = Color.white;
        }

    }
}
