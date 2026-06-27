using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

public class ParallaxBackgroundTests
{
    [Test]
    public void LayerMoveAmount_IsEditableInInspectorWithGuidance()
    {
        Type backgroundType = FindParallaxBackgroundType();
        Assert.That(backgroundType, Is.Not.Null);

        Type layerType = FindLayerType(backgroundType);
        Assert.That(layerType, Is.Not.Null);

        FieldInfo field = layerType.GetField("travelDistance", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null);
        Assert.That(field.GetCustomAttribute<SerializeField>(), Is.Not.Null);
        Assert.That(field.GetCustomAttribute<TooltipAttribute>(), Is.Not.Null);
        Assert.That(field.GetCustomAttribute<InspectorNameAttribute>(), Is.Not.Null);
    }

    [Test]
    public void CustomEditor_ExistsForLayerMoveAmounts()
    {
        Type editorType = Type.GetType("ParallaxBackgroundEditor, Assembly-CSharp-Editor");
        Assert.That(editorType, Is.Not.Null);
        Assert.That(typeof(UnityEditor.Editor).IsAssignableFrom(editorType), Is.True);
        Assert.That(editorType.GetCustomAttribute<UnityEditor.CustomEditor>(), Is.Not.Null);
    }

    [Test]
    public void SceneSetup_UsesBottomToTopLayerOrder()
    {
        Type setupType = Type.GetType("ParallaxBackgroundSceneSetup, Assembly-CSharp-Editor");
        Assert.That(setupType, Is.Not.Null);

        FieldInfo field = setupType.GetField("LayerDefinitions", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null);

        var definitions = (Array)field.GetValue(null);
        string[] expected =
        {
            "1.png",
            "2_.png",
            "3_.png",
            "4_.png",
            "5_.png",
            "6_.png",
            "7_.png"
        };

        Assert.That(definitions.Length, Is.EqualTo(expected.Length));
        for (int i = 0; i < definitions.Length; ++i)
        {
            object definition = definitions.GetValue(i);
            FieldInfo fileNameField = definition.GetType().GetField("FileName", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(fileNameField, Is.Not.Null);
            Assert.That((string)fileNameField.GetValue(definition), Is.EqualTo(expected[i]));
        }
    }

    [Test]
    public void SceneSetup_KeepsSkyStaticAndUsesOriginalTopRock()
    {
        Type setupType = Type.GetType("ParallaxBackgroundSceneSetup, Assembly-CSharp-Editor");
        Assert.That(setupType, Is.Not.Null);

        FieldInfo field = setupType.GetField("LayerDefinitions", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null);

        var definitions = (Array)field.GetValue(null);
        Assert.That(definitions.Length, Is.EqualTo(7));

        object sky = definitions.GetValue(0);
        object topRock = definitions.GetValue(6);
        FieldInfo fileNameField = sky.GetType().GetField("FileName", BindingFlags.Instance | BindingFlags.Public);
        FieldInfo travelDistanceField = sky.GetType().GetField("TravelDistance", BindingFlags.Instance | BindingFlags.Public);
        Assert.That(fileNameField, Is.Not.Null);
        Assert.That(travelDistanceField, Is.Not.Null);

        Assert.That((string)fileNameField.GetValue(sky), Is.EqualTo("1.png"));
        Assert.That((float)travelDistanceField.GetValue(sky), Is.EqualTo(0f).Within(0.0001f));
        Assert.That((string)fileNameField.GetValue(topRock), Is.EqualTo("7_.png"));
    }

    [Test]
    public void CalculateLayerOffset_ClampsProgressAndUsesTravelDistance()
    {
        Type backgroundType = FindParallaxBackgroundType();
        Assert.That(backgroundType, Is.Not.Null);

        Type layerType = FindLayerType(backgroundType);
        Assert.That(layerType, Is.Not.Null);

        var backgroundObject = new GameObject("ParallaxBackground");
        Component background = backgroundObject.AddComponent(backgroundType);
        object layer = Activator.CreateInstance(layerType);
        Invoke(layer, "Configure", null, 2.5f, 3);

        var halfOffset = (Vector3)Invoke(background, "CalculateLayerOffset", 0.5f, layer);
        var clampedOffset = (Vector3)Invoke(background, "CalculateLayerOffset", 1.5f, layer);

        Assert.That(halfOffset.y, Is.EqualTo(-1.25f).Within(0.0001f));
        Assert.That(clampedOffset.y, Is.EqualTo(-2.5f).Within(0.0001f));
        Object.DestroyImmediate(backgroundObject);
    }

    [Test]
    public void Tick_MovesLayerOnceAcrossStageDuration()
    {
        Type backgroundType = FindParallaxBackgroundType();
        Assert.That(backgroundType, Is.Not.Null);

        Type layerType = FindLayerType(backgroundType);
        Assert.That(layerType, Is.Not.Null);

        Sprite sprite = CreateTestSprite();
        var backgroundObject = new GameObject("ParallaxBackground");
        Component background = backgroundObject.AddComponent(backgroundType);
        object layer = Activator.CreateInstance(layerType);
        Invoke(layer, "Configure", sprite, 3f, -10);
        Array layers = Array.CreateInstance(layerType, 1);
        layers.SetValue(layer, 0);
        Invoke(background, "Configure", layers, 10f, 10f);

        Invoke(background, "Rebuild");
        Transform visual = (Transform)GetProperty(layer, "Visual");

        Invoke(background, "Tick", 5f);
        Assert.That(visual.position.y, Is.EqualTo(-1.5f).Within(0.0001f));

        Invoke(background, "Tick", 10f);
        Assert.That(visual.position.y, Is.EqualTo(-3f).Within(0.0001f));

        Invoke(background, "Tick", 10f);
        Assert.That(visual.position.y, Is.EqualTo(-3f).Within(0.0001f));
        Object.DestroyImmediate(backgroundObject);
        Object.DestroyImmediate(sprite.texture);
    }

    [Test]
    public void Rebuild_CreatesSingleSpritePerLayerWithIncreasingSortingOrders()
    {
        Type backgroundType = FindParallaxBackgroundType();
        Assert.That(backgroundType, Is.Not.Null);

        Type layerType = FindLayerType(backgroundType);
        Assert.That(layerType, Is.Not.Null);

        Sprite sprite = CreateTestSprite();
        var backgroundObject = new GameObject("ParallaxBackground");
        Component background = backgroundObject.AddComponent(backgroundType);
        object far = Activator.CreateInstance(layerType);
        object near = Activator.CreateInstance(layerType);
        Invoke(far, "Configure", sprite, 0.1f, -70);
        Invoke(near, "Configure", sprite, 1.8f, -10);
        Array layers = Array.CreateInstance(layerType, 2);
        layers.SetValue(far, 0);
        layers.SetValue(near, 1);
        Invoke(background, "Configure", layers, 180f, 10f);

        Invoke(background, "Rebuild");

        Transform farVisual = (Transform)GetProperty(far, "Visual");
        Transform nearVisual = (Transform)GetProperty(near, "Visual");
        var farRenderer = farVisual.GetComponent<SpriteRenderer>();
        var nearRenderer = nearVisual.GetComponent<SpriteRenderer>();
        Assert.That(backgroundObject.transform.childCount, Is.EqualTo(2));
        Assert.That(farRenderer.sortingLayerName, Is.EqualTo("Background"));
        Assert.That(farRenderer.sortingOrder, Is.LessThan(nearRenderer.sortingOrder));
        Object.DestroyImmediate(backgroundObject);
        Object.DestroyImmediate(sprite.texture);
    }

    [Test]
    public void Rebuild_ScalesLayerToCoverCameraWidthWhenAspectIsWiderThanSprite()
    {
        Type backgroundType = FindParallaxBackgroundType();
        Assert.That(backgroundType, Is.Not.Null);

        Type layerType = FindLayerType(backgroundType);
        Assert.That(layerType, Is.Not.Null);

        Sprite sprite = CreateTestSprite(8, 16);
        GameObject[] originalMainCameras = GameObject.FindGameObjectsWithTag("MainCamera");
        var cameraObject = new GameObject("Main Camera");
        var backgroundObject = new GameObject("ParallaxBackground");

        try
        {
            foreach (GameObject originalMainCamera in originalMainCameras)
            {
                originalMainCamera.tag = "Untagged";
            }

            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.aspect = 2f;
            Assert.That(Camera.main, Is.SameAs(camera));

            Component background = backgroundObject.AddComponent(backgroundType);
            object layer = Activator.CreateInstance(layerType);
            Invoke(layer, "Configure", sprite, 0f, -10);
            Array layers = Array.CreateInstance(layerType, 1);
            layers.SetValue(layer, 0);
            Invoke(background, "Configure", layers, 180f, 10f);

            Invoke(background, "Rebuild");

            Transform visual = (Transform)GetProperty(layer, "Visual");
            var renderer = visual.GetComponent<SpriteRenderer>();
            float cameraWorldWidth = camera.orthographicSize * 2f * camera.aspect;
            Assert.That(renderer.bounds.size.x, Is.GreaterThanOrEqualTo(cameraWorldWidth - 0.0001f));
        }
        finally
        {
            Object.DestroyImmediate(backgroundObject);
            Object.DestroyImmediate(cameraObject);
            Object.DestroyImmediate(sprite.texture);

            foreach (GameObject originalMainCamera in originalMainCameras)
            {
                if (originalMainCamera != null)
                    originalMainCamera.tag = "MainCamera";
            }
        }
    }

    private static Type FindParallaxBackgroundType()
    {
        return Type.GetType("ParallaxBackground, Assembly-CSharp");
    }

    private static Type FindLayerType(Type backgroundType)
    {
        return backgroundType?.GetNestedType("Layer", BindingFlags.Public);
    }

    private static object Invoke(object target, string methodName, params object[] arguments)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
        Assert.That(method, Is.Not.Null);
        return method.Invoke(target, arguments);
    }

    private static object GetProperty(object target, string propertyName)
    {
        PropertyInfo property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
        Assert.That(property, Is.Not.Null);
        return property.GetValue(target);
    }

    private static Sprite CreateTestSprite(int width = 8, int height = 8)
    {
        var texture = new Texture2D(width, height);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 8f);
    }
}
