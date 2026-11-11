#!/bin/zsh
# Build the mod and zip ./mod as dist/SoldierTotalStats-<version>.zip, ready to attach to a
# GitHub release. The zip holds one SoldierTotalStats/ folder to drop into the game's Mods folder.
set -e
cd "${0:a:h}"
./build.sh
[[ -f mod/preview.png ]] || echo "warning: mod/preview.png is missing; the Steam Workshop upload needs it"
version=$(python3 -c "import json; print(json.load(open('mod/manifest.json'))['asset']['Version'])")
stage=$(mktemp -d)
cp -R mod "$stage/SoldierTotalStats"
mkdir -p dist
zip_path="$PWD/dist/SoldierTotalStats-$version.zip"
rm -f "$zip_path"
(cd "$stage" && zip -qr "$zip_path" SoldierTotalStats -x '*.DS_Store')
echo "Packaged $zip_path"
