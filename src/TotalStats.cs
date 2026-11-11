using System.Collections.Generic;
using Artitas;
using Strategy.UI.Elements.Soldiers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xenonauts.Strategy.Utils;

namespace SoldierTotalStats;

internal static class TotalStats
{
    // Not a value the game defines; lets the game's own comparer carry "sort by total"
    // so clicking the header twice flips the order exactly like the built-in columns.
    public const SortField TotalSortField = (SortField)1000;

    // Rounded per stat so the total matches the numbers shown in the row.
    public static int Compute(Entity actor) =>
        Mathf.RoundToInt(actor.UnmodifiedHitPoints().ToStrategyBoundedRange().Value)
        + Mathf.RoundToInt(actor.TimeUnits().ToStrategyBoundedRange().Value)
        + Mathf.RoundToInt(actor.Accuracy().ToStrategyBoundedRange().Value)
        + Mathf.RoundToInt(actor.Strength().ToStrategyBoundedRange().Value)
        + Mathf.RoundToInt(actor.Reflexes().ToStrategyBoundedRange().Value)
        + Mathf.RoundToInt(actor.Bravery().ToStrategyBoundedRange().Value);
}

// Colours every total from red (lowest in the list) to green (highest), separately per screen.
internal static class TotalColors
{
    private sealed class Entry
    {
        public TMP_Text Text;
        public Graphic[] Graphics;
        public int Value;
    }

    private static readonly Color Low = new Color(0.85f, 0.2f, 0.2f);
    private static readonly Color Mid = new Color(0.95f, 0.85f, 0.3f);
    private static readonly Color High = new Color(0.45f, 0.85f, 0.3f);

    private static readonly Dictionary<string, List<Entry>> Screens = new Dictionary<string, List<Entry>>();

    private static List<Entry> Entries(string screen)
    {
        if (!Screens.TryGetValue(screen, out var entries))
            Screens[screen] = entries = new List<Entry>();
        return entries;
    }

    public static void Clear(string screen) => Entries(screen).Clear();

    public static void Remove(string screen, TMP_Text text) =>
        Entries(screen).RemoveAll(e => e.Text == null || e.Text == text);

    // graphics: the cloned cell's background/bar graphics. When the cell has any, those are
    // tinted (like the game's own stat cells) and the text keeps its colour; otherwise the text is tinted.
    public static void Set(string screen, TMP_Text text, Graphic[] graphics, int value)
    {
        var entries = Entries(screen);
        entries.RemoveAll(e => e.Text == null);
        var entry = entries.Find(e => e.Text == text);
        if (entry == null)
        {
            entry = new Entry { Text = text };
            entries.Add(entry);
        }
        entry.Graphics = graphics;
        entry.Value = value;

        // Rows are pooled: a soldier who was fired or transferred leaves a hidden row behind,
        // which must not stretch the scale. Fall back to every row while none are shown yet.
        var shown = entries.FindAll(e => e.Text.gameObject.activeInHierarchy);
        if (shown.Count == 0)
            shown = entries;
        int min = int.MaxValue, max = int.MinValue;
        foreach (var e in shown)
        {
            min = Mathf.Min(min, e.Value);
            max = Mathf.Max(max, e.Value);
        }
        foreach (var e in entries)
        {
            var color = Interpolate(max > min ? (float)(e.Value - min) / (max - min) : 0.5f);
            if (e.Graphics != null && e.Graphics.Length > 0)
            {
                foreach (var g in e.Graphics)
                {
                    if (g != null)
                        g.color = color;
                }
            }
            else
            {
                e.Text.color = color;
            }
        }
    }

    private static Color Interpolate(float t) =>
        t < 0.5f ? Color.Lerp(Low, Mid, t * 2f) : Color.Lerp(Mid, High, (t - 0.5f) * 2f);
}
