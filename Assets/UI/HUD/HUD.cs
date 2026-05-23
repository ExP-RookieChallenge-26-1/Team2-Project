using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
	[SerializeField] private Button skill1Button;
	[SerializeField] private Button skill2Button;
	[SerializeField] private SkillEventChannel skillEventChannel;

	private void Start()
	{
		this.skill1Button.onClick.AddListener(OnClickSkill1);
		this.skill2Button.onClick.AddListener(OnClickSkill2);
	}

	private void Oestroy()
	{
		this.skill1Button.onClick.RemoveListener(OnClickSkill1);
		this.skill2Button.onClick.RemoveListener(OnClickSkill2);
	}
	private void OnClickSkill1()
	{
		this.skillEventChannel.RaiseSkill1();
	}

	private void OnClickSkill2()
	{
		this.skillEventChannel.RaiseSkill2();	
	}
}