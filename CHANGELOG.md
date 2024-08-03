# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

-   GraphRenderer for rendering graphs.
-   GUIScope.Multiple, which serves as a drop-in for Verse.TextBlock, but keep the same API across all three supported RW versions.
-   IlyvionDebugActionAttribute that works like the DebugActionAttribute, but has the same API across all three supported RW versions.

## [0.2.0] 2024-08-01

### Added

-   Debug action for hot reloading language files.
-   Add a "draw UI helpers" debug setting. Does nothing on its own, but is used by dependents to decide whether or not to draw extra UI bits for debugging purposes.
-   Import Bradson's GUIScope utility class with adaptions to better fit my needs.

### Changed

-   Make it so that the CustomFontManager only patches the font system if it's been enabled. This way it won't affect performance if it goes unused.

### Fixed

-   The name ilyvion was misspelled in several places and file paths as 'ilvyion'.

## [0.1.0] 2024-07-25

### Added

-   First implementation of the library.

[Unreleased]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.2.0...HEAD
[0.2.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/ilyvion/ilyvion-laboratory/releases/tag/v0.1.0
