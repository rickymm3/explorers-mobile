using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using RSG;
using NodeJS;

using Rand = UnityEngine.Random;

public class CheatManager :Tracer {
    private static string _EST_TO_EXIT = " [ESC] = Back to Main Menu";
    public static bool ALWAYS_CREATE_AS_UNIDENTIFIED = false;

    Action _currentState;
    Action _lastState;
    bool changedState = false;

    public static GameAPIManager API { get { return GameAPIManager.Instance; } }
    public static AudioManager AUDIO { get { return AudioManager.Instance; } }
    public static DataManager DATAMAN { get { return DataManager.Instance; } }
    public static GameManager GAMEMAN { get { return GameManager.Instance; } }
    public static PlayerManager PLAYERMAN { get { return PlayerManager.Instance; } }
    public static CurrencyManager CURRENCY { get { return CurrencyManager.Instance; } }


    bool _isShifted = false;
    bool _isDeletingPlayerPrefs = false;

    public static void traceCheats(object msg) {
        trace("CHEATS", msg);
    }

    void Start() {
        traceCheats("============= Cheats Menu =============");
    }

    // Update is called once per frame
    void Update() {
        if (!DATAMAN.isLoaded || ConfirmYesNoInterface.Instance!=null) return;

        if (_currentState == null) _currentState = STATE_Main;


        _isShifted = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        changedState = false;

        if (_lastState != _currentState) {
            changedState = true;
            _lastState = _currentState;
        }

        _currentState();

        //There's ALWAYS an escape route ;)
        if (Input.GetKeyDown(KeyCode.Escape)) {
            DoEscapeKey();
        }
    }

    private void DoEscapeKey() {
        bool changed = _currentState != STATE_Main;

        if (_isShifted) {
            if (!_isDeletingPlayerPrefs) {
                _isDeletingPlayerPrefs = true;
                traceCheats("CAUTION: Do you really wish to delete ALL player prefs?");
                return;
            }

            PlayerPrefs.DeleteAll();
            traceCheats("ALL PLAYER PREFS DELETED");
            changed = true;
        }

        _isDeletingPlayerPrefs = false;

        if (!changed) return;

        AUDIO.Play(SFX_UI.Explosion);
        MenuManager.Instance.UIRoot.GetChild(0).DOShakePosition(0.4f, 30, 30);
        _currentState = STATE_Main;
    }

    void STATE_Main() {
        if (changedState) {
            traceCheats("Menus [SHIFT + ...] = [C]urrency, [L]ootCrates, [W]ipe-Data, [T]esting.\n" + _EST_TO_EXIT);
        }

        if (!_isShifted) return;

        if (Input.GetKeyDown(KeyCode.C)) {
            _currentState = STATE_Currency;
        }

        if (Input.GetKeyDown(KeyCode.L)) {
            _currentState = STATE_LootCrates;
        }

        if (Input.GetKeyDown(KeyCode.W)) {
            _currentState = STATE_WipeData;
        }

        if (Input.GetKeyDown(KeyCode.T)) {
            _currentState = STATE_Testing;
        }
    }

