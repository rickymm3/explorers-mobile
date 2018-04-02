using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PartyMemberSelector : MonoBehaviour {
    static Color COLOR_CLEAR = new Color(1, 1, 1, 0);

    public Action<PartyMemberSelector> OnPartySlotSelected;
    public Action<PartyMemberSelector> OnPartySlotCleared;
    public RectTransform SelectedIndicator;
    public Image MemberPortrait;
    public Button btnClear;
    
    bool _selected = false;
    Hero _hero = null;

    public Hero Hero {
        get { return _hero; }
        set {
            _hero = value;

            UpdateClearButtonState();

            if (_hero==null) {
                MemberPortrait.DOFade(0, 0.3f);
                return;
            }
            
            // Load image
            MemberPortrait.sprite = _hero.data.LoadPortraitSprite();
            MemberPortrait.color = COLOR_CLEAR;
            MemberPortrait.DOFade(1, 0.3f);
        }
    }
    
    void Start() {
        MemberPortrait.color = COLOR_CLEAR;

        SelectedIndicator.gameObject.SetActive(true);
        SelectedIndicator.localScale = Vector2.zero;

        btnClear.onClick.AddListener( Btn_HeroClear );
        UpdateClearButtonState();
    }

    void UpdateClearButtonState() {
        btnClear.gameObject.SetActive(_selected && _hero != null);
    }

    public void Btn_HeroSelect() {
        //actInterfaceRef.OnPartySlotSelected(this);
        OnPartySlotSelected(this);
    }

    public void Btn_HeroClear() {
        //actInterfaceRef.OnPartySlotClear(this);
        OnPartySlotCleared(this);
    }

    public void ClearSlot() {
        Hero = null;
        Deselect();
    }

    public void Select() {
        _selected = true;
        UpdateClearButtonState();
        SelectedIndicator.DOKill();
        SelectedIndicator.DOScale(1, 0.3f).SetEase(Ease.OutBack);
    }

    public void Deselect() {
        _selected = false;
        UpdateClearButtonState();
        SelectedIndicator.DOKill();
        SelectedIndicator.DOScale(0, 0.3f).SetEase(Ease.InBack);
    }
}
