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
    private GameObject[,] m_tTerrainGridElements;
    private GameObject[,] m_tRiverGridElements;
    private GameObject[,] m_tConstructionGridElements;
    private GameObject[,] m_tItemGridElements;
    private int[,] m_tTerrainGridElementValues;
    private int[,] m_tRiverGridElementValues;
    private int[,] m_tConstructionGridElementValues;
    private int[,] m_tItemGridElementValues;

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
                Debug.Log(m_iWidth.ToString() + " " + m_iHeight.ToString());

                m_tTerrainGridElements = new GameObject[m_iWidth, m_iHeight];
                m_tRiverGridElements = new GameObject[m_iWidth, m_iHeight];
                m_tConstructionGridElements = new GameObject[m_iWidth, m_iHeight];
                m_tItemGridElements = new GameObject[m_iWidth, m_iHeight];

                m_tTerrainGridElementValues = new int[m_iWidth, m_iHeight];
                m_tRiverGridElementValues = new int[m_iWidth, m_iHeight];
                m_tConstructionGridElementValues = new int[m_iWidth, m_iHeight];
                m_tItemGridElementValues = new int[m_iWidth, m_iHeight];
                for (int i = 0; i < m_iWidth; i++)
                {
                    for (int j = 0; j < m_iHeight; j++)
                    {
                        int iTerrainMaterialId = br.ReadInt32();
                        int iRiverMaterialId = br.ReadInt32();
                        int iRiverRotation = br.ReadInt32();
                        int iConstructionMaterialId = br.ReadInt32();
                        int iConstructionRotation = br.ReadInt32();
                        int iItemMaterialId = br.ReadInt32();


                        GameObject newTerrainGridElement = (GameObject)Instantiate(m_GridElement, new Vector3(i * 10, j * 10), Quaternion.identity);
                        newTerrainGridElement.GetComponent<GridElement>().m_iX = i;
                        newTerrainGridElement.GetComponent<GridElement>().m_iY = j;
                        newTerrainGridElement.transform.SetParent(m_GridHolder.transform);
                        newTerrainGridElement.GetComponent<Renderer>().material = m_vMaterials[iTerrainMaterialId];

                        m_tTerrainGridElements[i, j] = newTerrainGridElement;
                        m_tTerrainGridElementValues[i, j] = iTerrainMaterialId;

                        if (iRiverMaterialId > 0)
                        {
                            GameObject newRiverGridElement = (GameObject)Instantiate(m_GridElement, new Vector3(i * 10, j * 10), Quaternion.identity);
                            newRiverGridElement.GetComponent<GridElement>().m_iX = i;
                            newRiverGridElement.GetComponent<GridElement>().m_iY = j;
                            newRiverGridElement.transform.SetParent(m_GridHolder.transform);
                            newRiverGridElement.transform.Rotate(Vector3.forward, iRiverRotation);
                            newRiverGridElement.GetComponent<Renderer>().material = m_vMaterials[iRiverMaterialId];

                            m_tRiverGridElements[i, j] = newRiverGridElement;
                        }
                        else
                        {
                            m_tRiverGridElements[i, j] = null;
                        }
                        m_tRiverGridElementValues[i, j] = iRiverMaterialId;
                        if (iConstructionMaterialId > 0)
                        {
                            GameObject newConstructionGridElement = (GameObject)Instantiate(m_GridElement, new Vector3(i * 10, j * 10), Quaternion.identity);
                            newConstructionGridElement.GetComponent<GridElement>().m_iX = i;
                            newConstructionGridElement.GetComponent<GridElement>().m_iY = j;
                            newConstructionGridElement.transform.SetParent(m_GridHolder.transform);
                            newConstructionGridElement.transform.Rotate(Vector3.forward, iConstructionRotation);
                            newConstructionGridElement.GetComponent<Renderer>().material = m_vMaterials[iConstructionMaterialId];

                            m_tConstructionGridElements[i, j] = newConstructionGridElement;
                        }
                        else
                        {
                            m_tConstructionGridElements[i, j] = null;
                        }
                        m_tConstructionGridElementValues[i, j] = iConstructionMaterialId;

                        m_tItemGridElements[i, j] = null;
                        m_tItemGridElementValues[i, j] = iItemMaterialId;

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
