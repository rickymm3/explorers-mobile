using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using ExtensionMethods;

public class HeroDetailsInterface : PanelWithGetters {
    Hero hero;
    [HideInInspector] public EquipedItemsDictionary Equipped { get { return hero.EquipedItems; } }

    public Image HeroImage;
    public Image HeroPortrait;
    public TextMeshProUGUI HeroName;
    public TextMeshProUGUI HeroQuality;
    public TextMeshProUGUI HeroLevel;

    [Header("Experience Area")]
    public TextMeshProUGUI ExperienceText;
    public Image ExperienceBar;

    [Header("Skill Area")]
    public List<GameObject> Skills = new List<GameObject>();
    public GameObject TapSkill;
    public GameObject LeaderSkillDisplay;

    [Header("Core Stats")]
    public TextMeshProUGUI StatHealth;
    public TextMeshProUGUI StatDamage;
    public TextMeshProUGUI StatDefense;

    [Header("Primary Stats")]
    public TextMeshProUGUI StatStrength;
    public TextMeshProUGUI StatVitality;
    public TextMeshProUGUI StatIntelligence;
    public TextMeshProUGUI StatSpeed;

    [Header("Main Stat Highlight")]
    public GameObject strengthStatHL;
    public GameObject vitalityStatHL;
    public GameObject intelligenceStatHL;
    public GameObject speedStatHL;

    [Header("Secondary Stats")]
    public TextMeshProUGUI StatCritChance;
    public TextMeshProUGUI StatCritDamage;
    public TextMeshProUGUI StatMagicFind;
    public TextMeshProUGUI StatMonsterFind;
    public TextMeshProUGUI StatTreasureFind;
    public TextMeshProUGUI StatSkillIncrease;
    public TextMeshProUGUI StatSkillCooldown;
    public TextMeshProUGUI StatCounterChance;
    public TextMeshProUGUI StatThorns;

    [Header("Button References")]
    public GameObject UpgradeButton;
    public GameObject RetireButton;
    public Button CloseButton;

    [Header("Equiped Items")][Inspectionary]
    public EquipmentSlotDictionary EquipmentSlots = new EquipmentSlotDictionary();

    [Header("Equipment Selected Indicators")]
    public Image HelmSelector;
    public Image ChestSelector;
    public Image GlovesSelector;
    public Image BootsSelector;
    public Image ArtifactSelector;
    public Image WeaponSelector;

    [Header("Color Reference")]
    public Color PositiveColor;
    public Color NegativeColor;


    [Header("Stat Update TextAreas")]
    [Inspectionary("Core Stat", "Text Field")]
    public CoreStatsUpdatesDictionary CoreStatsUpdates = new CoreStatsUpdatesDictionary();
    [Inspectionary("Core Stat", "Text Field")]
    public PrimaryStatsUpdatesDictionary PrimaryStatsUpdates = new PrimaryStatsUpdatesDictionary();
    [Inspectionary("Core Stat", "Text Field")]
    public SecondaryStatsUpdatesDictionary SecondaryStatsUpdates = new SecondaryStatsUpdatesDictionary();

    [Header("Inventory")]
    public HeroInventoryInterface InventoryScreen;
    public ItemDetailsOnHeroScreenInterface ItemDetailsScreen;
    public ItemComparesOnHeroScreenInterface ItemCompareScreen;

    int heroReferenceIndex = 0;
    public CampHeroInterface campHeroInterface;

    int targetIndex = -1;
    GameObject heroDisplay = null;

