using System;
using System.Linq;

namespace RocketRaja.Core
{
    public enum TutorialNote { None, Welcome, Training, TrainingResult, Product, ProductResult, Upgrade, UpgradeResult, Achievement, AchievementResult, Prestige, Talent, CardBenefit }
    [Flags] public enum TutorialProgress
    {
        None=0, TrainingOffered=1, TrainingDone=2, ProductOffered=4, ProductDone=8,
        UpgradeOffered=16, UpgradeDone=32, AchievementOffered=64, AchievementDone=128,
        PrestigeOffered=256, PrestigeDone=512, TalentOffered=1024, TalentDone=2048, CardExplained=4096
    }
    [Serializable] public sealed class ShopkeeperTutorialState
    {
        public int version, serial, presented;
        public TutorialProgress progress;
        public TutorialNote note;
        public bool skipped;
        public Amount before, after;
        public string cardId;
        public Natural cardBefore, cardAfter, openingTaps;
        public bool Active => version==1 && !skipped && !Has(TutorialProgress.CardExplained);
        public bool Has(TutorialProgress flag) => (progress & flag)!=0;
        public static ShopkeeperTutorialState Fresh() => new ShopkeeperTutorialState {
            version=1, serial=1, note=TutorialNote.Welcome, cardId="", cardBefore=0, cardAfter=0, openingTaps=0
        };
        public ShopkeeperTutorialState Copy() => (ShopkeeperTutorialState)MemberwiseClone();
        public void Show(TutorialNote value, Amount from=default, Amount to=default)
        { note=value; before=from; after=to; serial++; }
        public bool Valid(RedesignCatalog data) => version==0 || version==1 && serial>=1 && presented>=0 && presented<=serial &&
            (int)progress>=0 && ((int)progress & ~8191)==0 && Enum.IsDefined(typeof(TutorialNote),note) &&
            before.Valid && after.Valid && cardBefore.Valid && cardAfter.Valid && openingTaps.Valid && cardId!=null &&
            (cardId=="" || data.TryCard(cardId,out _) && cardAfter.Value==cardBefore.Value+1);
    }

    public sealed partial class RedesignEngine
    {
        public const string TutorialUpgrade = "PP-PHU-01";
        public bool TutorialActive => State.tutorial!=null && State.tutorial.Active;
        static bool EarningResult(TutorialNote note) => note==TutorialNote.TrainingResult || note==TutorialNote.ProductResult || note==TutorialNote.UpgradeResult;
        public bool CanContinueTutorialEarning => TutorialActive &&
            (State.tutorial.note==TutorialNote.Welcome || EarningResult(State.tutorial.note)) &&
            State.manualTaps.CompareTo(State.tutorial.openingTaps)>0;
        public TutorialNote NextTutorialOffer()
        {
            var t=State.tutorial; if(!TutorialActive || t.note!=TutorialNote.None && !EarningResult(t.note)) return TutorialNote.None;
            if(!t.Has(TutorialProgress.TrainingDone))
                return !t.Has(TutorialProgress.TrainingOffered) && LevelQuote(-1,1).affordable ? TutorialNote.Training : TutorialNote.None;
            if(!t.Has(TutorialProgress.ProductDone))
                return !t.Has(TutorialProgress.ProductOffered) && LevelQuote(0,1).affordable ? TutorialNote.Product : TutorialNote.None;
            if(!t.Has(TutorialProgress.UpgradeDone))
                return !t.Has(TutorialProgress.UpgradeOffered) && State.products[0]>0 && Eligible(TutorialUpgrade) && UpgradeCost(TutorialUpgrade)<=State.cash ? TutorialNote.Upgrade : TutorialNote.None;
            // Claim readiness and prestige eligibility are independent, real economy checks.
            if(!t.Has(TutorialProgress.AchievementOffered) && !t.Has(TutorialProgress.AchievementDone) && RedesignAchievements.ReadyCount(State)>0) return TutorialNote.Achievement;
            if(!t.Has(TutorialProgress.PrestigeOffered) && State.completedPrestiges.IsZero && PreviewPrestige().Eligible) return TutorialNote.Prestige;
            if(!t.Has(TutorialProgress.TalentOffered) && !t.Has(TutorialProgress.TalentDone) && !State.completedPrestiges.IsZero && !State.freeCredits.IsZero && CanDraw(true)) return TutorialNote.Talent;
            return TutorialNote.None;
        }
        static TutorialProgress OfferFlag(TutorialNote note)
        {
            switch(note) {
                case TutorialNote.Training:return TutorialProgress.TrainingOffered;
                case TutorialNote.Product:return TutorialProgress.ProductOffered;
                case TutorialNote.Upgrade:return TutorialProgress.UpgradeOffered;
                case TutorialNote.Achievement:return TutorialProgress.AchievementOffered;
                case TutorialNote.Prestige:return TutorialProgress.PrestigeOffered;
                case TutorialNote.Talent:return TutorialProgress.TalentOffered;
                default:return TutorialProgress.None;
            }
        }
        public bool OfferTutorial(TutorialNote note) => note!=TutorialNote.None && NextTutorialOffer()==note && Commit(s=> {
            s.tutorial.progress|=OfferFlag(note); s.tutorial.Show(note); return true;
        },false);
        public bool BeginTutorialEarning() => CanContinueTutorialEarning &&
            Commit(s=> {s.tutorial.note=TutorialNote.None;return true;},false);
        public bool RepeatTutorialTip(TutorialNote note) => TutorialActive && OfferFlag(note)!=TutorialProgress.None &&
            Commit(s=> {s.tutorial.Show(note);return true;},false);
        // Mark before routing. Reloading or ordinary navigation cannot repeat an introduction.
        public bool PresentTutorial(int serial) => TutorialActive && State.tutorial.serial==serial && State.tutorial.presented<serial && Commit(s=> {s.tutorial.presented=serial;return true;},false);
        public bool DismissTutorialNote(int serial) => TutorialActive && State.tutorial.serial==serial && State.tutorial.note!=TutorialNote.None && Commit(s=> {
            if(s.tutorial.note==TutorialNote.CardBenefit)s.tutorial.progress|=TutorialProgress.CardExplained;
            s.tutorial.note=TutorialNote.None;return true;
        },false);
        public bool SkipTutorial() => TutorialActive && Commit(s=> {s.tutorial.skipped=true;s.tutorial.note=TutorialNote.None;return true;},false);
        public bool ReplayTutorial() => Commit(s=> {
            s.tutorial=ShopkeeperTutorialState.Fresh();var t=s.tutorial;t.openingTaps=s.manualTaps;
            if(s.tapLevel>0)t.progress|=TutorialProgress.TrainingDone;
            if(s.products[0]>0 || !s.completedPrestiges.IsZero)t.progress|=TutorialProgress.ProductDone;
            if(s.everPurchased.Contains(TutorialUpgrade))t.progress|=TutorialProgress.UpgradeDone;
            if(s.claimedAchievements?.Count>0)t.progress|=TutorialProgress.AchievementDone;
            if(!s.completedPrestiges.IsZero)t.progress|=TutorialProgress.TrainingDone|TutorialProgress.PrestigeDone;
            return true;
        },false);

