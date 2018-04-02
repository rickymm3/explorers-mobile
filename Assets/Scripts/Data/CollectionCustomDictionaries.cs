using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable] public class EquipSlotIconsDictionary : SerializableDictionary<EquipmentType, Sprite> { }
[Serializable] public class EquipedItemsDictionary : SerializableDictionary<EquipmentType, Item> { }
[Serializable] public class LootTableDictionary : SerializableDictionary<ItemData, float> { }
[Serializable] public class ItemTypeDictionary : SerializableDictionary<EquipmentType, float> { }
[Serializable] public class TierChanceDictionary : SerializableDictionary<int, float> { }
[Serializable] public class CrateTypeChanceDictionary : SerializableDictionary<CrateTypeData, float> { }
[Serializable] public class CrateChanceDictionary : SerializableDictionary<ItemQuality, float> { }
[Serializable] public class SFXTypeAllocations : SerializableDictionary<SFX_Types, int> { }

[Serializable] public class AffixSecondaryStatsDictionary : SerializableDictionary<SecondaryStats, AffixData> { }
[Serializable] public class AffixPrimaryStatsDictionary : SerializableDictionary<PrimaryStats, AffixData> { }
[Serializable] public class AffixCoreStatsDictionary : SerializableDictionary<CoreStats, AffixData> { }

[Serializable] public class RelicImageReferenceDictionary : SerializableDictionary<HeroClass, UnityEngine.Sprite> { }
[Serializable] public class EquipmentSlotDictionary : SerializableDictionary<EquipmentType, Image> { }
[Serializable] public class CoreStatsUpdatesDictionary : SerializableDictionary<CoreStats, TextMeshProUGUI> { }
[Serializable] public class PrimaryStatsUpdatesDictionary : SerializableDictionary<PrimaryStats, TextMeshProUGUI> { }
[Serializable] public class SecondaryStatsUpdatesDictionary : SerializableDictionary<SecondaryStats, TextMeshProUGUI> { }

[Serializable] public class TankLevelScale : SerializableDictionary<PrimaryStats, float> { }
[Serializable] public class AssassinLevelScale : SerializableDictionary<PrimaryStats, float> { }
[Serializable] public class MageLevelScale : SerializableDictionary<PrimaryStats, float> { }
[Serializable] public class AttackerLevelScale : SerializableDictionary<PrimaryStats, float> { }

[Serializable] public class BossPhaseSkillList : SerializableDictionary<BossPhases, List<Skill>> { }

[Serializable] public class CurrencyDictionary : EasyDictionary<CurrencyTypes, int> { }
[Serializable] public class BoostCurrencyDictionary : EasyDictionary<BoostType, int> { }

[Serializable] public class LootCrateDictionary : ListDictionary<ZoneData, LootCrate> { }

[Serializable] public class GlobalsDictionary : EasyDictionary<string, GlobalsValuesDictionary> { }
[Serializable] public class GlobalsValuesDictionary : EasyDictionary<GlobalProps, string> { };

[Serializable] public class SkillTimeLine : SerializableDictionary<float, SkillBehaviour> { }

[Serializable] public class RetireRewardDictionary : SerializableDictionary<string, float> { }

[Serializable] public class CurrencyIconsDictionary : SerializableDictionary<CurrencyTypes, CurrencyIcon> { }

[Serializable]
public class EasyDictionary<K, V> : SerializableDictionary<K, V> {
    public V AddOrSet(K key, V value) {
        if (this.ContainsKey(key)) this[key] = value;
        else this.Add(key, value);
        return value;
    }
}

[Serializable]
public class ListDictionary<K, V> :SerializableDictionary<K, List<V>> {
    public V AddToList(K key, V value) {
        if (!this.ContainsKey(key)) {
            this.Add(key, new List<V>());
        };

        this[key].Add(value);

        return value;
    }
}