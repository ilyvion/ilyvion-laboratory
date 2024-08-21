# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Fixed

-   Only register required version request when mismatch

## [0.8.0] 2024-08-20

### Changed

-   Change the version check mechanism from using a method call to using a VersionCheckDef declared in a Defs XML file. The failing assembly won't even get to load to call VersionCheck.ShowRequiresAtLeastVersionMessageFor if we need it, making it useless, and this also makes it so that mods that only need XML features can still specify a version requirement without having to add a whole assembly jus for that.

## [0.7.0] 2024-08-20

### Added

-   PatchOperationFindModById. Does what it says. Alternative to vanilla's PatchOperationFindMod but relies on mod id rather than on mod name, which, at least in theory, is more stable/less likely to change.

## [0.6.0] 2024-08-18

### Added

-   MultiTickCoroutineManager: a GameComponent that orchestrates the registration and execution of, as the name suggests, multi-tick coroutines, i.e. tasks that span multiple ticks. Heavily modeled (as a concept, all code original) after the Unity Coroutine type. Makes use of the fact that C# allows you to write IEnumerables using yield (return|break) keywords, allowing natural 'break points' in a task. Created mainly to alleviate per-tick strain in the Colony Manager Redux, but I can see myself making use of this in many other situations going forward where you have too much work to perform for a single tick to handle well.

## [0.5.0] 2024-08-15

### Added

-   Version check mechanism. Something to make me more comfortable with releasing mods that depend on this mod; even if something goes wrong, at least the user will get an explanation.
-   Import Widgets_Labels class' methods from Colony Manager Redux.
-   Import CacheValue(s) classes from Colony Manager Redux.
-   Utility methods/types for dealing with arrays and ArrayPools.
-   CustomBackCompatibility utility class for doing custom type replacements on game load.
-   EnumerableExtensions.MinAndMax for calculating both the min and the max value of an IEnumerable in a single pass.

## [0.4.0] 2024-08-10

### Added

-   Extension for muting a color.
-   Extension for inline dumping the value of any value during debugging.
-   Scriber method for CircularBuffers.

## [0.3.0] 2024-08-04

### Added

-   GraphRenderer for rendering graphs.
-   GUIScope.Multiple, which serves as a drop-in for Verse.TextBlock, but keep the same API across all three supported RW versions.
-   IlyvionDebugActionAttribute that works like the DebugActionAttribute, but has the same API across all three supported RW versions.
-   A circular buffer.
-   LogDebug method added to IlyvionMod; only logs when the DEBUG symbol is present.
-   Various useful extension methods.

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

[Unreleased]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.8.0...HEAD
[0.8.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.7.0...v0.8.0
[0.7.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.6.0...v0.7.0
[0.6.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.5.0...v0.6.0
[0.5.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.4.0...v0.5.0
[0.4.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.3.0...v0.4.0
[0.3.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/ilyvion/ilyvion-laboratory/releases/tag/v0.1.0
