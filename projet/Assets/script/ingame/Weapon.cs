using UnityEngine;
using System.Collections;

public enum Caracteristic { Strength, Agility, Vitality, Inteligence, Spirit };
public class Weapon
{
    public int m_iMinRange = 1;
    public int m_iMaxRange = 1;
    int m_iDamages = 1;
    int m_iSpeed = 1;
    float m_fDamageMultiplier = 1;
    Caracteristic m_UsedCaracteristic = Caracteristic.Strength;

    public void Init(int _iMinRange, int _iMaxRange, int _iDamages, int _iSpeed, float _fDamageMultiplier, Caracteristic _UsedCaracteristic)
    {
        m_iMinRange = _iMinRange;
        m_iMaxRange = _iMaxRange;
        m_iDamages = _iDamages;
        m_iSpeed = _iSpeed;
        m_fDamageMultiplier = _fDamageMultiplier;
        m_UsedCaracteristic = _UsedCaracteristic;
    }
}
