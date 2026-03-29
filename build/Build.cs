using System;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.ReportGenerator;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;
using static Serilog.Log;

[GitHubActions("continuous",
  GitHubActionsImage.UbuntuLatest,
  AutoGenerate = false,
  FetchDepth = 0,
  OnPushBranches = new[]
  {
    "main",
    "develop",
    "releases/**",
    "feature/**"
  },
  OnPullRequestBranches = new[]
  {
    "releases/**"
  },
  InvokedTargets = new[]
  {
    nameof(ContinuousIntegration),
  },
  PublishArtifacts = true,
  EnableGitHubToken = true,
  ImportSecrets = new[]
  {
    nameof(NuGetApiKey),
    nameof(CoverallsRepoToken)
  }
)]
class Build : NukeBuild
{
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Output directory for build artifacts")]
    readonly AbsolutePath BuildDirectory = RootDirectory / ".build";

    [Parameter("NuGet publish url. Default is 'https://api.nuget.org/v3/index.json'")] string NuGetFeed = "https://api.nuget.org/v3/index.json";

    [Parameter("NuGet api key. Default is ''"), Secret] readonly string NuGetApiKey;

    [Parameter("coveralls.io repo token. Default is ''"), Secret] readonly string CoverallsRepoToken;

    [Parameter("Skip tests")]
    readonly bool SkipTests = false;

    [Parameter("Enable coverage")]
    readonly bool EnableCoverage = true;

    [Parameter("Enable reports")]
    readonly bool EnableReports = true;

    [Solution] readonly Solution Solution;

    [GitRepository]
    readonly GitRepository GitRepository;

    [GitVersion(NoFetch = true, NoCache = true)]
    readonly GitVersion GitVersion;

    GitHubActions GitHubActions => GitHubActions.Instance;

    string BranchSpec => GitHubActions?.Ref;

    string BuildNumber => GitHubActions?.RunNumber.ToString();

    VersionDetails VersionDetails;

    AbsolutePath SrcDirectory => RootDirectory / "src";

    AbsolutePath TestsDirectory => RootDirectory / "tst";

    AbsolutePath TestResultsDirectory => BuildDirectory / "test-results";

    AbsolutePath TestResultFile => TestResultsDirectory / "test-result.xml";

    AbsolutePath ReportsDirectory => BuildDirectory / "reports";

    AbsolutePath CoverageReportDirectory => ReportsDirectory / "coverage";

    AbsolutePath PublishDirectory => BuildDirectory / "publish";

    AbsolutePath PublishNugetDirectory => PublishDirectory / "nuget";

