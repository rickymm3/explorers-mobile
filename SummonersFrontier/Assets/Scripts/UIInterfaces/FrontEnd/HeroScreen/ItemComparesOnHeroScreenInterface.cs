using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ItemComparesOnHeroScreenInterface : MonoBehaviour {
    public static bool KEEP_UNTIL_CONFIRMED = false;

    Item item;
    Item equipedItem;

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

    public Button BTN_Equip;
    public Image Frame;
    public RectTransform Arrow;

    public Image CurrentlyEquipped;
    public Image SelectedEquipment;

    [Inspectionary]
    public EquipSlotIconsDictionary EquipSlotIcons = new EquipSlotIconsDictionary();
    
    HeroDetailsInterface heroInterface;
    RectTransform _rectTranform;
    EquipmentType type;
    System.Action<Item> onEquip;
    bool _isClosing = false;
    //bool _isBusy = false;

    public void SelectItem(Item item, EquipmentType type, HeroDetailsInterface heroInterface, System.Action<Item> onEquip) {
        _isClosing = false;

        this.type = type;
        this.onEquip = onEquip;
        this.heroInterface = heroInterface;

        _rectTranform = GetComponent<RectTransform>();
        
        this.item = item;
        itemIcon.sprite = item.data.LoadSprite();

        if (item.data.ClassRestriction != null) {
            requireText.text = "Restricted to " + item.data.ClassRestriction.ToString() + " class";
            RequireGO.SetActive(true);
        } else {
            RequireGO.SetActive(false);
        }

        UpdateDetails(item.isIdentified);

        if (!item.isIdentified)
            BTN_Equip.interactable = false;
        else
            BTN_Equip.interactable = true;

        if (heroInterface.Equipped.ContainsKey(type)) {
            CurrentlyEquipped.sprite = item.data.LoadSprite();
            equipedItem = heroInterface.Equipped[type];
        } else {
            CurrentlyEquipped.sprite = EquipSlotIcons[type];
            equipedItem = null;
        }

        SelectedEquipment.sprite = item.data.LoadSprite();

        // Show Window
        MoveWindow(0f);
    }

    public void MoveWindow(float xpos) {
        if (_rectTranform == null) return;
        _rectTranform.DOAnchorPos(new Vector2(xpos, _rectTranform.anchoredPosition.y), 0.3f).SetEase(Ease.InSine);
    }

    public void BtnHide() {
        // Show Hide
        if (heroInterface != null) heroInterface.DeselectEquipSlot();
        ClearStats();

        MoveWindow(900f);
    }

    public void BtnEquip() {
        if (!KEEP_UNTIL_CONFIRMED && !Close()) return; //Prevents multiple clicks/taps

        if (onEquip == null) return;

        // Equip the item
        onEquip(item);
    }

    public void Btn_Close() {
        Close();
        ClearStats();
    }

    public bool Close() {
        //if (_isBusy) return true;
        if (_isClosing) return false;
        _isClosing = true;

        ClearStats();

        heroInterface.DeselectEquipSlot();
        MoveWindow(900f);

        return true;
    }

    public void Tap_DisplayCurrentlyEquippedItem() {
        if (equipedItem != null) {
            UpdateDetails(true, equipedItem);
            Arrow.DOKill();
            Arrow.DOScaleX(1f, 0.05f);
            Frame.DOKill();
            Frame.DOColor(new Color(1f, 1f, 1f, 1f), 0.1f);
        }
    }
    public void Release_DisplayCurrentlyEquippedItem() {
        UpdateDetails(item.isIdentified);
        Arrow.DOKill();
        Arrow.DOScaleX(0f, 0.05f);
        Frame.DOKill();
        Frame.DOColor(new Color(1f, 1f, 1f, 0f), 0.1f);
    }

    private void UpdateDetails(bool isIdentified, Item equipedItem) {
        itemName.text = equipedItem.Name;
        itemQuality.text = equipedItem.Quality.ToString();
        itemType.text = equipedItem.data.EquipType.ToString();

        if (equipedItem.data.EquipType == EquipmentType.Weapon) {
            itemType.text += " - " + ((ItemWeapon) equipedItem.data).WeaponType.ToString();
            coreStatValue.text = equipedItem.GetStats(CoreStats.Damage).ToString();
            coreStatDescriptor.text = "Damage";
        } else if (equipedItem.data.EquipType == EquipmentType.Artifact) {
            coreStatValue.text = ((ItemArtifact) equipedItem.data).GetSkillLevel(equipedItem.ItemLevel).ToString();
            coreStatDescriptor.text = "Skill";
        } else {
            coreStatValue.text = equipedItem.GetStats(CoreStats.Defense).ToString();
            coreStatDescriptor.text = "Defense";
        }

        questionMark.enabled = !isIdentified;

        string detailsStr = equipedItem.data.Description;

        if (!isIdentified) {
            questionMark.color = Color.white;

            string Q = "???";
            itemName.text = "Unknown " + equipedItem.Type;
            coreStatValue.text = Q;
            coreStatDescriptor.text = Q;
            detailsStr = Q;
        }

        detailsStr += "\n\n";

        List<AffixDisplayInformation> affixDisplayList = new List<AffixDisplayInformation>();
        foreach (ItemAffix affix in equipedItem.Affixes) {
            if (affix.isStatType(ItemModEffects.Core))
                foreach (CoreStats stat in System.Enum.GetValues(typeof(CoreStats))) {
                    if (affix.ContainsStat(stat)) {
                        AddToAffixList(affixDisplayList, stat, affix, equipedItem);
                    }
                }

            if (affix.isStatType(ItemModEffects.Primary))
                foreach (PrimaryStats stat in System.Enum.GetValues(typeof(PrimaryStats))) {
                    if (affix.ContainsStat(stat)) {
                        AddToAffixList(affixDisplayList, stat, affix, equipedItem);
                    }
                }

            if (affix.isStatType(ItemModEffects.Secondary))
                foreach (SecondaryStats stat in System.Enum.GetValues(typeof(SecondaryStats))) {
                    if (affix.ContainsStat(stat)) {
                        AddToAffixList(affixDisplayList, stat, affix, equipedItem);
                    }
                }
        }

        string test = " >>> [Cmp] Affix breakdown: [" + equipedItem.MongoID + "]\n";
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

        if (isIdentified) {
            foreach (AffixDisplayInformation affix in affixDisplayList) {
                detailsStr += affix.GetDisplayValue() + "\n";
            }
        } else {
            detailsStr += "???\n";
        }

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
    List<AffixDisplayInformation> AddToAffixList(List<AffixDisplayInformation> list, CoreStats stat, ItemAffix affix, Item item) {
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
    List<AffixDisplayInformation> AddToAffixList(List<AffixDisplayInformation> list, PrimaryStats stat, ItemAffix affix, Item item) {
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
    List<AffixDisplayInformation> AddToAffixList(List<AffixDisplayInformation> list, SecondaryStats stat, ItemAffix affix, Item item) {
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
        if (heroInterface == null) return;

        heroInterface.ClearStatDifferences();
    }

    void UpdateStats() {
        if (heroInterface == null) return;

        heroInterface.CalculateStatDifference(item);
    }
}
