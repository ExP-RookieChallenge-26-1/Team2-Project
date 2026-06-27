#if UNITY_EDITOR
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class CowKingAnimationAssetTests
{
    private const string CowKingAnimatorControllerPath = "Assets/Gameplay/Cowking/CowKingAnimator.controller";
    private const string CowKingAttackAnimationPath = "Assets/Gameplay/Cowking/CowKing_Attack.anim";
    private const string CowKingAttackReadyAnimationPath = "Assets/Gameplay/Cowking/CowKing_AttackReady.anim";
    private const string CowKingEntryAnimationPath = "Assets/Gameplay/Cowking/Cowking_Entry.anim";
    private const string CowKingPrefabPath = "Assets/Gameplay/Cowking/CowKing.prefab";
    private const string CowKingBreathPrefabPath = "Assets/Gameplay/Cowking/Breath/CowKingBreath.prefab";
    private const float ExpectedCowKingLocalScale = 0.5333333f;
    private const float ExpectedBreathSpawnLocalX = 0f;
    private const float ExpectedBreathSpawnLocalY = -7.8475f;
    private const float ExpectedAttackCenterTolerancePixels = 4f;
    private const float ExpectedMouthFlameCenterTolerancePixels = 8f;
    private static readonly string[] CowKingAttackBodyFramePaths =
    {
        "Assets/Art/CowKing/Attack/우마왕어택01.png",
        "Assets/Art/CowKing/Attack/우마왕어택02.png",
        "Assets/Art/CowKing/Attack/우마왕어택03.png",
        "Assets/Art/CowKing/Attack/우마왕어택04.png",
        "Assets/Art/CowKing/Attack/우마왕어택05.png",
        "Assets/Art/CowKing/Attack/우마왕어택06.png",
        "Assets/Art/CowKing/Attack/우마왕어택07.png",
        "Assets/Art/CowKing/Attack/우마왕어택08.png",
        "Assets/Art/CowKing/Attack/우마왕어택09.png",
        "Assets/Art/CowKing/Attack/우마왕어택10.png",
    };
    private static readonly RectInt[] CowKingAttackBodyFrameRects =
    {
        new RectInt(660, 1931, 493, 484),
        new RectInt(660, 1944, 493, 471),
        new RectInt(660, 1947, 493, 468),
        new RectInt(660, 1947, 493, 468),
        new RectInt(660, 1947, 493, 468),
        new RectInt(658, 1956, 495, 459),
        new RectInt(658, 1956, 495, 459),
        new RectInt(658, 1956, 495, 459),
        new RectInt(468, 1956, 685, 459),
        new RectInt(468, 1956, 685, 459),
    };
    private static readonly string[] CowKingBreathFramePaths =
    {
        "Assets/Art/CowKing/Attack/breath/11.png",
        "Assets/Art/CowKing/Attack/breath/12.png",
        "Assets/Art/CowKing/Attack/breath/13.png",
        "Assets/Art/CowKing/Attack/breath/14.png",
        "Assets/Art/CowKing/Attack/breath/15.png",
        "Assets/Art/CowKing/Attack/breath/16.png",
        "Assets/Art/CowKing/Attack/breath/17.png",
        "Assets/Art/CowKing/Attack/breath/18.png",
        "Assets/Art/CowKing/Attack/breath/19.png",
        "Assets/Art/CowKing/Attack/breath/20.png",
    };

    [Test]
    public void CowKingAnimatorMotionReferencesResolve()
    {
        string controller = ReadProjectFile(CowKingAnimatorControllerPath);
        MatchCollection motionReferences = Regex.Matches(
            controller,
            @"m_Motion: \{fileID: 7400000, guid: ([0-9a-f]{32}), type: 2\}");

        Assert.That(motionReferences.Count, Is.GreaterThan(0));

        foreach (Match motionReference in motionReferences)
        {
            string guid = motionReference.Groups[1].Value;
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            Assert.That(assetPath, Is.Not.Empty, $"Animator motion GUID {guid} does not resolve to an asset.");
            Assert.That(
                AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath),
                Is.Not.Null,
                $"Animator motion GUID {guid} resolves to {assetPath}, but it is not an AnimationClip.");
        }
    }

    [Test]
    public void CowKingEntryStateUsesEntryAnimationClip()
    {
        string controller = ReadProjectFile(CowKingAnimatorControllerPath);
        string entryGuid = AssetDatabase.AssetPathToGUID(CowKingEntryAnimationPath);
        Assert.That(entryGuid, Is.Not.Empty, $"{CowKingEntryAnimationPath} guid is missing.");

        Match entryState = Regex.Match(
            controller,
            @"m_Name: Cowking_Entry[\s\S]*?m_Motion: \{fileID: 7400000, guid: ([0-9a-f]{32}), type: 2\}");

        Assert.That(entryState.Success, Is.True, "Cowking_Entry state is missing.");
        Assert.That(entryState.Groups[1].Value, Is.EqualTo(entryGuid));
    }

    [Test]
    public void CowKingBreathRendersOnEffectLayer()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CowKingBreathPrefabPath);
        Assert.That(prefab, Is.Not.Null, $"{CowKingBreathPrefabPath} is missing.");

        SpriteRenderer renderer = prefab.GetComponent<SpriteRenderer>();
        Assert.That(renderer, Is.Not.Null, $"{CowKingBreathPrefabPath} is missing a SpriteRenderer.");
        Assert.That(renderer.sortingLayerID, Is.EqualTo(SortingLayer.NameToID("Effect")));
        Assert.That(renderer.sortingOrder, Is.GreaterThanOrEqualTo(0));
    }

    [Test]
    public void CowKingBreathColliderMatchesBellFlareBeam()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CowKingBreathPrefabPath);
        Assert.That(prefab, Is.Not.Null, $"{CowKingBreathPrefabPath} is missing.");

        PolygonCollider2D collider = prefab.GetComponent<PolygonCollider2D>();
        Assert.That(collider, Is.Not.Null, $"{CowKingBreathPrefabPath} is missing a PolygonCollider2D.");
        Assert.That(collider.pathCount, Is.EqualTo(1));

        Vector2[] points = collider.GetPath(0);
        Assert.That(points.Length, Is.GreaterThanOrEqualTo(20));

        float minX = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float minY = float.PositiveInfinity;
        float maxY = float.NegativeInfinity;

        foreach (Vector2 point in points)
        {
            minX = Mathf.Min(minX, point.x);
            maxX = Mathf.Max(maxX, point.x);
            minY = Mathf.Min(minY, point.y);
            maxY = Mathf.Max(maxY, point.y);
        }

        Assert.That(minY, Is.LessThanOrEqualTo(-10.8f));
        Assert.That(maxY, Is.GreaterThanOrEqualTo(9f));
        Assert.That(maxX - minX, Is.InRange(2.45f, 2.8f));
    }

    [Test]
    public void CowKingScaleAndBreathSpawnPointKeepVerticalBeamCenterOffset()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CowKingPrefabPath);
        Assert.That(prefab, Is.Not.Null, $"{CowKingPrefabPath} is missing.");

        Assert.That(prefab.transform.localScale.x, Is.EqualTo(ExpectedCowKingLocalScale).Within(0.0001f));
        Assert.That(prefab.transform.localScale.y, Is.EqualTo(ExpectedCowKingLocalScale).Within(0.0001f));
        Assert.That(prefab.transform.localScale.z, Is.EqualTo(1f).Within(0.0001f));

        Transform breathSpawnPoint = prefab.transform.Find("BreathSpawnPoint");
        Assert.That(breathSpawnPoint, Is.Not.Null, $"{CowKingPrefabPath} is missing BreathSpawnPoint.");
        Assert.That(breathSpawnPoint.localPosition.x, Is.EqualTo(ExpectedBreathSpawnLocalX).Within(0.001f));
        Assert.That(breathSpawnPoint.localPosition.y, Is.EqualTo(ExpectedBreathSpawnLocalY).Within(0.001f));
    }

    [Test]
    public void CowKingAttackBodyFramesKeepExistingSliceImport()
    {
        for (int i = 0; i < CowKingAttackBodyFramePaths.Length; i++)
        {
            string framePath = CowKingAttackBodyFramePaths[i];
            RectInt expectedRect = CowKingAttackBodyFrameRects[i];
            string meta = ReadProjectFile($"{framePath}.meta");

            Assert.That(meta, Does.Match(@"spriteMeshType: 1\b"), $"{framePath} should keep its existing sliced import; alignment is fixed in the PNG pixels.");
            Assert.That(meta, Does.Match($@"x: {expectedRect.x}\b"), $"{framePath} sprite rect x changed unexpectedly.");
            Assert.That(meta, Does.Match($@"y: {expectedRect.y}\b"), $"{framePath} sprite rect y changed unexpectedly.");
            Assert.That(meta, Does.Match($@"width: {expectedRect.width}\b"), $"{framePath} sprite rect width changed unexpectedly.");
            Assert.That(meta, Does.Match($@"height: {expectedRect.height}\b"), $"{framePath} sprite rect height changed unexpectedly.");
            Assert.That(meta, Does.Match(@"alignment: 0\b"), $"{framePath} must keep Unity's center sprite alignment.");
            Assert.That(meta, Does.Match(@"pivot: \{x: 0, y: 0\}"), $"{framePath} should keep the existing sliced pivot data; visual alignment belongs in the PNG pixels.");
        }
    }

    [Test]
    public void CowKingAttackBodyPixelsStayCenteredOnSpritePivot()
    {
        for (int i = 0; i < CowKingAttackBodyFramePaths.Length; i++)
        {
            string framePath = CowKingAttackBodyFramePaths[i];
            RectInt spriteRect = CowKingAttackBodyFrameRects[i];
            Texture2D texture = LoadTexture(framePath);

            try
            {
                Color32[] pixels = texture.GetPixels32();
                FindDarkBodyBounds(pixels, texture.width, texture.height, out int minX, out int maxX, out _, out _);
                FindMouthFlameBounds(pixels, texture.width, texture.height, out int flameMinX, out int flameMaxX, out _, out _);

                float expectedCenterX = spriteRect.x + (spriteRect.width - 1) * 0.5f;
                float darkBodyCenterX = (minX + maxX) * 0.5f;
                float mouthFlameCenterX = (flameMinX + flameMaxX) * 0.5f;

                Assert.That(darkBodyCenterX, Is.EqualTo(expectedCenterX).Within(ExpectedAttackCenterTolerancePixels), $"{framePath} dark body pixels are horizontally offset from the sprite pivot.");
                Assert.That(mouthFlameCenterX, Is.EqualTo(expectedCenterX).Within(ExpectedMouthFlameCenterTolerancePixels), $"{framePath} mouth flame pixels are horizontally offset from the sprite pivot.");
                Assert.That(maxX - minX, Is.InRange(485, 500), $"{framePath} dark body width changed unexpectedly.");
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }
    }

    [Test]
    public void CowKingBreathFramesDoNotUseMouthSocketStart()
    {
        foreach (string framePath in CowKingBreathFramePaths)
        {
            Texture2D texture = LoadTexture(framePath);

            try
            {
                Color32[] pixels = texture.GetPixels32();
                FindOpaqueBounds(pixels, texture.width, texture.height, out int minX, out int maxX, out _, out int maxY);

                int darkOpaquePixels = 0;
                int bandBottom = Mathf.Max(0, maxY - 90);

                for (int y = bandBottom; y <= maxY; y++)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        Color32 pixel = pixels[y * texture.width + x];

                        if (pixel.a > 180 && pixel.r < 45 && pixel.g < 45 && pixel.b < 45)
                        {
                            darkOpaquePixels++;
                        }
                    }
                }

                Assert.That(
                    darkOpaquePixels,
                    Is.LessThanOrEqualTo(800),
                    $"{framePath} has too many dark pixels near the breath start; this should only be the flame outline.");
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }
    }

    [Test]
    public void CowKingBreathFramesUseBellFlareIntoParallelBody()
    {
        foreach (string framePath in CowKingBreathFramePaths)
        {
            Texture2D texture = LoadTexture(framePath);

            try
            {
                Color32[] pixels = texture.GetPixels32();
                FindOpaqueBounds(pixels, texture.width, texture.height, out int minX, out int maxX, out int minY, out int maxY);

                Assert.That(maxY - minY, Is.GreaterThanOrEqualTo(2230), $"{framePath} must keep the original full-length breath body plus the raised bell flare.");
                Assert.That(maxX - minX, Is.InRange(240, 280), $"{framePath} must keep the narrow bell-flare silhouette instead of a wide cone.");
                AssertBellFlareIntoParallelBody(pixels, texture.width, minX, maxX, maxY, framePath);
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }
    }

    [Test]
    public void CowKingBreathFramesUseStableFullRectSpriteImport()
    {
        foreach (string framePath in CowKingBreathFramePaths)
        {
            string meta = ReadProjectFile($"{framePath}.meta");

            Assert.That(meta, Does.Match(@"spriteMode: 1\b"), $"{framePath} must be imported as a single sprite.");
            Assert.That(meta, Does.Match(@"spriteMeshType: 0\b"), $"{framePath} must use Full Rect mesh so frame alpha bounds cannot move the visual center.");
            Assert.That(meta, Does.Not.Match(@"internalIDToNameTable:\s*\n\s*-"), $"{framePath} must not keep stale sliced sprite IDs.");
            Assert.That(meta, Does.Not.Match(@"sprites:\s*\n\s*-"), $"{framePath} must not keep stale sliced sprite rects.");
        }
    }

    [Test]
    public void CowKingBreathFramesKeepIdenticalOpaqueBounds()
    {
        Texture2D firstTexture = LoadTexture(CowKingBreathFramePaths[0]);

        try
        {
            Color32[] firstPixels = firstTexture.GetPixels32();
            FindOpaqueBounds(
                firstPixels,
                firstTexture.width,
                firstTexture.height,
                out int expectedMinX,
                out int expectedMaxX,
                out int expectedMinY,
                out int expectedMaxY);

            foreach (string framePath in CowKingBreathFramePaths)
            {
                Texture2D texture = LoadTexture(framePath);

                try
                {
                    Color32[] pixels = texture.GetPixels32();
                    FindOpaqueBounds(
                        pixels,
                        texture.width,
                        texture.height,
                        out int minX,
                        out int maxX,
                        out int minY,
                        out int maxY);

                    Assert.That(minX, Is.EqualTo(expectedMinX), $"{framePath} min opaque x differs from the first frame.");
                    Assert.That(maxX, Is.EqualTo(expectedMaxX), $"{framePath} max opaque x differs from the first frame.");
                    Assert.That(minY, Is.EqualTo(expectedMinY), $"{framePath} min opaque y differs from the first frame.");
                    Assert.That(maxY, Is.EqualTo(expectedMaxY), $"{framePath} max opaque y differs from the first frame.");
                }
                finally
                {
                    Object.DestroyImmediate(texture);
                }
            }
        }
        finally
        {
            Object.DestroyImmediate(firstTexture);
        }
    }

    [Test]
    public void CowKingAttackAnimationStaysUntilBreathEnds()
    {
        string controller = ReadProjectFile(CowKingAnimatorControllerPath);

        Assert.That(
            controller,
            Does.Match(@"m_Name: IsBreathing\s+m_Type: 4"),
            "CowKing animator needs an IsBreathing bool parameter to hold the attack animation during the breath.");

        string attackStateBlock = FindAnimatorStateBlock(controller, "CowKing_Attack");
        Match transitionReference = Regex.Match(attackStateBlock, @"m_Transitions:\s+- \{fileID: (-?\d+)\}");
        Assert.That(transitionReference.Success, Is.True, "CowKing_Attack needs an exit transition.");

        string exitTransition = FindAnimatorTransitionBlock(controller, transitionReference.Groups[1].Value);
        Assert.That(exitTransition, Does.Contain("m_ConditionEvent: IsBreathing"));
        Assert.That(exitTransition, Does.Contain("m_ConditionMode: 2"), "CowKing_Attack should exit only when IsBreathing is false.");
    }

    [Test]
    public void CowKingAttackReadyWaitsForBreathBeforeAttackState()
    {
        string controller = ReadProjectFile(CowKingAnimatorControllerPath);
        string attackReadyStateBlock = FindAnimatorStateBlock(controller, "CowKing_AttackReady");
        Match transitionReference = Regex.Match(attackReadyStateBlock, @"m_Transitions:\s+- \{fileID: (-?\d+)\}");
        Assert.That(transitionReference.Success, Is.True, "CowKing_AttackReady needs a transition to CowKing_Attack.");

        string attackTransition = FindAnimatorTransitionBlock(controller, transitionReference.Groups[1].Value);
        Assert.That(attackTransition, Does.Contain("m_ConditionEvent: IsBreathing"));
        Assert.That(attackTransition, Does.Contain("m_ConditionMode: 1"), "CowKing_AttackReady should enter CowKing_Attack only when IsBreathing is true.");
    }

    [Test]
    public void CowKingBreathingTransitionsAreImmediate()
    {
        string controller = ReadProjectFile(CowKingAnimatorControllerPath);
        MatchCollection transitions = Regex.Matches(
            controller,
            @"--- !u!1101 &-?\d+[\s\S]*?(?=\n--- !u!|\z)");

        int breathingTransitionCount = 0;

        foreach (Match transition in transitions)
        {
            string transitionBlock = transition.Value;
            if (!transitionBlock.Contains("m_ConditionEvent: IsBreathing"))
                continue;

            breathingTransitionCount++;
            Assert.That(transitionBlock, Does.Match(@"m_TransitionDuration: 0\b"));
            Assert.That(transitionBlock, Does.Match(@"m_HasExitTime: 0\b"));
        }

        Assert.That(breathingTransitionCount, Is.EqualTo(2));
    }

    [Test]
    public void CowKingAttackAnimationLoopsWhileBreathing()
    {
        string animationClip = ReadProjectFile(CowKingAttackAnimationPath);
        Assert.That(animationClip, Does.Match(@"m_LoopTime: 1\b"));
    }

    [Test]
    public void CowKingAttackAnimationReusesAttackReadyFinalBodyFrameWhileBreathing()
    {
        string animationClip = ReadProjectFile(CowKingAttackAnimationPath);
        string attackReadyClip = ReadProjectFile(CowKingAttackReadyAnimationPath);
        string attackReadyFinalSpriteReference = FindLastSpriteReference(attackReadyClip);
        string croppedAttackGuid = AssetDatabase.AssetPathToGUID("Assets/Art/CowKing/Attack/Png3/11-21.png");
        string droppedMouthFlameGuid = AssetDatabase.AssetPathToGUID("Assets/Art/CowKing/Attack/Png3/22.png");

        Assert.That(croppedAttackGuid, Is.Not.Empty);
        Assert.That(droppedMouthFlameGuid, Is.Not.Empty);
        Assert.That(
            animationClip,
            Does.Contain($"value: {attackReadyFinalSpriteReference}"),
            "The sustained attack must keep the same sprite rect/pivot as the final ready frame so the cow body cannot jump.");
        Assert.That(
            animationClip,
            Does.Not.Contain($"guid: {croppedAttackGuid}"),
            "The cropped Png3 body sprite uses a different transparent canvas and shifts the cow during the sustained breath.");
        Assert.That(
            animationClip,
            Does.Not.Contain($"guid: {droppedMouthFlameGuid}"),
            "The 22.png attack frame drops the mouth flame and shifts the visible cow during the sustained breath.");
    }

    private static void AssertBellFlareIntoParallelBody(
        Color32[] pixels,
        int width,
        int minX,
        int maxX,
        int maxY,
        string framePath)
    {
        float centerX = (minX + maxX) * 0.5f;
        int maxTopWidth = 0;
        int parallelWidth = 0;
        int minParallelWidth = int.MaxValue;
        float maxImbalance = 0f;
        float bodyCenterX = 0f;
        int bodyCenterSamples = 0;
        float maxCenterDrift = 0f;

        for (int y = maxY; y >= Mathf.Max(0, maxY - 430); y--)
        {
            int rowMinX = width;
            int rowMaxX = -1;

            for (int x = minX; x <= maxX; x++)
            {
                if (pixels[y * width + x].a <= 10)
                {
                    continue;
                }

                rowMinX = Mathf.Min(rowMinX, x);
                rowMaxX = Mathf.Max(rowMaxX, x);
            }

            if (rowMaxX < rowMinX)
            {
                continue;
            }

            int rowWidth = rowMaxX - rowMinX + 1;
            maxTopWidth = Mathf.Max(maxTopWidth, rowWidth);

            if (y <= maxY - 320)
            {
                parallelWidth = Mathf.Max(parallelWidth, rowWidth);
                minParallelWidth = Mathf.Min(minParallelWidth, rowWidth);
                bodyCenterX += (rowMinX + rowMaxX) * 0.5f;
                bodyCenterSamples++;
            }

            if (rowWidth < 8)
            {
                continue;
            }

            float leftDistance = centerX - rowMinX;
            float rightDistance = rowMaxX - centerX;
            maxImbalance = Mathf.Max(maxImbalance, Mathf.Abs(leftDistance - rightDistance));
        }

        Assert.That(bodyCenterSamples, Is.GreaterThan(0), $"{framePath} has no measurable parallel body join region.");
        bodyCenterX /= bodyCenterSamples;

        for (int y = maxY; y >= Mathf.Max(0, maxY - 430); y--)
        {
            int rowMinX = width;
            int rowMaxX = -1;

            for (int x = minX; x <= maxX; x++)
            {
                if (pixels[y * width + x].a <= 10)
                {
                    continue;
                }

                rowMinX = Mathf.Min(rowMinX, x);
                rowMaxX = Mathf.Max(rowMaxX, x);
            }

            if (rowMaxX < rowMinX)
            {
                continue;
            }

            int rowWidth = rowMaxX - rowMinX + 1;

            if (rowWidth < 8)
            {
                continue;
            }

            float rowCenterX = (rowMinX + rowMaxX) * 0.5f;
            maxCenterDrift = Mathf.Max(maxCenterDrift, Mathf.Abs(rowCenterX - bodyCenterX));
        }

        Assert.That(maxTopWidth, Is.LessThanOrEqualTo(95), $"{framePath} has a protruding or too-wide bell flare.");
        Assert.That(maxTopWidth - parallelWidth, Is.LessThanOrEqualTo(8), $"{framePath} bell flare should not narrow again before it reaches the body.");
        Assert.That(parallelWidth - minParallelWidth, Is.LessThanOrEqualTo(2), $"{framePath} bell flare/body join width changes too abruptly.");
        Assert.That(maxCenterDrift, Is.LessThanOrEqualTo(2f), $"{framePath} bell flare is not centered on the body.");
        Assert.That(maxImbalance, Is.LessThanOrEqualTo(6f), $"{framePath} bell flare must stay left/right symmetric.");
    }

    private static string FindAnimatorStateBlock(string controller, string stateName)
    {
        Match stateBlock = Regex.Match(
            controller,
            $@"--- !u!1102 &-?\d+(?:(?!\n--- !u!)[\s\S])*?m_Name: {Regex.Escape(stateName)}\r?\n(?:(?!\n--- !u!)[\s\S])*");

        Assert.That(stateBlock.Success, Is.True, $"Animator state {stateName} was not found.");
        return stateBlock.Value;
    }

    private static string FindAnimatorTransitionBlock(string controller, string transitionFileId)
    {
        Match transitionBlock = Regex.Match(
            controller,
            $@"--- !u!1101 &{Regex.Escape(transitionFileId)}[\s\S]*?(?=\n--- !u!|\z)");

        Assert.That(transitionBlock.Success, Is.True, $"Animator transition {transitionFileId} was not found.");
        return transitionBlock.Value;
    }

    private static string FindLastSpriteReference(string animationClip)
    {
        MatchCollection spriteReferences = Regex.Matches(
            animationClip,
            @"value: (\{fileID: -?\d+, guid: [0-9a-f]{32}, type: 3\})");

        Assert.That(spriteReferences.Count, Is.GreaterThan(0), "Animation clip has no sprite references.");
        return spriteReferences[spriteReferences.Count - 1].Groups[1].Value;
    }

    private static string ReadProjectFile(string projectRelativePath)
    {
        return File.ReadAllText(Path.Combine(Application.dataPath, "../", projectRelativePath));
    }

    private static Texture2D LoadTexture(string projectRelativePath)
    {
        string fullPath = Path.Combine(Application.dataPath, "../", projectRelativePath);
        byte[] bytes = File.ReadAllBytes(fullPath);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

        Assert.That(ImageConversion.LoadImage(texture, bytes), Is.True, $"Failed to load {projectRelativePath}.");
        return texture;
    }

    private static void FindOpaqueBounds(
        Color32[] pixels,
        int width,
        int height,
        out int minX,
        out int maxX,
        out int minY,
        out int maxY)
    {
        minX = width;
        maxX = -1;
        minY = height;
        maxY = -1;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (pixels[y * width + x].a <= 10)
                {
                    continue;
                }

                minX = Mathf.Min(minX, x);
                maxX = Mathf.Max(maxX, x);
                minY = Mathf.Min(minY, y);
                maxY = Mathf.Max(maxY, y);
            }
        }

        Assert.That(maxY, Is.GreaterThanOrEqualTo(0), "Texture has no visible pixels.");
    }

    private static void FindDarkBodyBounds(
        Color32[] pixels,
        int width,
        int height,
        out int minX,
        out int maxX,
        out int minY,
        out int maxY)
    {
        minX = width;
        maxX = -1;
        minY = height;
        maxY = -1;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color32 pixel = pixels[y * width + x];
                if (pixel.a <= 80 || pixel.r >= 75 || pixel.g >= 75 || pixel.b >= 75)
                {
                    continue;
                }

                minX = Mathf.Min(minX, x);
                maxX = Mathf.Max(maxX, x);
                minY = Mathf.Min(minY, y);
                maxY = Mathf.Max(maxY, y);
            }
        }

        Assert.That(maxY, Is.GreaterThanOrEqualTo(0), "Texture has no measurable dark body pixels.");
    }

    private static void FindMouthFlameBounds(
        Color32[] pixels,
        int width,
        int height,
        out int minX,
        out int maxX,
        out int minY,
        out int maxY)
    {
        minX = width;
        maxX = -1;
        minY = height;
        maxY = -1;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color32 pixel = pixels[y * width + x];
                if (pixel.a <= 80 || pixel.r <= 150 || pixel.g >= 190 || pixel.b >= 120 || pixel.r <= pixel.g + 25)
                {
                    continue;
                }

                minX = Mathf.Min(minX, x);
                maxX = Mathf.Max(maxX, x);
                minY = Mathf.Min(minY, y);
                maxY = Mathf.Max(maxY, y);
            }
        }

        Assert.That(maxY, Is.GreaterThanOrEqualTo(0), "Texture has no measurable mouth flame pixels.");
    }
}
#endif
