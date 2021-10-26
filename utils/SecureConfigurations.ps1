$MyDir = [System.IO.Path]::GetDirectoryName($myInvocation.MyCommand.Definition)
$path = Join-Path -Path $MyDir  -ChildPath "./Recrypt/Recrypt.dll"
$publicXmlPath = Join-Path -Path $MyDir  -ChildPath "./Recrypt/public.xml"
$projectsPath = Join-Path -Path $MyDir -ChildPath "../src"
$path  =[System.IO.Path]::GetFullPath($path )
$publicXmlPath =[System.IO.Path]::GetFullPath($publicXmlPath)
$projectsPath = [System.IO.Path]::GetFullPath($projectsPath)
Write-Host $projectsPath
$projects = Get-ChildItem $projectsPath 


Function EncryptFilesInFolder($folder) {
    Write-Host "Folder to encrypt: " $folder
    $files = Get-ChildItem -Path $folder -Filter "*.json"
    foreach ($file in $files) {
        $fileFullName = $file.Fullname
        CheckAgainstSecureParameters -file $fileFullName
        Write-Host "File can be secured :  $file" 
        dotnet $path  -ec -key $publicXmlPath $fileFullName 
    }
}

Function CheckAgainstSecureParameters($file) {
    $fileNameToLower = $file.ToLower()
    $errorMessageLastPart = "Debugging is not possible without privatekey which is only stored in deploy server."
    if ($fileNameToLower.IndexOf("appsettings.json") -ge 0) {
        $fileContent = Get-Content -Path $file -Raw
        if ($fileContent.IndexOf("_secure""") -ge 0) {
            $errorMessage = "File : $file - Generic (across environments) settings can not include secure parameters. $errorMessageLastPart"
            Write-Error $errorMessage
            throw $errorMessage
        }
    }
    if ($fileNameToLower.IndexOf("appsettings.dev.json") -ge 0) {
        $fileContent = Get-Content -Path $file -Raw
       
        if ($fileContent.IndexOf("_secure""") -ge 0) {
            $errorMessage = "File : $file - Development settings can not include secure parameters.$errorMessageLastPart"
            Write-Error $errorMessage
            throw $errorMessage
        }
    }

}

Function Init() {
    Write-Host "Start - Secure Encryption Process"
    foreach ($project in $projects) {
        $configPath = Join-Path -Path $project.Fullname -ChildPath "config"
        if ([System.IO.Directory]::Exists($configPath)) {
            Write-Host "Attempting to encrypt configuration common for all lambda functions"
            EncryptFilesInFolder($configPath)
            $lambdaFuncFolders = Get-ChildItem $configPath -Directory
            foreach ($lambdaFuncFolder in $lambdaFuncFolders) {
                Write-Host "Encrypting lambda folder -> $lambdaFuncFolder"
                EncryptFilesInFolder($lambdaFuncFolder.Fullname)
            }
        }
    }
    Write-Host "End - Secure Encryption Process"
}

Init