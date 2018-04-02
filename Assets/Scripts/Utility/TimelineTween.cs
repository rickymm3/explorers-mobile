using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using UnityEngine;
using RSG;

using NextAction = System.Action<System.Action>;

public class TimelineTween {
    private List<NextAction> _tweens;

    private int _currentTweenID = 0;
    
    public TimelineTween() {
        _tweens = new List<NextAction>();
    }

    public static TimelineTween Create( params NextAction[] tweens ) {
        var timeline = new TimelineTween();
        timeline.Add(tweens);
        timeline.Play();
        return timeline;
    }

    public TimelineTween Add( params NextAction[] tweens ) {
        _tweens.AddRange(tweens);

        return this;
    }

    private void __NextStep() {
        if(_currentTweenID>=_tweens.Count) {
            return;
        }

        NextAction twn = _tweens[_currentTweenID++];
        twn(__NextStep);
    }
    
    public void Play() {
        _currentTweenID = 0;

        __NextStep();
    }

    public static void Scatter<T>(float scatterDelay, List<T> items, Func<T, int, Tweener> cbIterator) {
         for(int i=0; i<items.Count; i++) {
            T item = items[i];
            Tweener twn = cbIterator(item, i);
            twn.SetDelay(i * scatterDelay);
        }
    }

    public static Sequence WaitEach(float delayPre, float delayEach, params TweenCallback[] actions) {
        Sequence twn = DOTween.Sequence();
        twn.AppendInterval(delayPre);

        foreach(TweenCallback action in actions) {
            twn.AppendCallback(action);
            twn.AppendInterval(delayEach);
        }

        return twn;
    }

    public static Tweener Pulsate(Transform transform, float scaleTo, float totalDuration) {
        Vector2 scaleBefore = transform.localScale;

        return transform.DOScale(scaleBefore * scaleTo, totalDuration * .5f)
            .SetEase(Ease.OutSine)
            .OnComplete(() => {
                transform.DOScale(scaleBefore, totalDuration * .5f).SetEase(Ease.InSine);
            });
    }

    public static Tweener ShakeError(GameObject target, float duration=0.3f, float strength=15f, int vibrato=20, int randomness=90, TweenCallback onComplete = null) {
        return target.transform.DOShakePosition(0.3f, 15f, 20, 90, false, true)
            .OnComplete(onComplete);
    }
}
