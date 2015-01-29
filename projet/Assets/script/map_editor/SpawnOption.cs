using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpawnOption : MonoBehaviour
{
    public Toggle[] m_Toggles;
    public int GetSelectedPlayer()
    {
        for (int i = 0; i < m_Toggles.Length; i++)
        {
            if (m_Toggles[i].isOn)
            {
                return i + 1;
            }
        }
        return 0;
    }
    public void SetSelectedPlayer(int _iPlayer)
    {
        for (int i = 0; i < m_Toggles.Length; i++)
        {
            if (_iPlayer-1 == i)
            {
                m_Toggles[i].isOn = true;
            }
            else
            {
                m_Toggles[i].isOn = false;
            }
        }
    }
}
