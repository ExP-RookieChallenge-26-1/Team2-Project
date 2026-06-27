# Enhancement Log Time Slow Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make enhancement card selection start at `0.2x` time scale and slow further over real elapsed time using a logarithmic curve.

**Architecture:** Keep ownership in `GameManager`, where gameplay state time-scale transitions already live. Add a pure static calculation helper for edit-mode tests, then tick the enhancement slowdown in `Update()` only while the game state is `Enhancement`.

**Tech Stack:** Unity C#, NUnit EditMode tests, `Time.timeScale`, `Time.unscaledDeltaTime`, `Mathf.Log`.

---

### Task 1: Enhancement Time-Scale Curve

**Files:**
- Modify: `Assets/_Global/GameManager.cs`
- Modify: `Assets/Tests/EditMode/Editor/UISequenceTests.cs`

- [ ] **Step 1: Write failing edit-mode tests**

Add these tests to `Assets/Tests/EditMode/Editor/UISequenceTests.cs` before the helper methods:

```csharp
[Test]
public void EnhancementTimeScaleCurveStartsAtInitialScale()
{
    float scale = GameManager.CalculateEnhancementTimeScale(0f, 0.2f, 2f, 0.001f);

    Assert.That(scale, Is.EqualTo(0.2f).Within(0.0001f));
}

[Test]
public void EnhancementTimeScaleCurveSlowsDownOverElapsedTime()
{
    float earlyScale = GameManager.CalculateEnhancementTimeScale(1f, 0.2f, 2f, 0.001f);
    float laterScale = GameManager.CalculateEnhancementTimeScale(10f, 0.2f, 2f, 0.001f);

    Assert.That(earlyScale, Is.EqualTo(0.2f / 3f).Within(0.0001f));
    Assert.That(earlyScale, Is.LessThan(0.2f));
    Assert.That(laterScale, Is.LessThan(earlyScale));
    Assert.That(laterScale, Is.GreaterThan(0.001f));
}

[Test]
public void EnhancementTimeScaleCurveClampsToMinimumScale()
{
    float scale = GameManager.CalculateEnhancementTimeScale(1000000f, 0.2f, 2f, 0.05f);

    Assert.That(scale, Is.EqualTo(0.05f).Within(0.0001f));
}
```

- [ ] **Step 2: Run tests to verify failure**

Run:

```bash
/Applications/Unity/Hub/Editor/6000.3.7f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath . -runTests -testPlatform EditMode -testResults TestResults/editmode.xml -quit
```

Expected: compilation or test failure because `GameManager.CalculateEnhancementTimeScale` does not exist yet.

- [ ] **Step 3: Implement minimal code**

In `Assets/_Global/GameManager.cs`, add serialized fields and runtime state near the other serialized fields:

```csharp
[Header("Enhancement Time Slow")]
[SerializeField] private float enhancementInitialTimeScale = 0.2f;
[SerializeField] private float enhancementSlowdownRate = 2f;
[SerializeField] private float enhancementMinimumTimeScale = 0.001f;

private bool isEnhancementTimeSlowActive;
private float enhancementElapsedSeconds;
```

Update `Update()`:

```csharp
private void Update()
{
    this.Input.Tick();
    TickEnhancementTimeSlow();
    CheckBallState();
}
```

Update the `Enhancement` case in `OnGameStateChanged()`:

```csharp
case GameStateMachine.State.Enhancement:
    StartEnhancementTimeSlow();
    break;
```

Update the `Playing` and `GameOver` cases so they stop the enhancement ticker before assigning their time scale:

```csharp
case GameStateMachine.State.Playing:
    StopEnhancementTimeSlow();
    Time.timeScale = 1f;
    break;
```

```csharp
case GameStateMachine.State.GameOver:
    StopEnhancementTimeSlow();
    if (AudioManager.Instance != null)
        AudioManager.Instance.PlayGameOverSound();
```

Add these methods inside `GameManager`:

```csharp
private void StartEnhancementTimeSlow()
{
    this.enhancementElapsedSeconds = 0f;
    this.isEnhancementTimeSlowActive = true;
    Time.timeScale = CalculateEnhancementTimeScale(
        this.enhancementElapsedSeconds,
        this.enhancementInitialTimeScale,
        this.enhancementSlowdownRate,
        this.enhancementMinimumTimeScale);
}

private void StopEnhancementTimeSlow()
{
    this.isEnhancementTimeSlowActive = false;
    this.enhancementElapsedSeconds = 0f;
}

private void TickEnhancementTimeSlow()
{
    if (!this.isEnhancementTimeSlowActive || this.State.Current != GameStateMachine.State.Enhancement)
        return;

    this.enhancementElapsedSeconds += Time.unscaledDeltaTime;
    Time.timeScale = CalculateEnhancementTimeScale(
        this.enhancementElapsedSeconds,
        this.enhancementInitialTimeScale,
        this.enhancementSlowdownRate,
        this.enhancementMinimumTimeScale);
}

public static float CalculateEnhancementTimeScale(float elapsedSeconds, float initialScale, float slowdownRate, float minimumScale)
{
    float safeInitialScale = Mathf.Max(0f, initialScale);
    float safeMinimumScale = Mathf.Min(Mathf.Max(0f, minimumScale), safeInitialScale);
    float safeElapsedSeconds = Mathf.Max(0f, elapsedSeconds);
    float safeSlowdownRate = Mathf.Max(0f, slowdownRate);

    float denominator = 1f + safeElapsedSeconds * safeSlowdownRate;
    float scale = denominator > 0f ? safeInitialScale / denominator : safeInitialScale;
    return Mathf.Max(safeMinimumScale, scale);
}
```

- [ ] **Step 4: Run tests to verify pass**

Run:

```bash
/Applications/Unity/Hub/Editor/6000.3.7f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath . -runTests -testPlatform EditMode -testResults TestResults/editmode.xml -quit
```

Expected: EditMode tests pass, including the three new enhancement time-scale curve tests.

- [ ] **Step 5: Review changed files**

Run:

```bash
git diff -- Assets/_Global/GameManager.cs Assets/Tests/EditMode/Editor/UISequenceTests.cs docs/superpowers/specs/2026-06-25-enhancement-log-time-slow-design.md docs/superpowers/plans/2026-06-25-enhancement-log-time-slow.md
```

Expected: diff only contains the enhancement slowdown implementation, tests, and planning docs.
