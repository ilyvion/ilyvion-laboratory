// TranspilerMismatchException.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

namespace ilyvion.Laboratory;

#pragma warning disable CA1032 // Implement standard exception constructors
/// <summary>
/// Exception thrown by a transpiler body run through
/// <see cref="TranspilerUtility.PatchOrOriginal"/> when the IL does not match its expectations.
/// <see cref="TranspilerUtility.PatchOrOriginal"/> catches it, logs <see cref="Detail"/> and
/// returns the original instructions.
/// </summary>
/// <param name="detail">What was expected but not found, e.g. <c>call to Widgets.Label not found.</c></param>
[Serializable]
[SinceVersion(0, 27, 0)]
public class TranspilerMismatchException(string detail) : Exception(detail)
{
    /// <summary>
    /// Gets what was expected but not found.
    /// </summary>
    public string Detail { get; } = detail;
}
#pragma warning restore CA1032 // Implement standard exception constructors
