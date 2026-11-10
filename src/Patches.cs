using Artitas;
using Common.Utilities.Comparers;
using HarmonyLib;
using Strategy.UI.Elements.Soldiers;
using Xenonauts.Common.Util.Comparers;
using Xenonauts.Strategy.UI;

namespace SquadSelectTotal;

// One header patch, one "screen opened" patch and one row patch per soldier list.
// Colours are relative to the soldiers listed on that screen, so they reset when it opens.

// --- Soldier picker (filling a dropship / strike team slot) ---

[HarmonyPatch(typeof(SoldierSelectionElement), nameof(SoldierSelectionElement.OnCreate))]
public static class SelectionHeaderPatch
{
    public const string Screen = "soldier picker";

    [HarmonyPostfix]
    public static void Postfix(SoldierSelectionElement __instance) =>
        TotalColumn.AddHeader(__instance, __instance.columnToggles, "_controllers", Screen);
}

[HarmonyPatch(typeof(SoldierSelectionElement), nameof(SoldierSelectionElement.OnEnter))]
public static class SelectionEnterPatch
{
    [HarmonyPrefix]
    public static void Prefix() => TotalColors.Clear(SelectionHeaderPatch.Screen);
}

[HarmonyPatch(typeof(SoldierSelectionRowController), "OnSetTarget")]
public static class SelectionRowPatch
{
    [HarmonyPostfix]
    public static void Postfix(SoldierSelectionRowController __instance) =>
        TotalColumn.UpdateRow(__instance.braveryText, __instance.reflexesText, __instance.Target?.X, SelectionHeaderPatch.Screen);
}

// --- Soldiers > Manage ---

[HarmonyPatch(typeof(SoldiersManageElement), nameof(SoldiersManageElement.OnCreate))]
public static class ManageHeaderPatch
{
    public const string Screen = "manage soldiers";

    [HarmonyPostfix]
    public static void Postfix(SoldiersManageElement __instance) =>
        TotalColumn.AddHeader(__instance, __instance.columnToggles, "_soldierControllers", Screen);
}

[HarmonyPatch(typeof(SoldiersManageElement), nameof(SoldiersManageElement.OnEnter))]
public static class ManageEnterPatch
{
    [HarmonyPrefix]
    public static void Prefix() => TotalColors.Clear(ManageHeaderPatch.Screen);
}

[HarmonyPatch(typeof(SoldierInfoRowController), "OnSetTarget")]
public static class ManageRowPatch
{
    [HarmonyPostfix]
    public static void Postfix(SoldierInfoRowController __instance) =>
        TotalColumn.UpdateRow(__instance.braveryText, __instance.reflexesText, __instance.Target?.X, ManageHeaderPatch.Screen);
}

// --- Soldiers > Recruit ---

[HarmonyPatch(typeof(SoldiersRecruitElement), nameof(SoldiersRecruitElement.OnCreate))]
public static class RecruitHeaderPatch
{
    public const string Screen = "recruit soldiers";

    [HarmonyPostfix]
    public static void Postfix(SoldiersRecruitElement __instance) =>
        TotalColumn.AddHeader(__instance, __instance.columnToggles, "_soldierControllers", Screen);
}

[HarmonyPatch(typeof(SoldiersRecruitElement), nameof(SoldiersRecruitElement.OnEnter))]
public static class RecruitEnterPatch
{
    [HarmonyPrefix]
    public static void Prefix() => TotalColors.Clear(RecruitHeaderPatch.Screen);
}

[HarmonyPatch(typeof(SoldierRecruitmentRowController), "OnSetTarget")]
public static class RecruitRowPatch
{
    [HarmonyPostfix]
    public static void Postfix(SoldierRecruitmentRowController __instance) =>
        TotalColumn.UpdateRow(__instance.braveryText, __instance.reflexesText, __instance.Target, RecruitHeaderPatch.Screen);
}

// Sorting by total is scoped to comparers whose sort field was set by our header,
// so sorting by any of the game's own columns is unaffected.
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
