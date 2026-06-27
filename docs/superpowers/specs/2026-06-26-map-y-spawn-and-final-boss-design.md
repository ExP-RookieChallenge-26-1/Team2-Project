# Map Y Spawn And Final Boss Design

## Goal

Update `Map0` through `Map50` so enemy spawn points are distributed evenly by height while remaining on top-facing tile surfaces. Configure boss spawning only on the final generated map, `Map50`.

## Design

Enemy spawn placement is data-driven in `Assets/Data/Maps/Map*.prefab`. Each map keeps its existing enemy count, spawn point objects, enemy spawn data assets, and `WorldEnemySpawner` references. A deterministic pass reads each tilemap's top-facing surface cells, sorts unique surface heights, samples those heights evenly for the map's spawn count, and moves each existing `SpawnPoint_N` transform onto a selected top surface at that height. Within the same height band, x placement is secondary and chosen to avoid repeatedly using the same horizontal side when options exist.

The top-surface rule means the ground cell below a spawn must contain a tile and the cell above it must be empty. It does not require the spawn to sit on the highest cell in a connected terrain component, because that would conflict with y-axis distribution across lower platforms.

Boss spawning remains implemented by `BossSpawnTrigger`. Generated maps `Map0` through `Map49` must not contain a `BossSpawnTrigger` or `BossSpawnPoint`; `Map50` contains both and references the existing CowKing prefab.

## Verification

EditMode tests cover the data contract:

- Every generated enemy spawn point resolves to a top-facing tile surface.
- Generated map spawn heights match evenly sampled top-surface height rows.
- Only `Map50` has a boss spawn trigger and configured boss spawn point.
