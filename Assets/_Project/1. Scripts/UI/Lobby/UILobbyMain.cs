using UnityEngine;

public class UILobbyMain : UIPanel
{
    public void OnClickStartGame()
    {
        if (InGameSession.TryCreateInGameEnterInfo(out var inGameEnterInfo))
        {
            InGameSession.EnterInGame(inGameEnterInfo);
        }
    }
}