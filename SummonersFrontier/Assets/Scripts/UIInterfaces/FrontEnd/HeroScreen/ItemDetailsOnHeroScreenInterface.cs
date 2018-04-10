using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ItemDetailsOnHeroScreenInterface : MonoBehaviour {
    
    Item item;
    
    [Header("Inventory")]
    public HeroInventoryInterface InventoryScreen;

    [Header("Item Details")]
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemType;
    public TextMeshProUGUI itemQuality;

    public GameObject RequireGO;
    public TextMeshProUGUI requireText;

    public Image questionMark;
    public Image itemIcon;
    public TextMeshProUGUI coreStatValue;
    public TextMeshProUGUI coreStatDescriptor;
    public TextMeshProUGUI itemDetails;

    [Inspectionary] public EquipSlotIconsDictionary EquipSlotIcons = new EquipSlotIconsDictionary();

    HeroDetailsInterface heroDetails;
    
    HeroDetailsInterface heroInterface;
    RectTransform _rectTranform;
    EquipmentType type;
    System.Action<Item> onEquip;

    public void ShowItem(Item item, EquipmentType type, HeroDetailsInterface heroInterface, System.Action<Item> onEquip) {
        
        this.type = type;
        this.onEquip = onEquip;
        this.heroInterface = heroInterface;
        
        _rectTranform = GetComponent<RectTransform>();

        if (item == null) {
            itemIcon.sprite = EquipSlotIcons[type];
            NoItemDisplay();
        } else {
            this.item = item;
            itemIcon.sprite = item.data.LoadSprite();

            if (item.data.ClassRestriction != null) {
                requireText.text = "Restricted to " + item.data.ClassRestriction.ToString() + " class";
                RequireGO.SetActive(true);
            } else {
                RequireGO.SetActive(false);
            }

            UpdateDetails(item.isIdentified);
        }

        // Show Window
        MoveWindow(0f);
    }

    public void MoveWindow(float xpos) {
        _rectTranform.DOAnchorPos(new Vector2(xpos, _rectTranform.anchoredPosition.y), 0.3f).SetEase(Ease.InSine);
    }

    public void BtnHide() {
        // Show Hide
        heroInterface.DeselectEquipSlot();
        MoveWindow(900f);
    }

    public void BtnEquip() {
        InventoryScreen.LoadInventory(type, onEquip, heroInterface, true);
    }

    private void NoItemDisplay() {
        itemName.text = "No Equipped Item";
        itemQuality.text = "";
        itemType.text = "";
        coreStatValue.text = "";
        coreStatDescriptor.text = "";
        itemDetails.text = "Currently no item equipped.";
        questionMark.enabled = false;
        RequireGO.SetActive(false);
    }

    private void UpdateDetails(bool isIdentified) {
        itemName.text = item.Name;
        itemQuality.text = item.Quality.ToString();
        itemType.text = item.data.EquipType.ToString();

        if (item.data.EquipType == EquipmentType.Weapon) {
            itemType.text += " - " + ((ItemWeapon) item.data).WeaponType.ToString();
            coreStatValue.text = item.GetStats(CoreStats.Damage).ToString();
            coreStatDescriptor.text = "Damage";
        } else if (item.data.EquipType == EquipmentType.Artifact) {
            coreStatValue.text = ((ItemArtifact) item.data).GetSkillLevel(item.ItemLevel).ToString();
            coreStatDescriptor.text = "Skill";
        } else {
            coreStatValue.text = item.GetStats(CoreStats.Defense).ToString();
            coreStatDescriptor.text = "Defense";
        }

        questionMark.enabled = !isIdentified;

        string detailsStr = item.data.Description;

        if (!isIdentified) {
            questionMark.color = Color.white;

            string Q = "???";
            itemName.text = "Unknown " + item.Type;
            coreStatValue.text = Q;
            coreStatDescriptor.text = Q;
            detailsStr = Q;
        }

        detailsStr += "\n\n";

        List<AffixDisplayInformation> affixDisplayList = new List<AffixDisplayInformation>();
        foreach (ItemAffix affix in item.Affixes) {
            if (affix.isStatType(ItemModEffects.Core))
                foreach (CoreStats stat in System.Enum.GetValues(typeof(CoreStats))) {
                    if (affix.ContainsStat(stat)) {
                        AddToAffixList(affixDisplayList, stat, affix);
                    }
                }

            if (affix.isStatType(ItemModEffects.Primary))
                foreach (PrimaryStats stat in System.Enum.GetValues(typeof(PrimaryStats))) {
                    if (affix.ContainsStat(stat)) {
                        AddToAffixList(affixDisplayList, stat, affix);
                    }
                }

            if (affix.isStatType(ItemModEffects.Secondary))
                foreach (SecondaryStats stat in System.Enum.GetValues(typeof(SecondaryStats))) {
                    if (affix.ContainsStat(stat)) {
                        AddToAffixList(affixDisplayList, stat, affix);
                    }
                }
        }

        string test = " >>> [1st] Affix breakdown: [" + item.MongoID + "]\n";
        if (isIdentified) {
            foreach (AffixDisplayInformation affix in affixDisplayList) {
                test += affix.GetDisplayValue() + " [" + affix.value + "]\n";
                detailsStr += affix.GetDisplayValue() + "\n";
            }
        } else {
            detailsStr += "???\n";
        }

        Debug.Log(test);

        itemDetails.text = detailsStr;
    }

    List<AffixDisplayInformation> AddToAffixList(List<AffixDisplayInformation> list, CoreStats stat, ItemAffix affix) {
        bool existsInList = false;
        foreach (AffixDisplayInformation info in list) {
            if (info.core != null && info.core == stat && info.type == affix.GetModType()) {
                info.value += affix.GetValue(stat, item.ItemLevel, item.VarianceSeed);
                existsInList = true;
            }
        }

        if (!existsInList) {
            AffixDisplayInformation info = new AffixDisplayInformation();
            info.core = stat;
            info.value = affix.GetValue(stat, item.ItemLevel, item.VarianceSeed);
            info.type = affix.GetModType();

            list.Add(info);
        }

        return list;
    }

    List<AffixDisplayInformation> AddToAffixList(List<AffixDisplayInformation> list, PrimaryStats stat, ItemAffix affix) {
        bool existsInList = false;
        foreach (AffixDisplayInformation info in list) {
            if (info.primary != null && info.primary == stat && info.type == affix.GetModType()) {
                info.value += affix.GetValue(stat, item.ItemLevel, item.VarianceSeed);
                existsInList = true;
            }
        }

        if (!existsInList) {
            AffixDisplayInformation info = new AffixDisplayInformation();
            info.primary = stat;
            info.value = affix.GetValue(stat, item.ItemLevel, item.VarianceSeed);
            info.type = affix.GetModType();

            list.Add(info);
        }

        return list;
    }

    List<AffixDisplayInformation> AddToAffixList(List<AffixDisplayInformation> list, SecondaryStats stat, ItemAffix affix) {
        bool existsInList = false;
        foreach (AffixDisplayInformation info in list) {
            if (info.secondary != null && info.secondary == stat && info.type == affix.GetModType()) {
                info.value += affix.GetValue(stat, item.ItemLevel, item.VarianceSeed);
                existsInList = true;
            }
        }

        if (!existsInList) {
            AffixDisplayInformation info = new AffixDisplayInformation();
            info.secondary = stat;
            info.value = affix.GetValue(stat, item.ItemLevel, item.VarianceSeed);
            info.type = affix.GetModType();

            list.Add(info);
        }

        return list;
    }

    void ClearStats() {
        if (heroDetails == null) return;

        heroDetails.ClearStatDifferences();
    }

    void UpdateStats() {
        if (heroDetails == null) return;

        heroDetails.CalculateStatDifference(item);
    }
}
