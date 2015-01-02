using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;

public class editor_menu : MonoBehaviour
{
    public Image selectedTool;
    public GameObject GridHolder;
    public GameObject GridElement;
    private int m_iWidth = 0;
    private int m_iHeight = 0;
    private int m_iNewWidth = 10;
    private int m_iNewHeight = 10;
    private GameObject[,] GridElements;
    public Camera m_MainCamera;
    public Material m_SelectedMaterial;
	// Use this for initialization
	void Start ()
    {
        updateGrid();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfos;
            if (Physics.Raycast(ray,out hitInfos))
            {
                hitInfos.collider.gameObject.GetComponent<Renderer>().material = m_SelectedMaterial;
            }
        }
	}
    public void changeSelected(Image clickedButton)
    {
        selectedTool.color = clickedButton.color;
    }
    public void changeSelectedMaterial(Material _material)
    {
        m_SelectedMaterial = _material;
    }
    public void updateGrid()
    {
        GameObject[,] oldGridElements = GridElements;
        /*foreach (Transform child in GridHolder.transform)
        {
            GameObject.Destroy(child.gameObject);
        }*/
        GridElements = new GameObject[m_iNewWidth, m_iNewHeight];
        int maxWidth = Mathf.Max(m_iWidth, m_iNewWidth);
        int maxHeight = Mathf.Max(m_iHeight, m_iNewHeight);
        for (int i = 0; i < maxWidth; i++)
        {
            for (int j = 0; j < maxHeight; j++)
            {
                if (i >= m_iNewWidth || j >= m_iNewHeight)
                {
                    GameObject.Destroy(oldGridElements[i, j]);
                }
                else if (i >= m_iWidth || j >= m_iHeight)
                {
                    GameObject newGridElement = (GameObject)Instantiate(GridElement, new Vector3(i * 10, j * 10), Quaternion.identity);
                    newGridElement.transform.SetParent(GridHolder.transform);
                    GridElements[i, j] = newGridElement;
                }
                else
                {
                    GridElements[i, j] = oldGridElements[i, j];
                }
            }
        }
        m_iWidth = m_iNewWidth;
        m_iHeight = m_iNewHeight;
        Camera.main.GetComponent<editor_camera>().SetMaxPosition(new Vector2((m_iWidth - 1) * 10, (m_iHeight - 1) * 10));
    }
    public void changeHeight(string _sValue)
    {
        int iHeight = int.Parse('0' + _sValue);
        if(iHeight > 0 && iHeight < 1000)
        {
            m_iNewHeight = iHeight;
            //updateGrid();
        }
    }
    public void changeWidth(string _sValue)
    {
        int iWidth = int.Parse('0' + _sValue);
        if (iWidth > 0 && iWidth < 1000)
        {
            m_iNewWidth = iWidth;
            //updateGrid();
        }
    }
}
