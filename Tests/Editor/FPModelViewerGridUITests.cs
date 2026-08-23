namespace FuzzPhyte.ModelViewer.Tests
{
    using System.Collections.Generic;
    using FuzzPhyte.Placement.OrbitalCamera;
    using FuzzPhyte.Utility;
    using FuzzPhyte.Utility.Meta;
    using NUnit.Framework;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    internal sealed class FPModelViewerGridUITestWindow : EditorWindow
    {
    }

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
        public void Build_OneByFourWithSixItems_DoesNotGenerateEmptyCells()
        {
            var gameObject = new GameObject("Model Viewer Partial Panel Test");
            gameObject.SetActive(false);
            var catalog = ScriptableObject.CreateInstance<FP_ModelViewerCatalogData>();
            var items = new List<FP_ModelViewerItemData>();
            var host = new VisualElement();

            try
            {
                for (int i = 0; i < 6; i++)
                {
                    items.Add(ScriptableObject.CreateInstance<FP_ModelViewerItemData>());
                }
                SetCatalogItems(catalog, items);

                FP_ModelViewerGridUI grid = gameObject.AddComponent<FP_ModelViewerGridUI>();
                grid.SetCatalog(catalog);
                grid.SetGridDimensions(1, 4);
                grid.Build(host);

                List<VisualElement> panels = host
                    .Query<VisualElement>(className: FP_ModelViewerGridUI.PanelClass)
                    .ToList();
                List<Button> finalPanelItems = panels[1]
                    .Query<Button>(className: FP_ModelViewerGridUI.ItemClass)
                    .ToList();
                List<VisualElement> emptyCells = host
                    .Query<VisualElement>(className: FP_ModelViewerGridUI.EmptyCellClass)
                    .ToList();

                Assert.That(panels.Count, Is.EqualTo(2));
                Assert.That(finalPanelItems.Count, Is.EqualTo(2));
                Assert.That(emptyCells, Is.Empty);
                Assert.That(finalPanelItems[0].style.flexGrow.value, Is.EqualTo(1f));
                Assert.That(finalPanelItems[0].style.flexBasis.value.value, Is.EqualTo(0f));
                Assert.That(finalPanelItems[1].style.flexBasis.value.value, Is.EqualTo(0f));
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
        public void CompanionUiToggle_StartsWithOffActionAndPublishesBothStates()
        {
            var gameObject = new GameObject("Model Viewer Companion UI Test");
            gameObject.SetActive(false);
            var host = new VisualElement();
            int unityOffCount = 0;
            int unityOnCount = 0;
            int csharpOffCount = 0;
            int csharpOnCount = 0;

            try
            {
                FP_ModelViewerGridUI grid = gameObject.AddComponent<FP_ModelViewerGridUI>();
                grid.OnCompanionUiTurnedOff.AddListener(() => unityOffCount++);
                grid.OnCompanionUiTurnedOn.AddListener(() => unityOnCount++);
                grid.CompanionUiTurnedOff += () => csharpOffCount++;
                grid.CompanionUiTurnedOn += () => csharpOnCount++;
                grid.Build(host);

                Button offButton = host.Q<Button>(
                    className: FP_ModelViewerGridUI.CompanionUiOffButtonClass);
                Button onButton = host.Q<Button>(
                    className: FP_ModelViewerGridUI.CompanionUiOnButtonClass);

                Assert.That(offButton, Is.Not.Null);
                Assert.That(onButton, Is.Not.Null);
                Assert.That(grid.IsCompanionUiOn, Is.True);
                Assert.That(offButton.text, Is.EqualTo("Off"));
                Assert.That(onButton.text, Is.EqualTo("On"));
                Assert.That(offButton.style.display.value, Is.EqualTo(DisplayStyle.Flex));
                Assert.That(onButton.style.display.value, Is.EqualTo(DisplayStyle.None));
                Assert.That(offButton.style.right.value.value, Is.EqualTo(12f));
                Assert.That(offButton.style.bottom.value.value, Is.EqualTo(12f));

                Assert.That(grid.TurnCompanionUiOff(), Is.True);
                Assert.That(grid.IsCompanionUiOn, Is.False);
                Assert.That(offButton.style.display.value, Is.EqualTo(DisplayStyle.None));
                Assert.That(onButton.style.display.value, Is.EqualTo(DisplayStyle.Flex));
                Assert.That(unityOffCount, Is.EqualTo(1));
                Assert.That(csharpOffCount, Is.EqualTo(1));
                Assert.That(grid.TurnCompanionUiOff(), Is.False);
                Assert.That(unityOffCount, Is.EqualTo(1));
                Assert.That(csharpOffCount, Is.EqualTo(1));

                grid.Refresh();
                offButton = host.Q<Button>(
                    className: FP_ModelViewerGridUI.CompanionUiOffButtonClass);
                onButton = host.Q<Button>(
                    className: FP_ModelViewerGridUI.CompanionUiOnButtonClass);
                Assert.That(offButton.style.display.value, Is.EqualTo(DisplayStyle.None));
                Assert.That(onButton.style.display.value, Is.EqualTo(DisplayStyle.Flex));

                Assert.That(grid.TurnCompanionUiOn(), Is.True);
                Assert.That(grid.IsCompanionUiOn, Is.True);
                Assert.That(offButton.style.display.value, Is.EqualTo(DisplayStyle.Flex));
                Assert.That(onButton.style.display.value, Is.EqualTo(DisplayStyle.None));
                Assert.That(unityOnCount, Is.EqualTo(1));
                Assert.That(csharpOnCount, Is.EqualTo(1));
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

        [Test]
        public void TagFilters_DeduplicateCatalogTagsAndRequireJoinedMatchingImmediately()
        {
            var gameObject = new GameObject("Model Viewer Tag Filter Test");
            gameObject.SetActive(false);
            var catalog = ScriptableObject.CreateInstance<FP_ModelViewerCatalogData>();
            var firstItem = ScriptableObject.CreateInstance<FP_ModelViewerItemData>();
            var secondItem = ScriptableObject.CreateInstance<FP_ModelViewerItemData>();
            var thirdItem = ScriptableObject.CreateInstance<FP_ModelViewerItemData>();
            var furniture = ScriptableObject.CreateInstance<FP_Tag>();
            var table = ScriptableObject.CreateInstance<FP_Tag>();
            FPModelViewerGridUITestWindow window = CreateAttachedHost(out VisualElement host);

            try
            {
                furniture.TagName = "Furniture";
                table.TagName = "Table";
                SetTags(firstItem, furniture);
                SetTags(secondItem, furniture, table);
                SetTags(thirdItem, table);
                SetCatalogItems(catalog, new[] { firstItem, secondItem, thirdItem });

                FP_ModelViewerGridUI grid = gameObject.AddComponent<FP_ModelViewerGridUI>();
                grid.SetCatalog(catalog);
                grid.Build(host);

                Assert.That(grid.CatalogTags.Count, Is.EqualTo(2));
                Assert.That(grid.VisibleItemCount, Is.EqualTo(3));

                grid.ShowTagFilterPanel();
                Toggle furnitureRadio = host
                    .Query<Toggle>(className: FP_ModelViewerGridUI.TagFilterRadioClass)
                    .ToList()
                    .Find(toggle => object.ReferenceEquals(toggle.userData, furniture));
                Assert.That(furnitureRadio, Is.Not.Null);
                furnitureRadio.value = true;

                Assert.That(grid.VisibleItemCount, Is.EqualTo(2));
                Assert.That(grid.ActiveTagFilters, Is.EquivalentTo(new[] { furniture }));
                Assert.That(
                    host.Query<Button>(className: FP_ModelViewerGridUI.ItemClass).ToList().Count,
                    Is.EqualTo(2));

                Assert.That(grid.ToggleTagFilter(table), Is.True);
                Assert.That(grid.VisibleItemCount, Is.EqualTo(1));
                Assert.That(grid.ActiveTagFilters.Count, Is.EqualTo(2));
                Assert.That(
                    host.Query<Button>(className: FP_ModelViewerGridUI.ItemClass).ToList().Count,
                    Is.EqualTo(1));
                Assert.That(
                    host.Query<VisualElement>(className: FP_ModelViewerGridUI.EmptyCellClass)
                        .ToList(),
                    Is.Empty);

                Assert.That(grid.ClearTagFilters(), Is.True);
                Assert.That(grid.ActiveTagFilters, Is.Empty);
                Assert.That(grid.VisibleItemCount, Is.EqualTo(3));
            }
            finally
            {
                window.Close();
                Object.DestroyImmediate(furniture);
                Object.DestroyImmediate(table);
                Object.DestroyImmediate(firstItem);
                Object.DestroyImmediate(secondItem);
                Object.DestroyImmediate(thirdItem);
                Object.DestroyImmediate(catalog);
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void TagFilterPanel_IsScrollableAndUsesConfiguredColumnsAndTruncation()
        {
            var gameObject = new GameObject("Model Viewer Tag Drawer Test");
            gameObject.SetActive(false);
            var catalog = ScriptableObject.CreateInstance<FP_ModelViewerCatalogData>();
            var item = ScriptableObject.CreateInstance<FP_ModelViewerItemData>();
            var tags = new List<FP_Tag>();
            var host = new VisualElement();

            try
            {
                for (int i = 0; i < 4; i++)
                {
                    var tag = ScriptableObject.CreateInstance<FP_Tag>();
                    tag.TagName = i == 0 ? "VeryLongFurnitureTag" : $"Tag {i + 1}";
                    tags.Add(tag);
                }
                SetTags(item, tags.ToArray());
                SetCatalogItems(catalog, new[] { item });

                FP_ModelViewerGridUI grid = gameObject.AddComponent<FP_ModelViewerGridUI>();
                grid.SetCatalog(catalog);
                grid.SetTagFilterLayout(2, 8);
                grid.Build(host);

                Assert.That(grid.ShowTagFilterPanel(), Is.True);
                VisualElement panel = host.Q<VisualElement>(
                    className: FP_ModelViewerGridUI.TagFilterPanelClass);
                ScrollView scroll = host.Q<ScrollView>(
                    className: FP_ModelViewerGridUI.TagFilterScrollClass);
                List<VisualElement> rows = host
                    .Query<VisualElement>(className: FP_ModelViewerGridUI.TagFilterRowClass)
                    .ToList();
                List<Toggle> radios = host
                    .Query<Toggle>(className: FP_ModelViewerGridUI.TagFilterRadioClass)
                    .ToList();

                Assert.That(panel.style.display.value, Is.EqualTo(DisplayStyle.Flex));
                Assert.That(scroll, Is.Not.Null);
                Assert.That(
                    scroll.horizontalScrollerVisibility,
                    Is.EqualTo(ScrollerVisibility.Hidden));
                Assert.That(
                    scroll.verticalScrollerVisibility,
                    Is.EqualTo(ScrollerVisibility.Auto));
                Assert.That(rows.Count, Is.EqualTo(2));
                Assert.That(radios.Count, Is.EqualTo(4));
                Assert.That(radios[0].text, Is.EqualTo("VeryL..."));
                Assert.That(radios[0].tooltip, Is.EqualTo("VeryLongFurnitureTag"));
                Assert.That(radios[0].value, Is.False);
                Assert.That(radios[0].style.width.keyword, Is.EqualTo(StyleKeyword.Null));
                Assert.That(radios[0].style.flexGrow.value, Is.EqualTo(1f));
                Assert.That(radios[0].style.flexBasis.value.value, Is.EqualTo(0f));
                Assert.That(radios[0].style.minWidth.value.value, Is.EqualTo(0f));
                VisualElement radioInput = radios[0].Q<VisualElement>(
                    className: "unity-toggle__input");
                Assert.That(radioInput, Is.Not.Null);
                Assert.That(radioInput.style.width.keyword, Is.EqualTo(StyleKeyword.Null));
                VisualElement radioCheckmark = radios[0].Q<VisualElement>(
                    className: "unity-toggle__checkmark");
                Assert.That(radioCheckmark, Is.Not.Null);
                Assert.That(radioCheckmark.style.width.value.value, Is.EqualTo(16f));
                Assert.That(radioCheckmark.style.height.value.value, Is.EqualTo(16f));
                Assert.That(
                    radioCheckmark.style.borderTopLeftRadius.value.value,
                    Is.EqualTo(999f));
                VisualElement radioDot = radios[0].Q<VisualElement>(
                    className: FP_ModelViewerGridUI.TagFilterRadioDotClass);
                Assert.That(radioDot, Is.Not.Null);
                Assert.That(radioDot.style.width.value.value, Is.EqualTo(8f));
                Assert.That(radioDot.style.height.value.value, Is.EqualTo(8f));
                Assert.That(radioDot.style.display.value, Is.EqualTo(DisplayStyle.None));
                Assert.That(grid.HideTagFilterPanel(), Is.True);
                Assert.That(panel.style.display.value, Is.EqualTo(DisplayStyle.None));
            }
            finally
            {
                for (int i = 0; i < tags.Count; i++)
                {
                    Object.DestroyImmediate(tags[i]);
                }
                Object.DestroyImmediate(item);
                Object.DestroyImmediate(catalog);
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void Branding_UsesTitleOverrideAndHostLevelLogoPlacement()
        {
            var gameObject = new GameObject("Model Viewer Branding Test");
            gameObject.SetActive(false);
            var catalog = ScriptableObject.CreateInstance<FP_ModelViewerCatalogData>();
            var texture = new Texture2D(32, 16);
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
            var host = new VisualElement();

            try
            {
                FP_ModelViewerGridUI grid = gameObject.AddComponent<FP_ModelViewerGridUI>();
                grid.SetCatalog(catalog);
                grid.SetCatalogTitle("Custom Catalog");
                grid.SetLogo(
                    sprite,
                    FP_ModelViewerLogoPlacement.TopLeft,
                    new Vector2(96f, 48f),
                    new Vector2(12f, 8f));
                grid.Build(host);

                Label title = host.Q<Label>(className: FP_ModelViewerGridUI.HeaderClass);
                VisualElement logoContainer = host.Q<VisualElement>(
                    className: FP_ModelViewerGridUI.LogoContainerClass);
                Image logo = host.Q<Image>(className: FP_ModelViewerGridUI.LogoImageClass);

                Assert.That(title, Is.Not.Null);
                Assert.That(title.text, Is.EqualTo("Custom Catalog"));
                Assert.That(logoContainer, Is.Not.Null);
                Assert.That(logo, Is.Not.Null);
                Assert.That(logo.sprite, Is.SameAs(sprite));
                Assert.That(
                    logoContainer.style.justifyContent.value,
                    Is.EqualTo(Justify.FlexStart));
                Assert.That(logoContainer.style.top.value.value, Is.EqualTo(8f));
                Assert.That(logo.style.width.value.value, Is.EqualTo(96f));
                Assert.That(logo.style.height.value.value, Is.EqualTo(48f));
                Assert.That(logo.style.marginLeft.value.value, Is.EqualTo(12f));

                grid.SetCatalogTitle("Updated Catalog");
                Assert.That(title.text, Is.EqualTo("Updated Catalog"));

                grid.SetLogo(
                    sprite,
                    FP_ModelViewerLogoPlacement.TopCenter,
                    new Vector2(64f, 32f),
                    new Vector2(4f, 10f));
                logoContainer = host.Q<VisualElement>(
                    className: FP_ModelViewerGridUI.LogoContainerClass);
                logo = host.Q<Image>(className: FP_ModelViewerGridUI.LogoImageClass);
                Assert.That(
                    logoContainer.style.justifyContent.value,
                    Is.EqualTo(Justify.Center));
                Assert.That(logo.style.left.value.value, Is.EqualTo(4f));

                grid.ClearLogo();
                Assert.That(
                    host.Q<VisualElement>(className: FP_ModelViewerGridUI.LogoContainerClass),
                    Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(sprite);
                Object.DestroyImmediate(texture);
                Object.DestroyImmediate(catalog);
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void GeneratedControlButtons_UseSharedCornerRadius()
        {
            var gameObject = new GameObject("Model Viewer Button Radius Test");
            gameObject.SetActive(false);
            var catalog = ScriptableObject.CreateInstance<FP_ModelViewerCatalogData>();
            var item = ScriptableObject.CreateInstance<FP_ModelViewerItemData>();
            var tag = ScriptableObject.CreateInstance<FP_Tag>();
            var host = new VisualElement();
            var textColor = new Color(0.9f, 0.8f, 0.7f, 1f);
            var backgroundColor = new Color(0.2f, 0.25f, 0.3f, 1f);
            var hoverColor = new Color(0.35f, 0.4f, 0.45f, 1f);
            var selectedColor = new Color(0.1f, 0.5f, 0.8f, 1f);

            try
            {
                tag.TagName = "Furniture";
                SetTags(item, tag);
                SetCatalogItems(catalog, new[] { item });
                FP_ModelViewerGridUI grid = gameObject.AddComponent<FP_ModelViewerGridUI>();
                grid.SetCatalog(catalog);
                grid.SetButtonStyle(
                    textColor,
                    backgroundColor,
                    hoverColor,
                    selectedColor,
                    14f,
                    2.5f);
                grid.Build(host);
                grid.ShowTagFilterPanel();
                grid.SelectItem(item);

                List<Button> buttons = host.Query<Button>().ToList();
                Assert.That(buttons.Count, Is.GreaterThan(0));
                for (int i = 0; i < buttons.Count; i++)
                {
                    if (buttons[i].ClassListContains(FP_ModelViewerGridUI.ItemClass))
                    {
                        continue;
                    }

                    Assert.That(
                        buttons[i].style.borderTopLeftRadius.value.value,
                        Is.EqualTo(14f));
                    Assert.That(
                        buttons[i].style.borderBottomRightRadius.value.value,
                        Is.EqualTo(14f));
                    Assert.That(buttons[i].style.color.value, Is.EqualTo(textColor));
                    Assert.That(
                        buttons[i].style.backgroundColor.value,
                        Is.EqualTo(backgroundColor));
                    Assert.That(buttons[i].style.borderTopWidth.value, Is.EqualTo(2.5f));
                    Assert.That(buttons[i].style.borderRightWidth.value, Is.EqualTo(2.5f));
                    Assert.That(buttons[i].style.borderBottomWidth.value, Is.EqualTo(2.5f));
                    Assert.That(buttons[i].style.borderLeftWidth.value, Is.EqualTo(2.5f));
                }

                Assert.That(grid.ButtonOutlineThickness, Is.EqualTo(2.5f));

                Toggle radio = host.Q<Toggle>(
                    className: FP_ModelViewerGridUI.TagFilterRadioClass);
                Assert.That(radio, Is.Not.Null);
                Assert.That(radio.style.borderTopLeftRadius.value.value, Is.EqualTo(14f));

                Assert.That(grid.ButtonHoverColor, Is.EqualTo(hoverColor));
                Assert.That(grid.ButtonSelectedColor, Is.EqualTo(selectedColor));
            }
            finally
            {
                Object.DestroyImmediate(tag);
                Object.DestroyImmediate(item);
                Object.DestroyImmediate(catalog);
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void SelectedItemObjPackage_UsesIncludedPrefabMeshHierarchy()
        {
            var viewerObject = new GameObject("Model Viewer OBJ Test");
            viewerObject.SetActive(false);
            var item = ScriptableObject.CreateInstance<FP_ModelViewerItemData>();
            var template = new GameObject("Export Template");
            var mesh = new Mesh { name = "Export Triangle" };
            mesh.vertices = new[]
            {
                Vector3.zero,
                Vector3.right,
                Vector3.up
            };
            mesh.triangles = new[] { 0, 1, 2 };
            template.AddComponent<MeshFilter>().sharedMesh = mesh;

            try
            {
                SetIncludedPrefab(item, template);
                FP_ModelViewerGridUI grid =
                    viewerObject.AddComponent<FP_ModelViewerGridUI>();
                grid.SelectItem(item);

                bool success = grid.TryBuildSelectedItemObjPackage(
                    out FPMeshRuntimeObjExportResult result,
                    out string message);

                Assert.That(success, Is.True, message);
                Assert.That(result, Is.Not.Null);
                Assert.That(result.ExportedMeshCount, Is.EqualTo(1));
                Assert.That(result.VertexCount, Is.EqualTo(3));
                Assert.That(result.Data.Length, Is.GreaterThan(0));
            }
            finally
            {
                Object.DestroyImmediate(mesh);
                Object.DestroyImmediate(template);
                Object.DestroyImmediate(item);
                Object.DestroyImmediate(viewerObject);
            }
        }

        [Test]
        public void Awake_AssignedStyleSheet_AttachesItToDocumentRoot()
        {
            var gameObject = new GameObject("Model Viewer Style Sheet Test");
            gameObject.SetActive(false);
            var styleSheet = ScriptableObject.CreateInstance<StyleSheet>();

            try
            {
                UIDocument document = gameObject.AddComponent<UIDocument>();
                FP_ModelViewerGridUI grid =
                    gameObject.AddComponent<FP_ModelViewerGridUI>();
                grid.Document = document;
                grid.DocumentStyleSheet = styleSheet;

                grid.enabled = false;
                gameObject.SetActive(true);
                grid.Awake();

                Assert.That(
                    document.rootVisualElement.styleSheets.Contains(styleSheet),
                    Is.True);
                Assert.That(
                    document.rootVisualElement.Q<VisualElement>("FPModelViewerGrid"),
                    Is.Not.Null);
            }
            finally
            {
                Object.DestroyImmediate(styleSheet);
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

        private static FPModelViewerGridUITestWindow CreateAttachedHost(
            out VisualElement host)
        {
            var window = ScriptableObject.CreateInstance<FPModelViewerGridUITestWindow>();
            window.Show();
            host = new VisualElement();
            window.rootVisualElement.Add(host);
            return window;
        }

        private static void SetIncludedPrefab(
            FP_ModelViewerItemData item,
            GameObject includedPrefab)
        {
            var serializedItem = new SerializedObject(item);
            serializedItem.FindProperty("_includedPrefab").objectReferenceValue = includedPrefab;
            serializedItem.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetTags(
            FP_ModelViewerItemData item,
            params FP_Tag[] tags)
        {
            var serializedItem = new SerializedObject(item);
            SerializedProperty serializedTags = serializedItem.FindProperty("_tags");
            serializedTags.arraySize = tags.Length;
            for (int i = 0; i < tags.Length; i++)
            {
                serializedTags.GetArrayElementAtIndex(i).objectReferenceValue = tags[i];
            }
            serializedItem.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
