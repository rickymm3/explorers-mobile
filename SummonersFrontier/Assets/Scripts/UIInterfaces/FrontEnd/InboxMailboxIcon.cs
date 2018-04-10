using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;

public class InboxMailboxIcon : MonoBehaviour {
    public SpriteRenderer spriteRenderer;

    public Sprite mailboxClosed;
    public Sprite mailboxOpened;
    
    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Use this for initialization
    void Start () {
		//Check if there is mail.
        UpdateSprite(false);

        GameAPIManager.API.Messages.OnStatusChanged += UpdateSprite;
    }

    void OnDestroy() {
        GameAPIManager.API.Messages.OnStatusChanged -= UpdateSprite;
    }

    void UpdateSprite(bool hasNewMail) {
        if (hasNewMail) {
            spriteRenderer.sprite = mailboxOpened;
        } else {
            spriteRenderer.sprite = mailboxClosed;
        }
    }
}
