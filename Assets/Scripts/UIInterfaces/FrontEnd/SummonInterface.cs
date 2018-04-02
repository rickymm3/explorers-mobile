using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;

public class SummonInterface : Panel {
    
    Hero hero;

    public TextMeshProUGUI heroNameText;
    public TextMeshProUGUI heroQualityText;
    public Image HeroImage;

    public Image beamBKG;
    public Image beam;
    public Image gradiantRevealer;
    public Image starburst;
    public Image heroImg; // Switch this to an animated thing

    public List<Image> stars = new List<Image>();

    bool summonComplete = false;
    CampSummonInterface campRef;

    public void Initialize(Hero data) {
        hero = data;
        GameObject go = GameObject.Find("SectionSummon");
        campRef = go.GetComponent<CampSummonInterface>();
        campRef.Crystal.DOColor(new Color(1f, 1f, 1f, 0f), 0.5f);

        StartCoroutine(SummonHero());
    }

    IEnumerator SummonHero() {
        if (hero.Type == HeroType.Hero) {
            // if the hero is a 4*+ do the burst and delay an extra 0.2f
            print("burst for rare hero goes here");
            yield return new WaitForSeconds(0.2f);
        }

        // Do the beam animation stuff, Fade in and upward motion
        beamBKG.rectTransform.DOScaleY(1f, 0.3f);
        beamBKG.rectTransform.DOScaleX(1.5f, 2f);
        beamBKG.DOColor(new Color(1f, 1f, 1f, 0.5f), 1f);

        beam.rectTransform.DOScaleX(0.3f, 4f);
        beam.DOColor(new Color(1f, 1f, 1f, 1f), .75f);
        // Add upward motion particles here        
        yield return new WaitForSeconds(1f);
        beam.DOKill();
        beam.rectTransform.DOScaleX(1.5f, 1f);
        yield return new WaitForSeconds(0.25f);

        gradiantRevealer.DOColor(Color.white, 0.75f);
        gradiantRevealer.rectTransform.DOScaleY(1.25f, 0.5f);
        gradiantRevealer.rectTransform.DOScaleX(2.5f, 0.75f);
        yield return new WaitForSeconds(1.25f);

        beam.rectTransform.DOScaleX(0f, 0.1f);
        beamBKG.rectTransform.DOScaleX(0f, 0.1f);
        beamBKG.DOColor(new Color(1f, 1f, 1f, 0f), 0.5f);
        beam.DOColor(new Color(1f, 1f, 1f, 0f), 0.5f);

        // Swap hero image here
        heroImg.DOColor(Color.white, 0.2f);
        heroImg.sprite = hero.data.LoadBodySprite();
        heroNameText.text = hero.data.Name;
        heroNameText.transform.parent.gameObject.SetActive(true); // activate the shadow object holding the text

        // enable starburst
        starburst.DOColor(Color.white, 0.5f);
        yield return new WaitForSeconds(0.5f); // reveal delay

        // reveal
        gradiantRevealer.rectTransform.DOScaleY(0f, 0.75f);
        gradiantRevealer.rectTransform.DOScaleX(0f, 0.75f);
        gradiantRevealer.DOColor(new Color(1f, 1f, 1f, 0f), 0.75f);
        yield return new WaitForSeconds(0.5f);

        // Do the star reveal here
        for (int starIndex = 0; starIndex <= (int)hero.Quality; starIndex++) {
            stars[starIndex].gameObject.SetActive(true);
            stars[starIndex].rectTransform.DOScale(1f, 0.25f);
            // play sound here
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.25f);

        // complete?
        GameManager.Instance.isSummonComplete = true;
    }
    
	void Update () {
		if (Input.GetMouseButtonDown(0) && GameManager.Instance.isSummonComplete) {
            campRef.Crystal.DOColor(new Color(1f, 1f, 1f, 1f), 0.5f);

            // Clear the Panel
            MenuManager.Instance.Pop();
        }
	}
}
