using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class worldMap : MonoBehaviour
{
    private int[] m_tTerrainCosts = {1000, 2, 4, 4, 6, 3, 3, 4, 1, 1, 1, 1, 0, 4, 4, 4, 4, 0};
    public GameObject m_GridHolder;
    public GameObject m_GridElement;
    public GameObject m_CharacterPrefab;
    public GameObject m_SpawnPrefab;
    public Material[] m_vMaterials;
    private int m_iWidth = 0;
    private int m_iHeight = 0;
    private GameObject[,] m_tTerrainGridElements;
    private GameObject[,] m_tRiverGridElements;
    private GameObject[,] m_tConstructionGridElements;
    private GameObject[,] m_tItemGridElements;
    private GameObject[,] m_tCharacterGridElements;
    private int[,] m_tTerrainGridElementValues;
    private int[,] m_tRiverGridElementValues;
    private int[,] m_tConstructionGridElementValues;
    private int[,] m_tItemGridElementValues;
    private int[,] m_tTerrainInfos;
    List<Spawn> m_tSpawns = new List<Spawn>();

    public Character instanciateCharacter(int _iX, int _iY)
    {
        GameObject newCharater = (GameObject)Instantiate(m_CharacterPrefab, new Vector3(_iX * 10, _iY * 10, -0.04f), Quaternion.identity);
        GridElement gridElem = newCharater.GetComponent<GridElement>();
        gridElem.Init(_iX, _iY, 4);
        return gridElem.GetComponent<Character>();
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
                int iPlayerCount = br.ReadInt32();

                m_tTerrainGridElements = new GameObject[m_iWidth, m_iHeight];
                m_tRiverGridElements = new GameObject[m_iWidth, m_iHeight];
                m_tConstructionGridElements = new GameObject[m_iWidth, m_iHeight];
                m_tItemGridElements = new GameObject[m_iWidth, m_iHeight];
                m_tCharacterGridElements = new GameObject[m_iWidth, m_iHeight];

                m_tTerrainGridElementValues = new int[m_iWidth, m_iHeight];
                m_tRiverGridElementValues = new int[m_iWidth, m_iHeight];
                m_tConstructionGridElementValues = new int[m_iWidth, m_iHeight];
                m_tItemGridElementValues = new int[m_iWidth, m_iHeight];
                m_tTerrainInfos = new int[m_iWidth, m_iHeight];
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
                        newTerrainGridElement.GetComponent<GridElement>().Init(i, j, 0);
                        newTerrainGridElement.transform.SetParent(m_GridHolder.transform);
                        newTerrainGridElement.GetComponent<Renderer>().material = m_vMaterials[iTerrainMaterialId];

                        m_tTerrainGridElements[i, j] = newTerrainGridElement;
                        m_tTerrainGridElementValues[i, j] = iTerrainMaterialId;
                        m_tTerrainInfos[i, j] = iTerrainMaterialId;

                        if (iRiverMaterialId > 0)
                        {
                            GameObject newRiverGridElement = (GameObject)Instantiate(m_GridElement, new Vector3(i * 10, j * 10, -0.01f), Quaternion.identity);
                            newRiverGridElement.GetComponent<GridElement>().Init(i, j, 1);
                            newRiverGridElement.transform.SetParent(m_GridHolder.transform);
                            newRiverGridElement.transform.Rotate(Vector3.forward, iRiverRotation);
                            newRiverGridElement.GetComponent<Renderer>().material = m_vMaterials[iRiverMaterialId];

                            m_tRiverGridElements[i, j] = newRiverGridElement;
                            m_tTerrainInfos[i, j] = iRiverMaterialId;
                        }
                        else
                        {
                            m_tRiverGridElements[i, j] = null;
                        }
                        m_tRiverGridElementValues[i, j] = iRiverMaterialId;
                        if (iConstructionMaterialId > 0)
                        {
                            GameObject newConstructionGridElement = (GameObject)Instantiate(m_GridElement, new Vector3(i * 10, j * 10, -0.02f), Quaternion.identity);
                            newConstructionGridElement.GetComponent<GridElement>().Init(i, j, 2);
                            newConstructionGridElement.transform.SetParent(m_GridHolder.transform);
                            newConstructionGridElement.transform.Rotate(Vector3.forward, iConstructionRotation);
                            newConstructionGridElement.GetComponent<Renderer>().material = m_vMaterials[iConstructionMaterialId];

                            m_tConstructionGridElements[i, j] = newConstructionGridElement;
                            m_tTerrainInfos[i, j] = iConstructionMaterialId;
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
                int iSpawnCount = br.ReadInt32();
                for (int i = 0; i < iSpawnCount; i++)
                {
                    int iX = br.ReadInt32();
                    int iY = br.ReadInt32();
                    int iPlayerID = br.ReadInt32();
                    
                    GameObject newSpawn = (GameObject)Instantiate(m_SpawnPrefab, new Vector3(iX * 10, iY * 10), Quaternion.identity);
                    newSpawn.renderer.enabled = false;
                    newSpawn.GetComponent<GridElement>().Init(iX, iY, 0);
                    newSpawn.transform.SetParent(m_GridHolder.transform);
                    Spawn spawn = newSpawn.GetComponent<Spawn>();
                    spawn.m_iPlayerID = iPlayerID;
                    m_tSpawns.Add(spawn);
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
    public int GetTerrainCost(int _iTerrainId)
    {
        return m_tTerrainCosts[_iTerrainId];
    }
    public int[,] GetTerrainInfos()
    {
        return m_tTerrainInfos;
    }
    public int GetMapWidth()
    {
        return m_iWidth;
    }
    public int GetMapHeight()
    {
        return m_iHeight;
    }
    public List<Spawn> GetSpawnList()
    {
        return m_tSpawns;
    }
}
