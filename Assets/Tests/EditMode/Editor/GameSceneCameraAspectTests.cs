#if UNITY_EDITOR
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

public class GameSceneCameraAspectTests
{
    [Test]
    public void MainCameraKeepsFullWorldHeightForPaddleAndOverlayUi()
    {
        bool previousIgnoreFailingMessages = LogAssert.ignoreFailingMessages;
        LogAssert.ignoreFailingMessages = true;
        try
        {
            EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity");
        }
        finally
        {
            LogAssert.ignoreFailingMessages = previousIgnoreFailingMessages;
        }

        Camera camera = Camera.main;
        Assert.That(camera, Is.Not.Null);
        Assert.That(camera.orthographic, Is.True);
        Assert.That(camera.orthographicSize, Is.EqualTo(5f).Within(0.0001f));

        AssertNoFixedAspectCropper(camera);

        camera.aspect = 0.72f;
        Assert.That(camera.orthographicSize, Is.EqualTo(5f).Within(0.0001f));

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        Assert.That(canvas, Is.Not.Null);
        Assert.That(canvas.renderMode, Is.EqualTo(RenderMode.ScreenSpaceOverlay));

        GameObject paddle = GameObject.FindWithTag("Paddle");
        Assert.That(paddle, Is.Not.Null);
        Assert.That(paddle.transform.position.y, Is.EqualTo(-4f).Within(0.0001f));
    }

    private static void AssertNoFixedAspectCropper(Camera camera)
    {
        foreach (MonoBehaviour component in camera.GetComponents<MonoBehaviour>())
        {
            Assert.That(component, Is.Not.Null, "Main Camera has a missing MonoBehaviour.");
            Assert.That(component.GetType().Name, Is.Not.EqualTo("FixedAspectOrthographicCamera"));
        }
    }
}
#endif
