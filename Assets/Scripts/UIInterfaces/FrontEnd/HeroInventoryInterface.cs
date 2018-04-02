using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using ExtensionMethods;

public class HeroInventoryInterface : Tracer {
    public static ItemFilterType _ITEM_FILTER_TYPE = ItemFilterType.Default;

    public TextMeshProUGUI InventoryTitle;
    public TextMeshProUGUI FilterType;
    public Button btnFilterType;

    public Transform ItemContainer;
    public GameObject ItemPrefab;

    public ItemDetailsOnHeroScreenInterface itemDetailsOnHero;
    public ItemComparesOnHeroScreenInterface ItemCompareScreen;

    RectTransform _rectTranform;
    Button _btnEquip;
    Button _btnShard;
    ItemDisplayInterface _currentItemDisplay;

    List<GameObject> _itemPrefabList = new List<GameObject>();
    ItemDetailsInterface _itemPanel;
    HeroDetailsInterface _heroInterface;
    EquipmentType _EquipType;
    Action<Item> _onEquip;

    void Start() {
        _rectTranform = GetComponent<RectTransform>();

        btnFilterType.onClick.AddListener(Btn_OnFilterTypeOpen);
        PlayerManager.Instance.Signals.OnChangedCurrency += OnChangedCurrency;

        UpdateFilterLabel();
    }

    private void UpdateFilterLabel() {
        FilterType.text = "<color=#ccc>SORT BY:</color> " + _ITEM_FILTER_TYPE;
    }

    private void Btn_OnFilterTypeOpen() {
        var labels = EnumUtils.ToStringArray<ItemFilterType>();

        ConfirmFilter.Ask("Item Filter", null, labels)
            .Then(answer => {
                trace("Answer is: " + answer);

                _ITEM_FILTER_TYPE = answer.AsEnum<ItemFilterType>();

                UpdateFilterLabel();
                RefreshItems();
            });
    }

    void OnDestroy() {
        PlayerManager.Instance.Signals.OnChangedCurrency -= OnChangedCurrency;
        _heroInterface = null;
        _itemPanel = null;
    }

    //public void ReloadInventory() {

    //}

    public void RemoveItem(Item data) {
        //GameObject removeObj = null;
        foreach (GameObject go in _itemPrefabList) {
            if (go.activeSelf) {
                if (go.GetComponent<ItemDisplayInterface>().item == data) {
                    go.SetActive(false);
                }
            }
        }
    }

    public void LoadInventory(EquipmentType EquipType, System.Action<Item> onEquip, HeroDetailsInterface heroInterface, bool reveal = false) {
        if(_currentItemDisplay!=null) return;

        InventoryTitle.text = "Inventory - " + EquipType;

        _EquipType = EquipType;
        _heroInterface = heroInterface;

        if (reveal) _rectTranform.DOAnchorPos(new Vector2(0f, _rectTranform.anchoredPosition.y), 0.3f).SetEase(Ease.OutSine);

        // Load the items from the player inventory here

        RefreshItems();

        this._onEquip = onEquip;
    }

    public void RefreshItems() {
        _itemPrefabList.SetActiveForAll(false);

        List<ItemDisplayInterface> sorted = new List<ItemDisplayInterface>();

        List<Item> availableItems = DataManager.Instance.GetItemsAvailable(_EquipType, _ITEM_FILTER_TYPE);
        Func<Item, int> statsFunc = DataManager.Instance.GetItemsStatsFunc(_ITEM_FILTER_TYPE);

        foreach (var item in availableItems) {
            GameObject itemGO = _itemPrefabList.GetOrCreate(ItemPrefab);
            Button itemBtn = itemGO.GetComponent<Button>();
            ItemDisplayInterface itemDisplay = itemGO.GetComponent<ItemDisplayInterface>();
            RectTransform itemRect = itemGO.GetRect();

            itemDisplay.LoadItem(item, Btn_ShowItem, _heroInterface);
            
            itemRect.SetParent(ItemContainer);
            itemRect.localScale = Vector3.one;
            itemRect.anchoredPosition = Vector2.zero;
            itemRect.sizeDelta = new Vector2(1f, 1f);

            itemDisplay.StatsInfo.SetActive(true);
            itemDisplay.txtStatsInfo.text = item.isIdentified && statsFunc!=null ? statsFunc(item).ToString() : "...";
            
            itemGO.transform.SetAsLastSibling();
        }
    }

    public void Btn_Hide() {
        _heroInterface.DeselectEquipSlot();
        _rectTranform.DOAnchorPos(new Vector2(900f, _rectTranform.anchoredPosition.y), 0.3f).SetEase(Ease.InSine);
        itemDetailsOnHero.BtnHide();
    }

    public void Btn_ShowItem(Item item, ItemDisplayInterface itemDisplay) {
        ItemCompareScreen.SelectItem(item, _EquipType, _heroInterface, _onEquip);
        /*_currentItemDisplay = itemDisplay;

        // Pop the item interface and load in the item
        _itemPanel = ItemDetailsInterface.Open(item, _onEquip);
        _btnEquip = _itemPanel.AddButton("Equip", _itemPanel.Btn_Equip);
        _btnShard = _itemPanel.AddButton("Shard", _itemPanel.Btn_Shard);
        
        _itemPanel.onRemove += (Panel panel) => { _currentItemDisplay = null; };

        UpdateButtons();*/
    }

    private void OnChangedCurrency(int newValue, int oldValue, CurrencyTypes currencyType) {
        if(currencyType!=CurrencyTypes.SCROLLS_IDENTIFY) return;

        UpdateButtons();
    }

    private void UpdateButtons() {
        if (_currentItemDisplay == null) {
            Tracer.traceError("Missing '_currentItemDisplay', can't update the buttons!");
            return;
        }

        Item item = _currentItemDisplay.item;
        if(_btnEquip!=null) _btnEquip.gameObject.SetActive(item.isIdentified);
        _currentItemDisplay.UpdateIdentify();
    }
}
