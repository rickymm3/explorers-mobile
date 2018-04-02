using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemAffix {
    public AffixData data;
    
    public ItemAffix(AffixData data) {
        this.data = data;
    }

    public float GetValue(CoreStats stat, float ilvl, int seed) {
        if (!ContainsStat(stat)) return 0;
        return (data.GetModEffect(ilvl, seed));
    }
    public float GetValue(PrimaryStats stat, float ilvl, int seed) {
        if (!ContainsStat(stat)) return 0;
        return (data.GetModEffect(ilvl, seed));
    }
    public float GetValue(SecondaryStats stat, float ilvl, int seed) {
        if (!ContainsStat(stat)) return 0;
        return (data.GetModEffect(ilvl, seed));
    }

    public string GetDisplayValue(CoreStats stat, float ilvl, int seed) {
        if (!ContainsStat(stat)) return "";
        string result = "";

        switch (data.ModType) {
            case ItemModType.Add:
                result = "+" + Mathf.RoundToInt(data.GetModEffect(ilvl, seed)) + " " + stat.ToString();
                break;
            case ItemModType.Multi:
            default:
                Debug.Log("[Display Stat] " + stat + ": " + (data.GetModEffect(ilvl, seed)));
                result = "+" + Mathf.RoundToInt(data.GetModEffect(ilvl, seed) * 100f) + "% " + stat.ToString();
                break;
        }
        return result;
    }
    public string GetDisplayValue(PrimaryStats stat, float ilvl, int seed) {
        if (!ContainsStat(stat)) return "";
        string result = "";

        switch (data.ModType) {
            case ItemModType.Add:
                result = "+" + Mathf.RoundToInt(data.GetModEffect(ilvl, seed)) + " " + stat.ToString();
                break;
            case ItemModType.Multi:
            default:
                Debug.Log("[Display Stat] " + stat + ": " + (data.GetModEffect(ilvl, seed)));
                result = "+" + Mathf.RoundToInt(data.GetModEffect(ilvl, seed) * 100f) + "% " + stat.ToString();
                break;
        }
        return result;
    }
    public string GetDisplayValue(SecondaryStats stat, float ilvl, int seed) {
        if (!ContainsStat(stat)) return "";
        string result = "";

        switch (data.ModType) {
            case ItemModType.Add:
                result = "+" + Mathf.RoundToInt(data.GetModEffect(ilvl, seed)) + " " + stat.ToString();
                break;
            case ItemModType.Multi:
                Debug.Log("[Display Stat] " + stat + ": " + (data.GetModEffect(ilvl, seed)));
                result = "+" + Mathf.RoundToInt(data.GetModEffect(ilvl, seed) * 100f) + "% " + stat.ToString();
                break;
            case ItemModType.Status:
            default:
                result = "TODO fill this out";
                break;
        }
        return result;
    }

    public List<EquipmentType> RestrictedEquipmentType = new List<EquipmentType>();
    public List<WeaponType> RestrictedWeaponType = new List<WeaponType>();


    public bool isStatType(ItemModEffects type) {
        return data.isStatType(type);
    }
    public ItemModType GetModType() {
        return data.GetModType();
    }

    public bool ContainsStatus() {
        if (GetModType() == ItemModType.Status) return true;
        return false;
    }

    public bool ContainsStat(CoreStats stat) {
        if (GetModType() == ItemModType.Status) return false;
        return (data.isStatType(ItemModEffects.Core) && data.isStat(stat));
    }
    public bool ContainsStat(PrimaryStats stat) {
        if (GetModType() == ItemModType.Status) return false;
        return (data.isStatType(ItemModEffects.Primary) && data.isStat(stat));
    }
    public bool ContainsStat(SecondaryStats stat) {
        if (GetModType() == ItemModType.Status) return false;
        return (data.isStatType(ItemModEffects.Secondary) && data.isStat(stat));
    }

    public override string ToString() {
        return data.ToString();
    }
}