using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

/**
 * Hopefully the Omega Gamma Ultra Mega scrollrect *SNAP*-ing script
 * that we won't have to re-write ever, ever again.
 * 
 * - Pierre
 */
public class ScrollSnap_v2 : Tracer {

    public RectTransform container;
    public RectTransform centerReference;
    public Button btnNavNext;
    public Button btnNavPrev;

    [MinMaxRange(0.01f, 10f)] public float snapSpeed = 1.0f;

    private EventTrigger _eventTrigger;
    private SnapElement[] _snapElements;
    private SnapElement _snapCurrent;
    private ScrollRect _scrollRect;
    private bool _isDragging = false;
    private bool _isJustReleased = false;
    private bool _isRecalcOK = false;
    private int _numElements;
    private float _distBetweenEach;
    private float _posLast = 0;
    private float _posNow = 0;
    private float _posDiff = 0;

    public float CurrentX { get { return container.anchoredPosition.x; } }
    public float CurrentY { get { return container.anchoredPosition.y; } }

    public int CurrentID {
        get {
            return _snapCurrent==null ? 0 : _snapCurrent.id;
        }
        set {
            if(_snapElements.Length==0) {
                traceError("ScrollSnap_v2 cannot set CurrentID to 0");
                return;
            }
            value %= _snapElements.Length;
            while(value<0) value += _snapElements.Length;

            _snapCurrent = _snapElements[value];
        }
    }

    void Start () {
        if (container == null) {
            traceError("Need a 'container' to correctly run snapping!");
            return;
        }

        if (centerReference == null) {
            traceError("Need a 'centerReference' to correctly run snapping!");
            return;
        }

        _scrollRect = GetComponent<ScrollRect>();
        _scrollRect.inertia = false;

        SetupEventTrigger();

        //Delay the 1st recalculation by 1 frame to let the Horizontal Layout do its thing:
        StartCoroutine(__Start());
    }

    private IEnumerator __Start() {
        yield return new WaitForEndOfFrame();
        RecalculateLayout();
    }

    private void SetupEventTrigger() {
        _eventTrigger = gameObject.AddComponent<EventTrigger>();
        AddEventTrigger(EventTriggerType.Drag, (nope) => _isDragging = true);
        AddEventTrigger(EventTriggerType.EndDrag, (nope) => _isDragging = false);
        
        if(btnNavNext!=null) btnNavNext.onClick.AddListener(BtnClickNext);
        if(btnNavPrev!=null) btnNavPrev.onClick.AddListener(BtnClickPrev);
    }

    private void BtnClickNext() {
        _isJustReleased = true;
        CurrentID++;
    }

    private void BtnClickPrev() {
        _isJustReleased = true;
        CurrentID--;
    }

    private void AddEventTrigger(EventTriggerType eventID, UnityAction<BaseEventData> cb) {
        var entry = new EventTrigger.Entry();
        entry.eventID = eventID;
        entry.callback.AddListener(cb);
        _eventTrigger.triggers.Add(entry);
    }

    ///////////////////////////////////////////////

    public void RecalculateLayout() {
        _isRecalcOK = false;
        _numElements = container.childCount;
        _snapElements = new SnapElement[_numElements];
        
        if (_numElements < 2) {
            //traceError("Cannot calculate snapping on less than 2 objects.");
            Reset();
            return;
        }

        StartCoroutine(__RecalcLayout());
    }

    private IEnumerator __RecalcLayout() {
        yield return new WaitForEndOfFrame();

        trace("ScrollSnap_v2 - RecalculateLayout: " + container.childCount);

        _scrollRect.enabled = true;

        for (int c = 0; c < _numElements; c++) {
            var snap = _snapElements[c] = new SnapElement();
            snap.rect = (RectTransform) container.GetChild(c);
            snap.id = c;
        }

        var snap0 = _snapElements[0].rect.anchoredPosition;
        var snap1 = _snapElements[1].rect.anchoredPosition;

        _distBetweenEach = Mathf.Abs(snap0.x - snap1.x);

        if (_distBetweenEach <= 0.001) {
            traceError("Distance is too small!");
            yield break;
        }

        _posNow = _posLast = CurrentX;

        _isRecalcOK = true;
    }

    private void Reset() {
        _scrollRect.enabled = false;
        container.anchoredPosition = centerReference.anchoredPosition;
    }

    ///////////////////////////////////////////////

    void Update () {
        if(centerReference==null || container==null || !_isRecalcOK) return;
        if(_numElements!=container.childCount) RecalculateLayout();

        if (!_isDragging && _snapCurrent != null) {
            SnapToCurrentElement();
            return;
        }

        if(!_isJustReleased) {
            container.DOKill();
        }

        _isJustReleased = true; //Arm "release" for the 'EndDrag' event.

        _posLast = _posNow;
        _posNow = CurrentX;
        _posDiff = _posNow - _posLast;

        FindClosestSnapElement();
    }

    private void FindClosestSnapElement() {
        float centerX = centerReference.position.x - _posDiff * 2;

        SnapElement minSnap = null;
        for(int s=0; s<_numElements; s++) {
            var snap = _snapElements[s];
            snap.distance = Math.Abs(centerX - snap.rect.position.x);
            if (minSnap == null || snap.distance < minSnap.distance) {
                minSnap = snap;
            }
        }

        _snapCurrent = minSnap;
    }

    private void SnapToCurrentElement() {
        if (!_isJustReleased) return;

        float posDiff = _posNow - _posLast;

        container
                .DOAnchorPosX(_snapCurrent.id * -_distBetweenEach, snapSpeed)
                .SetEase(Ease.OutSine);

        _isJustReleased = false;
    }

    [Serializable]
    internal class SnapElement {
        public RectTransform rect;
        public float distance = 0;
        public int id;
    }
}
