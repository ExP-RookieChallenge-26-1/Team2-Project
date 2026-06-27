# Map Y Spawn And Final Boss Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Redistribute generated enemy spawn points by y-axis top-surface height and move boss spawning to `Map50` only.

**Architecture:** This is a serialized Unity data update backed by EditMode tests. Tests assert the map-data contract, then a deterministic YAML update moves existing spawn transforms and adds the final-map boss trigger without changing enemy stats or spawn counts.

**Tech Stack:** Unity text-serialized prefabs, NUnit EditMode tests, `AssetDatabase`, `Tilemap`.

---

### Task 1: Lock Enemy Spawn Placement Rules

**Files:**
- Modify: `Assets/Tests/EditMode/MapEnemySpawnPlacementTests.cs`

- [ ] **Step 1: Write the failing tests**

Add assertions that generated map enemy spawns sit on top-facing tiles and that their sorted top-surface rows match evenly sampled y-axis rows.

- [ ] **Step 2: Run tests to verify they fail**

Run: `/Applications/Unity/Hub/Editor/6000.3.7f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath "$PWD" -runTests -testPlatform EditMode -testFilter MapEnemySpawnPlacementTests -testResults TestResults/MapEnemySpawnPlacementTests.xml -quit`

Expected: FAIL before prefab data is regenerated.

- [ ] **Step 3: Regenerate spawn transform positions**

For each `Assets/Data/Maps/MapN.prefab`, parse tile top surfaces, sample unique surface y rows evenly, and update existing `SpawnPoint_N` `m_LocalPosition` values.

- [ ] **Step 4: Run tests to verify they pass**

Run the same Unity command. Expected: PASS.

### Task 2: Lock Final-Map Boss Spawn

**Files:**
- Modify: `Assets/Tests/EditMode/BossSpawnConfigurationTests.cs`
- Modify: `Assets/Data/Maps/Map50.prefab`

- [ ] **Step 1: Write the failing tests**

Change the boss map expectation to `Assets/Data/Maps/Map50.prefab` and assert `Map0` through `Map49` have no `BossSpawnTrigger` or `BossSpawnPoint`.

- [ ] **Step 2: Run tests to verify they fail**

Run: `/Applications/Unity/Hub/Editor/6000.3.7f1/Unity.app/Contents/MacOS/Unity -batchmode -projectPath "$PWD" -runTests -testPlatform EditMode -testFilter BossSpawnConfigurationTests -testResults TestResults/BossSpawnConfigurationTests.xml -quit`

Expected: FAIL before `Map50` receives the boss trigger.

- [ ] **Step 3: Add `BossSpawnPoint` and `BossSpawnTrigger` to `Map50`**

Append a child `BossSpawnPoint` transform to `Map50`, add a `BossSpawnTrigger` component to the root component list, and serialize references to `Assets/Gameplay/Cowking/CowKing.prefab` and the new spawn point.

- [ ] **Step 4: Run tests to verify they pass**

Run the same Unity command. Expected: PASS.
