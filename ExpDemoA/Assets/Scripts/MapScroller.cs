using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class MapScroller : MonoBehaviour
{
	[SerializeField] private GameObject mapPrefab;
	[SerializeField] private float scrollSpeed = 2f;

	private const float deathY = -7f;
	private const float cameraTopY = 5f;

	private float mapHeight;
	private float contentBottomOffset;

	private List<GameObject> maps = new List<GameObject>();

	void Start()
	{
		MeasureMap();
		SpawnMap(cameraTopY);
		SpawnMap(cameraTopY + mapHeight);
	}

	void MeasureMap()
	{
		var temp = Instantiate(mapPrefab);
		var tilemap = temp.GetComponentInChildren<Tilemap>();
		tilemap.CompressBounds();

		float minY = tilemap.transform.TransformPoint(tilemap.localBounds.min).y;
		float maxY = tilemap.transform.TransformPoint(tilemap.localBounds.max).y;

		contentBottomOffset = minY; // temp is at y=0
		mapHeight = maxY - minY;

		Destroy(temp);
	}

	void SpawnMap(float contentBottomY)
	{
		var go = Instantiate(mapPrefab);
		go.transform.position = new Vector3(0f, contentBottomY - contentBottomOffset, 0f);
		maps.Add(go);
	}

	void Update()
	{
		float move = scrollSpeed * Time.deltaTime;

		for (int i = maps.Count - 1; i >= 0; i--)
		{
			maps[i].transform.position += Vector3.down * move;

			float contentTopY = maps[i].transform.position.y + contentBottomOffset + mapHeight;

			if (contentTopY < deathY)
			{
				Destroy(maps[i]);
				maps.RemoveAt(i);
				SpawnNextMap();
			}
		}
	}

	void SpawnNextMap()
	{
		float highestTop = deathY;
		foreach (var m in maps)
			highestTop = Mathf.Max(highestTop, m.transform.position.y + contentBottomOffset + mapHeight);

		SpawnMap(highestTop);
	}
}
