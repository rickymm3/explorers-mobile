using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using ExtensionMethods;

public class ActDetailsOverviewInterface : MonoBehaviour {
    ActiveExploration data = null;
    ZoneData zone = null;
    bool revealed = false;
    bool isComplete = false;
    bool isLoaded = false;

    public RectTransform RevealSection;

    ActInformationInterface actInterface;
    public Mask backgroundMask;

    [Header("Zone UI Details")]
    public TextMeshProUGUI ActTitle;
    public TextMeshProUGUI CompletionPercent;
    public TextMeshProUGUI TimeRemaining;
    public GameObject TimeInfo;
    public Image ImageProgress;
    public GameObject BossFightButton;
    public GameObject ExploreButton;
    public GameObject RevealButton;

    [Header("Party UI")]
    public List<Image> portraits = new List<Image>();
    public TextMeshProUGUI StatValues;
    public TextMeshProUGUI ChestCount;

    [Header("UI Elements To Update")]
    public Image colorlessBkg;
    public Image overlayBkg;


    float progress = 0f;
    float progressLast = -1f;
    float timeToReveal = 0.3f;
    float timeFillBackground = 1.0f;

    bool displayMode = false;
    bool isFirstUpdate = true;

    void Update() {
        if (!isLoaded || isComplete || displayMode || data == null || data.DPS <= 0) return;

        TimeInfo.SetActive(true);

        if (progress >= 1) {
            ExploreComplete();
        } else {
            progress = 1 - (data.RemainingSeconds / data.Duration);
            //if (progressLast == progress) return;

            progressLast = progress;
            CompletionPercent.text = Mathf.CeilToInt(progress * 100).ToString() + "% complete";
            TimeRemaining.text = data.Remaining.TotalSeconds.ToHHMMSS(isMonospaceHTML: true);
            ImageProgress.fillAmount = progress; //, isFirstUpdate ? 0 : timeFillBackground).SetEase(Ease.Linear);

            isFirstUpdate = false;
        }
    }

    public void LoadActZoneDetails(ActiveExploration data) {
        this.data = data;
        ResetButtons();

        StatValues.text = data.DPS.ToString("0") + "\n" + Mathf.RoundToInt(data.MagicFind * 100) + "%\n" + Mathf.RoundToInt(data.MonsterFind * 100) + "%";

        int index = 0;
        foreach (Image portrait in portraits) {
            if (index < data.Party.Count) {
                portrait.sprite = data.Party[index].data.LoadPortraitSprite();
            } else {
                portrait.gameObject.SetActive(false);
            }
            index++;
        }

        LoadFromZone(data.Zone, true);
    }

    public void LoadActZoneDetails(ZoneData zone, ActInformationInterface actInterface) {
        this.zone = zone;
        ResetButtons();

        //Set the type
        displayMode = true;
        this.actInterface = actInterface;

        // Hide the other Buttons and show the Explore button
        ExploreButton.SetActive(true);
        
        // Disable the stuf that wont be visible
        RevealSection.gameObject.SetActive(false);
        
        LoadFromZone(zone, false);
    }

    private void LoadFromZone(ZoneData zone, bool showAct) {
        ActTitle.text = (showAct ? "Act " + zone.Act + " - " : "") + "Zone " + zone.Zone + " - " + zone.Name;
        colorlessBkg.sprite = zone.LoadSpriteBackgroundBW();
        overlayBkg.sprite = zone.LoadSpriteBackground();

        Update();

        isLoaded = true;
    }

    private void ResetButtons() {
        backgroundMask.enabled = true;
        ExploreButton.SetActive(false);
        BossFightButton.SetActive(false);
        TimeInfo.SetActive(false);
        RevealButton.SetActive(true);

        isLoaded = false;
        isComplete = false;
    }

    private void ExploreComplete() {
        //if(isComplete) return;

        isComplete = true;
        TimeRemaining.text = "00:00:00";
        CompletionPercent.text = "Explore Complete";
        BossFightButton.SetActive(true);
        TimeInfo.SetActive(false);

        //LootCollection lootObj = data.Zone.LootTable.GetRandomItems();
        //Debug.Log(lootObj.randomItems.ToJSONString(true));
    }

    public void Btn_RevealPartyInformation() {
        revealed = !revealed;

        if (revealed)
            RevealSection.DOAnchorPosY(0, timeToReveal).SetEase(Ease.OutSine);
        else
            RevealSection.DOAnchorPosY(-RevealSection.rect.height, timeToReveal).SetEase(Ease.InSine);
    }

    public void Btn_Fight() {
        Debug.Log("Go fight the boss");
        PlayerManager.Instance.SelectedBattle = data;
        //UnityEngine.SceneManagement.SceneManager.LoadScene("BossBattle");
        StartCoroutine(LoadScene("BossBattle"));
    }

    System.Collections.IEnumerator LoadScene(string scene) {
        Debug.Log("Loading Scene");
        yield return new WaitForSeconds(1f);
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }

    public void Btn_Explore() {
        // set zone on actinfo
        //if (zone != null && actInterface != null)
            //actInterface.SelectZone(zone);

        // TODO set this as selected

        //actInterface.ShowParty();
    }

    [ContextMenu("Explore")]
    public void Btn_AidTheExploration() {
        PlayerManager.Instance.SelectedBattle = data;
        //UnityEngine.SceneManagement.SceneManager.LoadScene("BossBattle"); // Temp for combat testing
        UnityEngine.SceneManagement.SceneManager.LoadScene("TapBattle");
    }

    [ContextMenu("Fake Tap")]
    void AddTapDamageDebug() {
        data.AccumulatedDamage += 2500;
    }
}
