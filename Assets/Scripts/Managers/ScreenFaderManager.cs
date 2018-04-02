using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum FadeStyle { In, Out }
public class ScreenFaderManager : Singleton<ScreenFaderManager> {

    public Image Fader;

    public void Fade(float delay, FadeStyle style = FadeStyle.In) {
        if (style == FadeStyle.In)
            Fader.DOColor(new Color(0f, 0f, 0f, 1f), delay);
        else
            Fader.DOColor(new Color(0f, 0f, 0f, 0f), delay);
    }
}
