#tool nuget:?package=NUnit.ConsoleRunner&version=3.7.0
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target                  = Argument("target", "Default");
var configuration           = Argument("configuration", "Release");
FilePath solution           = MakeAbsolute(File("./Klarna.Rest/Klarna.Rest.sln"));
DirectoryPath solutionDir   = solution.GetDirectory();
FilePath project            = solutionDir.CombineWithFilePath("Klarna.Rest/Klarna.Rest.csproj");
DirectoryPath artifacts     = MakeAbsolute(Directory("./artifacts"));
var isLocalBuild            = !AppVeyor.IsRunningOnAppVeyor;
var isMasterBranch          = StringComparer.OrdinalIgnoreCase.Equals("master", AppVeyor.Environment.Repository.Branch);
var version                 = isLocalBuild
                                    ? "1.0.0.0"
                                    : string.Format(
                                        System.Globalization.CultureInfo.InvariantCulture,
                                        "{0}.{1}.{2}.{3}",
                                        DateTime.Now.Year,
                                        DateTime.Now.Month,
                                        DateTime.Now.Day,
                                        AppVeyor.Environment.Build.Number
                                        );
var semVersion              =  string.Concat(
                                    version,
                                    (isMasterBranch) ? string.Empty : "-beta"
                                    );
var nuGetSource             = EnvironmentVariable("NUGET_PUSH_SOURCE");
var nuGetApiKey             = EnvironmentVariable("NUGET_PUSH_APIKEY");

Func<MSBuildSettings,MSBuildSettings>
    commonSettings         = settings => settings
                                .UseToolVersion(MSBuildToolVersion.VS2017)
                                .SetConfiguration(configuration)
                                .SetVerbosity(Verbosity.Minimal)
                                .WithProperty("PackageOutputPath", artifacts.FullPath)
                                .WithProperty("Version", semVersion)
                                .WithProperty("AssemblyVersion", version)
                                .WithProperty("FileVersion", version);

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
    if (!isLocalBuild)
    {
        AppVeyor.UpdateBuildVersion(semVersion);
    }
});


//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
  .Does(() =>
  {
    CleanDirectories(string.Format("{0}/**/obj/{1}", solutionDir, configuration));
    CleanDirectories(string.Format("{0}/**/bin/{1}", solutionDir, configuration));
    CleanDirectory(artifacts);
  });

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(solution,
        new NuGetRestoreSettings {
            Verbosity = NuGetVerbosity.Quiet
            });
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    MSBuild(solution, settings => commonSettings(settings).WithTarget("Build"));
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    NUnit3(solutionDir.FullPath + "/**/bin/" + configuration + "/*.Tests.dll", new NUnit3Settings {
        NoResults = true
        });
});

Task("Create-NuGet-Package")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{
     MSBuild(project,
        settings => commonSettings(settings)
                        .WithTarget("Pack")
                        .WithProperty("IncludeSymbols","true"));
});

Task("Upload-Artifacts")
    .IsDependentOn("Default")
    .Does(() =>
{
    foreach(var artifact in GetFiles(string.Concat(artifacts,"/**/*.*")))
    {
        AppVeyor.UploadArtifact(artifact);
    }
});

Task("Publish-To-NuGet-Feed")
    .IsDependentOn("Upload-Artifacts")
    .WithCriteria(() => !string.IsNullOrEmpty(nuGetSource))
    .WithCriteria(() => !string.IsNullOrEmpty(nuGetApiKey))
    .Does(() =>
{
    var nugetPackages = GetFiles(string.Concat(artifacts,"/**/*.nupkg"))
                        - GetFiles(string.Concat(artifacts,"/**/*.symbols.nupkg"));

    foreach(var package in nugetPackages)
    {
         NuGetPush(package, new NuGetPushSettings {
             Source = nuGetSource,
             ApiKey = nuGetApiKey
         });
    }
});


//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Create-NuGet-Package");

Task("AppVeyor")
    .IsDependentOn("Publish-To-NuGet-Feed");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);