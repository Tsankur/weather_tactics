using UnityEngine;
using System.Collections;

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