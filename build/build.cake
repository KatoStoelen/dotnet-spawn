#tool "nuget:?package=GitVersion.CommandLine&version=5.8.2"
#addin "nuget:?package=Cake.Git&version=2.0.0"

#load "utils/pretty-output.cake"

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var _target = Argument("target", "Pack");
var _configuration = Argument("configuration", "Debug");
var _nugetFeed = Argument<string>("nuget-feed", null);
var _nugetFeedApiKey = Argument<string>("nuget-apikey", null);
var _buildCounter = Argument("build-counter", 1);

///////////////////////////////////////////////////////////////////////////////
// VARIABLES
///////////////////////////////////////////////////////////////////////////////

var _rootDir = Directory("..");
var _srcDir = _rootDir + Directory("src");
var _testsDir = _rootDir + Directory("tests");
var _artifactsDir = _rootDir + Directory("artifacts");
var _solutionFile = GetFiles($"{_rootDir}/*.sln").SingleOrDefault() ??
                    throw new InvalidOperationException("Did not find the solution file");

Func<DotNetMSBuildSettings> _getDefaultMSBuildSettings =
    () => new DotNetMSBuildSettings
    {
        NoLogo = true,
        Verbosity = DotNetVerbosity.Minimal
    };

var _buildContext = GetBuildContext();

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(_ =>
{
    Info($"Branch: {_buildContext.BranchName}");
    Info($"Is CI? {_buildContext.IsCI}");
    Info($"Version: {_buildContext.Version}");
    Info($"Informational Version: {_buildContext.InformationalVersion}");
    Info($"Configuration: {_configuration}");

    if (_buildContext.IsAzurePipelinesBuild)
    {
        AzurePipelines.Commands.UpdateBuildNumber(_buildContext.Version);
    }
});

