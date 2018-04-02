using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BattleCratesInterface : PanelWithGetters {

    [Header("Chest Counts")]
    public TextMeshProUGUI magicChest;
    public TextMeshProUGUI rareChest;
    public TextMeshProUGUI legendaryChest;

    [Header("shadow")]
    public Image shadow;

    ZoneData zone;
    List<LootCrate> lootCrates = new List<LootCrate>();
    bool forceClose = false;
    BattleResultsInterface brInterface;
    
    public void Initialize(List<LootCrate> lootCrates, BattleResultsInterface brInterface) {
        this.lootCrates = lootCrates;
        this.brInterface = brInterface;

        Debug.Log("[Dropped Crates] Crate Count: " + lootCrates.Count);

        shadow.DOColor(new Color(0f, 0f, 0f, 0.75f), 0.5f);
        GetComponent<RectTransform>().DOScale(1f, 0.5f).SetEase(Ease.OutBack);
        
        UpdateChestText();
    }

    void UpdateChestText() {
        magicChest.text = "x" + lootCrates.FindAll(crate => crate.CrateType.Identity == "ct_magic").Count;
        rareChest.text = "x" + lootCrates.FindAll(crate => crate.CrateType.Identity == "ct_rare").Count;
        legendaryChest.text = "x" + lootCrates.FindAll(crate => crate.CrateType.Identity == "ct_legendary").Count;
    }

    void checkToCloseResults() {
        if (forceClose) {
            brInterface.BtnExitBattle();
        }
    }
    
    public void BtnOpenChest(string crateType) {
        switch (crateType.Trim().ToLower()) {
            case "magic":
                OpenChest("ct_magic");
                break;
            case "rare":
                OpenChest("ct_rare");
                break;
            case "legendary":
                OpenChest("ct_legendary");
                break;
            default:
                Debug.Log("Invalid Chest Type on UI Button - passed: " + crateType);
                break;
        }
    }

    void OpenChest(string type) {
        if (lootCrates.Count <= 0) return;

        LootCrate crate = lootCrates.Find(lc => lc.CrateType.Identity == type);

        if (crate == null) {
            Debug.LogError("Error: How is the loot crate even null here?");
            return;
        }

        if (lootCrates.Count <= 1) forceClose = true;

        LootCrateInterface lci = (LootCrateInterface) MenuManager.Instance.Push("Interface_LootCrate");

        lootCrates.Remove(crate);
        lci.Initialize(crate, UpdateChestText, checkToCloseResults);
    }
}
