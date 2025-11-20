using UnityEngine;
using System;

// TODO move to a common file once we start creating items for the player
public enum ItemGrade
{
    C,
    B,
    A,
    S,
}

[Serializable]
public struct ShieldGrade
{
    public ItemGrade grade;
    public float damageReduction; // e.g., 0.1 for 10% reduction
    public int shieldCapacity; // e.g., 50 for 50 shield points
}


