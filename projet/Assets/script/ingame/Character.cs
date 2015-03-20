using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class Character : MonoBehaviour
{
    public GridElement m_GridElement;
    public Transform m_DestinationHolder;
    public GameObject m_DestinationPrefab;
    public Transform m_PathHolder;
    public GameObject[] m_PathPrefabs;
    public GameObject m_SelectableElem;
    public HealthBar m_HealthBar;
    List<Destination> m_vAvailableDestinations;
    int m_iMouvementPoints = 0;
    bool m_bNavigation = false;
    bool m_bSwimming = false;
    bool m_bClimbing = false;
    bool m_bMovementDone = false;
    bool m_bActionDone = false;

    // movement
    bool m_bMoving = false;
    private float m_fSpeed = 4.0f;
    private float m_fStartTime;
    private Vector3 m_vStartPosition;
    private Vector3 m_vEndPosition;
    Destination m_OverDestination = null;
    Destination m_Destination = null;
    Stack<Destination> m_PathToDestination = new Stack<Destination>();
    Destination m_CurrentPathDestination = null;
    private List<Character> m_vCharacterList;
    private worldMap m_WorldMap;
    Destination m_LastPosition = null;

    // player
    public int m_iTeam = 0;
    public int m_iPlayerID = 0;

    // actions
    Weapon m_Weapon;
    Action m_SelectedAction = Action.None;
    Classes m_Class = Classes.Marauder;

    // statistics
    int m_iVitality = 10;
    int m_iStrength = 10;
    int m_iAgility = 10;
    int m_iInteligence = 10;
    int m_iSpirit = 10;

    int m_iArmor = 1;
    int m_iResistance = 1;
    int m_iDodge = 1;
    int m_iLuck = 1;

    int m_iHealthPoint = 10;

    public bool IsMoving()
    {
        return m_bMoving;
    }
    public bool IsTurnFinish()
    {
        return m_bActionDone;
    }
    public bool CanMove()
    {
        return !m_bMovementDone;
    }
    public void ResetTurn()
    {
        m_bActionDone = false;
        m_bMovementDone = false;
        ComputeAvailableDestinations();
        m_SelectableElem.SetActive(true);
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
                    m_CurrentPathDestination = m_PathToDestination.Pop();
                    GetComponent<GridElement>().m_iX = m_CurrentPathDestination.m_iX;
                    GetComponent<GridElement>().m_iY = m_CurrentPathDestination.m_iY;
                    m_fStartTime = Time.time;
                    m_vStartPosition = transform.position;
                    m_vEndPosition = new Vector3(m_CurrentPathDestination.m_iX * 10, m_CurrentPathDestination.m_iY * 10, -0.05f);
                }
                else
                {
                    /*GetComponent<GridElement>().m_iX = m_CurrentPathDestination.m_iX;
                    GetComponent<GridElement>().m_iY = m_CurrentPathDestination.m_iY;*/
                    m_bMoving = false;
                    m_CurrentPathDestination = null;
                    m_bMovementDone = true;
                    //m_bActionDone = true;
                }
            }
        }
    }
    public void Init(int _iMouvementPoints, int _iPlayerID, int _iTeam, List<Character> _vCharaterList, worldMap _worldMap, bool _bSwimming = false, bool _bClimbing = false, bool _bNavigation = false)
    {
        m_iMouvementPoints = _iMouvementPoints;
        m_iPlayerID = _iPlayerID;
        m_iTeam = _iTeam;
        m_vCharacterList = _vCharaterList;
        m_WorldMap = _worldMap;
        m_bSwimming = _bSwimming;
        m_bClimbing = _bClimbing;
        m_bNavigation = _bNavigation;
        m_vAvailableDestinations = new List<Destination>();
        m_Weapon = new Weapon();
        m_Weapon.Init(2, 3, 5, 1, 1.0f, Caracteristic.Agility);
        Classes Class = (Classes)Random.Range(0, 11);
        ClassBaseStatistics stats = gameDesignVariables.Instance.GetClassStatistics(Class);
        Debug.Log(Class);

        m_iVitality = stats.m_iVitality;
        m_iStrength = stats.m_iStrength;
        m_iAgility = stats.m_iAgility;
        m_iInteligence = stats.m_iInteligence;
        m_iSpirit = stats.m_iSpirit;

        m_iArmor = stats.m_iArmor;
        m_iResistance = stats.m_iResistance;
        m_iDodge = stats.m_iDodge;
        m_iLuck = stats.m_iLuck;

        m_iHealthPoint = Mathf.RoundToInt(m_iVitality * 1.1f);
        m_HealthBar.Init(m_iHealthPoint);
    }

    public void ComputeAvailableDestinations()
    {
        //float beginTime = Time.realtimeSinceStartup;
        m_vAvailableDestinations.Clear();
        Destination Origin = new Destination(m_GridElement.m_iX, m_GridElement.m_iY, m_iMouvementPoints);
        List<Destination> vOldDestinations = new List<Destination>();
        vOldDestinations.Add(Origin);
        int[,] _tTerrainInfos = m_WorldMap.GetTerrainInfos();
        int iMapLimitX = m_WorldMap.GetMapWidth();
        int iMapLimitY = m_WorldMap.GetMapHeight();
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

                        bool onAnotherCharacter = false;
                        // do not show destinations where another character is
                        Destination otherCharDestination = new Destination(0, 0, 0);
                        int iOtherCharTeam = 0;
                        foreach (Character character in m_vCharacterList)
                        {
                            otherCharDestination.m_iX = character.m_GridElement.m_iX;
                            otherCharDestination.m_iY = character.m_GridElement.m_iY;
                            if (newDestination == otherCharDestination)
                            {
                                iOtherCharTeam = character.m_iTeam;
                                onAnotherCharacter = true;
                                break;
                            }
                        }

                        if (newDestination != dest.m_Previous && (iOtherCharTeam == m_iTeam || !onAnotherCharacter))
                        {
                            int iTerrainType = _tTerrainInfos[iNewX, iNewY];
                            int iMPRest = computeCost(iTerrainType, dest.m_iMouvementPoints);
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
    int computeCost(int _iTerrainId, int _iMP)
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
        return _iMP - m_WorldMap.GetTerrainCost(_iTerrainId);
    }

    public void ShowDestinations()
    {
        if (!m_bMovementDone)
        {
            m_SelectableElem.SetActive(false);
            foreach (Destination dest in m_vAvailableDestinations)
            {
                bool onAnotherCharacter = false;
                // do not show destinations where another character is
                Destination newDestination = new Destination(0, 0, 0);
                foreach (Character character in m_vCharacterList)
                {
                    GridElement elem = character.GetComponent<GridElement>();
                    newDestination.m_iX = elem.m_iX;
                    newDestination.m_iY = elem.m_iY;
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
    [RPC]
    void StartPath()
    {
        m_PathToDestination.Clear();
    }
    [RPC]
    void AddPathDestination(int _iX, int _iY)
    {
        Destination newDestination = new Destination(_iX, _iY, 0);
        m_PathToDestination.Push(newDestination);
    }
    [RPC]
    void EndPath()
    {
        m_CurrentPathDestination = m_PathToDestination.Pop();
        GetComponent<GridElement>().m_iX = m_CurrentPathDestination.m_iX;
        GetComponent<GridElement>().m_iY = m_CurrentPathDestination.m_iY;
        m_fStartTime = Time.time;
        m_vStartPosition = transform.position;
        m_vEndPosition = new Vector3(m_CurrentPathDestination.m_iX * 10, m_CurrentPathDestination.m_iY * 10, -0.05f);
        m_bMoving = true;
    }
    public bool SetDestination(int _iX, int _iY)
    {
        m_SelectedAction = Action.None;
        m_PathToDestination.Clear();
        if (Network.isClient || Network.isServer)
        {
            GetComponent<NetworkView>().RPC("StartPath", RPCMode.Others);
        }
        m_Destination = m_vAvailableDestinations.Find(destination => destination == new Destination(_iX, _iY, 0));
        if (m_Destination != null)
        {
            m_LastPosition = new Destination(m_GridElement.m_iX, m_GridElement.m_iY, 0);
            m_PathToDestination.Push(m_Destination);
            if (Network.isClient || Network.isServer)
            {
                GetComponent<NetworkView>().RPC("AddPathDestination", RPCMode.Others, m_Destination.m_iX, m_Destination.m_iY);
            }
            Destination previousDestination = m_Destination.m_Previous;
            while (previousDestination.m_Previous != null)
            {
                m_PathToDestination.Push(previousDestination);
                if(Network.isClient || Network.isServer)
                {
                    GetComponent<NetworkView>().RPC("AddPathDestination", RPCMode.Others, previousDestination.m_iX, previousDestination.m_iY);
                }
                previousDestination = previousDestination.m_Previous;
            }
            m_CurrentPathDestination = m_PathToDestination.Pop();
            GetComponent<GridElement>().m_iX = m_CurrentPathDestination.m_iX;
            GetComponent<GridElement>().m_iY = m_CurrentPathDestination.m_iY;
            HideDestinations();
            m_fStartTime = Time.time;
            m_vStartPosition = transform.position;
            m_vEndPosition = new Vector3(m_CurrentPathDestination.m_iX * 10, m_CurrentPathDestination.m_iY * 10, -0.05f);
            m_bMoving = true;
            if (Network.isClient || Network.isServer)
            {
                GetComponent<NetworkView>().RPC("EndPath", RPCMode.Others);
            }
        }
        return m_bMoving;
    }

    // Action management
    public List<Action> getActions()
    {
        List<Action> Actions = new List<Action>();
        foreach(Character character in m_vCharacterList)
        {
            if(character.m_iTeam != m_iTeam)
            {
                int distance = Mathf.Abs(character.GetComponent<GridElement>().m_iX - GetComponent<GridElement>().m_iX) + Mathf.Abs(character.GetComponent<GridElement>().m_iY - GetComponent<GridElement>().m_iY);
                if(distance >= m_Weapon.m_iMinRange && distance <= m_Weapon.m_iMaxRange)
                {
                    Debug.Log("attack possible");
                    Actions.Add(Action.Attack);
                    break;
                }
            }
        }
        return Actions;
    }
    public void ShowActiontTargets(Action _Action)
    {
        m_SelectedAction = _Action;
        int iMinRange = 0;
        int iMaxRange = 0;
        bool allyAction = false;
        if (_Action == Action.Attack)
        {
            iMinRange = m_Weapon.m_iMinRange;
            iMaxRange = m_Weapon.m_iMaxRange;
        }
        foreach (Character character in m_vCharacterList)
        {
            if ((character.m_iTeam != m_iTeam && !allyAction) || (character.m_iTeam == m_iTeam && allyAction))
            {
                GridElement characterGridElem = character.GetComponent<GridElement>();
                int distance = Mathf.Abs(characterGridElem.m_iX - GetComponent<GridElement>().m_iX) + Mathf.Abs(characterGridElem.m_iY - GetComponent<GridElement>().m_iY);
                if (distance >= iMinRange && distance <= iMaxRange)
                {
                    GameObject newTargetGridElement = (GameObject)Instantiate(m_DestinationPrefab, new Vector3(characterGridElem.m_iX * 10, characterGridElem.m_iY * 10, -0.04f), Quaternion.identity);
                    if (allyAction)
                    {
                        newTargetGridElement.GetComponent<Renderer>().material.color = new Color(0,1,0,0.36f);
                    }
                    else
                    {
                        newTargetGridElement.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.36f);
                    }
                    newTargetGridElement.GetComponent<GridElement>().m_iX = characterGridElem.m_iX;
                    newTargetGridElement.GetComponent<GridElement>().m_iY = characterGridElem.m_iY;
                    newTargetGridElement.GetComponent<GridElement>().m_iLayer = 10;
                    newTargetGridElement.transform.SetParent(m_DestinationHolder);
                }
            }
        }
    }
    public void Act(int _iPosX, int _iPosY)
    {
        if(m_SelectedAction != Action.None)
        {
            if(m_SelectedAction == Action.Attack)
            {
                Character target = null;
                foreach (Character character in m_vCharacterList)
                {
                    GridElement characterGridElem = character.GetComponent<GridElement>();
                    if(characterGridElem.m_iX == _iPosX && characterGridElem.m_iY == _iPosY)
                    {
                        target = character;
                        break;
                    }
                }
                if (target != null)
                {
                    if (Network.isClient || Network.isServer)
                    {
                        target.GetComponent<NetworkView>().RPC("UpdateHealthPoint", RPCMode.All, -2);
                        GetComponent<NetworkView>().RPC("UpdateHealthPoint", RPCMode.All, -1);
                    }
                    else
                    {
                        target.UpdateHealthPoint(-2);
                        UpdateHealthPoint(-1);
                    }
                }
            }
            m_SelectedAction = Action.None;
            m_bActionDone = true;
            HideDestinations();
            m_LastPosition = null;
        }
    }
    public void UpdateHealthPoint(int _iHealthPointChange)
    {
        m_iHealthPoint += _iHealthPointChange;
        if (m_iHealthPoint < 0)
        {
            m_iHealthPoint = 0;
        }
        m_HealthBar.SetHealthPoints(m_iHealthPoint);
    }
    public void EndMove()
    {
        m_bMovementDone = true;
    }
    public void Cancel()
    {
        m_bMoving = false;
        m_CurrentPathDestination = null;
        m_bMovementDone = false;
        if (m_LastPosition != null)
        {
            m_PathToDestination.Clear();
            if (Network.isClient || Network.isServer)
            {
                GetComponent<NetworkView>().RPC("StartPath", RPCMode.Others);
                GetComponent<NetworkView>().RPC("AddPathDestination", RPCMode.Others, m_LastPosition.m_iX, m_LastPosition.m_iY);
                GetComponent<NetworkView>().RPC("EndPath", RPCMode.Others);
            }
            transform.position = new Vector3(m_LastPosition.m_iX * 10, m_LastPosition.m_iY * 10, -0.05f);
            GetComponent<GridElement>().m_iX = m_LastPosition.m_iX;
            GetComponent<GridElement>().m_iY = m_LastPosition.m_iY;
        }
    }
    public void Wait()
    {
        m_bActionDone = true;
        m_CurrentPathDestination = null;
        m_LastPosition = null;
    }
}
