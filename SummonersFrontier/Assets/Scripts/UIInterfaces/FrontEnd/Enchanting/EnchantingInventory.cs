using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using ExtensionMethods;

public class EnchantingInventory : MonoBehaviour {
    public static ItemFilterType _ITEM_FILTER_TYPE = ItemFilterType.Default;

    [Header("Titles and Filters")]
    public TextMeshProUGUI InventoryTitle;
    public TextMeshProUGUI FilterType;
    public TextMeshProUGUI ItemTypeLabel;
    //public Button btnFilterType;

    [Header("Item References")]
    public Transform ItemContainer;
    public GameObject ItemPrefab;

    RectTransform _rectTranform;
    List<GameObject> _itemPrefabList = new List<GameObject>();
    EquipmentType _EquipType;
    Action<Item> onSelect;

    void Start () {
        _rectTranform = GetComponent<RectTransform>();

        UpdateFilterLabel();
    }
    
    public void LoadInventory(System.Action<Item> onSelect, EquipmentType EquipType = EquipmentType.None) {
        InventoryTitle.text = "Inventory";
        _EquipType = EquipType;

        RefreshItems();

        this.onSelect = onSelect;
    }

    public void Show() {
        _rectTranform.DOAnchorPos(new Vector2(0f, _rectTranform.anchoredPosition.y), 0.3f).SetEase(Ease.OutSine);
    }

    public void Btn_Hide() {
        if (_rectTranform != null) _rectTranform.DOAnchorPos(new Vector2(1000f, _rectTranform.anchoredPosition.y), 0.3f).SetEase(Ease.InSine);
    }

    public void Btn_ShowItem(Item item, ItemDisplayInterface requiredButNotUsed = null) {
        //ItemCompareScreen.SelectItem(item, _EquipType, _heroInterface, _onEquip);
        if (onSelect != null) onSelect(item);
        else Debug.Log(" [Enchanting] No OnSelect Setup --- Do item loading here [Selected: " + item.Name + "]");
    }

    public void RefreshItems() {
        _itemPrefabList.SetActiveForAll(false);

        List<ItemDisplayInterface> sorted = new List<ItemDisplayInterface>();

        List<Item> availableItems = new List<Item>();
        if (_EquipType == EquipmentType.None)
            availableItems = DataManager.Instance.GetItemsAvailable(_ITEM_FILTER_TYPE);
        else
            availableItems = DataManager.Instance.GetItemsAvailable(_EquipType, _ITEM_FILTER_TYPE);
        Func<Item, int> statsFunc = DataManager.Instance.GetItemsStatsFunc(_ITEM_FILTER_TYPE);

        foreach (var item in availableItems) {
            GameObject itemGO = _itemPrefabList.GetOrCreate(ItemPrefab);
            Button itemBtn = itemGO.GetComponent<Button>();
            ItemDisplayInterface itemDisplay = itemGO.GetComponent<ItemDisplayInterface>();
            RectTransform itemRect = itemGO.GetRect();

            itemDisplay.LoadItem(item, Btn_ShowItem, null);

            itemRect.SetParent(ItemContainer);
            itemRect.localScale = Vector3.one;
            itemRect.anchoredPosition = Vector2.zero;
            itemRect.sizeDelta = new Vector2(1f, 1f);

            itemDisplay.StatsInfo.SetActive(true);
            itemDisplay.txtStatsInfo.text = item.isIdentified && statsFunc != null ? statsFunc(item).ToString() : "...";

            itemGO.transform.SetAsLastSibling();
        }
    }

    private void UpdateFilterLabel() {
        FilterType.text = _ITEM_FILTER_TYPE.ToString();
    }

    public void Btn_OnFilterTypeOpen() {
        var labels = EnumUtils.ToStringArray<ItemFilterType>();

        ConfirmFilter.Ask("Item Filter", null, labels)
            .Then(answer => {
                Debug.Log("Answer is: " + answer);

                _ITEM_FILTER_TYPE = answer.AsEnum<ItemFilterType>();

                UpdateFilterLabel();
                RefreshItems();
            });
    }

    private void UpdateItemTypeLabel() {
        ItemTypeLabel.text = ((_EquipType == EquipmentType.None) ? "All" : _EquipType.ToString());
    }

    public void Btn_OnItemTypeOpen() {
        var labels = EnumUtils.ToStringArray<EquipmentType>();
        for (int i = 0; i < labels.Length; i++) {
            if (labels[i] == "None")
                labels[i] = "All";
        }

        ConfirmFilter.Ask("Item Type", null, labels)
            .Then(answer => {
                Debug.Log("Answer is: " + answer);

                if (answer == "All")
                    _EquipType = EquipmentType.None;
                else
                    _EquipType = answer.AsEnum<EquipmentType>();

                UpdateItemTypeLabel();
                RefreshItems();
            });
    }
}
