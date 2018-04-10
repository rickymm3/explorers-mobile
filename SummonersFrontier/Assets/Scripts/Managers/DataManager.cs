using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using System;
using SimpleJSON;
using ExtensionMethods;
using RSG;
using System.Collections;
using NodeJS;

[Serializable] public class SkillDataList : DataList<SkillData> { }
[Serializable] public class HeroDataList : DataList<HeroData> { }
[Serializable] public class ItemDataList : DataList<ItemData> { }
[Serializable] public class ActDataList : DataList<ActData> { }
[Serializable] public class BoostDataList : DataList<BoostData> { }
[Serializable] public class ZoneDataList : DataList<ZoneData> { }
[Serializable] public class RewardsByPlayerLevelDataList : DataList<RewardsByPlayerLevel> { }
[Serializable] public class PlayerLevelRewardsDataList : DataList<PlayerLevelReward> { }
[Serializable] public class BossDataList : DataList<BossData> { }
[Serializable] public class UniqueReferenceDataList : DataList<UniqueReference> { }
[Serializable] public class BossFightDataList : DataList<BossFightData> { }
[Serializable] public class MonsterDataList : DataList<MonsterData> { }
[Serializable] public class RetireRewardsList : DataList<RetireRewardsData> { }
[Serializable] public class LootTableDataList : DataList<LootTableData> { }
[Serializable] public class ItemModDataList : DataList<ItemModData> { }
[Serializable] public class AffixDataList : DataList<AffixData> { }
[Serializable] public class CrateTypeDataList : DataList<CrateTypeData> { }
[Serializable] public class HeroList : MongoList<Hero> { }
[Serializable] public class ItemList : MongoList<Item> { }
[Serializable] public class LootCrateList : MongoList<LootCrate> { }
[Serializable] public class ExplorationList : MongoList<ActiveExploration> { }
[Serializable] public class StoryDataList : DataList<StorySection> { }

[Serializable]
public class DataManager : ManagerSingleton<DataManager> {
    // Player Values
    public GlobalsData globalData;
    public HeroList allHeroesList;
    public ItemList allItemsList;
    public LootCrateList allLootCratesList;
    public ExplorationList allExplorationsList;
    
    // Data Values
    public SkillDataList skillDataList;
    public HeroDataList heroDataList;
    public ItemDataList itemDataList; //<-- weapons, armors, relics, etc.
    public UniqueReferenceDataList uniqueReferenceDataList;
    public ActDataList actDataList;
    public BoostDataList boostDataList;
    public PlayerLevelRewardsDataList playerLevelRewardsDataList;
    public RewardsByPlayerLevelDataList rewardsByPlayerLevelDataList;
    public ZoneDataList zoneDataList;
    public BossDataList bossDataList;
    public BossFightDataList bossFightDataList;
    public MonsterDataList monsterDataList;
    public RetireRewardsList retireRewardsDataList;
    public LootTableDataList lootTableDataList;
    public ItemModDataList itemModDataList;
    public AffixDataList affixDataList;
    public CrateTypeDataList crateTypeDataList;
    public StoryDataList storyDataList;

    public LocalizationManager localizationManager;

    public bool isLoaded = false;

    public Action OnJSONLoaded;
    
    //void Start() {
    //    signals.OnUserLoginOK += OnUserLoginOK;
    //}

    //void OnUserLoginOK(NodeResponse res) {
    //    LoadPlayerData();
    //}

    [ContextMenu("Load JSON")]
    public IPromise<JSONNode> LoadJSON() {
        bool isLocal = buildMan.useLocalJSON;
        string url = isLocal ? buildMan.LOCAL_JSON : buildMan.URL_JSON;
        isLoaded = false;

        print("Loading JSON at: " + url);

        return JSONManager.Instance.Load(url)
            .Then((JSONNode jsonData) => {
                if (!isLocal) {
                    JSONManager.Instance.SaveLastFileTo(buildMan.LOCAL_JSON);
                }

                trace("LOADING", "JSON Data Loaded, now populate the lists with objects");

                JSONNode jsonSheets = jsonData["sheets"];

                globalData = new GlobalsData();
                allHeroesList = new HeroList();
                allItemsList = new ItemList();
                allLootCratesList = new LootCrateList();

                skillDataList = new SkillDataList();
                heroDataList = new HeroDataList();
                uniqueReferenceDataList = new UniqueReferenceDataList();
                itemDataList = new ItemDataList();
                zoneDataList = new ZoneDataList();
                actDataList = new ActDataList();
                boostDataList = new BoostDataList();
                rewardsByPlayerLevelDataList = new RewardsByPlayerLevelDataList();
                playerLevelRewardsDataList = new PlayerLevelRewardsDataList();
                bossDataList = new BossDataList();
                bossFightDataList = new BossFightDataList();
                monsterDataList = new MonsterDataList();
                retireRewardsDataList = new RetireRewardsList();
                lootTableDataList = new LootTableDataList();
                itemModDataList = new ItemModDataList();
                affixDataList = new AffixDataList();
                crateTypeDataList = new CrateTypeDataList();
                storyDataList = new StoryDataList();

                globalData.LoadGlobals(jsonSheets["globals"]);

                LoadSkills(jsonSheets["skills"]);
                LoadItemMods(jsonSheets["mods"]);
                LoadAffixes(jsonSheets["affixes-and-suffixes"]);
                LoadUniqueItemReferences(jsonSheets["unique-items"]);
                LoadWeapons(jsonSheets["item-weapons"]);
                LoadArmors(jsonSheets["item-armors"]);
                LoadArtifacts(jsonSheets["item-artifacts"]);
                LoadCurrencyItems(jsonSheets["item-currency"]);
                LoadRetireRewards(jsonSheets["retire-rewards"]);
                LoadRewardsByPlayerLevels(jsonSheets["rewards-by-player-level"]);
                LoadPlayerLevelRewards(jsonSheets["player-level-rewards"]);
                LoadCrateTypes(jsonSheets["crate-types"]);
                LoadLootTables(jsonSheets["loot-tables"]);
                LoadHeroes(jsonSheets["heroes"]);
                LoadBosses(jsonSheets["bosses"]);
                LoadBossFights(jsonSheets["boss-fights"]);
                LoadMonsters(jsonSheets["monsters"]);
                LoadZones(jsonSheets["zones"]);
                LoadActs(jsonSheets["acts"]);
                LoadBoosts(jsonSheets["boosts"]);
                LoadStorySections(jsonSheets["story-segments"]);

                LocalizationManager.ParseData(jsonSheets["localization"]);

                trace(//"LOADING",
                    heroDataList.Count + " heroes, " +
                    skillDataList.Count + " skills, " +
                    itemDataList.Count + " items, " +
                    uniqueReferenceDataList.Count + " unique references, " +
                    actDataList.Count + " acts, " +
                    boostDataList.Count + " boosts, " +
                    playerLevelRewardsDataList.Count + " player level rewards, " +
                    zoneDataList.Count + " zones, " +
                    bossDataList.Count + " bosses, " +
                    bossFightDataList.Count + " boss fights, " +
                    itemModDataList.Count + " item mods, " +
                    affixDataList.Count + " affixes, " +
                    monsterDataList.Count + " monsters, " +
                    crateTypeDataList.Count + " crate types, " +
                    storyDataList.Count + " story segments, " +
                    lootTableDataList.Count + " loot-tables."
                );

                if (OnJSONLoaded != null) OnJSONLoaded();
            })
            .Catch((Exception err) => {
                if (isLocal) {
                    traceError("Could not even load the local file!");
                    return;
                }

                traceError("Switching to Local JSON, reloading because: " + err.Message);
                Debug.LogError(err.StackTrace);
                buildMan.useLocalJSON = true;

                LoadJSON();
            });
    }

