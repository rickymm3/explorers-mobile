using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnchantInterface : PanelWithGetters {

    public Image itemIcon;

    public TextMeshProUGUI itemAffixes;
    public TextMeshProUGUI valueCurrency;
    public TextMeshProUGUI affixCurrency;
    public TextMeshProUGUI currencyTypeAmount;

    public Image valueIcon;
    public Image affixIcon;

    int _valueRerollCost = 0;
    int _affixRerollCost = 0;
    Action<Item> _callback = null;
    Item _item;

    public int NumShards { get { return GetShardItemType().GetAmount(); } }


    public void Init(Item item, Action<Item> callback = null) {
        this._item = item;

        string details = "";

        foreach (ItemAffix affix in item.Affixes) {
            foreach (CoreStats stat in Enum.GetValues(typeof(CoreStats))) {
                if (affix.ContainsStat(stat))
                    details += affix.GetDisplayValue(stat, item.ItemLevel, item.VarianceSeed) + "\n";
            }
            foreach (PrimaryStats stat in Enum.GetValues(typeof(PrimaryStats))) {
                if (affix.ContainsStat(stat))
                    details += affix.GetDisplayValue(stat, item.ItemLevel, item.VarianceSeed) + "\n";
            }
            foreach (SecondaryStats stat in Enum.GetValues(typeof(SecondaryStats))) {
                if (affix.ContainsStat(stat))
                    details += affix.GetDisplayValue(stat, item.ItemLevel, item.VarianceSeed) + "\n";
            }
        }

        itemAffixes.text = details;

        itemIcon.sprite = item.data.LoadSprite();

        _valueRerollCost = Mathf.CeilToInt(item.ItemLevel * 0.35f);
        _affixRerollCost = Mathf.CeilToInt(item.ItemLevel * 0.2f);

        valueCurrency.text = _valueRerollCost.ToString();
        affixCurrency.text = _affixRerollCost.ToString();

        // TODO show the currently relevant shard values
        trace("TODO", "show the currently relevant shard values");

        this._callback = callback;

        UpdateCurrentCurrency();
    }

    public void Btn_Back() {
        if (_callback != null) _callback(_item);
        MenuManager.Instance.Pop();
    }

    public void Btn_RerollValue() {
        if (HasUnsufficientShards(_valueRerollCost, "Value")) return;

        API.Currency.AddCurrency(GetShardItemType(), -_valueRerollCost)
            .Then( res => {
                // Update item values and re-init the UI
                _item.RerollVarianceLevel();
                OnRerollSuccessful();
            })
            .Catch( err => traceError("Could not spend Shards for RerollVarianceLevel: " + err.Message));
    }

    public void Btn_RerollAffix() {
        if (HasUnsufficientShards(_affixRerollCost, "Affix")) return;

        API.Currency.AddCurrency(GetShardItemType(), -_affixRerollCost)
            .Then(res => {
                // Update item values and re-init the UI
                _item.RerollAffixes();
                OnRerollSuccessful();
            })
            .Catch(err => traceError("Could not spend Shards for RerollAffixes: " + err.Message));
    }

    ////////////////////////////////////////////////////////////////////

    void OnRerollSuccessful() {
        //CurrencyManager.Instance.RemoveShards(GetShardType(), valueRerollCost);
        AudioManager.Instance.Play(SFX_UI.Coin);
        
        // Might need to store the new item into the item in the player inventory here? not sure if it is by ref or not
        traceWarn("Probably need to call API to modify Shards here!!!");
        Init(_item, _callback);

        UpdateCurrentCurrency();
    }

    CurrencyTypes GetShardItemType() {
        switch(_item.Quality) {
            case ItemQuality.Common: return CurrencyTypes.SHARDS_ITEMS_COMMON;
            case ItemQuality.Rare: return CurrencyTypes.SHARDS_ITEMS_RARE;
            case ItemQuality.Magic: return CurrencyTypes.SHARDS_ITEMS_MAGIC;
            case ItemQuality.Unique: return CurrencyTypes.SHARDS_ITEMS_RARE;
        }
        return CurrencyTypes.NONE;
    }

    bool HasUnsufficientShards(int cost, string rerollWhat) {
        if(NumShards >= cost) return false;

        traceError(string.Format("Not enough Shards to re-roll {0}! {1} shards.", rerollWhat, NumShards));
        return true;
    }

    void UpdateCurrentCurrency() {
        string details = GetShardItemType().GetAmount() + " ";

        switch (GetShardItemType()) {
            case CurrencyTypes.SHARDS_ITEMS_COMMON: details += "cs"; break;
            case CurrencyTypes.SHARDS_ITEMS_MAGIC: details += "ms"; break;
            case CurrencyTypes.SHARDS_ITEMS_RARE: details += "rs"; break;
        }

        currencyTypeAmount.text = details;
    }
}
