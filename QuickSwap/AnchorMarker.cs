using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuickSwap
{
    /// <summary>
    /// Outlines the anchored slot wherever it is on screen — the hotbar along the bottom,
    /// and the top row of the inventory while it is open.
    /// </summary>
    internal static class AnchorMarker
    {
        private const string MarkerName = "QuickSwapAnchorMarker";

        /// <summary>Edge width in the element's own units; slots are roughly 64-70 across.</summary>
        private const float Thickness = 3f;

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

            foreach (InventoryGrid.Element element in grid.m_elements)
            {
                if (element == null || element.m_go == null)
                {
                    continue;
                }

                Mark(element.m_go, element.m_pos.y == 0 && element.m_pos.x == anchorIndex, colour);
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

            foreach (Image edge in marker.GetComponentsInChildren<Image>(true))
            {
                if (edge.color != colour)
                {
                    edge.color = colour;
                }
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
        /// Builds the outline as four thin edges rather than one filled rect, so the item
        /// icon stays fully visible and nothing lands on top of the durability bar.
        /// </summary>
        private static GameObject CreateMarker(GameObject host)
        {
            Markers.RemoveAll(marker => marker == null);

            var frame = new GameObject(MarkerName, typeof(RectTransform));
            var rect = (RectTransform)frame.transform;
            rect.SetParent(host.transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            AddEdge(rect, new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -Thickness), Vector2.zero);
            AddEdge(rect, Vector2.zero, new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, Thickness));
            AddEdge(rect, Vector2.zero, new Vector2(0f, 1f), Vector2.zero, new Vector2(Thickness, 0f));
            AddEdge(rect, new Vector2(1f, 0f), Vector2.one, new Vector2(-Thickness, 0f), Vector2.zero);

            rect.SetAsLastSibling();
            Markers.Add(frame);
            return frame;
        }

        private static void AddEdge(RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var edge = new GameObject("Edge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rect = (RectTransform)edge.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            edge.GetComponent<Image>().raycastTarget = false;
        }
    }
}