    void LoadSkills(JSONNode sheet) {
        JSONManager.ForEach(sheet, skillDataList, (JSONNode data, int id) => {
            return new SkillData(
                itemDataList.Count,
                 data["identity"].AsDecodedURL(),
                 data["name"].AsDecodedURL(),
                 data["weight"].AsFloat,
                 data["cooldown"].AsInt,
                 data["icon"].AsDecodedURL()
             );
        });
    }

    void LoadHeroes(JSONNode sheet) {
        JSONManager.ForEach(sheet, heroDataList, (JSONNode data, int id) => {
            return new HeroData(
                heroDataList.Count,
                data["identity"].AsDecodedURL(),
                data["name"].AsDecodedURL(),
                data["hero-class"].AsEnum<HeroClass>(true),
                data["hero-type"].AsEnum<HeroType>(true),
                data["element"].AsEnum<ElementalTypes>(true),
                data["strength"].AsInt,
                data["vitality"].AsInt,
                data["intelligence"].AsInt,
                data["speed"].AsInt,
                data["strength-scale"].AsFloat,
                data["vitality-scale"].AsFloat,
                data["intelligence-scale"].AsFloat,
                data["speed-scale"].AsFloat,
                data["qstr-base"].AsFloat,
                data["qvit-base"].AsFloat,
                data["qint-base"].AsFloat,
                data["qspd-base"].AsFloat,
                data["qstr-level"].AsFloat,
                data["qvit-level"].AsFloat,
                data["qint-level"].AsFloat,
                data["qspd-level"].AsFloat,
                Resources.Load<LeaderSkill>("Skills/LeadershipSkills/" + data["leader-skill"]),
                data["default-weapon"],
                data["awaken-reference"],
                GetSkillsByData(data["skills"]),
                data["order"].AsInt,
                data["rarity"].AsInt
            );
        });
    }

    List<Skill> GetSkillsByData(JSONNode jsonData) {
        List<Skill> skills = new List<Skill>();

        string[] identities = JSONManager.SplitArray(jsonData);
        foreach (string identity in identities) {
            Skill skill = Resources.Load<Skill>("Skills/Scriptables/" + identity);
            skills.Add(skill);
        }

        return skills;
    }

    BossPhaseSkillList GetPhaseSkillsByData(JSONNode jsonData) {
        BossPhaseSkillList phaseSkills = new BossPhaseSkillList();
        List<Skill> skillList = new List<Skill>();

        //print("[Phase Skill] json: " + jsonData);

        string[] phases = JSONManager.SplitArray(jsonData, ',', false);

        foreach (string phase in phases) {
            string[] skills = JSONManager.SplitArray(phase.Replace("{", "").Replace("}", "").Trim(), '\n', false);
            for (int i = 1; i < skills.Length; i++) {
                Skill skill = UnityEngine.Object.Instantiate(Resources.Load<Skill>("Skills/Scriptables/" + skills[i]));
                skillList.Add(skill);
            }

            if (!phaseSkills.Keys.Contains(skills[0].AsEnum<BossPhases>()))
                phaseSkills.Add(skills[0].AsEnum<BossPhases>(), skillList);

            //Debug.Log("[Phase Skills] phase: " + phase.Trim() + "[" + phaseSkills.Count + "]\n");
        }

        return phaseSkills;
    }

    List<BossPatternTrigger> GetBossPatternTriggers(JSONNode jsonData, JSONNode messageJsonData) {
        List<BossPatternTrigger> patternList = new List<BossPatternTrigger>();
        Dictionary<BossPhases, string> messageRef = new Dictionary<BossPhases, string>();

        string[] triggers = JSONManager.SplitArray(jsonData, ',', false);
        string[] messages = JSONManager.SplitArray(messageJsonData, '\n', false);

        if (messages.Length > 0)
            foreach (string msg in messages) {
                string[] sa = msg.Split(':');
                if (sa.Length > 1)
                    messageRef.Add(sa[0].AsEnum<BossPhases>(), sa[1].Trim());
            }

        //print(" [Phase Trigger] Triggers: " + jsonData + "\n");
        foreach (string trigger in triggers) {
            string[] state = JSONManager.SplitArray(trigger, ':', false);
            //print("-- [Phase Trigger] [Length: " + state.Length + " ]" + trigger + "\n");
            if (state[0].ToLower() != "None".ToLower()) {
                if (messageRef.ContainsKey(state[0].AsEnum<BossPhases>()))
                    patternList.Add(new BossPatternTrigger(state[0].AsEnum<BossPhases>(), state[3].AsEnum<BossPhases>(), state[1].AsEnum<BossPhaseTriggerType>(), float.Parse(state[2]), messageRef[state[0].AsEnum<BossPhases>()]));
                else
                    patternList.Add(new BossPatternTrigger(state[0].AsEnum<BossPhases>(), state[3].AsEnum<BossPhases>(), state[1].AsEnum<BossPhaseTriggerType>(), float.Parse(state[2])));
            }
        }

        return patternList;
    }

