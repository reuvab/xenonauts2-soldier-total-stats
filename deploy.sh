#!/bin/zsh
# Mirror ./mod into the game's Mods folder as "SoldierTotalStats". Only that one folder is touched.
set -e
source "${0:a:h}/env.sh"
: "${XENONAUTS2_MODS:?set XENONAUTS2_MODS to the game's Mods folder (see README)}"
[[ -d "$XENONAUTS2_MODS" ]] || { echo "Mods folder not found: $XENONAUTS2_MODS"; exit 1; }
cd "${0:a:h}"
[[ -f mod/assembly/common/SoldierTotalStats.dll ]] || { echo "run ./build.sh first"; exit 1; }
rsync -a --delete mod/ "$XENONAUTS2_MODS/SoldierTotalStats/"
echo "Deployed SoldierTotalStats"
