using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExtensionMethods;
using DG.Tweening;
using TMPro;

public class BossBattleInterface : Panel {

    [Header("(Right-click for some POPULATE commands...)")]
    BossBattleController controller;

    [Header("Hero Select Area")]
    public RectTransform HeroArea;
    public Image portrait;
    public TextMeshProUGUI HeroName;
    public TextMeshProUGUI HeroNameShadowText;
    public TextMeshProUGUI HeroHealthText;
    public RectTransform HeroHealthBar;
    public RectTransform PartyContainer;
    public RectTransform StatusContainer;
    public GameObject StatusPrefab;
    List<GameObject> statusIconList = new List<GameObject>();

    [Header("Skill Area")]
    public GameObject SkillArea;
    public List<SkillUIHandler> SkillSlots = new List<SkillUIHandler>();

    [Header("Party Area")]
    public GameObject PartyArea;
    public List<PartyMemberUISection> PartyList = new List<PartyMemberUISection>();

    [Header("Turn Order")]
    public Image NextActor;
    public Image NextActorFrame;
    public Image NextActorBackground;
    public GameObject TurnObjectPrefab;
    public GameObject TurnOrderContainer;
    public List<Image> turnOrderPortraits = new List<Image>();

    Dictionary<BattleActor, TurnOrderHandler> TurnOrderHandlerList = new Dictionary<BattleActor, TurnOrderHandler>();
    
    public void Initialize(BossBattleController controller) {
        this.controller = controller;
        
        SetupParty();
        HideMemberArea();
    }

#if UNITY_EDITOR
    [ContextMenu("ERDS -> PopulateTurnOrders")]
    void PopulateTurnOrders() {
        if (TurnOrderContainer == null) {
            traceError("Missing TurnOrderContainer, drag-and-drop it!");
            return;
        }

        turnOrderPortraits.Clear();
        turnOrderPortraits.PopulateListFromEachChild(TurnOrderContainer, @"Portrait$");
    }

    [ContextMenu("ERDS -> PopulateSkills")]
    void PopulateSkills() {
        if (SkillArea == null) {
            traceError("Missing SkillArea, drag-and-drop it!");
            return;
        }

        SkillSlots.Clear();
        SkillSlots.PopulateListFromEachChild(SkillArea, "Skill", true);
    }

    [ContextMenu("ERDS -> PopulateParty")]
    void PopulateParty() {
        if (PartyArea == null) {
            traceError("Missing PartyArea, drag-and-drop it!");
            return;
        }

        PartyList.Clear();
        PartyList.PopulateListFromEachChild(PartyArea, "Member", true);
    }
#endif

    public void ShowMemberArea(HeroActor heroActor) {
        portrait.sprite = Resources.Load<Sprite>("Hero/" + heroActor.hero.data.Identity.ToLower() + "/portrait");
        HeroName.text = heroActor.hero.data.Name;
        HeroNameShadowText.text = heroActor.hero.data.Name;
        HeroHealthText.text = heroActor.Health.ToString("0");

        float progress = (heroActor.Health / ((float) heroActor.hero.GetCoreStat(CoreStats.Health) * PlayerManager.Instance.GetBoost(BoostType.Health)));
        HeroHealthBar.DOScaleX(progress, 0.2f);

        LoadSkills(heroActor.hero);

        HeroArea.DOAnchorPosY(0f, 0.2f);
        PartyContainer.DOAnchorPosY(330f, 0.2f);

        StatusPrefab.SetActive(false);
        OnStatusApply(heroActor);
    }
    public void HideMemberArea() {
        HeroArea.DOAnchorPosY(-500f, 0.2f);
        PartyContainer.DOAnchorPosY(30f, 0.2f);
    }

    public void SetCurrentTurn(BattleActor actor) {
        if (actor is HeroActor) {
            NextActor.sprite = Resources.Load<Sprite>("Hero/" + ((HeroActor) actor).hero.data.Identity.ToLower() + "/portrait");
            NextActorFrame.color = Color.white;
            NextActorBackground.color = Color.white;
        } else {
            NextActor.sprite = Resources.Load<Sprite>("Monster/Portrait/" + ((BossActor) actor).boss.Name.Replace(" ", ""));
            NextActorFrame.color = new Color(0.7f, 0f, 0f, 1f);
            NextActorBackground.color = new Color(0.7f, 0f, 0f, 1f);
        }

        // Temporary until the proper shifting UI is in
        UpdateTurnOrder();
    }

