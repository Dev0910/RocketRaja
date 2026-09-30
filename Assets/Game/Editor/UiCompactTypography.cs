using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace RocketRaja.EditorTools
{
    /// <summary>Reflows the saved browse panels for a 12-pixel text floor at 360-pixel width.</summary>
    public static class UiCompactTypography
    {
        const float MinimumSize = 36;

        public static void Apply(GameObject root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Compact typography must be authored outside Play Mode.");

            var rects = root.GetComponentsInChildren<RectTransform>(true);
            if (root.GetComponent<ImportedProductsScreen>() != null) Products(rects);
            else if (root.GetComponent<ImportedPlayerScreen>() != null) Player(rects);
            else if (root.GetComponent<ImportedLegacyScreen>() != null) Legacy(rects);
            else return;

            // Quantity choices are saved controls too. Their 132-unit rows already
            // accommodate the selected option's two lines at the same readable size.
            foreach (var rect in rects.Where(x => x.name.StartsWith("Quantity Buy ", StringComparison.Ordinal)))
                Read(rect.Find("Label").GetComponent<TMP_Text>(), MinimumSize, true);
            EditorUtility.SetDirty(root);
        }

        static void Products(RectTransform[] rects)
        {
            foreach (var row in rects.Where(x => x.name.StartsWith("Product Row ", StringComparison.Ordinal)))
            {
                Height(row, 260);
                var name = row.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name.StartsWith("product-name-", StringComparison.Ordinal));
                var level = row.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name.StartsWith("product-level-", StringComparison.Ordinal));
                var income = row.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name.StartsWith("product-income-", StringComparison.Ordinal));
                var next = row.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name.StartsWith("product-next-", StringComparison.Ordinal));
                Top(name.rectTransform, 213, 350, 12, 56); Read(name, 38, false);
                Top(level.rectTransform, 213, 350, 70, 52); Read(level, MinimumSize, false);
                Top(income.rectTransform, 213, 350, 124, 52); Read(income, MinimumSize, false);
                Top(next.rectTransform, 213, 350, 178, 52); Read(next, MinimumSize, false);
                var price = row.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == "Price");
                ButtonCopy(price);
                var cue = row.Find("Art Frame/Details Cue")?.GetComponent<TMP_Text>();
                if (cue != null) Read(cue, MinimumSize, false);
            }
        }

        static void Player(RectTransform[] rects)
        {
            var toolbar = Named(rects, "Player Toolbar");
            Read(toolbar.Find("open-stats/Stats Label").GetComponent<TMP_Text>(), MinimumSize, false);
            Read(toolbar.Find("open-skills/Skills Label").GetComponent<TMP_Text>(), MinimumSize, false);

            var tap = Named(rects, "Salesmanship");
            Height(tap, 260);
            var name = tap.Find("Name").GetComponent<TMP_Text>();
            Top(name.rectTransform, 170, 350, 15, 60); Read(name, 41, false);
            var level = tap.Find("tap-level").GetComponent<TMP_Text>();
            Top(level.rectTransform, 170, 350, 83, 54); Read(level, MinimumSize, false);
            var income = tap.Find("tap-income").GetComponent<TMP_Text>();
            Top(income.rectTransform, 170, 350, 148, 54); Read(income, MinimumSize, false);
            ButtonCopy(tap.Find("buy-tap/Buy Label").GetComponent<TMP_Text>());

            var prestige = Named(rects, "Prestige");
            Height(prestige, 264);
            var title = prestige.Find("Heading").GetComponent<TMP_Text>();
            Top(title.rectTransform, 170, 350, 18, 60); Read(title, 41, false);
            var description = prestige.Find("Description").GetComponent<TMP_Text>();
            Top(description.rectTransform, 170, 350, 96, 112); Read(description, MinimumSize, true);
            ButtonCopy(prestige.Find("open-prestige/Prestige Label").GetComponent<TMP_Text>());

            var heading = Named(rects, "Unlockables Heading");
            Height(heading, 164);
            var headingTitle = heading.Find("Title").GetComponent<TMP_Text>();
            Top(headingTitle.rectTransform, 12, 400, 12, 68); Read(headingTitle, 41, false);
            var count = heading.Find("unlockable-count").GetComponent<TMP_Text>();
            Top(count.rectTransform, 12, 350, 95, 54); Read(count, MinimumSize, false);
            var buyAll = heading.Find("buy-all") as RectTransform;
            buyAll.offsetMin = new Vector2(buyAll.offsetMin.x, 10);
            buyAll.offsetMax = new Vector2(buyAll.offsetMax.x, -10);
            ButtonCopy(buyAll.Find("Label").GetComponent<TMP_Text>());

            foreach (var row in rects.Where(x => x.name.StartsWith("Unlockable Row ", StringComparison.Ordinal)))
            {
                Height(row, 236);
                var rowName = row.Find("Name").GetComponent<TMP_Text>();
                Top(rowName.rectTransform, 42, 365, 14, 54); Read(rowName, 37, false);
                var effect = row.Find("Effect").GetComponent<TMP_Text>();
                Top(effect.rectTransform, 42, 365, 82, 104); Read(effect, MinimumSize, true);
                ButtonCopy(row.Find("Buy/Price").GetComponent<TMP_Text>());
            }
        }

        static void Legacy(RectTransform[] rects)
        {
            var summary = Named(rects, "Reputation Summary");
            Height(summary, 342);
            var balance = summary.Find("reputation-value").GetComponent<TMP_Text>();
            Top(balance.rectTransform, 60, 60, 20, 120); Read(balance, 80, false);
            var caption = summary.Find("Reputation caption").GetComponent<TMP_Text>();
            Top(caption.rectTransform, 170, 170, 150, 58); Read(caption, 38, false);
            var explanation = summary.Find("Explanation").GetComponent<TMP_Text>();
            Top(explanation.rectTransform, 60, 60, 218, 104); Read(explanation, MinimumSize, true);

            foreach (var row in rects.Where(x => x.name.StartsWith("Legacy Row ", StringComparison.Ordinal)))
            {
                Height(row, 280);
                var name = row.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name.StartsWith("perk-name-", StringComparison.Ordinal));
                var rank = row.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name.StartsWith("perk-rank-", StringComparison.Ordinal));
                var effect = row.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name.StartsWith("perk-effect-", StringComparison.Ordinal));
                Top(name.rectTransform, 150, 365, 16, 60); Read(name, MinimumSize, false);
                Top(rank.rectTransform, 150, 365, 82, 54); Read(rank, MinimumSize, false);
                Top(effect.rectTransform, 150, 365, 146, 104); Read(effect, MinimumSize, true);
                ButtonCopy(row.GetComponentsInChildren<TMP_Text>(true).Single(x => x.name == "Price"));
            }
        }

        static RectTransform Named(RectTransform[] rects, string name) => rects.Single(x => x.name == name);

        static void Height(RectTransform rect, float height)
        {
            var layout = rect.GetComponent<UnityEngine.UI.LayoutElement>();
            layout.preferredHeight = height;
            EditorUtility.SetDirty(layout);
        }

        static void Top(RectTransform rect, float left, float right, float top, float height)
        {
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(.5f, 1);
            rect.offsetMin = new Vector2(left, -top - height);
            rect.offsetMax = new Vector2(-right, -top);
            EditorUtility.SetDirty(rect);
        }

        static void ButtonCopy(TMP_Text text)
        {
            Read(text, MinimumSize, true);
            text.margin = new Vector4(12, 8, 12, 8);
        }

        static void Read(TMP_Text text, float size, bool wrap)
        {
            text.enableAutoSizing = false;
            text.fontSize = text.fontSizeMin = text.fontSizeMax = size;
            text.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Ellipsis;
            EditorUtility.SetDirty(text);
        }
    }
}
