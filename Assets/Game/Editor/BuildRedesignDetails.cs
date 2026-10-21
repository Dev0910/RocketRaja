using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    public static partial class BuildRedesignUI
    {
        static VerticalLayoutGroup Vertical(RectTransform root, float padding = 0, float spacing = 10)
        {
            var group = root.gameObject.AddComponent<VerticalLayoutGroup>();
            group.padding = new RectOffset((int)(padding * S), (int)(padding * S), (int)(padding * S), (int)(padding * S));
            group.spacing = spacing * S; group.childControlHeight = group.childControlWidth = true;
            group.childForceExpandHeight = false; group.childForceExpandWidth = true; return group;
        }
        static void IgnoreLayout(RectTransform root) => root.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
        static void Height(RectTransform root, float height, float flexible = 0)
        {
            var element = root.gameObject.AddComponent<LayoutElement>(); element.minHeight = height * S;
            element.preferredHeight = height * S; element.flexibleHeight = flexible;
        }
        static TMP_Text FlowText(Transform parent, string name, string value, float size = 15, bool title = false)
        {
            var text = Text(New(name, parent), value, size, title); text.overflowMode = TextOverflowModes.Overflow;
            return text;
        }
        static RectTransform Panel(Transform parent, string name, string title)
        {
            var root = New(name, parent); Image(root, Hex("CC9B53"), false, true);
            var face = Fill("Face", root, 1.5f, 1.5f, 1.5f, 1.5f); Image(face, Hex("FFF4D8"), false, true); Texture(face); IgnoreLayout(face);
            Vertical(root, 14, 8); if (!string.IsNullOrEmpty(title)) FlowText(root, "Heading", title, 19, true);
            return root;
        }
        static Button FlowButton(Transform parent, string name, string label, bool primary = false)
        {
            var rect = New(name, parent); Height(rect, 52); return Button(rect, label, primary);
        }
        static Button[] FlowPair(Transform parent, string name, string a, string b)
        {
            var root = New(name, parent); Height(root, 52); return Segment(root, new[] { a, b });
        }
        static RectTransform FlowScroll(RectTransform root, out ScrollRect scroll)
        {
            var scrollRoot = Fill("Scroll", root); scroll = scrollRoot.gameObject.AddComponent<ScrollRect>();
            var viewport = Fill("Viewport", scrollRoot); Image(viewport, new Color(1, 1, 1, .001f), true); viewport.gameObject.AddComponent<RectMask2D>();
            var content = Top("Content", viewport, 0, 0); Vertical(content, 14, 10);
            var fit = content.gameObject.AddComponent<ContentSizeFitter>(); fit.horizontalFit = ContentSizeFitter.FitMode.Unconstrained; fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false; scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 45 * S; return content;
        }
        public static void AddDetailsAndBatch()
        {
            if (EditorApplication.isPlaying || EditorApplication.isCompiling) throw new InvalidOperationException("Require compiled Edit Mode");
            LoadAssets();
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var pages = (RectTransform)root.transform.Find("Safe Area/Screen States");
                if (pages.Find("screen-upgradedetails") || pages.Find("screen-buyall")) throw new InvalidOperationException("Detail/batch pages already exist; use a scoped revision");
                BuildDetails(Fill("screen-upgradedetails", pages)); BuildBatch(Fill("screen-buyall", pages));
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        public static void BindCatalogueRouter()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Edit Mode only");
            LoadAssets(); var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var router = root.GetComponent<RedesignScreenRouter>(); if (!router) router = root.AddComponent<RedesignScreenRouter>();
                var driver = root.GetComponent<RedesignRuntimeDriver>(); if (!driver) driver = root.AddComponent<RedesignRuntimeDriver>();
                router.driver = driver; router.upgrades = root.GetComponentInChildren<RedesignUpgradesScreen>(true);
                router.details = root.GetComponentInChildren<RedesignUpgradeDetailsScreen>(true); router.batch = root.GetComponentInChildren<RedesignBatchScreen>(true);
                router.title = root.transform.Find("Safe Area/Header/screen-title").GetComponent<TMP_Text>();
                router.summary = root.transform.Find("Safe Area/Header/header-summary").GetComponent<TMP_Text>();
                router.upgradesTab = root.transform.Find("Safe Area/Navigation/Tabs/Upgrades").GetComponent<Button>();
                // Startup remains explicitly staged until Home/offline/recovery and remaining pages are bound.
                router.initializeGuestOnStart = false;
                var toast = root.transform.Find("Safe Area/Toast") as RectTransform;
                if (!toast)
                {
                    toast = Fill("Toast", root.transform.Find("Safe Area"), 20, 20, 80, 0); toast.anchorMax = new Vector2(1, 0);
                    toast.offsetMax = new Vector2(-20 * S, 158 * S); Image(toast, Maroon, false, true);
                    var t = Text(Fill("Message", toast, 12, 12, 8, 8), "", 14); t.color = Cream; t.alignment = TextAlignmentOptions.Center;
                }
                router.toast = toast.GetComponentInChildren<TMP_Text>(); router.toastRoot = toast.gameObject; toast.gameObject.SetActive(false);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        static void BuildDetails(RectTransform root)
        {
            var ui = root.gameObject.AddComponent<RedesignUpgradeDetailsScreen>();
            var content = FlowScroll(root, out ui.scroll);
            var identity = Panel(content, "Upgrade", "");
            ui.itemName = FlowText(identity, "detail-upgrade-name", "Phuljhari Signature Breakthrough I", 22, true);
            ui.status = FlowText(identity, "Purchase status", "First purchase · adds 1 to lifetime count", 13);
            ui.effect = FlowText(identity, "Effect", "Phuljhari income ×2");
            var projection = Panel(content, "Projection", "Before → after");
            ui.projection = FlowText(projection, "detail-projection", "Normal shop income: ₹0.85 → ₹1.7/s\nPhuljhari: ₹0.85 → ₹1.7/s\nShop share: 100% → 100%");
            var requirements = Panel(content, "Requirements", "Requirements");
            ui.requirements = FlowText(requirements, "Required", "Own Phuljhari");
            ui.nextRequirement = FlowButton(requirements, "show-requirement", "View next missing requirement"); ui.requirementLabel = ui.nextRequirement.GetComponentInChildren<TMP_Text>();
            var cost = Panel(content, "Price", "Cash price"); ui.price = FlowText(cost, "detail-upgrade-price", "₹115.279");
            ui.exact = FlowButton(cost, "exact-upgrade", "Show full precision"); ui.exactLabel = ui.exact.GetComponentInChildren<TMP_Text>();
            var actions = FlowPair(content, "Purchase and goal", "Pin as goal", "Buy upgrade"); ui.pin = actions[0]; ui.buy = actions[1];
            ui.pinLabel = ui.pin.GetComponentInChildren<TMP_Text>(); ui.buyLabel = ui.buy.GetComponentInChildren<TMP_Text>();
            ui.buy.targetGraphic.color = Teal; ui.buyLabel.color = Cream;
            ui.alternatives = FlowButton(content, "alternatives", "Other useful next steps");
            var goals = Panel(content, "Goal alternatives", "Your next goal"); ui.goals = goals.gameObject.AddComponent<RedesignGoalsScreen>();
            ui.goals.pinnedLabel = FlowText(goals, "Pinned", "No pinned goal. Pin a cash upgrade from its details.");
            var goalActions = FlowPair(goals, "Pinned actions", "View pinned", "Clear goal"); ui.goals.openPinned = goalActions[0]; ui.goals.clearPinned = goalActions[1];
            ui.goals.alternatives = new Button[3]; ui.goals.alternativeLabels = new TMP_Text[3];
            for (int i = 0; i < 3; i++)
            {
                ui.goals.alternatives[i] = FlowButton(goals, "alternative-" + i, "View next step");
                ui.goals.alternatives[i].GetComponent<LayoutElement>().minHeight = ui.goals.alternatives[i].GetComponent<LayoutElement>().preferredHeight = 82 * S;
                ui.goals.alternativeLabels[i] = ui.goals.alternatives[i].GetComponentInChildren<TMP_Text>();
            }
            ui.goals.back = FlowButton(goals, "hide-alternatives", "Hide alternatives"); goals.gameObject.SetActive(false);
            ui.back = FlowButton(content, "back-upgrades", "Back to previous screen"); root.gameObject.SetActive(false);
        }
        static void BuildBatch(RectTransform root)
        {
            var ui = root.gameObject.AddComponent<RedesignBatchScreen>(); ui.rowHeight = 112 * S;
            var frame = Fill("Review", root, 14, 14, 14, 14); Vertical(frame, 0, 8);
            var totals = Panel(frame, "Summary", "Your reviewed purchase list");
            ui.summary = FlowText(totals, "batch-totals", "Scope: Products / Phuljhari\nBudget at review: ₹2K\n3 upgrades · 3 first purchases\nTotal: ₹472.062", 13);
            ui.status = FlowText(frame, "Review status", "Only this list will be bought. Normal sales continue; new earnings do not add entries.", 12);
            var scroller = New("Included upgrades", frame); Height(scroller, 80, 1); ui.scroll = scroller.gameObject.AddComponent<ScrollRect>();
            var viewport = Fill("Viewport", scroller); Image(viewport, new Color(1, 1, 1, .001f), true); viewport.gameObject.AddComponent<RectMask2D>();
            ui.content = Top("Content", viewport, 0, 8 * 112); ui.scroll.viewport = viewport; ui.scroll.content = ui.content;
            ui.scroll.horizontal = false; ui.scroll.vertical = true; ui.scroll.movementType = ScrollRect.MovementType.Clamped; ui.scroll.scrollSensitivity = 45 * S;
            ui.rows = new RedesignBatchRow[8];
            for (int i = 0; i < 8; i++)
            {
                var row = Top("batch-row-" + i, ui.content, i * 112, 104); Image(row, Hex("CC9B53"), false, true);
                var face = Fill("Face", row, 1.5f, 1.5f, 1.5f, 1.5f); Image(face, Hex("FFF4D8"), false, true); Texture(face);
                ui.rows[i] = new RedesignBatchRow { root = row, itemName = Text(Top("Name", row, 10, 44, 12, 12), "1. Phuljhari Signature Breakthrough I", 16, true), price = Text(Top("Price", row, 57, 37, 12, 12), "₹115.279", 13) };
            }
            var options = FlowPair(frame, "Review options", "Full precision", "Review again"); ui.exact = options[0]; ui.refreshReview = options[1]; ui.exactLabel = ui.exact.GetComponentInChildren<TMP_Text>();
            var actions = FlowPair(frame, "Confirm or return", "Cancel", "Buy 3 · ₹472.06"); ui.cancel = actions[0]; ui.confirm = actions[1]; ui.confirmLabel = ui.confirm.GetComponentInChildren<TMP_Text>();
            ui.confirm.targetGraphic.color = Teal; ui.confirmLabel.color = Cream; root.gameObject.SetActive(false);
        }
    }
}
