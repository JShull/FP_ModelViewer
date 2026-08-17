# FP Model Viewer

FP Model Viewer is a data-driven catalog, thumbnail, and model-viewing package for Unity applications. It composes existing FuzzPhyte libraries instead of duplicating their responsibilities:

- FP Placement supplies model display metadata, world bounds, and orbital camera behavior.
- FP UI supplies the UI Toolkit base used by the generated runtime catalog interface.
- FP Utility supplies shared FuzzPhyte data conventions and runtime utilities.

The package covers the complete viewer pipeline: prepare model display data, generate thumbnail photography and catalog assets in the Editor, present those assets through a configurable paged UI Toolkit grid, filter them with reusable tags, inspect an item's available views, spawn one selected prefab into a viewing area, publish interaction events, and optionally export its readable mesh hierarchy as an OBJ package at runtime.

## Documentation and Sample

- [Full FP Model Viewer README](https://github.com/jshull/FP_ModelViewer/blob/main/README.md)
- [Model Viewer sample project](https://github.com/jshull/FP_ModelViewer/tree/main/Samples/ModelViewerEx)
- Unity menu: **FuzzPhyte > Model Viewer > Builder**

The checked-in sample at `Samples/ModelViewerEx` contains a populated six-item catalog, generated viewer-item data and thumbnails, a configured `FP_ModelViewerGridUI`, tag filtering, popup actions, catalog visibility changes, spawning, and an FP Placement orbital-camera setup.

## Package Foundation

- `FP_ModelViewerItemData` describes one catalog item, its model metadata, reusable `FP_Tag` asset references, optional included prefab or external asset key, download URL, thumbnails, cover view, and default view.
- `FP_ModelViewerCatalogData` stores an ordered collection of viewer items.
- `FP_ModelCaptureProfile` stores resolution, projection, bounds padding, capture layers, and solid or transparent background settings.
- `FP_ModelViewerBinding` connects catalog item data to an existing `FP_ModelDisplayBinding` in a scene.
- `FP_ModelThumbnailCaptureUtility` captures any of the six face or eight corner views into an in-memory `Texture2D` at runtime or in the Editor.
- `FP_ModelViewerPagination` provides deterministic page ranges for grid and horizontal-strip layouts.
- `FP_ModelViewerGridUI` derives from the FP UI Toolkit base and generates a catalog-backed grid, all required panels, item buttons, and previous/next navigation at runtime.

The capture utility uses a caller-owned camera. Isolate the target with the profile's capture layer mask or place a temporary model copy on a dedicated capture layer. The utility restores the camera transform, projection, clipping, background, culling mask, and render target after every capture.

Item tags use `FuzzPhyte.Utility.Meta.FP_Tag` ScriptableObject references rather than strings. Runtime filtering can use `HasTag`, `HasAnyTag`, or `HasAllTags`, which compare stable Unity asset identity and safely ignore null filter entries.

Existing viewer items with empty string-tag lists migrate cleanly to the asset-reference list. Any previously populated string tags must be replaced manually with the intended `FP_Tag` assets because a string cannot be mapped reliably to a unique asset reference.

## Building a Catalog

The catalog data chain is `FP_ModelDisplayData` -> `FP_ModelViewerItemData` -> `FP_ModelViewerCatalogData`. Display data defines how a model is centered, bounded, scaled, and framed. Each viewer item references that display data plus its included prefab, reusable `FP_Tag` assets, thumbnails, cover view, and optional external asset metadata. The catalog stores the ordered viewer-item list consumed by `FP_ModelViewerGridUI`.

Open **FuzzPhyte > Model Viewer > Builder** and:

1. Enter an output folder under `Assets`, or select one with **Browse...**. Generated content should remain outside the package.
2. Assign or create an `FP_ModelCaptureProfile`.
3. Add scene objects or prefab assets. If they are not configured yet, use **Auto Setup Missing** or the per-source **Generate and Assign Display Data** button.
4. Enable any of the 14 face and corner views and choose an enabled cover view.
5. Generate the catalog and thumbnails.

The builder creates or updates one `FP_ModelViewerItemData` asset per source, imports its PNG thumbnails, and builds an ordered `FP_ModelViewerCatalogData` asset. When a source is a scene-only GameObject, the builder also saves `<ModelName>_ViewerPrefab.prefab` beside the item data and assigns it to `Included Prefab`. The authored scene object remains disconnected from the generated prefab. Prefab assets and prefab instances continue referencing their existing prefab assets instead of generating duplicates.

Before catalog items are committed, Model Viewer scans each `Included Prefab` for `MeshFilter`, `SkinnedMeshRenderer`, and `MeshCollider` mesh references. Imported FBX, OBJ, and other model assets are deduplicated by source path, their `ModelImporter` **Read/Write** setting is enabled when necessary, and only changed importers are reimported. The same preparation runs when item entries are edited directly in the catalog Inspector. Procedural or native mesh assets without a `ModelImporter` must already be readable; an unreadable mesh that Unity cannot reconfigure blocks utility-driven catalog assignment and reports the affected item and mesh.

The collapsible **Sources** table provides three optional `FP_Tag` slots beside every source. Generation synchronizes those assignments into the corresponding viewer item in table order. Empty slots are ignored, repeated references are stored once, and generating a row with no tags clears any tags previously generated for that item.

Sources are cloned temporarily and rendered on isolated layer 31; source objects are not moved or modified for capture. A temporary capture camera is created when no scene camera is assigned.

Generated display-data assets are written to a `DisplayData` folder beneath the selected output folder. Auto setup adds `FP_ModelDisplayBinding` when needed, gathers every `Renderer` on the model root and its children (including inactive children), and transforms their bounds into the binding root's local space. It uses the source GameObject name for `DisplayName`, enables the local bounds override, writes the calculated center and size, and sets `LocalPivotOffset` to the negative bounds center so the model envelope is centered on the display pivot. Existing bindings with assigned data are left unchanged by **Auto Setup Missing**.

Every populated source must have both an `FP_ModelDisplayBinding` and a non-null `FP_ModelDisplayData` reference on that binding. The builder disables catalog and thumbnail generation when any source is missing either requirement; it does not treat an empty binding as capture-ready.

### Data-Driven Camera Framing

`FP_ModelDisplayData` is required for every capture source. The builder applies its local pivot offset, default rotation, default scale, and bounds padding before framing the camera. When `UseLocalBoundsOverride` is enabled, its bounds center and size define the model volume; otherwise `FP_ModelDisplayBinding` calculates the final renderer envelope after applying the presentation defaults.

For every selected view, the capture system uses that final world-space volume to update the camera position, rotation, perspective distance or orthographic size, and near/far clipping planes. `FP_ModelCaptureProfile` supplies the shared photographic settings—projection, FOV, resolution, additional framing padding, capture layers, and background—without replacing the model-specific display data.

Solid-color and transparent backgrounds are supported. Textured environments and prefab-authored lighting rigs remain possible future expansions.

### Lighting and Capture Space

Each `FP_ModelCaptureProfile` provides two independent controls:

- `ThreePointRig` creates a temporary camera-relative front key spot, back rim spot, directional fill, and controlled flat ambient light for each thumbnail. Existing scene lights are prevented from affecting the captured layers during the render and are restored immediately afterward.
- `SceneLighting` creates no lights and leaves the loaded scene's lights, ambient settings, light probes, and reflection probes in control.
- `IsolatedAtOrigin` uses the model's presentation defaults, moves a temporary copy to the capture origin, and renders only the isolated capture layer. This is the standard product-thumbnail mode.
- `PreserveScenePosition` captures a scene object directly at its authored transform and uses the profile's `Capture Layers`. This keeps point and spot lights spatially aligned for rooms, collections, and large objects.

The recommended singular-object preset is `ThreePointRig` plus `IsolatedAtOrigin`. The recommended room or collection preset is `SceneLighting` plus `PreserveScenePosition`. A prefab asset has no scene-authored position, so scene-lighting captures are most meaningful when the source is an object in the loaded scene. In-place sources must be active and their layers must be included by the profile.

Each front, back, and directional rig light has a Unity-style `Appearance` option. `Color` uses the selected light color without color temperature. `FilterAndTemperature` exposes both a color filter and a draggable 1,000–20,000 Kelvin gradient generated with Unity's correlated-color-temperature conversion, plus a numeric Kelvin field for precise entry. Unity multiplies the correlated color temperature by that filter to calculate the final light color. The capture temporarily enables Unity's linear-intensity and color-temperature graphics settings when a rig light uses temperature, then restores both settings after rendering.

### Prefab Capture

Prefab assets do not have to be placed in the open scene. The builder temporarily instantiates them for capture. Auto setup can add and save the binding on an editable `.prefab` asset. Imported model prefabs are immutable; place one beneath an editable wrapper prefab before running auto setup. If a prefab is skipped without producing PNGs, confirm that its root or a child contains `FP_ModelDisplayBinding` and that the binding references an `FP_ModelDisplayData` asset. The builder reports both missing requirements beside the affected source before generation.

## Runtime UI Settings

`FP_ModelViewerGridUI` is the first runtime catalog-viewer component. Add a `UIDocument` and `FP_ModelViewerGridUI` to a scene object, then assign the inherited `Document` reference plus a generated catalog. The document can be empty, or **Host Element Name** can target a named element inside an authored UXML document. **Document Style Sheet** is optional because the generated hierarchy includes a complete functional inline layout; when a stylesheet is assigned, Model Viewer automatically attaches it to the UIDocument root before building.

Set **Rows** and **Columns** to control maximum panel capacity. The component uses `FP_ModelViewerPagination` to create every required panel; for example, 50 items in a 3x3 layout create six panels, while a 1x5 layout produces a horizontal five-item strip per panel. Only the active panel is displayed. Cover thumbnails and item names populate each button. Partial pages and filtered results generate only their real items; each populated row divides its available width evenly without placeholder cells or unused rows.

Use **Branding > Catalog Title Override** to replace the catalog asset's display name in the generated heading. Leaving it empty keeps the catalog display name. Assign an optional Sprite to **Logo Sprite**, select Top Left, Top Center, or Top Right, and set its pixel size and offset to place a non-interactive logo along the top edge of the UI host. Because the logo is hosted outside the catalog container, it remains visible when the catalog is hidden. `SetCatalogTitle`, `SetLogo`, and `ClearLogo` update the currently generated UI at runtime.

Use **Container Insets (%)** to reserve screen space around the complete generated grid. Top and Bottom are percentages of the host height; Left and Right are percentages of the host width. All four default to zero, preserving the full-host layout. For example, Top `60`, Right `5`, Bottom `5`, and Left `5` places the viewer in the lower portion of a full-screen host with a five-percent border on the other sides. Opposing inset pairs are normalized when their total exceeds 99 percent so the grid always retains visible space. The same values can be changed at runtime with `SetContainerInsetsPercent(top, right, bottom, left)`.

**Item Cell Style** provides a background color, text color, and pixel corner radius for generated catalog cells. The corner radius clips cell contents so thumbnail and label presentation follow the rounded silhouette. These values remain compatible with the `.fp-model-viewer-grid__item` USS class and can also be changed with `SetItemCellStyle`.

**Button Style** applies shared text, normal background, hover, selected/pressed, corner-radius, and outline-thickness values to every generated control button: Filters, Clear, Close, Previous, Next, popup actions, and Hide/Show Catalog. The selected color is used while pressing a button or when it receives keyboard focus. Item cells retain their independent styling. Use `SetButtonColors`, `SetButtonCornerRadius`, `SetButtonOutlineThickness`, or either `SetButtonStyle` overload to update all currently generated controls at runtime without rebuilding the UI.

```csharp
grid.SetButtonStyle(
    textColor: Color.white,
    backgroundColor: new Color(0.15f, 0.15f, 0.15f),
    hoverColor: new Color(0.25f, 0.25f, 0.25f),
    selectedColor: new Color(0.1f, 0.45f, 0.8f),
    cornerRadius: 10f,
    outlineThickness: 2f);
```

The generated hierarchy includes functional inline layout and stable USS classes rooted at `.fp-model-viewer-grid`, so applications can completely restyle it without replacing its runtime logic. Use the Inspector-facing **On Item Selected**, **On Page Changed**, and **On Grid Rebuilt** events or the corresponding C# events. `SetCatalog`, `SetGridDimensions`, `SetContainerInsetsPercent`, `GoToPage`, `NextPage`, `PreviousPage`, `SelectItem`, and `Refresh` support application-driven control.

Selecting an item opens a modal detail popup over the entire UI host. **Popup Backdrop Color** controls the dimmed scene/catalog background, including its alpha. The panel color, text color, corner radius, width percentage, and height percentage are also configurable. All non-null item thumbnails are arranged automatically into a near-square grid: five images use three columns and two rows, while all 14 supported views use four columns and four rows. Each view caption is anchored as a shaded footer inside its square image tile, so smaller screens cannot separate the caption from its thumbnail. The grid scrolls vertically when its content exceeds the popup height.

The popup provides **Back to Catalog**, **Spawn Item**, and optional **Download OBJ** actions. Assign **Spawn Target** to the scene `Transform` whose world position and rotation define the deployment area. Spawn Item is enabled only when the selected item has an `Included Prefab` and a target is assigned. The component can parent the new instance to that target and close the popup after spawning. The grid owns one spawned instance at a time: a successful new spawn removes the instance it previously created before placing the replacement. `SpawnedItem` exposes the active instance, and `RemoveSpawnedItem` clears it explicitly. Unrelated children and scene objects are never removed. **On Item Spawned** and **On Spawned Item Removed** are available as Inspector and C# events. External asset keys and download URLs remain application-owned; the built-in spawn action currently instantiates only `Included Prefab`.

### Runtime OBJ Download

Enable **Show Obj Export Button** to let a user download the selected included prefab's mesh hierarchy. If that same item is currently spawned, the exporter uses the live spawned instance so its current transforms and skinned-mesh pose are captured; otherwise it creates a temporary prefab instance for the export. The generated ZIP contains an OBJ and optional MTL/PNG files. MeshFilters and submeshes are included by default, with separate options for inactive children, skinned meshes, mesh colliders, material data, and texture snapshots.

**Maximum Vertex Count** defaults to 500,000 to reject unexpectedly large in-memory exports before they pressure a WebGL heap; set it to zero for no limit. **Maximum Texture Size** caps optional PNG readback. Model Viewer automatically enables **Read/Write** on imported model meshes when their items enter a generated or Inspector-edited catalog. Texture export is off by default because GPU readback, PNG encoding, and ZIP creation temporarily coexist in memory. The result exports geometry rather than the whole Unity prefab: scripts, audio, text components, animations, and other runtime behavior are not included.

On WebGL, **Download OBJ** creates a browser Blob and starts a normal user download. On iOS it opens the Files export picker. The Unity Editor and Windows standalone players open a Save File prompt; cancelling returns to the viewer without firing success or failure events. Platforms without a prompt integration save a unique ZIP beneath `Application.persistentDataPath/FP_Exports`. `TryBuildSelectedItemObjPackage` exposes generation without delivery, while `ExportSelectedItemObj` performs both stages. **On Obj Exported** reports the downloaded filename or saved path; **On Obj Export Failed** reports a readable error. The matching C# events are `ObjExported` and `ObjExportFailed`, and `SetObjExportEnabled` plus `SetObjExportSettings` support runtime configuration.

Enable **Show Catalog Visibility Button** to keep a compact Hide/Show control available at the lower-left of the UI host. Hiding affects only the generated catalog container, leaving the spawned 3D object and restore button visible. **Hide Catalog After Spawn** can perform that transition automatically after a successful spawn. `HideCatalog`, `ShowCatalog`, `ToggleCatalogVisibility`, and `SetHideCatalogAfterSpawn` support the same flow from code; **On Catalog Visibility Changed** can coordinate camera or interaction systems. Popup behavior remains available through `SetSpawnTarget`, `ShowItemDetails`, `CloseItemDetails`, and `SpawnSelectedItem`, with **On Popup Closed** for listeners.

**Companion UI Toggle** adds paired **Off** and **On** action buttons at the lower-right of the UI host, aligned with the Hide/Show Catalog control. Off is visible initially and On is hidden. Selecting Off publishes **On Companion UI Turned Off**, hides itself, and reveals On; selecting On publishes **On Companion UI Turned On** and restores the initial presentation. These host-level buttons remain available when the catalog is hidden and use the shared control-button styling. `TurnCompanionUiOff`, `TurnCompanionUiOn`, and `SetCompanionUiState` provide the same state transitions from code, while `IsCompanionUiOn` exposes the current state. Matching `CompanionUiTurnedOff` and `CompanionUiTurnedOn` C# events support runtime-owned wiring.

### Tag Filtering

Enable **Show Tag Filter Button** to add a compact Filters control to the catalog. It opens and closes a scrollable tag drawer generated from the current catalog's item tags. Each `FP_Tag` asset appears once regardless of how many items reference it. Tags use independently selectable radio-style toggles with fixed circular indicators, making their selected state visible while preserving multi-tag filtering. Selecting a tag filters immediately and resets the viewer to the first panel; selecting multiple tags uses joined AND matching, so an item remains visible only when it has every selected tag. Select an active tag again to remove it, or use **Clear** to remove every selection and restore the complete catalog.

**Tag Filter Columns** controls how many filter buttons appear across each row. Cells divide the available row width evenly without horizontal overflow, and the drawer scrolls vertically only. **Tag Filter Max Characters** limits visible button text and adds an ellipsis when required; the complete tag name remains available as the button tooltip. Panel width and height percentages, panel color, unselected button color, and selected button color are also configurable. `ShowTagFilterPanel`, `HideTagFilterPanel`, `ToggleTagFilterPanel`, `ToggleTagFilter`, `ClearTagFilters`, and `SetTagFilterLayout` expose the workflow to code. `CatalogTags`, `ActiveTagFilters`, and `VisibleItemCount` expose the current state, while **On Tag Filters Changed** reports the new visible-item count and the C# event supplies the active tag collection.

## Item Selection, Actions, and Events

Clicking a generated catalog cell calls `SelectItem` and publishes **On Item Selected** with the selected `FP_ModelViewerItemData`. When **Show Popup On Selection** is enabled, the same interaction opens the item-detail popup and displays every available thumbnail. The selected item remains available through `SelectedItem`, so application code can update descriptions, camera focus, analytics, download state, or other project-specific UI without searching the catalog again.

The popup's built-in actions publish focused lifecycle events:

- **On Popup Closed** runs when the detail view is dismissed.
- **On Item Spawned** supplies the instantiated included prefab, while **On Spawned Item Removed** supplies the viewer-owned instance being replaced or removed.
- **On OBJ Exported** supplies the browser filename or saved path, while **On OBJ Export Failed** supplies a readable failure message. Cancelling a desktop Save File prompt publishes neither event.

The grid also exposes **On Page Changed**, **On Grid Rebuilt**, **On Tag Filters Changed**, **On Catalog Visibility Changed**, **On Companion UI Turned Off**, and **On Companion UI Turned On**. The dedicated **On Catalog Hidden** and **On Catalog Visible** Inspector events provide their configured `FP_ScreenRegionAsset`, which can coordinate the viewer with FP Placement orbital-camera input regions. Matching C# events are available for item selection, page changes, spawning/removal, popup closure, visibility, companion-UI state, active tag filters, and OBJ delivery.

Use Inspector UnityEvents for scene wiring and the C# events when another runtime system owns the behavior. The package publishes the interaction and selected data; the application decides how cameras, analytics, downloads, or other panels respond.

## Extending the Viewer

Use the published selection, spawn, visibility, filter, and export events to connect project-specific systems without modifying the generated catalog UI. A deployed application can use the selected item's `FP_ModelDisplayData` to update orbital-camera bounds and framing, add its own popup action controls, coordinate screen regions, or replace the included runtime spawn and OBJ-delivery behavior. The `Samples/ModelViewerEx` scene demonstrates the current FP Placement orbital-camera and screen-region wiring.

## Dependencies

Required package versions are declared in [package.json](./package.json). See [Installation~/Dependencies.md](Installation~/Dependencies.md) for repository links.

## License Notes

See [LICENSE.md](LICENSE.md) for details

## Contact

- [John Shull](mailto:JShull@fuzzphyte.com)
