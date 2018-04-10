using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using NodeJS;
using SimpleJSON;
using ExtensionMethods;

public class CampInterface : PanelWithGetters {
    static string[] SECTION_TITLES = new string[] {"TRADER", "HEROES", "CAMP", "SUMMON", "EXPLORE", "RESEARCH", "ENCHANTER", "PREMIUM SHOP"};

    //TODO may want to adjust the UI to use an enum instead of an index for more control

    public ScrollRectSnap ScrollMenuRef;

    public int panelIndex = 2;

    [Header("Menu References")]
    public TextMeshProUGUI screenLabel;
    public RectTransform menu;
    public Button btnBack;

    [Header("Fade Out Image")]
    public Image FadeOutImage;

    [Header("Explore Screen")]
    public TextMeshProUGUI noExploreText;
    public RectTransform ExploreDetailsContainer;
    public GameObject ExploreDetailsPrefab;
    public GameObject ExploreDetailsEmptyPrefab;

    [Header("Trader Screen")]
    public ShopInterface shopInterface;
    //public RectTransform BuyShopScreen;


    [Header("Enchanting Screen")]
    public CampEnchantingInterface enchantingInterface;


    HeaderBarInterface UserInformation;

    List<GameObject> heroGOList = new List<GameObject>();
    List<GameObject> exploreGOList = new List<GameObject>();
    List<GameObject> emptyExploreGOList = new List<GameObject>();
    List<GameObject> removeRef = new List<GameObject>();

    TutorialManager tutorialManager;

    public void Initialize(HeaderBarInterface userInfo) {
        if(!DataManager.Instance.isLoaded) return;

        tutorialManager = GameObject.Find("TutorialManager").GetComponent<TutorialManager>();
        MenuManager.Instance.Remove("Interface_Splash");

        Name = "CampInterface";

        UserInformation = userInfo;
        UserInformation.ShowHeader();

        signals.OnExploreStarted += OnExploreStarted;

        ScrollMenuRef.onIndexChanged += OnIndexChangedFromSwipe;

        //Triggers callbacks hooked to ".OnStatusChanged", such as the mailbox icon.
        API.Messages.CheckInbox();

        LoadExplorePanel();
        ChangePanelIndex(2, false);

        // Check player level ups
        StartCoroutine(CheckLevel());

        if (playerMan.CampScreenToLoadInto > -1) {
            ChangePanelIndex(playerMan.CampScreenToLoadInto, false);
            playerMan.CampScreenToLoadInto = -1;
        }

        if (!PlayerPrefs.HasKey("ftue_intro_story") && !PlayerPrefs.HasKey("tutorial_step")) {
            StoryManager.Instance.DisplayStory("story_intro_story", tutorialManager.LockToSummon);
            PlayerPrefs.SetInt("tutorial_step", 1);
            PlayerPrefs.SetInt("ftue_intro_story", 1);
        } else {
            // If the user quit out partway through the tutorial, show the current state here
            switch(PlayerPrefs.GetInt("tutorial_step")) {
                case 1:
                    tutorialManager.LockToSummon();
                    break;
                case 2:
                    tutorialManager.LockToSummon();
                    break;
                case 3:
                    tutorialManager.LockToHeroDetails();
                    break;
                case 4:
                    tutorialManager.LockToHeroDetails();
                    PlayerPrefs.SetInt("tutorial_step", 3);
                    break;
                case 5:
                case 6:
                case 7:
                case 8:
                    StoryManager.Instance.DisplayStory("story_tutorial_herodetails_complete", tutorialManager.LockToExplore);
                    PlayerPrefs.SetInt("tutorial_step", 6);
                    break;
                case 9:
                    // Play the hero details complete story
                    StoryManager.Instance.DisplayStory("story_tutorial_end_ftue", tutorialManager.Unlock);
                    PlayerPrefs.SetInt("tutorial_step", 10);
                    break;
            }
        }
    }

    IEnumerator CheckLevel() {
        while (PlayerManager.Instance.Level > PlayerManager.Instance.LastLevel) {
            if (!PlayerManager.Instance.isLevelSequenceInProgress) {
                PlayerManager.Instance.LevelSequence();
            }
            yield return new WaitForEndOfFrame();
        }
    }

    void OnDestroy() {
        signals.OnExploreStarted -= OnExploreStarted;
    }

    void OnExploreStarted(ActiveExploration newExploration) {
        LoadExplorePanel();
    }

    void LoadExplorePanel() {
        ExplorationList explorationList = dataMan.allExplorationsList;
        noExploreText.gameObject.SetActive(false);

        exploreGOList.SetActiveForAll(false);
        emptyExploreGOList.SetActiveForAll(false);
        
        // Load Explore
        GameObject exploreObj;

        int numOfSlots = GlobalProps.EXPLORE_LIMIT.GetInt() + playerMan.ExploreSlots;
        for (int i = 0; i < numOfSlots; i++) {
            if(i < explorationList.Count) {
                //For each active explorations, instantiate an actual ActDetailsOverview UI:
                ActiveExploration mission = explorationList[i];
                exploreObj = exploreGOList.GetOrCreate(ExploreDetailsPrefab, ExploreDetailsContainer);
                exploreObj.GetComponent<ActDetailsOverviewInterface>().LoadActZoneDetails(mission);
            } else {
                //For inactive ones, just populate an empty "NO EXPLORATION" UI:
                exploreObj = emptyExploreGOList.GetOrCreate(ExploreDetailsEmptyPrefab, ExploreDetailsContainer);
            }
            
            exploreObj.transform.SetAsLastSibling();
        }
    }

