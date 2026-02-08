using System;
using UnityEngine;

namespace StarCloudgamesLibrary
{
    [CreateAssetMenu(fileName = "AdKeyConfig", menuName = "SCG/Ad Key Config")]
    public class AdKeyConfig : ScriptableObject
    {
        [Header("Android")]
        public PlatformAdKeys android;

        [Header("iOS")]
        public PlatformAdKeys ios;

        public PlatformAdKeys Current
        {
            get
            {
#if UNITY_ANDROID
                return android;
#elif UNITY_IOS
                return ios;
#else
                return android;
#endif
            }
        }

        [Serializable]
        public class PlatformAdKeys
        {
            public string appKey;
            public string interstitialUnitId;
            public string rewardedUnitId;
            public string bannerUnitId;
        }
    }
}