    void STATE_Currency() {
        int COIN_AMOUNT = 1000;
        int SCROLLS_AMOUNT = 10;
        int SHARDS_AMOUNT = 10;
        int RELIC_AMOUNT = 10;
        int ESSENCE_AMOUNT = 10;
        int BOOST_AMOUNT = 1;

        if (changedState) {
            traceCheats("CURRENCY: Add "+ COIN_AMOUNT +
                " of [G]old, G[e]ms, Scrolls-[I]dentify, Scrolls-[S]ummon, Scrolls-[M]onster, [B]oosts, [A]ll" +
                _EST_TO_EXIT);
        }

        IPromise<NodeResponse> promise = null;
        
        if (Input.GetKeyDown(KeyCode.G)) {
            AUDIO.Play(SFX_UI.Coin);
            promise = API.Currency.AddCurrency(CurrencyTypes.GOLD, COIN_AMOUNT);
        }

        if (Input.GetKeyDown(KeyCode.E)) {
            AUDIO.Play(SFX_UI.ShardsChing);
            promise = API.Currency.AddCurrency(CurrencyTypes.GEMS, COIN_AMOUNT);
        }

        if (Input.GetKeyDown(KeyCode.I)) {
            AUDIO.Play(SFX_UI.Identify);
            promise = API.Currency.AddCurrency(CurrencyTypes.SCROLLS_IDENTIFY, SCROLLS_AMOUNT);
        }

        if (Input.GetKeyDown(KeyCode.S)) {
            AUDIO.Play(SFX_UI.WooshIn);

            var cost = new CurrencyManager.Cost();
            cost.AddOrSet(CurrencyTypes.SCROLLS_IDENTIFY, SCROLLS_AMOUNT);
            cost.AddOrSet(CurrencyTypes.SCROLLS_SUMMON_COMMON, SCROLLS_AMOUNT);
            cost.AddOrSet(CurrencyTypes.SCROLLS_SUMMON_RARE, SCROLLS_AMOUNT);
            cost.AddOrSet(CurrencyTypes.SCROLLS_SUMMON_MONSTER_DARK, SCROLLS_AMOUNT);
            cost.AddOrSet(CurrencyTypes.SCROLLS_SUMMON_MONSTER_FIRE, SCROLLS_AMOUNT);
            cost.AddOrSet(CurrencyTypes.SCROLLS_SUMMON_MONSTER_LIGHT, SCROLLS_AMOUNT);
            cost.AddOrSet(CurrencyTypes.SCROLLS_SUMMON_MONSTER_WATER, SCROLLS_AMOUNT);
            cost.AddOrSet(CurrencyTypes.SCROLLS_SUMMON_MONSTER_NATURE, SCROLLS_AMOUNT);

            promise = API.Currency.AddCurrency(cost);
        }

        if (Input.GetKeyDown(KeyCode.M)) {
            AUDIO.Play(SFX_UI.WooshIn);
            promise = API.Currency.AddCurrency(CurrencyTypes.SCROLLS_SUMMON_MONSTER_DARK, SCROLLS_AMOUNT);
        }

        if (Input.GetKeyDown(KeyCode.B)) {
            AUDIO.Play(SFX_UI.PageFlip);

            var boostCost = new CurrencyManager.BoostCost();
            boostCost.AddOrSet(BoostType.Gold, BOOST_AMOUNT);
            boostCost.AddOrSet(BoostType.MagicFind, BOOST_AMOUNT);
            boostCost.AddOrSet(BoostType.Health, BOOST_AMOUNT);
            boostCost.AddOrSet(BoostType.XP, BOOST_AMOUNT);

            promise = API.Users.BoostAddCurrency(boostCost);
        }

        if (Input.GetKeyDown(KeyCode.A)) {
            AUDIO.Play(SFX_UI.ShardsChing);
            var cost = new CurrencyManager.Cost();
            cost.AddOrSet(CurrencyTypes.GOLD, COIN_AMOUNT);
            cost.AddOrSet(CurrencyTypes.GEMS, COIN_AMOUNT);
            cost.AddOrSet(CurrencyTypes.MAGIC_ORBS, COIN_AMOUNT);

            cost.AddOrSet(CurrencyTypes.ESSENCE_HIGH, ESSENCE_AMOUNT);
            cost.AddOrSet(CurrencyTypes.ESSENCE_LOW, ESSENCE_AMOUNT);
            cost.AddOrSet(CurrencyTypes.ESSENCE_MID, ESSENCE_AMOUNT);

            cost.AddOrSet(CurrencyTypes.SCROLLS_IDENTIFY, SCROLLS_AMOUNT);
            cost.AddOrSet(CurrencyTypes.SCROLLS_SUMMON_COMMON, SCROLLS_AMOUNT);
            cost.AddOrSet(CurrencyTypes.SCROLLS_SUMMON_RARE, SCROLLS_AMOUNT);
            cost.AddOrSet(CurrencyTypes.SCROLLS_SUMMON_MONSTER_DARK, SCROLLS_AMOUNT);
            cost.AddOrSet(CurrencyTypes.SCROLLS_SUMMON_MONSTER_FIRE, SCROLLS_AMOUNT);
            cost.AddOrSet(CurrencyTypes.SCROLLS_SUMMON_MONSTER_LIGHT, SCROLLS_AMOUNT);
            cost.AddOrSet(CurrencyTypes.SCROLLS_SUMMON_MONSTER_WATER, SCROLLS_AMOUNT);
            cost.AddOrSet(CurrencyTypes.SCROLLS_SUMMON_MONSTER_NATURE, SCROLLS_AMOUNT);

            cost.AddOrSet(CurrencyTypes.SHARDS_ITEMS_COMMON, SHARDS_AMOUNT);
            cost.AddOrSet(CurrencyTypes.SHARDS_ITEMS_MAGIC, SHARDS_AMOUNT);
            cost.AddOrSet(CurrencyTypes.SHARDS_ITEMS_RARE, SHARDS_AMOUNT);
            cost.AddOrSet(CurrencyTypes.SHARDS_ITEMS_RARE, SHARDS_AMOUNT);
            cost.AddOrSet(CurrencyTypes.XP_FRAGMENT, SHARDS_AMOUNT);
            cost.AddOrSet(CurrencyTypes.XP_FRAGMENT, SHARDS_AMOUNT);
            cost.AddOrSet(CurrencyTypes.XP_FRAGMENT_PLUS, SHARDS_AMOUNT);
            cost.AddOrSet(CurrencyTypes.XP_FRAGMENT_PLUS, SHARDS_AMOUNT);

            cost.AddOrSet(CurrencyTypes.RELICS_BOW, RELIC_AMOUNT);
            cost.AddOrSet(CurrencyTypes.RELICS_SHIELD, RELIC_AMOUNT);
            cost.AddOrSet(CurrencyTypes.RELICS_STAFF, RELIC_AMOUNT);
            cost.AddOrSet(CurrencyTypes.RELICS_SWORD, RELIC_AMOUNT);

            var boostCost = new CurrencyManager.BoostCost();
            boostCost.AddOrSet(BoostType.Gold, BOOST_AMOUNT);
            boostCost.AddOrSet(BoostType.MagicFind, BOOST_AMOUNT);
            boostCost.AddOrSet(BoostType.Health, BOOST_AMOUNT);
            boostCost.AddOrSet(BoostType.XP, BOOST_AMOUNT);

            promise = API.Users.BoostAddCurrency(boostCost)
                .Then(res => {
                    traceCheats("Boost Currency set: " + res.pretty);
                    return API.Currency.AddCurrency(cost);
                });
        }

        if (promise != null) {
            promise.Then(res => {
                traceCheats("Currency set: " + res.pretty);
            });
        }
    }

