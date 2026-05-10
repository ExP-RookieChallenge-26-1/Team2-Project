using System;
using UnityEngine;

public class GameStateMachine : MonoBehaviour
{
	public enum State
	{
		Playing,
		Enhancement,
		GameOver
	}
	public State Current { get; private set; }
	public event Action<State> OnChanged;

	private void Start()
	{
		Change(State.Playing);
	}
	
	public void Change(State newState)
	{
		if (Current == newState)
			return;

		Current = newState;
		OnChanged?.Invoke(this.Current);
	}
}
