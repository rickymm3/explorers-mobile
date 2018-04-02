using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExtensionMethods;
using DG.Tweening;

public class UserProfileTabGear : UserProfileTab_Base {
    [Header("Templates")]
    public UserProfileItemContainer templateItemContainer;
    public UserProfileCategoryContainer templateCategoryContainer;

    ItemDetailsInterface _details;
    Button _detailsIdentify;

    // Use this for initialization
    void Start() {
        templateCategoryContainer.gameObject.SetActive(false);
        templateItemContainer.gameObject.SetActive(false);

        preloadDelay = 0.5f;
        WaitForData();
    }
    
    public override void OnDataLoaded() {
        AddCategory("Weapons");
        PopulateItemsBy(EquipmentType.Weapon);

        AddCategory("Chests");
        PopulateItemsBy(EquipmentType.Chest);

        AddCategory("Helms");
        PopulateItemsBy(EquipmentType.Helm);

        AddCategory("Gloves");
        PopulateItemsBy(EquipmentType.Gloves);

        AddCategory("Boots");
        PopulateItemsBy(EquipmentType.Boots);

        AddCategory("Artifacts");
        PopulateItemsBy(EquipmentType.Artifact);
    }

    void PopulateItemsBy(EquipmentType elementType) {
        List<Item> items = DataManager.Instance.GetItemsAvailable(elementType, ItemFilterType.Default, true);
        
        foreach (Item item in items) {
            AddItem(item, container.transform);
        }
    }

    UserProfileCategoryContainer AddCategory(string name) {
        UserProfileCategoryContainer clone = this.Clone<UserProfileCategoryContainer>(templateCategoryContainer.gameObject);

        clone.txtName.text = name;

        return clone;
    }

    private void AddItem(Item item, Transform parent) {
        UserProfileItemContainer itemContainer = this.Clone<UserProfileItemContainer>(templateItemContainer.gameObject);
        itemContainer.item = item;
        itemContainer.imgCounter.gameObject.SetActive(false);
        itemContainer.txtItemName.text = item.Name;
        itemContainer.imgCellBackground.color = item.GetBackgroundColor();

        itemContainer.imgItem.sprite = item.data.LoadSprite();

        itemContainer.btn.onClick.AddListener(() => OpenItemDetails(itemContainer));

        if(!item.isIdentified || item.isResearched) {
            itemContainer.imgQuestion.gameObject.SetActive(true);
            itemContainer.txtItemDescription.text = "Unidentified item";
        } else {
            itemContainer.imgQuestion.gameObject.SetActive(false);
            itemContainer.txtItemDescription.text = item.data.Description;
        }
    }

    private void OpenItemDetails(UserProfileItemContainer itemContainer) {
        if (itemContainer.imgQuestion.gameObject.activeSelf) {
            itemContainer.imgQuestion.transform.DOKill();
            TimelineTween.ShakeError(itemContainer.imgQuestion.gameObject);
            return;
        }

        _details = ItemDetailsInterface.Open(itemContainer.item);
        _details.btnEnchant.gameObject.SetActive(false);

        _details.AddButton("Close", () => _details.Close());
    }
}
