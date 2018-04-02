using System;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;

using Random = UnityEngine.Random;

[Serializable]
public class Hero : MongoData<HeroData> {
    // These are for the status effects
    public float DamageReduction = 1f;
    public float DefenseEffectModifier = 1f;
    public float DamageEffectModifier = 1f;
    public bool EndureTriggered = false;
    public int ExploringActZone;

    public HeroPersonality personality = HeroPersonality.Astute;
    HeroQuality _quality = HeroQuality.Common;
    public HeroQuality Quality {
        get {
            return (_quality + QualityLevel);
        }
    }
    public int QualityLevel = 0;
    public DateTime lastUsedTapAbility = DateTime.Now.AddMinutes(-30);

    [Header("Callbacks")]
    public Action OnExperienceGain;
    public Action OnLevel;

    [Header("Skills")]
    public List<Skill> Skills = new List<Skill>();
    //public Dictionary<string, int> SkillLevels = new Dictionary<string, int>();
    public TapSkill tapSkill;

    [Inspectionary("EquipmentType", "ItemData")]
    public EquipedItemsDictionary EquipedItems = new EquipedItemsDictionary();

    internal Color GetQualityColor() {
        switch(Quality) {
            case HeroQuality.Common: return ColorConstants.HERO_COMMON.ToHexColor();
            case HeroQuality.Rare: return ColorConstants.HERO_RARE.ToHexColor();
            case HeroQuality.Legendary: return ColorConstants.HERO_LEGENDARY.ToHexColor();
        }

        return Color.white;
    }

    public HeroData heroData { get { return data; } }

    int _StatBaseOffset = 10;
    int _CoreBaseHealthOffset = 500;
    int _CoreBaseOffset = 50;

    public string CustomName;

    int _heroVarianceSeed = 0;
    public int HeroVarianceSeed { get { return _heroVarianceSeed; } }
    
    int _lastXP = 0;
    int _level = 1;
    public int Level {
        get {
            if (_lastXP != _experience) {
                _level = GetLevelBasedOnExperience();
                _lastXP = _experience;
            }
            if (isMaxLevel())
                return GetMaxLevel();
            return _level;
        }
    }
    public bool isMaxLevel() {
        if (_level > GetMaxLevel())
            return true;

        return false;
    }
    public int GetMaxLevel() {
        return (20 + (int) _quality * 10);
    }

    int _experience = 0;
    public int Experience {
        get {
            if (isMaxLevel())
                return 0;
            return _experience;
        }
        set {
            int lastLevel = Level;
            _experience = value;
            if (OnExperienceGain != null) OnExperienceGain();
            if (lastLevel < Level)
                LevelUp();
        }
    }
    
    public bool isAscended {
        get {
            if (data.AwokenReference != null || (data.Type == HeroType.Monster && Quality == HeroQuality.Legendary))
                return true;
            return false;
        }
    }

    public void IncreaseQuality() {
        Debug.Log("Increasing Hero Quality on " + Name);
        if (_quality == HeroQuality.Legendary) {
            Debug.LogError("[Bug] Should be Ascending not increasing quality");
            return;
        }
        QualityLevel++;

        // Save quality to server here
        GameAPIManager.Instance.Heroes.SetQualityLevel(this, QualityLevel);
    }

