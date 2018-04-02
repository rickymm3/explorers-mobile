using System;
using UnityEngine;
using ExtensionMethods;

public enum BuildType { Development, Test, Production }

public enum CoreStats { Health, Damage, Defense }
public enum PrimaryStats { Strength, Vitality, Intelligence, Speed }
public enum SecondaryStats { CriticalChance, CriticalDamage, Dodge, MagicFind, MonsterFind, TreasureFind, CounterChance, SkillLevel, SkillDamage }

//public enum HeroQuality { Common = 1, Uncommon = 2, Magic = 3, Rare = 4, Legendary = 5 }
public enum ItemQuality { Common, Magic, Rare, Unique }
public enum ItemType { Weapon, Armor, Artifact, Currency }
public enum ItemFilterType { Default, Affixes, ItemLevel, Variance, MagicFind, Damage, Defense, Health, Intelligence, Strength, Speed, Vitality, SkillLevel, SkillDamage, Dodge, CounterChance, CriticalChance, CriticalDamage, MonsterFind, TreasureFind }

public enum EquipmentType { None, Helm, Weapon, Chest, Gloves, Boots, Artifact }
public enum WeaponType { Sword, Scythe, Bow, Dagger, Polearm, Staff, Club }
public enum ArtifactType { ElementalLevelIncrease, SkillDamageMultiplier, SkillLevelIncrease, ElementalDamageIncrease }
public enum ElementalTypes { None, Nature, Fire, Water, Light, Dark }
public enum SkillAttackTypes { Physical, Elemental, Slashing, Blunt, Ranged, Ice, Healing }
public enum ItemModType { Multi, Add, Status }
public enum ItemModEffects { None, Core, Primary, Secondary }
public enum AffixType { Affix, Suffix }
public enum PlayerLevelRewardType { Currency, ExploreSlot, Other }
public enum BoostType { None, Gold, XP, Health, MagicFind } // ideas to add: boosted retire rewards, boosted enchanting shards (would need to be time based for these)

public enum HeroClass { None, Bruiser, Assassin, Mage, Tank }
public enum HeroType { Monster, Soldier, Hero }
public enum HeroQuality { Common, Rare, Legendary }
public enum HeroPersonality { Couragous, Steady, Energetic, Astute, Wise, Perceptive, Stalwart, Tenacious, Vigorous, Agile, Nimble, Swift }
public enum HeroListFilterType { Level, Rarity, Health, Damage, Defense, Strength, Vitality, Intelligence, Speed, MagicFind }

public enum SummonType { Common, Rare, MonsterFire, MonsterWater, MonsterNature, MonsterDark, MonsterLight}
public enum ZoneDifficulty { Normal, Nightmare, Hell }

public enum CombatHitType { Normal, Critical, Glancing}
public enum TapMonsterType { Normal, Timed, Unique }

public enum CharacterBattleMode { Tap, Boss }
public enum BattlePhases { Loading, Initialize, Start, Battle, Results }

public enum GlobalProps {
    GOLD, GEMS, MAGIC_ORBS,
    SCROLLS_IDENTIFY,
    SCROLLS_SUMMON_COMMON,SCROLLS_SUMMON_RARE,SCROLLS_SUMMON_MONSTER_FIRE,SCROLLS_SUMMON_MONSTER_WATER,SCROLLS_SUMMON_MONSTER_NATURE,SCROLLS_SUMMON_MONSTER_LIGHT,SCROLLS_SUMMON_MONSTER_DARK,
    SHARDS_ITEMS_COMMON, SHARDS_ITEMS_MAGIC, SHARDS_ITEMS_RARE,
    XP_FRAGMENT, XP_FRAGMENT_PLUS,
    ESSENCE_LOW, ESSENCE_MID, ESSENCE_HIGH,
    RELIC_SWORD, RELIC_SHIELD, RELIC_STAFF, RELIC_BOW,
    BOOST_INIT_AMOUNT,
    UNEQUIP_FEE,
    SELL_UNIDENTIFIED_MAGIC,
    SELL_UNIDENTIFIED_RARE,
    SELL_UNIDENTIFIED_UNIQUE,
    SELL_UNIDENTIFIED_COMMON,
    SELL_UNIDENTIFIED_UNCOMMON,
    SELL_VALUE_MULTIPLIER,
    ZONE_MONSTER_BASE,
    ZONE_MONSTER_MULTIPLIER,
    ZONE_MONSTER_SCALER,
    ZONE_MONSTER_COUNT,
    ZONE_ITEM_BASE,
    ZONE_ITEM_MULTIPLIER,
    ZONE_ITEM_SCALER,
    SHOP_BASE_ITEM_COUNT,
    SHOP_ITEM_PLAYER_LEVEL_SCALE,
    SHOP_ITEM_LEVEL_BASE,
    SHOP_ITEM_LEVEL_VARIANCE,
    SHOP_EXPANSION_LIMIT,
    SHOP_REFRESH_KEY_COST,
    SHOP_ITEM_LOOT_TABLE_LEVEL_10,
    SHOP_ITEM_LOOT_TABLE_LEVEL_20,
    SHOP_ITEM_LOOT_TABLE_LEVEL_30,
    SHOP_ITEM_LOOT_TABLE_LEVEL_40,
    SHOP_CURRENCY_PURCHASABLE_ITEMS,
    EXPLORE_LIMIT,
    BOOST_LIMIT,
    BOOST_SLOT_COST,
    SUMMON_COMMON_MONSTER,
    SUMMON_COMMON_SOLDIER,
    SUMMON_COMMON_HERO,
    SUMMON_RARE_MONSTER,
    SUMMON_RARE_SOLDIER,
    SUMMON_RARE_HERO,
    SUMMON_QUALITY_1STAR,
    SUMMON_QUALITY_2STAR,
    SUMMON_QUALITY_3STAR,
    RESEARCH_TRAY_LIMIT,
    RESEARCH_SLOT_LIMIT,
    RESEARCH_UNLOCK_COST,
    RESEARCH_BOOSTER_BASE,
    RESEARCH_BOOSTER_MULTIPLIER,
    RESEARCH_TRAY_DURATIONS,
    STARTUP_HERO_LIST,
    TAP_BATTLE_EVENT_TRIGGER,
    TAP_BATTLE_EVENT_DURATION,
    BATTLE_RESURRECTION_COST
}

