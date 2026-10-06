using System;
using System.Linq;

namespace RocketRaja.Core
{
    // The marker remains readable in release builds so test saves are rejected,
    // rather than silently turning simulated currency into a paid entitlement.
    [Serializable] public sealed class DevelopmentShopState
    {
        public const string Namespace = "redesign-v2-disposable";
        public string profileNamespace, operationId, packId, status;
        public bool IsEmpty => string.IsNullOrEmpty(profileNamespace) && string.IsNullOrEmpty(operationId) && string.IsNullOrEmpty(packId) && string.IsNullOrEmpty(status);
        public DevelopmentShopState Copy() => (DevelopmentShopState)MemberwiseClone();
        public bool Valid(RedesignCatalog data)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD || REDESIGN_LAB
            return profileNamespace == Namespace && operationId != null && packId != null && status != null &&
                (operationId == "" ? packId == "" && status == "ready" : PremiumWalletProtocol.OperationId(operationId) &&
                data.Packs.Any(p => p.id == packId) && (status == "pending" || status == "delivered" || status == "cancelled" || status == "failed"));
#else
            return false;
#endif
        }
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD || REDESIGN_LAB
    public enum DevelopmentPurchaseOutcome { Delivered, Cancelled, Failed }

    public sealed partial class RedesignEngine
    {
        public bool EnableDevelopmentShop(string profileNamespace)
        {
            if (profileNamespace != DevelopmentShopState.Namespace || State.HasPremium || !MutationAllowed()) return false;
            if (State.HasDevelopmentShop) return true;
            return Commit(s => { s.developmentShop = new DevelopmentShopState {
                profileNamespace = profileNamespace, operationId = "", packId = "", status = "ready"
            }; return true; });
        }

        // One operation may be outstanding. A new request replaces the previous
        // terminal record, so delayed callbacks for older IDs cannot match/grant.
        public string BeginDevelopmentPurchase(string packId)
        {
            if (!State.HasDevelopmentShop || State.HasPremium || !MutationAllowed() || !Data.Packs.Any(p => p.id == packId)) return null;
            if (State.developmentShop.status == "pending")
                return State.developmentShop.packId == packId ? State.developmentShop.operationId : null;
            string id = Guid.NewGuid().ToString("N");
            return Commit(s => { s.developmentShop.operationId = id; s.developmentShop.packId = packId; s.developmentShop.status = "pending"; return true; }) ? id : null;
        }

        // Called only by the explicitly labeled development UI. No receipt,
        // Firebase, Google Play or real purchase is created or acknowledged.
        public bool ResolveDevelopmentPurchase(string operationId, DevelopmentPurchaseOutcome outcome)
        {
            if (!State.HasDevelopmentShop || State.HasPremium || State.developmentShop.status != "pending" ||
                State.developmentShop.operationId != operationId || !Enum.IsDefined(typeof(DevelopmentPurchaseOutcome), outcome)) return false;
            return Commit(s => {
                if (outcome == DevelopmentPurchaseOutcome.Delivered)
                {
                    s.kajuKatli += (Natural)Data.Packs.Single(p => p.id == s.developmentShop.packId).kajuKatli;
                    s.developmentShop.status = "delivered";
                }
                else s.developmentShop.status = outcome == DevelopmentPurchaseOutcome.Cancelled ? "cancelled" : "failed";
                return true;
            });
        }
    }
#endif
}
