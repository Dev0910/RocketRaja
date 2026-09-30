using System;
using System.Globalization;
using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    /// <summary>Saved, read-only guide to the three authoritative game skills.</summary>
    public sealed class ImportedSkillsScreen : MonoBehaviour
    {
        [SerializeField] Button closeButton;
        [SerializeField] Button backButton;
        [SerializeField] TMP_Text[] rankLabels;
        [SerializeField] TMP_Text[] effectLabels;
        [SerializeField] TMP_Text[] cooldownLabels;
        [SerializeField] TMP_Text[] statusLabels;
        [SerializeField] Image[] statusFaces;

        GameController controller;
        float refreshClock;

        void Awake()
        {
            if (closeButton == null || backButton == null || !Valid(rankLabels) || !Valid(effectLabels) ||
                !Valid(cooldownLabels) || !Valid(statusLabels) || statusFaces == null || statusFaces.Length != 3 ||
                Array.Exists(statusFaces, x => x == null))
            {
                Debug.LogError("Skills canvas has missing editor-authored references.", this);
                enabled = false;
                return;
            }
            closeButton.onClick.AddListener(Close);
            backButton.onClick.AddListener(Close);
        }

        static bool Valid(TMP_Text[] labels) => labels != null && labels.Length == 3 && !Array.Exists(labels, x => x == null);

        void OnDestroy()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
            if (backButton != null) backButton.onClick.RemoveListener(Close);
        }

        public void Bind(GameController owner) => controller = owner;

        public void Open()
        {
            if (controller == null || controller.Engine == null)
            {
                Debug.LogError("Skills canvas was opened before binding its game owner.", this);
                return;
            }
            Refresh();
            UiMotion.Show(gameObject, controller, true);
        }

        void Update()
        {
            if (!gameObject.activeInHierarchy) return;
            refreshClock += Time.unscaledDeltaTime;
            if (refreshClock < .2f) return;
            refreshClock = 0;
            Refresh();
        }

        public void Refresh()
        {
            if (controller == null || controller.Engine == null) return;
            var engine = controller.Engine;
            var state = engine.State;
            for (int i = 0; i < 3; i++)
            {
                int rank = engine.SkillRank(i);
                SkillDef definition = engine.Data.skills[i];
                int shownRank = Math.Max(1, rank);
                double value = definition.values[shownRank - 1];
                rankLabels[i].text = rank > 0 ? "Rank " + rank + " / 4" : "Not unlocked";
                effectLabels[i].text = i == 0
                    ? "×" + value.ToString("0.##", CultureInfo.InvariantCulture) + " automatic sales for " + definition.duration.ToString("0", CultureInfo.InvariantCulture) + "s"
                    : i == 1
                        ? value.ToString("0.##", CultureInfo.InvariantCulture) + " automatic taps/sec for " + definition.duration.ToString("0", CultureInfo.InvariantCulture) + "s"
                        : value.ToString("0", CultureInfo.InvariantCulture) + " seconds of normal product sales";
                cooldownLabels[i].text = "Cooldown " + GameView.Duration(engine.SkillCooldown(i));
                var skill = state.skills[i];
                if (rank == 0)
                {
                    statusLabels[i].text = "LOCKED";
                    statusFaces[i].color = new Color32(240, 230, 210, 255);
                }
                else if (skill.activeUntil > state.now)
                {
                    statusLabels[i].text = "ACTIVE · " + GameView.Duration(skill.activeUntil - state.now);
                    statusFaces[i].color = new Color32(230, 240, 250, 255);
                }
                else if (skill.readyAt > state.now)
                {
                    statusLabels[i].text = "WAIT · " + GameView.Duration(skill.readyAt - state.now);
                    statusFaces[i].color = new Color32(250, 241, 222, 255);
                }
                else
                {
                    statusLabels[i].text = "READY";
                    statusFaces[i].color = new Color32(231, 246, 232, 255);
                }
            }
        }

        public void Close()
        {
            UiMotion.Hide(gameObject, controller);
        }
    }
}
