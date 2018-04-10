using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using ExtensionMethods;
using TMPro;
using System.Linq;
using System;

public class CampHeroInterface : PanelWithGetters {
    
    [Header("General Elements")]
    public TextMeshProUGUI noHeroesText;
    public GameObject heroPrefab;
    public RectTransform container;
    public HeroListFilterType filter = HeroListFilterType.Level;
    public TextMeshProUGUI txtFilter;
    public Button btnBack;

    [Header("Selected Hero")]
    public TextMeshProUGUI selectedHeroName;
    public TextMeshProUGUI selectedHeroDescription;
    public Image selectedHeroPortrait;
    public RectTransform uiHeroContainer;
    GameObject selectedHeroAnimations;

    // GC avoidance
    private List<HeroDisplay> heroGOList = new List<HeroDisplay>();
    private HeroDisplay tempHeroPanel = null;
    public List<Hero> heroes = new List<Hero>();

    public int SelectedHeroID = 0;

    Hero selectedHero;
    
    void Start () {
        signals.OnHeroCreated += OnHeroCreated;
        signals.OnExploreStarted += OnExploreStarted;

        PopulateHeroList();
    }

    void OnDestroy() {
        signals.OnHeroCreated -= OnHeroCreated;
        signals.OnExploreStarted -= OnExploreStarted;
    }

    void OnExploreStarted(ActiveExploration exploration) {
        //HideZoneDetails();
        PopulateHeroList();
    }

    void OnHeroCreated(Hero hero) {
        PopulateHeroList();
    }

    public void PopulateHeroList() {
        List<Hero> heroes = PlayerManager.Instance.GetFilteredHeroes(filter, true);

        if (heroes.Count < 1) {
            noHeroesText.gameObject.SetActive(true);
            noHeroesText.text = "No Heroes To Display";
            return;
        } else {
            noHeroesText.gameObject.SetActive(false);
        }

        heroGOList.SetActiveForAll(false);

        txtFilter.text = "<color=#ddd>SORT BY:</color> " + filter; //.ToString()

        // Load Heroes
        for (int c=0; c<heroes.Count; c++) {
            SpawnHero(heroes[c]);
        }

        HeroDisplay firstAvailableHero = heroGOList.Find(hd => hd.hero.ExploringActZone<1);

        HeroSelected(firstAvailableHero);
    }

    public void AddNewHero() {
        PopulateHeroList();
    }
    
    void SpawnHero(Hero hero) {
        tempHeroPanel = heroGOList.GetOrCreate(heroPrefab, container, isAnchoredZero: true);

        // Init the UI
        tempHeroPanel.isDisabledIfExploring = true;
        tempHeroPanel.Initialize(hero, filter, HeroSelected);
    }

    void HeroSelected(HeroDisplay heroDisplay) {
        if(heroDisplay==null) return;

        selectedHero = heroDisplay.hero;
        int index = 0;
        foreach (Hero h in heroes) {
            if (h == selectedHero)
                SelectedHeroID = index;

            index++;
        }

        foreach (HeroDisplay hd in heroGOList) {
            if (hd.gameObject.activeSelf) hd.Deselect();
        }

        heroDisplay.Selected();

        // Populate the selected data here
        selectedHeroName.text = selectedHero.Name;
        selectedHeroDescription.text = "LVL " + selectedHero.Level + " " + selectedHero.data.Class + "\n" + selectedHero.GetPrimaryStat(PrimaryStats.Strength) + " STR - " + selectedHero.GetPrimaryStat(PrimaryStats.Vitality) + " VIT - " + selectedHero.GetPrimaryStat(PrimaryStats.Intelligence) + " INT - " + selectedHero.GetPrimaryStat(PrimaryStats.Speed) + " SPD";
        selectedHeroPortrait.sprite = selectedHero.LoadPortraitSprite();

        Destroy(selectedHeroAnimations);
        selectedHeroAnimations = Instantiate(selectedHero.LoadUIAnimationReference());

        RectTransform rect = selectedHeroAnimations.GetRect();
        rect.SetParent(uiHeroContainer);
        rect.anchoredPosition = new Vector2(0, 925f + rect.anchoredPosition.y);
        rect.localScale = rect.localScale;

        var animHandler = selectedHeroAnimations==null ? null : selectedHeroAnimations.GetComponent<HeroUIAnimationHandler>();
        if(animHandler != null) {
            animHandler.Initialize(selectedHero);
        } else {
            traceError("Cannot find <HeroUIAnimationHandler> component on Hero '{0}'".Format2(selectedHero.data.FullIdentity));
        }
    }

    public void Btn_Filter() {
        // Pop the filter menu here
        print("implement filter UI popup to handle switching filters");
        FilterHeroInterface fiInterface = (FilterHeroInterface) menuMan.Push("Interface_FilterHero");
        fiInterface.Initialize(OnFilterChanged);
    }

    void OnFilterChanged(HeroListFilterType filter) {
        this.filter = filter;

        PopulateHeroList();
    }

    public void Btn_ShowHeroDetails() {
        HeroDetailsInterface heroDetails = (HeroDetailsInterface) MenuManager.Instance.Push("Interface_HeroDetails");
        heroDetails.Initialize(selectedHero, this, btnBack);
    }

    public void Btn_RetireHero() {
        ConfirmYesNoInterface.Ask("Retire Hero", "Are you sure, this\nhero will no longer\nbe accessible?").Then(answer => {
            if (answer == "YES") RetireConfirm();
        });
    }

    void RetireConfirm() {
        // Add the retiring stuff
        HeroRetireRewardsInterface panel = (HeroRetireRewardsInterface) MenuManager.Instance.Push("Interface_HeroRetired");
        panel.Initialize(selectedHero);

        // API call here?
        GameAPIManager.Instance.Heroes.Remove(selectedHero);
        DataManager.Instance.allHeroesList.Remove(selectedHero);
        PopulateHeroList();
    }
    
    public void Btn_RenameHero() {
        ConfirmYesNoInterface renamePrompt = ConfirmYesNoInterface.AskCustom("Rename Hero", "Interface_ConfirmTextPrompt");
        renamePrompt.SetCharLimit(24);
        var input = renamePrompt.GetInputText();
        input.text = selectedHero.Name;

        renamePrompt
            .Then( answer => {
                if (answer=="YES") RenameHero(input.text);
            })
            .Catch( err => {
                traceError("Nope! Not renaming hero: " + err.Message);
            });
    }

    private void RenameHero(string customName) {
        GameAPIManager.API.Heroes.Rename(selectedHero, customName)
            .Then( res => {
                selectedHeroName.text = selectedHero.Name;
            });
    }
}
