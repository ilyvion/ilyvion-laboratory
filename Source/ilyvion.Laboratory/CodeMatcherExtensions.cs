// CodeMatcherExtensions.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

namespace ilyvion.Laboratory;

/// <summary>
/// Extensions for <see cref="CodeMatcher"/> that throw <see cref="TranspilerMismatchException"/>
/// when a search finds nothing, for use inside <see cref="TranspilerUtility.PatchOrOriginal"/>.
/// </summary>
[SinceVersion(0, 27, 0)]
public static class CodeMatcherExtensions
{
    /// <summary>
    /// Searches forward for an instruction matching <paramref name="predicate"/>.
    /// </summary>
    /// <param name="codeMatcher">The code matcher.</param>
    /// <param name="predicate">The predicate the instruction must match.</param>
    /// <param name="expected">
    /// What is searched for, e.g. <c>call to Widgets.Label</c>; the exception's detail is this
    /// followed by <c> not found.</c>
    /// </param>
    /// <exception cref="TranspilerMismatchException">No instruction matches.</exception>
    public static CodeMatcher ExpectForward(
        this CodeMatcher codeMatcher,
        Func<CodeInstruction, bool> predicate,
        string expected
    )
    {
        ArgumentNullException.ThrowIfNull(codeMatcher);

        return ThrowIfInvalid(codeMatcher.SearchForward(predicate), expected);
    }

    /// <summary>
    /// Searches backwards for an instruction matching <paramref name="predicate"/>.
    /// </summary>
    /// <inheritdoc cref="ExpectForward"/>
    public static CodeMatcher ExpectBackwards(
        this CodeMatcher codeMatcher,
        Func<CodeInstruction, bool> predicate,
        string expected
    )
    {
        ArgumentNullException.ThrowIfNull(codeMatcher);

        return ThrowIfInvalid(codeMatcher.SearchBackwards(predicate), expected);
    }

    private static CodeMatcher ThrowIfInvalid(CodeMatcher codeMatcher, string expected) =>
        codeMatcher.IsValid
            ? codeMatcher
            : throw new TranspilerMismatchException($"{expected} not found.");
}
