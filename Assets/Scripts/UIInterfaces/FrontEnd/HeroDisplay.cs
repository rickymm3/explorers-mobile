using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using ExtensionMethods;

public class HeroDisplay : Tracer {

    public Image portrait;
    public TextMeshProUGUI statLabel;
    public List<GameObject> stars;
    public CanvasGroup canvasGroup;
    public Image frame;
    public Image selectedBorder;
    public Button btnClear;
    public Button btn;
    public bool isClearable = false;
    public bool isUsingFades = false;
    public bool isDisabledIfExploring = false;

    [Header("Filter Areas")]
    public GameObject RarityFilterArea;
    public GameObject StatFilterArea;

    HeroListFilterType _filter;
    Color _selectColor = "#00CA0A".ToHexColor();
    Action<HeroDisplay> _callback = null;

    Hero _hero;
    public Hero hero {
        get { return _hero; }
        set {
            _hero = value;

            UpdateHeroInfo();
        }
    }

    void Start() {
        btn.onClick.AddListener(Btn_Click);
        if (btnClear==null) return;
        btnClear.onClick.AddListener(Btn_ClearSlot);
    }

    public void Btn_Click() {
        if(_callback!=null) _callback(this);
    }

    public void Initialize(Hero hero, HeroListFilterType filter, Action<HeroDisplay> callback) {
        _callback = callback;
        _filter = filter;
        
        _selectColor.a = 0f;
        selectedBorder.DOColor(_selectColor, 0f);
        
        this.hero = hero;
    }

    public void Btn_ClearSlot() {
        hero = null;
    }

    public void UpdateHeroInfo() {
        btnClear.gameObject.SetActive(false);
        stars.SetActiveForAll(false);
        RarityFilterArea.SetActive(false);
        StatFilterArea.SetActive(true);
        
        if (_hero==null) {
            Color fadeBlack = new Color(0, 0, 0, 0);

            if (portrait.sprite==null || !isUsingFades) {
                portrait.color = fadeBlack;
            } else {
                portrait.DOColor(fadeBlack, 0.3f);
            }

            StatFilterArea.SetActive(false);
            frame.color = "#aaaaaa".ToHexColor();
            return;
        }

        if(isClearable) {
            btnClear.gameObject.SetActive(true);
        }

        portrait.sprite = _hero.LoadPortraitSprite();

        if(isUsingFades) {
            portrait.color = new Color(0, 0, 0, 0);
            portrait.DOColor(Color.white, 0.3f);
        } else {
            portrait.color = Color.white;
        }
        
        frame.color = _hero.GetQualityColor();

        for (int i = 0; i <= (int)hero.Quality; i++) {
            stars[i].SetActive(true);
        }

        if(isDisabledIfExploring && hero.ExploringActZone>0) {
            canvasGroup.alpha = 0.5f; //ColorConstants.HERO_DISABLED.ToHexColor(true);
            btn.interactable = false;
        } else {
            canvasGroup.alpha = 1.0f;
            btn.interactable = true;
        }

        switch (_filter) {
            case HeroListFilterType.Level:
                statLabel.text = "<size=-5>Lvl</size> " + hero.Level.ToString();
                break;
            case HeroListFilterType.Health:
                statLabel.text = hero.GetCoreStat(CoreStats.Health).ToString();
                break;
            case HeroListFilterType.Damage:
                statLabel.text = hero.GetCoreStat(CoreStats.Damage).ToString();
                break;
            case HeroListFilterType.Defense:
                statLabel.text = hero.GetCoreStat(CoreStats.Defense).ToString();
                break;
            case HeroListFilterType.Strength:
                statLabel.text = hero.GetPrimaryStat(PrimaryStats.Strength).ToString();
                break;
            case HeroListFilterType.Intelligence:
                statLabel.text = hero.GetPrimaryStat(PrimaryStats.Intelligence).ToString();
                break;
            case HeroListFilterType.Vitality:
                statLabel.text = hero.GetPrimaryStat(PrimaryStats.Vitality).ToString();
                break;
            case HeroListFilterType.Speed:
                statLabel.text = hero.GetPrimaryStat(PrimaryStats.Speed).ToString();
                break;
            case HeroListFilterType.MagicFind:
                statLabel.text = hero.GetSecondaryStat(SecondaryStats.MagicFind).ToString();
                break;  
            case HeroListFilterType.Rarity:
            default:
                StatFilterArea.SetActive(false);
                RarityFilterArea.SetActive(true);
                break;
        }
    }

    public void Selected() {
        selectedBorder.DOFade(1, 0.3f);
    }

    public void Deselect() {
        selectedBorder.DOFade(0, 0.3f);
    }
}
