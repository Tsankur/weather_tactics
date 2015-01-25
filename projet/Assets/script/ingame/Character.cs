﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Destination
{
    public int m_iX;
    public int m_iY;
    public Destination m_Previous = null;
    public int m_iMouvementPoints;
    //public bool m_bLock = false;
    public Destination(int _iX, int _iY, int _iMouvementPoints, Destination _Previous = null)
    {
        m_iX = _iX;
        m_iY = _iY;
        m_iMouvementPoints = _iMouvementPoints;
        m_Previous = _Previous;
    }
    public static bool operator ==(Destination _first, Destination _second)
    {
        // If both are null, or both are same instance, return true.
        if (System.Object.ReferenceEquals(_first, _second))
        {
            return true;
        }

        // If one is null, but not both, return false.
        if (((object)_first == null) || ((object)_second == null))
        {
            return false;
        }
        return (_first.m_iX == _second.m_iX && _first.m_iY == _second.m_iY);
    }
    public static bool operator !=(Destination _first, Destination _second)
    {
        // If both are null, or both are same instance, return true.
        if (System.Object.ReferenceEquals(_first, _second))
        {
            return false;
        }

        // If one is null, but not both, return false.
        if (((object)_first == null) || ((object)_second == null))
        {
            return true;
        }
        return !(_first.m_iX == _second.m_iX && _first.m_iY == _second.m_iY);
    }
    public static bool operator >(Destination _first, Destination _second)
    {
        return _first.m_iMouvementPoints > _second.m_iMouvementPoints;
    }
    public static bool operator <(Destination _first, Destination _second)
    {
        return _first.m_iMouvementPoints < _second.m_iMouvementPoints;
    }
    public override string ToString()
    {
        return "Destination : x=" + m_iX + " y=" + m_iY + " MP=" + m_iMouvementPoints;
    }

    // Override the Object.Equals(object o) method:
    public override bool Equals(object o)
    {
        try
        {
            return (bool)(this == (Destination)o);
        }
        catch
        {
            return false;
        }
    }
    // Override the Object.GetHashCode() method:
    public override int GetHashCode()
    {
        return m_iX * 1000 + m_iY;
    }
}

public class Character : MonoBehaviour
{
    public GridElement m_GridElement;
    public Transform m_DestinationHolder;
    public GameObject m_DestinationPrefab;
    public Transform m_PathHolder;
    public GameObject[] m_PathPrefabs;
    List<Destination> m_vAvailableDestinations;
    int m_iMouvementPoints = 0;
    bool m_bNavigation = false;
    bool m_bSwimming = false;
    bool m_bClimbing = false;
    bool m_bMovementDone = false;

    // movement
    bool m_bMoving = false;
    private float m_fSpeed = 2.0f;
    private float m_fStartTime;
    private Vector3 m_vStartPosition;
    private Vector3 m_vEndPosition;
    Destination m_OverDestination = null;
    Destination m_Destination = null;
    Stack<Destination> m_PathToDestination = new Stack<Destination>();
    Destination m_CurrentPathDestination = null;

    public bool IsMoving()
    {
        return m_bMoving;
    }
    void Update()
    {
        if(m_bMoving)
        {
            float t = (Time.time - m_fStartTime) * m_fSpeed;
            transform.position = Vector3.Lerp(m_vStartPosition, m_vEndPosition, t);
            if (t >= 1.0f)
            {
                if(m_PathToDestination.Count > 0)
                {
                    Debug.Log("Next Destination, rest : " + m_PathToDestination.Count);
                    m_CurrentPathDestination = m_PathToDestination.Pop();
                    m_fStartTime = Time.time;
                    m_vStartPosition = transform.position;
                    m_vEndPosition = new Vector3(m_CurrentPathDestination.m_iX * 10, m_CurrentPathDestination.m_iY * 10, -0.05f);
                }
                else
                {
                    GetComponent<GridElement>().m_iX = m_CurrentPathDestination.m_iX;
                    GetComponent<GridElement>().m_iY = m_CurrentPathDestination.m_iY;
                    m_bMoving = false;
                    m_CurrentPathDestination = null;
                    m_bMovementDone = true;
                }
            }
        }
    }
    public void Init(int _iMouvementPoints, bool _bSwimming = false, bool _bClimbing = false, bool _bNavigation = false)
    {
        m_iMouvementPoints = _iMouvementPoints;
        m_bSwimming = _bSwimming;
        m_bClimbing = _bClimbing;
        m_bNavigation = _bNavigation;
        m_vAvailableDestinations = new List<Destination>();
    }

