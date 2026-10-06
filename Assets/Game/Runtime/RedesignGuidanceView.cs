using System;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    [Serializable] public sealed class RedesignGuidanceText { public string id, title, brief, body; }

    /// <summary>Optional saved help strip and reader. Opening and closing help never executes gameplay.</summary>
    public sealed class RedesignGuidanceView : MonoBehaviour
    {
        public RedesignScreenRouter owner;
        public RectTransform screenBody;
        public GameObject strip, reader;
        public Button read, gotIt, skip;
        public TMP_Text title, brief, heading, copy;
        public RedesignGuidanceText[] topics;
        public float topWithoutTip, reservedHeight;
        RedesignUiContext context;
        string route, argument, current, suppressedRoute, suppressedArgument, suppressedCommand;
        public string CurrentTopic => current;
        public bool ReaderOpen => reader && reader.activeSelf;
        void Awake()
        {
            if (!owner || !screenBody || !strip || !reader || !read || !gotIt || !skip || !title || !brief || !heading || !copy || topics == null || topics.Length != RedesignGuidance.Topics.Count)
                throw new InvalidOperationException("Incomplete serialized contextual guidance");
            read.onClick.AddListener(OpenReader); gotIt.onClick.AddListener(Acknowledge); skip.onClick.AddListener(Acknowledge);
        }
        void OnDestroy() { if (read) read.onClick.RemoveListener(OpenReader); if (gotIt) gotIt.onClick.RemoveListener(Acknowledge); if (skip) skip.onClick.RemoveListener(Acknowledge); }
        public void Bind(RedesignUiContext value) { context = value; current = null; suppressedCommand = null; CloseReader(); }
        public void Refresh(string destination, string parameter, bool navigation = false)
        {
            bool changed = navigation || route != destination || argument != parameter;
            route = destination; argument = parameter;
            if (changed) { CloseReader(); suppressedCommand = null; }
            string next = context != null && context.CanMutate() ? RedesignGuidance.Next(context.Engine, route, argument) : null;
            if (next != null && suppressedRoute == route && suppressedArgument == argument && suppressedCommand == context.Engine.State.commandRevision.digits) next = null;
            if (next != current)
            {
                CloseReader(); current = next;
                if (next != null)
                {
                    RedesignGuidanceText text = null;
                    for (int i = 0; i < topics.Length; i++) if (topics[i].id == next) { text = topics[i]; break; }
                    if (text == null) throw new InvalidOperationException("Missing saved guidance text: " + next);
                    title.text = heading.text = text.title; brief.text = text.brief; copy.text = text.body;
                }
            }
            bool visible = current != null;
            if (strip.activeSelf != visible) strip.SetActive(visible);
            var inset = screenBody.offsetMax; float top = -(topWithoutTip + (visible ? reservedHeight : 0));
            if (inset.y != top) screenBody.offsetMax = new Vector2(inset.x, top);
        }
        public void OpenReader()
        {
            if (context == null || !context.CanMutate() || current == null) return;
            owner.DismissNotice(); reader.SetActive(true);
        }
        public void CloseReader() { if (reader) reader.SetActive(false); }
        void Acknowledge()
        {
            if (context == null || !context.CanMutate() || !ReaderOpen || current == null) return;
            string id = current;
            if (!context.Execute(e => e.LearnTopic(id))) { owner.Notice("Tip could not be saved. Try again after saving recovers."); Refresh(route, argument); return; }
            suppressedRoute = route; suppressedArgument = argument; suppressedCommand = context.Engine.State.commandRevision.digits;
            CloseReader(); Refresh(route, argument);
        }
    }
}
