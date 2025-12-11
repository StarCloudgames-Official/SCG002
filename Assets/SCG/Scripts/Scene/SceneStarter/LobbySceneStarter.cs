using UnityEngine;

public class LobbySceneStarter : SceneStarter
{
    public override async Awaitable StartScene()
    {
        Debug.Log("Starting LobbySceneStarter");

        await UIManager.OpenUI<UILobbyMain>();
        
        await LoadingFade.StartFadeOut();
    }
}