using System;
using System.IO;
using System.Linq;
using RocketRaja.Core;
using UnityEngine;

namespace RocketRaja.Classic
{
    /// <summary>Isolated Editor/device playtest. This scene is never added to production build settings.</summary>
    [DefaultExecutionOrder(-50)]
    public sealed class ClassicPreviewSession : MonoBehaviour
    {
        public RedesignRuntimeDriver driver;
        public bool seedReviewProgress=true;
        public string ProfileRoot { get; private set; }
        void Start()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "HTML Importer" || Cloud.RedesignSessionHost.Instance)
                throw new InvalidOperationException("Preview sessions cannot bind production startup or a selected account");
#if UNITY_EDITOR || (DEVELOPMENT_BUILD && CLASSIC_DEVICE_PREVIEW)
            if(!driver)throw new InvalidOperationException("Classic preview requires a saved session driver");
#if UNITY_EDITOR
            ProfileRoot=Path.Combine(Application.temporaryCachePath,"ClassicImporter-"+Guid.NewGuid().ToString("N"));
#else
            if(Application.identifier!="com.diwali.rocketrajaclassic.playtest"||!Debug.isDebugBuild)
                throw new InvalidOperationException("Classic test progress requires its dedicated development package");
            Application.targetFrameRate=60;
            ProfileRoot=Path.Combine(Application.persistentDataPath,"classic-playtest-v1");
#endif
            // Seed only a brand-new test profile. Relaunches and updates retain device progress.
            bool create=!Directory.Exists(ProfileRoot);
            var data=RedesignRuntimeContent.Load();var s=RedesignState.Fresh(RedesignRuntimeDriver.Utc);
            s.developmentFixture=true;
            if(seedReviewProgress)
            {
                s.products=new[]{25,18,12,8,5,3,1,1};s.cash=1240000;s.tapLevel=12;s.completedPrestiges=1;s.fame=1;s.freeCredits=4;s.kajuKatli=400;s.target=ProgressionNumbers.RunTarget(s.completedPrestiges);
                s.cardLevels[0]=1;s.cardLevels[2]=2;s.cardLevels[16]=1;s.cardLevels[18]=1;s.storeName="DIWALI DUKAAN";s.storeNamePromptHandled=true;
                s.upgrades=data.Upgrades.Where(u=>u.effect==CashEffect.SkillPower&&u.rank==1).Select(u=>u.id).ToList();s.everPurchased=s.upgrades.ToList();
            }
            if(create)new RedesignSaveStore(Path.Combine(ProfileRoot,"rocket-raja-v2.save"),data,JsonUtility.ToJson,JsonUtility.FromJson<RedesignState>,JsonUtility.FromJson<SaveVersionHeader>).Save(s);
            if(!driver.InitializeGuest(ProfileRoot, true))throw new InvalidOperationException("Classic playtest could not load/save its isolated profile");
#else
            throw new InvalidOperationException("Classic importer requires the dedicated development build or a production session integration");
#endif
        }
    }
}
