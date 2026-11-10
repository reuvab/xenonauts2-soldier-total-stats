# Xenonauts 2 — Soldier Total Stats

Adds a sortable TOT column (Health + Time Units + Accuracy + Strength + Reflexes + Bravery) to soldier lists. Currently covers the soldier picker used when filling a dropship slot.

A code mod for Xenonauts 2 (Harmony patches, loaded by the game's own mod system).

## Layout

- `src/` — C# source and project
- `mod/` — the mod folder as the game expects it (`manifest.json`; the DLL is built into `mod/assembly/common/`)

## Build and install

Requires the .NET SDK and a Xenonauts 2 install. Set two paths, either as environment
variables or in a `local.env` file in this folder or its parent:

```sh
export XENONAUTS2_MANAGED="<game>/Xenonauts2_Data/Managed"
export XENONAUTS2_MODS="<user data>/Goldhawk Interactive/Xenonauts 2/Mods"
```

Then:

```sh
./build.sh
./deploy.sh
```

Enable the mod in the game's mod list and restart. The mod logs to `XenoMods.log` in the
game's user data folder.
