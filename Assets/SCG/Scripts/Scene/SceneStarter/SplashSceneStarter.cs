using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// 스플래시 씬은 시작 씬이기 때문에 Mono로 씬에 넣어두기
public class SplashSceneStarter : MonoBehaviour
{
    [SerializeField] private Image companyLogo;
    
    private void Awake()
    {
        Load().Forget();
    }

    private async Awaitable Load()
    {
        var loadManagerTask = LoadManager();
        await FadeIn();
        await loadManagerTask;
        await FadeOut();

        SceneController.ChangeScene(SceneController.Scene.Title, true).Forget();
    }

    private async Awaitable LoadManager()
    {
        LocalizationManager.Initialize();
        ApplicationManager.Create(true);
        DatabaseManager.Create(true);
        SoundManager.Create(true);
        
        var awaitList = new List<Awaitable>
        {
            AtlasManager.Initialize(),
            SoundManager.Instance.Initialize(),
            ApplicationManager.Instance.Initialize()
        };

        await awaitList.WhenAll();
    }

    private async Awaitable FadeIn()
    {
        if (companyLogo == null) return;
        await companyLogo.DOFade(1f, 1f).ToAwaitable();
    }

    private async Awaitable FadeOut()
    {
        if (companyLogo == null) return;
        await companyLogo.DOFade(0f, 0.8f).ToAwaitable();
    }
}