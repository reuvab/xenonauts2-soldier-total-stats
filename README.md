# Soldier Total Stats — a Xenonauts 2 mod

Adds a **TOT** column with the sum of a soldier's six stats
(Health + Time Units + Accuracy + Strength + Reflexes + Bravery) to every soldier list:

- **Soldiers > Manage**
- **Soldiers > Recruit**
- **The soldier picker** used when assigning soldiers to a dropship

<!-- screenshot: mod/preview.png -->

## Features

- **Sortable**: click the TOT header to sort by total, click again to reverse, like the
  game's own columns.
- **Colour scale**: totals run from red (lowest on that screen) to green (highest).
- **Stays aligned**: the stat columns are narrowed slightly to make room, so the header and
  rows line up and the name, loadout and status columns do not move.
- **Save-safe**: nothing is stored in your campaign; enable or disable the mod at any time.

## Install

**Steam Workshop:** subscribe, then enable *Soldier Total Stats* in the game's mod list and
restart the game.

**Manual:** download `SoldierTotalStats-<version>.zip` from the
[releases](https://github.com/reuvab/xenonauts2-soldier-total-stats/releases) and extract it into
the game's `Mods` folder, so that you get `Mods/SoldierTotalStats/manifest.json`. The `Mods` folder
is in the game's user data folder:

- Windows: `%USERPROFILE%\AppData\LocalLow\Goldhawk Interactive\Xenonauts 2\Mods`

Then enable the mod in the game's mod list and restart.

## Compatibility

- **Not compatible with [Soldier Total Column](https://steamcommunity.com/sharedfiles/filedetails/?id=3726627096)**,
  which adds a similar column to the Manage and Recruit screens. Enable only one of them; the
  game warns you if both are on.
- Built and tested against Xenonauts 2 version 7.28.0.
- Text is English only (the column is labelled TOT).

## Troubleshooting

The mod writes to `SoldierTotalStats.log` in the game's user data folder (the parent of
`Mods`). Please include it when reporting a problem in the
[issues](https://github.com/reuvab/xenonauts2-soldier-total-stats/issues).

## Credits

Inspired by [Soldier Total Column](https://steamcommunity.com/sharedfiles/filedetails/?id=3726627096)
by mnshdw.

## Building from source

Requires the .NET SDK and a Xenonauts 2 install; the game's DLLs are referenced from your install
and are not included here. Set two paths, as environment variables or in a `local.env` file in
this folder or its parent:

```sh
export XENONAUTS2_MANAGED="<game>/Xenonauts2_Data/Managed"
export XENONAUTS2_MODS="<user data>/Goldhawk Interactive/Xenonauts 2/Mods"
```

| Script | What it does |
|---|---|
| `./build.sh` | Builds the DLL into `mod/assembly/common/` |
| `./deploy.sh` | Copies `mod/` into the game's `Mods/SoldierTotalStats` |
| `./package.sh` | Builds and zips `mod/` into `dist/` for a GitHub release |

Layout: `src/` holds the C# source (Harmony patches loaded by the game's own mod system);
`mod/` is the mod folder as the game expects it.

## License

[MIT](LICENSE)