    void LoadUniqueItemReferences(JSONNode sheet) {
        JSONManager.ForEach(sheet, uniqueReferenceDataList, (JSONNode data, int id) => {
            List<ItemAffix> affixes = new List<ItemAffix>();
            string[] ar = data["affix-reference"].AsDecodedURL().Split("\n");

            foreach (string aff in ar) {
                affixes.Add(new ItemAffix(affixDataList.Find(a => a.Identity == aff)));
            }

            return new UniqueReference(
                 itemDataList.Count,
                 data["identity"].AsDecodedURL(),
                 data["reference"].AsDecodedURL(),
                 data["name"].AsDecodedURL(),
                 data["description"].AsDecodedURL(),
                 data["value"].AsInt,
                 affixes,
                 data["sprite"].AsDecodedURL()
             );
        });
    }

    void LoadWeapons(JSONNode sheet) {
       JSONManager.ForEach(sheet, itemDataList, (JSONNode data, int id) => {
           return new ItemWeapon(
               itemDataList.Count,
                data["identity"].AsDecodedURL(),
                data["name"].AsDecodedURL(),
                data["value"].AsInt,
                data["description"].AsDecodedURL(),
                data["weapon-type"].AsEnum<WeaponType>(true),
                data["damage"].AsInt,
                data["sprite"].AsDecodedURL(),
                data["tier"].AsInt,
                data["multiplier"].AsFloat,
                uniqueReferenceDataList.FindAll(r => r.Reference == data["identity"].AsDecodedURL())
            );
        });
    }

    void LoadArmors(JSONNode sheet) {
        JSONManager.ForEach(sheet, itemDataList, (JSONNode data, int id) => {
            return new ItemArmor(
                itemDataList.Count,
                data["identity"].AsDecodedURL(),
                data["name"].AsDecodedURL(),
                data["value"].AsInt,
                data["description"].AsDecodedURL(),
                data["equipment-type"].AsEnum<EquipmentType>(true),
                data["defense"].AsInt,
                data["sprite"].AsDecodedURL(),
                data["tier"].AsInt,
                data["multiplier"].AsFloat,
                uniqueReferenceDataList.FindAll(r => r.Reference == data["identity"].AsDecodedURL())
            );
        });
    }

    void LoadArtifacts(JSONNode sheet) {
        JSONManager.ForEach(sheet, itemDataList, (JSONNode data, int id) => {
            return new ItemArtifact(
                itemDataList.Count,
                data["identity"].AsDecodedURL(),
                data["name"].AsDecodedURL(),
                data["value"].AsInt,
                data["description"].AsDecodedURL(),
                data["sprite"].AsDecodedURL(),
                data["tier"].AsInt,
                data["multiplier"].AsFloat,
                data["skill-base"].AsInt,
                data["elemental-type"].AsEnum<ElementalTypes>(true),
                uniqueReferenceDataList.FindAll(r => r.Reference == data["identity"].AsDecodedURL())
            );
        });
    }

    void LoadCurrencyItems(JSONNode sheet) {
        JSONManager.ForEach(sheet, itemDataList, (JSONNode data, int id) => {
            return new ItemCurrency(
                itemDataList.Count,
                data["identity"].AsDecodedURL(),
                data["name"].AsDecodedURL(),
                data["description"].AsDecodedURL(),
                data["sprite"].AsDecodedURL(),
                data["value"].AsInt,
                data["type"].AsEnum<CurrencyTypes>(true),
                data["reward"].AsInt,
                uniqueReferenceDataList.FindAll(r => r.Reference == data["identity"].AsDecodedURL())
            );
        });
    }

    void LoadBosses(JSONNode sheet) {
        JSONManager.ForEach(sheet, bossDataList, (JSONNode data, int id) => {
            return new BossData(
                data["identity"].AsDecodedURL(),
                data["name"].AsDecodedURL(),
                data["health"].AsInt,
                data["damage"].AsInt,
                data["defense"].AsInt,
                data["speed"].AsInt,
                data["element"].AsEnum<ElementalTypes>(true),
                data["sprite"].AsDecodedURL(),
                GetPhaseSkillsByData(data["phases"]),
                GetBossPatternTriggers(data["triggers"], data["phase-change-message"])
            );
        });
    }

    void LoadMonsters(JSONNode sheet) {
        JSONManager.ForEach(sheet, monsterDataList, (JSONNode data, int id) => {
            return new MonsterData(
                data["identity"].AsDecodedURL(),
                data["name"].AsDecodedURL(),
                data["element"].AsEnum<ElementalTypes>(true),
                data["type"].AsEnum<TapMonsterType>(true),
                data["sprite"].AsDecodedURL()
            );
        });
    }

    void LoadRewardsByPlayerLevels(JSONNode sheet) {
        JSONManager.ForEach(sheet, rewardsByPlayerLevelDataList, (JSONNode data, int id) => {
            List<string> rewards = new List<string>();
            foreach (string rwd in data["rewards"].AsDecodedURL().Split("\n")) {
                rewards.Add(rwd.Trim());
            }

            return new RewardsByPlayerLevel(
                data["level"].AsInt,
                rewards
            );
        });
    }

    void LoadPlayerLevelRewards(JSONNode sheet) {
        JSONManager.ForEach(sheet, playerLevelRewardsDataList, (JSONNode data, int id) => {
            return new PlayerLevelReward(
                id,
                data["identity"].AsDecodedURL(),
                data["name"].AsDecodedURL(),
                data["type"].AsEnum<PlayerLevelRewardType>(true),
                data["reward"] == "Other" ? CurrencyTypes.NONE : data["reward"].AsEnum<CurrencyTypes>(true),
                data["amount"].AsInt,
                data["show-amount"].AsBool,
                data["sprite"].AsDecodedURL()
            );
        });
    }

    void LoadCrateTypes(JSONNode sheet) {
        JSONManager.ForEach(sheet, crateTypeDataList, (JSONNode data, int id) => {
            Dictionary<string, string> ctcdData = JSONManager.SplitKVStrings(data["quality"]);
            CrateChanceDictionary ctcd = new CrateChanceDictionary();
            foreach(KeyValuePair<string, string> kv in ctcdData) {
                ItemQuality quality = kv.Key.Trim().AsEnum<ItemQuality>();
                ctcd.Add(quality, float.Parse(kv.Value));
            }

            return new CrateTypeData(
                 crateTypeDataList.Count,
                 data["identity"].AsDecodedURL(),
                 data["name"].AsDecodedURL(),
                 ctcd
             );
        });
    }

