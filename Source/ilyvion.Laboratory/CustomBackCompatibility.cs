using System.Xml;

namespace ilyvion.Laboratory;

public delegate Type? GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node);

public static partial class CustomBackCompatibility
{
    internal static Dictionary<Type, Type> TypeReplacements = [];
    internal static Dictionary<string, Type> ProvidedClassNameReplacements = [];
    internal static List<GetBackCompatibleType> CustomReplacements = [];

    public static void RegisterBaseTypeReplacement(Type baseType, Type newBaseType)
    {
        if (TypeReplacements.ContainsKey(baseType))
        {
            Logger.LogWarning(
                $"A base type replacement for '{baseType}' is already registered; overwriting it."
            );
        }
        TypeReplacements[baseType] = newBaseType;
    }

    public static void RegisterProvidedClassNameReplacement(string providedClassName, Type newType)
    {
        if (ProvidedClassNameReplacements.ContainsKey(providedClassName))
        {
            Logger.LogWarning(
                $"A provided class name replacement for '{providedClassName}' is already "
                    + "registered; overwriting it."
            );
        }
        ProvidedClassNameReplacements[providedClassName] = newType;
    }

    public static void RegisterCustomReplacement(GetBackCompatibleType customReplacement) =>
        CustomReplacements.Add(customReplacement);

    public static void RegisterBaseTypeDummy(Type baseType) =>
        RegisterBaseTypeReplacement(baseType, typeof(ExposableDummy));

    public static void RegisterProvidedClassNameDummy(string providedClassName) =>
        RegisterProvidedClassNameReplacement(providedClassName, typeof(ExposableDummy));
}
