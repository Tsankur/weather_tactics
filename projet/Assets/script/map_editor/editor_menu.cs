using UnityEngine;
using UnityEngine.UI;
//using UnityEditor;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class editor_menu : MonoBehaviour
{
    public Camera m_MainCamera;

    public Image m_SelectedTool;
    private int m_iSelectedToolId = 1;
    private int m_iUpdateToolId = 1;
    public Material[] m_vMaterials;
    public Material m_SelectedMaterial;

    public GameObject m_GridHolder;
    public GameObject m_GridElementPrefab;
    public GameObject m_SpawnPrefab;
    public GameObject m_OverRect;
    public GameObject m_SelectedRect;
    public InputField m_WidthInput;
    public InputField m_HeightInput;
    private int m_iWidth = 0;
    private int m_iHeight = 0;
    private int m_iNewWidth = 10;
    private int m_iNewHeight = 10;
    private GameObject[,] m_tTerrainGridElements;
    private GameObject[,] m_tRiverGridElements;
    private GameObject[,] m_tConstructionGridElements;
    private GameObject[,] m_tItemGridElements;
    private int[,] m_tTerrainGridElementValues;
    private int[,] m_tRiverGridElementValues;
    private int[,] m_tConstructionGridElementValues;
    private int[,] m_tItemGridElementValues;
    public GameObject m_SpanwOptionPopup;
    List<Spawn> m_tSpawns = new List<Spawn>();
    public GameObject m_SelectedSpawn;

    private bool m_bReplaceLevel = false;
    public InputField m_FileNameInput;
    public GameObject m_OveridePopup;
    public GameObject m_LevelListPanel;
    public GameObject m_LevelNamePrefab;
    public Toggle[] m_LevelPlayerCountToggles;

    public GameObject m_ServerListPanel;
    public GameObject m_StartServerButton;
    public GameObject m_StopServerButton;
    public GameObject m_DisconnectFromServerButton;
    private bool m_bWaitingForServerList = false;

    // Use this for initialization
    void Start()
    {
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
        if (!m_OveridePopup.activeSelf && !m_SpanwOptionPopup.activeSelf)
        {
#if UNITY_IPHONE || UNITY_ANDROID
            if (Input.touchCount == 1)
            {
                Touch touchZero = Input.GetTouch(0);
                if (touchZero.phase == TouchPhase.Began && !Camera.main.GetComponent<editor_camera>().isInInterface(touchZero.position))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hitInfos;
                    if (Physics.Raycast(ray, out hitInfos))
                    {
                        GridElement gridElem = hitInfos.collider.gameObject.GetComponent<GridElement>();
                        m_OverRect.transform.position = new Vector3(gridElem.m_iX * 10, gridElem.m_iY * 10, -0.06f);
                        if (Network.isClient || Network.isServer)
                        {
                            networkView.RPC("SetGridElementMaterial", RPCMode.All, gridElem.m_iX, gridElem.m_iY, m_iSelectedToolId, true, 0);
                        }
                        else
                        {
                            SetGridElementMaterial(gridElem.m_iX, gridElem.m_iY, m_iSelectedToolId, true, 0);
                        }
                    }
                }
            }
#else
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfos;
                if (Physics.Raycast(ray, out hitInfos))
                {
                    GridElement gridElem = hitInfos.collider.gameObject.GetComponent<GridElement>();
                    m_OverRect.transform.position = new Vector3(gridElem.m_iX * 10, gridElem.m_iY * 10, -0.06f);
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (m_iSelectedToolId < 18)
                        {
                            if (Network.isClient || Network.isServer)
                            {
                                networkView.RPC("SetGridElementMaterial", RPCMode.All, gridElem.m_iX, gridElem.m_iY, m_iSelectedToolId, true, 0);
                            }
                            else
                            {
                                SetGridElementMaterial(gridElem.m_iX, gridElem.m_iY, m_iSelectedToolId, true, 0);
                            }
                        }
                        if (m_iSelectedToolId == 18)
                        {
                            if (gridElem.m_iLayer == 5)
                            {
                                m_SelectedSpawn = gridElem.gameObject;
                                Spawn spawn = gridElem.GetComponent<Spawn>();
                                m_SpanwOptionPopup.GetComponent<SpawnOption>().SetSelectedPlayer(spawn.m_iPlayerID);
                            }
                            else
                            {
                                if (Network.isClient || Network.isServer)
                                {
                                    networkView.RPC("CreateSpawn", RPCMode.Others, gridElem.m_iX, gridElem.m_iY, false);
                                }
                                CreateSpawn(gridElem.m_iX, gridElem.m_iY, true);
                                m_SpanwOptionPopup.GetComponent<SpawnOption>().SetSelectedPlayer(1);
                            }
                            m_SelectedRect.transform.position = new Vector3(gridElem.m_iX * 10, gridElem.m_iY * 10, -0.06f);
                            m_SelectedRect.SetActive(true);
                            m_SpanwOptionPopup.SetActive(true);
                        }
                    }
                    if (Input.GetMouseButton(0) && m_iSelectedToolId < 18)
                    {
                        if (Network.isClient || Network.isServer)
                        {
                            networkView.RPC("SetGridElementMaterial", RPCMode.All, gridElem.m_iX, gridElem.m_iY, m_iSelectedToolId, false, 0);
                        }
                        else
                        {
                            SetGridElementMaterial(gridElem.m_iX, gridElem.m_iY, m_iSelectedToolId, false, 0);
                        }
                    }
                }
            }

