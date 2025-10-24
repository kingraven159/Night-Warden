using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    //maxHp
    public int maxHp = 5;
    //hp
    public int hp = 5;
    //overShield
    public int overShield = 0;
    //atk
    public int atk = 3;
    //item
    public List<string> items;
}
