using UnityEngine;
using System.Collections;

public class HealthBar : MonoBehaviour
{
    public GameObject m_healthBarElem;
    int m_iHealthPoints;
    int m_iMaxHealthPoints;
    public void Init(int _iMaxHealthPoints)
    {
        m_iMaxHealthPoints = _iMaxHealthPoints;
    }
    public void SetHealthPoints(int _iHealthPoints)
    {
        m_iHealthPoints = _iHealthPoints;
        m_healthBarElem.transform.localScale = new Vector3(0.95f * m_iHealthPoints / m_iMaxHealthPoints, 0.7f, 1.0f);
        m_healthBarElem.transform.localPosition = new Vector3((-0.45f + 0.45f * m_iHealthPoints / m_iMaxHealthPoints), 0.0f, -0.01f);
    }
}