#endif
        }
        if (m_bWaitingForServerList)
        {
            m_bWaitingForServerList = !ShowServerList();
        }
    }

    //Level Edition
    [RPC]
    void SetGridElementMaterial(int x, int y, int id, bool _bRotate, int _iRotation)
    {
        if (id == 12)
        {
            if (m_tConstructionGridElements[x, y] != null)
            {
                GameObject.Destroy(m_tConstructionGridElements[x, y]);
                m_tConstructionGridElementValues[x, y] = 0;
            }
        }
        else if (id == 17)
        {
            if (m_tRiverGridElements[x, y] != null)
            {
                GameObject.Destroy(m_tRiverGridElements[x, y]);
                m_tRiverGridElementValues[x, y] = 0;
            }
        }
        else if (id >= 8 && id <= 11)
        {
            if (m_tTerrainGridElementValues[x, y] != 2)
            {
                if (m_tConstructionGridElements[x, y] == null)
                {
                    GameObject newGridElement = (GameObject)Instantiate(m_GridElementPrefab, new Vector3(x * 10, y * 10, -0.02f), Quaternion.identity);
                    newGridElement.GetComponent<GridElement>().Init(x, y, 2);
                    newGridElement.transform.SetParent(m_GridHolder.transform);
                    newGridElement.transform.Rotate(Vector3.forward, _iRotation);

                    m_tConstructionGridElements[x, y] = newGridElement;
                }
                if (m_tConstructionGridElementValues[x, y] == id)
                {
                    if (_bRotate)
                    {
                        m_tConstructionGridElements[x, y].transform.Rotate(Vector3.forward, -90);
                    }
                }
                else
                {
                    m_tConstructionGridElements[x, y].GetComponent<Renderer>().material = m_vMaterials[id];
                    m_tConstructionGridElementValues[x, y] = id;
                }
            }
        }
        else if (id >= 13 && id <= 16)
        {
            if (m_tTerrainGridElementValues[x, y] != 2)
            {
                if (m_tRiverGridElements[x, y] == null)
                {
                    GameObject newGridElement = (GameObject)Instantiate(m_GridElementPrefab, new Vector3(x * 10, y * 10, -0.01f), Quaternion.identity);
                    newGridElement.GetComponent<GridElement>().Init(x, y, 1);
                    newGridElement.transform.SetParent(m_GridHolder.transform);
                    newGridElement.transform.Rotate(Vector3.forward, _iRotation);

                    m_tRiverGridElements[x, y] = newGridElement;
                }
                if (m_tRiverGridElementValues[x, y] == id)
                {
                    if (_bRotate)
                    {
                        m_tRiverGridElements[x, y].transform.Rotate(Vector3.forward, -90);
                    }
                }
                else
                {
                    m_tRiverGridElements[x, y].GetComponent<Renderer>().material = m_vMaterials[id];
                    m_tRiverGridElementValues[x, y] = id;
                }
            }
        }
        else if(id > 17)
        {

        }
        else
        {
            m_tTerrainGridElements[x, y].GetComponent<Renderer>().material = m_vMaterials[id];
            m_tTerrainGridElementValues[x, y] = id;
            if (id == 2)
            {
                if (m_tConstructionGridElements[x, y] != null)
                {
                    GameObject.Destroy(m_tConstructionGridElements[x, y]);
                    m_tConstructionGridElementValues[x, y] = 0;
                }
                if (m_tRiverGridElements[x, y] != null)
                {
                    GameObject.Destroy(m_tRiverGridElements[x, y]);
                    m_tRiverGridElementValues[x, y] = 0;
                }
            }
        }
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
    void SetGridSize(int _iWidth, int _iHeight, int _iUpdateToolId)
    {
        m_iUpdateToolId = _iUpdateToolId;
        m_WidthInput.text = _iWidth.ToString();
        m_HeightInput.text = _iHeight.ToString();
        m_iNewWidth = _iWidth;
        m_iNewHeight = _iHeight;
    }
    public void changeSelected(Image _oClickedButton)
    {
        m_SelectedTool.color = _oClickedButton.color;
        m_SelectedTool.sprite = _oClickedButton.sprite;
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
        m_iUpdateToolId = m_iSelectedToolId;
        if (Network.isClient || Network.isServer)
        {
            networkView.RPC("SetGridSize", RPCMode.Others, m_iNewWidth, m_iNewHeight, m_iSelectedToolId);
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
        // old arrays
        GameObject[,] oldTerrainGridElements = m_tTerrainGridElements;
        GameObject[,] oldRiverGridElements = m_tRiverGridElements;
        GameObject[,] oldConstructionGridElements = m_tConstructionGridElements;
        GameObject[,] oldItemGridElements = m_tItemGridElements;

        int[,] oldTerrainGridElementValues = m_tTerrainGridElementValues;
        int[,] oldRiverGridElementValues = m_tRiverGridElementValues;
        int[,] oldConstructionGridElementValues = m_tConstructionGridElementValues;
        int[,] oldItemGridElementValues = m_tItemGridElementValues;
        // new arrays
        m_tTerrainGridElements = new GameObject[m_iNewWidth, m_iNewHeight];
        m_tRiverGridElements = new GameObject[m_iNewWidth, m_iNewHeight];
        m_tConstructionGridElements = new GameObject[m_iNewWidth, m_iNewHeight];
        m_tItemGridElements = new GameObject[m_iNewWidth, m_iNewHeight];

        m_tTerrainGridElementValues = new int[m_iNewWidth, m_iNewHeight];
        m_tRiverGridElementValues = new int[m_iNewWidth, m_iNewHeight];
        m_tConstructionGridElementValues = new int[m_iNewWidth, m_iNewHeight];
        m_tItemGridElementValues = new int[m_iNewWidth, m_iNewHeight];

        int maxWidth = Mathf.Max(m_iWidth, m_iNewWidth);
        int maxHeight = Mathf.Max(m_iHeight, m_iNewHeight);
        for (int i = 0; i < maxWidth; i++)
        {
            for (int j = 0; j < maxHeight; j++)
            {
                if (i >= m_iNewWidth || j >= m_iNewHeight)
                {
                    GameObject.Destroy(oldTerrainGridElements[i, j]);
                    if (oldRiverGridElementValues[i, j] > 0)
                    {
                        GameObject.Destroy(oldRiverGridElements[i, j]);
                    }
                    if (oldConstructionGridElementValues[i, j] > 0)
                    {
                        GameObject.Destroy(oldConstructionGridElements[i, j]);
                    }
                    if (oldItemGridElementValues[i, j] > 0)
                    {
                        GameObject.Destroy(oldItemGridElements[i, j]);
                    }
                    if (Network.isClient || Network.isServer)
                    {
                        networkView.RPC("DeleteSpawn", RPCMode.All, i, j);
                    }
                    else
                    {
                        DeleteSpawn(i, j);
                    }
                }
                else if (i >= m_iWidth || j >= m_iHeight)
                {
                    GameObject newGridElement = (GameObject)Instantiate(m_GridElementPrefab, new Vector3(i * 10, j * 10), Quaternion.identity);
                    newGridElement.GetComponent<GridElement>().Init(i, j, 0);
                    newGridElement.transform.SetParent(m_GridHolder.transform);

                    m_tTerrainGridElements[i, j] = newGridElement;
                    m_tRiverGridElements[i, j] = null;
                    m_tConstructionGridElements[i, j] = null;
                    m_tItemGridElements[i, j] = null;

                    m_tTerrainGridElementValues[i, j] = 1;
                    m_tRiverGridElementValues[i, j] = 0;
                    m_tConstructionGridElementValues[i, j] = 0;
                    m_tItemGridElementValues[i, j] = 0;
                    SetGridElementMaterial(i, j, m_iUpdateToolId, false, 0);
                }
                else
                {
                    m_tTerrainGridElements[i, j] = oldTerrainGridElements[i, j];
                    m_tRiverGridElements[i, j] = oldRiverGridElements[i, j];
                    m_tConstructionGridElements[i, j] = oldConstructionGridElements[i, j];
                    m_tItemGridElements[i, j] = oldItemGridElements[i, j];

                    m_tTerrainGridElementValues[i, j] = oldTerrainGridElementValues[i, j];
                    m_tRiverGridElementValues[i, j] = oldRiverGridElementValues[i, j];
                    m_tConstructionGridElementValues[i, j] = oldConstructionGridElementValues[i, j];
                    m_tItemGridElementValues[i, j] = oldItemGridElementValues[i, j];
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
        if (iHeight > 0 && iHeight <= 200)
        {
            m_iNewHeight = iHeight;
        }
    }
    public void changeWidth(string _sValue)
    {
        int iWidth = int.Parse('0' + _sValue);
        if (iWidth > 0 && iWidth <= 200)
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
        m_tSpawns.Clear();
        m_iWidth = 0;
        m_iHeight = 0;
        m_FileNameInput.text = "";
        updateGrid();
    }
    [RPC]
    void CreateSpawn(int _iX, int _iY, bool _AmICreator)
    {
        GameObject newGridElement = (GameObject)Instantiate(m_SpawnPrefab, new Vector3(_iX * 10, _iY * 10, -0.03f), Quaternion.identity);
        newGridElement.GetComponent<GridElement>().Init(_iX, _iY, 5);
        newGridElement.transform.SetParent(m_GridHolder.transform);
        m_tSpawns.Add(newGridElement.GetComponent<Spawn>());
        if(_AmICreator)
        {
            m_SelectedSpawn = newGridElement;

        }
    }
    public void DeleteSpawnButtonClicked()
    {
        if (m_SelectedSpawn != null)
        {
            GridElement elem = m_SelectedSpawn.GetComponent<GridElement>();
            if (Network.isServer || Network.isClient)
            {
                networkView.RPC("DeleteSpawn", RPCMode.All, elem.m_iX, elem.m_iY);
            }
            else
            {
                DeleteSpawn(elem.m_iX, elem.m_iY);
            }
            m_SelectedSpawn = null;
        }
        m_SpanwOptionPopup.SetActive(false);
        m_SelectedRect.SetActive(false);
    }
    [RPC]
    void DeleteSpawn(int _iX, int _iY)
    {
        Spawn spawnToDelete = null;
        foreach (Spawn spawn in m_tSpawns)
        {
            GridElement elem = spawn.GetComponent<GridElement>();
            if(elem.m_iX == _iX && elem.m_iY == _iY)
            {
                spawnToDelete = spawn;
                break;
            }
        }
        if (spawnToDelete != null)
        {
            m_tSpawns.Remove(spawnToDelete);
            GameObject.Destroy(spawnToDelete.gameObject);
        }
    }
    public void ApplySpawnOtionClicked()
    {
        int iPlayerId = m_SpanwOptionPopup.GetComponent<SpawnOption>().GetSelectedPlayer();
        if(m_SelectedSpawn != null)
        {
            GridElement elem = m_SelectedSpawn.GetComponent<GridElement>();
            if (Network.isServer || Network.isClient)
            {
                networkView.RPC("UpdateSpawn", RPCMode.All, elem.m_iX, elem.m_iY, iPlayerId);
            }
            else
            {
                UpdateSpawn(elem.m_iX, elem.m_iY, iPlayerId);
            }
            m_SelectedSpawn = null;
        }
        m_SpanwOptionPopup.SetActive(false);
        m_SelectedRect.SetActive(false);
    }
    [RPC]
    void UpdateSpawn(int _iX, int _iY, int _iPlayerID)
    {
        foreach(Spawn spawn in m_tSpawns)
        {
            GridElement elem = spawn.GetComponent<GridElement>();
            if(elem.m_iX == _iX && elem.m_iY == _iY)
            {
                spawn.m_iPlayerID = _iPlayerID;
            }
        }
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
    int GetMaxPlayerCount()
    {
        for (int i = 0; i < m_LevelPlayerCountToggles.Length; i++)
        {
            if (m_LevelPlayerCountToggles[i].isOn)
            {
                return (i+1) *2;
            }
        }
        return 0;
    }
    int SetMaxPlayerCount(int _iMaxPlayerCount)
    {
        for (int i = 0; i < m_LevelPlayerCountToggles.Length; i++)
        {
            if (_iMaxPlayerCount == (i+1)*2)
            {
                m_LevelPlayerCountToggles[i].isOn = true;
            }
            else
            {
                m_LevelPlayerCountToggles[i].isOn = false;
            }
        }
        return 0;
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
                    bw.Write(GetMaxPlayerCount());
                    for (int i = 0; i < m_iWidth; i++)
                    {
                        for (int j = 0; j < m_iHeight; j++)
                        {
                            bw.Write(m_tTerrainGridElementValues[i, j]);
                            bw.Write(m_tRiverGridElementValues[i, j]);
                            if (m_tRiverGridElements[i, j] != null)
                            {
                                bw.Write((int)(m_tRiverGridElements[i, j].transform.rotation.eulerAngles.z));
                            }
                            else
                            {
                                bw.Write(0);
                            }
                            bw.Write(m_tConstructionGridElementValues[i, j]);
                            if (m_tConstructionGridElements[i, j] != null)
                            {
                                bw.Write((int)(m_tConstructionGridElements[i, j].transform.rotation.eulerAngles.z));
                            }
                            else
                            {
                                bw.Write(0);
                            }
                            bw.Write(m_tItemGridElementValues[i, j]);
                        }
                    }
                    bw.Write(m_tSpawns.Count);
                    foreach(Spawn spawn in m_tSpawns)
                    {
                        GridElement elem = spawn.GetComponent<GridElement>();
                        bw.Write(elem.m_iX);
                        bw.Write(elem.m_iY);
                        bw.Write(spawn.m_iPlayerID);
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
                int iPlayerCount = br.ReadInt32();
                SetMaxPlayerCount(iPlayerCount);
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
                        int iTerrainMaterialId = br.ReadInt32();
                        int iRiverMaterialId = br.ReadInt32();
                        int iRiverRotation = br.ReadInt32();
                        int iConstructionMaterialId = br.ReadInt32();
                        int iConstructionRotation = br.ReadInt32();
                        int iItemMaterialId = br.ReadInt32();
                        if(Network.isServer || Network.isClient)
                        {
                            networkView.RPC("SetGridElementMaterial", RPCMode.All, i, j, iTerrainMaterialId, false , 0);
                            if(iRiverMaterialId != 0)
                            {
                                networkView.RPC("SetGridElementMaterial", RPCMode.All, i, j, iRiverMaterialId, false, iRiverRotation);
                            }
                            if (iConstructionMaterialId != 0)
                            {
                                networkView.RPC("SetGridElementMaterial", RPCMode.All, i, j, iConstructionMaterialId, false, iConstructionRotation);
                            }
                            if (iItemMaterialId != 0)
                            {
                                networkView.RPC("SetGridElementMaterial", RPCMode.All, i, j, iItemMaterialId, false, 0);
                            }
                        }
                        else
                        {
                            SetGridElementMaterial(i, j, iTerrainMaterialId, false , 0);
                            if (iRiverMaterialId != 0)
                            {
                                SetGridElementMaterial(i, j, iRiverMaterialId, false, iRiverRotation);
                            }
                            if (iConstructionMaterialId != 0)
                            {
                                SetGridElementMaterial(i, j, iConstructionMaterialId, false, iConstructionRotation);
                            }
                            if (iItemMaterialId != 0)
                            {
                                SetGridElementMaterial(i, j, iItemMaterialId, false , 0);
                            }
                        }
                    }
                }
                int iSpawnCount = br.ReadInt32();
                for (int i = 0; i < iSpawnCount; i++)
                {
                    int iX = br.ReadInt32();
                    int iY = br.ReadInt32();
                    int iPlayerID = br.ReadInt32();
                    if (Network.isServer || Network.isClient)
                    {
                        networkView.RPC("CreateSpawn", RPCMode.All, iX, iY, false);
                        networkView.RPC("UpdateSpawn", RPCMode.All, iX, iY, iPlayerID);
                    }
                    else
                    {
                        CreateSpawn(iX, iY, false);
                        UpdateSpawn(iX, iY, iPlayerID);
                    }
                }
            }
            catch (IOException e)
            {
                Debug.Log(e.Message + "\n Cannot read file.");
                return;
            }
            br.Close();
            m_FileNameInput.text = _sMapName;
        }
    }

    // network management (server)
    public void startServer()
    {
        if (!Network.isClient)
        {
            Network.InitializeServer(5, 6000, true);
            MasterServer.RegisterHost("WeatherTacticsEditor", "Tsan editor", "trololol");
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
                networkView.RPC("SetGridElementMaterial", _oPlayer, i, j, m_tTerrainGridElementValues[i, j], false, 0);
                if (m_tRiverGridElementValues[i, j] != 0 && m_tRiverGridElements[i, j] != null)
                {
                    networkView.RPC("SetGridElementMaterial", _oPlayer, i, j, m_tRiverGridElementValues[i, j], false, (int)m_tRiverGridElements[i, j].transform.rotation.eulerAngles.z);
                }
                if (m_tConstructionGridElementValues[i, j] != 0 && m_tConstructionGridElements[i, j] != null)
                {
                    networkView.RPC("SetGridElementMaterial", _oPlayer, i, j, m_tConstructionGridElementValues[i, j], false, (int)m_tConstructionGridElements[i, j].transform.rotation.eulerAngles.z);
                }
                if (m_tItemGridElementValues[i, j] != 0 && m_tItemGridElements[i, j] != null)
                {
                    networkView.RPC("SetGridElementMaterial", _oPlayer, i, j, m_tItemGridElementValues[i, j], false, 0);
                }
            }
        }
        foreach (Spawn spawn in m_tSpawns)
        {
            GridElement elem = spawn.GetComponent<GridElement>();
            int iX = elem.m_iX;
            int iY = elem.m_iY;
            networkView.RPC("CreateSpawn", _oPlayer, iX, iY, false);
            networkView.RPC("UpdateSpawn", _oPlayer, iX, iY, spawn.m_iPlayerID);
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
        MasterServer.RequestHostList("WeatherTacticsEditor");
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
