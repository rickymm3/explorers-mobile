using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActSelectInterface : Panel {

    public CanvasGroup HeaderText;

    List<ActData> acts = new List<ActData>();
    public RectTransform actContainer;
    public GameObject actPrefab;
    public ActNavigation actNav;
    public Image fadeOut;

    public int ActIndex = 0;
    
    void Start () {
        // Get Act List
        acts = DataManager.Instance.actDataList;

        int counter = 1;
        // Spawn each act in
        foreach(ActData act in acts) {
            GameObject go = (GameObject) Instantiate(actPrefab, actContainer);
            go.GetComponent<ActPortalInterfaceObject>().Init(act, this, (counter == 1), (counter == acts.Count));
            counter++;
        }

        actNav.panels = acts.Count;

    }
	
    void Update() {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            actNav.Previous();

        if (Input.GetKeyDown(KeyCode.RightArrow))
            actNav.Next();
    }
}
