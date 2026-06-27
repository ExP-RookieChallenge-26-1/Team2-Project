#if UNITY_EDITOR
using System.IO;
using NUnit.Framework;

public class EXPBarUITests
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";
    private const string ScriptMetaPath = "Assets/UI/HUD/EXPBarUI.cs.meta";

    public static void RunGameSceneUsesCurrentEXPBarUIScriptGuid()
    {
        new EXPBarUITests().GameSceneUsesCurrentEXPBarUIScriptGuid();
    }

    [Test]
    public void GameSceneUsesCurrentEXPBarUIScriptGuid()
    {
        string scriptGuid = ReadGuid(ScriptMetaPath);
        string sceneYaml = File.ReadAllText(ScenePath);

        Assert.That(sceneYaml, Does.Contain("m_EditorClassIdentifier: Assembly-CSharp::EXPBarUI"));
        Assert.That(
            sceneYaml,
            Does.Contain($"m_Script: {{fileID: 11500000, guid: {scriptGuid}, type: 3}}"));
    }

    private static string ReadGuid(string metaPath)
    {
        foreach (string line in File.ReadLines(metaPath))
        {
            if (line.StartsWith("guid: "))
                return line.Substring("guid: ".Length);
        }

        Assert.Fail($"No guid found in {metaPath}");
        return string.Empty;
    }
}
#endif
