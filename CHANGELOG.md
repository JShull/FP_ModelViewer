# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Fixed

- Guarded Model Viewer UI setup when an assigned inactive `UIDocument` has not
  created its root visual element yet; setup retries through the existing
  enable lifecycle.

## [0.2.0] - 2026-08-17

### 0.2.0 Added

- [@JShull](https://github.com/jshull)
  - Added lower-right companion UI Off/On action buttons with mutually exclusive visibility, Inspector UnityEvents, matching C# events, and runtime state APIs.
  - Readme file internal editor updates
  - Readme.md file updates
  - Repository went public
  - Added model viewer item and catalog data assets.
  - Added the 14 supported face and corner thumbnail views.
  - Added runtime capture profiles and in-memory thumbnail capture.
  - Added the Model Viewer Builder editor window for batch scene-object and prefab capture.
  - Added PNG persistence, texture import configuration, item updates, and ordered catalog generation.
  - Added scene model-viewer bindings and deterministic grid pagination.
  - Added Edit Mode coverage for item thumbnails, pagination, and editor asset generation.
  - Added automatic `FP_ModelDisplayData` generation from root-and-child renderer bounds, including binding assignment for scene objects and editable   prefabs.
  - Added deployment-prefab generation for scene-only sources and automatic `Included Prefab` assignment on viewer item data.
  - Added profile-selectable three-point or scene lighting and isolated-origin or preserved-scene-position capture modes.
  - Added temporary front key, back rim, directional fill, and ambient-light capture setup with scene-light state restoration.
  - Added Unity-style Color or Filter-and-Temperature appearance controls for every three-point rig light, including Kelvin settings and graphics-state   restoration.
  - Added a draggable Unity-derived Kelvin color gradient and precise numeric temperature entry to the capture-profile inspector.
  - Replaced viewer-item string tags with reusable FP Utility `FP_Tag` asset references and added runtime tag-membership helpers.
  - Added a collapsible source table with three optional `FP_Tag` assignments per source and automatic tag synchronization during generation.
  - Added `FP_ModelViewerGridUI`, a catalog-fed UI Toolkit grid that generates rows, columns, panels, cover-image buttons, navigation, and selection/page   events from runtime parameters.
  - Added host-relative Top, Right, Bottom, and Left percentage insets for positioning the complete generated grid within selected screen space.
  - Added configurable catalog-cell background color, text color, and rounded corners.
  - Added a dimmed selected-item popup with automatic near-square thumbnail layout, configurable popup styling, Back to Catalog, and included-prefab   spawning at a supplied Transform.
  - Added popup-close and item-spawned Inspector/C# events plus deterministic layout and spawn coverage.
  - Anchored popup thumbnail captions as shaded footers inside their responsive image tiles.
  - Added persistent Hide/Show Catalog controls, optional hide-after-spawn behavior, and catalog-visibility events for unobstructed 3D viewing.
  - Changed built-in spawning to manage one viewer-owned instance at a time, replacing the prior spawn and exposing explicit removal plus removal events.
  - Added a hideable, scrollable catalog-tag filter drawer with duplicate-free tag discovery, immediate joined matching, reset controls, configurable   columns, and truncated labels with full tooltips.
  - Added tag-filter runtime APIs, state accessors, visible-item-count events, and deterministic filtering/layout coverage.
  - Added a shared rounded-corner parameter for every generated navigation, filter, popup-action, and catalog-visibility button.
  - Replaced tag filter action buttons with independently selectable radio-style toggles while preserving immediate joined multi-tag filtering.
  - Extended filtering coverage to assert the actual rendered grid cells after UI-driven tag selection.
  - Added shared text, normal background, hover, and selected/pressed colors for every generated control button, with immediate runtime setters and pointer/  focus-state coverage.
  - Added a catalog-title override and optional host-level Sprite logo with top-left, top-center, or top-right placement plus runtime branding setters.
  - Constrained only the tag-filter checkmark element to fixed circular geometry so responsive rows cannot stretch it or clip tag text.
  - Changed tag-filter rows to equal flex-width cells and vertical-only scrolling, preventing percentage margins from creating a horizontal scrollbar.
  - Removed empty catalog placeholder cells and unused rows so partial pages and filtered results distribute only their real items across the available row   width.
  - Added a shared control-button outline-thickness parameter, runtime setter, and non-breaking `SetButtonStyle` overload.
  - Added optional runtime OBJ download controls to the item popup, using the selected prefab or matching spawned instance and exposing material, texture,   skinned-mesh, collider, vertex-limit, and texture-size settings.
  - Added WebGL browser download, iOS Files export, native persistent-file fallback, success/failure events, and build-only export coverage through the FP   Utility runtime exporter.
  - Added automatic ModelImporter Read/Write preparation for every MeshFilter, SkinnedMeshRenderer, and MeshCollider referenced by catalog item prefabs,   covering builder generation and direct catalog Inspector edits.
  - Made the inherited Document Style Sheet optional for `FP_ModelViewerGridUI` and automatically attach an assigned stylesheet to the UIDocument root,   avoiding FP_UI's mismatched-stylesheet error for generated inline layouts.
  - Changed desktop OBJ delivery to use Save File prompts in the Unity Editor and Windows standalone players, with cancellation returning quietly to the viewer.

### 0.2.0 Changed

- [@JShull](https://github.com/jshull).
  - Replaced copied Unity package dependencies with explicit FP Utility, FP Placement, and FP UI dependencies.
  - Simplified runtime and editor assembly references around the package's actual dependency boundary.
  - Replaced the `DefaultAsset` output-folder field with a project-folder path and **Browse...** workflow.
  - Required capture sources to provide `FP_ModelDisplayData` through `FP_ModelDisplayBinding` so model presentation and camera framing remain data-driven.
  - Changed batch validation to block generation when any populated source has a missing binding or a null `FP_ModelDisplayData` reference.

## [0.1.0] - 2026-08-12

### 0.1.0 Added

- [@JShull](https://github.com/jshull).
  - setup project UPM

### 0.1.0 Changed

- None... yet

### 0.1.0 Fixed

- Setup the contents to align with Unity naming conventions

### 0.1.0 Removed

- None... yet