    //void STATE_Items() {
    //    if (changedState) {
    //        traceCheats("ITEMS: Add [H]elm, [C]hest, [G]loves, [B]oots, -R-elic, [W]eapon" + _EST_TO_EXIT);
    //    }

    //    ItemData randomItemData = null;

    //    IPromise<NodeResponse> promise = null;

    //    if (Input.GetKeyDown(KeyCode.H)) {
    //        AUDIO.Play(SFX_UI.Click);
    //        randomItemData = DATAMAN.GetRandomItemType(EquipmentType.Helm);
    //    }

    //    if (Input.GetKeyDown(KeyCode.C)) {
    //        AUDIO.Play(SFX_UI.Click);
    //        randomItemData = DATAMAN.GetRandomItemType(EquipmentType.Chest);
    //    }

    //    if (Input.GetKeyDown(KeyCode.G)) {
    //        AUDIO.Play(SFX_UI.Click);
    //        randomItemData = DATAMAN.GetRandomItemType(EquipmentType.Gloves);
    //    }

    //    if (Input.GetKeyDown(KeyCode.B)) {
    //        AUDIO.Play(SFX_UI.Click);
    //        randomItemData = DATAMAN.GetRandomItemType(EquipmentType.Boots);
    //    }

    //    if (Input.GetKeyDown(KeyCode.R)) {
    //        AUDIO.Play(SFX_UI.Invalid);
    //        traceError("Relics not implemented yet!");
    //        return;
    //        //promise = API.ItemRandomize(EquipmentType.Relic, 1);
    //        //randomItemData = DATAMAN.GetRandomItemType(EquipmentType.Artifact);
    //    }

    //    if (Input.GetKeyDown(KeyCode.W)) {
    //        AUDIO.Play(SFX_UI.Click);
    //        randomItemData = DATAMAN.GetRandomItemType(EquipmentType.Weapon);
    //    }

    //    Item item = null;
    //    if (randomItemData != null) {
    //        item = new Item(randomItemData, GAMEMAN.GetSeed(), GAMEMAN.GetSeed(), GAMEMAN.GetSeed(), GAMEMAN.GetSeed(), Rand.Range(0f, 100f), 20f, 0f);
    //        promise = API.Items.Add(item);
    //    }

    //    if (promise != null) {
    //        promise.Then(res => {
    //            traceCheats("Item added!");
    //            trace(res.pretty);
    //            DATAMAN.allItemsList.Add(item);
    //        }).Catch(err => {
    //            traceError("Oh no! Item couldn't be added!");
    //        });
    //    }
    //}

    //void STATE_Heroes() {
    //    if (changedState) {
    //        traceCheats("HEROES: Add [H]ero" + _EST_TO_EXIT);
    //    }

    //    Hero hero = null;
    //    IPromise<NodeResponse> promise = null;

    //    if (Input.GetKeyDown(KeyCode.H)) {
    //        AUDIO.Play(SFX_UI.ShardsChing);
    //        hero = new Hero(DATAMAN.heroDataList.GetRandom(), GAMEMAN.GetSeed());
    //        promise = API.Heroes.Add(hero);
    //    }

