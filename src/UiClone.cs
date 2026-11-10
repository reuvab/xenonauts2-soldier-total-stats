using System;
using System.Collections.Generic;
using Common.UI.Tooltips;
using Common.UI.Tooltips.Creators;
using UnityEngine;
using UnityEngine.UI;

namespace SquadSelectTotal;

internal static class UiClone
{
    // Clones `source` as a new sibling placed right after it. Header and row are both plain
    // HorizontalLayoutGroups with fixed column widths that already fill the panel exactly
    // (258 + 6 x 70 + 150 + 180 = 1008), so the extra column cannot simply be appended: the
    // header cells would shrink while the row cells (which have a min width) would not.
    // Instead the columns as wide as `source` (the stat columns) are narrowed so that,
    // together with the clone, they take the same total width as before (7 x 60 = 6 x 70).
    // Applied identically to the header and to every row, this keeps them aligned by layout.
    public static GameObject CloneAfter(Transform source, string name)
    {
        var parent = source.parent;
        var existing = parent.Find(name);
        if (existing != null)
            return existing.gameObject;

        var clone = UnityEngine.Object.Instantiate(source.gameObject, parent);
        clone.name = name;
        clone.transform.SetSiblingIndex(source.GetSiblingIndex() + 1);

        // Localization components would overwrite our text with the original column's label.
        foreach (var t in clone.GetComponentsInChildren<Transform>(true))
            StripLocalization(t.gameObject);

        MakeRoom(source, parent);
        return clone;
    }

    private static void MakeRoom(Transform source, Transform parent)
    {
        var sourceLayout = source.GetComponent<LayoutElement>();
        if (sourceLayout == null || sourceLayout.preferredWidth <= 0f)
        {
            ModLog.Warn($"'{source.name}' has no fixed layout width; column widths left unchanged");
            return;
        }
        float width = sourceLayout.preferredWidth;
        var columns = new List<LayoutElement>();
        foreach (Transform child in parent)
        {
            var layout = child.GetComponent<LayoutElement>();
            // Hidden columns (the Manage row keeps an inactive "stress" cell) take no space.
            if (child.gameObject.activeSelf && layout != null && !layout.ignoreLayout
                && Mathf.Approximately(layout.preferredWidth, width))
                columns.Add(layout);
        }
        // columns includes the clone, so there is one more than before.
        float narrowed = width * (columns.Count - 1) / columns.Count;
        foreach (var layout in columns)
        {
            layout.preferredWidth = narrowed;
            if (layout.minWidth > 0f)
                layout.minWidth = narrowed;
        }
        if (parent is RectTransform rect)
            LayoutRebuilder.MarkLayoutForRebuild(rect);
    }

    private static void StripLocalization(GameObject go)
    {
        foreach (var mb in go.GetComponents<MonoBehaviour>())
        {
            if (mb == null)
                continue;
            var type = mb.GetType().FullName ?? string.Empty;
            if (type == "I2.Loc.Localize" || type == "Common.Translation.Assets.TranslationScanKeyBehaviour")
                UnityEngine.Object.Destroy(mb);
        }
    }

    // Finds the ancestor of `descendant` that is a direct child of `container`.
    public static Transform ChildOf(Transform descendant, Transform container)
    {
        for (var t = descendant; t != null; t = t.parent)
        {
            if (t.parent == container)
                return t;
        }
        return null;
    }

    // Finds the closest ancestors of `a` and `b` that share a parent (the two column cells).
    public static (Transform a, Transform b) SiblingCells(Transform a, Transform b)
    {
        var ancestorsOfB = new HashSet<Transform>();
        for (var p = b.parent; p != null; p = p.parent)
            ancestorsOfB.Add(p);
        for (var t = a; t != null && t.parent != null; t = t.parent)
        {
            if (ancestorsOfB.Contains(t.parent))
                return (t, ChildOf(b, t.parent));
        }
        return (null, null);
    }

    public static void ReplaceTooltip(GameObject clone, string title, string description)
    {
        foreach (var mb in clone.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (mb != null && (mb.GetType().FullName ?? string.Empty).EndsWith("TooltipCreator", StringComparison.Ordinal))
                UnityEngine.Object.DestroyImmediate(mb);
        }

        StaticTextTooltip prefab = null;
        foreach (var creator in UnityEngine.Object.FindObjectsOfType<StaticTextTooltipCreator>(true))
        {
            if (creator != null && creator.Tooltip != null)
            {
                prefab = creator.Tooltip;
                break;
            }
        }
        if (prefab == null)
        {
            ModLog.Warn("no tooltip prefab found; header has no tooltip");
            return;
        }
        var tooltip = clone.AddComponent<StaticTextTooltipCreator>();
        tooltip.Tooltip = prefab;
        tooltip.Title = title;
        tooltip.Description = description;
        tooltip.AllowTooltip = true;
    }
}
