using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class CampExploreActDisplayInterface : MonoBehaviour {
    public TextMeshProUGUI Title;
    public Image ActImage;
    public GameObject locked;
    RectTransform rectTransform;

    void Start() {
        rectTransform = GetComponent<RectTransform>();

        StartCoroutine(FloatingMovement());
    }

    public void Initialize(ActData data, bool available) {
        Title.text = "Act " + data.ActNumber;
        ActImage.sprite = data.LoadSprite();

        locked.SetActive(available);
    }
    
    IEnumerator FloatingMovement() {
        while (true) {
            yield return rectTransform.DOAnchorPosY(-320f, 2f).WaitForCompletion();
            yield return rectTransform.DOAnchorPosY(-315f, 2f).WaitForCompletion();
        }
    }
}
