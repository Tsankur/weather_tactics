using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;

public class editor_menu : MonoBehaviour
{
    public Image selectedTool;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public void changeSelected(Image clickedButton)
    {
        selectedTool.color = clickedButton.color;
    }
}
