#if UNITY_EDITOR
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class HUDRuntimeSkillButtonTests
{
    private static readonly Vector2 Skill1Position = new(-70f, -490f);
    private static readonly Vector2 Skill2Position = new(-70f, -350f);

    [Test]
    public void HudKeepsSerializedSkillButtonsVisibleWhenTheyExist()
    {
        GameObject hudObject = new("HUD");
        hudObject.SetActive(false);
        RectTransform hudRect = hudObject.AddComponent<RectTransform>();
        HUD hud = hudObject.AddComponent<HUD>();
        Button legacySkill1 = CreateLegacyButton("LegacySkill1Button", hudRect);
        Button legacySkill2 = CreateLegacyButton("LegacySkill2Button", hudRect);
        legacySkill1.gameObject.SetActive(false);
        legacySkill2.gameObject.SetActive(false);

        SetPrivateField(hud, "skill1Button", legacySkill1);
        SetPrivateField(hud, "skill2Button", legacySkill2);

        InvokePrivateMethod(hud, "RebuildRuntimeSkillButtons");

        Assert.That(hudObject.transform.Find("RuntimeSkillButtons"), Is.Null);
        Assert.That(legacySkill1.gameObject.activeSelf, Is.True);
        Assert.That(legacySkill2.gameObject.activeSelf, Is.True);

        Object.DestroyImmediate(hudObject);
    }

    [Test]
    public void HudCreatesFallbackSkillButtonsOnRightSideWhenSerializedButtonsAreMissing()
    {
        GameObject hudObject = new("HUD");
        hudObject.SetActive(false);
        HUD hud = hudObject.AddComponent<HUD>();

        InvokePrivateMethod(hud, "RebuildRuntimeSkillButtons");

        Transform container = hudObject.transform.Find("RuntimeSkillButtons");
        Assert.That(container, Is.Not.Null);

        AssertRuntimeSkillButton(container, "Skill1Button", Skill1Position);
        AssertRuntimeSkillButton(container, "Skill2Button", Skill2Position);

        Object.DestroyImmediate(hudObject);
    }

    private static void AssertRuntimeSkillButton(Transform container, string name, Vector2 expectedPosition)
    {
        RectTransform rect = container.Find(name) as RectTransform;
        Assert.That(rect, Is.Not.Null, name);
        AssertVector(rect.anchorMin, new Vector2(1f, 0.5f));
        AssertVector(rect.anchorMax, new Vector2(1f, 0.5f));
        AssertVector(rect.anchoredPosition, expectedPosition);
        AssertVector(rect.sizeDelta, new Vector2(120f, 120f));
        Assert.That(rect.GetComponent<Button>(), Is.Not.Null);
        Assert.That(rect.GetComponent<Image>(), Is.Not.Null);
    }

    private static Button CreateLegacyButton(string name, Transform parent)
    {
        GameObject gameObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        gameObject.transform.SetParent(parent, false);
        return gameObject.GetComponent<Button>();
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, fieldName);
        field.SetValue(target, value);
    }

    private static void InvokePrivateMethod(object target, string methodName)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null, methodName);
        method.Invoke(target, null);
    }

    private static void AssertVector(Vector2 actual, Vector2 expected)
    {
        Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.001f));
        Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.001f));
    }

}
#endif
