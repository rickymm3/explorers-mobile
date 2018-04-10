using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PartyMemberUISection : MonoBehaviour {

    public Image portrait;
    public RectTransform healthBar;
    public GameObject statusObject;
    public RectTransform StatusContainer;

    float progress = 0f;
    HeroActor hero;

    List<GameObject> statusIconList = new List<GameObject>();

    public void SetupHero(HeroActor heroActor) {
        // load portrait, get total hp and current progress
        hero = heroActor;
        hero.onStatusChanged += UpdateStatus;
        statusObject.SetActive(false);

        UpdateStatus();

        portrait.sprite = heroActor.hero.heroData.LoadPortraitSprite();

        if(this.isActiveAndEnabled) {
            StartCoroutine(UpdateHeroUI());
        }
    }
    
    IEnumerator UpdateHeroUI() {
        do {
            progress = (float) ((float) hero.Health / (((float) hero.hero.GetCoreStat(CoreStats.Health)) * PlayerManager.Instance.GetBoost(BoostType.Health)));
            
            healthBar.DOScaleX(progress, 0.25f);

            yield return new WaitForSeconds(0.25f);
        } while (hero.Health > 0f);

        healthBar.DOScaleX(0f, 0.25f);
    }

    void UpdateStatus() {
        foreach (GameObject status in statusIconList)
            status.SetActive(false);

        //print(hero.Name + " showing " + hero.effects.Count + " effects on UI");

        foreach ( StatusEffect effect in hero.effects ) {
            GameObject statusIcon = GetStatusIconGameObject();
            statusIcon.SetActive(true);
            statusIcon.GetComponent<Image>().sprite = effect.StatusIcon;
        }
    }

    GameObject GetStatusIconGameObject() {
        foreach (GameObject status in statusIconList)
            if (!status.activeSelf) return status;

        statusIconList.Add((GameObject) Instantiate(statusObject, StatusContainer));

        return statusIconList[statusIconList.Count - 1];
    }
}
