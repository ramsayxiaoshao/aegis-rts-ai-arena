# Runtime architecture

The prototype runtime is being split incrementally so gameplay stays playable during the transition.

## Current components

- `GameBootstrap` coordinates match state, world creation, input, UI, combat, production, and the Arena facade.
- `GameDomain` owns shared team, entity-type, building, and unit runtime models.
- `RtsCameraController` owns camera setup, movement, zoom, and map bounds.
- `RtsGameConfig` is the ScriptableObject source for map, economy, combat, production, AI, movement, and camera balance values.
- `ArenaGameRules` contains deterministic economy and damage rules.
- `GridPathfinder` contains deterministic grid path search.
- `ArenaContracts` defines serializable observations, actions, entities, and results.

The default balance asset is `Assets/_Project/Resources/RtsGameConfig.asset`. `GameBootstrap` loads it at startup and falls back to scene values only if the asset is missing.

## Next extractions

The remaining responsibilities should leave `GameBootstrap` in this order:

1. selection and command input;
2. production and economy;
3. combat and entity lifecycle;
4. Arena observation/action orchestration;
5. IMGUI presentation, to be replaced by UI Toolkit or uGUI.

Each extraction should retain the existing Arena contract and add Edit Mode or Play Mode coverage before behavior changes are introduced.
