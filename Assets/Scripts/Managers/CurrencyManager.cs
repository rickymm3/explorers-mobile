using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;
using ExtensionMethods;
using TMPro;

public class CurrencyManager : ManagerSingleton<CurrencyManager> {
    
    bool _isInited = false;
    public bool IsInited { get { return _isInited; } }

    CurrencyDictionary _currencies = new CurrencyDictionary();
    public static CurrencyDictionary currencies { get { return Instance._currencies; } }

    BoostCurrencyDictionary _boosts = new BoostCurrencyDictionary();
    public static BoostCurrencyDictionary boosts { get { return Instance._boosts; } }

    // Use this for initialization
    void Awake () {
        ResetCurrencies();
    }

    // TODO remove this
    // debug variables and shard viewing
    int cs = 0, ms = 0, rs = 0, us = 0;
    void Update() {
        if (_isInited) {
            cs = _currencies[CurrencyTypes.SHARDS_ITEMS_COMMON];
            ms = _currencies[CurrencyTypes.SHARDS_ITEMS_MAGIC];
            rs = _currencies[CurrencyTypes.SHARDS_ITEMS_RARE];
            us = _currencies[CurrencyTypes.SHARDS_ITEMS_RARE];
        }
    }

    public void InitWithGlobals() {
        _isInited = false;

        //Set currencies to global props:
        _currencies.AddOrSet(CurrencyTypes.GOLD, GlobalProps.GOLD.GetInt());
        _currencies.AddOrSet(CurrencyTypes.GEMS, GlobalProps.GEMS.GetInt());
        _currencies.AddOrSet(CurrencyTypes.MAGIC_ORBS, GlobalProps.MAGIC_ORBS.GetInt());
        _currencies.AddOrSet(CurrencyTypes.SCROLLS_IDENTIFY, GlobalProps.SCROLLS_IDENTIFY.GetInt());
        _currencies.AddOrSet(CurrencyTypes.SCROLLS_SUMMON_COMMON, GlobalProps.SCROLLS_SUMMON_COMMON.GetInt());
        _currencies.AddOrSet(CurrencyTypes.SCROLLS_SUMMON_RARE, GlobalProps.SCROLLS_SUMMON_RARE.GetInt());
        _currencies.AddOrSet(CurrencyTypes.SCROLLS_SUMMON_MONSTER_FIRE, GlobalProps.SCROLLS_SUMMON_MONSTER_FIRE.GetInt());
        _currencies.AddOrSet(CurrencyTypes.SCROLLS_SUMMON_MONSTER_WATER, GlobalProps.SCROLLS_SUMMON_MONSTER_WATER.GetInt());
        _currencies.AddOrSet(CurrencyTypes.SCROLLS_SUMMON_MONSTER_NATURE, GlobalProps.SCROLLS_SUMMON_MONSTER_NATURE.GetInt());
        _currencies.AddOrSet(CurrencyTypes.SCROLLS_SUMMON_MONSTER_DARK, GlobalProps.SCROLLS_SUMMON_MONSTER_DARK.GetInt());
        _currencies.AddOrSet(CurrencyTypes.SCROLLS_SUMMON_MONSTER_LIGHT, GlobalProps.SCROLLS_SUMMON_MONSTER_LIGHT.GetInt());
        _currencies.AddOrSet(CurrencyTypes.SHARDS_ITEMS_COMMON, GlobalProps.SHARDS_ITEMS_COMMON.GetInt());
        _currencies.AddOrSet(CurrencyTypes.SHARDS_ITEMS_MAGIC, GlobalProps.SHARDS_ITEMS_MAGIC.GetInt());
        _currencies.AddOrSet(CurrencyTypes.SHARDS_ITEMS_RARE, GlobalProps.SHARDS_ITEMS_RARE.GetInt());
        _currencies.AddOrSet(CurrencyTypes.SHARDS_ITEMS_RARE, GlobalProps.SHARDS_ITEMS_RARE.GetInt());
        _currencies.AddOrSet(CurrencyTypes.XP_FRAGMENT, GlobalProps.XP_FRAGMENT.GetInt());
        _currencies.AddOrSet(CurrencyTypes.XP_FRAGMENT, GlobalProps.XP_FRAGMENT.GetInt());
        _currencies.AddOrSet(CurrencyTypes.XP_FRAGMENT_PLUS, GlobalProps.XP_FRAGMENT_PLUS.GetInt());
        _currencies.AddOrSet(CurrencyTypes.XP_FRAGMENT_PLUS, GlobalProps.XP_FRAGMENT_PLUS.GetInt());
        _currencies.AddOrSet(CurrencyTypes.ESSENCE_LOW, GlobalProps.ESSENCE_LOW.GetInt());
        _currencies.AddOrSet(CurrencyTypes.ESSENCE_MID, GlobalProps.ESSENCE_MID.GetInt());
        _currencies.AddOrSet(CurrencyTypes.ESSENCE_HIGH, GlobalProps.ESSENCE_HIGH.GetInt());
        _currencies.AddOrSet(CurrencyTypes.RELICS_SWORD, GlobalProps.RELIC_SWORD.GetInt());
        _currencies.AddOrSet(CurrencyTypes.RELICS_SHIELD, GlobalProps.RELIC_SHIELD.GetInt());
        _currencies.AddOrSet(CurrencyTypes.RELICS_STAFF, GlobalProps.RELIC_STAFF.GetInt());
        _currencies.AddOrSet(CurrencyTypes.RELICS_BOW, GlobalProps.RELIC_BOW.GetInt());

        trace("Setting boosts.......................");
        _boosts.AddOrSet(BoostType.Gold, 0);
        _boosts.AddOrSet(BoostType.Health, 0);
        _boosts.AddOrSet(BoostType.MagicFind, 0);
        _boosts.AddOrSet(BoostType.XP, 0);

        NotifyAllCurrencies();

        _isInited = true;
    }