    string GithubNugetFeed => GitHubActions != null
      ? $"https://nuget.pkg.github.com/{GitHubActions.RepositoryOwner??string.Empty}/index.json"
      : string.Empty;

    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.ContinuousIntegration);

    Target GenerateVersionDetails => _ => _
      .Executes(() =>
      {
        try
        {
          bool havePreReleaseLabel = !string.IsNullOrEmpty(GitVersion.PreReleaseLabel);

          VersionDetails = new VersionDetails
          {
            PackageVersionPrefix = GitVersion.MajorMinorPatch,
            PackageVersionSuffix = havePreReleaseLabel ? GitVersion.PreReleaseTag : string.Empty,
            Version = GitVersion.SemVer,
            AssemblyVersion = GitVersion.AssemblySemVer,
            FileVersion = GitVersion.AssemblySemFileVer,
            InformationalVersion = GitVersion.InformationalVersion,
          };
        }
        catch
        {
          VersionDetails = VersionDetails.BuildDefaultFallbackVersion();
        }

        Information("PackageVersionPrefix = {packageVersionPrefix} / PackageVersionSuffix = {packageVersionSuffix} / Version = {version} / AssemblyVersion = {assemblyVersion} / FileVersion = {fileVersion} / InformationalVersion = {informationalVersion}",
          VersionDetails.PackageVersionPrefix, VersionDetails.PackageVersionSuffix,
          VersionDetails.Version, VersionDetails.AssemblyVersion,
          VersionDetails.FileVersion, VersionDetails.InformationalVersion
        );
      });

    Target Clean => d => d
        .Before(Restore)
        .Executes(() =>
        {
            foreach (AbsolutePath directory in (AbsolutePath[]) [SrcDirectory, TestsDirectory])
            {
                directory.GlobDirectories("**/bin", "**/obj").ForEach(d => d.DeleteDirectory());
            }

            BuildDirectory.CreateOrCleanDirectory();
            TestResultsDirectory.CreateOrCleanDirectory();
            ReportsDirectory.CreateOrCleanDirectory();
            CoverageReportDirectory.CreateOrCleanDirectory();
            PublishDirectory.CreateOrCleanDirectory();
            PublishNugetDirectory.CreateOrCleanDirectory();
        });

    Target Restore => d => d
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution))
                ;
        });

    Target RestoreTools => d => d
      .Before(Clean)
      .Executes(() =>
      {
        DotNetToolRestore()
          ;
      });

    Target Compile => d => d
        .DependsOn(GenerateVersionDetails)
        .DependsOn(Clean)
        .DependsOn(Restore)
        .Executes(() =>
        {
          ReportSummary(s => s
            .WhenNotNull(VersionDetails, (summary, versionDetails) => summary
              .AddPair("PackageVersionPrefix", versionDetails.PackageVersionPrefix)
              .AddPair("PackageVersionSuffix", versionDetails.PackageVersionSuffix)
              .AddPair("Version", versionDetails.Version)
              .AddPair("AssemblyVersion", versionDetails.AssemblyVersion)
              .AddPair("FileVersion", versionDetails.FileVersion)
              .AddPair("InformationalVersion", versionDetails.InformationalVersion)
            ));

          DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoLogo()
                .EnableNoRestore()
                .SetVersion(VersionDetails.Version)
                .SetAssemblyVersion(VersionDetails.AssemblyVersion)
                .SetFileVersion(VersionDetails.FileVersion)
                .SetInformationalVersion(VersionDetails.InformationalVersion)
          );
        });

    Target Test => d => d
        .DependsOn(Compile)
        .OnlyWhenDynamic(() => !SkipTests)
        .Executes(() =>
        {
          string dataCollector = EnableCoverage ? "XPlat Code Coverage" : null; // coverlet.collector

          DotNetTest(_ => _
            .SetProjectFile(Solution)
            .SetConfiguration(Configuration)
            .EnableNoRestore()
            .EnableNoBuild()
            .When(TestResultsDirectory.DirectoryExists(), _ => _.SetResultsDirectory(TestResultsDirectory))
            .When(!string.IsNullOrEmpty(dataCollector), t => t
              .SetDataCollector(dataCollector)
              .SetProcessAdditionalArguments(
                  "-- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura",
                  "-- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Include=\"[ConfirmStep.Net]*\"",
                  "-- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Exclude=\"[*.Tests]*\""
                )
              )
            );

            if (EnableCoverage)
            {
                DotNet($"coverage merge --output-format cobertura --output \"{TestResultFile}\" \"{TestResultsDirectory}/**/*.cobertura.xml\"");
            }
        });

    Target ReportCoverage => d => d
      .DependsOn(Test)
      .OnlyWhenDynamic(() => EnableReports && EnableCoverage)
      .Executes(() =>
      {
        ReportGenerator(s => s
          .AddReports(TestResultFile)
          .AddReportTypes(ReportTypes.lcov, ReportTypes.Cobertura, ReportTypes.Html, ReportTypes.HtmlChart)
          .SetTargetDirectory(CoverageReportDirectory)
          .AddFileFilters("-*.g.cs"));

        if (!string.IsNullOrEmpty(CoverallsRepoToken))
        {
          DotNet($"csmacnz.Coveralls --lcov -i \"{CoverageReportDirectory}/lcov.info\" --useRelativePaths --basePath \"{RootDirectory}\"");
        }

        string link = ReportsDirectory / "index.html";
        Information($"Code coverage report: \x1b]8;;file://{link.Replace('\\', '/')}\x1b\\{link}\x1b]8;;\x1b\\");
      });

    Target Reports => d => d
        .OnlyWhenDynamic(() => EnableReports)
        .DependsOn(ReportCoverage)
        ;

    Target Pack => d => d
        .DependsOn(Reports)
        .Produces(PublishNugetDirectory / "*.nupkg")
        .Triggers(Push)
        .Executes(() =>
        {
          foreach (Project project in Solution.AllProjects)
          {
            bool isPackable = project.GetProperty<bool>("IsPackable");

            if (!isPackable)
            {
              continue;
            }

            DotNetPack(s => s
              .SetProject(project)
              .SetConfiguration(Configuration)
              .SetOutputDirectory(PublishNugetDirectory)
              .When(string.IsNullOrEmpty(VersionDetails.PackageVersionSuffix), _ => _
                .SetVersion(VersionDetails.PackageVersionPrefix)
              )
              .When(!string.IsNullOrEmpty(VersionDetails.PackageVersionSuffix), _ => _
                .SetVersionPrefix(VersionDetails.PackageVersionPrefix)
                .SetVersionSuffix(VersionDetails.PackageVersionSuffix)
              )
              .EnableNoRestore()
              .EnableNoBuild()
            );
          }

        })
    ;

    Target PushToGithub => _ => _
      .Description($"Publishing to Github for Development only.")
      .Requires(() => Configuration.Equals(Configuration.Release))
      .DependsOn(Pack)
      .OnlyWhenStatic(() => !string.IsNullOrEmpty(GitHubActions?.Token) &&
                            (GitRepository.IsOnDevelopBranch() || (GitHubActions != null && GitHubActions.IsPullRequest)))
      .Executes(() =>
      {
        PublishNugetDirectory.GlobFiles("*.nupkg")
          .ForEach(x =>
          {
            DotNetNuGetPush(s => s
              .SetTargetPath(x)
              .SetSource(GithubNugetFeed)
              .SetApiKey(GitHubActions.Token)
              .EnableSkipDuplicate()
            );
          });
      });

    Target PushToNuGet => _ => _
      .Description($"Publishing to NuGet with the version.")
      .Requires(() => Configuration.Equals(Configuration.Release))
      .DependsOn(Pack)
      .OnlyWhenStatic(() => !string.IsNullOrEmpty(NuGetApiKey) &&
                            (GitRepository.IsOnMainOrMasterBranch() || IsTag))
      .Executes(() =>
      {
        PublishNugetDirectory.GlobFiles("*.nupkg")
          .ForEach(x =>
          {
            DotNetNuGetPush(s => s
              .SetTargetPath(x)
              .SetSource(NuGetFeed)
              .SetApiKey(NuGetApiKey)
              .EnableSkipDuplicate()
            );
          });
      });

    Target Push => d => d
      .DependsOn(PushToGithub)
      .DependsOn(PushToNuGet)
    ;

    Target ContinuousIntegration => _ => _
      .DependsOn(RestoreTools, Clean, Restore, Compile, Test, Reports, Pack, Push)
    ;

    bool IsPullRequest => GitHubActions?.IsPullRequest ?? false;

    bool IsTag => BranchSpec != null && BranchSpec.Contains("refs/tags", StringComparison.OrdinalIgnoreCase);
}
