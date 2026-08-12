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

## Planned UI Workflow

A UI Toolkit viewer derived from `FP_UI` will render configurable paged grids, item-detail popups, and application-defined action rows from the generated catalog.

## Dependencies

Required package versions are declared in [package.json](./package.json). See [Installation~/Dependencies.md](Installation~/Dependencies.md) for repository links.

## License Notes

See [LICENSE.md](LICENSE.md) for details

## Contact

- [John Shull](mailto:JShull@fuzzphyte.com)
