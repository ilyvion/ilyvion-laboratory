namespace ilyvion.Laboratory;

[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class SinceVersionAttribute(int major, int minor, int build) : Attribute
{
    public Version Version { get; } = new Version(major, minor, build);
}