        void ObserveTutorial(RedesignState next)
        {
            var t=next.tutorial; if(t==null || !t.Active)return;
            var effects=new RunEffects(Data,next);
            if(!t.Has(TutorialProgress.TrainingDone) && next.tapLevel>State.tapLevel) {
                t.progress|=TutorialProgress.TrainingDone;
                t.Show(TutorialNote.TrainingResult,BaseTapPayout(false,State,Effects),BaseTapPayout(false,next,effects));
            }
            if(!t.Has(TutorialProgress.ProductDone) && State.products[0]==0 && next.products[0]>0) {
                t.progress|=TutorialProgress.ProductDone;
                t.Show(TutorialNote.ProductResult,ProductIncome(0,State,Effects),ProductIncome(0,next,effects));
            }
            if(!t.Has(TutorialProgress.UpgradeDone) && !State.everPurchased.Contains(TutorialUpgrade) && next.everPurchased.Contains(TutorialUpgrade)) {
                t.progress|=TutorialProgress.UpgradeDone;
                t.Show(TutorialNote.UpgradeResult,NormalIncome(State,Effects),NormalIncome(next,effects));
            }
            if(!t.Has(TutorialProgress.AchievementDone) && (next.claimedAchievements?.Count??0)>(State.claimedAchievements?.Count??0)) {
                t.progress|=TutorialProgress.AchievementDone;t.Show(TutorialNote.AchievementResult,State.kajuKatli.ToAmount(),next.kajuKatli.ToAmount());
            }
            if(next.completedPrestiges.CompareTo(State.completedPrestiges)>0 && !t.Has(TutorialProgress.PrestigeDone)) {
                t.progress|=TutorialProgress.PrestigeDone;t.note=TutorialNote.None;
            }
            if(!t.Has(TutorialProgress.TalentDone) && !next.completedPrestiges.IsZero && next.reveal.pending && next.reveal.operationId!=State.reveal.operationId && next.reveal.payment=="credit") {
                t.progress|=TutorialProgress.TalentDone;t.cardId=next.reveal.cardId;t.cardBefore=next.reveal.before;t.cardAfter=next.reveal.after;t.note=TutorialNote.None;
            }
            if(t.Has(TutorialProgress.TalentDone) && !t.Has(TutorialProgress.CardExplained) && State.reveal.pending && !next.reveal.pending && t.cardId!="")t.Show(TutorialNote.CardBenefit);
            // Only taps made after this result should dismiss it, including after a reload.
            if(t.serial!=State.tutorial.serial && EarningResult(t.note))t.openingTaps=next.manualTaps;
        }
    }
}