Teardown(_ =>
{
    if (_buildContext.IsAzurePipelinesBuild)
    {
        var testResultsFiles = GetFiles($"{_testsDir}/**/*.trx").ToList();

        if (testResultsFiles.Any())
        {
            AzurePipelines.Commands.PublishTestResults(new AzurePipelinesPublishTestResultsData
            {
                TestRunTitle = _buildContext.Version,
                TestRunner = AzurePipelinesTestRunnerType.VSTest,
                Configuration = _configuration,
                TestResultsFiles = testResultsFiles
            });
        }
    }
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Plugins")
    .IsDependentOn("Clean-Artifacts")
    .Does(() =>
{
    var plugins = new[]
    {
        "DotnetSpawn.Bitbucket",
        "DotnetSpawn.DotnetCLI",
        "DotnetSpawn.OctopusDeploy",
        "DotnetSpawn.TeamCity",
        "TestPlugin"
    };

    var feedDir = _artifactsDir + Directory("feed");

    foreach (var plugin in plugins)
    {
        var csproj = _srcDir + Directory(plugin) + File($"{plugin}.csproj");

        DotNetPublish(csproj, new DotNetPublishSettings
        {
            Configuration = _configuration,
            MSBuildSettings = _getDefaultMSBuildSettings()
                .SetVersion(_buildContext.Version)
                .SetInformationalVersion(_buildContext.InformationalVersion)
        });

        DotNetPack(csproj, new DotNetPackSettings
        {
            Configuration = _configuration,
            OutputDirectory = feedDir,
            NoRestore = true,
            NoBuild = true,
            MSBuildSettings = _getDefaultMSBuildSettings()
                .SetVersion(_buildContext.Version)
                .WithProperty("RepositoryCommit", GitLogTip(_rootDir).Sha)
                .WithProperty("RepositoryBranch", _buildContext.BranchName)
                // .WithProperty("PackageReleaseNotes", releaseNotes)
                .WithProperty("Copyright", $"Copyright © Kato Stoelen {DateTime.Today.Year}")
        });
    }
});

Task("Clean")
    .Does(() =>
{
    Info($"Cleaning {Relative(_solutionFile)}");

    DotNetClean(_solutionFile.FullPath, new DotNetCleanSettings
    {
        Configuration = _configuration,
        Verbosity = DotNetVerbosity.Quiet,
        MSBuildSettings = _getDefaultMSBuildSettings()
    });
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    Info($"Building {Relative(_solutionFile)}");

    DotNetBuild(_solutionFile.FullPath, new DotNetBuildSettings
    {
        Configuration = _configuration,
        MSBuildSettings = _getDefaultMSBuildSettings()
            .SetVersion(_buildContext.Version)
            .SetInformationalVersion(_buildContext.InformationalVersion)
            .WithProperty("ContinuousIntegrationBuild", _buildContext.IsCI.ToString().ToLower())
            .WithProperty("Copyright", $"Copyright © Kato Stoelen {DateTime.Today.Year}")
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    Info($"Running tests within {Relative(_solutionFile)}");

    DotNetTest(_solutionFile.FullPath, new DotNetTestSettings
    {
        Configuration = _configuration,
        NoRestore = true,
        NoBuild = true,
        ArgumentCustomization = args => args.Append("--nologo"),
        Loggers = _buildContext.IsAzurePipelinesBuild ? new[] { "trx" } : new string[0]
    });
});

Task("Clean-Artifacts")
    .Does(() =>
{
    if (DirectoryExists(_artifactsDir))
    {
        Info($"Cleaning directory {Relative(_artifactsDir)}");

        CleanDirectory(_artifactsDir);
    }
    else
    {
        Info($"Creating directory {Relative(_artifactsDir)}");

        CreateDirectory(_artifactsDir);
    }
});

Task("Pack")
    .IsDependentOn("Build")
    .IsDependentOn("Clean-Artifacts")
    .Does(() =>
{
    var libraryProjects = GetFiles($"{_srcDir}/**/*.csproj");

    foreach (var project in libraryProjects)
    {
        // var releaseNotes = GetReleaseNotes(
        //     GetCommitsWithChangesInDirectory(
        //         _buildContext.CommitLog,
        //         project.GetDirectory()));

        Info($"Packing project {Relative(project)}");

        DotNetPack(project.FullPath, new DotNetPackSettings
        {
            Configuration = _configuration,
            OutputDirectory = _artifactsDir,
            NoRestore = true,
            NoBuild = true,
            MSBuildSettings = _getDefaultMSBuildSettings()
                .SetVersion(_buildContext.Version)
                .WithProperty("RepositoryCommit", GitLogTip(_rootDir).Sha)
                .WithProperty("RepositoryBranch", _buildContext.BranchName)
                // .WithProperty("PackageReleaseNotes", releaseNotes)
                .WithProperty("Copyright", $"Copyright © Kato Stoelen {DateTime.Today.Year}")
        });
    }
});

Task("Push")
    .IsDependentOn("Pack")
    .Does(() =>
{
    if (_buildContext.IsPullRequestBuild)
    {
        Warn("Skipping push for pull request build");
        return;
    }

    if (!_buildContext.IsSourceFilesChanged)
    {
        Warn("Skipping push. No changes.");
        return;
    }

    if (string.IsNullOrWhiteSpace(_nugetFeed))
    {
        throw new ArgumentException("NuGet feed not specified", "--nuget-feed");
    }

    Info($"Using NuGet feed {_nugetFeed}");

    var packages = GetFiles($"{_artifactsDir}/*.nupkg");

    foreach (var package in packages)
    {
        Info($"Pushing package {Relative(package)}");

        DotNetNuGetPush(package.FullPath, new DotNetNuGetPushSettings
        {
            Source = _nugetFeed,
            ApiKey = _nugetFeedApiKey,
            SkipDuplicate = true
        });
    }
});

Task("CI")
    .IsDependentOn("Test")
    .IsDependentOn("Push");

RunTarget(_target);

///////////////////////////////////////////////////////////////////////////////
// CUSTOM
///////////////////////////////////////////////////////////////////////////////

private FilePath Relative(FilePath filePath)
{
    var absoluteRootPath = MakeAbsolute(_rootDir);
    var absoluteFilePath = MakeAbsolute(filePath);

    return absoluteRootPath.GetRelativePath(absoluteFilePath);
}

private DirectoryPath Relative(DirectoryPath directoryPath)
{
    var absoluteRootPath = MakeAbsolute(_rootDir);
    var absoluteDirectoryPath = MakeAbsolute(directoryPath);

    return absoluteRootPath.GetRelativePath(absoluteDirectoryPath);
}

public BuildContext GetBuildContext()
{
    var commitLog = GetCommitsSinceLatestTag(_rootDir);
    var isSourceFilesChanged = DirectoryHasChanges(commitLog, _srcDir);

    var gitVersionInfo = GitVersion();
    var currentCommitSha = GitLogTip(_rootDir).Sha;

    var version = gitVersionInfo.MajorMinorPatch;

    var isMaster = gitVersionInfo.BranchName.Equals("master", StringComparison.OrdinalIgnoreCase);
    var pullRequestMatch = System.Text.RegularExpressions.Regex
        .Match(gitVersionInfo.BranchName, "pull/(?<prId>[0-9]+)/merge");
    var isPullRequestBuild = pullRequestMatch.Success;

    var buildCounterPadded = _buildCounter.ToString("D").PadLeft(4, '0');

    if (isPullRequestBuild)
    {
        version += $"-pr{pullRequestMatch.Groups["prId"]}";
    }
    else if (!isSourceFilesChanged)
    {
        version += $"-noop{buildCounterPadded}";
    }
    else if (!isMaster)
    {
        version += $"-{gitVersionInfo.PreReleaseLabel}{buildCounterPadded}";
    }

    return new BuildContext
    {
        BranchName = gitVersionInfo.BranchName,
        CurrentCommitSha = currentCommitSha,
        IsCI = !BuildSystem.IsLocalBuild,
        Version = version,
        InformationalVersion = $"{version}+{currentCommitSha}",
        CommitLog = commitLog,
        IsSourceFilesChanged = isSourceFilesChanged,
        IsPullRequestBuild = isPullRequestBuild,
        IsAzurePipelinesBuild = BuildSystem.IsRunningOnAzurePipelines,
        ShouldTag = isMaster && isSourceFilesChanged
    };
}

public List<Commit> GetCommitsSinceLatestTag(DirectoryPath repositoryDirectoryPath, bool includeMergeCommits = false)
{
    var latestTag = GitDescribe(repositoryDirectoryPath, renderLongFormat: false, GitDescribeStrategy.Tags, 0);

    using (var repository = new LibGit2Sharp.Repository(repositoryDirectoryPath.FullPath))
    {
        var filter = new LibGit2Sharp.CommitFilter
        {
            IncludeReachableFrom = repository.Head
        };

        if (!string.IsNullOrEmpty(latestTag))
            filter.ExcludeReachableFrom = repository.Tags[latestTag].Target.Sha;

        return repository.Commits
            .QueryBy(filter)
            .Where(c => includeMergeCommits || c.Parents.Count() < 2)
            .Select(c => new Commit
            {
                ChangedFiles = repository.Diff
                    .Compare<LibGit2Sharp.Patch>(c.Parents.SingleOrDefault()?.Tree, c.Tree)
                    .Select(patch => File(patch.Path).Path)
                    .ToList()
            })
            .ToList();
    }
}

public bool DirectoryHasChanges(IReadOnlyCollection<Commit> commits, DirectoryPath directory) =>
    GetCommitsWithChangesInDirectory(commits, directory).Any();

public IEnumerable<Commit> GetCommitsWithChangesInDirectory(
    IReadOnlyCollection<Commit> commits, DirectoryPath directory)
{
    var relativeDirectoryPath = Relative(directory);

    return commits
        .Where(commit => commit.ChangedFiles
            .Any(file =>
                relativeDirectoryPath.Segments.SequenceEqual(
                    file.Segments.Take(relativeDirectoryPath.Segments.Length),
                    StringComparer.OrdinalIgnoreCase)));
}

public class Commit
{
    public List<FilePath> ChangedFiles { get; set; }
}

public class BuildContext
{
    public string BranchName { get; set; }
    public string CurrentCommitSha { get; set; }
    public bool IsCI { get; set; }
    public string Version { get; set; }
    public string InformationalVersion { get; set; }
    public bool IsSourceFilesChanged { get; set; }
    public bool IsPullRequestBuild { get; set; }
    public bool IsAzurePipelinesBuild { get; set; }
    public IReadOnlyList<Commit> CommitLog { get; set; }
    public bool ShouldTag { get; set; }
}