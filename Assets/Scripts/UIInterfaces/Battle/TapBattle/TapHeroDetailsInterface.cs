using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TapHeroDetailsInterface : MonoBehaviour {
    Hero hero;

    public Image skillIcon;
    public Image skillCooldown;
    public Image portrait;
    public Image Element;
    public Image ElementBackground;
    public SkillTooltip tooltip;

    float cooldown = 45f;
    float cooldownTotal = 45f;

    float skillProgress = 0f;
    TapBattleInterface tapBattleRef;
    TapBattleController controller;
    FloatingCombatTextInterface fctInterface;

    float tapduration = 0f;
    bool trackTouch = false;

    TapCharacterHandler handler;

    public void Initialize(Hero hero, TapBattleInterface tapBattleRef, TapBattleController controller, TapCharacterHandler handler) {
        this.hero = hero;
        this.tapBattleRef = tapBattleRef;
        this.controller = controller;
        this.fctInterface = controller.FCTInterface;
        this.handler = handler;

        portrait.sprite = hero.LoadPortraitSprite();
        skillIcon.sprite = hero.tapSkill.sprite;// Resources.Load<Sprite>("Hero/" + hero.data.Identity.ToLower() + "/portrait");
        
        switch (hero.tapSkill.element) {
            case ElementalTypes.Fire:
                Element.sprite = Resources.Load<Sprite>("Items/None/element_fire");
                ElementBackground.sprite = Element.sprite;
                break;
            case ElementalTypes.Water:
                Element.sprite = Resources.Load<Sprite>("Items/None/element_water");
                ElementBackground.sprite = Element.sprite;
                break;
            case ElementalTypes.Nature:
                Element.sprite = Resources.Load<Sprite>("Items/None/element_nature");
                ElementBackground.sprite = Element.sprite;
                break;
            case ElementalTypes.Light:
                Element.sprite = Resources.Load<Sprite>("Items/None/element_light");
                ElementBackground.sprite = Element.sprite;
                break;
            case ElementalTypes.Dark:
                Element.sprite = Resources.Load<Sprite>("Items/None/element_dark");
                ElementBackground.sprite = Element.sprite;
                break;
            default:
                Element.gameObject.SetActive(false);
                ElementBackground.gameObject.SetActive(false);
                break;
        }

        tooltip.Initialize(hero.tapSkill);
        cooldownTotal = hero.tapSkill.cooldown * 60f;// 90f - (60f * ((float) hero.GetPrimaryStat(PrimaryStats.Speed) / 100f));
    }

    void Update() {
        if (hero == null) return;

        //Handle CD
        cooldown = (float)(DateTime.Now - hero.lastUsedTapAbility).TotalSeconds;

        if (cooldown >= cooldownTotal) cooldown = cooldownTotal;

        skillProgress = (1f - (cooldown / cooldownTotal));
        
        if (controller.IsAbilityTypeInPlay(hero.tapSkill.type)) {
            // Grays out the ability if there is a similar one being used already
            skillProgress = 1f;
        }
        skillCooldown.fillAmount = skillProgress;

        if (trackTouch) tapduration += Time.deltaTime;

        if (tapduration > 0.25f) {
            tooltip.ShowTooltip();
        }
    }

    public void Btn_SkillTap() {
        TapSkill skill = hero.tapSkill;
        bool canUseTapAbility = cooldown >= cooldownTotal && !controller.IsAbilityTypeInPlay(skill.type);
        if (!canUseTapAbility) return;

        switch (hero.tapSkill.type) {
            case TapSkillType.DamageSkill:
                controller.ExecuteSkill(hero.tapSkill, hero.GetCoreStat(CoreStats.Damage) * skill.DamageMultiplier);
                //tapBattleRef.data.AccumulatedDamage += hero.GetCoreStat(CoreStats.Damage) * hero.tapSkill.DamageMultiplier;

                fctInterface.SpawnText(hero.GetCoreStat(CoreStats.Damage).ToString("0"), Vector2.zero + new Vector2(0f, 300f), 110f, CombatHitType.Glancing); // replace with monster position

                // for now just cause damage and reset cooldown
                hero.lastUsedTapAbility = DateTime.Now;
                skill.lastUsedTapAbility = hero.lastUsedTapAbility;

                // Play animation for skill
                handler.Skill();
                break;
            case TapSkillType.DPSBoost:
            case TapSkillType.TapDamageBoost:
            case TapSkillType.AutoTap:
                hero.lastUsedTapAbility = DateTime.Now;
                skill.lastUsedTapAbility = hero.lastUsedTapAbility;
                controller.activeSkills.Add(skill);
                break;
        }

        //Update TapAbility with the API:
        GameAPIManager.API.Heroes.SetTapAbility(hero, hero.lastUsedTapAbility)
            .Then(res => {
                Tracer.trace("Successfully updated hero Tap-Ability server-side.");
            })
            .Catch(err => {
                Tracer.traceError("Could not update hero Tap-Ability server-side! " + err.Message);
            });
    }
    
    public void SkillTapDown() {
        tapduration = 0f;
        trackTouch = true;
    }

    public void SkillTapRelease() {
        if (tapduration <= 0.25f)
            Btn_SkillTap();
        else {
            tooltip.HideTooltip();
        }
        trackTouch = false;
        tapduration = 0f;
    }
}
