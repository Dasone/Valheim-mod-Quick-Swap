using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuickSwap
{
    /// <summary>
    /// Draws a thin coloured bar under the anchored slot on the on-screen hotbar,
    /// so you can see at a glance where the anchor key will take you.
    /// </summary>
    internal static class AnchorMarker
    {
        private const string MarkerName = "QuickSwapAnchorMarker";

        /// <summary>Hotbars we have touched, so a hot reload can clean up after itself.</summary>
        private static readonly List<HotkeyBar> Decorated = new List<HotkeyBar>();

        internal static void Apply(HotkeyBar bar)
        {
            if (bar == null || bar.m_elements == null || bar.m_elements.Count == 0)
            {
                return;
            }

            // Other mods (e.g. extended inventories) add their own HotkeyBar for rows
            // below the hotbar. Only the bar backed by inventory row 0 gets a marker.
            if (!IsMainHotbar(bar))
            {
                Clear(bar);
                return;
            }

            if (!Decorated.Contains(bar))
            {
                Decorated.Add(bar);
            }

            int anchorIndex = ModConfig.ShowAnchorMarker.Value ? ModConfig.AnchorSlot.Value - 1 : -1;
            Color color = ModConfig.MarkerColor();

            for (int i = 0; i < bar.m_elements.Count; i++)
            {
                GameObject host = bar.m_elements[i].m_go;
                if (host == null)
                {
                    continue;
                }

                bool wanted = i == anchorIndex;
                GameObject marker = FindMarker(host);

                if (marker == null)
                {
                    if (!wanted)
                    {
                        continue;
                    }

                    marker = CreateMarker(host);
                }

                if (marker.activeSelf != wanted)
                {
                    marker.SetActive(wanted);
                }

                if (wanted)
                {
                    Image image = marker.GetComponent<Image>();
                    if (image != null && image.color != color)
                    {
                        image.color = color;
                    }
                }
            }
        }

        /// <summary>Removes every marker we created. Called on unload so hot reloads stay clean.</summary>
        internal static void RemoveAll()
        {
            foreach (HotkeyBar bar in Decorated)
            {
                Clear(bar);
            }

            Decorated.Clear();
        }

        private static void Clear(HotkeyBar bar)
        {
            if (bar == null || bar.m_elements == null)
            {
                return;
            }

            foreach (HotkeyBar.ElementData element in bar.m_elements)
            {
                if (element == null || element.m_go == null)
                {
                    continue;
                }

                GameObject marker = FindMarker(element.m_go);
                if (marker != null)
                {
                    Object.Destroy(marker);
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

        private static GameObject CreateMarker(GameObject host)
        {
            var marker = new GameObject(MarkerName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

            var rect = (RectTransform)marker.transform;
            rect.SetParent(host.transform, false);
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = new Vector2(3f, 1f);   // left / bottom inset
            rect.offsetMax = new Vector2(-3f, 5f);  // right inset, 4px tall

            Image image = marker.GetComponent<Image>();
            image.raycastTarget = false;
            image.color = ModConfig.MarkerColor();

            rect.SetAsLastSibling();
            return marker;
        }
    }
}
