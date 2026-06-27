#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ParallaxBackgroundSceneSetup
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";
    private const string AssetFolder = "Assets/Art/Background/Parallax";
    private const string BackgroundObjectName = "ParallaxBackground";
    private const float StageDurationSeconds = 180f;

    private static readonly LayerDefinition[] LayerDefinitions =
    {
        new("1.png", 0f, -70),
        new("2_.png", 0.40f, -60),
        new("3_.png", 0.65f, -50),
        new("4_.png", 0.90f, -40),
        new("5_.png", 1.15f, -30),
        new("6_.png", 1.35f, -20),
        new("7_.png", 1.65f, -10),
    };

    [MenuItem("Tools/Team2/Setup Parallax Background")]
    public static void Setup()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath);
        GameObject root = GameObject.Find(BackgroundObjectName);
        if (root == null)
            root = new GameObject(BackgroundObjectName);

        root.transform.position = Vector3.zero;
        var background = root.GetComponent<ParallaxBackground>();
        if (background == null)
            background = root.AddComponent<ParallaxBackground>();

        var layers = new ParallaxBackground.Layer[LayerDefinitions.Length];
        for (int i = 0; i < LayerDefinitions.Length; ++i)
        {
            LayerDefinition definition = LayerDefinitions[i];
            string assetPath = Path.Combine(AssetFolder, definition.FileName).Replace('\\', '/');
            EnsureSpriteImportSettings(assetPath);

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite == null)
                throw new InvalidOperationException($"Missing parallax sprite asset: {assetPath}");

            layers[i] = new ParallaxBackground.Layer();
            layers[i].Configure(sprite, definition.TravelDistance, definition.SortingOrder);
        }

        background.Configure(layers, StageDurationSeconds);
        EditorUtility.SetDirty(background);
        EditorUtility.SetDirty(root);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }

    private static void EnsureSpriteImportSettings(string assetPath)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
            throw new InvalidOperationException($"Missing texture importer for parallax sprite: {assetPath}");

        bool changed = false;
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            changed = true;
        }

        if (importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            changed = true;
        }

        if (!importer.alphaIsTransparency)
        {
            importer.alphaIsTransparency = true;
            changed = true;
        }

        if (importer.mipmapEnabled)
        {
            importer.mipmapEnabled = false;
            changed = true;
        }

        if (importer.maxTextureSize < 2048)
        {
            importer.maxTextureSize = 2048;
            changed = true;
        }

        if (changed)
            importer.SaveAndReimport();
    }

    private readonly struct LayerDefinition
    {
        public readonly string FileName;
        public readonly float TravelDistance;
        public readonly int SortingOrder;

        public LayerDefinition(string fileName, float travelDistance, int sortingOrder)
        {
            this.FileName = fileName;
            this.TravelDistance = travelDistance;
            this.SortingOrder = sortingOrder;
        }
    }
}
#endif