    public int GetSkillLevel(Skill skill) {
        int slvl = 1;
        List<ItemAffix> affixes = new List<ItemAffix>();
        // group all the +skill affixes
        foreach(EquipmentType slot in EquipedItems.Keys) {
            if (EquipedItems[slot] != null) {
                // There is an equiped item check it for skill level related affixes
                AffixData a_data;
                foreach (ItemAffix affix in EquipedItems[slot].Affixes) {
                    a_data = affix.data;
                    if (a_data.data.Identity == "mod_add_skilllevel") {
                        // It's a mod level type
                        affixes.Add(affix);
                        // or do calc here
                        if (a_data.RestrictedElement == ElementalTypes.None && a_data.RestrictedSkillType == SkillType.None) {
                            // No restrictions so just add the skill level
                            slvl += Mathf.RoundToInt(affix.GetValue(SecondaryStats.SkillLevel, EquipedItems[slot].ItemLevel, EquipedItems[slot].VarianceSeed));
                        } else {
                            // determine the restrictions
                            if (a_data.RestrictedSkillType != SkillType.None) {
                                if (a_data.RestrictedElement != ElementalTypes.None) {
                                    // check for both conditions
                                    if (skill.elementalType == a_data.RestrictedElement && skill.elementalSubType == a_data.RestrictedSkillType) {
                                        slvl += Mathf.RoundToInt(affix.GetValue(SecondaryStats.SkillLevel, EquipedItems[slot].ItemLevel, EquipedItems[slot].VarianceSeed));
                                    }
                                } else {
                                    // check for just skilltype
                                    if (skill.elementalSubType == a_data.RestrictedSkillType) {
                                        slvl += Mathf.RoundToInt(affix.GetValue(SecondaryStats.SkillLevel, EquipedItems[slot].ItemLevel, EquipedItems[slot].VarianceSeed));
                                    }
                                }
                            } else if (a_data.RestrictedElement != ElementalTypes.None) {
                                // Only elemental restriction
                                if (skill.elementalType == a_data.RestrictedElement) {
                                    slvl += Mathf.RoundToInt(affix.GetValue(SecondaryStats.SkillLevel, EquipedItems[slot].ItemLevel, EquipedItems[slot].VarianceSeed));
                                }
                            }
                        }
                    }
                }
            }
        }

        slvl = Mathf.CeilToInt((float) slvl / 100f);

        if (EquipedItems.ContainsKey(EquipmentType.Artifact)) {
            slvl += ((ItemArtifact) EquipedItems[EquipmentType.Artifact].data).GetSkillLevel(EquipedItems[EquipmentType.Artifact].ItemLevel);
        }
        
        if (slvl < 1) { Debug.LogError("[Skill Level Error] Skill levels are below the minimum of 1"); }

        return slvl;
    }

    public float GetSkillDamageMultiplier(Skill skill) {
        return GetSecondaryStat(SecondaryStats.SkillDamage);
    }

    public Hero(HeroData data, int Seed, int QualityLevel = 0, Dictionary<string, int> skillLevelDictionary = null) {
        this.data = data;

        if (data == null) {
            Tracer.traceWarn("Testing dummy Hero object.");
            return;
        }
        
        // Hero Variance
        _heroVarianceSeed = Seed;

        Random.InitState(_heroVarianceSeed);

        // Hero Quality
        _quality = HeroQuality.Common + this.QualityLevel;
        float Qchance = Random.Range(0f, 1f);
        if (Qchance <= dataMan.globalData.GetGlobalAsFloat(GlobalProps.SUMMON_QUALITY_3STAR)) {
            _quality = HeroQuality.Legendary;
        } else if (Qchance <= (dataMan.globalData.GetGlobalAsFloat(GlobalProps.SUMMON_QUALITY_3STAR) + dataMan.globalData.GetGlobalAsFloat(GlobalProps.SUMMON_QUALITY_2STAR))) {
            _quality = HeroQuality.Rare + this.QualityLevel;
        }

        // Hero Personality
        HeroPersonality[] personalities = (HeroPersonality[]) System.Enum.GetValues(typeof(HeroPersonality));
        int personatlityIndex = Random.Range(0, personalities.Length);
        
        personality = personalities[personatlityIndex];


        if (data.Skills!=null && data.Skills.Count>0) {
            if (data.Skills.Count > 4) {
                Debug.LogError("Too many skills on hero: " + data.Identity);
                for (int i = 0; i < (data.Skills.Count - 4); i++) {
                    data.Skills.RemoveAt(data.Skills.Count - 1);
                    Debug.Log("Remove skill on " + data.Identity + " at index [" + (data.Skills.Count - 1) + "]");
                }
            }
            foreach (Skill skill in data.Skills) {
                Skills.Add((Skill) UnityEngine.Object.Instantiate(skill));

                /*if (skillLevelDictionary != null && skillLevelDictionary.Count > 0) // skills exist on the server
                    SkillLevels.Add(skill.Identity, skillLevelDictionary[skill.Identity]);
                else
                    SkillLevels.Add(skill.Identity, 1);*/
            }
            ResetSkillsCooldown();

            tapSkill = GameManager.Instance.GetRandomTapSkill();
            if (tapSkill == null) {
                Tracer.traceCounter("TapSkill is null, probably because there was no TapSkills in the GameManager (yet?)");
            }
        } else {
            Tracer.traceError("This hero has no skills: " + this.DebugID);
        }

        //Let's the GameManager know to check all heroes to run CheckHasWeapon() method.
        GameManager.Instance.checkHeroWeapons = true;
    }

