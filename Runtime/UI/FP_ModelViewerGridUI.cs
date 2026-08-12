namespace FuzzPhyte.ModelViewer
{
    using System;
    using System.Collections.Generic;
    using FuzzPhyte.UI;
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
        public const string CatalogVisibilityButtonClass = "fp-model-viewer-grid__visibility-toggle";

        [Header("Catalog")]
        [SerializeField] private FP_ModelViewerCatalogData _catalog;
        [SerializeField, Tooltip("Optional name of a VisualElement in the UIDocument that will host the generated grid.")]
        private string _hostElementName;

        [Header("Grid Layout")]
        [SerializeField, Min(1)] private int _rows = 3;
        [SerializeField, Min(1)] private int _columns = 3;
        [SerializeField] private bool _showCatalogTitle = true;
        [SerializeField] private bool _showItemNames = true;
        [SerializeField] private bool _showNavigation = true;
        [SerializeField] private bool _wrapNavigation;

        [Header("Catalog Visibility")]
        [SerializeField] private bool _showCatalogVisibilityButton = true;
        [SerializeField] private string _hideCatalogLabel = "Hide Catalog";
        [SerializeField] private string _showCatalogLabel = "Show Catalog";
        [SerializeField, Tooltip("Hide the catalog after a successful spawn so the model remains unobstructed.")]
        private bool _hideCatalogAfterSpawn;

        [Header("Item Cell Style")]
        [SerializeField] private Color _itemCellColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        [SerializeField] private Color _itemTextColor = new Color(0.12f, 0.12f, 0.12f, 1f);
        [SerializeField, Min(0f)] private float _itemCornerRadius = 8f;

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

        private readonly List<VisualElement> _panels = new List<VisualElement>();
        private VisualElement _host;
        private VisualElement _generatedRoot;
        private Button _previousButton;
        private Button _nextButton;
        private Label _pageLabel;
        private VisualElement _popupOverlay;
        private Button _popupSpawnButton;
        private Button _catalogVisibilityButton;
        private FP_ModelViewerItemData _selectedItem;
        private GameObject _spawnedItem;
        private int _currentPageIndex;
        private bool _hasAwakened;
        private bool _isCatalogVisible = true;

        public event Action<FP_ModelViewerItemData> ItemSelected;
        public event Action<int> PageChanged;
        public event Action<GameObject> ItemSpawned;
        public event Action<GameObject> SpawnedItemRemoved;
        public event Action PopupClosed;
        public event Action<bool> CatalogVisibilityChanged;

        public FP_ModelViewerCatalogData Catalog => _catalog;
        public int Rows => _rows;
        public int Columns => _columns;
        public int ItemsPerPanel => FP_ModelViewerPagination.GetItemsPerPage(_rows, _columns);
        public int PanelCount => FP_ModelViewerPagination.GetPageCount(
            _catalog != null ? _catalog.Count : 0,
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
        public Transform SpawnTarget => _spawnTarget;
        public FP_ModelViewerItemData SelectedItem => _selectedItem;
        public GameObject SpawnedItem => _spawnedItem;
        public bool IsPopupOpen => _popupOverlay != null;
        public bool IsCatalogVisible => _isCatalogVisible;
        public FP_ModelViewerItemUnityEvent OnItemSelected => _onItemSelected;
        public FP_ModelViewerPageUnityEvent OnPageChanged => _onPageChanged;
        public UnityEvent OnGridRebuilt => _onGridRebuilt;
        public FP_ModelViewerSpawnedUnityEvent OnItemSpawned => _onItemSpawned;
        public FP_ModelViewerSpawnedUnityEvent OnSpawnedItemRemoved => _onSpawnedItemRemoved;
        public UnityEvent OnPopupClosed => _onPopupClosed;
        public FP_ModelViewerVisibilityUnityEvent OnCatalogVisibilityChanged =>
            _onCatalogVisibilityChanged;

        public override void Awake()
        {
            base.Awake();
            _hasAwakened = true;
            ResolveHostAndBuild();
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

        private void OnValidate()
        {
            _rows = Mathf.Max(1, _rows);
            _columns = Mathf.Max(1, _columns);
            _itemCornerRadius = Mathf.Max(0f, _itemCornerRadius);
            _popupCornerRadius = Mathf.Max(0f, _popupCornerRadius);
            NormalizeInsetPair(ref _topInsetPercent, ref _bottomInsetPercent);
            NormalizeInsetPair(ref _leftInsetPercent, ref _rightInsetPercent);
            ApplyContainerInsets();
            ApplyItemCellStyle();
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
            _currentPageIndex = 0;
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

        public void SetSpawnTarget(Transform spawnTarget)
        {
            _spawnTarget = spawnTarget;
            UpdatePopupSpawnButton();
        }

        public void SetHideCatalogAfterSpawn(bool hideAfterSpawn)
        {
            _hideCatalogAfterSpawn = hideAfterSpawn;
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
            _generatedRoot = CreateRoot();
            _host.Add(_generatedRoot);
            BuildPanels();
            BuildNavigation();
            BuildCatalogVisibilityButton();
            ShowPage(_currentPageIndex, false);
            ApplyCatalogVisibility();
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

        private VisualElement CreateRoot()
        {
            var root = new VisualElement { name = "FPModelViewerGrid" };
            root.AddToClassList(RootClass);
            root.style.flexDirection = FlexDirection.Column;
            root.style.position = Position.Absolute;
            ApplyContainerInsets(root);

            if (_showCatalogTitle && _catalog != null)
            {
                var title = new Label(_catalog.DisplayName);
                title.AddToClassList(HeaderClass);
                title.style.unityFontStyleAndWeight = FontStyle.Bold;
                title.style.fontSize = 20f;
                title.style.marginBottom = 6f;
                root.Add(title);
            }

            var panelsContainer = new VisualElement { name = "Panels" };
            panelsContainer.AddToClassList(PanelsClass);
            panelsContainer.style.flexGrow = 1f;
            root.Add(panelsContainer);
            return root;
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

        private void BuildPanels()
        {
            _panels.Clear();
            VisualElement panelsContainer = _generatedRoot.Q<VisualElement>("Panels");
            int itemCount = _catalog != null ? _catalog.Count : 0;
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
                var row = new VisualElement { name = $"Row-{rowIndex + 1}" };
                row.AddToClassList(RowClass);
                row.style.flexGrow = 1f;
                row.style.flexDirection = FlexDirection.Row;
                panel.Add(row);

                for (int columnIndex = 0; columnIndex < _columns; columnIndex++)
                {
                    int pageOffset = (rowIndex * _columns) + columnIndex;
                    int catalogIndex = range.StartIndex + pageOffset;
                    VisualElement cell = pageOffset < range.Count &&
                        _catalog.TryGetItem(catalogIndex, out FP_ModelViewerItemData item)
                        ? CreateItemButton(item, catalogIndex)
                        : CreateEmptyCell();
                    cell.style.width = Length.Percent(100f / _columns);
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

        private VisualElement CreateEmptyCell()
        {
            var emptyCell = new VisualElement { pickingMode = PickingMode.Ignore };
            emptyCell.AddToClassList(EmptyCellClass);
            emptyCell.style.flexGrow = 1f;
            emptyCell.style.marginLeft = 4f;
            emptyCell.style.marginRight = 4f;
            emptyCell.style.marginTop = 4f;
            emptyCell.style.marginBottom = 4f;
            emptyCell.style.backgroundColor = _itemCellColor;
            ApplyCornerRadius(emptyCell, _itemCornerRadius);
            return emptyCell;
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

            List<VisualElement> emptyCells = _generatedRoot
                .Query<VisualElement>(className: EmptyCellClass)
                .ToList();
            for (int i = 0; i < emptyCells.Count; i++)
            {
                emptyCells[i].style.backgroundColor = _itemCellColor;
                ApplyCornerRadius(emptyCells[i], _itemCornerRadius);
            }
        }

        private void ApplyItemCellStyle(Button button)
        {
            button.style.backgroundColor = _itemCellColor;
            button.style.color = _itemTextColor;
            button.style.overflow = Overflow.Hidden;
            ApplyCornerRadius(button, _itemCornerRadius);
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
            UpdatePopupSpawnButton();
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

        private static void ApplyCornerRadius(VisualElement element, float radius)
        {
            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
            element.style.borderBottomRightRadius = radius;
            element.style.borderBottomLeftRadius = radius;
        }

        private void BuildNavigation()
        {
            var navigation = new VisualElement { name = "Navigation" };
            navigation.AddToClassList(NavigationClass);
            navigation.style.flexDirection = FlexDirection.Row;
            navigation.style.justifyContent = Justify.Center;
            navigation.style.alignItems = Align.Center;
            navigation.style.marginTop = 6f;

            _previousButton = new Button(() => PreviousPage()) { text = "Previous" };
            _previousButton.AddToClassList(PreviousButtonClass);
            navigation.Add(_previousButton);

            _pageLabel = new Label();
            _pageLabel.AddToClassList(PageLabelClass);
            _pageLabel.style.marginLeft = 12f;
            _pageLabel.style.marginRight = 12f;
            navigation.Add(_pageLabel);

            _nextButton = new Button(() => NextPage()) { text = "Next" };
            _nextButton.AddToClassList(NextButtonClass);
            navigation.Add(_nextButton);

            navigation.style.display = _showNavigation && _panels.Count > 1
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            _generatedRoot.Add(navigation);
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
            if (_generatedRoot != null)
            {
                _generatedRoot.RemoveFromHierarchy();
            }

            _generatedRoot = null;
            _previousButton = null;
            _nextButton = null;
            _pageLabel = null;
            _catalogVisibilityButton = null;
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
            if (notify)
            {
                _onPopupClosed.Invoke();
                PopupClosed?.Invoke();
            }
        }
    }
}
