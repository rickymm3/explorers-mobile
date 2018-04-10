using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using ExtensionMethods;
using SimpleJSON;
using NodeJS;
using TMPro;

public class UserLoginInterface : PanelWithGetters {
    private static string TITLE_LOGIN = "USER LOGIN";
    private static string TITLE_SIGNUP = "USER SIGNUP";

    public RectTransform Frame;
    public Image Shadow;
    public TextMeshProUGUI Title;
    public TextMeshProUGUI errorsLabel;
    public Image Spinner;

    [Header("Animation Timing")]
    public float timeShowHide = 0.5f;
    public float timeShowHidePanels = 0.25f;

    [Header("Buttons")]
    public Button btnLogin;
    public Button btnCancel;
    public Button btnSignup;
    public Button btnGotoSignup;
    public Button btnGotoLogin;
    public Button[] allButtons;
    
    [Header("Login Details")]
    public CanvasGroup loginPanel;
    public TMP_InputField loginUsername;
    public TMP_InputField loginPassword;
    public Button[] loginButtons;

    [Header("Signup Details")]
    public CanvasGroup signupPanel;
    public TMP_InputField signupUsername;
    public TMP_InputField signupEmail;
    public TMP_InputField signupPassword;
    public TMP_InputField signupFullname;
    public Button[] signupButtons;

    bool _isBusy = false;
    CanvasGroup _lastPanel;

	// Use this for initialization
	void Start () {
        Name = "UserLoginSignup";
        Title.text = "";
        errorsLabel.text = "";

        var lastUsername = API.usernameLast;
        loginUsername.text = string.IsNullOrEmpty(lastUsername) ? "USERNAME" : lastUsername;

        Frame.transform.localScale = Vector2.zero;
        Shadow.color = new Color(0,0,0,0);
        Spinner.color = new Color(1, 1, 1, 0);

        btnLogin.onClick.AddListener(Btn_Login);
        btnCancel.onClick.AddListener(Btn_Cancel);
        btnSignup.onClick.AddListener(Btn_Signup);
        btnGotoSignup.onClick.AddListener(Btn_GotoSignup);
        btnGotoLogin.onClick.AddListener(Btn_GotoLogin);

        SetButtonActive(false, allButtons);

        HidePanel(signupPanel, immediate: true);
        HidePanel(loginPanel, immediate: true);
        TogglePanel(loginPanel, loginButtons, TITLE_LOGIN, immediate: true);

        loginUsername.textComponent.text = GameAPIManager.API != null ? GameAPIManager.API.username : "";
        loginPassword.text = "";

        Show();
    }

    void Update() {
        CheckTabKey();
    }

    //////////////////////////////////////////////////////////////////////////////////

    public void Btn_Cancel() {
        SetButtonInteraction(false);

        Hide();
    }

    public void Btn_Login() {
        StartCoroutine(__TryLogin());
    }

    public void Btn_Signup() {
        StartCoroutine(__TrySignup());
    }

    public void Btn_GotoLogin() {
        TogglePanel(loginPanel, loginButtons, TITLE_LOGIN);
    }

    public void Btn_GotoSignup() {
        TogglePanel(signupPanel, signupButtons, TITLE_SIGNUP);
    }

    //////////////////////////////////////////////////////////////////////////////////

    public void Show() {
        Shadow.DOFade(0.5f, timeShowHide * 1.2f);
        Frame.transform.DOScale(1, timeShowHide).SetEase(Ease.OutBack);
    }

    public void Hide(Action OnHideComplete = null) {
        DoClosingTransition(Frame.transform, Shadow, timeScale: timeShowHide * 0.8f, OnComplete: OnHideComplete);
    }

    public void ShowPanel(CanvasGroup group, Action OnComplete=null, bool immediate = false) {
        _lastPanel = group;
        
        group.gameObject.SetActive(true);
        group.DOKill();
        group.DOFade(1, immediate ? 0 : timeShowHidePanels)
            .OnComplete(() => {
                group.interactable = true;
                if (OnComplete != null) OnComplete();
            });
    }

