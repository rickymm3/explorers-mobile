using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using ExtensionMethods;

public class ErrorLogs : PanelWithGetters {
    public static ErrorLogs Instance;

    public Button btnClose;
    public TextMeshProUGUI txtError;

    void Start() {
        Instance = this;

        btnClose.onClick.AddListener(Btn_Close);
        Hide(true);
     
        Tracer.OnTraceError += (message) => {
            audioMan.Play(SFX_UI.Invalid);
            Show(message == null ? "*null*" : message.ToString());
        };
    }

    public void Btn_Close() {
        Hide();
    }

    public void Show(string msg) {
        if(txtError==null) return;
        txtError.text = msg;
        Show();
    }

    public void Show(bool isImmediate = false) {
        transform.DOScale(1, isImmediate ? 0 : 0.4f).SetEase(Ease.OutBack);
    }

    public void Hide(bool isImmediate=false) {
        transform.DOScale(0, isImmediate ? 0 : 0.3f).SetEase(Ease.InBack);
    }
}
