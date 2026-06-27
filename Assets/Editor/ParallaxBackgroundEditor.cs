#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ParallaxBackground))]
public class ParallaxBackgroundEditor : Editor
{
    private SerializedProperty layersProperty;
    private SerializedProperty stageDurationSecondsProperty;
    private SerializedProperty fallbackWorldHeightProperty;
    private SerializedProperty rebuildOnStartProperty;
    private bool showAdvancedLayerSettings;

    private void OnEnable()
    {
        this.layersProperty = serializedObject.FindProperty("layers");
        this.stageDurationSecondsProperty = serializedObject.FindProperty("stageDurationSeconds");
        this.fallbackWorldHeightProperty = serializedObject.FindProperty("fallbackWorldHeight");
        this.rebuildOnStartProperty = serializedObject.FindProperty("rebuildOnStart");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(
            this.stageDurationSecondsProperty,
            new GUIContent("Stage Duration Seconds", "Seconds for layers 2-7 to complete their one downward movement."));

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Layer Move Amounts", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Layer 1 is the sky and should stay at 0. Larger values move farther down over the stage.", MessageType.Info);

        DrawLayerMoveAmounts();

        EditorGUILayout.Space(8f);
        DrawAdvancedSettings();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawLayerMoveAmounts()
    {
        if (this.layersProperty == null || !this.layersProperty.isArray)
        {
            EditorGUILayout.HelpBox("Layers are not configured.", MessageType.Warning);
            return;
        }

        for (int i = 0; i < this.layersProperty.arraySize; ++i)
        {
            SerializedProperty layerProperty = this.layersProperty.GetArrayElementAtIndex(i);
            SerializedProperty spriteProperty = layerProperty.FindPropertyRelative("sprite");
            SerializedProperty travelDistanceProperty = layerProperty.FindPropertyRelative("travelDistance");

            string spriteName = ResolveSpriteName(spriteProperty);
            string label = $"Layer {i + 1} ({spriteName})";
            EditorGUILayout.PropertyField(
                travelDistanceProperty,
                new GUIContent(label, "Total downward movement over Stage Duration. Larger values move faster."));
        }
    }

    private void DrawAdvancedSettings()
    {
        this.showAdvancedLayerSettings = EditorGUILayout.Foldout(this.showAdvancedLayerSettings, "Advanced Layer Settings", true);
        if (!this.showAdvancedLayerSettings)
            return;

        using (new EditorGUI.IndentLevelScope())
        {
            for (int i = 0; i < this.layersProperty.arraySize; ++i)
            {
                SerializedProperty layerProperty = this.layersProperty.GetArrayElementAtIndex(i);
                SerializedProperty spriteProperty = layerProperty.FindPropertyRelative("sprite");
                SerializedProperty sortingOrderProperty = layerProperty.FindPropertyRelative("sortingOrder");

                EditorGUILayout.LabelField($"Layer {i + 1}", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(spriteProperty);
                EditorGUILayout.PropertyField(sortingOrderProperty);
                EditorGUILayout.Space(4f);
            }

            EditorGUILayout.PropertyField(this.fallbackWorldHeightProperty);
            EditorGUILayout.PropertyField(this.rebuildOnStartProperty);
        }
    }

    private static string ResolveSpriteName(SerializedProperty spriteProperty)
    {
        if (spriteProperty?.objectReferenceValue == null)
            return "No Sprite";

        return spriteProperty.objectReferenceValue.name;
    }
}
#endif
