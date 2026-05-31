using UnityEngine;

public class User : MonoBehaviour
{
	public UserHealth Health { get; private set; }
	public UserLevel Level { get; private set; }

	private void Awake()
	{
		this.Health = GetComponent<UserHealth>();
		this.Level = GetComponent<UserLevel>();
	}
}
