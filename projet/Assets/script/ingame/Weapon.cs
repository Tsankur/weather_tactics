using UnityEngine;
using System.Collections;

enum Caracteristic { Strength, Agility, Vitality, Inteligence, Spirit};
public class Weapon
{
    int m_iMinRange = 1;
    int m_iMaxRange = 1;
    int m_iDamages = 1;
    int m_iSpeed = 1;
    float m_fDamageMultiplier = 1;
    Caracteristic m_UsedCaracteristic = Caracteristic.Strength;
}
