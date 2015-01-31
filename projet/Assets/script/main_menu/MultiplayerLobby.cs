using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MultiplayerLobby : MonoBehaviour
{
    public GameObject[] m_Panels;
    public GameObject[] m_PlayerSlotPanels;
    public GameObject[] m_PlayerReadyCheckmark;
    public void SetPlayerSlot(int _ioldSlot, int _iSlot, string _szPseudo)
    {
        if(_ioldSlot >= 0)
        {
            SetPlayerReady(_ioldSlot, false);
            m_Panels[_ioldSlot].SetActive(false);
            m_Panels[_ioldSlot].transform.GetChild(0).GetComponent<Text>().text = "";
        }
        if (_iSlot >= 0)
        {
            m_Panels[_iSlot].SetActive(true);
            m_Panels[_iSlot].transform.GetChild(0).GetComponent<Text>().text = _szPseudo;
        }
    }
    public void SetPlayerReady(int _iSlot, bool _bReady)
    {
        if (_iSlot >= 0)
        {
            if (_bReady)
            {
                m_PlayerReadyCheckmark[_iSlot].SetActive(true);
            }
            else
            {
                m_PlayerReadyCheckmark[_iSlot].SetActive(false);
            }
        }
    }
    public void Reset(int _iMaxPlayerCount)
    {
        for (int i = 0; i < 4; i++)
        {
            m_Panels[i].SetActive(false);
            m_Panels[i].transform.GetChild(0).GetComponent<Text>().text = "";
            m_PlayerReadyCheckmark[i].SetActive(false);
            if(i >= _iMaxPlayerCount)
            {
                m_PlayerSlotPanels[i].SetActive(false);
            }
            else
            {
                m_PlayerSlotPanels[i].SetActive(true);
            }
        }
    }
}
