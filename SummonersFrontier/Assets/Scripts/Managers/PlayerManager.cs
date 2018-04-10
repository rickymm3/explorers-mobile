using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
using NodeJS;
using ExtensionMethods;
using RSG;

public class PlayerManager : ManagerSingleton<PlayerManager> {
    public List<BossData> DiscoveredBosses = new List<BossData>();
    public List<MonsterData> DiscoveredMonsters = new List<MonsterData>();
    public List<BoostSlot> BoostsSlotsActive = new List<BoostSlot>();
    public ActiveExploration SelectedBattle = null;
    public PlayerSignals Signals = new PlayerSignals();
    
    public bool isLastFeaturedItemPurchased = false;
    public bool isReturningFromBattleResults = false;
    public bool isLevelSequenceInProgress = false;

    public int CampScreenToLoadInto = -1;

    int _inventoryIDCounter = 0;
    float PlayerExperienceScale = 1f;

    public string Username = "Tap to login";
    public System.Action OnExperienceGain;
    public System.Action OnLevel;

    public LeaderSkill CurrentLeaderSkill = null;

    public int ActZoneCompleted = 0;
    public int ShopExpansion = 0;

    public int _lastLevel = 1;
    public int LastLevel {
        get { return _lastLevel; }
        set {
            // API call here
            GameAPIManager.Instance.Users.SetLastLevel(value)
            .Then(res => {
                _lastLevel = res["game"]["lastLevel"].AsInt;
            });
        }
    }

    public int _exploreSlots = 0;
    public int ExploreSlots {
        get { return _exploreSlots; }
        set {
            // API call here
            GameAPIManager.Instance.Users.SetExploreSlots(value)
            .Then(res => {
                _exploreSlots = res["game"]["actsZones"]["exploreSlots"].AsInt;
            });
        }
    }

    int lastXP = 0;
    int _level = 1;
    public int Level {
        get {
            if (lastXP != experience) {
                _level = GetLevelBasedOnExperience();
                lastXP = experience;
            }
            return _level;
        }
    }

    int experience = 0;
    public int Experience {
        get { return experience; }
        set {
            int lastLevel = Level;
            experience = value;
            if (OnExperienceGain != null) OnExperienceGain();
            if (lastLevel < Level)
                LevelUp();
        }
    }

    void Start() {
        
    }
    
    public void LevelSequence() {
        // Pop level up UI
        PlayerLevelInterface playerLevel = (PlayerLevelInterface) menuMan.Push("Interface_PlayerLevel");
        playerLevel.Initialize(LastLevel+1);
    }

    void LevelUp() {
        if (OnLevel != null) OnLevel();
        print("You leveled");
    }

    public int GetNextXPBasedOnLevel() {
        return GetExperienceRequiredForLevel(Level) - GetExperienceRequiredForLevel(Level - 1);
    }

    int GetLevelBasedOnExperience() {
        int xp = Experience;
        int level = 0;
        do {
            level++;
            xp -= ((GetExperienceRequiredForLevel(level) - GetExperienceRequiredForLevel(level - 1)));
        } while (xp >= 0);

        return level;
    }

    public int GetExperienceThisLevel() {
        return (experience - GetExperienceRequiredForLevel(Level - 1));
    }

    public float ExperienceProgress() {
        return (float) (experience - GetExperienceRequiredForLevel(Level - 1)) / (float) GetNextXPBasedOnLevel();
    }

    int GetExperienceRequiredForLevel(int lvl) {
        if (lvl > 50)
            return Mathf.CeilToInt(((2500 * lvl * lvl) - (145000 * lvl) + 2102500) * PlayerExperienceScale);
        else if (lvl > 35)
            return Mathf.CeilToInt(((200 * lvl * lvl) + (1800 * lvl) - 80000) * PlayerExperienceScale);
        else if (lvl > 30)
            return Mathf.CeilToInt(((500 * lvl * lvl) - (13500 * lvl) + 88000) * PlayerExperienceScale);
        else if (lvl > 22)
            return Mathf.CeilToInt(((250 * lvl * lvl) - (1500 * lvl) - 22750) * PlayerExperienceScale);
        else if (lvl > 12)
            return Mathf.CeilToInt(((50 * lvl * lvl) + (1750 * lvl) + 9800) * PlayerExperienceScale);
        else if (lvl > 7)
            return Mathf.CeilToInt(((200 * lvl * lvl) + (1050 * lvl) - 2450) * PlayerExperienceScale);
        else
            return Mathf.CeilToInt(((150 * lvl * lvl) + (1050 * lvl)) * PlayerExperienceScale);
    }

    public List<LootCrate> GetLootCrates() {
        if (isReturningFromBattleResults) {
            Debug.Log("Do we need this filter anymore? getting crates from the boss.");
        }
        
        return dataMan.allLootCratesList;
    }

    public Promise TryIdentifyScroll(Item item) {
        return new Promise((resolve, reject) => {
            int before = CurrencyTypes.SCROLLS_IDENTIFY.GetAmount();
            int cost = 1; //itemData.scrollCost;
            
            if (item.isIdentified) {
                reject(new Exception("Item is already identified!"));
                return;
            }

            if (before < cost) {
                reject(new Exception("Sorry, not enough scrolls!"));
                return;
            }

            API.Items.Identify(item)
                .Then(res => {
                    item.isIdentified = true;
                    var currency = res["user"]["game"]["currency"];
                    trace(currency.ToString("  "));

                    API.Currency.ParseCurrencyData(currency);

                    resolve();
                })
                .Catch(err => {
                    reject(err);
                });
        });
    }

