using UnityEngine;

public abstract class CardData : ScriptableObject
{
	[field: SerializeField] public int Id { get; private set; }
	[field: SerializeField] public string CardName { get; private set; }
	[field: SerializeField] public Sprite Icon { get; private set; }
	[field: SerializeField, TextArea] public string Description { get; private set; }

	protected void InitializeCard(int id, string cardName, Sprite icon, string description)
	{
		Id = id;
		CardName = cardName;
		Icon = icon;
		Description = description;
	}

	public abstract void Apply();

	public virtual void Apply(CardUseContext context)
	{
		Apply();
	}

	public virtual string GetCardName(CardUseContext context)
	{
		return CardName;
	}

	public virtual string GetDescription(CardUseContext context)
	{
		return Description;
	}
}

public sealed class ScoreBonusCardData : CardData
{
	[SerializeField] private int scoreAmount = CardIds.ScoreBonusMinAmount;

	public int ScoreAmount => GetScoreAmount();

	public void InitializeForRuntime(int amount)
	{
		int normalizedAmount = CardIds.GetScoreBonusAmount(CardIds.GetScoreBonusId(amount));
		this.scoreAmount = normalizedAmount;
		InitializeCard(
			CardIds.GetScoreBonusId(normalizedAmount),
			$"점수 +{normalizedAmount}",
			Resources.Load<Sprite>("Cards/ScoreBonusCard"),
			$"점수가 {normalizedAmount}점 증가합니다.");
	}

	public override void Apply()
	{
		ScoreManager.AddScoreToSession(GetScoreAmount());
	}

	public override string GetCardName(CardUseContext context)
	{
		return $"점수 +{GetScoreAmount()}";
	}

	public override string GetDescription(CardUseContext context)
	{
		return $"점수가 {GetScoreAmount()}점 증가합니다.";
	}

	private int GetScoreAmount()
	{
		int amountFromId = CardIds.GetScoreBonusAmount(Id);
		if (amountFromId > 0)
			return amountFromId;

		return CardIds.GetScoreBonusAmount(CardIds.GetScoreBonusId(this.scoreAmount));
	}
}
