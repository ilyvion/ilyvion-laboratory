using System.Xml;

namespace ilyvion.Laboratory;

public class PatchOperationFindModById : PatchOperation
{
#pragma warning disable CS0649, CS8618 // Set by reflection
    private List<string> mods;

    private PatchOperation match;

    private PatchOperation nomatch;
#pragma warning restore CS8618, CS0649

    protected override bool ApplyWorker(XmlDocument xml)
    {
        bool matched = false;
        foreach (string mod in mods)
        {
            if (ModsConfig.IsActive(mod))
            {
                matched = true;
                break;
            }
        }
        if (matched)
        {
            if (match != null)
            {
                return match.Apply(xml);
            }
        }
        else if (nomatch != null)
        {
            return nomatch.Apply(xml);
        }
        return true;
    }

    public override string ToString()
    {
        return $"{base.ToString()}({mods.ToCommaList()})";
    }
}
