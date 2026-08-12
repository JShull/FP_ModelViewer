namespace FuzzPhyte.ModelViewer.Editor
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(FP_ModelCaptureProfile))]
    public sealed class FP_ModelCaptureProfileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField(
                    "Script",
                    MonoScript.FromScriptableObject((FP_ModelCaptureProfile)target),
                    typeof(MonoScript),
                    false);
            }

            DrawProperty("UniqueID", "Unique ID");

            DrawHeader("Output");
            DrawProperty("_width", "Width");
            DrawProperty("_height", "Height");

            DrawHeader("Camera");
            DrawProperty("_projection", "Projection");
            DrawProperty("_fieldOfView", "Field Of View");
            DrawProperty("_boundsPadding", "Bounds Padding");
            DrawProperty("_captureLayers", "Capture Layers");
            DrawProperty("_captureSpace", "Capture Space");

            DrawHeader("Lighting");
            SerializedProperty lightingMode = DrawProperty("_lightingMode", "Lighting Mode");
            if (!lightingMode.hasMultipleDifferentValues &&
                lightingMode.enumValueIndex ==
                    (int)FP_ModelCaptureLightingMode.ThreePointRig)
            {
                DrawRigLight(
                    "Front Key Light",
                    "_frontLightAppearance",
                    "_frontLightColor",
                    "_frontLightTemperature",
                    "_frontLightIntensity",
                    "_frontLightSpotAngle");
                DrawRigLight(
                    "Back Rim Light",
                    "_backLightAppearance",
                    "_backLightColor",
                    "_backLightTemperature",
                    "_backLightIntensity",
                    "_backLightSpotAngle");
                DrawRigLight(
                    "Directional Fill Light",
                    "_directionalLightAppearance",
                    "_directionalLightColor",
                    "_directionalLightTemperature",
                    "_directionalLightIntensity",
                    null);

                EditorGUILayout.Space(2f);
                DrawProperty("_ambientColor", "Ambient Color");
                DrawProperty("_lightDistanceMultiplier", "Light Distance Multiplier");
                DrawProperty("_lightShadows", "Light Shadows");
            }

            DrawHeader("Background");
            DrawProperty("_backgroundMode", "Background Mode");
            DrawProperty("_backgroundColor", "Background Color");

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawRigLight(
            string label,
            string appearanceProperty,
            string colorProperty,
            string temperatureProperty,
            string intensityProperty,
            string spotAngleProperty)
        {
            EditorGUILayout.Space(5f);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            SerializedProperty appearance = DrawProperty(appearanceProperty, "Appearance");
            bool showTemperature = !appearance.hasMultipleDifferentValues &&
                appearance.enumValueIndex ==
                    (int)FP_ModelCaptureLightAppearance.FilterAndTemperature;
            DrawProperty(colorProperty, showTemperature ? "Filter" : "Color");
            if (showTemperature)
            {
                DrawTemperatureProperty(serializedObject.FindProperty(temperatureProperty));
            }
            DrawProperty(intensityProperty, "Intensity");
            if (!string.IsNullOrEmpty(spotAngleProperty))
            {
                DrawProperty(spotAngleProperty, "Spot Angle");
            }

            EditorGUI.indentLevel--;
        }

        private static void DrawTemperatureProperty(SerializedProperty temperature)
        {
            Rect gradientRow = EditorGUILayout.GetControlRect();
            Rect gradientRect = EditorGUI.PrefixLabel(
                gradientRow,
                new GUIContent(
                    "Temperature",
                    "Correlated color temperature in Kelvin."));
            const float kelvinLabelWidth = 42f;
            Rect kelvinRect = new Rect(
                gradientRect.xMax - kelvinLabelWidth,
                gradientRect.y,
                kelvinLabelWidth,
                gradientRect.height);
            gradientRect.xMax = kelvinRect.xMin - 4f;

            DrawTemperatureGradient(gradientRect, temperature.floatValue);
            EditorGUI.LabelField(kelvinRect, "Kelvin");
            HandleTemperatureGradientInput(gradientRect, temperature);

            Rect valueRow = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
            Rect valueRect = new Rect(
                valueRow.x + EditorGUIUtility.labelWidth,
                valueRow.y,
                Mathf.Max(1f, valueRow.width - EditorGUIUtility.labelWidth -
                    kelvinLabelWidth - 4f),
                valueRow.height);
            EditorGUI.BeginChangeCheck();
            float value = EditorGUI.FloatField(valueRect, temperature.floatValue);
            if (EditorGUI.EndChangeCheck())
            {
                temperature.floatValue = Mathf.Clamp(
                    value,
                    FP_ModelCaptureProfile.MinimumColorTemperature,
                    FP_ModelCaptureProfile.MaximumColorTemperature);
            }
        }

        private static void DrawTemperatureGradient(Rect rect, float temperature)
        {
            const int segmentCount = 64;
            EditorGUI.DrawRect(rect, new Color(0.08f, 0.08f, 0.08f, 1f));
            Rect inner = new Rect(rect.x + 1f, rect.y + 1f, rect.width - 2f, rect.height - 2f);
            for (int i = 0; i < segmentCount; i++)
            {
                float start = (float)i / segmentCount;
                float end = (float)(i + 1) / segmentCount;
                float kelvin = Mathf.Lerp(
                    FP_ModelCaptureProfile.MinimumColorTemperature,
                    FP_ModelCaptureProfile.MaximumColorTemperature,
                    (start + end) * 0.5f);
                Rect segment = new Rect(
                    inner.x + inner.width * start,
                    inner.y,
                    inner.width * (end - start) + 1f,
                    inner.height);
                EditorGUI.DrawRect(
                    segment,
                    Mathf.CorrelatedColorTemperatureToRGB(kelvin));
            }

            float normalized = Mathf.InverseLerp(
                FP_ModelCaptureProfile.MinimumColorTemperature,
                FP_ModelCaptureProfile.MaximumColorTemperature,
                temperature);
            float markerX = Mathf.Lerp(inner.x, inner.xMax, normalized);
            EditorGUI.DrawRect(
                new Rect(markerX - 2f, rect.y, 4f, rect.height),
                Color.black);
            EditorGUI.DrawRect(
                new Rect(markerX - 1f, rect.y + 1f, 2f, rect.height - 2f),
                Color.white);
        }

        private static void HandleTemperatureGradientInput(
            Rect rect,
            SerializedProperty temperature)
        {
            int controlId = GUIUtility.GetControlID(FocusType.Passive, rect);
            Event current = Event.current;
            bool beginsDrag = current.type == EventType.MouseDown &&
                current.button == 0 && rect.Contains(current.mousePosition);
            bool continuesDrag = current.type == EventType.MouseDrag &&
                GUIUtility.hotControl == controlId;
            if (beginsDrag)
            {
                GUIUtility.hotControl = controlId;
            }

            if (beginsDrag || continuesDrag)
            {
                float normalized = Mathf.Clamp01(
                    (current.mousePosition.x - rect.x) / Mathf.Max(1f, rect.width));
                temperature.floatValue = Mathf.Lerp(
                    FP_ModelCaptureProfile.MinimumColorTemperature,
                    FP_ModelCaptureProfile.MaximumColorTemperature,
                    normalized);
                GUI.changed = true;
                current.Use();
            }
            else if (current.type == EventType.MouseUp &&
                GUIUtility.hotControl == controlId)
            {
                GUIUtility.hotControl = 0;
                current.Use();
            }
        }

        private SerializedProperty DrawProperty(string propertyName, string label)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            EditorGUILayout.PropertyField(property, new GUIContent(label), true);
            return property;
        }

        private static void DrawHeader(string label)
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        }
    }
}
