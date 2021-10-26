# all the packages are extracted to this directory. 
# this folder is typically under d:\Octopus\Work in a folder with DateTime signature
$workingDirectory = $OctopusParameters["env:OctopusCalamariWorkingDirectory"]

#create output folder for deployment process to place processed template
mkdir output

#list all contents
Get-ChildItem -Path $workingDirectory -Recurse

# Set environment variables
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
Write-Host Running on $OctopusParameters["Environment"] environment
Write-Host Setting environment variables

[Environment]::SetEnvironmentVariable("AWS_ACCESS_KEY_ID",  $OctopusParameters["aws-access-key"], "Process")
[Environment]::SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", $OctopusParameters["aws-access-secret"], "Process")
[Environment]::SetEnvironmentVariable("AWS_DEFAULT_REGION", $OctopusParameters["aws-region"], "Process")


[Environment]::SetEnvironmentVariable("Application__LambdaVpcAccess", $OctopusParameters["AWS:LambdaVpcAccess"], "Process")
[Environment]::SetEnvironmentVariable("Application__SecurityGroupIds", $OctopusParameters["AWS:SECURITYGROUPIDS"], "Process")
[Environment]::SetEnvironmentVariable("Application__SubnetIds", $OctopusParameters["AWS:SUBNETIDS"], "Process")
[Environment]::SetEnvironmentVariable("Application__Version", $OctopusParameters["Octopus.Release.Number"] , "Process")
[Environment]::SetEnvironmentVariable("Application__AppEnvironment", $OctopusParameters["ApplicationEnvironment"] , "Process")
[Environment]::SetEnvironmentVariable("Application__Environment", $OctopusParameters["Environment"] , "Process")
[Environment]::SetEnvironmentVariable("Application__UploaderPrivateKeyString", $OctopusParameters["PrivateKey"] , "Process")
[Environment]::SetEnvironmentVariable("Application__KMSKey", $OctopusParameters["DefaultKMSKeyArn"] , "Process")
[Environment]::SetEnvironmentVariable("Application__KmsKeyArn", $OctopusParameters["DefaultKMSKeyArn"] , "Process")
[Environment]::SetEnvironmentVariable("Application__UploaderKmsAlias", $OctopusParameters["DefaultKMSKeyAlias"] , "Process")
[Environment]::SetEnvironmentVariable("Application__ApiGatewayDomainName", $OctopusParameters["APIGATEWAYDOMAINNAME"] , "Process")
[Environment]::SetEnvironmentVariable("Application__ArtifactsS3Bucket", $OctopusParameters["ArtifactsS3BucketName"] , "Process")
[Environment]::SetEnvironmentVariable("Application__ApiGatewayDomainCertificateArn", $OctopusParameters["ApiGatewayDomainCertificateArn"] , "Process")
[Environment]::SetEnvironmentVariable("Application__RoleArn", $OctopusParameters["RoleArn"] , "Process")

Write-Host Validate environment vars
$accessKey = [Environment]::GetEnvironmentVariable("AWS_ACCESS_KEY_ID")
$region = [Environment]::GetEnvironmentVariable("AWS_DEFAULT_REGION")
Write-Host AccessKey: $accessKey Region: $region

$hasVpcAccess = [Environment]::GetEnvironmentVariable("Application__LambdaVpcAccess")
$securityGroupIds = [Environment]::GetEnvironmentVariable("Application__SecurityGroupIds")
$subnetIds = [Environment]::GetEnvironmentVariable("Application__SubnetIds")

Write-Host VPCAccess: $hasVpcAccess SecurityGroups: $securityGroupIds Subnets: $subnetIds

Write-Host Start deployment
$buildScript = $PSScriptRoot+"\build.ps1"
& $buildScript --Target Deploy
