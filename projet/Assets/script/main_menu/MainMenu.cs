using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class MainMenu : MonoBehaviour
{
    public GameObject m_ServerListPanel;
    public GameObject m_MultiplayerCreatePanel;
    public GameObject m_MultiplayerJoinPanel;
    public GameObject m_MultiplayerLobbyPanel;
    public GameObject m_ServerNamePrefab;
    bool m_bWaitingForServerList = false;

    public GameObject m_MultiplayerMapListPanel;
    public Button m_MultiplayerLaunchButton;
    public Button m_MultiplayerCreateButton;
    public string m_szMapName = "";
    Player[] m_PlayerList = new Player[4];
    public MultiplayerLobby m_MultiplayerLobby; 
    public int m_iMaxPlayerCount = 2;
    bool m_bReady = false;

    void Start()
    {
        MasterServer.ipAddress = "78.236.192.198";
        MasterServer.port = 23466;
        if (!Directory.Exists(Application.persistentDataPath + "/Levels"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Levels");
        }
        if (!Directory.Exists(Application.persistentDataPath + "/Levels/MultiplayerMaps"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Levels/MultiplayerMaps");
        }
    }
    void Update()
    {
        if (m_bWaitingForServerList)
        {
            m_bWaitingForServerList = !ShowServerList();
        }
    }
    bool ShowServerList()
    {
        HostData[] tServeurs = MasterServer.PollHostList();
        if (tServeurs.Length > 0)
        {
            int iServerId = 0;
            float fHolderHeight = m_ServerListPanel.GetComponent<RectTransform>().parent.GetComponent<RectTransform>().sizeDelta.y;
            Debug.Log(fHolderHeight.ToString());
            m_ServerListPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 5 + 70 * tServeurs.Length);
            foreach (HostData oServer in tServeurs)
            {
                if (oServer.comment == "open")
                {
                    string szServerName = oServer.gameName;
                    GameObject oNewServerName = (GameObject)Instantiate(m_ServerNamePrefab);
                    oNewServerName.transform.SetParent(m_ServerListPanel.transform, false);
                    Button oNewButton = oNewServerName.GetComponent<Button>();
                    oNewServerName.transform.localPosition = new Vector3(0, -5 - 70 * iServerId, 0);
                    oNewServerName.transform.GetChild(0).GetComponent<Text>().text = szServerName + " " + oServer.connectedPlayers + "/" + oServer.playerLimit;
                    AddListenerToServerButton(oNewButton, oServer);
                    iServerId++;
                }
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
    void ConnectToServer(HostData _oServer)
    {
        if (!Network.isServer)
        {
            if (!Network.isClient)
            {
                if (_oServer.connectedPlayers < _oServer.playerLimit)
                {
                    Network.Connect(_oServer);
                }
                else
                {
                    Debug.Log("can't connect to server the server is full");
                }
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
    public void SetMapName(string _szMapName)
    {
        m_szMapName = _szMapName;
        m_MultiplayerCreateButton.interactable = true;
    }

    public void loadMultiplayerLevelList()
    {
        foreach (Transform child in m_MultiplayerMapListPanel.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        string[] tLevels = Directory.GetFiles(Application.persistentDataPath + "/Levels");
        int iLevelId = 0;
        m_MultiplayerMapListPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 5 + 70 * tLevels.Length);
        foreach (string path in tLevels)
        {
            string szLevel = Path.GetFileNameWithoutExtension(path);
            GameObject oNewLevelName = (GameObject)Instantiate(m_ServerNamePrefab);
            oNewLevelName.transform.SetParent(m_MultiplayerMapListPanel.transform, false);
            Button oNewButton = oNewLevelName.GetComponent<Button>();
            oNewLevelName.transform.localPosition = new Vector3(0, -5 - 70 * iLevelId, 0);
            oNewLevelName.transform.GetChild(0).GetComponent<Text>().text = szLevel;
            AddListenerToLevelButton(oNewButton, szLevel);
            iLevelId++;
        }
    }
    void AddListenerToLevelButton(Button _oBbutton, string _szLevel)
    {
        _oBbutton.onClick.AddListener(() => SetMapName(_szLevel));
    }
    public void ReturnButtonPressed()
    {
        if (Network.isServer)
        {
            StopServer();
            m_MultiplayerCreatePanel.SetActive(true);
        }
        if (Network.isClient)
        {
            DisconnectFromServer();
            m_MultiplayerJoinPanel.SetActive(true);
            getServerList();
        }
    }


    int CreatePlayer(string _szPsuedo, NetworkPlayer _oPlayer)
    {
        for (int i = 0; i < m_iMaxPlayerCount; i++)
        {
            if (m_PlayerList[i] == null)
            {
                networkView.RPC("SetPlayerSlot", RPCMode.All, -1, i, _szPsuedo, _oPlayer);
                return i + 1;
            }
        }
        return -1;
    }
    public void CreateServer()
    {
        if (!Network.isClient)
        {
            m_iMaxPlayerCount = 2;
            string szFilePath = Application.persistentDataPath + "/Levels/" + m_szMapName + ".lvl";
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
                    int iMapWidth = br.ReadInt32();
                    int iMapHeight = br.ReadInt32();
                    m_iMaxPlayerCount = br.ReadInt32();
                }
                catch (IOException e)
                {
                    Debug.Log(e.Message + "\n Cannot read file.");
                    return;
                }
                br.Close();
            }
            m_PlayerList = new Player[m_iMaxPlayerCount];
            m_MultiplayerLobby.Reset(m_iMaxPlayerCount);
            Network.InitializeServer(m_iMaxPlayerCount - 1, 6000, true);
            MasterServer.RegisterHost("WeatherTactics", "Tsan game", "open");
            GlobalVariables.m_szPseudo = "Tsan";
            SetPlayerID(CreatePlayer("Tsan", Network.player));
            m_bReady = true;
            SetPlayerReady(GlobalVariables.m_iCurrentPlayerId - 1, true);
        }
        else
        {
            Debug.Log("can't start server you are already connected to a server");
        }
    }
    [RPC]
    void SetMaxPlayerCount(int _iMaxPlayerCount)
    {
        m_iMaxPlayerCount = _iMaxPlayerCount;
        m_MultiplayerLobby.Reset(m_iMaxPlayerCount);
        m_PlayerList = new Player[m_iMaxPlayerCount];
    }
    void OnPlayerConnected(NetworkPlayer _oPlayer)
    {
        networkView.RPC("SetMaxPlayerCount", _oPlayer, m_iMaxPlayerCount);
        for (int i = 0; i < m_iMaxPlayerCount; i++)
        {
            if (m_PlayerList[i] != null)
            {
                networkView.RPC("SetPlayerSlot", _oPlayer, -1, i, m_PlayerList[i].m_szPseudo, m_PlayerList[i].m_oPlayer);
                networkView.RPC("SetPlayerReady", _oPlayer, i, m_PlayerList[i].m_bReady);
            }
        }
        Debug.Log("new player connected");
    }
    void OnPlayerDisconnected(NetworkPlayer _oPlayer)
    {
        for (int i = 0; i < m_iMaxPlayerCount; i++)
        {
            if (m_PlayerList[i] != null)
            {
                if (m_PlayerList[i].m_oPlayer == _oPlayer)
                {
                    networkView.RPC("SetPlayerSlot", RPCMode.All, i, -1, "", _oPlayer);
                    break;
                }
            }
        }
        Debug.Log("player disconnected : " + _oPlayer);
    }
    public void GameLaunchButtonPressed()
    {
        Network.maxConnections = -1;
        MasterServer.RegisterHost("WeatherTactics", "Tsan game", "closed");
        MasterServer.UnregisterHost();
    }
    public void StopServer()
    {
        MasterServer.UnregisterHost();
        Network.Disconnect();
    }


    [RPC]
    void SetPlayerID(int _iID)
    {
        GlobalVariables.m_iCurrentPlayerId = _iID;
        GlobalVariables.m_iCurrentPlayerTeam = _iID % 2;
    }
    [RPC]
    void SetPseudo(string _szPseudo, NetworkMessageInfo _Info)
    {
        int iNewPlayerID = CreatePlayer(_szPseudo, _Info.sender);
        networkView.RPC("SetPlayerID", _Info.sender, iNewPlayerID);
    }
    void OnConnectedToServer()
    {
        Debug.Log("Connected to server");
        if (Network.isClient)
        {
            m_bReady = false;
            m_MultiplayerJoinPanel.SetActive(false);
            m_MultiplayerLobbyPanel.SetActive(true);
            GlobalVariables.m_szPseudo = "test";
            networkView.RPC("SetPseudo", RPCMode.Server, "test");
        }
    }
    void OnDisconnectedFromServer(NetworkDisconnection _oInfo)
    {
        Debug.Log("Disconnected from server");
        if (Network.isClient)
        {
            m_MultiplayerJoinPanel.SetActive(true);
            m_MultiplayerLobbyPanel.SetActive(false);
            getServerList();
        }
    }
    public void ReadyButtonPressed()
    {
        m_bReady = !m_bReady;
        networkView.RPC("SetPlayerReady", RPCMode.All, GlobalVariables.m_iCurrentPlayerId - 1, m_bReady);
    }
    public void DisconnectFromServer()
    {
        if (Network.isClient)
        {
            Network.Disconnect();
        }
    }

    [RPC]
    void SetPlayerReady(int _iSlot, bool _bReady)
    {
        m_PlayerList[_iSlot].m_bReady = _bReady;
        m_MultiplayerLobby.SetPlayerReady(_iSlot, _bReady);
    }
    [RPC]
    void SetPlayerSlot(int _ioldSlot, int _iSlot, string _szPseudo, NetworkPlayer _oPlayer)
    {
        if (_ioldSlot > -1)
        {
            m_PlayerList[_ioldSlot] = null;
        }
        if(_iSlot > -1)
        {
            Player newPlayer = new Player();
            newPlayer.m_iID = _iSlot + 1;
            newPlayer.m_iTeam = _iSlot % 2 + 1;
            newPlayer.m_szPseudo = _szPseudo;
            newPlayer.m_bReady = false;
            if (Network.isServer)
            {
                newPlayer.m_oPlayer = _oPlayer;
            }
            m_PlayerList[_iSlot] = newPlayer;
        }
        m_MultiplayerLobby.SetPlayerSlot(_ioldSlot, _iSlot, _szPseudo);
    }
    public void PlayerSlotButtonPressed(int _iID)
    {
        if(Network.isClient)
        {
            m_bReady = false;
        }
        networkView.RPC("SetPlayerSlot", RPCMode.All, GlobalVariables.m_iCurrentPlayerId - 1, _iID, GlobalVariables.m_szPseudo, Network.player);
        GlobalVariables.m_iCurrentPlayerId = _iID + 1;
        GlobalVariables.m_iCurrentPlayerTeam = _iID % 2 + 1;
        if (Network.isServer)
        {
            networkView.RPC("SetPlayerReady", RPCMode.All, GlobalVariables.m_iCurrentPlayerId - 1, true);
        }
    }
}
