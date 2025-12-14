using UnityEngine;

public static class InGameSession
{
    public static InGameEnterInfo CurrentInGameEnterInfo { get; private set; }
    
    public static bool TryCreateInGameEnterInfo(out InGameEnterInfo newEnterInfo)
    {
        //TODO : 인게임 들어갈 수 있는지 상태 체크
        
        newEnterInfo = new InGameEnterInfo();
        return true;
    }

    public static void EnterInGame(InGameEnterInfo newEnterInfo)
    {
        CurrentInGameEnterInfo = newEnterInfo;
        SceneController.ChangeScene(SceneController.Scene.InGame).Forget();
    }
}