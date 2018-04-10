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

public class ConfirmYesNoInterface : PanelWithGetters {
    static string[] _DEFAULT_LABELS = { "yes", "no" };
    public static ConfirmYesNoInterface Instance;

    public TextMeshProUGUI labelTitle;
    public TextMeshProUGUI labelConfirm;
    public RectTransform panel;
    public RectTransform buttonsContainer;
    public Image fader;
    public Sprite backgroundReject;
    public Sprite backgroundResolve;
    public RectTransform CustomArea;
    public TMP_InputField _input;
    public Button BtnClose;
    public Button BtnBackground;

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
        Show();

        BtnClose.onClick.AddListener(() => Close());
        BtnBackground.onClick.AddListener(() => Close());
    }

    void OnDestroy() {
        _promise = null;
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
                if(_input!=null) {
                    _input.onValueChanged.RemoveAllListeners();
                }
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

    public IPromise<string> Then(Action<string> cb) {
        return _promise.Then(cb);
    }

    public IPromise<T> Then<T>(Func<string, T> cb) {
        return _promise.Then<T>(cb);
    }

    public IPromise<T> Then<T>(Func<string, IPromise<T>> cb) {
        return _promise.Then<T>(cb);
    }


    private List<PromiseButton> CreateButtonsFromLabels(string[] labels) {
        if (labels == null || labels.Length == 0) labels = _DEFAULT_LABELS;

        var buttons = new List<PromiseButton>();

        foreach (string label in labels) {
            GameObject btnGO = MakeFromPrefab("SubItems/GeneralButton", this.buttonsContainer);
            PromiseButton btnPromise = btnGO.AddComponent<PromiseButton>();
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

        SetButtons(buttons);

        return buttons;
    }

    public void SetLabelText(string labelName, string text) {
        var child = transform.FindDeepChild(labelName);
        if (child == null) {
            traceError("GetInputText() child not found: " + labelName);
            return;
        }

        var label = child.GetComponent<TextMeshProUGUI>();
        if (label == null) {
            traceError("GetInputText() label not found: " + labelName);
            return;
        }

        label.text = text;
    }

    public TMP_InputField GetInputText(string labelName="customInput") {
        var child = transform.FindDeepChild(labelName);
        if (child == null) {
            traceError("GetInputText() child not found: " + labelName);
            return null;
        }

        var input = child.GetComponent<TMP_InputField>();
        if (input==null) {
            traceError("GetInputText() input not found: " + labelName);
            return null;
        }

        return input;
    }

    ///////////////////////////////////////////////////////////

    public static ConfirmYesNoInterface Ask(string title, string confirmMsg, params string[] labels) {
        ConfirmYesNoInterface confirm = (ConfirmYesNoInterface)menuMan.Load("Interface_ConfirmYesNo");

        confirm._promise = new Promise<string>();
        confirm.labelTitle.text = title;
        confirm.labelConfirm.text = confirmMsg;
        confirm.CreateButtonsFromLabels(labels);

        return confirm;
    }

    public static ConfirmYesNoInterface AskCustom(string title, string interfaceName, params string[] labels) {
        ConfirmYesNoInterface confirm = (ConfirmYesNoInterface)menuMan.Load("Interface_ConfirmYesNo");

        confirm._promise = new Promise<string>();
        confirm.labelTitle.text = title;
        confirm.labelConfirm.gameObject.SetActive(false);
        confirm.CreateButtonsFromLabels(labels);

        GameObject prefab = Resources.Load<GameObject>("Interfaces/" + interfaceName);
        GameObject custom = Instantiate<GameObject>(prefab);

        custom.transform.SetParent(confirm.CustomArea);
        custom.transform.localScale = Vector2.one;
        custom.transform.localPosition = Vector2.zero;

        TMP_InputField input = custom.GetComponentInChildren<TMP_InputField>();
        if(input!=null) {
            input.Select();
            input.ActivateInputField();
        }

        return confirm;
    }

    ///////////////////////////////////////////////////////////

    public class PromiseButton : MonoBehaviour {
        public bool isReject = false;
        public Button btn;
        public Image background;
        public string text;
        public TextMeshProUGUI label;
    }

    internal void SetCharLimit(int v) {
        _input = this.GetInputText();
        _input.characterLimit = 24;
        _input.onValueChanged.AddListener(OnInputChanged);
    }

    private void OnInputChanged(string changed) {
        _input.textComponent.color = changed.Length >= _input.characterLimit ? Color.red : Color.white;
    }
}
