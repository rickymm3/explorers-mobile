using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using ExtensionMethods;

public class CampExploreInterface : MonoBehaviour {

    public ActScrollRectBehaviour actSelector;
    public GameObject ActSelectPrefab;
    public RectTransform container;
    public RectTransform ActZoneDetailsObject;
    public Button BackButton;

    CampExploreActZoneDetailsInterface _actZoneDetails;

    ZoneDifficulty difficulty = ZoneDifficulty.Normal;

    [HideInInspector] public int currentAct = 0;

    List<GameObject> actSelectors = new List<GameObject>();

    void Start () {
        PopulateActSelectList();
        
        ActZoneDetailsObject.DOAnchorPosY(-1200f, 0f);

        actSelector.onActSelect += SelectAct;

        _actZoneDetails = ActZoneDetailsObject.GetComponent<CampExploreActZoneDetailsInterface>();
        _actZoneDetails.exploreInterface = this;

        //SelectAct(PrefType.LAST_SELECTED_ACT.GetInt(1));
    }

    void OnDestroy() {
        actSelector.onActSelect -= SelectAct;
    }

    public void PopulateActSelectList() {
        actSelectors.SetActiveForAll(false);

        foreach (ActData act in DataManager.Instance.actDataList) {
            GameObject actSelector = actSelectors.GetOrCreate(ActSelectPrefab, container, isAnchoredZero: true);

            // Init the UI
            var actDisplay = actSelector.GetComponent<CampExploreActDisplayInterface>();
            bool isAvailable = (act.ActNumber * 4) > PlayerManager.Instance.ActZoneCompleted;
            actDisplay.Initialize(act, isAvailable);
        }
    }

    void SelectAct(int i) {
        //print("act: " + i + " | azc: " + (Mathf.FloorToInt(PlayerManager.Instance.ActZoneCompleted / 4)+1));
        if (i > Mathf.FloorToInt(PlayerManager.Instance.ActZoneCompleted / 4) + 1) return;

        _actZoneDetails.Initialize(DataManager.Instance.actDataList[i - 1]);
        ActZoneDetailsObject.DOAnchorPosY(-100f, 0.5f);

        PrefType.LAST_SELECTED_ACT.SetInt(i);
    }

    public void HideZoneDetails() {
        ActZoneDetailsObject.DOAnchorPosY(-1200f, 0.5f);


        if (PlayerPrefs.GetInt("tutorial_step") == 8) {
            StoryManager.Instance.DisplayStory("story_tutorial_explore_after");

            PlayerPrefs.SetInt("tutorial_step", 9);
        }
    }
}