    void LoadLootTables(JSONNode sheet) {
        JSONManager.ForEach(sheet, lootTableDataList, (JSONNode data, int id) => {
            /*LootTableDictionary ltd = new LootTableDictionary();
            Dictionary<string, string> ltdData = JSONManager.SplitKVStrings(data["loot-tables"]);
            //print("item list count: " + itemDataList.Count);
            foreach (KeyValuePair<string,string> kv in ltdData) {
                ItemData item = itemDataList.GetByIdentity(kv.Key.Trim());
                if(item==null) {
                    traceError("LootTable error, cannot find item: " + kv.Key + " with value: " + kv.Value);
                    continue;
                }

                ltd.Add(item, float.Parse(kv.Value));
            }*/

            Dictionary<string, string> cdData = JSONManager.SplitKVStrings(data["crate-type"]);
            CrateTypeChanceDictionary cd = new CrateTypeChanceDictionary();
            foreach (KeyValuePair<string, string> kv in cdData) {
                CrateTypeData crateTypeData = crateTypeDataList.GetByIdentity(kv.Key.Trim());
                cd.Add(crateTypeData, float.Parse(kv.Value));
            }

            Dictionary<string, string> tcData = JSONManager.SplitKVStrings(data["tier"]);
            TierChanceDictionary tcd = new TierChanceDictionary();
            foreach (KeyValuePair<string, string> kv in tcData) {
                tcd.Add(int.Parse(kv.Key), float.Parse(kv.Value));
            }

            Dictionary<string, string> itData = JSONManager.SplitKVStrings(data["item-type"]);
            ItemTypeDictionary itd = new ItemTypeDictionary();
            foreach (KeyValuePair<string, string> kv in itData) {
                itd.Add(kv.Key.Trim().AsEnum<EquipmentType>(), float.Parse(kv.Value));
            }

            List<CurrencyTypes> availableCurrencies = new List<CurrencyTypes>();
            foreach (string type in data["currency-types"].AsDecodedURL().Split("\n")) {
                if (type.Trim().ToLower() != "none")
                    availableCurrencies.Add(type.Trim().AsEnum<CurrencyTypes>());
            }

            return new LootTableData() {
                Identity = data["identity"].AsDecodedURL(),
                minDrops = data["min-drop"].AsInt,
                maxDrops = data["max-drop"].AsInt,
                crateType = cd,
                tierChance = tcd,
                itemTypes = itd,
                AvailableCurrencyTypes = availableCurrencies
            };
        });
    }

    void LoadRetireRewards(JSONNode sheet) {
        JSONManager.ForEach(sheet, retireRewardsDataList, (JSONNode data, int id) => {
            RetireRewardsData retireReward = new RetireRewardsData();
            retireReward.ID = id;
            try { 
                retireReward.ParseJSON(data);
            } catch(Exception err) {
                traceError("Could not parse the RetireRewardsData {0} properly, here's the JSON of it: ".Format2(data["identity"]) + data.ToString("  "));
                return null;
            }
            return retireReward;
        });
    }

    void LoadBossFights(JSONNode sheet) {
        JSONManager.ForEach(sheet, bossFightDataList, (JSONNode data, int id) => {
            return new BossFightData(
                data["identity"].AsDecodedURL(),
                FindByIdentities(data["monsters"], bossDataList),
                lootTableDataList.GetByIdentity(data["loot-table"]),
                data["gold"].AsInt,
                data["experience"].AsInt
            );
        });
    }

    void LoadZones(JSONNode sheet) {
        JSONManager.ForEach(sheet, zoneDataList, (JSONNode data, int id) => {
            return new ZoneData(
                data["identity"].AsDecodedURL(),
                data["name"].AsDecodedURL(),
                data["description"].AsDecodedURL(),
                data["act"].AsInt,
                data["zone"].AsInt,
                data["act-zone"].AsInt,
                data["boss-power"].AsFloat,
                data["zone-hp"].AsInt,
                lootTableDataList.GetByIdentity(data["loot-table-name"]), /* Make this table before the return based on a lookup? */
                data["min-ilvl"].AsFloat,
                data["item-variance"].AsFloat,
                data["monster-timer"].AsFloat,
                bossFightDataList.GetByIdentity(data["boss-fight"].AsDecodedURL()),
                FindByIdentities(data["monster-list"], monsterDataList),
                data["difficulty"].AsEnum<ZoneDifficulty>(),
                data["story-events"].AsDecodedURL()
            );
        });
    }

    void LoadActs(JSONNode sheet) {
        JSONManager.ForEach(sheet, actDataList, (JSONNode data, int id) => {
            return new ActData(
                data["identity"].AsDecodedURL(),
                data["name"].AsDecodedURL(),
                data["description"].AsDecodedURL(),
                data["act-id"].AsInt,
                GetZonesByAct(data["act-id"].AsInt)
            );
        });
    }

    void LoadItemMods(JSONNode sheet) {
        JSONManager.ForEach(sheet, itemModDataList, (JSONNode data, int id) => {
            return new ItemModData(
                data["identity"].AsDecodedURL(),
                data["mod-type"].AsEnum<ItemModType>(true),
                data["stat-type"].AsEnum<ItemModEffects>(true),
                data["base"].AsFloat,
                data["scaling-factor"].AsFloat,
                data["variance"].AsFloat,
                data["apply-time"].AsEnum<StatusTriggerTime>(),
                data["status"].AsDecodedURL(),
                data["core-stat"].AsEnum<CoreStats>(true),
                data["primary-stat"].AsEnum<PrimaryStats>(true),
                data["secondary-stat"].AsEnum<SecondaryStats>(true)
            );
        });
    }

    void LoadBoosts(JSONNode sheet) {
        JSONManager.ForEach(sheet, boostDataList, (JSONNode data, int id) => {
            return new BoostData(
                data["identity"].AsDecodedURL(),
                data["num-of-battles"].AsInt,
                data["type"].AsEnum<BoostType>(true),
                data["value"].AsFloat,
                data["sprite"],
                data["description"]
            );
        });
    }

    void LoadAffixes(JSONNode sheet) {
        JSONManager.ForEach(sheet, affixDataList, (JSONNode data, int id) => {
            return new AffixData(
                data["identity"].AsDecodedURL(),
                data["name"].AsDecodedURL(),
                data["type"].AsEnum<AffixType>(true),
                data["unique"].AsBool,
                data["rare"].AsBool,
                data["magic"].AsBool,
                data["min-level"].AsInt,
                data["frequency"].AsInt,
                GetEnumList<ItemType>(data["restricted-item-types"]),
                data["restricted-element"].AsEnum<ElementalTypes>(),
                data["restricted-skill-type"].AsEnum<SkillType>(),
                itemModDataList.GetByIdentity(data["mod"])
            );
        });
    }

