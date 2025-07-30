using System.Xml;

namespace ilyvion.Laboratory.ParentNameHandlers;

public interface ICustomParentNameHandler
{
    XmlNode? GetBestParentFor(XmlNode node, string parentNameData, List<XmlNode> allRegisteredNodes);
}