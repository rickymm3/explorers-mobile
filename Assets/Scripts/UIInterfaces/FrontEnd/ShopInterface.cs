using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using ExtensionMethods;

public class ShopInterface : Tracer {

    public static ShopInterface Instance;
    public static CurrencyManager.Cost REFRESH_COST;

    public RectTransform backButton;
    public RectTransform sellButton;

    public TextMeshProUGUI txtTimer;
    public TextMeshProUGUI txtRefreshCost;
    public TextMeshProUGUI txtCommonSellPrice;
    public TextMeshProUGUI txtUncommonSellPrice;
    public Button btnRefresh;
    public Button btnSellAllCommon;
    public Button btnSellAllUncommon;
    public RectTransform childPanel;
    public RectTransform itemContainer;
    public RectTransform sellItemContainer;

    public GameObject timerObject;
    public GameObject refreshObject;

    public ScrollRect itemsScrollRect;

    [HideInInspector] public bool isSell = false;

    public GameObject itemDisplayPrefab;
    List<GameObject> _itemPrefabs = new List<GameObject>();
    List<Item> _itemsToBuy = new List<Item>();
    ShopRefreshKey _refreshKeyLast;
    ShopRefreshKey _refreshKeyCurrent;

    List<Item> _itemsAllSorted;
    List<Item> _itemsCommon;
    List<Item> _itemsUncommon;
    int _sellPriceCommon = 0;
    int _sellPriceUncommon = 0;
    bool _isBusy = false;
    DateTime _dateLastRefreshed;

    public static LootTableData shopLootTable {
        get {
            string lootTableName = GlobalProps.SHOP_ITEM_LOOT_TABLE_LEVEL_10.GetString();

            int lvl = PlayerManager.Instance.Level;

            if (lvl > 30)
                lootTableName = GlobalProps.SHOP_ITEM_LOOT_TABLE_LEVEL_40.GetString();
            else if (lvl > 20)
                lootTableName = GlobalProps.SHOP_ITEM_LOOT_TABLE_LEVEL_30.GetString();
            else if (lvl > 10)
                lootTableName = GlobalProps.SHOP_ITEM_LOOT_TABLE_LEVEL_20.GetString();

            return DataManager.Instance.lootTableDataList.GetByIdentity(lootTableName);
        }
    }

    void Start () {
        if (!DataManager.Instance.isLoaded) return;

        Instance = this;

        _dateLastRefreshed = DateTime.MinValue;

        string refreshCostStr = GlobalProps.SHOP_REFRESH_KEY_COST.GetString();
        REFRESH_COST = CurrencyManager.ParseToCost(refreshCostStr);

        txtRefreshCost.text = "REFRESH FOR " + REFRESH_COST.amount;

        UpdateSortedInventoryItems();

        Btn_ShowStoreBuyMenu();
    }

    private void UpdateSortedInventoryItems() {
        _itemsAllSorted = SortItemsByQuality(DataManager.Instance.GetItemsAvailable().FindAll(i => !i.isResearched));
        _itemsCommon = _itemsAllSorted.FindAll(i => i.Quality==ItemQuality.Common);
        _itemsUncommon = _itemsAllSorted.FindAll(i => i.Quality==ItemQuality.Common);

        txtCommonSellPrice.text = (_sellPriceCommon = SumOfItemValues(_itemsCommon)).ToString();
        txtUncommonSellPrice.text = (_sellPriceUncommon = SumOfItemValues(_itemsUncommon)).ToString();
    }

    int SumOfItemValues(List<Item> items) {
        int sum = 0;
        foreach(Item item in items) sum += item.SellValue;
        return sum;
    }

    void Update() {
        if (isSell) return;

        UpdateRefreshKeyTimer();

        if(_itemsToBuy.Count>0 && _itemPrefabs.Count>0 && _itemPrefabs[0].transform.localScale.x==0) {
            trace("SHOP", "Items are not visible!!!!");
        }
    }

    //////////////////////////////////////////////////////////////////////////

    public void Show() { childPanel.DOAnchorPosY(-675f, 0.5f); }
    public void Hide() { childPanel.DOAnchorPosY(-2000f, 0.5f); }
    public void Btn_OnBack() { Hide(); }

