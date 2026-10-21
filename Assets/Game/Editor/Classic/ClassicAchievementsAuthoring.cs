using System;
using System.Collections.Generic;
using System.Linq;
using RocketRaja.Classic;
using RocketRaja.Cloud;
using RocketRaja.Core;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RocketRaja.Editor
{
    public static partial class BuildClassicUI
    {
        [MenuItem("Rocket Raja/Classic/Add achievements and fix loading spinner")]
        public static void AddAchievementsAndFixSpinner()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit Mode required");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            var main = SceneManager.GetSceneByPath(ScenePath);
            if (!main.isLoaded) main = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            ui = main.GetRootGameObjects().Single(g => g.name == RootName).GetComponent<ClassicUI>();
            body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local + "Fonts/Body SDF.asset");
            bold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local + "Fonts/Bold SDF.asset");
            serif = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Local + "Fonts/Card Serif SDF.asset");
            heading = serif; blue = ui.blueButton; ivory = ui.ivoryButton;
            rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            row = AssetDatabase.LoadAssetAtPath<Sprite>(SyncArt + "skills-list-row.png");
            texts.Clear(); views.Clear();
            Undo.RegisterFullObjectHierarchyUndo(ui.gameObject, "Add achievements");
            AuthorAchievements();
            EditorUtility.SetDirty(ui);
            EditorSceneManager.MarkSceneDirty(main); EditorSceneManager.SaveScene(main);

            var bootstrap = SceneManager.GetSceneByPath("Assets/Scenes/Bootstrap.unity");
            bool opened = !bootstrap.isLoaded;
            if (opened) bootstrap = EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity", OpenSceneMode.Additive);
            foreach (var rect in bootstrap.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<RectTransform>(true)).Where(t => t.name == "Spinner"))
            {
                Undo.RecordObject(rect, "Center loading spinner pivot");
                BootstrapController.CenterSpinnerPivot(rect);
                rect.localRotation = Quaternion.identity;
                EditorUtility.SetDirty(rect);
            }
            EditorSceneManager.MarkSceneDirty(bootstrap); EditorSceneManager.SaveScene(bootstrap);
            if (opened) EditorSceneManager.CloseScene(bootstrap, true);
            if (!setup.Any(s => s.path == ScenePath)) EditorSceneManager.CloseScene(main, true);
            Debug.Log("Achievements authored: 15 permanent Kaju Katli rewards; bootstrap spinner pivots centered.");
        }

        static void AuthorAchievements()
        {
            var toolbar = ui.panels.Single(v => v.route == "Player").scroll.content.Find("Toolbar");
            var tab = toolbar.Find("Achievements") as RectTransform;
            if (!tab)
            {
                tab = Region("Achievements", toolbar, .38f, 0, .68f, 1);
                var button = Button(tab, "Navigate", "Achievements", "", "Achievements", ivory, 9);
                ((Image)button.targetGraphic).sprite = null; ((Image)button.targetGraphic).color = Color.clear;
                button.transition = Selectable.Transition.None;
            }
            AchievementToolbarBox((RectTransform)toolbar.Find("Stats"), 0, .18f);
            AchievementToolbarBox((RectTransform)toolbar.Find("Skills"), .19f, .37f);
            AchievementToolbarBox(tab, .38f, .68f);
            AchievementToolbarBox((RectTransform)toolbar.Find("Quantity"), .70f, 1);
            ApplyPlayerToolbarIcons(toolbar);
            ui.achievementTabLabel = tab.GetComponentInChildren<TMP_Text>(true);
            ui.achievementTabLabel.fontSize = ui.achievementTabLabel.fontSizeMax = 9;
            ui.achievementTabLabel.enableAutoSizing = true; ui.achievementTabLabel.fontSizeMin = 8;
            var existing = ui.dialogs.FirstOrDefault(v => v.route == "Achievements");
            if (existing != null) { ConfigureAchievementClose(existing); return; }

            var source = ui.dialogs.Single(v => v.route == "Statistics");
            var view = Dialog(source.root.transform.parent, "Achievements", "Your achievements", out var content);
            StatisticsSkin(view.window.GetComponent<Image>(), AssetDatabase.LoadAssetAtPath<Sprite>(SyncArt + "skills-list-panel.png"));
            var sourceFrame = source.window.Find("Painted gold and cobalt frame");
            if (sourceFrame) { var frame = Object.Instantiate(sourceFrame.gameObject, view.window, false); frame.name = sourceFrame.name; frame.transform.SetAsFirstSibling(); }
            var title = view.window.Find("Header/Title").GetComponent<TMP_Text>();
            title.font = serif; title.fontSize = title.fontSizeMax = 23; title.fontSizeMin = 17; title.enableAutoSizing = true;
            ConfigureAchievementClose(view);
            view.root.transform.Find("Outside dismissal").GetComponent<Button>().interactable = true;
            var summary = New("Progress and rewards", content); Height(summary, 58);
            ui.achievementSummary = Label("", Fill("Summary", summary, 4), "0 / 15 claimed\nGrow your stall. Collect Kaju Katli.", 11, true, Ink, TextAlignmentOptions.Center);
            var rows = new List<ClassicAchievementRow>();
            foreach (var definition in RedesignAchievements.All)
            {
                var card = Row(definition.Id, content, 116); StatisticsSkin(card.GetComponent<Image>(), row);
                var name = Label("", Region("Title", card, 0, .72f, 1, 1), definition.Title, 14, true);
                name.rectTransform.offsetMin = new Vector2(12, 0); name.rectTransform.offsetMax = new Vector2(-12, -4);
                var description = Label("", Region("Description", card, 0, .51f, 1, .74f), definition.Description, 10, false, Muted);
                description.rectTransform.offsetMin = new Vector2(12, 0); description.rectTransform.offsetMax = new Vector2(-12, 0);
                var progress = Label("", Region("Progress", card, .04f, .30f, .60f, .49f), "0 / " + definition.Target, 10, true);
                var track = Region("Progress track", card, .04f, .24f, .60f, .28f);
                Face(track, new Color32(224, 216, 194, 255), rounded);
                var fill = Face(Fill("Fill", track), new Color32(203, 151, 58, 255), rounded);
                ConfigureProgressFill(fill); fill.fillAmount = 0;
                var reward = Label("", Region("Reward", card, .04f, .035f, .62f, .23f), "+" + definition.Reward + " Kaju Katli", 11, true);
                var claim = Button(Region("Claim reward", card, .66f, .075f, .96f, .49f), "ClaimAchievement", definition.Id, "", "In progress", blue, 11);
                claim.GetComponent<ClassicControl>().htmlStyle = 1;
                claim.interactable = false;
                var action = claim.GetComponentInChildren<TMP_Text>(true); action.enableAutoSizing = true; action.fontSizeMin = 8;
                rows.Add(new ClassicAchievementRow { id = definition.Id, progress = progress, reward = reward, claim = claim, action = action, fill = fill });
            }
            var note = New("Permanent rewards note", content); Height(note, 40);
            Label("", Fill("Copy", note, 4), "Achievements and collected rewards stay with your save, even after prestige.", 10, false, Muted, TextAlignmentOptions.Center);
            Footer(view, 0, "Close", "", "", "Back to your stall", true);
            ui.achievementRows = rows.ToArray(); ui.dialogs = ui.dialogs.Concat(new[] { view }).ToArray();
            ClassicPopupClearanceAuthoring.ApplyView(view);
            view.root.SetActive(false);
            Canvas.ForceUpdateCanvases(); ui.GetComponent<ClassicLayout>().Apply();
        }

        static void AchievementToolbarBox(RectTransform rect, float left, float right)
        { rect.anchorMin = new Vector2(left, 0); rect.anchorMax = new Vector2(right, 1); rect.offsetMin = rect.offsetMax = Vector2.zero; }

        static void ConfigureAchievementClose(ClassicView view)
        {
            var close = view.window.Find("Header/Close").GetComponentInChildren<TMP_Text>(true);
            close.fontSize = close.fontSizeMax = 22; close.fontSizeMin = 18;
            close.enableAutoSizing = true; close.textWrappingMode = TextWrappingModes.NoWrap;
            close.overflowMode = TextOverflowModes.Overflow;
            EditorUtility.SetDirty(close);
        }
    }
}
