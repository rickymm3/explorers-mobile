using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class EnemyHealthBarInterface : MonoBehaviour {

    public GameObject hpBarObj;
    public TextMeshProUGUI hpBarText;

    public GameObject StatusPrefab;
    public RectTransform StatusContainer;
    List<GameObject> statusIconList = new List<GameObject>();

    BossActor boss;
    RectTransform hpBar;

    public void Init(BossActor boss) {
        //Debug.Log("Start init of boss: " + boss.Name);
        this.boss = boss;
        boss.onHealthChanged += UpdateHealth;
        boss.onStatusChanged += UpdateStatus;
        hpBar = hpBarObj.GetComponent<RectTransform>();
        hpBar.DOScaleX(1f, 0f);
        hpBarText.text = "100% HP";
    }

    void UpdateHealth() {
        hpBar.DOScaleX(boss.Health / (float) boss.boss.MaxHealth, 0.25f);
        hpBarText.text = Mathf.RoundToInt((boss.Health / (float) boss.boss.MaxHealth) * 100f) + "% HP";

        if (boss.Health <= 0)
            Invoke("RemoveBar", 0.5f);
    }

    void RemoveBar() {
        this.gameObject.SetActive(false);
    }

    void UpdateStatus() {
        foreach (GameObject status in statusIconList)
            status.SetActive(false);

        foreach (StatusEffect effect in boss.effects) {
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
