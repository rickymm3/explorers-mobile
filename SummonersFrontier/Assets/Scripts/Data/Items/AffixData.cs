using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AffixData : BaseData {

    public string Name;
    public AffixType Type;

    bool onUnique = false;
    bool onRare = false;
    bool onMagic = false;

    public int MinLevel;
    public float Frequency;

    public List<ItemType> RestrictedToItems = new List<ItemType>();
    public ElementalTypes RestrictedElement = ElementalTypes.None;
    public SkillType RestrictedSkillType = SkillType.None;
    public ItemModData data;

    public ItemModType ModType {
        get { return data.ModType; }
    }

    public AffixData(string Identity, string Name, AffixType Type, bool onUnique, bool onRare, bool onMagic, int MinLevel, float Frequency, List<ItemType> RestrictedToItems, ElementalTypes RestrictedElement, SkillType RestrictedSkillType, ItemModData data) {
        this.Identity = Identity;
        this.Name = Name;
        this.Type = Type;

        this.onUnique = onUnique;
        this.onRare = onRare;
        this.onMagic = onMagic;
        
        this.MinLevel = MinLevel;
        this.Frequency = Frequency;

        this.RestrictedToItems = RestrictedToItems;
        this.RestrictedElement = RestrictedElement;
        this.RestrictedSkillType = RestrictedSkillType;

        this.data = data;
    }

    public bool ValidQuality(ItemQuality quality) {
        switch (quality) {
            case ItemQuality.Unique:
                if (!onUnique) return false;
                break;
            case ItemQuality.Rare:
                if (!onRare) return false;
                break;
            case ItemQuality.Magic:
                if (!onMagic) return false;
                break;
        }

        return true;
    }

    public float GetModEffect(float ilvl, int seed) {
        // TODO return something that will give us the stat we need to update
        // Based on
        return data.GetModEffect(ilvl, seed); // Include Variance if we need it
    }

    public bool isStatType(ItemModEffects type) {
        if(data==null) {
            Tracer.traceError("AffixData.isStatType's data is null!");
            return false;
        }

        if (type == data.StatType)
            return true;
        return false;
    }

    public ItemModType GetModType() {
        return data.ModType;
    }
    public bool isStat(CoreStats stat) {
        return data.isStat(stat);
    }
    public bool isStat(PrimaryStats stat) {
        return data.isStat(stat);
    }
    public bool isStat(SecondaryStats stat) {
        return data.isStat(stat);
    }

    public override string ToString() {
        return Name + " [" + data.ToString() + "]";
    }
}
