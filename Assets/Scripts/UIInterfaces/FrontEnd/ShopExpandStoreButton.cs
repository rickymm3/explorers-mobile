using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ExtensionMethods;

public class ShopExpandStoreButton : MonoBehaviour {

    public ShopInterface shopInterface;
    public TextMeshProUGUI costTxt;
    public TextMeshProUGUI labelTxt;
    int cost;

    void Start() {
        if(!DataManager.Instance.isLoaded) return;
        
        cost = 25 + (PlayerManager.Instance.ShopExpansion * 5);
        costTxt.text = cost.ToString();

        UpdateExpandText();
    }

    private void UpdateExpandText() {
        int remaining = GlobalProps.SHOP_EXPANSION_LIMIT.GetInt() - PlayerManager.Instance.ShopExpansion;
        labelTxt.text = "EXPAND STORE\n<align=\"center\"><size=25><color=#999>({0} remaining)</color></size></align>".Format2(remaining);

        if (PlayerManager.Instance.ShopExpansion >= GlobalProps.SHOP_EXPANSION_LIMIT.GetInt())
            gameObject.SetActive(false);
    }

    public void Btn_ExpandStore() {
        if (CurrencyTypes.GEMS.GetAmount() >= cost) {
            GameAPIManager.Instance.Shop.SetExpansionSlots(PlayerManager.Instance.ShopExpansion + 1, CurrencyTypes.GEMS, cost)
                .Then(res => {
                    Tracer.trace("PlayerManager.Instance.ShopExpansion: " + PlayerManager.Instance.ShopExpansion);
                    cost = 30 + (PlayerManager.Instance.ShopExpansion * 5);
                    costTxt.text = cost.ToString();

                    AudioManager.Instance.Play<SFX_UI>(SFX_UI.Coin);

                    UpdateExpandText();
                    shopInterface.RefreshStoreList();
                });
        } else {
            AudioManager.Instance.Play<SFX_UI>(SFX_UI.Invalid);
            TimelineTween.ShakeError(gameObject);
            Debug.Log("Pop our 'Get more currency by watching a video' interface here because the player does not have enough.");
        }
    }
}
