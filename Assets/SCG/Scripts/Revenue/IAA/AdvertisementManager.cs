using System;
using System.Collections;
using Unity.Services.LevelPlay;
using UnityEngine;

namespace StarCloudgamesLibrary
{
    public class AdvertisementManager : Singleton<AdvertisementManager>
    {
        private bool initialized;

        private LevelPlayRewardedAd rewardedAd;
        private LevelPlayInterstitialAd interstitialAd;
        private LevelPlayBannerAd bannerAd;

        private Action currentRewardAction;

        #region "Keys"

#if UNITY_ANDROID && !UNITY_EDITOR
        private readonly string appKey = "REDACTED_APP_KEY";
        private readonly string interstitialUnitId = "REDACTED_INTERSTITIAL_ID";
        private readonly string rewardedUnitId = "REDACTED_REWARDED_ID";
        private readonly string bannerUnitId = "REDACTED_BANNER_ID";
#elif UNITY_IOS
        private readonly string appKey = "unexpectedPlatform";
        private readonly string interstitialUnitId = "unexpectedPlatform";
        private readonly string rewardedUnitId = "unexpectedPlatform";
        private readonly string bannerUnitId = "unexpectedPlatform";
#else
        private readonly string appKey = "REDACTED_APP_KEY";
        private readonly string interstitialUnitId = "REDACTED_INTERSTITIAL_ID";
        private readonly string rewardedUnitId = "REDACTED_REWARDED_ID";
        private readonly string bannerUnitId = "REDACTED_BANNER_ID";
#endif

        #endregion

        #region "Rewarded Ad"

        public void ShowRewardedAd(Action rewardAction)
        {
            if (!initialized)
            {
                Debug.LogWarning("AdvertisementManager: ShowRewardedAd failed - Not Initialized");
                return;
            }
            
            if(rewardedAd != null && rewardedAd.IsAdReady())
            {
                if(bannerAd != null)
                {
                    HideBannerAd();
                }

                currentRewardAction = rewardAction;
                rewardedAd.ShowAd();
            }
            else
            {
                Debug.LogWarning("AdvertisementManager: ShowRewardedAd failed - Ad Not Ready or Null");
                rewardAction?.Invoke();
            }
        }

        private IEnumerator GiveRewardedAdReward()
        {
            yield return new WaitForEndOfFrame();
            currentRewardAction?.Invoke();
            currentRewardAction = null;

            if(bannerAd != null)
            {
                ShowBannerAd();
            }
        }

        private void InitializeRewardedAd()
        {
            rewardedAd = new LevelPlayRewardedAd(rewardedUnitId);

            rewardedAd.OnAdLoaded += RewardedOnAdLoadedEvent;
            rewardedAd.OnAdLoadFailed += RewardedOnAdLoadFailedEvent;
            rewardedAd.OnAdDisplayed += RewardedOnAdDisplayedEvent;
            rewardedAd.OnAdClicked += RewardedOnAdClickedEvent;
            rewardedAd.OnAdClosed += RewardedOnAdClosedEvent;
            rewardedAd.OnAdRewarded += RewardedOnAdRewarded;
            rewardedAd.OnAdInfoChanged += RewardedOnAdInfoChangedEvent;

            rewardedAd.LoadAd();
        }

        void RewardedOnAdLoadedEvent(LevelPlayAdInfo adInfo) 
        { 
            Debug.Log("AdvertisementManager: Rewarded Ad Loaded Successfully");
        }
        void RewardedOnAdLoadFailedEvent(LevelPlayAdError error) 
        { 
            Debug.LogWarning($"AdvertisementManager: Rewarded Ad Failed to Load - {error.ErrorMessage}");
        }
        void RewardedOnAdClickedEvent(LevelPlayAdInfo adInfo) { }
        void RewardedOnAdDisplayedEvent(LevelPlayAdInfo adInfo) { }
        void RewardedOnAdClosedEvent(LevelPlayAdInfo adInfo)
        {
            rewardedAd.LoadAd();
        }
        void RewardedOnAdInfoChangedEvent(LevelPlayAdInfo adInfo) { }

        void RewardedOnAdRewarded(LevelPlayAdInfo adReward, LevelPlayReward adInfo)
        {
            StartCoroutine(GiveRewardedAdReward());
            rewardedAd.LoadAd();
        }

#endregion

        #region "Interstitial Ad"

        public void ShowInterstitial()
        {
            if (!initialized)
            {
                Debug.LogWarning("AdvertisementManager: ShowInterstitial failed - Not Initialized");
                return;
            }
            
            if (interstitialAd == null || !interstitialAd.IsAdReady()) 
            {
                Debug.LogWarning("AdvertisementManager: ShowInterstitial failed - Ad Not Ready or Null");
                return;
            }
            
            if(bannerAd != null)
            {
                HideBannerAd();
            }
            
            interstitialAd.ShowAd();
        }

