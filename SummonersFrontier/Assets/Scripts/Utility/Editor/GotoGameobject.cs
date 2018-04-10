using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class GotoGameobject : EditorWindow {
    public static string PREF_NAME = "GotoGameobject:gameObjectName";

    //[ContextMenu("Open GotoGameobject Window")]
    static GotoGameobject window;
    string gameObjectName;
    bool isReturnDown = false;
    bool isReturnPressed = false;
    bool isJustOpened = false;

    [MenuItem("ERDS/GotoGameobject %g")] // %& = CTRL + ALT
    static void Init() {
        if(window!=null) {
            CloseMe();
            return;
        }
        window = (GotoGameobject) EditorWindow.GetWindow<GotoGameobject>();
        window.Show();
        window.isJustOpened = true;
        window.gameObjectName = PlayerPrefs.GetString(PREF_NAME);
    }

    [MenuItem("ERDS/Mount Prefab to UICanvas %u")] // %& = CTRL + ALT
    static void MountToUICanvas() {
        if(Selection.objects.Length!=1) return;
        
        UnityEngine.Object obj = Selection.objects[0];
        bool isPrefab = PrefabUtility.GetPrefabType(obj)==PrefabType.Prefab;
        if(!isPrefab) return;

        GameObject prefab = (GameObject) obj;
        GameObject uiCanvas = GameObject.Find("UIRoot");
        GameObject inst = Instantiate(prefab, uiCanvas.transform, false);

        //This causes the crash in Unity... GRRRRRR!
        PrefabUtility.ConnectGameObjectToPrefab(inst, prefab);

        prefab.transform.SetParent(uiCanvas.transform, false);
    }

    static void CloseMe() {
        window.Close();
        window = null;
    }

    private void OnGUI() {
        CheckInput();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Goto:", GUILayout.Width(40));

        GUI.SetNextControlName("txtGoto");
        string newStr = GUILayout.TextField(gameObjectName);
        if(!isReturnDown) gameObjectName = newStr;

        GUILayout.EndHorizontal();

        if(isJustOpened) {
            isJustOpened = false;
            GUI.FocusControl("txtGoto");
        }

        this.Repaint();
    }

    private void CheckInput() {
        Event e = Event.current;
        isReturnDown = e.isKey && e.keyCode == KeyCode.Return;
        if(isReturnDown && !isReturnPressed) {
            isReturnPressed = true;
            GotoLocation();
        }

        if(!isReturnDown) {
            isReturnPressed = false;
        }
    }

    private void GotoLocation() {
        Selection.activeGameObject = null;
        PlayerPrefs.SetString(PREF_NAME, gameObjectName);
        GameObject go = GameObject.Find(gameObjectName);
        Selection.activeGameObject = go;
        Collapse(go, false);
        CloseMe();
    }

    public static void Collapse(GameObject go, bool collapse) {
        // bail out immediately if the go doesn't have children
        if (go.transform.childCount == 0) return;
        // get a reference to the hierarchy window
        var hierarchy = GetFocusedWindow("Hierarchy");
        // select our go
        SelectObject(go);
        // create a new key event (RightArrow for collapsing, LeftArrow for folding)
        var key = new Event { keyCode = collapse ? KeyCode.RightArrow : KeyCode.LeftArrow, type = EventType.KeyDown };
        // finally, send the window the event
        hierarchy.SendEvent(key);
    }
    public static void SelectObject(UnityEngine.Object obj) {
        Selection.activeObject = obj;
    }
    public static EditorWindow GetFocusedWindow(string window) {
        FocusOnWindow(window);
        return EditorWindow.focusedWindow;
    }
    public static void FocusOnWindow(string window) {
        EditorApplication.ExecuteMenuItem("Window/" + window);
    }
}
