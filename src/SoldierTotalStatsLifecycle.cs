using System;
using System.Collections.Generic;
using Artitas;
using Common.Content;
using Common.Content.DataStructures;
using Common.Content.Lifecycle;
using HarmonyLib;
using UnityEngine;

namespace SoldierTotalStats;

// Writes to Unity's Player.log (the game's log4net config only records errors) and to
// XenoMods.log in the game's data folder, which is written at once rather than buffered.
public static class ModLog
{
    public const string Prefix = "[SoldierTotalStats]";

    private static string _file;

    public static void Info(string message)
    {
        Debug.Log($"{Prefix} {message}");
        Append(message);
    }

    public static void Warn(string message)
    {
        Debug.LogWarning($"{Prefix} {message}");
        Append("WARN " + message);
    }

    private static void Append(string message)
    {
        try
        {
            _file ??= System.IO.Path.Combine(Application.persistentDataPath, "XenoMods.log");
            System.IO.File.AppendAllText(_file, $"{System.DateTime.Now:yyyy-MM-dd HH:mm:ss} {Prefix} {message}\n");
        }
        catch (System.Exception)
        {
            // logging must never break the game
        }
    }
}

// The game calls Harmony.PatchAll on every assembly in the content pack, so the
// lifecycle only needs to exist; it is used here to confirm the mod loaded.
public class SoldierTotalStatsLifecycle : IContentPackLifecycle
{
    public void Create(ContentPackState pack, Harmony patcher) => ModLog.Info("Create - mod loaded");

    public void Destroy() => ModLog.Info("Destroy - mod unloaded");

    public void OnWorldCreate(IContentPackLifecycle.Section section, WeakReference<World> world) { }

    public void OnWorldDispose(IContentPackLifecycle.Section section, WeakReference<World> world) { }

    public IEnumerable<Descriptor> GetRequiredAssets(IContentPackLifecycle.Section section) =>
        Array.Empty<Descriptor>();
}
