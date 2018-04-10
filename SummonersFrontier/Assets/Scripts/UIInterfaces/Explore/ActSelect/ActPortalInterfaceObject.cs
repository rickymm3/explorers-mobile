using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class ActPortalInterfaceObject : MonoBehaviour {
    
    ActData data;
    ActSelectInterface actInterface;

    public TextMeshProUGUI title;
    public Image display;

    public RectTransform Portal;

    public GameObject LeftButton;
    public GameObject RightButton;

    public void Init(ActData data, ActSelectInterface actInterface, bool hideLeft = false, bool hideRight = false) {
        this.data = data;
        this.actInterface = actInterface;

        title.text = "Act " + data.ActNumber + "\n" + data.Name;
        display.sprite = data.LoadSprite();

        if (hideLeft) LeftButton.SetActive(false);
        if (hideRight) RightButton.SetActive(false);
    }
	
    public void LoadAct() {
        StartCoroutine(LoadActSequence());
    }

    IEnumerator LoadActSequence() {
        actInterface.HeaderText.DOFade(0, 0.5f);
        Portal.DOScale(10f, 1.5f);
        actInterface.fadeOut.DOColor(new Color(0f, 0f, 0f, 1f), 1.4f);

        yield return new WaitForSeconds(1.5f);

        ActInformationInterface p = (ActInformationInterface) MenuManager.Instance.Push("Interface_ActInformation");
        //p.Init(data);

        Portal.DOScale(1f, 0f);
        actInterface.fadeOut.DOColor(new Color(0f, 0f, 0f, 0f), 0f);
        actInterface.HeaderText.DOFade(1, 0.5f);
    }

    public void Btn_Next() {
        actInterface.actNav.Next();
    }
    public void Btn_Previous() {
        actInterface.actNav.Previous();
    }
}
