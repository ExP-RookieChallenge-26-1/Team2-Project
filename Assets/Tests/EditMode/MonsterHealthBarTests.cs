#if UNITY_EDITOR
using NUnit.Framework;
using UnityEngine;

public class MonsterHealthBarTests
{
    [Test]
    public void HealthBarStaysInsideOwnerSortingBand()
    {
        GameObject enemy = new GameObject("Enemy");
        SpriteRenderer enemyRenderer = enemy.AddComponent<SpriteRenderer>();
        enemyRenderer.sortingOrder = 1;
        MonsterHealthBar enemyHealthBar = enemy.AddComponent<MonsterHealthBar>();

        GameObject boss = new GameObject("Boss");
        SpriteRenderer bossRenderer = boss.AddComponent<SpriteRenderer>();
        bossRenderer.sortingOrder = 7;

        try
        {
            enemyHealthBar.SetHealth(10, 10);

            GameObject barRoot = GameObject.Find("Enemy_HealthBar");
            Assert.That(barRoot, Is.Not.Null);

            SpriteRenderer bodyRenderer = barRoot.transform.Find("Body").GetComponent<SpriteRenderer>();
            SpriteRenderer fillRenderer = barRoot.transform.Find("Fill").GetComponent<SpriteRenderer>();

            Assert.That(bodyRenderer.sortingOrder, Is.GreaterThan(enemyRenderer.sortingOrder));
            Assert.That(fillRenderer.sortingOrder, Is.GreaterThan(bodyRenderer.sortingOrder));
            Assert.That(fillRenderer.sortingOrder, Is.LessThan(bossRenderer.sortingOrder));
        }
        finally
        {
            GameObject barRoot = GameObject.Find("Enemy_HealthBar");
            if (barRoot != null)
                Object.DestroyImmediate(barRoot);

            Object.DestroyImmediate(enemy);
            Object.DestroyImmediate(boss);
        }
    }

    [Test]
    public void SetHealthHidesBarImmediatelyAtZeroHp()
    {
        GameObject monster = new GameObject("Monster");
        monster.AddComponent<SpriteRenderer>();
        MonsterHealthBar healthBar = monster.AddComponent<MonsterHealthBar>();

        try
        {
            healthBar.SetHealth(10, 10);
            GameObject barRoot = GameObject.Find("Monster_HealthBar");

            Assert.That(barRoot, Is.Not.Null);
            Assert.That(barRoot.activeSelf, Is.True);

            healthBar.SetHealth(0, 10);

            Assert.That(barRoot.activeSelf, Is.False);
        }
        finally
        {
            GameObject barRoot = GameObject.Find("Monster_HealthBar");
            if (barRoot != null)
                Object.DestroyImmediate(barRoot);

            Object.DestroyImmediate(monster);
        }
    }

    [Test]
    public void EnemyHealthBarStaysVisibleAtZeroHp()
    {
        GameObject enemy = new GameObject("Enemy");
        enemy.AddComponent<SpriteRenderer>();
        enemy.AddComponent<Enemy>();

        try
        {
            MonsterHealthBar healthBar = enemy.GetComponent<MonsterHealthBar>();
            Assert.That(healthBar, Is.Not.Null);

            healthBar.SetHealth(10, 10);
            GameObject barRoot = GameObject.Find("Enemy_HealthBar");

            Assert.That(barRoot, Is.Not.Null);
            Assert.That(barRoot.activeSelf, Is.True);

            healthBar.SetHealth(0, 10);

            Assert.That(barRoot.activeSelf, Is.True);
        }
        finally
        {
            GameObject barRoot = GameObject.Find("Enemy_HealthBar");
            if (barRoot != null)
                Object.DestroyImmediate(barRoot);

            Object.DestroyImmediate(enemy);
        }
    }
}
#endif
