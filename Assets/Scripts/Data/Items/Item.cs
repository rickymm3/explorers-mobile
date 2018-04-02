using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExtensionMethods;

[System.Serializable]
public class Item : MongoData<ItemData> {
    //Only assignable from the Constructor:
    int _QualitySeed = 0;
    int _AffixSeed = 0;
    int _ItemLevelSeed = 0;
    int _VarianceSeed = 0;
    float _MagicFind = 0f;
    float _baseItemLevel = 0f;
    float _variance = 0f;

    public string stackTrace;

    //Read-Only:
    public int QualitySeed { get { return _QualitySeed; } }
    public int AffixSeed { get { return _AffixSeed; } }
    public int ItemLevelSeed { get { return _ItemLevelSeed; } }
    public int VarianceSeed { get { return _VarianceSeed; } }
    public float MagicFind { get { return _MagicFind; } }
    public float BaseItemLevel { get { return _baseItemLevel; } }
    public float Variance { get { return _variance; } }

    public bool isIdentified = false;
    public bool isResearched = false;
    public bool isUseable { get { return !isIdentified && !isResearched; } }

    public int heroID = -1;

    public ItemQuality Quality = ItemQuality.Common;
    public float ItemLevel = 0f;
    public List<ItemAffix> Affixes = new List<ItemAffix>();
    int UniqueItemIndex = 0;

    public ItemType Type {
        get { return data.Type; }
    }

    public int ConvertToCommonShards() {
        if (Quality == ItemQuality.Common)
            return Mathf.CeilToInt(ItemLevel * 0.3f);

        return 0;
    }
    public int ConvertToMagicShards() {
        if (Quality == ItemQuality.Magic)
            return Mathf.CeilToInt(ItemLevel * 0.1f);

        return 0;
    }
    public int ConvertToRareShards() {
        if (Quality == ItemQuality.Rare)
            return Mathf.CeilToInt(ItemLevel * 0.05f);

        return 0;
    }
    public int ConvertToUniqueShards() {
        if (Quality == ItemQuality.Unique)
            return 1;

        return 0;
    }
    public CurrencyTypes GetDisenchantType() {
        switch (Quality) {
            case ItemQuality.Magic:
                return CurrencyTypes.SHARDS_ITEMS_MAGIC;
            case ItemQuality.Rare:
                if (Random.Range(0f, 1f) < 0.6f)
                    return CurrencyTypes.SHARDS_ITEMS_RARE;
                return CurrencyTypes.SHARDS_ITEMS_MAGIC;
            case ItemQuality.Unique:
                return CurrencyTypes.SHARDS_ITEMS_RARE;
            default:
            case ItemQuality.Common:
                return CurrencyTypes.SHARDS_ITEMS_COMMON;
        }
    }

    public int GetDisenchantValue(CurrencyTypes type = CurrencyTypes.SHARDS_ITEMS_COMMON) {

        switch (Quality) {
            case ItemQuality.Magic:
                return Random.Range(1, 3);
            case ItemQuality.Rare:
                if (type == CurrencyTypes.SHARDS_ITEMS_RARE)
                    return Random.Range(1, 3);
                else
                    return Random.Range(3, 7);
            case ItemQuality.Unique:
                return Random.Range(3, 7);
            default:
            case ItemQuality.Common:
                return Mathf.RoundToInt(Random.Range(1.5f, 7.5f) * Random.Range(1.5f, 7.5f));
        }
    }
    
