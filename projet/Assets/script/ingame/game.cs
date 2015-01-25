using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.EventSystems;

public class game : MonoBehaviour
{
    // map related variables
    public GridElement m_SelectedChar;
    public worldMap m_WorldMap;
    private List<Character> m_vCharaterList;
    bool m_bCharacterMoving = false;

    // Use this for initialization
    void Start()
    {
        m_vCharaterList = new List<Character>();
        m_WorldMap.loadMap(GlobalVariables.m_szMapToLoad);
        m_vCharaterList.Add(m_WorldMap.instanciateCharacter(4, 7));
        m_vCharaterList.Add(m_WorldMap.instanciateCharacter(4, 5));
        m_vCharaterList[0].Init(10);
        m_vCharaterList[0].ComputeAvailableDestinations(m_WorldMap, m_vCharaterList);
        m_vCharaterList[1].Init(10, true);
        m_vCharaterList[1].ComputeAvailableDestinations(m_WorldMap, m_vCharaterList);
    }

    // Update is called once per frame
    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (!m_bCharacterMoving)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hitInfos;
                    if (Physics.Raycast(ray, out hitInfos))
                    {
                        GridElement gridElem = hitInfos.collider.gameObject.GetComponent<GridElement>();
                        if (gridElem.m_iLayer == 10)
                        {
                            if (m_SelectedChar != null)
                            {
                                m_bCharacterMoving = m_SelectedChar.GetComponent<Character>().SetDestination(gridElem.m_iX, gridElem.m_iY);
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
                                m_SelectedChar.GetComponent<Character>().ShowDestinations(m_vCharaterList);
                            }
                            else
                            {
                                if (m_SelectedChar != null)
                                {
                                    m_SelectedChar.GetComponent<Character>().HideDestinations();
                                }
                                m_SelectedChar = null;
                            }
                        }
                        else
                        {
                            if (m_SelectedChar != null)
                            {
                                m_SelectedChar.GetComponent<Character>().HideDestinations();
                            }
                            m_SelectedChar = null;
                        }
                    }
                }
                if (m_SelectedChar != null)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hitInfos;
                    if (Physics.Raycast(ray, out hitInfos, 1000, (1 << 8)))
                    {
                        GridElement gridElem = hitInfos.collider.gameObject.GetComponent<GridElement>();
                        m_SelectedChar.GetComponent<Character>().SetOverDestination(gridElem.m_iX, gridElem.m_iY);
                    }
                }
            }
            else
            {
                if(m_SelectedChar != null)
                {
                    m_bCharacterMoving = m_SelectedChar.GetComponent<Character>().IsMoving();
                }
                else
                {
                    m_bCharacterMoving = false;
                }
            }
        }
    }
}
