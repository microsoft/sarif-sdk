$location=$args[0]
$repo=$args[1]
$name=$args[2]

$JSON = (Get-Content $location) -join "`n" | ConvertFrom-Json

$base = "BASEURI"
if($repo.Title -eq "github")
{
  $base = "https://github.com/"
}
#Renaming the key value of file
$URI = $JSON.runs
for($i = 0; $i -lt $URI.Count; $i++){
  $URIReplacement = ($JSON.runs[$i].files | Get-Member -MemberType *Property).Name
  $content = $JSON.runs[$i].files.$URIReplacement
  $JSON.runs[$i].files.PSObject.Properties.Remove($URIReplacement)
  $URIReplacement = $base + $URIReplacement.SubString($URIReplacement.IndexOf($name))
  $JSON.runs[$i].files | add-member -Name $URIReplacement -Value $content -MemberType NoteProperty
}

$URI = $JSON.runs.files
for($i = 0; $i -lt $URI.Count; $i++){
  $JSON.runs.files[$i].uri = $base + $URI[$i].uri.SubString($URI[$i].uri.IndexOf($name))
}

# Rewriting all urls for the results locations
$URI = $JSON.runs.results.locations.resultFile
for($i = 0;$i -lt $URI.Count; $i++)
{
   $JSON.runs.results.locations.resultFile[$i].uri = $base + $URI[$i].uri.SubString($URI[$i].uri.IndexOf($name))
}

$URI = $JSON.runs.results.codeFlows.locations.physicalLocation
for($i = 0;$i -lt $URI.Count; $i++)
{
  $JSON.runs.results.codeFlows.locations.physicalLocation[$i].uri = $base + $URI[$i].uri.SubString($URI[$i].uri.IndexOf($name))
}

$URI = $JSON.runs.results.locations.analysisTarget
for($i = 0;$i -lt $URI.Count; $i++)
{
  $JSON.runs.results.locations.analysisTarget[$i].uri = $base + $URI[$i].uri.SubString($URI[$i].uri.IndexOf($name))
}

$URI = $JSON.runs.results.relatedLocations.physicalLocation
for($i = 0; $i -lt $URI.Count; $i++)
{
  $JSON.runs.results.relatedLocations.physicalLocation[$i].uri = $base + $URI[$i].uri.SubString($URI[$i].uri.IndexOf($name))
}

$URI = $JSON.runs.results.stacks.frames
for($i = 0;$i -lt $URI.Count; $i++)
{
  $JSON.runs.results.stacks.frames[$i].uri = $base + $URI[$i].uri.SubString($URI[$i].uri.IndexOf($name))
}

$URI = $JSON.runs.results.fixes.fileChanges
for($i = 0; $i -lt $URI.Count; $i++)
{
  $JSON.runs.results.fixes.fileChanges[$i].uri = $base + $URI[$i].uri.SubString($URI[$i].uri.IndexOf($name))
}

#Only works for one value
$URI = $JSON.runs.configurationNotifications.physicalLocation.uri
for($i = 0; $i -lt $URI.Count; $i++)
{
  $JSON.runs.configurationNotifications.physicalLocation.uri = $base + $URI.SubString($URI.IndexOf($name))
}

#Only works for one value
$URI = $JSON.runs.toolNotifications.physicalLocation.uri
for($i = 0; $i -lt $URI.Count; $i++)
{
  $JSON.runs.toolNotifications.physicalLocation.uri = $base + $URI.SubString($URI.IndexOf($name))
}
$newlocation = "github" + $location

$JSON | ConvertTo-Json -depth 999 | Out-File $newlocation
