using System.Diagnostics;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: DllMetadataVersion <assembly-path>");
    return 2;
}

var version = FileVersionInfo.GetVersionInfo(args[0]).FileVersion;
if (string.IsNullOrWhiteSpace(version))
{
    Console.Error.WriteLine($"The DLL has no FileVersion property: {args[0]}");
    return 1;
}

Console.WriteLine(version);
return 0;
