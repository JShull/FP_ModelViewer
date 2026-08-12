namespace FuzzPhyte.ModelViewer
{
    using System;
    using System.Collections.Generic;
    using FuzzPhyte.Placement.OrbitalCamera;
    using UnityEngine;
    using UnityEngine.Rendering;

    /// <summary>
    /// Captures one model view with a caller-owned camera. The caller controls scene isolation
    /// through the capture profile layer mask or by placing a temporary model copy on that layer.
    /// </summary>
    public static class FP_ModelThumbnailCaptureUtility
    {
        public static void ConfigureLightAppearance(
            Light light,
            FP_ModelCaptureLightAppearance appearance,
            Color colorFilter,
            float colorTemperature)
        {
            if (light == null)
            {
                return;
            }

            light.color = colorFilter;
            light.useColorTemperature =
                appearance == FP_ModelCaptureLightAppearance.FilterAndTemperature;
            light.colorTemperature = Mathf.Clamp(
                colorTemperature,
                FP_ModelCaptureProfile.MinimumColorTemperature,
                FP_ModelCaptureProfile.MaximumColorTemperature);
        }

        public static bool TryCapture(
            Camera camera,
            FP_ModelDisplayBinding binding,
            FP_ModelCaptureProfile profile,
            FP_ViewCubeHit view,
            out Texture2D thumbnail,
            Transform orientationFrame = null,
            int? cullingMaskOverride = null)
        {
            thumbnail = null;
            if (camera == null || binding == null || profile == null ||
                !FP_ModelViewerViewUtility.IsSupportedThumbnailView(view))
            {
                return false;
            }

            Bounds bounds = binding.GetWorldBounds();
            if (bounds.size.sqrMagnitude <= Mathf.Epsilon)
            {
                return false;
            }

            RenderTexture target = RenderTexture.GetTemporary(
                profile.Width,
                profile.Height,
                24,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Default);

            RenderTexture previousActive = RenderTexture.active;
            RenderTexture previousTarget = camera.targetTexture;
            Vector3 previousPosition = camera.transform.position;
            Quaternion previousRotation = camera.transform.rotation;
            bool previousOrthographic = camera.orthographic;
            float previousOrthographicSize = camera.orthographicSize;
            float previousFieldOfView = camera.fieldOfView;
            float previousAspect = camera.aspect;
            float previousNearClip = camera.nearClipPlane;
            float previousFarClip = camera.farClipPlane;
            CameraClearFlags previousClearFlags = camera.clearFlags;
            Color previousBackgroundColor = camera.backgroundColor;
            int previousCullingMask = camera.cullingMask;
            ThreePointLightingScope lightingScope = null;

            try
            {
                ConfigureCamera(
                    camera,
                    binding,
                    bounds,
                    profile,
                    view,
                    orientationFrame,
                    cullingMaskOverride);
                if (profile.LightingMode == FP_ModelCaptureLightingMode.ThreePointRig)
                {
                    lightingScope = ThreePointLightingScope.Create(
                        camera,
                        bounds,
                        profile,
                        camera.cullingMask);
                }
                camera.targetTexture = target;
                camera.Render();

                RenderTexture.active = target;
                thumbnail = new Texture2D(
                    profile.Width,
                    profile.Height,
                    TextureFormat.RGBA32,
                    false,
                    false)
                {
                    name = $"{binding.name}_{view}"
                };
                thumbnail.ReadPixels(new Rect(0f, 0f, profile.Width, profile.Height), 0, 0);
                thumbnail.Apply(false, false);
                return true;
            }
            finally
            {
                lightingScope?.Dispose();
                camera.targetTexture = previousTarget;
                camera.transform.SetPositionAndRotation(previousPosition, previousRotation);
                camera.orthographic = previousOrthographic;
                camera.orthographicSize = previousOrthographicSize;
                camera.fieldOfView = previousFieldOfView;
                camera.aspect = previousAspect;
                camera.nearClipPlane = previousNearClip;
                camera.farClipPlane = previousFarClip;
                camera.clearFlags = previousClearFlags;
                camera.backgroundColor = previousBackgroundColor;
                camera.cullingMask = previousCullingMask;
                RenderTexture.active = previousActive;
                RenderTexture.ReleaseTemporary(target);
            }
        }

        private static void ConfigureCamera(
            Camera camera,
            FP_ModelDisplayBinding binding,
            Bounds bounds,
            FP_ModelCaptureProfile profile,
            FP_ViewCubeHit view,
            Transform orientationFrame,
            int? cullingMaskOverride)
        {
            FP_ViewPose pose = FP_ViewCubePoses.Get(view);
            pose.NormalizeDirection();

            Transform frame = orientationFrame != null ? orientationFrame : binding.transform;
            Vector3 fromDirection = frame.TransformDirection(pose.FromDirection).normalized;
            Vector3 upDirection = frame.TransformDirection(pose.UpDirection).normalized;
            Quaternion rotation = Quaternion.LookRotation(-fromDirection, upDirection);

            float radius = Mathf.Max(0.001f, bounds.extents.magnitude);
            float paddedRadius = radius * profile.BoundsPadding;
            float aspect = (float)profile.Width / profile.Height;
            bool isOrthographic = profile.Projection == FP_ProjectionMode.Orthographic;

            camera.aspect = aspect;
            camera.orthographic = isOrthographic;
            camera.fieldOfView = profile.FieldOfView;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = profile.BackgroundColor;
            camera.cullingMask = cullingMaskOverride ?? profile.CaptureLayers.value;

            float distance;
            if (isOrthographic)
            {
                camera.orthographicSize = Mathf.Max(paddedRadius, paddedRadius / aspect);
                distance = paddedRadius * 2f;
            }
            else
            {
                float verticalHalfFov = profile.FieldOfView * 0.5f * Mathf.Deg2Rad;
                float horizontalHalfFov = Mathf.Atan(Mathf.Tan(verticalHalfFov) * aspect);
                float limitingHalfFov = Mathf.Min(verticalHalfFov, horizontalHalfFov);
                distance = paddedRadius / Mathf.Sin(Mathf.Max(0.001f, limitingHalfFov));
            }

            camera.nearClipPlane = Mathf.Max(0.01f, distance - paddedRadius * 1.5f);
            camera.farClipPlane = Mathf.Max(camera.nearClipPlane + 0.01f, distance + paddedRadius * 1.5f);
            camera.transform.SetPositionAndRotation(
                bounds.center + fromDirection * distance,
                rotation);
        }

        private sealed class ThreePointLightingScope : IDisposable
        {
            private readonly List<SceneLightState> _sceneLights =
                new List<SceneLightState>();
            private GameObject _rigRoot;
            private AmbientMode _previousAmbientMode;
            private Color _previousAmbientLight;
            private float _previousAmbientIntensity;
            private bool _ambientStateCaptured;
            private bool _previousLightsUseLinearIntensity;
            private bool _previousLightsUseColorTemperature;
            private bool _graphicsStateCaptured;

            private ThreePointLightingScope()
            {
            }

            public static ThreePointLightingScope Create(
                Camera camera,
                Bounds bounds,
                FP_ModelCaptureProfile profile,
                int captureMask)
            {
                var scope = new ThreePointLightingScope();
                try
                {
                    scope.Initialize(camera, bounds, profile, captureMask);
                    return scope;
                }
                catch
                {
                    scope.Dispose();
                    throw;
                }
            }

            public void Dispose()
            {
                for (int i = 0; i < _sceneLights.Count; i++)
                {
                    SceneLightState state = _sceneLights[i];
                    if (state.Light != null)
                    {
                        state.Light.cullingMask = state.CullingMask;
                    }
                }
                _sceneLights.Clear();

                if (_ambientStateCaptured)
                {
                    RenderSettings.ambientMode = _previousAmbientMode;
                    RenderSettings.ambientLight = _previousAmbientLight;
                    RenderSettings.ambientIntensity = _previousAmbientIntensity;
                    _ambientStateCaptured = false;
                }

                if (_graphicsStateCaptured)
                {
                    GraphicsSettings.lightsUseLinearIntensity =
                        _previousLightsUseLinearIntensity;
                    GraphicsSettings.lightsUseColorTemperature =
                        _previousLightsUseColorTemperature;
                    _graphicsStateCaptured = false;
                }

                if (_rigRoot != null)
                {
                    _rigRoot.SetActive(false);
                    if (Application.isPlaying)
                    {
                        UnityEngine.Object.Destroy(_rigRoot);
                    }
                    else
                    {
                        UnityEngine.Object.DestroyImmediate(_rigRoot);
                    }
                    _rigRoot = null;
                }
            }

            private void Initialize(
                Camera camera,
                Bounds bounds,
                FP_ModelCaptureProfile profile,
                int captureMask)
            {
                Light[] lights = UnityEngine.Object.FindObjectsByType<Light>(
                    FindObjectsInactive.Include);
                for (int i = 0; i < lights.Length; i++)
                {
                    Light light = lights[i];
                    if (light == null || !light.gameObject.scene.IsValid() ||
                        !light.gameObject.scene.isLoaded)
                    {
                        continue;
                    }

                    _sceneLights.Add(new SceneLightState(light, light.cullingMask));
                    light.cullingMask &= ~captureMask;
                }

                _previousAmbientMode = RenderSettings.ambientMode;
                _previousAmbientLight = RenderSettings.ambientLight;
                _previousAmbientIntensity = RenderSettings.ambientIntensity;
                _ambientStateCaptured = true;
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = profile.AmbientColor;
                RenderSettings.ambientIntensity = 1f;

                if (UsesColorTemperature(profile))
                {
                    _previousLightsUseLinearIntensity =
                        GraphicsSettings.lightsUseLinearIntensity;
                    _previousLightsUseColorTemperature =
                        GraphicsSettings.lightsUseColorTemperature;
                    _graphicsStateCaptured = true;
                    GraphicsSettings.lightsUseLinearIntensity = true;
                    GraphicsSettings.lightsUseColorTemperature = true;
                }

                _rigRoot = new GameObject("FP Model Viewer Three Point Rig")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };

                float radius = Mathf.Max(0.001f, bounds.extents.magnitude);
                float distance = Mathf.Max(0.5f, radius * profile.LightDistanceMultiplier);
                Vector3 center = bounds.center;

                Vector3 frontPosition = center +
                    (-camera.transform.forward - camera.transform.right * 0.65f +
                     camera.transform.up * 0.75f).normalized * distance;
                CreateSpotLight(
                    "Front Key Light",
                    frontPosition,
                    center,
                    profile.FrontLightAppearance,
                    profile.FrontLightColor,
                    profile.FrontLightTemperature,
                    profile.FrontLightIntensity,
                    profile.FrontLightSpotAngle,
                    distance,
                    profile.LightShadows,
                    captureMask);

                Vector3 backPosition = center +
                    (camera.transform.forward + camera.transform.right * 0.65f +
                     camera.transform.up * 0.55f).normalized * distance;
                CreateSpotLight(
                    "Back Rim Light",
                    backPosition,
                    center,
                    profile.BackLightAppearance,
                    profile.BackLightColor,
                    profile.BackLightTemperature,
                    profile.BackLightIntensity,
                    profile.BackLightSpotAngle,
                    distance,
                    profile.LightShadows,
                    captureMask);

                GameObject directionalObject = CreateLightObject("Directional Fill Light");
                directionalObject.transform.rotation = Quaternion.LookRotation(
                    (camera.transform.forward - camera.transform.right * 0.25f -
                     camera.transform.up * 0.5f).normalized,
                    camera.transform.up);
                Light directional = directionalObject.AddComponent<Light>();
                directional.type = LightType.Directional;
                ConfigureLightAppearance(
                    directional,
                    profile.DirectionalLightAppearance,
                    profile.DirectionalLightColor,
                    profile.DirectionalLightTemperature);
                directional.intensity = profile.DirectionalLightIntensity;
                directional.shadows = profile.LightShadows;
                directional.cullingMask = captureMask;
            }

            private void CreateSpotLight(
                string name,
                Vector3 position,
                Vector3 target,
                FP_ModelCaptureLightAppearance appearance,
                Color color,
                float colorTemperature,
                float intensity,
                float spotAngle,
                float distance,
                LightShadows shadows,
                int captureMask)
            {
                GameObject lightObject = CreateLightObject(name);
                lightObject.transform.SetPositionAndRotation(
                    position,
                    Quaternion.LookRotation((target - position).normalized));

                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Spot;
                ConfigureLightAppearance(light, appearance, color, colorTemperature);
                light.intensity = intensity;
                light.spotAngle = spotAngle;
                light.range = Mathf.Max(1f, distance * 3f);
                light.shadows = shadows;
                light.cullingMask = captureMask;
            }

            private GameObject CreateLightObject(string name)
            {
                var lightObject = new GameObject(name)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                lightObject.transform.SetParent(_rigRoot.transform, false);
                return lightObject;
            }

            private static bool UsesColorTemperature(FP_ModelCaptureProfile profile)
            {
                return profile.FrontLightAppearance ==
                        FP_ModelCaptureLightAppearance.FilterAndTemperature ||
                    profile.BackLightAppearance ==
                        FP_ModelCaptureLightAppearance.FilterAndTemperature ||
                    profile.DirectionalLightAppearance ==
                        FP_ModelCaptureLightAppearance.FilterAndTemperature;
            }

            private readonly struct SceneLightState
            {
                public SceneLightState(Light light, int cullingMask)
                {
                    Light = light;
                    CullingMask = cullingMask;
                }

                public Light Light { get; }
                public int CullingMask { get; }
            }
        }
    }
}
