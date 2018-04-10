using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExtensionMethods;
using TMPro;
using RSG;
using DG.Tweening;
using System;

using PromiseStr = RSG.IPromise<string>;

public class ConfirmFilter: PanelWithGetters {
    public static ConfirmFilter Instance;

    public TextMeshProUGUI labelTitle;
    public TextMeshProUGUI labelConfirm;
    public RectTransform panel;
    public RectTransform buttonsContainer;
    public Image fader;
    public Sprite backgroundReject;
    public Sprite backgroundResolve;
    public RectTransform CustomArea;
    public Button BtnModal;
    public Button BtnClose;
    public Button BtnTemplate;

    float _timeShow = 0.50f;
    float _timeHide = 0.25f;
    float _faderAlpha = 0;
    Promise<string> _promise;
    List<PromiseButton> _buttons;

    // Use this for initialization
    void Start () {
        Instance = this;

        _faderAlpha = fader.color.a;
        fader.color = new Color(0,0,0,0);

        panel.localScale = Vector2.zero;

        BtnModal.onClick.AddListener(Btn_OnModalClickAnywhere);
        BtnClose.onClick.AddListener(Btn_OnModalClickAnywhere);

        Show();
    }

    private void Btn_OnModalClickAnywhere() {
        if(!Close()) return;
    }

    public Tweener Show(float time = -1) {
        if (time < 0) time = _timeShow;

        fader.DOFade(_faderAlpha, time);
        return panel.DOScale(1, time).SetEase(Ease.OutBack);
    }

    public override void CloseTransition() {
        fader.DOFade(0, _timeHide);
        panel.DOScale(0, _timeHide)
            .SetEase(Ease.InBack)
            .OnComplete(() => {
                menuMan.Remove(this);
                Instance = null;
            });

        foreach(PromiseButton btnPromise in _buttons) {
            btnPromise.btn.onClick.RemoveAllListeners();
        }
    }

    public PromiseButton GetButtonByLabel(string label) {
        label = label.ToUpper();
        return _buttons.Find(btnPromise => btnPromise.label.text.ToUpper() == label);
    }

    ///////////////////////////////////////////////////////////

    void SetButtons(List<PromiseButton> buttons) {
        _buttons = buttons;
        foreach(PromiseButton btnPromise in _buttons) {
            btnPromise.btn.onClick.AddListener(() => Btn_OnClick(btnPromise));
        }
    }

    void Btn_OnClick(PromiseButton btnPromise) {
        if(!Close()) return;

        string answerCaps = btnPromise.label.text.ToUpper();

        if (btnPromise.isReject) {
            _promise.Reject(new Exception(answerCaps));
            return;
        }

        _promise.Resolve(answerCaps);
    }

    private List<PromiseButton> CreateButtonsFromLabels(string[] labels) {
        var buttons = new List<PromiseButton>();

        foreach (string label in labels) {
            Button btnGO = this.Clone<Button>(BtnTemplate.gameObject);
            PromiseButton btnPromise = btnGO.gameObject.AddComponent<PromiseButton>();
            btnPromise.btn = btnGO.GetComponent<Button>();
            btnPromise.background = btnGO.GetComponent<Image>();
            btnPromise.text = label;
            btnPromise.label = btnGO.GetComponentInChildren<TextMeshProUGUI>();
            btnPromise.label.text = label.TrimStart('*');
            btnPromise.isReject = label.StartsWith("*");
            btnGO.transform.localScale = Vector2.one;

            if (btnPromise.isReject) {
                btnPromise.background.sprite = this.backgroundReject;
            } else {
                btnPromise.background.sprite = this.backgroundResolve;
            }

            buttons.Add(btnPromise);
        }

        BtnTemplate.gameObject.SetActive(false);

        SetButtons(buttons);

        return buttons;
    }

    public IPromise<string> Then(Action<string> cb) {
        return _promise.Then(cb);
    }

    public IPromise<T> Then<T>(Func<string, T> cb) {
        return _promise.Then<T>(cb);
    }

    public IPromise<T> Then<T>(Func<string, IPromise<T>> cb) {
        return _promise.Then<T>(cb);
    }

    ///////////////////////////////////////////////////////////

    public static ConfirmFilter Ask(string title, string confirmMsg, params string[] labels) {
        ConfirmFilter confirm = (ConfirmFilter)menuMan.Load("Interface_ConfirmFilter");

        confirm._promise = new Promise<string>();
        confirm.labelTitle.text = title;
        confirm.CreateButtonsFromLabels(labels);

        return confirm;
    }

    ///////////////////////////////////////////////////////////

    public class PromiseButton : MonoBehaviour {
        public bool isReject = false;
        public string text;
        public Button btn;
        public Image background;
        public TextMeshProUGUI label;
    }
}