    public List<Hero> GetAvailableHeroes(bool includeAll=false) {
        List<Hero> heroesAvailable = dataMan.allHeroesList.Clone();

        heroesAvailable = heroesAvailable
                            .OrderBy(x => x.data.Identity)
                            .OrderByDescending(x => (int)x.Quality).ToList();

        if (includeAll) return heroesAvailable;

        foreach (ActiveExploration exploration in dataMan.allExplorationsList) {
            foreach (Hero hero in exploration.Party) {
                heroesAvailable.Remove(hero);
            }
        }

        return heroesAvailable;
    }

    public List<Hero> GetFilteredHeroes(HeroListFilterType filter, bool includeAll=false) {
        switch (filter) {
            case HeroListFilterType.Level: return FilterHeroes(hr => hr.Level, includeAll);
            case HeroListFilterType.Health: return FilterHeroes(hr => hr.GetCoreStat(CoreStats.Health), includeAll);
            case HeroListFilterType.Damage: return FilterHeroes(hr => hr.GetCoreStat(CoreStats.Damage), includeAll);
            case HeroListFilterType.Defense: return FilterHeroes(hr => hr.GetCoreStat(CoreStats.Defense), includeAll);
            case HeroListFilterType.Strength: return FilterHeroes(hr => hr.GetPrimaryStat(PrimaryStats.Strength), includeAll);
            case HeroListFilterType.Intelligence: return FilterHeroes(hr => hr.GetPrimaryStat(PrimaryStats.Intelligence), includeAll);
            case HeroListFilterType.Vitality: return FilterHeroes(hr => hr.GetPrimaryStat(PrimaryStats.Vitality), includeAll);
            case HeroListFilterType.Speed: return FilterHeroes(hr => hr.GetPrimaryStat(PrimaryStats.Speed), includeAll);
            case HeroListFilterType.MagicFind: return FilterHeroes(hr => hr.GetSecondaryStat(SecondaryStats.MagicFind), includeAll);
            default: return GetAvailableHeroes(includeAll);
        }
    }
    
    List<Hero> FilterHeroes(Func<Hero, float> sortFunc, bool includeAll = false) {
        return GetAvailableHeroes(includeAll).OrderByDescending(sortFunc).ToList();
    }

    public IPromise<NodeResponse> StartExplore(ZoneData zone, List<Hero> party) {
        return API.Explorations.StartExploring(zone, party, DateTime.Now)
            .Then(res => {
                foreach (Hero hero in party) {
                    if(hero==null) continue;
                    hero.ExploringActZone = zone.ActZoneID;
                }

                //This should add the explorations to the DataManager's allExplorationList automatically.
                var exploration = dataMan.ProcessExplorationData(res["exploration"]);

                if (signals.OnExploreStarted != null) signals.OnExploreStarted(exploration);
            })
            .Catch(err => {
                traceError("Failed to Start Exploring ActZoneID #" + zone.ActZoneID + ": " + GameAPIManager.GetErrorMessage(err));
                traceError(err.StackTrace);
            });
    }

    public void CompleteCurrentExplore(bool success = true) {
        dataMan.allExplorationsList.Remove(SelectedBattle);
        SelectedBattle = null;
    }

    public List<string> GetLevelRewards(int level) {
        return DataManager.Instance.GetPlayerLevelRewardsRewardsByLevel(level).Rewards;
    }

    public void AddLevelRewards(PlayerLevelReward reward) {
        switch (reward.Type) {
            case PlayerLevelRewardType.Currency:
                GameAPIManager.Instance.Currency.AddCurrency(reward.CurrencyType, reward.Amount);
                break;
            case PlayerLevelRewardType.ExploreSlot:
                playerMan.ExploreSlots += 1;
                break;
            default:
                Debug.LogError("Reward type not implemented: " + reward.Type);
                break;
        }
    }

    public float GetBoost(BoostType type) {
        //print(" ||||| [Boosts] Re-implement me");
        //return 1f;
        if (BoostsSlotsActive.Count <= 0) return 1f;

        BoostSlot boost = BoostsSlotsActive.Find(b => b.data != null && b.data.boostType == type);
        if(boost==null) return 1f;
        return boost.data.value;
    }

    public void ConsumeBoostTime() {
        // reduce the boost remaining time here?
        foreach(BoostSlot boost in BoostsSlotsActive) {
            if(boost.data==null) continue;
            GameAPIManager.Instance.Users.BoostDecrease(boost.slotID);
        }
    }

    public bool HasItemReference(UniqueReference unique) {
        if (PlayerPrefs.HasKey(unique.Identity)) return true;

        return false;
    }
}

public class PlayerSignals {
    public Action OnAllCurrenciesUpdated;
    public Action<int, int, CurrencyTypes> OnChangedCurrency;
    public Action<int, int, BoostType> OnChangedBoostCurrency;
    public Action<int, int, ItemQuality> OnChangedShards;
    public Action<NodeResponse> OnUserLoginOK;
    public Action OnGetEverything;
    public Action<Item> OnIdentifiedItem;
    public Action<Item> OnItemRemoved;
    public Action<Hero> OnHeroCreated;
    public Action<ActiveExploration> OnExploreStarted;
    public Action OnFeaturedItemUpdated;
    public Action OnBoostsUpdated;
}
