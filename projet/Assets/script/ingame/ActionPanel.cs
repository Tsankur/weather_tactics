using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ActionPanel : MonoBehaviour
{
    public game m_Game;
    Character m_SelectedChar = null;
    public GameObject m_ActionButtonPrefab;
    List<GameObject> m_buttons = new List<GameObject>(); 

    public void SetPossibleActions(List<Action> _Actions, Character _SelectedChar)
    {
        foreach (GameObject button in m_buttons)
        {
            GameObject.Destroy(button);
        }
        m_SelectedChar = _SelectedChar;
        int iButtonId = 0;
        foreach(Action action in _Actions)
        {
            GameObject oNewButtonName = (GameObject)Instantiate(m_ActionButtonPrefab);
            oNewButtonName.transform.SetParent(transform, false);
            Button oNewButton = oNewButtonName.GetComponent<Button>();
            oNewButtonName.transform.localPosition = new Vector3(0, -24 - 30 * iButtonId + GetComponent<RectTransform>().sizeDelta.y / 2, 0);
            oNewButtonName.transform.GetChild(0).GetComponent<Text>().text = action.ToString();
            AddListenerToActionButton(oNewButton, action);
            iButtonId++;
            m_buttons.Add(oNewButtonName);
        }
    }
    void AddListenerToActionButton(Button _oButton, Action _Action)
    {
        _oButton.onClick.AddListener(() => { m_SelectedChar.ShowActiontTargets(_Action); m_Game.SetActionSelected(); });
    }
}
