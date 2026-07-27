#!/usr/bin/env bash
set -e

export RimWorldVersion="$1"
CONFIGURATION="Debug"
TARGET="$HOME/.var/app/com.valvesoftware.Steam/.local/share/Steam/steamapps/common/RimWorld/Mods/ilyvion.LaboratoryMod"

mkdir -p .savedatafolder/1.6
mkdir -p .savedatafolder/1.5
mkdir -p .savedatafolder/1.4

# build dlls
dotnet build --configuration "$CONFIGURATION" ./ilyvion.LaboratoryMod/ilyvion.LaboratoryMod.csproj

# remove mod folder
rm -rf "$TARGET"

# copy mod files
mkdir -p "$TARGET"
cp -r "$RimWorldVersion" "$TARGET/$RimWorldVersion"
cp -r Common "$TARGET/Common"
rsync -av --exclude='*.pdn' --exclude='*.svg' --exclude='*.ttf' About/ "$TARGET/About"
cp CHANGELOG.md "$TARGET"
cp LICENSE "$TARGET"
cp LICENSE.Apache-2.0 "$TARGET"
cp LICENSE.MIT "$TARGET"
cp README.md "$TARGET"
cp LoadFolders.xml "$TARGET"

# Trigger auto-hotswap
mkdir -p "$TARGET/$RimWorldVersion/Assemblies"
touch "$TARGET/$RimWorldVersion/Assemblies/ilyvion.LaboratoryMod.dll.hotswap"
