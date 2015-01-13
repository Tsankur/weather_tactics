using UnityEngine;
using System.Collections;
using System.IO;

public class game : MonoBehaviour
{
    // map related variables
    public GameObject m_GridHolder;
    public GameObject m_GridElement;
    public Material[] m_vMaterials;
    private int m_iWidth = 0;
    private int m_iHeight = 0;
    private GameObject[,] m_tGridElements;
    private int[,] m_tGridElementValue;

	// Use this for initialization
	void Start ()
    {
        loadMap(GlobalVariables.m_szMapToLoad);
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}
    public void loadMap(string _sMapName)
    {
        string szFilePath = Application.persistentDataPath + "/Levels/" + _sMapName + ".lvl";
        if (File.Exists(szFilePath))
        {
            BinaryReader br;
            //create the file
            try
            {
                br = new BinaryReader(new FileStream(szFilePath, FileMode.Open));
            }
            catch (IOException e)
            {
                Debug.Log(e.Message + "\n Cannot open file.");
                return;
            }
            //read the file
            try
            {
                m_iWidth = br.ReadInt32();
                m_iHeight = br.ReadInt32();

                foreach (Transform child in m_GridHolder.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }

                m_tGridElements = new GameObject[m_iWidth, m_iHeight];
                m_tGridElementValue = new int[m_iWidth, m_iHeight];
                for (int i = 0; i < m_iWidth; i++)
                {
                    for (int j = 0; j < m_iWidth; j++)
                    {
                        int iMaterialId = br.ReadInt32();
                        GameObject newGridElement = (GameObject)Instantiate(m_GridElement, new Vector3(i * 10, j * 10), Quaternion.identity);
                        newGridElement.GetComponent<GridElement>().m_iX = i;
                        newGridElement.GetComponent<GridElement>().m_iY = j;
                        newGridElement.transform.SetParent(m_GridHolder.transform);
                        newGridElement.GetComponent<Renderer>().material = m_vMaterials[iMaterialId];
                        m_tGridElements[i, j] = newGridElement;
                        m_tGridElementValue[i, j] = iMaterialId;
                    }
                }
                Camera.main.GetComponent<ingame_camera>().SetMaxPosition(new Vector2((m_iWidth - 1) * 10, (m_iHeight - 1) * 10));
            }
            catch (IOException e)
            {
                Debug.Log(e.Message + "\n Cannot read file.");
                return;
            }
            br.Close();
        }
        else
        {
            Debug.LogError("File not found");
        }
    }
}
