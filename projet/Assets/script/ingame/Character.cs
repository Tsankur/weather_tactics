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
    List<Destination> m_vDestinations;
    int m_iMouvementPoints;
    bool m_bNavigation = false;
    bool m_bSwimming = false;
    bool m_bClimbing = false;
    public void Start()
    {
    }
    public void Init(int _iMouvementPoints)
    {
        m_iMouvementPoints = _iMouvementPoints;
        m_vDestinations = new List<Destination>();
    }
    public void ComputeAvailableDestinations(worldMap _worldMap, List<Character> _vCharacterList)
    {
        float beginTime = Time.realtimeSinceStartup;
        m_vDestinations.Clear();
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

                    if (i == 0)
                    {
                        if (dest.m_iX > 0)
                        {
                            iNewX = dest.m_iX - 1;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (i == 1)
                    {
                        if (dest.m_iX < iMapLimitX - 1)
                        {
                            iNewX = dest.m_iX + 1;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (i == 2)
                    {
                        if (dest.m_iY > 0)
                        {
                            iNewY = dest.m_iY - 1;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (i == 3)
                    {
                        if (dest.m_iY < iMapLimitY - 1)
                        {
                            iNewY = dest.m_iY + 1;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    Destination newDestination = new Destination(iNewX, iNewY, 0, dest);
                    if (newDestination != dest.m_Previous)
                    {
                        int iTerrainType = _tTerrainInfos[iNewX, iNewY];
                        int iMPRest = computeCost(iTerrainType, dest.m_iMouvementPoints, _worldMap);
                        newDestination.m_iMouvementPoints = iMPRest;
                        if (iMPRest >= 0)
                        {
                            Destination EquivalentDestination = m_vDestinations.Find(destination => destination == newDestination);
                            if (EquivalentDestination != null)
                            {
                                if (newDestination > EquivalentDestination)
                                {
                                    m_vDestinations.Remove(EquivalentDestination);
                                    m_vDestinations.Add(newDestination);
                                    vNewDestinations.Add(newDestination);
                                }
                            }
                            else
                            {
                                m_vDestinations.Add(newDestination);
                                vNewDestinations.Add(newDestination);
                            }
                        }
                    }
                }/*
                if (dest.m_iX > 0)
                {
                    int iNewX = dest.m_iX - 1;
                    int iNewY = dest.m_iY;
                    Destination newDestination = new Destination(iNewX, iNewY, 0, dest);
                    if (newDestination != dest.m_Previous)
                    {
                        int iTerrainType = _tTerrainInfos[iNewX, iNewY];
                        int iMPRest = computeCost(iTerrainType, dest.m_iMouvementPoints, _worldMap);
                        newDestination.m_iMouvementPoints = iMPRest;
                        if (iMPRest >= 0)
                        {
                            Destination EquivalentDestination = m_vDestinations.Find(destination => destination == newDestination);
                            if (EquivalentDestination != null)
                            {
                                if (newDestination > EquivalentDestination)
                                {
                                    m_vDestinations.Remove(EquivalentDestination);
                                    m_vDestinations.Add(newDestination);
                                    vNewDestinations.Add(newDestination);
                                }
                            }
                            else
                            {
                                m_vDestinations.Add(newDestination);
                                vNewDestinations.Add(newDestination);
                            }
                        }
                    }
                }
                if (dest.m_iX < iMapLimitX - 1)
                {
                    int iNewX = dest.m_iX + 1;
                    int iNewY = dest.m_iY;
                    Destination newDestination = new Destination(iNewX, iNewY, 0, dest);
                    if (newDestination != dest.m_Previous)
                    {
                        int iTerrainType = _tTerrainInfos[iNewX, iNewY];
                        int iMPRest = computeCost(iTerrainType, dest.m_iMouvementPoints, _worldMap);
                        newDestination.m_iMouvementPoints = iMPRest;
                        if (iMPRest >= 0)
                        {
                            Destination EquivalentDestination = m_vDestinations.Find(destination => destination == newDestination);
                            if (EquivalentDestination != null)
                            {
                                if (newDestination > EquivalentDestination)
                                {
                                    m_vDestinations.Remove(EquivalentDestination);
                                    m_vDestinations.Add(newDestination);
                                    vNewDestinations.Add(newDestination);
                                }
                            }
                            else
                            {
                                m_vDestinations.Add(newDestination);
                                vNewDestinations.Add(newDestination);
                            }
                        }
                    }
                }
                if (dest.m_iY > 0)
                {
                    int iNewX = dest.m_iX;
                    int iNewY = dest.m_iY - 1;
                    Destination newDestination = new Destination(iNewX, iNewY, 0, dest);
                    if (newDestination != dest.m_Previous)
                    {
                        int iTerrainType = _tTerrainInfos[iNewX, iNewY];
                        int iMPRest = computeCost(iTerrainType, dest.m_iMouvementPoints, _worldMap);
                        newDestination.m_iMouvementPoints = iMPRest;
                        if (iMPRest >= 0)
                        {
                            Destination EquivalentDestination = m_vDestinations.Find(destination => destination == newDestination);
                            if (EquivalentDestination != null)
                            {
                                if (newDestination > EquivalentDestination)
                                {
                                    m_vDestinations.Remove(EquivalentDestination);
                                    m_vDestinations.Add(newDestination);
                                    vNewDestinations.Add(newDestination);
                                }
                            }
                            else
                            {
                                m_vDestinations.Add(newDestination);
                                vNewDestinations.Add(newDestination);
                            }
                        }
                    }
                }
                if (dest.m_iY < iMapLimitY - 1)
                {
                    int iNewX = dest.m_iX;
                    int iNewY = dest.m_iY - 1;
                    Destination newDestination = new Destination(iNewX, iNewY, 0, dest);
                    if (newDestination != dest.m_Previous)
                    {
                        int iTerrainType = _tTerrainInfos[iNewX, iNewY];
                        int iMPRest = computeCost(iTerrainType, dest.m_iMouvementPoints, _worldMap);
                        newDestination.m_iMouvementPoints = iMPRest;
                        if (iMPRest >= 0)
                        {
                            Destination EquivalentDestination = m_vDestinations.Find(destination => destination == newDestination);
                            if (EquivalentDestination != null)
                            {
                                if (newDestination > EquivalentDestination)
                                {
                                    m_vDestinations.Remove(EquivalentDestination);
                                    m_vDestinations.Add(newDestination);
                                    vNewDestinations.Add(newDestination);
                                }
                            }
                            else
                            {
                                m_vDestinations.Add(newDestination);
                                vNewDestinations.Add(newDestination);
                            }
                        }
                    }
                }*/
            }
            vOldDestinations = vNewDestinations;
        }
        // removing destinations where another character is
        Debug.Log(m_vDestinations.Count);
        foreach(Character character in _vCharacterList)
        {
            GridElement elem = character.GetComponent<GridElement>();
            Destination newDestination = new Destination(elem.m_iX, elem.m_iY, 0);
            Destination EquivalentDestination = m_vDestinations.Find(destination => destination == newDestination);
            if(EquivalentDestination != null)
            {
                m_vDestinations.Remove(EquivalentDestination);
            }
        }

        Debug.Log(m_vDestinations.Count);
        foreach (Destination dest in m_vDestinations)
        {
            Debug.Log(dest.ToString());
        }
        Debug.Log(Time.realtimeSinceStartup - beginTime);
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
}