        private void LoadInterstitialAd()
        {
            interstitialAd.LoadAd();
        }

        private void InitializeInterstitialAd()
        {
            interstitialAd = new LevelPlayInterstitialAd(interstitialUnitId);
            interstitialAd.OnAdLoaded += InterstitialOnAdLoadedEvent;
            interstitialAd.OnAdLoadFailed += InterstitialOnAdLoadFailedEvent;
            interstitialAd.OnAdDisplayed += InterstitialOnAdDisplayedEvent;
            interstitialAd.OnAdClicked += InterstitialOnAdClickedEvent;
            interstitialAd.OnAdClosed += InterstitialOnAdClosedEvent;
            interstitialAd.OnAdInfoChanged += InterstitialOnAdInfoChangedEvent;

            LoadInterstitialAd();
        }

        private void InterstitialOnAdLoadedEvent(LevelPlayAdInfo adInfo) 
        { 
            Debug.Log("AdvertisementManager: Interstitial Ad Loaded Successfully");
        }
        private void InterstitialOnAdLoadFailedEvent(LevelPlayAdError error) 
        { 
            Debug.LogWarning($"AdvertisementManager: Interstitial Ad Failed to Load - {error.ErrorMessage}");
        }
        private void InterstitialOnAdDisplayedEvent(LevelPlayAdInfo adInfo) { }
        private void InterstitialOnAdClickedEvent(LevelPlayAdInfo adInfo) { }
        private void InterstitialOnAdClosedEvent(LevelPlayAdInfo adInfo)
        {
            interstitialAd.LoadAd();
            if(bannerAd != null)
            {
                ShowBannerAd();
            }
        }
        private void InterstitialOnAdInfoChangedEvent(LevelPlayAdInfo adInfo) { }

        #endregion

        #region "Banner Ad"

        private void InitializeBannerAd()
        {
            var config = new LevelPlayBannerAd.Config.Builder()
                .SetDisplayOnLoad(false)
                .Build();
            bannerAd = new LevelPlayBannerAd(bannerUnitId, config);
            bannerAd.OnAdLoaded += BannerOnAdLoadedEvent;
            bannerAd.OnAdLoadFailed += BannerOnAdLoadFailedEvent;
            bannerAd.OnAdDisplayed += BannerOnAdDisplayedEvent;
            bannerAd.OnAdClicked += BannerOnAdClickedEvent;
            bannerAd.OnAdCollapsed += BannerOnAdCollapsedEvent;
            bannerAd.OnAdLeftApplication += BannerOnAdLeftApplicationEvent;
            bannerAd.OnAdExpanded += BannerOnAdExpandedEvent;

            LoadBannerAd();
        }
        private void LoadBannerAd()
        {
            bannerAd.LoadAd();
        }
        public void ShowBannerAd()
        {
            if (bannerAd == null)
            {
                Debug.LogWarning("AdvertisementManager: ShowBannerAd failed - BannerAd is null");
                return;
            }
            bannerAd.ShowAd();
        }
        public void HideBannerAd()
        {
            bannerAd?.HideAd();
        }
        private void DestroyBannerAd()
        {
            bannerAd?.DestroyAd();
        }

        private void BannerOnAdLoadedEvent(LevelPlayAdInfo adInfo) { }
        private void BannerOnAdLoadFailedEvent(LevelPlayAdError ironSourceError) { }
        private void BannerOnAdClickedEvent(LevelPlayAdInfo adInfo) { }
        private void BannerOnAdDisplayedEvent(LevelPlayAdInfo adInfo) { }
        private void BannerOnAdCollapsedEvent(LevelPlayAdInfo adInfo) { }
        private void BannerOnAdLeftApplicationEvent(LevelPlayAdInfo adInfo) { }
        private void BannerOnAdExpandedEvent(LevelPlayAdInfo adInfo) { }

        #endregion

        #region "Initialize"

        public bool Initialized() => initialized;

        public override async Awaitable Initialize()
        {
            LevelPlay.OnInitSuccess += LevelPlayInitializeCompleted;
            LevelPlay.OnInitFailed += LevelPlayInitializeFailed;

            LevelPlay.Init(appKey);
            await AwaitableExtensions.WaitUntilAsync(() => initialized);
        }

        private void LevelPlayInitializeCompleted(LevelPlayConfiguration configuration)
        {
            Debug.Log("AdvertisementManager: LevelPlay SDK Initialized Successfully");
            
            InitializeRewardedAd();
            InitializeInterstitialAd();
            InitializeBannerAd();

            initialized = true;
        }

        private void LevelPlayInitializeFailed(LevelPlayInitError error)
        {
            Debug.LogError($"AdvertisementManager: LevelPlay SDK Failed to Initialize - {error.ErrorMessage}");
            initialized = false;
        }

        #endregion
    }
}