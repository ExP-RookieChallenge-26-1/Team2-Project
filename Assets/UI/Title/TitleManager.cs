using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
	public void OnStartButtonClicked()
	{
		// 이전 GameManager 완전 삭제
		if (GameManager.Instance != null)
		{
			Object.Destroy(GameManager.Instance.gameObject);
		}
		SceneManager.LoadScene("GameScene");
	}
}