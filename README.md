# FP Model Viewer

FP Model Viewer is a data-driven catalog, thumbnail, and model-viewing package for Unity applications. It composes existing FuzzPhyte libraries instead of duplicating their responsibilities:

- FP Placement supplies model display metadata, world bounds, and orbital camera behavior.
- FP UI supplies the UI Toolkit base used by the forthcoming catalog interface.
- FP Utility supplies shared FuzzPhyte data conventions and runtime utilities.

## Current Runtime Foundation

- `FP_ModelViewerItemData` describes one catalog item, its model metadata, reusable `FP_Tag` asset references, optional included prefab or external asset key, download URL, thumbnails, cover view, and default view.
- `FP_ModelViewerCatalogData` stores an ordered collection of viewer items.
- `FP_ModelCaptureProfile` stores resolution, projection, bounds padding, capture layers, and solid or transparent background settings.
- `FP_ModelViewerBinding` connects catalog item data to an existing `FP_ModelDisplayBinding` in a scene.
- `FP_ModelThumbnailCaptureUtility` captures any of the six face or eight corner views into an in-memory `Texture2D` at runtime or in the Editor.
- `FP_ModelViewerPagination` provides deterministic page ranges for grid and horizontal-strip layouts.
- `FP_ModelViewerGridUI` derives from the FP UI Toolkit base and generates a catalog-backed grid, all required panels, item buttons, and previous/next navigation at runtime.

The capture utility uses a caller-owned camera. Isolate the target with the profile's capture layer mask or place a temporary model copy on a dedicated capture layer. The utility restores the camera transform, projection, clipping, background, culling mask, and render target after every capture.

Item tags use `FuzzPhyte.Utility.Meta.FP_Tag` ScriptableObject references rather than strings. Runtime filtering can use `HasTag` or `HasAnyTag`, which compare stable Unity asset identity and safely ignore null filter entries.

Existing viewer items with empty string-tag lists migrate cleanly to the asset-reference list. Any previously populated string tags must be replaced manually with the intended `FP_Tag` assets because a string cannot be mapped reliably to a unique asset reference.

## Editor Generation Workflow

Open **FuzzPhyte > Model Viewer > Builder** and:

1. Enter an output folder under `Assets`, or select one with **Browse...**. Generated content should remain outside the package.
2. Assign or create an `FP_ModelCaptureProfile`.
3. Add scene objects or prefab assets. If they are not configured yet, use **Auto Setup Missing** or the per-source **Generate and Assign Display Data** button.
4. Enable any of the 14 face and corner views and choose an enabled cover view.
5. Generate the catalog and thumbnails.

The builder creates or updates one `FP_ModelViewerItemData` asset per source, imports its PNG thumbnails, and builds an ordered `FP_ModelViewerCatalogData` asset. When a source is a scene-only GameObject, the builder also saves `<ModelName>_ViewerPrefab.prefab` beside the item data and assigns it to `Included Prefab`. The authored scene object remains disconnected from the generated prefab. Prefab assets and prefab instances continue referencing their existing prefab assets instead of generating duplicates.

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

## Runtime UI Grid

`FP_ModelViewerGridUI` is the first runtime catalog-viewer component. Add a `UIDocument` and `FP_ModelViewerGridUI` to a scene object, then assign the inherited `Document` and `Document Style Sheet` references plus a generated catalog. The document can be empty, or **Host Element Name** can target a named element inside an authored UXML document.

Set **Rows** and **Columns** to control panel capacity. The component uses `FP_ModelViewerPagination` to create every required panel; for example, 50 items in a 3x3 layout create six panels, while a 1x5 layout produces a horizontal five-item strip per panel. Only the active panel is displayed. Cover thumbnails and item names populate each button, and the final panel receives empty layout cells so its grid alignment remains stable.

Use **Container Insets (%)** to reserve screen space around the complete generated grid. Top and Bottom are percentages of the host height; Left and Right are percentages of the host width. All four default to zero, preserving the full-host layout. For example, Top `60`, Right `5`, Bottom `5`, and Left `5` places the viewer in the lower portion of a full-screen host with a five-percent border on the other sides. Opposing inset pairs are normalized when their total exceeds 99 percent so the grid always retains visible space. The same values can be changed at runtime with `SetContainerInsetsPercent(top, right, bottom, left)`.

**Item Cell Style** provides a background color, text color, and pixel corner radius for generated catalog cells. The corner radius clips cell contents so thumbnail and label presentation follow the rounded silhouette. These values remain compatible with the `.fp-model-viewer-grid__item` USS class and can also be changed with `SetItemCellStyle`.

The generated hierarchy includes functional inline layout and stable USS classes rooted at `.fp-model-viewer-grid`, so applications can completely restyle it without replacing its runtime logic. Use the Inspector-facing **On Item Selected**, **On Page Changed**, and **On Grid Rebuilt** events or the corresponding C# events. `SetCatalog`, `SetGridDimensions`, `SetContainerInsetsPercent`, `GoToPage`, `NextPage`, `PreviousPage`, `SelectItem`, and `Refresh` support application-driven control.

Selecting an item opens a modal detail popup over the entire UI host. **Popup Backdrop Color** controls the dimmed scene/catalog background, including its alpha. The panel color, text color, corner radius, width percentage, and height percentage are also configurable. All non-null item thumbnails are arranged automatically into a near-square grid: five images use three columns and two rows, while all 14 supported views use four columns and four rows. Each view caption is anchored as a shaded footer inside its square image tile, so smaller screens cannot separate the caption from its thumbnail. The grid scrolls vertically when its content exceeds the popup height.

The popup provides **Back to Catalog** and **Spawn Item** actions. Assign **Spawn Target** to the scene `Transform` whose world position and rotation define the deployment area. Spawn Item is enabled only when the selected item has an `Included Prefab` and a target is assigned. The component can parent the new instance to that target and close the popup after spawning. The grid owns one spawned instance at a time: a successful new spawn removes the instance it previously created before placing the replacement. `SpawnedItem` exposes the active instance, and `RemoveSpawnedItem` clears it explicitly. Unrelated children and scene objects are never removed. **On Item Spawned** and **On Spawned Item Removed** are available as Inspector and C# events. External asset keys and download URLs remain application-owned; the built-in spawn action currently instantiates only `Included Prefab`.

Enable **Show Catalog Visibility Button** to keep a compact Hide/Show control available at the lower-left of the UI host. Hiding affects only the generated catalog container, leaving the spawned 3D object and restore button visible. **Hide Catalog After Spawn** can perform that transition automatically after a successful spawn. `HideCatalog`, `ShowCatalog`, `ToggleCatalogVisibility`, and `SetHideCatalogAfterSpawn` support the same flow from code; **On Catalog Visibility Changed** can coordinate camera or interaction systems. Popup behavior remains available through `SetSpawnTarget`, `ShowItemDetails`, `CloseItemDetails`, and `SpawnSelectedItem`, with **On Popup Closed** for listeners.

## Planned UI Workflow

The next UI slice can connect spawned items to the orbital camera/viewing workflow and generalize the popup action row for download and project-specific actions.

## Dependencies

Required package versions are declared in [package.json](./package.json). See [Installation~/Dependencies.md](Installation~/Dependencies.md) for repository links.

## License Notes

See [LICENSE.md](LICENSE.md) for details

## Contact

- [John Shull](mailto:JShull@fuzzphyte.com)
