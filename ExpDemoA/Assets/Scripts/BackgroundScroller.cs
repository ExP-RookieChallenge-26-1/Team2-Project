using UnityEngine;
using System.Collections.Generic;

public class BackgroundScroller : MonoBehaviour
{
	[SerializeField] private GameObject bgPrefab;
	[SerializeField] private float scrollSpeed = 0.8f;

	private const float cameraBottomY = -5f;
	private const float cameraTopY = 5f;

	private float bgHeight;
	private List<GameObject> bgs = new List<GameObject>();

	void Start()
	{
		bgHeight = MeasureHeight();
		SpawnBg(cameraBottomY);
		SpawnBg(cameraBottomY + bgHeight);
	}

	float MeasureHeight()
	{
		var temp = Instantiate(bgPrefab);
		float minY = float.MaxValue;
		float maxY = float.MinValue;

		foreach (var r in temp.GetComponentsInChildren<Renderer>())
		{
			minY = Mathf.Min(minY, r.bounds.min.y);
			maxY = Mathf.Max(maxY, r.bounds.max.y);
		}

		Destroy(temp);
		return maxY - minY;
	}

	void SpawnBg(float bottomY)
	{
		var go = Instantiate(bgPrefab);
		go.transform.position = new Vector3(0f, bottomY, 1f); // z=1 : 맵보다 뒤
		bgs.Add(go);
	}

	void Update()
	{
		float move = scrollSpeed * Time.deltaTime;

		foreach (var bg in bgs)
			bg.transform.position += Vector3.down * move;

		// 가장 아래 bg가 카메라 아래로 완전히 사라지면 맨 위로 재배치
		for (int i = 0; i < bgs.Count; i++)
		{
			float topY = bgs[i].transform.position.y + bgHeight;
			if (topY < cameraBottomY)
			{
				float highestTop = cameraBottomY;
				foreach (var bg in bgs)
					highestTop = Mathf.Max(highestTop, bg.transform.position.y + bgHeight);

				bgs[i].transform.position = new Vector3(0f, highestTop, 1f);
			}
		}
	}
}
