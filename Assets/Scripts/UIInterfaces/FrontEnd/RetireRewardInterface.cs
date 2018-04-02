using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RetireRewardInterface : MonoBehaviour {

    public TextMeshProUGUI Title;
    public TextMeshProUGUI Description;
    public TextMeshProUGUI Count;
    public Image Icon;

    public void Init(string identity, int amount) {
        Debug.Log("   [Retire] Giving the player " + amount + " " + identity);
        switch(identity) {
            case "low_essence":
                Title.text = "Low Essence";
                Description.text = "A low quality component used to power up heroes.";
                GameAPIManager.Instance.Currency.AddCurrency(CurrencyTypes.ESSENCE_LOW, amount);
                break;
            case "medium_essence":
                Title.text = "Medium Essence";
                Description.text = "A medium quality component used to power up heroes.";
                GameAPIManager.Instance.Currency.AddCurrency(CurrencyTypes.ESSENCE_MID, amount);
                break;
            case "high_essence":
                Title.text = "High Essence";
                Description.text = "A high quality component used to power up heroes.";
                GameAPIManager.Instance.Currency.AddCurrency(CurrencyTypes.ESSENCE_HIGH, amount);
                break;
            case "rare_scroll":
                Title.text = "Rare Scroll";
                Description.text = "A rare scroll for summoning new heroes.";
                GameAPIManager.Instance.Currency.AddCurrency(CurrencyTypes.SCROLLS_SUMMON_RARE, amount);
                break;
            case "hero_scroll":
                Title.text = "Common Scroll";
                Description.text = "A common scroll for summoning new heroes.";
                GameAPIManager.Instance.Currency.AddCurrency(CurrencyTypes.SCROLLS_SUMMON_COMMON, amount);
                break;
            case "experience_shard":
                Title.text = "Experience Shard";
                Description.text = "A shard of experience from past heroes.";
                GameAPIManager.Instance.Currency.AddCurrency(CurrencyTypes.XP_FRAGMENT, amount);
                break;
        }

        if (amount > 0)
            Count.text = "x" + amount;
        else
            Count.text = "";

        Icon.sprite = Resources.Load<Sprite>("Items/Essences/" + identity);
    }
}
