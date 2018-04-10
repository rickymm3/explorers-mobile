using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuManager : Singleton<MenuManager> {
    public Canvas ParentCanvas;
    public RectTransform UIRoot;
    public EventSystem eventSystem;
    public bool UIOnCampScreenPoped = false;

    List<Panel> UIStack = new List<Panel>();
    List<Panel> UINonStackRef = new List<Panel>();
    GameObject panelReference = null;

    /// <summary>
    /// Load a UI Interface ignoring the stack.
    /// </summary>
    /// <param name="panel">The name of the panel to load in from resources.</param>
    public Panel Load(string panel) {
        panelReference = Resources.Load<GameObject>("Interfaces/" + panel);
        GameObject tempPanelObj = (GameObject) Instantiate(panelReference);
        Panel panelRef = tempPanelObj.GetComponent<Panel>();
        panelRef.onRemove += DestroyPanel;

        RectTransform panelRectTrans = tempPanelObj.GetComponent<RectTransform>();

        tempPanelObj.name = panel;
        tempPanelObj.SetActive(true);
        
        panelRectTrans.SetParent(UIRoot);

        panelRectTrans.localScale = Vector3.one;
        panelRectTrans.anchoredPosition = Vector2.zero;
        panelRectTrans.sizeDelta = Vector2.one;
        panelRectTrans.SetAsLastSibling();

        UINonStackRef.Add(panelRef);

        panelRef.Initialize();

        return panelRef;
    }

    public Panel Push(string panel) {
        Panel p = Load(panel);
        UINonStackRef.Remove(p);
        UIStack.Add(p);
        return p;
    }

    public void Pop() {
        Panel panelRef = UIStack[UIStack.Count - 1];
        UIStack.RemoveAt(UIStack.Count - 1);
        RemovePanel(panelRef);
    }

    public void PopToPanel(string panel) {
        if (UIStack.Count <= 0) return;
            
        Panel panelRef = UIStack[UIStack.Count - 1];
        UIStack.RemoveAt(UIStack.Count - 1);
        RemovePanel(panelRef);

        if (panelRef.gameObject.name == panel) return;
    }

    public void Remove(Panel panel) {
        if(UIStack.Contains(panel)) {
            UIStack.Remove(panel);
            RemovePanel(panel);
        } else if(UINonStackRef.Contains(panel)) {
            UINonStackRef.Remove(panel);
            RemovePanel(panel);
        } else {
            traceError("Problem while removing the panel: " + panel);
        }
    }

    public void Remove(string panel) {
        Panel panelRef = UIStack.Find(o => o.Name == panel);
        if (panelRef != null) {
            UIStack.Remove(panelRef);
            RemovePanel(panelRef);
        }
        panelRef = UINonStackRef.Find(o => o.Name == panel);
        if (panelRef != null) {
            UINonStackRef.Remove(panelRef);
            RemovePanel(panelRef);
        }
    }

    public void ClearUI() {
        foreach (Panel panel in UIRoot.GetComponentsInChildren<Panel>(true)) {
            Destroy(panel.gameObject);
        }
    }

    private void RemovePanel(Panel panel) {
        if(panel.onRemove!=null) panel.onRemove(panel);
    }

    private void DestroyPanel(Panel panel) {
        Destroy(panel.gameObject);
    }

    public bool IsPointerOverGameObject() {
        if (eventSystem.IsPointerOverGameObject())
            return true;
        
        foreach (Touch t in Input.touches) {
            if (EventSystem.current.IsPointerOverGameObject(t.fingerId))
                return true;
        }

        return false;
    }

    public Panel GetInterfaceFromStack(string interfaceName) {
        foreach (Panel panel in UIStack) {
            if (panel.gameObject.name == interfaceName)
                return panel;
        }

        return null;
    }
}