    public void ComputeAvailableDestinations(worldMap _worldMap, List<Character> _vCharacterList)
    {
        float beginTime = Time.realtimeSinceStartup;
        m_vAvailableDestinations.Clear();
        Destination Origin = new Destination(m_GridElement.m_iX, m_GridElement.m_iY, m_iMouvementPoints);
        List<Destination> vOldDestinations = new List<Destination>();
        vOldDestinations.Add(Origin);
        int[,] _tTerrainInfos = _worldMap.GetTerrainInfos();
        int iMapLimitX = _worldMap.GetMapWidth();
        int iMapLimitY = _worldMap.GetMapHeight();
        while (vOldDestinations.Count > 0)
        {
            List<Destination> vNewDestinations = new List<Destination>();
            foreach (Destination dest in vOldDestinations)
            {
                for (int i = 0; i < 4; i++)
                {
                    int iNewX = dest.m_iX;
                    int iNewY = dest.m_iY;
                    bool ok = false;
                    if (i == 0)
                    {
                        if (dest.m_iX > 0)
                        {
                            iNewX = dest.m_iX - 1;
                            ok = true;
                        }
                    }
                    else if (i == 1)
                    {
                        if (dest.m_iX < iMapLimitX - 1)
                        {
                            iNewX = dest.m_iX + 1;
                            ok = true;
                        }
                    }
                    else if (i == 2)
                    {
                        if (dest.m_iY > 0)
                        {
                            iNewY = dest.m_iY - 1;
                            ok = true;
                        }
                    }
                    else if (i == 3)
                    {
                        if (dest.m_iY < iMapLimitY - 1)
                        {
                            iNewY = dest.m_iY + 1;
                            ok = true;
                        }
                    }
                    if (ok)
                    {
                        Destination newDestination = new Destination(iNewX, iNewY, 0, dest);
                        if (newDestination != dest.m_Previous)
                        {
                            int iTerrainType = _tTerrainInfos[iNewX, iNewY];
                            int iMPRest = computeCost(iTerrainType, dest.m_iMouvementPoints, _worldMap);
                            newDestination.m_iMouvementPoints = iMPRest;
                            if (iMPRest >= 0)
                            {
                                Destination EquivalentDestination = m_vAvailableDestinations.Find(destination => destination == newDestination);
                                if (EquivalentDestination != null)
                                {
                                    if (newDestination > EquivalentDestination)
                                    {
                                        m_vAvailableDestinations.Remove(EquivalentDestination);
                                        m_vAvailableDestinations.Add(newDestination);
                                        vNewDestinations.Add(newDestination);
                                    }
                                }
                                else
                                {
                                    m_vAvailableDestinations.Add(newDestination);
                                    vNewDestinations.Add(newDestination);
                                }
                            }
                        }
                    }
                }
            }
            vOldDestinations = vNewDestinations;
        }

        //Debug.Log(m_vDestinations.Count);
        /*foreach (Destination dest in m_vDestinations)
        {
            Debug.Log(dest.ToString());
        }*/
        //Debug.Log(Time.realtimeSinceStartup - beginTime);
    }
    int computeCost(int _iTerrainId, int _iMP, worldMap _worldMap)
    {
        if(_iTerrainId == 2 && !m_bNavigation)
        {
            return -1;
        }
        if(_iTerrainId == 4 && !m_bClimbing)
        {
            return -1;
        }
        if(_iTerrainId >= 13 && _iTerrainId <= 16 && !m_bSwimming)
        {
            return -1;
        }
        return _iMP - _worldMap.GetTerrainCost(_iTerrainId);
    }

    public void ShowDestinations(List<Character> _vCharacterList)
    {
        if (!m_bMovementDone)
        {
            foreach (Destination dest in m_vAvailableDestinations)
            {
                bool onAnotherCharacter = false;
                // do not show destinations where another character is
                foreach (Character character in _vCharacterList)
                {
                    GridElement elem = character.GetComponent<GridElement>();
                    Destination newDestination = new Destination(elem.m_iX, elem.m_iY, 0);
                    if(dest == newDestination)
                    {
                        onAnotherCharacter = true;
                        break;
                    }
                }
                if(!onAnotherCharacter)
                {
                    GameObject newDestinationGridElement = (GameObject)Instantiate(m_DestinationPrefab, new Vector3(dest.m_iX * 10, dest.m_iY * 10, -0.04f), Quaternion.identity);
                    newDestinationGridElement.GetComponent<GridElement>().m_iX = dest.m_iX;
                    newDestinationGridElement.GetComponent<GridElement>().m_iY = dest.m_iY;
                    newDestinationGridElement.GetComponent<GridElement>().m_iLayer = 10;
                    newDestinationGridElement.transform.SetParent(m_DestinationHolder);
                }
            }
        }
    }
    public void HideDestinations()
    {
        foreach (Transform child in m_DestinationHolder)
        {
            GameObject.Destroy(child.gameObject);
            ClearPath();
        }
    }

