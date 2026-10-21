using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    public static partial class BuildRedesignUI
    {
        static void BuildSkillDetails(RectTransform root)
        {
            var ui = root.gameObject.AddComponent<RedesignSkillScreen>();
            ui.skills = Segment(Top("Select skill", root, 8, 52, 14, 14), new[] { "Rush", "Hands", "Order" });
            var content = FlowScroll(Fill("Skill body", root, 0, 0, 0, 66), out ui.scroll);
            var effect = Panel(content, "Skill effect", ""); ui.itemName = FlowText(effect, "skill-name", "Festival Rush", 24, true);
            var art = New("Skill artwork", effect); Height(art, 105); ui.artwork = Image(art, Color.white); ui.artwork.preserveAspect = true;
            ui.sprites = new Sprite[3]; string[] files = { "Burst", "Helping Hands", "Bulk Order" };
            for (int i = 0; i < 3; i++) ui.sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/ImportedUI/Art/" + files[i] + ".png");
            ui.effect = FlowText(effect, "skill-effect", "Actual current effect");
            var timing = Panel(content, "Skill timing", "Timing"); ui.timing = FlowText(timing, "skill-timing", "Cooldown starts on activation");
            var status = Panel(content, "Skill availability", "This run"); ui.status = FlowText(status, "skill-status", "Buy this skill again after prestige", 17);
            ui.requirement = FlowText(status, "skill-requirement", "Run requirements", 14);
            ui.activate = FlowButton(content, "skill-activate", "Activate skill", true); ui.activateLabel = ui.activate.GetComponentInChildren<TMP_Text>();
            ui.unlock = FlowButton(content, "skill-unlock", "View base skill in Upgrades");
            ui.upgrades = FlowButton(content, "skill-upgrades", "View power and timing upgrades");
            ui.back = FlowButton(content, "skill-back", "Back"); root.gameObject.SetActive(false);
        }
        static void BuildOffline(RectTransform root)
        {
            var ui = root.gameObject.AddComponent<RedesignOfflineScreen>(); var content = FlowScroll(root, out ui.scroll);
            var earnings = Panel(content, "Return earnings", "Your earnings have been added"); ui.amount = FlowText(earnings, "offline-amount", "+₹0 cash", 19);
            var calculation = Panel(content, "Return calculation", "How this was earned"); ui.calculation = FlowText(calculation, "offline-calculation", "Credited time and efficiency");
            var next = Panel(content, "Return next action", "Your next step"); ui.nextStep = FlowText(next, "offline-next-step", "Review your next purchase");
            ui.skills = FlowText(next, "offline-skill-readiness", "Inspect skills", 14);
            ui.next = FlowButton(content, "offline-next", "Browse cash upgrades", true); ui.nextLabel = ui.next.GetComponentInChildren<TMP_Text>();
            ui.inspectSkills = FlowButton(content, "offline-skills", "Review skills");
            ui.close = FlowButton(content, "offline-close", "Continue to Upgrades");
            ui.status = FlowText(content, "offline-status", "Closing keeps all credited cash", 13); root.gameObject.SetActive(false);
        }
        public static void AddReturnAndSkills()
        {
            if (EditorApplication.isPlaying || EditorApplication.isCompiling) throw new InvalidOperationException("Require compiled Edit Mode");
            LoadAssets(); var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var pages = root.transform.Find("Safe Area/Screen States");
                if (pages.Find("screen-offline") || pages.Find("screen-skilldetails")) throw new InvalidOperationException("Return/skills already authored; use a scoped revision");
                var skills = Fill("screen-skilldetails", pages); BuildSkillDetails(skills); var offline = Fill("screen-offline", pages); BuildOffline(offline);
                var router = root.GetComponent<RedesignScreenRouter>(); router.skillDetails = skills.GetComponent<RedesignSkillScreen>(); router.offline = offline.GetComponent<RedesignOfflineScreen>();
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            SerializeTextSlots();
        }
    }
}