    //    if (promise != null) {
    //        promise.Then(res => {
    //            traceCheats("Hero added!");
    //            trace(res.pretty);
    //            DATAMAN.allHeroesList.Add(hero);
    //        }).Catch(err => {
    //            traceError("Oh no! Hero couldn't be added!");
    //        });
    //    }
    //}

    void STATE_LootCrates() {
        if (changedState) {
            traceCheats("LOOT-CRATES: Add [L]ootCrate" + _EST_TO_EXIT);
        }

        LootCrate lootCrate = null;
        IPromise<NodeResponse> promise = null;

        if (Input.GetKeyDown(KeyCode.L)) {
            AUDIO.Play(SFX_UI.ShardsChing);
            var lootTable = DATAMAN.lootTableDataList.GetByIdentity("lt_zone_training_1");

            lootCrate = new LootCrate(lootTable.Identity, 20f, 20f, DataManager.Instance.crateTypeDataList.GetRandom());
            promise = API.LootCrates.Add(lootCrate);
        }

        if (promise != null) {
            promise.Then(res => {
                traceCheats("LootCrate added!");
                trace(res.pretty);
                DATAMAN.allLootCratesList.Add(lootCrate);
            }).Catch(err => {
                traceError("Oh no! LootCrate couldn't be added!");
            });
        }
    }

    void STATE_WipeData() {
        if (changedState) {
            traceCheats("WIPE-DATA: [I]tems, [H]eroes, [C]urrency, , E[x]plorations, [B]oost slots, Hero.explorationAct[Z]one, [A]ll");
        }

        IPromise<NodeResponse> promise = null;

        string which = "all";

        if (Input.GetKeyDown(KeyCode.I)) {
            which = "Items";
            promise = API.Items.RemoveAll();
        }

        if (Input.GetKeyDown(KeyCode.H)) {
            which = "Heroes";
            promise = API.Heroes.RemoveAll();
        }

        if (Input.GetKeyDown(KeyCode.C)) {
            which = "Currencies";
            promise = API.Currency.Reset();
        }

        if (Input.GetKeyDown(KeyCode.X)) {
            which = "Explorations";
            promise = API.Explorations.RemoveAll();
        }

        if (Input.GetKeyDown(KeyCode.B)) {
            which = "Boost Slots";
            promise = API.Users.BoostClearAll();
        }

        if (Input.GetKeyDown(KeyCode.Z)) {
            which = "Hero's Exploration Zones";
            promise = API.Heroes.ResetExplorations();
        }

        if (Input.GetKeyDown(KeyCode.A)) {
            which = "All (n/a)";
            trace("not implemented yet.");
        }

        if (promise != null) {
            promise.Then(res => {
                traceCheats("Cleared data for: " + which);
                trace(res.pretty);
                AUDIO.Play(SFX_UI.Explosion);
            })
            .Catch(err => {
                traceError("Error running Cheat command: " + err.Message);
            });
        }
    }

    void STATE_Testing() {
        if (changedState) {
            traceCheats("TESTING: Always make [U]nidentified Items, [H]ero Remove 1st, Skip to [B]oss Battle");
        }

        IPromise<NodeResponse> promise = null;

        if(Input.GetKeyDown(KeyCode.U)) {
            ALWAYS_CREATE_AS_UNIDENTIFIED = !ALWAYS_CREATE_AS_UNIDENTIFIED;
            trace("ALWAYS_CREATE_AS_UNIDENTIFIED: " + ALWAYS_CREATE_AS_UNIDENTIFIED);
            AudioManager.Instance.Play(ALWAYS_CREATE_AS_UNIDENTIFIED ? SFX_UI.Coin : SFX_UI.Explosion);
        }

        if (Input.GetKeyDown(KeyCode.B)) {
            foreach(ActiveExploration exploration in DATAMAN.allExplorationsList) {
                exploration.AccumulatedDamage += exploration.Zone.ZoneHP * 2f;
                promise = API.Explorations.Update(exploration)
                    .Then(res => {
                        trace("Explorations Updates.");
                    });
            }
        }

        if (Input.GetKeyDown(KeyCode.H)) {
            promise = API.Heroes.Remove(DATAMAN.allHeroesList[0])
                .Then(res => {
                    DATAMAN.allHeroesList.RemoveAt(0);
                });
        }

        if (promise != null) {
            promise.Then(res => {
                traceCheats("Testing data.");
                trace(res.pretty);
                AUDIO.Play(SFX_UI.WooshOut);
            })
            .Catch(err => {
                traceError("Error running Cheat command: " + err.Message);
            });
        }
    }
}
