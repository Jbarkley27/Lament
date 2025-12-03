using System;
using UnityEngine;

public enum ProjectilePatternType
{
    Single,
    Row
}

[Serializable]
public struct ProjectilePattern
{
    public ProjectilePatternType patternType;
    public int count;
    [Tooltip("Used for row/fan spreads. Degrees across the whole arc.")]
    public float arcAngle;
    public float speed;
    public float lifetime;
    public int damage;
}
