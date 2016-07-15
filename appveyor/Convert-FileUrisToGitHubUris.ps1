#Reads in SARIF files and rewrite absolute URIs to reference the source control version

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
  throw "Repository provider i.e. gitHub, bitbucket ... is not recognized"
}

function Rebase-Uri($originalURI){
  $replace = $originalURI.IndexOf("file:///")
  if($replace -ne  -1){
      $caseSensitiveUri = Get-CaseSensitivePath($originalURI.SubString("file:///".Length))
      $replaceLength = $caseSensitiveUri.IndexOf($projectSlug)
      if($replaceLength -ne -1){
          $builder.Path = ($repoName, $repoCommit, $caseSensitiveUri.SubString($replaceLength+$projectSlug.Length+1) -join '/')
          return $builder.ToString()
      }
  }
  Write-Error "Unable to rewrite URI"
  return $originalURI
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
  $sarifLog.runs[$i].files.PSObject.Properties | foreach-object{
    $key = $_.Name
    $content = $_.Value
    if($content.uri){
        $content.uri = Rebase-Uri $content.uri
    }

    if($content.parentKey){
      $content.parentKey = Rebase-Uri $content.parentKey
    }

    $sarifLog.runs.files.PSObject.Properties.Remove($_.Name)
    $rewrite = Rebase-Uri $key
    $sarifLog.runs.files | add-member -Name $rewrite -Value $content -MemberType NoteProperty
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
        $sarifLog.runs[$i].results[$j].codeFlows[$k].locations[$l].physicalLocation.uri = Rebase-Uri $sarifLog.runs[$i].results[$j].codeFlows[$k].locations[$l].physicalLocation.uri
      }
    }

    for($k = 0; $k -lt $sarifLog.runs[$i].results[$j].relatedLocations.Count; $k++){
      $sarifLog.runs[$i].results[$j].relatedLocations[$k].physicalLocation.uri = Rebase-Uri $sarifLog.runs[$i].results[$j].relatedLocations[$k].physicalLocation.uri
    }

    for($k = 0; $k -lt $sarifLog.runs[$i].results[$j].stacks.Count; $k++){
      $sarifLog.runs[$i].results[$j].stacks[$k].frames.uri = Rebase-Uri $sarifLog.runs[$i].results[$j].stacks[$k].frames.uri
    }

    for($k = 0; $k -lt $sarifLog.runs[$i].results[$j].fixes.Count; $k++){
      $sarifLog.runs[$i].results[$j].fixes[$k].fileChanges.uri = Rebase-Uri $sarifLog.runs[$i].results[$j].fixes[$k].fileChanges.uri
    }
  }

  for($j=0; $j -lt $sarifLog.runs[$i].configurationNotifications.Count; $j++){
    $sarifLog.runs[$i].configurationNotifications[$j].physicalLocation.uri = Rebase-Uri $sarifLog.runs[$i].configurationNotifications[$j].physicalLocation.uri
  }

  for($j=0; $j -lt $sarifLog.runs[$i].toolNotifications.Count; $j++){
    $sarifLog.runs[$i].toolNotifications[$j].physicalLocation.uri = Rebase-Uri $sarifLog.runs[$i].toolNotifications[$j].physicalLocation.uri
  }
}

$newExtension = ".github.sarif"
$writeLocation = [System.IO.Path]::ChangeExtension($logFilePath, $newExtension)
$sarifLog | ConvertTo-Json -depth 999 | Out-File $writeLocation