    public void ResetSkillsCooldown() {
        foreach (Skill skill in Skills) {
            skill._cooldown = 0;
        }

        //TODO reset tap skill here if we want to have a fresh tap skill / explore
    }
    

    public void CheckHasWeapon() {
        if(EquipedItems.ContainsKey(EquipmentType.Weapon)) {
            return;
        }
        
        //DataManager.Instance.GetRandomItemType(EquipmentType.Weapon)
        ItemData defaultWeaponData = heroData.DefaultWeaponItemData;

        if (defaultWeaponData != null) {
            var weapon = new Item(defaultWeaponData, 0, 0, 0, 0, 0, 20f, 0f);
            API.Items.Add(weapon, this.MongoID)
            .Then(res => {
                Tracer.traceWarn("Default weapon Added OK, now on to Equipping...");
                weapon.heroID = this.MongoID;
                EquipedItems[EquipmentType.Weapon] = weapon;
            })
            .Catch(err => {
            //Tracer.traceError("Could not add a Default Weapon to Hero: " + this.DebugID + " item: " + weapon.DebugID);
            Tracer.traceError("Could not Equip the newly added Default Weapon to Hero: " + this.DebugID + " item: " + weapon.DebugID);
            });
        }
    }

    void LevelUp() {
        if (OnLevel != null) OnLevel();
        //print("You leveled");
    }

    public int GetNextXPBasedOnLevel() {
        return GameManager.Instance.GetExperienceRequiredForLevel(Level) - GameManager.Instance.GetExperienceRequiredForLevel(Level - 1);
    }

    int GetLevelBasedOnExperience() {
        int xp = Experience;
        int level = 0;
        do {
            level++;
            xp -= ((GameManager.Instance.GetExperienceRequiredForLevel(level) - GameManager.Instance.GetExperienceRequiredForLevel(level - 1)));
        } while (xp >= 0);

        return level;
    }

    public int GetExperienceThisLevel() {
        return (_experience - GameManager.Instance.GetExperienceRequiredForLevel(Level - 1));
    }

    public float ExperienceProgress() {
        return (float) (_experience - GameManager.Instance.GetExperienceRequiredForLevel(Level - 1)) / (float) GetNextXPBasedOnLevel();
    }


    public float GetStatMultiplier(CoreStats stat) {
        float multiplier = 1f;
        foreach (EquipmentType type in EquipedItems.Keys) {
            if (EquipedItems[type] != null) {
                multiplier += EquipedItems[type].GetMultiplier(stat);
            }
        }

        return multiplier;
    }
    public float GetStatMultiplier(PrimaryStats stat) {
        float multiplier = 1f;
        foreach (EquipmentType type in EquipedItems.Keys) {
            if (EquipedItems[type] != null) {
                multiplier += EquipedItems[type].GetMultiplier(stat);
            }
        }

        return multiplier;
    }
    public float GetStatMultiplier(SecondaryStats stat) {
        float multiplier = 1f;
        foreach (EquipmentType type in EquipedItems.Keys) {
            if (EquipedItems[type] != null) {
                multiplier += EquipedItems[type].GetMultiplier(stat);
            }
        }

        return multiplier;
    }

