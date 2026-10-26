// InjectCallBeforeReturnException.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

namespace ilyvion.Laboratory;

#pragma warning disable CA1032 // Implement standard exception constructors
/// <summary>
/// Exception thrown when <see cref="TranspilerUtility.InjectCallBeforeReturn"/> fails to inject
/// its call.
/// </summary>
/// <param name="instructions">The instructions at the point the exception was thrown.</param>
[Serializable]
[SinceVersion(0, 27, 0)]
public class InjectCallBeforeReturnException(IEnumerable<CodeInstruction> instructions)
    : Exception()
{
    /// <summary>
    /// Gets the instructions at the point the exception was thrown.
    /// </summary>
    public IEnumerable<CodeInstruction> Instructions { get; } = instructions;
}
#pragma warning restore CA1032 // Implement standard exception constructors
