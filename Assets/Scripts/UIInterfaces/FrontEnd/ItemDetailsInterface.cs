using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using ExtensionMethods;

public class ItemDetailsInterface : PanelWithGetters {
    public static bool KEEP_UNTIL_CONFIRMED = false;
    public static ItemDetailsInterface Instance;

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
    public Image revealFX;
    public GameObject wrapper;
    public Image modalShadow;

    public HorizontalLayoutGroup buttonsFooter;
    public HeroDetailsInterface heroDetails;

    public Button btnEnchant;

    public float identifyAnimTime = 1.0f;

    public Item item;
    Action<Item> _onEquipped;
    bool _isBusy = false;

    void Start() {
        Instance = this;

        revealFX.gameObject.SetActive(true);
        revealFX.color = new Color(1,1,1,0);
        wrapper.transform.localScale = Vector2.zero;
        wrapper.transform.DOScale(Vector2.one, 0.5f).SetEase(Ease.OutBack);
        modalShadow.color = new Color(0,0,0,0);
        modalShadow.DOFade(0.5f, 0.5f);

        heroDetails = GameObject.FindObjectOfType<HeroDetailsInterface>();
    }

    public override void CloseTransition() {
        DoClosingTransition(wrapper.transform, modalShadow);
    }

    public static ItemDetailsInterface Open(Item item, Action<Item> OnEquip=null) {
        ItemDetailsInterface inst = (ItemDetailsInterface)MenuManager.Instance.Push("Interface_ItemDetails");

        inst.LoadItem(item, OnEquip);

        return inst;
    }

    public Button AddButton(string label, Action callbackOnClick) {
        GameObject btnGO = MakeFromPrefab("SubItems/GeneralButton", buttonsFooter);
        Button btn = btnGO.GetComponent<Button>();
        TextMeshProUGUI btnLabel = btnGO.GetComponentInChildren<TextMeshProUGUI>();
        btnLabel.text = label;

        btn.transform.localScale = Vector3.one;

        btn.onClick.AddListener(new UnityAction(callbackOnClick));

        return btn;
    }

    public void LoadItem(Item data, Action<Item> onEquipped=null) {
        this.item = data;
        this._onEquipped = onEquipped;

        itemIcon.sprite = item.data.LoadSprite();// Resources.Load<Sprite>("Items/" + _data.data.EquipType + "/" + _data.Sprite);

        if (item.data.ClassRestriction != null) {
            requireText.text = "Restricted to " + item.data.ClassRestriction.ToString() + " class";
            RequireGO.SetActive(true);
        } else {
            RequireGO.SetActive(false);
        }

        //First time, apply the actual text, no matter what (to get the proper layout size)
        UpdateDetails(true);
        //this.SetEnabledInChildren<ContentSizeFitter>(true);

        // BUT if it's not identified, apply the "???" text (WAIT 1 FRAME, since ContentSizeFitters are problematic...):
        if(!item.isIdentified) {
            this.Wait(-1, () => {
                //this.SetEnabledInChildren<ContentSizeFitter>(false);
                UpdateDetails(false);
            });
        }
    }

