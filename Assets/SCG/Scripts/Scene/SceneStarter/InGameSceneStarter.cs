using Cysharp.Threading.Tasks;
using UnityEngine;

public class InGameSceneStarter : SceneStarter
{
    public override async UniTask StartScene()
    {
        Debug.Log("Starting InGameSceneStarter");

        var inGameManager = InGameManager.Create();
        await inGameManager.Initialize();

        await LoadingFade.StartFadeOut();
        inGameManager.InGameContext.StageManager.StartStage().Forget();
    }
}