    public static CurrencyTypes ConvertSummonTypeToCurrency(SummonType type) {
        switch (type) {
            case SummonType.Common: return CurrencyTypes.SCROLLS_SUMMON_COMMON;
            case SummonType.Rare: return CurrencyTypes.SCROLLS_SUMMON_RARE;
            case SummonType.MonsterDark: return CurrencyTypes.SCROLLS_SUMMON_MONSTER_DARK;
            case SummonType.MonsterFire: return CurrencyTypes.SCROLLS_SUMMON_MONSTER_FIRE;
            case SummonType.MonsterLight: return CurrencyTypes.SCROLLS_SUMMON_MONSTER_LIGHT;
            case SummonType.MonsterWater: return CurrencyTypes.SCROLLS_SUMMON_MONSTER_WATER;
            case SummonType.MonsterNature: return CurrencyTypes.SCROLLS_SUMMON_MONSTER_NATURE;
            default: return CurrencyTypes.GOLD;
        }
    }

    public static SummonType ConvertCurrencyToSummonType(CurrencyTypes type) {
        switch (type) {
            case CurrencyTypes.SCROLLS_SUMMON_COMMON: return SummonType.Common;
            case CurrencyTypes.SCROLLS_SUMMON_RARE: return SummonType.Rare;
            case CurrencyTypes.SCROLLS_SUMMON_MONSTER_DARK: return SummonType.MonsterDark;
            case CurrencyTypes.SCROLLS_SUMMON_MONSTER_FIRE: return SummonType.MonsterFire;
            case CurrencyTypes.SCROLLS_SUMMON_MONSTER_LIGHT: return SummonType.MonsterLight;
            case CurrencyTypes.SCROLLS_SUMMON_MONSTER_WATER: return SummonType.MonsterWater;
            case CurrencyTypes.SCROLLS_SUMMON_MONSTER_NATURE: return SummonType.MonsterNature;
            default: return SummonType.Common;
        }
    }

    public void NotifyAllCurrencies() {
        EnumUtils.ForEach<CurrencyTypes>(currency => NotifyCurrency(currency));
    }

    public void NotifyCurrency(CurrencyTypes currency, int before = -1) {
        if (signals == null) return;
        if (before < 0) before = _currencies[currency];
        if (signals.OnChangedCurrency != null) signals.OnChangedCurrency(currency.GetAmount(), before, currency);
    }

    public void NotifyBoostCurrency(BoostType boostType, int before = -1) {
        if (signals == null) return;
        if (before < 0) before = _boosts[boostType];
        if (signals.OnChangedBoostCurrency != null) signals.OnChangedBoostCurrency(boostType.GetAmount(), before, boostType);
    }

    //////////////////////////////////////////////////////

    public void ResetCurrencies() {
        EnumUtils.ForEach<CurrencyTypes>(currency => _currencies.AddOrSet(currency, 0));
        EnumUtils.ForEach<BoostType>(boost => _boosts.AddOrSet(boost, 0));
    }

    public static void ColorizeSlashText(CurrencyTypes currencyType, int requiredAmount, TextMeshProUGUI txtField, Button btn=null) {
        int amount = currencyType.GetAmount();
        bool hasSufficient = amount >= requiredAmount;
        txtField.text = amount + "/" + requiredAmount;
        txtField.color = hasSufficient ? Color.white : Color.red;
        if(btn) btn.interactable = hasSufficient;
    }

    ////////////////////////////////////////////////////// STATIC:

    public static object ConvertToCurrencyObject(CurrencyTypes type, int amount) {
        return MiniJSON.Json.Deserialize("{" + GetJSONCurrencyValueBasedOnENUM(type, amount) + "}");
    }

