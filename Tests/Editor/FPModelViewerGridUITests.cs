namespace FuzzPhyte.ModelViewer.Tests
{
    using System.Collections.Generic;
    using FuzzPhyte.Placement.OrbitalCamera;
    using NUnit.Framework;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    public sealed class FPModelViewerGridUITests
    {
        [Test]
        public void Build_ThreeByThreeWithFiftyItems_GeneratesSixPanels()
        {
            var gameObject = new GameObject("Model Viewer Grid Test");
            gameObject.SetActive(false);
            var catalog = ScriptableObject.CreateInstance<FP_ModelViewerCatalogData>();
            var items = new List<FP_ModelViewerItemData>();
            var host = new VisualElement();

            try
            {
                for (int i = 0; i < 50; i++)
                {
                    items.Add(ScriptableObject.CreateInstance<FP_ModelViewerItemData>());
                }
                SetCatalogItems(catalog, items);

                FP_ModelViewerGridUI grid = gameObject.AddComponent<FP_ModelViewerGridUI>();
                grid.SetCatalog(catalog);
                grid.SetGridDimensions(3, 3);
                grid.Build(host);

                List<VisualElement> panels = host
                    .Query<VisualElement>(className: FP_ModelViewerGridUI.PanelClass)
                    .ToList();

                Assert.That(grid.ItemsPerPanel, Is.EqualTo(9));
                Assert.That(grid.PanelCount, Is.EqualTo(6));
                Assert.That(grid.GeneratedPanelCount, Is.EqualTo(6));
                Assert.That(panels.Count, Is.EqualTo(6));
            }
            finally
            {
                for (int i = 0; i < items.Count; i++)
                {
                    Object.DestroyImmediate(items[i]);
                }
                Object.DestroyImmediate(catalog);
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Navigation_ChangesVisiblePanelAndPublishesZeroBasedIndex()
        {
            var gameObject = new GameObject("Model Viewer Navigation Test");
            gameObject.SetActive(false);
            var catalog = ScriptableObject.CreateInstance<FP_ModelViewerCatalogData>();
            var items = new List<FP_ModelViewerItemData>();
            var host = new VisualElement();

            try
            {
                for (int i = 0; i < 5; i++)
                {
                    items.Add(ScriptableObject.CreateInstance<FP_ModelViewerItemData>());
                }
                SetCatalogItems(catalog, items);

                FP_ModelViewerGridUI grid = gameObject.AddComponent<FP_ModelViewerGridUI>();
                grid.SetCatalog(catalog);
                grid.SetGridDimensions(1, 2);
                grid.Build(host);

                int reportedPage = -1;
                grid.PageChanged += pageIndex => reportedPage = pageIndex;

                bool changed = grid.NextPage();

                Assert.That(changed, Is.True);
                Assert.That(grid.CurrentPageIndex, Is.EqualTo(1));
                Assert.That(reportedPage, Is.EqualTo(1));

                List<VisualElement> panels = host
                    .Query<VisualElement>(className: FP_ModelViewerGridUI.PanelClass)
                    .ToList();
                Assert.That(panels[0].style.display.value, Is.EqualTo(DisplayStyle.None));
                Assert.That(panels[1].style.display.value, Is.EqualTo(DisplayStyle.Flex));
            }
            finally
            {
                for (int i = 0; i < items.Count; i++)
                {
                    Object.DestroyImmediate(items[i]);
                }
                Object.DestroyImmediate(catalog);
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void SelectItem_PublishesSelectedCatalogItem()
        {
            var gameObject = new GameObject("Model Viewer Selection Test");
            gameObject.SetActive(false);
            var item = ScriptableObject.CreateInstance<FP_ModelViewerItemData>();

            try
            {
                FP_ModelViewerGridUI grid = gameObject.AddComponent<FP_ModelViewerGridUI>();
                FP_ModelViewerItemData selected = null;
                grid.ItemSelected += value => selected = value;

                bool result = grid.SelectItem(item);

                Assert.That(result, Is.True);
                Assert.That(selected, Is.SameAs(item));
            }
            finally
            {
                Object.DestroyImmediate(item);
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void ContainerInsets_UseHostRelativePercentages()
        {
            var gameObject = new GameObject("Model Viewer Inset Test");
            gameObject.SetActive(false);
            var host = new VisualElement();

            try
            {
                FP_ModelViewerGridUI grid = gameObject.AddComponent<FP_ModelViewerGridUI>();
                grid.SetContainerInsetsPercent(60f, 5f, 10f, 15f);
                grid.Build(host);

                VisualElement root = host.Q<VisualElement>("FPModelViewerGrid");

                Assert.That(root, Is.Not.Null);
                Assert.That(root.style.position.value, Is.EqualTo(Position.Absolute));
                Assert.That(root.style.top.value.value, Is.EqualTo(60f));
                Assert.That(root.style.top.value.unit, Is.EqualTo(LengthUnit.Percent));
                Assert.That(root.style.right.value.value, Is.EqualTo(5f));
                Assert.That(root.style.bottom.value.value, Is.EqualTo(10f));
                Assert.That(root.style.left.value.value, Is.EqualTo(15f));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void ContainerInsets_NormalizeOpposingValuesToLeaveVisibleSpace()
        {
            var gameObject = new GameObject("Model Viewer Inset Clamp Test");
            gameObject.SetActive(false);

            try
            {
                FP_ModelViewerGridUI grid = gameObject.AddComponent<FP_ModelViewerGridUI>();

                grid.SetContainerInsetsPercent(80f, 70f, 80f, 70f);

                Assert.That(
                    grid.TopInsetPercent + grid.BottomInsetPercent,
                    Is.EqualTo(99f).Within(0.001f));
                Assert.That(
                    grid.LeftInsetPercent + grid.RightInsetPercent,
                    Is.EqualTo(99f).Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void ItemCellStyle_AppliesColorAndCornerRadius()
        {
            var gameObject = new GameObject("Model Viewer Style Test");
            gameObject.SetActive(false);
            var catalog = ScriptableObject.CreateInstance<FP_ModelViewerCatalogData>();
            var item = ScriptableObject.CreateInstance<FP_ModelViewerItemData>();
            var host = new VisualElement();
            var cellColor = new Color(0.2f, 0.4f, 0.6f, 1f);
            var textColor = new Color(0.9f, 0.8f, 0.7f, 1f);

            try
            {
                SetCatalogItems(catalog, new[] { item });
                FP_ModelViewerGridUI grid = gameObject.AddComponent<FP_ModelViewerGridUI>();
                grid.SetCatalog(catalog);
                grid.SetItemCellStyle(cellColor, textColor, 18f);
                grid.Build(host);

                Button button = host.Q<Button>(className: FP_ModelViewerGridUI.ItemClass);

                Assert.That(button, Is.Not.Null);
                Assert.That(button.style.backgroundColor.value, Is.EqualTo(cellColor));
                Assert.That(button.style.color.value, Is.EqualTo(textColor));
                Assert.That(button.style.borderTopLeftRadius.value.value, Is.EqualTo(18f));
                Assert.That(button.style.borderBottomRightRadius.value.value, Is.EqualTo(18f));
            }
            finally
            {
                Object.DestroyImmediate(item);
                Object.DestroyImmediate(catalog);
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void SelectItem_OpensDimmedPopupWithSquareThumbnailGrid()
        {
            var gameObject = new GameObject("Model Viewer Popup Test");
            gameObject.SetActive(false);
            var item = ScriptableObject.CreateInstance<FP_ModelViewerItemData>();
            var textures = new List<Texture2D>();
            var host = new VisualElement();

            try
            {
                IReadOnlyList<FP_ViewCubeHit> views =
                    FP_ModelViewerViewUtility.SupportedThumbnailViews;
                for (int i = 0; i < 5; i++)
                {
                    var texture = new Texture2D(8, 8);
                    textures.Add(texture);
                    item.SetThumbnail(views[i], texture, views[i].ToString());
                }

                FP_ModelViewerGridUI grid = gameObject.AddComponent<FP_ModelViewerGridUI>();
                grid.Build(host);

                bool selected = grid.SelectItem(item);

                List<VisualElement> rows = host
                    .Query<VisualElement>(className: FP_ModelViewerGridUI.PopupThumbnailRowClass)
                    .ToList();
                List<VisualElement> thumbnails = host
                    .Query<VisualElement>(className: FP_ModelViewerGridUI.PopupThumbnailClass)
                    .ToList();
                Assert.That(selected, Is.True);
                Assert.That(grid.IsPopupOpen, Is.True);
                Assert.That(rows.Count, Is.EqualTo(2));
                Assert.That(thumbnails.Count, Is.EqualTo(5));

                Label caption = host.Q<Label>(
                    className: FP_ModelViewerGridUI.PopupThumbnailCaptionClass);
                Assert.That(caption, Is.Not.Null);
                Assert.That(
                    caption.parent.ClassListContains(FP_ModelViewerGridUI.PopupThumbnailClass),
                    Is.True);
                Assert.That(caption.style.position.value, Is.EqualTo(Position.Absolute));
                Assert.That(caption.style.bottom.value.value, Is.Zero);

                Assert.That(grid.CloseItemDetails(), Is.True);
                Assert.That(grid.IsPopupOpen, Is.False);
            }
            finally
            {
                for (int i = 0; i < textures.Count; i++)
                {
                    Object.DestroyImmediate(textures[i]);
                }
                Object.DestroyImmediate(item);
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void SpawnSelectedItem_UsesConfiguredTargetAndIncludedPrefab()
        {
            var gameObject = new GameObject("Model Viewer Spawn Test");
            gameObject.SetActive(false);
            var spawnTarget = new GameObject("Spawn Target");
            var template = new GameObject("Included Model");
            var item = ScriptableObject.CreateInstance<FP_ModelViewerItemData>();
            var host = new VisualElement();
            GameObject spawned = null;

            try
            {
                spawnTarget.transform.SetPositionAndRotation(
                    new Vector3(3f, 2f, 1f),
                    Quaternion.Euler(0f, 45f, 0f));
                SetIncludedPrefab(item, template);

                FP_ModelViewerGridUI grid = gameObject.AddComponent<FP_ModelViewerGridUI>();
                grid.SetSpawnTarget(spawnTarget.transform);
                grid.Build(host);
                grid.SelectItem(item);

                spawned = grid.SpawnSelectedItem();

                Assert.That(spawned, Is.Not.Null);
                Assert.That(spawned.transform.parent, Is.SameAs(spawnTarget.transform));
                Assert.That(spawned.transform.position, Is.EqualTo(spawnTarget.transform.position));
                Assert.That(spawned.transform.rotation, Is.EqualTo(spawnTarget.transform.rotation));
                Assert.That(grid.IsPopupOpen, Is.False);
            }
            finally
            {
                if (spawned != null)
                {
                    Object.DestroyImmediate(spawned);
                }
                Object.DestroyImmediate(item);
                Object.DestroyImmediate(template);
                Object.DestroyImmediate(spawnTarget);
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void CatalogVisibility_CanHideAndRestoreGeneratedGrid()
        {
            var gameObject = new GameObject("Model Viewer Visibility Test");
            gameObject.SetActive(false);
            var host = new VisualElement();

            try
            {
                FP_ModelViewerGridUI grid = gameObject.AddComponent<FP_ModelViewerGridUI>();
                grid.Build(host);
                VisualElement root = host.Q<VisualElement>("FPModelViewerGrid");
                Button visibilityButton = host.Q<Button>(
                    className: FP_ModelViewerGridUI.CatalogVisibilityButtonClass);

                Assert.That(grid.HideCatalog(), Is.True);
                Assert.That(grid.IsCatalogVisible, Is.False);
                Assert.That(root.style.display.value, Is.EqualTo(DisplayStyle.None));
                Assert.That(visibilityButton.text, Is.EqualTo("Show Catalog"));

                Assert.That(grid.ShowCatalog(), Is.True);
                Assert.That(grid.IsCatalogVisible, Is.True);
                Assert.That(root.style.display.value, Is.EqualTo(DisplayStyle.Flex));
                Assert.That(visibilityButton.text, Is.EqualTo("Hide Catalog"));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void SpawnSelectedItem_ReplacesPreviouslySpawnedItem()
        {
            var gameObject = new GameObject("Model Viewer Replacement Test");
            gameObject.SetActive(false);
            var spawnTarget = new GameObject("Spawn Target");
            var firstTemplate = new GameObject("First Included Model");
            var secondTemplate = new GameObject("Second Included Model");
            var firstItem = ScriptableObject.CreateInstance<FP_ModelViewerItemData>();
            var secondItem = ScriptableObject.CreateInstance<FP_ModelViewerItemData>();
            var host = new VisualElement();
            GameObject secondSpawn = null;

            try
            {
                SetIncludedPrefab(firstItem, firstTemplate);
                SetIncludedPrefab(secondItem, secondTemplate);
                FP_ModelViewerGridUI grid = gameObject.AddComponent<FP_ModelViewerGridUI>();
                grid.SetSpawnTarget(spawnTarget.transform);
                grid.Build(host);

                grid.SelectItem(firstItem);
                GameObject firstSpawn = grid.SpawnSelectedItem();
                grid.SelectItem(secondItem);
                secondSpawn = grid.SpawnSelectedItem();

                Assert.That(firstSpawn == null, Is.True);
                Assert.That(secondSpawn, Is.Not.Null);
                Assert.That(grid.SpawnedItem, Is.SameAs(secondSpawn));
                Assert.That(spawnTarget.transform.childCount, Is.EqualTo(1));
            }
            finally
            {
                if (secondSpawn != null)
                {
                    Object.DestroyImmediate(secondSpawn);
                }
                Object.DestroyImmediate(firstItem);
                Object.DestroyImmediate(secondItem);
                Object.DestroyImmediate(firstTemplate);
                Object.DestroyImmediate(secondTemplate);
                Object.DestroyImmediate(spawnTarget);
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void SpawnSelectedItem_CanHideCatalogAfterSuccessfulSpawn()
        {
            var gameObject = new GameObject("Model Viewer Hide After Spawn Test");
            gameObject.SetActive(false);
            var spawnTarget = new GameObject("Spawn Target");
            var template = new GameObject("Included Model");
            var item = ScriptableObject.CreateInstance<FP_ModelViewerItemData>();
            var host = new VisualElement();
            GameObject spawned = null;

            try
            {
                SetIncludedPrefab(item, template);
                FP_ModelViewerGridUI grid = gameObject.AddComponent<FP_ModelViewerGridUI>();
                grid.SetSpawnTarget(spawnTarget.transform);
                grid.SetHideCatalogAfterSpawn(true);
                grid.Build(host);
                grid.SelectItem(item);

                spawned = grid.SpawnSelectedItem();

                Assert.That(spawned, Is.Not.Null);
                Assert.That(grid.IsCatalogVisible, Is.False);
                Assert.That(grid.ShowCatalog(), Is.True);
                Assert.That(grid.IsCatalogVisible, Is.True);
            }
            finally
            {
                if (spawned != null)
                {
                    Object.DestroyImmediate(spawned);
                }
                Object.DestroyImmediate(item);
                Object.DestroyImmediate(template);
                Object.DestroyImmediate(spawnTarget);
                Object.DestroyImmediate(gameObject);
            }
        }

        private static void SetCatalogItems(
            FP_ModelViewerCatalogData catalog,
            IReadOnlyList<FP_ModelViewerItemData> items)
        {
            var serializedCatalog = new SerializedObject(catalog);
            SerializedProperty serializedItems = serializedCatalog.FindProperty("_items");
            serializedItems.arraySize = items.Count;
            for (int i = 0; i < items.Count; i++)
            {
                serializedItems.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
            }
            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetIncludedPrefab(
            FP_ModelViewerItemData item,
            GameObject includedPrefab)
        {
            var serializedItem = new SerializedObject(item);
            serializedItem.FindProperty("_includedPrefab").objectReferenceValue = includedPrefab;
            serializedItem.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