    public int GetCoreStat(CoreStats stat) {
        int results = 0;
        int vitality = 0;
        float multiplier = 1f;
        float altmultiplier = 1f;
        LeaderSkill LeadershipSkill = PlayerManager.Instance.CurrentLeaderSkill;
        switch (stat) {
            case CoreStats.Health:
                // Calculate health
                vitality = (GetPrimaryStat(PrimaryStats.Vitality));
                foreach (EquipmentType type in EquipedItems.Keys) {
                    if (EquipedItems[type] != null) {
                        vitality += EquipedItems[type].GetStats(PrimaryStats.Vitality);
                        altmultiplier += EquipedItems[type].GetMultiplier(PrimaryStats.Vitality);
                        multiplier += EquipedItems[type].GetMultiplier(stat);
                    }
                }

                results = Mathf.RoundToInt(43 + (Level * 7) + (vitality * altmultiplier) * 10);

                foreach (EquipmentType type in EquipedItems.Keys) {
                    if (EquipedItems[type] != null)
                        results += EquipedItems[type].GetStats(stat);
                }

                //Debug.Log(stat + " calc: " + results + " * " + (multiplier * 100f).ToString("0") + "%");

                if (LeadershipSkill != null && LeadershipSkill.type == LeadershipSkillType.CoreStatBoost && LeadershipSkill.coreStat == CoreStats.Health)
                    return Mathf.RoundToInt(results * multiplier * LeadershipSkill.Multiplier);
                else
                    return Mathf.RoundToInt(results * multiplier);
            case CoreStats.Damage:
                // Calculate the damage
                switch (data.Class) {
                    case HeroClass.Bruiser:
                    case HeroClass.Tank:
                        // Attackers use strength for their attack
                        int strength = GetPrimaryStat(PrimaryStats.Strength);
                        foreach (EquipmentType type in EquipedItems.Keys) {
                            if (EquipedItems[type] != null) {
                                strength += EquipedItems[type].GetStats(PrimaryStats.Strength);
                                altmultiplier += EquipedItems[type].GetMultiplier(PrimaryStats.Strength);
                            }
                        }

                        // Hero Damage based on class
                        results = Mathf.RoundToInt(strength * altmultiplier * 1.5f);
                        break;
                    case HeroClass.Mage:
                        // Mages use Int for their attack
                        int intelligence = GetPrimaryStat(PrimaryStats.Intelligence);
                        foreach (EquipmentType type in EquipedItems.Keys) {
                            if (EquipedItems[type] != null) {
                                intelligence += EquipedItems[type].GetStats(PrimaryStats.Intelligence);
                                altmultiplier += EquipedItems[type].GetMultiplier(PrimaryStats.Intelligence);
                            }
                        }

                        // Hero Damage based on class
                        results = Mathf.RoundToInt(intelligence * altmultiplier * 2f);
                        break;
                    case HeroClass.Assassin:
                        // Mages use Int for their attack
                        int speed = GetPrimaryStat(PrimaryStats.Speed);
                        foreach (EquipmentType type in EquipedItems.Keys) {
                            if (EquipedItems[type] != null) {
                                speed += EquipedItems[type].GetStats(PrimaryStats.Speed);
                                altmultiplier += EquipedItems[type].GetMultiplier(PrimaryStats.Speed);
                            }
                        }

                        // Hero Damage based on class
                        results = Mathf.RoundToInt(speed * altmultiplier);
                        break;
                }
                // Get Weapon Damage
                if (EquipedItems.ContainsKey(EquipmentType.Weapon) && EquipedItems[EquipmentType.Weapon] != null)
                    results += ((ItemWeapon) EquipedItems[EquipmentType.Weapon].data).Damage;
                
                // Get added damage from Affixes
                foreach (EquipmentType type in EquipedItems.Keys) {
                    if (EquipedItems[type].Affixes.Count > 0 && type != EquipmentType.Weapon) {
                        results += EquipedItems[type].GetStats(stat);
                        multiplier += EquipedItems[type].GetMultiplier(stat);
                    }
                }

                //Debug.Log(stat + " calc: " + results + " * " + (multiplier * 100f).ToString("0") + "%");

                if (LeadershipSkill != null && LeadershipSkill.type == LeadershipSkillType.CoreStatBoost && LeadershipSkill.coreStat == CoreStats.Damage)
                    return Mathf.RoundToInt(results * multiplier * LeadershipSkill.Multiplier);
                else
                    return Mathf.RoundToInt(results * multiplier);
            case CoreStats.Defense:
                vitality = GetPrimaryStat(PrimaryStats.Vitality);
                foreach (EquipmentType type in EquipedItems.Keys) {
                    if (EquipedItems[type] != null) {
                        vitality += EquipedItems[type].GetStats(PrimaryStats.Vitality);
                        altmultiplier += EquipedItems[type].GetMultiplier(PrimaryStats.Vitality);
                    }
                }
                // Hero Defense
                results = Mathf.RoundToInt(vitality * altmultiplier * 3.5f);

                // Item Defense
                foreach (EquipmentType type in EquipedItems.Keys) {
                    if (EquipedItems[type] != null) {
                        results += EquipedItems[type].GetStats(stat);
                        multiplier += EquipedItems[type].GetMultiplier(stat);
                    }
                }

                // Hero Strength
                switch (data.Class) {
                    case HeroClass.Tank:
                        int additionalDefense = GetPrimaryStat(PrimaryStats.Strength);
                        altmultiplier = 1f;
                        // Item Strength
                        foreach (EquipmentType type in EquipedItems.Keys) {
                            if (EquipedItems[type] != null) {
                                additionalDefense += EquipedItems[type].GetStats(PrimaryStats.Strength);
                                altmultiplier += EquipedItems[type].GetMultiplier(PrimaryStats.Strength);
                            }
                        }

                        // Strength added to defense
                        results += Mathf.RoundToInt(additionalDefense * altmultiplier);
                        break;
                }

                //Debug.Log(stat + " calc: " + results + " * " + (multiplier * 100f).ToString("0") + "%");

                if (LeadershipSkill != null && LeadershipSkill.type == LeadershipSkillType.CoreStatBoost && LeadershipSkill.coreStat == CoreStats.Damage)
                    return Mathf.CeilToInt(results * multiplier * DefenseEffectModifier * LeadershipSkill.Multiplier);
                else
                    return Mathf.CeilToInt(results * multiplier * DefenseEffectModifier);
        }

        foreach (EquipmentType type in EquipedItems.Keys) {
            if (EquipedItems[type] != null) {
                results += EquipedItems[type].GetStats(stat);
                multiplier += EquipedItems[type].GetMultiplier(stat);
            }
        }
        return Mathf.RoundToInt(results * multiplier);
    }

