# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Added IlyvionDebugLogCategories, letting mods enable/disable specific debug log categories (such as coroutine scheduling diagnostics) at runtime for easier troubleshooting.
- Added LruCache, a reusable bounded cache that evicts only its least-recently-used entry once full, for mods that want the same eviction behaviour used internally for text-measurement caching.
- VersionCheckDefs (the outdated-version warning popup) can now target any installed mod, not just ilyvion's Laboratory itself, so mods can warn players when one of their own dependencies is out of date. This also lets a version requirement be as precise as a build/revision number, not just major.minor. Requires RimWorld 1.4 or newer to check a mod other than ilyvion's Laboratory; on 1.3 only ilyvion's Laboratory itself can still be checked.

### Changed

- MultiTickCachedValue.DoUpdateIfNeeded(bool force) is now obsolete and will become private in a future version; use the new parameterless DoUpdateIfNeeded() or ForceUpdate() instead, which no longer break compile-time null-checking for calling code.

### Fixed

- The internal cache used to measure whether text fits a given width no longer wipes itself entirely once it fills up.
- Saving/loading an Either using the Reference, LocalTargetInfo, TargetInfo, GlobalTargetInfo, or BodyPart modes no longer silently loses the loaded value.
- Saving/loading an Either holding a null BodyPart value no longer crashes on load.
- CachedValues.Update() on a key that hadn't been added yet no longer stores a value that's immediately considered stale.
- Graph axis labels no longer crash when a plotted value is large enough to run past the k/M/G unit suffixes.
- Graph axis rounding now respects the requested precision for smaller values instead of always rounding to the nearest 10.
- Graphs drawn without target lines and without target data no longer crash.
- Clicking or right-clicking a graph legend entry now shows/hides the correct series when a hidden series is present.
- Graphs with only a single data point no longer misbehave when hovered.
- Graph series with a per-series unit label now use it in mouse-over tooltips instead of always falling back to the graph's Y-axis unit.
- Mousing over a graph whose series have unevenly-sized target lines no longer occasionally shows values or tooltips from the wrong series.
- GUIScope.Multiple() no longer permanently corrupts the game's shared font style when combining a font size change with a game font change.
- GUIScope.ScrollView() no longer ends the scroll view twice if disposed more than once.
- Saving/loading a CircularBuffer now correctly uses the requested look mode for its elements instead of always guessing based on the element type, and CircularBuffers of references (e.g. things) now load correctly instead of always coming back empty.
- Loading a CircularBuffer from a corrupted save (zero capacity or a missing values list) no longer crashes; it now recovers with an empty buffer instead.
- A coroutine forced to run to completion immediately no longer freezes the game forever if it starts a child coroutine or waits on a condition that can't be resolved right away; it now gives up and logs an error instead.
- The mod XML data used to resolve custom ParentName inheritance is now released when a mod list is reloaded, instead of accumulating in memory for the rest of the session.
- A custom ParentName referencing a type that doesn't exist now logs a clear error instead of crashing with an undiagnostic exception.
- Text-fits-width measurements are no longer cached across different text sizes (e.g. tiny vs medium font), which could previously show stale results using the wrong font's measurement.
- CircularBuffer no longer silently returns the wrong element when accessed with a negative index; it now throws, matching how it already behaved for out-of-range positive indexes.
- A coroutine waiting a negative number of ticks no longer gets stuck forever; it now resumes right away, same as waiting zero ticks.
- Starting a coroutine while no game is loaded now fails with a clear error instead of an undiagnostic crash.
- VersionCheck.IsAtLeastVersion(version) now correctly returns true when the mod's version exactly matches the required version, instead of only when it's strictly newer.
- Dumping a null value via Dump() in dev mode now logs 'DUMP: <null>' instead of just 'DUMP: '.
- ArrayPool.RentWithSelfReturn() rentals no longer risk returning the same pooled array to the pool twice when the rental is copied, which could previously let two unrelated rentals end up sharing the same array.
- IlyvionMod.LogException() now prefixes logged messages with the mod's name, like the other logging methods, instead of leaving it off.
- Two loaded mods requiring an update to a newer version of ilyvion's Laboratory no longer crashes the version-check dialog when both mods share the same name; a warning is logged and the higher required version is kept instead.
- Two mods registering a custom back-compatibility type replacement for the same base type or class name no longer crashes the game with an undiagnostic exception; a warning is logged and the later registration takes precedence instead.
- Two mods registering a custom font under the same key no longer crashes the game with an undiagnostic exception; a warning is logged and the later registration takes precedence instead.

