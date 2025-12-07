using GoogleMobileAds.Ump.Api;
using UnityEngine;

public static class ConsentManager
{
    public static bool IsInitialized { get; private set; }
    public static bool IsInitializing { get; private set; }
    public static bool CanAdInitialize { get; private set; }
    public static ConsentStatus CurrentConsentStatus => ConsentInformation.ConsentStatus;

    public static void Initialize()
    {
#if UNITY_EDITOR
        if (!IsInitialized)
        {
            Debug.Log("[ConsentManager] Editor: skipping UMP flow, mark initialized.");
            IsInitialized   = true;
            CanAdInitialize = true;
        }
        return;
#else
        if (IsInitialized)
            return;

        if (IsInitializing)
            return;

        IsInitializing   = true;
        CanAdInitialize  = false;

        Debug.Log("[ConsentManager] UMP consent flow started.");

        var request = new ConsentRequestParameters();
        ConsentInformation.Update(request, OnUpdateCompleted);
#endif
    }

    private static void OnUpdateCompleted(FormError updateError)
    {
        if (updateError != null)
            Debug.LogWarning($"[ConsentManager] UMP Update error: {updateError.Message}");

        var status = ConsentInformation.ConsentStatus;
        Debug.Log($"[ConsentManager] After Update: status={status}");

        if (status == ConsentStatus.NotRequired)
        {
            Debug.Log("[ConsentManager] GDPR not required. Skipping consent UI.");
            FinishConsentFlow(true);
            return;
        }

        if (!ConsentInformation.IsConsentFormAvailable())
        {
            Debug.Log("[ConsentManager] Consent form is NOT available. Using UMP CanRequestAds flag.");
            bool canRequest = ConsentInformation.CanRequestAds();
            FinishConsentFlow(canRequest);
            return;
        }

        Debug.Log("[ConsentManager] Consent form is available. Loading...");

        ConsentForm.Load((form, loadError) =>
        {
            if (loadError != null)
            {
                Debug.LogWarning($"[ConsentManager] UMP Load error: {loadError.Message}");
                bool canRequest = ConsentInformation.CanRequestAds();
                FinishConsentFlow(canRequest);
                return;
            }

            if (form == null)
            {
                Debug.LogWarning("[ConsentManager] UMP Load returned null form.");
                bool canRequest = ConsentInformation.CanRequestAds();
                FinishConsentFlow(canRequest);
                return;
            }

            Debug.Log("[ConsentManager] Consent form loaded. Showing...");

            form.Show(showError =>
            {
                if (showError != null)
                    Debug.LogWarning($"[ConsentManager] UMP Show error: {showError.Message}");
                else
                    Debug.Log("[ConsentManager] Consent form closed by user.");

                bool canRequest = ConsentInformation.CanRequestAds();
                FinishConsentFlow(canRequest);
            });
        });
    }

    private static void FinishConsentFlow(bool canAdInitialize)
    {
        var status = ConsentInformation.ConsentStatus;
        var umpCanRequestAds = ConsentInformation.CanRequestAds();

        CanAdInitialize = canAdInitialize;
        IsInitialized   = true;
        IsInitializing  = false;

        Debug.Log(
            $"[ConsentManager] FinishConsentFlow. status={status}, " +
            $"UMP_CanRequestAds={umpCanRequestAds}, CanAdInitialize={CanAdInitialize}"
        );
    }

    public static void ResetConsent()
    {
        Debug.Log("[ConsentManager] ResetConsent called. Clearing UMP state.");

        IsInitialized   = false;
        IsInitializing  = false;
        CanAdInitialize = false;

        ConsentInformation.Reset();
    }
}