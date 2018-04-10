using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ExtensionMethods;

public class PremiumShopItemButton : MonoBehaviour {
    public TextMeshProUGUI label;
    public TextMeshProUGUI cost;

    public string currency;
    public CurrencyTypes type = CurrencyTypes.SCROLLS_IDENTIFY;
    public int value = 25;

    void Start() {
        label.text = currency;
        cost.text = value.ToString();
    }
    
    public void BtnPurchase() {
        ConfirmYesNoInterface.Ask("Confirm Purchase", "Purchase\n" + currency + "\nfor <color=#ff5555>" + value + " Gems</color>?")
            .Then(answer => {
                if (answer != "YES") return;
                AudioManager.Instance.Play(SFX_UI.ShardsChing);
                
                CurrencyManager.Cost cost = CurrencyTypes.GEMS.ToCostObj(-value);

                DataManager.API.Currency.AddCurrency(cost)
                    .Then(res => {
                        DataManager.API.Currency.AddCurrency(type, 1);
                    })
                    .Catch(err => { Debug.LogError("Could not spend Shards in PowerChange: " + err); });
            });
    }
}
