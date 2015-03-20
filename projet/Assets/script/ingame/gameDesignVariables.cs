using UnityEngine;
using System.Collections;

public enum Classes { Marauder = 0, Archer, ElementaryMage, Phenologist, Engineer, Assassin, Monk, Paladin, Priest, Lancer, Aegis };

public struct ClassBaseStatistics
{
    public int m_iVitality;
    public int m_iStrength;
    public int m_iAgility;
    public int m_iIntelligence;
    public int m_iSpirit;

    public int m_iArmor;
    public int m_iResistance;
    public int m_iLuck;
    public ClassBaseStatistics(int _iVitality = 10, int _iStrength = 10, int _iAgility = 10, int _iInteligence = 10, int _iSpirit = 10, int _iArmor = 1, int _iResistance = 1, int _iLuck = 1)
    {
        m_iVitality = _iVitality;
        m_iStrength = _iStrength;
        m_iAgility = _iAgility;
        m_iIntelligence = _iInteligence;
        m_iSpirit = _iSpirit;

        m_iArmor = _iArmor;
        m_iResistance = _iResistance;
        m_iLuck = _iLuck;
    }
}

public class gameDesignVariables
{
    private static gameDesignVariables m_instance = new gameDesignVariables();


    private gameDesignVariables()
    {
        m_vClassesBaseStatistics[(int)Classes.Marauder] = new ClassBaseStatistics(10, 10, 2, 2, 0, 2, 2, 1);
        m_vClassesBaseStatistics[(int)Classes.Archer] = new ClassBaseStatistics(8, 2, 10, 2, 0, 0, 2, 3);
        m_vClassesBaseStatistics[(int)Classes.ElementaryMage] = new ClassBaseStatistics(10, 2, 2, 10, 10, 0, 5, 1);
        m_vClassesBaseStatistics[(int)Classes.Phenologist] = new ClassBaseStatistics(10, 2, 2, 10, 8, 0, 5, 1);
        m_vClassesBaseStatistics[(int)Classes.Engineer] = new ClassBaseStatistics(8, 2, 2, 10, 8, 3, 3, 3);
        m_vClassesBaseStatistics[(int)Classes.Assassin] = new ClassBaseStatistics(8, 4, 10, 2, 0, 1, 2, 4);
        m_vClassesBaseStatistics[(int)Classes.Monk] = new ClassBaseStatistics(12, 4, 10, 2, 0, 1, 2, 2);
        m_vClassesBaseStatistics[(int)Classes.Paladin] = new ClassBaseStatistics(12, 5, 2, 3, 3, 7, 7, 1);
        m_vClassesBaseStatistics[(int)Classes.Priest] = new ClassBaseStatistics(10, 2, 2, 10, 10, 0, 5, 1);
        m_vClassesBaseStatistics[(int)Classes.Lancer] = new ClassBaseStatistics(10, 2, 2, 10, 10, 0, 5, 1);
        m_vClassesBaseStatistics[(int)Classes.Aegis] = new ClassBaseStatistics(15, 7, 2, 2, 0, 10, 5, 1);
    }

    public static gameDesignVariables Instance
    {
        get { return m_instance; }
    }

    public float vitalityMultiplier = 1.1f;
    ClassBaseStatistics[] m_vClassesBaseStatistics = new ClassBaseStatistics[11];

    public ClassBaseStatistics GetClassStatistics(Classes _class)
    {
        return m_vClassesBaseStatistics[(int)_class];
    }
}
