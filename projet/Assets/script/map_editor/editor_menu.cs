using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;

public class editor_menu : MonoBehaviour
{
    public Image selectedTool;
    public GameObject GridHolder;
    public GameObject GridElement;
    private int m_iWidth = 10;
    private int m_iHeight = 10;
	// Use this for initialization
	void Start ()
    {
        updateGrid();
	}
	
	// Update is called once per frame
	void Update ()
    {
	}
    public void changeSelected(Image clickedButton)
    {
        selectedTool.color = clickedButton.color;
    }
    private void updateGrid()
    {
        foreach (Transform child in GridHolder.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        float iStartX = -m_iWidth / 2.0f + 0.5f;
        float iStartY = -m_iHeight / 2.0f + 0.5f;
        for(int i = 0; i < m_iWidth; i++)
        {
            for (int j = 0; j < m_iHeight; j++)
            {
                GameObject newGridElement = (GameObject)Instantiate(GridElement, new Vector3((iStartX + i) * 10, (iStartY + j) * 10), Quaternion.identity);
                newGridElement.transform.SetParent(GridHolder.transform);
            }
        }
    }
    public void changeHeight(string _sValue)
    {
        int iHeight = int.Parse('0' + _sValue);
        if(iHeight > 0 && iHeight < 1000)
        {
            m_iHeight = iHeight;
            updateGrid();
        }
    }
    public void changeWidth(string _sValue)
    {
        int iWidth = int.Parse('0' + _sValue);
        if (iWidth > 0 && iWidth < 1000)
        {
            m_iWidth = iWidth;
            updateGrid();
        }
    }
}
