using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum BossBattleState { NewTurn, SelectAction, WaitForAction, ExecuteAction, EndOfBattle, ShowResults, EndOfTurn }

public class BossBattleController : MonoBehaviour {

    BattlePhases Phase = BattlePhases.Loading;
    BattleRootController battleRoot;
    BossBattleInterface battleInterface;
    ActiveExploration data;
    FloatingCombatTextInterface FCTInterface;
    ActorSelector selector;

    BossBattleState state = BossBattleState.NewTurn;
    public List<HeroActor> Party = new List<HeroActor>();
    public List<BattleActor> TurnOrder = new List<BattleActor>();
    public BattleActor currentActor = null;
    public Skill currentSkill;
    public Skill DefendSkillRef;

    Vector2 fctLocation = Vector2.zero;

    bool bossTurn = false;

    IEnumerator bossActionRoutine = null;
    IEnumerator executeActionRoutine = null;

    public static PlayerManager playerMan { get { return PlayerManager.Instance; } }
    public static DataManager dataMan{ get { return DataManager.Instance; } }

    Dictionary<BattleActor, Skill> OnDeathSkillExecutionList = new Dictionary<BattleActor, Skill>();

    bool initialized = false;

    void Update() {
        switch (Phase) {
            case BattlePhases.Loading:
                Loading();
                break;
            case BattlePhases.Initialize:
                if (!initialized) Initialize();
                break;
            case BattlePhases.Start:
                StartBattle();
                break;
            case BattlePhases.Battle:
                RunBattleSimulation();
                break;
        }
    }

    void Loading() {
        // Get the BattleRoot Object
        if (battleRoot == null) battleRoot = GameObject.FindObjectOfType<BattleRootController>();
        if (FCTInterface == null) FCTInterface = (FloatingCombatTextInterface) MenuManager.Instance.Load("Interface_FloatingCombatText");
        if (battleInterface == null) battleInterface = (BossBattleInterface) MenuManager.Instance.Push("Interface_BossBattle");
        if ((selector == null) && (battleRoot != null)) selector = (ActorSelector) battleRoot.GetSelector();

        data = playerMan.SelectedBattle;

        playerMan.CurrentLeaderSkill = data.Party[0].data.LeadershipSkill;

        // If the battleRoot object isn't null goto the next phase
        if (battleRoot != null/* && battleInterface != null*/)
            Phase++;
    }

    void Initialize() {
        // Load Party and Boss in
        int index = 0;

        float totalSpeed = 0f;
        HeroActor hActor;
        CharacterHandler handler;
        // Spawn the party sprites
        foreach (Hero hero in data.Party) {
            if(hero==null || hero.data==null || hero.data.Name==null) {
                Tracer.traceError("Could not load Hero, something about the Hero data isn't intialized yet.");
                continue;
            }

            GameObject heroObj = hero.heroData.LoadFightModel();
            if(heroObj==null) continue;

            heroObj.transform.SetParent(battleRoot.PartyPositions[index]);
            // set sprite order render as well here
            heroObj.transform.localPosition = Vector3.zero;
            heroObj.transform.localScale = heroObj.transform.localScale;
            handler = heroObj.GetComponent<CharacterHandler>();
            handler.Initialize(hero);

            totalSpeed += hero.GetPrimaryStat(PrimaryStats.Speed);
            hero.ResetSkillsCooldown();
            hActor = new HeroActor(hero, handler, FCTInterface, this);
            TurnOrder.Add(hActor);
            Party.Add(hActor);

            DefendSkillRef = (Skill) UnityEngine.Object.Instantiate(Resources.Load<Skill>("Skills/Scriptables/Skill_Defend"));

            index++;

            // Apply start of battle status effects
            hActor.ApplyStartOfBattleBuffs();
        }

        print("Spawned all heroes");

        battleInterface.Initialize(this);

        // Spawn the Boss
        totalSpeed += LoadBoss();

        // Calculate Turn Order
        CalculateTurnOrder(totalSpeed);

        initialized = true;

        // Next phase once complete
        //Phase++;
        if (!PlayerPrefs.HasKey("tutorial_boss_battle")) {
            StoryManager.Instance.DisplayStory("story_tutorial_boss_battle", () => { Phase++; });

            // Do the overlay tutorial here

            PlayerPrefs.SetInt("tutorial_boss_battle", 1);
        } else {
            Phase++; // now done after the story if we need to show the tutorial story piece
        }
    }
     
