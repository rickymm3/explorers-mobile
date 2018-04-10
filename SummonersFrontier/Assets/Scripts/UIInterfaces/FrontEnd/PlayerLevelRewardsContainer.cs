using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerLevelRewardsContainer : MonoBehaviour {

    public TextMeshProUGUI title;
    public TextMeshProUGUI amount;
    public Image icon;

    public void Initialize(PlayerLevelReward reward) {
        title.text = reward.Name;
        amount.transform.parent.gameObject.SetActive(reward.ShowAmount);
        amount.text = reward.Amount.ToString();

        icon.sprite = reward.sprite;
	}
}
