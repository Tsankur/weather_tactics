using UnityEngine;
using System.Collections;

public class GridElement : MonoBehaviour
{
    public int m_iX = 0;
    public int m_iY = 0;
    public int m_iLayer = 0;
    public void Init(int _iX, int _iY, int _iLayer)
    {
        m_iX = _iX;
        m_iY = _iY;
        m_iLayer = _iLayer;
    }
}