    void StartBattle() {
        Phase++;
    }

    bool IsBattleComplete() {
        bool heroStillExists = false;
        bool bossStillExists = false;
        foreach (BattleActor actor in TurnOrder) {
            if (actor is HeroActor)
                heroStillExists = true;
            if (actor is BossActor)
                bossStillExists = true;
        }

        if (!bossStillExists || !heroStillExists)
            return true;

        return false;
    }

    public void AddOnDeathSKill(BattleActor actor, Skill skill) {
        OnDeathSkillExecutionList.Add(actor, skill);
    }

    void StartEndOfTurn() {
        state = BossBattleState.EndOfTurn;

        StartCoroutine(EndOfTurnCleanUp());
    }

    public IEnumerator EndOfTurnCleanUp() {

        // Put UI/Death stuff here if we have something special at the end of the turn
        
        // Execute Dieing Actors OnDeath Skills
        foreach (BattleActor actor in OnDeathSkillExecutionList.Keys) {
            currentActor = actor; // may not want to set the current actor, need to test to verify
            currentSkill = OnDeathSkillExecutionList[actor];

            // Get the required target
            BattleActor target = GetTargetBasedOnSkill(currentSkill, currentActor);

            print("Executing " + currentSkill.name + " [" + target + "]");

            if (!OnDeathSkillExecutionList[actor].OneTimeTrigger)
                yield return StartCoroutine(ExecuteSkillTiming(target));
            else {
                Debug.Log("Triggered Once: " + OnDeathSkillExecutionList[actor].triggeredOnce);
                if (!OnDeathSkillExecutionList[actor].triggeredOnce) {
                    yield return StartCoroutine(ExecuteSkillTiming(target));
                    OnDeathSkillExecutionList[actor].triggeredOnce = true;
                }
            }
        }

        OnDeathSkillExecutionList.Clear();

        // Next Turn
        yield return new WaitForSeconds(1f);
        state = BossBattleState.NewTurn;
    }

