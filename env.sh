# Sourced by build.sh and deploy.sh: loads machine-specific paths from local.env
# (in this repo or in the parent folder) unless they are already set.
here="${0:a:h}"
for f in "$here/local.env" "$here/../local.env"; do
  [[ -f "$f" ]] && source "$f" && break
done
: "${XENONAUTS2_MANAGED:?set XENONAUTS2_MANAGED to the game's Xenonauts2_Data/Managed folder (see README)}"
export DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}" PATH="$HOME/.dotnet:$PATH" DOTNET_CLI_TELEMETRY_OPTOUT=1