    public Item(ItemData data, int QualitySeed, int AffixSeed, int ItemLevelSeed, int VarianceSeed, float MagicFind, float ilvl, float variance) {
        this.data = data;
        _QualitySeed = QualitySeed;
        _AffixSeed = AffixSeed;
        _ItemLevelSeed = ItemLevelSeed;
        _VarianceSeed = VarianceSeed;
        _MagicFind = MagicFind;
        _baseItemLevel = ilvl;
        _variance = variance;

        isIdentified = true;

#if UNITY_EDITOR
        stackTrace = System.Environment.StackTrace;
#endif
        
        // Get the Item level range of the base item
        Random.InitState(_ItemLevelSeed);
        this.ItemLevel = ilvl + (variance * 0.5f * Mathf.Pow(Random.Range(0f, 1f), 2f)) + ((0.4f * ((float) Quality)) * variance);
        //if (this.ItemLevel < 20f) Debug.LogError("Item level should never be this low!");
        
        Random.InitState(_QualitySeed);

        // TODO randomly apply prefixes at a decreasing rate
        if (data is ItemCurrency) {
            isIdentified = true;
        } else {
            if (_QualitySeed == 0) {
                this.ItemLevel = (float) this.data.Tier * 20f;
            } else {
                if (UpgradeItem(ItemQuality.Unique, MagicFind) && data.UniqueItemReference.Count > 0) {
                    Quality = ItemQuality.Unique;
                    isIdentified = false;

                    UniqueItemIndex = Random.Range(0, data.UniqueItemReference.Count);
                    Affixes.AddRange(data.UniqueItemReference[UniqueItemIndex].AffixReferences);

                    Random.InitState(_AffixSeed);
                    for (int i = data.UniqueItemReference[UniqueItemIndex].AffixReferences.Count; i < Random.Range(5, 7);) {
                        AffixData affix = GetQualifiedAffix(ItemQuality.Rare, data);
                        if (affix != null) {
                            Affixes.Add(new ItemAffix(affix));
                            i++;
                        }
                    }

                    // Legendaries/Uniques will be seperate from the other calculations after, probably get the type and pull a Unique from a seperate list based on that
                } else if (UpgradeItem(ItemQuality.Rare, MagicFind)) {
                    Quality = ItemQuality.Rare;
                    isIdentified = false;

                    Random.InitState(_AffixSeed);
                    for (int i = 0; i < Random.Range(3, 6);) {
                        AffixData affix = GetQualifiedAffix(ItemQuality.Rare, data);
                        if (affix != null) {
                            Affixes.Add(new ItemAffix(affix));
                            i++;
                        }
                    }
                } else if (UpgradeItem(ItemQuality.Magic, MagicFind)) {
                    Quality = ItemQuality.Magic;
                    isIdentified = false;

                    Random.InitState(_AffixSeed);
                    for (int i = 0; i < Random.Range(2, 4);) {
                        AffixData affix = GetQualifiedAffix(ItemQuality.Rare, data);
                        if (affix != null) {
                            Affixes.Add(new ItemAffix(affix));
                            i++;
                        }
                    }
                } else {
                    Quality = ItemQuality.Common;

                    Random.InitState(_AffixSeed);
                    if (Random.Range(0f, 1f) < 0.25f) {
                        AffixData affix = GetQualifiedAffix(ItemQuality.Rare, data);
                        if (affix != null) Affixes.Add(new ItemAffix(affix));
                    }
                }
            }
        }
    }

    AffixData GetQualifiedAffix(ItemQuality quality, ItemData data) {
        List<AffixData> QualifiedAffixes = DataManager.Instance.affixDataList.FindAll(a => a.MinLevel <= this.ItemLevel && a.RestrictedToItems.Contains(data.Type) && a.ValidQuality(quality));
        Dictionary<AffixData, float> AffixChances = new Dictionary<AffixData, float>();

        //if (QualifiedAffixes.Count == 0) return null;

        foreach (AffixData aff in QualifiedAffixes) {
            AffixChances.Add(aff, aff.Frequency);
        }
        
        return MathHelper.WeightedRandom(AffixChances).Key; //QualifiedAffixes[Random.Range(0, QualifiedAffixes.Count)];
    }

    public void RerollAffixes() {
        Affixes.Clear();

        _AffixSeed = GameManager.Instance.GetSeed();
        Random.InitState(_AffixSeed);

        switch (Quality) {
            case ItemQuality.Common:
                break;
            case ItemQuality.Magic:
                for (int i = 0; i < Random.Range(1, 4); i++) {
                    Affixes.Add(new ItemAffix(DataManager.Instance.affixDataList.GetRandom()));
                }
                break;
            case ItemQuality.Rare:
                for (int i = 0; i < Random.Range(3, 6); i++) {
                    Affixes.Add(new ItemAffix(DataManager.Instance.affixDataList.GetRandom()));
                }
                break;
            case ItemQuality.Unique:
                for (int i = 0; i < Random.Range(4, 7); i++) {
                    Affixes.Add(new ItemAffix(DataManager.Instance.affixDataList.GetRandom()));
                }
                break;
        }
    }

    public void RerollItemLevel() {
        _ItemLevelSeed = GameManager.Instance.GetSeed();
        Random.InitState(_ItemLevelSeed);
        this.ItemLevel = _baseItemLevel + (_variance * 0.5f * Mathf.Pow(Random.Range(0f, 1f), 2f)) + ((0.4f * ((float) Quality)) * _variance);
    }

    public void RerollVarianceLevel() {
        _VarianceSeed = GameManager.Instance.GetSeed();
    }

    bool UpgradeItem(ItemQuality Quality, float MagicFind) {
        float AdjustedMagicFind = 0f;

        // TODO change the weights for odds based on crate type

        switch (Quality) {
            case ItemQuality.Unique:
                AdjustedMagicFind = 1f + MagicFind;
                return WeightedRandom(AdjustedMagicFind);
            case ItemQuality.Rare:
                AdjustedMagicFind = 1f + MagicFind;
                return WeightedRandom(3f * AdjustedMagicFind);
            case ItemQuality.Magic:
                AdjustedMagicFind = 1f + MagicFind;
                return WeightedRandom(7f * AdjustedMagicFind);
            case ItemQuality.Common:
            default:
                AdjustedMagicFind = 1f + MagicFind;
                return WeightedRandom(12f * AdjustedMagicFind); // This makes 825% MF the 99% chance cap, diminishing returns will come in handy
        }
    }

