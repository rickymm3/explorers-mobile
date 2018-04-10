using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using RSG;

public abstract class Panel : Tracer {
    private string _name = "";
    public string Name {
        get { return _name; }
        set { _name = value; }
    }

    public Action<Panel> onRemove;
    
    protected bool _isClosing = false;
    private EventSystem _eventSystem;
    private EasyDictionary<RectTransform, Vector2> _initPositions = new EasyDictionary<RectTransform, Vector2>();


    public void Initialize() {
        _eventSystem = EventSystem.current;
    }

    public virtual bool Close() {
        if (_isClosing) return false;
        _isClosing = true;

        CloseTransition();

        return true;
    }

    public void RemoveSelf() {
        MenuManager.Instance.Remove(this);
    }

    public Promise DoClosingTransition(Transform panel, Graphic fader=null, float timeFade=0.5f, float timeScale=0.3f, Action OnComplete=null) {
        if(fader!=null) fader.DOFade(0, timeFade);

        var closingPromise = new Promise();
        
        panel.transform.DOScale(0, timeScale)
            .SetEase(Ease.InBack)
            .OnComplete(() => {
                MenuManager.Instance.UIOnCampScreenPoped = false;

                RemoveSelf();
                if(OnComplete!=null) OnComplete();
                closingPromise.Resolve();
            });

        return closingPromise;
    }

    //Leave blank - Implement in subclasses...
    public virtual void CloseTransition() {}

    public Vector2 GetScreenSize(float ratioX=1, float ratioY=1) {
        return new Vector2(Screen.width * ratioX, Screen.height * ratioY);
    }

    public static GameObject MakeFromPrefab(string prefabName) {
        GameObject prefab = GetPrefab(prefabName);
        return MakeFromPrefab(prefab);
    }

    public static GameObject MakeFromPrefab(string prefabName, GameObject parent) {
        return MakeFromPrefab(prefabName, parent.gameObject);
    }

    public static GameObject MakeFromPrefab(string prefabName, Component parent) {
        GameObject prefab = GetPrefab(prefabName);
        return MakeFromPrefab(prefab, parent);
    }

    public static GameObject MakeFromPrefab(GameObject prefab, Component parent=null) {
        GameObject inst = Instantiate(prefab);
        if (parent!=null) {
            inst.transform.SetParent(parent.transform);
        }
        return inst;
    }

    public static GameObject GetPrefab(string prefabName) {
        return Resources.Load<GameObject>("Interfaces/" + prefabName);
    }

    public void CheckTabKey() {
        if(!Input.GetKeyDown(KeyCode.Tab) || _eventSystem==null) return;

        bool isShiftDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        Selectable current = _eventSystem.currentSelectedGameObject.GetComponent<Selectable>();
        Selectable other = isShiftDown ? current.FindSelectableOnUp() : current.FindSelectableOnDown();
        
        if (other==null) return;

        InputField inputfield = other.GetComponent<InputField>();

        //if it's an input field, also set the text caret
        if (inputfield != null) inputfield.OnPointerClick(new PointerEventData(_eventSystem));

        FocusOnInput(other);
    }

    public void FocusOnInput(Selectable input) {
        _eventSystem.SetSelectedGameObject(input.gameObject, new BaseEventData(_eventSystem));
    }

    public void SetInitPositions(params RectTransform[] transforms) {
        foreach(RectTransform trans in transforms) {
            _initPositions.AddOrSet(trans, trans.localPosition);
        }
    }

    public Vector2 GetInitPosition(RectTransform trans) {
        return _initPositions[trans];
    }
}
