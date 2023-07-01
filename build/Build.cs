using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.ReportGenerator;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;

[GitHubActions("continuous",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    FetchDepth = 0,
    OnPushBranches = new[]
    {
        "main",
        "develop",
        "releases/**"
    },
    OnPullRequestBranches = new[]
    {
        "releases/**"
    },
    InvokedTargets = new[]
    {
        nameof(PackSolution),
    },
    PublishArtifacts = true,
    EnableGitHubToken = true,
    ImportSecrets = new[]
    {
        nameof(NuGetApiKey)
    }
)]
class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.ContinuousIntegration);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("NuGet publish url. Default is 'https://api.nuget.org/v3/index.json'")] string NuGetFeed = "https://api.nuget.org/v3/index.json";
    [Parameter("NuGet api key. Default is ''"), Secret] readonly string NuGetApiKey;

    [Solution] readonly Solution Solution;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath BuildDirectory => RootDirectory / ".build";
    AbsolutePath TestResultsDirectory => BuildDirectory / "tests";
    AbsolutePath CoverageDirectory => BuildDirectory / "coverage";
    AbsolutePath NuGetDirectory => BuildDirectory / "nuget";

    [GitRepository] readonly GitRepository GitRepository;

    [GitVersion] readonly GitVersion GitVersion;

    GitHubActions GitHubActions => GitHubActions.Instance;

    string GithubNugetFeed => GitHubActions != null
         ? $"https://nuget.pkg.github.com/{GitHubActions.RepositoryOwner??string.Empty}/index.json"
         : string.Empty;

    Target RestoreTools => _ => _
        .Description("Restore tools")
        .Executes(() =>
        {
            DotNet("tool restore");
        });

    Target CleanSolution => _ => _
        .DependsOn(RestoreTools)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(p => p.DeleteDirectory());
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(p => p.DeleteDirectory());
            BuildDirectory.CreateOrCleanDirectory();
            DotNetClean();
        });

    Target RestoreSolution => _ => _
        .DependsOn(CleanSolution)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target CompileSolution => _ => _
        .DependsOn(RestoreSolution)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetVersion(GitVersion.NuGetVersionV2)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .EnableNoRestore());
        });

    Target ExecuteUnitTests => _ => _
        .DependsOn(CompileSolution)
        .Executes(() =>
        {
            IEnumerable<Project> GetProjects()
            {
                return Solution.GetAllProjects("*.Tests");
            }

            IEnumerable<Project> projects = GetProjects();
            foreach (Project project in projects)
            {
                foreach (string fw in project.GetTargetFrameworks())
                {
                    DotNetTest(c => c
                        .SetProjectFile(project.Path)
                        .SetConfiguration(Configuration.ToString())
                        .SetFramework(fw)
                        .SetResultsDirectory(TestResultsDirectory)
                        .SetProcessWorkingDirectory(Directory.GetParent(project).FullName)
                        .SetLoggers("console;verbosity=normal", "trx")
                        .EnableNoBuild()
                        .SetProcessArgumentConfigurator(_ => _
                            .Add("--collect:\"XPlat Code Coverage\"")
                            .Add($"--settings \"{TestsDirectory / "coverlet.runsettings"}\"")
                            .Add("--")
                            .Add("NUnit.DisplayName=FullName")
                        )
                    );
                }
            }
        });

    Target GenerateCoverageReports => _ => _
        .DependsOn(ExecuteUnitTests)
        .Executes(() =>
        {
            IEnumerable<string> coverageReportFiles =
                TestResultsDirectory.GlobFiles("*/coverage.cobertura.xml").Select(f => (string)f);
            ReportGenerator(c => c
                .SetReports(coverageReportFiles)
                .SetTargetDirectory(CoverageDirectory)
                .SetReportTypes(ReportTypes.HtmlSummary, ReportTypes.Cobertura, ReportTypes.SonarQube,
                    ReportTypes.HtmlInline_AzurePipelines_Dark, ReportTypes.MarkdownSummary,
                    "MarkdownSummaryGithub", ReportTypes.Badges)
                .SetFramework("net7.0")
            );
        });

    Target GenerateReports => _ => _
        .DependsOn(GenerateCoverageReports);

    Target PackSolution => _ => _
        .Requires(() => Configuration.Equals(Configuration.Release))
        .Produces(NuGetDirectory / "*.nupkg", NuGetDirectory / "*.snupkg")
        .DependsOn(GenerateReports)
        .Triggers(PublishToGithub, PublishToNuGet)
        .Executes(() =>
        {
            DotNetPack(s => s
                .SetOutputDirectory(NuGetDirectory)
                .SetConfiguration(Configuration)
                .SetVersion(GitVersion.NuGetVersionV2)
                .EnableNoRestore());
        });

    Target PublishToGithub => _ => _
       .Description($"Publishing to Github for Development only.")
       .Requires(() => Configuration.Equals(Configuration.Release))
       .OnlyWhenStatic(() => GitRepository.IsOnDevelopBranch() || (GitHubActions != null && GitHubActions.IsPullRequest))
       .Executes(() =>
       {
           NuGetDirectory.GlobFiles("*.nupkg")
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

    Target PublishToNuGet => _ => _
       .Description($"Publishing to NuGet with the version.")
       .Requires(() => Configuration.Equals(Configuration.Release))
       .OnlyWhenStatic(() => GitRepository.IsOnMainOrMasterBranch())
       .Executes(() =>
       {
           NuGetDirectory.GlobFiles("*.nupkg")
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

    Target ContinuousIntegration => _ => _
        .DependsOn(GenerateCoverageReports);

    Target ContinuousDeploy => _ => _
        .DependsOn(PackSolution);
}