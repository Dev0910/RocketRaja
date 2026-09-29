using System.IO;
using UnityEditor.Android;

// EDM4U can regenerate unescaped file URLs when the project path contains spaces.
public sealed class FirebaseRepositoryUriFix : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => int.MaxValue;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        var settingsPath = Path.Combine(path, "settings.gradle");
        if (!File.Exists(settingsPath))
            settingsPath = Path.Combine(Path.GetDirectoryName(path), "settings.gradle");
        if (!File.Exists(settingsPath))
            return;

        var original = File.ReadAllText(settingsPath);
        const string declaration = "def unityProjectPath = ";
        var lines = original.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i].TrimStart().StartsWith(declaration) &&
                lines[i].Contains("file:///") &&
                !lines[i].Contains(".replace(\" \", \"%20\")"))
            {
                lines[i] = lines[i].TrimEnd('\r') + ".replace(\" \", \"%20\")";
            }
        }

        var updated = string.Join("\n", lines);
        if (updated != original)
            File.WriteAllText(settingsPath, updated);
    }
}
