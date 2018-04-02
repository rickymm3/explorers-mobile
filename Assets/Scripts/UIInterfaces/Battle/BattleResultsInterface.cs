using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class BattleResultsInterface : PanelWithGetters {
    
    [Header("Victory Intro")]
    public TextMeshProUGUI VictoryText;
    public RectTransform VictoryExpandRect;
    RectTransform VictoryRect;

    [Header("Battle Results")]
    public GameObject ResultsScreenObj;
    public TextMeshProUGUI gpText;
    public TextMeshProUGUI expText;
    public TextMeshProUGUI chestText;
    public GameObject partyMemberPrefab;
    public RectTransform partyContainer;
    public Image FadeOutImage;
    public BattleCratesInterface droppedCrates;
    public RectTransform btnContinue;

    ActiveExploration data;
    int gold = 0;
    int exp = 0;
    int chests = 0;
    bool allowContinue = false;
    bool defeated = false;
    int ExplorationID = -1;

    List<LootCrate> explorationLoot = new List<LootCrate>();

    void Start() {
        ResultsScreenObj.SetActive(false);
    }

    void Update() {
        if (Input.GetMouseButtonDown(0) && allowContinue) {
            //Debug.Log("[BattleResults] Crate Count is " + DataManager.Instance.GetLootCratesByExploration(ExplorationID).Count + " with explorationID " + ExplorationID);

            droppedCrates.Initialize(explorationLoot, this);

            btnContinue.DOAnchorPosY(50f, 1f);
            allowContinue = false;
        }
    }

    public void BtnExitBattle() {
        StartCoroutine(FadeOut());
    }

    public void Initialize(int gold, int exp, int chests, ActiveExploration data, bool defeated = false) {
        this.gold = Mathf.RoundToInt((float)gold * playerMan.GetBoost(BoostType.Gold));
        Debug.Log("gold " + this.gold);
        this.exp = Mathf.RoundToInt((float) exp * playerMan.GetBoost(BoostType.XP));
        Debug.Log("exp " + this.exp);
        this.chests = chests;
        this.defeated = defeated;
        this.data = data;
        Debug.Log("Exploration [Act " + data.Zone.Act + "][Zone " + data.Zone.Zone + "] Mongo ID: " + data.MongoID);
        ExplorationID = data.MongoID;
        explorationLoot = DataManager.Instance.GetLootCratesByExploration(ExplorationID);

        PlayerManager.Instance.ConsumeBoostTime();

        if (defeated) {
            VictoryText.text = "Defeat";
            VictoryExpandRect.GetComponent<TextMeshProUGUI>().text = "Defeat";
        }
        VictoryRect = VictoryText.GetComponent<RectTransform>();

        StartCoroutine(ShowVictory());
	}

    IEnumerator ShowVictory() {
        //yield return VictoryRect.DOScale(1.2f, 1.2f);
        //yield return VictoryRect.DOScale(1f, 0.4f);
        yield return new WaitForSeconds(2f);
        VictoryExpandRect.gameObject.SetActive(true);
        VictoryExpandRect.DOScale(100f, 0.75f);
        yield return new WaitForSeconds(0.5f);
        VictoryText.gameObject.SetActive(false);
        VictoryExpandRect.GetComponent<TextMeshProUGUI>().DOColor(new Color(1f, 1f, 1f, 0f), 1f);

        StartCoroutine(BattleResultsIn());
    }

    IEnumerator BattleResultsIn() {
        ResultsScreenObj.SetActive(true);

        StartCoroutine(TweenIntToValue(gold, gpText, " gold", 2.5f));
        StartCoroutine(TweenIntToValue(exp, expText, " exp", 2.5f));
        StartCoroutine(TweenIntToValue(chests, chestText, " Chests", 2.5f));

        // init the party gaining xp
        int h=0;
        foreach(Hero hero in data.Party) {
            // Spawn hero xp object here
            GameObject member = (GameObject) Instantiate(partyMemberPrefab, partyContainer);
            member.GetComponent<RectTransform>().localScale = Vector3.one;
            member.GetComponent<BattleResultsHeroResults>().Initialize(hero, exp / data.Party.Count, h++);
            Debug.Log("- Hero Details before xp increase: [Level " + hero.Level + "][XP " + hero.Experience + "]");
            hero.Experience += (exp / data.Party.Count);
            hero.ResetSkillsCooldown();

            print("Updating " + hero.data.Name + "'s [" + hero.MongoID + "] Experience to: " + hero.Experience);
            
            API.Heroes.SetXP(hero, hero.Experience)
                .Then(res => {
                    trace("Hero successfully set XP value: " + hero.Experience);
                });
        }


        bool isXPUpdated = false;
        bool isGoldUpdated = false;
        PlayerManager.Instance.Experience += Mathf.CeilToInt(exp * 0.15f);

        API.Currency.AddCurrency(CurrencyTypes.GOLD, gold)
            .Then(res => {
                return API.Users.SetXP(PlayerManager.Instance.Experience);
            })
            .Then(res => { isXPUpdated = true; })
            .Catch(err => { isXPUpdated = true; traceError(err); }); //TODO <-------------- Finish this up!
        
        playerMan.SelectedBattle.CompleteExploration(!defeated);

        while (!isXPUpdated && !isGoldUpdated) yield return new WaitForEndOfFrame();

        // clear exploration

        yield return new WaitForSeconds(2f);

        // TODO add flashing text to 'tap to continue'

        allowContinue = true;
    }

    IEnumerator TweenIntToValue(int value, TextMeshProUGUI textArea, string affix, float duration) {
        float current = 0f;
        float timer = 0f;
        while (timer < duration) {
            timer += Time.deltaTime;
            if (timer > duration) timer = duration;
            current = timer / duration;

            textArea.text = (current * (float)value).ToString("0") + affix;
            
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator FadeOut() {
        droppedCrates.transform.DOScale(0f, 0.5f);
        btnContinue.DOAnchorPosY(-150f, 0.5f);

        yield return new WaitForSeconds(0.5f);

        FadeOutImage.DOColor(new Color(0f, 0f, 0f, 1f), 1.5f);

        yield return new WaitForSeconds(1.6f);

        playerMan.CampScreenToLoadInto = 4;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Camp");
    }
}
