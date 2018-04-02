using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;

public enum TapBattleState { Battle, Transition}

public class TapBattleController : MonoBehaviour {

    bool showingResults = false;
    
    public TimeSpan remaining = new TimeSpan();

    public TapBattleState state = TapBattleState.Transition;
    public BattlePhases Phase = BattlePhases.Loading;
    ActiveExploration data;
    BattleRootController battleRoot;
    TapBattleInterface TapInterface;

    [Header("Object References")]
    public GameObject tapEffectPrefab;

    [Header("Tap Battle Interface Required Publics")]
    // Public for the Tap Battle Interface
    public FloatingCombatTextInterface FCTInterface;
    public float TapDamage = 0f;

    // Tap Counter
    public float TapSkillTracker = 0f;
    public float TapCounter = 0f;
    public float TapTimer = 0f;
    public float TapTimerTotal = 0f;
    public float TapDamageMultiplier = 1f;

    // DPS counter
    float timer = 0f;
    public float monsterTimer = 0f;

    // Tap damage check for fct
    float damageDealt = 0f;

    // Monster Data
    public TapMonsterActor currentMonster;
    public GameObject EventOfInterest;

    public List<TapSkill> activeSkills = new List<TapSkill>();

    bool initialized = false;
    bool storyEvent = false;
    float storyEventTimer = 0f;
    bool startLoading = false;

    void Start() {
        Invoke("startLoad", 0.5f);
    }

    void startLoad() {
        startLoading = true;
    }

    void Update() {
        switch (Phase) {
            case BattlePhases.Loading:
                if (startLoading) Loading();
                break;
            case BattlePhases.Initialize:
                if (!initialized) Initialize();
                break;
            case BattlePhases.Start:
                StartBattle();
                break;
            case BattlePhases.Battle:
                remaining = data.TimeStarted.AddSeconds(((data.Zone.ZoneHP - data.AccumulatedDamage) / data.DPS) + (DataManager.Instance.globalData.GetGlobalAsInt(GlobalProps.ZONE_MONSTER_COUNT) * GameManager.DEATH_DELAY)) - DateTime.Now;

                RunBattleSimulation();
                break;
            default:
                break;
        }
    }

    public void Loading() {
        data = PlayerManager.Instance.SelectedBattle;

        // Get the BattleRoot Object and Load the UI
        if (battleRoot == null) battleRoot = FindObjectOfType<BattleRootController>();
        if (FCTInterface == null) FCTInterface = (FloatingCombatTextInterface) MenuManager.Instance.Load("Interface_FloatingCombatText");
        if (TapInterface == null) TapInterface = (TapBattleInterface) MenuManager.Instance.Push("Interface_TapBattle");

        // If the battleRoot object is not null go to the next phase
        if (battleRoot != null && FCTInterface != null && TapInterface != null) Phase++;
    }

    void Initialize() {
        // Load Party and Boss in
        int index = 0;

        List<GameObject> heroList = new List<GameObject>();

        foreach (Hero hero in data.Party) {
            string resourcePath = "Hero/" + hero.data.Identity.ToLower() + "/TapModel";
            Tracer.trace(resourcePath);

            GameObject heroPrefab = Resources.Load<GameObject>(resourcePath);
            GameObject heroObj = (GameObject) Instantiate(heroPrefab);
            heroObj.transform.SetParent(battleRoot.PartyPositions[index]);
            // set sprite order render as well here
            heroObj.transform.localPosition = Vector3.zero;
            heroObj.transform.localScale = heroPrefab.transform.localScale;
            heroObj.GetComponent<TapCharacterHandler>().Initialize(CharacterBattleMode.Tap, hero, this);
            heroList.Add(heroObj);

            index++;
        }

        // Setup initial variables
        TapDamage = (data.DPS * 0.3f);

        // Initialize the UI
        TapInterface.Initialize(this, heroList);

        NewMonster();

        initialized = true;

        if (!PlayerPrefs.HasKey("tutorial_tap_battle")) {
            StoryManager.Instance.DisplayStory("story_tutorial_tap_battle", () => { Phase++; });

            PlayerPrefs.SetInt("tutorial_tap_battle", 1);
        } else {
            Phase++; // now done after the story if we need to show the tutorial story piece
        }
    }

    void StartBattle() {
        // For now just increase the Phase, but use this to detect that the user is ready / play the intro stuff
        Phase++;
    }

    public void onHeroSkill(Hero hero) {
        data.AccumulatedDamage += currentMonster.GetHit(hero.GetCoreStat(CoreStats.Damage)); // Change this for the tap specific skills

        // check element on the skill
        //if (hero.Skill.ElementalType == GameManager.Instance.GetCounterElement(currentMonster.ElementalType)) {
        // Or something like this
        //}
    }