    public void Btn_OnRefresh() {
        if(_isBusy) return;

        int gems = REFRESH_COST.type.GetAmount();
        if (gems < REFRESH_COST.amount) {
            //traceError("Unsufficient Funds to refresh!");
            TimelineTween.ShakeError(btnRefresh.gameObject);
            return;
        }
        LoadBuyItemList(true);
    }

    public void Btn_ShowStoreBuyMenu() {
        if (_isBusy) return;
        UpdateActiveUI(false);
        
        if (_itemsToBuy.Count == 0 || (_refreshKeyCurrent!=null && _refreshKeyCurrent.GetRemainingSeconds()<=0)) {
            LoadBuyItemList(false);
        } else {
            PopulateItemList(_itemsToBuy, _refreshKeyCurrent.seed);
        }
        Hide();
    }

    public void RefreshStoreList() {
        if (_isBusy) return;
        UpdateActiveUI(false);
        RandomizeItemsFromSeed(_refreshKeyCurrent.seed);

        LoadBuyItemList(false, OnRefreshKeyUpdated);
        
        Hide();
    }

    void OnRefreshKeyUpdated() {
        // Update the purchases array
        int[] newPurchased = new int[GlobalProps.SHOP_BASE_ITEM_COUNT.GetInt() + PlayerManager.Instance.ShopExpansion];
        string temp = "";
        for (int i = 0; i < newPurchased.Length; i++) {
            newPurchased[i] = -1;
        }
        for (int i = 0; i < _refreshKeyCurrent.purchased.Length; i++) {
            newPurchased[i] = _refreshKeyCurrent.purchased[i];
            temp += "Index [" + i + "] Value [" + _refreshKeyCurrent.purchased[i] + "]\n";
        }
        _refreshKeyCurrent.purchased = newPurchased;

        print(temp);


        if (_itemsToBuy.Count == 0 || (_refreshKeyCurrent != null && _refreshKeyCurrent.GetRemainingSeconds() <= 0)) {
            LoadBuyItemList(false);
        } else {
            PopulateItemList(_itemsToBuy, _refreshKeyCurrent.seed);
        }
    }

    public void Btn_ShowStoreSellMenu() {
        if (_isBusy) return;

        UpdateActiveUI(true);
        
        // LoadSellItemList
        PopulateItemList(_itemsAllSorted, isSelling: true);
        AnimateItems(true);

        Show();
    }

    public void Btn_SellAllCommon() {
        if (_isBusy) return;
        
        StartCoroutine( __SellBatchOfItems(btnSellAllCommon, _itemsCommon, CurrencyTypes.GOLD, _sellPriceCommon) );
    }

    public void Btn_SellAllUncommon() {
        if (_isBusy) return;
        
        StartCoroutine( __SellBatchOfItems(btnSellAllUncommon, _itemsUncommon, CurrencyTypes.GOLD, _sellPriceUncommon) );
    }

    IEnumerator __SellBatchOfItems(Button btn, List<Item> items, CurrencyTypes type, int cost) {
        if (items.Count == 0) {
            AudioManager.Instance.Play(SFX_UI.Invalid);
            TimelineTween.ShakeError(btn.gameObject);
            yield break;
        }

        btnSellAllCommon.interactable = false;
        btnSellAllUncommon.interactable = false;

        AnimateItems(false);

        yield return new WaitForSeconds(0.5f);

        _isBusy = false;

        GameAPIManager.API.Shop.SellItems(items, type, cost)
            .Then(res => {
                DataManager.Instance.allItemsList.RemoveAll(i => items.Contains(i));

                UpdateSortedInventoryItems();
                Btn_ShowStoreSellMenu();
                _isBusy = false;
            })
            .Catch(err => {
                traceError("Could not sell back of items: " + err.Message);
                _isBusy = false;
            });

        while(!_isBusy) yield return new WaitForEndOfFrame();

        AnimateItems(true);

        btnSellAllCommon.interactable = true;
        btnSellAllUncommon.interactable = true;

        AudioManager.Instance.Play(SFX_UI.Coin);
    }

    //////////////////////////////////////////////////////////////////////////

