Task ("DotnetCore-Pack-SelfContained")
    .Description ("Packages self contained .net core project.")
    .Does (() =>
    {
        var projectList = new List<string> ();
        var projectFiles = GetFiles ("./src/**/*.csproj");
        foreach (var project in projectFiles)
        {
            var addProject = false;
            var fullPath = project.ToString ();
            foreach (var dotnetCorePublishProject in dotnetCoreSelfContainedProjects)
            {
                if (fullPath.Contains (dotnetCorePublishProject))
                {
                    addProject = true;
                    break;
                }
            }
            if (addProject)
            {
                projectList.Add (fullPath);
            }
        }
        var projects = projectList.ToArray ();

        foreach (var project in projects)
        {
            var projectName = project
                .Split (new [] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                .Last ()
                .Replace (".csproj", string.Empty);

            var outputDirectory = System.IO.Path.Combine (publishDir, projectName);

            var msBuildSettings = new DotNetCoreMSBuildSettings
            {
                TreatAllWarningsAs = MSBuildTreatAllWarningsAs.Error,
                Verbosity = dotNetCoreVerbosity
            };
            var settings = new DotNetCorePublishSettings
            {
                Configuration = configuration,
                MSBuildSettings = msBuildSettings,
                NoRestore = true,
                OutputDirectory = outputDirectory,
                Verbosity = dotNetCoreVerbosity,
                SelfContained = true,
                Runtime="win-x64"
            }; 

            Information ("Publishing '{0}'...", projectName);
            DotNetCorePublish (project, settings);
            Information ("'{0}' has been published.", projectName);

             var nuGetPackSettings = new NuGetPackSettings
            {
                Version = applicationVersion,
                OutputDirectory = artifactsDir
            };
            var nuspecFile = project.Replace(".csproj",".nuspec");
            NuGetPack (nuspecFile, nuGetPackSettings);

            Information ("'{0}' has been packed.", projectName);
        }
    });

    Task ("DotnetCore-Pack-settings")
    .Description ("Publish the Lambda Functions.")
    .Does (() =>
    {
        var nuspecList = new List<string> ();
        var nuspecFiles = GetFiles ("./src/**/*.nuspec");
        Information ("'{0}' nuspec files have been found", nuspecFiles.Count);
        foreach (var nuspec in nuspecFiles)
        {
            var fullPath = nuspec.ToString ();
            foreach (var settingNuspec in settingNuspecs)
            {
                if (fullPath.Contains (settingNuspec))
                {
                   nuspecList.Add (fullPath);
                    break;
                }
            } 
        } 

        foreach (var nuspec in nuspecList)
        { 
             var nuGetPackSettings = new NuGetPackSettings
            {
                Version = applicationVersion,
                OutputDirectory = artifactsDir
            }; 

            NuGetPack (nuspec, nuGetPackSettings); 
            Information ("'{0}' has been packed.", nuspec);
        }
    });