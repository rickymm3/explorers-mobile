using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using ExtensionMethods;

public class UserProfileTabItems : UserProfileTab_Base {
    [Serializable] public class CurrencySprites : SerializableDictionary<CurrencyTypes, Sprite> { }

    [Header("Templates")]
    public UserProfileItemContainer templateItemContainer;
    public UserProfileCategoryContainer templateCategoryContainer;

    [Inspectionary]
    [SerializeField]
    public CurrencySprites currencySprites;

    public LocalizationManager loc;

    // Use this for initialization
    void Start () {
		templateCategoryContainer.gameObject.SetActive(false);
        templateItemContainer.gameObject.SetActive(false);

        WaitForData();
    }

    public override void OnDataLoaded() {
        loc = LocalizationManager.GetSection(Section.USER_PROFILE_ITEMS);
        
        AddCurrencies("#ffffffff", "CATEGORY_SCROLLS",
            CurrencyTypes.SCROLLS_IDENTIFY,
            CurrencyTypes.SCROLLS_SUMMON_COMMON,
            CurrencyTypes.SCROLLS_SUMMON_RARE,
            CurrencyTypes.SCROLLS_SUMMON_MONSTER_DARK,
            CurrencyTypes.SCROLLS_SUMMON_MONSTER_FIRE,
            CurrencyTypes.SCROLLS_SUMMON_MONSTER_LIGHT,
            CurrencyTypes.SCROLLS_SUMMON_MONSTER_WATER,
            CurrencyTypes.SCROLLS_SUMMON_MONSTER_NATURE);

        AddCurrencies("#ffff4433", "CATEGORY_SHARDS_ITEMS",
            CurrencyTypes.SHARDS_ITEMS_COMMON,
            CurrencyTypes.SHARDS_ITEMS_MAGIC,
            CurrencyTypes.SHARDS_ITEMS_RARE);

        AddCurrencies("#ff3366ff", "CATEGORY_SHARDS_XP",
            CurrencyTypes.XP_FRAGMENT,
            CurrencyTypes.XP_FRAGMENT_PLUS);

        AddCurrencies("#ff3366ff", "CATEGORY_ESSENCE", 
            CurrencyTypes.ESSENCE_LOW,
            CurrencyTypes.ESSENCE_MID,
            CurrencyTypes.ESSENCE_HIGH);

        AddCurrencies("#ff3366ff", "CATEGORY_RELICS",
            CurrencyTypes.RELICS_BOW,
            CurrencyTypes.RELICS_SHIELD,
            CurrencyTypes.RELICS_STAFF,
            CurrencyTypes.RELICS_SWORD);

        AddBoostTypes("#ff941EFF", "CATEGORY_BOOSTS");

        scrollRect.DOVerticalNormalizedPos(1, 0.3f);
    }

    void AddCurrencies(string colorHex, string category, params CurrencyTypes[] currencies) {
        Color color = colorHex.ToHexColor();
        AddCategory(loc.GetText(category));

        foreach (CurrencyTypes currency in currencies) {
            string currencyStr = currency.ToString();
            string[] labelAndDesc = loc.GetText("LABEL_" + currencyStr).Split("\n");
            string title = labelAndDesc[0];
            string desc = labelAndDesc.Slice(1).Join("\n");
            Sprite sprite = currencySprites.ContainsKey(currency) ? currencySprites[currency] : null;

            AddItem(title, desc, currency.GetAmount(), sprite, color);
        }
    }

    void AddBoostTypes(string colorHex, string category) {
        Color color = colorHex.ToHexColor();
        AddCategory(loc.GetText(category));

        EnumUtils.ForEach( (BoostType boostType) => {
            if(boostType== BoostType.None) return;

            string boostStr = boostType.ToString().ToUpper();
            string[] labelAndDesc = loc.GetText("LABEL_BOOST_" + boostStr).Split("\n");
            string title = labelAndDesc[0];
            string desc = labelAndDesc.Slice(1).Join("\n");

            var boostData = DataManager.Instance.boostDataList.Find(b => b.boostType == boostType);
            Sprite sprite = boostData == null ? null : boostData.LoadSprite();

            AddItem(title, desc, boostType.GetAmount(), sprite, color);
        });
    }

    void AddCategory(string name) {
        UserProfileCategoryContainer clone = this.Clone<UserProfileCategoryContainer>(templateCategoryContainer.gameObject);

        clone.txtName.text = name;
    }

    private void AddItem(string name, string desc, int amount, Sprite sprite, Color ribbonColor) {
        UserProfileItemContainer clone = this.Clone<UserProfileItemContainer>(templateItemContainer.gameObject);
        clone.txtCounter.text = amount.ToString();
        clone.txtItemName.text = name;
        clone.txtItemDescription.text = desc;

        clone.imgRibbon.color = ribbonColor;

        clone.imgItem.sprite = sprite;
    }
}
