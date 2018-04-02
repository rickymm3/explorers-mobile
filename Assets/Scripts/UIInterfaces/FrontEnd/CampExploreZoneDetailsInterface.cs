using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CampExploreZoneDetailsInterface : MonoBehaviour {
    
    public TextMeshProUGUI ZoneName;
    public TextMeshProUGUI ZoneDescription;

    [Header("Item Drop Type Details")]
    public PossibleZoneLoot ZoneLootArea;
    public PossibleZoneLoot UniqueLootArea;

    [Header("Boss Details")]
    public List<ZoneBossDetails> BossList = new List<ZoneBossDetails>();

    [Header("Chest Details")]
    public TextMeshProUGUI magicChest;
    public TextMeshProUGUI rareChest;
    public TextMeshProUGUI legendaryChest;

    ZoneData zone;
    List<LootCrate> lootCrates = new List<LootCrate>();

    public void Initialize(ZoneData zone) {
        this.zone = zone;
        ZoneName.text = "Zone " + zone.Zone + " - " + zone.Name;
        ZoneDescription.text = zone.Description;
        //BossPortrait.sprite = zone.BossFight.monsters[0].LoadPortrait();
        ZoneLootArea.Initialize(zone);
        UniqueLootArea.Initialize(zone);

        for (int i = 0; i < BossList.Count; i++) {
            if (i < zone.BossFight.monsters.Count && zone.BossFight.monsters[i] != null) {
                BossList[i].gameObject.SetActive(true);
                BossList[i].Initialize(zone.BossFight.monsters[i]);
            } else
                BossList[i].gameObject.SetActive(false);
        }

        string temp = "Crates:\n";
        foreach (var item in lootCrates) {
            temp += " > " + item.lootTableIdentity + " | " + item.CrateType.Identity + "\n";
        }

        UpdateChestText();
    }

    void UpdateChestText() {
        UpdateCrates();

        magicChest.text = "x" + lootCrates.FindAll(crate => crate.CrateType.Identity == "ct_magic").Count;
        rareChest.text = "x" + lootCrates.FindAll(crate => crate.CrateType.Identity == "ct_rare").Count;
        legendaryChest.text = "x" + lootCrates.FindAll(crate => crate.CrateType.Identity == "ct_legendary").Count;
    }

    void UpdateCrates() {
        lootCrates.Clear();
        lootCrates = DataManager.Instance.GetLootCratesByLootTableIdentity(zone.LootTable.Identity);
        lootCrates.AddRange(DataManager.Instance.GetLootCratesByLootTableIdentity(zone.BossFight.lootTable.Identity));
    }

    public void BtnOpenChest(string crateType) {
        switch(crateType.Trim().ToLower()) {
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

        LootCrateInterface lci = (LootCrateInterface) MenuManager.Instance.Push("Interface_LootCrate");
        lci.Initialize(crate, UpdateChestText);
    }
}

public enum CrateType { Magic, Rare, Legendary }
