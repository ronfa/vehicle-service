param (
        [Parameter(Mandatory=$True)]    
        [string]$basefolder, 

        [Parameter(Mandatory=$True)]
        [string]$version
      )

function UninstallService {
    Param($uninstallFilter)
   
    $filter = "displayname like '%" + $uninstallFilter + "%'" ;
    Write-Host $filter ;
    Get-CimInstance win32_service -Filter  $filter | Stop-Service
    Get-CimInstance win32_service -Filter  $filter | Remove-CimInstance
}

Write-Host "Base Folder :" + $basefolder ;
Write-Host "Version :" + $version ;
 
$installationFolder = $basefolder + "\VehicleNotificationService.WebFleetEventsWorker\*"
$serviceName = "WebFleetEventsWorker"
 
$ServiceFolder = $basefolder + "\" + $serviceName 

UninstallService $serviceName;

if ((Test-Path -Path $ServiceFolder )) {
    Remove-Item $ServiceFolder -Force -Recurse
}  

# create a new folder fo each service  
New-Item -ItemType directory -Path $ServiceFolder

# copy from initial folder to the servie folder
Copy-Item $installationFolder -Destination $ServiceFolder -Recurse  
$servicePath = $ServiceFolder + "\bin\VehicleNotificationService.WebFleetEventsWorker.exe" ;

$displayName = $serviceName + "-" + $version     
New-Service -Name $serviceName -BinaryPathName $servicePath   -Description "VehicleNotificationService.WebFleetEventsWorker" -DisplayName $displayName  -StartupType Automatic
Start-Service -Name $serviceName