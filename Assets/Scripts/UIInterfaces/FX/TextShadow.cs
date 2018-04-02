using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ExtensionMethods;

public class TextShadow : Tracer {

    public TextMeshProUGUI textShadow;
    public TextMeshProUGUI textActual;
    bool hasErrors = false;
    
	void Update () {
        if (textShadow == null || textActual == null) {
            if(!hasErrors) {
                traceError("Missing a textShadow / textActual reference somewhere on object:\n -> " + this.GetFullName());
                hasErrors = true;
            }
            
            return;
        }

		textShadow.text = textActual.text;
	}
}
