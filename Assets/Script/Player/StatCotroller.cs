using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatCotroller : MonoBehaviour
{
    [Header("About Lv")]
    [SerializeField] private float exp;
    [SerializeField] private float needExp;
    [SerializeField] private int level;
    [SerializeField] private int plusHpByLevel;
    [SerializeField] private int plusMpByLevel;

    [Header("About HP")]
    [SerializeField] private int hp;
    public int Hp { get { return hp; } set { hp = Mathf.Clamp(value, 0, maxHp); } }
    [SerializeField] private int maxHp;
    public int MaxHp { get { return maxHp; } set { maxHp = value; } }

    [Header("About Mp")]
    [SerializeField] private int mana;
    public int Mana { get { return mana; } set { mana = Mathf.Clamp(value, 0, maxMana); } }
    [SerializeField] private int maxMana;
    public int MaxMana { get { return maxMana; } set { maxMana = value; } }

    public void GetExp(int get)
    {
        exp += get;
        while (exp >= needExp)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        maxHp += plusHpByLevel;
        hp = maxHp;
        maxMana += plusMpByLevel;
        mana = maxMana;
        exp = exp - needExp;
        needExp *= 1.5f;
    }
}