    void LoadStorySections(JSONNode sheet) {
        JSONManager.ForEach(sheet, storyDataList, (JSONNode data, int id) => {
            return new StorySection(
                itemDataList.Count,
                data["identity"].AsDecodedURL(),
                data["section"].AsDecodedURL(),
                data["order"].AsInt,
                data["name"].AsDecodedURL(),
                data["text"].AsDecodedURL(),
                Resources.Load<Sprite>("Hero/" + data["character-art"].AsDecodedURL() + "/fullbody"),
                data["action"].AsEnum<StoryAction>(),
                data["focus"].AsEnum<StoryCharacterPosition>(),
                data["emotion"].AsEnum<StoryCharacterEmotion>()
             );
        });
    }

    ////////////////////////////////////////////////////////////////////////

    public void LoadPlayerData() {
        if(!API.isLoggedIn) {
            traceError("LOADING", "No Player logged in yet! Waiting until OnUserLoginOK is triggered...");
            return;
        }

        API.Users.GetEverything()
            .Then(res => {
                string serverTimeStr = res.json["headers"]["dateResponded"];

                GameManager.Instance.UpdateServerTime( serverTimeStr.FromNodeDateTime() );

                this.Wait(-1, () => ProcessEverythingData(res));
            })
            .Catch(err => {
                traceError("Could not load / process all the user's data: " + err.Message);
                StartCoroutine(__CheckIfNeedsDefaultObjects());
            });
    }

    void ProcessEverythingData(NodeResponse res) {
        trace("LOADING", "Called: API.GetEverythingFromDB");
        trace("==API== DateTime when last loaded / web-hooked SF-DEV JSON:\n" + DateTime.Parse(res["jsonLoader"]["dateLoaded"]));

        ProcessUserData(res["user"]);
        JSONArray heroesJSON = res["heroes"].AsArray;
        JSONArray itemsJSON = res["items"].AsArray;
        JSONArray lootCratesJSON = res["lootCrates"].AsArray;
        JSONArray explorationsJSON = res["explorations"].AsArray;

        allItemsList.Clear();
        allHeroesList.Clear();
        allLootCratesList.Clear();
        allExplorationsList.Clear();

        foreach (JSONNode heroJSON in heroesJSON) {
            Hero hero = ProcessHeroData(heroJSON);
        }

        trace("itemsJSON count " + itemsJSON.Count);

        foreach (JSONNode itemJSON in itemsJSON) {
            Item item = ProcessItemData(itemJSON);
        }

        foreach (JSONNode lootCrateJSON in lootCratesJSON) {
            LootCrate lootCrate = ProcessLootCrate(lootCrateJSON);
        }

        foreach(JSONNode exploreJSON in explorationsJSON) {
            ProcessExplorationData(exploreJSON);//CurrentActiveExplorationList
        }

        StartCoroutine(__CheckIfNeedsDefaultObjects());
    }

    //void SavePlayerData() {

    //}

    private IEnumerator __CheckIfNeedsDefaultObjects() {
        int numTasks = 0, numHeroes = 1;

        if (allHeroesList.Count == 0) {
            trace("LOADING", "Generating NEW RANDOM HEROES...");

            allHeroesList.AllowAnyMongoID = true;

            for (int h = 0; h < numHeroes; h++) {
                Hero newHero = new Hero(
                    dataMan.heroDataList.GetRandom(),
                    gameMan.GetSeed()
                );

                allHeroesList.Add(newHero);
            }

            numTasks++;
            API.Heroes.AddMany(allHeroesList)
                .Then(res => --numTasks)
                .Catch(err => --numTasks);

            allHeroesList.AllowAnyMongoID = false;
        }

        while(numTasks>0) yield return new WaitForEndOfFrame();
        
        isLoaded = true;

        trace("LOADING", "Done Loading.");
        if(signals.OnGetEverything!=null) signals.OnGetEverything();
    }

    HeroData GetStarterHero() {
        List<HeroData> heroes = dataMan.heroDataList.FindAll(h => h.AwokenReference == null && !h.Identity.Contains("_monster_"));
        return heroes[UnityEngine.Random.Range(0, heroes.Count)];
    }

    [ContextMenu("Test Starting Hero List")]
    void TestStartingHeroList() {
        List<HeroData> heroes = dataMan.heroDataList.FindAll(h => h.AwokenReference == null && !h.Identity.Contains("_monster_"));
        string temp = "Starting Heroes\n";
        foreach (HeroData hero in heroes) {
            temp += hero.Identity + "\n";
        }
        Debug.Log(temp);
    }

    ////////////////////////////////////////////////////////////////////////

    public void ProcessUserData(JSONNode userJSON) {
        //trace(userJSON.ToString("  "));
        var jsonGame = userJSON["game"];

        API.Currency.ParseCurrencyData(jsonGame["currency"]);
        PlayerManager.Instance.Experience = jsonGame["xp"].AsInt;
        PlayerManager.Instance._lastLevel = jsonGame["lastLevel"].AsInt;
        PlayerManager.Instance._exploreSlots = jsonGame["actsZones"]["exploreSlots"].AsInt;
        PlayerManager.Instance.ShopExpansion = jsonGame["shopInfo"]["expansionSlots"].AsInt;
        PlayerManager.Instance.ActZoneCompleted = jsonGame["actsZones"]["completed"].AsInt;

        ProcessBoostSlots(jsonGame["boosts"]);
    }

    public void ProcessBoostSlots(JSONNode jsonBoosts) {
        if (!jsonBoosts.Exists()) {
            traceError("Missing 'boosts' data in JSON response of /everything/ REST API call.");
            return;
        }

        JSONNode jsonCurrency = jsonBoosts["currency"];
        JSONArray jsonSlots = jsonBoosts["slots"].AsArray;

        if(!jsonCurrency.Exists() || !jsonSlots.Exists()) {
            trace(jsonBoosts.ToString("  "));
            traceError("Missing currency and/or slots in 'boosts' JSON response.");
            return;
        }

        PlayerManager.Instance.BoostsSlotsActive.Clear();

        int b=0;
        foreach (JSONNode jsonBoostSlot in jsonSlots) {
            string boostIdentity = jsonBoostSlot["identity"].AsDecodedURL();
            BoostSlot boostSlot = new BoostSlot();
            BoostData boostData = boostDataList.Find(boost => boost.Identity==boostIdentity);

            if (boostData==null && boostIdentity!=null && boostIdentity.Length>0) {
                traceError("WHERE IS THE BOOST-DATA, WHY DOES IT NOT EXISTS? " + boostIdentity);
                continue;
            }

            boostSlot.slotID = b;
            boostSlot.data = boostData;
            boostSlot.count = jsonBoostSlot["count"].AsInt;
            PlayerManager.Instance.BoostsSlotsActive.Add(boostSlot);

            b++;
        }

        //Set each of the boost currencies:
        EnumUtils.ForEach<BoostType>(boostType => {
            string boostIdentity = "boost_" + boostType.ToString().ToLower();
            int value = 0;

            if (jsonCurrency[boostIdentity].Exists()) {
                value = jsonCurrency[boostIdentity].AsInt;
            }

            boostType.SetAmount(value);
        });
    }

