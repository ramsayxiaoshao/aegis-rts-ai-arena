using System.Collections.Generic;
using UnityEngine;

internal sealed class UnitMovementSystem
{
    private const int FormationSearchRadius = 6;

    private readonly RtsGameConfig config;
    private readonly GridMapService gridMap;
    private readonly IList<UnitData> units;

    public UnitMovementSystem(
        RtsGameConfig gameConfig,
        GridMapService mapService,
        IList<UnitData> unitList
    )
    {
        config = gameConfig;
        gridMap = mapService;
        units = unitList;
    }

    public int CommandGroupMove(IList<UnitData> actors, Vector2Int centerCell)
    {
        List<UnitData> movableUnits = new List<UnitData>();
        HashSet<Vector2Int> actorCells = new HashSet<Vector2Int>();

        foreach (UnitData unit in actors)
        {
            if (unit == null || unit.Team != Team.Player)
            {
                continue;
            }

            movableUnits.Add(unit);
            actorCells.Add(unit.Cell);
        }

        if (movableUnits.Count == 0)
        {
            return 0;
        }

        List<Vector2Int> targetCells = FindFormationCells(
            centerCell,
            movableUnits.Count,
            actorCells
        );

        if (targetCells.Count == 0)
        {
            return 0;
        }

        HashSet<Vector2Int> blockedCells = new HashSet<Vector2Int>(gridMap.OccupiedCells);

        foreach (Vector2Int actorCell in actorCells)
        {
            blockedCells.Remove(actorCell);
        }

        foreach (UnitData unit in movableUnits)
        {
            gridMap.Release(unit.Cell);
        }

        int commandedCount = Mathf.Min(movableUnits.Count, targetCells.Count);
        int acceptedCount = 0;

        for (int i = 0; i < commandedCount; i++)
        {
            UnitData unit = movableUnits[i];
            Vector2Int targetCell = targetCells[i];
            List<Vector2Int> path = GridPathfinder.FindPath(
                unit.Cell,
                targetCell,
                gridMap.MapSize,
                gridMap.MapSize,
                blockedCells
            );

            if (path.Count == 0 || !gridMap.TryOccupy(targetCell))
            {
                gridMap.TryOccupy(unit.Cell);
                unit.IsMoving = false;
                continue;
            }

            unit.AttackTarget = null;
            unit.AttackUnitTarget = null;
            unit.Cell = targetCell;
            unit.TargetCell = targetCell;
            unit.TargetPosition = gridMap.CellToWorld(targetCell);
            unit.Waypoints.Clear();

            for (int pathIndex = 1; pathIndex < path.Count; pathIndex++)
            {
                unit.Waypoints.Add(gridMap.CellToWorld(path[pathIndex]));
            }

            unit.IsMoving = unit.Waypoints.Count > 0;
            blockedCells.Add(targetCell);
            acceptedCount++;
        }

        return acceptedCount;
    }

    public List<Vector2Int> FindFormationCells(
        Vector2Int centerCell,
        int requiredCount,
        ISet<Vector2Int> actorCells
    )
    {
        List<Vector2Int> result = new List<Vector2Int>();
        HashSet<Vector2Int> reservedCells = new HashSet<Vector2Int>();

        for (int radius = 0; radius <= FormationSearchRadius; radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    Vector2Int candidate = new Vector2Int(centerCell.x + dx, centerCell.y + dy);

                    if (!gridMap.IsCellInside(candidate) || reservedCells.Contains(candidate))
                    {
                        continue;
                    }

                    bool blockedByOtherEntity =
                        gridMap.IsOccupied(candidate) &&
                        (actorCells == null || !actorCells.Contains(candidate));

                    if (blockedByOtherEntity)
                    {
                        continue;
                    }

                    result.Add(candidate);
                    reservedCells.Add(candidate);

                    if (result.Count >= requiredCount)
                    {
                        return result;
                    }
                }
            }
        }

        return result;
    }

    public void MoveTowards(UnitData unit, Vector2 targetPosition, float deltaTime)
    {
        Vector2 nextPosition = Vector2.MoveTowards(
            unit.Position,
            targetPosition,
            config.UnitMoveSpeed * deltaTime
        );

        ApplyPosition(unit, nextPosition);
        SyncCombatCell(unit);
    }

    public void Tick(float deltaTime)
    {
        foreach (UnitData unit in units)
        {
            if (unit.AttackTarget != null || unit.AttackUnitTarget != null || !unit.IsMoving)
            {
                continue;
            }

            Vector2 movementTarget = unit.Waypoints.Count > 0
                ? unit.Waypoints[0]
                : unit.TargetPosition;
            Vector2 nextPosition = Vector2.MoveTowards(
                unit.Position,
                movementTarget,
                config.UnitMoveSpeed * deltaTime
            );

            ApplyPosition(unit, nextPosition);

            if (Vector2.Distance(nextPosition, movementTarget) >= 0.01f)
            {
                continue;
            }

            ApplyPosition(unit, movementTarget);

            if (unit.Waypoints.Count > 0)
            {
                unit.Waypoints.RemoveAt(0);
            }

            unit.IsMoving = unit.Waypoints.Count > 0;

            if (!unit.IsMoving)
            {
                Debug.Log($"{unit.DisplayName} arrived at cell {unit.Cell}");
            }
        }
    }

    private void SyncCombatCell(UnitData unit)
    {
        Vector2Int currentCell = gridMap.WorldToCell(unit.Position);

        if (currentCell == unit.Cell)
        {
            return;
        }

        gridMap.Release(unit.Cell);
        unit.Cell = currentCell;
        unit.TargetCell = currentCell;
        gridMap.TryOccupy(currentCell);
    }

    private static void ApplyPosition(UnitData unit, Vector2 position)
    {
        if (unit.GameObject != null)
        {
            unit.GameObject.transform.position = new Vector3(position.x, position.y, 0f);
        }

        unit.Position = position;
    }
}
