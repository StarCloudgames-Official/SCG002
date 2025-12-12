using UnityEngine;

public class InGameSceneStarter : SceneStarter
{
    public override async Awaitable StartScene()
    {
        Debug.Log("Starting InGameSceneStarter");

        var ingameManager = InGameManager.Create();

        await ingameManager.Initialize();

        await LoadingFade.StartFadeOut();
    }
}