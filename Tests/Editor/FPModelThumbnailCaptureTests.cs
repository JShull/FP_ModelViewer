namespace FuzzPhyte.ModelViewer.Tests
{
    using FuzzPhyte.Placement.OrbitalCamera;
    using NUnit.Framework;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering;

    public sealed class FPModelThumbnailCaptureTests
    {
        [Test]
        public void CaptureProfile_DefaultsToThreePointLightingAtCaptureOrigin()
        {
            var profile = ScriptableObject.CreateInstance<FP_ModelCaptureProfile>();

            try
            {
                Assert.That(
                    profile.LightingMode,
                    Is.EqualTo(FP_ModelCaptureLightingMode.ThreePointRig));
                Assert.That(
                    profile.CaptureSpace,
                    Is.EqualTo(FP_ModelCaptureSpace.IsolatedAtOrigin));
                Assert.That(
                    profile.FrontLightAppearance,
                    Is.EqualTo(FP_ModelCaptureLightAppearance.Color));
            }
            finally
            {
                Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void ConfigureLightAppearance_MatchesUnityColorTemperatureControls()
        {
            var lightObject = new GameObject("Configured Light");

            try
            {
                Light light = lightObject.AddComponent<Light>();
                Color filter = new Color(0.7f, 0.8f, 0.9f, 1f);

                FP_ModelThumbnailCaptureUtility.ConfigureLightAppearance(
                    light,
                    FP_ModelCaptureLightAppearance.FilterAndTemperature,
                    filter,
                    2700f);

                Assert.That(light.color, Is.EqualTo(filter));
                Assert.That(light.useColorTemperature, Is.True);
                Assert.That(light.colorTemperature, Is.EqualTo(2700f).Within(0.0001f));

                FP_ModelThumbnailCaptureUtility.ConfigureLightAppearance(
                    light,
                    FP_ModelCaptureLightAppearance.Color,
                    Color.red,
                    500f);

                Assert.That(light.color, Is.EqualTo(Color.red));
                Assert.That(light.useColorTemperature, Is.False);
                Assert.That(
                    light.colorTemperature,
                    Is.EqualTo(FP_ModelCaptureProfile.MinimumColorTemperature)
                        .Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(lightObject);
            }
        }

        [Test]
        public void TryCapture_ThreePointRigRestoresSceneLightingState()
        {
            GameObject model = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var cameraObject = new GameObject("Capture Camera");
            var sceneLightObject = new GameObject("Scene Light");
            var displayData = ScriptableObject.CreateInstance<FP_ModelDisplayData>();
            var profile = ScriptableObject.CreateInstance<FP_ModelCaptureProfile>();
            Texture2D thumbnail = null;

            AmbientMode previousAmbientMode = RenderSettings.ambientMode;
            Color previousAmbientLight = RenderSettings.ambientLight;
            float previousAmbientIntensity = RenderSettings.ambientIntensity;
            bool previousLightsUseLinearIntensity = GraphicsSettings.lightsUseLinearIntensity;
            bool previousLightsUseColorTemperature = GraphicsSettings.lightsUseColorTemperature;

            try
            {
                displayData.UseLocalBoundsOverride = true;
                displayData.BoundsCenter = Vector3.zero;
                displayData.BoundsSize = Vector3.one;

                FP_ModelDisplayBinding binding = model.AddComponent<FP_ModelDisplayBinding>();
                var serializedBinding = new SerializedObject(binding);
                serializedBinding.FindProperty("_data").objectReferenceValue = displayData;
                serializedBinding.ApplyModifiedPropertiesWithoutUndo();

                var serializedProfile = new SerializedObject(profile);
                serializedProfile.FindProperty("_frontLightAppearance").enumValueIndex =
                    (int)FP_ModelCaptureLightAppearance.FilterAndTemperature;
                serializedProfile.ApplyModifiedPropertiesWithoutUndo();

                Camera camera = cameraObject.AddComponent<Camera>();
                camera.enabled = false;
                Light sceneLight = sceneLightObject.AddComponent<Light>();
                sceneLight.type = LightType.Directional;
                sceneLight.cullingMask = (1 << 0) | (1 << 8);

                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.15f, 0.25f, 0.35f, 1f);
                RenderSettings.ambientIntensity = 0.75f;
                int expectedLightMask = sceneLight.cullingMask;
                Color expectedAmbientLight = RenderSettings.ambientLight;
                float expectedAmbientIntensity = RenderSettings.ambientIntensity;
                bool expectedLightsUseLinearIntensity =
                    GraphicsSettings.lightsUseLinearIntensity;
                bool expectedLightsUseColorTemperature =
                    GraphicsSettings.lightsUseColorTemperature;

                bool result = FP_ModelThumbnailCaptureUtility.TryCapture(
                    camera,
                    binding,
                    profile,
                    FP_ViewCubeHit.Front,
                    out thumbnail,
                    binding.transform,
                    1 << 0);

                Assert.That(result, Is.True);
                Assert.That(thumbnail, Is.Not.Null);
                Assert.That(sceneLight.cullingMask, Is.EqualTo(expectedLightMask));
                Assert.That(RenderSettings.ambientMode, Is.EqualTo(AmbientMode.Flat));
                Assert.That(RenderSettings.ambientLight, Is.EqualTo(expectedAmbientLight));
                Assert.That(
                    RenderSettings.ambientIntensity,
                    Is.EqualTo(expectedAmbientIntensity).Within(0.0001f));
                Assert.That(
                    GraphicsSettings.lightsUseLinearIntensity,
                    Is.EqualTo(expectedLightsUseLinearIntensity));
                Assert.That(
                    GraphicsSettings.lightsUseColorTemperature,
                    Is.EqualTo(expectedLightsUseColorTemperature));
            }
            finally
            {
                RenderSettings.ambientMode = previousAmbientMode;
                RenderSettings.ambientLight = previousAmbientLight;
                RenderSettings.ambientIntensity = previousAmbientIntensity;
                GraphicsSettings.lightsUseLinearIntensity =
                    previousLightsUseLinearIntensity;
                GraphicsSettings.lightsUseColorTemperature =
                    previousLightsUseColorTemperature;
                if (thumbnail != null)
                {
                    Object.DestroyImmediate(thumbnail);
                }
                Object.DestroyImmediate(model);
                Object.DestroyImmediate(cameraObject);
                Object.DestroyImmediate(sceneLightObject);
                Object.DestroyImmediate(displayData);
                Object.DestroyImmediate(profile);
            }
        }
    }
}
