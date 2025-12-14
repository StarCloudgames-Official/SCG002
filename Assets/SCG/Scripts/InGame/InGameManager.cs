using StarCloudgamesLibrary;
using UnityEngine;

public class InGameManager : Singleton<InGameManager>
{
    public InGameContext InGameContext { get; private set; }
    public UIInGameMain UIInGameMain { get; private set; }

    public override async Awaitable Initialize()
    {
        var spawnManager = SpawnManager.Create();
        await spawnManager.Initialize();
        
        InGameContext = new InGameContext();
        InGameContext.Initialize(InGameSession.CurrentInGameEnterInfo);

        UIInGameMain = await UIManager.OpenUI<UIInGameMain>();

        //set state by enterinfo
    }
}