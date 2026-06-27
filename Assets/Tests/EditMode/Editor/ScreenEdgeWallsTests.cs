#if UNITY_EDITOR
using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

public class ScreenEdgeWallsTests
{
    [Test]
    public void Rebuild_CropsSideWallsHalfOutsideCameraAndBleedsCeiling()
    {
        Type wallType = FindScreenEdgeWallsType();
        Assert.That(wallType, Is.Not.Null);

        Camera camera = CreateOrthographicCamera(new Vector3(2f, -1f, -10f), 5f, 0.6f);
        Sprite wallSprite = CreateTestSprite(10, 100, 10f);
        Sprite ceilingSprite = CreateTestSprite(60, 10, 10f);
        var root = new GameObject("ScreenEdgeWalls");
        Component walls = root.AddComponent(wallType);

        Invoke(walls, "Configure", wallSprite, wallSprite, ceilingSprite, camera, "UI", 100);
        Invoke(walls, "Rebuild");

        SpriteRenderer left = GetRenderer(root.transform, "ScreenEdgeWallVisuals/LeftWall");
        SpriteRenderer right = GetRenderer(root.transform, "ScreenEdgeWallVisuals/RightWall");
        SpriteRenderer ceiling = GetRenderer(root.transform, "ScreenEdgeWallVisuals/Ceiling");

        Assert.That(left.bounds.center.x, Is.EqualTo(-1f).Within(0.001f));
        Assert.That(left.bounds.max.x, Is.EqualTo(-0.475f).Within(0.001f));
        Assert.That(right.bounds.center.x, Is.EqualTo(5f).Within(0.001f));
        Assert.That(right.bounds.min.x, Is.EqualTo(4.475f).Within(0.001f));
        Assert.That(ceiling.bounds.max.y, Is.EqualTo(4.3306665f).Within(0.001f));
        Assert.That(left.bounds.size.y, Is.EqualTo(10.5f).Within(0.001f));
        Assert.That(right.bounds.size.y, Is.EqualTo(10.5f).Within(0.001f));
        Assert.That(left.bounds.center.y, Is.EqualTo(-1f).Within(0.001f));
        Assert.That(right.bounds.center.y, Is.EqualTo(-1f).Within(0.001f));
        Assert.That(ceiling.bounds.center.x, Is.EqualTo(2f).Within(0.001f));
        Assert.That(ceiling.bounds.size.x, Is.EqualTo(6.2f).Within(0.001f));
        Assert.That(left.sortingLayerName, Is.EqualTo("UI"));
        Assert.That(left.sortingOrder, Is.EqualTo(101));
        Assert.That(right.sortingLayerName, Is.EqualTo("UI"));
        Assert.That(right.sortingOrder, Is.EqualTo(101));
        Assert.That(ceiling.sortingLayerName, Is.EqualTo("UI"));
        Assert.That(ceiling.sortingOrder, Is.EqualTo(100));

        Object.DestroyImmediate(root);
        Object.DestroyImmediate(camera.gameObject);
        Object.DestroyImmediate(wallSprite.texture);
        Object.DestroyImmediate(ceilingSprite.texture);
    }

    [Test]
    public void ApplyLayout_TracksCameraAspectChanges()
    {
        Type wallType = FindScreenEdgeWallsType();
        Assert.That(wallType, Is.Not.Null);

        Camera camera = CreateOrthographicCamera(Vector3.back * 10f, 5f, 0.5f);
        Sprite wallSprite = CreateTestSprite(10, 100, 10f);
        Sprite ceilingSprite = CreateTestSprite(60, 10, 10f);
        var root = new GameObject("ScreenEdgeWalls");
        Component walls = root.AddComponent(wallType);

        Invoke(walls, "Configure", wallSprite, wallSprite, ceilingSprite, camera, "UI", 100);
        Invoke(walls, "Rebuild");

        camera.aspect = 0.8f;
        Invoke(walls, "ApplyLayout");

        SpriteRenderer left = GetRenderer(root.transform, "ScreenEdgeWallVisuals/LeftWall");
        SpriteRenderer right = GetRenderer(root.transform, "ScreenEdgeWallVisuals/RightWall");
        SpriteRenderer ceiling = GetRenderer(root.transform, "ScreenEdgeWallVisuals/Ceiling");

        Assert.That(left.bounds.center.x, Is.EqualTo(-4f).Within(0.001f));
        Assert.That(left.bounds.max.x, Is.EqualTo(-3.475f).Within(0.001f));
        Assert.That(right.bounds.center.x, Is.EqualTo(4f).Within(0.001f));
        Assert.That(right.bounds.min.x, Is.EqualTo(3.475f).Within(0.001f));
        Assert.That(ceiling.bounds.max.y, Is.EqualTo(5.437333f).Within(0.001f));
        Assert.That(left.bounds.size.y, Is.EqualTo(10.5f).Within(0.001f));
        Assert.That(right.bounds.size.y, Is.EqualTo(10.5f).Within(0.001f));
        Assert.That(ceiling.bounds.size.x, Is.EqualTo(8.2f).Within(0.001f));

        Object.DestroyImmediate(root);
        Object.DestroyImmediate(camera.gameObject);
        Object.DestroyImmediate(wallSprite.texture);
        Object.DestroyImmediate(ceilingSprite.texture);
    }

