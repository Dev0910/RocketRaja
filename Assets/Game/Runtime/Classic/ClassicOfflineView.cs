using RocketRaja.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RocketRaja.Classic
{
    /// <summary>Only binds and animates the saved offline hierarchy; never constructs UI.</summary>
    public sealed class ClassicOfflineView : MonoBehaviour
    {
        public TMP_Text amount, rewardState, stored, cap, away, overflow, status, efficiency;
        public TMP_Text normalReward, adReward, adStatus, watchLabel;
        public Image timeFill, efficiencyFill;
        public RectTransform capMarker;
        public Button watch;
        string receiptId;

        public void Refresh(ClassicUI ui, bool watching, string message, bool ready)
        {
            var receipt = ui.Engine.State.offline;
            var data = new OfflineTimeline(receipt, ui.Engine.State.cardLevels[25]);
            bool fresh = receiptId != receipt.id;
            receiptId = receipt.id;
            amount.text = "₹" + (receipt.amount * (receipt.adRewarded ? 2 : 1));
            rewardState.text = receipt.adRewarded ? "2× reward collected" : "Normal reward · already saved";
            stored.text = OfflineTimeline.Duration(data.Stored) + " / " + OfflineTimeline.Duration(data.Cap) + " stored";
            cap.text = OfflineTimeline.Duration(data.Cap) + " cap";
            away.text = OfflineTimeline.Duration(data.Away) + " away";
            overflow.text = data.Overflow > 0 ? OfflineTimeline.Duration(data.Overflow) + " beyond cap" :
                OfflineTimeline.Duration(data.Cap - data.Stored) + " storage remaining";
            status.text = data.Capped ? "CAP REACHED" : "ROOM TO EARN";
            efficiency.text = (data.Efficiency * 100).ToString("0.#") + "% efficiency";
            normalReward.text = "₹" + receipt.amount;
            adReward.text = "₹" + receipt.amount * 2;
            capMarker.anchorMin = new Vector2((float)data.CapPosition, 0);
            capMarker.anchorMax = new Vector2((float)data.CapPosition, 1);
            if (fresh && gameObject.activeInHierarchy && !ui.Engine.State.reducedMotion)
            {
                timeFill.fillAmount = efficiencyFill.fillAmount = 0;
                ui.motion.Fill(timeFill, (float)data.Fill);
                ui.motion.Fill(efficiencyFill, (float)data.Efficiency);
            }
            else { ui.motion.Fill(timeFill, (float)data.Fill); ui.motion.Fill(efficiencyFill, (float)data.Efficiency); }
            watch.interactable = ready && !watching && receipt.pending && !receipt.adRewarded && ui.driver.Session.CanMutate;
            watchLabel.text = receipt.adRewarded ? "2× reward collected" : watching ? "Watching ad…" : "Watch ad for 2× rewards";
            adStatus.text = receipt.adRewarded ? "Bonus saved. Continue when you're ready." : message;
        }
    }
}
