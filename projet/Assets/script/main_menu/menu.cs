using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class menu : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	    
	}

    public void changeScene(string scene_name)
    {
        Application.LoadLevel(scene_name);
    }

	public void quitGame()
	{
		Application.Quit();
	}
}
