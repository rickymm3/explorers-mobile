using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using ExtensionMethods;
using System;

public class ActInformationInterface : PanelWithGetters {
    /*
    ActData act;
    ZoneData selectedZone;
    
    [Header("Text Area")]
    public TextMeshProUGUI actTitle;
    public TextMeshProUGUI actDescription;
    public TextMeshProUGUI zoneDifficulty;
    public Image zoneDisplayImage;

    [Header("Zone Area")]
    public GameObject zonePrefab;
    public RectTransform zoneContainer;

    [Header("Hero Select Area")]
    public RectTransform HeroSelectPanel;
    public GameObject HeroSelectPrefab;
    public RectTransform HeroSelectContainer;
    public GameObject partyArea;
    public List<PartyMemberSelector> partyList;

    public RectTransform PartyContainer;

    public List<Hero> selectedPartyMembers = new List<Hero>();
    List<GameObject> heroSelectPrefabList = new List<GameObject>();

    PartyMemberSelector _currentPartyMemberSelector;
    HeroSelectObjectInterface _currentHeroSelector;

    //////////////////////////////////////////////////////////////////

#if UNITY_EDITOR
    [ContextMenu("ERDS -> PopulatePartyArea")]
    void PopulatePartyArea() {
        if (partyArea == null) {
            traceError("Missing 'partyArea', drag-and-drop it!");
            return;
        }

        partyList.Clear();
        partyList.PopulateListFromEachChild(partyArea, @"PartyMember", true);
    }
#endif

    static void ErrorStartingExplore(string reason) {
        Tracer.traceError("Error Starting Explore: " + reason);
    }

    //////////////////////////////////////////////////////////////////

    void Start() {
        partyList.ForEach(slot => {
            slot.OnPartySlotCleared += this.OnPartySlotClear;
            slot.OnPartySlotSelected += this.OnPartySlotSelected;
        });
    }

    public void Init(ActData act) {
        this.act = act;
        
        actTitle.text = "Act " + act.ActNumber + " - " + act.Name;
        actDescription.text = act.Description;

        SelectZone(act.Zones[0]);

        // Load each zone in
        foreach(ZoneData zone in act.Zones) {
            GameObject go = (GameObject) Instantiate(zonePrefab, zoneContainer);
            go.GetComponent<ActDetailsOverviewInterface>().LoadActZoneDetails(zone, this);
        }
    }

    public void SelectZone(ZoneData zone) {
        selectedZone = zone;

        trace("TODO: Change zone difficulty dynamically....");
        zoneDifficulty.text = "Difficulty: Hard";
        zoneDisplayImage.sprite = zone.LoadSprite();

        List<GameObject> heroSelectPrefabList = new List<GameObject>();
    }

    //////////////////////////////////////////////////////////////////

    public void Btn_Back() {
        AudioManager.Instance.Play(SFX_UI.PageFlip);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Camp");
    }
    
    public void Btn_Cancel() {
        selectedPartyMembers.Clear();

        partyList.ForEach(slot => slot.Hero = null);

        DeselectAllParty();
        HideParty();
    }

    public void Btn_Explore() {
        ZoneData data = selectedZone;

        if (selectedPartyMembers.Count < 1) {
            ErrorStartingExplore("No party selected.");
            return;
        } else if (data == null) {
            ErrorStartingExplore("No Zone Data or Zone Data is Null.");
            return;
        }

        playerMan.StartExplore(data, selectedPartyMembers);

        HideParty();
    }

    //////////////////////////////////////////////////////////////////

    public void ShowParty() {
        //Select the 1st slot right-off the bat:
        OnPartySlotSelected(partyList[0]);

        //Slide-in the Party Slots:
        PartyContainer.DOAnchorPosY(0, 0.3f)
            .SetEase(Ease.OutSine);
    }

    public void HideParty() {
        audioMan.Play(SFX_UI.WooshOut);

        heroSelectPrefabList.SetActiveForAll(false);

        HeroSelectPanel.DOAnchorPosX(900f, 0.3f)
            .SetEase(Ease.InSine);

        //Slide-out the Party Slots:
        PartyContainer.DOAnchorPosY(-PartyContainer.rect.height, 0.3f)
            .SetEase(Ease.InSine)
            .OnComplete(ClearPartySelection);
    }

    public void ShowHeroSelect() {
        audioMan.Play(SFX_UI.WooshIn);

        foreach (Hero hero in playerMan.GetAvailableHeroes()) {
            if (selectedPartyMembers.Contains(hero)) continue;

            PopulateHeroList(hero);
        }

        HeroSelectPanel.DOAnchorPosX(0f, 0.5f)
            .SetEase(Ease.OutSine);
    }

    private void PopulateHeroList(Hero hero) {
        // Gets the 1st inactive GameObject OR creates a new one:
        GameObject heroObj = heroSelectPrefabList.GetOrCreate(HeroSelectPrefab);

        // Set parent
        heroObj.transform.SetParent(HeroSelectContainer);
        heroObj.transform.localScale = Vector3.one;
        heroObj.transform.SetAsLastSibling();

        // Init the UI
        heroObj.GetComponent<HeroSelectObjectInterface>().Initialize(hero, OnHeroSelected, filter);
    }

    //////////////////////////////////////////////////////////////////

    public void ClearPartySelection() {
        partyList.ForEach(slot => slot.ClearSlot());
    }

    public void DeselectAllParty() {
        heroSelectPrefabList.SetActiveForAll(false);

        partyList.ForEach(slot => slot.Deselect());
    }

    public void OnPartySlotSelected(PartyMemberSelector member) {
        DeselectAllParty();
        _currentPartyMemberSelector = member;
        
        member.Select();
        ShowHeroSelect();
    }

    public void OnPartySlotClear(PartyMemberSelector member) {
        if(_currentPartyMemberSelector==null) return;

        AudioManager.Instance.Play(SFX_UI.PageFlip);

        PopulateHeroList(member.Hero);
        selectedPartyMembers.Remove(member.Hero);
        member.Hero = null;
    }

    public void OnHeroSelected(HeroSelectObjectInterface heroSelect) {
        _currentHeroSelector = heroSelect;

        if (_currentPartyMemberSelector==null) {
            traceError("Missing _currentPartyMemberSelector!");
            return;
        }
        
        SwapHeroes(heroSelect.Hero);
    }

    //////////////////////////////////////////////////////////////////

    public void SwapHeroes(Hero newHero) {
        Hero prevHero = _currentPartyMemberSelector.Hero;
        bool isContinueNextSlot = false;

        if (prevHero != null) {
            selectedPartyMembers.Remove(prevHero);
            _currentHeroSelector.Hero = prevHero;
            AudioManager.Instance.Play(SFX_UI.Toggle);
        } else {
            AudioManager.Instance.Play(SFX_UI.Click);
            _currentHeroSelector.gameObject.SetActive(false);
            isContinueNextSlot = true;
        }

        selectedPartyMembers.Add(newHero);
        _currentPartyMemberSelector.Hero = newHero;

        int slotID = partyList.IndexOf(_currentPartyMemberSelector);
        if (isContinueNextSlot && slotID < (partyList.Count - 1)) {
            OnPartySlotSelected(partyList[slotID + 1]);
        }
    }*/
}
