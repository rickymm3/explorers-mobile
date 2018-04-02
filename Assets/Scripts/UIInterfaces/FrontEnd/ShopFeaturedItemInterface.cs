using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RSG;
using ExtensionMethods;
using DG.Tweening;

public class ShopFeaturedItemInterface : Tracer {
    public static ShopFeaturedItemInterface Instance;
    static FeaturedItemResponse _CurrentResponse;
    public static FeaturedItemResponse CurrentResponse {
        get { return _CurrentResponse; }
    }

    [Header("Feature Item")]
    public TextMeshProUGUI txtFeaturedCost;
    public Image featuredBkg;
    public Image featuredIcon;
    public GameObject FI_container;
    public GameObject FI_alreadySold;
    public TextMeshProUGUI txtFeaturedTimer;
    public Image featuredWipeTimer;
    public Button btnBuyFeaturedItem;
    
    Item _featuredItem;
    bool _featuredItemAlreadyBought = false;
    
    void Start () {
        Instance = this;

        PlayerManager.signals.OnFeaturedItemUpdated += OnFeaturedItemUpdated;

        if (_CurrentResponse != null) {
            LoadFeaturedItem(_CurrentResponse.seed);
        }
        OnFeaturedItemUpdated();
    }

    void OnDestroy() {
        trace("SHOP", "Destroying the Shop / listeners.");
        PlayerManager.signals.OnFeaturedItemUpdated -= OnFeaturedItemUpdated;
    }

    void Update () {
        if (PlayerManager.Instance.isLastFeaturedItemPurchased) {
            FI_container.SetActive(false);
            FI_alreadySold.SetActive(true);
        } else {
            FI_alreadySold.SetActive(false);
            FI_container.SetActive(true);
        }

        if (_CurrentResponse != null) {
            featuredWipeTimer.fillAmount = _CurrentResponse.progressPercent;
            txtFeaturedTimer.text = _CurrentResponse.progressSeconds.ToHHMMSS(isMonospaceHTML: true);
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////// Static:

    public static void SetCurrentResponseAndUpdate(FeaturedItemResponse fiResponse) {
        _CurrentResponse = fiResponse;
        TimeSpan diff = fiResponse.dateNext - DateTime.Now;

        PlayerManager.Instance.isLastFeaturedItemPurchased = fiResponse.isItemPurchased;
        GameManager.Instance.UpdateFeatureItemTimer();
    }

    ////////////////////////////////////////////////////////////////////////////////////////// Button Handlers:

    public void Btn_ShowFeaturedDetails() {
        ItemDetailsInterface details = ItemDetailsInterface.Open(_featuredItem);
    }

    public void Btn_BuyFeatured() {
        // Buy the item and set the timestamp of bought
        if (_featuredItem == null) {
            print("Can't buy featured Item it is null");
            return;
        }

        int gold = CurrencyTypes.GOLD.GetAmount();
        if (gold < _featuredItem.Value) {
            TimelineTween.ShakeError(btnBuyFeaturedItem.gameObject);
            return;
        }

        GameAPIManager.API.Shop.BuyFeaturedItem(_featuredItem, CurrencyTypes.GOLD, _featuredItem.Value)
            .Then(featuredResponse => {
                PlayerManager.Instance.isLastFeaturedItemPurchased = true;

                print("Successfully bought " + _featuredItem.Name);
            })
            .Catch(error => Debug.LogError(error));
    }

    //////////////////////////////////////////////////////////////////////////////////////////

    void OnFeaturedItemUpdated() {
        if (!GameManager.Instance.isUpdateFeaturedItemPending) return;

        GameManager.Instance.isUpdateFeaturedItemPending = false;

        trace("SHOP", "OnFeaturedItemUpdated.....");
        GameAPIManager.Instance.Shop.GetFeaturedItemSeed()
            .Then(fiResponse => {
                LoadFeaturedItem(fiResponse.seed);
                SetCurrentResponseAndUpdate(fiResponse);
            })
            .Catch(err => {
                traceError("Could not update the Featured Item & Seed: " + err.Message);
            });
    }

    void LoadFeaturedItem(int seed) {
        trace("SHOP", "========== LoadFeaturedItem: " + seed);
        UnityEngine.Random.InitState(seed);

        int qSeed = GameManager.Instance.GetFeaturedQualitySeed();
        int affSeed = GameManager.Instance.GetSeed(true);
        int ilvlSeed = GameManager.Instance.GetSeed(true);
        int varSeed = GameManager.Instance.GetSeed(true);

        ItemData itemIdentity = ShopInterface.shopLootTable.GetWeightedRandom();
        int breakCheck = 0;
        while (itemIdentity.Type == ItemType.Currency && breakCheck < 100000) {
            itemIdentity = ShopInterface.shopLootTable.GetWeightedRandom();
            breakCheck++; // safety
        }

        var item = _featuredItem = new Item(itemIdentity, qSeed, affSeed, ilvlSeed, varSeed, 0f, (PlayerManager.Instance.Level * 5f) + 20f, 20f);

        item.isIdentified = true;
        txtFeaturedCost.text = item.Value.ToString();
        featuredIcon.sprite = item.data.LoadSprite();
        featuredBkg.color = item.GetBackgroundColor();
    }
}
