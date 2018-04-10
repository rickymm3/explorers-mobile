using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class HeroDisplayInterface : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler {
    Hero hero;

    public Image Portrait;
    public TextMeshProUGUI HeroName;
    public TextMeshProUGUI HeroNameHighlight;
    public TextMeshProUGUI HeroQuality;

    public TextMeshProUGUI StatHealth;
    public TextMeshProUGUI StatDamage;
    public TextMeshProUGUI StatDefense;

    public TextMeshProUGUI StatStrength;
    public TextMeshProUGUI StatVitality;
    public TextMeshProUGUI StatIntelligence;
    public TextMeshProUGUI StatSpeed;

    public GameObject retireOverlay;

    bool heroHeld = false;
    float heldTotal = 0.5f;

    public void Initialize(Hero hero) {
        this.hero = hero;

        HeroName.text = hero.data.Name;
        HeroNameHighlight.text = hero.data.Name;
        HeroQuality.transform.parent.gameObject.SetActive(false);
        HeroQuality.gameObject.SetActive(false);
        //HeroQuality.text = hero.quality.ToString();

        StatHealth.text = hero.GetCoreStat(CoreStats.Health).ToString();
        StatDamage.text = hero.GetCoreStat(CoreStats.Damage).ToString();
        StatDefense.text = hero.GetCoreStat(CoreStats.Defense).ToString();

        StatStrength.text = hero.GetPrimaryStat(PrimaryStats.Strength).ToString();
        StatVitality.text = hero.GetPrimaryStat(PrimaryStats.Vitality).ToString();
        StatIntelligence.text = hero.GetPrimaryStat(PrimaryStats.Intelligence).ToString();
        StatSpeed.text = hero.GetPrimaryStat(PrimaryStats.Speed).ToString();

        Portrait.sprite = hero.data.LoadPortraitSprite();
    }

    IEnumerator trackHeldRoutine;
    Vector3 clickOrigin = Vector3.zero;
    public void Btn_Held() {
        // Show retire option
        trackHeldRoutine = TrackHeld();
        StartCoroutine(trackHeldRoutine);
        clickOrigin = Input.mousePosition;
    }

    IEnumerator TrackHeld() {
        float timer = 0f;
        while (trackHeldRoutine != null) {
            timer += Time.deltaTime;

            if (timer > heldTotal) {
                heroHeld = true;

                // Trigger UI Overlay
                retireOverlay.SetActive(true);
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public void Btn_Clicked() {
        StopCoroutine(trackHeldRoutine);
        if (!heroHeld && Input.GetMouseButtonUp(0)) {
            HeroDetailsInterface heroDetails = (HeroDetailsInterface) MenuManager.Instance.Push("Interface_HeroDetails");
            //heroDetails.Initialize(hero, campHeroInterface);
        }
        heroHeld = false;
    }

    public void Btn_Retire() {
        print("Retire " + hero.data.Name);
        // Add the retiring stuff
        HeroRetireRewardsInterface panel = (HeroRetireRewardsInterface) MenuManager.Instance.Push("Interface_HeroRetired");
        panel.Initialize(hero);

        // API call here?
        GameAPIManager.Instance.Heroes.Remove(hero);

        // UI remove
        Destroy(this.gameObject);

        retireOverlay.SetActive(false);
    }

    public void Btn_HideRetire() {
        retireOverlay.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData) {
        //Debug.Log("Pointer down on " + hero.data.Name);
        Btn_Held();
    }

    public void OnPointerUp(PointerEventData eventData) {
        //Debug.Log("Pointer up on " + hero.data.Name);
        Btn_Clicked();
    }

    public void OnPointerClick(PointerEventData eventData) {
        //Debug.Log("Clicked on " + hero.data.Name);
    }
}