    void RunBattleSimulation() {
        if (state == TapBattleState.Battle) {
            AutoBattle();
            HandleTapInput();
            HandleTapTimers();
        }
        if (storyEventTimer > 0f) {
            if (Input.GetMouseButtonDown(0) && !MenuManager.Instance.IsPointerOverGameObject()) {
                TapEventInteract();
            }
        }
    }

    float tapAmountTime = 0f;

    void AutoBattle() {
        // apply DPS to monster HP
        timer += Time.deltaTime;

        foreach (TapSkill skill in activeSkills) {
            if (skill.type == TapSkillType.AutoTap) {
                // auto tap
                float tapAmount = 1f / skill.TapsPerSecond;
                if ((timer - tapAmountTime) > tapAmount) {
                    tapAmountTime += tapAmount;
                    Tap(new Vector3(2f, 1f, -2f));
                }
            }
        }

        if (timer >= 1.0f) {
            // Apply DPS
            float dps = data.DPS;

            foreach(TapSkill skill in activeSkills) {
                if (skill.type == TapSkillType.DPSBoost) {
                    dps *= skill.DamageMultiplier;
                    data.AccumulatedDamage += (dps - data.DPS);
                }
            }

            currentMonster.DealDPSDamage(dps);

            timer -= 1.0f;
            tapAmountTime = 0f;
        }

        if (currentMonster.monster.TapType == TapMonsterType.Timed) {
            if (state == TapBattleState.Battle) {
                monsterTimer -= Time.deltaTime;

                if (monsterTimer <= 0f) {
                    monsterTimer = 0f;
                    // New Monster Transition
                    StartMonsterRun();
                }
            }
        }
    }

    public void ExecuteSkill(TapSkill skill, float damage) {
        data.AccumulatedDamage += damage;

        // Do shield breaking check
        currentMonster.BreakShieldCheck(skill);
    }

