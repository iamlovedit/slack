$projectPath = "src/Slack/Slack.csproj"
$outputDir = "publish"

Write-Host "Publishing Slack CLI..." -ForegroundColor Green

# Clean previous publish
if (Test-Path $outputDir) {
    Remove-Item $outputDir -Recurse -Force
}

# Publish SingleFile (Non-AOT for compatibility)
# To use AOT, remove "-p:PublishAot=false" and ensure C++ Desktop Development tools are installed.
dotnet publish $projectPath -c Release -r win-x64 -p:PublishAot=false -p:PublishSingleFile=true -o $outputDir

Write-Host "Publish complete. Output in ./$outputDir" -ForegroundColor Green
