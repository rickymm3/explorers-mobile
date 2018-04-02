using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HeroRetireRewardsInterface : Panel {
    public GameObject rewardPrefab;
    public RectTransform container;
    public RectTransform btnContinue;
    System.Action callback = null;

    public void Initialize(Hero hero, System.Action callback = null) {
        this.callback = callback;
        //RetireRewardsData resultingTable;// = new RetireRewardsData();
        Dictionary<string, int> rewards = DataManager.Instance.GetRetireRewardsForHero(hero).GetRewards();
        
        StartCoroutine(SetupThePanel(rewards));

        PlayerManager.API.Heroes.Remove(hero)
            .Then(res => {
                Debug.Log(hero.Name + "hero being removed");
                DataManager.Instance.allHeroesList.Remove(hero);
            });

        // Might need to remove from active list
    }
    
    IEnumerator SetupThePanel(Dictionary<string, int> rewards) {
        // Spawn each item reveal with a delay/pop-in effect
        GameObject temp;

        foreach (string item in rewards.Keys) {
            temp = (Instantiate(rewardPrefab));
            temp.transform.SetParent(container.transform);
            temp.transform.localPosition = Vector3.zero;
            temp.transform.localScale = Vector3.zero;
            temp.transform.DOScale(1f, 0.5f);
            temp.GetComponent<RetireRewardInterface>().Init(item, rewards[item]); // we are giving the player the rewards in this call
            
            yield return new WaitForSeconds(0.1f);
        }
        
        yield return new WaitForEndOfFrame();

        btnContinue.DOAnchorPosY(50f, 0.5f);
    }
    
    public void Btn_Continue() {
        MenuManager.Instance.Pop();
        if (callback != null) callback();
    }
}