    public Hero ProcessHeroData(JSONNode heroJSON) {
        JSONNode gameData = heroJSON["game"];
        JSONNode randomSeeds = gameData["randomSeeds"];
        JSONArray skills = gameData["skills"].AsArray;

        if (!heroJSON.Exists() || !gameData.Exists()) {
            traceError("Couldn't ProcessHeroData, something's off about the JSON data:\n" + heroJSON.ToString("  "));
            return null;
        }

        HeroData heroData = heroDataList.GetByIdentity(gameData["identity"]);
        if(heroData==null) {
            traceError("Cannot create Hero with 'null' HeroData object: " + gameData["identity"]);
            return null;
        }

        Hero hero = new Hero(heroData, randomSeeds["variance"].AsInt);
        hero.Experience = gameData["xp"].AsInt;
        hero.ExploringActZone = gameData["exploringActZone"].AsInt;
        hero.lastUsedTapAbility = DateTime.Parse(gameData["dateLastUsedTapAbility"]);
        hero.CustomName = gameData["customName"];
        hero.QualityLevel = gameData["qualityLevel"].AsInt;

        //Set skill levels:
        /*var skillsArr = hero.SkillLevels.Keys.ToArray();
        for(int s=0; s<skillsArr.Length; s++) {
            int skillLevel = skills[s]["level"].AsInt;
            hero.SkillLevels[skillsArr[s]] = skillLevel;
        }*/

        hero.MongoID = heroJSON["id"].AsInt;

        trace("LOADING", heroJSON.ToString("  "));
        
        trace("LOADING", "Added " + hero.DebugID + " to player hero-list.");
        allHeroesList.Add(hero);
        
        return hero;
    }

    public Item ProcessItemData(JSONNode itemJSON) {
        JSONNode gameData = itemJSON["game"];
        JSONNode randomSeeds = gameData["randomSeeds"];
        ItemData itemData = itemDataList.GetByIdentity(gameData["identity"]);
        Item item;

        try {
            item = new Item(
                itemData,
                randomSeeds["quality"].AsInt,
                randomSeeds["affix"].AsInt,
                randomSeeds["itemLevel"].AsInt,
                randomSeeds["variance"].AsInt,
                gameData["magicFind"].AsFloat,
                gameData["itemLevel"].AsInt,
                gameData["variance"].AsInt
            );
        } catch(Exception err) {
            traceError("Item is causing an error: " + itemJSON.ToString("  "));
            throw err;
        }

        item.isIdentified = gameData["isIdentified"].AsBool;
        item.isResearched = gameData["isResearched"].AsBool;

        item.MongoID = itemJSON["id"].AsInt;
        item.heroID = gameData["heroEquipped"].AsInt;

        if(item.heroID > 0) {
            Hero hero = GetHeroByMongoID(item.heroID);
            
            if (hero==null) {
                traceError("ERROR! The item is apparently equipped to a Hero we don't actually have!");
                API.Items.Unequip(item, new CurrencyManager.Cost(CurrencyTypes.GEMS, 0))
                    .Then(res => trace("Item successfully unequipped."))
                    .Catch(err => traceError("Could not unequip item: " + GameAPIManager.GetErrorMessage(err)));

            } else {
                trace("LOADING", "Equipped " + item.DebugID + " to hero: " + hero.DebugID);
                EquipmentType type = item.data.EquipType;

                var equipped = hero.EquipedItems;
                if (equipped.ContainsKey(type) && equipped[type]!=null) {
                    traceWarn(
                        "Hero is already equipped with: " + type + " == " + equipped[type].DebugID + "\n" +
                        "Unequipping item (without cost penalty)"
                        );
                    API.Items.Unequip(item, CurrencyTypes.GOLD, 0, isForced: true);
                    //traceError("Should probably de-equip it OR the last processed item.");
                } else {
                    hero.EquipedItems[type] = item;
                }
            }
        }

        allItemsList.Add(item);
        
        return item;
    }

    public LootCrate ProcessLootCrate(JSONNode lootCrateJSON) {
        JSONNode gameData = lootCrateJSON["game"];
        LootCrate lootCrate = new LootCrate(
            gameData["lootTableIdentity"],
            gameData["itemLevel"].AsFloat,
            gameData["variance"].AsFloat,
            crateTypeDataList.GetByIdentity(gameData["crateTypeIdentity"]),
            gameData["magicFind"].AsFloat
        );
        lootCrate.MongoID = lootCrateJSON["id"].AsInt;
        lootCrate.ExplorationID = gameData["explorationId"].AsInt;

        allLootCratesList.Add(lootCrate);

        return lootCrate;
    }

    public ActiveExploration ProcessExplorationData(JSONNode exploreJSON) {
        if(exploreJSON==null) {
            traceError("Could not parse Exploration JSON from server.");
            return null;
        }

        JSONNode gameData = exploreJSON["game"];
        int actZoneID = gameData["actZoneID"].AsInt;
        DateTime dateStarted = DateTime.Parse(gameData["dateStarted"]);
        int[] partyIDs = gameData["party"].AsArrayInt();

        ZoneData zone = zoneDataList.Find(z => z.ActZoneID == actZoneID);
        List<Hero> party = new List<Hero>();

        foreach(int heroID in partyIDs) {
            Hero partyHero = allHeroesList.Find(h => h.MongoID == heroID);
            party.Add(partyHero);
        }

        Hero groupLeader = null;
        if (party.Count>0) {
            groupLeader = party[0]; //Always selects the 1st hero as the leader
        } else {
            traceError("Your exploration does not have a party list! You should probably clear your Exploration data to start a clean slate.");
        }
        if (zone==null) {
            traceError("ActiveExploration has an invalid zone! " + actZoneID);
            return null;
        }

        if(party==null || party.Count==0) {
            traceError("ActiveExploration does not have any party / heroes! ActZoneID#" + zone.ActZoneID);
            API.Explorations.Remove(zone.ActZoneID)
                .Then( res => {
                    trace("Removed ActiveExploration on ActZoneID: " + zone.ActZoneID);
                });
            return null;
        }

        if (groupLeader == null) {
            traceError("Missing a valid 'groupLeader' in ActZone '{0}': ".Format2(actZoneID) + gameData["groupLeader"]);
            return null;
        }

        ActiveExploration exploration = new ActiveExploration(
            zone,
            party,
            dateStarted,
            groupLeader
        );

        exploration.MongoID = exploreJSON["id"].AsInt;

        allExplorationsList.Add(exploration);

        return exploration;
    }

