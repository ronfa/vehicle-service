#load nuget:https://www.myget.org/F/parknow-dotnet/auth/54abb7b8-c5bb-421d-97c5-1b584d0c5312/api/v2?package=MasterCake&version=0.1.3&include=scripts/include.cake
#load "local:?path=selfcontained-pack.cake"
string serverlessArtifactNuspecFileName = "VehicleNotificationService.nuspec";

//application name is going to be registered in Sonar Cube
applicationName = "VehicleNotificationService";
dotCoverFilters = "+:VehicleNotificationService.*,-:VehicleNotificationService.*.Tests";
dotnetCorePublishProjects = new List<string>(){
  "VehicleNotificationService.WebApi.csproj", 
  "VehicleNotificationService.QueueProcessor.csproj"
};

var  dotnetCoreSelfContainedProjects = new List<string>(){
    "VehicleNotificationService.WebFleetEventsWorker.csproj"
};

var settingNuspecs = new List<string>(){
    "VehicleNotificationService.WebFleetEventsWorker.Settings.nuspec"
};

Task("Default")
    .Description("Runs serverless pipeline including local deployment")
    .IsDependentOn("GitVersion")
    .IsDependentOn("Common-Clean")
    .IsDependentOn("DotnetCore-Restore")
    .IsDependentOn("DotnetCore-Build")
    .IsDependentOn("DotNetCore-TestAndCover")
    .IsDependentOn("DotnetCore-Publish")
    .IsDependentOn("DotnetCore-Pack-SelfContained")
    .IsDependentOn("DotnetCore-Pack-settings")
    .IsDependentOn("Serverless-Artifact")
    .IsDependentOn("Teamcity-PublishArtifact")
    .IsDependentOn("Serverless-UploadParameters")
    .IsDependentOn("Serverless-Deploy")
    .IsDependentOn("GitVersion-Finalize")
    .Does(() => {
         Information("Serverless pipeline is finished");
    });

  Task("TestCover")
    .Description("Only triggers test and cover")
    .IsDependentOn("Common-Clean")
    .IsDependentOn("DotnetCore-Restore")
    .IsDependentOn("DotnetCore-Build")
    .IsDependentOn("DotNetCore-TestAndCover")
    .Does(() => {
         Information("Test & cover is finished");
    });

  Task("Deploy")
    .Description("Only triggers deploy")
    .IsDependentOn("Serverless-UploadParameters")
    .IsDependentOn("Serverless-Deploy")    
    .Does(() => {
         Information("Deployment is finished");
    });

RunTarget(target);
