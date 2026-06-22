using System;
using UnityEngine;

public class CloneSkill : BallSkill
{
	[field: SerializeField] public int CloneLevel { get; private set; }
	[SerializeField] private int[] cloneCountPerLevel = { 2, 3, 4 };
	[SerializeField] private float blockedHorizontalAngle = 20f;
	[SerializeField] private int maxBallCount = 128;
	public int MaxCloneLevel => this.cloneCountPerLevel.Length - 1;

	protected override void Subscribe()
	{
		this.skillEventChannel.OnSkill2Activated += TryManualActivate;
	}

	protected override void Unsubscribe()
	{
		this.skillEventChannel.OnSkill2Activated -= TryManualActivate;
	}

	protected override void OnActivate()
	{
		SpawnClones();

		foreach (Ball ball in FindObjectsByType<Ball>(FindObjectsSortMode.None))
			ball.Animation.TriggerClone();
	}

	protected override void OnDeactivate()
	{
	}

	public void IncreaseCloneLevel(int amount)
	{
		this.CloneLevel = Math.Clamp(this.CloneLevel + amount, 0, this.MaxCloneLevel);
	}

	private void SpawnClones()
	{
		int currentBallCount;
		int spawnCount;

		if (this.cloneCountPerLevel == null || this.cloneCountPerLevel.Length == 0)
			return;

		currentBallCount = FindObjectsByType<Ball>(FindObjectsSortMode.None).Length;
		spawnCount = Mathf.Min(this.cloneCountPerLevel[this.CloneLevel], this.maxBallCount - currentBallCount);

		if (spawnCount <= 0)
			return;

		for (int i = 0; i < spawnCount; i++)
		{
			float radian;
			Vector2 velocity;
			
			radian = GetRandomValidAngle() * Mathf.Deg2Rad;
			velocity = new Vector2(Mathf.Cos(radian), Mathf.Sin(radian)) * this.ball.Stats.Speed;
			SpawnClone(this.ball.transform.position, velocity);
		}
	}

	private float GetRandomValidAngle()
	{
		float angle;

		do
		{
			angle = UnityEngine.Random.Range(0f, 360f);
		}
		while (IsBlocked(angle));

		return angle;
	}

	private bool IsBlocked(float angle)
	{
		// 0° (오른쪽) 기준 ±n도
		if (angle <= this.blockedHorizontalAngle || angle >= 360f - this.blockedHorizontalAngle)
			return true;

		// 180° (왼쪽) 기준 ±n도
		if (angle >= 180f - this.blockedHorizontalAngle && angle <= 180f + this.blockedHorizontalAngle)
			return true;

		return false;
	}

	private void SpawnClone(Vector2 position, Vector2 velocity)
	{
		GameObject clone = Instantiate(this.ball.gameObject, position, Quaternion.identity);
		clone.GetComponent<Ball>().Physics.SetVelocity(velocity);
	}
}