    ////////////////////////////////////////////////////////////////////////

    private static List<T> FindByIdentities<T>(JSONNode jsonData, DataList<T> fullList) where T : BaseData {
        string[] identities = JSONManager.SplitArray(jsonData);
        return fullList.GetByIdentities(identities);
    }

    private static List<T> GetEnumList<T>(JSONNode jsonData) {
        string[] identities = JSONManager.SplitArray(jsonData);

        List<T> list = new List<T>();

        foreach (string id in identities) {
            list.Add((T) System.Enum.Parse(typeof(T), id));
        }

        return list;
    }

    ///////////////////////// Special Getters:

    public List<ZoneData> GetZonesByAct(int act, ZoneDifficulty difficulty = ZoneDifficulty.Normal) {
        return zoneDataList.FindAll(zone => zone.Act == act && zone.Difficulty == difficulty);
    }

    public ZoneData GetZonesByActAndZoneID(int act, int zone, ZoneDifficulty difficulty = ZoneDifficulty.Normal) {
        return zoneDataList.Find(z => z.Act == act && z.Zone == zone && z.Difficulty == difficulty);
    }

    public Hero GetHeroByMongoID(int mongoID) {
        return allHeroesList.Find(h => h.MongoID == mongoID);
    }

    public List<StorySection> GetStorySegmentsBySection(string section) {
        return storyDataList.FindAll(story => story.Section == section).OrderBy(story => story.Order).ToList();
    }

    public Item GetItemByMongoID(int mongoID) {
        return allItemsList.Find(i => i.MongoID == mongoID);
    }

    public List<ItemData> GetItemsByTierAndType(int tier, EquipmentType eType) {
        return itemDataList.FindAll(i => i.Tier == tier && i.EquipType == eType);
    }

    public List<ItemData> GetItemsByLootTableFilters(ZoneData data) {
        List<ItemData> itemOptions = new List<ItemData>();
        List<int> tiers = data.LootTable.tierChance.Keys.ToList();
        List<EquipmentType> types = data.LootTable.itemTypes.Keys.ToList();
        List<CurrencyTypes> currencyTypes = data.LootTable.AvailableCurrencyTypes;

        // Zone Items
        foreach (int tier in tiers) {
            foreach (EquipmentType type in types) {
                if (type == EquipmentType.None) {
                    if (currencyTypes.Count > 0) {
                        // Add Currency Here
                        foreach (CurrencyTypes currency in currencyTypes) {
                            itemOptions.AddRange(itemDataList.FindAll(i => i.EquipType == type && ((ItemCurrency) i).CurrencyType == currency));
                        }
                    }
                } else {
                    // Add equipment here
                    itemOptions.AddRange(itemDataList.FindAll(i => i.Tier == tier && i.EquipType == type));
                }
            }
        }

        // Boss Items
        tiers = data.BossFight.lootTable.tierChance.Keys.ToList();
        types = data.BossFight.lootTable.itemTypes.Keys.ToList();
        currencyTypes = data.BossFight.lootTable.AvailableCurrencyTypes;

        foreach (int tier in tiers) {
            foreach (EquipmentType type in types) {
                if (type == EquipmentType.None) {
                    if (currencyTypes.Count > 0) {
                        // Add Currency Here
                        foreach (CurrencyTypes currency in currencyTypes) {
                            itemOptions.AddRange(itemDataList.FindAll(i => i.EquipType == type && ((ItemCurrency) i).CurrencyType == currency));
                        }
                    }
                } else {
                    // Add equipment here
                    itemOptions.AddRange(itemDataList.FindAll(i => i.Tier == tier && i.EquipType == type));
                }
            }
        }

        return itemOptions;
    }

    public List<ItemData> GetItemsByLootTableFilters(List<int> tiers, List<EquipmentType> types, List<CurrencyTypes> currencyTypes) {
        List<ItemData> itemOptions = new List<ItemData>();

        foreach (int tier in tiers) {
            foreach (EquipmentType type in types) {
                if (type == EquipmentType.None) {
                    if (currencyTypes.Count > 0) {
                        // Add Currency Here
                        foreach (CurrencyTypes currency in currencyTypes) {
                            itemOptions.AddRange(itemDataList.FindAll(i => i.EquipType == type && ((ItemCurrency) i).CurrencyType == currency));
                        }
                    }
                } else {
                    // Add equipment here
                    itemOptions.AddRange(itemDataList.FindAll(i => i.Tier == tier && i.EquipType == type));
                }
            }
        }

        return itemOptions;
    }

    public List<ItemData> GetItemsByLootTableFilters(int tier, EquipmentType eType, CurrencyTypes cType) {
        //Debug.Log("Getting Currency Item [tier: " + tier + "][eType: " + eType + "][cType: " + cType + "]");
        if (eType == EquipmentType.None && cType != CurrencyTypes.NONE) {
            Debug.Log("Getting Currency Item in if statement");
            foreach(ItemCurrency item in itemDataList.FindAll(i => i.EquipType == eType && ((ItemCurrency) i).CurrencyType == cType)){
                Debug.Log(" --- [Item List] " + item.Name);
            }
            return itemDataList.FindAll(i => i.EquipType == eType && ((ItemCurrency) i).CurrencyType == cType);
        }
        return itemDataList.FindAll(i => i.Tier == tier && i.EquipType == eType);
    }

    public List<Item> GetItemsAvailable() {
        return allItemsList.FindAll(i => i.heroID < 1);
    }

    public List<Item> GetItemsAvailable(ItemFilterType filter = ItemFilterType.Default, bool isIncludeEquipped = false) {
        var results = allItemsList.FindAll(i => (isIncludeEquipped || i.heroID < 1) && i.data.EquipType != EquipmentType.None);
        Comparison<Item> sortFunc;
        var statsFunc = GetItemsStatsFunc(filter);

        sortFunc = (a, b) => {
            if (b.isIdentified && !a.isIdentified) return 1;
            if (a.isIdentified && !b.isIdentified) return -1;

            int result = statsFunc(b) - statsFunc(a);
            if (result == 0 && a.isIdentified && b.isIdentified) {
                return b.Quality - a.Quality;
            }

            return result;
        };

        results.Sort(sortFunc);

        return results;
    }

