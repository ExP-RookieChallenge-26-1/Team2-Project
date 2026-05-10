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

	public void IncreasePaddleSizeLevel(int amount)
	{
		this.PaddleSizeLevel = Math.Clamp(this.PaddleSizeLevel + amount, 0, this.MaxPaddleSizeLevel);
		Debug.Log($"Paddle Size Level: {this.PaddleSizeLevel}");
	}
}