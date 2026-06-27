public enum CardOfferSource
{
	None,
	LevelUp,
	Item
}

public readonly struct CardUseContext
{
	public static readonly CardUseContext None = new CardUseContext(CardOfferSource.None, 0);

	public CardUseContext(CardOfferSource source, int progressionIndex)
	{
		Source = source;
		ProgressionIndex = progressionIndex;
	}

	public CardOfferSource Source { get; }
	public int ProgressionIndex { get; }
	public bool HasProgressionIndex => Source != CardOfferSource.None && ProgressionIndex >= 0;
}
