using System.Collections.Generic;
using UnityEngine;

internal enum GameState
{
    MainMenu,
    Playing
}

internal enum BuildingType
{
    None,
    Base,
    Factory
}

internal enum UnitType
{
    Infantry
}

internal enum Team
{
    Player,
    Enemy
}

internal sealed class BuildingData
{
    public int Id;
    public string DisplayName;
    public BuildingType Type;
    public GameObject GameObject;
    public Vector2 Position;
    public Vector2Int Cell;
    public float Radius;
    public string Description;
    public Team Team;
    public int MaxHitPoints;
    public int HitPoints;
    public int InfantryQueue;
    public float ProductionTimer;

    public BuildingData(
        string displayName,
        BuildingType type,
        GameObject gameObject,
        Vector2 position,
        Vector2Int cell,
        float radius,
        string description,
        Team team,
        int maxHitPoints
    )
    {
        DisplayName = displayName;
        Type = type;
        GameObject = gameObject;
        Position = position;
        Cell = cell;
        Radius = radius;
        Description = description;
        Team = team;
        MaxHitPoints = maxHitPoints;
        HitPoints = maxHitPoints;
    }
}

internal sealed class UnitData
{
    public int Id;
    public string DisplayName;
    public UnitType Type;
    public GameObject GameObject;
    public Vector2 Position;
    public Vector2Int Cell;
    public float Radius;
    public string Description;
    public Team Team;
    public int MaxHitPoints;
    public int HitPoints;
    public bool IsMoving;
    public Vector2 TargetPosition;
    public Vector2Int TargetCell;
    public readonly List<Vector2> Waypoints = new List<Vector2>();
    public int AttackDamage;
    public float AttackRange;
    public float AttackCooldown;
    public float AttackTimer;
    public BuildingData AttackTarget;
    public UnitData AttackUnitTarget;

    public UnitData(
        string displayName,
        UnitType type,
        GameObject gameObject,
        Vector2 position,
        Vector2Int cell,
        float radius,
        string description,
        Team team,
        int maxHitPoints,
        int attackDamage,
        float attackRange,
        float attackCooldown
    )
    {
        DisplayName = displayName;
        Type = type;
        GameObject = gameObject;
        Position = position;
        Cell = cell;
        Radius = radius;
        Description = description;
        Team = team;
        MaxHitPoints = maxHitPoints;
        HitPoints = maxHitPoints;
        TargetPosition = position;
        TargetCell = cell;
        AttackDamage = attackDamage;
        AttackRange = attackRange;
        AttackCooldown = attackCooldown;
    }
}
