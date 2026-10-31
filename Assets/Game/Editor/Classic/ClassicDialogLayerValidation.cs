using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RocketRaja.Classic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RocketRaja.Editor
{
    public static class ClassicDialogLayerValidation
    {
        public const string Folder = "Documentation/Implementation/SettingsFix";
        public static string Result { get; private set; }
        static bool running;

        public static string Start()
        {
            if (!EditorApplication.isPlaying || running) throw new InvalidOperationException("Idle Play Mode required");
            var ui = UnityEngine.Object.FindFirstObjectByType<ClassicUI>();
            if (!ui || !ui.driver.Initialized || ui.driver.Session == null || !ui.driver.Session.CanMutate)
                throw new InvalidOperationException("Wait for the production session to finish loading");
            running = true;Result = "RUNNING";Run();return Result;
        }

        static async void Run()
        {
            var checks = new List<string>();
            var ui = UnityEngine.Object.FindFirstObjectByType<ClassicUI>();
            var tutorial = ui ? ui.tutorial : null;
            bool tutorialEnabled = tutorial && tutorial.enabled;
            int width = Screen.width, height = Screen.height;
            try
            {
                if (!ui || !ui.driver.Initialized || !ui.driver.Session.CanMutate) throw new InvalidOperationException("Ready production session required");
                Directory.CreateDirectory(Folder);
                void Check(bool ok, string label)
                {
                    if (!ok) throw new InvalidOperationException("FAIL " + label);
                    checks.Add("PASS " + label);
                }
                if (tutorial) tutorial.enabled = false;
                if (ui.CurrentDialog == "Offline") { ui.Close();await Task.Delay(250); }
                ui.SetTab("Home");ui.Open("Settings");await Task.Delay(400);
                var settings = ui.dialogs.Single(v => v.route == "Settings");
                foreach (var route in new[] { "Naming", "Privacy", "Help", "TutorialHelp", "DeveloperReset" })
                {
                    settings.scroll.verticalNormalizedPosition = .37f;settings.scroll.StopMovement();
                    await Task.Delay(150);
                    Vector2 position = settings.scroll.content.anchoredPosition;
                    var opener = settings.root.GetComponentsInChildren<ClassicControl>(true).Single(c => c.action == "Navigate" && c.parameter == route).GetComponent<Button>();
                    EventSystem.current.SetSelectedGameObject(opener.gameObject);
                    opener.onClick.Invoke();
                    for (int frame = 0; frame < 12; frame++)
                    {
                        Check(settings.root.activeInHierarchy && settings.group.alpha == 1, route + " retains Settings throughout child presentation " + frame);
                        await Task.Delay(30);
                    }
                    var child = ui.dialogs.Single(v => v.route == route);
                    Check(ui.CurrentDialog == route && child.root.activeInHierarchy && child.group.interactable && child.group.blocksRaycasts, route + " is the interactive top layer");
                    Check(child.root.transform.GetSiblingIndex() > settings.root.transform.GetSiblingIndex(), route + " renders above Settings");
                    Check(!settings.group.interactable && !settings.group.blocksRaycasts && !opener.IsInteractable(), route + " blocks background input");
                    Check(!ui.driver.Session.GameplayInputEnabled, route + " blocks gameplay taps");
                    if (route == "Naming") ClassicImportQA.Capture(Folder + "/settings-naming-layered-1080x2400.png");
                    var closeControls = child.root.GetComponentsInChildren<ClassicControl>(true).Where(c => c.action == "Close" && c.name != "Outside dismissal");
                    var back = (closeControls.FirstOrDefault(c => c.name != "Close") ?? closeControls.First()).GetComponent<Button>();
                    back.onClick.Invoke();await Task.Delay(250);
                    Check(ui.CurrentDialog == "Settings" && settings.root.activeInHierarchy && !child.root.activeSelf && settings.group.interactable && settings.group.blocksRaycasts, route + " Back reveals the existing Settings layer");
                    Check(Vector2.Distance(position, settings.scroll.content.anchoredPosition) < .1f, route + " preserves Settings scroll position");
                    Check(EventSystem.current.currentSelectedGameObject == opener.gameObject, route + " restores keyboard focus");
                }
                ui.Open("Help");await Task.Delay(400);ui.Open("Privacy");await Task.Delay(400);
                var help = ui.dialogs.Single(v => v.route == "Help");
                Check(settings.root.activeInHierarchy && help.root.activeInHierarchy && !settings.group.interactable && !help.group.interactable, "Three popup layers retain covered parents without accepting input");
                ui.Open("Settings");await Task.Delay(150);
                Check(ui.CurrentDialog == "Settings" && settings.group.interactable && !help.root.activeSelf && !ui.dialogs.Single(v => v.route == "Privacy").root.activeSelf, "Returning to an existing ancestor removes its descendants");
                ui.Open("Naming");await Task.Delay(350);ui.SetTab("Products");await Task.Delay(200);
                Check(ui.dialogs.All(v => !v.root.activeSelf) && ui.driver.Session.GameplayInputEnabled, "Switching panels closes the full stack and restores gameplay input");
                foreach (var size in new[] { new Vector2Int(360, 540), new Vector2Int(390, 844) })
                {
                    ClassicImportQA.Viewport(size.x, size.y);await Task.Delay(250);
                    ui.Open("Settings");await Task.Delay(350);ui.Open("Naming");await Task.Delay(350);
                    var naming = ui.dialogs.Single(v => v.route == "Naming");
                    Check(settings.root.activeInHierarchy && naming.root.activeInHierarchy && settings.window.rect.height > 220, "Layered Settings works at " + size);
                    ClassicImportQA.Capture(Folder + "/settings-naming-layered-" + size.x + "x" + size.y + ".png");
                    ui.SetTab("Home");
                }
                int buttons = 0;
                foreach (var view in ui.panels.Concat(ui.dialogs))
                    foreach (var button in view.root.GetComponentsInChildren<Button>(true))
                    {
                        var control = button.GetComponent<ClassicControl>();var face = button.targetGraphic as Image;
                        if (!face || !face.sprite || control && (control.action == "Card" || control.action == "Setting" || control.htmlStyle == 3)) continue;
                        Check(AssetDatabase.GetAssetPath(face.sprite).StartsWith("Assets/Game/ImportedUI/Art/"), view.route + "/" + button.name + " uses imported button artwork");buttons++;
                    }
                Check(buttons > 150, "Shared button artwork covers all current panels and popups");
                Result = "PASS " + checks.Count + " checks; " + buttons + " button faces verified";
            }
            catch (Exception e) { Result = e.ToString();Debug.LogException(e); }
            finally
            {
                ClassicImportQA.Viewport(width, height);
                if (ui && ui.driver.Session != null && ui.driver.Session.CanMutate) { ui.SetTab("Home");ui.Open("Settings"); }
                if (tutorial) tutorial.enabled = tutorialEnabled;
                checks.Add(Result);Directory.CreateDirectory(Folder);File.WriteAllLines(Folder + "/dialog-layer-validation.txt", checks);
                running = false;
            }
        }
    }
}
