using System;
using System.Collections.Generic;
using System.IO;
using RocketRaja.Core;
using UnityEditor;
using UnityEngine;

namespace RocketRaja.Editor
{
    public static class RedesignGenerationValidation
    {
        [MenuItem("Rocket Raja/Redesign/Validate Content Regeneration")]
        public static void Run()
        {
            var checks = new List<string>();
            void Check(bool value, string name) { if (!value) throw new InvalidOperationException(name); checks.Add("PASS " + name); }
            string assetPath = "Assets/RedesignGenerationQA-" + Guid.NewGuid().ToString("N") + ".asset";
            string otherPath = "Assets/RedesignGenerationQA-" + Guid.NewGuid().ToString("N") + ".asset";
            try
            {
                var source = RedesignAuthoring.Import();
                var created = RedesignAuthoring.UpsertAsset(assetPath, source);
                Check(created.redesign.upgrades.Length == 1000 && created.redesign.cards.Length == 28, "Missing asset regenerates all redesign content from source");
                Check(created.catalog == null || created.catalog.legacy == null && created.catalog.upgrades == null, "New content asset never populates the retired Legacy or 96-entry catalogue");
                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                string bytes = SavePayload.Digest(File.ReadAllText(assetPath));
                var writtenAt = File.GetLastWriteTimeUtc(assetPath);
                RedesignAuthoring.UpsertAsset(assetPath, RedesignAuthoring.Import());
                Check(AssetDatabase.AssetPathToGUID(assetPath) == guid && SavePayload.Digest(File.ReadAllText(assetPath)) == bytes && File.GetLastWriteTimeUtc(assetPath) == writtenAt,
                    "Repeated regeneration preserves the asset GUID and byte-identical unmodified file");
                var invalid = JsonUtility.FromJson<RedesignContent>(JsonUtility.ToJson(source)); invalid.contentVersion = "future";
                bool rejected = false; try { RedesignAuthoring.UpsertAsset(assetPath, invalid); } catch { rejected = true; }
                Check(rejected && SavePayload.Digest(File.ReadAllText(assetPath)) == bytes, "Invalid authored content cannot partially replace an existing asset");
                var changed = JsonUtility.FromJson<RedesignContent>(JsonUtility.ToJson(source)); changed.upgrades[0].name += " outdated";
                rejected = false; try { RedesignAuthoring.RequireSourceMatch(changed, source); } catch (InvalidOperationException) { rejected = true; }
                Check(rejected, "Build validation rejects source drift even when counts and version strings still match");
                var wrong = ScriptableObject.CreateInstance<ProductionConfig>(); AssetDatabase.CreateAsset(wrong, otherPath);
                bytes = SavePayload.Digest(File.ReadAllText(otherPath));
                rejected = false; try { RedesignAuthoring.UpsertAsset(otherPath, source); } catch (InvalidOperationException) { rejected = true; }
                Check(rejected && SavePayload.Digest(File.ReadAllText(otherPath)) == bytes, "An incompatible asset at the target path is preserved for inspection");
                var main = AssetDatabase.LoadAssetAtPath<GameContent>("Assets/Resources/GameContent.asset");
                string priorLegacy = JsonUtility.ToJson(main.catalog);
                string scene = File.ReadAllText("Assets/Scenes/Main.unity");
                string legacyExport = File.ReadAllText("Documentation/ContentCatalog.json");
                main = BuildGame.PrepareContent();
                Check(JsonUtility.ToJson(main.catalog) == priorLegacy && File.ReadAllText("Assets/Scenes/Main.unity") == scene && File.ReadAllText("Documentation/ContentCatalog.json") == legacyExport,
                    "Prepare content updates the selected economy without regenerating legacy data or changing existing scenes");
                string export = File.ReadAllText("Documentation/RedesignContentCatalog.json");
                string manifest = File.ReadAllText("Assets/Resources/RedesignContentManifest.json");
                BuildGame.PrepareContent();
                Check(File.ReadAllText("Documentation/RedesignContentCatalog.json") == export && File.ReadAllText("Assets/Resources/RedesignContentManifest.json") == manifest,
                    "Repeated authoring keeps simulator export and runtime manifest identical");
            }
            finally
            {
                // Only the two uniquely named test assets are eligible for cleanup.
                foreach (string path in new[] { assetPath, otherPath })
                {
                    string resolved = Path.GetFullPath(path);
                    if (!resolved.StartsWith(Path.GetFullPath(Application.dataPath) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                        !Path.GetFileName(path).StartsWith("RedesignGenerationQA-", StringComparison.Ordinal)) throw new InvalidOperationException("Unexpected validation cleanup path");
                    if (File.Exists(path)) AssetDatabase.DeleteAsset(path);
                }
            }
            Directory.CreateDirectory("Documentation/Implementation");
            File.WriteAllLines("Documentation/Implementation/Unity-Generation.txt", checks);
            Debug.Log("REDESIGN_GENERATION_OK " + checks.Count);
        }
    }
}
