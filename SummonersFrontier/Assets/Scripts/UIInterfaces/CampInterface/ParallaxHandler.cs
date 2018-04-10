using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ParallaxHandler : MonoBehaviour {
    public SpriteRenderer campMain;
    [Inspectionary] public ParallaxObjects ParallaxObjects = new ParallaxObjects();
    [Inspectionary] public ParallaxStartPosition OriginPosition = new ParallaxStartPosition();

    float last_offset = 0f;
    bool isMouseDown = false;
    Vector3 mouseOrigin = Vector3.zero;
    Vector3 offset;

    float MaxX = 7.85f;

    void Start() {
        foreach (GameObject key in ParallaxObjects.Keys) {
            OriginPosition.Add(key, key.transform.position);
        }

        //MaxX = Mathf.Floor(campMain.bounds.size.x)/4f;
        //Debug.Log(MaxX);
    }

    void Update() {
        if (MenuManager.Instance.IsPointerOverGameObject()) {
            isMouseDown = false;
            return;
        }

        if (MenuManager.Instance.UIOnCampScreenPoped)
            return;

        if (Input.GetMouseButtonDown(0)) {
            isMouseDown = true;
            mouseOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }   
        
        if (Input.GetMouseButton(0) && isMouseDown) {
            // Drag
            offset = Camera.main.ScreenToWorldPoint(Input.mousePosition) - mouseOrigin;
            MoveObjectsFromOriginBy(offset.x);
        }

        if (Input.GetMouseButtonUp(0)) {
            last_offset += offset.x;
            
            if (offset.magnitude < 0.1f) {
                // Check what we clicked on
                Ray ray = new Ray(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit)) {
                    //Debug.Log("Hit: " + hit.collider.name + ", " + offset.magnitude);
                    // Grab the object, Get the interactable component and interact
                    hit.collider.GetComponent<CampInteractable>().Interact();
                }
            }
        }
    }

    public void ResetScrollPosition() {
        isMouseDown = false;
        last_offset = 0f;
        MoveObjectsFromOriginBy(0f);
    }

    void MoveObjectsFromOriginBy(float x_offset) {
        Vector3 tempPosition = Vector3.zero;
        x_offset += last_offset;

        /**
         * TODO:    **BUG** Something should Clamp the Min/Max of the x_offset variable.
         *          It's causing issues if a player keeps scrolling out-of-bounds
         *          like it keeps accumulating the distance dragged, making it odd
         *          to swipe / drag to opposite direction when nothing happens, but
         *          in actuality it's progressively catching back up the x_offset.
         */
        foreach (GameObject key in ParallaxObjects.Keys) {
            tempPosition = OriginPosition[key];
            tempPosition.x = x_offset * ParallaxObjects[key];

            // Limits
            if (tempPosition.x > MaxX * ParallaxObjects[key]) tempPosition.x = MaxX * ParallaxObjects[key];
            if (tempPosition.x < -MaxX * ParallaxObjects[key]) tempPosition.x = -MaxX * ParallaxObjects[key];

            key.transform.position = tempPosition;
            key.transform.DOKill();
            key.transform.DOMoveX(tempPosition.x, 0.5f);
        }
    }
}


[System.Serializable] public class ParallaxObjects : SerializableDictionary<GameObject, float> { }
[System.Serializable] public class ParallaxStartPosition : SerializableDictionary<GameObject, Vector3> { }