    bool WeightedRandom(float chance) {
        // TODO Change this to a better random function, this makes different levels of MF useless compared to lower values
        if (chance > 90f)
            chance = 90f;
        
        if (Random.Range(0f, 100f) < chance)
            return true;
        else
            return false;
    }


    public int GetStatDifference(CoreStats stat, Item data, Hero hero) {
        // Get the base hero stats without the item
        // Get the (original % effect) then remove the item stats after the multi
        float heroBaseStat = (hero.GetCoreStat(stat) / hero.GetStatMultiplier(stat)) - GetStats(stat);
        float baseMulti = hero.GetStatMultiplier(stat) - GetMultiplier(stat);

        // Get the hero stats with the new item
        // Add the  item stats before the multi then scale by the % effect, finish by subtracting the original value to get the difference
        float diffValue = ((heroBaseStat + data.GetStats(stat)) * (baseMulti + data.GetMultiplier(stat))) - hero.GetCoreStat(stat);

        return Mathf.RoundToInt(diffValue);
        // return data.GetStats(stat) - GetStats(stat);
    }
    public int GetStatDifference(PrimaryStats stat, Item data, Hero hero) {
        // Get the base hero stats without the item
        // Get the (original % effect) then remove the item stats after the multi
        float heroBaseStat = (hero.GetPrimaryStat(stat) / hero.GetStatMultiplier(stat)) - GetStats(stat);
        float baseMulti = hero.GetStatMultiplier(stat) - GetMultiplier(stat);

        // Get the hero stats with the new item
        // Add the  item stats before the multi then scale by the % effect, finish by subtracting the original value to get the difference
        float diffValue = ((heroBaseStat + data.GetStats(stat)) * (baseMulti + data.GetMultiplier(stat))) - hero.GetPrimaryStat(stat);
        //Debug.Log("[Item Calc] " + stat + " multi: " + data.GetMultiplier(stat));

        return Mathf.RoundToInt(diffValue);
        //return data.GetStats(stat) - GetStats(stat);
    }
    public int GetStatDifference(SecondaryStats stat, Item data, Hero hero) {
        // Get the base hero stats without the item
        // Get the (original % effect) then remove the item stats after the multi
        float heroBaseStat = (hero.GetSecondaryStat(stat) / hero.GetStatMultiplier(stat)) - GetStats(stat);
        float baseMulti = hero.GetStatMultiplier(stat) - GetMultiplier(stat);

        // Get the hero stats with the new item
        // Add the  item stats before the multi then scale by the % effect, finish by subtracting the original value to get the difference
        float diffValue = ((heroBaseStat + data.GetStats(stat)) * (baseMulti + data.GetMultiplier(stat))) - hero.GetSecondaryStat(stat);

        return Mathf.RoundToInt(diffValue);
        //return data.GetStats(stat) - GetStats(stat);
    }

    
    public int GetStats(CoreStats stat) {
        int total = 0;

        if (data.Type == ItemType.Weapon)
            total = ((ItemWeapon) data).GetStatValue(stat, ItemLevel);
        else if (data.Type == ItemType.Armor)
            total = ((ItemArmor) data).GetStatValue(stat, ItemLevel);
        else
            total = data.GetStatValue(stat, ItemLevel);

        Random.InitState(_VarianceSeed);

        foreach (ItemAffix affix in Affixes) {
            if (affix.isStatType(ItemModEffects.Core))
                switch(affix.GetModType()) {
                    case ItemModType.Add:
                        total += Mathf.RoundToInt(affix.GetValue(stat, ItemLevel, _VarianceSeed));
                        break;
                    default:
                        break;
                }
        }
        
        return total;
    }
    public int GetStats(PrimaryStats stat) {
        int total = 0;

        if (data is ItemWeapon)
            total = ((ItemWeapon) data).GetStatValue(stat, ItemLevel);
        else if (data is ItemArmor)
            total = ((ItemArmor) data).GetStatValue(stat, ItemLevel);
        else
            total = 0;

        foreach (ItemAffix affix in Affixes) {
            if (affix.isStatType(ItemModEffects.Primary))
                switch (affix.GetModType()) {
                    case ItemModType.Add:
                        total += Mathf.RoundToInt(affix.GetValue(stat, ItemLevel, _VarianceSeed));
                        break;
                    default:
                        break;
                }
        }

        return total;
    }
    public int GetStats(SecondaryStats stat) {
        int total = 0;

        if (data is ItemWeapon)
            total = ((ItemWeapon) data).GetStatValue(stat, ItemLevel);
        else if (data is ItemArmor)
            total = ((ItemArmor) data).GetStatValue(stat, ItemLevel);
        else
            total = 0;

        foreach (ItemAffix affix in Affixes) {
            if (affix.isStatType(ItemModEffects.Secondary))
                switch (affix.GetModType()) {
                    case ItemModType.Add:
                        total += Mathf.RoundToInt(affix.GetValue(stat, ItemLevel, _VarianceSeed));
                        break;
                    default:
                        break;
                }
        }

        if (stat == SecondaryStats.SkillLevel)
            total = Mathf.CeilToInt(total / 100f);
        
        return total;
    }
    public float GetMultiplier(CoreStats stat) {
        float multiplier = 0f;

        Random.InitState(_VarianceSeed);

        foreach (ItemAffix affix in Affixes) {
            if (affix.isStatType(ItemModEffects.Core))
                switch (affix.GetModType()) {
                    case ItemModType.Multi:
                        multiplier += affix.GetValue(stat, ItemLevel, _VarianceSeed);
                        break;
                    default:
                        break;
                }
        }

        return multiplier;
    }
    public float GetMultiplier(PrimaryStats stat) {
        float multiplier = 0f;

        Random.InitState(_VarianceSeed);

        foreach (ItemAffix affix in Affixes) {
            if (affix.isStatType(ItemModEffects.Primary))
                switch (affix.GetModType()) {
                    case ItemModType.Multi:
                        multiplier += affix.GetValue(stat, ItemLevel, _VarianceSeed);
                        break;
                    default:
                        break;
                }
        }

        return multiplier;
    }
    public float GetMultiplier(SecondaryStats stat) {
        float multiplier = 0f;

        Random.InitState(_VarianceSeed);

        foreach (ItemAffix affix in Affixes) {
            if (affix.isStatType(ItemModEffects.Secondary))
                switch (affix.GetModType()) {
                    case ItemModType.Multi:
                        multiplier += affix.GetValue(stat, ItemLevel, _VarianceSeed);
                        break;
                    default:
                        break;
                }
        }

        return multiplier;
    }

