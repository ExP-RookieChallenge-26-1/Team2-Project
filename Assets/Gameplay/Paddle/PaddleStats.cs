using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PaddleStats", menuName = "Stats/PaddleStats")]
public class PaddleStats : ScriptableObject
{
	[field: SerializeField] public float MaxPaddleSpeed { get; private set; }
	[field: SerializeField] public float MoveRange { get; private set; }
	[field: SerializeField] public float ReflectionWeight { get; private set; }
	[field: SerializeField] public int PaddleSizeLevel { get; private set; }
	[field: SerializeField] public int MaxPaddleSizeLevel { get; private set; }
	[field: SerializeField] public float[] PaddleSizeMultiplierPerLevel { get; private set; } = { 1f };
	[field: SerializeField] public Vector2 BaseColliderSize { get; private set; } = Vector2.one;

	public event Action<int> OnPaddleSizeLevelChanged;

	public float CurrentPaddleSizeMultiplier
	{
		get
		{
			if (this.PaddleSizeMultiplierPerLevel == null || this.PaddleSizeMultiplierPerLevel.Length == 0)
				return 1f;

			int index = Math.Clamp(this.PaddleSizeLevel, 0, this.PaddleSizeMultiplierPerLevel.Length - 1);
			return this.PaddleSizeMultiplierPerLevel[index];
		}
	}

	public Vector2 CurrentColliderSize => new(
		this.BaseColliderSize.x * this.CurrentPaddleSizeMultiplier,
		this.BaseColliderSize.y);

	public bool IncreasePaddleSizeLevel(int amount)
	{
		bool changed = this.SetPaddleSizeLevel(this.PaddleSizeLevel + amount);
		Debug.Log($"Paddle Size Level: {this.PaddleSizeLevel}");
		return changed;
	}

	public bool SetPaddleSizeLevel(int level)
	{
		int nextLevel = Math.Clamp(level, 0, this.MaxPaddleSizeLevel);

		if (this.PaddleSizeLevel == nextLevel)
			return false;

		this.PaddleSizeLevel = nextLevel;
		this.OnPaddleSizeLevelChanged?.Invoke(this.PaddleSizeLevel);
		return true;
	}
}
