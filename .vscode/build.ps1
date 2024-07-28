$ErrorActionPreference = 'Stop'

$env:RimWorldVersion = $args[0]
$Configuration = 'Debug'

$VersionTargetPrefix = "D:\RimWorld"
$VersionTargetSuffix = "Mods\ilyvion.Laboratory"
$Target = "$VersionTargetPrefix\$env:RimWorldVersion\$VersionTargetSuffix"

# build dlls
dotnet build --configuration $Configuration .\ilyvion.LaboratoryMod\ilyvion.LaboratoryMod.csproj
if ($LASTEXITCODE -gt 0) {
    throw "Build failed"
}

# remove mod folder
Remove-Item -Path $Target -Recurse -ErrorAction SilentlyContinue

# copy mod files
Copy-Item -Path $env:RimWorldVersion $Target\$env:RimWorldVersion -Recurse

Copy-Item -Path Common $Target\Common -Recurse
Copy-Item -Path About $Target\About -Recurse -Exclude *.pdn, *.svg, *.ttf

Copy-Item -Path CHANGELOG.md $Target
Copy-Item -Path LICENSE $Target
Copy-Item -Path LICENSE.Apache-2.0 $Target
Copy-Item -Path LICENSE.MIT $Target
Copy-Item -Path README.md $Target
Copy-Item -Path LoadFolders.xml $Target
