using UnityEngine;

public class UserLevel : MonoBehaviour
{
	[SerializeField] private int currentLevel = 1;
	[SerializeField] private int currentExp = 0;
	[SerializeField] private int[] requiredExpByLevel = { 10, 20, 35, 50, 70, 100 };

	[Header("Default Upgrade Pool")]
	[SerializeField] private int[] defaultUpgradeIds = { 1, 2, 3, 4, 5, 6 };
	[SerializeField] private float[] defaultUpgradeWeights = { 30f, 25f, 20f, 15f, 10f, 20f };

	[SerializeField] private EnhanceUI enhanceUI;

	public int CurrentLevel => this.currentLevel;
	public int CurrentExp => this.currentExp;

	private void Start()
	{
		if (this.enhanceUI != null)
			this.enhanceUI.gameObject.SetActive(false);
	}

	public void AddExp(int amount)
	{
		this.currentExp += amount;
		Debug.Log($"경험치 획득: +{amount}, 현재 EXP: {this.currentExp}");
		CheckLevelUp();
	}

	private void CheckLevelUp()
	{
		while (this.currentLevel - 1 < this.requiredExpByLevel.Length &&
		       this.currentExp >= this.requiredExpByLevel[this.currentLevel - 1])
		{
			this.currentExp -= this.requiredExpByLevel[this.currentLevel - 1];
			this.currentLevel++;

			Debug.Log($"레벨업! 현재 레벨: {this.currentLevel}");
			OpenDefaultUpgradeUI();
		}
	}

	private void OpenDefaultUpgradeUI()
	{
		OpenUpgradeUI(this.defaultUpgradeIds, this.defaultUpgradeWeights);
	}

	public void OpenUpgradeUI(int[] ids, float[] weights)
	{
		if (this.enhanceUI == null)
		{
			Debug.LogWarning("EnhanceUI is not assigned.");
			return;
		}

		if (ids == null || weights == null || ids.Length == 0 || ids.Length != weights.Length)
		{
			Debug.LogWarning("Upgrade UI open skipped because the card pool is invalid.");
			return;
		}

		GameManager.Instance.State.Change(GameStateMachine.State.Enhancement);
		this.enhanceUI.gameObject.SetActive(true);
		this.enhanceUI.ShowCardsByIds(ids, weights);
	}
}
