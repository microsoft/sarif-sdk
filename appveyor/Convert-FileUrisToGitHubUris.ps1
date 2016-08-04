#Reads in SARIF files and rewrite absolute Uris to reference the source control version
$ErrorActionPreference = "Stop"

$logFilePath=$args[0]
$repoProvider=$env:APPVEYOR_REPO_PROVIDER
$projectSlug=$env:APPVEYOR_PROJECT_SLUG
$repoName=$env:APPVEYOR_REPO_NAME
$repoCommit=$env:APPVEYOR_REPO_COMMIT

$builder = New-Object System.UriBuilder

if($repoProvider -eq "gitHub"){
  $builder.Scheme = 'https'
  $builder.Host = 'raw.githubusercontent.com'
}
else{
  throw "Unrecognized repository provider: $repoProvider. Set the APPVEYOR_REPO_PROVIDER environment variable to a supported provider. Supported providers are: gitHub."
}

function Rebase-Uri($originalUri){
  if($originalUri){
    $fileUriPrefix = "file:///"
    if($originalUri.StartsWith($fileUriPrefix)){
        $caseSensitiveUri = Get-CaseSensitivePath $originalUri.SubString($fileUriPrefix.Length)
        $projectSlugIndex = $caseSensitiveUri.IndexOf($projectSlug)
        if($projectSlugIndex -ne -1){
            $builder.Path = $repoName, $repoCommit, $caseSensitiveUri.SubString($projectSlugIndex+$projectSlug.Length+1) -join '/'
            return $builder.ToString()
        }
    }
  }
  Write-Host "Unable to rewrite URI: " $originalUri
  return $originalUri
}

function Get-CaseSensitivePath($pathName){
  $pathExists = Test-Path($pathName)
  if (-Not $pathExists){
      return $pathName
  }
  $directoryInfo = New-Object IO.DirectoryInfo($pathName)

  if ($directoryInfo.Parent -ne $null){
      $parentPath = Get-CaseSensitivePath $directoryInfo.Parent.FullName
      $childPath = $directoryInfo.Parent.GetFileSystemInfos($directoryInfo.Name)[0].Name
      return(Join-Path $parentPath $childpath -resolve)
  }
  else{
      return $directoryInfo.Name
  }
}

$sarifLog = (Get-Content $logFilePath) -join "`n" | ConvertFrom-Json

for ($i = 0; $i -lt $sarifLog.runs.Count; $i++){
  if($sarifLog.runs[$i].files){
    $sarifLog.runs[$i].files.PSObject.Properties | foreach-object{
      $key = $_.Name
      $value = $_.Value
      if($key -and $value){
        if($value.uri){
            $value.uri = Rebase-Uri $value.uri
        }

        if($value.parentKey){
          $value.parentKey = Rebase-Uri $value.parentKey
        }

        $sarifLog.runs[$i].files.PSObject.Properties.Remove($key)
        $rebasedUri = Rebase-Uri $key
        $sarifLog.runs[$i].files | add-member -Name $rebasedUri -Value $value -MemberType NoteProperty
      }
    }
  }
  #results
  for($j = 0; $j -lt $sarifLog.runs[$i].results.Count; $j++){
    for($k = 0; $k -lt $sarifLog.runs[$i].results[$j].locations.Count; $k++){
      $location = $sarifLog.runs[$i].results[$j].locations[$k]
      if($location.resultFile.uri){
        $sarifLog.runs[$i].results[$j].locations[$k].resultFile.uri = Rebase-Uri $location.resultFile.uri
      }

      if($location.analysisTarget.uri){
        $sarifLog.runs[$i].results[$j].locations[$k].analysisTarget.uri = Rebase-Uri $location.analysisTarget.uri
      }
    }

    #Codeflow
    for($k = 0; $k -lt $sarifLog.runs[$i].results[$j].codeFlows.Count; $k++){
      for($l = 0; $l -lt $sarifLog.runs[$i].results[$j].codeFlows[$k].locations.Count; $l++){
        $uri = $sarifLog.runs[$i].results[$j].codeFlows[$k].locations[$l].physicalLocation.uri
        if($uri){
          $sarifLog.runs[$i].results[$j].codeFlows[$k].locations[$l].physicalLocation.uri = Rebase-Uri $uri
        }
      }
    }

    for($k = 0; $k -lt $sarifLog.runs[$i].results[$j].relatedLocations.Count; $k++){
      $uri = $sarifLog.runs[$i].results[$j].relatedLocations[$k].physicalLocation.uri
      if($uri){
        $sarifLog.runs[$i].results[$j].relatedLocations[$k].physicalLocation.uri = Rebase-Uri $uri
      }
    }

    for($k = 0; $k -lt $sarifLog.runs[$i].results[$j].stacks.Count; $k++){
      $uri = $sarifLog.runs[$i].results[$j].stacks[$k].frames.uri
      if($uri){
        $sarifLog.runs[$i].results[$j].stacks[$k].frames.uri = Rebase-Uri $uri
      }
    }

    for($k = 0; $k -lt $sarifLog.runs[$i].results[$j].fixes.Count; $k++){
      $uri = $sarifLog.runs[$i].results[$j].fixes[$k].fileChanges.uri
      if($uri){
        $sarifLog.runs[$i].results[$j].fixes[$k].fileChanges.uri = Rebase-Uri $uri
      }
    }
  }

  for($j=0; $j -lt $sarifLog.runs[$i].configurationNotifications.Count; $j++){
    $uri = $sarifLog.runs[$i].configurationNotifications[$j].physicalLocation.uri
    if($uri){
      $sarifLog.runs[$i].configurationNotifications[$j].physicalLocation.uri = Rebase-Uri $uri
    }
  }

  for($j=0; $j -lt $sarifLog.runs[$i].toolNotifications.Count; $j++){
    $uri = $sarifLog.runs[$i].toolNotifications[$j].physicalLocation.uri
    if($uri){
      $sarifLog.runs[$i].toolNotifications[$j].physicalLocation.uri = Rebase-Uri $uri
    }
  }
}

$newExtension = ".github.sarif"
$writeLocation = [System.IO.Path]::ChangeExtension($logFilePath, $newExtension)
$writeFile = $sarifLog | ConvertTo-Json -depth 999
[System.IO.File]::WriteAllLines($writeLocation, $writeFile)