    private void UpdateDetails(bool isIdentified) {
        itemName.text = item.Name;
        itemQuality.text = item.Quality.ToString();
        itemType.text = item.data.EquipType.ToString();

        if (item.data.EquipType == EquipmentType.Weapon) {
            itemType.text += " - " + ((ItemWeapon)item.data).WeaponType.ToString();
            coreStatValue.text = item.GetStats(CoreStats.Damage).ToString();// ((ItemWeapon) _data.data).Damage.ToString();
            coreStatDescriptor.text = "Damage";
        } else if (item.data.EquipType == EquipmentType.Artifact) {
            coreStatValue.text = ((ItemArtifact) item.data).GetSkillLevel(item.ItemLevel).ToString();
            coreStatDescriptor.text = "Skill";
        } else {
            coreStatValue.text = item.GetStats(CoreStats.Defense).ToString();// ((ItemArmor) _data.data).Defense.ToString();
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

        // Parse the affix list and group all the same affixes
        /*Dictionary<CoreStats, int> coreGroup = new Dictionary<CoreStats, int>();
        Dictionary<PrimaryStats, int> primaryGroup = new Dictionary<PrimaryStats, int>();
        Dictionary<SecondaryStats, int> secondaryGroup = new Dictionary<SecondaryStats, int>();
        foreach (ItemAffix affix in item.Affixes) {
            if (affix.isStatType(ItemModEffects.Core))
                foreach (CoreStats stat in System.Enum.GetValues(typeof(CoreStats))) {
                    if (affix.ContainsStat(stat)) {
                        if (coreGroup.ContainsKey(stat))
                            coreGroup[stat] += affix.GetValue(stat, item.ItemLevel, item.VarianceSeed);
                        else
                            coreGroup.Add(stat, affix.GetValue(stat, item.ItemLevel, item.VarianceSeed));
                    }
                }

            if (affix.isStatType(ItemModEffects.Primary))
                foreach (PrimaryStats stat in System.Enum.GetValues(typeof(PrimaryStats))) {
                    if (affix.ContainsStat(stat)) {
                        if (primaryGroup.ContainsKey(stat))
                            primaryGroup[stat] += affix.GetValue(stat, item.ItemLevel, item.VarianceSeed);
                        else
                            primaryGroup.Add(stat, affix.GetValue(stat, item.ItemLevel, item.VarianceSeed));
                    }
                }

            if (affix.isStatType(ItemModEffects.Secondary))
                foreach (SecondaryStats stat in System.Enum.GetValues(typeof(SecondaryStats))) {
                    if (affix.ContainsStat(stat)) {
                        if (secondaryGroup.ContainsKey(stat))
                            secondaryGroup[stat] += affix.GetValue(stat, item.ItemLevel, item.VarianceSeed);
                        else
                            secondaryGroup.Add(stat, affix.GetValue(stat, item.ItemLevel, item.VarianceSeed));
                    }
                }
        }*/

        // go through the affix lists and display them
        // TODO Take into consideration the + or %
        
        if (isIdentified) {
            foreach (AffixDisplayInformation affix in affixDisplayList) {
                detailsStr += affix.GetDisplayValue() + "\n";
            }
            /*foreach (CoreStats stat in coreGroup.Keys) {
                detailsStr += "+" + coreGroup[stat].ToString() + " " + stat.ToString() + "\n";
            }
            foreach (PrimaryStats stat in primaryGroup.Keys) {
                detailsStr += "+" + primaryGroup[stat].ToString() + " " + stat.ToString() + "\n";
            }
            foreach (SecondaryStats stat in secondaryGroup.Keys) {
                detailsStr += "+" + secondaryGroup[stat].ToString() + " " + stat.ToString() + "\n";
            }*/
        } else {
            detailsStr += "???\n";
        }
        /*foreach (ItemAffix affix in item.Affixes) {
            //print(affix.ToString());
            if (isIdentified) {
                foreach (CoreStats stat in System.Enum.GetValues(typeof(CoreStats))) {
                    if (affix.ContainsStat(stat))
                        detailsStr += affix.GetDisplayValue(stat, item.ItemLevel, item.VarianceSeed) + "\n";
                }
                foreach (PrimaryStats stat in System.Enum.GetValues(typeof(PrimaryStats))) {
                    if (affix.ContainsStat(stat))
                        detailsStr += affix.GetDisplayValue(stat, item.ItemLevel, item.VarianceSeed) + "\n";
                }
                foreach (SecondaryStats stat in System.Enum.GetValues(typeof(SecondaryStats))) {
                    if (affix.ContainsStat(stat))
                        detailsStr += affix.GetDisplayValue(stat, item.ItemLevel, item.VarianceSeed) + "\n";
                }
            } else {
                detailsStr += "???\n";
            }
        }*/

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

    public void Btn_Close() {
        Close();
        ClearStats();
    }

    public override bool Close() {
        if(_isBusy) return true;
        
        Instance = null;

        return base.Close();
    }

    void ClearStats() {
        if (heroDetails == null) return;

        heroDetails.ClearStatDifferences();
    }

    void UpdateStats() {
        if (heroDetails == null) return;

        heroDetails.CalculateStatDifference(item);
    }

    //////////////////////////////////// These are separately assigned "preset" button handlers, common, but not always used.

    public void Btn_Equip() {
        if (!KEEP_UNTIL_CONFIRMED && !Close()) return; //Prevents multiple clicks/taps

        if (_onEquipped == null) {
            Tracer.traceError("Can't call the *equip* callback, it is NULL.");
            return;
        }

        // Equip the item
        _onEquipped(item);
    }

    public void Btn_Sell() {
        if (!Close()) return; //Prevents multiple clicks/taps

        AudioManager.Instance.Play(SFX_UI.Coin);

        // Remove the item and add the value * marketfees (like 20% of buy value or something) of it to the currency
        int gold = item.SellValue; //Mathf.RoundToInt(item.Value * item.ItemLevel);
        Debug.Log("Sell " + item.data.Identity + " for " + gold + "[Needs to be implemented]");
        API.Shop.SellItems(item, CurrencyTypes.GOLD, gold)
            .Then(res => {
                trace("Btn_Sell OK");
                trace(res.pretty);
                RemoveFromInventory(item);
            })
            .Catch(err => traceError("Could not add gold from sold item: " + err.Message));
    }

    public void Btn_Shard() {
        if (!Close()) return; //Prevents multiple clicks/taps

        // TODO
        trace("TODO", "Remove the item and add the value * marketfees (like 20% of buy value or something) of it to the currency");

        Debug.Log("Shard Gear - [C: " + item.ConvertToCommonShards() +
                              "][M: " + item.ConvertToMagicShards() +
                              "][R: " + item.ConvertToRareShards() +
                              "][U: " + item.ConvertToUniqueShards() + "]");
        
        CurrencyManager.Cost shards = null;

        switch (item.Quality) {
            case ItemQuality.Common:
                shards = CurrencyTypes.SHARDS_ITEMS_COMMON.ToCostObj( item.ConvertToCommonShards() );
                break;
            case ItemQuality.Magic:
                shards = CurrencyTypes.SHARDS_ITEMS_MAGIC.ToCostObj(item.ConvertToMagicShards());
                break;
            case ItemQuality.Rare:
                shards = CurrencyTypes.SHARDS_ITEMS_RARE.ToCostObj(item.ConvertToRareShards());
                break;
            case ItemQuality.Unique:
                shards = CurrencyTypes.SHARDS_ITEMS_RARE.ToCostObj(item.ConvertToUniqueShards());
                break;
        }

        string shardInfo = shards.amount + " " + shards.type.ToString().Replace('_', ' ');
        string q = "Are you sure you\nwant to shard this\nitem?\n<size=+5><color=#a00>{0}</color></size>".Format2(shardInfo);

        ConfirmYesNoInterface.Ask("Confirm Shards", q)
            .Then(answer => {
                if(answer != "YES") return;

                AudioManager.Instance.Play(SFX_UI.ShardsChing);

                API.Currency.AddCurrency(shards)
                    .Then(res => API.Items.Remove(item))
                    .Then(res => RemoveFromInventory(item));
            });
    }

    void RemoveFromInventory(Item item) {
        DataManager.Instance.allItemsList.Remove(item);

        if(signals.OnItemRemoved!=null) signals.OnItemRemoved(item);

        ClearStats();

        //GameObject go = GameObject.Find("Interface_InventoryInterface");
        HeroInventoryInterface inventoryRef = GameObject.FindObjectOfType<HeroInventoryInterface>();

        if (inventoryRef == null) return;

        inventoryRef.GetComponent<HeroInventoryInterface>().RemoveItem(item);
    }

    public void Btn_Enchant() {
        EnchantInterface interf = (EnchantInterface) MenuManager.Instance.Push("Interface_Enchant");
        interf.Init(item, CallbackForEnchant);
    }

    void CallbackForEnchant(Item item) {
        print("Do we need to update the item here? probably");
    }
}

public class AffixDisplayInformation {
    public CoreStats? core = null;
    public PrimaryStats? primary = null;
    public SecondaryStats? secondary = null;

    public ItemModType type = ItemModType.Add;
    public float value = 0;

    public string GetDisplayValue() {
        string multiStr = "";
        string stat = "null";
        string displayValue = "";
        
        displayValue = Mathf.RoundToInt(value).ToString();

        if (type == ItemModType.Multi) {
            displayValue = Mathf.RoundToInt(value * 100f).ToString();
            multiStr = "%";
        }

        if (core != null)
            stat = core.Value.ToString();
        else if (primary != null)
            stat = primary.Value.ToString();
        else if (secondary != null) {
            if (secondary.Value == SecondaryStats.SkillLevel) {
                displayValue = Mathf.CeilToInt(value/100f).ToString();
                stat = secondary.Value.ToString();
            } else
                stat = secondary.Value.ToString();
        } else
            Debug.LogError("Display value error, a stat is not being set correctly in the [ItemDetailsInterface.cs] script");

        return "+" + displayValue + multiStr + " " + stat;
    }
}