## [0.22.0] - 2026-07-31

### Added

- Either<TLeft, TRight>, an unbiased sum type for holding one of two possible values, with a full functional API (Map, Bind, Match, Reduce, TryGet, Deconstruct, equality, Scribe support, and more).

## [0.21.0] - 2026-07-27

### Added

- GUIScope.FontStyle for scoped font style changes.

## [0.20.0] - 2025-09-12

### Added

- LogDevMessage and LogDebug with lambda only that are only called when dev mode is enabled which is useful to avoid doing potentially expensive calculations for the sake of logging when it's not.

## [0.19.0] - 2025-08-29

### Added

- ValueRef fields on Boxed and AnyBoxed type for ref-based access.

## [0.18.0] - 2025-08-27

### Added

- Log\*Once utility logging methods on IlyvionMod that takes a ref bool to only log something once.

## [0.17.0] - 2025-08-19

### Added

- ConditionalWeakTable class that has the same API as 1.6 for use with older RimWorld versions.

## [0.16.0] - 2025-07-30

### Added

- CodeInstructionsExtensions.CallMatches for use with transpilers so you can do things like `i.CallMatches(m => m.Name == "SortBy")`.
- Custom ParentName Handler support, so you can set a Def's ParentName to `::<full path to type that implements ilyvion.Laboratory.ParentNameHandlers.ICustomParentNameHandler>:<the data to pass to the handler's GetBestParentFor's parentNameData parameter>`.
- Custom ParentName Handler that lets you select a parent using XPath; an example of use is `<ThingDef ParentName="::ilyvion.Laboratory.ParentNameHandlers.XPathParent:[defName='GrowthVat']">` which will select the match of `/Def/ThingDef[defName='GrowthVat']`. (This feature isn't well tested and might not yet be ready for production use; use with caution for anything but simple testing.)

## [0.15.2] 2025-07-20

### Changed

- Harmonize the category name used across the mod.

## [0.15.1] 2025-07-02

### Fixed

- Lock Harmony to correct versions for 1.3 and 1.4 and then fix code to be compatible.

## [0.15.0] 2025-06-28

### Added

- Rimworld 1.6 support.

## [0.14.0] 2024-09-12

### Added

- Import some enumerable utilities from https://github.com/LogosBible/Logos.Utility (mainly for the LazyOrderBy methods)

## [0.13.0] 2024-09-07

### Added

- Multi-tick version of the CacheValue class for caching values that take multiple ticks to calculate.

## [0.12.0] 2024-09-04

### Changed

- Multi-tick coroutines now immediately start coroutines that are added while coroutines are already being executed. This prevents an issue where a lot of calls to nested coroutines would postpone the execution of each coroutine by a tick, which unnecessarily paused coroutine execution when it wasn't necessary. If you need to pause execution immediately when a coroutine starts for some reason, you can immediately yield with e.g. ResumeImmediately.Singleton and it won't run proper until the next tick.

## [0.11.0] 2024-08-23

### Added

- Provide DrawIfUIHelpers to automate functionality. Now consumers only have to provide a closure that will get called at the right time, but also won't be called when the mod isn't compiled without the DEBUG symbol, so it becomes effectively free to pepper your code with it where you need it.

## [0.10.0] 2024-08-22

### Added

- Improved tab/tabrecords.
- Util type DoOnDispose.

## [0.9.0] 2024-08-21

### Added

- It is now possible to cancel multi-tick coroutines in the middle of execution.

### Fixed

- Only register required version request when mismatch

## [0.8.0] 2024-08-20

### Changed

- Change the version check mechanism from using a method call to using a VersionCheckDef declared in a Defs XML file. The failing assembly won't even get to load to call VersionCheck.ShowRequiresAtLeastVersionMessageFor if we need it, making it useless, and this also makes it so that mods that only need XML features can still specify a version requirement without having to add a whole assembly jus for that.

## [0.7.0] 2024-08-20

### Added

- PatchOperationFindModById. Does what it says. Alternative to vanilla's PatchOperationFindMod but relies on mod id rather than on mod name, which, at least in theory, is more stable/less likely to change.

## [0.6.0] 2024-08-18

### Added

- MultiTickCoroutineManager: a GameComponent that orchestrates the registration and execution of, as the name suggests, multi-tick coroutines, i.e. tasks that span multiple ticks. Heavily modeled (as a concept, all code original) after the Unity Coroutine type. Makes use of the fact that C# allows you to write IEnumerables using yield (return|break) keywords, allowing natural 'break points' in a task. Created mainly to alleviate per-tick strain in the Colony Manager Redux, but I can see myself making use of this in many other situations going forward where you have too much work to perform for a single tick to handle well.

## [0.5.0] 2024-08-15

### Added

- Version check mechanism. Something to make me more comfortable with releasing mods that depend on this mod; even if something goes wrong, at least the user will get an explanation.
- Import Widgets_Labels class' methods from Colony Manager Redux.
- Import CacheValue(s) classes from Colony Manager Redux.
- Utility methods/types for dealing with arrays and ArrayPools.
- CustomBackCompatibility utility class for doing custom type replacements on game load.
- EnumerableExtensions.MinAndMax for calculating both the min and the max value of an IEnumerable in a single pass.

## [0.4.0] 2024-08-10

### Added

- Extension for muting a color.
- Extension for inline dumping the value of any value during debugging.
- Scriber method for CircularBuffers.

## [0.3.0] 2024-08-04

### Added

- GraphRenderer for rendering graphs.
- GUIScope.Multiple, which serves as a drop-in for Verse.TextBlock, but keep the same API across all three supported RW versions.
- IlyvionDebugActionAttribute that works like the DebugActionAttribute, but has the same API across all three supported RW versions.
- A circular buffer.
- LogDebug method added to IlyvionMod; only logs when the DEBUG symbol is present.
- Various useful extension methods.

## [0.2.0] 2024-08-01

### Added

- Debug action for hot reloading language files.
- Add a "draw UI helpers" debug setting. Does nothing on its own, but is used by dependents to decide whether or not to draw extra UI bits for debugging purposes.
- Import Bradson's GUIScope utility class with adaptions to better fit my needs.

### Changed

- Make it so that the CustomFontManager only patches the font system if it's been enabled. This way it won't affect performance if it goes unused.

### Fixed

- The name ilyvion was misspelled in several places and file paths as 'ilvyion'.

## [0.1.0] 2024-07-25

### Added

- First implementation of the library.

[Unreleased]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.22.0...HEAD
[0.22.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.21.0..v0.22.0
[0.21.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.20.0..v0.21.0
[0.20.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.19.0..v0.20.0
[0.19.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.18.0..v0.19.0
[0.18.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.17.0..v0.18.0
[0.17.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.16.0..v0.17.0
[0.16.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.15.2..v0.16.0
[0.15.2]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.15.1...v0.15.2
[0.15.1]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.15.0...v0.15.1
[0.15.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.14.0...v0.15.0
[0.14.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.13.0...v0.14.0
[0.13.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.12.0...v0.13.0
[0.12.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.11.0...v0.12.0
[0.11.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.10.0...v0.11.0
[0.10.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.9.0...v0.10.0
[0.9.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.8.0...v0.9.0
[0.8.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.7.0...v0.8.0
[0.7.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.6.0...v0.7.0
[0.6.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.5.0...v0.6.0
[0.5.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.4.0...v0.5.0
[0.4.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.3.0...v0.4.0
[0.3.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/ilyvion/ilyvion-laboratory/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/ilyvion/ilyvion-laboratory/releases/tag/v0.1.0
