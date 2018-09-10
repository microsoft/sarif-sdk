
function Exit-WithFailureMessage($scriptName, $message) {
    Write-Information "${scriptName}: $message"
    Write-Information "$scriptName FAILED."
    throw
}

function Execute-Npm($command, $message)
{
    Write-Information "Running: npm $message"
    npm $command
     npm $command
    
}

# TODO:  Expand on this.
Push-Location Sarif.Sdk
try 
{
    npm install
    if ($lastExitCode -ne 0) {
        Exit-WithFailureMessage "BuildTypeScript.ps1" "npm $command failed"
    }
    npm run build
    if ($lastExitCode -ne 0) {
        Exit-WithFailureMessage "BuildTypeScript.ps1" "npm $command failed"
    }
    npm run test
    if ($lastExitCode -ne 0) {
        Exit-WithFailureMessage "BuildTypeScript.ps1" "npm $command failed"
    }
} finally
{
    Pop-Location
}