    public void Initialize(Hero hero, CampHeroInterface campHeroInterface, Button btnBack = null) {
        this.hero = hero;
        this.campHeroInterface = campHeroInterface;
        HeroName.text = this.hero.Name;
        HeroLevel.text = "LVL " + hero.Level.ToString() + " " + hero.personality.ToString() + " " + hero.data.Class;
        HeroQuality.text = "";
        HeroPortrait.sprite = hero.LoadPortraitSprite();
        //HeroQuality.text = actor.quality.ToString();

        ExperienceText.text = hero.GetExperienceThisLevel() + " / " + hero.GetNextXPBasedOnLevel();
        ExperienceBar.transform.DOScaleX(hero.ExperienceProgress(), 0f);

        strengthStatHL.SetActive(false);
        vitalityStatHL.SetActive(false);
        intelligenceStatHL.SetActive(false);
        speedStatHL.SetActive(false);

        switch (hero.data.Class) {
            case HeroClass.Assassin:
                speedStatHL.SetActive(true);
                break;
            case HeroClass.Bruiser:
                strengthStatHL.SetActive(true);
                break;
            case HeroClass.Tank:
                vitalityStatHL.SetActive(true);
                break;
            case HeroClass.Mage:
                intelligenceStatHL.SetActive(true);
                break;
        }

        // Skills
        foreach(GameObject goSkill in Skills) {
            goSkill.SetActive(false);
        }
        for (int i = 0; i < hero.Skills.Count; i++) {
            Skills[i].SetActive(true);
            Skills[i].GetComponent<SkillDisplayHandler>().Initialize(hero.Skills[i], hero);
        }

        TapSkill.GetComponent<TapSkillDisplayHandler>().Initialize(hero.tapSkill);
        if (hero.data.LeadershipSkill != null)
            LeaderSkillDisplay.GetComponent<LeaderSkillDisplayHandler>().Initialize(hero.data.LeadershipSkill);
        else
            LeaderSkillDisplay.SetActive(false);


        StatHealth.text = hero.GetCoreStat(CoreStats.Health).ToString();
        StatDamage.text = hero.GetCoreStat(CoreStats.Damage).ToString();
        StatDefense.text = hero.GetCoreStat(CoreStats.Defense).ToString();

        StatStrength.text = hero.GetPrimaryStat(PrimaryStats.Strength).ToString();
        StatVitality.text = hero.GetPrimaryStat(PrimaryStats.Vitality).ToString();
        StatIntelligence.text = hero.GetPrimaryStat(PrimaryStats.Intelligence).ToString();
        StatSpeed.text = hero.GetPrimaryStat(PrimaryStats.Speed).ToString();

        StatCritChance.text = hero.GetSecondaryStat(SecondaryStats.CriticalChance).ToString();
        StatCritDamage.text = hero.GetSecondaryStat(SecondaryStats.CriticalDamage).ToString();
        StatMagicFind.text = hero.GetSecondaryStat(SecondaryStats.MagicFind).ToString();
        StatMonsterFind.text = hero.GetSecondaryStat(SecondaryStats.MonsterFind).ToString();
        StatTreasureFind.text = hero.GetSecondaryStat(SecondaryStats.TreasureFind).ToString();
        //StatSkillIncrease.text = hero.GetSecondaryStat(SecondaryStats.SkillIncrease).ToString();
        //StatSkillCooldown.text = hero.GetSecondaryStat(SecondaryStats.CooldownReduction).ToString();
        StatCounterChance.text = hero.GetSecondaryStat(SecondaryStats.CounterChance).ToString();
        StatThorns.text = hero.GetSecondaryStat(SecondaryStats.Dodge).ToString();

        // TODO Equiped 
        foreach (EquipmentType type in EquipmentSlots.Keys) {
            if (!hero.EquipedItems.ContainsKey(type)) continue;

            Item equippedItem = hero.EquipedItems[type];
            Image equippedSlot = EquipmentSlots[type];

            equippedSlot.sprite = equippedItem.data.LoadSprite(); // <--- Extracted the "Resource.Load..." to a reusable method

            // TODO Handle unequip later
            equippedSlot.color = new Color(1f, 1f, 1f, 1f);
        }
        
        heroReferenceIndex = DataManager.Instance.allHeroesList.IndexOf(hero);

        if (heroDisplay != null) Destroy(heroDisplay);
        heroDisplay = Instantiate(hero.LoadUIAnimationReference());
        RectTransform rect = heroDisplay.GetRect();
        rect.SetParent(HeroImage.gameObject.transform);
        rect.anchoredPosition = new Vector2(0, 675f + rect.anchoredPosition.y);
        rect.localScale = Vector3.one * 3f;
        //HeroImage.sprite = Resources.Load<Sprite>("Hero/" + hero.data.Identity.ToLower() + "/fullbody");

        ClearStatDifferences();

        /*if (!hero.isMaxLevel())
            UpgradeButton.GetComponent<Button>().interactable = false;*/


        DeselectEquipSlot();


        if (PlayerPrefs.GetInt("tutorial_step") == 4) {
            // Disable the summon backgound lockout
            StoryManager.Instance.DisplayStory("story_tutorial_herodetails_details");

            // Lock Summon back until summon complete
            if (btnBack != null) btnBack.interactable = true;
            CloseButton.interactable = false;

            PlayerPrefs.SetInt("tutorial_step", 5);
        }
    }

