using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExtensionMethods;

public class UserProfileItemContainer : Tracer {
    
    public TextMeshProUGUI txtItemName;
    public TextMeshProUGUI txtItemDescription;
    public TextMeshProUGUI txtCounter;
    public Button btn;
    public Image imgItem;
    public Image imgRibbon;
    public Image imgCellBackground;
    public Image imgQuestion;
    public Image imgCounter;

    [HideInInspector] public Item item;
}