    void OnIndexChangedFromSwipe(int index) {
        ChangeTab(index);
    }

    public void BtnTrader() { ChangePanelIndex(0); }
    public void BtnHeroes() { ChangePanelIndex(1); }
    public void BtnCamp() { ChangePanelIndex(2); }
    public void BtnSummonHeroes() { ChangePanelIndex(3); }
    public void BtnExplore() { ChangePanelIndex(4); }
    public void BtnPremiumShop() { ChangePanelIndex(7); }

    public void BtnLootCrate() {
        //MenuManager.Instance.Remove("CampInterface");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Crates");
    }

    IEnumerator screenChangeRoutine = null;
    public void ChangePanelIndex(int index, bool showFade = true) {
        if(ScrollMenuRef==null) return; //panelIndex == index
        //Debug.Log("Changing Panel to index: " + index);
        if (screenChangeRoutine != null) StopCoroutine(screenChangeRoutine);
        screenChangeRoutine = ChangeScreen(index, showFade);
        StartCoroutine(screenChangeRoutine);
    }

    IEnumerator ChangeScreen(int index, bool showFade) {
        AudioManager.Instance.Play(SFX_UI.Click);
        if (showFade) ScreenFaderManager.Instance.Fade(0.15f);
        yield return new WaitForSeconds(0.15f);
        ChangeTab(index);
        ScrollMenuRef.ForceChange(panelIndex);

        yield return new WaitForSeconds(0.05f);
        ScreenFaderManager.Instance.Fade(0.15f, FadeStyle.Out);

        yield return new WaitForSeconds(0.15f);

        switch(index) {
            case 0:
                // Shop
                if (!PlayerPrefs.HasKey("tutorial_shop")) {
                    StoryManager.Instance.DisplayStory("story_tutorial_shop");

                    PlayerPrefs.SetInt("tutorial_shop", 1);
                }
                break;
            case 1:
                // Hero Details
                if (PlayerPrefs.GetInt("tutorial_step") == 3) {
                    StoryManager.Instance.DisplayStory("story_tutorial_herodetails");
                    
                    btnBack.interactable = false;

                    PlayerPrefs.SetInt("tutorial_step", 4);
                }
                break;
            case 2:
                // Camp
                switch (PlayerPrefs.GetInt("tutorial_step")) {
                    case 2:
                        // Play the summon complete story
                        StoryManager.Instance.DisplayStory("story_tutorial_summon_complete", tutorialManager.LockToHeroDetails);
                        PlayerPrefs.SetInt("tutorial_step", 3);
                        break;
                    case 5:
                        // Play the hero details complete story
                        StoryManager.Instance.DisplayStory("story_tutorial_herodetails_complete", tutorialManager.LockToExplore);
                        PlayerPrefs.SetInt("tutorial_step", 6);
                        break;
                    case 9:
                        // Play the hero details complete story
                        StoryManager.Instance.DisplayStory("story_tutorial_end_ftue", tutorialManager.Unlock);
                        PlayerPrefs.SetInt("tutorial_step", 10);
                        break;
                    default:
                        break;
                }
                break;
            case 3:
                // Summon
                if (PlayerPrefs.GetInt("tutorial_step") == 1) {
                    StoryManager.Instance.DisplayStory("story_tutorial_summon");
                    
                    btnBack.interactable = false;

                    PlayerPrefs.SetInt("tutorial_step", 2);
                }
                break;
            case 4:
                // Explore
                if (PlayerPrefs.GetInt("tutorial_step") == 6) {
                    StoryManager.Instance.DisplayStory("story_tutorial_explore");
                    
                    btnBack.interactable = false;

                    PlayerPrefs.SetInt("tutorial_step", 7);
                }
                break;
            case 5:
                // Research Lab
                if (!PlayerPrefs.HasKey("tutorial_researchlab")) {
                    StoryManager.Instance.DisplayStory("story_tutorial_research");

                    PlayerPrefs.SetInt("tutorial_researchlab", 1);
                }
                break;
            case 6:
                // Enchanting
                if (!PlayerPrefs.HasKey("tutorial_enchanting")) {
                    StoryManager.Instance.DisplayStory("story_tutorial_enchanting");

                    PlayerPrefs.SetInt("tutorial_enchanting", 1);
                }
                break;
        }
    }

    private void ChangeTab(int index) {
        // Make sure the index is valid
        if (index < 0 || index >= ScrollMenuRef.screens) return;

        screenLabel.text = SECTION_TITLES[index];
        //UserInformation.SetSectionTitle(SECTION_TITLES[index]);
        panelIndex = index;

        if (index == 2)
            menu.DOAnchorPosY(-130f, 0.05f);
        else
            menu.DOAnchorPosY(0f, 0.05f);

        switch (index) {
            case 2:
                GameObject resetCampScroll = GameObject.Find("ParallaxHandler");
                if (resetCampScroll != null)
                    resetCampScroll.GetComponent<ParallaxHandler>().ResetScrollPosition();
                break;
            case 4:
                LoadExplorePanel();
                break;
            case 6:
                enchantingInterface.Initialize();
                break;
        }   
    }

    public void Btn_LoadExploreScreen() {
        AudioManager.Instance.Play(SFX_UI.PageFlip);

        // TODO make a transition between different sections (Pokemon Magicarp Game Style? it was pretty slick)
        UnityEngine.SceneManagement.SceneManager.LoadScene("Explore");
    }
}
