using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class UpgradeHeroInterface : Panel {
    Hero hero;
    HeroDetailsInterface heroInterface;

    public Image fader;
    public RectTransform RevealPanel;

    [Header("Information Containers")]
    public RectTransform StarContainer;
    public RectTransform AwakenContainer;
    public RectTransform ResultStarContainer;
    public List<GameObject> CurrentStars = new List<GameObject>();
    public List<GameObject> TargetStars = new List<GameObject>();
    public List<GameObject> ResultStars = new List<GameObject>();

    [Header("Relic Icon References ")]
    [Inspectionary] public RelicImageReferenceDictionary IconReferenceList = new RelicImageReferenceDictionary();
    // Might need to add the reference on the heroData instead of by class type

    [Header("UI References")]
    public TextMeshProUGUI AwakenText;
    public RectTransform ModelContainer;
    public Image RelicRequiredIcon;
    public TextMeshProUGUI RelicRequiredAmount;
    public Image MaterialRequiredIcon;
    public TextMeshProUGUI MaterialRequiredAmount;

    [Header("Button References")]
    public Button AcceptButton;
    public Button CancelButton;
    public Button ContinueButton;

    [Header("UI References for Accept")]
    public Image flashImg;

    GameObject heroRef;

    public void Initialize (Hero hero, HeroDetailsInterface heroInterface) {
        this.hero = hero;
        this.heroInterface = heroInterface;

        // Fade the Shadow in
        fader.color = new Color(0f, 0f, 0f, 0f);
        fader.DOColor(new Color(0f, 0f, 0f, 0.7f), 0.3f);

        // Transition the panel in
        RevealPanel.localScale = Vector3.zero;
        RevealPanel.DOScale(1f, 0.4f);

        // Skip worrying about UI configuration
        ResetStars();
        AwakenContainer.gameObject.SetActive(false);
        
        // Spawn hero UI element
        heroRef = Instantiate(hero.LoadUIAnimationReference(), ModelContainer);
        RectTransform heroRT = heroRef.GetComponent<RectTransform>();
        heroRT.anchorMin = new Vector2(0.5f, 0f);
        heroRT.anchorMax = new Vector2(0.5f, 0f);
        heroRT.pivot = new Vector2(0.5f, 0f);
        heroRT.anchoredPosition = new Vector2(0f, 50f);
        heroRT.localScale *= 2.5f;

        if (hero.isAscended) {
            AcceptButton.gameObject.SetActive(false);
            CancelButton.gameObject.SetActive(false);
            ContinueButton.gameObject.SetActive(true);

            StarContainer.gameObject.SetActive(false);

            AwakenText.text = "Limit Reached";
            AwakenContainer.gameObject.SetActive(true);

            // Swap out requirement icons
            MaterialRequiredAmount.text = "None";
            RelicRequiredAmount.text = "None";

            RelicRequiredAmount.color = Color.white;
            MaterialRequiredAmount.color = Color.white;

            RelicRequiredIcon.sprite = null;
            MaterialRequiredIcon.sprite = null;

            RelicRequiredIcon.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            MaterialRequiredIcon.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        } else {
            // Required Relic Checks and display code
            CurrencyTypes relicType = CurrencyTypes.RELICS_SWORD;
            switch (hero.data.Class) {
                case HeroClass.Assassin:
                    relicType = CurrencyTypes.RELICS_BOW;
                    break;
                case HeroClass.Tank:
                    relicType = CurrencyTypes.RELICS_SHIELD;
                    break;
                case HeroClass.Mage:
                    relicType = CurrencyTypes.RELICS_STAFF;
                    break;
                default:
                case HeroClass.Bruiser:
                    // Already set to sword
                    break;
            }

            RelicRequiredIcon.sprite = IconReferenceList[hero.data.Class];

            CurrencyManager.ColorizeSlashText(relicType, 1, RelicRequiredAmount, AcceptButton);

            // Required Material Checks and display code
            Debug.Log("Hero Quality: [" + hero.Quality.ToString() + "]\n");
            int currencyCount = 0;
            bool hasEnoughCurrency = false;
            switch (hero.Quality) {
                case HeroQuality.Common:
                    currencyCount = CurrencyTypes.ESSENCE_LOW.GetAmount();
                    MaterialRequiredIcon.sprite = Resources.Load<Sprite>("Items/Essences/low_essence");
                    MaterialRequiredAmount.text = currencyCount + "/25";

                    if (currencyCount >= 25)
                        hasEnoughCurrency = true;

                    SetCurrentStars(1);
                    SetTargetStars(2);

                    break;
                case HeroQuality.Rare:
                    currencyCount = CurrencyTypes.ESSENCE_MID.GetAmount();
                    MaterialRequiredIcon.sprite = Resources.Load<Sprite>("Items/Essences/medium_essence");
                    MaterialRequiredAmount.text = currencyCount + "/15";

                    if (currencyCount >= 15)
                        hasEnoughCurrency = true;

                    SetCurrentStars(2);
                    SetTargetStars(3);

                    break;
                case HeroQuality.Legendary:
                    currencyCount = CurrencyTypes.ESSENCE_HIGH.GetAmount();
                    MaterialRequiredIcon.sprite = Resources.Load<Sprite>("Items/Essences/high_essence");
                    MaterialRequiredAmount.text = currencyCount + "/15";

                    if (currencyCount >= 15)
                        hasEnoughCurrency = true;

                    // Awaken test show
                    StarContainer.gameObject.SetActive(false);
                    AwakenContainer.gameObject.SetActive(true);
                    break;
            }

            if (!hasEnoughCurrency) {
                // Disable the accept button
                AcceptButton.interactable = false;
                MaterialRequiredAmount.color = Color.red;
            } else {
                MaterialRequiredAmount.color = Color.white;
            }

            if (!hero.isMaxLevel()) {
                AcceptButton.interactable = false;

                AwakenText.text = "Not Max Level";
                AwakenText.color = Color.red;

                StarContainer.gameObject.SetActive(false);
                AwakenContainer.gameObject.SetActive(true);
            }
        }
        
        if (!PlayerPrefs.HasKey("tutorial_hero_upgrades")) {
            StoryManager.Instance.DisplayStory("story_tutorial_hero_upgrades");

            PlayerPrefs.SetInt("tutorial_hero_upgrades", 1);
        }
    }
    
    void ResetStars() {
        foreach (var star in CurrentStars) {
            star.SetActive(false);
        }
        foreach (var star in TargetStars) {
            star.SetActive(false);
        }
        foreach (var star in ResultStars) {
            star.transform.localScale = Vector3.zero;
            star.SetActive(false);
        }
    }

    void SetCurrentStars(int count) {
        for (int i = 0; i < count; i++) {
            Debug.Log("counter in loop for current star, index: [" + i + "]\n");
            CurrentStars[i].SetActive(true);
        }
    }

    void SetTargetStars(int count) {
        for (int i = 0; i < count; i++) {
            TargetStars[i].SetActive(true);
        }
    }

    public void BtnCancel() {
        fader.DOColor(new Color(0f, 0f, 0f, 0f), 0.4f);
        RevealPanel.DOScale(0f, 0.3f);

        StartCoroutine(DelayClose(0.4f));
    }

    IEnumerator DelayClose(float delay) {
        yield return new WaitForSeconds(delay);
        MenuManager.Instance.Pop();
    }

    public void BtnUpgrade() {
        Debug.Log("UPGRADE!");
        // Consume Materials
        // Trigger UI Transitions
        // Flash the screen for an upgrade

        StartCoroutine(UpgradeSequence());
    }

    IEnumerator UpgradeSequence() {
        // Switch buttons to just an OK
        AcceptButton.gameObject.SetActive(false);
        CancelButton.gameObject.SetActive(false);
        ContinueButton.gameObject.SetActive(true);

        // full sequence for an Awaken
        // Flash
        flashImg.DOColor(new Color(1f, 1f, 1f, 1f), 0.75f).SetEase(Ease.InCubic);
        StarContainer.DOScale(0f, 0.5f);
        AwakenContainer.DOScale(0f, 0.5f);

        yield return new WaitForSeconds(0.75f);

        // Do the upgrade here
        // replace this with the actual quality increase after testing is complete
        int quality = (int) hero.Quality + 1;

        // Need to charge the user for the upgrade and feed it through the API Around here

        //if ()
        if (quality <= (int) HeroQuality.Legendary) {
            // Consume currency here
            CurrencyManager.Cost cost = new CurrencyManager.Cost();
            switch (hero.data.Class) {
                case HeroClass.Assassin:
                    cost.AddOrSet(CurrencyTypes.RELICS_BOW, -1);
                    break;
                case HeroClass.Tank:
                    cost.AddOrSet(CurrencyTypes.RELICS_SHIELD, -1);
                    break;
                case HeroClass.Mage:
                    cost.AddOrSet(CurrencyTypes.RELICS_STAFF, -1);
                    break;
                default:
                case HeroClass.Bruiser:
                    cost.AddOrSet(CurrencyTypes.RELICS_SWORD, -1);
                    // Already set to sword
                    break;
            }
            switch (hero.Quality) {
                case HeroQuality.Common:
                    cost.AddOrSet(CurrencyTypes.ESSENCE_LOW, -25);

                    break;
                case HeroQuality.Rare:
                    cost.AddOrSet(CurrencyTypes.ESSENCE_MID, -15);

                    break;
                case HeroQuality.Legendary:
                    cost.AddOrSet(CurrencyTypes.ESSENCE_HIGH, -15);
                    break;
            }
            GameAPIManager.Instance.Currency.AddCurrency(cost);
            hero.IncreaseQuality();
            StartCoroutine(ResultStarTransitionIn(quality, 0.3f));
        } else {
            AwakenHero();

            AwakenText.text = "Awoken";
            AwakenContainer.DOScale(1f, 0.5f);
        }
        
        // Wait for loaded Assets if needed
        yield return new WaitForSeconds(0.25f);
        // Ease out flash
        flashImg.DOColor(new Color(0.5f, 0.5f, 0.5f, 0f), 0.5f).SetEase(Ease.OutCubic);

        // Show star gain if it was a non-awaken upgrade
        if (hero.Quality < HeroQuality.Legendary) {
            StarContainer.gameObject.SetActive(false);
            ResultStarContainer.gameObject.SetActive(true);
        }
    }
    
    IEnumerator ResultStarTransitionIn(int count, float delay) {
        ResultStarContainer.gameObject.SetActive(true);
        for (int i = 0; i <= count; i++) {
            ResultStars[i].SetActive(true);
            ResultStars[i].transform.DOScale(1f, delay * 2f).SetEase(Ease.OutBack);

            yield return new WaitForSeconds(delay);
        }
        heroInterface.campHeroInterface.PopulateHeroList();
    }

    void AwakenHero() {
        Debug.Log("Awaken Hero");
        Random.InitState(GameManager.Instance.GetSeed());

        // Get the target hero Identity
        string newHeroIdentity = "";
        List<HeroData> heroList = DataManager.Instance.heroDataList.FindAll(h => ((h.AwokenReference != null) ? h.AwokenReference.Identity == hero.data.Identity : false));
        
        if (heroList.Count < 1) {
            Debug.LogError("[Error] No heroes to awaken to");
            return;
        }

        print("Awaken Options: " + heroList.Count);
        newHeroIdentity = heroList[Random.Range(0, heroList.Count)].Identity;

        // Get the cost of the awaken
        CurrencyManager.Cost cost = new CurrencyManager.Cost();
        switch (hero.data.Class) {
            case HeroClass.Assassin:
                cost.AddOrSet(CurrencyTypes.RELICS_BOW, 1);
                break;
            case HeroClass.Tank:
                cost.AddOrSet(CurrencyTypes.RELICS_SHIELD, 1);
                break;
            case HeroClass.Mage:
                cost.AddOrSet(CurrencyTypes.RELICS_STAFF, 1);
                break;
            default:
            case HeroClass.Bruiser:
                cost.AddOrSet(CurrencyTypes.RELICS_SWORD, 1);
                // Already set to sword
                break;
        }
        cost.AddOrSet(CurrencyTypes.ESSENCE_HIGH, 15);

        // Swap the identity to preserve the hero seeds and levels
        GameAPIManager.Instance.Heroes.SwapIdentity(hero, newHeroIdentity, cost)
            .Then((res) => {
                //hero = DataManager.Instance.allHeroesList.
                print("updated to " + hero.data.Identity);

                // Destroy the old hero ui piece
                Destroy(heroRef);

                // Update the current UI with the new hero UIModel
                heroRef = Instantiate(hero.LoadUIAnimationReference(), ModelContainer);
                RectTransform heroRT = heroRef.GetComponent<RectTransform>();
                heroRT.anchorMin = new Vector2(0.5f, 0f);
                heroRT.anchorMax = new Vector2(0.5f, 0f);
                heroRT.pivot = new Vector2(0.5f, 0f);
                heroRT.anchoredPosition = new Vector2(0f, 50f);
                heroRT.localScale *= 2.5f;

                // Update the hero details interface with the new hero
                heroInterface.Initialize(hero, heroInterface.campHeroInterface);

                // Populate Hero List
                heroInterface.campHeroInterface.PopulateHeroList();
            });
    }
}
