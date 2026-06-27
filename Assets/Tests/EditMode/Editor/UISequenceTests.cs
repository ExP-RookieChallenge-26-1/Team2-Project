#if UNITY_EDITOR
using System;
using System.Reflection;
using TMPro;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class UISequenceTests
{
    [TestCase("Assets/UI/GameOverPanel.prefab")]
    [TestCase("Assets/UI/GameClearPanel.prefab")]
    public void ResultPanelExitButtonHasRuntimeTitleReturnHandler(string prefabPath)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        Assert.That(prefab, Is.Not.Null);

        Transform exitButtonTransform = prefab.transform.Find("ExitButton");
        Assert.That(exitButtonTransform, Is.Not.Null);

        Button exitButton = exitButtonTransform.GetComponent<Button>();
        Assert.That(exitButton, Is.Not.Null);
        Assert.That(exitButtonTransform.GetComponent("ReturnToTitleButton"), Is.Not.Null);

        for (int i = 0; i < exitButton.onClick.GetPersistentEventCount(); ++i)
        {
            Assert.That(
                exitButton.onClick.GetPersistentTarget(i),
                Is.Not.Null,
                $"{prefabPath} has a broken persistent OnClick target at index {i}.");
        }
    }

    [Test]
    public void SettingPanelCreditButtonOpensCreditPanelAndHidesSettings()
    {
        GameObject settingObject = new GameObject("SettingPanel");
        GameObject creditObject = new GameObject("CreditPanel");
        GameObject creditButtonObject = new GameObject("CreditButton");

        try
        {
            SettingPanel settingPanel = settingObject.AddComponent<SettingPanel>();
            CreditPanel creditPanel = creditObject.AddComponent<CreditPanel>();
            Button creditButton = creditButtonObject.AddComponent<Button>();

            creditButtonObject.transform.SetParent(settingObject.transform);
            settingObject.SetActive(true);
            creditObject.SetActive(false);

            SetPrivateField(settingPanel, "creditButton", creditButton);
            SetPrivateField(settingPanel, "creditPanel", creditPanel);
            InvokePrivate(settingPanel, "OnCreditClicked");

            Assert.That(settingObject.activeSelf, Is.False);
            Assert.That(creditObject.activeSelf, Is.True);
        }
        finally
        {
            Object.DestroyImmediate(creditButtonObject);
            Object.DestroyImmediate(creditObject);
            Object.DestroyImmediate(settingObject);
        }
    }

    [Test]
    public void CreditPanelSkipReturnsToCallingSettingPanel()
    {
        GameObject settingObject = new GameObject("SettingPanel");
        GameObject creditObject = new GameObject("CreditPanel");
        GameObject skipButtonObject = new GameObject("SkipButton");

        try
        {
            creditObject.SetActive(false);
            CreditPanel creditPanel = creditObject.AddComponent<CreditPanel>();
            Button skipButton = skipButtonObject.AddComponent<Button>();

            skipButtonObject.transform.SetParent(creditObject.transform);
            SetPrivateField(creditPanel, "skipButton", skipButton);

            settingObject.SetActive(false);

            creditPanel.ShowFrom(settingObject);
            Assert.That(creditObject.activeSelf, Is.True);
            Assert.That(settingObject.activeSelf, Is.False);

            skipButton.onClick.Invoke();
            Assert.That(creditObject.activeSelf, Is.False);
            Assert.That(settingObject.activeSelf, Is.True);
        }
        finally
        {
            Object.DestroyImmediate(skipButtonObject);
            Object.DestroyImmediate(creditObject);
            Object.DestroyImmediate(settingObject);
        }
    }

    [Test]
    public void CreditPanelKeepsAuthoredTextPositionWhenScrollIsDisabled()
    {
        GameObject creditObject = new GameObject("CreditPanel", typeof(RectTransform));
        GameObject creditTextObject = new GameObject("CreditText", typeof(RectTransform));

        try
        {
            creditObject.SetActive(false);
            CreditPanel creditPanel = creditObject.AddComponent<CreditPanel>();
            creditTextObject.transform.SetParent(creditObject.transform);
            TextMeshProUGUI creditText = creditTextObject.AddComponent<TextMeshProUGUI>();
            RectTransform creditTextRect = creditText.transform as RectTransform;
            Assert.That(creditTextRect, Is.Not.Null);
            creditTextRect.anchoredPosition = new Vector2(0f, 120f);

            SetPrivateField(creditPanel, "creditText", creditText);
            SetPrivateField(creditPanel, "scrollSpeed", 0f);

            creditPanel.ShowFrom(null);

            Assert.That(creditObject.activeSelf, Is.True);
            Assert.That(creditTextRect.anchoredPosition.y, Is.EqualTo(120f).Within(0.001f));
        }
        finally
        {
            Object.DestroyImmediate(creditTextObject);
            Object.DestroyImmediate(creditObject);
        }
    }

    [Test]
    public void TitleSceneMatchesRequestedTitleLayout()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/TitleScene.unity");

        TitleUI titleUI = Object.FindFirstObjectByType<TitleUI>();
        Assert.That(titleUI, Is.Not.Null);
        InvokePrivate(titleUI, "ApplyTitleLayout");
        Assert.That(titleUI, Is.AssignableTo<IPointerClickHandler>());

        Image titleHitArea = titleUI.GetComponent<Image>();
        Assert.That(titleHitArea, Is.Not.Null);
        Assert.That(titleHitArea.raycastTarget, Is.True);
        Assert.That(titleHitArea.color.a, Is.GreaterThan(0f));
        Assert.That(titleHitArea.color.a, Is.LessThanOrEqualTo(0.01f));
        Assert.That(titleHitArea.canvasRenderer.cullTransparentMesh, Is.False);

        Button startButton = GetPrivateField<Button>(titleUI, "startButton");
        Assert.That(startButton, Is.Not.Null);

        TextMeshProUGUI startLabel = startButton.GetComponentInChildren<TextMeshProUGUI>(true);
        Assert.That(startLabel, Is.Not.Null);
        Assert.That(startLabel.text, Is.EqualTo("touch to start"));
        Assert.That(startLabel.color, Is.EqualTo(Color.white));
        Assert.That(startButton.targetGraphic, Is.EqualTo(startLabel));
        Assert.That(startLabel.raycastTarget, Is.False);

        RectTransform startLabelRect = startLabel.transform as RectTransform;
        Assert.That(startLabelRect, Is.Not.Null);
        InvokePrivate(titleUI, "ApplyStartPromptPulse", 0.25f);
        Assert.That(startLabelRect.localScale.x, Is.GreaterThan(1f));
        InvokePrivate(titleUI, "ApplyStartPromptPulse", 0.75f);
        Assert.That(startLabelRect.localScale.x, Is.LessThan(1f));

        Image startImage = startButton.GetComponent<Image>();
        Assert.That(startImage, Is.Not.Null);
        Assert.That(startImage.raycastTarget, Is.True);
        Assert.That(startImage.color.a, Is.GreaterThan(0f));
        Assert.That(startImage.color.a, Is.LessThanOrEqualTo(0.01f));
        Assert.That(startImage.canvasRenderer.cullTransparentMesh, Is.False);

        Button settingButton = GetPrivateField<Button>(titleUI, "settingButton");
        Assert.That(settingButton, Is.Not.Null);

        RectTransform settingRect = settingButton.transform as RectTransform;
        Assert.That(settingRect, Is.Not.Null);
        Assert.That(settingRect.anchorMin, Is.EqualTo(new Vector2(1f, 1f)));
        Assert.That(settingRect.anchorMax, Is.EqualTo(new Vector2(1f, 1f)));
        Assert.That(settingRect.anchoredPosition.x, Is.LessThan(0f));
        Assert.That(settingRect.anchoredPosition.y, Is.LessThan(0f));
        Assert.That(settingRect.sizeDelta.x, Is.EqualTo(176f).Within(0.001f));
        Assert.That(settingRect.sizeDelta.y, Is.EqualTo(176f).Within(0.001f));

        Canvas titleCanvas = titleUI.GetComponentInParent<Canvas>();
        Assert.That(titleCanvas, Is.Not.Null);
        Assert.That(settingButton.transform.parent, Is.EqualTo(titleCanvas.transform));
        Assert.That(settingButton.transform.GetSiblingIndex(), Is.EqualTo(titleCanvas.transform.childCount - 1));
        Assert.That(settingButton.gameObject.activeSelf, Is.True);

        Image settingImage = settingButton.GetComponent<Image>();
        Assert.That(settingImage, Is.Not.Null);
        SettingButtonVisual titleSettingVisual = settingButton.GetComponent<SettingButtonVisual>();
        Assert.That(titleSettingVisual, Is.Not.Null);
        InvokePrivate(titleSettingVisual, "ApplyVisual");
        Assert.That(settingImage.sprite == null, Is.False);
        Assert.That(settingImage.sprite.name, Is.EqualTo("setting-button-generated"));
        Assert.That(settingImage.preserveAspect, Is.True);
        Assert.That(settingImage.color.a, Is.EqualTo(1f).Within(0.001f));
        Assert.That(settingImage.color, Is.EqualTo(Color.white));
        Assert.That(settingButton.transition, Is.EqualTo(Selectable.Transition.ColorTint));

        TextMeshProUGUI settingLabel = settingButton.GetComponentInChildren<TextMeshProUGUI>(true);
        Assert.That(settingLabel, Is.Not.Null);
        Assert.That(settingLabel.text, Is.EqualTo(string.Empty));
        Assert.That(settingLabel.color, Is.EqualTo(Color.white));
        Assert.That(settingLabel.raycastTarget, Is.False);

        Button creditButton = GetPrivateField<Button>(titleUI, "creditButton");
        Assert.That(creditButton, Is.Not.Null);
        Assert.That(creditButton.gameObject.activeSelf, Is.False);

        InvokePrivate(titleUI, "OnSettingClicked");
        Assert.That(titleUI.gameObject.activeSelf, Is.False);
        Assert.That(settingButton.gameObject.activeSelf, Is.False);
        Assert.That(GetPrivateField<SettingPanel>(titleUI, "settingPanel").gameObject.activeSelf, Is.True);
    }

    [Test]
    public void TitleSceneCreditPanelUsesVerticalScrollAndTeamTwoCredits()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/TitleScene.unity");

        CreditPanel creditPanel = Object.FindFirstObjectByType<CreditPanel>(FindObjectsInactive.Include);
        Assert.That(creditPanel, Is.Not.Null);
        Assert.That(creditPanel.gameObject.activeSelf, Is.False);

        RectTransform creditRect = creditPanel.transform as RectTransform;
        Assert.That(creditRect, Is.Not.Null);
        Assert.That(creditRect.anchorMin, Is.EqualTo(new Vector2(0.5f, 0.5f)));
        Assert.That(creditRect.anchorMax, Is.EqualTo(new Vector2(0.5f, 0.5f)));
        Assert.That(creditRect.sizeDelta.x, Is.EqualTo(692f).Within(0.01f));
        Assert.That(creditRect.sizeDelta.y, Is.EqualTo(853f).Within(0.01f));

        Image creditImage = creditPanel.GetComponent<Image>();
        Assert.That(creditImage, Is.Not.Null);
        Assert.That(creditImage.sprite, Is.Not.Null);
        Assert.That(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(creditImage.sprite)), Is.EqualTo("0a0d8abd1ccbde14d9ab4befcfc97364"));
        Assert.That(creditImage.preserveAspect, Is.True);

        TextMeshProUGUI creditText = GetPrivateField<TextMeshProUGUI>(creditPanel, "creditText");
        Assert.That(creditText, Is.Not.Null);
        Assert.That(creditText.text, Does.Contain("2팀"));
        Assert.That(creditText.text, Does.Contain("메인 기획  김태윤"));
        Assert.That(creditText.text, Does.Contain("서브 기획  신은성"));
        Assert.That(creditText.text, Does.Contain("플머  안정윤"));
        Assert.That(creditText.text, Does.Contain("플머  최재윤"));
        Assert.That(creditText.text, Does.Contain("그래픽  최지우"));
        Assert.That(creditText.text, Does.Contain("그래픽  조윤시"));
        Assert.That(creditText.text, Does.Contain("그래픽  김서진"));
        Assert.That(creditText.text, Does.Contain("사운드  박재석"));
        Assert.That(creditText.text, Does.Not.Contain("차승민"));

        TextMeshProUGUI skipLabel = creditPanel.transform
            .Find("SkipButton/Text (TMP)")
            ?.GetComponent<TextMeshProUGUI>();
        Assert.That(skipLabel, Is.Not.Null);
        Assert.That(skipLabel.text, Is.EqualTo("닫기"));

        SettingPanel settingPanel = Object.FindFirstObjectByType<SettingPanel>(FindObjectsInactive.Include);
        Assert.That(settingPanel, Is.Not.Null);
        Assert.That(GetPrivateField<CreditPanel>(settingPanel, "creditPanel"), Is.SameAs(creditPanel));
    }

    [Test]
    public void GameSceneHasVisibleSettingButtonOnCanvas()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity");

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        Assert.That(canvas, Is.Not.Null);

        Transform settingTransform = canvas.transform.Find("SettingButton");
        Assert.That(settingTransform, Is.Not.Null);
        Assert.That(settingTransform.parent, Is.EqualTo(canvas.transform));
        Assert.That(settingTransform.GetSiblingIndex(), Is.EqualTo(canvas.transform.childCount - 1));
        Assert.That(settingTransform.gameObject.activeSelf, Is.True);

        RectTransform settingRect = settingTransform as RectTransform;
        Assert.That(settingRect, Is.Not.Null);
        Assert.That(settingRect.anchorMin, Is.EqualTo(new Vector2(1f, 1f)));
        Assert.That(settingRect.anchorMax, Is.EqualTo(new Vector2(1f, 1f)));
        Assert.That(settingRect.sizeDelta.x, Is.EqualTo(176f).Within(0.001f));
        Assert.That(settingRect.sizeDelta.y, Is.EqualTo(176f).Within(0.001f));

        Image settingImage = settingTransform.GetComponent<Image>();
        Assert.That(settingImage, Is.Not.Null);
        Assert.That(settingImage.sprite == null, Is.False);
        Assert.That(settingImage.sprite.name, Is.EqualTo("setting-button-generated"));
        Assert.That(settingImage.preserveAspect, Is.True);
        Assert.That(settingImage.color, Is.EqualTo(Color.white));
        Assert.That(settingImage.raycastTarget, Is.True);

        Button settingButton = settingTransform.GetComponent<Button>();
        Assert.That(settingButton, Is.Not.Null);
        Assert.That(settingButton.transition, Is.EqualTo(Selectable.Transition.ColorTint));
        Component gameSettingsButton = settingTransform.GetComponent("GameSettingsButton");
        Assert.That(gameSettingsButton, Is.Not.Null);

        SettingPanel sceneSettingPanel = Object.FindFirstObjectByType<SettingPanel>(FindObjectsInactive.Include);
        Assert.That(sceneSettingPanel, Is.Not.Null);
        Assert.That(sceneSettingPanel.gameObject.activeSelf, Is.False);
        Assert.That(GetPrivateField<SettingPanel>(gameSettingsButton, "settingPanel"), Is.SameAs(sceneSettingPanel));

        Image sceneSettingPanelImage = sceneSettingPanel.GetComponent<Image>();
        Assert.That(sceneSettingPanelImage, Is.Not.Null);
        Assert.That(sceneSettingPanelImage.sprite, Is.Not.Null);
        string settingPanelSpritePath = AssetDatabase.GetAssetPath(sceneSettingPanelImage.sprite);
        Assert.That(AssetDatabase.AssetPathToGUID(settingPanelSpritePath), Is.EqualTo("b84dbbbff86104a44ac5dd5557b37669"));

        TextMeshProUGUI settingLabel = settingTransform.GetComponentInChildren<TextMeshProUGUI>(true);
        Assert.That(settingLabel, Is.Not.Null);
        Assert.That(settingLabel.text, Is.EqualTo(string.Empty));
        Assert.That(settingLabel.color, Is.EqualTo(Color.white));
        Assert.That(settingLabel.raycastTarget, Is.False);

        try
        {
            Time.timeScale = 1f;
            settingButton.onClick.Invoke();

            SettingPanel settingPanel = Object.FindFirstObjectByType<SettingPanel>(FindObjectsInactive.Include);
            Assert.That(settingPanel, Is.SameAs(sceneSettingPanel));
            Assert.That(sceneSettingPanel.gameObject.activeSelf, Is.True);
            Assert.That(Time.timeScale, Is.EqualTo(0f));
        }
        finally
        {
            Time.timeScale = 1f;
        }
    }

    [Test]
    public void GameSceneCreditButtonOpensCreditPanelAndReturnsToSettings()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity");

        SettingPanel settingPanel = Object.FindFirstObjectByType<SettingPanel>(FindObjectsInactive.Include);
        CreditPanel creditPanel = Object.FindFirstObjectByType<CreditPanel>(FindObjectsInactive.Include);
        Assert.That(settingPanel, Is.Not.Null);
        Assert.That(creditPanel, Is.Not.Null);
        Assert.That(GetPrivateField<CreditPanel>(settingPanel, "creditPanel"), Is.SameAs(creditPanel));

        Button skipButton = GetPrivateField<Button>(creditPanel, "skipButton");
        Assert.That(skipButton, Is.Not.Null);

        settingPanel.gameObject.SetActive(true);
        creditPanel.gameObject.SetActive(false);

        InvokePrivate(settingPanel, "OnCreditClicked");

        Assert.That(settingPanel.gameObject.activeSelf, Is.False);
        Assert.That(creditPanel.gameObject.activeSelf, Is.True);

        skipButton.onClick.Invoke();

        Assert.That(creditPanel.gameObject.activeSelf, Is.False);
        Assert.That(settingPanel.gameObject.activeSelf, Is.True);
    }

    [Test]
    public void GameSettingsButtonOpensSettingPanelAndPausesTime()
    {
        Type gameSettingsButtonType = Type.GetType("GameSettingsButton, Assembly-CSharp");
        Assert.That(gameSettingsButtonType, Is.Not.Null);

        GameObject buttonObject = new GameObject("SettingButton");
        GameObject panelObject = new GameObject("SettingPanel");

        try
        {
            Button button = buttonObject.AddComponent<Button>();
            Component gameSettingsButton = buttonObject.AddComponent(gameSettingsButtonType);
            SettingPanel settingPanel = panelObject.AddComponent<SettingPanel>();
            panelObject.SetActive(false);

            SetPrivateField(gameSettingsButton, "settingPanel", settingPanel);

            Time.timeScale = 1f;
            button.onClick.Invoke();

            Assert.That(panelObject.activeSelf, Is.True);
            Assert.That(Time.timeScale, Is.EqualTo(0f));
        }
        finally
        {
            Time.timeScale = 1f;
            Object.DestroyImmediate(panelObject);
            Object.DestroyImmediate(buttonObject);
        }
    }

    [Test]
    public void SettingPanelPreservesPrefabAuthoredLayoutWhenEnabled()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Prefabs/SettingPanel.prefab");
        Assert.That(prefab, Is.Not.Null);

        RectTransform prefabRect = prefab.transform as RectTransform;
        Assert.That(prefabRect, Is.Not.Null);
        Vector2 prefabSize = prefabRect.sizeDelta;

        TextMeshProUGUI prefabBackLabel = prefab.transform
            .Find("BackButton/Text (TMP)")
            ?.GetComponent<TextMeshProUGUI>();
        Assert.That(prefabBackLabel, Is.Not.Null);
        string prefabBackText = prefabBackLabel.text;

        GameObject instance = null;

        try
        {
            instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Assert.That(instance, Is.Not.Null);
            instance.SetActive(true);

            RectTransform instanceRect = instance.transform as RectTransform;
            Assert.That(instanceRect, Is.Not.Null);
            Assert.That(instanceRect.sizeDelta, Is.EqualTo(prefabSize));

            TextMeshProUGUI instanceBackLabel = instance.transform
                .Find("BackButton/Text (TMP)")
                ?.GetComponent<TextMeshProUGUI>();
            Assert.That(instanceBackLabel, Is.Not.Null);
            Assert.That(instanceBackLabel.text, Is.EqualTo(prefabBackText));
        }
        finally
        {
            if (instance != null)
                Object.DestroyImmediate(instance);
        }
    }

    [Test]
    public void SettingPanelPrefabKeepsBackgroundSpriteAspectRatio()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Prefabs/SettingPanel.prefab");
        Assert.That(prefab, Is.Not.Null);

        RectTransform prefabRect = prefab.transform as RectTransform;
        Assert.That(prefabRect, Is.Not.Null);

        Image panelImage = prefab.GetComponent<Image>();
        Assert.That(panelImage, Is.Not.Null);
        Assert.That(panelImage.sprite, Is.Not.Null);
        Assert.That(panelImage.preserveAspect, Is.True);

        float prefabAspect = prefabRect.sizeDelta.x / prefabRect.sizeDelta.y;
        float spriteAspect = panelImage.sprite.rect.width / panelImage.sprite.rect.height;
        Assert.That(prefabAspect, Is.EqualTo(spriteAspect).Within(0.001f));
    }

    [Test]
    public void SettingPanelPrefabUsesAuthoredHorizontalScrollLayout()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Prefabs/SettingPanel.prefab");
        Assert.That(prefab, Is.Not.Null);

        AssertSettingPanelRect(prefab.transform, "MasterGroup", new Vector2(-170f, 15f), new Vector2(130f, 300f), 0f);
        AssertSettingPanelRect(prefab.transform, "BGMGroup", new Vector2(0f, 15f), new Vector2(130f, 300f), 0f);
        AssertSettingPanelRect(prefab.transform, "SFXGroup", new Vector2(170f, 15f), new Vector2(130f, 300f), 0f);

        AssertSettingPanelSlider(prefab.transform, "MasterGroup/MasterSlider");
        AssertSettingPanelSlider(prefab.transform, "BGMGroup/BGMSlider");
        AssertSettingPanelSlider(prefab.transform, "SFXGroup/SFXSlider");

        AssertSettingPanelRect(prefab.transform, "MasterGroup/MasterLabel", new Vector2(45f, 55f), new Vector2(120f, 28f), 90f);
        AssertSettingPanelRect(prefab.transform, "BGMGroup/BGMLabel", new Vector2(45f, 55f), new Vector2(120f, 28f), 90f);
        AssertSettingPanelRect(prefab.transform, "SFXGroup/SFXLabel", new Vector2(45f, 55f), new Vector2(120f, 28f), 90f);
        AssertSettingPanelText(prefab.transform, "MasterGroup/MasterLabel", "Master", 26f);
        AssertSettingPanelText(prefab.transform, "BGMGroup/BGMLabel", "BGM", 26f);
        AssertSettingPanelText(prefab.transform, "SFXGroup/SFXLabel", "SFX", 26f);

        AssertSettingPanelRect(prefab.transform, "MasterGroup/MasterValueText", new Vector2(0f, -100f), new Vector2(82f, 34f), 0f);
        AssertSettingPanelRect(prefab.transform, "BGMGroup/BGMValueText", new Vector2(0f, -100f), new Vector2(82f, 34f), 0f);
        AssertSettingPanelRect(prefab.transform, "SFXGroup/SFXValueText", new Vector2(0f, -100f), new Vector2(82f, 34f), 0f);
        AssertSettingPanelText(prefab.transform, "MasterGroup/MasterValueText", "100%", 28f);
        AssertSettingPanelText(prefab.transform, "BGMGroup/BGMValueText", "100%", 28f);
        AssertSettingPanelText(prefab.transform, "SFXGroup/SFXValueText", "100%", 28f);

        AssertSettingPanelRect(prefab.transform, "BackButton", new Vector2(0f, -160f), new Vector2(180f, 52f), 0f);
        AssertSettingPanelRect(prefab.transform, "BackButton/Text (TMP)", new Vector2(0f, 2f), new Vector2(160f, 40f), 0f);
        AssertSettingPanelText(prefab.transform, "BackButton/Text (TMP)", "닫기", 32f);

        AssertSettingPanelRect(prefab.transform, "CreditButton", new Vector2(-285f, -145f), new Vector2(48f, 78f), 0f);
        AssertSettingPanelText(prefab.transform, "CreditButton/Text (TMP)", string.Empty, 1f);

        AssertSettingPanelRect(prefab.transform, "TitleText", new Vector2(250f, 70f), new Vector2(80f, 130f), 0f);
        AssertSettingPanelText(prefab.transform, "TitleText", "설\n정", 42f);

        Image backButtonImage = prefab.transform.Find("BackButton")?.GetComponent<Image>();
        Assert.That(backButtonImage, Is.Not.Null);
        Assert.That(backButtonImage.preserveAspect, Is.True);

        Image creditButtonImage = prefab.transform.Find("CreditButton")?.GetComponent<Image>();
        Assert.That(creditButtonImage, Is.Not.Null);
        Assert.That(creditButtonImage.preserveAspect, Is.True);

        SettingPanel settingPanel = prefab.GetComponent<SettingPanel>();
        Assert.That(settingPanel, Is.Not.Null);
        Assert.That(GetPrivateField<Button>(settingPanel, "creditButton"), Is.EqualTo(creditButtonImage.GetComponent<Button>()));
    }

    [Test]
    public void SettingPanelBackButtonResumesPausedGame()
    {
        GameObject panelObject = new GameObject("SettingPanel");

        try
        {
            SettingPanel settingPanel = panelObject.AddComponent<SettingPanel>();
            panelObject.SetActive(false);

            MethodInfo showForGameplayPause = typeof(SettingPanel).GetMethod(
                "ShowForGameplayPause",
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(showForGameplayPause, Is.Not.Null);

            Time.timeScale = 0f;
            showForGameplayPause.Invoke(settingPanel, new object[] { 1f });
            InvokePrivate(settingPanel, "OnBackClicked");

            Assert.That(panelObject.activeSelf, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
        }
        finally
        {
            Time.timeScale = 1f;
            Object.DestroyImmediate(panelObject);
        }
    }

    [Test]
    public void SettingPanelBackButtonDoesNotOverwriteTimeScaleChangedWhileOpen()
    {
        GameObject panelObject = new GameObject("SettingPanel");

        try
        {
            SettingPanel settingPanel = panelObject.AddComponent<SettingPanel>();
            panelObject.SetActive(false);

            MethodInfo showForGameplayPause = typeof(SettingPanel).GetMethod(
                "ShowForGameplayPause",
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(showForGameplayPause, Is.Not.Null);

            Time.timeScale = 0f;
            showForGameplayPause.Invoke(settingPanel, new object[] { 0.05f });
            Time.timeScale = 1f;
            InvokePrivate(settingPanel, "OnBackClicked");

            Assert.That(panelObject.activeSelf, Is.False);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
        }
        finally
        {
            Time.timeScale = 1f;
            Object.DestroyImmediate(panelObject);
        }
    }

    [Test]
    public void GameSettingsButtonCalculatesResumeScaleFromGameState()
    {
        Assert.That(
            GameSettingsButton.CalculateResumeTimeScale(GameStateMachine.State.Playing, 0.05f),
            Is.EqualTo(1f));
        Assert.That(
            GameSettingsButton.CalculateResumeTimeScale(GameStateMachine.State.Playing, 0f),
            Is.EqualTo(0f));
        Assert.That(
            GameSettingsButton.CalculateResumeTimeScale(GameStateMachine.State.Enhancement, 0.05f),
            Is.EqualTo(0.05f));
        Assert.That(
            GameSettingsButton.CalculateResumeTimeScale(GameStateMachine.State.GameOver, 0.05f),
            Is.EqualTo(0f));
    }

    [Test]
    public void EnhancementTimeScaleCurveStartsAtInitialScale()
    {
        float scale = GameManager.CalculateEnhancementTimeScale(0f, 0.2f, 2f, 0.001f);

        Assert.That(scale, Is.EqualTo(0.2f).Within(0.0001f));
    }

    [Test]
    public void EnhancementTimeScaleCurveSlowsDownOverElapsedTime()
    {
        float earlyScale = GameManager.CalculateEnhancementTimeScale(1f, 0.2f, 2f, 0.001f);
        float laterScale = GameManager.CalculateEnhancementTimeScale(10f, 0.2f, 2f, 0.001f);

        Assert.That(earlyScale, Is.EqualTo(0.2f / 3f).Within(0.0001f));
        Assert.That(earlyScale, Is.LessThan(0.2f));
        Assert.That(laterScale, Is.LessThan(earlyScale));
        Assert.That(laterScale, Is.GreaterThan(0.001f));
    }

    [Test]
    public void EnhancementTimeScaleCurveClampsToMinimumScale()
    {
        float scale = GameManager.CalculateEnhancementTimeScale(1000000f, 0.2f, 2f, 0.05f);

        Assert.That(scale, Is.EqualTo(0.05f).Within(0.0001f));
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"{target.GetType().Name}.{fieldName} is missing.");
        field.SetValue(target, value);
    }

    private static T GetPrivateField<T>(object target, string fieldName)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(field, Is.Not.Null, $"{target.GetType().Name}.{fieldName} is missing.");
        return (T)field.GetValue(target);
    }

    private static void InvokePrivate(object target, string methodName, params object[] parameters)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null, $"{target.GetType().Name}.{methodName} is missing.");
        method.Invoke(target, parameters);
    }

    private static void AssertSettingPanelRect(
        Transform root,
        string path,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        float zRotation)
    {
        Transform child = root.Find(path);
        Assert.That(child, Is.Not.Null, $"{path} is missing.");

        RectTransform rectTransform = child as RectTransform;
        Assert.That(rectTransform, Is.Not.Null, $"{path} is not a RectTransform.");
        Assert.That(rectTransform.anchoredPosition.x, Is.EqualTo(anchoredPosition.x).Within(0.01f), path);
        Assert.That(rectTransform.anchoredPosition.y, Is.EqualTo(anchoredPosition.y).Within(0.01f), path);
        Assert.That(rectTransform.sizeDelta.x, Is.EqualTo(sizeDelta.x).Within(0.01f), path);
        Assert.That(rectTransform.sizeDelta.y, Is.EqualTo(sizeDelta.y).Within(0.01f), path);
        Assert.That(Mathf.Round(rectTransform.localEulerAngles.z), Is.EqualTo(zRotation).Within(0.01f), path);
    }

    private static void AssertSettingPanelText(Transform root, string path, string textValue, float fontSize)
    {
        Transform child = root.Find(path);
        Assert.That(child, Is.Not.Null, $"{path} is missing.");

        TextMeshProUGUI text = child.GetComponent<TextMeshProUGUI>();
        Assert.That(text, Is.Not.Null, $"{path} is not TextMeshProUGUI.");
        Assert.That(text.text, Is.EqualTo(textValue), path);
        Assert.That(text.fontSize, Is.EqualTo(fontSize).Within(0.01f), path);
    }

    private static void AssertSettingPanelSlider(Transform root, string path)
    {
        AssertSettingPanelRect(root, path, new Vector2(0f, 35f), new Vector2(42f, 220f), 0f);

        Slider slider = root.Find(path)?.GetComponent<Slider>();
        Assert.That(slider, Is.Not.Null, $"{path} is missing a Slider.");
        Assert.That(slider.direction, Is.EqualTo(Slider.Direction.BottomToTop), path);

        Image backgroundImage = root.Find($"{path}/Background")?.GetComponent<Image>();
        Assert.That(backgroundImage, Is.Not.Null, $"{path}/Background is missing.");
        Assert.That(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(backgroundImage.sprite)), Is.EqualTo("e66f6f6bf7ab94c4cadfdfc8cd25ead3"));
        Assert.That(backgroundImage.preserveAspect, Is.True);
    }

}
#endif
