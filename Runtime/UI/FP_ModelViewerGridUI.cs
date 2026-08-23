namespace FuzzPhyte.ModelViewer
{
    using System;
    using System.Collections.Generic;
    using FuzzPhyte.UI;
    using FuzzPhyte.Utility;
    using FuzzPhyte.Utility.Meta;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UIElements;

    [Serializable]
    public sealed class FP_ModelViewerItemUnityEvent : UnityEvent<FP_ModelViewerItemData>
    {
    }

    [Serializable]
    public sealed class FP_ModelViewerPageUnityEvent : UnityEvent<int>
    {
    }

    [Serializable]
    public sealed class FP_ModelViewerSpawnedUnityEvent : UnityEvent<GameObject>
    {
    }

    [Serializable]
    public sealed class FP_ModelViewerVisibilityUnityEvent : UnityEvent<bool>
    {
    }
    [Serializable]
    public sealed class FP_ModelViewerVisibilityUnityEventInputRegion : UnityEvent<FP_ScreenRegionAsset>
    {

    }

    [Serializable]
    public sealed class FP_ModelViewerFilterUnityEvent : UnityEvent<int>
    {
    }

    [Serializable]
    public sealed class FP_ModelViewerStringUnityEvent : UnityEvent<string>
    {
    }

    public enum FP_ModelViewerLogoPlacement
    {
        TopLeft,
        TopCenter,
        TopRight
    }

    /// <summary>
    /// Builds a paged UI Toolkit catalog grid from FP_ModelViewerCatalogData.
    /// The generated hierarchy has functional inline layout and stable USS class names.
    /// </summary>
    public sealed class FP_ModelViewerGridUI : FP_UI
    {
        private const float MaximumCombinedInsetPercent = 99f;

        public const string RootClass = "fp-model-viewer-grid";
        public const string HeaderClass = "fp-model-viewer-grid__header";
        public const string PanelsClass = "fp-model-viewer-grid__panels";
        public const string PanelClass = "fp-model-viewer-grid__panel";
        public const string RowClass = "fp-model-viewer-grid__row";
        public const string ItemClass = "fp-model-viewer-grid__item";
        public const string ItemImageClass = "fp-model-viewer-grid__item-image";
        public const string ItemLabelClass = "fp-model-viewer-grid__item-label";
        public const string EmptyCellClass = "fp-model-viewer-grid__empty-cell";
        public const string EmptyMessageClass = "fp-model-viewer-grid__empty-message";
        public const string NavigationClass = "fp-model-viewer-grid__navigation";
        public const string PreviousButtonClass = "fp-model-viewer-grid__previous";
        public const string NextButtonClass = "fp-model-viewer-grid__next";
        public const string PageLabelClass = "fp-model-viewer-grid__page-label";
        public const string PopupOverlayClass = "fp-model-viewer-popup__overlay";
        public const string PopupPanelClass = "fp-model-viewer-popup__panel";
        public const string PopupTitleClass = "fp-model-viewer-popup__title";
        public const string PopupDescriptionClass = "fp-model-viewer-popup__description";
        public const string PopupThumbnailGridClass = "fp-model-viewer-popup__thumbnail-grid";
        public const string PopupThumbnailRowClass = "fp-model-viewer-popup__thumbnail-row";
        public const string PopupThumbnailClass = "fp-model-viewer-popup__thumbnail";
        public const string PopupThumbnailCaptionClass = "fp-model-viewer-popup__thumbnail-caption";
        public const string PopupActionsClass = "fp-model-viewer-popup__actions";
        public const string PopupBackButtonClass = "fp-model-viewer-popup__back";
        public const string PopupSpawnButtonClass = "fp-model-viewer-popup__spawn";
        public const string PopupObjExportButtonClass = "fp-model-viewer-popup__obj-export";
        public const string CatalogVisibilityButtonClass = "fp-model-viewer-grid__visibility-toggle";
        public const string CompanionUiOffButtonClass =
            "fp-model-viewer-grid__companion-ui-off";
        public const string CompanionUiOnButtonClass =
            "fp-model-viewer-grid__companion-ui-on";
        public const string TagFilterToggleClass = "fp-model-viewer-filter__toggle";
        public const string TagFilterPanelClass = "fp-model-viewer-filter__panel";
        public const string TagFilterHeaderClass = "fp-model-viewer-filter__header";
        public const string TagFilterScrollClass = "fp-model-viewer-filter__scroll";
        public const string TagFilterRowClass = "fp-model-viewer-filter__row";
        public const string TagFilterButtonClass = "fp-model-viewer-filter__tag";
        public const string TagFilterRadioClass = "fp-model-viewer-filter__radio";
        public const string TagFilterRadioDotClass = "fp-model-viewer-filter__radio-dot";
        public const string TagFilterSelectedClass = "fp-model-viewer-filter__tag--selected";
        public const string TagFilterClearClass = "fp-model-viewer-filter__clear";
        public const string LogoContainerClass = "fp-model-viewer-branding__logo-container";
        public const string LogoImageClass = "fp-model-viewer-branding__logo";
        private const string ButtonStyleRegisteredClass =
            "fp-model-viewer-button--interactive-style";

        [Header("Catalog")]
        [SerializeField] private FP_ModelViewerCatalogData _catalog;
        [SerializeField, Tooltip("Optional name of a VisualElement in the UIDocument that will host the generated grid.")]
        private string _hostElementName;
        [SerializeField, Tooltip("When the Catalog is visible, this sets the screen region of the orbital system to interact with")]
        private FP_ScreenRegionAsset _catalogVisibleOrbitScreenRegion;
        [SerializeField, Tooltip("When the Catalog is hidden, this sets the screen region of the orbital system to interact with")]
        private FP_ScreenRegionAsset _catalogHiddenOrbitScreenRegion;

        [Header("Grid Layout")]
        [SerializeField, Min(1)] private int _rows = 3;
        [SerializeField, Min(1)] private int _columns = 3;
        [SerializeField] private bool _showCatalogTitle = true;
        [SerializeField] private bool _showItemNames = true;
        [SerializeField] private bool _showNavigation = true;
        [SerializeField] private bool _wrapNavigation;

        [Header("Branding")]
        [SerializeField, Tooltip("Optional title override. Leave empty to use the assigned catalog's Display Name.")]
        private string _catalogTitleOverride;
        [SerializeField, Tooltip("Optional logo displayed along the top edge of the UI host.")]
        private Sprite _logoSprite;
        [SerializeField] private FP_ModelViewerLogoPlacement _logoPlacement =
            FP_ModelViewerLogoPlacement.TopRight;
        [SerializeField] private Vector2 _logoSize = new Vector2(128f, 64f);
        [SerializeField, Tooltip("Horizontal and top padding in pixels. Top Center uses X as an offset from center.")]
        private Vector2 _logoOffset = new Vector2(16f, 16f);

        [Header("Catalog Visibility")]
        [SerializeField] private bool _showCatalogVisibilityButton = true;
        [SerializeField] private string _hideCatalogLabel = "Hide Catalog";
        [SerializeField] private string _showCatalogLabel = "Show Catalog";
        [SerializeField, Tooltip("Hide the catalog after a successful spawn so the model remains unobstructed.")]
        private bool _hideCatalogAfterSpawn;

        [Header("Companion UI Toggle")]
        [SerializeField, Tooltip("Show paired Off and On action buttons at the lower-right of the UI host.")]
        private bool _showCompanionUiToggleButtons = true;
        [SerializeField] private string _companionUiOffButtonLabel = "Off";
        [SerializeField] private string _companionUiOnButtonLabel = "On";

        [Header("Tag Filters")]
        [SerializeField] private bool _showTagFilterButton = true;
        [SerializeField, Min(1)] private int _tagFilterColumns = 3;
        [SerializeField, Min(4), Tooltip("Maximum visible characters per tag button. Longer names end with an ellipsis.")]
        private int _tagFilterMaxCharacters = 18;
        [SerializeField, Range(10f, 100f)] private float _tagFilterPanelWidthPercent = 45f;
        [SerializeField, Range(10f, 100f)] private float _tagFilterPanelHeightPercent = 50f;
        [SerializeField] private Color _tagFilterPanelColor = new Color(0.12f, 0.12f, 0.12f, 0.98f);
        [SerializeField] private Color _tagFilterButtonColor = new Color(0.25f, 0.25f, 0.25f, 1f);
        [SerializeField] private Color _selectedTagFilterColor = new Color(0.12f, 0.45f, 0.78f, 1f);

        [Header("Item Cell Style")]
        [SerializeField] private Color _itemCellColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        [SerializeField] private Color _itemTextColor = new Color(0.12f, 0.12f, 0.12f, 1f);
        [SerializeField, Min(0f)] private float _itemCornerRadius = 8f;

        [Header("Button Style")]
        [SerializeField, Min(0f), Tooltip("Corner radius applied to every generated control button. Item cells retain their separate radius.")]
        private float _buttonCornerRadius = 8f;
        [SerializeField, Min(0f), Tooltip("Border thickness applied to every generated control button. Item cells retain their separate styling.")]
        private float _buttonOutlineThickness = 1f;
        [SerializeField] private Color _buttonTextColor = new Color(0.12f, 0.12f, 0.12f, 1f);
        [SerializeField] private Color _buttonBackgroundColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        [SerializeField] private Color _buttonHoverColor = new Color(0.88f, 0.88f, 0.88f, 1f);
        [SerializeField, Tooltip("Color used while a button is pressed or receives keyboard focus.")]
        private Color _buttonSelectedColor = new Color(0.12f, 0.45f, 0.78f, 1f);

        [Header("Container Insets (%)")]
        [SerializeField, Range(0f, 100f), Tooltip("Percentage of the host height reserved above the generated grid.")]
        private float _topInsetPercent;
        [SerializeField, Range(0f, 100f), Tooltip("Percentage of the host width reserved to the right of the generated grid.")]
        private float _rightInsetPercent;
        [SerializeField, Range(0f, 100f), Tooltip("Percentage of the host height reserved below the generated grid.")]
        private float _bottomInsetPercent;
        [SerializeField, Range(0f, 100f), Tooltip("Percentage of the host width reserved to the left of the generated grid.")]
        private float _leftInsetPercent;

        [Header("Item Detail Popup")]
        [SerializeField] private bool _showPopupOnSelection = true;
        [SerializeField] private Color _popupBackdropColor = new Color(0f, 0f, 0f, 0.72f);
        [SerializeField] private Color _popupPanelColor = new Color(0.16f, 0.16f, 0.16f, 1f);
        [SerializeField] private Color _popupTextColor = Color.white;
        [SerializeField, Min(0f)] private float _popupCornerRadius = 12f;
        [SerializeField, Range(10f, 100f)] private float _popupWidthPercent = 80f;
        [SerializeField, Range(10f, 100f)] private float _popupHeightPercent = 80f;

        [Header("Item Spawning")]
        [SerializeField, Tooltip("The selected included prefab is spawned at this Transform's position and rotation.")]
        private Transform _spawnTarget;
        [SerializeField] private bool _parentSpawnedItemToTarget = true;
        [SerializeField] private bool _closePopupAfterSpawn = true;

        [Header("Item OBJ Export")]
        [SerializeField, Tooltip("Show a runtime OBJ download action in the item detail popup.")]
        private bool _showObjExportButton = true;
        [SerializeField] private string _objExportButtonLabel = "Download OBJ";
        [SerializeField] private bool _objExportMaterials = true;
        [SerializeField, Tooltip("Include PNG copies of the materials' main textures. Texture readback increases memory use, especially in WebGL.")]
        private bool _objExportTextures;
        [SerializeField] private bool _objExportIncludeInactive = true;
        [SerializeField] private bool _objExportIncludeSkinnedMeshes = true;
        [SerializeField, Tooltip("Mesh colliders are excluded by default to avoid duplicating visible MeshFilter geometry.")]
        private bool _objExportIncludeMeshColliders;
        [SerializeField, Min(0), Tooltip("Maximum total vertices allowed in one runtime export. Use 0 for no limit.")]
        private int _objExportMaximumVertexCount = 500000;
        [SerializeField, Min(1), Tooltip("Largest width or height used when PNG textures are included.")]
        private int _objExportMaximumTextureSize = 2048;

        [Header("Events")]
        [SerializeField] private FP_ModelViewerItemUnityEvent _onItemSelected =
            new FP_ModelViewerItemUnityEvent();
        [SerializeField] private FP_ModelViewerPageUnityEvent _onPageChanged =
            new FP_ModelViewerPageUnityEvent();
        [SerializeField] private UnityEvent _onGridRebuilt = new UnityEvent();
        [SerializeField] private FP_ModelViewerSpawnedUnityEvent _onItemSpawned =
            new FP_ModelViewerSpawnedUnityEvent();
        [SerializeField] private UnityEvent _onPopupClosed = new UnityEvent();
        [SerializeField] private FP_ModelViewerSpawnedUnityEvent _onSpawnedItemRemoved =
            new FP_ModelViewerSpawnedUnityEvent();
        [SerializeField] private FP_ModelViewerVisibilityUnityEvent _onCatalogVisibilityChanged =
            new FP_ModelViewerVisibilityUnityEvent();
        [SerializeField] private FP_ModelViewerVisibilityUnityEventInputRegion _onCatalogHidden = 
            new FP_ModelViewerVisibilityUnityEventInputRegion();
        [SerializeField] private FP_ModelViewerVisibilityUnityEventInputRegion _onCatalogVisible =
            new FP_ModelViewerVisibilityUnityEventInputRegion();
        [SerializeField] private FP_ModelViewerFilterUnityEvent _onTagFiltersChanged =
            new FP_ModelViewerFilterUnityEvent();
        [SerializeField] private FP_ModelViewerStringUnityEvent _onObjExported =
            new FP_ModelViewerStringUnityEvent();
        [SerializeField] private FP_ModelViewerStringUnityEvent _onObjExportFailed =
            new FP_ModelViewerStringUnityEvent();
        [SerializeField] private UnityEvent _onCompanionUiTurnedOff = new UnityEvent();
        [SerializeField] private UnityEvent _onCompanionUiTurnedOn = new UnityEvent();

        private readonly List<VisualElement> _panels = new List<VisualElement>();
        private readonly List<FP_Tag> _catalogTags = new List<FP_Tag>();
        private readonly List<FP_Tag> _activeTagFilters = new List<FP_Tag>();
        private readonly List<FP_ModelViewerItemData> _visibleItems =
            new List<FP_ModelViewerItemData>();
        private VisualElement _host;
        private VisualElement _generatedRoot;
        private Button _previousButton;
        private Button _nextButton;
        private VisualElement _navigation;
        private Label _pageLabel;
        private VisualElement _popupOverlay;
        private Button _popupSpawnButton;
        private Button _popupObjExportButton;
        private Button _catalogVisibilityButton;
        private Button _companionUiOffButton;
        private Button _companionUiOnButton;
        private Button _tagFilterToggleButton;
        private Button _tagFilterClearButton;
        private VisualElement _tagFilterPanel;
        private Label _catalogTitleLabel;
        private VisualElement _logoContainer;
        private Image _logoImage;
        private FP_ModelViewerItemData _selectedItem;
        private GameObject _spawnedItem;
        private FP_ModelViewerItemData _spawnedItemData;
        private int _currentPageIndex;
        private bool _hasAwakened;
        private bool _isCatalogVisible = true;
        private bool _isCompanionUiOn = true;
        private bool _isTagFilterPanelVisible;

        public event Action<FP_ModelViewerItemData> ItemSelected;
        public event Action<int> PageChanged;
        public event Action<GameObject> ItemSpawned;
        public event Action<GameObject> SpawnedItemRemoved;
        public event Action PopupClosed;
        public event Action<bool> CatalogVisibilityChanged;
        public event Action<IReadOnlyList<FP_Tag>> TagFiltersChanged;
        public event Action<string> ObjExported;
        public event Action<string> ObjExportFailed;
        public event Action CompanionUiTurnedOff;
        public event Action CompanionUiTurnedOn;

        public FP_ModelViewerCatalogData Catalog => _catalog;
        public int Rows => _rows;
        public int Columns => _columns;
        public int ItemsPerPanel => FP_ModelViewerPagination.GetItemsPerPage(_rows, _columns);
        public int PanelCount => FP_ModelViewerPagination.GetPageCount(
            _visibleItems.Count,
            _rows,
            _columns);
        public int GeneratedPanelCount => _panels.Count;
        public int CurrentPageIndex => _currentPageIndex;
        public float TopInsetPercent => _topInsetPercent;
        public float RightInsetPercent => _rightInsetPercent;
        public float BottomInsetPercent => _bottomInsetPercent;
        public float LeftInsetPercent => _leftInsetPercent;
        public Color ItemCellColor => _itemCellColor;
        public Color ItemTextColor => _itemTextColor;
        public float ItemCornerRadius => _itemCornerRadius;
        public float ButtonCornerRadius => _buttonCornerRadius;
        public float ButtonOutlineThickness => _buttonOutlineThickness;
        public Color ButtonTextColor => _buttonTextColor;
        public Color ButtonBackgroundColor => _buttonBackgroundColor;
        public Color ButtonHoverColor => _buttonHoverColor;
        public Color ButtonSelectedColor => _buttonSelectedColor;
        public string CatalogTitle => GetCatalogTitle();
        public Sprite LogoSprite => _logoSprite;
        public FP_ModelViewerLogoPlacement LogoPlacement => _logoPlacement;
        public Vector2 LogoSize => _logoSize;
        public Vector2 LogoOffset => _logoOffset;
        public Transform SpawnTarget => _spawnTarget;
        public FP_ModelViewerItemData SelectedItem => _selectedItem;
        public GameObject SpawnedItem => _spawnedItem;
        public bool IsPopupOpen => _popupOverlay != null;
        public bool IsCatalogVisible => _isCatalogVisible;
        public bool IsCompanionUiOn => _isCompanionUiOn;
        public bool IsTagFilterPanelVisible => _isTagFilterPanelVisible;
        public int VisibleItemCount => _visibleItems.Count;
        public IReadOnlyList<FP_Tag> CatalogTags => _catalogTags;
        public IReadOnlyList<FP_Tag> ActiveTagFilters => _activeTagFilters;
        public FP_ModelViewerItemUnityEvent OnItemSelected => _onItemSelected;
        public FP_ModelViewerPageUnityEvent OnPageChanged => _onPageChanged;
        public UnityEvent OnGridRebuilt => _onGridRebuilt;
        public FP_ModelViewerSpawnedUnityEvent OnItemSpawned => _onItemSpawned;
        public FP_ModelViewerSpawnedUnityEvent OnSpawnedItemRemoved => _onSpawnedItemRemoved;
        public UnityEvent OnPopupClosed => _onPopupClosed;
        public FP_ModelViewerVisibilityUnityEvent OnCatalogVisibilityChanged =>
            _onCatalogVisibilityChanged;
        public FP_ModelViewerVisibilityUnityEventInputRegion OnCatalogVisibilityHidden =>
            _onCatalogHidden;
        public FP_ModelViewerVisibilityUnityEventInputRegion OnCatalogVisibiltyShown =>
            _onCatalogVisible;
        public FP_ModelViewerFilterUnityEvent OnTagFiltersChanged => _onTagFiltersChanged;
        public FP_ModelViewerStringUnityEvent OnObjExported => _onObjExported;
        public FP_ModelViewerStringUnityEvent OnObjExportFailed => _onObjExportFailed;
        public UnityEvent OnCompanionUiTurnedOff => _onCompanionUiTurnedOff;
        public UnityEvent OnCompanionUiTurnedOn => _onCompanionUiTurnedOn;

        public override void Awake()
        {
            base.Awake();
            _hasAwakened = true;
            ResolveHostAndBuild();
        }

        /// <summary>
        /// Model Viewer has a complete inline layout and does not require a USS asset.
        /// When a caller supplies one, attach it to the UIDocument root automatically.
        /// </summary>
        protected override void SetupUIElements()
        {
            if (Document == null)
            {
                RootContainer = null;
                Debug.LogError("FP Model Viewer requires a UIDocument reference.", this);
                return;
            }

            RootContainer = Document.rootVisualElement;
            if (RootContainer == null)
            {
                return;
            }

            AttachDocumentStyleSheet();
        }

        private void OnEnable()
        {
            if (_hasAwakened && _generatedRoot == null)
            {
                ResolveHostAndBuild();
            }
        }

        private void OnDisable()
        {
            RemoveGeneratedUI();
        }

        private void ResolveHostAndBuild()
        {
            if (Document != null)
            {
                RootContainer = Document.rootVisualElement;
                AttachDocumentStyleSheet();
            }
            if (RootContainer == null)
            {
                return;
            }

            VisualElement host = string.IsNullOrWhiteSpace(_hostElementName)
                ? RootContainer
                : RootContainer.Q<VisualElement>(_hostElementName);
            if (host == null)
            {
                Debug.LogError(
                    $"FP Model Viewer could not find UI Toolkit host '{_hostElementName}'.",
                    this);
                return;
            }

            Build(host);
        }

        private void AttachDocumentStyleSheet()
        {
            if (RootContainer != null &&
                DocumentStyleSheet != null &&
                !RootContainer.styleSheets.Contains(DocumentStyleSheet))
            {
                RootContainer.styleSheets.Add(DocumentStyleSheet);
            }
        }

        private void OnValidate()
        {
            _rows = Mathf.Max(1, _rows);
            _columns = Mathf.Max(1, _columns);
            _tagFilterColumns = Mathf.Max(1, _tagFilterColumns);
            _tagFilterMaxCharacters = Mathf.Max(4, _tagFilterMaxCharacters);
            _itemCornerRadius = Mathf.Max(0f, _itemCornerRadius);
            _buttonCornerRadius = Mathf.Max(0f, _buttonCornerRadius);
            _buttonOutlineThickness = Mathf.Max(0f, _buttonOutlineThickness);
            _popupCornerRadius = Mathf.Max(0f, _popupCornerRadius);
            _objExportMaximumVertexCount = Mathf.Max(0, _objExportMaximumVertexCount);
            _objExportMaximumTextureSize = Mathf.Max(1, _objExportMaximumTextureSize);
            _logoSize.x = Mathf.Max(1f, _logoSize.x);
            _logoSize.y = Mathf.Max(1f, _logoSize.y);
            NormalizeInsetPair(ref _topInsetPercent, ref _bottomInsetPercent);
            NormalizeInsetPair(ref _leftInsetPercent, ref _rightInsetPercent);
            ApplyContainerInsets();
            ApplyItemCellStyle();
            ApplyGeneratedButtonStyle();
            UpdateCatalogTitle();
            RebuildLogo();
        }

        private void OnDestroy()
        {
            RemoveGeneratedUI();
        }

        public void SetCatalog(FP_ModelViewerCatalogData catalog)
        {
            if (_catalog == catalog)
            {
                return;
            }

            _catalog = catalog;
            _activeTagFilters.Clear();
            _currentPageIndex = 0;
            RebuildCatalogTagState();
            RebuildVisibleItems();
            Refresh();
        }

        public void SetGridDimensions(int rows, int columns)
        {
            int safeRows = Mathf.Max(1, rows);
            int safeColumns = Mathf.Max(1, columns);
            if (_rows == safeRows && _columns == safeColumns)
            {
                return;
            }

            _rows = safeRows;
            _columns = safeColumns;
            _currentPageIndex = 0;
            Refresh();
        }

        public void SetContainerInsetsPercent(
            float top,
            float right,
            float bottom,
            float left)
        {
            _topInsetPercent = top;
            _rightInsetPercent = right;
            _bottomInsetPercent = bottom;
            _leftInsetPercent = left;
            NormalizeInsetPair(ref _topInsetPercent, ref _bottomInsetPercent);
            NormalizeInsetPair(ref _leftInsetPercent, ref _rightInsetPercent);
            ApplyContainerInsets();
        }

        public void SetItemCellStyle(Color cellColor, Color textColor, float cornerRadius)
        {
            _itemCellColor = cellColor;
            _itemTextColor = textColor;
            _itemCornerRadius = Mathf.Max(0f, cornerRadius);
            ApplyItemCellStyle();
        }

        public void SetButtonCornerRadius(float cornerRadius)
        {
            _buttonCornerRadius = Mathf.Max(0f, cornerRadius);
            ApplyGeneratedButtonStyle();
        }

        public void SetButtonOutlineThickness(float outlineThickness)
        {
            _buttonOutlineThickness = Mathf.Max(0f, outlineThickness);
            ApplyGeneratedButtonStyle();
        }

        public void SetButtonColors(
            Color textColor,
            Color backgroundColor,
            Color hoverColor,
            Color selectedColor)
        {
            _buttonTextColor = textColor;
            _buttonBackgroundColor = backgroundColor;
            _buttonHoverColor = hoverColor;
            _buttonSelectedColor = selectedColor;
            ApplyGeneratedButtonStyle();
        }

        public void SetButtonStyle(
            Color textColor,
            Color backgroundColor,
            Color hoverColor,
            Color selectedColor,
            float cornerRadius)
        {
            _buttonCornerRadius = Mathf.Max(0f, cornerRadius);
            SetButtonColors(textColor, backgroundColor, hoverColor, selectedColor);
        }

        public void SetButtonStyle(
            Color textColor,
            Color backgroundColor,
            Color hoverColor,
            Color selectedColor,
            float cornerRadius,
            float outlineThickness)
        {
            _buttonOutlineThickness = Mathf.Max(0f, outlineThickness);
            SetButtonStyle(
                textColor,
                backgroundColor,
                hoverColor,
                selectedColor,
                cornerRadius);
        }

        public void SetCatalogTitle(string titleOverride)
        {
            _catalogTitleOverride = titleOverride ?? string.Empty;
            UpdateCatalogTitle();
        }

        public void SetLogo(
            Sprite logo,
            FP_ModelViewerLogoPlacement placement,
            Vector2 size,
            Vector2 offset)
        {
            _logoSprite = logo;
            _logoPlacement = placement;
            _logoSize = new Vector2(Mathf.Max(1f, size.x), Mathf.Max(1f, size.y));
            _logoOffset = offset;
            RebuildLogo();
        }

        public void ClearLogo()
        {
            _logoSprite = null;
            RebuildLogo();
        }

        public void SetSpawnTarget(Transform spawnTarget)
        {
            _spawnTarget = spawnTarget;
            UpdatePopupSpawnButton();
        }

        public void SetHideCatalogAfterSpawn(bool hideAfterSpawn)
        {
            _hideCatalogAfterSpawn = hideAfterSpawn;
        }

        public void SetObjExportEnabled(bool enabled)
        {
            _showObjExportButton = enabled;
            if (_selectedItem != null && _popupOverlay != null)
            {
                ShowItemDetails(_selectedItem);
            }
        }

        public void SetObjExportSettings(
            bool exportMaterials,
            bool exportTextures,
            bool includeInactive,
            bool includeSkinnedMeshes,
            bool includeMeshColliders,
            int maximumVertexCount,
            int maximumTextureSize)
        {
            _objExportMaterials = exportMaterials;
            _objExportTextures = exportTextures;
            _objExportIncludeInactive = includeInactive;
            _objExportIncludeSkinnedMeshes = includeSkinnedMeshes;
            _objExportIncludeMeshColliders = includeMeshColliders;
            _objExportMaximumVertexCount = Mathf.Max(0, maximumVertexCount);
            _objExportMaximumTextureSize = Mathf.Max(1, maximumTextureSize);
        }

        public void SetTagFilterLayout(int columns, int maxCharacters)
        {
            _tagFilterColumns = Mathf.Max(1, columns);
            _tagFilterMaxCharacters = Mathf.Max(4, maxCharacters);
            Refresh();
        }

        public bool ToggleTagFilterPanel()
        {
            return SetTagFilterPanelVisibility(!_isTagFilterPanelVisible);
        }

        public bool ShowTagFilterPanel()
        {
            return SetTagFilterPanelVisibility(true);
        }

        public bool HideTagFilterPanel()
        {
            return SetTagFilterPanelVisibility(false);
        }

        public bool ToggleTagFilter(FP_Tag tag)
        {
            if (tag == null || !_catalogTags.Contains(tag))
            {
                return false;
            }

            if (_activeTagFilters.Contains(tag))
            {
                _activeTagFilters.Remove(tag);
            }
            else
            {
                _activeTagFilters.Add(tag);
            }

            ApplyTagFilters();
            return true;
        }

        public bool ClearTagFilters()
        {
            if (_activeTagFilters.Count == 0)
            {
                return false;
            }

            _activeTagFilters.Clear();
            ApplyTagFilters();
            return true;
        }

        public bool HideCatalog()
        {
            return SetCatalogVisibility(false);
        }

        public bool ShowCatalog()
        {
            return SetCatalogVisibility(true);
        }

        public bool ToggleCatalogVisibility()
        {
            return SetCatalogVisibility(!_isCatalogVisible);
        }

        public bool TurnCompanionUiOff()
        {
            return SetCompanionUiState(false);
        }

        public bool TurnCompanionUiOn()
        {
            return SetCompanionUiState(true);
        }

        public bool SetCompanionUiState(bool isOn)
        {
            if (_isCompanionUiOn == isOn)
            {
                return false;
            }

            _isCompanionUiOn = isOn;
            ApplyCompanionUiButtonVisibility();
            if (_isCompanionUiOn)
            {
                _onCompanionUiTurnedOn.Invoke();
                CompanionUiTurnedOn?.Invoke();
            }
            else
            {
                _onCompanionUiTurnedOff.Invoke();
                CompanionUiTurnedOff?.Invoke();
            }
            return true;
        }

        /// <summary>
        /// Builds into a supplied host. This is also useful when a caller owns the UIDocument setup.
        /// </summary>
        public void Build(VisualElement host)
        {
            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            RemoveGeneratedUI();
            _host = host;
            RebuildCatalogTagState();
            RebuildVisibleItems();
            _generatedRoot = CreateRoot();
            _host.Add(_generatedRoot);
            RebuildLogo();
            BuildPanels();
            BuildNavigation();
            BuildTagFilterControls();
            BuildCatalogVisibilityButton();
            BuildCompanionUiToggleButtons();
            ShowPage(_currentPageIndex, false);
            ApplyCatalogVisibility();
            ApplyGeneratedButtonStyle();
            _onGridRebuilt.Invoke();
        }

        public void Refresh()
        {
            if (_host != null)
            {
                Build(_host);
            }
        }

        public bool GoToPage(int pageIndex)
        {
            return ShowPage(pageIndex, true);
        }

        public bool NextPage()
        {
            int pageCount = _panels.Count;
            if (pageCount == 0)
            {
                return false;
            }

            int requestedIndex = _currentPageIndex + 1;
            if (requestedIndex >= pageCount)
            {
                requestedIndex = _wrapNavigation ? 0 : pageCount - 1;
            }

            return ShowPage(requestedIndex, true);
        }

        public bool PreviousPage()
        {
            int pageCount = _panels.Count;
            if (pageCount == 0)
            {
                return false;
            }

            int requestedIndex = _currentPageIndex - 1;
            if (requestedIndex < 0)
            {
                requestedIndex = _wrapNavigation ? pageCount - 1 : 0;
            }

            return ShowPage(requestedIndex, true);
        }

        public bool SelectItem(FP_ModelViewerItemData item)
        {
            if (item == null)
            {
                return false;
            }

            _selectedItem = item;
            if (_showPopupOnSelection)
            {
                ShowItemDetails(item);
            }

            _onItemSelected.Invoke(item);
            ItemSelected?.Invoke(item);
            return true;
        }

        public bool ShowItemDetails(FP_ModelViewerItemData item)
        {
            if (item == null || _host == null)
            {
                return false;
            }

            RemovePopup(false);
            _selectedItem = item;
            _popupOverlay = CreatePopupOverlay(item);
            _host.Add(_popupOverlay);
            ApplyGeneratedButtonStyle();
            return true;
        }

        public bool CloseItemDetails()
        {
            if (_popupOverlay == null)
            {
                return false;
            }

            RemovePopup(true);
            return true;
        }

        public GameObject SpawnSelectedItem()
        {
            if (_selectedItem == null ||
                _selectedItem.IncludedPrefab == null ||
                _spawnTarget == null)
            {
                return null;
            }

            RemoveSpawnedItem();
            Transform parent = _parentSpawnedItemToTarget ? _spawnTarget : null;
            _spawnedItem = Instantiate(
                _selectedItem.IncludedPrefab,
                _spawnTarget.position,
                _spawnTarget.rotation,
                parent);
            _spawnedItemData = _selectedItem;
            _onItemSpawned.Invoke(_spawnedItem);
            ItemSpawned?.Invoke(_spawnedItem);
            if (_closePopupAfterSpawn)
            {
                CloseItemDetails();
            }
            if (_hideCatalogAfterSpawn)
            {
                HideCatalog();
            }

            return _spawnedItem;
        }

        public bool RemoveSpawnedItem()
        {
            if (_spawnedItem == null)
            {
                return false;
            }

            GameObject itemToRemove = _spawnedItem;
            _spawnedItem = null;
            _spawnedItemData = null;
            _onSpawnedItemRemoved.Invoke(itemToRemove);
            SpawnedItemRemoved?.Invoke(itemToRemove);
            itemToRemove.SetActive(false);
            if (Application.isPlaying)
            {
                Destroy(itemToRemove);
            }
            else
            {
                DestroyImmediate(itemToRemove);
            }

            return true;
        }

        /// <summary>
        /// Builds the selected item's OBJ ZIP without opening a platform save/download UI.
        /// </summary>
        public bool TryBuildSelectedItemObjPackage(
            out FPMeshRuntimeObjExportResult result,
            out string message)
        {
            result = null;
            message = string.Empty;
            if (_selectedItem == null)
            {
                message = "No model viewer item is selected.";
                return false;
            }

            GameObject exportRoot = null;
            bool destroyExportRoot = false;
            if (_spawnedItem != null && _spawnedItemData == _selectedItem)
            {
                exportRoot = _spawnedItem;
            }
            else if (_selectedItem.IncludedPrefab != null)
            {
                exportRoot = Instantiate(_selectedItem.IncludedPrefab);
                exportRoot.name = _selectedItem.DisplayName;
                destroyExportRoot = true;
            }

            if (exportRoot == null)
            {
                message = "The selected item does not reference an included prefab to export.";
                return false;
            }

            try
            {
                var options = new FPMeshRuntimeObjExportOptions
                {
                    IncludeChildren = true,
                    IncludeInactive = _objExportIncludeInactive,
                    IncludeMeshFilters = true,
                    IncludeSkinnedMeshRenderers = _objExportIncludeSkinnedMeshes,
                    IncludeMeshColliders = _objExportIncludeMeshColliders,
                    ExportMaterials = _objExportMaterials,
                    ExportTextures = _objExportMaterials && _objExportTextures,
                    RootLocalSpace = true,
                    MirrorX = true,
                    MaximumVertexCount = _objExportMaximumVertexCount,
                    MaximumTextureSize = _objExportMaximumTextureSize
                };
                return FPMeshRuntimeObjExporter.TryBuildPackage(
                    exportRoot,
                    options,
                    out result,
                    out message);
            }
            finally
            {
                if (destroyExportRoot)
                {
                    exportRoot.SetActive(false);
                    if (Application.isPlaying)
                    {
                        Destroy(exportRoot);
                    }
                    else
                    {
                        DestroyImmediate(exportRoot);
                    }
                }
            }
        }

        /// <summary>
        /// Builds and delivers the selected item's OBJ ZIP for the active platform.
        /// </summary>
        public bool ExportSelectedItemObj()
        {
            if (!TryBuildSelectedItemObjPackage(
                    out FPMeshRuntimeObjExportResult result,
                    out string buildMessage))
            {
                PublishObjExportFailure(buildMessage);
                return false;
            }

            if (!FPFileExportUtility.TrySaveOrDownload(
                    result.Data,
                    result.FileName,
                    result.MimeType,
                    out string deliveredLocation,
                    out string deliveryMessage))
            {
                PublishObjExportFailure(deliveryMessage);
                return false;
            }

            if (string.IsNullOrWhiteSpace(deliveredLocation))
            {
                Debug.Log($"[FP Model Viewer] {deliveryMessage}", this);
                return false;
            }

            Debug.Log($"[FP Model Viewer] {deliveryMessage}", this);
            _onObjExported.Invoke(deliveredLocation);
            ObjExported?.Invoke(deliveredLocation);
            return true;
        }

        private void PublishObjExportFailure(string message)
        {
            string safeMessage = string.IsNullOrWhiteSpace(message)
                ? "The OBJ export could not be completed."
                : message;
            Debug.LogError($"[FP Model Viewer] {safeMessage}", this);
            _onObjExportFailed.Invoke(safeMessage);
            ObjExportFailed?.Invoke(safeMessage);
        }

        private VisualElement CreateRoot()
        {
            var root = new VisualElement { name = "FPModelViewerGrid" };
            root.AddToClassList(RootClass);
            root.style.flexDirection = FlexDirection.Column;
            root.style.position = Position.Absolute;
            ApplyContainerInsets(root);

            if (_showCatalogTitle &&
                (_catalog != null || !string.IsNullOrWhiteSpace(_catalogTitleOverride)))
            {
                _catalogTitleLabel = new Label(GetCatalogTitle());
                _catalogTitleLabel.AddToClassList(HeaderClass);
                _catalogTitleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                _catalogTitleLabel.style.fontSize = 20f;
                _catalogTitleLabel.style.marginBottom = 6f;
                root.Add(_catalogTitleLabel);
            }

            var panelsContainer = new VisualElement { name = "Panels" };
            panelsContainer.AddToClassList(PanelsClass);
            panelsContainer.style.flexGrow = 1f;
            root.Add(panelsContainer);
            return root;
        }

        private string GetCatalogTitle()
        {
            if (!string.IsNullOrWhiteSpace(_catalogTitleOverride))
            {
                return _catalogTitleOverride;
            }

            return _catalog != null ? _catalog.DisplayName : string.Empty;
        }

        private void UpdateCatalogTitle()
        {
            if (_catalogTitleLabel != null)
            {
                _catalogTitleLabel.text = GetCatalogTitle();
            }
        }

        private void RebuildLogo()
        {
            if (_logoContainer != null)
            {
                _logoContainer.RemoveFromHierarchy();
                _logoContainer = null;
                _logoImage = null;
            }
            if (_host == null || _logoSprite == null)
            {
                return;
            }

            _logoContainer = new VisualElement
            {
                name = "FPModelViewerLogoContainer",
                pickingMode = PickingMode.Ignore
            };
            _logoContainer.AddToClassList(LogoContainerClass);
            _logoContainer.style.position = Position.Absolute;
            _logoContainer.style.top = Mathf.Max(0f, _logoOffset.y);
            _logoContainer.style.left = 0f;
            _logoContainer.style.right = 0f;
            _logoContainer.style.flexDirection = FlexDirection.Row;
            _logoContainer.style.justifyContent = GetLogoJustification(_logoPlacement);

            _logoImage = new Image
            {
                name = "FPModelViewerLogo",
                sprite = _logoSprite,
                scaleMode = ScaleMode.ScaleToFit,
                pickingMode = PickingMode.Ignore
            };
            _logoImage.AddToClassList(LogoImageClass);
            _logoImage.style.width = _logoSize.x;
            _logoImage.style.height = _logoSize.y;
            ApplyLogoHorizontalOffset(_logoImage);
            _logoContainer.Add(_logoImage);
            _host.Add(_logoContainer);
        }

        private void ApplyLogoHorizontalOffset(VisualElement logo)
        {
            float horizontalOffset = _logoOffset.x;
            switch (_logoPlacement)
            {
                case FP_ModelViewerLogoPlacement.TopLeft:
                    logo.style.marginLeft = Mathf.Max(0f, horizontalOffset);
                    break;
                case FP_ModelViewerLogoPlacement.TopCenter:
                    logo.style.position = Position.Relative;
                    logo.style.left = horizontalOffset;
                    break;
                case FP_ModelViewerLogoPlacement.TopRight:
                    logo.style.marginRight = Mathf.Max(0f, horizontalOffset);
                    break;
            }
        }

        private static Justify GetLogoJustification(FP_ModelViewerLogoPlacement placement)
        {
            switch (placement)
            {
                case FP_ModelViewerLogoPlacement.TopLeft:
                    return Justify.FlexStart;
                case FP_ModelViewerLogoPlacement.TopCenter:
                    return Justify.Center;
                default:
                    return Justify.FlexEnd;
            }
        }

        private void ApplyContainerInsets()
        {
            if (_generatedRoot != null)
            {
                ApplyContainerInsets(_generatedRoot);
            }
        }

        private void ApplyContainerInsets(VisualElement root)
        {
            root.style.top = Length.Percent(_topInsetPercent);
            root.style.right = Length.Percent(_rightInsetPercent);
            root.style.bottom = Length.Percent(_bottomInsetPercent);
            root.style.left = Length.Percent(_leftInsetPercent);
        }

        private static void NormalizeInsetPair(ref float first, ref float second)
        {
            first = Mathf.Max(0f, first);
            second = Mathf.Max(0f, second);
            float total = first + second;
            if (total <= MaximumCombinedInsetPercent)
            {
                return;
            }

            float scale = MaximumCombinedInsetPercent / total;
            first *= scale;
            second *= scale;
        }

        private void RebuildCatalogTagState()
        {
            _catalogTags.Clear();
            if (_catalog != null)
            {
                for (int itemIndex = 0; itemIndex < _catalog.Items.Count; itemIndex++)
                {
                    FP_ModelViewerItemData item = _catalog.Items[itemIndex];
                    if (item == null)
                    {
                        continue;
                    }

                    for (int tagIndex = 0; tagIndex < item.Tags.Count; tagIndex++)
                    {
                        FP_Tag tag = item.Tags[tagIndex];
                        if (tag != null && !_catalogTags.Contains(tag))
                        {
                            _catalogTags.Add(tag);
                        }
                    }
                }
            }

            for (int i = _activeTagFilters.Count - 1; i >= 0; i--)
            {
                if (!_catalogTags.Contains(_activeTagFilters[i]))
                {
                    _activeTagFilters.RemoveAt(i);
                }
            }
        }

        private void RebuildVisibleItems()
        {
            _visibleItems.Clear();
            if (_catalog == null)
            {
                return;
            }

            for (int i = 0; i < _catalog.Items.Count; i++)
            {
                FP_ModelViewerItemData item = _catalog.Items[i];
                if (item != null &&
                    (_activeTagFilters.Count == 0 || item.HasAllTags(_activeTagFilters)))
                {
                    _visibleItems.Add(item);
                }
            }
        }

        private void ApplyTagFilters()
        {
            _currentPageIndex = 0;
            Refresh();
            _onTagFiltersChanged.Invoke(_visibleItems.Count);
            TagFiltersChanged?.Invoke(_activeTagFilters);
        }

        private void BuildPanels()
        {
            _panels.Clear();
            VisualElement panelsContainer = _generatedRoot.Q<VisualElement>("Panels");
            int itemCount = _visibleItems.Count;
            int panelCount = FP_ModelViewerPagination.GetPageCount(
                itemCount,
                _rows,
                _columns);
            if (panelCount == 0)
            {
                var emptyMessage = new Label("No catalog items are available.");
                emptyMessage.AddToClassList(EmptyMessageClass);
                panelsContainer.Add(emptyMessage);
                return;
            }

            for (int pageIndex = 0; pageIndex < panelCount; pageIndex++)
            {
                VisualElement panel = CreatePanel(pageIndex, itemCount);
                _panels.Add(panel);
                panelsContainer.Add(panel);
            }
        }

        private VisualElement CreatePanel(int pageIndex, int itemCount)
        {
            FP_ModelViewerPageRange range = FP_ModelViewerPagination.GetPageRange(
                itemCount,
                _rows,
                _columns,
                pageIndex);
            var panel = new VisualElement { name = $"Panel-{pageIndex + 1}" };
            panel.AddToClassList(PanelClass);
            panel.style.flexGrow = 1f;
            panel.style.flexDirection = FlexDirection.Column;

            for (int rowIndex = 0; rowIndex < _rows; rowIndex++)
            {
                int rowStartOffset = rowIndex * _columns;
                if (rowStartOffset >= range.Count)
                {
                    break;
                }

                var row = new VisualElement { name = $"Row-{rowIndex + 1}" };
                row.AddToClassList(RowClass);
                row.style.flexGrow = 1f;
                row.style.flexDirection = FlexDirection.Row;
                panel.Add(row);

                int rowItemCount = Mathf.Min(_columns, range.Count - rowStartOffset);
                for (int columnIndex = 0; columnIndex < rowItemCount; columnIndex++)
                {
                    int pageOffset = rowStartOffset + columnIndex;
                    int catalogIndex = range.StartIndex + pageOffset;
                    VisualElement cell = CreateItemButton(
                        _visibleItems[catalogIndex],
                        catalogIndex);
                    cell.style.flexBasis = 0f;
                    cell.style.minWidth = 0f;
                    row.Add(cell);
                }
            }

            return panel;
        }

        private VisualElement CreateItemButton(FP_ModelViewerItemData item, int catalogIndex)
        {
            var button = new Button(() => SelectItem(item))
            {
                name = $"Item-{catalogIndex + 1}",
                userData = item,
                tooltip = item.DisplayName
            };
            button.AddToClassList(ItemClass);
            button.style.flexGrow = 1f;
            button.style.flexDirection = FlexDirection.Column;
            button.style.marginLeft = 4f;
            button.style.marginRight = 4f;
            button.style.marginTop = 4f;
            button.style.marginBottom = 4f;
            ApplyItemCellStyle(button);

            var image = new Image
            {
                name = "CoverImage",
                image = item.CoverTexture,
                scaleMode = ScaleMode.ScaleToFit,
                pickingMode = PickingMode.Ignore
            };
            image.AddToClassList(ItemImageClass);
            image.style.flexGrow = 1f;
            image.style.minHeight = 48f;
            button.Add(image);

            if (_showItemNames)
            {
                var label = new Label(item.DisplayName)
                {
                    name = "ItemLabel",
                    pickingMode = PickingMode.Ignore
                };
                label.AddToClassList(ItemLabelClass);
                label.style.unityTextAlign = TextAnchor.MiddleCenter;
                label.style.whiteSpace = WhiteSpace.Normal;
                button.Add(label);
            }

            return button;
        }

        private void ApplyItemCellStyle()
        {
            if (_generatedRoot == null)
            {
                return;
            }

            List<Button> itemButtons = _generatedRoot
                .Query<Button>(className: ItemClass)
                .ToList();
            for (int i = 0; i < itemButtons.Count; i++)
            {
                ApplyItemCellStyle(itemButtons[i]);
            }
        }

        private void ApplyItemCellStyle(Button button)
        {
            button.style.backgroundColor = _itemCellColor;
            button.style.color = _itemTextColor;
            button.style.overflow = Overflow.Hidden;
            ApplyCornerRadius(button, _itemCornerRadius);
        }

        private void ApplyGeneratedButtonStyle()
        {
            ApplyGeneratedButtonStyle(_generatedRoot);
            ApplyGeneratedButtonStyle(_popupOverlay);
            if (_catalogVisibilityButton != null)
            {
                ApplyControlButtonStyle(_catalogVisibilityButton);
            }
            if (_companionUiOffButton != null)
            {
                ApplyControlButtonStyle(_companionUiOffButton);
            }
            if (_companionUiOnButton != null)
            {
                ApplyControlButtonStyle(_companionUiOnButton);
            }
        }

        private void ApplyGeneratedButtonStyle(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            List<Button> buttons = root.Query<Button>().ToList();
            for (int i = 0; i < buttons.Count; i++)
            {
                Button button = buttons[i];
                if (button.ClassListContains(ItemClass))
                {
                    continue;
                }

                ApplyControlButtonStyle(button);
            }
        }

        private void ApplyControlButtonStyle(Button button)
        {
            button.style.color = _buttonTextColor;
            button.style.backgroundColor = _buttonBackgroundColor;
            button.style.overflow = Overflow.Hidden;
            button.style.borderTopWidth = _buttonOutlineThickness;
            button.style.borderRightWidth = _buttonOutlineThickness;
            button.style.borderBottomWidth = _buttonOutlineThickness;
            button.style.borderLeftWidth = _buttonOutlineThickness;
            ApplyCornerRadius(button, _buttonCornerRadius);
            if (button.ClassListContains(ButtonStyleRegisteredClass))
            {
                return;
            }

            button.AddToClassList(ButtonStyleRegisteredClass);
            button.RegisterCallback<PointerEnterEvent>(_ =>
            {
                if (button.enabledInHierarchy)
                {
                    button.style.backgroundColor = _buttonHoverColor;
                }
            });
            button.RegisterCallback<PointerLeaveEvent>(_ =>
                button.style.backgroundColor = _buttonBackgroundColor);
            button.RegisterCallback<PointerDownEvent>(_ =>
            {
                if (button.enabledInHierarchy)
                {
                    button.style.backgroundColor = _buttonSelectedColor;
                }
            });
            button.RegisterCallback<PointerUpEvent>(_ =>
            {
                if (button.enabledInHierarchy)
                {
                    button.style.backgroundColor = _buttonHoverColor;
                }
            });
            button.RegisterCallback<FocusInEvent>(_ =>
            {
                if (button.enabledInHierarchy)
                {
                    button.style.backgroundColor = _buttonSelectedColor;
                }
            });
            button.RegisterCallback<FocusOutEvent>(_ =>
                button.style.backgroundColor = _buttonBackgroundColor);
        }

        private VisualElement CreatePopupOverlay(FP_ModelViewerItemData item)
        {
            var overlay = new VisualElement { name = "FPModelViewerPopupOverlay" };
            overlay.AddToClassList(PopupOverlayClass);
            overlay.style.position = Position.Absolute;
            overlay.style.top = 0f;
            overlay.style.right = 0f;
            overlay.style.bottom = 0f;
            overlay.style.left = 0f;
            overlay.style.backgroundColor = _popupBackdropColor;
            overlay.style.justifyContent = Justify.Center;
            overlay.style.alignItems = Align.Center;

            var panel = new VisualElement { name = "FPModelViewerPopupPanel" };
            panel.AddToClassList(PopupPanelClass);
            panel.style.width = Length.Percent(_popupWidthPercent);
            panel.style.height = Length.Percent(_popupHeightPercent);
            panel.style.paddingLeft = 16f;
            panel.style.paddingRight = 16f;
            panel.style.paddingTop = 12f;
            panel.style.paddingBottom = 12f;
            panel.style.backgroundColor = _popupPanelColor;
            panel.style.color = _popupTextColor;
            panel.style.overflow = Overflow.Hidden;
            ApplyCornerRadius(panel, _popupCornerRadius);
            overlay.Add(panel);

            var title = new Label(item.DisplayName) { name = "PopupTitle" };
            title.AddToClassList(PopupTitleClass);
            title.style.fontSize = 22f;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            title.style.color = _popupTextColor;
            title.style.marginBottom = 6f;
            panel.Add(title);

            if (!string.IsNullOrWhiteSpace(item.Description))
            {
                var description = new Label(item.Description) { name = "PopupDescription" };
                description.AddToClassList(PopupDescriptionClass);
                description.style.whiteSpace = WhiteSpace.Normal;
                description.style.color = _popupTextColor;
                description.style.marginBottom = 6f;
                panel.Add(description);
            }

            BuildPopupThumbnailGrid(panel, item);
            BuildPopupActions(panel);
            return overlay;
        }

        private void BuildPopupThumbnailGrid(
            VisualElement panel,
            FP_ModelViewerItemData item)
        {
            var thumbnails = new List<FP_ModelThumbnailReference>();
            for (int i = 0; i < item.Thumbnails.Count; i++)
            {
                FP_ModelThumbnailReference thumbnail = item.Thumbnails[i];
                if (thumbnail != null && thumbnail.Texture != null)
                {
                    thumbnails.Add(thumbnail);
                }
            }

            var scrollView = new ScrollView(ScrollViewMode.Vertical)
            {
                name = "PopupThumbnailGrid"
            };
            scrollView.AddToClassList(PopupThumbnailGridClass);
            scrollView.style.flexGrow = 1f;
            panel.Add(scrollView);

            if (thumbnails.Count == 0)
            {
                var noImages = new Label("No thumbnails are available for this item.");
                noImages.style.unityTextAlign = TextAnchor.MiddleCenter;
                noImages.style.color = _popupTextColor;
                scrollView.Add(noImages);
                return;
            }

            int columns = FP_ModelViewerPagination.GetSquareGridColumns(thumbnails.Count);
            int rows = FP_ModelViewerPagination.GetSquareGridRows(thumbnails.Count);
            for (int rowIndex = 0; rowIndex < rows; rowIndex++)
            {
                var row = new VisualElement { name = $"ThumbnailRow-{rowIndex + 1}" };
                row.AddToClassList(PopupThumbnailRowClass);
                row.style.flexDirection = FlexDirection.Row;
                row.style.flexGrow = 1f;
                scrollView.Add(row);

                for (int columnIndex = 0; columnIndex < columns; columnIndex++)
                {
                    int index = (rowIndex * columns) + columnIndex;
                    VisualElement cell = index < thumbnails.Count
                        ? CreatePopupThumbnail(thumbnails[index])
                        : new VisualElement { pickingMode = PickingMode.Ignore };
                    cell.style.width = Length.Percent(100f / columns);
                    row.Add(cell);
                }
            }
        }

        private VisualElement CreatePopupThumbnail(FP_ModelThumbnailReference thumbnail)
        {
            var cell = new VisualElement();
            cell.AddToClassList(PopupThumbnailClass);
            cell.style.flexGrow = 1f;
            cell.style.position = Position.Relative;
            cell.style.overflow = Overflow.Hidden;
            cell.style.minHeight = 96f;
            cell.style.marginLeft = 4f;
            cell.style.marginRight = 4f;
            cell.style.marginTop = 4f;
            cell.style.marginBottom = 4f;
            cell.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                if (evt.newRect.width > 0f &&
                    !Mathf.Approximately(cell.resolvedStyle.height, evt.newRect.width))
                {
                    cell.style.height = evt.newRect.width;
                }
            });

            var image = new Image
            {
                image = thumbnail.Texture,
                scaleMode = ScaleMode.ScaleToFit,
                pickingMode = PickingMode.Ignore
            };
            image.style.position = Position.Absolute;
            image.style.top = 0f;
            image.style.right = 0f;
            image.style.bottom = 0f;
            image.style.left = 0f;
            cell.Add(image);

            if (!string.IsNullOrWhiteSpace(thumbnail.Caption))
            {
                var caption = new Label(thumbnail.Caption)
                {
                    pickingMode = PickingMode.Ignore
                };
                caption.AddToClassList(PopupThumbnailCaptionClass);
                caption.style.position = Position.Absolute;
                caption.style.right = 0f;
                caption.style.bottom = 0f;
                caption.style.left = 0f;
                caption.style.minHeight = 24f;
                caption.style.paddingLeft = 4f;
                caption.style.paddingRight = 4f;
                caption.style.paddingTop = 3f;
                caption.style.paddingBottom = 3f;
                caption.style.backgroundColor = new Color(0f, 0f, 0f, 0.72f);
                caption.style.unityTextAlign = TextAnchor.MiddleCenter;
                caption.style.color = _popupTextColor;
                cell.Add(caption);
            }

            return cell;
        }

        private void BuildCatalogVisibilityButton()
        {
            if (!_showCatalogVisibilityButton || _host == null)
            {
                return;
            }

            _catalogVisibilityButton = new Button(() => ToggleCatalogVisibility())
            {
                name = "FPModelViewerCatalogVisibility"
            };
            _catalogVisibilityButton.AddToClassList(CatalogVisibilityButtonClass);
            _catalogVisibilityButton.style.position = Position.Absolute;
            _catalogVisibilityButton.style.left = 12f;
            _catalogVisibilityButton.style.bottom = 12f;
            _catalogVisibilityButton.style.minWidth = 110f;
            _host.Add(_catalogVisibilityButton);
            UpdateCatalogVisibilityButton();
        }

        private void BuildCompanionUiToggleButtons()
        {
            if (!_showCompanionUiToggleButtons || _host == null)
            {
                return;
            }

            _companionUiOffButton = CreateCompanionUiButton(
                "FPModelViewerCompanionUiOff",
                _companionUiOffButtonLabel,
                CompanionUiOffButtonClass,
                TurnCompanionUiOff);
            _companionUiOnButton = CreateCompanionUiButton(
                "FPModelViewerCompanionUiOn",
                _companionUiOnButtonLabel,
                CompanionUiOnButtonClass,
                TurnCompanionUiOn);
            _host.Add(_companionUiOffButton);
            _host.Add(_companionUiOnButton);
            ApplyCompanionUiButtonVisibility();
        }

        private static Button CreateCompanionUiButton(
            string name,
            string label,
            string className,
            Func<bool> action)
        {
            var button = new Button(() => action())
            {
                name = name,
                text = label
            };
            button.AddToClassList(className);
            button.style.position = Position.Absolute;
            button.style.right = 12f;
            button.style.bottom = 12f;
            button.style.minWidth = 110f;
            return button;
        }

        private void ApplyCompanionUiButtonVisibility()
        {
            if (_companionUiOffButton != null)
            {
                _companionUiOffButton.style.display = _isCompanionUiOn
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            }
            if (_companionUiOnButton != null)
            {
                _companionUiOnButton.style.display = _isCompanionUiOn
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
            }
        }

        private bool SetCatalogVisibility(bool visible)
        {
            if (_isCatalogVisible == visible)
            {
                return false;
            }

            _isCatalogVisible = visible;
            if (!visible)
            {
                RemovePopup(false);
                _onCatalogHidden.Invoke(_catalogHiddenOrbitScreenRegion);
            }
            else 
            {
                _onCatalogVisible.Invoke(_catalogVisibleOrbitScreenRegion);
            }
            ApplyCatalogVisibility();
            _onCatalogVisibilityChanged.Invoke(_isCatalogVisible);
            CatalogVisibilityChanged?.Invoke(_isCatalogVisible);
            return true;
        }

        private void ApplyCatalogVisibility()
        {
            if (_generatedRoot != null)
            {
                _generatedRoot.style.display = _isCatalogVisible
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            }
            UpdateCatalogVisibilityButton();
        }

        private void UpdateCatalogVisibilityButton()
        {
            if (_catalogVisibilityButton != null)
            {
                _catalogVisibilityButton.text = _isCatalogVisible
                    ? _hideCatalogLabel
                    : _showCatalogLabel;
            }
        }

        private void BuildTagFilterControls()
        {
            if (!_showTagFilterButton)
            {
                return;
            }

            _tagFilterToggleButton = new Button(() => ToggleTagFilterPanel())
            {
                name = "FPModelViewerTagFilterToggle"
            };
            _tagFilterToggleButton.AddToClassList(TagFilterToggleClass);
            _tagFilterToggleButton.style.position = Position.Absolute;
            _tagFilterToggleButton.style.top = 4f;
            _tagFilterToggleButton.style.right = 4f;
            _generatedRoot.Add(_tagFilterToggleButton);

            _tagFilterPanel = new VisualElement { name = "FPModelViewerTagFilterPanel" };
            _tagFilterPanel.AddToClassList(TagFilterPanelClass);
            _tagFilterPanel.style.position = Position.Absolute;
            _tagFilterPanel.style.top = 36f;
            _tagFilterPanel.style.right = 4f;
            _tagFilterPanel.style.width = Length.Percent(_tagFilterPanelWidthPercent);
            _tagFilterPanel.style.height = Length.Percent(_tagFilterPanelHeightPercent);
            _tagFilterPanel.style.paddingLeft = 8f;
            _tagFilterPanel.style.paddingRight = 8f;
            _tagFilterPanel.style.paddingTop = 8f;
            _tagFilterPanel.style.paddingBottom = 8f;
            _tagFilterPanel.style.backgroundColor = _tagFilterPanelColor;
            _tagFilterPanel.style.overflow = Overflow.Hidden;
            ApplyCornerRadius(_tagFilterPanel, 8f);
            _generatedRoot.Add(_tagFilterPanel);

            var header = new VisualElement { name = "TagFilterHeader" };
            header.AddToClassList(TagFilterHeaderClass);
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;
            _tagFilterPanel.Add(header);

            var title = new Label("Filter by Tags");
            title.style.flexGrow = 1f;
            title.style.color = Color.white;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.Add(title);

            _tagFilterClearButton = new Button(() => ClearTagFilters()) { text = "Clear" };
            _tagFilterClearButton.AddToClassList(TagFilterClearClass);
            _tagFilterClearButton.SetEnabled(_activeTagFilters.Count > 0);
            header.Add(_tagFilterClearButton);

            var closeButton = new Button(() => HideTagFilterPanel()) { text = "Close" };
            closeButton.style.marginLeft = 4f;
            header.Add(closeButton);

            var scrollView = new ScrollView(ScrollViewMode.Vertical)
            {
                name = "TagFilterScroll"
            };
            scrollView.AddToClassList(TagFilterScrollClass);
            scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            scrollView.verticalScrollerVisibility = ScrollerVisibility.Auto;
            scrollView.style.flexGrow = 1f;
            scrollView.style.marginTop = 6f;
            _tagFilterPanel.Add(scrollView);

            if (_catalogTags.Count == 0)
            {
                var emptyLabel = new Label("This catalog has no tags.");
                emptyLabel.style.color = Color.white;
                scrollView.Add(emptyLabel);
            }
            else
            {
                BuildTagFilterRows(scrollView);
            }

            ApplyTagFilterPanelVisibility();
        }

        private void BuildTagFilterRows(ScrollView scrollView)
        {
            int rowCount = (_catalogTags.Count + _tagFilterColumns - 1) /
                _tagFilterColumns;
            for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                var row = new VisualElement { name = $"TagFilterRow-{rowIndex + 1}" };
                row.AddToClassList(TagFilterRowClass);
                row.style.flexDirection = FlexDirection.Row;
                row.style.width = Length.Percent(100f);
                scrollView.Add(row);

                for (int columnIndex = 0; columnIndex < _tagFilterColumns; columnIndex++)
                {
                    int tagIndex = (rowIndex * _tagFilterColumns) + columnIndex;
                    if (tagIndex >= _catalogTags.Count)
                    {
                        var spacer = new VisualElement { pickingMode = PickingMode.Ignore };
                        ApplyTagFilterCellLayout(spacer);
                        row.Add(spacer);
                        continue;
                    }

                    FP_Tag tag = _catalogTags[tagIndex];
                    string fullName = GetTagDisplayName(tag);
                    bool selected = _activeTagFilters.Contains(tag);
                    var radio = new Toggle
                    {
                        name = $"TagFilter-{tagIndex + 1}",
                        text = TruncateTagLabel(fullName),
                        tooltip = fullName,
                        userData = tag,
                        value = selected
                    };
                    radio.AddToClassList(TagFilterButtonClass);
                    radio.AddToClassList(TagFilterRadioClass);
                    if (selected)
                    {
                        radio.AddToClassList(TagFilterSelectedClass);
                    }
                    ApplyTagFilterCellLayout(radio);
                    radio.style.paddingLeft = 6f;
                    radio.style.paddingRight = 6f;
                    radio.style.paddingTop = 4f;
                    radio.style.paddingBottom = 4f;
                    radio.style.backgroundColor = selected
                        ? _selectedTagFilterColor
                        : _tagFilterButtonColor;
                    radio.style.overflow = Overflow.Hidden;
                    ApplyCornerRadius(radio, _buttonCornerRadius);
                    VisualElement checkmark = radio.Q<VisualElement>(
                        className: "unity-toggle__checkmark");
                    if (checkmark != null)
                    {
                        checkmark.style.width = 16f;
                        checkmark.style.height = 16f;
                        checkmark.style.minWidth = 16f;
                        checkmark.style.minHeight = 16f;
                        checkmark.style.maxWidth = 16f;
                        checkmark.style.maxHeight = 16f;
                        checkmark.style.flexShrink = 0f;
                        checkmark.style.position = Position.Relative;
                        checkmark.style.unityBackgroundImageTintColor = Color.clear;
                        checkmark.style.backgroundColor = Color.clear;
                        checkmark.style.borderTopWidth = 1f;
                        checkmark.style.borderRightWidth = 1f;
                        checkmark.style.borderBottomWidth = 1f;
                        checkmark.style.borderLeftWidth = 1f;
                        checkmark.style.borderTopColor = Color.white;
                        checkmark.style.borderRightColor = Color.white;
                        checkmark.style.borderBottomColor = Color.white;
                        checkmark.style.borderLeftColor = Color.white;
                        ApplyCornerRadius(checkmark, 999f);

                        var dot = new VisualElement { pickingMode = PickingMode.Ignore };
                        dot.AddToClassList(TagFilterRadioDotClass);
                        dot.style.position = Position.Absolute;
                        dot.style.top = 3f;
                        dot.style.left = 3f;
                        dot.style.width = 8f;
                        dot.style.height = 8f;
                        dot.style.backgroundColor = Color.white;
                        dot.style.display = selected ? DisplayStyle.Flex : DisplayStyle.None;
                        ApplyCornerRadius(dot, 999f);
                        checkmark.Add(dot);
                    }
                    radio.RegisterValueChangedCallback(evt =>
                    {
                        bool isSelected = _activeTagFilters.Contains(tag);
                        if (evt.newValue != isSelected)
                        {
                            ToggleTagFilter(tag);
                        }
                    });
                    row.Add(radio);
                }
            }
        }

        private static void ApplyTagFilterCellLayout(VisualElement cell)
        {
            cell.style.flexGrow = 1f;
            cell.style.flexBasis = 0f;
            cell.style.minWidth = 0f;
            cell.style.marginLeft = 2f;
            cell.style.marginRight = 2f;
            cell.style.marginTop = 2f;
            cell.style.marginBottom = 2f;
        }

        private bool SetTagFilterPanelVisibility(bool visible)
        {
            if (_isTagFilterPanelVisible == visible)
            {
                return false;
            }

            _isTagFilterPanelVisible = visible;
            ApplyTagFilterPanelVisibility();
            return true;
        }

        private void ApplyTagFilterPanelVisibility()
        {
            if (_tagFilterPanel != null)
            {
                _tagFilterPanel.style.display = _isTagFilterPanelVisible
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            }
            if (_tagFilterToggleButton != null)
            {
                _tagFilterToggleButton.text = _activeTagFilters.Count == 0
                    ? "Filters"
                    : $"Filters ({_activeTagFilters.Count})";
                _tagFilterToggleButton.SetEnabled(_catalogTags.Count > 0);
            }
            if (_tagFilterClearButton != null)
            {
                _tagFilterClearButton.SetEnabled(_activeTagFilters.Count > 0);
            }
        }

        private string TruncateTagLabel(string label)
        {
            if (label.Length <= _tagFilterMaxCharacters)
            {
                return label;
            }

            return $"{label.Substring(0, _tagFilterMaxCharacters - 3)}...";
        }

        private static string GetTagDisplayName(FP_Tag tag)
        {
            return !string.IsNullOrWhiteSpace(tag.TagName) ? tag.TagName : tag.name;
        }

        private void BuildPopupActions(VisualElement panel)
        {
            var actions = new VisualElement { name = "PopupActions" };
            actions.AddToClassList(PopupActionsClass);
            actions.style.flexDirection = FlexDirection.Row;
            actions.style.justifyContent = Justify.Center;
            actions.style.marginTop = 8f;
            panel.Add(actions);

            var backButton = new Button(() => CloseItemDetails()) { text = "Back to Catalog" };
            backButton.AddToClassList(PopupBackButtonClass);
            actions.Add(backButton);

            _popupSpawnButton = new Button(() => SpawnSelectedItem()) { text = "Spawn Item" };
            _popupSpawnButton.AddToClassList(PopupSpawnButtonClass);
            _popupSpawnButton.style.marginLeft = 8f;
            actions.Add(_popupSpawnButton);

            if (_showObjExportButton)
            {
                string exportLabel = string.IsNullOrWhiteSpace(_objExportButtonLabel)
                    ? "Download OBJ"
                    : _objExportButtonLabel;
                _popupObjExportButton = new Button(() => ExportSelectedItemObj())
                {
                    text = exportLabel
                };
                _popupObjExportButton.AddToClassList(PopupObjExportButtonClass);
                _popupObjExportButton.style.marginLeft = 8f;
                actions.Add(_popupObjExportButton);
            }
            UpdatePopupSpawnButton();
            UpdatePopupObjExportButton();
        }

        private void UpdatePopupSpawnButton()
        {
            if (_popupSpawnButton != null)
            {
                _popupSpawnButton.SetEnabled(
                    _spawnTarget != null &&
                    _selectedItem != null &&
                    _selectedItem.IncludedPrefab != null);
            }
        }

        private void UpdatePopupObjExportButton()
        {
            if (_popupObjExportButton != null)
            {
                bool hasMatchingSpawnedItem =
                    _spawnedItem != null && _spawnedItemData == _selectedItem;
                _popupObjExportButton.SetEnabled(
                    _selectedItem != null &&
                    (hasMatchingSpawnedItem || _selectedItem.IncludedPrefab != null));
            }
        }

        private static void ApplyCornerRadius(VisualElement element, float radius)
        {
            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
            element.style.borderBottomRightRadius = radius;
            element.style.borderBottomLeftRadius = radius;
        }

        private void BuildNavigation()
        {
            _navigation = new VisualElement { name = "Navigation" };
            _navigation.AddToClassList(NavigationClass);
            _navigation.style.flexDirection = FlexDirection.Row;
            _navigation.style.justifyContent = Justify.Center;
            _navigation.style.alignItems = Align.Center;
            _navigation.style.marginTop = 6f;

            _previousButton = new Button(() => PreviousPage()) { text = "Previous" };
            _previousButton.AddToClassList(PreviousButtonClass);
            _navigation.Add(_previousButton);

            _pageLabel = new Label();
            _pageLabel.AddToClassList(PageLabelClass);
            _pageLabel.style.marginLeft = 12f;
            _pageLabel.style.marginRight = 12f;
            _navigation.Add(_pageLabel);

            _nextButton = new Button(() => NextPage()) { text = "Next" };
            _nextButton.AddToClassList(NextButtonClass);
            _navigation.Add(_nextButton);

            _navigation.style.display = _showNavigation && _panels.Count > 1
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            _generatedRoot.Add(_navigation);
        }

        private bool ShowPage(int requestedIndex, bool notify)
        {
            if (_panels.Count == 0)
            {
                _currentPageIndex = 0;
                UpdateNavigation();
                return false;
            }

            int pageIndex = Mathf.Clamp(requestedIndex, 0, _panels.Count - 1);
            bool changed = pageIndex != _currentPageIndex;
            _currentPageIndex = pageIndex;
            for (int i = 0; i < _panels.Count; i++)
            {
                _panels[i].style.display = i == _currentPageIndex
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            }

            UpdateNavigation();
            if (notify && changed)
            {
                _onPageChanged.Invoke(_currentPageIndex);
                PageChanged?.Invoke(_currentPageIndex);
            }

            return changed;
        }

        private void UpdateNavigation()
        {
            if (_pageLabel == null)
            {
                return;
            }

            _pageLabel.text = _panels.Count == 0
                ? "0 / 0"
                : $"{_currentPageIndex + 1} / {_panels.Count}";
            _previousButton.SetEnabled(
                _wrapNavigation || (_panels.Count > 1 && _currentPageIndex > 0));
            _nextButton.SetEnabled(
                _wrapNavigation ||
                (_panels.Count > 1 && _currentPageIndex < _panels.Count - 1));
        }

        private void RemoveGeneratedUI()
        {
            RemovePopup(false);
            if (_catalogVisibilityButton != null)
            {
                _catalogVisibilityButton.RemoveFromHierarchy();
            }
            if (_companionUiOffButton != null)
            {
                _companionUiOffButton.RemoveFromHierarchy();
            }
            if (_companionUiOnButton != null)
            {
                _companionUiOnButton.RemoveFromHierarchy();
            }
            if (_logoContainer != null)
            {
                _logoContainer.RemoveFromHierarchy();
            }
            if (_generatedRoot != null)
            {
                _generatedRoot.RemoveFromHierarchy();
            }

            _generatedRoot = null;
            _previousButton = null;
            _nextButton = null;
            _pageLabel = null;
            _navigation = null;
            _catalogVisibilityButton = null;
            _companionUiOffButton = null;
            _companionUiOnButton = null;
            _tagFilterToggleButton = null;
            _tagFilterClearButton = null;
            _tagFilterPanel = null;
            _catalogTitleLabel = null;
            _logoContainer = null;
            _logoImage = null;
            _panels.Clear();
        }

        private void RemovePopup(bool notify)
        {
            if (_popupOverlay != null)
            {
                _popupOverlay.RemoveFromHierarchy();
            }

            _popupOverlay = null;
            _popupSpawnButton = null;
            _popupObjExportButton = null;
            if (notify)
            {
                _onPopupClosed.Invoke();
                PopupClosed?.Invoke();
            }
        }
    }
}
