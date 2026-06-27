using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapAspectCropperTests
{
    [Test]
    public void ApplyCrop_ScalesOnlyTilemapForWiderAspect()
    {
        var cameraObject = new GameObject("Camera");
        var chunkObject = new GameObject("MapChunk");
        var tilemapObject = new GameObject("Tilemap", typeof(Tilemap));
        var spawnRoot = new GameObject("EnemySpawnPoints");

        try
        {
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.aspect = 0.72f;

            tilemapObject.transform.SetParent(chunkObject.transform, false);
            tilemapObject.transform.localScale = new Vector3(2f, 3f, 1f);
            spawnRoot.transform.SetParent(chunkObject.transform, false);

            TilemapAspectCropper cropper = chunkObject.AddComponent<TilemapAspectCropper>();
            cropper.Configure(camera, 0.6f);

            Assert.That(tilemapObject.transform.localScale.x, Is.EqualTo(2.4f).Within(0.0001f));
            Assert.That(tilemapObject.transform.localScale.y, Is.EqualTo(3.6f).Within(0.0001f));
            Assert.That(spawnRoot.transform.localScale, Is.EqualTo(Vector3.one));

            camera.aspect = 0.6f;
            cropper.ApplyCrop();

            Assert.That(tilemapObject.transform.localScale.x, Is.EqualTo(2f).Within(0.0001f));
            Assert.That(tilemapObject.transform.localScale.y, Is.EqualTo(3f).Within(0.0001f));
        }
        finally
        {
            Object.DestroyImmediate(chunkObject);
            Object.DestroyImmediate(cameraObject);
        }
    }

    [Test]
    public void CalculateTilemapCoverScale_OnlyExpandsForWiderAspects()
    {
        Assert.That(TilemapAspectCropper.CalculateTilemapCoverScale(0.6f, 0.72f), Is.EqualTo(1.2f).Within(0.0001f));
        Assert.That(TilemapAspectCropper.CalculateTilemapCoverScale(0.6f, 0.6f), Is.EqualTo(1f).Within(0.0001f));
        Assert.That(TilemapAspectCropper.CalculateTilemapCoverScale(0.6f, 0.5f), Is.EqualTo(1f).Within(0.0001f));
    }
}
