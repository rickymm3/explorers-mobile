using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;


[System.Serializable]
public class ItemData : BaseData {
    
    public string Name;
    public ItemQuality Quality;
	public ItemType Type;
    public EquipmentType EquipType;
	public int Value;
	public string Description;
    public string Sprite;
    public HeroClass? ClassRestriction = null;
    public int Tier;
    public float Multiplier;

    public List<UniqueReference> UniqueItemReference;

    public ItemData(int ID, string Identity, string Name, ItemType Type, EquipmentType EquipType, int Value, string Description, string Sprite, int Tier, float Multiplier, List<UniqueReference> UniqueItemReference, HeroClass? ClassRestriction = null) {
        this.ID = ID;
        this.Name = Name;
        this.Identity = Identity;
        this.Type = Type;
        this.EquipType = EquipType;
        this.Value = Value;
        this.Description = Description;
        this.Sprite = Sprite;
        this.ClassRestriction = ClassRestriction;
        this.Tier = Tier;
        this.Multiplier = Multiplier;

        this.UniqueItemReference = UniqueItemReference;
    }

    public ItemData(ItemData data) {
        this.ID = data.ID;
        this.Name = data.Name;
        this.Identity = data.Identity;
        this.Type = data.Type;
        this.EquipType = data.EquipType;
        this.Value = data.Value;
        this.Description = data.Description;
        this.Sprite = data.Sprite;
        this.ClassRestriction = data.ClassRestriction;
        this.Tier = data.Tier;

        this.UniqueItemReference = data.UniqueItemReference;
    }

    public ItemData Clone() {
        if(this is ItemArmor) {
            return new ItemArmor((ItemArmor) this);
        } else if (this is ItemWeapon) {
            return new ItemWeapon((ItemWeapon) this);
        } else if (this is ItemArtifact) {
            return new ItemArtifact((ItemArtifact) this);
        } else if (this is ItemCurrency) {
            return new ItemCurrency((ItemCurrency) this);
        }

        return new ItemData(this);
    }

    public int StatDifference(CoreStats stat, ItemData data, float ilvl) {
        int results = GetStatValue(stat, ilvl) - data.GetStatValue(stat, ilvl);

        return results;
    }
    public int StatDifference(PrimaryStats stat, ItemData data, float ilvl) {
        int results = GetStatValue(stat, ilvl) - data.GetStatValue(stat, ilvl);

        return results;
    }
    public int StatDifference(SecondaryStats stat, ItemData data, float ilvl) {
        int results = GetStatValue(stat, ilvl) - data.GetStatValue(stat, ilvl);

        return results;
    }

    public virtual int GetStatValue(CoreStats stat, float ilvl) {
        return 0;
    }
    public virtual int GetStatValue(PrimaryStats stat, float ilvl) {
        return 0;
    }
    public virtual int GetStatValue(SecondaryStats stat, float ilvl) {
        return 0;
    }

    public Sprite LoadSprite() {
        return Resources.Load<Sprite>("Items/" + this.EquipType + "/" + this.Sprite);
    }
}
