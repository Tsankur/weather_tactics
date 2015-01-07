﻿using UnityEngine;
using UnityEngine.UI;
//using UnityEditor;
using System.Collections;
using System.IO;

public class editor_menu : MonoBehaviour
{
    public Image selectedTool;
    public GameObject GridHolder;
    public GameObject GridElement;
    private int m_iWidth = 0;
    private int m_iHeight = 0;
    private int m_iNewWidth = 10;
    private int m_iNewHeight = 10;
    private int m_iSelectedToolId = 1;
    private GameObject[,] m_tGridElements;
    private int[,] m_tGridElementValue;
    private bool m_bReplaceLevel = false;
    private bool m_bWaitingForServerList = false;
    public Camera m_MainCamera;
    public Material m_SelectedMaterial;
    public InputField m_FileName;
    public Material[] m_vMaterials;
    public GameObject m_OveridePopup;
    public GameObject m_LevelListPanel;
    public GameObject m_LevelNamePrefab;
    public GameObject m_ServerListPanel;
    public GameObject m_StartServerButton;
    public GameObject m_StopServerButton;
    public GameObject m_DisconnectFromServerButton;
    // Use this for initialization
    void Start()
    {
        loadLevelList();
        updateGrid();
        m_OveridePopup.SetActive(false);
        MasterServer.ipAddress = "192.168.0.12";
        Network.natFacilitatorIP = "192.168.0.12";
    }

