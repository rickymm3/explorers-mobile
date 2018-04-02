using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour {

    public GameObject CampImage;
    public GameObject SummonTutorialImage;
    public GameObject HeroDetailsTutorialImage;
    public GameObject ExploreTutorialImage;
    public GameObject ShopTutorialImage;

    public void LockToSummon() {
        SetAllNotActive();
        SummonTutorialImage.SetActive(true);
    }

    public void LockToHeroDetails() {
        SetAllNotActive();
        HeroDetailsTutorialImage.SetActive(true);
    }

    public void LockToExplore() {
        SetAllNotActive();
        ExploreTutorialImage.SetActive(true);
    }

    public void LockToShop() {
        SetAllNotActive();
        ShopTutorialImage.SetActive(true);
    }

    public void Unlock() {
        CampImage.SetActive(true);
        SummonTutorialImage.SetActive(false);
        HeroDetailsTutorialImage.SetActive(false);
        ExploreTutorialImage.SetActive(false);
    }

    void SetAllNotActive() {
        CampImage.SetActive(false);
        SummonTutorialImage.SetActive(false);
        HeroDetailsTutorialImage.SetActive(false);
        ExploreTutorialImage.SetActive(false);
        ShopTutorialImage.SetActive(false);
    }
}
