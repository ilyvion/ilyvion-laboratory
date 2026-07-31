namespace ilyvion.Laboratory;

[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
#pragma warning disable CA1019 // Define accessors for attribute arguments
public sealed class SinceVersionAttribute(int major, int minor, int build) : Attribute
#pragma warning restore CA1019 // Define accessors for attribute arguments
{
    public Version Version { get; } = new Version(major, minor, build);
}
