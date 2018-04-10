using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using RSG;
using NodeJS;
using UnityEngine.SceneManagement;
using SimpleJSON;

using NodePromise = RSG.IPromise<NodeJS.NodeResponse>;
using System;

public class GameAPIManager : NodeJSManager<GameAPIManager> {

    static string GITINFO_LONG_HASH = "fab8656876c2e0895aec422ec395a413dc56330f";

    string _githubLongHash;
    NodeResponse _lastGoodLoginResponse;
    
    //Made individual Child APIs for each sections / tables on the Database:
    public ShopAPI Shop = new ShopAPI();
    public UserAPI Users = new UserAPI();
    public ItemAPI Items = new ItemAPI();
    public HeroAPI Heroes = new HeroAPI();
    public CurrencyAPI Currency = new CurrencyAPI();
    public LootCrateAPI LootCrates = new LootCrateAPI();
    public ExplorationAPI Explorations = new ExplorationAPI();
    public MessageAPI Messages = new MessageAPI();
    public ResearchAPI Research = new ResearchAPI();
    
    public bool IsMatchingGitInfo {
        get { return GITINFO_LONG_HASH == _githubLongHash; }
    }

    void NotifyUserLoginOK() {
        if (signals.OnUserLoginOK == null || _lastGoodLoginResponse == null) return;

        this.Wait(-1, () => signals.OnUserLoginOK(_lastGoodLoginResponse));
    }

    public IPromise AutoLogin() {
        trace("NODE", "AutoLogin started...");

        return new Promise((resolve, reject) => {
            //If there isn't any Username / Password stored in PlayerPrefs, don't bother auto-login!
            if (!ReloadUserFromPrefs()) {
                reject(new Exception("PlayerPrefs not set yet."));
                return;
            }

            Users.Login(this._authUser, this._authPassMD5, true)
                .Then(res => resolve())
                .Catch(err => reject(err));
        });
    }

    static void SetMongoID(IMongoData data, JSONNode json) {
        data.MongoID = json["id"].AsInt;
    }

    ////////////////////////////////////////////////////////////////////////////////////////
    
    public class Child_API<T> where T : IMongoData {
        public static GameAPIManager API { get { return GameAPIManager.API; } }

        public string apiName = "???";
        public string updateURLFormat = "PUT::/{0}/{1}/{2}";

        protected NodePromise __Update(T obj, string property, object jsonData = null, int id=-1) {
            if (id < 0) id = obj.MongoID;
            if (id < 1) {
                traceError("Invalid '"+ apiName + "' ID, can't update '" + property + "' of it: " + obj.DebugID);
            }

            return API.SendWhileLoggedIn(string.Format(updateURLFormat, apiName, id, property), jsonData: jsonData)
                .Catch(err => {
                    traceError(string.Format("Failed to update '{0}' for {1}: {2}\n{3}", property, apiName, obj.DebugID, GetErrorMessage(err)));
                });
        }

