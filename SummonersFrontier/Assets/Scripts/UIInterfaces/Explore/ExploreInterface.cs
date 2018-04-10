using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using ExtensionMethods;

public class ExploreInterface : Panel {

    public RectTransform ActDetailsPanel;
    HeaderBarInterface UserInformation;

    [Header("Zone Details")]
    public GameObject ZoneDetailsPrefab;
    public RectTransform ZoneDetailsContainer;
    public ZoneMovementInterface zoneMoveRef;

    [Header("Hero Select Area")]
    public RectTransform HeroSelectPanel;
    public GameObject HeroSelectPrefab;
    public RectTransform HeroSelectContainer;
    public List<HeroDisplay> partyList;

    public List<Hero> selectedPartyMembers = new List<Hero>();

    List<GameObject> zonePrefabList = new List<GameObject>();
    List<GameObject> heroSelectPrefabList = new List<GameObject>();

    public int ZoneIndex = 1;
    int ActIndex = 1;

    void Start() {
        //UserInformation = (HeaderBarInterface) MenuManager.Instance.Load("Interface_HeaderBar");
    }

    public void Btn_Back() {
        AudioManager.Instance.Play(SFX_UI.PageFlip);
        //UnityEngine.SceneManagement.SceneManager.LoadScene("Camp");
        MenuManager.Instance.Pop();
    }

    public void GoExplore() {
        if (selectedPartyMembers.Count < 1) {
            ErrorStartingExplore("No party selected.");
            return;
        }

        // Check for 2 parties in the same zone, is that even legal/wanted?

        ZoneData data = DataManager.Instance.GetZonesByActAndZoneID(ActIndex, ZoneIndex);

        if (data == null) {
            ErrorStartingExplore("No Zone Data or Zone Data is Null.");
            return;
        }

        PlayerManager.Instance.StartExplore(data, selectedPartyMembers);
        Debug.Log("Explore Act " + ActIndex + " - Zone " + ZoneIndex + " - " + data.Name);

        ClearPartySelection();
        HideActDetails();
    }

    void ErrorStartingExplore(string reason) {
        Tracer.traceError("Error Starting Explore: " + reason);
    }

    void ClearPartySelection() {
        foreach (HeroDisplay member in partyList) {
            member.Btn_ClearSlot();
        }
    }

    public void ShowActDetails(int act) {
        ActIndex = act;
        AudioManager.Instance.Play(SFX_UI.WooshIn);

        Debug.Log("Loading Act: " + act);
        // Get the zone list based on the act number
        List<ZoneData> zoneList = DataManager.Instance.GetZonesByAct(act);

        // Populate the Zones
        int panel_counter = 0;
        foreach (ZoneData zone in zoneList) {
            GameObject zoneObj = zonePrefabList.GetOrCreate(ZoneDetailsPrefab, ZoneDetailsContainer);
            
            // Init the UI
            //zoneObj.GetComponent<ZoneInformationInterface>().Initialize(zone, zoneMoveRef);
            panel_counter++;
        }
        zoneMoveRef.panels = panel_counter;

        //UserInformation.HideHeader();
        MovePanel(0f, ActDetailsPanel);
    }
    public void HideActDetails() {
        AudioManager.Instance.Play(SFX_UI.WooshOut);

        zonePrefabList.SetActiveForAll(false);
        zoneMoveRef.CancelHit();
        //UserInformation.ShowHeader();
        MovePanel(900f, ActDetailsPanel);
    }

    public void ShowHeroSelect() {
        //System.Action<HeroDisplay> callback
        AudioManager.Instance.Play(SFX_UI.WooshIn);

        foreach (Hero hero in PlayerManager.Instance.GetAvailableHeroes()) {
            if (selectedPartyMembers.Contains(hero)) continue;

            GameObject heroObj = heroSelectPrefabList.GetOrCreate(HeroSelectPrefab, HeroSelectContainer);
            
            // Init the UI
            //heroObj.GetComponent<HeroSelectObjectInterface>().Initialize(hero, callback, filter);
        }

        MovePanel(0f, HeroSelectPanel);
    }

    public void HideHeroSelect() {
        AudioManager.Instance.Play(SFX_UI.WooshOut);

        heroSelectPrefabList.SetActiveForAll(false);

        MovePanel(900f, HeroSelectPanel);
    }

    void MovePanel(float xPosition, RectTransform rectTrans, float duration = 0.5f) {
        rectTrans.DOAnchorPosX(xPosition, duration);
    }

    public void DeselectAllParty() {
        heroSelectPrefabList.SetActiveForAll(false);

        foreach (HeroDisplay member in partyList) {
            member.Deselect();
        }
    }
}
