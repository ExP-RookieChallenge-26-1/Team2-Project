using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateMachine : MonoBehaviour
{
	public GameState Current { get; private set; }
	public event Action<GameState> OnChanged;

	private void Start()
	{
		Change(GameState.Playing);
	}
	
	public void Change(GameState newState)
	{
		if (Current == newState)
			return;

		Current = newState;
		OnChanged?.Invoke(this.Current);

		if (newState == GameState.GameOver)
			SceneManager.LoadScene("TitleScene");
	}
}
