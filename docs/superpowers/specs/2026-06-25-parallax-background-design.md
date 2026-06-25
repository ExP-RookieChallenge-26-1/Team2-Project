# GameScene Parallax Background Design

## Goal

Add a scrolling parallax background to `Assets/Scenes/GameScene.unity` using the seven layered PNG files from `drive-download-20260625T102414Z-3-001.zip`. The background should create depth by moving distant layers slowly and near layers faster while the current gameplay world continues scrolling downward.

## Scope

- Applies only to `GameScene.unity`.
- Uses the provided layered background images:
  - `1.png`
  - `2_.png`
  - `3_.png`
  - `4_.png`
  - `5_.png`
  - `6_.png`
  - `7_.png`
- Does not change map chunk spawning, tilemap collisions, gameplay physics, UI, or camera behavior.
- Does not add colliders to background layers.

## Current Context

`GameScene` uses an orthographic camera with size `5`, so the visible vertical world height is `10` units. The gameplay world is controlled by `World`, `WorldSpawner`, and `WorldScroller`. `WorldScroller` moves map chunks downward using `GameManager.Instance.WorldStats.ScrollSpeed`.

The new parallax background should follow the same visual movement direction as the world, but with different layer speed multipliers.

## Architecture

Create a render-only background system:

- Add background image assets under `Assets/Art/Background/Parallax/`.
- Add a new `ParallaxBackground` MonoBehaviour under `Assets/Gameplay/World/`.
- Add a `ParallaxBackground` GameObject to `GameScene.unity`.
- Use `SpriteRenderer` children on the `Background` sorting layer.

Each configured layer contains:

- A sprite reference.
- A `speedMultiplier`.
- A `sortingOrder`.
- A runtime pair of sprite instances used for vertical wraparound.

The component reads the base scroll speed from `GameManager.Instance.WorldStats.ScrollSpeed` when available. If the game manager is unavailable in edit/test context, it falls back to a serialized `fallbackScrollSpeed`.

## Layering

Render order should follow visual depth:

1. `1.png`: sky, moon, clouds. Slowest movement.
2. `4_.png` and `3_.png`: distant rocks. Slow movement.
3. `5_.png`: main midground rock tower. Medium movement.
4. `2_.png`: mid-to-near ground path. Faster movement.
5. `6_.png` and `7_.png`: foreground rocks. Fastest movement.

All layers render behind terrain and gameplay objects by using the existing `Background` sorting layer. Sorting orders increase from far to near.

## Scrolling Behavior

The background scrolls downward. For each layer:

```text
layerSpeed = baseWorldScrollSpeed * speedMultiplier
layerPosition += Vector3.down * layerSpeed * deltaTime
```

Each layer uses two sprite copies stacked vertically. When one copy moves below the lower wrap threshold, it is repositioned above the other copy. This keeps the background continuous without spawning or destroying objects during play.

The sprite should be scaled to fill the camera height. The provided PNGs are `1080x1920`, matching a 9:16 portrait ratio. With the current camera height of `10` units, the sprite width becomes `5.625` units, which fits the scene width.

## Proposed Speed Multipliers

- `1.png`: `0.05`
- `4_.png`: `0.12`
- `3_.png`: `0.18`
- `5_.png`: `0.45`
- `2_.png`: `0.75`
- `6_.png`: `0.95`
- `7_.png`: `1.10`

These values keep the far sky nearly static, make midground rocks drift more noticeably, and let the foreground sell the motion.

## Error Handling

- Missing layer sprites are skipped with a warning.
- Empty layer configuration disables runtime setup without throwing.
- Missing camera uses the configured world height fallback.
- Missing `GameManager` uses `fallbackScrollSpeed`.

## Testing

Add focused edit-mode tests for `ParallaxBackground`:

- Layers keep far-to-near sorting order.
- Speed calculation uses the base scroll speed and multiplier.
- Wraparound moves a sprite copy back above the active visible span.

Run Unity edit-mode tests or at least compile through Unity batchmode when available.

## Acceptance Criteria

- `GameScene` shows the supplied layered artwork behind the current gameplay map.
- Far, middle, and near layers scroll at visibly different speeds.
- Background layers loop continuously without visible gaps during normal play.
- Existing map chunk scrolling, collisions, paddle, ball, enemies, and UI keep their existing behavior.
- The implementation is configurable in the inspector without code changes.