    void OnApplicationQuit()
    {
        if (!Network.isServer)
        {
            MasterServer.UnregisterHost();
        }
        Network.Disconnect();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfos;
            if (Physics.Raycast(ray, out hitInfos))
            {
                hitInfos.collider.gameObject.GetComponent<Renderer>().material = m_SelectedMaterial;
                GridElement gridElem = hitInfos.collider.gameObject.GetComponent<GridElement>();
                m_tGridElementValue[gridElem.x, gridElem.y] = m_iSelectedToolId;
            }
        }
        if(m_bWaitingForServerList)
        {
            m_bWaitingForServerList = !ShowServerList();
        }
    }

    //Level Edition
    public void changeSelected(Image clickedButton)
    {
        selectedTool.color = clickedButton.color;
    }
    public void SetSelectToolID(int _itoolID)
    {
        m_iSelectedToolId = _itoolID;
    }
    public void changeSelectedMaterial(Material _material)
    {
        m_SelectedMaterial = _material;
    }
    public void updateGrid()
    {
        GameObject[,] oldGridElements = m_tGridElements;
        int[,] oldGridElementValue = m_tGridElementValue;
        /*foreach (Transform child in GridHolder.transform)
        {
            GameObject.Destroy(child.gameObject);
        }*/
        m_tGridElements = new GameObject[m_iNewWidth, m_iNewHeight];
        m_tGridElementValue = new int[m_iNewWidth, m_iNewHeight];
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
                    newGridElement.GetComponent<GridElement>().x = i;
                    newGridElement.GetComponent<GridElement>().y = j;
                    newGridElement.transform.SetParent(GridHolder.transform);
                    m_tGridElements[i, j] = newGridElement;
                    m_tGridElementValue[i, j] = 2;
                }
                else
                {
                    m_tGridElements[i, j] = oldGridElements[i, j];
                    m_tGridElementValue[i, j] = oldGridElementValue[i, j];
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
        if (iHeight > 0 && iHeight < 1000)
        {
            m_iNewHeight = iHeight;
        }
    }
    public void changeWidth(string _sValue)
    {
        int iWidth = int.Parse('0' + _sValue);
        if (iWidth > 0 && iWidth < 1000)
        {
            m_iNewWidth = iWidth;
        }
    }

    // Level Management
    void loadLevelList()
    {
        foreach (Transform child in m_LevelListPanel.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        
        string[] levels = Directory.GetFiles("./Levels");
        int levelId = 0;
        m_LevelListPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(130, Mathf.Max(5 + 35 * levels.Length, 110));
        m_LevelListPanel.transform.localPosition = new Vector3(0, Mathf.Min((-5 - 35 * levels.Length) / 2 + 55, 0), 0);
        foreach(string path in levels)
        {
            string level = path.Substring(9, path.Length - 9 - 4);
            GameObject newLevelName = (GameObject)Instantiate(m_LevelNamePrefab);
            newLevelName.transform.SetParent(m_LevelListPanel.transform, false);
            Button newButton = newLevelName.GetComponent<Button>();
            newLevelName.transform.localPosition = new Vector3(0, -20 - 35 * levelId + m_LevelListPanel.GetComponent<RectTransform>().sizeDelta.y / 2, 0);
            newLevelName.transform.GetChild(0).GetComponent<Text>().text = level;
            AddListenerToLevelButton(newButton, level);
            levelId++;
        }
    }
    void AddListenerToLevelButton(Button button, string level)
    {
        button.onClick.AddListener(() => loadMap(level));
    }
    public void saveMap()
    {
        if(m_FileName.text.Length > 0)
        {

            string filePath = "./Levels/" + m_FileName.text + ".lvl";
            if (!File.Exists(filePath) || m_bReplaceLevel)
            {
                BinaryWriter bw;
                //create the file
                try
                {
                    bw = new BinaryWriter(new FileStream(filePath, FileMode.Create));
                }
                catch (IOException e)
                {
                    Debug.Log(e.Message + "\n Cannot create file.");
                    return;
                }
                //write in the file
                try
                {
                    bw.Write(m_iWidth);
                    bw.Write(m_iHeight);
                    for (int i = 0; i < m_iWidth; i++)
                    {
                        for (int j = 0; j < m_iWidth; j++)
                        {
                            bw.Write(m_tGridElementValue[i, j]);
                        }
                    }
                }
                catch (IOException e)
                {
                    Debug.Log(e.Message + "\n Cannot write to file.");
                    return;
                }
                bw.Close();
                if (m_bReplaceLevel)
                {
                    m_bReplaceLevel = false;
                }
                else
                {
                    loadLevelList();
                }
            }
            else
            {
                m_OveridePopup.SetActive(true);
            }
        }
    }
    public void forceSaveMap(bool force)
    {
        if(force)
        {
            m_bReplaceLevel = true;
            saveMap();
        }
        m_OveridePopup.SetActive(false);
    }
    public void loadMap(string _sMapName)
    {
        m_FileName.text = _sMapName;
        string filePath = "./Levels/" + _sMapName + ".lvl";
        if (File.Exists(filePath))
        {
            BinaryReader br;
            //create the file
            try
            {
                br = new BinaryReader(new FileStream(filePath, FileMode.Open));
            }
            catch (IOException e)
            {
                Debug.Log(e.Message + "\n Cannot open file.");
                return;
            }
            //read the file
            try
            {
                foreach (Transform child in GridHolder.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }
                m_iWidth = br.ReadInt32();
                m_iHeight = br.ReadInt32();
                m_tGridElements = new GameObject[m_iWidth, m_iHeight];
                m_tGridElementValue = new int[m_iWidth, m_iHeight];
                for (int i = 0; i < m_iWidth; i++)
                {
                    for (int j = 0; j < m_iHeight; j++)
                    {
                        int iMaterialId = br.ReadInt32();
                        GameObject newGridElement = (GameObject)Instantiate(GridElement, new Vector3(i * 10, j * 10), Quaternion.identity);
                        newGridElement.GetComponent<GridElement>().x = i;
                        newGridElement.GetComponent<GridElement>().y = j;
                        newGridElement.transform.SetParent(GridHolder.transform);
                        newGridElement.GetComponent<Renderer>().material = m_vMaterials[iMaterialId];
                        m_tGridElements[i, j] = newGridElement;
                        m_tGridElementValue[i, j] = iMaterialId;
                    }
                }
                Camera.main.GetComponent<editor_camera>().SetMaxPosition(new Vector2((m_iWidth - 1) * 10, (m_iHeight - 1) * 10));
            }
            catch (IOException e)
            {
                Debug.Log(e.Message + "\n Cannot read file.");
                return;
            }
            br.Close();
        }
    }
    public void resetMap()
    {
        foreach (Transform child in GridHolder.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        m_iWidth = 0;
        m_iHeight = 0;
        m_FileName.text = "";
        updateGrid();
    }
    // network management
    public void startServer()
    {
        if (!Network.isClient)
        {
            Network.InitializeServer(6, 2305, true);
            MasterServer.RegisterHost("WeatherTactics", "Tsan editor", "trololol");
            m_StartServerButton.SetActive(false);
            m_StopServerButton.SetActive(true);
        }
    }
    public void stopServer()
    {
        Network.Disconnect();
        MasterServer.UnregisterHost();
        m_StartServerButton.SetActive(true);
        m_StopServerButton.SetActive(false);
    }
    bool ShowServerList()
    {
        HostData[] serveurs = MasterServer.PollHostList();
        if (serveurs.Length > 0)
        {
            foreach (Transform child in m_ServerListPanel.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            int ServerId = 0;
            m_ServerListPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(130, Mathf.Max(5 + 35 * serveurs.Length, 110));
            m_ServerListPanel.transform.localPosition = new Vector3(0, Mathf.Min((-5 - 35 * serveurs.Length) / 2 + 55, 0), 0);
            foreach (HostData server in serveurs)
            {
                string serverName = server.gameName;
                GameObject newLevelName = (GameObject)Instantiate(m_LevelNamePrefab);
                newLevelName.transform.SetParent(m_ServerListPanel.transform, false);
                Button newButton = newLevelName.GetComponent<Button>();
                newLevelName.transform.localPosition = new Vector3(0, -20 - 35 * ServerId + m_ServerListPanel.GetComponent<RectTransform>().sizeDelta.y / 2, 0);
                newLevelName.transform.GetChild(0).GetComponent<Text>().text = serverName;
                AddListenerToServerButton(newButton, server);
                ServerId++;
            }
            return true;
        }
        else
        {
            return false;
        }
    }
    public void getServerList()
    {
        MasterServer.ClearHostList();
        MasterServer.RequestHostList("WeatherTactics");
        m_bWaitingForServerList = true;
    }
    void AddListenerToServerButton(Button button, HostData server)
    {
        button.onClick.AddListener(() => ConnectToServer(server));
    }
    void ConnectToServer(HostData server)
    {
        if(!Network.isServer)
        {
            Network.Connect(server);
            m_DisconnectFromServerButton.SetActive(true);
        }
        else
        {
            Debug.Log("can't connect to server you already are a server");
        }
    }
    public void DisconnectFromServer()
    {
        Network.Disconnect();
        m_DisconnectFromServerButton.SetActive(false);
    }
}
