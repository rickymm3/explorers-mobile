using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InteractionType { SceneLoad, MenuPanelChange, MenuPanelPush }

public class CampInteractable : MonoBehaviour {

    public LoadCamp campRef;
    public InteractionType interaction = InteractionType.MenuPanelChange;
    public int panelIndex = 0;
    public string scene = "";

    public void Interact() {
        //print("Interacting with " + gameObject.name);
        if (campRef.campInterface.panelIndex != 2 || GameManager.Instance.InStory || GameManager.Instance.InResearch) return;
        // Call the menu movement stuff
        switch (interaction) {
            // load screen
            case InteractionType.SceneLoad:
                UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
                break;
            case InteractionType.MenuPanelChange:
                campRef.campInterface.ChangePanelIndex(panelIndex);
                break;
            case InteractionType.MenuPanelPush:
                MenuManager.Instance.Push(scene);
                break;
        }
    }
}
