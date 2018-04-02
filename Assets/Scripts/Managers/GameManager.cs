using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using DG.Tweening;

using Random = UnityEngine.Random;

public enum GameNetworkingType { Local, Public, Private }

public class GameManager : ManagerSingleton<GameManager> {
    public static float DEATH_DELAY = 3.0f;
    int lastSeed = -1;

    public bool isSummonComplete = true;
    public bool checkHeroWeapons = false;
    public bool isUpdateFeaturedItemPending = false;
    public bool InStory = false;
    public bool InResearch = false;
    //public HeroType NextSummonType = HeroType.Monster;

    public List<TapSkill> tapSkills = new List<TapSkill>();

    [Inspectionary] public TankLevelScale tankLevelScale = new TankLevelScale();
    [Inspectionary] public AssassinLevelScale assassinLevelScale = new AssassinLevelScale();
    [Inspectionary] public MageLevelScale mageLevelScale = new MageLevelScale();
    [Inspectionary] public AttackerLevelScale attackerLevelScale = new AttackerLevelScale();
    
    public float GetClassLevelScale(HeroClass hclass, PrimaryStats stat) {
        switch(hclass) {
            case HeroClass.Tank:
                return tankLevelScale[stat];
            case HeroClass.Assassin:
                return assassinLevelScale[stat];
            case HeroClass.Mage:
                return mageLevelScale[stat];
            case HeroClass.Bruiser:
                return attackerLevelScale[stat];
        }
        return 0f;
    }

    private void Start() {
#if PIERRE
        traceSetTagFilters("CHEATS", "PIERRE");
#else
        traceSetTagFilters("CHEATS");
#endif
    }

    public void UpdateServerTime(DateTime serverTime) {
        // Get server offset
        _serverOffset = serverTime - DateTime.Now;
        trace("NODE", "Server Offset is: " + _serverOffset.TotalHours);
    }

    public DateTime GetServerTime() {
        return DateTime.Now + ServerOffset;
    }

    TimeSpan _serverOffset;
    public TimeSpan ServerOffset {
        get { return _serverOffset; }
    }

    private void Update() {
        Tracer.CheckCounters();

        if(checkHeroWeapons) {
            checkHeroWeapons = false;
            foreach (Hero hero in dataMan.allHeroesList) {
                hero.CheckHasWeapon();
            }
        }
    }

    public void SummonHeroAction(SummonType type, Action<Hero> callback = null) {
        CurrencyTypes scrollType = CurrencyManager.ConvertSummonTypeToCurrency(type);
        int scrollsTotal = scrollType.GetAmount();
        if (scrollsTotal < 1) {
            throw new Exception("Unsufficient scrolls to summon the specified type: " + type + " => " + scrollType + ", scrolls: " +scrollsTotal);
        }

        int Seed = GetSeed();
        isSummonComplete = false;
        
        List<HeroData> possibleSummons = new List<HeroData>();
        HeroType summonType = HeroType.Monster;
        ElementalTypes ElementalType = ElementalTypes.None;
        float chance = Random.Range(0f, 1f);

        // Determine the type of hero to summon
        switch (type) {
            case SummonType.Common:

                if (chance <= GlobalProps.SUMMON_COMMON_HERO.GetFloat())
                    summonType = HeroType.Hero;
                else if (chance <= (GlobalProps.SUMMON_COMMON_HERO.GetFloat() + GlobalProps.SUMMON_COMMON_SOLDIER.GetFloat()))
                    summonType = HeroType.Soldier;
                else
                    summonType = HeroType.Monster;

                break;
            case SummonType.Rare:

                if (chance <= GlobalProps.SUMMON_RARE_HERO.GetFloat())
                    summonType = HeroType.Hero;
                else if (chance <= (GlobalProps.SUMMON_RARE_HERO.GetFloat() + GlobalProps.SUMMON_RARE_MONSTER.GetFloat()))
                    summonType = HeroType.Monster;
                else
                    summonType = HeroType.Soldier;
                
                break;
            case SummonType.MonsterFire: ElementalType = ElementalTypes.Fire; break;
            case SummonType.MonsterWater: ElementalType = ElementalTypes.Water; break;
            case SummonType.MonsterNature: ElementalType = ElementalTypes.Nature; break;
            case SummonType.MonsterDark: ElementalType = ElementalTypes.Dark; break;
            case SummonType.MonsterLight: ElementalType = ElementalTypes.Light; break;
        }

        possibleSummons = dataMan.heroDataList.FindAll(h => h.Type == summonType && h.AwokenReference == null);

        // Get the hero data list based on the types
        if (ElementalType != ElementalTypes.None) {
            // Summon w/ element (filter again from the above filtered list)
            possibleSummons = possibleSummons.FindAll(h => h.ElementalType == ElementalType);
        }

        Dictionary<HeroData, float> HeroChance = new Dictionary<HeroData, float>();
        foreach (HeroData hd in possibleSummons) {
            HeroChance.Add(hd, hd.Rarity);
        }
        
        // Get the hero from the list
        int summonIndex = Random.Range(0, possibleSummons.Count);
        trace(summonIndex);

        //eroChance.GetRandom();
        HeroData heroData = MathHelper.WeightedRandom(HeroChance).Key; //possibleSummons[summonIndex];

        //heroData = dataMan.heroDataList.GetByIdentity("hero_rareassassin"); // for testing awakening
        Hero tempHero = new Hero(heroData, Seed);

        trace("-- Summoning a hero: " + tempHero.Name + " [" + tempHero.Quality + "] (possibilities: " + possibleSummons.Count + ")\n"+(scrollsTotal-1));

        SummonInterface summonUI = (SummonInterface) MenuManager.Instance.Push("Interface_SummonHero");

        //First, do the currency transaction:
        API.Currency.AddCurrency(scrollType, -1 )
            //Then, do the actual adding of the new hero:
            .Then(res => API.Heroes.Add(tempHero))
            .Then(res => {
                Hero SelectedHero = dataMan.ProcessHeroData(res["newest"].AsArray[0]);

                summonUI.Initialize(SelectedHero);

                if (callback != null) callback(SelectedHero);
                if (signals.OnHeroCreated != null) signals.OnHeroCreated(SelectedHero);
            });
    }

