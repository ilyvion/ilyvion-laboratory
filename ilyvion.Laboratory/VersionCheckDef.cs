namespace ilyvion.Laboratory;

public class VersionCheckDef : Def
{
    public int majorVersion = -1;
    public int minorVersion = -1;
    public string? modName;

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (string item in base.ConfigErrors())
        {
            yield return item;
        }

        if (majorVersion == -1)
        {
            yield return $"{nameof(majorVersion)} is not set";
        }

        if (minorVersion == -1)
        {
            yield return $"{nameof(minorVersion)} is not set";
        }

        if (modName == null)
        {
            yield return $"{nameof(modName)} is not set";
        }
    }
}
