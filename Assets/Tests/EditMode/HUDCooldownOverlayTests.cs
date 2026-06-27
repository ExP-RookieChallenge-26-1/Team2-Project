#if UNITY_EDITOR
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class HUDCooldownOverlayTests
{
    [Test]
    public void CooldownOverlayShowsLockWhenSkillIsNotAcquired()
    {
        using OverlayContext context = OverlayContext.Create();

        context.Widget.Render(new CooldownOverlayState(
            isAcquired: false,
            remainingSeconds: 0f,
            cooldownRatio: 0f));

        Assert.That(context.Overlay.enabled, Is.True);
        Assert.That(context.Overlay.fillAmount, Is.EqualTo(1f).Within(0.001f));
        Assert.That(context.Label.gameObject.activeSelf, Is.False);
        Assert.That(context.LockIcon.gameObject.activeSelf, Is.True);
    }

    [Test]
    public void CooldownOverlayShowsCeilingRemainingSecondsWhileCoolingDown()
    {
        using OverlayContext context = OverlayContext.Create();

        context.Widget.Render(new CooldownOverlayState(
            isAcquired: true,
            remainingSeconds: 3.2f,
            cooldownRatio: 0.42f));

        Assert.That(context.Overlay.enabled, Is.True);
        Assert.That(context.Overlay.type, Is.EqualTo(Image.Type.Filled));
        Assert.That(context.Overlay.fillMethod, Is.EqualTo(Image.FillMethod.Radial360));
        Assert.That(context.Overlay.fillClockwise, Is.True);
        Assert.That(context.Overlay.fillAmount, Is.EqualTo(0.42f).Within(0.001f));
        Assert.That(context.Label.gameObject.activeSelf, Is.True);
        Assert.That(context.Label.text, Is.EqualTo("4"));
        Assert.That(context.LockIcon.gameObject.activeSelf, Is.False);
    }

    [Test]
    public void CooldownOverlayHidesWhenAcquiredSkillIsReady()
    {
        using OverlayContext context = OverlayContext.Create();

        context.Widget.Render(new CooldownOverlayState(
            isAcquired: true,
            remainingSeconds: 0f,
            cooldownRatio: 0f));

        Assert.That(context.Overlay.enabled, Is.False);
        Assert.That(context.Widget.gameObject.activeSelf, Is.True);
        Assert.That(context.Label.gameObject.activeSelf, Is.False);
        Assert.That(context.LockIcon.gameObject.activeSelf, Is.False);
    }

    [Test]
    public void CooldownStateUsesTriggerFlagForAcquisition()
    {
        using SkillContext context = SkillContext.Create();
        context.Skill.SetTriggerSettings(hasManualTrigger: true, hasAutoTrigger: false);
        context.Skill.SetManualCooldown(10f);
        context.Skill.TryManualActivate();

        CooldownOverlayState manual = CooldownOverlayState.FromSkill(
            context.Skill,
            CooldownOverlayMode.Manual);
        CooldownOverlayState auto = CooldownOverlayState.FromSkill(
            context.Skill,
            CooldownOverlayMode.Auto);

        Assert.That(manual.IsAcquired, Is.True);
        Assert.That(manual.CooldownRatio, Is.EqualTo(1f).Within(0.001f));
        Assert.That(auto.IsAcquired, Is.False);
    }

    [Test]
    public void GameSceneHudCreatesAutoAndManualCooldownOverlays()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity");

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        Assert.That(canvas, Is.Not.Null);

        HUD hud = Object.FindFirstObjectByType<HUD>();
        Assert.That(hud, Is.Not.Null);
        InvokePrivateMethod(hud, "RebuildRuntimeSkillButtons");

        RectTransform cloneAuto = GetRect(canvas.transform, "HUD/PlayerStatus/Frame/AutoCooldownOverlayClone");
        RectTransform giantAuto = GetRect(canvas.transform, "HUD/PlayerStatus/Frame/AutoCooldownOverlayGiant");
        RectTransform giantManual = GetRect(canvas.transform, "HUD/PlayerStatus/Skill1Button/ManualCooldownOverlayGiant");
        RectTransform cloneManual = GetRect(canvas.transform, "HUD/PlayerStatus/Skill2Button/ManualCooldownOverlayClone");

        AssertOverlayRect(cloneAuto, new Vector2(115f, 72f), new Vector2(80f, 80f));
        AssertOverlayRect(giantAuto, new Vector2(212f, 74f), new Vector2(80f, 80f));
        AssertOverlayRect(giantManual, Vector2.zero, Vector2.zero);
        AssertOverlayRect(cloneManual, Vector2.zero, Vector2.zero);
    }

    private static void AssertOverlayRect(RectTransform rect, Vector2 expectedPosition, Vector2 expectedSize)
    {
        Assert.That(rect.GetComponent<CooldownOverlayWidget>(), Is.Not.Null);
        Assert.That(rect.GetComponent<Image>(), Is.Not.Null);
        Assert.That(rect.GetComponentInChildren<TextMeshProUGUI>(true), Is.Not.Null);
        Assert.That(rect.GetComponentInChildren<LockIconGraphic>(true), Is.Not.Null);
        AssertVector(rect.anchoredPosition, expectedPosition);
        AssertVector(rect.sizeDelta, expectedSize);
    }

    private static RectTransform GetRect(Transform parent, string path)
    {
        Transform target = parent.Find(path);
        Assert.That(target, Is.Not.Null, path);

        RectTransform rect = target as RectTransform;
        Assert.That(rect, Is.Not.Null, path);
        return rect;
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

    private sealed class OverlayContext : System.IDisposable
    {
        private OverlayContext(GameObject root, CooldownOverlayWidget widget, Image overlay, TextMeshProUGUI label, LockIconGraphic lockIcon)
        {
            Root = root;
            Widget = widget;
            Overlay = overlay;
            Label = label;
            LockIcon = lockIcon;
        }

        private GameObject Root { get; }
        public CooldownOverlayWidget Widget { get; }
        public Image Overlay { get; }
        public TextMeshProUGUI Label { get; }
        public LockIconGraphic LockIcon { get; }

        public static OverlayContext Create()
        {
            GameObject root = new("Overlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CooldownOverlayWidget));
            Image overlay = root.GetComponent<Image>();
            CooldownOverlayWidget widget = root.GetComponent<CooldownOverlayWidget>();

            GameObject labelObject = new("CooldownText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(root.transform, false);
            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();

            GameObject lockObject = new("LockIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(LockIconGraphic));
            lockObject.transform.SetParent(root.transform, false);
            LockIconGraphic lockIcon = lockObject.GetComponent<LockIconGraphic>();

            widget.ConfigureVisuals(overlay, label, lockIcon);
            return new OverlayContext(root, widget, overlay, label, lockIcon);
        }

        public void Dispose()
        {
            Object.DestroyImmediate(Root);
        }
    }

    private sealed class TestBallSkill : BallSkill
    {
        protected override void OnActivate()
        {
        }

        protected override void OnDeactivate()
        {
        }
    }

    private sealed class SkillContext : System.IDisposable
    {
        private SkillContext(GameObject root, TestBallSkill skill)
        {
            Root = root;
            Skill = skill;
        }

        private GameObject Root { get; }
        public TestBallSkill Skill { get; }

        public static SkillContext Create()
        {
            GameObject root = new("Skill");
            TestBallSkill skill = root.AddComponent<TestBallSkill>();
            return new SkillContext(root, skill);
        }

        public void Dispose()
        {
            Object.DestroyImmediate(Root);
        }
    }
}
#endif
