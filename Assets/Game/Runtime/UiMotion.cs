using System;
using System.Collections;
using UnityEngine;
namespace RocketRaja
{
    /// <summary>HTML transitions on saved motion hosts. Never creates runtime UI.</summary>
    [DisallowMultipleComponent]
    public sealed class UiMotion : MonoBehaviour
    {
        public enum Kind { Panel, Modal, Quantity, Notice }
        [SerializeField] Kind kind;
        [SerializeField] CanvasGroup group;
        [SerializeField] RectTransform surface;
        GameController owner;
        Coroutine transition;
        Vector3 restScale;
        Vector2 restPosition;
        bool initialized, closing, modal;
        Action completion;
        int screenWidth, screenHeight;
        public bool IsClosing => closing;
        public Kind Profile => kind;
        public RectTransform Surface => surface;
        static UiMotion Get(GameObject root, GameController game)
        {
            if (!root.TryGetComponent<UiMotion>(out var m) || m.group == null || m.surface == null)
            { Debug.LogError("Missing Editor-authored motion references on " + root.name, root); return null; }
            m.owner = game;
            if (!m.initialized) { m.restScale = m.surface.localScale; m.restPosition = m.surface.anchoredPosition; m.initialized = true; UiFeedback.Wire(root, game); }
            return m;
        }
        bool Reduced => owner == null || owner.Engine == null || owner.Engine.State.reducedMotion;
        public static void Show(GameObject root, GameController game, bool isModal = false, int direction = 0, bool replay = false)
        {
            if (root == null) return;
            var m = Get(root, game); if (m == null) return;
            bool wasActive = root.activeSelf;
            bool visible = !replay && wasActive && !m.closing && m.transition == null;
            m.Cancel(); m.Restore(); m.closing = false; m.modal = isModal;
            root.SetActive(true); if (isModal) game?.HoldModal(m);
            if (isModal && !wasActive)
                foreach (var child in root.GetComponentsInChildren<UiMotion>())
                    if (child != m && child.kind == Kind.Notice) Show(child.gameObject, game, false, 0, true);
            m.group.blocksRaycasts = m.kind != Kind.Notice;
            if (visible || m.Reduced) { m.group.interactable = true; return; }
            m.transition = m.StartCoroutine(m.Animate(true, direction));
        }
        public static void Hide(GameObject root, GameController game, Action completed = null, int direction = 0)
        {
            if (root == null) { completed?.Invoke(); return; }
            var m = Get(root, game); if (m == null || m.closing) return;
            m.Cancel(); m.Restore(); m.closing = true; m.completion = completed;
            if (!root.activeInHierarchy || m.Reduced || m.kind == Kind.Quantity || m.kind == Kind.Notice) { m.Finish(false); return; }
            m.transition = m.StartCoroutine(m.Animate(false, direction));
        }
        public static void SetVisible(GameObject root, bool visible, GameController game, int direction = 0)
        {
            if (root == null) return;
            if (visible) { var m = root.GetComponent<UiMotion>(); if (!root.activeSelf || (m != null && m.closing)) Show(root, game, false, direction); }
            else if (root.activeSelf) Hide(root, game, null, direction);
        }
        public static void Toggle(GameObject root, GameController game)
        { var m = root.GetComponent<UiMotion>(); SetVisible(root, !root.activeSelf || (m != null && m.closing), game); }
        public static void SettleExit(GameObject root)
        { var m = root.GetComponent<UiMotion>(); if (m != null && m.closing) m.Settle(); }
        public float Duration(bool opening) => opening ? kind == Kind.Modal ? .25f : kind == Kind.Quantity ? .17f : kind == Kind.Notice ? .18f : .23f : .16f;
        IEnumerator Animate(bool opening, int direction)
        {
            group.interactable = false; group.blocksRaycasts = kind != Kind.Notice && (opening || modal);
            screenWidth = Screen.width; screenHeight = Screen.height;
            float start = Time.unscaledTime;
            while (Time.unscaledTime - start < Duration(opening) && !Reduced)
            { Sample(opening, Time.unscaledTime - start, direction); yield return null; }
            Finish(opening);
        }
        // Editor trajectory review uses the same sampler as the live animation.
        public void Sample(bool opening, float elapsed, int direction = 0)
        {
            if (!initialized) { restScale = surface.localScale; restPosition = surface.anchoredPosition; initialized = true; }
            float t = Mathf.Clamp01(elapsed / Duration(opening));
            float e = kind == Kind.Notice ? UiEasing.EaseOut(t) : UiEasing.Standard(t);
            Vector2 offset = Vector2.zero; Vector3 scale = Vector3.one;
            float alphaT = kind == Kind.Modal && opening ? UiEasing.Standard(Mathf.Clamp01(elapsed / .19f)) : e;
            group.alpha = opening ? alphaT : 1 - alphaT;
            if (kind == Kind.Modal) { offset.y = -(opening ? 16 * (1 - e) : 10 * e); scale *= opening ? Mathf.Lerp(.98f, 1, e) : Mathf.Lerp(1, .985f, e); }
            else if (kind == Kind.Quantity) { offset.y = -7 * (1 - e); scale.y = Mathf.Lerp(.97f, 1, e); }
            else if (kind == Kind.Notice) offset.y = -5 * (1 - e);
            else if (direction == 0) offset.y = -(opening ? 16 * (1 - e) : 14 * e);
            else offset.x = opening ? direction * 18 * (1 - e) : -direction * 14 * e;
            surface.localScale = Vector3.Scale(restScale, scale);
            surface.anchoredPosition = restPosition + offset * UiEasing.Pixels(surface);
        }
        void LateUpdate() { if (transition != null && (Screen.width != screenWidth || Screen.height != screenHeight)) Settle(); }
        public void Settle() { if (transition == null) return; bool opening = !closing; StopCoroutine(transition); transition = null; Finish(opening); }
        void OnApplicationFocus(bool focus) { if (!focus) Settle(); }
        void OnApplicationPause(bool pause) { if (pause) Settle(); }
        void Finish(bool opening) { var done = completion; completion = null; transition = null; Restore(); group.interactable = true; if (!opening) { gameObject.SetActive(false); done?.Invoke(); } }
        void Restore() { if (!initialized) return; surface.localScale = restScale; surface.anchoredPosition = restPosition; group.alpha = 1; }
        void Cancel() { if (transition != null) StopCoroutine(transition); transition = null; completion = null; }
        void OnDisable() { Cancel(); Restore(); if (group != null) { group.interactable = true; group.blocksRaycasts = kind != Kind.Notice; } if (modal) owner?.ReleaseModal(this); modal = closing = false; }
    }
}
