#if !v1_7_OR_GREATER
namespace ilyvion.Laboratory;

/// <summary>
/// Marks a parameter the method validates as non-null. CA1062 recognizes it by name, including
/// from other assemblies.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class ValidatedNotNullAttribute : Attribute { }
#endif
