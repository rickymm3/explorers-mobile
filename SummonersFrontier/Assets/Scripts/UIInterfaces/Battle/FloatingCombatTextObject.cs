using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class FloatingCombatTextObject : MonoBehaviour {
    // Handle the floating shrinking and fading of the text
    RectTransform trans;
    TextMeshProUGUI textObj;

    Color color;

    float progress = 0f;
    float speed = 0f;

    public void Initialize(string message, Vector2 position, float textSize, CombatHitType type, Color? textColor = null) {
        progress = 0f;
        trans = GetComponent<RectTransform>();
        textObj = GetComponent<TextMeshProUGUI>();

        //trans.anchoredPosition = position;
        trans.position = position;
        textObj.text = message;
        textObj.fontSize = textSize;
        color = textColor ?? Color.white;
        
        switch (type) {
            case CombatHitType.Normal:
                if (textColor == null) color = Color.white;
                break;
            case CombatHitType.Critical:
                if (textColor == null) color = Color.yellow;
                textObj.fontSize = textSize * 1.2f;
                break;
            case CombatHitType.Glancing:
                if (textColor == null) color = Color.red;
                break;
        }
        textObj.color = color;

        StartCoroutine(HandleFloatingAction());
    }

    float distance;
    float xoffset;
    Vector2 origin;
    Vector2 movement = Vector2.zero;
    bool alphaTrigger;
    IEnumerator HandleFloatingAction() {
        distance = Random.Range(800f, 900f);
        xoffset = Random.Range(-30f, 30f);
        origin = trans.anchoredPosition;
        movement = Vector2.zero;
        alphaTrigger = false;

        while (progress <= 1f) {
            movement.y = ((1f - progress) * progress) * distance;
            movement.x = progress * xoffset;

            progress += Time.deltaTime * 0.5f;

            if (progress > 0.4f && !alphaTrigger) {
                color.a = 0f;
                textObj.DOColor(color, 0.3f);
                alphaTrigger = true;
            }

            trans.anchoredPosition = origin + movement;
            yield return new WaitForEndOfFrame();
        }

        gameObject.SetActive(false);
    }
}
