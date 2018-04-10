using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using ExtensionMethods;

public class CampEnchantingInterface : MonoBehaviour {
    
    public EnchantingInventory inventory;
    public EnchantingItemDetails itemDetails;
    Item selectedItem = null;

    [Header("Enchanting Buttons")]
    public Button BtnPower;
    public Button BtnAffix;
    public TextMeshProUGUI PowerTitle;
    public TextMeshProUGUI PowerDescription;
    public TextMeshProUGUI AffixTitle;
    public TextMeshProUGUI AffixDescription;
    public List<EnchantingCostIcon> PowerLabels = new List<EnchantingCostIcon>();
    public List<EnchantingCostIcon> AffixLabels = new List<EnchantingCostIcon>();
    public Image FadeArea;

    void Start() {
        Initialize();
    }

    public void Initialize() {
        itemDetails.Hide();
        inventory.Btn_Hide();

        BtnPower.interactable = false;
        BtnAffix.interactable = false;
        SetEnchantButtonTextColors(new Color(0.5f, 0.5f, 0.5f, 1f));

        foreach (EnchantingCostIcon icon in PowerLabels) {
            icon.ShowNoCost();
        }
        foreach (EnchantingCostIcon icon in AffixLabels) {
            icon.ShowNoCost();
        }

        inventory.LoadInventory(SelectItem);
    }

	public void BtnSelectItem() {
        selectedItem = null;
        itemDetails.Hide();
        inventory.LoadInventory(SelectItem);
        inventory.Show();
    }

    void SetEnchantButtonTextColors(Color color) {
        PowerTitle.color = color;
        PowerDescription.color = color;
        AffixTitle.color = color;
        AffixDescription.color = color;
        /*CommonCostPower.color = color;
        HigherCostPower.color = color;
        CommonCostAffix.color = color;
        HigherCostAffix.color = color;*/

        foreach (EnchantingCostIcon icon in PowerLabels) {
            icon.SetColor(color);
        }
        foreach (EnchantingCostIcon icon in AffixLabels) {
            icon.SetColor(color);
        }
    }

    void SelectItem(Item item) {
        selectedItem = item;
        inventory.Btn_Hide();
        itemDetails.LoadItem(item);

        if (!item.isIdentified) {
            BtnPower.interactable = false;
            BtnAffix.interactable = false;
            SetEnchantButtonTextColors(new Color(0.5f, 0.5f, 0.5f, 1f));
        } else {
            // Set the enchanting prices based on the rarity of the items
            // globals?
            BtnPower.interactable = true;
            BtnAffix.interactable = true;
            SetEnchantButtonTextColors(Color.white);

            int commonCount = CurrencyTypes.SHARDS_ITEMS_COMMON.GetAmount();
            int magicCount = CurrencyTypes.SHARDS_ITEMS_MAGIC.GetAmount();
            int rareCount = CurrencyTypes.SHARDS_ITEMS_RARE.GetAmount();

            CurrencyManager.Cost powerCost = GetEnchantValue();
            CurrencyManager.Cost affixCost = GetEnchantValue(true);

            if (powerCost.Count > 0) {
                int index = 0;
                foreach (CurrencyTypes type in powerCost.Keys) {
                    PowerLabels[index].gameObject.SetActive(true);
                    PowerLabels[index].Init(type.GetAmount() + "/" + -powerCost[type], GetShardSprite(type));

                    if (type.GetAmount() < powerCost[type])
                        PowerLabels[index].SetColor(Color.red);

                    index++;
                }
                index = 0;
                foreach (CurrencyTypes type in affixCost.Keys) {
                    AffixLabels[index].Init(type.GetAmount() + "/" + -affixCost[type], GetShardSprite(type));

                    if (type.GetAmount() < affixCost[type])
                        AffixLabels[index].SetColor(Color.red);

                    index++;
                }

                for (index = powerCost.Count; index < PowerLabels.Count; index++) {
                    PowerLabels[index].gameObject.SetActive(false);
                }
                for (index = affixCost.Count; index < AffixLabels.Count; index++) {
                    AffixLabels[index].gameObject.SetActive(false);
                }
            } else {
                BtnPower.interactable = false;
                BtnAffix.interactable = false;
                SetEnchantButtonTextColors(new Color(0.5f, 0.5f, 0.5f, 1f));

                foreach (EnchantingCostIcon icon in PowerLabels) {
                    icon.ShowNoCost();
                }
                foreach (EnchantingCostIcon icon in AffixLabels) {
                    icon.ShowNoCost();
                }
            }
        }
    }

    Sprite GetShardSprite(CurrencyTypes type) {
        switch (type) {
            case CurrencyTypes.SHARDS_ITEMS_MAGIC:
                return Resources.Load<Sprite>("Items/Essences/shard_magic");
            case CurrencyTypes.SHARDS_ITEMS_RARE:
                return Resources.Load<Sprite>("Items/Essences/shard_rare");
            case CurrencyTypes.SHARDS_ITEMS_COMMON:
            default:
                return Resources.Load<Sprite>("Items/Essences/shard_common");
        }
    }