    public void UpdateTurnOrder() {
        int index = 0;

        // Setup the dictionary adn update the positional index
        foreach (BattleActor actor in controller.TurnOrder) {
            if (actor.Health <= 0f) {
                // Actor is dead clean it up
                RemoveActorPortrait(actor);
            } else {
                if (TurnOrderHandlerList.ContainsKey(actor)) {
                    TurnOrderHandlerList[actor].index = index;
                } else {
                    TurnOrderHandler handler = new TurnOrderHandler();
                    handler.Initialize(index, TurnOrderContainer.transform, TurnObjectPrefab, actor);
                    TurnOrderHandlerList.Add(actor, handler);
                }
                
                // Handle the index change
                TurnOrderHandlerList[actor].HandleIndexChange();
                TurnOrderHandlerList[actor].Deselect();
                index++;
            }
        }
        TurnOrderHandlerList[controller.currentActor].Select();



        /*foreach (Image turnOrderObject in turnOrderPortraits) {
            if (index < controller.TurnOrder.Count) {
                //if (controller.TurnOrder[index] is HeroActor)
                //    turnOrderObject.sprite = controller.TurnOrder[index].LoadSprite();// Resources.Load<Sprite>("Hero/" + ((HeroActor) controller.TurnOrder[index]).hero.data.Identity.ToLower() + "/portrait");
                //else
                //    turnOrderObject.sprite = Resources.Load<Sprite>("Monster/Portrait/" + ((BossActor) controller.TurnOrder[index]).boss.Name.Replace(" ", ""));
                
                turnOrderObject.sprite = controller.TurnOrder[index].LoadSprite();
            } else {
                turnOrderObject.transform.parent.gameObject.SetActive(false);
            }
            index++;
        }*/
    }

    public void RemoveActorPortrait(BattleActor actor) {
        Debug.Log("--- [TOUI] Removing the actor from the UI turn order and removing the portrait");
        if (actor.Health <= 0)
            StartCoroutine(TurnOrderHandlerList[actor].ActorDied(() => { TurnOrderHandlerList.Remove(actor); }));
    }

    public void Btn_Defend() {
        controller.SelectDefend();
    }

    void SetupParty() {
        int i = 0;
        foreach (PartyMemberUISection member in PartyList) {
            if (i < controller.Party.Count) {
                member.SetupHero(controller.Party[i]);
            } else {
                member.gameObject.SetActive(false);
            }

            i++;
        }
    }

    void LoadSkills(Hero hero) {
        int i = 0;
        foreach(SkillUIHandler skillUI in SkillSlots) {
            if (i < hero.Skills.Count) {
                skillUI.gameObject.SetActive(true);
                skillUI.Initialize(i, hero, this);
                skillUI.Deselect();

                if (i == hero.Skills.Count-1)
                    skillUI.Select();
            } else
                skillUI.gameObject.SetActive(false);

            i++;
        }
    }

    public void SelectSkill(SkillUIHandler currentSkillUI) {
        foreach (SkillUIHandler skillUI in SkillSlots) {
            skillUI.Deselect();
        }

        // Skill was selected
        controller.SelectSkill(currentSkillUI.skill);
        currentSkillUI.Select();
    }

    
    void OnStatusApply(HeroActor hero) {
        foreach (GameObject status in statusIconList)
            status.SetActive(false);

        foreach (StatusEffect effect in hero.effects) {
            GameObject statusIcon = GetStatusIconGameObject();
            statusIcon.SetActive(true);
            statusIcon.GetComponent<Image>().sprite = effect.StatusIcon;
        }
    }

    GameObject GetStatusIconGameObject() {
        foreach (GameObject status in statusIconList)
            if (!status.activeSelf) return status;

        statusIconList.Add((GameObject) Instantiate(StatusPrefab, StatusContainer));

        return statusIconList[statusIconList.Count - 1];
    }
}
