using System;

public class VersionDetails
{
  public static VersionDetails BuildDefaultFallbackVersion()
  {
    const string version = "0.1.0";
    // const string preReleaseLabel = "ci";
    const string preReleaseLabel = "";
    // const string revision = "00001";
    const string revision = "";

    string revisionWithDot = !string.IsNullOrEmpty(revision)
      ? $".{revision}"
      : string.Empty;
    string preReleaseTag = !string.IsNullOrEmpty(preReleaseLabel) && !string.IsNullOrEmpty(revisionWithDot)
      ? $"{preReleaseLabel}{revisionWithDot}"
      : string.Empty;
    string preReleaseTagWithDash = !string.IsNullOrEmpty(preReleaseTag)
      ? $"-{preReleaseTag}"
      : string.Empty;

    return new VersionDetails
    {
      PackageVersionPrefix = version,
      PackageVersionSuffix = preReleaseTag,
      Version = $"{version}{preReleaseTagWithDash}{revisionWithDot}",
      AssemblyVersion = $"{version}{revisionWithDot}",
      FileVersion = $"{version}{revisionWithDot}",
      InformationalVersion = $"{version}{preReleaseTagWithDash}"
    };
  }

  public required string AssemblyVersion { get; init; }

  public required string FileVersion { get; init; }

  public required string InformationalVersion { get; init; }

  public required string PackageVersionPrefix { get; init; }

  public required string PackageVersionSuffix { get; init; }

  public required string Version { get; init; }
}
