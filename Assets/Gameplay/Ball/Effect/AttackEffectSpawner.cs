using UnityEngine;

public class AttackEffectSpawner : MonoBehaviour
{
    public static AttackEffectSpawner Instance { get; private set; }

    [SerializeField] private GameObject attackUpEffectPrefab;
    [SerializeField] private GameObject attackDownEffectPrefab;
    [SerializeField] private float destroyDelay = 0.25f;

    private void Awake()
    {
        Instance = this;
    }

    public void Spawn(Vector2 position, bool isMovingUp, bool flipX)
    {
        GameObject prefab = isMovingUp ? this.attackUpEffectPrefab : this.attackDownEffectPrefab;
        if (prefab == null)
            return;

        GameObject obj = Instantiate(prefab, position, Quaternion.identity);

        AttackEffect effect = obj.GetComponent<AttackEffect>();
        if (effect != null)
        {
            effect.ScrollSpeed = GameManager.Instance.WorldStats.ScrollSpeed;
            effect.DestroyDelay = this.destroyDelay;
        }

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.flipX = flipX;
    }
}
