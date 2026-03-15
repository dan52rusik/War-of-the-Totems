using UnityEngine;
using UnityEngine.UI;

namespace WarOfTheTotems.UI
{
    /// <summary>
    /// Статические хелперы для процедурного построения Unity UI.
    /// Заменяет EnsurePanel/EnsureText/EnsureButton из PrototypeBattleController.
    /// Логика "найти или создать" — если объект с таким именем уже есть в parent,
    /// он переиспользуется, а не создаётся заново (безопасно при повторных вызовах).
    /// </summary>
    public static class UiBuilder
    {
        // ----------------------------------------------------------------
        // Panel
        // ----------------------------------------------------------------

        public static RectTransform EnsurePanel(
            string name, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPos, Vector2 size,
            Color color)
        {
            var rect = GetOrCreate(name, parent);
            rect.anchorMin        = anchorMin;
            rect.anchorMax        = anchorMax;
            rect.pivot            = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta        = size;

            var img = EnsureComponent<Image>(rect.gameObject);
            img.color = color;
            return rect;
        }

        // ----------------------------------------------------------------
        // Text
        // ----------------------------------------------------------------

        public static Text EnsureText(
            string name, Transform parent,
            string content, TextAnchor alignment, int fontSize,
            Vector2 anchoredPos, Vector2 size,
            Color? color = null)
        {
            var rect = GetOrCreate(name, parent);
            rect.anchorMin        = new Vector2(0.5f, 0.5f);
            rect.anchorMax        = new Vector2(0.5f, 0.5f);
            rect.pivot            = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta        = size;

            var txt = EnsureComponent<Text>(rect.gameObject);
            txt.text      = content;
            txt.alignment = alignment;
            txt.fontSize  = fontSize;
            txt.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.color     = color ?? new Color(0.95f, 0.92f, 0.84f);
            return txt;
        }

        // ----------------------------------------------------------------
        // Button
        // ----------------------------------------------------------------

        public static Button EnsureButton(
            string name, Transform parent,
            string label, Vector2 anchoredPos, Vector2 size,
            Color bgColor)
        {
            var rect = GetOrCreate(name, parent);
            rect.anchorMin        = new Vector2(0.5f, 0.5f);
            rect.anchorMax        = new Vector2(0.5f, 0.5f);
            rect.pivot            = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta        = size;

            var img = EnsureComponent<Image>(rect.gameObject);
            img.color = bgColor;

            var btn = EnsureComponent<Button>(rect.gameObject);

            EnsureText($"{name}Label", rect, label, TextAnchor.MiddleCenter, 22,
                Vector2.zero, size * 0.9f,
                new Color(0.98f, 0.95f, 0.88f));
            return btn;
        }

        // ----------------------------------------------------------------
        // SetRect — выставить позицию/размер произвольному RectTransform
        // ----------------------------------------------------------------

        public static void SetRect(RectTransform? rect, Vector2 anchoredPos, Vector2 size)
        {
            if (rect == null) return;
            rect.anchorMin        = new Vector2(0.5f, 0.5f);
            rect.anchorMax        = new Vector2(0.5f, 0.5f);
            rect.pivot            = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta        = size;
        }

        // ----------------------------------------------------------------
        // SetPanelVisible
        // ----------------------------------------------------------------

        public static void SetVisible(RectTransform? panel, bool visible)
        {
            if (panel != null) panel.gameObject.SetActive(visible);
        }

        // ----------------------------------------------------------------
        // Приватные хелперы
        // ----------------------------------------------------------------

        private static RectTransform GetOrCreate(string name, Transform parent)
        {
            var existing = parent.Find(name);
            if (existing != null)
            {
                var r = existing.GetComponent<RectTransform>();
                if (r != null) return r;
            }

            var go   = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.AddComponent<RectTransform>();
        }

        private static T EnsureComponent<T>(GameObject go) where T : Component
            => go.TryGetComponent<T>(out var c) ? c : go.AddComponent<T>();
    }
}
