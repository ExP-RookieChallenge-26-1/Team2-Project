using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillCooldownUI : MonoBehaviour
{
	public enum SkillSlot { Skill1, Skill2 }
	public enum CooldownType { Auto, Manual }

	[SerializeField] private SkillSlot skillSlot;
	[SerializeField] private CooldownType cooldownType;
	[SerializeField] private Image cooldownOverlay;
	[SerializeField] private TextMeshProUGUI cooldownText;

	private Ball cachedBall;

	private void Update()
	{
		BallSkill skill = GetTargetSkill();

		float maxCooldown = this.cooldownType == CooldownType.Auto ? skill?.AutoCooldown ?? 0f : skill?.ManualCooldown ?? 0f;

		if (skill == null || maxCooldown == 0f)
		{
			this.cooldownOverlay.gameObject.SetActive(false);
			this.cooldownText.gameObject.SetActive(false);
			return;
		}

		float ratio = this.cooldownType == CooldownType.Auto ? skill.AutoCooldownRatio : skill.ManualCooldownRatio;
		float remaining = this.cooldownType == CooldownType.Auto ? skill.AutoCooldownRemaining : skill.ManualCooldownRemaining;
		bool onCooldown = ratio > 0f;

		this.cooldownOverlay.gameObject.SetActive(onCooldown);
		this.cooldownText.gameObject.SetActive(onCooldown);
		this.cooldownOverlay.fillAmount = ratio;

		if (onCooldown)
			this.cooldownText.text = remaining.ToString("F1");
	}

	private BallSkill GetTargetSkill()
	{
		if (this.cachedBall == null)
			this.cachedBall = FindFirstObjectByType<Ball>();

		if (this.cachedBall == null)
			return null;

		return this.skillSlot == SkillSlot.Skill1
			? (BallSkill)this.cachedBall.GiantSkill
			: this.cachedBall.CloneSkill;
	}
}
