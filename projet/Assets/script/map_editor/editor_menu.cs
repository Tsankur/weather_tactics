using UnityEngine;
using UnityEngine.UI;
//using UnityEditor;
using System.Collections;
using System.IO;

public class editor_menu : MonoBehaviour
{
    public Camera m_MainCamera;

    public Image m_SelectedTool;
    private int m_iSelectedToolId = 1;
    public Material[] m_vMaterials;
    public Material m_SelectedMaterial;

    public GameObject m_GridHolder;
    public GameObject m_GridElement;
    public InputField m_WidthInput;
    public InputField m_HeightInput;
    private int m_iWidth = 0;
    private int m_iHeight = 0;
    private int m_iNewWidth = 10;
    private int m_iNewHeight = 10;
    private GameObject[,] m_tGridElements;
    private int[,] m_tGridElementValue;

    private bool m_bReplaceLevel = false;
    public InputField m_FileNameInput;
    public GameObject m_OveridePopup;
    public GameObject m_LevelListPanel;
    public GameObject m_LevelNamePrefab;

    public GameObject m_ServerListPanel;
    public GameObject m_StartServerButton;
    public GameObject m_StopServerButton;
    public GameObject m_DisconnectFromServerButton;
    private bool m_bWaitingForServerList = false;

    // Use this for initialization
    void Start()
    {
        if(!Directory.Exists(Application.persistentDataPath + "/Levels"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Levels");
        }
        loadLevelList();
        updateGrid();
        m_OveridePopup.SetActive(false);
        MasterServer.ipAddress = "78.236.192.198";
        //Network.natFacilitatorIP = "78.236.192.198";
        getServerList();
    }

    void OnApplicationQuit()
    {
        if (Network.isServer)
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
                GridElement gridElem = hitInfos.collider.gameObject.GetComponent<GridElement>();
                if(Network.isClient || Network.isServer)
                {
                    networkView.RPC("SetGridElementMaterial", RPCMode.All, gridElem.m_iX, gridElem.m_iY, m_iSelectedToolId);
                }
                else
                {
                    SetGridElementMaterial(gridElem.m_iX, gridElem.m_iY, m_iSelectedToolId);
                }
            }
        }
        if(m_bWaitingForServerList)
        {
            m_bWaitingForServerList = !ShowServerList();
        }
    }

    //Level Edition
    [RPC]
    void SetGridElementMaterial(int x, int y, int id)
    {
        m_tGridElements[x, y].GetComponent<Renderer>().material = m_vMaterials[id];
        m_tGridElementValue[x, y] = id;
    }

    [RPC]
    void ResetGrid(int _iWidth, int _iHeight)
    {
        m_WidthInput.text = _iWidth.ToString();
        m_HeightInput.text = _iHeight.ToString();
        m_iNewWidth = _iWidth;
        m_iNewHeight = _iHeight;
        resetMap();
    }

    [RPC]
    void SetGridSize(int _iWidth, int _iHeight)
    {
        m_WidthInput.text = _iWidth.ToString();
        m_HeightInput.text = _iHeight.ToString();
        m_iNewWidth = _iWidth;
        m_iNewHeight = _iHeight;
    }
    public void changeSelected(Image _oClickedButton)
    {
        m_SelectedTool.color = _oClickedButton.color;
    }
    public void SetSelectToolID(int _itoolID)
    {
        m_iSelectedToolId = _itoolID;
    }
    public void changeSelectedMaterial(Material _material)
    {
        m_SelectedMaterial = _material;
    }
    public void updateGridButtonPressed()
    {
        if (Network.isClient || Network.isServer)
        {
            networkView.RPC("SetGridSize", RPCMode.Others, m_iNewWidth, m_iNewHeight);
            networkView.RPC("updateGrid", RPCMode.All);
        }
        else
        {
            updateGrid();
        }
    }
    [RPC]
    void updateGrid()
    {
        GameObject[,] oldGridElements = m_tGridElements;
        int[,] oldGridElementValue = m_tGridElementValue;
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
                    GameObject newGridElement = (GameObject)Instantiate(m_GridElement, new Vector3(i * 10, j * 10), Quaternion.identity);
                    newGridElement.GetComponent<GridElement>().m_iX = i;
                    newGridElement.GetComponent<GridElement>().m_iY = j;
                    newGridElement.transform.SetParent(m_GridHolder.transform);
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
    public void resetMapButtonPressed()
    {
        if (Network.isServer || Network.isClient)
        {
            networkView.RPC("resetMap", RPCMode.All);
        }
        else
        {
            resetMap();
        }
    }
    [RPC]
    void resetMap()
    {
        foreach (Transform child in m_GridHolder.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        m_iWidth = 0;
        m_iHeight = 0;
        m_FileNameInput.text = "";
        updateGrid();
    }

    // Level Management
    void loadLevelList()
    {
        foreach (Transform child in m_LevelListPanel.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        string[] tLevels = Directory.GetFiles(Application.persistentDataPath + "/Levels");
        int iLevelId = 0;
        m_LevelListPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(130, Mathf.Max(5 + 35 * tLevels.Length, 110));
        m_LevelListPanel.transform.localPosition = new Vector3(0, Mathf.Min((-5 - 35 * tLevels.Length) / 2 + 55, 0), 0);
        foreach(string path in tLevels)
        {
            string szLevel = Path.GetFileNameWithoutExtension(path);
            GameObject oNewLevelName = (GameObject)Instantiate(m_LevelNamePrefab);
            oNewLevelName.transform.SetParent(m_LevelListPanel.transform, false);
            Button oNewButton = oNewLevelName.GetComponent<Button>();
            oNewLevelName.transform.localPosition = new Vector3(0, -20 - 35 * iLevelId + m_LevelListPanel.GetComponent<RectTransform>().sizeDelta.y / 2, 0);
            oNewLevelName.transform.GetChild(0).GetComponent<Text>().text = szLevel;
            AddListenerToLevelButton(oNewButton, szLevel);
            iLevelId++;
        }
    }
    void AddListenerToLevelButton(Button _oBbutton, string _szLevel)
    {
        _oBbutton.onClick.AddListener(() => loadMap(_szLevel));
    }
    public void saveMap()
    {
        if(m_FileNameInput.text.Length > 0)
        {
            string szFilePath = Application.persistentDataPath + "/Levels/" + m_FileNameInput.text + ".lvl";
            if (!File.Exists(szFilePath) || m_bReplaceLevel)
            {
                BinaryWriter bw;
                //create the file
                try
                {
                    bw = new BinaryWriter(new FileStream(szFilePath, FileMode.Create));
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
    public void forceSaveMap(bool _bForce)
    {
        if(_bForce)
        {
            m_bReplaceLevel = true;
            saveMap();
        }
        m_OveridePopup.SetActive(false);
    }
    public void loadMap(string _sMapName)
    {
        m_FileNameInput.text = _sMapName;
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
                m_iNewWidth = br.ReadInt32();
                m_iNewHeight = br.ReadInt32();
                if (Network.isServer || Network.isClient)
                {
                    networkView.RPC("ResetGrid", RPCMode.All, m_iNewWidth, m_iNewHeight);
                }
                else
                {
                    ResetGrid(m_iNewWidth, m_iNewHeight);
                }
                for (int i = 0; i < m_iWidth; i++)
                {
                    for (int j = 0; j < m_iHeight; j++)
                    {
                        int iMaterialId = br.ReadInt32();
                        if(Network.isServer || Network.isClient)
                        {
                            networkView.RPC("SetGridElementMaterial", RPCMode.All, i, j, iMaterialId);
                        }
                        else
                        {
                            SetGridElementMaterial(i, j, iMaterialId);
                        }
                    }
                }
            }
            catch (IOException e)
            {
                Debug.Log(e.Message + "\n Cannot read file.");
                return;
            }
            br.Close();
        }
    }

    // network management (server)
    public void startServer()
    {
        if (!Network.isClient)
        {
            Network.InitializeServer(5, 6000, true);
            MasterServer.RegisterHost("WeatherTactics", "Tsan editor", "trololol");
            m_StartServerButton.SetActive(false);
            m_StopServerButton.SetActive(true);
        }
        else
        {
            Debug.Log("can't start server you are already connected to a server");
        }
    }
    public void stopServer()
    {
        if (Network.isServer)
        {
            Network.Disconnect();
            MasterServer.UnregisterHost();
            m_StartServerButton.SetActive(true);
            m_StopServerButton.SetActive(false);
        }
    }
    void OnPlayerConnected(NetworkPlayer _oPlayer)
    {
        Debug.Log("new player connected");
        networkView.RPC("ResetGrid", _oPlayer, m_iNewWidth, m_iNewHeight);
        for (int i = 0; i < m_iWidth; i++)
        {
            for (int j = 0; j < m_iHeight; j++)
            {
                networkView.RPC("SetGridElementMaterial", _oPlayer, i, j, m_tGridElementValue[i, j]);
            }
        }
    }
    void OnPlayerDisconnected(NetworkPlayer _oPlayer)
    {
        Debug.Log("player disconnected : " + _oPlayer);
    }

    // network management (client)
    bool ShowServerList()
    {
        HostData[] tServeurs = MasterServer.PollHostList();
        if (tServeurs.Length > 0)
        {
            int iServerId = 0;
            m_ServerListPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(130, Mathf.Max(5 + 35 * tServeurs.Length, 110));
            m_ServerListPanel.transform.localPosition = new Vector3(0, Mathf.Min((-5 - 35 * tServeurs.Length) / 2 + 55, 0), 0);
            foreach (HostData oServer in tServeurs)
            {
                string szServerName = oServer.gameName;
                /*foreach(string ip in oServer.ip)
                {
                    Debug.Log(ip);
                }
                Debug.Log(oServer.port.ToString());*/
                GameObject oNewServerName = (GameObject)Instantiate(m_LevelNamePrefab);
                oNewServerName.transform.SetParent(m_ServerListPanel.transform, false);
                Button oNewButton = oNewServerName.GetComponent<Button>();
                oNewServerName.transform.localPosition = new Vector3(0, -20 - 35 * iServerId + m_ServerListPanel.GetComponent<RectTransform>().sizeDelta.y / 2, 0);
                oNewServerName.transform.GetChild(0).GetComponent<Text>().text = szServerName + " " + oServer.connectedPlayers + "/" + oServer.playerLimit;
                AddListenerToServerButton(oNewButton, oServer);
                iServerId++;
            }
            return true;
        }
        else
        {
            return false;
        }
    }
    void AddListenerToServerButton(Button _oButton, HostData _oServer)
    {
        _oButton.onClick.AddListener(() => ConnectToServer(_oServer));
    }
    public void getServerList()
    {
        MasterServer.ClearHostList();
        MasterServer.RequestHostList("WeatherTactics");
        foreach (Transform child in m_ServerListPanel.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        m_bWaitingForServerList = true;
    }

    void ConnectToServer(HostData _oServer)
    {
        if (!Network.isServer)
        {
            if(!Network.isClient)
            {
                Network.Connect(_oServer);
            }
            else
            {
                Debug.Log("can't connect to server you are already connected to a server");
            }
        }
        else
        {
            Debug.Log("can't connect to server you are already a server");
        }
    }
    public void DisconnectFromServer()
    {
        Network.Disconnect();
    }
    void OnConnectedToServer()
    {
        Debug.Log("Connected to server");
        m_DisconnectFromServerButton.SetActive(true);
    }
    void OnDisconnectedFromServer(NetworkDisconnection _oInfo)
    {
        Debug.Log("Disconnected from server");
        m_DisconnectFromServerButton.SetActive(false);
    }
}
