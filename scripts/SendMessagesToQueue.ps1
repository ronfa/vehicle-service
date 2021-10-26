
$dateTimeRaw = Get-Date -Format o
$dateTime = $dateTimeRaw.Substring(0, 19).Replace('-', '_').Replace(':', '_')
$global:exportLogFileName = "./" + "RunLog___"+"emailBounceData" + "$dateTime" + ".log"
$global:dryRun = 0
$global:profile = "playpenprofile"
$global:emailBounceQueueUrl = "https://sqs.eu-west-2.amazonaws.com/637422166946/dev-parknow-phonixx-vehicle-notification-queue2"
Function LogProgressMessage ($message) {
    $nl = [Environment]::NewLine
    Add-Content -Path $global:exportLogFileName -Value "$nl $message $nl"
    Write-Host "$message"
}
Function SendQueueMessage($queueUrl, $messageBodyString,$profile) {
    if ($global:dryRun -eq 0) {
        LogProgressMessage -message "sending enforcement message"
        aws sqs send-message --queue-url $queueUrl --message-body $messageBodyString --delay-seconds 10 --profile $profile
        LogProgressMessage -message "Send message complete. $sendMessageResponse"
        return $sendMessageResponse | Out-String
    }
    else {
        $dryRunMessage = "Dry run is turned on. No Enforcement message is sent. Message Body:" + $messageBodyString
        LogProgressMessage -message $dryRunMessage
        return $dryRunMessage
    }
}

#git init 
# git add ..\GitVersion.yml 
# git commit -m test
for($i=0;$i -ne 5;$i++){
    $guid = New-Guid
    $guidString = $guid.ToString()
    LogProgressMessage -message "Guid value is $guidString"
    SendQueueMessage -queueUrl $global:emailBounceQueueUrl -messageBodyString "testmessage - guid $guidString" -profile $global:profile
}