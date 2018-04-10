using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

[Serializable]
public class BoostData : BaseData {
    public string Identity;
    public int numOfBattles;
    public BoostType boostType;
    public float value;
    public string sprite;
    private string _description;

    public BoostData(string Identity, int numOfBattles, BoostType boostType, float value, string sprite, string description) {
        this.Identity = Identity;
        this.numOfBattles = numOfBattles;
        this.boostType = boostType;
        this.value = value;
        this.sprite = sprite;
        this._description = description;
    }

    public string Description {
        get {
            int percent = Mathf.FloorToInt(value * 100);
            return _description.Format2( percent.ToString() );
        }
    }

    public Sprite LoadSprite() {
        return Resources.Load<Sprite>("Boosts/" + this.sprite);
    }
}

public class BoostSlot : MongoData<BoostData> {
    public int count;
    internal int slotID;
}

public static class BoostTypeExtensions {
    public static BoostType ToCurrencyType(this BoostData boostData) {
        return (BoostType) Enum.Parse(typeof(BoostType), boostData.Identity.Replace("boost_", ""), true);
    }
}