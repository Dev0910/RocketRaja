#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RocketRaja
{
    public sealed partial class SmokeHarness
    {
#if UNITY_EDITOR
        int previousGameViewSize = -1;
        UnityEditor.EditorWindow reviewGameView;
        static readonly BindingFlags ViewFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;
#endif
        void ResizeForReview(int width, int height)
        {
#if UNITY_EDITOR
            // Screen.SetResolution does not resize the Editor Game view. Exercise a real fixed viewport.
            var assembly = typeof(UnityEditor.Editor).Assembly;
            var sizesType = assembly.GetType("UnityEditor.GameViewSizes");
            var singleton = typeof(UnityEditor.ScriptableSingleton<>).MakeGenericType(sizesType);
            var sizes = singleton.GetProperty("instance", ViewFlags).GetValue(null);
            var group = sizesType.GetProperty("currentGroup", ViewFlags).GetValue(sizes);
            var groupType = group.GetType();
            int count = (int)groupType.GetMethod("GetTotalCount", ViewFlags).Invoke(group, null), index = -1;
            var sizeType = assembly.GetType("UnityEditor.GameViewSize");
            for (int i = 0; i < count; i++)
            {
                var size = groupType.GetMethod("GetGameViewSize", ViewFlags).Invoke(group, new object[] { i });
                if ((int)sizeType.GetProperty("width", ViewFlags).GetValue(size) == width && (int)sizeType.GetProperty("height", ViewFlags).GetValue(size) == height) { index = i; break; }
            }
            if (index < 0)
            {
                var kind = Enum.Parse(assembly.GetType("UnityEditor.GameViewSizeType"), "FixedResolution");
                var size = Activator.CreateInstance(sizeType, new object[] { kind, width, height, "HTML Motion Review " + width + "x" + height });
                groupType.GetMethod("AddCustomSize", ViewFlags).Invoke(group, new[] { size }); index = count;
            }
            var viewType = assembly.GetType("UnityEditor.GameView");
            reviewGameView = Resources.FindObjectsOfTypeAll(viewType).FirstOrDefault() as UnityEditor.EditorWindow;
            if (reviewGameView == null) reviewGameView = UnityEditor.EditorWindow.GetWindow(viewType);
            var selection = viewType.GetProperty("selectedSizeIndex", ViewFlags);
            if (previousGameViewSize < 0) previousGameViewSize = (int)selection.GetValue(reviewGameView);
            selection.SetValue(reviewGameView, index); reviewGameView.Repaint();
#else
            Screen.SetResolution(width, height, FullScreenMode.Windowed);
#endif
        }
#if UNITY_EDITOR
        void RestoreReviewViewport()
        {
            if (previousGameViewSize < 0 || reviewGameView == null) return;
            reviewGameView.GetType().GetProperty("selectedSizeIndex", ViewFlags).SetValue(reviewGameView, previousGameViewSize);
            previousGameViewSize = -1; reviewGameView.Repaint();
        }
#endif
    }
}
#endif
