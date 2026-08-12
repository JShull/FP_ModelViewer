# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Added model viewer item and catalog data assets.
- Added the 14 supported face and corner thumbnail views.
- Added runtime capture profiles and in-memory thumbnail capture.
- Added the Model Viewer Builder editor window for batch scene-object and prefab capture.
- Added PNG persistence, texture import configuration, item updates, and ordered catalog generation.
- Added scene model-viewer bindings and deterministic grid pagination.
- Added Edit Mode coverage for item thumbnails, pagination, and editor asset generation.
- Added automatic `FP_ModelDisplayData` generation from root-and-child renderer bounds, including binding assignment for scene objects and editable prefabs.
- Added deployment-prefab generation for scene-only sources and automatic `Included Prefab` assignment on viewer item data.
- Added profile-selectable three-point or scene lighting and isolated-origin or preserved-scene-position capture modes.
- Added temporary front key, back rim, directional fill, and ambient-light capture setup with scene-light state restoration.
- Added Unity-style Color or Filter-and-Temperature appearance controls for every three-point rig light, including Kelvin settings and graphics-state restoration.
- Added a draggable Unity-derived Kelvin color gradient and precise numeric temperature entry to the capture-profile inspector.
- Replaced viewer-item string tags with reusable FP Utility `FP_Tag` asset references and added runtime tag-membership helpers.
- Added a collapsible source table with three optional `FP_Tag` assignments per source and automatic tag synchronization during generation.

### Changed

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
