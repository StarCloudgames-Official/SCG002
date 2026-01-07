using Cysharp.Threading.Tasks;
using StarCloudgamesLibrary;
using UnityEngine;

public class TitleSceneStarter : SceneStarter
{
    public override async UniTask StartScene()
    {
        Debug.Log("Starting TitleSceneStarter");

        var titleUI = await UIManager.OpenUI<UITitle>();
        titleUI.GaugeSlider.SetValueImmediate(0, 1.0f);

        ApplicationManager.Instance.AddBackListener(UIManager.BackspaceClose);

        var advertisementManager = AdvertisementManager.Create(true);
        var dataTableManager = DataTableManager.Create(true);
        var iapManager = IAPManager.Create(true);

        await LoadingFade.StartFadeOut();

        await AppEvent.Initialize();
        await titleUI.GaugeSlider.AnimateTo(0.1f, 1.0f, 0.1f);

        var versionOk = await VersionChecker.CheckVersion();
        if (!versionOk)
        {
            VersionChecker.HandleUpdateRequired();
            return;
        }
        await titleUI.GaugeSlider.AnimateTo(0.2f, 1.0f, 0.1f);

        await PushAlert.Initialize();

        ConsentManager.Initialize();
        await UniTask.WaitUntil(() => ConsentManager.IsInitialized);

        await advertisementManager.Initialize();
        await titleUI.GaugeSlider.AnimateTo(0.4f, 1.0f, 0.1f);

        await TimeManager.Initialize();
        await titleUI.GaugeSlider.AnimateTo(0.5f, 1.0f, 0.1f);

        await dataTableManager.Initialize();
        await titleUI.GaugeSlider.AnimateTo(0.6f, 1.0f, 0.1f);

        await iapManager.Initialize();
        await titleUI.GaugeSlider.AnimateTo(0.8f, 1.0f, 0.1f);

        await DatabaseManager.Instance.LocalInitialize();
        await titleUI.GaugeSlider.AnimateTo(1.0f, 1.0f, 0.1f);

        SceneController.ChangeScene(SceneController.Scene.Lobby).Forget();
    }
}
