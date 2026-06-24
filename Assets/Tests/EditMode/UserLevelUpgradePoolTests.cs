#if UNITY_EDITOR
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class UserLevelUpgradePoolTests
{
    [Test]
    public void GetDefaultUpgradePoolForLevelUsesMatchingPoolAndClampsToLast()
    {
        GameObject gameObject = new GameObject("UserLevel");
        UserLevel userLevel = gameObject.AddComponent<UserLevel>();
        UserLevel.UpgradePool[] pools =
        {
            new UserLevel.UpgradePool(new[] { 1, 2 }, new[] { 10f, 20f }),
            new UserLevel.UpgradePool(new[] { 3, 4 }, new[] { 30f, 40f })
        };
        SetPrivateField(userLevel, "defaultUpgradePools", pools);

        Assert.That(userLevel.GetDefaultUpgradePoolForLevel(2).UpgradeIds, Is.EqualTo(new[] { 1, 2 }));
        Assert.That(userLevel.GetDefaultUpgradePoolForLevel(3).UpgradeIds, Is.EqualTo(new[] { 3, 4 }));
        Assert.That(userLevel.GetDefaultUpgradePoolForLevel(10).UpgradeIds, Is.EqualTo(new[] { 3, 4 }));

        Object.DestroyImmediate(gameObject);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        field.SetValue(target, value);
    }
}
#endif