    List<BattleActor> deadActors = new List<BattleActor>();
    void RunBattleSimulation() {
        // 2 states - Select Action and Execute Action
        // on select action set the actor to the current
        // show the UI for where the default action will place them

        switch (state) {
            case BossBattleState.EndOfTurn:
                break;
            case BossBattleState.NewTurn:

                // Clean up the list of dead actors
                List<BattleActor> removeList = new List<BattleActor>();
                foreach (BattleActor actor in TurnOrder) {
                    if (actor.Health <= 0f) {
                        actor.StopCast();
                        battleInterface.RemoveActorPortrait(actor);
                        removeList.Add(actor);
                        deadActors.Add(actor);
                        if (actor is BossActor)
                            ((BossActor) actor).handler.Die();
                        else
                            ((HeroActor) actor).handler.Die();

                    }
                }

                bool bossKilled = false;
                foreach (BattleActor actor in removeList) {
                    if (actor is BossActor)
                        bossKilled = true;

                    TurnOrder.Remove(actor);
                }

                if (bossKilled) {
                    // on allyDeathTrigger
                    foreach(BattleActor actor in TurnOrder) {
                        if (actor is BossActor)
                            ((BossActor) actor).OnAllyDeath();
                    }
                }

                currentActor = TurnOrder[0];
                selector.GoToTarget(currentActor.GetGameObject().transform.position);

                if (currentActor is HeroActor)
                    print(" --- [New Turn]\n   Hero Turn: " + currentActor.Name);
                else
                    print(" --- [New Turn]\n   Boss Turn: " + currentActor.Name);

                // Check if the current actor is stunned here and skip their turn
                if (currentActor.IsStunned()) {
                    // Spawn text displaying this actor is stunned then return to restart this turn and choose a new actor
                    currentActor.UpdateStatusEffects(StatusTriggerTime.StartOfTurn);
                    currentActor.UpdateStatusEffects(StatusTriggerTime.EndOfTurn);
                    return;
                }

                // this position allows stun transitions to be delayed when stunned, we can move this above the stun code block to change this
                if (currentActor is BossActor) {
                    ((BossActor) currentActor).UpdateTurnCount();
                }

                //Check for end of battle here
                if (IsBattleComplete()) {
                    state = BossBattleState.EndOfBattle;
                } else {
                    // Check if the actor has a skill waiting to be used
                    if (currentActor.HasQueuedSkill()) {
                        // Skip straight to the use the skill part of a turn
                        SelectSkill(currentActor.QueuedSkill);
                        ExecuteSkill(currentActor.QueuedSkillTarget, true);
                        currentActor.QueuedSkill = null;
                        currentActor.QueuedSkillTarget = null;
                    } else
                        state = BossBattleState.SelectAction;
                }
                break;
            case BossBattleState.SelectAction:
                battleInterface.SetCurrentTurn(currentActor);
                currentActor.UpdateStatusEffects(StatusTriggerTime.StartOfTurn);

                // Load UI for the hero or hide it on enemy
                if (currentActor is HeroActor) {
                    // Reduce skill cooldowns by a turn
                    foreach (Skill skill in ((HeroActor) currentActor).hero.Skills) {
                        /*if (((HeroActor) currentActor).hero.Skills[skill] > 0)
                            ((HeroActor) currentActor).hero.Skills[skill] -= 1;*/
                        if (skill._cooldown > 0)
                            skill._cooldown--;
                    }

                    battleInterface.ShowMemberArea((HeroActor) currentActor);
                    SelectSkill(((HeroActor) currentActor).hero.Skills[((HeroActor) currentActor).hero.Skills.Count-1]); // default skill selected

                    bossTurn = false;
                } else {
                    SelectSkill(((BossActor) currentActor).boss.Skills[0]);

                    // Choose a skill based on how many targets
                    int targetCount = 0;
                    bool targetCasting = false;
                    foreach (HeroActor actor in Party) {
                        if (actor.Health > 0) targetCount++;
                        if (actor.QueuedSkill != null) {
                            targetCasting = true;
                        }
                    }

                    Skill skillSelect = null;
                    foreach (Skill skill in ((BossActor) currentActor).boss.Skills) {
                        if (skill._cooldown < 1 && skill.CanInterrupt) {
                            skillSelect = skill;
                        }
                    }
                    // TODO put the interrupt AI chance on the boss data it self, current set to 25% of the time
                    if (targetCasting && Random.Range(0f, 1f) < 0.25f && skillSelect != null) {
                        // Choose the skill that can interrupt
                        SelectSkill(skillSelect);
                    } else if (targetCount > 2) {
                        // It can be an AOE skill
                        bool hasAOE = false;
                        foreach (Skill skill in ((BossActor) currentActor).boss.Skills) {
                            if (skill._cooldown < 1 && skill.TargetType != SkillTargetType.SingleTarget) {
                                hasAOE = true;
                                SelectSkill(skill);
                                continue;
                            }
                        }

                        if (!hasAOE)
                            foreach (Skill skill in ((BossActor) currentActor).boss.Skills) {
                                if (skill._cooldown < 1) {
                                    SelectSkill(skill);
                                    continue;
                                }
                            }
                    } else {
                        // Not AOE skill
                        foreach (Skill skill in ((BossActor) currentActor).boss.Skills) {
                            if (skill._cooldown < 1) {
                                SelectSkill(skill);
                                continue;
                            }
                        }
                    }
                    

                    // if target count is >= 3 then you can use an AOE skill
                    // otherwise use your best ability thats not an attack
                    // if no other abilities then use an attack

                    bossTurn = true;
                }

                state = BossBattleState.WaitForAction;
                break;
            case BossBattleState.ExecuteAction:
                //print("Execute Action");
                // Do Queued Actions then (if there is a heal on multiple members or an attack that hits multiple targets etc)

                if (!PlayerPrefs.HasKey("tutorial_boss_battle_castable_skill")) {
                    foreach (BattleActor actor in TurnOrder) {
                        if (actor is BossActor && ((BossActor) actor).QueuedSkill != null) {
                            StoryManager.Instance.DisplayStory("story_tutorial_boss_battle_interrupt");

                            PlayerPrefs.SetInt("tutorial_boss_battle_castable_skill", 1);
                        }
                    }
                }
                // go to the next actor
                currentActor.UpdateStatusEffects(StatusTriggerTime.EndOfTurn);
                //state = BossBattleState.NewTurn;
                StartEndOfTurn();
                break;
            case BossBattleState.WaitForAction:

                // Update the turn order position based on selected skill
                if (bossTurn && bossActionRoutine == null)
                    AIActionSelect();

                // Select skill from UI
                // on target select add the ability to the queue and
                if (Input.GetMouseButtonDown(0) && !bossTurn && executeActionRoutine == null) {
                    // see what we tapped, but for now just use the action
                    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                    //fctLocation = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    if (hit.collider != null) {
                        BattleActor target = null;
                        //Debug.Log("Target Position: " + hit.collider.gameObject.transform.position + " | hit: " + hit.collider.gameObject.name);

                        CharacterHandler h_handler = hit.collider.gameObject.GetComponent<CharacterHandler>();
                        if (h_handler != null) {
                            print("Have CharacterHandler");
                            // Is a hero
                            foreach (BattleActor hA in TurnOrder) {
                                if (hA is HeroActor)
                                    if (((HeroActor) hA).handler == h_handler)
                                        target = hA;
                            }
                            print(target);
                            if (target != null)
                                ExecuteSkill(target);
                        }
                        MonsterHandler m_handler = hit.collider.gameObject.GetComponent<MonsterHandler>();
                        if (m_handler != null) {
                            // Is a monster
                            foreach (BattleActor bA in TurnOrder) {
                                if (bA is BossActor)
                                    if (((BossActor) bA).handler == m_handler)
                                        target = bA;
                            }
                            //print(target);
                            if (target != null)
                                ExecuteSkill(target);
                        }
                    }
                }
                break;
            case BossBattleState.EndOfBattle:
                bool heroStillExists = false;
                bool bossStillExists = false;
                foreach (BattleActor actor in TurnOrder) {
                    if (actor is HeroActor)
                        heroStillExists = true;
                    if (actor is BossActor)
                        bossStillExists = true;
                }

                ActiveExploration activeZone = playerMan.SelectedBattle;

                if (!bossStillExists) {
                    // Player won
                    float chance = 0.2f + Mathf.Min((activeZone.TreasureFind / 3000f), 0.75f) * 0.8f;
                    for (int i = 0; i < dataMan.globalData.GetGlobalAsInt(GlobalProps.ZONE_MONSTER_COUNT); i++) {
                        if (Random.Range(0f, 1f) <= chance) {
                            LootCrateScreenInterface.GenerateLootCrate(activeZone, MagicFindBoostMultiplier: playerMan.GetBoost(BoostType.MagicFind));
                        }
                    }
                    for (int i = 0; i < activeZone.ChestsEarned; i++) {
                        LootCrateScreenInterface.GenerateLootCrate(activeZone, MagicFindBoostMultiplier: playerMan.GetBoost(BoostType.MagicFind));
                    }
                    LootCrateScreenInterface.GenerateLootCrate(activeZone, TriggerInterfaceAfterLootCratesAdded, true, playerMan.GetBoost(BoostType.MagicFind));
                } else if (!heroStillExists) {
                    // Player was defeated
                    StartCoroutine(ResurrectForGems(activeZone));
                }
                
                playerMan.CurrentLeaderSkill = null;
                state = BossBattleState.ShowResults;
                
                break;
            case BossBattleState.ShowResults:
                break;
        }
    }

    IEnumerator ResurrectForGems(ActiveExploration activeZone) {
        bool optionSelected = false;

        // popup here
        string q = "Would you like to\ntry again?\n<color=#2fb20e>25 Gems</color>";

        ConfirmYesNoInterface.Ask("Resurrect", q)
            .Then(answer => {
                if (answer != "YES") {
                    optionSelected = true;

                    BattleResultsInterface battleResultsPanel = (BattleResultsInterface) MenuManager.Instance.Load("Interface_BattleResults");
                    battleResultsPanel.Initialize(0, 0, 0, activeZone, true);
                } else {
                    AudioManager.Instance.Play(SFX_UI.ShardsChing);

                    CurrencyManager.Cost cost = CurrencyTypes.GEMS.ToCostObj(dataMan.globalData.GetGlobalAsInt(GlobalProps.BATTLE_RESURRECTION_COST));

                    DataManager.API.Currency.AddCurrency(cost)
                        .Then(res => {
                            optionSelected = true;
                            
                            UnityEngine.SceneManagement.SceneManager.LoadScene("BossBattle");
                        });
                }
                
            });

        while (!optionSelected) {
            yield return new WaitForEndOfFrame();
        }


    }
    
    // has the lootcrate for the callback, not needed in this instance
    void TriggerInterfaceAfterLootCratesAdded(LootCrate crate) {
        GameAPIManager.Instance.LootCrates.GetList().Then(res => {
            BattleResultsInterface battleResultsPanel = (BattleResultsInterface) MenuManager.Instance.Load("Interface_BattleResults");
            //Debug.Log("[callback] Looking for Crate Count with an exploration ID of " + playerMan.SelectedBattle.MongoID);
            battleResultsPanel.Initialize(
                data.Zone.BossFight.Gold,
                data.Zone.BossFight.Experience,
                dataMan.GetLootCratesByExploration(playerMan.SelectedBattle.MongoID).Count,
                playerMan.SelectedBattle
            );
        });
    }

    void AIActionSelect() {
        print("Wait for AI Action");

        // An action was selected from either the player or AI start the next turn
        battleInterface.HideMemberArea();

        bossActionRoutine = AISelection();
        StartCoroutine(bossActionRoutine);
    }

    BattleActor GetTargetBasedOnSkill(Skill skill, BattleActor actor) {
        List<HeroActor> targetOptionsHero = new List<HeroActor>();
        List<BossActor> targetOptionsBoss = new List<BossActor>();

        // Get the viable targets
        foreach (BattleActor entity in TurnOrder) {
            if (entity is BossActor && entity.Health > 0)
                targetOptionsBoss.Add((BossActor) entity);
            if (entity is HeroActor && entity.Health > 0)
                targetOptionsHero.Add((HeroActor) entity);
        }

        // Choose an initial target
        if (actor is HeroActor) {
            switch (skill.skillTargeting) {
                case SkillTargeting.Enemy:
                    // Get a boss - return a single entity because the skill will handle the AOE, we just need an initial target
                    return targetOptionsBoss[Random.Range(0, targetOptionsBoss.Count)];
                case SkillTargeting.AllParty:
                case SkillTargeting.Party:
                    // hero
                    return targetOptionsHero[Random.Range(0, targetOptionsHero.Count)];
                default:
                    return actor;
            }
        } else {
            switch (skill.skillTargeting) {
                case SkillTargeting.Enemy:
                    // hero
                    return targetOptionsHero[Random.Range(0, targetOptionsHero.Count)];
                case SkillTargeting.AllParty:
                case SkillTargeting.Party:
                    // Get a boss - return a single entity because the skill will handle the AOE, we just need an initial target
                    return targetOptionsBoss[Random.Range(0, targetOptionsBoss.Count)];
                default:
                    return actor;
            }
        }
    }

    IEnumerator AISelection() {
        // Choose a target 
        List<HeroActor> targetOptions = new List<HeroActor>();
        List<HeroActor> castingOptions = new List<HeroActor>();
        bool actorCasting = false;
        foreach (HeroActor actor in Party) {
            if (actor.Health > 0)
                targetOptions.Add(actor);
            if (actor.QueuedSkill != null)
                actorCasting = true;
        }
        
        BattleActor target = targetOptions[Random.Range(0, targetOptions.Count)];
        if (actorCasting && currentSkill.CanInterrupt && castingOptions.Count > 0) {
            target = castingOptions[Random.Range(0, castingOptions.Count)];
        } else if (currentSkill.TargetType == SkillTargetType.SingleTarget) {
            // if it's not AOE choose a target
            if (Random.Range(0f, 1f) < 0.8f) {
                // choose based on priority
                targetOptions = new List<HeroActor>();
                foreach (HeroActor actor in Party) {
                    if (actor.hero.data.Class == currentSkill.PreferredTarget && actor.Health > 0)
                        targetOptions.Add(actor);
                }

                if (targetOptions.Count > 0)
                    target = targetOptions[Random.Range(0, targetOptions.Count)];
            }
        }
        
        yield return new WaitForSeconds(1.5f);

        // Execute Actions here
        ExecuteSkill(target);
    }

    public void SelectDefend() {
        if (state != BossBattleState.WaitForAction) return;
        
        currentActor.TemporaryOffset = currentActor.AverageSpeed * DefendSkillRef.Weight;
        SortTurnOrder();

        currentSkill = DefendSkillRef;

        battleInterface.UpdateTurnOrder();

        // execute defend
        ExecuteSkill(currentActor);
    }

    public void SelectSkill(Skill skill) {
        //Debug.Log("Skill Selected: " + skill.Identity);
        // if the skill has a delay the tempoffset will be different
        if (skill.CastDelay > 0f) {
            currentActor.TemporaryOffset = currentActor.AverageSpeed * skill.CastDelay;

            currentActor.QueuedSkill = skill;
        } else {
            currentActor.TemporaryOffset = currentActor.AverageSpeed * skill.Weight;

            currentActor.QueuedSkill = null;
        }
        
        foreach (BattleActor actor in TurnOrder) {
            if (actor is BossActor) {
                if (skill.TurnDelayScale > 0f) {
                    actor.TemporaryOffset = actor.AverageSpeed * skill.TurnDelayScale;
                } else {
                    actor.TemporaryOffset = 0f;
                }
            }
        }

        SortTurnOrder();

        // Set the current skill
        currentSkill = skill;

        battleInterface.UpdateTurnOrder();
    }

    public void ExecuteSkill(BattleActor target, bool forceAction = false) {
        if (executeActionRoutine == null) {
            if (currentActor.QueuedSkill != null && !forceAction) {
                // We have a skill that needs to be delayed
                currentActor.QueuedSkillTarget = target;
                currentActor.TriggerCast();

                // Clean up the turn
                // Set the turn order
                currentActor.CurrentSpeedOffset += currentActor.AverageSpeed * currentActor.QueuedSkill.CastDelay;
                currentActor.TemporaryOffset = 0f;

                foreach (BattleActor actor in TurnOrder)
                    if (actor is BossActor)
                        actor.TemporaryOffset = 0f;
                
                SortTurnOrder();

                // Hide the Menu
                battleInterface.HideMemberArea();

                // Do the action
                executeActionRoutine = null;
                bossActionRoutine = null;

                state = BossBattleState.ExecuteAction;
            } else {
                currentActor.StopCast();
                executeActionRoutine = ExecuteSkillTiming(target);
                StartCoroutine(executeActionRoutine);
            }
        }
    }

    IEnumerator ExecuteSkillTiming(BattleActor target, BattleActor alternativeActor = null) {
        // Check skill type, if its an AOE skill send in all of the targets, if its a singletarget skill send in the selected target
        List<BattleActor> targets = new List<BattleActor>();

        //if (alternativeActor == null)  // we may need this but need to be careful with the turn delay section when enabling it
        alternativeActor = currentActor;

        print("Skill Timing: " + alternativeActor);
        
        currentSkill.Init(alternativeActor);
        if (currentSkill.TargetType == SkillTargetType.SingleTarget) {
            // TODO Verify the selected Target
            switch (currentSkill.skillTargeting) {
                case SkillTargeting.Self:
                    targets.Add(alternativeActor);
                    break;
                case SkillTargeting.Enemy:
                case SkillTargeting.Party:
                    targets.Add(target);
                    break;
            }
        } else {
            // Get all the targets based on the skill type
            switch (currentSkill.skillTargeting) {
                case SkillTargeting.Enemy:
                    // Get all of the BossActor Targets
                    if (alternativeActor is HeroActor) {
                        foreach (BattleActor actor in TurnOrder) {
                            if (actor is BossActor)
                                targets.Add(actor);
                        }
                    } else if (alternativeActor is BossActor) {
                        foreach (HeroActor hero in Party) {
                            targets.Add(hero);
                        }
                    }
                    break;
                case SkillTargeting.Party:
                case SkillTargeting.AllParty:
                    // Get all the heroActors
                    if (alternativeActor is HeroActor) {
                        foreach (HeroActor hero in Party) {
                            targets.Add(hero);
                        }
                    } else if (alternativeActor is BossActor) {
                        foreach (BattleActor actor in TurnOrder) {
                            if (actor is BossActor)
                                targets.Add(actor);
                        }
                    }
                    break;
            }
        }


        print("Skill Timing targets count: " + targets.Count);
        if (targets.Count < 1)
            targets.Add(target);

        yield return StartCoroutine(currentSkill.ExecuteSkill(targets));

        // Set the turn order
        if (alternativeActor is HeroActor && ((HeroActor) alternativeActor).TurnDelay != null && Random.Range(0f, 1f) < ((HeroActor) alternativeActor).TurnDelay.chance) {
            alternativeActor.CurrentSpeedOffset += alternativeActor.AverageSpeed * ((HeroActor) alternativeActor).TurnDelay.delayWeight;
        } else {
            alternativeActor.CurrentSpeedOffset += alternativeActor.AverageSpeed * currentSkill.Weight;
        }
        alternativeActor.TemporaryOffset = 0f;
        SortTurnOrder();
        
        // Hide the Menu
        battleInterface.HideMemberArea();

        // Do the action
        executeActionRoutine = null;
        bossActionRoutine = null;
        state = BossBattleState.ExecuteAction;
    }

    // Load the bosses and retrun their speed
    float LoadBoss() {
        float totalSpeed = 0f;
        int index = 0;

        MonsterHandler handler;
        print(" - boss count: " + data.Zone.BossFight.monsters.Count);
        foreach (BossData boss in data.Zone.BossFight.monsters) {
            //Debug.Log(" - Loading Boss: " + boss.Name);
            string bossSpritePath = "Monster/" + boss.Sprite;
            GameObject prefab = Resources.Load<GameObject>(bossSpritePath);
            if(prefab==null) {
                Tracer.traceError("Could not Instantiate Monster: " + bossSpritePath);
                continue;
            }

            GameObject bossObj = (GameObject) Instantiate(prefab);
            bossObj.transform.SetParent(battleRoot.MonsterPositions[index]);
            // set sprite order render as well here
            bossObj.transform.localPosition = Vector3.zero;
            /*Vector3 tempscale = Vector3.one;
            tempscale.x *= prefab.transform.localScale.x;
            bossObj.transform.localScale = tempscale;*/
            bossObj.name = boss.Name;
            handler = bossObj.AddComponent<MonsterHandler>();
            
            BossActor enemy = new BossActor(boss, handler, FCTInterface, this);
            battleRoot.HealthBars[index].gameObject.SetActive(true);
            battleRoot.HealthBars[index].Init(enemy);
            TurnOrder.Add(enemy);
            totalSpeed += boss.Speed;

            index++;
            //Debug.Log("Finished Spawning " + boss.Name);
        }

        return totalSpeed;
    }

    void CalculateTurnOrder(float totalSpeed) {
        foreach(BattleActor actor in TurnOrder) {
            // Calc avg speed
            actor.AverageSpeed = (1f - (actor.speed / totalSpeed));
            actor.CurrentSpeedOffset = actor.AverageSpeed;
        }
        SortTurnOrder();
    }

    void SortTurnOrder() {
        TurnOrder = TurnOrder.OrderBy(x => x.CurrentSpeedOffset + x.TemporaryOffset).ToList();
    }
}
