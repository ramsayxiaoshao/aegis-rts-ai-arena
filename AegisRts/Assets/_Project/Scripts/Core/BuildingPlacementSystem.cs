using UnityEngine;

internal sealed class BuildingPlacementSystem
{
    private readonly RtsGameConfig config;
    private readonly RtsEconomyProductionSystem economy;
    private readonly GridMapService gridMap;

    public BuildingPlacementSystem(
        RtsGameConfig gameConfig,
        RtsEconomyProductionSystem economySystem,
        GridMapService mapService
    )
    {
        config = gameConfig;
        economy = economySystem;
        gridMap = mapService;
    }

    public int GetCost(BuildingType buildingType)
    {
        return buildingType == BuildingType.Factory ? config.FactoryCost : 0;
    }

    public bool CanAfford(BuildingType buildingType)
    {
        return economy.CanAfford(GetCost(buildingType));
    }

    public bool CanPlace(
        BuildingType buildingType,
        Vector2 playerBasePosition,
        Vector2 worldPosition,
        Vector2Int cell
    )
    {
        return buildingType == BuildingType.Factory &&
            gridMap.IsCellInside(cell) &&
            gridMap.IsWorldInside(worldPosition) &&
            CanAfford(buildingType) &&
            Vector2.Distance(playerBasePosition, worldPosition) <= config.BuildRadius &&
            !gridMap.IsOccupied(cell);
    }

    public bool TryReserve(
        BuildingType buildingType,
        Vector2 playerBasePosition,
        Vector2 worldPosition,
        Vector2Int cell
    )
    {
        if (!CanPlace(buildingType, playerBasePosition, worldPosition, cell))
        {
            return false;
        }

        if (!gridMap.TryOccupy(cell))
        {
            return false;
        }

        if (economy.TrySpend(GetCost(buildingType)))
        {
            return true;
        }

        gridMap.Release(cell);
        return false;
    }
}