    void HandleTapInput() {
        if (Input.GetMouseButtonDown(0) && !MenuManager.Instance.IsPointerOverGameObject()) {
            Tap(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
    }

    void Tap(Vector3 tapLocation) {

        // Tap Counter Calculations
        // Increase the Tap Counter
        TapCounter += 1.0f;

        // Reduce the total time the multiplier will last based on how high the counter is
        TapTimerTotal = 1f + (-0.95f * (TapCounter / 400f));
        TapTimer = TapTimerTotal;

        // Calculate the Tap Damage Multiplier
        if (TapCounter >= 10f)
            TapDamageMultiplier = 1f + (0.5f * (float) TapCounter / 100f);
        else
            TapDamageMultiplier = 1f;

        TapDamageMultiplier = Mathf.Min(3f, TapDamageMultiplier); // cap it at 300%

        // Calculate the Tap Damage Based on the Tap Counter Multiplier
        float Damage = (TapDamage + TapDamage * Random.Range(-0.2f, 0.2f)) * TapDamageMultiplier;


        if (currentMonster.isShielded) {
            TapSkillTracker += 0.05f * (((1f - TapSkillTracker) * 0.8f) + 0.2f);
        }

        foreach (TapSkill skill in activeSkills) {
            if (skill.type == TapSkillType.TapDamageBoost) {
                Damage *= skill.DamageMultiplier;
            }
        }

        damageDealt = currentMonster.GetHit(Damage);
        data.AccumulatedDamage += damageDealt;

        //data.AddedTapDamage += Damage;
        FCTInterface.SpawnText(damageDealt.ToString("0"), Vector2.zero + new Vector2(200f, 300f), 92f); // replace with monster position

        // Tap slashes
        Instantiate(tapEffectPrefab, tapLocation + new Vector3(0f, 0f, 1f), Quaternion.identity);

    }

    void HandleTapTimers() {
        // Tap Counter and Skill Clean up Stuff
        if (TapSkillTracker >= 1f) {
            // Do skill, lock skill bar
            currentMonster.isShielded = false;
            TapSkillTracker = 0f;
        } else {
            TapSkillTracker -= Time.deltaTime * 0.025f;
        }
        TapSkillTracker = Mathf.Max(0f, TapSkillTracker);
        TapSkillTracker = Mathf.Min(1f, TapSkillTracker);

        TapTimer -= Time.deltaTime;
        if (TapTimer <= 0f) {
            TapCounter = 0;
        }
    }

    void ShowResults() {
        //show the tap battle results with a continue screen to go fight the boss
        StartCoroutine(TapBattleComplete());
    }
    
    IEnumerator TapBattleComplete() {
        yield return new WaitForSeconds(GameManager.DEATH_DELAY);
        UnityEngine.SceneManagement.SceneManager.LoadScene("BossBattle");
        // Change this to a UI event, maybe a screen that pops up with a summary or tap effectiveness and the choice to fight the boss or back out
    }



    void NewMonster() {
        RemoveMonster();

        MonsterData monsterdata;
        float chance = Mathf.Min((data.MonsterFind / 3000f) * 0.65f, 0.65f); // max 65% chance to find treasure monsters if they are there
        if (Random.Range(0f, 1f) < chance) {
            monsterdata = GetTimedMonster();
        } else
            monsterdata = data.Zone.Monsters[Random.Range(0, data.Zone.Monsters.Count)];

        // load the sprite, play the intro/spawn
        GameObject monsterObj = (GameObject) Instantiate(Resources.Load("Monster/" + monsterdata.Sprite));
        monsterObj.transform.SetParent(battleRoot.MonsterPositions[0]);
        monsterObj.transform.localPosition = Vector3.zero;
        currentMonster = monsterObj.AddComponent<TapMonsterActor>();
        currentMonster.Initialize(monsterdata, KillMonster);


        if (currentMonster.monster.TapType == TapMonsterType.Timed) {
            monsterTimer = data.Zone.MonsterTimer;
            print("Timed Monster");
        }

        // Call update on UI
        TapInterface.UpdateMonster();

        StartCoroutine(MonsterSpawn());
    }

    MonsterData GetTimedMonster() {
        foreach (MonsterData monster in data.Zone.Monsters) {
            if (monster.TapType == TapMonsterType.Timed)
                return monster;
        }
        return data.Zone.Monsters[Random.Range(0, data.Zone.Monsters.Count)];
    }

    IEnumerator MonsterSpawn() {
        // do spawn animation here

        yield return new WaitForEndOfFrame();

        state = TapBattleState.Battle;
    }

    void KillMonster() {
        state = TapBattleState.Transition;

        if (currentMonster.monster.TapType == TapMonsterType.Timed && monsterTimer > 0f)
            PlayerManager.Instance.SelectedBattle.ChestsEarned++;

        StartCoroutine(MonsterDeath());
    }

    IEnumerator MonsterDeath() {
        TapInterface.ClearMonster();
        
        yield return new WaitForSeconds(GameManager.DEATH_DELAY);

        // Event here?
        if (Random.Range(0f, 1f) < DataManager.Instance.globalData.GetGlobalAsFloat(GlobalProps.TAP_BATTLE_EVENT_TRIGGER) && !data.EventTriggered) {
        //if (Random.Range(0f, 1f) < 2f) { // Test if so we can ignore the logic
            // See if we have seen this event before?
            storyEventTimer = DataManager.Instance.globalData.GetGlobalAsFloat(GlobalProps.TAP_BATTLE_EVENT_DURATION);
            RemoveMonster();

            // Enable the event indicator here and as a click action call the story
            EventOfInterest.SetActive(true);
        }

        while (storyEvent || storyEventTimer > 0f) {
            storyEventTimer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        EventOfInterest.SetActive(false);
        storyEventTimer = 0f;

        // Check if that was the last monster
        if (wasLastMonster()) {
            Phase = BattlePhases.Results;
            ShowResults();
        } else {
            NewMonster();
        }
    }

    public bool IsAbilityTypeInPlay(TapSkillType type) {
        TapSkill remove = null;
        foreach(TapSkill skill in activeSkills) {
            if (skill.type == type && skill.isActive())
                return true;
            else if (skill.type == type && !skill.isActive())
                remove = skill; // will only remove 1 skill per check, but we are checking often enough for it not to matter
        }
        activeSkills.Remove(remove);

        return false;
    }

    void StartMonsterRun() {
        state = TapBattleState.Transition;
        StartCoroutine(MonsterRun());
    }
    IEnumerator MonsterRun() {
        currentMonster.gameObject.transform.DOMoveX(currentMonster.transform.position.x + 4f, 2f);

        yield return new WaitForSeconds(GameManager.DEATH_DELAY);

        NewMonster();
    }

    public void TapEventInteract() {
        EventOfInterest.SetActive(false);
        storyEvent = true;
        StoryManager.Instance.DisplayStory(data.Zone.StoryEvent, () => { storyEvent = false; }, true);
    }

    void RemoveMonster() {
        if (currentMonster != null && currentMonster.gameObject != null) Destroy(currentMonster.gameObject);
    }

    bool wasLastMonster() {
        if (remaining.TotalSeconds <= 0f)
            return true;

        return false;
    }
}
