using UnityEngine;

public class MakeGround : MonoBehaviour
{
    public GameObject ground;
    public GameObject monster;

    float timer = 0;
    public float timediff;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > timediff)
        {
            GameObject newgorund = Instantiate(ground);

            float randomWidth = Random.Range(1f, 2f);

            newgorund.transform.localScale = new Vector3(randomWidth, 1f, 1f);
            newgorund.transform.position = new Vector3(Random.Range(-3.5f, 2.8f), 5.4f, 0);

            if (Random.value < 0.7f)
            {
                GameObject monsterObj = Instantiate(monster);

                float offsetX = Random.Range(-randomWidth / 2f, randomWidth / 2f);

                float worldX = newgorund.transform.position.x + offsetX;

                worldX = Mathf.Clamp(worldX, -2.5f, 2.5f);

                monsterObj.transform.position = new Vector3(
                    worldX,
                    newgorund.transform.position.y + 2f, 0);
                monsterObj.transform.parent = newgorund.transform;

            }

            timer = 0;
            Destroy(newgorund, 15.0f);
        }

    }
}
