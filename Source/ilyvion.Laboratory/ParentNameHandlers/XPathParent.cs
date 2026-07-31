using System.Xml;

namespace ilyvion.Laboratory.ParentNameHandlers;

public class XPathParent : ICustomParentNameHandler
{
    public XmlNode? GetBestParentFor(XmlNode node, string xpath, List<XmlNode> allRegisteredNodes)
    {
        var resolvedXPath = $"/Defs/{node.Name}{xpath}";
        XmlNodeList matches;
        try
        {
            matches = node.OwnerDocument.SelectNodes(resolvedXPath);
        }
        catch (Exception e)
        {
            Logger.LogError(
                $"Error resolving XPath {resolvedXPath} for node: \"{node.Name}\". "
                    + $"Full node: \"{node.OuterXml}\"; Exception: {e}"
            );
            return null;
        }
        if (matches.Count == 0)
        {
            Logger.LogError(
                $"No matches found for ilyvion.XPathParent using provided XPath fragment "
                    + $"'{xpath}' for node: \"{node.Name}\". "
                    + $"Resolved XPath: '{resolvedXPath}' "
                    + $"Full node: \"{node.OuterXml}\""
            );
            return null;
        }
        else if (matches.Count > 1)
        {
            Logger.LogError(
                $"Multiple matches found for ilyvion.XPathParent using provided XPath fragment "
                    + $"'{xpath}' for node: \"{node.Name}\". "
                    + $"Resolved XPath: '{resolvedXPath}' "
                    + $"Full node: \"{node.OuterXml}\"; list of matches:\n- {string.Join("\n- ", matches.Cast<XmlNode>().Select(n => n.OuterXml))}"
            );
            return null;
        }

        return matches[0];
    }
}
