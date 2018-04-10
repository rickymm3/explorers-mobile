using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TurnOrderHandler {
    const float offset = -25f;
    const float space = 110f;

    public int index;
    public GameObject goRef;
    public RectTransform rt;
    public Sprite sprite;
    public TurnOrderHeroController controller;
    public BattleActor actor;

    public void Initialize(int index, Transform parent, GameObject prefab, BattleActor actor) {
        this.index = index;
        goRef = MonoBehaviour.Instantiate(prefab);
        goRef.transform.SetParent(parent);
        goRef.transform.localScale = Vector3.one;
        goRef.transform.localPosition = Vector3.zero;

        sprite = actor.LoadSprite();

        controller = goRef.GetComponent<TurnOrderHeroController>();
        //Debug.Log("Actor null: " + (actor is BossActor));
        controller.Initialize(sprite, (actor is BossActor));

        rt = goRef.GetComponent<RectTransform>();
    }

    public void HandleIndexChange() {
        if (actor != null)
            if (actor.QueuedSkill != null)
                controller.Casting();
            else
                controller.NotCasting();

        rt.DOAnchorPosX(index * space, 0.5f);
    }

    public void Select() {
        rt.DOAnchorPosY(offset, 0.25f);
    }
    public void Deselect() {
        rt.DOAnchorPosY(0f, 0.25f);
    }

    public IEnumerator ActorDied(System.Action callback) {
        // do cleanup anim stuff here
        controller.Portrait.DOColor(new Color(0.5f, 0.2f, 0.2f), 0.2f);
        rt.DOAnchorPosY(offset * 2f, 0.75f).SetEase(Ease.OutQuad);
        yield return new WaitForSeconds(0.35f);

        rt.gameObject.GetComponent<CanvasGroup>().DOFade(0f, 0.4f).SetEase(Ease.OutQuad);
        yield return new WaitForSeconds(0.4f);

        rt = null;
        sprite = null;
        goRef.SetActive(false);
        callback();
    }
}
