using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum Action { None, Attack, Skill1, Skill2, Skill3, Power };

public class game : MonoBehaviour
{
    // map related variables
    public GridElement m_SelectedChar = null;
    private GridElement m_MouseDownElement = null;
    public worldMap m_WorldMap;
    private List<Character> m_vCharacterList;
    bool m_bCharacterMoving = false;
    public GameObject m_OverRect;
    public GameObject m_SelectedRect;
    public Text m_PlayerTurn;
    public GameObject m_ActionPanel;
    public Toggle[] m_ActionToggles;
    public bool m_bActionSelected = false;

    //players
    private int m_iCurrentPlayerId = 1;
    private int m_iCurrentPlayerTeam = 1;
    private int m_iPlayerCount = 2;
    private int m_iCurrentTurnPlayerId = 0;
    private int m_iCurrentTurn = 0;
    private bool m_bAutoEndTurn = true;

    float m_fCumulativeMouseMove = 0;
    Vector3 m_vLastMousePos;



    // Use this for initialization
    void Start()
    {
        m_iCurrentPlayerId = GlobalVariables.m_iCurrentPlayerId;
        m_iCurrentPlayerTeam = GlobalVariables.m_iCurrentPlayerTeam;
        m_vCharacterList = new List<Character>();
        m_WorldMap.loadMap(GlobalVariables.m_szMapToLoad);
        m_iPlayerCount = m_WorldMap.GetPlayerCount();
        //Debug.Log(m_iPlayerCount);
        List<Spawn> spawns = m_WorldMap.GetSpawnList();
        foreach (Spawn spawn in spawns)
        {
            GridElement elem = spawn.GetComponent<GridElement>();
            if (Network.isServer)
            {
                NetworkViewID ViewID = Network.AllocateViewID();
                GetComponent<NetworkView>().RPC("AddCharacter", RPCMode.All, elem.m_iX, elem.m_iY, 10, spawn.m_iPlayerID, (spawn.m_iPlayerID - 1) % 2 + 1, ViewID);
            }
            else if (!Network.isClient)
            {
                AddCharacter(elem.m_iX, elem.m_iY, 10, spawn.m_iPlayerID, (spawn.m_iPlayerID - 1) % 2 + 1, new NetworkViewID());
            }
        }
        if (Network.isServer)
        {
            GetComponent<NetworkView>().RPC("NextTurn", RPCMode.All);
        }
        else if (!Network.isClient)
        {
            NextTurn();
        }
    }
    [RPC]
    void AddCharacter(int _iX, int _iY, int _iMouvementPoints, int _iPlayerID, int _iTeam, NetworkViewID _ViewID)
    {
        Character newCharacter = m_WorldMap.instanciateCharacter(_iX, _iY);
        newCharacter.Init(_iMouvementPoints, _iPlayerID, _iTeam, m_vCharacterList, m_WorldMap);
        m_vCharacterList.Add(newCharacter);
        newCharacter.GetComponent<NetworkView>().viewID = _ViewID;
    }
    [RPC]
    void NextTurn()
    {
        m_iCurrentTurnPlayerId++;
        if (m_iCurrentTurnPlayerId > m_iPlayerCount)
        {
            m_iCurrentTurnPlayerId = 1;
            m_iCurrentTurn++;
        }
        if (!Network.isClient && !Network.isServer)
        {
            m_iCurrentPlayerId = m_iCurrentTurnPlayerId;
            m_iCurrentPlayerTeam = (m_iCurrentPlayerId - 1) % 2 + 1;
        }
        if (m_iCurrentPlayerId == m_iCurrentTurnPlayerId)
        {
            foreach (Character character in m_vCharacterList)
            {
                if (character.m_iPlayerID == m_iCurrentPlayerId)
                {
                    character.ResetTurn();
                }
            }
        }
        m_PlayerTurn.text = m_iCurrentTurnPlayerId.ToString();
    }
    // Update is called once per frame
    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfos;
            if (Physics.Raycast(ray, out hitInfos))
            {
                GridElement gridElem = hitInfos.collider.gameObject.GetComponent<GridElement>();
                m_OverRect.transform.position = new Vector3(gridElem.m_iX * 10, gridElem.m_iY * 10, -0.06f);
                if (m_SelectedChar != null)
                {
                    if (gridElem.m_iLayer == 10 && !m_bActionSelected)
                    {
                        m_SelectedChar.GetComponent<Character>().SetOverDestination(gridElem.m_iX, gridElem.m_iY);
                    }
                }
                if (!m_bCharacterMoving && m_iCurrentTurnPlayerId == m_iCurrentPlayerId)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        m_MouseDownElement = gridElem;
                        m_vLastMousePos = Input.mousePosition;
                    }
                    if (Input.GetMouseButtonUp(0) && m_fCumulativeMouseMove < 10)
                    {
                        if (m_MouseDownElement == gridElem)
                        {
                            if (m_bActionSelected)
                            {
                                if (gridElem.m_iLayer == 10)
                                {
                                    if (m_SelectedChar != null)
                                    {
                                        m_SelectedChar.GetComponent<Character>().Act(gridElem.m_iX, gridElem.m_iY);
                                        m_SelectedRect.SetActive(false);
                                        m_bActionSelected = false;
                                    }
                                }
                            }
                            else
                            {
                                if (gridElem.m_iLayer == 10)
                                {
                                    if (m_SelectedChar != null)
                                    {
                                        m_bCharacterMoving = m_SelectedChar.GetComponent<Character>().SetDestination(gridElem.m_iX, gridElem.m_iY);
                                        m_SelectedRect.SetActive(false);
                                    }
                                }
                                else if (gridElem.m_iLayer == 4)
                                {
                                    if (m_SelectedChar != gridElem)
                                    {
                                        if (m_SelectedChar != null)
                                        {
                                            m_SelectedChar.GetComponent<Character>().HideDestinations();
                                            m_SelectedChar.GetComponent<Character>().Cancel();
                                            m_ActionPanel.SetActive(false);
                                        }
                                        m_SelectedChar = gridElem;
                                        m_SelectedRect.transform.position = new Vector3(gridElem.m_iX * 10, gridElem.m_iY * 10, -0.06f);
                                        m_SelectedRect.SetActive(true);
                                        if (m_iCurrentPlayerId == gridElem.GetComponent<Character>().m_iPlayerID)
                                        {
                                            if (!gridElem.GetComponent<Character>().IsTurnFinish())
                                            {
                                                m_SelectedChar.GetComponent<Character>().ShowDestinations();
                                                m_SelectedRect.GetComponent<Renderer>().material.color = Color.white;
                                            }
                                            else
                                            {
                                                m_SelectedChar = null;
                                                m_SelectedRect.SetActive(false);
                                                m_ActionPanel.SetActive(false);
                                            }
                                        }
                                        else
                                        {
                                            if (m_iCurrentPlayerTeam == gridElem.GetComponent<Character>().m_iTeam)
                                            {
                                                m_SelectedRect.GetComponent<Renderer>().material.color = Color.yellow;
                                            }
                                            else
                                            {
                                                m_SelectedRect.GetComponent<Renderer>().material.color = Color.red;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (m_SelectedChar != null)
                                        {
                                            if (m_SelectedChar.GetComponent<Character>().CanMove())
                                            {
                                                m_SelectedChar.GetComponent<Character>().EndMove();
                                                m_ActionPanel.SetActive(true);
                                                m_SelectedChar.GetComponent<Character>().HideDestinations();
                                                SetActions();
                                            }
                                            else
                                            {
                                                m_SelectedChar.GetComponent<Character>().Cancel();
                                                m_SelectedRect.transform.position = new Vector3(m_SelectedChar.m_iX * 10, m_SelectedChar.m_iY * 10, -0.06f);
                                                m_ActionPanel.SetActive(false);
                                                m_SelectedChar.GetComponent<Character>().ShowDestinations();
                                            }
                                        }
                                        //m_SelectedChar = null;
                                        //m_SelectedRect.SetActive(false);
                                    }
                                }
                                else
                                {
                                    if (m_SelectedChar != null)
                                    {
                                        m_SelectedChar.GetComponent<Character>().HideDestinations();
                                        m_SelectedChar.GetComponent<Character>().Cancel();
                                        m_ActionPanel.SetActive(false);
                                    }
                                    m_SelectedChar = null;
                                    m_SelectedRect.SetActive(false);
                                }
                            }
                        }
                    }
                }
            }
        }
        if (m_bCharacterMoving)
        {
            if (m_SelectedChar != null)
            {
                m_bCharacterMoving = m_SelectedChar.GetComponent<Character>().IsMoving();
                if (m_bCharacterMoving == false)
                {
                    m_SelectedRect.transform.position = new Vector3(m_SelectedChar.m_iX * 10, m_SelectedChar.m_iY * 10, -0.06f);
                    m_SelectedRect.SetActive(true);
                    m_ActionPanel.SetActive(true);
                    SetActions();
                }
            }
            else
            {
                m_bCharacterMoving = false;
            }
        }
        if (Input.GetMouseButton(0))
        {
            m_fCumulativeMouseMove += (m_vLastMousePos - Input.mousePosition).magnitude;
        }
        if (Input.GetMouseButtonUp(0))
        {
            m_fCumulativeMouseMove = 0;
            m_MouseDownElement = null;
        }
        if (Input.GetMouseButtonUp(1))
        {
            if (m_SelectedChar != null)
            {

                if (m_SelectedChar.GetComponent<Character>().m_iPlayerID == m_iCurrentPlayerId)
                {
                    m_SelectedChar.GetComponent<Character>().m_SelectableElem.SetActive(true);
                }
                if(m_bActionSelected)
                {
                    m_ActionPanel.SetActive(true);
                    m_bActionSelected = false;
                    m_SelectedChar.GetComponent<Character>().HideDestinations();
                }
                else
                {
                    m_SelectedChar.GetComponent<Character>().HideDestinations();
                    m_SelectedChar.GetComponent<Character>().Cancel();
                    m_SelectedRect.SetActive(false);
                    m_ActionPanel.SetActive(false);
                    m_SelectedChar = null;
                }
            }
        }
        if (m_SelectedChar != null)
        {
            if (m_SelectedChar.GetComponent<Character>().m_iPlayerID == m_iCurrentTurnPlayerId)
            {
                if (m_SelectedChar.GetComponent<Character>().IsTurnFinish())
                {
                    m_SelectedChar = null;
                    m_SelectedRect.SetActive(false);
                    m_ActionPanel.SetActive(false);
                }
            }
        }
        if (m_bAutoEndTurn && m_iCurrentPlayerId == m_iCurrentTurnPlayerId)
        {
            bool bEndTurn = true;
            foreach (Character character in m_vCharacterList)
            {
                if (character.m_iPlayerID == m_iCurrentPlayerId)
                {
                    if (!character.IsTurnFinish())
                    {
                        bEndTurn = false;
                        break;
                    }
                }
            }
            if (bEndTurn)
            {
                if (Network.isServer || Network.isClient)
                {
                    GetComponent<NetworkView>().RPC("NextTurn", RPCMode.All);
                }
                else
                {
                    NextTurn();
                }
            }
        }
    }

    //actions
    void SetActions()
    {
        Character selectedChar = m_SelectedChar.GetComponent<Character>();
        m_ActionPanel.GetComponent<ActionPanel>().SetPossibleActions(selectedChar.getActions(), selectedChar);
    }

    //interface events
    public void SetAutoEndTurn(bool value)
    {
        m_bAutoEndTurn = value;
    }
    public void NextButtonClic()
    {
        if (m_iCurrentPlayerId == m_iCurrentTurnPlayerId && !m_bCharacterMoving)
        {
            if (m_SelectedChar != null)
            {
                m_SelectedChar.GetComponent<Character>().Cancel();
                m_SelectedChar.GetComponent<Character>().HideDestinations();
                m_SelectedChar = null;
                m_SelectedRect.SetActive(false);
                m_ActionPanel.SetActive(false);
            }
            if (Network.isServer || Network.isClient)
            {
                GetComponent<NetworkView>().RPC("NextTurn", RPCMode.All);
            }
            else
            {
                NextTurn();
            }
        }
    }
    /*public void Act(Action _Action)
    {
        Debug.Log(_Action);
    }*/
    public void SetActionSelected()
    {
        m_bActionSelected = true;
        m_ActionPanel.SetActive(false);
    }
    public void CancelButtonClic()
    {
        if (m_SelectedChar != null)
        {
            m_SelectedChar.GetComponent<Character>().Cancel();
            m_SelectedRect.transform.position = new Vector3(m_SelectedChar.m_iX * 10, m_SelectedChar.m_iY * 10, -0.06f);
            m_ActionPanel.SetActive(false);
            m_SelectedChar.GetComponent<Character>().ShowDestinations();
        }
    }
    public void WaitButtonClic()
    {
        if (m_SelectedChar != null)
        {
            m_SelectedChar.GetComponent<Character>().Wait();
            m_SelectedRect.SetActive(false);
            m_ActionPanel.SetActive(false);
            m_SelectedChar = null;
        }
    }
}
