using System;
using System.Collections.Generic;
using System.Linq;
using Artitas;
using Common.UI.Unity;
using HarmonyLib;
using Strategy.UI.Elements.Soldiers;
using TMPro;
using UnityEngine.UI;
using Xenonauts.Common.Util.Comparers;

namespace SoldierTotalStats;

// The TOT column itself. Every soldier list in the game (soldier picker, Manage, Recruit) is built
// the same way: a header row of sort toggles and pooled rows of stat cells, sorted through a
// StrategyActorComparer held in the screen's "_comparer" field. So one implementation serves all
// of them; the per-screen patches only say which screen they are and where its row list lives.
internal static class TotalColumn
{
    public const string HeaderName = "SoldierTotalStats_Header";
    public const string CellName = "SoldierTotalStats_Cell";

    // screen: the screen object (its private _comparer / _columnsChangeComparer fields are used).
    // controllersField: name of the screen's private row list field, which has the Sort() method.
    public static void AddHeader(object screen, Dictionary<SortField, GHIToggle> toggles, string controllersField, string label)
    {
        try
        {
            if (toggles == null || !toggles.TryGetValue(SortField.Bravery, out var bravery) || bravery == null)
            {
                ModLog.Warn($"{label}: Bravery header not found; columns present: "
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
                ModLog.Warn($"{label}: cloned header has no GHIToggle; sorting disabled");
                return;
            }
            toggle.SetIsOnWithoutNotify(false);
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onLeftClick.RemoveAllListeners();
            // Same behaviour as the game's own column handlers in OnCreate.
            toggle.onValueChanged.AddListener(isOn =>
            {
                try
                {
                    var traverse = Traverse.Create(screen);
                    if (!isOn || !traverse.Field<bool>("_columnsChangeComparer").Value)
                        return;
                    traverse.Field<StrategyActorComparer>("_comparer").Value.SetSelectedSortColumn(TotalStats.TotalSortField);
                    traverse.Field(controllersField).Method("Sort").GetValue();
                }
                catch (Exception e)
                {
                    ModLog.Warn($"{label}: sort by total failed: {e}");
                }
            });
            // Registered after the game wired its own listeners, so the game only uses this entry
            // to reset the header highlight when the screen opens, as it does for its own columns.
            toggles[TotalStats.TotalSortField] = toggle;
            ModLog.Info($"{label}: header added");
        }
        catch (Exception e)
        {
            ModLog.Warn($"{label}: header patch failed: {e}");
        }
    }

    public static void UpdateRow(TMP_Text braveryText, TMP_Text reflexesText, Entity actor, string label)
    {
        try
        {
            if (braveryText == null || reflexesText == null)
                return;
            var (braveryCell, reflexesCell) = UiClone.SiblingCells(braveryText.transform, reflexesText.transform);
            if (braveryCell == null || reflexesCell == null)
            {
                ModLog.Warn($"{label}: could not locate the Bravery cell in the row");
                return;
            }
            var cell = UiClone.CloneAfter(braveryCell, CellName);
            var text = cell.GetComponentsInChildren<TMP_Text>(true).FirstOrDefault();
            if (text == null)
                return;
            text.textWrappingMode = TextWrappingModes.NoWrap;

            if (actor == null)
            {
                text.text = string.Empty;
                TotalColors.Remove(label, text);
                return;
            }
            int total = TotalStats.Compute(actor);
            text.text = total.ToString("N0");
            var graphics = cell.GetComponentsInChildren<Graphic>(true).Where(g => !(g is TMP_Text)).ToArray();
            TotalColors.Set(label, text, graphics, total);
        }
        catch (Exception e)
        {
            ModLog.Warn($"{label}: row patch failed: {e}");
        }
    }
}
