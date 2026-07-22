# Art and audio assets

## Authored sprites

The entity sprites under `Assets/_Project/Art/Units` and `Assets/_Project/Art/Buildings` were generated with the built-in image-generation tool, then converted from a flat magenta chroma-key background to alpha PNGs with the local `remove_chroma_key.py` helper.

The shared prompt specification was:

- top-down orthographic 2D RTS game sprite;
- polished hand-painted sci-fi tactical-board-game style;
- chunky, readable silhouette at grid scale;
- player palette: blue armor and cyan lights;
- enemy palette: crimson armor and amber lights;
- factory palette: steel, industrial yellow, and small cyan accents;
- one centered subject with generous padding;
- no cast shadow, text, border, UI frame, logo, or watermark;
- perfectly flat `#ff00ff` background for local removal.

Player/enemy infantry and player/enemy bases were generated as paired variants so their silhouettes remain faction-readable. The factory was generated separately with a rectangular assembly-bay silhouette.

## Generated sound effects

`AegisPresentationPrefabGenerator` deterministically generates three short mono WAV assets:

- `Attack.wav`: descending energy shot;
- `Hit.wav`: low metallic impact;
- `ProductionComplete.wav`: three-note ascending confirmation.

The clips are assigned through `PresentationPrefabCatalog`. Runtime audio uses `PlayOneShot`, remains two-dimensional, and safely skips missing clips.

## Regeneration

Use `Aegis RTS > Generate Presentation Prefabs` to rebuild prefab references, importer settings, animation components, the overlay circle, grid material, and generated audio. The editor also performs a one-time upgrade when it detects old circle-based entity prefabs or missing audio references.