    void Update() {
        // Check for swipe here
        // if left swipe
        /*targetIndex = campHeroInterface.SelectedHeroID - 1;
        if (WithinHeroListBounds(targetIndex))
            Initialize(campHeroInterface.heroes[targetIndex], campHeroInterface);

        //if right swipe
        targetIndex = campHeroInterface.SelectedHeroID + 1;
        if (WithinHeroListBounds(targetIndex))
            Initialize(campHeroInterface.heroes[targetIndex], campHeroInterface);*/
    }

    bool WithinHeroListBounds(int index) {
        if (index >= 0 && index < campHeroInterface.heroes.Count)
            return true;
        return false;
    }

    public void ClearStatDifferences() {
        foreach (CoreStats stat in CoreStatsUpdates.Keys) {
            CoreStatsUpdates[stat].text = "";
        }
        foreach (PrimaryStats stat in PrimaryStatsUpdates.Keys) {
            PrimaryStatsUpdates[stat].text = "";
        }
        foreach (SecondaryStats stat in SecondaryStatsUpdates.Keys) {
            SecondaryStatsUpdates[stat].text = "";
        }
    }

    public void CalculateStatDifference(Item item) {
        //Debug.Log("In Calc Stat Diff");

        //Removed from arguments above, probably never will need to specify this separately.
        EquipmentType type = item.data.EquipType;

        if (hero.EquipedItems.ContainsKey(type)) {
            Item currentEquiped = hero.EquipedItems[type];

            // Item exists so compare
            foreach (CoreStats stat in System.Enum.GetValues(typeof(CoreStats))) {
                UpdateStatValue(stat, currentEquiped.GetStatDifference(stat, item, hero));
            }
            foreach (PrimaryStats stat in System.Enum.GetValues(typeof(PrimaryStats))) {
                UpdateStatValue(stat, currentEquiped.GetStatDifference(stat, item, hero));
            }
            foreach (SecondaryStats stat in System.Enum.GetValues(typeof(SecondaryStats))) {
                UpdateStatValue(stat, currentEquiped.GetStatDifference(stat, item, hero));
            }
        } else {
            int difference = 0;
            foreach (CoreStats stat in System.Enum.GetValues(typeof(CoreStats))) {
                // Calculate the new item effect and get the difference
                difference = Mathf.RoundToInt((((hero.GetCoreStat(stat) / hero.GetStatMultiplier(stat)) + item.GetStats(stat)) * (hero.GetStatMultiplier(stat) + item.GetMultiplier(stat))) - hero.GetCoreStat(stat));
                UpdateStatValue(stat, difference);
            }
            foreach (PrimaryStats stat in System.Enum.GetValues(typeof(PrimaryStats))) {
                // Calculate the new item effect and get the difference
                difference = Mathf.RoundToInt((((hero.GetPrimaryStat(stat) / hero.GetStatMultiplier(stat)) + item.GetStats(stat)) * (hero.GetStatMultiplier(stat) + item.GetMultiplier(stat))) - hero.GetPrimaryStat(stat));
                UpdateStatValue(stat, difference);
            }
            foreach (SecondaryStats stat in System.Enum.GetValues(typeof(SecondaryStats))) {
                // Calculate the new item effect and get the difference
                difference = Mathf.RoundToInt((((hero.GetSecondaryStat(stat) / hero.GetStatMultiplier(stat)) + item.GetStats(stat)) * (hero.GetStatMultiplier(stat) + item.GetMultiplier(stat))) - hero.GetSecondaryStat(stat));
                UpdateStatValue(stat, difference);
            }
        }
    }