    public void SetOverDestination(int _iX, int _iY)
    {
        Destination OverDestination = m_vAvailableDestinations.Find(destination => destination == new Destination(_iX, _iY, 0));
        if(OverDestination != m_OverDestination)
        {
            m_OverDestination = OverDestination;
            ShowPath();
        }
    }
    private void ShowPath()
    {
        ClearPath();
        Destination previousDestination = m_OverDestination.m_Previous;
        Destination nextDestination = m_OverDestination;
        int iRotation = 0;
        int iType = 2;
        if(previousDestination.m_iX - nextDestination.m_iX == 1)
        {
            iRotation = 90;
        }
        else if (previousDestination.m_iX - nextDestination.m_iX == -1)
        {
            iRotation = 270;
        }
        else if (previousDestination.m_iY - nextDestination.m_iY == 1)
        {
            iRotation = 180;
        }
        GameObject newEndPathGridElement = (GameObject)Instantiate(m_PathPrefabs[iType], new Vector3(m_OverDestination.m_iX * 10, m_OverDestination.m_iY * 10, -0.05f), Quaternion.identity);
        newEndPathGridElement.transform.Rotate(Vector3.forward, iRotation);
        newEndPathGridElement.transform.SetParent(m_PathHolder);
        while(previousDestination.m_Previous != null)
        {
            iRotation = 0;
            iType = 0;
            if (previousDestination.m_iX - nextDestination.m_iX == 1)
            {
                if (previousDestination.m_Previous.m_iX - previousDestination.m_iX == 1)
                {
                    iRotation = 90;
                }
                if (previousDestination.m_Previous.m_iY - previousDestination.m_iY == 1)
                {
                    iRotation = 180;
                    iType = 1;
                }
                if (previousDestination.m_Previous.m_iY - previousDestination.m_iY == -1)
                {
                    iRotation = 270;
                    iType = 1;
                }
            }
            else if (previousDestination.m_iX - nextDestination.m_iX == -1)
            {
                if (previousDestination.m_Previous.m_iX - previousDestination.m_iX == -1)
                {
                    iRotation = 90;
                }
                if (previousDestination.m_Previous.m_iY - previousDestination.m_iY == 1)
                {
                    iRotation = 90;
                    iType = 1;
                }
                if (previousDestination.m_Previous.m_iY - previousDestination.m_iY == -1)
                {
                    iType = 1;
                }
            }
            else if (previousDestination.m_iY - nextDestination.m_iY == 1)
            {
                if (previousDestination.m_Previous.m_iX - previousDestination.m_iX == -1)
                {
                    iRotation = 270;
                    iType = 1;
                }
                if (previousDestination.m_Previous.m_iX - previousDestination.m_iX == 1)
                {
                    iRotation = 0;
                    iType = 1;
                }
            }
            else if (previousDestination.m_iY - nextDestination.m_iY == -1)
            {
                if (previousDestination.m_Previous.m_iX - previousDestination.m_iX == -1)
                {
                    iRotation = 180;
                    iType = 1;
                }
                if (previousDestination.m_Previous.m_iX - previousDestination.m_iX == 1)
                {
                    iRotation = 90;
                    iType = 1;
                }
            }
            GameObject newPathGridElement = (GameObject)Instantiate(m_PathPrefabs[iType], new Vector3(previousDestination.m_iX * 10, previousDestination.m_iY * 10, -0.05f), Quaternion.identity);
            newPathGridElement.transform.Rotate(Vector3.forward, iRotation);
            newPathGridElement.transform.SetParent(m_PathHolder);
            nextDestination = previousDestination;
            previousDestination = previousDestination.m_Previous;
        }
        /*GameObject newPathEndGridElement = (GameObject)Instantiate(m_PathPrefabs[0], new Vector3(previousDestination.m_iX * 10, previousDestination.m_iY * 10, -0.05f), Quaternion.identity);
        newPathEndGridElement.transform.SetParent(m_PathHolder);*/
    }
    private void ClearPath()
    {
        foreach (Transform child in m_PathHolder)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
    public bool SetDestination(int _iX, int _iY)
    {
        m_PathToDestination.Clear();
        m_Destination = m_vAvailableDestinations.Find(destination => destination == new Destination(_iX, _iY, 0));
        if (m_Destination != null)
        {
            m_PathToDestination.Push(m_Destination);
            Destination previousDestination = m_Destination.m_Previous;
            while (previousDestination.m_Previous != null)
            {
                m_PathToDestination.Push(previousDestination);
                previousDestination = previousDestination.m_Previous;
            }
            Debug.Log(m_PathToDestination.Count);

            m_CurrentPathDestination = m_PathToDestination.Pop();
            HideDestinations();
            m_fStartTime = Time.time;
            m_vStartPosition = transform.position;
            m_vEndPosition = new Vector3(m_CurrentPathDestination.m_iX * 10, m_CurrentPathDestination.m_iY * 10, -0.05f);
            m_bMoving = true;
        }
        return m_bMoving;
    }
}
