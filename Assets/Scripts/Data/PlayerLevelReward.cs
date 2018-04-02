using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLevelReward : BaseData {
    public string Name;
    public PlayerLevelRewardType Type;
    public CurrencyTypes CurrencyType;
    public int Amount;
    public bool ShowAmount;
    public Sprite sprite;

    public PlayerLevelReward(int id, string identity, string name, PlayerLevelRewardType type, CurrencyTypes cType, int amount, bool showAmount, string sprite) {
        ID = id;
        Identity = identity;

        Name = name;
        Type = type;
        CurrencyType = cType;
        Amount = amount;
        ShowAmount = showAmount;
        this.sprite = Resources.Load<Sprite>("Items/LevelRewards/" + sprite);
    }
}
