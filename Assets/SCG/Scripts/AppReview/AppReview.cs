using Cysharp.Threading.Tasks;
using UnityEngine;
#if UNITY_ANDROID
using Google.Play.Review;
#endif

public static class AppReview
{
    #region ## File I/O ##
    private static bool? _isAlreadyReview = null;
    private static bool IsAlreadyReview
    {
        get
        {
            if (_isAlreadyReview == null)
            {
                _isAlreadyReview = PlayerPrefs.GetInt("StoreReview", 0) == 1;
            }
            return _isAlreadyReview.Value;
        }
        set
        {
            if (_isAlreadyReview != value)
            {
                _isAlreadyReview = value;
                PlayerPrefs.SetInt("StoreReview", value ? 1 : 0);
            }
        }
    }
    #endregion

    public static async UniTask Open()
    {
        if (!IsAlreadyReview)
        {
            IsAlreadyReview = true;
#if UNITY_ANDROID
            await RequestGooglePlayStoreReview();
#elif UNITY_IOS
            RequestAppStoreReview();
#endif
        }
        else
        {
            Debug.Log("이미 리뷰 평점 요청을 한 적이 있습니다.");
        }
    }

    private static async UniTask RequestGooglePlayStoreReview()
    {
#if UNITY_ANDROID
        ReviewManager reviewManager = new ReviewManager();
        var requestFlowOperation = reviewManager.RequestReviewFlow();
        while (!requestFlowOperation.IsDone)
        {
            await UniTask.NextFrame();
        }

        if (requestFlowOperation.Error != ReviewErrorCode.NoError)
        {
            Debug.LogError("requestFlowOperation" + requestFlowOperation.Error);
            return;
        }
        PlayReviewInfo playReviewInfo = requestFlowOperation.GetResult();

        var launchFlowOperation = reviewManager.LaunchReviewFlow(playReviewInfo);
        while (!launchFlowOperation.IsDone)
        {
            await UniTask.NextFrame();
        }

        playReviewInfo = null;
        if (launchFlowOperation.Error != ReviewErrorCode.NoError)
        {
            Debug.LogError("launchFlowOperation: " + launchFlowOperation.Error);
            return;
        }
        Debug.Log("Open Review Popup");
#endif
    }

    private static void OpenGooglePlayStorePage()
    {
#if UNITY_ANDROID
        Application.OpenURL($"market://details?id={Application.identifier}");
#endif
    }

    private static void RequestAppStoreReview()
    {
#if UNITY_IOS
        UnityEngine.iOS.Device.RequestStoreReview();
#endif
    }
}