    void UpdateRefreshKeyTimer() {
        if (_isBusy || _refreshKeyCurrent == null) {
            txtTimer.text = "--:--";
            return;
        }

        var seconds = _refreshKeyCurrent.GetRemainingSeconds();
        txtTimer.text = seconds.ToHHMMSS(isMonospaceHTML: true);

        if (seconds <= 0) {
            //Auto Refresh the items with a new seed:
            LoadBuyItemList(false);
        }
    }

    void UpdateActiveUI(bool isSelling) {
        timerObject.SetActive(!isSelling);
        refreshObject.SetActive(!isSelling);
        isSell = isSelling;

        StartCoroutine(__ButtonFlip());
    }

    void LoadBuyItemList(bool isRefresh, Action callback = null) {
        if(ItemDetailsInterface.Instance!=null) {
            ItemDetailsInterface.Instance.Close();
        }

        DateTime now = DateTime.Now;
        TimeSpan diff = now - _dateLastRefreshed;

        if (diff.TotalSeconds < 2) return;

        _dateLastRefreshed = now;

        _isBusy = true;
        StartCoroutine(__WaitForRefreshSeed(isRefresh, callback));
    }

    IEnumerator __ButtonFlip() {
        RectTransform btnToShow = isSell ? backButton : sellButton;
        RectTransform btnToHide = isSell ? sellButton : backButton;

        btnToHide.DOScaleY(0f, 0.35f);
        yield return new WaitForSeconds(0.3f);
        btnToShow.DOScaleY(1f, 0.25f);
    }

    IEnumerator __WaitForRefreshSeed(bool isRefresh, Action callback = null) {
        _refreshKeyLast = _refreshKeyCurrent != null ? _refreshKeyCurrent : _refreshKeyLast;
        _refreshKeyCurrent = null;
        txtTimer.text = "--:--";
        
        AnimateItems(false);

        //While the API keeps returning a refresh-key that is still about to expire, keep calling it:
        while(_refreshKeyCurrent==null || _refreshKeyCurrent.GetRemainingSeconds()<=0) {
            btnRefresh.interactable = false;

            yield return new WaitForSeconds(0.5f);

            CallAPI(isRefresh);

            while (!btnRefresh.interactable) yield return new WaitForEndOfFrame();
        }

        RandomizeItemsFromSeed(_refreshKeyCurrent.seed);
        PopulateItemList(_itemsToBuy, _refreshKeyCurrent.seed);

        AnimateItems(true);

        _isBusy = false;

        if (callback != null) callback();
    }

    void CallAPI(bool isRefresh) {
        if (isRefresh) {
            GameAPIManager.API.Shop.RefreshSeed(REFRESH_COST)
                .Then(refreshKey => OnShopSeedLoaded(refreshKey, true))
                .Catch(OnShopError);
        } else {
            GameAPIManager.API.Shop.GetSeed()
                .Then(refreshKey => OnShopSeedLoaded(refreshKey, false))
                .Catch(OnShopError);
        }
    }

    void OnShopSeedLoaded(ShopRefreshKey refreshKey, bool reset = false) {
        trace("SHOP", "current Refresh Key (Seed value): " + refreshKey.seed);
        trace("SHOP", "Shop Seed DateTime: " + refreshKey.dateExpires.ToUniversalTime() + " : NOW = " + DateTime.Now.ToUniversalTime());
        trace("SHOP", refreshKey.GetRemainingSeconds());

        trace("SHOP", "PURCHASED: " + JsonUtility.ToJson( refreshKey.purchased ) );

        _refreshKeyCurrent = refreshKey;
        btnRefresh.interactable = true;
    }

    void OnShopError(Exception obj) {
        btnRefresh.interactable = true;

        if (_refreshKeyCurrent == null) {
            _refreshKeyCurrent = _refreshKeyLast;
        }
    }