    public int GetPrimaryStat(PrimaryStats stat) {
        int results = 0;
        float multiplier = 1f;
        LeaderSkill LeadershipSkill = PlayerManager.Instance.CurrentLeaderSkill;

        switch (stat) {
            case PrimaryStats.Strength:
                results = Mathf.RoundToInt(data.Strength + Mathf.RoundToInt(data.StrengthScale * Level) + (data.StrengthQualityBase * (int) _quality) + (Level * data.StrengthQualityScale * (int) _quality));
                
                // Personality Effect
                switch (personality) {
                    case HeroPersonality.Couragous:
                    case HeroPersonality.Steady:
                    case HeroPersonality.Energetic:
                        // Increase stat based on personality
                        results = Mathf.RoundToInt(results * 1.1f);
                        break;
                    case HeroPersonality.Perceptive:
                    case HeroPersonality.Stalwart:
                    case HeroPersonality.Agile:
                        // Decrease stat based on personality
                        results = Mathf.RoundToInt(results * 0.9f);
                        break;
                }
                break;
            case PrimaryStats.Vitality:
                results = Mathf.RoundToInt(data.Vitality + Mathf.RoundToInt(data.VitalityScale * Level) + (data.VitalityQualityBase * (int) _quality) + (Level * data.VitalityQualityScale * (int) _quality));

                // Personality Effect
                switch (personality) {
                    case HeroPersonality.Stalwart:
                    case HeroPersonality.Tenacious:
                    case HeroPersonality.Vigorous:
                        // Increase stat based on personality
                        results = Mathf.RoundToInt(results * 1.1f);
                        break;
                    case HeroPersonality.Couragous:
                    case HeroPersonality.Astute:
                    case HeroPersonality.Nimble:
                        // Decrease stat based on personality
                        results = Mathf.RoundToInt(results * 0.9f);
                        break;
                }
                break;
            case PrimaryStats.Intelligence:
                results = Mathf.RoundToInt(data.Intelligence + Mathf.RoundToInt(data.IntelligenceScale * Level) + (data.IntelligenceQualityBase * (int) _quality) + (Level * data.IntelligenceQualityScale * (int) _quality));

                // Personality Effect
                switch (personality) {
                    case HeroPersonality.Perceptive:
                    case HeroPersonality.Astute:
                    case HeroPersonality.Wise:
                        // Increase stat based on personality
                        results = Mathf.RoundToInt(results * 1.1f);
                        break;
                    case HeroPersonality.Energetic:
                    case HeroPersonality.Tenacious:
                    case HeroPersonality.Swift:
                        // Decrease stat based on personality
                        results = Mathf.RoundToInt(results * 0.9f);
                        break;
                }
                break;
            case PrimaryStats.Speed:
                results = Mathf.RoundToInt(data.Speed + Mathf.RoundToInt(data.Speed * Level) + (data.SpeedQualityBase * (int) _quality) + (Level * data.SpeedQualityScale * (int) _quality));

                // Personality Effect
                switch (personality) {
                    case HeroPersonality.Agile:
                    case HeroPersonality.Nimble:
                    case HeroPersonality.Swift:
                        // Increase stat based on personality
                        results = Mathf.RoundToInt(results * 1.1f);
                        break;
                    case HeroPersonality.Steady:
                    case HeroPersonality.Vigorous:
                    case HeroPersonality.Wise:
                        // Decrease stat based on personality
                        results = Mathf.RoundToInt(results * 0.9f);
                        break;
                }
                break;
        }

        foreach (EquipmentType type in EquipedItems.Keys) {
            results += EquipedItems[type].GetStats(stat);
            multiplier += EquipedItems[type].GetMultiplier(stat);
        }

        //Debug.Log(stat + " calc: " + results + " * " + (multiplier * 100f).ToString("0") + "%");

        if (LeadershipSkill != null && LeadershipSkill.type == LeadershipSkillType.PrimaryStatBoost && LeadershipSkill.primaryStat == stat)
            return Mathf.RoundToInt(results * multiplier * LeadershipSkill.Multiplier);
        else
            return Mathf.RoundToInt(results * multiplier);
    }

