using UnityEngine;
using UnityEngine.UI;

public sealed class LockIconGraphic : Graphic
{
	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();

		Rect rect = GetPixelAdjustedRect();
		Color32 vertexColor = color;
		float width = rect.width;
		float height = rect.height;
		float left = rect.xMin;
		float bottom = rect.yMin;

		AddRect(
			vh,
			new Rect(left + width * 0.18f, bottom + height * 0.08f, width * 0.64f, height * 0.48f),
			vertexColor);
		AddRect(
			vh,
			new Rect(left + width * 0.28f, bottom + height * 0.46f, width * 0.12f, height * 0.28f),
			vertexColor);
		AddRect(
			vh,
			new Rect(left + width * 0.60f, bottom + height * 0.46f, width * 0.12f, height * 0.28f),
			vertexColor);
		AddRect(
			vh,
			new Rect(left + width * 0.36f, bottom + height * 0.66f, width * 0.28f, height * 0.12f),
			vertexColor);
	}

	private static void AddRect(VertexHelper vh, Rect rect, Color32 color)
	{
		int start = vh.currentVertCount;
		vh.AddVert(new Vector3(rect.xMin, rect.yMin), color, Vector2.zero);
		vh.AddVert(new Vector3(rect.xMin, rect.yMax), color, Vector2.zero);
		vh.AddVert(new Vector3(rect.xMax, rect.yMax), color, Vector2.zero);
		vh.AddVert(new Vector3(rect.xMax, rect.yMin), color, Vector2.zero);
		vh.AddTriangle(start, start + 1, start + 2);
		vh.AddTriangle(start + 2, start + 3, start);
	}
}
