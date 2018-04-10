using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using DG.Tweening;
using ExtensionMethods;

public class UserProfileTabHeroes : UserProfileTab_Base {

    [Header("Templates")]
    public UserProfileHeroContainer templateHeroContainer;
    public GridLayoutGroup gridLayout;

    private List<HeroDiscoveredData> _heroDiscList;
    public List<HeroDiscoveredData> HeroDiscoveredList {
        get { return _heroDiscList; }
    }

    // Use this for initialization
    void Start() {
        templateHeroContainer.gameObject.SetActive(false);

        WaitForData();
    }

    public override void OnDataLoaded() {
        GameAPIManager.API.Users.GetAnalytics()
            .Then(res => {
                JSONArray jsonHeroesDisc = res["heroesDiscovered"].AsArray;
                _heroDiscList = new List<HeroDiscoveredData>();

                //First, populate a list of mockup data for all the Analytics HeroesDiscovered:
                foreach (JSONNode jsonHeroDisc in jsonHeroesDisc) {
                    _heroDiscList.Add(new HeroDiscoveredData(jsonHeroDisc));
                }

                //Order by the "Order" field written in the JSON / Google Sheet data:
                var heroDataOrdered = DataManager.Instance.heroDataList.Clone();
                heroDataOrdered.Sort((a, b) => a.Order - b.Order);

                foreach (HeroData heroData in heroDataOrdered) {
                    var discovered = _heroDiscList.Find(hero => hero.identity == heroData.Identity);
                    CreateHeroTile(heroData, discovered);
                }

                scrollRect.DOVerticalNormalizedPos(1, 0.3f);
            });
    }

    void CreateHeroTile(HeroData heroData, HeroDiscoveredData heroDiscovered) {
        UserProfileHeroContainer clone = this.Clone<UserProfileHeroContainer>(templateHeroContainer.gameObject);

        clone.imgPortrait.sprite = heroData.LoadPortraitSprite();

        if (heroDiscovered!=null) {
            clone.txtCounter.text = heroDiscovered.count.ToString();
            clone.imgPortrait.color = Color.white;
            clone.imgQuestion.enabled = false;
            clone.badgeCounter.SetActive(true);
            clone.Init(heroData, heroDiscovered);
        } else {
            clone.imgPortrait.color = Color.black;
            clone.imgQuestion.enabled = true;
            clone.badgeCounter.SetActive(false);
        }
    }
}

public class HeroDiscoveredData {
    public string identity;
    public int count;
    public DateTime dateDiscovered;
    public DateTime dateLastCounted;

    public HeroDiscoveredData(JSONNode jsonData) {
        identity = jsonData["identity"];
        count = jsonData["count"].AsInt;
        dateDiscovered = DateTime.Parse(jsonData["dateDiscovered"]);
        dateLastCounted = DateTime.Parse(jsonData["dateLastCounted"]);
    }
}