    void UpdateStatValue(SecondaryStats stat, int difference) {
        //print(stat.ToString() + " [" + difference + "]");
        if (!SecondaryStatsUpdates.ContainsKey(stat)) {
            print("No UI Text Element for stat: " + stat);
            return;
        }
        UpdateStatValue(SecondaryStatsUpdates[stat], difference);
    }
    void UpdateStatValue(PrimaryStats stat, int difference) {
        //print(stat.ToString() + " [" + difference + "]");
        if (!PrimaryStatsUpdates.ContainsKey(stat)) {
            print("Error: No UI Text Element for stat: " + stat);
            return;
        }
        UpdateStatValue(PrimaryStatsUpdates[stat], difference);
    }
    void UpdateStatValue(CoreStats stat, int difference) {
        //print(stat.ToString() + " [" + difference + "]");
        if (!CoreStatsUpdates.ContainsKey(stat)) {
            print("Error: No UI Text Element for stat: " + stat);
            return;
        }
        UpdateStatValue(CoreStatsUpdates[stat], difference);
    }

    void UpdateStatValue(TextMeshProUGUI statsText, int difference) {
        if (difference > 0) {
            statsText.text = "+" + difference;
            statsText.color = PositiveColor;
        } else if (difference < 0) {
            statsText.text = difference.ToString();
            statsText.color = NegativeColor;
        } else {
            statsText.text = "";
        }
    }

    public void Btn_CloseUI() {
        AudioManager.Instance.Play(SFX_UI.PageFlip);

        MenuManager.Instance.Pop();
    }

    public void Btn_Upgrade() {
        print("Upgrade Button Hit");
        // Pop the UI to show cost of upgrade with this hero reference
        // Do we have 2 UI interfaces for this? also does the button change when it's an awakening?
        UpgradeHeroInterface ui = (UpgradeHeroInterface) menuMan.Push("Interface_UpgradeHero");
        ui.Initialize(hero, this);
    }

    public void Btn_Retire() {
        print("Retire Button Hit");
        HeroRetireRewardsInterface ui = (HeroRetireRewardsInterface) menuMan.Push("Interface_HeroRetired");
        ui.Initialize(hero, () => { campHeroInterface.PopulateHeroList(); Btn_CloseUI(); });
    }

    public void DeselectEquipSlot(float delay = 0.1f) {
        HelmSelector.GetRect().DOScaleX(0f, delay);
        ChestSelector.GetRect().DOScaleX(0f, delay);
        GlovesSelector.GetRect().DOScaleX(0f, delay);
        BootsSelector.GetRect().DOScaleX(0f, delay);
        ArtifactSelector.GetRect().DOScaleX(0f, delay);
        WeaponSelector.GetRect().DOScaleX(0f, delay);
    }

    public void Btn_Equip(string typeStr) {
        EquipmentType type = typeStr.AsEnum<EquipmentType>();

        // Close Equipment after inventory panel
        ItemCompareScreen.BtnHide();

        DeselectEquipSlot();

        switch (type) {
            case EquipmentType.Helm:
                HelmSelector.GetRect().DOScaleX(1f, 0.2f);
                break;
            case EquipmentType.Chest:
                ChestSelector.GetRect().DOScaleX(1f, 0.2f);
                break;
            case EquipmentType.Gloves:
                GlovesSelector.GetRect().DOScaleX(1f, 0.2f);
                break;
            case EquipmentType.Boots:
                BootsSelector.GetRect().DOScaleX(1f, 0.2f);
                break;
            case EquipmentType.Artifact:
                ArtifactSelector.GetRect().DOScaleX(1f, 0.2f);
                break;
            case EquipmentType.Weapon:
                WeaponSelector.GetRect().DOScaleX(1f, 0.2f);
                break;
        }

        // Keep the [Equip | Sell] popup around until the player finished confirming:
        ItemDetailsInterface.KEEP_UNTIL_CONFIRMED = Equipped.ContainsKey(type);

        // Load Inventory for Item Type
        if (!Equipped.ContainsKey(type))
            ItemDetailsScreen.ShowItem(null, type, this, TryEquip);
        else
            ItemDetailsScreen.ShowItem(Equipped[type], type, this, TryEquip);

        InventoryScreen.LoadInventory(type, TryEquip, this);
    }

