# Parallax Background Implementation Plan

## Goal

Add the supplied layered PNG background to `GameScene` as a non-looping parallax scene backdrop. Every layer moves downward once across the full stage duration, with distant layers moving less than near layers.

## Files

- `Assets/Gameplay/World/ParallaxBackground.cs`
  - Runtime component that creates one `SpriteRenderer` child per layer.
  - Uses `stageDurationSeconds` and per-layer `travelDistance`, exposed as `Move Amount (Speed)` in the inspector.
  - Clamps progress to `0..1`, so layers stop at their final offset and never wrap.
- `Assets/Editor/ParallaxBackgroundSceneSetup.cs`
  - Imports/configures parallax sprites.
  - Adds and configures `ParallaxBackground` in `Assets/Scenes/GameScene.unity`.
- `Assets/Tests/EditMode/Editor/ParallaxBackgroundTests.cs`
  - Verifies clamped offset calculation.
  - Verifies one-shot stage-duration movement.
  - Verifies one sprite child per layer and far-to-near sorting order.
- `Assets/Art/Background/Parallax/`
  - Contains the provided layer PNGs.
  - Uses the original rock layer PNGs; no cropped rock layer is used.

## Layer Travel

Configured for `stageDurationSeconds = 180`:

- `1.png`: `0.00`
- `2_.png`: `0.40`
- `3_.png`: `0.65`
- `4_.png`: `0.90`
- `5_.png`: `1.15`
- `6_.png`: `1.35`
- `7_.png`: `1.65`

The configured render order is `1 -> 2 -> 3 -> 4 -> 5 -> 6 -> 7`.

## Verification

- `ParallaxBackgroundTests`: 3 passed.
- Full EditMode run still has unrelated `UISequenceTests` failures caused by missing UI sprite references.
