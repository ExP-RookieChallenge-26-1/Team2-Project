using UnityEngine;

public class EnhanceUITest : MonoBehaviour
{
	public void OnClickTest()
	{
		int[] ids = { 1, 2, 3, 4, 5 };
		float[] weights = { 5f, 4f, 3f, 2f, 1f };
		FindFirstObjectByType<LevelSystem>()?.OpenUpgradeUI(ids, weights);
	}
}