    [Test]
    public void ApplyLayout_MovesPhysicalWallCollidersToCameraEdges()
    {
        Type wallType = FindScreenEdgeWallsType();
        Assert.That(wallType, Is.Not.Null);

        Camera camera = CreateOrthographicCamera(new Vector3(2f, -1f, -10f), 5f, 0.6f);
        var root = new GameObject("Walls");
        CreateCollider(root.transform, "WallLeft");
        CreateCollider(root.transform, "WallRight");
        CreateCollider(root.transform, "Ceiling");
        Component walls = root.AddComponent(wallType);

        Invoke(walls, "Configure", null, null, null, camera, "UI", 100);
        Invoke(walls, "Rebuild");
        Physics2D.SyncTransforms();

        BoxCollider2D left = GetCollider(root.transform, "WallLeft");
        BoxCollider2D right = GetCollider(root.transform, "WallRight");
        BoxCollider2D ceiling = GetCollider(root.transform, "Ceiling");

        Assert.That(left.bounds.center.x, Is.EqualTo(-1f).Within(0.001f));
        Assert.That(left.bounds.size.x, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(left.bounds.size.y, Is.EqualTo(10f).Within(0.001f));
        Assert.That(right.bounds.center.x, Is.EqualTo(5f).Within(0.001f));
        Assert.That(right.bounds.size.x, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(right.bounds.size.y, Is.EqualTo(10f).Within(0.001f));
        Assert.That(ceiling.bounds.center.x, Is.EqualTo(2f).Within(0.001f));
        Assert.That(ceiling.bounds.size.x, Is.EqualTo(6f).Within(0.001f));
        Assert.That(ceiling.bounds.size.y, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(ceiling.bounds.min.y, Is.EqualTo(4f).Within(0.001f));

        Object.DestroyImmediate(root);
        Object.DestroyImmediate(camera.gameObject);
    }

    private static Type FindScreenEdgeWallsType()
    {
        return Type.GetType("ScreenEdgeWalls, Assembly-CSharp");
    }

    private static Camera CreateOrthographicCamera(Vector3 position, float orthographicSize, float aspect)
    {
        var cameraObject = new GameObject("TestCamera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = orthographicSize;
        camera.aspect = aspect;
        camera.transform.position = position;
        return camera;
    }

    private static Sprite CreateTestSprite(int width, int height, float pixelsPerUnit)
    {
        var texture = new Texture2D(width, height);
        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
                texture.SetPixel(x, y, Color.white);
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

    private static SpriteRenderer GetRenderer(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        Assert.That(child, Is.Not.Null, childName);

        SpriteRenderer renderer = child.GetComponent<SpriteRenderer>();
        Assert.That(renderer, Is.Not.Null, childName);
        return renderer;
    }

    private static BoxCollider2D CreateCollider(Transform parent, string childName)
    {
        var child = new GameObject(childName);
        child.transform.SetParent(parent, false);
        return child.AddComponent<BoxCollider2D>();
    }

    private static BoxCollider2D GetCollider(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        Assert.That(child, Is.Not.Null, childName);

        BoxCollider2D collider = child.GetComponent<BoxCollider2D>();
        Assert.That(collider, Is.Not.Null, childName);
        return collider;
    }

    private static object Invoke(object target, string methodName, params object[] arguments)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
        Assert.That(method, Is.Not.Null, methodName);
        return method.Invoke(target, arguments);
    }
}
#endif