    public void HidePanel(CanvasGroup group=null, Action OnComplete = null, bool immediate = false) {
        if (group == null) {
            if (_lastPanel == null) return;
            group = _lastPanel;
        }
        
        errorsLabel.text = "";
        
        group.interactable = false;
        group.DOKill();
        group.DOFade(0, immediate ? 0 : timeShowHidePanels)
            .OnComplete(() => {
                group.gameObject.SetActive(false);
                if (OnComplete != null) OnComplete();
            });
    }

    private void TogglePanel(CanvasGroup group, Button[] buttons, string title, bool immediate = false) {
        SetButtonInteraction(false);

        var inputToFocus = group == loginPanel ? loginUsername : signupUsername;

        Action OnComplete = () => {
            SetButtonActive(false, allButtons);
            SetButtonActive(true, buttons);

            Title.text = title;
            ShowPanel(group, () => {
                FocusOnInput(inputToFocus);
            });

            SetButtonInteraction(true);
        };

        if(immediate) {
            HidePanel(immediate:true);
            OnComplete();
            return;
        }

        HidePanel(OnComplete: OnComplete);
    }

    public void SetButtonActive(bool enabled, params Button[] buttons) {
        foreach (Button btn in buttons) btn.gameObject.SetActive(enabled);
    }

    public void SetButtonInteraction(bool enabled) {
        foreach (Button btn in allButtons) btn.interactable = enabled;
    }

    public void ShowError(string text) {
        traceError(text);
        errorsLabel.text = text;
        errorsLabel.transform.DOShakePosition(0.5f, 5, 30, 20f);
    }

    //////////////////////////////////////////////////////////////////////////////////

    private IEnumerator __TryLogin() {
        SetBusy(true);

        API.Users.Login(loginUsername.text, loginPassword.text)
            .Then(res => {
                trace("Login Success!!!!!!");
                this.Wait(1.0f, () => {
                    SetBusy(false);
                    Hide();
                });
            })
            .Catch(err => {
                ShowError("Login Failed!\n" + GameAPIManager.GetErrorMessage(err));
                ShowPanel(loginPanel);
                SetBusy(false);
            });

        while (_isBusy) yield return new WaitForSeconds(1.0f);

        yield break;
    }

    private IEnumerator __TrySignup() {
        SetBusy(true);

        API.Users.Signup(signupFullname.text, signupUsername.text, signupEmail.text, signupPassword.text)
            .Then(res => {
                trace("Signup Success!!!!!!");
                this.Wait(1.0f, () => {
                    SetBusy(false);
                    Hide();
                });
            })
            .Catch(err => {
                NodeError nodeErr = (NodeError) err;
                JSONNode data = nodeErr.json["data"];

                if (data.Exists() && data["duplicates"].Exists()) {
                    JSONArray fields = data["duplicates"]["fields"].AsArray;
                    
                    ShowError("Found duplicate user on field(s):\n" + fields.Join(", "));
                } else {
                    ShowError(nodeErr.json["error"]);
                    traceError(nodeErr.response.text);
                    traceError(nodeErr.Message);
                }
                
                ShowPanel(signupPanel);
                SetBusy(false);
            });

        while (_isBusy) yield return new WaitForSeconds(1.0f);

        yield break;
    }

    void SetBusy(bool busy) {
        _isBusy = busy;
        SetButtonInteraction(!busy);

        Spinner.DOKill();

        if (busy) {
            HidePanel(loginPanel);
            HidePanel(signupPanel);

            Spinner.gameObject.SetActive(true);
            Spinner.DOFade(1, 0.3f);
        } else {
            Spinner.DOFade(0, 0.3f)
                .OnComplete(() => {
                    Spinner.gameObject.SetActive(false);
                });
        }
    }
}
