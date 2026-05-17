using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillEventChannel", menuName = "EventChannel/SkillEventChannel")]
public class SkillEventChannel : ScriptableObject
{
	public event Action OnSkill1Activated;
	public event Action OnSkill2Activated;

	public void RaiseSkill1()
	{
		this.OnSkill1Activated?.Invoke();
	}

	public void RaiseSkill2()
	{
		this.OnSkill2Activated?.Invoke();
	}
}