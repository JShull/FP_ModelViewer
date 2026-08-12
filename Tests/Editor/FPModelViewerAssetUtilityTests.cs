namespace FuzzPhyte.ModelViewer.Tests
{
    using NUnit.Framework;
    using FuzzPhyte.ModelViewer.Editor;
    using FuzzPhyte.Placement.OrbitalCamera;
    using FuzzPhyte.Utility.Meta;
    using UnityEditor;
    using UnityEngine;

    public sealed class FPModelViewerAssetUtilityTests
    {
        private const string TestRoot = "Assets/__FPModelViewerEditorTests";

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(TestRoot);
        }

        [Test]
        public void SanitizeFileName_ReplacesUnsupportedCharacters()
        {
            string result = FP_ModelViewerAssetUtility.SanitizeFileName(" Chair / Blue #1 ");

            Assert.That(result, Is.EqualTo("Chair___Blue__1"));
        }

        [Test]
        public void TryGetCaptureBinding_RequiresBinding()
        {
            var source = new GameObject("Model Source");

            try
            {
                bool result = FP_ModelViewerAssetUtility.TryGetCaptureBinding(
                    source,
                    out FP_ModelDisplayBinding binding,
                    out string validationMessage);

                Assert.That(result, Is.False);
                Assert.That(binding, Is.Null);
                Assert.That(validationMessage, Does.Contain("FP_ModelDisplayBinding"));
            }
            finally
            {
                Object.DestroyImmediate(source);
            }
        }

        [Test]
        public void TryGetCaptureBinding_RequiresModelDisplayData()
        {
            var source = new GameObject("Model Source");
            var displayData = ScriptableObject.CreateInstance<FP_ModelDisplayData>();

            try
            {
                FP_ModelDisplayBinding binding = source.AddComponent<FP_ModelDisplayBinding>();

                bool missingDataResult = FP_ModelViewerAssetUtility.TryGetCaptureBinding(
                    source,
                    out _,
                    out string missingDataMessage);

                var serializedBinding = new SerializedObject(binding);
                serializedBinding.FindProperty("_data").objectReferenceValue = displayData;
                serializedBinding.ApplyModifiedPropertiesWithoutUndo();

                bool configuredResult = FP_ModelViewerAssetUtility.TryGetCaptureBinding(
                    source,
                    out FP_ModelDisplayBinding configuredBinding,
                    out string configuredMessage);

                Assert.That(missingDataResult, Is.False);
                Assert.That(missingDataMessage, Does.Contain("FP_ModelDisplayData"));
                Assert.That(configuredResult, Is.True);
                Assert.That(configuredBinding, Is.SameAs(binding));
                Assert.That(configuredMessage, Is.Empty);
            }
            finally
            {
                Object.DestroyImmediate(source);
                Object.DestroyImmediate(displayData);
            }
        }

        [Test]
        public void TryCalculateLocalRendererBounds_EncapsulatesChildRenderersInRootSpace()
        {
            var root = new GameObject("Model Root");
            GameObject first = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject second = GameObject.CreatePrimitive(PrimitiveType.Cube);

            try
            {
                root.transform.SetPositionAndRotation(
                    new Vector3(8f, 3f, -2f),
                    Quaternion.Euler(15f, 40f, 5f));
                root.transform.localScale = Vector3.one * 3f;

                first.transform.SetParent(root.transform, false);
                first.transform.localPosition = new Vector3(2f, 0f, 0f);
                first.transform.localScale = Vector3.one * 2f;

                second.transform.SetParent(root.transform, false);
                second.transform.localPosition = new Vector3(-2f, 1f, 0f);
                second.transform.localScale = new Vector3(1f, 2f, 1f);

                bool result = FP_ModelViewerAssetUtility.TryCalculateLocalRendererBounds(
                    root,
                    out Bounds bounds,
                    out string validationMessage);

                Assert.That(result, Is.True);
                Assert.That(validationMessage, Is.Empty);
                AssertVector3(new Vector3(0.25f, 0.5f, 0f), bounds.center);
                AssertVector3(new Vector3(5.5f, 3f, 2f), bounds.size);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void TryCreateAndAssignDisplayData_ConfiguresEditablePrefab()
        {
            FP_ModelViewerAssetUtility.EnsureAssetFolder(TestRoot);
            var source = new GameObject("Catalog Chair");
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.transform.SetParent(source.transform, false);
            visual.transform.localPosition = new Vector3(0f, 2f, 0f);
            string prefabPath = $"{TestRoot}/CatalogChair.prefab";

            try
            {
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(source, prefabPath);
                Object.DestroyImmediate(source);
                source = null;

                bool result = FP_ModelViewerAssetUtility.TryCreateAndAssignDisplayData(
                    prefab,
                    $"{TestRoot}/DisplayData",
                    out FP_ModelDisplayData displayData,
                    out string resultMessage);

                GameObject configuredPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                FP_ModelDisplayBinding binding =
                    configuredPrefab.GetComponent<FP_ModelDisplayBinding>();

                Assert.That(result, Is.True, resultMessage);
                Assert.That(binding, Is.Not.Null);
                Assert.That(binding.Data, Is.SameAs(displayData));
                Assert.That(displayData.DisplayName, Is.EqualTo("Catalog Chair"));
                Assert.That(displayData.UseLocalBoundsOverride, Is.True);
                AssertVector3(new Vector3(0f, 2f, 0f), displayData.BoundsCenter);
                AssertVector3(Vector3.one, displayData.BoundsSize);
                AssertVector3(new Vector3(0f, -2f, 0f), displayData.LocalPivotOffset);
            }
            finally
            {
                if (source != null)
                {
                    Object.DestroyImmediate(source);
                }
            }
        }

        [Test]
        public void ConfigureItem_AssignsIdentityOnceAndPreservesItOnUpdate()
        {
            string itemPath = $"{TestRoot}/Chair/Chair_ViewerItem.asset";
            FP_ModelViewerItemData item =
                FP_ModelViewerAssetUtility.CreateOrLoadItem(itemPath);

            FP_ModelViewerAssetUtility.ConfigureItem(item, "Chair", null, null);
            string firstId = item.UniqueID;
            FP_ModelViewerAssetUtility.ConfigureItem(item, "Updated Chair", null, null);

            Assert.That(firstId, Is.Not.Empty);
            Assert.That(item.UniqueID, Is.EqualTo(firstId));
            Assert.That(item.DisplayName, Is.EqualTo("Updated Chair"));
        }

        [Test]
        public void SetItemTags_ReplacesTagsAndIgnoresNullsAndDuplicates()
        {
            var item = ScriptableObject.CreateInstance<FP_ModelViewerItemData>();
            var first = ScriptableObject.CreateInstance<FP_Tag>();
            var second = ScriptableObject.CreateInstance<FP_Tag>();

            try
            {
                FP_ModelViewerAssetUtility.SetItemTags(
                    item,
                    new[] { first, null, first, second });

                Assert.That(item.Tags.Count, Is.EqualTo(2));
                Assert.That(item.Tags[0], Is.SameAs(first));
                Assert.That(item.Tags[1], Is.SameAs(second));

                FP_ModelViewerAssetUtility.SetItemTags(item, null);

                Assert.That(item.Tags, Is.Empty);
            }
            finally
            {
                Object.DestroyImmediate(item);
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void ResolveOrCreateIncludedPrefab_ExportsSceneSourceWithoutConnectingIt()
        {
            FP_ModelViewerAssetUtility.EnsureAssetFolder(TestRoot);
            var source = new GameObject("Scene Chair");
            var displayData = ScriptableObject.CreateInstance<FP_ModelDisplayData>();
            string dataPath = $"{TestRoot}/SceneChair_ModelDisplayData.asset";
            string prefabPath = $"{TestRoot}/SceneChair_ViewerPrefab.prefab";

            try
            {
                AssetDatabase.CreateAsset(displayData, dataPath);
                FP_ModelDisplayBinding binding = source.AddComponent<FP_ModelDisplayBinding>();
                var serializedBinding = new SerializedObject(binding);
                serializedBinding.FindProperty("_data").objectReferenceValue = displayData;
                serializedBinding.ApplyModifiedPropertiesWithoutUndo();

                GameObject includedPrefab =
                    FP_ModelViewerAssetUtility.ResolveOrCreateIncludedPrefab(
                        source,
                        prefabPath);

                Assert.That(includedPrefab, Is.Not.Null);
                Assert.That(AssetDatabase.GetAssetPath(includedPrefab), Is.EqualTo(prefabPath));
                Assert.That(PrefabUtility.IsPartOfPrefabInstance(source), Is.False);

                FP_ModelDisplayBinding savedBinding =
                    includedPrefab.GetComponent<FP_ModelDisplayBinding>();
                Assert.That(savedBinding, Is.Not.Null);
                Assert.That(savedBinding.Data, Is.SameAs(displayData));
            }
            finally
            {
                Object.DestroyImmediate(source);
            }
        }

        [Test]
        public void SetCatalogItems_ReplacesCatalogOrder()
        {
            FP_ModelViewerItemData first = FP_ModelViewerAssetUtility.CreateOrLoadItem(
                $"{TestRoot}/First.asset");
            FP_ModelViewerItemData second = FP_ModelViewerAssetUtility.CreateOrLoadItem(
                $"{TestRoot}/Second.asset");
            FP_ModelViewerCatalogData catalog = FP_ModelViewerAssetUtility.CreateOrLoadCatalog(
                $"{TestRoot}/Catalog.asset");

            FP_ModelViewerAssetUtility.SetCatalogItems(
                catalog,
                new[] { second, first },
                "Test Catalog");

            Assert.That(catalog.Count, Is.EqualTo(2));
            Assert.That(catalog.Items[0], Is.SameAs(second));
            Assert.That(catalog.Items[1], Is.SameAs(first));
            Assert.That(catalog.DisplayName, Is.EqualTo("Test Catalog"));
        }

        private static void AssertVector3(Vector3 expected, Vector3 actual)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.0001f));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.0001f));
            Assert.That(actual.z, Is.EqualTo(expected.z).Within(0.0001f));
        }
    }
}