    void TryEquip(Item item) {
        if(item.heroID > 0) {
            traceError("Item {0} is already equipped by Hero #{1}, unequip it first.".Format2(item.DebugID, item.heroID));
            return;
        }

        EquipmentType type = item.data.EquipType;
        
        if (!Equipped.ContainsKey(type)) {
            Equip(item);
            return;
        }

        //If we already have an item on this EquipType slot, unequip it first:
        Item itemPrevious = Equipped[type];
        if (itemPrevious.MongoID < 1) {
            trace(itemPrevious.stackTrace);
            throw new Exception("Previous equipped item doesn't have a valid MongoID: " + itemPrevious.DebugID);
        }

        var cost = CurrencyManager.ParseToCost( globals.GetGlobalAsString(GlobalProps.UNEQUIP_FEE) );
        cost.amount = Mathf.RoundToInt(cost.amount *  itemPrevious.ItemLevel);

        string question =
            "What do you wish to do\n" +
            "with your previous item?\n\n" +
            "<color=#fff><size=+8>{0}</size></color>\n\n" +
            "<color=#fff>Unbind:</color> Keep it for <color=#fdfc93>{1} {2}</color>\n" +
            "<color=#fff>Discard:</color> Lose it and replace\nwith new selection.";

        string[] choices = new string[] { "*CANCEL", "DISCARD", "UNBIND" };

        string itemName = SplitIfTooLong(itemPrevious.Name);

        ConfirmYesNoInterface.Ask("Unbinding Fee", question.Format2(itemName, cost.amount, cost.type), choices)
            .Then(answer => {
                switch (answer) {
                    case "UNBIND": SwapItems(itemPrevious, item, cost); break;
                    case "DISCARD": DiscardAndEquip(itemPrevious, item); break;
                }

                ItemDetailsInterface.Instance.Close();
            })
            .Catch(err=> {
                trace("Cancelled...");
            });
    }

    static string SplitIfTooLong(string name) {
        string[] nameSplit = name.Split(' ');

        if (nameSplit.Length<3) {
            return name;
        }

        return nameSplit.Slice(0, 1).Join(" ") + "\n" + nameSplit.Slice(2).Join(" ");
    }

    void DiscardAndEquip(Item itemPrevious, Item item) {
        int id = itemPrevious.MongoID;
        API.Items.Remove(itemPrevious)
            .Then(res => {
                audioMan.Play(SFX_UI.Explosion);
                dataMan.allItemsList.RemoveAll(i => i.MongoID == id);
                Equipped.Remove(item.data.EquipType);

                CloseButton.interactable = true;

                Equip(item);
            })
            .Catch(err => {
                trace("Could not remove previous item: " + itemPrevious.DebugID);
            });
    }

    void SwapItems(Item itemPrevious, Item item, CurrencyManager.Cost cost) {
        API.Items.Unequip(itemPrevious, cost.type, cost.amount)
            .Then(res => {
                audioMan.Play(SFX_UI.Coin);
                trace("Successfully unequipped before swapping with item " + item.DebugID);
                itemPrevious.heroID = 0;
                Equipped.Remove(item.data.EquipType);

                CloseButton.interactable = true;

                Equip(item);
            })
            .Catch(err => {
                traceError("Error unequipping previously equipped Item {0}: {1}".Format2(itemPrevious.DebugID, err.Message));
            });
    }

    void Equip(Item item) {
        API.Items.EquipToHero(item, hero)
            .Then(res => OnEquipComplete(item))
            .Catch(err => {
                TimelineTween.ShakeError(this.gameObject);
                traceError("Could not equip the item to hero: " + item.DebugID + " : " + hero.DebugID + "\n" + err.Message);
            });
    }

    private void OnEquipComplete(Item item) {
        trace("Successfully equipped item to hero on the server-side!");

        AudioManager.Instance.Play(SFX_UI.Toggle);
        EquipmentType type = item.data.EquipType;
        
        // equip the current item
        Equipped.Add(type, item);
        item.heroID = hero.MongoID;

        //DataManager.Instance.allHeroesList[heroReferenceIndex] = hero;
        Initialize(hero, campHeroInterface);

        // Update Image
        EquipmentSlots[type].sprite = item.data.LoadSprite();

        CloseButton.interactable = true;

        ClearStatDifferences();
        InventoryScreen.RefreshItems();
    }
}