    public string Name { get {
            if (Quality == ItemQuality.Unique)
                return data.UniqueItemReference[UniqueItemIndex].Name;

            if (Affixes.Count > 0) {
                string temp = "";
                if (Affixes[0].data.Type == AffixType.Affix) {
                    temp = Affixes[0].data.Name + " " + data.Name;
                } else {
                    temp = data.Name + " " + Affixes[0].data.Name;
                }
                return temp;
            } else {
                return data.Name;
            }
        }
    }

    //public static bool ONE_TIME_DEBUG = false;
    public int Value {
        get {
            if (data is ItemCurrency)
                return ((ItemCurrency) data).Value;

            if (!isIdentified) {
                float multiplier = 1;
                var globals = dataMan.globalData;

                switch (Quality) {
                    case ItemQuality.Magic: multiplier = globals.GetGlobalAsFloat(GlobalProps.SELL_UNIDENTIFIED_MAGIC); break;
                    case ItemQuality.Rare: multiplier = globals.GetGlobalAsFloat(GlobalProps.SELL_UNIDENTIFIED_RARE); break;
                    case ItemQuality.Unique: multiplier = globals.GetGlobalAsFloat(GlobalProps.SELL_UNIDENTIFIED_UNIQUE); break;
                    case ItemQuality.Common:
                    default: multiplier = globals.GetGlobalAsFloat(GlobalProps.SELL_UNIDENTIFIED_COMMON); break;
                }

                return Mathf.CeilToInt((1f + multiplier) * ((float)data.Value * _baseItemLevel));
            }

            if (Quality == ItemQuality.Unique)
                return Mathf.CeilToInt((float) data.UniqueItemReference[UniqueItemIndex].Value * ItemLevel);

            return Mathf.CeilToInt((float)data.Value * ItemLevel);
        }
    }

    public int SellValue {
        get {
            float multiplier = dataMan.globalData.GetGlobalAsFloat(GlobalProps.SELL_VALUE_MULTIPLIER);
            return Mathf.CeilToInt((float)Value * multiplier);
        }
    }

    public Color GetBackgroundColor() {
        switch (Quality) {
            case ItemQuality.Magic: return ColorConstants.ITEM_MAGIC.ToHexColor();
            case ItemQuality.Rare: return ColorConstants.ITEM_RARE.ToHexColor();
            case ItemQuality.Unique: return ColorConstants.ITEM_UNIQUE.ToHexColor();
            default: {
                    if(Affixes.Count>0) return ColorConstants.ITEM_COMMON_WITH_AFFIX.ToHexColor();
                    return ColorConstants.ITEM_COMMON.ToHexColor();
                }
        }
    }
}
