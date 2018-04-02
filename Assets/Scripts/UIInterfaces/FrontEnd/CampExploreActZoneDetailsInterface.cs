using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using ExtensionMethods;
using System.Linq;
using System;

public class CampExploreActZoneDetailsInterface : Tracer {

    [HideInInspector] public CampExploreInterface exploreInterface;

    [Header("Zone Spawning Details")]
    public GameObject ZoneDisplayPrefab;
    public RectTransform container;

    [Header("Act Details")]
    public TextMeshProUGUI ActNumber;
    public TextMeshProUGUI ActName;

    [Header("Hero filter")]
    public HeroListFilterType filter = HeroListFilterType.Level;

    public Button btnExplore;
    public ChoiceButton btnDifficulty;
    public TextMeshProUGUI buttonLabel;
    
    // GC
    List<GameObject> zoneSelectors = new List<GameObject>();
    private GameObject tempZoneSelector = null;

    Button BackButton;
    ActData act;

    [Header("Zone Buttons")]
    public GameObject ZoneButton;
    List<Image> _zoneButtons = new List<Image>();

    [Header("Party setup")]
    public RectTransform HeroSelectPanel;
    public RectTransform PartyContainer;
    public List<Hero> selectedPartyMembers = new List<Hero>();
    public RectTransform HeroSelectContainer;
    public GameObject HeroPrefab;
    public GridLayoutGroup heroGridLayout;

    public GameObject partyArea;
    List<HeroDisplay> _partyList = new List<HeroDisplay>();
    List<HeroDisplay> _heroesList = new List<HeroDisplay>();
    

    HeroDisplay _currentPartyMemberSelector;
    HeroDisplay _currentHeroSelector;
    bool _isInited = false;
    
    int _currentZone = 1;
    int _currentActZone = 1;

    void Start() {
        InitZoneButtons();
    }

    void InitZoneButtons() {
        if(_isInited) return;

        _isInited = true;

        btnExplore.onClick.AddListener(Btn_RevealPartySelect);

        for (int i = 0; i < 4; i++) {
            int currentIndex = i;
            var zoneButton = this.Clone<Button>(ZoneButton);
            var zoneText = zoneButton.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            var zoneSelected = zoneButton.transform.Find("Selected").GetComponent<Image>();

            zoneButton.onClick.AddListener(() => MoveZoneInformation(currentIndex));
            zoneText.text = (i + 1).ToString();

            _zoneButtons.Add(zoneSelected);
        }

        _zoneButtons.SetActiveForAll(false);

        ZoneButton.SetActive(false);
    }

    public void Initialize(ActData act, Button BackButton = null) {
        this.act = act;

        print("Selected Act[" + act.ActNumber + "] " + act.Name);
        ActNumber.text = "Act " + act.ActNumber;
        ActName.text = act.Name;
        container.DOAnchorPosX(0f, 0f);
        
        MoveZoneInformation(PrefType.LAST_SELECTED_ZONE.GetInt(0), true);
        PopulateZones();
        ClearPartyList();

        for (int h=0; h<5; h++) {
            HeroDisplay hd = _partyList.GetOrCreate(HeroPrefab, partyArea.transform);
            hd.isClearable = true;
            hd.isUsingFades = true;
            hd.Btn_ClearSlot();
            hd.Initialize(null, HeroListFilterType.Level, OnPartySlotSelected);
            hd.btnClear.onClick.AddListener(() => OnPartySlotClear(hd));
        }

        this.BackButton = BackButton;

        if (PlayerPrefs.GetInt("tutorial_step") == 7) {
            StoryManager.Instance.DisplayStory("story_tutorial_explore_choose_zone");
            
            PlayerPrefs.SetInt("tutorial_step", 8);
        }
    }

    private void ClearPartyList() {
        _partyList.Clear();

        foreach (Transform child in partyArea.transform) {
            GameObject.Destroy(child.gameObject);
        }

        selectedPartyMembers.Clear();

        //partyArea.transform.DetachChildren();
    }

    void PopulateZones() {
        foreach (GameObject ZoneSelectObject in zoneSelectors)
            ZoneSelectObject.SetActive(false);

        foreach (ZoneData zone in DataManager.Instance.GetZonesByAct(act.ActNumber)) {
            SpawnAct(zone);
        }
    }

    void SpawnAct(ZoneData zone) {
        tempZoneSelector = zoneSelectors.GetOrCreate(ZoneDisplayPrefab, container, isAnchoredZero: true);
        
        // Init the UI
        tempZoneSelector.GetComponent<CampExploreZoneDetailsInterface>().Initialize(zone);
    }

    public void MoveZoneInformation(int index, bool isImmediate=false) {
        float target = index * -900;

        _currentZone = index;
        _currentActZone = act.ActNumber * 4 + _currentZone + 1;

        PrefType.LAST_SELECTED_ZONE.SetInt(index);

        AudioManager.Instance.Play(SFX_UI.Click);

        InitZoneButtons();

        _zoneButtons.SetActiveForAll(false);
        _zoneButtons[index].gameObject.SetActive(true);
        
        if(isImmediate) {
            container.anchoredPosition = new Vector2(target, container.anchoredPosition.y);
        } else {
            container.DOAnchorPosX(target, 0.5f);
        }

        var explorations = DataManager.Instance.allExplorationsList;

        int numOfSlots = GlobalProps.EXPLORE_LIMIT.GetInt() + PlayerManager.Instance.ExploreSlots;

        if (explorations.Count >= numOfSlots) {
            buttonLabel.text = "Maxed";
            btnExplore.interactable = false;
        } else if (_currentActZone > PlayerManager.Instance.ActZoneCompleted + 1) {
            // display text saying the zone is locked
            btnExplore.interactable = false;
            buttonLabel.text = "Locked";
        } else {
            if (explorations.FindAll(x => x.Zone.ActZoneID == _currentActZone).Count > 0) {
                buttonLabel.text = "Active";
                btnExplore.interactable = false;
            } else {
                buttonLabel.text = "Explore";
                btnExplore.interactable = true;
            }
        }
    }

