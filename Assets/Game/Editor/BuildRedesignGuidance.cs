using System;
using System.IO;
using System.Linq;
using RocketRaja.Core;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace RocketRaja.Editor
{
    public static partial class BuildRedesignUI
    {
        [Serializable] sealed class GuidanceManifest { public RedesignGuidanceText[] topics; }
        public static void AddGuidance()
        {
            if (EditorApplication.isPlaying || EditorApplication.isCompiling) throw new InvalidOperationException("Require compiled Edit Mode");
            var manifest = JsonUtility.FromJson<GuidanceManifest>(File.ReadAllText("UI Builder/HTML/Redesign/guidance-topics.json"));
            if (manifest?.topics == null || !manifest.topics.Select(t => t.id).SequenceEqual(RedesignGuidance.Topics) || manifest.topics.Any(t => string.IsNullOrEmpty(t.body) || string.IsNullOrEmpty(t.brief) || string.IsNullOrEmpty(t.title))) throw new InvalidDataException("Guidance manifest must match known topics");
            LoadAssets(); var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var router = root.GetComponent<RedesignScreenRouter>(); var safe = root.transform.Find("Safe Area");
                var guide = router.guidance;
                if (!guide)
                {
                    guide = root.AddComponent<RedesignGuidanceView>(); router.guidance = guide;
                    guide.owner = router; guide.screenBody = (RectTransform)safe.Find("Screen States");
                    var strip = Top("Contextual tip", safe, 94, 52, 14, 14); guide.strip = strip.gameObject;
                    Image(strip, Gold, true, true); var stripFace = Fill("Face", strip, 1, 1, 1, 1); Image(stripFace, Cream, false, true); Texture(stripFace);
                    guide.title = Text(Top("Topic", strip, 5, 19, 8, 102), "Your first purchase", 13, true);
                    guide.brief = Text(Top("Brief", strip, 25, 24, 8, 102), "Buy one Phuljhari yourself.", 11);
                    var read = Fill("Read tip", strip); read.anchorMin = new Vector2(1, 0); read.offsetMin = new Vector2(-95 * S, 0); read.offsetMax = Vector2.zero; guide.read = Button(read, "Read tip");
                    var reader = New("Tip reader", safe); reader.anchorMin = Vector2.zero; reader.anchorMax = Vector2.right; reader.offsetMin = new Vector2(14, 80) * S; reader.offsetMax = new Vector2(-14, 310) * S; guide.reader = reader.gameObject;
                    Image(reader, Gold, true, true); var readerFace = Fill("Face", reader, 1.5f, 1.5f, 1.5f, 1.5f); Image(readerFace, Cream, false, true); Texture(readerFace);
                    guide.heading = Text(Top("Title", reader, 12, 26, 12, 12), "Your first purchase", 18, true);
                    guide.copy = Text(Top("Copy", reader, 46, 116, 12, 12), "", 13);
                    var actions = Fill("Actions", reader, 12, 12); actions.anchorMax = Vector2.right; actions.offsetMin = new Vector2(12, 12) * S; actions.offsetMax = new Vector2(-12, 64) * S;
                    var buttons = Segment(actions, new[] { "Got it", "Skip tip" }); guide.gotIt = buttons[0]; guide.skip = buttons[1];
                }
                guide.topics = manifest.topics; guide.topWithoutTip = 90 * S; guide.reservedHeight = 60 * S;
                guide.screenBody.offsetMax = new Vector2(guide.screenBody.offsetMax.x, -guide.topWithoutTip);
                guide.strip.SetActive(false); guide.reader.SetActive(false);
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            Debug.Log("REDESIGN_GUIDANCE_SAVED");
        }
    }
}
