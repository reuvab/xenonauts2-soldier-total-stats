using System;
using System.Linq;
using Artitas;
using Common.UI.Unity;
using Common.Utilities.Comparers;
using HarmonyLib;
using Strategy.UI.Elements.Soldiers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xenonauts.Common.Util.Comparers;
using Xenonauts.Strategy.UI;

namespace SquadSelectTotal;

// The soldier selection popup opened when filling a dropship / strike team slot
// from the Armory or Aircraft screens.
[HarmonyPatch(typeof(SoldierSelectionElement), nameof(SoldierSelectionElement.OnCreate))]
public static class SoldierSelectionHeaderPatch
{
    public const string HeaderName = "SquadSelectTotal_Header";

    [HarmonyPostfix]
    public static void Postfix(SoldierSelectionElement __instance)
    {
        try
        {
            var toggles = __instance.columnToggles;
            if (toggles == null || !toggles.TryGetValue(SortField.Bravery, out var bravery) || bravery == null)
            {
                ModLog.Warn("Bravery header not found; columns present: "
                    + (toggles == null ? "none" : string.Join(", ", toggles.Keys)));
                return;
            }

            var clone = UiClone.CloneAfter(bravery.transform, HeaderName);
            foreach (var text in clone.GetComponentsInChildren<TMP_Text>(true))
                text.text = "TOT";
            UiClone.ReplaceTooltip(clone, "TOTAL",
                "Sum of Health + Time Units + Accuracy + Strength + Reflexes + Bravery.");

            var toggle = clone.GetComponent<GHIToggle>();
            if (toggle == null)
            {
                ModLog.Warn("cloned header has no GHIToggle; sorting disabled");
                return;
            }
            toggle.SetIsOnWithoutNotify(false);
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onLeftClick.RemoveAllListeners();
            // Same behaviour as the game's own column handlers in OnCreate.
            toggle.onValueChanged.AddListener(isOn =>
            {
                var traverse = Traverse.Create(__instance);
                if (!isOn || !traverse.Field<bool>("_columnsChangeComparer").Value)
                    return;
                traverse.Field<StrategyActorComparer>("_comparer").Value.SetSelectedSortColumn(TotalStats.TotalSortField);
                traverse.Field("_controllers").Method("Sort").GetValue();
            });
            ModLog.Info("header added to soldier selection popup");
        }
        catch (Exception e)
        {
            ModLog.Warn($"header patch failed: {e}");
        }
    }
}

[HarmonyPatch(typeof(SoldierSelectionElement), nameof(SoldierSelectionElement.OnEnter))]
public static class SoldierSelectionEnterPatch
{
    // Each time the popup opens it lists a new set of soldiers; colours are relative to that set.
    [HarmonyPrefix]
    public static void Prefix() => TotalColors.Clear();
}

[HarmonyPatch(typeof(SoldierSelectionRowController), "OnSetTarget")]
public static class SoldierSelectionRowPatch
{
    public const string CellName = "SquadSelectTotal_Cell";

    [HarmonyPostfix]
    public static void Postfix(SoldierSelectionRowController __instance)
    {
        try
        {
            var (braveryCell, reflexesCell) = UiClone.SiblingCells(__instance.braveryText.transform, __instance.reflexesText.transform);
            if (braveryCell == null || reflexesCell == null)
            {
                ModLog.Warn("could not locate the Bravery cell in the selection row");
                return;
            }
            var cell = UiClone.CloneAfter(braveryCell, CellName);
            var text = cell.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault();
            if (text == null)
                return;
            text.textWrappingMode = TextWrappingModes.NoWrap;

            Entity actor = __instance.Target?.X;
            if (actor == null)
            {
                text.text = string.Empty;
                TotalColors.Remove(text);
                return;
            }
            int total = TotalStats.Compute(actor);
            text.text = total.ToString("N0");
            var graphics = cell.GetComponentsInChildren<Graphic>(true).Where(g => !(g is TMP_Text)).ToArray();
            TotalColors.Set(text, graphics, total);
        }
        catch (Exception e)
        {
            ModLog.Warn($"row patch failed: {e}");
        }
    }
}

// Sorting by total is scoped to comparers whose sort field was set by our header,
// so every other soldier list in the game is unaffected.
[HarmonyPatch(typeof(StrategyActorComparer), nameof(StrategyActorComparer.Compare))]
public static class StrategyActorComparerPatch
{
    [HarmonyPrefix]
    public static bool Prefix(StrategyActorComparer __instance, Entity x, Entity y, ref int __result)
    {
        if (__instance.sortField != TotalStats.TotalSortField)
            return true;
        if (Equals(x, y))
            __result = 0;
        else if (x == null)
            __result = 1;
        else if (y == null)
            __result = -1;
        else
        {
            __result = __instance.sortOrder * TotalStats.Compute(x).CompareTo(TotalStats.Compute(y));
            if (__result == 0)
                __result = EntityNameComparer.Default.Compare(x, y);
        }
        return false;
    }
}