    void RandomizeItemsFromSeed(int seed) {
        UnityEngine.Random.InitState(seed);

        // Generate the items to be put in the list
        int rareCount = 0;
        int rareLimit = 2;
        int magicCount = 0;
        int magicLimit = 3;

        _itemsToBuy.Clear();

        if(shopLootTable==null) {
            traceError("Cannot generated items in the shop! The LootTable is 'null'!");
            return;
        }

        // Add the random id and hero scrolls to the item list
        Item temp;
        var gm = GameManager.Instance;
        while (_itemsToBuy.Count < GlobalProps.SHOP_BASE_ITEM_COUNT.GetInt() + PlayerManager.Instance.ShopExpansion - 1) {
            // generate an item
            float playerLevelScale = GlobalProps.SHOP_ITEM_PLAYER_LEVEL_SCALE.GetFloat() * PlayerManager.Instance.Level;

            temp = new Item(
                shopLootTable.GetWeightedRandom(),
                gm.GetSeed(true),
                gm.GetSeed(true),
                gm.GetSeed(true),
                gm.GetSeed(true),
                0f,
                GlobalProps.SHOP_ITEM_LEVEL_BASE.GetFloat() + playerLevelScale,
                GlobalProps.SHOP_ITEM_LEVEL_VARIANCE.GetFloat()
                );

            // compare it to the restrictions, if it passes add it to the list, if not discard it
            if (temp.Quality == ItemQuality.Unique) continue;
            if (temp.Quality == ItemQuality.Rare) {
                if (rareCount >= rareLimit) continue;

                rareCount++;
            }
            if (temp.Quality == ItemQuality.Magic) {
                if (magicCount >= magicLimit) continue;

                magicCount++;
            }
            temp.isIdentified = true;
            _itemsToBuy.Add(temp);
        }

        //_itemsToBuy = SortItemsByQuality(_itemsToBuy);

        // Insert the identity scroll in the list randomly
        // Create an id scroll
        // insert it into the list
        string[] buyableCurrencies = GlobalProps.SHOP_CURRENCY_PURCHASABLE_ITEMS.GetString().Split("\n");
        string itemShopIdentity = buyableCurrencies[UnityEngine.Random.Range(0, buyableCurrencies.Length)];
        trace("SHOP", "Currency item to add: " + itemShopIdentity);
        temp = new Item(DataManager.Instance.itemDataList.GetByIdentity(itemShopIdentity), 0, 0, 0, 0, 0, 20f, 0);
        temp.isIdentified = true;
        _itemsToBuy.Insert(0, temp);

        trace("SHOP", "Store has generated " + _itemsToBuy.Count + "items [M: " + magicCount + "][R: " + rareCount + "]");
    }

    List<Item> SortItemsByQuality(List<Item> items) {
        return items.OrderByDescending(i => i.Quality).ToList();
    }

    void PopulateItemList(List<Item> items, int seed = -1, bool isSelling = false, bool sortItems=false) {
        if(!isSelling && _refreshKeyCurrent==null) {
            traceError("Cannot determine which item is sold-out without a ShopRefreshKey object: " + _refreshKeyCurrent);
            return;
        }

        _itemPrefabs.SetActiveForAll(false);

        // Load Heroes
        int index = 0;
        string output = "Purchased? ";
        foreach (Item item in items) {
            // Create Gameobject
            Transform container = isSelling ? sellItemContainer.transform : itemContainer.transform;
            GameObject itemPanel = _itemPrefabs.GetOrCreate(itemDisplayPrefab, container, isAnchoredZero: true);

            ItemStoreItemDetails details = itemPanel.GetComponent<ItemStoreItemDetails>();
            
            // Init the UI
            if (isSelling) {
                details.Initialize(index, seed, item, false, StoreTransactionType.Sell);
            } else {
                bool isAlreadyPurchased = (_refreshKeyCurrent.purchased[index] == seed) ? true : false;
                output += " #"+ index + "=" + (isAlreadyPurchased ? "Y" : "n");
                details.Initialize(index, seed, item, isAlreadyPurchased, StoreTransactionType.Buy);
            }

            index++;
        }

        trace("SHOP", output);
    }

    void AnimateItems(bool isShowing) {
        int index = 0;
        foreach (GameObject itemPrefab in _itemPrefabs) {
            RectTransform itemTrans = (RectTransform) itemPrefab.transform;

            itemTrans.DOKill();

            if (isShowing) {
                itemTrans.localScale = Vector2.zero;
                itemTrans.DOScale(1, 0.3f)
                    .SetEase(Ease.OutSine)
                    .SetDelay(index * 0.05f);
            } else {
                itemTrans.localScale = Vector2.one;
                itemTrans.DOScale(0, 0.3f)
                    .SetEase(Ease.InSine)
                    .SetDelay((_itemPrefabs.Count - index) * 0.05f);
            }

            index++;
        }
    }
}
