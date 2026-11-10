#!/bin/zsh
# Build the mod DLL into ./mod/assembly/common/.
set -e
source "${0:a:h}/env.sh"
cd "${0:a:h}"
dotnet build src/SquadSelectTotal.csproj -c Release -nologo -v q
mkdir -p mod/assembly/common
cp src/bin/Release/SquadSelectTotal.dll mod/assembly/common/
echo "Built SquadSelectTotal"
