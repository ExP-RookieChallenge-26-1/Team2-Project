# GameScene Parallax Background Design

## Goal

Add a one-shot parallax background to `Assets/Scenes/GameScene.unity` using the layered PNG files from `drive-download-20260625T102414Z-3-001.zip`. The background should create depth by moving distant layers slightly and near layers more noticeably over the full stage duration, without looping.

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

The new parallax background follows the same visual movement direction as the world, but it is not tied directly to chunk scroll speed. Each layer completes one very slow downward movement over the configured stage duration.

## Architecture

Create a render-only background system:

- Add background image assets under `Assets/Art/Background/Parallax/`.
- Add a new `ParallaxBackground` MonoBehaviour under `Assets/Gameplay/World/`.
- Add a `ParallaxBackground` GameObject to `GameScene.unity`.
- Use `SpriteRenderer` children on the `Background` sorting layer.

Each configured layer contains:

- A sprite reference.
- A total downward `travelDistance` in world units, shown in the inspector as `Move Amount (Speed)`.
- A `sortingOrder`.
- A single runtime sprite instance.

The component tracks elapsed stage time locally and maps it to a clamped `0..1` progress value using `stageDurationSeconds`.

## Layering

Render order should follow visual depth:

1. `1.png`: lowest layer in the source stack. Static sky/background plate.
2. `2_.png`
3. `3_.png`
4. `4_.png`
5. `5_.png`
6. `6_.png`
7. `7_.png`: highest layer in the source stack. Largest movement.

All layers render behind terrain and gameplay objects by using the existing `Background` sorting layer. Sorting orders increase from far to near.

## Scrolling Behavior

The background moves downward once. For each layer:

```text
progress = clamp01(elapsedSeconds / stageDurationSeconds)
layerOffset = Vector3.down * travelDistance * progress
layerPosition = layerStartPosition + layerOffset
```

The sprite should be scaled to fill the camera height. The provided PNGs are `1080x1920`, matching a 9:16 portrait ratio. With the current camera height of `10` units, the sprite width becomes `5.625` units, which fits the scene width.

`1.png` stays static with `Move Amount (Speed) = 0`, so the sky does not scroll.

All rock layers use the original provided images. They move downward once and can leave the bottom of the screen naturally.

## Proposed Travel Distances

- `1.png`: `0.00`
- `2_.png`: `0.40`
- `3_.png`: `0.65`
- `4_.png`: `0.90`
- `5_.png`: `1.15`
- `6_.png`: `1.35`
- `7_.png`: `1.65`

With `stageDurationSeconds` set to `180`, these values keep the movement very slow while keeping the sky fixed. Designers can adjust these numbers directly in the `ParallaxBackground` inspector.

## Error Handling

- Missing layer sprites are skipped with a warning.
- Empty layer configuration disables runtime setup without throwing.
- Missing camera uses the configured world height fallback.

## Testing

Add focused edit-mode tests for `ParallaxBackground`:

- Layers keep far-to-near sorting order.
- Offset calculation clamps stage progress and uses each layer's total travel distance.
- Ticking past the configured duration leaves layers at their final position instead of looping.

Run Unity edit-mode tests or at least compile through Unity batchmode when available.

## Acceptance Criteria

- `GameScene` shows the supplied layered artwork behind the current gameplay map.
- Far, middle, and near layers scroll at visibly different speeds.
- Background layers move downward once over the stage and do not loop.
- Existing map chunk scrolling, collisions, paddle, ball, enemies, and UI keep their existing behavior.
- The implementation is configurable in the inspector without code changes.
