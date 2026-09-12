using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuickSwap
{
    /// <summary>
    /// Badges the anchored slot wherever it is on screen — the hotbar along the bottom, and
    /// the top row of the inventory while it is open.
    /// </summary>
    internal static class AnchorMarker
    {
        private const string MarkerName = "QuickSwapAnchorMarker";

        /// <summary>
        /// Badge size in the element's own units. Slots run roughly 64-70 across, so this
        /// sits at about a third of the slot — big enough to read, small enough to leave the
        /// item icon alone. Fixed rather than measured: the host's rect can still be zero
        /// when we build this, and the canvas scaler handles UI scale for us either way.
        /// </summary>
        private const float BadgeWidth = 24f;

        private const float BadgeHeight = 17f;

        /// <summary>Inset from the slot's bottom-right corner.</summary>
        private const float BadgeMargin = 1f;

        private const string ArrowsName = "Arrows";

        /// <summary>Everything we have created, so a hot reload can take it all back off.</summary>
        private static readonly List<GameObject> Markers = new List<GameObject>();

        /// <summary>Marks the anchored slot on the on-screen hotbar.</summary>
        internal static void Apply(HotkeyBar bar)
        {
            if (bar == null || bar.m_elements == null || bar.m_elements.Count == 0)
            {
                return;
            }

            // Other mods (e.g. extended inventories) add their own HotkeyBar for rows below
            // the hotbar. Only the bar backed by inventory row 0 gets a marker.
            int anchorIndex = IsMainHotbar(bar) ? AnchorIndex() : -1;
            Color colour = ModConfig.MarkerColor();

            for (int i = 0; i < bar.m_elements.Count; i++)
            {
                GameObject host = bar.m_elements[i].m_go;
                if (host != null)
                {
                    Mark(host, i == anchorIndex, colour);
                }
            }
        }

        /// <summary>Marks the anchored slot in the open inventory's top row.</summary>
        internal static void Apply(InventoryGrid grid)
        {
            if (grid == null || grid.m_elements == null)
            {
                return;
            }

            // Container grids share this class but have no hotbar row, so only the player's
            // own grid is marked — otherwise every open chest would sprout a marker.
            InventoryGui gui = InventoryGui.instance;
            if (gui == null || gui.m_playerGrid != grid)
            {
                return;
            }

            int anchorIndex = AnchorIndex();
            Color colour = ModConfig.MarkerColor();

            foreach (InventoryElement element in grid.m_elements)
            {
                if (element == null)
                {
                    continue;
                }

                Vector2i pos = element.Position;
                Mark(element.gameObject, pos.y == 0 && pos.x == anchorIndex, colour);
            }
        }

        /// <summary>Removes every marker we created. Called on unload so hot reloads stay clean.</summary>
        internal static void RemoveAll()
        {
            foreach (GameObject marker in Markers)
            {
                if (marker != null)
                {
                    Object.Destroy(marker);
                }
            }

            Markers.Clear();
        }

        /// <summary>Zero-based index of the anchored slot, or -1 when there is nothing to show.</summary>
        private static int AnchorIndex()
        {
            return ModConfig.ShowAnchorMarker.Value ? ModConfig.AnchorSlot.Value - 1 : -1;
        }

        private static void Mark(GameObject host, bool wanted, Color colour)
        {
            GameObject marker = FindMarker(host);

            if (marker == null)
            {
                if (!wanted)
                {
                    return;
                }

                marker = CreateMarker(host);
            }

            if (marker.activeSelf != wanted)
            {
                marker.SetActive(wanted);
            }

            if (!wanted)
            {
                return;
            }

            Transform arrows = marker.transform.Find(ArrowsName);
            if (arrows == null)
            {
                return;
            }

            Image image = arrows.GetComponent<Image>();
            if (image != null && image.color != colour)
            {
                image.color = colour;
            }
        }

        private static bool IsMainHotbar(HotkeyBar bar)
        {
            List<ItemDrop.ItemData> items = bar.m_items;
            if (items == null || items.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == null || items[i].m_gridPos.y != 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static GameObject FindMarker(GameObject host)
        {
            Transform found = host.transform.Find(MarkerName);
            return found == null ? null : found.gameObject;
        }

        /// <summary>
        /// Builds the badge: a dark chip in the bottom-right corner carrying the mod's swap
        /// arrows. It deliberately sits over the durability bar's right end — that corner is
        /// the quietest part of a slot, and an outline round the whole slot collides with
        /// mods that frame slots themselves.
        /// </summary>
        private static GameObject CreateMarker(GameObject host)
        {
            Markers.RemoveAll(marker => marker == null);

            var badge = new GameObject(MarkerName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rect = (RectTransform)badge.transform;
            rect.SetParent(host.transform, false);
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.sizeDelta = new Vector2(BadgeWidth, BadgeHeight);
            rect.anchoredPosition = new Vector2(-BadgeMargin, BadgeMargin);

            Image plate = badge.GetComponent<Image>();
            plate.color = new Color(0f, 0f, 0f, 0.72f);
            plate.raycastTarget = false;

            var arrows = new GameObject(ArrowsName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var arrowsRect = (RectTransform)arrows.transform;
            arrowsRect.SetParent(rect, false);
            arrowsRect.anchorMin = Vector2.zero;
            arrowsRect.anchorMax = Vector2.one;
            arrowsRect.offsetMin = new Vector2(2f, 2f);
            arrowsRect.offsetMax = new Vector2(-2f, -2f);

            Image glyph = arrows.GetComponent<Image>();
            glyph.sprite = MarkerSprite.Get();
            glyph.preserveAspect = true;
            glyph.raycastTarget = false;
            glyph.color = ModConfig.MarkerColor();

            rect.SetAsLastSibling();
            Markers.Add(badge);
            return badge;
        }

    }
}
