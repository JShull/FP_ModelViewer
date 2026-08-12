namespace FuzzPhyte.ModelViewer.Editor
{
    using System;
    using System.Collections.Generic;
    using FuzzPhyte.Placement.OrbitalCamera;
    using FuzzPhyte.Utility.Meta;
    using UnityEditor;
    using UnityEngine;

    public sealed class FP_ModelViewerBuilderWindow : EditorWindow
    {
        private const int CaptureLayer = 31;
        private const int CaptureLayerMask = 1 << CaptureLayer;

        [Serializable]
        private sealed class SourceRow
        {
            [SerializeField] private GameObject _source;
            [SerializeField] private FP_Tag _tag1;
            [SerializeField] private FP_Tag _tag2;
            [SerializeField] private FP_Tag _tag3;

            public SourceRow(GameObject source = null)
            {
                _source = source;
            }

            public GameObject Source
            {
                get => _source;
                set => _source = value;
            }

            public IReadOnlyList<FP_Tag> Tags => new[] { _tag1, _tag2, _tag3 };

            public FP_Tag GetTag(int index)
            {
                switch (index)
                {
                    case 0:
                        return _tag1;
                    case 1:
                        return _tag2;
                    case 2:
                        return _tag3;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(index));
                }
            }

            public void SetTag(int index, FP_Tag tag)
            {
                switch (index)
                {
                    case 0:
                        _tag1 = tag;
                        break;
                    case 1:
                        _tag2 = tag;
                        break;
                    case 2:
                        _tag3 = tag;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }

        // Retained only so open windows can migrate their serialized pre-table source list.
        [SerializeField] private List<GameObject> _sources = new List<GameObject>();
        [SerializeField] private List<SourceRow> _sourceRows = new List<SourceRow>();
        [SerializeField] private bool _sourcesExpanded = true;
        [SerializeField] private Camera _captureCamera;
        [SerializeField] private FP_ModelCaptureProfile _captureProfile;
        [SerializeField] private string _outputAssetPath = "Assets/Generated/FP_ModelViewer";
        [SerializeField] private string _catalogName = "Model Viewer Catalog";
        [SerializeField] private int _viewMask = (1 << 14) - 1;
        [SerializeField] private int _coverViewIndex = 6;

        private Vector2 _scrollPosition;

        [MenuItem("FuzzPhyte/Model Viewer/Builder")]
        private static void OpenWindow()
        {
            GetWindow<FP_ModelViewerBuilderWindow>("FP Model Viewer");
        }

        private void OnEnable()
        {
            MigrateLegacySources();
        }

        private void OnGUI()
        {
            MigrateLegacySources();
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.LabelField("FP Model Viewer Builder", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Capture scene objects or prefabs that contain FP_ModelDisplayBinding. " +
                "Generated images and data assets are written outside the package.",
                MessageType.Info);

            DrawOutputSettings();
            EditorGUILayout.Space();
            DrawCaptureSettings();
            EditorGUILayout.Space();
            DrawViewSettings();
            EditorGUILayout.Space();
            DrawSources();
            EditorGUILayout.Space();
            DrawGenerateButton();

            EditorGUILayout.EndScrollView();
        }

        private void DrawOutputSettings()
        {
            EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
            _catalogName = EditorGUILayout.TextField("Catalog Name", _catalogName);

            EditorGUILayout.BeginHorizontal();
            _outputAssetPath = EditorGUILayout.TextField("Output Folder", _outputAssetPath);
            if (GUILayout.Button("Browse...", GUILayout.Width(76f)))
            {
                BrowseForOutputFolder();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCaptureSettings()
        {
            EditorGUILayout.LabelField("Capture", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _captureProfile = (FP_ModelCaptureProfile)EditorGUILayout.ObjectField(
                "Capture Profile",
                _captureProfile,
                typeof(FP_ModelCaptureProfile),
                false);
            if (GUILayout.Button("Create", GUILayout.Width(64f)))
            {
                CreateCaptureProfile();
            }
            EditorGUILayout.EndHorizontal();

            _captureCamera = (Camera)EditorGUILayout.ObjectField(
                new GUIContent(
                    "Capture Camera",
                    "Optional. A temporary camera is created when this is empty."),
                _captureCamera,
                typeof(Camera),
                true);

            if (_captureProfile != null)
            {
                EditorGUILayout.LabelField("Lighting Mode", _captureProfile.LightingMode.ToString());
                EditorGUILayout.LabelField("Capture Space", _captureProfile.CaptureSpace.ToString());
                if (_captureProfile.LightingMode == FP_ModelCaptureLightingMode.SceneLighting &&
                    _captureProfile.CaptureSpace == FP_ModelCaptureSpace.IsolatedAtOrigin)
                {
                    EditorGUILayout.HelpBox(
                        "Scene point and spot lights may not align with a model moved to the " +
                        "capture origin. Use Preserve Scene Position for spatial scene lighting.",
                        MessageType.Warning);
                }
            }
        }

        private void DrawViewSettings()
        {
            EditorGUILayout.LabelField("Thumbnail Views", EditorStyles.boldLabel);
            IReadOnlyList<FP_ViewCubeHit> views =
                FP_ModelViewerViewUtility.SupportedThumbnailViews;

            int half = (views.Count + 1) / 2;
            for (int row = 0; row < half; row++)
            {
                EditorGUILayout.BeginHorizontal();
                DrawViewToggle(views, row);
                DrawViewToggle(views, row + half);
                EditorGUILayout.EndHorizontal();
            }

            string[] labels = new string[views.Count];
            for (int i = 0; i < views.Count; i++)
            {
                labels[i] = views[i].ToString();
            }

            _coverViewIndex = EditorGUILayout.Popup(
                "Cover View",
                Mathf.Clamp(_coverViewIndex, 0, views.Count - 1),
                labels);

            if (!IsViewSelected(_coverViewIndex))
            {
                EditorGUILayout.HelpBox(
                    "The cover view must also be enabled for capture.",
                    MessageType.Warning);
            }
        }

        private void DrawViewToggle(IReadOnlyList<FP_ViewCubeHit> views, int index)
        {
            if (index >= views.Count)
            {
                GUILayout.FlexibleSpace();
                return;
            }

            bool selected = IsViewSelected(index);
            bool updated = EditorGUILayout.ToggleLeft(
                ObjectNames.NicifyVariableName(views[index].ToString()),
                selected,
                GUILayout.MinWidth(180f));
            if (selected == updated)
            {
                return;
            }

            if (updated)
            {
                _viewMask |= 1 << index;
            }
            else
            {
                _viewMask &= ~(1 << index);
            }
        }

        private void DrawSources()
        {
            EditorGUILayout.BeginHorizontal();
            _sourcesExpanded = EditorGUILayout.Foldout(
                _sourcesExpanded,
                $"Sources ({_sourceRows.Count})",
                true,
                EditorStyles.foldoutHeader);
            if (GUILayout.Button("Add Selection", GUILayout.Width(105f)))
            {
                AddSelectedSources();
            }
            if (GUILayout.Button("Add Slot", GUILayout.Width(75f)))
            {
                _sourceRows.Add(new SourceRow());
            }
            using (new EditorGUI.DisabledScope(
                string.IsNullOrWhiteSpace(_outputAssetPath) ||
                !IsAssetFolderPath(_outputAssetPath)))
            {
                if (GUILayout.Button("Auto Setup Missing", GUILayout.Width(130f)))
                {
                    AutoSetupMissingSources();
                }
            }
            if (GUILayout.Button("Clear", GUILayout.Width(55f)))
            {
                _sourceRows.Clear();
            }
            EditorGUILayout.EndHorizontal();

            if (!_sourcesExpanded)
            {
                return;
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("#", GUILayout.Width(24f));
            GUILayout.Label("Source", GUILayout.MinWidth(180f));
            GUILayout.Label("Tag 1", GUILayout.MinWidth(100f));
            GUILayout.Label("Tag 2", GUILayout.MinWidth(100f));
            GUILayout.Label("Tag 3", GUILayout.MinWidth(100f));
            GUILayout.Space(24f);
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < _sourceRows.Count; i++)
            {
                SourceRow row = _sourceRows[i] ?? new SourceRow();
                _sourceRows[i] = row;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label((i + 1).ToString(), GUILayout.Width(24f));
                row.Source = (GameObject)EditorGUILayout.ObjectField(
                    GUIContent.none,
                    row.Source,
                    typeof(GameObject),
                    true,
                    GUILayout.MinWidth(180f));
                for (int tagIndex = 0; tagIndex < 3; tagIndex++)
                {
                    row.SetTag(
                        tagIndex,
                        (FP_Tag)EditorGUILayout.ObjectField(
                            GUIContent.none,
                            row.GetTag(tagIndex),
                            typeof(FP_Tag),
                            false,
                            GUILayout.MinWidth(100f)));
                }

                bool removeSource = false;
                if (GUILayout.Button("-", GUILayout.Width(24f)))
                {
                    removeSource = true;
                }
                EditorGUILayout.EndHorizontal();

                if (removeSource)
                {
                    _sourceRows.RemoveAt(i);
                    i--;
                    continue;
                }

                GameObject source = row.Source;
                if (source != null &&
                    !FP_ModelViewerAssetUtility.TryGetCaptureBinding(
                        source,
                        out _,
                        out string validationMessage))
                {
                    EditorGUILayout.HelpBox(
                        $"{source.name}: {validationMessage}",
                        MessageType.Warning);
                    if (GUILayout.Button($"Generate and Assign Display Data for {source.name}"))
                    {
                        AutoSetupSource(source, true);
                    }
                }
            }
        }

        private void AutoSetupMissingSources()
        {
            int configuredCount = 0;
            int failedCount = 0;
            for (int i = 0; i < _sourceRows.Count; i++)
            {
                GameObject source = _sourceRows[i]?.Source;
                if (source == null ||
                    FP_ModelViewerAssetUtility.TryGetCaptureBinding(source, out _, out _))
                {
                    continue;
                }

                if (AutoSetupSource(source, false))
                {
                    configuredCount++;
                }
                else
                {
                    failedCount++;
                }
            }

            Debug.Log(
                $"[FP Model Viewer] Auto setup configured {configuredCount} source(s)" +
                (failedCount > 0 ? $"; {failedCount} failed." : "."));
        }

        private bool AutoSetupSource(GameObject source, bool pingCreatedAsset)
        {
            string displayDataFolder = $"{NormalizeOutputPath()}/DisplayData";
            bool succeeded = FP_ModelViewerAssetUtility.TryCreateAndAssignDisplayData(
                source,
                displayDataFolder,
                out FP_ModelDisplayData displayData,
                out string resultMessage);
            if (!succeeded)
            {
                Debug.LogWarning(
                    $"[FP Model Viewer] Could not configure {source.name}: {resultMessage}",
                    source);
                if (pingCreatedAsset)
                {
                    EditorUtility.DisplayDialog(
                        "Model Display Data Generation Failed",
                        resultMessage,
                        "OK");
                }

                return false;
            }

            if (pingCreatedAsset && displayData != null)
            {
                Selection.activeObject = displayData;
                EditorGUIUtility.PingObject(displayData);
            }

            Debug.Log($"[FP Model Viewer] {source.name}: {resultMessage}", source);
            Repaint();
            return true;
        }

        private void DrawGenerateButton()
        {
            string validationMessage = GetValidationMessage();
            if (!string.IsNullOrEmpty(validationMessage))
            {
                EditorGUILayout.HelpBox(validationMessage, MessageType.Error);
            }

            using (new EditorGUI.DisabledScope(!string.IsNullOrEmpty(validationMessage)))
            {
                if (GUILayout.Button("Generate Catalog and Thumbnails", GUILayout.Height(32f)))
                {
                    Generate();
                }
            }
        }

        private string GetValidationMessage()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return "Exit Play Mode before generating model viewer assets.";
            }
            if (_captureProfile == null)
            {
                return "Assign or create a capture profile.";
            }
            if (_sourceRows.Count == 0 ||
                !_sourceRows.Exists(row => row != null && row.Source != null))
            {
                return "Add at least one scene object or prefab source.";
            }
            for (int i = 0; i < _sourceRows.Count; i++)
            {
                GameObject source = _sourceRows[i]?.Source;
                if (source != null &&
                    !FP_ModelViewerAssetUtility.TryGetCaptureBinding(
                        source,
                        out _,
                        out string sourceValidationMessage))
                {
                    return $"{source.name} is not capture-ready: " +
                        $"{sourceValidationMessage} Use Auto Setup Missing to configure it.";
                }
            }
            if (string.IsNullOrWhiteSpace(_outputAssetPath) ||
                !IsAssetFolderPath(_outputAssetPath))
            {
                return "The output path must be a folder inside Assets.";
            }
            if (_viewMask == 0)
            {
                return "Enable at least one thumbnail view.";
            }
            if (!IsViewSelected(_coverViewIndex))
            {
                return "Enable the selected cover view.";
            }

            return string.Empty;
        }

        private void BrowseForOutputFolder()
        {
            string assetsAbsolutePath = Application.dataPath.Replace('\\', '/');
            string currentAssetPath = NormalizeOutputPath();
            string initialFolder = assetsAbsolutePath;
            if (currentAssetPath.StartsWith("Assets/", StringComparison.Ordinal))
            {
                string relativePath = currentAssetPath.Substring("Assets/".Length);
                string candidate = $"{assetsAbsolutePath}/{relativePath}";
                if (System.IO.Directory.Exists(candidate))
                {
                    initialFolder = candidate;
                }
            }

            string selectedFolder = EditorUtility.OpenFolderPanel(
                "Select Model Viewer Output Folder",
                initialFolder,
                string.Empty);
            if (string.IsNullOrWhiteSpace(selectedFolder))
            {
                return;
            }

            string normalized = selectedFolder.Replace('\\', '/').TrimEnd('/');
            bool isAssetsRoot = string.Equals(
                normalized,
                assetsAbsolutePath,
                StringComparison.OrdinalIgnoreCase);
            bool isInsideAssets = normalized.StartsWith(
                $"{assetsAbsolutePath}/",
                StringComparison.OrdinalIgnoreCase);
            if (!isAssetsRoot && !isInsideAssets)
            {
                EditorUtility.DisplayDialog(
                    "Invalid Output Folder",
                    "Model Viewer output must be stored inside this project's Assets folder.",
                    "OK");
                return;
            }

            _outputAssetPath = isAssetsRoot
                ? "Assets"
                : $"Assets{normalized.Substring(assetsAbsolutePath.Length)}";
            GUI.FocusControl(null);
        }

        private void AddSelectedSources()
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            for (int i = 0; i < selectedObjects.Length; i++)
            {
                GameObject selected = selectedObjects[i];
                if (selected != null &&
                    !_sourceRows.Exists(row => row != null && row.Source == selected))
                {
                    _sourceRows.Add(new SourceRow(selected));
                }
            }
        }

        private void MigrateLegacySources()
        {
            if (_sources == null)
            {
                _sources = new List<GameObject>();
            }
            if (_sourceRows == null)
            {
                _sourceRows = new List<SourceRow>();
            }

            for (int i = 0; i < _sources.Count; i++)
            {
                GameObject source = _sources[i];
                if (!_sourceRows.Exists(row => row != null && row.Source == source))
                {
                    _sourceRows.Add(new SourceRow(source));
                }
            }

            _sources.Clear();
        }

        private void CreateCaptureProfile()
        {
            try
            {
                FP_ModelViewerAssetUtility.EnsureAssetFolder(_outputAssetPath);
                string path = AssetDatabase.GenerateUniqueAssetPath(
                    $"{NormalizeOutputPath()}/FP_ModelCaptureProfile.asset");
                _captureProfile = CreateInstance<FP_ModelCaptureProfile>();
                AssetDatabase.CreateAsset(_captureProfile, path);
                AssetDatabase.SaveAssets();
                EditorGUIUtility.PingObject(_captureProfile);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private void Generate()
        {
            FP_ModelViewerAssetUtility.EnsureAssetFolder(_outputAssetPath);

            GameObject temporaryCameraObject = null;
            Camera camera = _captureCamera;
            if (camera == null)
            {
                temporaryCameraObject = new GameObject("FP Model Viewer Capture Camera")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                camera = temporaryCameraObject.AddComponent<Camera>();
                camera.enabled = false;
            }

            var generatedItems = new List<FP_ModelViewerItemData>();
            int validSourceCount = 0;
            for (int i = 0; i < _sourceRows.Count; i++)
            {
                if (_sourceRows[i]?.Source != null)
                {
                    validSourceCount++;
                }
            }

            int processed = 0;
            try
            {
                for (int i = 0; i < _sourceRows.Count; i++)
                {
                    SourceRow row = _sourceRows[i];
                    GameObject source = row?.Source;
                    if (source == null)
                    {
                        continue;
                    }

                    if (EditorUtility.DisplayCancelableProgressBar(
                        "FP Model Viewer",
                        $"Capturing {source.name}",
                        validSourceCount == 0 ? 0f : (float)processed / validSourceCount))
                    {
                        break;
                    }

                    try
                    {
                        FP_ModelViewerItemData item = GenerateItem(row, camera, processed);
                        if (item != null)
                        {
                            generatedItems.Add(item);
                        }
                    }
                    catch (Exception exception)
                    {
                        Debug.LogException(exception, source);
                    }

                    processed++;
                }

                if (generatedItems.Count > 0)
                {
                    string safeCatalogName =
                        FP_ModelViewerAssetUtility.SanitizeFileName(_catalogName);
                    string catalogPath = $"{NormalizeOutputPath()}/{safeCatalogName}.asset";
                    FP_ModelViewerCatalogData catalog =
                        FP_ModelViewerAssetUtility.CreateOrLoadCatalog(catalogPath);
                    FP_ModelViewerAssetUtility.SetCatalogItems(
                        catalog,
                        generatedItems,
                        _catalogName);
                    AssetDatabase.SaveAssets();
                    Selection.activeObject = catalog;
                    EditorGUIUtility.PingObject(catalog);
                    Debug.Log(
                        $"[FP Model Viewer] Generated {generatedItems.Count} item(s) at " +
                        $"{NormalizeOutputPath()}.");
                }
                else
                {
                    Debug.LogWarning(
                        "[FP Model Viewer] No valid sources were generated; the existing catalog was left unchanged.");
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                if (temporaryCameraObject != null)
                {
                    DestroyImmediate(temporaryCameraObject);
                }
            }
        }

        private FP_ModelViewerItemData GenerateItem(
            SourceRow row,
            Camera camera,
            int outputIndex)
        {
            GameObject source = row.Source;
            if (!FP_ModelViewerAssetUtility.TryGetCaptureBinding(
                source,
                out FP_ModelDisplayBinding sourceBinding,
                out string validationMessage))
            {
                Debug.LogWarning(
                    $"[FP Model Viewer] Skipping {source.name}: {validationMessage}",
                    source);
                return null;
            }

            bool captureSceneSourceInPlace =
                _captureProfile.CaptureSpace == FP_ModelCaptureSpace.PreserveScenePosition &&
                !EditorUtility.IsPersistent(source);
            GameObject captureObject = captureSceneSourceInPlace
                ? source
                : InstantiateSource(source);
            GameObject pivotObject = null;

            try
            {
                if (!captureSceneSourceInPlace)
                {
                    captureObject.hideFlags = HideFlags.HideAndDontSave;
                    captureObject.SetActive(true);
                }

                FP_ModelDisplayBinding captureBinding =
                    captureObject.GetComponentInChildren<FP_ModelDisplayBinding>(true);
                if (captureBinding == null || captureBinding.Data == null)
                {
                    return null;
                }

                int? cullingMaskOverride = null;
                if (_captureProfile.CaptureSpace == FP_ModelCaptureSpace.IsolatedAtOrigin)
                {
                    pivotObject = new GameObject("FP Model Viewer Capture Pivot")
                    {
                        hideFlags = HideFlags.HideAndDontSave
                    };
                    SetLayerRecursively(captureObject.transform, CaptureLayer);
                    captureBinding.ApplyPresentationDefaults(pivotObject.transform);
                    cullingMaskOverride = CaptureLayerMask;
                }

                string safeName = FP_ModelViewerAssetUtility.SanitizeFileName(source.name);
                string itemFolder =
                    $"{NormalizeOutputPath()}/{outputIndex + 1:000}_{safeName}";
                string imageFolder = $"{itemFolder}/Thumbnails";
                FP_ModelViewerAssetUtility.EnsureAssetFolder(imageFolder);

                string itemPath = $"{itemFolder}/{safeName}_ViewerItem.asset";
                FP_ModelViewerItemData item =
                    FP_ModelViewerAssetUtility.CreateOrLoadItem(itemPath);
                GameObject includedPrefab =
                    FP_ModelViewerAssetUtility.ResolveOrCreateIncludedPrefab(
                        source,
                        $"{itemFolder}/{safeName}_ViewerPrefab.prefab");
                FP_ModelViewerAssetUtility.ConfigureItem(
                    item,
                    source.name,
                    sourceBinding.Data,
                    includedPrefab);
                FP_ModelViewerAssetUtility.SetItemTags(item, row.Tags);

                IReadOnlyList<FP_ViewCubeHit> views =
                    FP_ModelViewerViewUtility.SupportedThumbnailViews;
                for (int i = 0; i < views.Count; i++)
                {
                    FP_ViewCubeHit view = views[i];
                    if (!IsViewSelected(i))
                    {
                        item.RemoveThumbnail(view);
                        continue;
                    }

                    if (!FP_ModelThumbnailCaptureUtility.TryCapture(
                        camera,
                        captureBinding,
                        _captureProfile,
                        view,
                        out Texture2D capturedTexture,
                        captureBinding.transform,
                        cullingMaskOverride))
                    {
                        Debug.LogWarning(
                            $"[FP Model Viewer] Could not capture {view} for {source.name}.",
                            source);
                        continue;
                    }

                    try
                    {
                        string imagePath = $"{imageFolder}/{safeName}_{view}.png";
                        Texture2D savedTexture = FP_ModelViewerAssetUtility.SaveThumbnail(
                            capturedTexture,
                            imagePath,
                            _captureProfile.BackgroundMode ==
                                FP_ModelCaptureBackgroundMode.Transparent);
                        item.SetThumbnail(view, savedTexture, ObjectNames.NicifyVariableName(view.ToString()));
                    }
                    finally
                    {
                        DestroyImmediate(capturedTexture);
                    }
                }

                item.SetCoverView(views[_coverViewIndex]);
                EditorUtility.SetDirty(item);
                return item;
            }
            finally
            {
                if (pivotObject != null)
                {
                    DestroyImmediate(pivotObject);
                }
                if (!captureSceneSourceInPlace && captureObject != null)
                {
                    DestroyImmediate(captureObject);
                }
            }
        }

        private static GameObject InstantiateSource(GameObject source)
        {
            if (PrefabUtility.IsPartOfPrefabAsset(source))
            {
                GameObject prefabInstance = PrefabUtility.InstantiatePrefab(source) as GameObject;
                if (prefabInstance != null)
                {
                    return prefabInstance;
                }
            }

            return Instantiate(source);
        }

        private static void SetLayerRecursively(Transform root, int layer)
        {
            root.gameObject.layer = layer;
            for (int i = 0; i < root.childCount; i++)
            {
                SetLayerRecursively(root.GetChild(i), layer);
            }
        }

        private bool IsViewSelected(int index)
        {
            return index >= 0 && index < 14 && (_viewMask & (1 << index)) != 0;
        }

        private string NormalizeOutputPath()
        {
            return string.IsNullOrWhiteSpace(_outputAssetPath)
                ? "Assets"
                : _outputAssetPath.Replace('\\', '/').TrimEnd('/');
        }

        private static bool IsAssetFolderPath(string path)
        {
            string normalized = path.Replace('\\', '/').TrimEnd('/');
            return normalized == "Assets" ||
                normalized.StartsWith("Assets/", StringComparison.Ordinal);
        }
    }
}