    int lastTickCount = 0;
    public int GetSeed(bool ignoreReset = false) {
        // Wanted to re-random before getting seeds, as it is a predictable random atm, but this doesn't work
        if (!ignoreReset) {
            if (lastTickCount != System.Environment.TickCount) {
                lastTickCount = System.Environment.TickCount;
                Random.InitState(lastTickCount);
            }
        }
        int newSeed = Random.Range(0, 2000000000);
        while (newSeed == lastSeed) {
            newSeed = Random.Range(0, 2000000000);
        }
        lastSeed = newSeed;
        return lastSeed;
    }

    public int GetFeaturedQualitySeed() {
        /*float chance = UnityRandom.Range(0f, 1f);
        if (chance <= 0.01f) {
            return 410927051; // rare
        } else if (chance <= 0.2f) {
            return 410927051; // rare
        }*/
        return 410927051; // rare
        //return 673646702; // magic
    }

    public float GetExperienceRequiredForNextLevel(int level) {
        return (float) GetExperienceRequiredForLevel(level) - (float) GetExperienceRequiredForLevel(level - 1);
    }
    public int GetExperienceRequiredForLevel(int level) {
        if (level > 50)
            return ((2500 * level * level) - (145000 * level) + 2102500);
        else if (level > 35)
            return ((200 * level * level) + (1800 * level) - 80000);
        else if (level > 30)
            return ((500 * level * level) - (13500 * level) + 88000);
        else if (level > 22)
            return ((250 * level * level) - (1500 * level) - 22750);
        else if (level > 12)
            return ((50 * level * level) + (1750 * level) + 9800);
        else if (level > 7)
            return ((200 * level * level) + (1050 * level) - 2450);
        else
            return ((150 * level * level) + (1050 * level));
    }

    public TapSkill GetRandomTapSkill() {
        if(tapSkills.Count==0) return null;
        return tapSkills[UnityEngine.Random.Range(0, tapSkills.Count)];
    }

    public static bool IsElementACounter(ElementalTypes type, ElementalTypes target) {
        switch(target) {
            case ElementalTypes.Light:
                if (type == ElementalTypes.Dark) return true;
                break;
            case ElementalTypes.Dark:
                if (type == ElementalTypes.Light) return true;
                break;
            case ElementalTypes.Nature:
                if (type == ElementalTypes.Fire) return true;
                break;
            case ElementalTypes.Fire:
                if (type == ElementalTypes.Water) return true;
                break;
            case ElementalTypes.Water:
                if (type == ElementalTypes.Nature) return true;
                break;
        }

        return false;
    }

    IEnumerator updateFeaturedItemRoutine = null;
    public void UpdateFeatureItemTimer() {
        if (updateFeaturedItemRoutine != null) {
            StopCoroutine(updateFeaturedItemRoutine);
            Debug.LogWarning("Stoping updateFeaturedItemRoutine");
        }
        updateFeaturedItemRoutine = __UpdateFeaturedItemData();
        StartCoroutine(updateFeaturedItemRoutine);
    }

    IEnumerator __UpdateFeaturedItemData() {
        isUpdateFeaturedItemPending = false;

        while(ShopFeaturedItemInterface.CurrentResponse.progressPercent < 1) {
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(1f);
        isUpdateFeaturedItemPending = true;
        

        if (signals.OnFeaturedItemUpdated != null) {
            signals.OnFeaturedItemUpdated();
        }
    }
}
