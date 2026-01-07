using Cysharp.Threading.Tasks;
using UnityEngine;

public class LobbySceneStarter : SceneStarter
{
    public override async UniTask StartScene()
    {
        Debug.Log("Starting LobbySceneStarter");

        await UIManager.OpenUI<UILobbyMain>();

        await LoadingFade.StartFadeOut();
    }
}