public enum CurrencyTypes {
    NONE,
    GOLD, GEMS, MAGIC_ORBS,
    SCROLLS_IDENTIFY, SCROLLS_SUMMON_COMMON, SCROLLS_SUMMON_RARE, SCROLLS_SUMMON_MONSTER_FIRE, SCROLLS_SUMMON_MONSTER_WATER, SCROLLS_SUMMON_MONSTER_NATURE, SCROLLS_SUMMON_MONSTER_LIGHT, SCROLLS_SUMMON_MONSTER_DARK,
    SHARDS_ITEMS_COMMON, SHARDS_ITEMS_MAGIC, SHARDS_ITEMS_RARE,
    XP_FRAGMENT, XP_FRAGMENT_PLUS,
    ESSENCE_LOW, ESSENCE_MID, ESSENCE_HIGH,
    RELICS_SWORD, RELICS_SHIELD, RELICS_STAFF, RELICS_BOW
}

// Never add between values, ALWAYS append new values at the end
public enum SFX_Types { UI, Music, Attacks, Defenses }
public enum SFX_UI { Null, Click, Invalid, Toggle, PageFlip, WooshIn, WooshOut, Coin, Identify, Earthquake, Explosion, ShardsChing }
public enum SFX_Music { Null, Song1, Song2, Song3 }
public enum SFX_Attacks { Null, Attack1, Attack2, Attack3 }
public enum SFX_Defenses { Null, Defense1, Defense2, Defense3 }

public enum InboxMessageType { NULL, GENERIC_MESSAGE, LOOTCRATE_REWARD, CURRENCY_REWARD }

public static class ColorConstants {
    public static string HTML_WHITE = "#ffffff";
    public static string HTML_BLACK = "#000000";
    public static string HTML_GOLD = "#fdfc93";
    public static string HTML_GEMS = "#93e9fd";
    public static string HTML_QUALITY = "#cca";

    public static string ITEM_COMMON = "#808080";
    public static string ITEM_COMMON_WITH_AFFIX = "#56d750";
    public static string ITEM_MAGIC = "#7858db";
    public static string ITEM_RARE = "#0e63ff";
    public static string ITEM_UNIQUE = "#d51818";

    public static string HERO_COMMON = "#8e83a6";
    public static string HERO_RARE = "#0e63ff";
    public static string HERO_LEGENDARY = "#ffbf36";
    public static string HERO_DISABLED = "#66666688";
}

public enum PrefType {
    LAST_SELECTED_ACT,
    LAST_SELECTED_ZONE
}

public static class PrefTypeExtensions {
    static string _PREFIX = "PREF";

    public static int GetInt(this PrefType prop, int defaultValue = 0) {
        return PlayerPrefs.GetInt(_PREFIX + prop.ToString(), defaultValue);
    }

    public static float GetFloat(this PrefType prop, float defaultValue = 0) {
        return PlayerPrefs.GetFloat(_PREFIX + prop.ToString(), defaultValue);
    }

    public static string GetString(this PrefType prop, string defaultValue = null) {
        return PlayerPrefs.GetString(_PREFIX + prop.ToString(), defaultValue);
    }

    public static int SetInt(this PrefType prop, int value) {
        PlayerPrefs.SetInt(_PREFIX + prop.ToString(), value);
        return value;
    }

    public static float SetFloat(this PrefType prop, float value) {
        PlayerPrefs.SetFloat(_PREFIX + prop.ToString(), value);
        return value;
    }

    public static string SetString(this PrefType prop, string value) {
        PlayerPrefs.SetString(_PREFIX + prop.ToString(), value);
        return value;
    }
}