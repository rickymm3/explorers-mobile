using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExtensionMethods;
using DG.Tweening;
using TMPro;

public class UserProfileHeroGenericDetails : PanelWithGetters {
    [Header("Labels")]
    public TextMeshProUGUI txtHeroName;
    public TextMeshProUGUI txtRarity;
    public TextMeshProUGUI txtElement;
    public TextMeshProUGUI txtDateDiscovered;
    public TextMeshProUGUI txtDateLastCounted;
    public TextMeshProUGUI txtTotalCollected;

    [Header("Images & Templates")]
    public Image imgPortrait;
    public Image imgLeaderSkill;
    public GameObject skillTemplate;

    [Header("Button & Panels")]
    public Image fader;
    public RectTransform panel;
    public Button btnClose;

    // ↓↓ NEVERMIND ↓↓ This only belongs to the generated Hero instance.
    //public Image imgTapSkill;

    HeroData _heroData;
    HeroDiscoveredData _heroDiscovered;

    public void Init(HeroData heroData, HeroDiscoveredData heroDiscovered) {
        _heroData = heroData;
        _heroDiscovered = heroDiscovered;

        imgPortrait.sprite = heroData.LoadBodySprite();
        txtHeroName.text = heroData.Name;
        // TODO: Tyler, setup whatever this should show up:
        txtRarity.text = heroData.Type.ToString();
        txtElement.text = heroData.ElementalType.ToString();

        txtDateDiscovered.text = FormatDateTime(heroDiscovered.dateDiscovered);
        txtDateLastCounted.text = FormatDateTime(heroDiscovered.dateLastCounted);
        txtTotalCollected.text = heroDiscovered.count.ToString();

        if(heroData.LeadershipSkill) {
            imgLeaderSkill.sprite = heroData.LeadershipSkill.LoadSprite();
        } else {
            imgLeaderSkill.transform.parent.gameObject.SetActive(false);
        }

        imgLeaderSkill.transform.parent.GetComponent<SkillDisplayHandler>().Initialize(heroData.LeadershipSkill);
        
        foreach (Skill skill in heroData.Skills) {
            var clone = this.Clone<RectTransform>(skillTemplate);
            var imgSkill = clone.Find("SkillPortrait").GetComponent<Image>();
            var skilldisplay = clone.GetComponent<SkillDisplayHandler>();
            skilldisplay.Initialize(skill);
            //trace("Showing Skill image for Identity: " + skill.Identity + " : " + skill.Identity + " : " + skill.Icon.name);
        }

        skillTemplate.SetActive(false);

        btnClose.onClick.AddListener(Btn_OnClose);

        fader.color = new Color(0,0,0,0);
        fader.DOFade(0.5f, 0.5f);
        panel.localScale = Vector2.zero;
        panel.DOScale(Vector2.one, 0.5f)
            .SetEase(Ease.OutBack);
    }

    private void Btn_OnClose() {
        DoClosingTransition(panel, fader);
    }

    string FormatDateTime(DateTime dt) {
        return dt.ToString("h:mm:ss tt 'on' MMMM d, yyyy", CultureInfo.InvariantCulture);
    }
}
