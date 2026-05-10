using UnityEngine;

[CreateAssetMenu(fileName = "PaddleStats", menuName = "Stats/PaddleStats")]
public class PaddleStats : ScriptableObject
{
	[field: SerializeField] public float MaxPaddleSpeed { get; private set; }
	[field: SerializeField] public float MoveRange { get; private set; }
	[field: SerializeField] public float PaddleWidth { get; private set; }
	[field: SerializeField] public float ReflectionWeight { get; private set; }
}