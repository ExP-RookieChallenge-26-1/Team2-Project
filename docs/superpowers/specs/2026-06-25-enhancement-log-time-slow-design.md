# Enhancement Log Time Slow Design

## Goal

Change the card selection slowdown so game time starts at `0.2x` and then flows increasingly slowly while the enhancement card screen remains open. If the player waits long enough, gameplay should feel almost stopped.

## Scope

- Applies only to the `GameStateMachine.State.Enhancement` state.
- Keeps normal gameplay at `1.0x`.
- Keeps game over, game clear, and settings pause behavior unchanged.
- Does not change card selection UI, card rewards, or level-up logic.

## Current Context

`UserLevel.OpenUpgradeUI()` changes the game state to `Enhancement` before showing `EnhanceUI`. `GameManager.OnGameStateChanged()` currently sets `Time.timeScale = 0.2f` once when that state starts. `EnhanceUI.OnClickConfirm()` applies the selected card and changes the state back to `Playing`, which restores `Time.timeScale = 1f`.

## Design

`GameManager` owns the enhancement slowdown because it already owns time scale transitions for gameplay states.

When entering `Enhancement`:

- Reset a local enhancement elapsed timer to `0`.
- Set `Time.timeScale` to `0.2f`.
- Enable enhancement time-scale ticking.

While in `Enhancement`:

- Advance the timer with `Time.unscaledDeltaTime`, so the slowdown curve continues based on real player wait time instead of slowed game time.
- Recalculate `Time.timeScale` each frame from a logarithmic decay curve.
- Clamp the result to a very small minimum such as `0.001f`, so gameplay effectively stops without forcing every Unity time-dependent system to exactly zero.

When leaving `Enhancement`:

- Disable enhancement time-scale ticking.
- Reset the elapsed timer.
- Restore the next state's existing time scale behavior. `Playing` returns to `1f`; `GameOver` remains `0f`.

## Slowdown Curve

Use an inspector-configurable curve with these defaults:

```text
enhancementInitialTimeScale = 0.2
enhancementSlowdownRate = 2.0
enhancementMinimumTimeScale = 0.001

scale = initial / (1 + elapsedSeconds * slowdownRate)
scale = max(scale, minimum)
```

This starts exactly at `0.2` when elapsed time is `0`. Because time scale is the derivative of the logarithmic game-time curve, the accumulated gameplay time grows like a log function while the per-frame time scale keeps falling toward the minimum.

## Testing

Add focused edit-mode coverage for the time-scale formula:

- At `0` elapsed seconds, the calculated scale equals `0.2`.
- Later elapsed times produce smaller positive scales.
- Very large elapsed times never go below the configured minimum.

Existing settings pause tests continue to verify that hard pause and resume still work.

## Acceptance Criteria

- Opening the card selection screen starts at `0.2x` speed.
- Staying on the card selection screen makes gameplay time keep slowing down.
- The slowdown uses real elapsed time, not scaled game time.
- Confirming a card resumes normal gameplay speed.
- Game over, game clear, and settings pause behavior remain unchanged.
