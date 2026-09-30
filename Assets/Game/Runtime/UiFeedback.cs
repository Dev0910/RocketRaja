using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace RocketRaja
{
    /// <summary>HTML control transitions on saved targets. One channel per animated property.</summary>
    [DisallowMultipleComponent]
    public sealed class UiFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
        IPointerEnterHandler, IPointerExitHandler, ISubmitHandler
    {
        public enum Kind { Button, Skill, Navigation, Accent }
        [SerializeField] Kind kind;
        [SerializeField] RectTransform target;
        [SerializeField] RectTransform navigationIcon;
        [SerializeField] Graphic face;
        [SerializeField] Sprite disabledFace;
        GameController owner;
        Button button;
        Color faceColor;
        Sprite normalOverride;
        TMP_Text[] labels;
        Color[] labelColors;
        bool availabilityApplied, wasInteractable;
        Vector3 baseline, iconBaseline;
        Vector2 position;
        float fromScale = 1, toScale = 1, scale = 1, fromY, toY, y, elapsed = 1;
        float selectedFrom, selectedTo, selectedValue, selectedAge = 1;
        float brightness = 1, brightnessFrom = 1, brightnessTo = 1, brightnessAge = 1;
        float pulseAge, pulseDuration, pulseStrength, submitClock;
        bool initialized, pressed, hovered, reduced;
        public Kind Profile => kind;
        public RectTransform Target => target;
        bool Reduced => owner == null || owner.Engine == null || owner.Engine.State.reducedMotion;
        bool CanPress => button != null && button.IsActive() && button.IsInteractable() && !Reduced;
        float PressDuration => kind == Kind.Skill ? .14f : .12f;

        void Initialize()
        {
            if (initialized) return;
            if (target == null) target = transform as RectTransform;
            baseline = target.localScale; position = target.anchoredPosition;
            if (navigationIcon != null) iconBaseline = navigationIcon.localScale;
            button = GetComponent<Button>();
            if (face != null) faceColor = face.color;
            if (disabledFace != null && face is Image image)
            {
                normalOverride = image.overrideSprite;
                labels = GetComponentsInChildren<TMP_Text>(true);
                labelColors = new Color[labels.Length];
                for (int i = 0; i < labels.Length; i++) labelColors[i] = labels[i].color;
            }
            initialized = true;
        }
        static UiFeedback Get(Transform target, GameController game)
        {
            if (target == null) return null;
            if (!target.TryGetComponent<UiFeedback>(out var feedback))
            { Debug.LogError("Missing Editor-authored feedback on " + target.name, target); return null; }
            feedback.owner = game; feedback.Initialize(); return feedback;
        }
        public static void Wire(GameObject root, GameController game)
        { foreach (var feedback in root.GetComponentsInChildren<UiFeedback>(true)) { feedback.owner = game; feedback.Initialize(); } }
        /// <summary>Records an intentional presenter change to a control's normal artwork and text colors.</summary>
        public static void SyncAppearance(Button control)
        {
            if (control == null || !control.TryGetComponent<UiFeedback>(out var feedback)) return;
            feedback.Initialize();
            if (!(feedback.face is Image image)) return;
            // The presenter owns Image.sprite; overrideSprite may still contain our older
            // normal or disabled appearance. Cache the newly assigned base artwork.
            feedback.normalOverride = image.sprite;
            if (feedback.labels == null) feedback.labels = feedback.GetComponentsInChildren<TMP_Text>(true);
            if (feedback.labelColors == null || feedback.labelColors.Length != feedback.labels.Length)
                feedback.labelColors = new Color[feedback.labels.Length];
            for (int i = 0; i < feedback.labels.Length; i++)
                if (feedback.labels[i] != null) feedback.labelColors[i] = feedback.labels[i].color;
            feedback.availabilityApplied = false;
            if (feedback.disabledFace == null) image.overrideSprite = feedback.normalOverride;
            else feedback.ApplyAvailability();
        }
        public static void Pulse(Transform target, GameController game, float amount = .09f, float seconds = .38f)
        {
            var feedback = Get(target, game);
            if (feedback == null || feedback.Reduced) return;
            feedback.enabled = true; feedback.pulseAge = 0; feedback.pulseDuration = seconds; feedback.pulseStrength = amount;
        }
        public static void Select(Transform target, GameController game, bool selected)
        {
            var feedback = Get(target, game); if (feedback == null) return;
            feedback.selectedFrom = feedback.selectedValue; feedback.selectedTo = selected ? 1 : 0;
            feedback.selectedAge = 0; feedback.enabled = true;
        }
        void State(bool down, bool over)
        {
            Initialize(); pressed = down && CanPress; hovered = over && CanPress;
            fromScale = scale; fromY = y; elapsed = 0;
            toScale = pressed ? kind == Kind.Skill ? .93f : kind == Kind.Navigation ? 1 : .98f : 1;
            toY = kind == Kind.Skill ? (!pressed && hovered ? 2 : 0) : pressed ? -1 : 0;
            brightnessFrom = brightness;
            brightnessTo = kind == Kind.Navigation ? pressed ? 1.15f : hovered ? 1.1f : 1 :
                kind == Kind.Button ? pressed ? .95f : hovered ? 1.09f : 1 : 1;
            brightnessAge = 0; enabled = true;
        }
        public void OnPointerDown(PointerEventData data) { if (data.button == PointerEventData.InputButton.Left) State(true, hovered); }
        public void OnPointerUp(PointerEventData data) { State(false, hovered); }
        public void OnPointerEnter(PointerEventData data) { State(pressed, data.pointerId < 0); }
        public void OnPointerExit(PointerEventData data) { State(false, false); }
        public void OnSubmit(BaseEventData data) { if (CanPress) { State(true, false); submitClock = PressDuration; } }
        void Update()
        {
            if (!initialized) return;
            if (Reduced)
            {
                if (!reduced) ResetMotion(); reduced = true;
                selectedValue = selectedTo; Apply(); return;
            }
            reduced = false;
            if (submitClock > 0 && (submitClock -= Time.unscaledDeltaTime) <= 0) State(false, false);
            if ((pressed || hovered) && !CanPress) State(false, false);
            if (elapsed >= PressDuration && selectedAge >= .18f && brightnessAge >= .16f && pulseAge >= pulseDuration) return;
            elapsed += Time.unscaledDeltaTime; selectedAge += Time.unscaledDeltaTime; brightnessAge += Time.unscaledDeltaTime;
            float ease = UiEasing.EaseOut(elapsed / PressDuration);
            scale = Mathf.Lerp(fromScale, toScale, ease); y = Mathf.Lerp(fromY, toY, ease);
            selectedValue = Mathf.Lerp(selectedFrom, selectedTo, UiEasing.EaseOut(selectedAge / .18f));
            brightness = Mathf.Lerp(brightnessFrom, brightnessTo, UiEasing.EaseOut(brightnessAge / .16f));
            pulseAge += Time.unscaledDeltaTime;
            Apply();
            if (kind == Kind.Accent && pulseAge >= pulseDuration) enabled = false;
        }
        void LateUpdate() => ApplyAvailability();

        void ApplyAvailability()
        {
            if (!initialized || button == null || disabledFace == null || !(face is Image image)) return;
            bool interactable = button.IsInteractable();
            if (availabilityApplied && wasInteractable == interactable) return;
            availabilityApplied = true; wasInteractable = interactable;
            image.overrideSprite = interactable ? normalOverride : disabledFace;
            for (int i = 0; i < labels.Length; i++)
                labels[i].color = interactable ? labelColors[i] : (Color)new Color32(81, 75, 60, 255);
        }
        void Apply()
        {
            float pulse = pulseDuration > 0 && pulseAge < pulseDuration ? Mathf.Sin(Mathf.Clamp01(pulseAge / pulseDuration) * Mathf.PI) * pulseStrength : 0;
            target.localScale = baseline * (scale + pulse);
            // Accent pulses only own scale; fitted label positions remain layout-owned.
            if (kind != Kind.Accent) target.anchoredPosition = position + Vector2.up * (y * UiEasing.Pixels(target));
            if (navigationIcon != null)
                navigationIcon.localScale = Vector3.Scale(iconBaseline, new Vector3(Mathf.Lerp(1, 40f / 34, selectedValue), Mathf.Lerp(1, 43f / 36, selectedValue), 1));
            // Selectable owns CanvasRenderer tint, including disabled transitions.
            // Apply hover brightness to the graphic instead so a purchase pulse cannot erase it.
            if (face != null) face.color = faceColor * new Color(brightness, brightness, brightness, 1);
        }
        void ResetMotion()
        {
            pressed = hovered = false; pulseAge = pulseDuration = submitClock = 0;
            scale = fromScale = toScale = 1; y = fromY = toY = 0; elapsed = 1;
            brightness = brightnessFrom = brightnessTo = 1; brightnessAge = 1;
            selectedValue = selectedTo; selectedAge = 1;
            if (initialized) Apply();
        }
        void OnDisable() => ResetMotion();
        void OnApplicationFocus(bool focus) { if (!focus) ResetMotion(); }
    }
}
