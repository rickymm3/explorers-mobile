using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExtensionMethods;
using DG.Tweening;
using TMPro;

public class CampSummonInterface : Tracer {
    [Serializable]
    public class ScrollsSpriteDictionarry :SerializableDictionary<CurrencyTypes, SummonInfo> { }

    [Serializable]
    public struct SummonInfo {
        public string label;
        public Sprite sprite;
    }

    public CampInterface camp;
    public GameObject BtnSummonTemplate;
    public Image Crystal;
    [Inspectionary] public ScrollsSpriteDictionarry scrollsSprite;

    [Header("Tutorial References")]
    public Button btnBack;

    SummonButtonContainer _currentChoice;
    
    List<SummonButtonContainer> _summonButtons = new List<SummonButtonContainer>();
    
    // Use this for initialization
    void Start () {
        foreach (var kv in scrollsSprite) {
            CurrencyTypes currency = kv.Key;
            SummonInfo info = kv.Value;
            SummonButtonContainer summonChoice = this.Clone<SummonButtonContainer>(BtnSummonTemplate);
            
            summonChoice.btn.onClick.AddListener(() => BtnAction_Summon(summonChoice));
            summonChoice.label.text = info.label + " SUMMON";
            summonChoice.icon.sprite = info.sprite;
            summonChoice.currency = currency;
            summonChoice.summonType = CurrencyManager.ConvertCurrencyToSummonType(currency);

            summonChoice.UpdateCounter();

            _summonButtons.Add(summonChoice);
        }
        
        BtnSummonTemplate.SetActive(false);

        PlayerManager.signals.OnChangedCurrency += OnChangedCurrency;
    }

    private void OnDestroy() {
        PlayerManager.signals.OnChangedCurrency -= OnChangedCurrency;
    }

    private void OnChangedCurrency(int newValue, int oldValue, CurrencyTypes currency) {
        var summonChoice = _summonButtons.Find(choice => choice.currency == currency);
        if(summonChoice==null) return;

        summonChoice.UpdateCounter();
    }

    public void BtnAction_Summon(SummonButtonContainer summonChoice) {
        if (!GameManager.Instance.isSummonComplete) return;

        _currentChoice = summonChoice;
        _summonButtons.ForEach( choice => choice.btn.interactable = false );

        try {
            GameManager.Instance.SummonHeroAction(summonChoice.summonType, OnSummonComplete);
        } catch(Exception err) {
            TimelineTween.ShakeError(_currentChoice.gameObject);
            AudioManager.Instance.Play(SFX_UI.Invalid);
            traceError("Error summoning hero: " + err.Message);
            traceError(err);

            OnSummonComplete(null);
            GameManager.Instance.isSummonComplete = true;

        }
    }

    private void OnSummonComplete(Hero summonedHero) {
        _summonButtons.ForEach(choice => choice.btn.interactable = true);

        //Crystal.DOColor(new Color(1f, 1f, 1f, 0f), 0.5f);

        _currentChoice.UpdateCounter();

        btnBack.interactable = true;
    }
}
