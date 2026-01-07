using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// 스플래시 씬은 시작 씬이기 때문에 Mono로 씬에 넣어두기
public class SplashSceneStarter : MonoBehaviour
{
    [SerializeField] private Image companyLogo;

    private void Awake()
    {
        Load().Forget();
    }

    private async UniTask Load()
    {
        var loadManagerTask = LoadManager();
        await FadeIn();
        await loadManagerTask;
        await FadeOut();

        SceneController.ChangeScene(SceneController.Scene.Title, true).Forget();
    }

    private async UniTask LoadManager()
    {
        LocalizationManager.Initialize();
        ApplicationManager.Create(true);
        DatabaseManager.Create(true);
        SoundManager.Create(true);

        await UniTask.WhenAll(
            AtlasManager.Initialize(),
            SoundManager.Instance.Initialize(),
            ApplicationManager.Instance.Initialize()
        );
    }

    private async UniTask FadeIn()
    {
        if (companyLogo == null) return;
        await companyLogo.DOFade(1f, 1f).ToUniTask();
    }

    private async UniTask FadeOut()
    {
        if (companyLogo == null) return;
        await companyLogo.DOFade(0f, 0.8f).ToUniTask();
    }
}