    public void Btn_RevealPartySelect() {
        ShowParty();
        ShowHeroSelect();
        heroGridLayout.gameObject.SetActive(false);
        heroGridLayout.gameObject.SetActive(true);
    }
    
    public void Btn_Explore() {
        Debug.Log("Explore Act " + act.ActNumber + " Zone " + _currentZone);
        ZoneData data = DataManager.Instance.GetZonesByActAndZoneID(act.ActNumber, _currentZone + 1, btnDifficulty.selected.AsEnum<ZoneDifficulty>());

        if (selectedPartyMembers.Count < 1) {
            TimelineTween.ShakeError(btnExplore.gameObject);
            traceError("No party selected.");
            return;
        } else if (data == null) {
            TimelineTween.ShakeError(btnExplore.gameObject);
            traceError("No Zone Data or Zone Data is Null.");
            return;
        }

        PlayerManager.Instance.StartExplore(data, selectedPartyMembers)
            .Then(res => {
                buttonLabel.text = "Active";
                btnExplore.interactable = false;
            })
            .Catch(err => {
                traceError(GameAPIManager.GetErrorMessage(err));
                TimelineTween.ShakeError(btnExplore.gameObject);
            });

        HideParty();
        if (BackButton != null) BackButton.interactable = true;

        exploreInterface.HideZoneDetails();
    }


    public void Btn_Cancel() {
        selectedPartyMembers.Clear();

        _partyList.ForEach(slot => slot.hero = null);

        DeselectAllParty();
        HideParty();
    }

    public void ShowParty() {
        //Select the 1st slot right-off the bat:
        OnPartySlotSelected(_partyList[0]);        //Slide-in the Party Slots:
        PartyContainer.DOAnchorPosY(215, 0.5f).SetEase(Ease.OutSine);
    }
    
    public void HideParty() {
        AudioManager.Instance.Play(SFX_UI.WooshOut);

        _heroesList.SetActiveForAll(false);

        HeroSelectPanel.DOAnchorPosX(900f, 0.3f)
            .SetEase(Ease.InSine);

        //Slide-out the Party Slots:
        PartyContainer.DOAnchorPosY(-350f, 0.3f)
            .SetEase(Ease.InSine)
            .OnComplete(ClearPartySelection);
    }

    public void ShowHeroSelect() {
        AudioManager.Instance.Play(SFX_UI.WooshIn);

        _heroesList.SetActiveForAll(false);

        List<Hero> heroes = PlayerManager.Instance.GetFilteredHeroes(filter);

        trace("Should be showing heroes: " + heroes.Count + " / "  + _heroesList.Count);

        foreach (Hero hero in heroes) {
            
            if (selectedPartyMembers.Contains(hero)) continue;

            HeroDisplay hd = _heroesList.GetOrCreate(HeroPrefab, HeroSelectContainer);

            hd.Initialize(hero, filter, OnHeroSelected);
        }

        HeroSelectPanel.DOAnchorPosX(0f, 0.5f)
            .SetEase(Ease.OutSine);
    }
    
    public void Btn_Filter() {
        FilterHeroInterface fiInterface = (FilterHeroInterface) MenuManager.Instance.Push("Interface_FilterHero");
        fiInterface.Initialize(OnFilterChanged);
    }

    void OnFilterChanged(HeroListFilterType filter) {
        this.filter = filter;
        
        ShowHeroSelect();
    }

    //////////////////////////////////////////////////////////////////

    public void ClearPartySelection() {
        _partyList.ForEach(slot => slot.Btn_ClearSlot());
    }

    public void DeselectAllParty() {
        //_heroesList.SetActiveForAll(false);

        _partyList.ForEach(slot => slot.Deselect());
    }

    public void OnPartySlotSelected(HeroDisplay member) {
        DeselectAllParty();
        _currentPartyMemberSelector = member;

        member.Selected();
    }

    public void OnPartySlotClear(HeroDisplay member) {
        if (_currentPartyMemberSelector == null) return;

        AudioManager.Instance.Play(SFX_UI.PageFlip);

        //PopulateHeroList(member.hero);
        selectedPartyMembers.Remove(member.hero);
        member.hero = null;

        ShowHeroSelect();
    }

    public void OnHeroSelected(HeroDisplay heroDisplay) {
        _currentHeroSelector = heroDisplay;

        if (_currentPartyMemberSelector == null) {
            print("Missing _currentPartyMemberSelector!");
            return;
        }

        SwapHeroes(heroDisplay.hero);
    }

    public void SwapHeroes(Hero newHero) {
        Hero prevHero = _currentPartyMemberSelector.hero;
        bool isContinueNextSlot = false;

        if (prevHero != null) {
            selectedPartyMembers.Remove(prevHero);
            _currentHeroSelector.hero = prevHero;
            AudioManager.Instance.Play(SFX_UI.Toggle);
        } else {
            AudioManager.Instance.Play(SFX_UI.Click);
            _currentHeroSelector.gameObject.SetActive(false);
            isContinueNextSlot = true;
        }

        selectedPartyMembers.Add(newHero);
        _currentPartyMemberSelector.hero = newHero;

        int slotID = _partyList.IndexOf(_currentPartyMemberSelector);
        if (isContinueNextSlot && slotID < (_partyList.Count - 1)) {
            OnPartySlotSelected(_partyList[slotID + 1]);
        }
    }
}