    public List<Item> GetItemsAvailable(EquipmentType type, ItemFilterType filter=ItemFilterType.Default, bool isIncludeEquipped=false) {
        var results = allItemsList.FindAll(i => (isIncludeEquipped || i.heroID < 1) && i.data.EquipType == type);
        Comparison<Item> sortFunc;
        var statsFunc = GetItemsStatsFunc(filter);

        sortFunc = (a, b) => {
            if(b.isIdentified && !a.isIdentified) return 1;
            if(a.isIdentified && !b.isIdentified) return -1;

            int result = statsFunc(b) - statsFunc(a);
            if (result==0 && a.isIdentified && b.isIdentified) {
                return b.Quality - a.Quality;
            }

            return result;
        };

        results.Sort(sortFunc);

        return results;
    }

    public Func<Item, int> GetItemsStatsFunc(ItemFilterType filter) {
        switch (filter) {
            case ItemFilterType.CounterChance: return item => item.GetStats(SecondaryStats.CounterChance);
            case ItemFilterType.CriticalChance: return item => item.GetStats(SecondaryStats.CounterChance);
            case ItemFilterType.CriticalDamage: return item => item.GetStats(SecondaryStats.CriticalDamage);
            case ItemFilterType.Dodge: return item => item.GetStats(SecondaryStats.Dodge);
            case ItemFilterType.TreasureFind: return item => item.GetStats(SecondaryStats.TreasureFind);
            case ItemFilterType.SkillLevel: return item => item.GetStats(SecondaryStats.SkillLevel);
            case ItemFilterType.SkillDamage: return item => item.GetStats(SecondaryStats.SkillDamage);
            case ItemFilterType.MonsterFind: return item => item.GetStats(SecondaryStats.MonsterFind);
            case ItemFilterType.Intelligence: return item => item.GetStats(PrimaryStats.Intelligence);
            case ItemFilterType.Strength: return item => item.GetStats(PrimaryStats.Strength);
            case ItemFilterType.Speed: return item => item.GetStats(PrimaryStats.Speed);
            case ItemFilterType.Vitality: return item => item.GetStats(PrimaryStats.Vitality);
            case ItemFilterType.Defense: return item => item.GetStats(CoreStats.Defense);
            case ItemFilterType.Damage: return item => item.GetStats(CoreStats.Damage);
            case ItemFilterType.Health: return item => item.GetStats(CoreStats.Health);
            case ItemFilterType.Variance: return item => Mathf.RoundToInt(item.Variance);
            case ItemFilterType.MagicFind: return item => Mathf.RoundToInt(item.MagicFind);
            case ItemFilterType.ItemLevel: return item => Mathf.RoundToInt(item.ItemLevel);
            case ItemFilterType.Affixes: return item => item.Affixes.Count;
            case ItemFilterType.Default: return item => item.isIdentified ? Mathf.RoundToInt(item.ItemLevel) : 0;
            default: return null;
        }
    }

    public List<LootCrate> GetLootCratesByExploration(int explorationID) {
        return allLootCratesList.FindAll(crate => {
            return crate.ExplorationID == explorationID;
        });
    }

    public List<LootCrate> GetLootCratesByLootTableIdentity(string lootTableIdentity) {
        return allLootCratesList.FindAll(crate => crate.lootTableIdentity == lootTableIdentity );
    }

    public RewardsByPlayerLevel GetPlayerLevelRewardsRewardsByLevel(int level) {
        return rewardsByPlayerLevelDataList.Find(rwd => { return rwd.Level == level; });
    }
    
    public RetireRewardsData GetRetireRewardsForHero(Hero hero) {
        //return GetRetireRewardsForHero(hero.Quality, (hero.data.AwokenReference != null));
        bool awoken = (hero.data.AwokenReference != null) || (hero.Quality == HeroQuality.Legendary && hero.Type == HeroType.Monster);
        Debug.Log("Awoken: " + awoken);
        RetireRewardsData reward = retireRewardsDataList.Find(rwd => { return rwd.Quality == hero.Quality && (rwd.RequireAwoken == awoken); });
        print("Reward table chosen: " + reward.Identity);
        return reward;
    }
    
    [ContextMenu("ERDS -> Test Demo Data.")]
    public void TestDemoData() {
        foreach(var boostSlot in playerMan.BoostsSlotsActive) {
            API.Users.BoostDecrease(boostSlot.slotID);
        }
    }
}

[Serializable]
public class DataList<T> : List<T> where T : BaseData {
    public T GetByIdentity(string identity) {
        if(identity==null) return null;
        identity = identity.ToLower();
        return Find(element => element.Identity.ToLower() == identity);
    }

    public T GetByID(int id) {
        return Find(element => element.ID==id);
    }

    public T GetRandom() {
        if (Count < 1) return null;
        return this[UnityEngine.Random.Range(0, Count)];
    }

    public List<T> GetByIdentities(string[] identities) {
        List<T> list = new List<T>();
        foreach (string identity in identities) {
            T found = GetByIdentity(identity);
            if (found == null) continue;
            list.Add(found);
        }
        return list;
    }

    public List<T> GetByIDs(int[] ids) {
        List<T> list = new List<T>();
        foreach (int id in ids) {
            T found = GetByID(id);
            if (found == null) continue;
            list.Add(found);
        }
        return list;
    }
}

[Serializable]
public class MongoList<T> : List<T> where T : IMongoData {
    public bool AllowAnyMongoID = false;
    public void Add(T item) {
        if(!AllowAnyMongoID && item.MongoID<1) {
            Tracer.traceError("Whoa, this " + item.DebugID + " does not have a MongoID yet! Cannot add it.");
            return;
        }

        T hasItem = Find((T i) => i.Equals(item) || i.MongoID == item.MongoID);
        if (hasItem!=null) {
            Tracer.traceError("Already contains MongoID object: " + item.DebugID);
            return;
        }

        base.Add(item);
    }

    public void Remove(T item) {
        if (!AllowAnyMongoID && item.MongoID < 1) {
            Tracer.traceError("Whoa, this " + item.DebugID + " does not have a MongoID yet! Cannot remove it.");
            return;
        }

        T hasItem = Find((T i) => i.Equals(item) || i.MongoID == item.MongoID);
        if (hasItem==null) {
            Tracer.traceError("Cannot remove invalid MongoID object: " + item.DebugID);
            return;
        }

        item.MongoID = -1;

        base.Remove(item);
    }
}