    public void BtnShardItem() {
        if (selectedItem == null) return;

        //selectedItem.ConvertToCommonShards();
        Debug.LogError(" --- Shard Item here");

        CurrencyTypes type = selectedItem.GetDisenchantType();
        int value = selectedItem.GetDisenchantValue(type);

        CurrencyManager.Cost shards = type.ToCostObj(value);

        string shardInfo = shards.amount + " " + shards.type.ToString().Replace('_', ' ');
        string q = "Are you sure you\nwant to shard this\nitem?";

        ConfirmYesNoInterface.Ask("Confirm Shards", q)
            .Then(answer => {
                if (answer != "YES") return;

                AudioManager.Instance.Play(SFX_UI.ShardsChing);

                DataManager.API.Currency.AddCurrency(shards)
                    .Then(res => DataManager.API.Items.Remove(selectedItem))
                    .Then(res => RemoveFromInventory(selectedItem));

                itemDetails.Hide();
            });

    }

    void RemoveFromInventory(Item item) {
        DataManager.Instance.allItemsList.Remove(item);

        if (DataManager.signals.OnItemRemoved != null) DataManager.signals.OnItemRemoved(item);
        
        inventory.LoadInventory(SelectItem);
    }

    public void BtnPowerChange() {
        if (selectedItem == null) return;

        CurrencyManager.Cost cost = GetEnchantValue();

        if (HasUnsufficientShards(cost)) {
            ConfirmYesNoInterface.Ask("Confirm Shards", "Are you sure you\nwant to adjust the\n power of this item?<color=#ff5555>Can't be undone.</color>")
                .Then(answer => {
                    if (answer != "YES") return;

                    AudioManager.Instance.Play(SFX_UI.ShardsChing);
                    
                    StartCoroutine(FadeToWhite());

                    DataManager.API.Currency.AddCurrency(cost)
                        .Then(res => {
                            selectedItem.RerollItemLevel();
                            OnRerollSuccess();
                        })
                        .Catch(err => { Debug.LogError("Could not spend Shards in PowerChange: " + err); });
                });
            
        } else {
            AudioManager.Instance.Play(SFX_UI.Invalid);
        }
    }

    public void BtnAffixChange() {
        if (selectedItem == null) return;

        CurrencyManager.Cost cost = GetEnchantValue(true);

        if (HasUnsufficientShards(cost, true)) {
            ConfirmYesNoInterface.Ask("Confirm Shards", "Are you sure you\nwant to adjust the\n affixes of this item?\n<color=#ff5555>This will alter the\naffixes and can't be\n undone.</color>")
                .Then(answer => {
                    if (answer != "YES") return;

                    AudioManager.Instance.Play(SFX_UI.ShardsChing);

                    StartCoroutine(FadeToWhite());

                    DataManager.API.Currency.AddCurrency(cost)
                        .Then(res => {
                            selectedItem.RerollAffixes();
                            OnRerollSuccess();
                        })
                        .Catch(err => { Debug.LogError("Could not spend Shards in AffixChange: " + err); });
                });

            
        } else {
            AudioManager.Instance.Play(SFX_UI.Invalid);
        }
    }

    IEnumerator FadeToWhite() {
        yield return FadeArea.DOFade(1f, 0.25f).WaitForCompletion();
        yield return new WaitForSeconds(0.75f);
        yield return FadeArea.DOFade(0f, 0.25f).WaitForCompletion();
    }

    // The costs for enchanting are determined here, you can change these
    CurrencyManager.Cost GetEnchantValue(bool forAffix = false) {
        CurrencyManager.Cost shards = new CurrencyManager.Cost();

        switch (selectedItem.Quality) {
            case ItemQuality.Magic:
                if (forAffix) {
                    // 7 common, 2 magic
                    shards.AddOrSet(CurrencyTypes.SHARDS_ITEMS_COMMON, -7);
                    shards.AddOrSet(CurrencyTypes.SHARDS_ITEMS_MAGIC, -2);
                } else {
                    // 5 common, 1 magic
                    shards.AddOrSet(CurrencyTypes.SHARDS_ITEMS_COMMON, -5);
                    shards.AddOrSet(CurrencyTypes.SHARDS_ITEMS_MAGIC, -1);
                }
                return shards;
            case ItemQuality.Rare:
                if (forAffix) {
                    // 7 common, 3 magic
                    shards.AddOrSet(CurrencyTypes.SHARDS_ITEMS_COMMON, -7);
                    shards.AddOrSet(CurrencyTypes.SHARDS_ITEMS_MAGIC, -3);
                } else {
                    // 5 common, 1 rare
                    shards.AddOrSet(CurrencyTypes.SHARDS_ITEMS_COMMON, -5);
                    shards.AddOrSet(CurrencyTypes.SHARDS_ITEMS_RARE, -1);
                }
                return shards;
            case ItemQuality.Unique:
                if (forAffix) {
                    // 13 common, 5 rare
                    shards.AddOrSet(CurrencyTypes.SHARDS_ITEMS_COMMON, -13);
                    shards.AddOrSet(CurrencyTypes.SHARDS_ITEMS_RARE, -3);
                } else {
                    // 10 common, 3 rare
                    shards.AddOrSet(CurrencyTypes.SHARDS_ITEMS_COMMON, -10);
                    shards.AddOrSet(CurrencyTypes.SHARDS_ITEMS_RARE, -3);
                }
                return shards;
            case ItemQuality.Common:
            default:
                return shards;
        }
    }

    bool HasUnsufficientShards(CurrencyManager.Cost cost, bool affixCost = false) {
        if (cost.Count <= 0) return false;

        foreach(CurrencyTypes type in cost.Keys) {
            if (cost[type] > type.GetAmount())
                return false;
        }

        return true;
    }
    
    void OnRerollSuccess() {
        Debug.Log("Update Item here!");

        SelectItem(selectedItem);
    }
}