    public static string GetJSONCurrencyValueBasedOnENUM(CurrencyTypes type, int amount) {
        string typeCamelCase = type.ToString().ToCamelCase();

        return "\"" + typeCamelCase + "\": " + amount;
    }

    public static Cost ParseToCost(string costStr) {
        string[] costSplit = costStr.Split('=');
        if (costSplit.Length != 2) {
            throw new Exception("Can only parse Cost Objects as 'type=amount' format: " + costStr);
        }

        return new Cost(
            costSplit[0].Trim().AsEnum<CurrencyTypes>(),
            int.Parse(costSplit[1].Trim())
        );
    }

    public static BoostCost ParseToBoostCost(string costStr) {
        string[] costSplit = costStr.Split('=');
        if (costSplit.Length != 2) {
            throw new Exception("Can only parse Cost Objects as 'type=amount' format: " + costStr);
        }

        return new BoostCost(
            costSplit[0].Trim().AsEnum<BoostType>(),
            int.Parse(costSplit[1].Trim())
        );
    }

    [Serializable]
    public class Cost :EasyDictionary<CurrencyTypes, int> {
        private CurrencyTypes _type = CurrencyTypes.NONE;

        public CurrencyTypes type {
            get {
                if (_type == CurrencyTypes.NONE)
                    throw new Exception("Cost dictionary must have a primary-type set from it's constructor!");
                return _type;
            }
        }

        public int amount {
            get { return this[_type]; }
            set { this.AddOrSet(_type, value); }
        }

        public Cost() { }
        public Cost(CurrencyTypes type, int amount) {
            this._type = type;
            this.amount = amount;
        }

        public object ToObject() {
            List<string> jsonFields = new List<string>();

            foreach (var kv in this) {
                jsonFields.Add(GetJSONCurrencyValueBasedOnENUM(kv.Key, kv.Value));
            }

            return MiniJSON.Json.Deserialize("{" + jsonFields.Join() + "}");
        }
    }

    [Serializable]
    public class BoostCost :EasyDictionary<BoostType, int> {
        private BoostType _type = BoostType.None;

        public BoostType type {
            get {
                if (_type == BoostType.None)
                    throw new Exception("Cost dictionary must have a primary-type set from it's constructor!");
                return _type;
            }
        }

        public int amount {
            get { return this[_type]; }
            set { this.AddOrSet(_type, value); }
        }

        public BoostCost() { }
        public BoostCost(BoostType type, int amount) {
            this._type = type;
            this.amount = amount;
        }

        public object ToObject() {
            List<string> jsonFields = new List<string>();

            foreach (var kv in this) {
                jsonFields.Add("\"boost_" + kv.Key.ToString().ToLower() + "\": " + kv.Value);
            }

            return MiniJSON.Json.Deserialize("{" + jsonFields.Join() + "}");
        }
    }
}

public static class CurrencyTypesExtras {
    //For standard currencies:
    public static CurrencyManager.Cost ToCostObj(this CurrencyTypes currency, int amount) {
        return new CurrencyManager.Cost(currency, amount);
    }

    public static bool HasEnough(this CurrencyTypes currency, int cost) {
        return CurrencyManager.currencies[currency] >= cost;
    }

    public static int GetAmount(this CurrencyTypes currency) {
        return CurrencyManager.currencies[currency];
    }

    public static int SetAmount(this CurrencyTypes currency, int amount, bool notifyIfChanged = false) {
        if(currency==CurrencyTypes.NONE) return 0;
        if (amount < 0) amount = 0;

        bool isChanged = CurrencyManager.currencies[currency] != amount;
        CurrencyManager.currencies[currency] = amount;

        if (notifyIfChanged && isChanged) {
            CurrencyManager.Instance.NotifyCurrency(currency);
        }

        return amount;
    }

    //For Boost currencies:
    public static CurrencyManager.BoostCost MakeCost(this BoostType boostType, int amount) {
        return new CurrencyManager.BoostCost(boostType, amount);
    }

    public static bool HasEnough(this BoostType boostType, int cost) {
        return CurrencyManager.boosts[boostType] >= cost;
    }

    public static int GetAmount(this BoostType boostType) {
        return CurrencyManager.boosts[boostType];
    }

    public static int SetAmount(this BoostType boostType, int amount, bool notifyIfChanged = false) {
        if (boostType == BoostType.None) return 0;
        if (amount < 0) amount = 0;
        
        bool isChanged = CurrencyManager.boosts[boostType] != amount;
        CurrencyManager.boosts[boostType] = amount;

        if (notifyIfChanged && isChanged) {
            CurrencyManager.Instance.NotifyBoostCurrency(boostType);
        }

        return amount;
    }
}
