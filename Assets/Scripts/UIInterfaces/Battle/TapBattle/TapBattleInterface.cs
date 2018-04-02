using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

using Random = UnityEngine.Random;

public class TapBattleInterface : PanelWithGetters {
    public ActiveExploration data = null;

    public GameObject UIShield;
    public GameObject tapEffectPrefab;
    public GameObject incomingBoss;

    [Header("Progress Bar")]
    public RectTransform progressBar;

    [Header("Party Details")]
    public RectTransform PartyArea;
    public GameObject CounterArea;
    public TextMeshProUGUI DPSText;
    public TextMeshProUGUI CounterText;
    public RectTransform CounterBar;
    public TextMeshProUGUI TapDamageText;
    public List<TapHeroDetailsInterface> PartyFrames = new List<TapHeroDetailsInterface>();

    [Header("Monster Details")]
    public TextMeshProUGUI MonsterName;
    public TextMeshProUGUI MonsterHealth;
    public RectTransform MonsterHealthBar;

    [Header("Timed Monster UI")]
    public GameObject MonsterTimerObj;
    public RectTransform MonsterTimerBar;
    public TextMeshProUGUI MonsterTimerText;

    [Header("Monster Element")]
    public GameObject ElementContainer;
    public Image MonsterElement;

    [Header("Skill Details")]
    public TextMeshProUGUI SkillPercent;
    public RectTransform SkillFillBar;

    TapBattleController controller;
    
    public void Initialize(TapBattleController controller, List<GameObject> heroList) {
        this.controller = controller;
        data = PlayerManager.Instance.SelectedBattle;

        DPSText.text = data.DPS.ToString("0");
        TapDamageText.text = controller.TapDamage.ToString("0");

        int index = 0;
        TapCharacterHandler tempHero;
        foreach (TapHeroDetailsInterface heroUI in PartyFrames) {
            if (index < heroList.Count) {
                tempHero = heroList[index].GetComponent<TapCharacterHandler>();
                heroUI.Initialize(tempHero.hero, this, controller, tempHero);
            } else {
                heroUI.gameObject.SetActive(false);
            }

            index++;
        }

        CounterArea.SetActive(false);
    }

    void Update() {
        if (data == null) return;
        if (controller == null) return;

        if (controller.Phase == BattlePhases.Results) {
            // GOTO boss screen
            // Clear Enemy Stuff
            MonsterHealthBar.DOKill();
            MonsterHealthBar.DOScaleX(0f, 0.3f);
            MonsterHealth.text = "0 HP";
            
            // Display UI for incoming boss
            incomingBoss.SetActive(true);
        } else {
            if (controller.state == TapBattleState.Battle) {
                float hpProgress = controller.currentMonster.Health / controller.currentMonster.TotalHealth;
                MonsterHealthBar.DOKill();
                MonsterHealthBar.DOScaleX(hpProgress, 0.3f);
                MonsterHealth.text = Mathf.Max(controller.currentMonster.Health, 0f).ToString("0") + " HP";
            }
        }
        
        // Tap Counter and Skill UI
        if (controller.TapCounter > 10) {
            CounterArea.SetActive(true);
        } else {
            CounterArea.SetActive(false);
        }

        SkillFillBar.DOScaleX(controller.TapSkillTracker, 0.1f);
        SkillPercent.text = Mathf.FloorToInt(controller.TapSkillTracker * 100f) + "%";

        if (controller.TapTimer <= 0f) {
            CounterArea.SetActive(false);
        }

        if (controller.TapCounter > 10 && controller.TapTimer > 0f) {
            CounterBar.DOScaleX(controller.TapTimer / controller.TapTimerTotal, Time.deltaTime);
            CounterText.text = controller.TapDamageMultiplier.ToString("0.0");
        }
        
        if (controller.currentMonster.monster.TapType == TapMonsterType.Timed) {
            if (controller.state == TapBattleState.Battle) {
                MonsterTimerBar.DOScaleX(controller.monsterTimer / data.Zone.MonsterTimer, 0.2f);
                MonsterTimerText.text = "00:" + controller.monsterTimer.ToString("00") + " remain";
            }
        }
        ChangeMonsterShield(controller.currentMonster.isShielded);

        progressBar.DOKill();
        progressBar.DOScaleX(1f - (data.RemainingSeconds / data.Duration), 0.2f);
    }

    void ChangeMonsterShield(bool value) {
        if (UIShield.activeSelf != value) {
            // Change UI Position
            if (value)
                PartyArea.DOAnchorPosY(0f, 0.2f);
            else
                PartyArea.DOAnchorPosY(-100f, 0.2f);
        }
        UIShield.SetActive(value);
    }

    public void ClearMonster() {
        MonsterName.text = "";
        MonsterHealthBar.DOKill();
        MonsterHealthBar.DOScaleX(0f, 0.3f);
        MonsterHealth.text = "0 HP";
    }

    public void UpdateMonster() {
        MonsterName.text = controller.currentMonster.monster.Name;
        ChangeMonsterShield(controller.currentMonster.isShielded);

        ElementContainer.SetActive(controller.currentMonster.isShielded);
        string elementString = "";
        switch(controller.currentMonster.monster.Element) {
            case ElementalTypes.Fire:
                elementString = "elemental_fire";
                break;
            case ElementalTypes.Water:
                elementString = "elemental_water";
                break;
            case ElementalTypes.Nature:
                elementString = "elemental_nature";
                break;
            case ElementalTypes.Light:
                elementString = "elemental_light";
                break;
            case ElementalTypes.Dark:
                elementString = "elemental_dark";
                break;
        }
        MonsterElement.sprite = Resources.Load<Sprite>("Items/None/" + elementString);

        // Pop timer UI
        if (controller.currentMonster.monster.TapType == TapMonsterType.Timed) {
            // Show UI
            MonsterTimerObj.SetActive(true);
        } else {
            // Hide UI
            MonsterTimerObj.SetActive(false);
        }
    }
    
    public void Btn_Back() {
        API.Explorations.Update(data)
            .Then(res => {
                trace("Successfully updated ActiveExploration!");
                trace(res.pretty);
                BackToCamp();
            })
            .Catch( err => {
                traceError("Error updating the ActiveExploration: " + GameAPIManager.GetErrorMessage(err));
                BackToCamp();
            });
    }

    void BackToCamp() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Camp");
    }

    void StartBossFight() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("BossBattle");
    }
}