    public float GetSecondaryStat(SecondaryStats stat) {
        float results = 0f;
        float multiplier = 1f;
        LeaderSkill LeadershipSkill = PlayerManager.Instance.CurrentLeaderSkill;

        foreach (EquipmentType type in EquipedItems.Keys) {
            if (EquipedItems[type] != null) {
                results += EquipedItems[type].GetStats(stat);
                multiplier += EquipedItems[type].GetMultiplier(stat);
            }
        }

        //Debug.Log(stat + " calc: " + results + " * " + (multiplier * 100f).ToString("0") + "%");

        if (LeadershipSkill != null && LeadershipSkill.type == LeadershipSkillType.SecondaryStatBoost && LeadershipSkill.secondaryStat == stat)
            return Mathf.RoundToInt(results * multiplier * LeadershipSkill.Multiplier);
        else
            return Mathf.RoundToInt(results * multiplier);
    }
    
    public float GetDamageMitigation {
        get {
            float mitigation = (1f - ((float) GetCoreStat(CoreStats.Defense) / 1000f)) * 0.3f;
            return mitigation + 0.7f;
        }
    }

    public Sprite LoadPortraitSprite() {
        return data.LoadPortraitSprite();
    }

    public GameObject LoadUIAnimationReference() {
        return Resources.Load<GameObject>("Hero/" + data.Identity.ToLower() + "/UIMode");
    }

    public GameObject LoadTapBattleModel() {
        return Resources.Load<GameObject>("Hero/" + data.Identity.ToLower() + "/TapModel");
    }

    public string Name {
        get {
            if(string.IsNullOrEmpty(CustomName) || CustomName.Trim().Length==0) return data.Name;
            return CustomName;
        }
    }

    public HeroType Type {
        get { return data.Type; }
    }

    public bool HasPassiveSkill() {
        foreach (Skill skill in Skills) {
            if (skill.skillType == SkillTypes.Passive)
                return true;
        }
        return false;
    }
    public bool HasPassiveSkill(SkillTriggers trigger) {
        foreach (Skill skill in Skills) {
            if (skill.skillType == SkillTypes.Passive && skill.skillTrigger == trigger)
                return true;
        }
        return false;
    }
    public List<Skill> GetPassiveSkills() {
        List<Skill> passives = new List<Skill>();

        foreach (Skill skill in Skills) {
            if (skill.skillType == SkillTypes.Passive)
                passives.Add(skill);
        }

        return passives;
    }
    public List<Skill> GetPassiveSkills(SkillTriggers trigger) {
        List<Skill> passives = new List<Skill>();

        foreach (Skill skill in Skills) {
            if (skill.skillType == SkillTypes.Passive && skill.skillTrigger == trigger)
                passives.Add(skill);
        }

        return passives;
    }

    /* no longer using stats in passives, we have leader skills for that
    float GetSkillPassiveStatEffect(CoreStats stat, bool multiplier = false) {
        foreach (Skill skill in Skills) {
            if (skill.skillType == SkillTypes.Passive && skill.skillTrigger == SkillTriggers.Active) {
                // Get the passive stat boost here
                skill.GetStat(stat);
            }
        }

        return 0f;
    }
    */
}
