#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;

public class GameSceneWorldReferencesTests
{
    private const string GameScenePath = "Assets/Scenes/GameScene.unity";

    [Test]
    public void GameSceneUsesLegacyRegisteredMaps()
    {
        string sceneText = File.ReadAllText(GameScenePath);
        string chunkPrefabs = Regex.Match(
            sceneText,
            @"chunkPrefabs:\n(?<items>(?:  - \{fileID: 7340138172155822266, guid: [0-9a-f]+, type: 3\}\n)+)").Groups["items"].Value;

        Assert.That(chunkPrefabs, Is.Not.Empty, "GameScene World chunkPrefabs block is missing.");

        List<string> guids = new List<string>();
        foreach (Match match in Regex.Matches(chunkPrefabs, @"guid: (?<guid>[0-9a-f]+)"))
            guids.Add(match.Groups["guid"].Value);

        Assert.That(guids, Is.EqualTo(new[]
        {
            AssetDatabase.AssetPathToGUID("Assets/Data/Maps/LegacyRegistered/LegacyMap0.prefab"),
            AssetDatabase.AssetPathToGUID("Assets/Data/Maps/LegacyRegistered/LegacyMap1.prefab"),
            AssetDatabase.AssetPathToGUID("Assets/Data/Maps/LegacyRegistered/LegacyMap2.prefab"),
        }));
    }
}
#endif
