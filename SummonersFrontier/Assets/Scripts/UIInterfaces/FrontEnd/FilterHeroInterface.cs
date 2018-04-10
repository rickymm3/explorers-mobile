using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExtensionMethods;
using DG.Tweening;

public class FilterHeroInterface : Panel {
    public Image fader;
    public RectTransform RevealPanel;

    System.Action<HeroListFilterType> callback;

    public void Initialize(System.Action<HeroListFilterType> callback) {
        this.callback = callback;

        // Fade the Shadow in
        fader.color = new Color(0f, 0f, 0f, 0f);
        fader.DOColor(new Color(0f, 0f, 0f, 0.7f), 0.3f);

        // Transition the panel in
        RevealPanel.localScale = Vector3.zero;
        RevealPanel.DOScale(1f, 0.4f);
    }

    public void Btn_Filter(string fType) {
        // The fType is defined on the button itself
        callback(fType.Trim().AsEnum<HeroListFilterType>());
        ClosePanel();
    }

    public void Btn_Close() {
        ClosePanel();
    }

    void ClosePanel() {
        fader.DOColor(new Color(0f, 0f, 0f, 0f), 0.4f);
        RevealPanel.DOScale(0f, 0.3f);

        StartCoroutine(DelayClose(0.4f));
    }

    IEnumerator DelayClose(float delay) {
        yield return new WaitForSeconds(delay);
        MenuManager.Instance.Pop();
    }
}
