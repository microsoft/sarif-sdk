#Find all Prefast results, push the original to appveyor, and then run the URI conversion

$Dir = get-childitem .\ -Recurse -Filter "*.prefast.sarif"

for ($i=0; $i -lt $Dir.Count; $i++)
{
  $fileLocation = $Dir[$i].FullName
  Push-AppVeyorArtifact $fileLocation
  .\appveyor\Convert-FileUrisToGitHubUris.ps1 $fileLocation
}
