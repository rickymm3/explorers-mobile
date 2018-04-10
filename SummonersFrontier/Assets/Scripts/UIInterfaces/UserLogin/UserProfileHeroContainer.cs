using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UserProfileHeroContainer : Tracer {

    //public TextMeshProUGUI txtHeroName;
    public TextMeshProUGUI txtCounter;
    public Button btn;
    public Image imgPortrait;
    public Image imgQuestion;
    public GameObject badgeCounter;

    [HideInInspector] public HeroData heroData;
    [HideInInspector] public HeroDiscoveredData heroDiscovered;
    
    internal void Init(HeroData heroData, HeroDiscoveredData heroDiscovered) {
        this.heroData = heroData;
        this.heroDiscovered = heroDiscovered;

        btn.onClick.AddListener(Btn_OnHeroClick);
    }

    private void Btn_OnHeroClick() {
        var heroDetails = (UserProfileHeroGenericDetails) MenuManager.Instance.Load("Interface_HeroGenericDetails");
        heroDetails.Init(heroData, heroDiscovered);
    }
}
