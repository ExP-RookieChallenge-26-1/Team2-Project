using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
	public void OnStartButtonClicked()
	{
		SceneManager.LoadScene("GameScene");
	}
}