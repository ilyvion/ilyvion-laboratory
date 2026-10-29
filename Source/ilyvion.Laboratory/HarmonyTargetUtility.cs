// HarmonyTargetUtility.cs
// Copyright (c) 2026 Alexander Krivács Schrøder

namespace ilyvion.Laboratory;

/// <summary>
/// Helpers for finding the methods a Harmony patch targets.
/// </summary>
[SinceVersion(0, 27, 0)]
public static class HarmonyTargetUtility
{
    /// <summary>
    /// Returns the methods declared on <paramref name="type"/> or on any type nested in it, at
    /// any depth, whose name contains every one of <paramref name="nameFragments"/>. Nested
    /// types include compiler-generated ones, such as the classes holding lambdas, local
    /// functions and iterators.
    /// </summary>
    public static List<MethodInfo> FindMethods(Type type, params string[] nameFragments)
    {
        ArgumentNullException.ThrowIfNull(nameFragments);

        return FindMethods(
            type,
            method =>
                nameFragments.All(fragment =>
#if v1_6_OR_GREATER
                    method.Name.Contains(fragment, StringComparison.Ordinal)
#else
                    method.Name.IndexOf(fragment, StringComparison.Ordinal) >= 0
#endif
                )
        );
    }

    /// <summary>
    /// Returns the methods declared on <paramref name="type"/> or on any type nested in it, at
    /// any depth, that match <paramref name="predicate"/>. Nested types include
    /// compiler-generated ones, such as the classes holding lambdas, local functions and
    /// iterators.
    /// </summary>
    public static List<MethodInfo> FindMethods(Type type, Func<MethodInfo, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(predicate);

        List<MethodInfo> methods = [];
        Stack<Type> types = new([type]);
        while (types.Count > 0)
        {
            var current = types.Pop();
            methods.AddRange(AccessTools.GetDeclaredMethods(current).Where(predicate));
            foreach (var nested in current.GetNestedTypes(AccessTools.all))
            {
                types.Push(nested);
            }
        }
        return methods;
    }

    /// <summary>
    /// Returns whether <paramref name="methods"/> holds exactly one method. Otherwise logs
    /// <c>Could not {target}, expected exactly one target method but found {count}</c>, with
    /// the methods found, through <paramref name="logError"/> and returns <see langword="false"/>.
    /// Meant to be returned from a patch's <c>[HarmonyPrepare]</c> method, with the same
    /// methods its <c>[HarmonyTargetMethods]</c> method returns, so the patch is skipped instead
    /// of failing or patching the wrong method when the target can't be told apart.
    /// </summary>
    /// <param name="methods">The candidate target methods.</param>
    /// <param name="target">
    /// What is being patched, including the verb, e.g. <c>patch Mineable.TrySpawnYield</c>.
    /// </param>
    /// <param name="logError">Where to log the error, e.g. the mod's <c>LogError</c>.</param>
    public static bool ExpectSingle(
        IEnumerable<MethodBase> methods,
        string target,
        Action<string> logError
    )
    {
        ArgumentNullException.ThrowIfNull(methods);
        ArgumentNullException.ThrowIfNull(logError);

        var list = methods.ToList();
        if (list.Count == 1)
        {
            return true;
        }

        var found =
            list.Count == 0
                ? ""
                : ": "
                    + string.Join(
                        ", ",
                        list.Select(m => $"{m.DeclaringType?.FullDescription()}.{m.Name}")
                    );
        logError(
            $"Could not {target}, expected exactly one target method but found {list.Count}{found}"
        );
        return false;
    }
}
