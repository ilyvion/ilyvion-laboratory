using System.Xml;
using ilyvion.Laboratory.ParentNameHandlers;
using Logger = ilyvion.Laboratory.Logger;
using XmlInheritanceNode = Verse.XmlInheritance.XmlInheritanceNode;

namespace ilyvion.LaboratoryMod;

[HarmonyPatch(typeof(XmlInheritance), nameof(XmlInheritance.GetBestParentFor))]
internal static class XmlInheritance_GetResolvedNodeFor_Patches
{
    private static readonly Dictionary<
        string,
        ICustomParentNameHandler
    > _customParentNameHandlers = [];

    private static bool Prefix(
        ref XmlInheritanceNode? __result,
        XmlInheritanceNode node,
        string parentName
    )
    {
        if (!parentName.StartsWith("::"))
        {
            return true; // Continue with the original method
        }

        parentName = parentName["::".Length..];
        var endOfTypeName = parentName.IndexOf(':');
        if (endOfTypeName == -1)
        {
            Logger.LogWarning(
                $"Encountered a 'ParentName' that starts with '::', but which does not follow the expected custom parent name handler format: '{parentName}'. Will assume it's something else and ignore it."
            );
            return true; // Continue with the original method
        }
        var typeName = parentName[..endOfTypeName];

        if (!_customParentNameHandlers.TryGetValue(typeName, out var handler))
        {
            var type = AccessTools.TypeByName(typeName);
            var customParentNameHandler = Activator.CreateInstance(type);
            if (customParentNameHandler is not ICustomParentNameHandler)
            {
                Logger.LogError(
                    $"Encountered a 'ParentName' that starts with '::', but the type provided, '{typeName}' ({type.FullName}) does not implement the ICustomParentNameHandler interface."
                );
                __result = null;
                return false; // Skip the original method
            }

            // Cache the handler for later use
            handler = (ICustomParentNameHandler)customParentNameHandler;
            _customParentNameHandlers[typeName] = handler;
        }

        var restOfParentName = parentName[(endOfTypeName + 1)..];
        try
        {
            var resolvedNode = handler.GetBestParentFor(
                node.xmlNode,
                restOfParentName,
                XmlInheritance_TryRegister_Patches.allRegisteredNodes
            );
            if (resolvedNode == null)
            {
                Logger.LogError(
                    $"Custom parent name handler {handler.GetType().FullName} returned null for ParentName='{parentName}' for node: \"{node.xmlNode.Name}\". Full node: \"{node.xmlNode.OuterXml}\""
                );
                __result = null;
                return false; // Skip the original method
            }

            // See if the node is already registered in the XmlInheritance system. If not, just make a new one.
            __result =
                XmlInheritance.unresolvedNodes.Find(n => n.xmlNode == resolvedNode)
                ?? new() { xmlNode = resolvedNode };
            return false; // Skip the original method
        }
        catch (Exception e)
        {
            Logger.LogError(
                $"Error while resolving custom parent name using {handler.GetType().FullName} (triggered by ParentName='{parentName}') for node: \"{node.xmlNode.Name}\". Full node: \"{node.xmlNode.OuterXml}\".\nException: {e}"
            );
            __result = null;
            return false; // Skip the original method
        }
    }
}

[HarmonyPatch(typeof(XmlInheritance), nameof(XmlInheritance.TryRegister))]
internal static class XmlInheritance_TryRegister_Patches
{
    internal static List<XmlNode> allRegisteredNodes = [];

    private static void Prefix(XmlNode node)
    {
        allRegisteredNodes.Add(node);
    }
}
