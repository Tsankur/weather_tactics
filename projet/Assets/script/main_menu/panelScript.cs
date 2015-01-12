using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class panelScript : MonoBehaviour
{
	// Use this for initialization
	void Start ()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.0f);
        rectTransform.anchorMax = new Vector2(0.5f, 1.0f);
        rectTransform.sizeDelta = new Vector2(Mathf.Min(526, Screen.width - 100), -100);
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}
}
