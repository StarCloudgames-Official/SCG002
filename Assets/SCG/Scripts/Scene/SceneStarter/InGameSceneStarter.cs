using UnityEngine;

public class InGameSceneStarter : SceneStarter
{
    public override async Awaitable StartScene()
    {
        Debug.Log("Starting InGameSceneStarter");

        var inGameManager = InGameManager.Create();
        await inGameManager.Initialize();

        await LoadingFade.StartFadeOut();
        inGameManager.InGameContext.StageManager.StartStage().Forget();
    }
}