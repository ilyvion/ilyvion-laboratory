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
        TypeReplacements.Add(baseType, newBaseType);
    }

    public static void RegisterProvidedClassNameReplacement(string providedClassName, Type newType)
    {
        ProvidedClassNameReplacements.Add(providedClassName, newType);
    }

    public static void RegisterCustomReplacement(GetBackCompatibleType customReplacement)
    {
        CustomReplacements.Add(customReplacement);
    }

    public static void RegisterBaseTypeDummy(Type baseType)
    {
        TypeReplacements.Add(baseType, typeof(ExposableDummy));
    }

    public static void RegisterProvidedClassNameDummy(string providedClassName)
    {
        ProvidedClassNameReplacements.Add(providedClassName, typeof(ExposableDummy));
    }
}
