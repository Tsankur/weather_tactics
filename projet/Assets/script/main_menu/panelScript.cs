using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class panelScript : MonoBehaviour {
	// Use this for initialization
	void Start () {
        if(Screen.currentResolution.width > 480);
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(526, 760); 
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
