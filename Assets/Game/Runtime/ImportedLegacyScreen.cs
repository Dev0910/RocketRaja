using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja
{
    /// <summary>The twelve saved legacy perk rows from the HTML screen.</summary>
    public sealed class ImportedLegacyScreen : MonoBehaviour
    {
        [SerializeField] Button collapseButton;
        [SerializeField] TMP_Text reputationLabel;
        [SerializeField] Button[] purchaseButtons;
        [SerializeField] TMP_Text[] names;
        [SerializeField] TMP_Text[] ranks;
        [SerializeField] TMP_Text[] effects;
        [SerializeField] TMP_Text[] purchaseLabels;

        GameController controller;
        GameView view;
        float refreshClock;

        void Awake()
        {
            if (collapseButton == null || reputationLabel == null || !Valid(purchaseButtons) ||
                !Valid(names) || !Valid(ranks) || !Valid(effects) || !Valid(purchaseLabels))
            {
                Debug.LogError("Legacy canvas has missing editor-authored references.", this);
                enabled = false;
                return;
            }
            collapseButton.onClick.AddListener(() => view?.Switch("Home"));
            for (int i = 0; i < 12; i++)
            {
                int index = i;
                purchaseButtons[i].onClick.AddListener(() => Purchase(index));
            }
        }

        static bool Valid<T>(T[] items) where T : UnityEngine.Object =>
            items != null && items.Length == 12 && !Array.Exists(items, x => x == null);

        public void Bind(GameController owner, GameView ownerView)
        {
            controller = owner;
            view = ownerView;
            UiFeedback.Wire(gameObject, owner);
            Refresh();
        }

        void Update()
        {
            if (controller == null || !gameObject.activeInHierarchy) return;
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
            reputationLabel.text = state.reputation.ToString();
            for (int i = 0; i < 12; i++)
            {
                int rank = state.Legacy(i);
                long cost = engine.LegacyCost(i);
                names[i].text = engine.Data.legacy[i].name;
                ranks[i].text = "Rank " + rank + " / 5";
                effects[i].text = i < 8 ? "×2 product income per rank" :
                    i == 8 ? "×2 tap income per rank" :
                    i == 9 ? "×2 all product income per rank" :
                    i == 10 ? "+2 hours offline capacity per rank" :
                    "5% shorter skill cooldowns per rank";
                purchaseLabels[i].text = rank >= 5 ? "Mastered" :
                    cost + " reputation\nImprove perk";
                purchaseButtons[i].interactable = engine.CanMutate && rank < 5 && state.reputation >= cost;
            }
        }

        public void CelebratePrestige()
        {
            UiFeedback.Pulse(reputationLabel.transform, controller, .22f, .85f);
            for (int i = 0; i < ranks.Length; i++)
                UiFeedback.Pulse(ranks[i].transform, controller, .05f, .4f + i * .035f);
        }

        void Purchase(int index)
        {
            if (controller == null || controller.Engine == null) return;
            if (controller.Engine.BuyLegacy(index))
            {
                controller.Save();
                view?.Notice("Permanent perk improved.");
                view?.RefreshNow();
                UiFeedback.Pulse(ranks[index].transform, controller, .12f, .5f);
                UiFeedback.Pulse(purchaseButtons[index].transform, controller, .07f);
                UiFeedback.Pulse(reputationLabel.transform, controller, .1f);
                controller.HomeScreen?.RewardFeedback();
            }
            Refresh();
        }
    }
}
