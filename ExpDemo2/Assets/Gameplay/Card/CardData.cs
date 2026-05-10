using UnityEngine;

public abstract class CardData : ScriptableObject
{
	[field: SerializeField] public int Id { get; private set; }
	[field: SerializeField] public string CardName { get; private set; }
	[field: SerializeField] public Sprite Icon { get; private set; }
	[field: SerializeField, TextArea] public string Description { get; private set; }

	public abstract void Apply();
}
