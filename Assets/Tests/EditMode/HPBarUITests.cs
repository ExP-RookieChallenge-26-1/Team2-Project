#if UNITY_EDITOR
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class HPBarUITests
{
    public static void RunSetFillKeepsRectTransformAndClipsImagePixels()
    {
        new HPBarUITests().SetFillKeepsRectTransformAndClipsImagePixels();
    }

    [Test]
    public void SetFillKeepsRectTransformAndClipsImagePixels()
    {
        GameObject gameObject = new GameObject("HPBar", typeof(RectTransform));
        try
        {
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 200f);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 20f);
            rectTransform.anchoredPosition = new Vector2(30f, 10f);

            Image image = gameObject.AddComponent<Image>();
            HPBarUI hpBarUI = gameObject.AddComponent<HPBarUI>();
            SetPrivateField(hpBarUI, "fillImage", image);

            Vector2 initialSize = rectTransform.sizeDelta;
            Vector2 initialPosition = rectTransform.anchoredPosition;

            InvokeSetFill(hpBarUI, 0.25f);

            Assert.That(rectTransform.sizeDelta, Is.EqualTo(initialSize));
            Assert.That(rectTransform.anchoredPosition, Is.EqualTo(initialPosition));
            Assert.That(image.type, Is.EqualTo(Image.Type.Filled));
            Assert.That(image.fillMethod, Is.EqualTo(Image.FillMethod.Horizontal));
            Assert.That(image.fillOrigin, Is.EqualTo((int)Image.OriginHorizontal.Left));
            Assert.That(image.fillAmount, Is.EqualTo(0.25f).Within(0.001f));
        }
        finally
        {
            Object.DestroyImmediate(gameObject);
        }
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        field.SetValue(target, value);
    }

    private static void InvokeSetFill(HPBarUI hpBarUI, float ratio)
    {
        MethodInfo method = typeof(HPBarUI).GetMethod("SetFill", BindingFlags.Instance | BindingFlags.NonPublic);
        method.Invoke(hpBarUI, new object[] { ratio });
    }
}
#endif
