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

	// Use this for initialization
	void Start ()
    {
        m_vCharaterList = new List<Character>();
        m_WorldMap.loadMap(GlobalVariables.m_szMapToLoad);
        m_vCharaterList.Add(m_WorldMap.instanciateCharacter(4, 7));
        m_vCharaterList.Add(m_WorldMap.instanciateCharacter(4, 5));
        m_vCharaterList[0].Init(10);
        m_vCharaterList[0].ComputeAvailableDestinations(m_WorldMap, m_vCharaterList);
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfos;
                if (Physics.Raycast(ray, out hitInfos))
                {
                    GridElement gridElem = hitInfos.collider.gameObject.GetComponent<GridElement>();
                    if(gridElem.m_iLayer == 4)
                    {
                        if (m_SelectedChar != gridElem)
                        {
                            m_SelectedChar = gridElem;
                        }
                        else
                        {
                            m_SelectedChar = null;
                        }
                    }
                    else
                    {
                        m_SelectedChar = null;
                    }
                }
            }
        }
	}
}
