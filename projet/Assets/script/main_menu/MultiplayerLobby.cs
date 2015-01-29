using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MultiplayerLobby : MonoBehaviour
{
    public GameObject[] m_Panels;
    public void SetPlayerSlot(int _ioldSlot, int _iSlot, string _szPseudo)
    {
        if(_ioldSlot >= 0)
        {
            m_Panels[_ioldSlot].SetActive(false);
            m_Panels[_ioldSlot].transform.GetChild(0).GetComponent<Text>().text = "";
        }
        if (_iSlot >= 0)
        {
            m_Panels[_iSlot].SetActive(true);
            m_Panels[_iSlot].transform.GetChild(0).GetComponent<Text>().text = _szPseudo;
        }
    }
    public void Reset()
    {
        for(int i = 0; i < 4; i++)
        {
            m_Panels[i].SetActive(false);
            m_Panels[i].transform.GetChild(0).GetComponent<Text>().text = "";
        }
    }
}
