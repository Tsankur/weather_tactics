using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.EventSystems;

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

    //players
    private int m_iCurrentPlayerId = 1;
    private int m_iCurrentPlayerTeam = 1;
    private int m_iPlayerCount = 3;
    private int m_iCurrentTurnPlayerId = 0;
    private int m_iCurrentTurn = 0;
    private bool m_bAutoEndTurn = true;
    private int[] m_tPlayerTeams = { 0, 1, 2, 1 };


    // Use this for initialization
    void Start()
    {
        m_iCurrentPlayerId = GlobalVariables.m_iCurrentPlayerId;
        m_vCharacterList = new List<Character>();
        m_WorldMap.loadMap(GlobalVariables.m_szMapToLoad);
        m_vCharacterList.Add(m_WorldMap.instanciateCharacter(4, 7));
        m_vCharacterList.Add(m_WorldMap.instanciateCharacter(4, 5));
        m_vCharacterList.Add(m_WorldMap.instanciateCharacter(5, 7));
        m_vCharacterList[0].Init(10, 1, 1, m_vCharacterList, m_WorldMap);
        m_vCharacterList[1].Init(10, 2, 2, m_vCharacterList, m_WorldMap, true);
        m_vCharacterList[2].Init(10, 3, 1, m_vCharacterList, m_WorldMap);
        NextTurn();
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
        m_iCurrentPlayerId = m_iCurrentTurnPlayerId; // a commenter pour le multi cela sert au multi joueur local.
        m_iCurrentPlayerTeam = m_tPlayerTeams[m_iCurrentPlayerId];
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
                    if (gridElem.m_iLayer == 10)
                    {
                        m_SelectedChar.GetComponent<Character>().SetOverDestination(gridElem.m_iX, gridElem.m_iY);
                    }
                }
                if (!m_bCharacterMoving && m_iCurrentTurnPlayerId == m_iCurrentPlayerId)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        m_MouseDownElement = gridElem;
                    }
                    if (Input.GetMouseButtonUp(0))
                    {
                        if (m_MouseDownElement == gridElem)
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
                                    }
                                    m_SelectedChar = gridElem;
                                    m_SelectedRect.transform.position = new Vector3(gridElem.m_iX * 10, gridElem.m_iY * 10, -0.06f);
                                    m_SelectedRect.SetActive(true);
                                    if (m_iCurrentPlayerId == gridElem.GetComponent<Character>().m_iPlayerID)
                                    {
                                        if (!gridElem.GetComponent<Character>().IsTurnFinish())
                                        {
                                            m_SelectedChar.GetComponent<Character>().ShowDestinations();
                                            m_SelectedRect.renderer.material.color = Color.white;
                                        }
                                        else
                                        {
                                            m_SelectedChar = null;
                                            m_SelectedRect.SetActive(false);
                                        }
                                    }
                                    else
                                    {
                                        if (m_iCurrentPlayerTeam == gridElem.GetComponent<Character>().m_iTeam)
                                        {
                                            m_SelectedRect.renderer.material.color = Color.yellow;
                                        }
                                        else
                                        {
                                            m_SelectedRect.renderer.material.color = Color.red;
                                        }
                                    }
                                }
                                else
                                {
                                    if (m_SelectedChar != null)
                                    {
                                        m_SelectedChar.GetComponent<Character>().HideDestinations();
                                    }
                                    m_SelectedChar = null;
                                    m_SelectedRect.SetActive(false);
                                }
                            }
                            else
                            {
                                if (m_SelectedChar != null)
                                {
                                    m_SelectedChar.GetComponent<Character>().HideDestinations();
                                }
                                m_SelectedChar = null;
                                m_SelectedRect.SetActive(false);
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
                if(m_bCharacterMoving == false)
                {
                    m_SelectedRect.transform.position = new Vector3(m_SelectedChar.m_iX * 10, m_SelectedChar.m_iY * 10, -0.06f);
                    m_SelectedRect.SetActive(true);
                }
            }
            else
            {
                m_bCharacterMoving = false;
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            m_MouseDownElement = null;
        }
        if (m_SelectedChar != null)
        {
            if (m_SelectedChar.GetComponent<Character>().m_iPlayerID == m_iCurrentTurnPlayerId)
            {
                if (m_SelectedChar.GetComponent<Character>().IsTurnFinish())
                {
                    m_SelectedChar = null;
                    m_SelectedRect.SetActive(false);
                }
            }
        }
        if(m_bAutoEndTurn && m_iCurrentPlayerId == m_iCurrentTurnPlayerId)
        {
            bool bEndTurn = true;
            foreach (Character character in m_vCharacterList)
            {
                if(character.m_iPlayerID == m_iCurrentPlayerId)
                {
                    if(!character.IsTurnFinish())
                    {
                        bEndTurn = false;
                        break;
                    }
                }
            }
            if(bEndTurn)
            {
                NextTurn();
            }
        }
    }
}