        public NodePromise Remove(T obj) {
            if (obj.MongoID < 1) {
                traceError(apiName.ToTitleCase() + " has an invalid ID, cannot remove it from DB: " + obj.MongoID);
            }

            string url = string.Format("DELETE::/{0}/{1}/remove", apiName, obj.MongoID);
            return API.SendWhileLoggedIn(url)
                .Catch(err => {
                    traceError(string.Format("Could not remove {0}: {1}", apiName.ToTitleCase(), url));
                });
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////

    public class UserAPI : Child_API<IMongoData> {
        public UserAPI() {
            apiName = "user";
        }

        public NodePromise Login(string username, string passwordRaw, bool isMD5 = false) {
            string passwordMD5 = isMD5 ? passwordRaw : passwordRaw.ToMD5();

            return API.SendAPI("POST::/user/public/login", jsonData: new {
                username = username,
                _password = passwordMD5
            })
                .Then(res => {
                    JSONNode gitInfo = res["gitInfo"];
                    string gitBranch = "master";
                    string gitLongHash = "none";
                    //trace("res: " + res.pretty);

                    if (gitInfo != null) {
                        gitBranch = gitInfo["branch"];
                        gitLongHash = gitInfo["long"];
                    }

                    trace("NODE", "Storing Username & Token in PlayerPrefs");

                    API._githubLongHash = gitLongHash;
                    API.SetCurrentUser(res["username"], res["login"]["token"], passwordMD5);
                    
                    if (!API.IsMatchingGitInfo) {
                        trace(string.Format("Latest Github branch '{0}' hash: {1}", gitBranch, gitLongHash));

                        traceError("SERVER-SIDE has been updated!\nMake sure you update / pull the latest Unity project to use the latest API changes!");
                    }

                    API._lastGoodLoginResponse = res;

                    API.NotifyUserLoginOK();
                });
        }

        public NodePromise Logout() {
            return API.SendWhileLoggedIn("GET::/user/logout")
                .Catch(err => traceError("Could not logout successfully: " + GetErrorMessage(err)));
        }

        public NodePromise Signup(string name, string username, string email, string passwordRaw, bool autoLogin = true) {
            NodePromise promise = API.SendAPI("POST::/user/public/add", jsonData: new {
                name = name,
                username = username,
                email = email,
                _password = passwordRaw.ToMD5()
            });

            if (autoLogin) {
                promise
                    .Then(res => {
                        trace("NODE", "Successfully created!");
                        return Login(username, passwordRaw);
                    })
                    .Then(res => {
                        trace("NODE", "Successfully auto-logged in!");
                    });
            }

            return promise;
        }

        public NodePromise GetEverything() {
            return API.SendWhileLoggedIn("GET::/user/everything")
                .Catch(err => traceError("Unable to load ALL User data: " + GetErrorMessage(err)));
        }

        public NodePromise GetAnalytics() {
            return API.SendWhileLoggedIn("GET::/user/analytics")
                .Catch(err => traceError("Unable to get User's analytics data: " + GetErrorMessage(err)));
        }

        public NodePromise ActZoneComplete(int actZone) {
            return API.SendWhileLoggedIn("POST::/user/completed-act-zone", jsonData: new { actZone = actZone })
                .Then(res => {
                    trace("Completed Act Zone: " + res.pretty);
                });
        }

        public NodePromise SetXP(int newXP) {
            return API.SendWhileLoggedIn("PUT::/user/xp", jsonData: new { xp = newXP })
                .Catch(err => {
                    traceError("Failed to update User's XP: " + GetErrorMessage(err));
                });
        }

        public NodePromise SetLastLevel(int newLevel) {
            return API.SendWhileLoggedIn("PUT::/user/lastLevel", jsonData: new { lastLevel = newLevel })
                .Catch(err => {
                    traceError("Failed to update User's LastLevel: " + GetErrorMessage(err));
                });
        }

        public NodePromise SetExploreSlots(int exploreSlots) {
            return API.SendWhileLoggedIn("PUT::/user/explore-slots", jsonData: new { exploreSlots = exploreSlots })
                .Catch(err => {
                    traceError("Failed to update User's exploreSlots: " + GetErrorMessage(err));
                });
        }

        public NodePromise BoostAddSlot(CurrencyManager.Cost cost) {
            object jsonData = new { cost = cost.ToObject() };

            return API.SendWhileLoggedIn("PUT::/user/boosts/add", jsonData: jsonData)
                .Then(res => {
                    DataManager.Instance.ProcessBoostSlots(res["boosts"]);
                    API.Currency.ParseCurrencyData(res["currency"]);
                    trace(res.pretty);

                    if (signals.OnBoostsUpdated != null) signals.OnBoostsUpdated();
                })
                .Catch(err => {
                    traceError("Failed to add boost slot: " + GetErrorMessage(err));
                });
        }

        public NodePromise BoostAddCurrency(CurrencyManager.BoostCost cost) {
            return API.SendWhileLoggedIn("PUT::/user/boosts/currency", jsonData: cost.ToObject()) 
                .Then(res => {
                    API.Currency.ParseBoostCurrencyData(res["currency"]);
                })
                .Catch(err => {
                    traceError("Failed to add boost currency: " + GetErrorMessage(err));
                });
        }

        public NodePromise BoostInfo(int boostID) {
            return API.SendWhileLoggedIn("GET::/user/boosts/{0}".Format2(boostID))
                .Then(res => { trace("BoostInfo: " + res.pretty); })
                .Catch(err => {
                    traceError("Failed to get boost info at #{0}: ".Format2(boostID) + GetErrorMessage(err));
                });
        }

        public NodePromise BoostActivate(int boostID, BoostData boostData, int forceCount = 0) {
            object jsonData = new { identity = boostData.Identity, forceCount = forceCount };

            return API.SendWhileLoggedIn("PUT::/user/boosts/{0}/activate".Format2(boostID), jsonData: jsonData)
                .Then(res => {
                    var jsonBoost = res["boost"];
                    var boostSlot = playerMan.BoostsSlotsActive[boostID];

                    boostSlot.data = boostData;
                    boostSlot.count = jsonBoost["count"].AsInt;

                    API.Currency.ParseBoostCurrencyData(res["currency"]);

                    if (signals.OnBoostsUpdated != null) signals.OnBoostsUpdated();
                })
                .Catch(err => {
                    traceError("Failed to get boost info at #{0}: ".Format2(boostID) + GetErrorMessage(err));
                });
        }

        public NodePromise BoostDecrease(int boostID) {
            return API.SendWhileLoggedIn("PUT::/user/boosts/{0}/decrease".Format2(boostID))
                .Then(res => {
                    var jsonBoost = res["boost"];
                    var boostSlot = playerMan.BoostsSlotsActive[boostID];

                    boostSlot.count = jsonBoost["count"].AsInt;
                    if (boostSlot.count == 0) boostSlot.data = null;

                    if (signals.OnBoostsUpdated != null) signals.OnBoostsUpdated();
                })
                .Catch(err => {
                    traceError("Failed to get boost info at #{0}: ".Format2(boostID) + GetErrorMessage(err));
                });
        }

        public NodePromise BoostClearAll() {
            return API.SendWhileLoggedIn("DELETE::/user/boosts/clear-all")
                .Then(res => {
                    playerMan.BoostsSlotsActive.Clear();
                    EnumUtils.ForEach<BoostType>(type => type.SetAmount(0));
                    
                    if (signals.OnBoostsUpdated != null) signals.OnBoostsUpdated();
                })
                .Catch(err => {
                    traceError("Failed to clear all boost slots: " + GetErrorMessage(err));
                });
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////

    public class CurrencyAPI : Child_API<IMongoData> {
        public CurrencyAPI() {
            apiName = "user";
        }

        public NodePromise AddCurrency(CurrencyTypes currencyType, int amount) {
            return __AddCurrency(CurrencyManager.ConvertToCurrencyObject(currencyType, amount));
        }

        public NodePromise AddCurrency(CurrencyManager.Cost cost) {
            trace(Newtonsoft.Json.JsonConvert.SerializeObject( cost.ToObject() ));
            return __AddCurrency(cost.ToObject());
        }

        public NodePromise Reset() {
            var cost = new CurrencyManager.Cost();
            EnumUtils.ForEach<CurrencyTypes>(type => cost.AddOrSet(type, -99999999));
            return __AddCurrency(cost.ToObject());
        }

        ///////////////////////////////// PRIVATE:

        NodePromise __AddCurrency(object jsonData) {
            return API.SendWhileLoggedIn("PUT::/user/currency", jsonData: jsonData)
            .Then(res => ParseCurrencyData(res.json["data"]))
            .Catch(err => traceError("Failed to add/remove currency: " + GetErrorMessage(err)));
        }

        public void ParseCurrencyData(JSONNode currencies) {
            EnumUtils.ForEach<CurrencyTypes>(type => {
                if (type == CurrencyTypes.NONE) return;

                string fieldName = type.ToString().ToCamelCase();

                if (!currencies[fieldName].Exists()) {
                    traceError("Currency type '{0}' does not exist on server-side, setting to zero (0).".Format2(fieldName));
                    type.SetAmount(0, true);
                    return;
                }

                type.SetAmount(currencies[fieldName].AsInt, true);
            });

            if (signals.OnAllCurrenciesUpdated != null) signals.OnAllCurrenciesUpdated();
        }

        public void ParseBoostCurrencyData(JSONNode currencies) {
            EnumUtils.ForEach<BoostType>(type => {
                if (type == BoostType.None) return;

                string fieldName = "boost_" + type.ToString().ToLower();

                if (!currencies[fieldName].Exists()) {
                    traceError("Boost Currency type '{0}' does not exist on server-side, setting to zero (0).".Format2(fieldName));
                    type.SetAmount(0, true);
                    return;
                }

                type.SetAmount(currencies[fieldName].AsInt, true);
            });

            if (signals.OnAllCurrenciesUpdated != null) signals.OnAllCurrenciesUpdated();
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////

    public class HeroAPI : Child_API<Hero> {
        public HeroAPI() {
            apiName = "hero";
        }

        public NodePromise Add(Hero hero) {
            if (hero.MongoID > 0) throw new Exception("This Hero seems like it was already added to the DB: " + hero.DebugID);

            return AddMany(new List<Hero>(1) { hero });
        }

        public NodePromise AddMany(List<Hero> heroes) {
            object[] heroesJSON = new object[heroes.Count];

            for (int h = 0; h < heroes.Count; h++) {
                Hero hero = heroes[h];

                if (hero.MongoID > 0) {
                    throw new Exception("This Hero seems like it was already added to the DB: " + hero.DebugID);
                }
                
                heroesJSON[h] = new {
                    identity = hero.data.Identity,
                    randomSeeds = new {
                        variance = hero.HeroVarianceSeed
                    }
                };
            }

            return API.SendWhileLoggedIn("POST::/hero/add", jsonData: new { list = heroesJSON })
                .Then(res => {
                    JSONArray heroesNewest = res.json["data"]["newest"].AsArray;

                    if (heroesNewest.Count != heroes.Count) {
                        throw new Exception("Heroes JSONArray and List<Hero> COUNT don't match!");
                    }

                    for (int h = 0; h < heroesNewest.Count; h++) {
                        JSONNode heroJSON = heroesNewest[h];
                        Hero hero = heroes[h];
                        SetMongoID(hero, heroJSON);
                    }
                })
                .Catch(err => traceError("Failed to add Heroes: " + GetErrorMessage(err)));
        }

        public NodePromise RemoveAll() {
            return API.SendWhileLoggedIn("DELETE::/hero/remove-all")
                .Catch(err => {
                    traceError("Could not remove all heroes: " + GetErrorMessage(err));
                });
        }

        public NodePromise SetXP(Hero hero, int xp) {
            return __Update(hero, "xp", new { xp = xp });
        }
        public NodePromise SetQualityLevel(Hero hero, int qlvl) {
            return __Update(hero, "qualityLevel", new { qualityLevel = qlvl });
        }

        public NodePromise SwapIdentity(Hero hero, string newIdentity, CurrencyTypes currency, int amount) {
            return SwapIdentity(hero, newIdentity, new CurrencyManager.Cost(currency, amount));
        }

        public NodePromise SwapIdentity(Hero hero, string newIdentity, CurrencyManager.Cost cost) {
            HeroData heroData = dataMan.heroDataList.GetByIdentity(newIdentity);
            if(heroData==null) {
                traceError("Invalid Hero Data supplied, doesn't appear to exist in the JSON data: " + newIdentity);
            }

            return __Update(hero, "swap-identity", new { identity = newIdentity, cost = cost.ToObject() })
                .Then(res => {
                    var jsonHero = res["hero"]["game"];

                    hero.data = heroData;

                    API.Currency.ParseCurrencyData(res["currency"]);
                });
        }

        public NodePromise SetSkillLevels(Hero hero, Dictionary<Skill, int> skillLevels) {
            object[] jsonSkills = new object[skillLevels.Count];
            int index = 0;

            foreach (KeyValuePair<Skill, int> kv in skillLevels) {
                jsonSkills[index++] = new { identity = kv.Key.Identity, level = kv.Value };
            }

            object jsonData = new { skillLevels = jsonSkills };

            return __Update(hero, "skill-levels", jsonData)
                .Then( res => {
                    trace("Hero Skill levels set successfully:\n" + res.pretty);
                })
                .Catch(err => {
                    traceError("Could not set hero #{0} Skill-Level: {1}".Format2(hero.DebugID, err.Message));
                });
        }

        public NodePromise SetTapAbility(Hero hero, DateTime dateTapped) {
            return __Update(hero, "tap-ability", new { dateTapped = dateTapped.ToNodeDateTime() })
                .Then( res => {
                    trace(res.pretty);

                    DateTime dateFromServer = DateTime.Parse(res["game"]["dateLastUsedTapAbility"]);
                    hero.lastUsedTapAbility = dateTapped;
                })
                .Catch( err => {
                    traceError("Could not set hero #{0} Tap-Ability date last used: {1}".Format2(hero.DebugID, err.Message));
                });
        }

        public NodePromise SetExploringActZone(Hero hero, ZoneData zone) {
            hero.ExploringActZone = zone.ActZoneID;

            return __Update(hero, "exploring/" + zone.ActZoneID);
        }

        public NodePromise Rename(Hero hero, string customName) {
            hero.CustomName = customName;

            return __Update(hero, "rename", new { customName = customName })
                .Then( res => {
                    trace("Renamed Hero OK.");
                    trace(res.pretty);
                })
                .Catch( err => {
                    traceError("Could not rename hero #{0}: ".Format2(hero.DebugID));
                });
        }

        public NodePromise ResetExplorations() {
            return API.SendWhileLoggedIn("PUT::/hero/reset-explorations")
                .Catch(err => {
                    traceError("Could not Reset Exploration ActZones for all heroes: " + GetErrorMessage(err));
                });
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////

    public class ItemAPI : Child_API<IMongoData> {
        public ItemAPI() {
            apiName = "item";
        }

        public NodePromise GetList() {
            traceError("ItemGetList() not fully implemented yet, generally GetEverything is called instead.");

            return API.SendWhileLoggedIn("GET::/item/list")
                .Then(res => {
                    trace(res.pretty);

                    trace("TODO....");
                })
                .Catch(err => {
                    traceError("Could not retrieve list of user's items! " + GetErrorMessage(err));
                });
        }

        public NodePromise Add(Item item, int heroID = -1) {
            if (item.MongoID > 0) throw new Exception("This Item seems like it was already added to the DB: " + item.DebugID);

            return AddMany(new List<Item>(1) { item }, heroID);
        }

        public NodePromise AddMany(List<Item> items, int heroID = -1) {
            object[] itemsJSON = new object[items.FindAll(i => (i.data.Type != ItemType.Currency)).Count];
            List<Item> nonCurrencyItems = items.FindAll(i => (i.data.Type != ItemType.Currency));

            Item item;
            // Non currency items
            for (int h = 0; h < nonCurrencyItems.Count; h++) {
                item = nonCurrencyItems[h];

                if (item.MongoID > 0) {
                    throw new Exception("This Item seems like it was already added to the DB: " + item.DebugID);
                }

                itemsJSON[h] = new {
                    identity = item.data.Identity,
                    isIdentified = item.isIdentified,
                    itemLevel = item.BaseItemLevel,
                    variance = item.Variance,
                    magicFind = item.MagicFind,

                    randomSeeds = new {
                        quality = item.QualitySeed,
                        affix = item.AffixSeed,
                        itemLevel = item.ItemLevelSeed,
                        variance = item.VarianceSeed,
                    }
                };
            }
            // Currency items
            for (int h = 0; h < items.Count; h++) {
                item = items[h];

                if (item.MongoID > 0) {
                    throw new Exception("This Item seems like it was already added to the DB: " + item.DebugID);
                }

                // HANDLE the currency items here
                if (item.Type == ItemType.Currency) {
                    // API call to add the currency?
                    if (nonCurrencyItems.Count == 0)
                        return API.Currency.AddCurrency(((ItemCurrency) item.data).CurrencyType, ((ItemCurrency) item.data).Reward);
                    else
                        API.Currency.AddCurrency(((ItemCurrency) item.data).CurrencyType, ((ItemCurrency) item.data).Reward);
                }
            }

            if(items.Count==0) {
                traceError("Can't add NO items, need 1 or more!");
                //return null;
            }

            if (nonCurrencyItems.Count == 0) return null;

            return API.SendWhileLoggedIn("POST::/item/add", jsonData: new { list = itemsJSON, heroID = heroID })
                .Then(res => {
                    JSONArray itemsNewest = res.json["data"]["newest"].AsArray;

                    if (itemsNewest.Count != nonCurrencyItems.Count) {
                        throw new Exception("Items JSONArray and List<Item> COUNT don't match!");
                    }

                    for (int h = 0; h < itemsNewest.Count; h++) {
                        JSONNode itemJSON = itemsNewest[h];
                        item = nonCurrencyItems[h];
                        SetMongoID(item, itemJSON);
                    }
                })
                .Catch(err => traceError("Failed to add Items: " + GetErrorMessage(err)));
        }

        public NodePromise RemoveAll() {
            return API.SendWhileLoggedIn("DELETE::/item/remove-all")
                .Catch(err => {
                    traceError("Could not remove all items: " + GetErrorMessage(err));
                });
        }

        public NodePromise Randomize(EquipmentType itemType, int quantity) {
            return API.SendWhileLoggedIn(string.Format("POST::/item/random/{0}/{1}", itemType, quantity))
                .Catch(err => traceError("Failed to generate random items: " + GetErrorMessage(err)));
        }

        public NodePromise Identify(Item item) {
            return __Update(item, "identify");
        }

        public NodePromise EquipToHero(Item item, Hero hero) {
            return __Update(item, "equip-to/" + hero.MongoID);
        }

        public NodePromise Unequip(Item item, string costStr, bool isForced = false) {
            var cost = CurrencyManager.ParseToCost(costStr);
            return Unequip(item, cost, isForced);
        }

        public NodePromise Unequip(Item item, CurrencyTypes type, int amount, bool isForced = false) {
            return Unequip(item, new CurrencyManager.Cost(type, amount), isForced);
        }

        public NodePromise Unequip(Item item, CurrencyManager.Cost cost, bool isForced = false) {
            item.heroID = 0;
            object jsonData = new {
                cost = cost.ToObject(), //CurrencyManager.ConvertToCurrencyObject(costType, -Math.Abs(costAmount))
                force = isForced
            };
            return __Update(item, "unequip", jsonData);
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////

    public class LootCrateAPI : Child_API<IMongoData> {
        public LootCrateAPI() {
            apiName = "lootcrate";
        }

        public NodePromise Add(LootCrate lootCrate) {
            if (lootCrate.MongoID > 0) {
                traceError("The LootCrate appears to already exists on the DB, MongoID == " + lootCrate.MongoID);
                return null;
            }

            object jsonData = new {
                lootCrate = new {
                    explorationId = lootCrate.ExplorationID,
                    lootTableIdentity = lootCrate.lootTableIdentity,
                    crateTypeIdentity = lootCrate.CrateType.ToString(),
                    itemLevel = lootCrate.ItemLevel,
                    variance = lootCrate.Variance,
                    magicFind = lootCrate.MagicFind,
                }
            };

            return API.SendWhileLoggedIn("POST::/lootcrate/add", jsonData: jsonData)
                .Then(res => {
                    SetMongoID(lootCrate, res.json["data"]);
                })
                .Catch(err => {
                    traceError("Could not add new LootCrate! " + GetErrorMessage(err));
                });
        }

        public NodePromise Remove(LootCrate lootCrate) {
            if (lootCrate.MongoID <= 0) {
                traceError("The LootCrate doesn't have a MongoID! " + lootCrate.MongoID);
                return null;
            }

            return API.SendWhileLoggedIn("DELETE::/lootcrate/remove/" + lootCrate.MongoID)
                .Catch(err => {
                    traceError("Could not remove the LootCrate: " + GetErrorMessage(err));
                });
        }

        public IPromise<List<LootCrate>> GetList() {
            return API.SendWhileLoggedIn("GET::/lootcrate/list/")
                .Then(res => {
                    JSONArray jsonCrates = res.json["data"].AsArray;

                    var list = new List<LootCrate>();

                    DataManager.Instance.allLootCratesList.Clear();

                    foreach (JSONNode jsonCrate in jsonCrates) {
                        LootCrate lootCrate = dataMan.ProcessLootCrate(jsonCrate);

                        list.Add(lootCrate);
                    }

                    return list;
                })
                .Catch(err => {
                    traceError("Could not get list of LootCrates: " + GetErrorMessage(err));
                    traceError(err.StackTrace);
                });
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////

    public class ShopAPI : Child_API<IMongoData> {
        public ShopAPI() {
            apiName = "shop";
        }

        public IPromise<ShopRefreshKey> GetSeed() {
            return API.SendWhileLoggedIn("GET::/shop/key")
                .Then(res => {
                    return __OnShopSeedResponse(res);
                })
                .Catch(err => {
                    traceError("Could not GET the Shop Seed key: " + err.StackTrace);
                });
        }

        public IPromise<ShopRefreshKey> RefreshSeed(CurrencyManager.Cost cost) {
            object jsonData = new {
                cost = cost.ToObject()
            };

            return API.SendWhileLoggedIn("PUT::/shop/key/refresh", jsonData: jsonData)
                .Then(res => {
                    API.Currency.ParseCurrencyData(res["currency"]);
                    return __OnShopSeedResponse(res);
                })
                .Catch(err => {
                    traceError("Could not REFRESH the Shop Seed key: " + GetErrorMessage(err));
                });
        }

        ShopRefreshKey __OnShopSeedResponse(NodeResponse res) {
            JSONNode refreshKey = res["refreshKey"];
            int seed = refreshKey["seed"].AsInt;
            JSONArray jsonPurchased = refreshKey["purchased"].AsArray;

            trace("SHOP", "Server 'dateExpires' = " + refreshKey["dateExpires"]);
            DateTime dateExpires = DateTime.Parse(refreshKey["dateExpires"]);
            int ItemCount = DataManager.Instance.globalData.GetGlobalAsInt(GlobalProps.SHOP_BASE_ITEM_COUNT) + PlayerManager.Instance.ShopExpansion;
            print("purchased size: " + ItemCount);
            int[] purchased = new int[ItemCount];

            //Default all purchased item indices to -1 (basically a non-existant seed)
            for (int i = 0; i < ItemCount; i++) {
                purchased[i] = -1;
            }

            foreach (JSONNode purchaseIndex in jsonPurchased) {
                purchased[purchaseIndex.AsInt] = seed;
            }

            trace("SHOP", "API SHOP PURCHASE INDICES: " + refreshKey["purchased"].ToString());

            return new ShopRefreshKey() {
                seed = seed,
                dateExpires = dateExpires,
                purchased = purchased
            };
        }

        bool PrepareItemToBuy(Item item, CurrencyTypes type, int cost, out object costObject, out object[] list) {
            costObject = CurrencyManager.ConvertToCurrencyObject(type, cost);

            list = new object[1];
            list[0] = new {
                identity = item.data.Identity,
                isIdentified = item.isIdentified,
                randomSeeds = new {
                    quality = item.QualitySeed,
                    affix = item.AffixSeed,
                    itemLevel = item.ItemLevelSeed,
                    variance = item.VarianceSeed,
                    magicFind = item.MagicFind,
                }
            };

            if (item.MongoID > 0) {
                traceError("Item seems like it was already added to the DB, it has a MongoID: " + item.MongoID);
                return false;
            }

            return true;
        }
        //Action<int itemID, int seed> callback
        public NodePromise BuyItem(Item item, int itemIndex, int currentSeed, CurrencyTypes currencyType, int cost, Action<int, int> callbackOnItem = null) {
            object costObject;
            object[] list;

            if (!PrepareItemToBuy(item, currencyType, cost, out costObject, out list)) {
                return null;
            }

            return API.SendWhileLoggedIn("POST::/shop/buy/item", jsonData: new {
                cost = costObject,
                list = list,
                item = new {
                    index = itemIndex,
                    seed = currentSeed
                },
            })
                .Then(res => {
                    JSONNode jsonItem = res["shop"]["game"]["item"];

                    item.MongoID = res["item"]["id"].AsInt;

                    API.Currency.ParseCurrencyData(res["currency"]);

                    if (callbackOnItem != null) {
                        callbackOnItem(jsonItem["index"].AsInt, jsonItem["seed"].AsInt);
                    }
                })
                .Catch(err => {
                    traceError("Could not purchase item! " + GetErrorMessage(err));
                });
        }

        public NodePromise SellItems(Item item, CurrencyTypes currencyType, int cost) {
            return SellItems(new List<Item> { item }, currencyType, cost);
        }

        public NodePromise SellItems(List<Item> items, CurrencyTypes currencyType, int cost) {
            object costObject = CurrencyManager.ConvertToCurrencyObject(currencyType, cost);
            object[] itemsObject = new object[items.Count];

            int i=0;
            foreach (Item item in items) {
                if (item.MongoID <= 0) {
                    traceError("Item doesn't have a MongoID, therefore it cannot be sold! " + item.MongoID);
                    return null;
                }

                itemsObject[i++] = new { id = item.MongoID };
            }

            object jsonData = new {
                cost = costObject,
                items = itemsObject
            };

            return API.SendWhileLoggedIn("DELETE::/shop/sell/items", jsonData: jsonData)
                .Then(res => {
                    trace(res.pretty);
                    API.Currency.ParseCurrencyData(res["currency"]);
                    trace(res);
                })
                .Catch(err => {
                    traceError("Could not sell item! REASON: " + GetErrorMessage(err));
                });
        }

        public IPromise<FeaturedItemResponse> GetFeaturedItemSeed() {
            return API.SendWhileLoggedIn("GET::/shop/featured-item")
                .Then(res => {
                    var result = new FeaturedItemResponse() {
                        res = res,
                        seed = res["seed"].AsInt,
                        isItemPurchased = res["isItemPurchased"].AsBool,
                        dateCurrent = DateTime.Parse(res["dateCurrent"]),
                        dateNext = DateTime.Parse(res["dateNext"]),
                        jsonItem = res["item"],
                        jsonCurrency = res["currency"]
                    };

                    result.diffDatesMillis = (float) (result.dateNext - result.dateCurrent).TotalMilliseconds;
                    
                    //trace("Got Featured Item Seed: " + res.pretty);
                    
                    return result;
                })
                .Catch(err => {
                    traceError("Could not get Featured Item seed: " + err.Message);
                });
        }

        public IPromise<FeaturedItemResponse> BuyFeaturedItem(Item item, CurrencyTypes currencyType, int cost) {
            object costObject;
            object[] list;

            if (!PrepareItemToBuy(item, currencyType, cost, out costObject, out list)) {
                return null;
            }

            return API.SendWhileLoggedIn("POST::/shop/featured-item/buy", jsonData: new {
                cost = costObject,
                list = list
            })
                .Then(res => {
                    var result = new FeaturedItemResponse() {
                        res = res,
                        seed = res["seed"].AsInt,
                        isItemPurchased = res["isItemPurchased"].AsBool,
                        dateCurrent = DateTime.Parse(res["dateCurrent"]),
                        dateNext = DateTime.Parse(res["dateNext"]),
                        jsonItem = res["item"],
                        jsonCurrency = res["currency"]
                    };
                    
                    SetMongoID(item, result.jsonItem);
                    API.Currency.ParseCurrencyData(result.jsonCurrency);

                    return result;
                })
                .Catch(err => {
                    traceError("Could not buy Featured Item: " + GetErrorMessage(err));
                });
        }

        public NodePromise SetExpansionSlots(int expansionSlots, CurrencyTypes currency, int amount) {
            return SetExpansionSlots(expansionSlots, new CurrencyManager.Cost(currency, amount));
        }

        public NodePromise SetExpansionSlots(int expansionSlots, CurrencyManager.Cost cost) {
            object jsonData = new { expansionSlots = expansionSlots, cost = cost.ToObject() };

            return API.SendWhileLoggedIn("PUT::/shop/expansion-slots", jsonData: jsonData)
                .Then(res => {
                    trace(res);
                    API.Currency.ParseCurrencyData(res["currency"]);

                    playerMan.ShopExpansion = expansionSlots;
                })
                .Catch(err => {
                    traceError("Could not set the # of expansion-slots in the Shop: " + GetErrorMessage(err));
                });
        }
    }
    
    ////////////////////////////////////////////////////////////////////////////////////////

    public class MessageAPI : Child_API<IMongoData> {
        public bool hasNewMail = false;
        public Action<bool> OnStatusChanged;
        public List<InboxMessageData> messages;

        public MessageAPI() {
            apiName = "message";
            updateURLFormat = "PUT::/{0}/open/{1}/{2}";
            messages = new List<InboxMessageData>();
        }

        public IPromise<List<InboxMessageData>> CheckInbox() {
            return API.SendWhileLoggedIn("GET::/message/inbox")
                .Then( res => {
                    messages.Clear();

                    JSONArray jsonMessages = res.json["data"].AsArray;
                    foreach (JSONNode jsonMessage in jsonMessages) {
                        InboxMessageData msg = new InboxMessageData();
                        JSONNode jsonHeader = jsonMessage["header"];

                        msg.receipt = new InboxReceiptData();
                        msg.MongoID = jsonHeader["id"].AsInt;
                        msg.sentFromID = jsonHeader["sentFrom"].AsInt;
                        msg.isForEveryone = jsonHeader["isForEveryone"].AsBool;
                        msg.hasReceipt = jsonMessage["hasReceipt"].AsBool;
                        msg.rewardsData = jsonHeader["reward"]["item"];

                        if (msg.hasReceipt) {
                            UpdateMessageReceipt(msg, jsonMessage["receipt"]);
                        }

                        msg.title = jsonHeader["title"];
                        msg.messageType = (jsonHeader["type"]+"").ToUpper().Replace(" ", "_").AsEnum<InboxMessageType>();

                        msg.dateExpiresStr = jsonHeader["dateExpires"];
                        msg.dateSentStr = jsonHeader["dateSent"];

                        ParseRewardInfo(msg);

                        messages.Add(msg);
                    }

                    messages.Sort((a,b) => {
                        if(a.dateSent < b.dateSent) return 1;
                        if(a.dateSent > b.dateSent) return -1;
                        return 0;
                    });

                    UpdateUnreadMailStatus();

                    return messages;
                })
                .Catch( err => {
                    traceError("Could not get inbox messages:\n" + err.Message + "\n" + err.StackTrace);
                });
        }

        void UpdateUnreadMailStatus() {
            int numOfNewMail = 0;
            foreach(InboxMessageData msg in messages) {
                if(!msg.isRead) numOfNewMail++;
            }

            hasNewMail = numOfNewMail>0;

            if (OnStatusChanged != null) OnStatusChanged(hasNewMail);
        }

        public IPromise<InboxMessageData> ReadMessage(InboxMessageData msg) {
            return __Update(msg, "read")
                .Then(res => {
                    JSONNode jsonMessage = res["message"]["game"];

                    msg.message = jsonMessage["message"];
                    msg.rewardsData = jsonMessage["reward"]["item"];

                    UpdateMessageReceipt(msg, res["receipt"]);
                    UpdateUnreadMailStatus();

                    return msg;
                });
        }

        public IPromise<InboxMessageData> DeleteMessage(InboxMessageData msg) {
            return __Update(msg, "delete")
                .Then(res => {
                    msg.receipt.isDeleted = true;
                    UpdateUnreadMailStatus();

                    return msg;
                });
        }

        public IPromise<InboxMessageData> ClaimReward(InboxMessageData msg) {
            return __Update(msg, "claim")
                .Then(res => {
                    JSONNode jsonMessage = res["message"]["game"];

                    msg.message = jsonMessage["message"];

                    UpdateMessageReceipt(msg, res["receipt"]);
                    UpdateUnreadMailStatus();
                    
                    return msg;
                });
        }

        void UpdateMessageReceipt(InboxMessageData msg, JSONNode jsonReceipt) {
            JSONNode jsonGame = jsonReceipt["game"];
            InboxReceiptData receipt = msg.receipt;
            
            receipt.isRead = jsonGame["isRead"].AsBool;
            receipt.isClaimed = jsonGame["isClaimed"].AsBool;
            receipt.isDeleted = jsonGame["isDeleted"].AsBool;
            
            receipt.dateReadStr = jsonGame["dateRead"];
            receipt.dateClaimedStr = jsonGame["dateClaimed"];
            receipt.dateDeletedStr = jsonGame["dateDeleted"];

            if (receipt.isRead) {
                msg.hasReceipt = true;
            }
        }

        void ParseRewardInfo(InboxMessageData msg) {
            switch(msg.messageType) {
                case InboxMessageType.CURRENCY_REWARD:
                    if (msg.rewardsData.Contains("boost")) {
                        var boost = msg.rewardBoost = CurrencyManager.ParseToBoostCost(msg.rewardsData);
                        msg.currencyType = boost.type.ToString();
                        msg.currencyAmount = boost.amount;
                    } else {
                        var currency = msg.rewardCurrency = CurrencyManager.ParseToCost(msg.rewardsData);
                        msg.currencyType = currency.type.ToString();
                        msg.currencyAmount = currency.amount;
                    }
                    break;

                case InboxMessageType.LOOTCRATE_REWARD:
                    msg.lootCrateData = JSONManager.SplitKVStrings(msg.rewardsData);
                    break;
            }
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////

    public class ExplorationAPI : Child_API<ActiveExploration> {
        public ExplorationAPI() {
            apiName = "exploration";
        }

        public NodePromise StartExploring(ZoneData zone, List<Hero> party, DateTime dateStarted) {
            if (zone == null) {
                traceError("Zone is null!");
                return null;
            }

            if (party == null || party.Count == 0) {
                traceError("Party is null or empty (Length==0)");
                return null;
            }

            if (party.Count > 5) {
                traceError("Party is too big (Length should be 1 to 5, you have {0} in your party)".Format2(party.Count));
                return null;
            }

            int[] heroIDs = new int[party.Count];
            for(int h=0; h<party.Count; h++) {
                heroIDs[h] = party[h].MongoID;
            }

            trace("StartExploring Party: " + heroIDs.ToJSONString(true));

            object jsonData = new {
                party = heroIDs,
                exploration = new {
                    dateStarted = dateStarted.ToNodeDateTime()
                }
            };

            return API.SendWhileLoggedIn(string.Format("POST::/exploration/{0}/start", zone.ActZoneID), jsonData: jsonData);
        }

        public NodePromise Update(ActiveExploration exploration) {
            int actZoneID = exploration.Zone.ActZoneID;
            object jsonData = new {
                exploration = new {
                    accumulativeDamage = exploration.AccumulatedDamage,
                    chests = exploration.ChestsEarned
                }
            };

            return __Update(exploration, "update", jsonData, actZoneID);
        }

        public NodePromise Remove(int actZoneID) {
            return API.SendWhileLoggedIn(string.Format("DELETE::/exploration/{0}/remove", actZoneID))
                .Then(res => {
                    trace("Successfully Removed Exploration, Heroes may need to be reset!");
                })
                .Catch(err => {
                    traceError("Could not remove exploration! " + GetErrorMessage(err));
                });
        }

        public NodePromise RemoveAll() {
            return API.SendWhileLoggedIn("DELETE::/exploration/remove-all")
                .Then(res => {
                    trace("Removed all Explorations.");
                });
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////

    public class ResearchAPI : Child_API<IMongoData> {

        public ResearchAPI() {
            apiName = "researchSlot";
        }

        public IPromise<JSONArray> GetAllSlots() {
            return API.SendWhileLoggedIn("GET::/researchSlot/list")
                .Then(res => {
                    return res.json["data"].AsArray;
                })
                .Catch(err => {
                    traceError("Error getting all Research-Slots details: " + GetErrorMessage(err));
                });
        }

        public void ProcessSlotData(ResearchSlotInterface slot, JSONNode jsonSlot) {
            var status = jsonSlot["status"].AsEnum<ResearchSlotStatus>();
            slot.status = status;
            slot.dateCompleted = DateTime.Parse(jsonSlot["dateCompleted"]);
            slot.dateUnlocked = DateTime.Parse(jsonSlot["dateUnlocked"]);
            slot.dateStarted = DateTime.Parse(jsonSlot["dateStarted"]);
            slot.dateEnd = DateTime.Parse(jsonSlot["dateEnd"]);
            slot.CalculateTimeDiff();

            int itemID = jsonSlot["itemID"].AsInt;
            if(itemID>0) {
                var item = dataMan.allItemsList.Find(i => i.MongoID == itemID);

                if (item==null) {
                    traceError("The Slot's research-data may have a corrupted ItemID, can't seem to find it in the DataManager cache.");
                    return;
                }

                slot.item = item;

                //if(status==ResearchSlotStatus.UNLOCKED) {
                //    item.isIdentified = true;
                //} else {
                //    item.isResearched = true;
                //}
            }
        }

        public IPromise<ResearchSlotInterface> ChangeStatus(ResearchSlotInterface slot, ResearchSlotStatus status, CurrencyTypes currency=CurrencyTypes.NONE, int amount=0, Item item=null) {
            string action = status.ToString().ToLower();
            var cost = currency==CurrencyTypes.NONE ? null : new CurrencyManager.Cost(currency, amount);

            return __ChangeStatus(action, slot.trayID, slot.slotID, new {
                itemID = item==null ? -1 : item.MongoID,
                cost = cost!=null ? cost.ToObject() : null
            })
                .Then(res => {
                    var jsonSlot = res["slot"].Exists() ? res["slot"]["game"] : null;
                    var jsonItem = res["item"].Exists() ? res["item"]["game"] : null;
                    var jsonCurrency = res["currency"].Exists() ? res["currency"] : null;

                    if (jsonSlot == null) throw new Exception("Missing 'slot' JSON field in Research-Slot response.");

                    ProcessSlotData(slot, jsonSlot);

                    if (jsonItem != null) {
                        int itemID = res["item"]["id"].AsInt;
                        var currentItem = dataMan.allItemsList.Find(i => i.MongoID == itemID);

                        currentItem.isIdentified = jsonItem["isIdentified"].AsBool;
                        currentItem.isResearched = jsonItem["isResearched"].AsBool;
                        slot.item = currentItem;
                    }

                    if (jsonCurrency != null) {
                        API.Currency.ParseCurrencyData(jsonCurrency);
                    }

                    return slot;
                });
        }

        public NodePromise __ChangeStatus(string status, int trayID, int slotID, object jsonData) {
            return API.SendWhileLoggedIn("PUT::/researchSlot/{0}/{1}/{2}".Format2(trayID, slotID, status), jsonData: jsonData)
                .Then(res => {
                    trace("Successfully changed status of Research Slot: {0}/{1} to {2}".Format2(trayID, slotID, status));
                })
                .Catch(err => {
                    traceError(
                        "Error setting the status of Research Slot: {0}/{1} to {2}".Format2(trayID, slotID, status) + "\n" +
                        GetErrorMessage(err)
                    );
                });
        }
    }
}

public class FeaturedItemResponse {
    public NodeResponse res;
    public JSONNode jsonItem;
    public JSONNode jsonCurrency;
    public int seed;
    public bool isItemPurchased;
    public DateTime dateCurrent;
    public DateTime dateNext;
    public float diffDatesMillis;
    
    public int progressSeconds {
        get {
            return Mathf.Max(0, (int) (dateNext - DateTime.Now).TotalSeconds);
        }
    }

    public float progressPercent {
        get {
            float diffNow = (float) (DateTime.Now - dateCurrent).TotalMilliseconds;
            return Mathf.Min(diffNow / diffDatesMillis, 1);
        }
    }
}

public class ShopRefreshKey {
    public int seed;
    public DateTime dateExpires;
    public int[] purchased;

    public int GetRemainingSeconds() {
        int seconds = (int)(dateExpires - DateTime.Now).TotalSeconds;
        return seconds < 0 ? 0 : seconds;
    }
}