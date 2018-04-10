using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ChoiceButton : MonoBehaviour {

	public Button btn;
    public TextMeshProUGUI txtValue;
    public List<string> choices;
    public SFX_UI sfx;

    int _selectedIndex = 0;
    public int selectedIndex {
        get { return _selectedIndex; }
        set {
            while(value<0) value += choices.Count;
            while(value>=choices.Count) value -= choices.Count;
            _selectedIndex = value;
            txtValue.text = choices[_selectedIndex];
        }
    }

    public string selected {
        get { return choices[_selectedIndex]; }
    }
    
    void Start () {
		btn.onClick.AddListener(Btn_OnClick);
	}

    void Btn_OnClick() {
        selectedIndex++;
        AudioManager.Instance.Play(sfx);
    }
}
