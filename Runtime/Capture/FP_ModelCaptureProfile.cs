namespace FuzzPhyte.ModelViewer
{
    using FuzzPhyte.Placement.OrbitalCamera;
    using FuzzPhyte.Utility;
    using UnityEngine;

    public enum FP_ModelCaptureBackgroundMode
    {
        SolidColor = 0,
        Transparent = 1
    }

    public enum FP_ModelCaptureLightingMode
    {
        ThreePointRig = 0,
        SceneLighting = 1
    }

    public enum FP_ModelCaptureSpace
    {
        IsolatedAtOrigin = 0,
        PreserveScenePosition = 1
    }

    public enum FP_ModelCaptureLightAppearance
    {
        Color = 0,
        FilterAndTemperature = 1
    }

    [CreateAssetMenu(
        fileName = "FP_ModelCaptureProfile",
        menuName = "FuzzPhyte/Model Viewer/Capture Profile")]
    public sealed class FP_ModelCaptureProfile : FP_Data
    {
        public const float MinimumColorTemperature = 1000f;
        public const float MaximumColorTemperature = 20000f;

        [Header("Output")]
        [SerializeField, Min(16)] private int _width = 512;
        [SerializeField, Min(16)] private int _height = 512;

        [Header("Camera")]
        [SerializeField] private FP_ProjectionMode _projection = FP_ProjectionMode.Perspective;
        [SerializeField, Range(1f, 179f)] private float _fieldOfView = 35f;
        [SerializeField, Min(1f)] private float _boundsPadding = 1.1f;
        [SerializeField] private LayerMask _captureLayers = ~0;
        [SerializeField] private FP_ModelCaptureSpace _captureSpace =
            FP_ModelCaptureSpace.IsolatedAtOrigin;

        [Header("Lighting")]
        [SerializeField] private FP_ModelCaptureLightingMode _lightingMode =
            FP_ModelCaptureLightingMode.ThreePointRig;

        [Header("Three Point Rig")]
        [SerializeField] private FP_ModelCaptureLightAppearance _frontLightAppearance =
            FP_ModelCaptureLightAppearance.Color;
        [SerializeField] private Color _frontLightColor = new Color(1f, 0.92f, 0.82f, 1f);
        [SerializeField, Range(MinimumColorTemperature, MaximumColorTemperature)]
        private float _frontLightTemperature = 3200f;
        [SerializeField, Min(0f)] private float _frontLightIntensity = 1.25f;
        [SerializeField, Range(1f, 179f)] private float _frontLightSpotAngle = 55f;
        [SerializeField] private FP_ModelCaptureLightAppearance _backLightAppearance =
            FP_ModelCaptureLightAppearance.Color;
        [SerializeField] private Color _backLightColor = new Color(0.72f, 0.82f, 1f, 1f);
        [SerializeField, Range(MinimumColorTemperature, MaximumColorTemperature)]
        private float _backLightTemperature = 9000f;
        [SerializeField, Min(0f)] private float _backLightIntensity = 1f;
        [SerializeField, Range(1f, 179f)] private float _backLightSpotAngle = 60f;
        [SerializeField] private FP_ModelCaptureLightAppearance _directionalLightAppearance =
            FP_ModelCaptureLightAppearance.Color;
        [SerializeField] private Color _directionalLightColor = Color.white;
        [SerializeField, Range(MinimumColorTemperature, MaximumColorTemperature)]
        private float _directionalLightTemperature = 6500f;
        [SerializeField, Min(0f)] private float _directionalLightIntensity = 0.35f;
        [SerializeField] private Color _ambientColor = new Color(0.08f, 0.08f, 0.08f, 1f);
        [SerializeField, Min(1f)] private float _lightDistanceMultiplier = 2.5f;
        [SerializeField] private LightShadows _lightShadows = LightShadows.Soft;

        [Header("Background")]
        [SerializeField] private FP_ModelCaptureBackgroundMode _backgroundMode =
            FP_ModelCaptureBackgroundMode.SolidColor;
        [SerializeField] private Color _backgroundColor = new Color(0.12f, 0.12f, 0.12f, 1f);

        public int Width => Mathf.Max(16, _width);
        public int Height => Mathf.Max(16, _height);
        public FP_ProjectionMode Projection => _projection;
        public float FieldOfView => Mathf.Clamp(_fieldOfView, 1f, 179f);
        public float BoundsPadding => Mathf.Max(1f, _boundsPadding);
        public LayerMask CaptureLayers => _captureLayers;
        public FP_ModelCaptureSpace CaptureSpace => _captureSpace;
        public FP_ModelCaptureLightingMode LightingMode => _lightingMode;
        public FP_ModelCaptureLightAppearance FrontLightAppearance => _frontLightAppearance;
        public Color FrontLightColor => _frontLightColor;
        public float FrontLightTemperature => Mathf.Clamp(
            _frontLightTemperature,
            MinimumColorTemperature,
            MaximumColorTemperature);
        public float FrontLightIntensity => Mathf.Max(0f, _frontLightIntensity);
        public float FrontLightSpotAngle => Mathf.Clamp(_frontLightSpotAngle, 1f, 179f);
        public FP_ModelCaptureLightAppearance BackLightAppearance => _backLightAppearance;
        public Color BackLightColor => _backLightColor;
        public float BackLightTemperature => Mathf.Clamp(
            _backLightTemperature,
            MinimumColorTemperature,
            MaximumColorTemperature);
        public float BackLightIntensity => Mathf.Max(0f, _backLightIntensity);
        public float BackLightSpotAngle => Mathf.Clamp(_backLightSpotAngle, 1f, 179f);
        public FP_ModelCaptureLightAppearance DirectionalLightAppearance =>
            _directionalLightAppearance;
        public Color DirectionalLightColor => _directionalLightColor;
        public float DirectionalLightTemperature =>
            Mathf.Clamp(
                _directionalLightTemperature,
                MinimumColorTemperature,
                MaximumColorTemperature);
        public float DirectionalLightIntensity => Mathf.Max(0f, _directionalLightIntensity);
        public Color AmbientColor => _ambientColor;
        public float LightDistanceMultiplier => Mathf.Max(1f, _lightDistanceMultiplier);
        public LightShadows LightShadows => _lightShadows;
        public FP_ModelCaptureBackgroundMode BackgroundMode => _backgroundMode;
        public Color BackgroundColor => _backgroundMode == FP_ModelCaptureBackgroundMode.Transparent
            ? new Color(_backgroundColor.r, _backgroundColor.g, _backgroundColor.b, 0f)
            : _backgroundColor;
    }
}
