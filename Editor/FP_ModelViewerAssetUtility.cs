namespace FuzzPhyte.ModelViewer.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using FuzzPhyte.Placement.OrbitalCamera;
    using FuzzPhyte.Utility.Meta;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;

    public static class FP_ModelViewerAssetUtility
    {
        public static bool TryGetCaptureBinding(
            GameObject source,
            out FP_ModelDisplayBinding binding,
            out string validationMessage)
        {
            binding = null;
            if (source == null)
            {
                validationMessage = "Source is missing.";
                return false;
            }

            binding = source.GetComponentInChildren<FP_ModelDisplayBinding>(true);
            if (binding == null)
            {
                validationMessage = "No FP_ModelDisplayBinding was found.";
                return false;
            }

            if (binding.Data == null)
            {
                validationMessage =
                    "FP_ModelDisplayBinding must reference FP_ModelDisplayData so presentation and camera framing are data-driven.";
                return false;
            }

            validationMessage = string.Empty;
            return true;
        }

        public static bool TryCalculateLocalRendererBounds(
            GameObject modelRoot,
            out Bounds localBounds,
            out string validationMessage)
        {
            localBounds = default;
            if (modelRoot == null)
            {
                validationMessage = "Model root is missing.";
                return false;
            }

            Renderer[] renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
            bool hasPoint = false;
            Matrix4x4 worldToRoot = modelRoot.transform.worldToLocalMatrix;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                Bounds rendererBounds = renderer.localBounds;
                Matrix4x4 rendererToRoot = worldToRoot * renderer.localToWorldMatrix;
                Vector3 center = rendererBounds.center;
                Vector3 extents = rendererBounds.extents;
                for (int x = -1; x <= 1; x += 2)
                {
                    for (int y = -1; y <= 1; y += 2)
                    {
                        for (int z = -1; z <= 1; z += 2)
                        {
                            Vector3 point = rendererToRoot.MultiplyPoint3x4(
                                center + Vector3.Scale(extents, new Vector3(x, y, z)));
                            if (!IsFinite(point))
                            {
                                continue;
                            }

                            if (!hasPoint)
                            {
                                localBounds = new Bounds(point, Vector3.zero);
                                hasPoint = true;
                            }
                            else
                            {
                                localBounds.Encapsulate(point);
                            }
                        }
                    }
                }
            }

            if (!hasPoint || localBounds.size.sqrMagnitude <= Mathf.Epsilon)
            {
                validationMessage =
                    "No usable Renderer bounds were found on the model root or its children.";
                return false;
            }

            validationMessage = string.Empty;
            return true;
        }

        public static bool TryCreateAndAssignDisplayData(
            GameObject source,
            string assetFolderPath,
            out FP_ModelDisplayData displayData,
            out string resultMessage)
        {
            displayData = null;
            if (source == null)
            {
                resultMessage = "Source is missing.";
                return false;
            }

            if (PrefabUtility.IsPartOfPrefabAsset(source))
            {
                return TryConfigurePrefabAsset(
                    source,
                    assetFolderPath,
                    out displayData,
                    out resultMessage);
            }

            FP_ModelDisplayBinding binding =
                source.GetComponentInChildren<FP_ModelDisplayBinding>(true);
            if (binding != null && binding.Data != null)
            {
                displayData = binding.Data;
                resultMessage = "The source already has assigned display data.";
                return true;
            }

            GameObject bindingRoot = binding != null ? binding.gameObject : source;
            if (!TryCalculateLocalRendererBounds(
                bindingRoot,
                out Bounds localBounds,
                out resultMessage))
            {
                return false;
            }

            displayData = CreateDisplayDataAsset(source.name, localBounds, assetFolderPath);
            if (binding == null)
            {
                binding = Undo.AddComponent<FP_ModelDisplayBinding>(source);
            }

            Undo.RecordObject(binding, "Assign FP Model Display Data");
            AssignDisplayData(binding, displayData);
            if (PrefabUtility.IsPartOfPrefabInstance(binding))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(binding);
            }
            if (binding.gameObject.scene.IsValid() && binding.gameObject.scene.isLoaded)
            {
                EditorSceneManager.MarkSceneDirty(binding.gameObject.scene);
            }

            AssetDatabase.SaveAssets();
            resultMessage = $"Created and assigned {displayData.name}.";
            return true;
        }

        public static string SanitizeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Model";
            }

            char[] result = value.Trim().ToCharArray();
            for (int i = 0; i < result.Length; i++)
            {
                char character = result[i];
                if (!char.IsLetterOrDigit(character) && character != '-' && character != '_')
                {
                    result[i] = '_';
                }
            }

            return new string(result);
        }

        public static void EnsureAssetFolder(string assetFolderPath)
        {
            string normalized = NormalizeAssetPath(assetFolderPath).TrimEnd('/');
            if (normalized == "Assets")
            {
                return;
            }

            if (!normalized.StartsWith("Assets/", StringComparison.Ordinal))
            {
                throw new ArgumentException("Output folder must be inside Assets.", nameof(assetFolderPath));
            }

            string[] segments = normalized.Split('/');
            string current = segments[0];
            for (int i = 1; i < segments.Length; i++)
            {
                string next = $"{current}/{segments[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[i]);
                }

                current = next;
            }
        }

        public static FP_ModelViewerItemData CreateOrLoadItem(string assetPath)
        {
            string normalized = NormalizeAssetPath(assetPath);
            FP_ModelViewerItemData item =
                AssetDatabase.LoadAssetAtPath<FP_ModelViewerItemData>(normalized);
            if (item != null)
            {
                return item;
            }

            EnsureAssetFolder(Path.GetDirectoryName(normalized));
            item = ScriptableObject.CreateInstance<FP_ModelViewerItemData>();
            AssetDatabase.CreateAsset(item, normalized);
            return item;
        }

        public static FP_ModelViewerCatalogData CreateOrLoadCatalog(string assetPath)
        {
            string normalized = NormalizeAssetPath(assetPath);
            FP_ModelViewerCatalogData catalog =
                AssetDatabase.LoadAssetAtPath<FP_ModelViewerCatalogData>(normalized);
            if (catalog != null)
            {
                return catalog;
            }

            EnsureAssetFolder(Path.GetDirectoryName(normalized));
            catalog = ScriptableObject.CreateInstance<FP_ModelViewerCatalogData>();
            AssetDatabase.CreateAsset(catalog, normalized);
            return catalog;
        }

        public static GameObject ResolveOrCreateIncludedPrefab(
            GameObject source,
            string generatedPrefabPath)
        {
            if (source == null)
            {
                return null;
            }

            if (PrefabUtility.IsPartOfPrefabAsset(source))
            {
                return source;
            }

            GameObject existingPrefab =
                PrefabUtility.GetCorrespondingObjectFromSource(source);
            if (existingPrefab != null)
            {
                return existingPrefab;
            }

            string normalizedPath = NormalizeAssetPath(generatedPrefabPath);
            if (!normalizedPath.StartsWith("Assets/", StringComparison.Ordinal) ||
                !normalizedPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Generated prefab path must be a .prefab asset inside Assets.",
                    nameof(generatedPrefabPath));
            }

            EnsureAssetFolder(Path.GetDirectoryName(normalizedPath));
            GameObject generatedPrefab = PrefabUtility.SaveAsPrefabAsset(source, normalizedPath);
            if (generatedPrefab == null)
            {
                throw new InvalidOperationException(
                    $"Unity could not save the generated viewer prefab at {normalizedPath}.");
            }

            return generatedPrefab;
        }

        public static void ConfigureItem(
            FP_ModelViewerItemData item,
            string displayName,
            FP_ModelDisplayData displayData,
            GameObject includedPrefab)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (string.IsNullOrWhiteSpace(item.UniqueID))
            {
                item.UniqueID = Guid.NewGuid().ToString();
            }

            var serializedItem = new SerializedObject(item);
            serializedItem.FindProperty("_displayName").stringValue = displayName;
            serializedItem.FindProperty("_modelDisplayData").objectReferenceValue = displayData;
            serializedItem.FindProperty("_includedPrefab").objectReferenceValue = includedPrefab;
            serializedItem.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
        }

        public static void SetItemTags(
            FP_ModelViewerItemData item,
            IReadOnlyList<FP_Tag> tags)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var serializedItem = new SerializedObject(item);
            SerializedProperty tagList = serializedItem.FindProperty("_tags");
            tagList.arraySize = 0;

            var uniqueTags = new HashSet<FP_Tag>();
            int count = tags?.Count ?? 0;
            for (int i = 0; i < count; i++)
            {
                FP_Tag tag = tags[i];
                if (tag == null || !uniqueTags.Add(tag))
                {
                    continue;
                }

                int tagIndex = tagList.arraySize;
                tagList.InsertArrayElementAtIndex(tagIndex);
                tagList.GetArrayElementAtIndex(tagIndex).objectReferenceValue = tag;
            }

            serializedItem.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
        }

        /// <summary>
        /// Enables Read/Write on every imported model asset referenced by mesh components
        /// beneath the supplied root. Meshes without a ModelImporter must already be readable.
        /// </summary>
        public static bool EnsureMeshReadWriteEnabled(
            GameObject modelRoot,
            out int updatedImporterCount,
            out string resultMessage)
        {
            updatedImporterCount = 0;
            if (modelRoot == null)
            {
                resultMessage = "Model root is missing.";
                return false;
            }

            var meshes = new HashSet<Mesh>();
            MeshFilter[] meshFilters = modelRoot.GetComponentsInChildren<MeshFilter>(true);
            for (int i = 0; i < meshFilters.Length; i++)
            {
                if (meshFilters[i] != null && meshFilters[i].sharedMesh != null)
                {
                    meshes.Add(meshFilters[i].sharedMesh);
                }
            }

            SkinnedMeshRenderer[] skinnedRenderers =
                modelRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < skinnedRenderers.Length; i++)
            {
                if (skinnedRenderers[i] != null && skinnedRenderers[i].sharedMesh != null)
                {
                    meshes.Add(skinnedRenderers[i].sharedMesh);
                }
            }

            MeshCollider[] meshColliders = modelRoot.GetComponentsInChildren<MeshCollider>(true);
            for (int i = 0; i < meshColliders.Length; i++)
            {
                if (meshColliders[i] != null && meshColliders[i].sharedMesh != null)
                {
                    meshes.Add(meshColliders[i].sharedMesh);
                }
            }

            var modelAssetPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var failures = new List<string>();
            foreach (Mesh mesh in meshes)
            {
                string assetPath = AssetDatabase.GetAssetPath(mesh);
                if (string.IsNullOrWhiteSpace(assetPath))
                {
                    if (!mesh.isReadable)
                    {
                        failures.Add(
                            $"Mesh '{mesh.name}' is not readable and has no importer asset path.");
                    }
                    continue;
                }

                if (AssetImporter.GetAtPath(assetPath) is ModelImporter)
                {
                    modelAssetPaths.Add(assetPath);
                }
                else if (!mesh.isReadable)
                {
                    failures.Add(
                        $"Mesh '{mesh.name}' at '{assetPath}' is not controlled by a ModelImporter.");
                }
            }

            foreach (string assetPath in modelAssetPaths)
            {
                if (!(AssetImporter.GetAtPath(assetPath) is ModelImporter importer) ||
                    importer.isReadable)
                {
                    continue;
                }

                try
                {
                    importer.isReadable = true;
                    importer.SaveAndReimport();
                    ModelImporter refreshedImporter =
                        AssetImporter.GetAtPath(assetPath) as ModelImporter;
                    if (refreshedImporter == null || !refreshedImporter.isReadable)
                    {
                        failures.Add($"Unity did not retain Read/Write for '{assetPath}'.");
                        continue;
                    }

                    updatedImporterCount++;
                }
                catch (Exception exception)
                {
                    failures.Add(
                        $"Could not enable Read/Write for '{assetPath}': {exception.Message}");
                }
            }

            if (failures.Count > 0)
            {
                resultMessage = string.Join("\n", failures);
                return false;
            }

            resultMessage = updatedImporterCount > 0
                ? $"Enabled Read/Write on {updatedImporterCount} imported model asset(s)."
                : $"All {meshes.Count} referenced mesh asset(s) are already runtime-readable.";
            return true;
        }

        /// <summary>
        /// Ensures every included prefab in a catalog exposes runtime-readable mesh data.
        /// </summary>
        public static bool EnsureCatalogMeshReadWriteEnabled(
            FP_ModelViewerCatalogData catalog,
            out int updatedImporterCount,
            out string resultMessage)
        {
            if (catalog == null)
            {
                updatedImporterCount = 0;
                resultMessage = "Catalog is missing.";
                return false;
            }

            return EnsureItemMeshReadWriteEnabled(
                catalog.Items,
                out updatedImporterCount,
                out resultMessage);
        }

        public static void SetCatalogItems(
            FP_ModelViewerCatalogData catalog,
            IReadOnlyList<FP_ModelViewerItemData> items,
            string displayName)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            if (!EnsureItemMeshReadWriteEnabled(
                    items,
                    out int updatedImporterCount,
                    out string readWriteMessage))
            {
                throw new InvalidOperationException(
                    $"Catalog mesh Read/Write preparation failed:\n{readWriteMessage}");
            }
            if (updatedImporterCount > 0)
            {
                Debug.Log($"[FP Model Viewer] {readWriteMessage}", catalog);
            }

            if (string.IsNullOrWhiteSpace(catalog.UniqueID))
            {
                catalog.UniqueID = Guid.NewGuid().ToString();
            }

            var serializedCatalog = new SerializedObject(catalog);
            serializedCatalog.FindProperty("_displayName").stringValue = displayName;

            SerializedProperty itemList = serializedCatalog.FindProperty("_items");
            int count = items?.Count ?? 0;
            itemList.arraySize = count;
            for (int i = 0; i < count; i++)
            {
                itemList.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
            }

            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
        }

        private static bool EnsureItemMeshReadWriteEnabled(
            IReadOnlyList<FP_ModelViewerItemData> items,
            out int updatedImporterCount,
            out string resultMessage)
        {
            updatedImporterCount = 0;
            var failures = new List<string>();
            int count = items?.Count ?? 0;
            for (int i = 0; i < count; i++)
            {
                FP_ModelViewerItemData item = items[i];
                if (item == null || item.IncludedPrefab == null)
                {
                    continue;
                }

                if (EnsureMeshReadWriteEnabled(
                    item.IncludedPrefab,
                    out int itemUpdateCount,
                    out string itemMessage))
                {
                    updatedImporterCount += itemUpdateCount;
                    continue;
                }

                failures.Add($"{item.DisplayName}: {itemMessage}");
            }

            if (failures.Count > 0)
            {
                resultMessage = string.Join("\n", failures);
                return false;
            }

            resultMessage = updatedImporterCount > 0
                ? $"Enabled Read/Write on {updatedImporterCount} imported model asset(s)."
                : "All catalog meshes are already runtime-readable.";
            return true;
        }

        public static Texture2D SaveThumbnail(
            Texture2D source,
            string assetPath,
            bool alphaIsTransparency)
        {
            if (source == null)
            {
                return null;
            }

            string normalized = NormalizeAssetPath(assetPath);
            EnsureAssetFolder(Path.GetDirectoryName(normalized));

            string relativeToAssets = normalized.Substring("Assets/".Length);
            string absolutePath = Path.Combine(Application.dataPath, relativeToAssets);
            File.WriteAllBytes(absolutePath, source.EncodeToPNG());

            AssetDatabase.ImportAsset(
                normalized,
                ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            if (AssetImporter.GetAtPath(normalized) is TextureImporter importer)
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = alphaIsTransparency;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.textureCompression = TextureImporterCompression.Compressed;
                importer.maxTextureSize = Mathf.Clamp(
                    Mathf.NextPowerOfTwo(Mathf.Max(source.width, source.height)),
                    32,
                    8192);
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(normalized);
        }

        private static string NormalizeAssetPath(string path)
        {
            return string.IsNullOrWhiteSpace(path) ? string.Empty : path.Replace('\\', '/');
        }

        private static bool TryConfigurePrefabAsset(
            GameObject source,
            string assetFolderPath,
            out FP_ModelDisplayData displayData,
            out string resultMessage)
        {
            displayData = null;
            if (PrefabUtility.IsPartOfImmutablePrefab(source))
            {
                resultMessage =
                    "Imported model prefabs are immutable. Create an editable wrapper prefab before generating display data.";
                return false;
            }

            string prefabPath = AssetDatabase.GetAssetPath(source);
            if (string.IsNullOrWhiteSpace(prefabPath))
            {
                resultMessage = "Could not resolve the prefab asset path.";
                return false;
            }

            GameObject prefabRoot = null;
            string createdDataPath = string.Empty;
            try
            {
                prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
                FP_ModelDisplayBinding binding =
                    prefabRoot.GetComponentInChildren<FP_ModelDisplayBinding>(true);
                if (binding != null && binding.Data != null)
                {
                    displayData = binding.Data;
                    resultMessage = "The prefab already has assigned display data.";
                    return true;
                }

                GameObject bindingRoot = binding != null ? binding.gameObject : prefabRoot;
                if (!TryCalculateLocalRendererBounds(
                    bindingRoot,
                    out Bounds localBounds,
                    out resultMessage))
                {
                    return false;
                }

                displayData = CreateDisplayDataAsset(
                    source.name,
                    localBounds,
                    assetFolderPath);
                createdDataPath = AssetDatabase.GetAssetPath(displayData);
                if (binding == null)
                {
                    binding = prefabRoot.AddComponent<FP_ModelDisplayBinding>();
                }

                AssignDisplayData(binding, displayData);
                if (PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath) == null)
                {
                    throw new InvalidOperationException("Unity could not save the configured prefab.");
                }

                AssetDatabase.SaveAssets();
                resultMessage = $"Created and assigned {displayData.name}.";
                return true;
            }
            catch (Exception exception)
            {
                if (!string.IsNullOrWhiteSpace(createdDataPath))
                {
                    AssetDatabase.DeleteAsset(createdDataPath);
                }

                displayData = null;
                resultMessage = exception.Message;
                return false;
            }
            finally
            {
                if (prefabRoot != null)
                {
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                }
            }
        }

        private static FP_ModelDisplayData CreateDisplayDataAsset(
            string displayName,
            Bounds localBounds,
            string assetFolderPath)
        {
            string normalizedFolder = NormalizeAssetPath(assetFolderPath).TrimEnd('/');
            EnsureAssetFolder(normalizedFolder);
            string safeName = SanitizeFileName(displayName);
            string assetPath = AssetDatabase.GenerateUniqueAssetPath(
                $"{normalizedFolder}/{safeName}_ModelDisplayData.asset");

            FP_ModelDisplayData displayData =
                ScriptableObject.CreateInstance<FP_ModelDisplayData>();
            ConfigureGeneratedDisplayData(displayData, displayName, localBounds);
            AssetDatabase.CreateAsset(displayData, assetPath);
            return displayData;
        }

        private static void ConfigureGeneratedDisplayData(
            FP_ModelDisplayData displayData,
            string displayName,
            Bounds localBounds)
        {
            displayData.DisplayName = displayName;
            displayData.UseLocalBoundsOverride = true;
            displayData.BoundsCenter = localBounds.center;
            displayData.BoundsSize = localBounds.size;
            displayData.LocalPivotOffset = -localBounds.center;
            EditorUtility.SetDirty(displayData);
        }

        private static void AssignDisplayData(
            FP_ModelDisplayBinding binding,
            FP_ModelDisplayData displayData)
        {
            var serializedBinding = new SerializedObject(binding);
            serializedBinding.FindProperty("_data").objectReferenceValue = displayData;
            serializedBinding.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(binding);
        }

        private static bool IsFinite(Vector3 value)
        {
            return !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
                !float.IsNaN(value.y) && !float.IsInfinity(value.y) &&
                !float.IsNaN(value.z) && !float.IsInfinity(value.z);
        }
    }
}
