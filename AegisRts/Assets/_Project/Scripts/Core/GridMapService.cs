using System.Collections.Generic;
using UnityEngine;

internal sealed class GridMapService
{
    private static readonly Vector2Int[] SpawnOffsets =
    {
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(-1, -1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(1, 1),
        new Vector2Int(0, -2),
        new Vector2Int(-2, 0),
        new Vector2Int(2, 0),
        new Vector2Int(0, 2)
    };

    private readonly HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();

    public int MapSize { get; }
    public float CellSize { get; }
    public float HalfSize => MapSize * CellSize / 2f;
    public ISet<Vector2Int> OccupiedCells => occupiedCells;

    public GridMapService(int mapSize, float cellSize)
    {
        MapSize = Mathf.Max(1, mapSize);
        CellSize = Mathf.Max(0.01f, cellSize);
    }

    public bool IsCellInside(Vector2Int cell)
    {
        return cell.x >= 0 &&
            cell.x < MapSize &&
            cell.y >= 0 &&
            cell.y < MapSize;
    }

    public bool IsWorldInside(Vector2 worldPosition)
    {
        return worldPosition.x >= -HalfSize &&
            worldPosition.x <= HalfSize &&
            worldPosition.y >= -HalfSize &&
            worldPosition.y <= HalfSize;
    }

    public Vector2Int WorldToCell(Vector2 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition.x + HalfSize) / CellSize);
        int y = Mathf.FloorToInt((worldPosition.y + HalfSize) / CellSize);

        return new Vector2Int(
            Mathf.Clamp(x, 0, MapSize - 1),
            Mathf.Clamp(y, 0, MapSize - 1)
        );
    }

    public Vector2 CellToWorld(Vector2Int cell)
    {
        return new Vector2(
            -HalfSize + cell.x * CellSize + CellSize / 2f,
            -HalfSize + cell.y * CellSize + CellSize / 2f
        );
    }

    public bool IsOccupied(Vector2Int cell)
    {
        return occupiedCells.Contains(cell);
    }

    public bool TryOccupy(Vector2Int cell)
    {
        return IsCellInside(cell) && occupiedCells.Add(cell);
    }

    public void Release(Vector2Int cell)
    {
        occupiedCells.Remove(cell);
    }

    public void Clear()
    {
        occupiedCells.Clear();
    }

    public bool TryFindOpenCellNear(Vector2Int originCell, out Vector2Int openCell)
    {
        foreach (Vector2Int offset in SpawnOffsets)
        {
            Vector2Int candidate = originCell + offset;

            if (IsCellInside(candidate) && !IsOccupied(candidate))
            {
                openCell = candidate;
                return true;
            }
        }

        openCell = originCell;
        return false;
    }
}
