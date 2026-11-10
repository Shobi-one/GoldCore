using UnityEditor;
using UnityEngine;

namespace GoldCore.EditorTools
{
    /// <summary>
    /// Adds Preview and Stop buttons to an effect preset. The preview plays at the Scene view's
    /// center, in edit mode too, and rebuilds as you change values so you can tune it live.
    /// </summary>
    [CustomEditor(typeof(EffectPreset))]
    public class EffectPresetEditor : Editor
    {
        static EffectInstance preview;
        static EffectPreset previewPreset;
        static int previewVersion;
        static double lastTime;
        static bool repeat = true;

        public override void OnInspectorGUI()
        {
            var preset = (EffectPreset)target;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Preview", GUILayout.Height(24))) StartPreview(preset);
            GUI.enabled = preview != null;
            if (GUILayout.Button("Stop", GUILayout.Height(24))) StopPreview();
            GUI.enabled = true;
            repeat = GUILayout.Toggle(repeat, "Repeat", GUILayout.Width(64));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox(
                $"Lasts {preset.Duration:0.00} s. Shapes are drawn by the shader, so no textures are needed. " +
                "Duplicate a preset (Ctrl+D) to make a variant.", MessageType.None);

            DrawDefaultInspector();
        }

        void OnDisable()
        {
            // Keep looping previews alive while another object is selected; clear one-shots.
            if (preview != null && previewPreset != null && !previewPreset.loop) StopPreview();
        }

        static void StartPreview(EffectPreset preset)
        {
            if (Application.isPlaying)
            {
                Vector3 at = SceneView.lastActiveSceneView != null ? SceneView.lastActiveSceneView.pivot : Vector3.zero;
                Effects.Play(preset, at, Quaternion.LookRotation(Vector3.up));
                return;
            }

            StopPreview();
            var go = new GameObject("Effect Preview") { hideFlags = HideFlags.HideAndDontSave };
            Vector3 pivot = SceneView.lastActiveSceneView != null ? SceneView.lastActiveSceneView.pivot : Vector3.zero;
            go.transform.SetPositionAndRotation(pivot, Quaternion.LookRotation(Vector3.up));
            preview = go.AddComponent<EffectInstance>();
            preview.Build(preset, 1f);
            preview.Play();
            previewPreset = preset;
            previewVersion = preset.Version;
            lastTime = EditorApplication.timeSinceStartup;
            EditorApplication.update -= Step;
            EditorApplication.update += Step;
        }

        static void StopPreview()
        {
            EditorApplication.update -= Step;
            if (preview != null) Object.DestroyImmediate(preview.gameObject);
            preview = null;
            previewPreset = null;
        }

        static void Step()
        {
            if (preview == null || previewPreset == null || Application.isPlaying)
            {
                StopPreview();
                return;
            }

            if (previewPreset.Version != previewVersion)
            {
                // Values changed in the inspector: rebuild and replay.
                previewVersion = previewPreset.Version;
                preview.Build(previewPreset, 1f);
                preview.Play();
            }

            double now = EditorApplication.timeSinceStartup;
            float dt = Mathf.Min((float)(now - lastTime), 0.1f);
            lastTime = now;
            preview.Simulate(dt);

            if (!preview.IsPlaying)
            {
                if (repeat) preview.Play();
                else StopPreview();
            }
            SceneView.RepaintAll();
        }
    }

    [CustomPropertyDrawer(typeof(FloatRange))]
    public class FloatRangeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            Rect field = EditorGUI.PrefixLabel(position, label);
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            float half = (field.width - 4f) / 2f;
            float oldLabel = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 28f;
            EditorGUI.PropertyField(new Rect(field.x, field.y, half, field.height), property.FindPropertyRelative("min"), new GUIContent("Min"));
            EditorGUI.PropertyField(new Rect(field.x + half + 4f, field.y, half, field.height), property.FindPropertyRelative("max"), new GUIContent("Max"));
            EditorGUIUtility.labelWidth = oldLabel;
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }

    static class EffectMenus
    {
        [MenuItem("GameObject/GoldCore/Effect Player", false, 10)]
        static void CreateEffectPlayer(MenuCommand command)
        {
            var go = new GameObject("Effect");
            go.AddComponent<EffectPlayer>();
            GameObjectUtility.SetParentAndAlign(go, command.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create Effect Player");
            Selection.activeObject = go;
        }
    }
}
