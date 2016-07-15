#Find all Prefast results, push the original to appveyor, and then run the URI conversion

$Dir = get-childitem .\ -Recurse -Filter "*.prefast.sarif"

for ($i=0; $i -lt $Dir.Count; $i++)
{
  $logFilePath = $Dir[$i].FullName
  .\appveyor\Convert-FileUrisToGitHubUris.ps1 $logFilePath
  $newExtension = ".github.sarif"
  $writeLocation = [System.IO.Path]::ChangeExtension($logFilePath, $newExtension)
  Push-AppVeyorArtifact $writeLocation
}
